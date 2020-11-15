using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinConsole.Misc {
    class MakeGridImage : Runnable {
        private static readonly string FILE_NAME = @"C:\Users\evans\Documents\Map Lines\Test\TestGrid.png";

        public override void Main(string[] args) {
            Console.WriteLine("MakeGridImage: Run at " + DateTime.Now);
            Image image = new Bitmap(1001, 1001);
            using (Graphics g = Graphics.FromImage(image))
            using (var pen0 = new Pen(Color.FromArgb(59, 159, 190), 1))
            using (var pen1 = new Pen(Color.FromArgb(19, 99, 141), 1)) {
                g.Clear(Color.White);
                for (int i = 0; i < 11; i++) {
                    g.DrawLine(pen1, 0, 100 * i, 1000, 100 * i);
                    g.DrawLine(pen1, 100 * i, 0, 100 * i, 1000);
                    if (i < 10) {
                        for (int j = 1; j < 10; j++) {
                            g.DrawLine(pen0, 0, 100 * i + 10 * j, 1000, 100 * i + 10 * j);
                            g.DrawLine(pen0, 100 * i + 10 * j, 0, 100 * i + 10 * j, 1000);

                        }
                    }
                }
            }
            image.Save(FILE_NAME, ImageFormat.Png);
            Console.WriteLine("Wrote " + FILE_NAME);
            Console.WriteLine("All done");
        }
    }
}
