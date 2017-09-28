using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMaker
{
    public delegate void ImageOpenHandler(string filePath);
    public delegate void ImageSaveHandler(string filePath); 
    public interface IImageViewer
    {
        List<string> PossibleFileFormatList { set; get; }
        Image CurrentImage { set; get; }
        string openedFileName { set; get; }
        string Tags { set; get; }

        void ShowImage();
        void ErrorMessage(string msg);

        event ImageOpenHandler ImageOpenEvent;
        event ImageSaveHandler ImageSaveEvent;
    }
}
