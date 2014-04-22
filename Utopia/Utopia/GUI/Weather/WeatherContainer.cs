using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.GUI.Inventory;
using Utopia.Shared.Structs.Helpers;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Weather;

namespace Utopia.GUI.TopPanel
{
    public class WeatherContainer : Control
    {
        #region Private variables
        private D3DEngine _d3DEngine;
        private IWeather _weather;
        private IWorldChunks _worldChunks;
        private PlayerEntityManager _playerEntityManager;

        private int _topPanelheight;

        //Child components
        private PanelControl _weatherPanel;

        //Energy bars
        private PanelControl _weatherFrame;
        private PanelControl _tempCursor;
        private PanelControl _moistureCursor;
        #endregion

        #region Public properties
        #endregion

        public WeatherContainer(D3DEngine d3DEngine,
                                IWeather weather,
                                IWorldChunks worldChunks,
                                PlayerEntityManager playerEntityManager)
        {
            _d3DEngine = d3DEngine;
            _weather = weather;
            _topPanelheight = 100;
            _worldChunks = worldChunks;
            _playerEntityManager = playerEntityManager;

            _d3DEngine.ScreenSize_Updated += ScreenSize_Updated;

            RefreshSize(_d3DEngine.ViewPort);
            CreateChildsComponents();
        }

        public override void BeforeDispose()
        {
            _d3DEngine.ScreenSize_Updated -= ScreenSize_Updated;
            base.BeforeDispose();
        }

        #region Public Methods
        public void Update(GameTime timeSpend)
        {
            var playerChunk = _worldChunks.GetChunkFromChunkCoord(_playerEntityManager.ChunkPosition);
            var inChunkPosition =  BlockHelper.GlobalToInternalChunkPosition(_playerEntityManager.Player.Position);
            if (inChunkPosition.X < 0 || inChunkPosition.Z < 0)
            {
                Console.WriteLine("");
            }
            var chunkColumnMeta = playerChunk.BlockData.GetColumnInfo(inChunkPosition.X, inChunkPosition.Z);
            //var c = GetChunk((int)PlayerManager.CameraWorldPosition.X, (int)PlayerManager.CameraWorldPosition.Z);
            ////From World Coord to Cube Array Coord
            //int arrayX = MathHelper.Mod((int)PlayerManager.CameraWorldPosition.X, AbstractChunk.ChunkSize.X);
            //int arrayZ = MathHelper.Mod((int)PlayerManager.CameraWorldPosition.Z, AbstractChunk.ChunkSize.Z);
            //var columnInfo = c.BlockData.GetColumnInfo(new Vector2I(arrayX, arrayZ));
            //var line2 = string.Format("Biomes MetaData : Temperature {0:0.00}, Moisture {1:0.00}, ColumnMaxHeight : {2}, ChunkID : {3}", columnInfo.Temperature / 255.0f, columnInfo.Moisture / 255.0f, columnInfo.MaxHeight, c.Position);

            float temperature = chunkColumnMeta.Temperature / 255.0f;
            //temperature += _weather.TemperatureOffset;
            temperature = Math.Max(Math.Min(temperature, 1.0f),0.0f);

            float moisture = chunkColumnMeta.Moisture / 255.0f;
            //moisture += _weather.MoistureOffset;
            moisture = Math.Max(Math.Min(moisture, 1.0f), 0.0f);

            _tempCursor.Bounds.Location.Y = _weatherFrame.GetAbsoluteBounds().Height - (temperature * _weatherFrame.GetAbsoluteBounds().Height);
            _moistureCursor.Bounds.Location.Y = _weatherFrame.GetAbsoluteBounds().Height - (moisture * _weatherFrame.GetAbsoluteBounds().Height);
        }
        #endregion

        #region Private Methods
        private void CreateChildsComponents()
        {
            _tempCursor = new PanelControl() { FrameName = "WeaterTemperature", Bounds = new UniRectangle(-16, 0, 16, 16) };
            _moistureCursor = new PanelControl() { FrameName = "WeaterMoisture", Bounds = new UniRectangle(new UniScalar(1.0f, 0.0f), 0, 16, 16) };

            _weatherPanel = ToDispose(new PanelControl() { IsRendable=false, FrameName = "panel", Bounds = new UniRectangle(new UniScalar(1.0f, -60.0f), 100, 30, 120) });

            _weatherPanel.Children.Add(_tempCursor);
            _weatherPanel.Children.Add(_moistureCursor);

            _weatherFrame = new PanelControl() { FrameName = "WeaterConditions", Bounds = new UniRectangle(5, 5, 20, new UniScalar(1.0f, -10)) };
            _weatherPanel.Children.Add(_weatherFrame);

            this.Children.Add(_weatherPanel);
        }

        private void ScreenSize_Updated(ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {
            RefreshSize(viewport);
        }

        private void RefreshSize(ViewportF viewport)
        {
            var screenSize = new Vector2I((int)viewport.Width, (int)viewport.Height);
            this.Bounds.Size = new UniVector(screenSize.X, _topPanelheight);
        }
        #endregion

    }
}
