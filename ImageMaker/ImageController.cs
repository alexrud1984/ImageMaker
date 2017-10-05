using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ImageMaker
{
    class ImageController
    {
        private IImageViewer imageViewer;
        private IModel model;

        public void Init(IImageViewer imageViewer)
        {
            this.imageViewer = imageViewer;
            model = new Model();
            imageViewer.PossibleFileFormatList = model.PossibleFileFormatList;
            AttachImageViewer();
            AttchModel();
        }

        private void AttchModel()
        {
            model.ImageOpenedEvent += Model_ImageOpenedEvent;
            model.ImageSavedEvent += Model_ImageSavedEvent;
            model.ExceptionEvent += Model_ExceptionEvent;
        }

        private void Model_ExceptionEvent(string msg)
        {
            imageViewer.ShowMessage(msg);
        }

        private void Model_ImageSavedEvent()
        {
            imageViewer.ShowMessage("File is saved.");
        }

        private void Model_ImageOpenedEvent()
        {
            imageViewer.ShowImage(model.CurrenImage);
            imageViewer.SetTags(model.Tags);
            imageViewer.OpenedFileName = model.CurrentFileInfo.Name;
        }

        private void AttachImageViewer()
        {
            imageViewer.ImageOpenEvent += ImageViewer_ImageOpen;
            imageViewer.ImageSaveEvent += ImageViewer_ImageSave;
        }

        private void ImageViewer_ImageSave(string filePath)
        {
            model.Tags = imageViewer.GetTags();
            model.SaveImage(filePath);
        }

        private void ImageViewer_ImageOpen(string filePath)
        {
            model.OpenImage(filePath);
        }
    }
}
