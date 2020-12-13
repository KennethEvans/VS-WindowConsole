using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace WinConsole.Misc {
    /// <summary>
    /// Class to get information about the images in .ico files. It does not
    /// get the information that IconInfo does, but the method used could be 
    /// used to extract the images if needed.
    /// </summary>
    class IconInfo2 : Runnable {
        private static readonly string[] fileNames = {
            @"C:\Users\evans\Pictures\Icons\Console.ico",
        };

        public short IconType { get; set; }
        public short NImages { get; set; }

        public override void Main(string[] args) {
            foreach (string fileName in fileNames) {
                using (Stream iconStream = new FileStream(fileName, FileMode.Open)) {
                    Console.WriteLine("File Name=" + fileName);
                    IconBitmapDecoder decoder = new IconBitmapDecoder(
                        iconStream,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.None);
                    Console.WriteLine("NFrames=" + decoder.Frames.Count);
                    foreach (var item in decoder.Frames) {
                        //Do whatever you want to do with the single images inside the file
                        Console.WriteLine(item.Width + "x" + item.Height);
                    }
                }
            }
        }
    }
}
