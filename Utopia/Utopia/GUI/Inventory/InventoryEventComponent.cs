﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using S33M3CoreComponents.Sound;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Shows inventory events when inventory is not open
    /// </summary>
    public class InventoryEventComponent : DrawableGameComponent
    {
        public enum EventType
        {
            Take,
            Put
        }

        public struct NotifyData
        {
            public IItem Item;
            public string Message;
            public EventType Event;
        }

        private SpriteFont _font;
        private SpriteRenderer _spriteRender;
        private SpriteTexture _icon;
        private int _textureArrayIndex;
        private Queue<NotifyData> _queue;
        private DateTime _lastUpdate;
        private NotifyData? _currentItem;
        private bool _hide;
        private float _alpha;

        #region Dependencies
        [Inject]
        public PlayerCharacter Player { get; set; }

        [Inject]
        public D3DEngine Engine { get; set; }

        [Inject]
        public IconFactory IconFactory { get; set; }

        [Inject]
        public ISoundEngine SoundEngine { get; set; }

        #endregion

        public InventoryEventComponent()
        {
            DrawOrders.UpdateIndex(0, 10000);
        }

        public void Notify(IItem item, string text, bool put)
        {
            Notify(new NotifyData { Item = item, Message = text, Event = put ? EventType.Put : EventType.Take});
        }

        public void Notify(NotifyData data)
        {
            if (Updatable)
                _queue.Enqueue(data);
        }

        public override void Initialize()
        {
            _queue = new Queue<NotifyData>();
        }

        public override void LoadContent(DeviceContext context)
        {
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Lucida Console", 16f, System.Drawing.FontStyle.Bold, true, Engine.Device);
            _spriteRender = ToDispose(new SpriteRenderer(Engine));
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (( DateTime.Now - _lastUpdate ).TotalSeconds >= 3)
            {
                if (_queue.Count > 0)
                {
                    _hide = false;
                    _lastUpdate = DateTime.Now;
                    _currentItem = _queue.Dequeue();
                    var item = _currentItem.Value;
                    IconFactory.Lookup(item.Item, out _icon, out _textureArrayIndex);

                    if (item.Event == EventType.Put)
                    {
                        if (item.Item.PutSound != null)
                        {
                            SoundEngine.StartPlay2D(item.Item.PutSound);
                        }
                    }
                }
                else 
                    _hide = true;
            }

            if (_currentItem.HasValue)
            {

                var delta = elapsedTime * 7f;
                if (_hide)
                {
                    if (_alpha > 0)
                        _alpha -= delta;
                    
                    if (_alpha < 0)
                        _alpha = 0;
                }
                else if (_alpha < 1)
                {
                    _alpha += delta;
                    if (_alpha > 1)
                        _alpha = 1;
                }

                if (_alpha == 0)
                {
                    _currentItem = null;
                }
            }

        }

        public override void Draw(DeviceContext context, int index)
        {
            if (_currentItem.HasValue)
            {
                _spriteRender.Begin(false, context);

                var textPos = new Vector2(75, 45);
                var textShadowPos = textPos + new Vector2(1, 1);
                var color = new ByteColor(255, 255, 255, (int)(255 *_alpha));
                var colorBlack = new ByteColor(0, 0, 0, (int)(255 * _alpha));
                var rect = new Rectangle(25, 25, 67, 67);

                _spriteRender.Draw(_icon, ref rect, ref color, _textureArrayIndex);
                _spriteRender.DrawText(_font, _currentItem.Value.Message, ref textShadowPos, ref colorBlack);
                _spriteRender.DrawText(_font, _currentItem.Value.Message, ref textPos, ref color);
                _spriteRender.End(context);
            }
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!isEnabled)
            {
                _queue.Clear();
            }

            base.OnUpdatableChanged(sender, args);
        }
    }
}
