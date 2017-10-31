using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageHSVTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap i = new Bitmap(Image.FromFile(
                @"C:\Users\chewycrashburn\Miniconda3\envs\tensorflow-gpu\screendata\png\train\WeapM9_C\10_0_862.png"));
            //Bitmap i = new Bitmap(Image.FromFile(
            //    @"C:\Users\chewycrashburn\Miniconda3\envs\tensorflow-gpu\screendata\png\train\WeapWinchester_C\00_0_9038.png"));
            
            int count = 0;//a ==75 b == 24
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int a=0;a<i.Height;a++)
            {
                for(int b=0;b<i.Width;b++)
                {
                    Color color = i.GetPixel(b, a);
                    double hue = color.GetHue() * (Math.PI / 180.0);
                    double sat = color.GetSaturation();
                    double val = color.GetBrightness();

                    double clampedRedHue = Math.Max(Math.Pow(hue - Math.PI, 2) - (Math.PI * Math.PI - 1), 0);

                    if(clampedRedHue > 0 && sat > 0.8 && val > 0.3)
                    {
                        count++;
                    }
                }
            }

            if(count > 1200)
            {
                Console.WriteLine("Out of ammo!");
            }

            Console.WriteLine("time: " + sw.ElapsedMilliseconds);
            
            Console.Read();
            
        }
    }
}
