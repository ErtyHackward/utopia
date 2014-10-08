using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using System.IO;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using System.Drawing;
using S33M3Resources.Structs;
using SharpDX;
using Color = SharpDX.Color;
using Rectangle = SharpDX.Rectangle;
using S33M3CoreComponents.Inputs;

namespace Utopia.Components
{
    public class StartUpComponent : DrawableGameComponent
    {
        #region Private variables
        private FileInfo[] _imagesSlideShowPath;
        private int _slideShowDelayInMS;
        private bool _slidShowFinished;
        private List<SpriteTexture> _slides = new List<SpriteTexture>();
        private int _renderedSlideId = 0;
        private int _maxSlideId = 0;
        private D3DEngine _engine;
        private SpriteRenderer _spriteRenderer;
        private Rectangle _slideDimension;
        private ByteColor _color = Color.White;
        private DateTime _slideSwitch = DateTime.Now;
        private InputsManager _inputsManager;
        #endregion

        #region Public variables/Properties
        #endregion

        #region Event
        public event EventHandler SlideShowFinished;
        #endregion
        public StartUpComponent(D3DEngine engine,
                                InputsManager inputsManager)
        {
            _engine = engine;
            _inputsManager = inputsManager;
            _engine.ScreenSize_Updated += _engine_ScreenSize_Updated;
            ResizeSlideDim(_engine.ViewPort);
        }

        public override void BeforeDispose()
        {
            _engine.ScreenSize_Updated -= _engine_ScreenSize_Updated;
        }
 
        #region Public methods
        public override void Initialize()
        {
            _spriteRenderer = ToDispose(new SpriteRenderer(_engine));

            _spriteRenderer.SpriteSamplerClamp = ToDispose(new SamplerState(_engine.Device,
                new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter = Filter.MinMagMipLinear,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0
                }));
            
            //Create the Slides textures
            foreach (var slidePath in _imagesSlideShowPath)
            {
                _slides.Add(ToDispose(new SpriteTexture(_engine.Device, slidePath.FullName)));
            }
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
        }

        public override void EnableComponent(bool forced = false)
        {
            base.EnableComponent(forced);
            _slideSwitch = DateTime.Now;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_slidShowFinished) return;

            if (_inputsManager.KeyboardManager.PrevKeyboardState.IsKeyDown(System.Windows.Forms.Keys.Escape) &&
                _inputsManager.KeyboardManager.CurKeyboardState.IsKeyUp(System.Windows.Forms.Keys.Escape))
            {
                _slideSwitch = _slideSwitch.AddMilliseconds(-_slideShowDelayInMS);
            }

            if ((DateTime.Now - _slideSwitch).TotalMilliseconds > _slideShowDelayInMS)
            {
                _slideSwitch = DateTime.Now;
                _renderedSlideId++;
                if (_renderedSlideId > _maxSlideId)
                {
                    if (SlideShowFinished != null) 
                        SlideShowFinished(this, null);
                    _slidShowFinished = true;
                    _renderedSlideId = _maxSlideId;
                }
            }
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            _spriteRenderer.Begin(false, context);

            _spriteRenderer.Draw(_slides[_renderedSlideId], ref _slideDimension, ref _color);

            _spriteRenderer.End(context);
        }

        public void SetSlideShows(FileInfo[] imagesSlideShowPath, int slideShowDelayInMS)
        {
            _imagesSlideShowPath = imagesSlideShowPath;
            _slideShowDelayInMS = slideShowDelayInMS;

            _maxSlideId = _imagesSlideShowPath.Count() - 1;
        }
        #endregion

        #region Private methods
        private void _engine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            ResizeSlideDim(viewport);
        }

        private void ResizeSlideDim(ViewportF viewport)
        {
            _slideDimension = new Rectangle(0, 0, (int)viewport.Width, (int)viewport.Height);
        }
        #endregion
    }
}
