using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using S33M3Engines.D3D.Effects.HLSLFramework;
using System.IO;

namespace S33M3Engines.D3D.Effects
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

    public abstract class HLSLShaderWrap : IDisposable
    {
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
        string _filePathName;
        string _fileName;
        VertexShader _vs;
        GeometryShader _gs;
        PixelShader _ps;

        //Vertex type use with this shader (must match the _inputLayout of the VS of the Effect !
        VertexDeclaration _vertexDeclaration;
        InputLayout _inputLayout;

        public D3DEngine _d3dEngine;
        public string CompilationErrors;
        #endregion

        #region public properties
        protected List<iCBuffer> CBuffers = new List<iCBuffer>();
        protected List<ShaderResource> ShaderResources = new List<ShaderResource>();
        protected List<ShaderSampler> ShaderSamplers = new List<ShaderSampler>();
        #endregion

        //Ctor
        public HLSLShaderWrap(D3DEngine d3dEngine, string filePathName, VertexDeclaration VertexDeclaration)
        {
            //Extract FileName from Path
            _fileName = Path.GetFileName(filePathName);

            _filePathName = filePathName;
            _d3dEngine = d3dEngine;
            _vertexDeclaration = VertexDeclaration;

            //Console.WriteLine(_filePathName + " Shader created");
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
            catch (Exception)
            {
                Console.WriteLine("Shader compilation error : " + CompilationErrors);
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
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.VertexShader_EntryPoint, VSProfiles.DirectX10Profile, _d3dEngine.ShaderFlags, EffectFlags.None, null, null, out CompilationErrors))
                    {
                        //Get the VS Input signature from the Vertex Shader
                        _signature = ShaderSignature.GetInputSignature(bytecode);
                        //Create the inputLayout from the signature (Must Match the Vertex Format used with this effect !!!)
                        _inputLayout = new InputLayout(_d3dEngine.Device, _signature, _vertexDeclaration.Elements);

                        _vs = new VertexShader(_d3dEngine.Device, bytecode);
                        D3D.Tools.Resource.SetName(_vs, "vs " + _fileName);
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.VS, ShaderIDs.VS, shaderMetaData);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception(CompilationErrors);
            }
        }

        // load and compile the geometry shader
        private void LoadGeometryShader()
        {
            try
            {

                if (_shaderEntryPoint.GeometryShader_EntryPoint != null)
                {
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.GeometryShader_EntryPoint, GSProfiles.DirectX10Profile, _d3dEngine.ShaderFlags, EffectFlags.None, null, null, out CompilationErrors))
                    {
                        _gs = new GeometryShader(_d3dEngine.Device, bytecode);
                        D3D.Tools.Resource.SetName(_gs, "gs " + _fileName);
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.GS, ShaderIDs.GS, shaderMetaData);
                        }

                    }
                }
            }
            catch (Exception)
            {
                throw new Exception(CompilationErrors);
            }
        }

        // load and compile the pixel shader
        private void LoadPixelShader()
        {
            try
            {
                if (_shaderEntryPoint.PixelShader_EntryPoint != null)
                {
                    using (var bytecode = ShaderBytecode.CompileFromFile(_filePathName, _shaderEntryPoint.PixelShader_EntryPoint, PSProfiles.DirectX10Profile, _d3dEngine.ShaderFlags, EffectFlags.None, null, null, out CompilationErrors))
                    {
                        _ps = new PixelShader(_d3dEngine.Device, bytecode);
                        D3D.Tools.Resource.SetName(_ps, "ps " + _fileName);
                        using (ShaderReflection shaderMetaData = new ShaderReflection(bytecode))
                        {
                            ShaderReflection(Shaders.PS, ShaderIDs.PS, shaderMetaData);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception(CompilationErrors);
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
        public void Begin()
        {
            if (_vs != null) _d3dEngine.Context.VertexShader.Set(_vs);
            if (_gs != null) _d3dEngine.Context.GeometryShader.Set(_gs); else _d3dEngine.Context.GeometryShader.Set(null);
            if (_ps != null) _d3dEngine.Context.PixelShader.Set(_ps);

            // Input Layout changed ?? ==> Need to send it to the InputAssembler
            if (HLSLShaderWrap.LastEffectSet != _inputLayout.GetHashCode())
            {
                _d3dEngine.Context.InputAssembler.InputLayout = _inputLayout;
                HLSLShaderWrap.LastEffectSet = _inputLayout.GetHashCode();
            }

            //Set the resources (forced)
            for (int i = 0; i < _shaderResources.Length; i++)
            {
                _shaderResources[i].Set2Device(true);
            }

            //Set the samplers (forced)
            for (int i = 0; i < _shaderSamplers.Length; i++)
            {
                _shaderSamplers[i].Set2Device(true);
            }
        }

        //Will update the Constant Buffers that have been modified set in dirty states
        public void Apply()
        {
            //Set the Constant Buffers
            for (int i = 0; i < _cBuffers.Length; i++)
            {
                _cBuffers[i].Set2Device();
            }

            //Set the resources
            for (int i = 0; i < _shaderResources.Length; i++)
            {
                _shaderResources[i].Set2Device();
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (_vs != null) _vs.Dispose();
            if (_gs != null) _gs.Dispose();
            if (_ps != null) _ps.Dispose();
            for (int i = 0; i < _cBuffers.Length; i++)
            {
                if(!_cBuffers[i].GlobalCB) _cBuffers[i].Dispose();
            }
            DisposeInputLayout();

            //Console.WriteLine(_filePathName + " Shader Disposed");

        }

        private void DisposeInputLayout()
        {
            if (_inputLayout == null) return;
            _inputLayout.Dispose();

        }

        #endregion
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

