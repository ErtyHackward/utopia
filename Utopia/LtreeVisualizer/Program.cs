using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LtreeVisualizer
{
    class Program
    {
        static void Main(string[] args)
        {
            LtreeRender render = new LtreeRender(new System.Drawing.Size(800, 600), "Ltree Visu");
            render.Run();
            render.Dispose();
        }
    }
}
