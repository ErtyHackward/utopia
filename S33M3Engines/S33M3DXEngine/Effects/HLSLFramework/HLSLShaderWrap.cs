using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX;
using System.IO;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Debug;

namespace S33M3DXEngine.Effects.HLSLFramework
{
    //Encapsulate Effect things !
    public class EntryPoints
    {
        public string VertexShader_EntryPoint = null;
        public string GeometryShader_EntryPoint = null;
        public string PixelShader_EntryPoint = null;
    }

    [Flags]
    public enum Shaders
    {
        VS = 1,
        GS = 2,
        PS = 4,
    }

    public static class ShaderIDs
    {
        public static readonly int NbrShaders = 3;
        public static readonly int VS = 0;
        public static readonly int GS = 1;
        public static readonly int PS = 2;
    }

    public abstract class HLSLShaderWrap : Component
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //Managing the Effect at a Higher level, in order to avoid to "Reset" states when not needed
        protected static int LastEffectSet = 0;
        public static void ResetEffectStateTracker()
        {
            LastEffectSet = 0;
        }
        //===========================================================================================

        #region Private variables
        private EntryPoints _shaderEntryPoint;
        private ShaderSignature _signature;

        private iCBuffer[] _cBuffers;
        private ShaderResource[] _shaderResources;
        private ShaderSampler[] _shaderSamplers;

        //Shaders
        private VertexShader _vs;
        private GeometryShader _gs;
        private PixelShader _ps;

        //Vertex type use with this shader (must match the _inputLayout of the VS of the Effect !
        private InputLayout _inputLayout;

        protected readonly Device device;
        private readonly string _filePathName;
        private readonly string _fileName;
        private readonly VertexDeclaration _vertexDeclaration;
        #endregion

        #region public properties
        protected List<iCBuffer> CBuffers = new List<iCBuffer>();
        protected List<ShaderResource> ShaderResources = new List<ShaderResource>();
        protected List<ShaderSampler> ShaderSamplers = new List<ShaderSampler>();
        #endregion

        //Ctor
        public HLSLShaderWrap(Device device, string filePathName, VertexDeclaration VertexDeclaration, params iCBuffer[] externalCBuffers)
        {
            //Extract FileName from Path
            _fileName = Path.GetFileName(filePathName);

            _filePathName = filePathName;
            this.device = device;
            _vertexDeclaration = VertexDeclaration;

            //Add Externaly managed CBuffer = Shared CBuffer
            foreach (var externalCBuffer in externalCBuffers)
            {
                CBuffers.Add(externalCBuffer.Clone());
            }
        }

        protected void LoadShaders(EntryPoints shaderEntryPoints)
        {
            try
            {
                //Map the List to Tables
                _cBuffers = CBuffers.ToArray();
                _shaderResources = ShaderResources.ToArray();
                _shaderSamplers = ShaderSamplers.ToArray();
                _shaderEntryPoint = shaderEntryPoints;

                //Load the HLSL file
                LoadVertexShader();
                LoadGeometryShader();
                LoadPixelShader();
            }
            catch (Exception e)
            {
                logger.Error("Shadder loading error : {0}, Error : {1}", _fileName, e.Message);
                throw;
            }
        }

        #region Shader Loading & Parsing
        // load and compile the vertex shader
        private void LoadVertexShader()
        {
            try
            {
                if (_shaderEntryPoint.VertexShader_EntryPoint != null)
                {
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.VertexShader_EntryPoint, VSProfiles.DirectX10Profile, D3DEngine.ShaderFlags, EffectFlags.None, null, null))
                    {
                        //Log Compilation Warning
                        if (bytecode.Message != null) logger.Warn("Vertex Shader [{0}] compilation message returned :\n{1}", _fileName, bytecode.Message);

                        //Get the VS Input signature from the Vertex Shader
                        _signature = ToDispose(ShaderSignature.GetInputSignature(bytecode));
                        //Create the inputLayout from the signature (Must Match the Vertex Format used with this effect !!!)
                        _inputLayout = ToDispose(new InputLayout(device, _signature, _vertexDeclaration.Elements));

                        _vs = ToDispose(new VertexShader(device, bytecode));
#if DEBUG
                        //Set resource Name, will only be done at debug time.
                        _vs.DebugName = "VertexShader from " + _fileName;
#endif
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.VS, ShaderIDs.VS, shaderMetaData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Shader compilation error : " + e.Message);
            }
        }

