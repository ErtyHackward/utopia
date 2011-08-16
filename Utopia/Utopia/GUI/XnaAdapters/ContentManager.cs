using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using Utopia.GUI.NuclexUIPort.Visuals.Flat;

namespace Nuclex.UserInterface.Visuals.Flat
{
    public class ContentManager
    {
        public IServiceProvider ServiceProvider { get; private set; }

        Device _device;
        String _contentPath;
        public ContentManager(IServiceProvider serviceProvider, string contentPath)
        {
            this.ServiceProvider = serviceProvider;
            IGraphicsDeviceService graphicsDeviceService =
                (IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService));
            _device = graphicsDeviceService.GraphicsDevice;
            _contentPath = contentPath;
        }

        internal void Dispose()
        {
           // throw new NotImplementedException();
        }

        internal SharpDX.Direct3D11.Texture2D Load<T1>(string name)
        {
            return Texture2D.FromFile<Texture2D>(_device, _contentPath+"\\"+name+".png");
        }
    }
}
