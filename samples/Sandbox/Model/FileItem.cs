using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;

namespace Sandbox.Model
{
    internal class FileItem
    {
        internal FileItem (string fileName)
        {
            FileName = fileName;
        }

        internal string FileName { get; init; }

        internal Bitmap Icon
        {
            get {
            
                var shellFile = ShellFile.FromFilePath(FileName);
                var thumbNail = shellFile.Thumbnail.ExtraLargeBitmap;

                using var ms = new MemoryStream();

                thumbNail.Save(ms, ImageFormat.Png);

                return new Bitmap(ms);
            }
        } 
    }
}
