using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace TakeScreenshot
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        static void Main(string[] args)
        {
            Size sSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            float scaleX = (float)sSize.Width / 1920.0f;
            float scaleY = (float)sSize.Height / 1080.0f;
            int scaledX = (int)(64.0f * scaleX);
            int scaledY = (int)(64.0f * scaleY);
            bool scoped = false;
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int imageCtrScoped = 0, imageCtrUnscoped = 0;

            System.IO.Directory.CreateDirectory("scoped");
            System.IO.Directory.CreateDirectory("unscoped");

            while (true)
            {
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    if(scoped)
                    {
                        using (Bitmap bmpScreenCapture = new Bitmap(scaledX, scaledY))
                        {
                            using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                            {
                                g.CopyFromScreen((sSize.Width / 2) - (scaledX / 2), (sSize.Height / 2) - (scaledY / 2),
                                                 0, 0,
                                                 new Size(scaledX, scaledY),
                                                 CopyPixelOperation.SourceCopy);
                            }
                            bmpScreenCapture.Save($"scoped/{imageCtrScoped}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        imageCtrScoped++;
                    }
                    else
                    {
                        using (Bitmap bmpScreenCapture = new Bitmap(scaledX, scaledY))
                        {
                            using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                            {
                                g.CopyFromScreen((sSize.Width / 2) - (scaledX / 2), (sSize.Height / 2) - (scaledY / 2),
                                                 0, 0,
                                                 new Size(scaledX, scaledY),
                                                 CopyPixelOperation.SourceCopy);
                            }
                            bmpScreenCapture.Save($"unscoped/{imageCtrUnscoped}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        imageCtrUnscoped++;
                    }
                    sw.Restart();
                }
                if (GetAsyncKeyState(Keys.RButton) != 0)
                {
                    scoped = !scoped;
                    System.Threading.Thread.Sleep(500);
                    sw.Restart();
                }
                System.Threading.Thread.Sleep(1);
            }
            
        }
    }
}
