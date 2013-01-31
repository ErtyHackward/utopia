using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Tools.XMLSerializer;

namespace LtreeVisualizer.DataPipe
{
    public class Pipe
    {
        public static bool StopThread = false;
        public static ConcurrentQueue<TreeBluePrint> MessagesQueue = new ConcurrentQueue<TreeBluePrint>();
        public static NamedPipeClientStream PipeStream;

        // Create a new XmlSerializer instance with the type of the test class
        XmlSerializer SerializerObj = new XmlSerializer(typeof(TreeBluePrint));

        public void Start()
        {
            PipeStream = new NamedPipeClientStream(".", "UtopiaEditor", PipeDirection.InOut, PipeOptions.Asynchronous);
            PipeStream.Connect(1000);
            if (PipeStream.IsConnected)
            {
                using (StreamReader sr = new StreamReader(PipeStream))
                {
                    while (StopThread != true)
                    {
                        string data = sr.ReadLine();
                        if (data != null)
                        {
                            data = data.Replace("|", Environment.NewLine);
                            TreeBluePrint BleuPrintdata = (TreeBluePrint)XmlSerialize.XmlDeserializeFromString(data, typeof(TreeBluePrint));
                            MessagesQueue.Enqueue(BleuPrintdata);
                        }
                        Thread.Sleep(1);
                    }                                      
                }
            }

            PipeStream.Dispose();

        }

        

    }
}
