//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SharpDX.Direct3D11;
//using SharpDX;

//namespace Nuclex.UserInterface.Visuals.Flat
//{

//    //needed cause of dx11 float based viewport
//    public class Viewport
//    {
//        public int X, Y, Width, Height;
//    }

//    //stateless wrapper to dx11 device with nuclex xna dx9 method signatures
//    public class GraphicsDevice
//    {
//        public Device device { get; private set; }
//        public Viewport ViewPort { get; private set; }

//        public GraphicsDevice(Device device)
//        {
//            this.device = device;
//            ViewPort = new Viewport();
//            ViewPort.X = (int)device.ImmediateContext.Rasterizer.GetViewports()[0].TopLeftX;
//            ViewPort.Y = (int)device.ImmediateContext.Rasterizer.GetViewports()[0].TopLeftY;
//            ViewPort.Width = (int)device.ImmediateContext.Rasterizer.GetViewports()[0].Width;
//            ViewPort.Height = (int)device.ImmediateContext.Rasterizer.GetViewports()[0].Height;
//        }

//        public SharpDX.Rectangle ScissorRectangle
//        {
//            get
//            {
//                Rectangle scissor = device.ImmediateContext.Rasterizer.GetScissorRectangles()[0];
//                return scissor;
//            }

//            set
//            { device.ImmediateContext.Rasterizer.SetScissorRectangles(new Rectangle[] { value }); }
//        }


//    }
//}
