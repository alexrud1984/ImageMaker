using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMaker
{
    public delegate void ImageOpenedHandler();
    public delegate void ImageSavedHandler();
    public delegate void ExceptionHandler(string msg);

    public interface IModel
    {
        Image CurrenImage { get; }
        FileInfo CurrentFileInfo { get; }
        List<string> Tags { set; get; }
        List<string> PossibleFileFormatList { get; }

        void OpenImage (string filePath);
        void SaveImage (string filePath);

        event ImageOpenedHandler ImageOpenedEvent;
        event ImageSavedHandler ImageSavedEvent;
        event ExceptionHandler ExceptionEvent;
    }
}
