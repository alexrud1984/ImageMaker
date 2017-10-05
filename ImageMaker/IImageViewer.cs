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
        string OpenedFileName { set; get; }

        List<string> GetTags();
        void SetTags(List<string> tagsList);
        void ShowImage(Image image);
        void ShowMessage(string msg);

        event ImageOpenHandler ImageOpenEvent;
        event ImageSaveHandler ImageSaveEvent;
    }
}
