using KEUtils.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace WinConsole.Misc {
    /// <summary>
    /// Class to get information about the images in .ico files.
    /// </summary>
    class IconInfo : Runnable {
        private static readonly string[] fileNames = {
            @"C:\Users\evans\Pictures\Icons\Numbers.ico",
            @"C:\Users\evans\Pictures\Icons\Console.ico",
            @"C:\Users\evans\Pictures\Icons\Console1.ico",
            @"C:\Users\evans\Pictures\Icons\Console2.ico",
            @"C:\Users\evans\Pictures\Icons\Console3.ico",
            @"C:\Users\evans\Pictures\Icons\Console4.ico",
            @"C:\Users\evans\Pictures\Icons\Console5.ico",
            @"C:\Users\evans\Pictures\Icons\Console6.ico",
            @"C:\Users\evans\Pictures\Icons\Console7.ico",
            @"C:\Users\evans\Pictures\Icons\Console8.ico",
            @"C:\Users\evans\Pictures\Icons\Console9.ico",
            @"C:\Users\evans\Pictures\Icons\Console10.ico",
            @"C:\Users\evans\Pictures\Icons\BlueMouse.ico",
            @"C:\Users\evans\Pictures\Icons\ArtPad.ico",
            @"C:\Users\evans\Pictures\Icons\GPXInspector.ico",
            @"C:\Users\evans\Pictures\Icons\GPXInspectorMagnifyingGlass.ico",
            @"C:\Users\evans\Pictures\Icons\FontViewer.ico",
        };

        List<Header> headers = new List<Header>();
        public short IconType { get; set; }
        public short NImages { get; set; }

        private void readIconDir(BinaryReader br) {
            br.ReadInt16(); // Reserved
            IconType = br.ReadInt16();
            NImages = br.ReadInt16();
            Console.WriteLine(((IconType == 1) ? "Icon" : "Cursor")
                + " with " + NImages + " images");
        }

        private void getTypes(string fileName) {
            int byte1, byte2;
            using (FileStream fs = File.Open(fileName, FileMode.Open)) {
                foreach (Header header in headers) {
                    fs.Position = header.Offset;
                    byte1 = fs.ReadByte();
                    byte2 = fs.ReadByte();
                    if (byte1 == 0x89 && byte2 == 0x50) {
                        header.Type = "PNG";
                    } else {
                        header.Type = "Bitmap";
                    }
                    //Console.WriteLine($"offset={header.Offset} " +
                    //    $"position={fs.Position} " +
                    //    $"byte1={byte1:X2} byte2={byte2:x2}");
                }
            }
        }

        public override void Main(string[] args) {
            // Get the directory entries
            Header header;
            foreach (string fileName in fileNames) {
                try {
                    headers = new List<Header>();
                    using (BinaryReader br = new BinaryReader(File.Open(fileName,
                        FileMode.Open))) {
                        Console.WriteLine(fileName);
                        Console.WriteLine("Size: " + formatFileSize(fileName));
                        readIconDir(br);
                        for (int i = 0; i < NImages; i++) {
                            header = new Header(br);
                            headers.Add(header);
                        }
                    }
                    getTypes(fileName);
                    foreach (Header header1 in headers) {
                        //Console.Write(header1.HeaderInfo());
                        Console.Write(header1.HeaderInfoShort());
                    }
                    Console.WriteLine();
                } catch (Exception ex) {
                    Utils.excMsg("Processing failed for " + fileName, ex);
                }
            }
        }

        public static string formatFileSize(string fileName) {
            double bytes = new FileInfo(fileName).Length;
            double kbytes = bytes / 1024.0;
            double mbytes = kbytes / 1024.0;
            double gbytes = mbytes / 1024.0;
            return String.Format("{0:0.00} GB, {1:0.00} MB, {2:0.00} KB, {3:0} bytes",
                         gbytes, mbytes, kbytes, bytes);
        }

    }

    public class Header {
        public static readonly string NL = Environment.NewLine;
        public byte Width { get; set; }
        public byte Height { get; set; }
        public byte NColors { get; set; }
        public short ColorPlanes { get; set; }
        public short BitsPerPixel { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }
        public string Type { get; set; } = "Unknown";

        public Header(BinaryReader br) {
            Width = br.ReadByte();
            Height = br.ReadByte();
            NColors = br.ReadByte();
            br.ReadByte(); // Reserved
            ColorPlanes = br.ReadInt16();
            BitsPerPixel = br.ReadInt16();
            Size = br.ReadInt32();
            Offset = br.ReadInt32();
        }

        public string HeaderInfo() {
            string msg = "";
            int width, height;
            // 0 means 256 for height and width
            if (Width == 0) width = 256; else width = Width;
            if (Height == 0) height = 256; else height = Height;
            msg += Type + " " + width + "x" + height + " nColors=" + NColors +
                " colorPlanes=" + ColorPlanes
                + " bitsPerPixel=" + BitsPerPixel + NL;
            msg += $"Size={Size} Offset={Offset}" + NL;
            return msg;
        }
        public string HeaderInfoShort() {
            int width, height;
            if (Width == 0) width = 256; else width = Width;
            if (Height == 0) height = 256; else height = Height;
            string type;
            switch (BitsPerPixel) {
                case 4:
                    type = "16";
                    break;
                case 8:
                    type = "256";
                    break;
                case 32:
                    type = "RGB/A";
                    break;
                default:
                    type = "Unknown";
                    break;
            }
            String size = $"{width}x{height}";
            return $"{type,-7}\t{size,7}\t{Type}" + NL;
        }
    }
}
