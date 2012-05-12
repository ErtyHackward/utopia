using System;
using System.Drawing;
using System.Drawing.Text;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;

namespace Realms.Client.Components.GUI
{
    /// <summary>
    /// Stores sandbox specific common resources
    /// </summary>
    public class SandboxCommonResources : IDisposable
    {
        public SpriteTexture StShadow;
        public SpriteTexture StLogo;
        public SpriteTexture StGameName;
        public SpriteTexture StCubesPattern;
        public SpriteTexture StLinenPattern;
        public SpriteTexture StInputBackground;
        public SpriteTexture StButtonBackground;
        public SpriteTexture StButtonBackgroundDown;
        public SpriteTexture StButtonBackgroundHover;

        public SpriteFont FontBebasNeue50;
        public SpriteFont FontBebasNeue35;
        public SpriteFont FontBebasNeue25;
        public SpriteFont FontBebasNeue17;

        public SpriteTexture StInventoryEquipmentSlot;
        public SpriteTexture StInventoryEquipmentSlotHover;
        public SpriteTexture StInventoryInfo;
        public SpriteTexture StInventorySlot;
        public SpriteTexture StInventorySlotHover;

        public PrivateFontCollection FontCollection;

        public static SpriteTexture LoadTexture(D3DEngine engine, string filePath)
        {
            return new SpriteTexture(engine.Device, filePath);
        }

        public void LoadFontAndMenuImages(D3DEngine engine)
        {
            if (StShadow != null)
                throw new InvalidOperationException("Common images is already loaded");
            
            StLogo =                    LoadTexture(engine, @"Images\logo.png");
            StShadow =                  LoadTexture(engine, @"Images\shadow.png");
            StGameName =                LoadTexture(engine, @"Images\version.png");
            StCubesPattern =            LoadTexture(engine, @"Images\cubes.png");
            StLinenPattern =            LoadTexture(engine, @"Images\black-linen.png");
            StInputBackground =         LoadTexture(engine, @"Images\Login\login_input_bg.png");
            StButtonBackground =        LoadTexture(engine, @"Images\MainMenu\menu_button.png");
            StButtonBackgroundDown =    LoadTexture(engine, @"Images\MainMenu\menu_button_down.png");
            StButtonBackgroundHover =   LoadTexture(engine, @"Images\MainMenu\menu_button_hover.png");

            FontCollection = new PrivateFontCollection();
            FontCollection.AddFontFile("Images\\BebasNeue.ttf");

            FontBebasNeue50 = new SpriteFont();
            FontBebasNeue50.Initialize(FontCollection.Families[0], 50, FontStyle.Regular, true, engine.Device);

            FontBebasNeue35 = new SpriteFont();
            FontBebasNeue35.Initialize(FontCollection.Families[0], 35, FontStyle.Regular, true, engine.Device);

            FontBebasNeue25 = new SpriteFont();
            FontBebasNeue25.Initialize(FontCollection.Families[0], 25, FontStyle.Regular, true, engine.Device);

            FontBebasNeue17 = new SpriteFont();
            FontBebasNeue17.Initialize(FontCollection.Families[0], 16, FontStyle.Regular, true, engine.Device);
        }

        public void LoadInventoryImages(D3DEngine engine)
        {
            StInventoryEquipmentSlot        = LoadTexture(engine, @"Images\Inventory\equipment_slot.png");
            StInventoryEquipmentSlotHover   = LoadTexture(engine, @"Images\Inventory\equipment_slot_hover.png");
            StInventoryInfo                 = LoadTexture(engine, @"Images\Inventory\inventory_info.png");
            StInventorySlot                 = LoadTexture(engine, @"Images\Inventory\inventory_slot.png");
            StInventorySlotHover            = LoadTexture(engine, @"Images\Inventory\inventory_slot_active.png");
        }

        public void Dispose()
        {
            if (StShadow != null)
            {
                StShadow.Dispose();
                StLogo.Dispose();
                StGameName.Dispose();
                StCubesPattern.Dispose();
                StLinenPattern.Dispose();
                StInputBackground.Dispose();
                StButtonBackground.Dispose();
                StButtonBackgroundDown.Dispose();
                StButtonBackgroundHover.Dispose();

                FontBebasNeue50.Dispose();
                FontBebasNeue35.Dispose();
                FontBebasNeue25.Dispose();
                FontBebasNeue17.Dispose();
            }

            if (StInventoryEquipmentSlot != null)
            {
                StInventoryEquipmentSlot.Dispose();
                StInventoryEquipmentSlotHover.Dispose();
                StInventoryInfo.Dispose();
                StInventorySlot.Dispose();
                StInventorySlotHover.Dispose();
            }
        }
    }
}
