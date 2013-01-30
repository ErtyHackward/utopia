using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace LtreeVisualizer.DataPipe
{
    public class Pipe
    {
        public static bool StopThread = false;
        public static ConcurrentQueue<string> MessagesQueue = new ConcurrentQueue<string>();
        public static NamedPipeClientStream PipeStream;
        public void Start()
        {
            PipeStream = new NamedPipeClientStream(".", "UtopiaEditor", PipeDirection.InOut, PipeOptions.Asynchronous);
            PipeStream.Connect(1000);
            if (PipeStream.IsConnected)
            {
                using (var sr = new StreamReader(PipeStream))
                {
                    while (StopThread != true)
                    {
                        string data = sr.ReadLine();
                        if (data != null)
                        {
                            MessagesQueue.Enqueue(data);
                        }
                        Thread.Sleep(1);
                    }                                      
                }
            }

            PipeStream.Dispose();

        }
    }
}