        // load and compile the geometry shader
        private void LoadGeometryShader()
        {
            try
            {
                if (_shaderEntryPoint.GeometryShader_EntryPoint != null)
                {
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.GeometryShader_EntryPoint, GSProfiles.DirectX10Profile, D3DEngine.ShaderFlags, EffectFlags.None, null, null))
                    {
                        //Log Compilation Warning
                        if (bytecode.Message != null) logger.Warn("Geometry Shader [{0}] compilation message returned :\n{1}", _fileName, bytecode.Message);

                        _gs = ToDispose(new GeometryShader(device, bytecode));
#if DEBUG
                        //Set resource Name, will only be done at debug time.
                        _gs.DebugName = "GeometryShader from " + _fileName;
#endif
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.GS, ShaderIDs.GS, shaderMetaData);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Shader compilation error : " + e.Message);
            }
        }

        // load and compile the pixel shader
        private void LoadPixelShader()
        {
            try
            {
                if (_shaderEntryPoint.PixelShader_EntryPoint != null)
                {
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.PixelShader_EntryPoint, PSProfiles.DirectX10Profile, D3DEngine.ShaderFlags, EffectFlags.None, null, null))
                    {
                        //Log Compilation Warning
                        if (bytecode.Message != null) logger.Warn("Pixel Shader [{0}] compilation message returned :\n{1}", _fileName, bytecode.Message);

                        _ps = ToDispose(new PixelShader(device, bytecode));
#if DEBUG
                        //Set resource Name, will only be done at debug time.
                        _ps.DebugName = "PixelShadder from " + _fileName;
#endif
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.PS, ShaderIDs.PS, shaderMetaData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Shader compilation error : " + e.Message);
            }
        }
        #endregion

        private void ShaderReflection(Shaders shaderType, int ShaderId, ShaderReflection shaderMetaData)
        {
            //Look with reflection, to find the Constant Buffers used with the shader;
            ConstantBuffer constantBuffer;
            InputBindingDescription inputBindingdesc;
            for (int cptCB = 0; cptCB < shaderMetaData.Description.ConstantBuffers; cptCB++)
            {
                constantBuffer = shaderMetaData.GetConstantBuffer(cptCB);

                if (constantBuffer.Description.Type == ConstantBufferType.ConstantBuffer || constantBuffer.Description.Type == ConstantBufferType.TextureBuffer)
                {
                    for (int i = 0; i < _cBuffers.Length; i++)
                    {
                        if (_cBuffers[i].Name == constantBuffer.Description.Name)
                        {
                            inputBindingdesc = shaderMetaData.GetResourceBindingDescription(constantBuffer.Description.Name);
                            _cBuffers[i].ShadersImpacted |= shaderType;
                            _cBuffers[i].Slot[ShaderId] = inputBindingdesc.BindPoint;
                        }
                    }
                }
            }

            //Look with reflection, to find the Resources (Textures) used by the shader;
            for (int cptResources = 0; cptResources < shaderMetaData.Description.BoundResources; cptResources++)
            {
                inputBindingdesc = shaderMetaData.GetResourceBindingDescription(cptResources);

                if (inputBindingdesc.Type == ShaderInputType.Texture)
                {
                    for (int i = 0; i < _shaderResources.Length; i++)
                    {
                        if (_shaderResources[i].Name == inputBindingdesc.Name)
                        {
                            _shaderResources[i].ShadersImpacted |= shaderType;
                            _shaderResources[i].Slot[ShaderId] = inputBindingdesc.BindPoint;
                        }
                    }
                }

                if (inputBindingdesc.Type == ShaderInputType.Sampler)
                {
                    for (int i = 0; i < _shaderSamplers.Length; i++)
                    {
                        if (_shaderSamplers[i].Name == inputBindingdesc.Name)
                        {
                            _shaderSamplers[i].ShadersImpacted |= shaderType;
                            _shaderSamplers[i].Slot[ShaderId] = inputBindingdesc.BindPoint;
                        }
                    }
                }
            }

        }

        //Set all states that should only be done once for the Effect
        public void Begin(DeviceContext context)
        {
            if (_vs != null) context.VertexShader.Set(_vs);
            if (_gs != null) context.GeometryShader.Set(_gs); else context.GeometryShader.Set(null);
            if (_ps != null) context.PixelShader.Set(_ps);

            if (D3DEngine.SingleThreadRenderingOptimization)
            {
                // Input Layout changed ?? ==> Need to send it to the InputAssembler
                if (HLSLShaderWrap.LastEffectSet != _inputLayout.GetHashCode())
                {
                    context.InputAssembler.InputLayout = _inputLayout;
                    HLSLShaderWrap.LastEffectSet = _inputLayout.GetHashCode();
                }
            }
            else
            {
                context.InputAssembler.InputLayout = _inputLayout;
            }

            //Set the resources (forced)
            for (int i = 0; i < _shaderResources.Length; i++)
            {
                _shaderResources[i].Set2Device(context, true);
            }

            //Set the samplers (forced)
            for (int i = 0; i < _shaderSamplers.Length; i++)
            {
                _shaderSamplers[i].Set2Device(context, true);
            }
        }

        //Will update the Constant Buffers that have been modified set in dirty states
        public void Apply(DeviceContext context)
        {
            //Set the Constant Buffers
            for (int i = 0; i < _cBuffers.Length; i++)
            {
                _cBuffers[i].Set2Device(context);
            }

            //Set the samplers (forced)
            for (int i = 0; i < _shaderSamplers.Length; i++)
            {
                _shaderSamplers[i].Set2Device(context, false);
            }

            //Set the resources
            for (int i = 0; i < _shaderResources.Length; i++)
            {
                _shaderResources[i].Set2Device(context, false);
            }
        }

        public virtual void End(DeviceContext context)
        {
        }

    }


    //ShaderReflection reflection = new ShaderReflection(bytecode);
    //ConstantBuffer constantBuffer;
    //ShaderReflectionVariable variable;

    //for (int cptCB = 0; cptCB < reflection.Description.ConstantBuffers; cptCB++)
    //{
    //    constantBuffer = reflection.GetConstantBuffer(cptCB);

    //    if (constantBuffer.Description.Type == ConstantBufferType.ConstantBuffer || constantBuffer.Description.Type == ConstantBufferType.TextureBuffer)
    //    {
    //        Console.WriteLine("CBuffer : " + constantBuffer.Description.Name);
    //        Console.WriteLine("Size CBuffer (Bytes) : " + constantBuffer.Description.Size);
    //        Console.WriteLine("Size CBuffer : " + constantBuffer.Description.Size);
    //        for (int cptCBVariable = 0; cptCBVariable < constantBuffer.Description.Variables; cptCBVariable++)
    //        {
    //            variable = constantBuffer.GetVariable(cptCBVariable);
    //            bool isUsed = (variable.Description.Flags | ShaderVariableFlags.Used) == ShaderVariableFlags.Used;
    //            ShaderReflectionType t = variable.GetVariableType();
    //            Console.WriteLine("   " + variable.Description.Name + " type : " + t.Description.Class.ToString() + " " + t.Description.Members + " Offset:" + variable.Description.StartOffset + " Size:" + variable.Description.Size + " variable used: " + isUsed);
    //        }
    //    }
    //}

}

