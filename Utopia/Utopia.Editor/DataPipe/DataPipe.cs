using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utopia.Editor.DataPipe
{
    public class Pipe
    {
        public static bool StopThread = false;
        public static Process RunningLtree;
        public static ConcurrentQueue<string> MessagesQueue = new ConcurrentQueue<string>();
        public void Start()
        {
            while (StopThread != true)
            {
                // Create a name pipe
                using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("UtopiaEditor", PipeDirection.InOut))
                {

                    Console.WriteLine("[Server] Pipe created {0}", pipeStream.GetHashCode());

                    // Wait for a connection
                    pipeStream.WaitForConnection();
                    Console.WriteLine("[Server] Pipe connection established");
                    try
                    {


                        using (StreamWriter sw = new StreamWriter(pipeStream))
                        {
                            using (StreamReader sr = new StreamReader(pipeStream))
                            {
                                sw.AutoFlush = true;
                                string msg;
                                sw.WriteLine("Bonjour");
                                while (StopThread != true && (RunningLtree != null && RunningLtree.HasExited == false))
                                {
                                    try
                                    {
                                        if (MessagesQueue.TryDequeue(out msg))
                                        {
                                            sw.WriteLine(msg);
                                        }
                                        Thread.Sleep(1);
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }

                Console.WriteLine("Connection lost");
            }
        }

        public void Stop()
        {

        }
    }
}
