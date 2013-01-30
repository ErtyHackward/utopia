using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LtreeVisualizer.DataPipe;
using S33M3DXEngine.Main;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.LandscapeEntities.Trees;

namespace LtreeVisualizer
{
    public class LtreeRender : Game
    {
        public LtreeRender(Size startingWindowsSize, string WindowsCaption, Size ResolutionSize = default(Size))
            : base(startingWindowsSize, WindowsCaption, new SharpDX.DXGI.SampleDescription(1, 0), ResolutionSize)
        {
            DXStates.CreateStates(Engine);
        }

        Thread _dataPipeThread;
        Pipe _dataPipe = new Pipe();
        public override void Initialize()
        {
            _dataPipeThread = new Thread(_dataPipe.Start);
            _dataPipeThread.Start();

            //Register Here all components
            //_gameComp = ToDispose(new Triangle(Engine) { Visible = true });
            //this.GameComponents.Add(_gameComp);
            this.Engine.GameWindow.KeyUp += new KeyEventHandler(GameWindow_KeyUp);
            base.Initialize();
        }

        void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                this.Engine.isFullScreen = !this.Engine.isFullScreen;
            }
        }

        public override void FTSUpdate(GameTime TimeSpend)
        {
            TreeBluePrint newTemplate;

            if (Pipe.MessagesQueue.TryDequeue(out newTemplate))
            {
                this.Engine.GameWindow.Text = newTemplate.Axiom;
            }

            base.FTSUpdate(TimeSpend);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            Pipe.StopThread = true;
            Pipe.PipeStream.Close();
            base.Dispose(disposeManagedResources);
        }
    }
}
