using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.StatesManager;
using S33M3Engines;
using SharpDX.Direct3D11;

namespace Utopia.GameDXStates
{
    public static class DXStates
    {
        public static void CreateStates(D3DEngine engine)
        {
            //Init the Repository Objects
            StatesRepository.Initialize(engine);

            CreateRasterStatesCollection();
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

        private static void CreateRasterStatesCollection()
        {
            //Rasters.Default
            Rasters.Default = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Solid });

            //Rasters.CullNone
            Rasters.CullNone = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.None, FillMode = FillMode.Solid });

            //Rasters.CullFront
            Rasters.CullFront = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Front, FillMode = FillMode.Solid });

            //Rasters.Wired
            Rasters.Wired = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Wireframe });

            //Rasters.Sprite
            Rasters.Sprite = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = CullMode.Back, FillMode = FillMode.Wireframe });
        }

        public static void Dispose()
        {
            StatesRepository.CleanUp();
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
            Blenders.Enabled = StatesRepository.AddBlendStates(BlendDescr);

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
            Blenders.Disabled = StatesRepository.AddBlendStates(BlendDescr);

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
            Blenders.Sprite = StatesRepository.AddBlendStates(BlendDescr);

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
            Blenders.AlphaToCoverage = StatesRepository.AddBlendStates(BlendDescr);
        }

        public static class DepthStencils
        {
            public static int DepthEnabled;
            public static int DepthDisabled;
        }

        private static void CreateDepthStencilCollection()
        {
            List<DepthStencilStateDescription> statesCollection = new List<DepthStencilStateDescription>();

            //DepthStencil.DepthEnabled
            DepthStencils.DepthEnabled = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription()
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
            DepthStencils.DepthDisabled = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            }
                                 );
        }


        public static class Samplers
        {
            public static int UVClamp_MinMagMipPoint;
            public static int UVWrap_MinLinearMagPointMipLinear;
            public static int UVWrap_MinMagMipPoint;
            public static int UWrapVClamp_MinMagMipLinear;
            public static int UVWrap_MinMagMipLinear;
        }

        private static void CreateSamplerStatesCollection()
        {
            //Samplers.UVClamp_MinMagMipPoint
            Samplers.UVClamp_MinMagMipPoint = StatesRepository.AddSamplerStates(new SamplerStateDescription()
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
            Samplers.UVWrap_MinLinearMagPointMipLinear = StatesRepository.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinLinearMagPointMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );

            //Samplers.UVWrap_MinMagMipPoint
            Samplers.UVWrap_MinMagMipPoint = StatesRepository.AddSamplerStates(new SamplerStateDescription()
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
            Samplers.UWrapVClamp_MinMagMipLinear = StatesRepository.AddSamplerStates(new SamplerStateDescription()
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
            Samplers.UVWrap_MinMagMipLinear = StatesRepository.AddSamplerStates(new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear,
                MaximumLod = float.MaxValue,
                MinimumLod = 0
            }
                    );
        }
    }
}
