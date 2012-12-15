using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            int splitSize = 0;
            string filePath = null;

            if (args.Length == 0)
                return;
            
            Image img = Image.FromFile(args[0]);

            Console.WriteLine("Enter the size of the texture you want to receive [32]?");
            var size = Console.ReadLine();

            if (!int.TryParse(size, out splitSize))
                splitSize = 32;

            

            int imgIndex = 0;


            for (int y = 0; y < img.Height; y += splitSize)
            {
                for (int x = 0; x < img.Width; x += splitSize)
                {
                    var srcRect = new Rectangle(x, y, splitSize, splitSize);
                    var destRect = new Rectangle(0, 0, splitSize, splitSize);
                    using (var resImg = new Bitmap(splitSize, splitSize))
                    {
                        using (var g = Graphics.FromImage(resImg))
                            g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);

                        resImg.Save("img" + ( imgIndex++ ) + ".png", ImageFormat.Png);
                    }

                }
            }

        }
    }
}
