using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using S33M3DXEngine.RenderStates;

namespace Utopia.Shared.GameDXStates
{
    public static class DXStates
    {
        public static void CreateStates(D3DEngine engine)
        {
            //Init the Repository Objects
            RenderStatesRepo.Initialize(engine);

            CreateRasterStatesCollection(engine);
            CreateBlendStatesCollection();
            CreateDepthStencilCollection();
            CreateSamplerStatesCollection();
        }

        public static int NotSet = -1;

        public static class Rasters
        {
            public static int Default;
            public static int CullNone;
            public static int CullFront;
            public static int Wired;
            public static int Sprite;
        }

        private static void CreateRasterStatesCollection(D3DEngine engine)
        {
            //Rasters.Default
            Rasters.Default = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Solid, IsMultisampleEnabled = engine.CurrentMSAASampling.Count == 1 ? false : true });

            //Rasters.CullNone
            Rasters.CullNone = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.None, FillMode = FillMode.Solid, IsMultisampleEnabled = engine.CurrentMSAASampling.Count == 1 ? false : true });

            //Rasters.CullFront
            Rasters.CullFront = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Front, FillMode = FillMode.Solid, IsMultisampleEnabled = engine.CurrentMSAASampling.Count == 1 ? false : true });

            //Rasters.Wired
            Rasters.Wired = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Wireframe, IsMultisampleEnabled = engine.CurrentMSAASampling.Count == 1 ? false : true });

            //Rasters.Sprite
            Rasters.Sprite = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Wireframe, IsMultisampleEnabled = engine.CurrentMSAASampling.Count == 1 ? false : true });
        }

        public static void Dispose()
        {
            RenderStatesRepo.Dispose();
        }

        public static class Blenders
        {
            public static int Enabled;
            public static int Disabled;
            public static int Sprite;
            public static int AlphaToCoverage;
        }

        private static void CreateBlendStatesCollection()
        {
            BlendStateDescription BlendDescr;
            //Blender.Enabled
            BlendDescr = new BlendStateDescription();
            BlendDescr.IndependentBlendEnable = false;
            BlendDescr.AlphaToCoverageEnable = false;
            for (int i = 0; i < 8; i++)
            {
                BlendDescr.RenderTarget[i].IsBlendEnabled = true;
                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            Blenders.Enabled = RenderStatesRepo.AddBlendStates(BlendDescr);

            //Blender.Disabled
            BlendDescr = new BlendStateDescription();
            BlendDescr.IndependentBlendEnable = false;
            BlendDescr.AlphaToCoverageEnable = false;
            for (int i = 0; i < 8; i++)
            {
                BlendDescr.RenderTarget[i].IsBlendEnabled = false;
                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            Blenders.Disabled = RenderStatesRepo.AddBlendStates(BlendDescr);

            //Blender.Sprite
            BlendDescr = new BlendStateDescription();
            BlendDescr.IndependentBlendEnable = false;
            BlendDescr.AlphaToCoverageEnable = false;
            for (int i = 0; i < 8; i++)
            {
                BlendDescr.RenderTarget[i].IsBlendEnabled = true;
                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            Blenders.Sprite = RenderStatesRepo.AddBlendStates(BlendDescr);

            //Blender.AlphaToCoverage
            BlendDescr = new BlendStateDescription();
            BlendDescr.IndependentBlendEnable = false;
            BlendDescr.AlphaToCoverageEnable = true;
            for (int i = 0; i < 8; i++)
            {
                BlendDescr.RenderTarget[i].IsBlendEnabled = false;
                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            Blenders.AlphaToCoverage = RenderStatesRepo.AddBlendStates(BlendDescr);
        }

        public static class DepthStencils
        {
            public static int DepthReadWriteEnabled;
            public static int DepthReadEnabled;
            public static int DepthDisabled;
        }

        private static void CreateDepthStencilCollection()
        {
            List<DepthStencilStateDescription> statesCollection = new List<DepthStencilStateDescription>();

            //DepthStencil.DepthEnabled
            DepthStencils.DepthReadWriteEnabled = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            }
                                 );

            //DepthStencil.DepthDisabled
            DepthStencils.DepthDisabled = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            }
                                 );

            //DepthStencil.DepthDisabled
            DepthStencils.DepthReadEnabled = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Less,
                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            }
                                 );
        }

        public static class Samplers
        {
            public static int UVClamp_MinMagMipLinear;
            public static int UVClamp_MinMagMipPoint;
            public static int UVWrap_MinLinearMagPointMipLinear;
            public static int UVWrap_MinMagMipPoint;
            public static int UWrapVClamp_MinMagMipLinear;
            public static int UVWrap_MinMagMipLinear;
            public static int UVWrap_MinMagPointMipLinear;
            public static int UVWrap_Text;
        }

        private static void CreateSamplerStatesCollection()
        {
            //Samplers.UVClamp_MinMagMipLinear
            Samplers.UVClamp_MinMagMipLinear = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                                );

            //Samplers.UVClamp_MinMagMipPoint
            Samplers.UVClamp_MinMagMipPoint = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipPoint,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                                );

            //Samplers.UVWrap_MinLinearMagPointMipLinear
            Samplers.UVWrap_MinLinearMagPointMipLinear = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinLinearMagPointMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );


            //Samplers.UVWrap_MinLinearMagPointMipLinear
            Samplers.UVWrap_MinMagPointMipLinear = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );

            //Samplers.UVWrap_MinMagMipPoint
            Samplers.UVWrap_MinMagMipPoint = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipPoint,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );

            //Samplers.UWrapVClamp_MinMagMipLinear
            Samplers.UWrapVClamp_MinMagMipLinear = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );

            //Samplers.UVWrap_MinMagMipLinear
            Samplers.UVWrap_MinMagMipLinear = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );

            //Samplers.UVWrap_MinMagMipLinear
            Samplers.UVWrap_Text = RenderStatesRepo.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.Anisotropic,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );
        }
    }
}
