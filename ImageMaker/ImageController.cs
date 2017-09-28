using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMaker
{
    class ImageController
    {
        private IImageViewer imageViewer;
        private FileInfo openedImageFileInfo;
        public List<string> PossibleFileFormatList { private set; get; }

        public void Init(IImageViewer ImageViewer)
        {
            PossibleFileFormatList = new List<string> {".jpg", ".png", ".bin"};
            this.imageViewer = ImageViewer;
            ImageViewer.PossibleFileFormatList = this.PossibleFileFormatList;
            AttachImageViewer();
        }

        private void AttachImageViewer()
        {
            imageViewer.ImageOpenEvent += ImageViewer_imageOpen;
            imageViewer.ImageSaveEvent += ImageViewer_imageSave;
        }

        private void ImageViewer_imageSave(string filePath)
        {
            FileInfo savePictureFileInfo = new FileInfo(filePath);
            try
            {
                List<string> tagsList = new List<string>();
                tagsList.Add(imageViewer.Tags);
                AttachTagsToImage(tagsList, imageViewer.CurrentImage);
                switch (savePictureFileInfo.Extension)
                {
                    case ".png":
                        imageViewer.CurrentImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".jpg":
                        imageViewer.CurrentImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".bin":
                        SaveImageToBinary(filePath);
                        break;
                    default: break;
                }
            }
            catch
            {
                imageViewer.ErrorMessage("Cannot save file.");
            }
        }

        private void SaveImageToBinary(string filePath)
        {
            Stream imageStream = ImageToStream(imageViewer.CurrentImage);
            using (FileStream fileStream = File.Create(filePath, 2048, FileOptions.None))
            {
                BinaryWriter writer = new BinaryWriter(fileStream);
                byte[] buffer = new byte[32 * 2048];
                int bytesRead;
                while ((bytesRead = imageStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    writer.Write(buffer);
                }
            }
        }

        private void ImageViewer_imageOpen(string filePath)
        {
            if(File.Exists(filePath))
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var memoryStream = new MemoryStream((int)fileStream.Length);
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        imageViewer.CurrentImage = Image.FromStream(memoryStream);
                    }
                    openedImageFileInfo = new FileInfo(filePath);
                    imageViewer.openedFileName = new FileInfo(filePath).Name;
                }
                catch
                {
                    imageViewer.ErrorMessage("Wrong file type of file doesn't exist.");
                }
                imageViewer.ShowImage();
                imageViewer.Tags = GetImageTags(imageViewer.CurrentImage, openedImageFileInfo.Extension);
            }
        }

        private Stream ImageToStream(Image image)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
            return stream;
        }

        private string GetImageTags(Image image, string extension)
        {
            PropertyItem tagPropertyItem = GetPropertyItemByID(image, 0x9286);
            string tags = String.Empty;
            if (tagPropertyItem != null)
            {
                switch(extension)
                {
                    case ".png":
                        tags = System.Text.Encoding.Default.GetString(tagPropertyItem.Value);
                        break;
                    default:
                        List<byte> tagByteList = new List<byte>();
                        for (int i = 0; i<tagPropertyItem.Value.Length; i+=2)
                        {
                            tagByteList.Add(tagPropertyItem.Value[i]);
                        }
                        byte[] tagByteArray = tagByteList.ToArray();
                        tags = System.Text.Encoding.Default.GetString(tagByteArray);
                        break;
                }
            }
            return tags;
        }

        private PropertyItem GetPropertyItemByID(Image img, int id)
        {
            return img.PropertyItems.Select(x => x).FirstOrDefault(x => x.Id == id);
        }

        private void AttachTagsToImage(List<string> tagsList, Image image)
        {
            PropertyItem propItem = image.PropertyItems[0];
            string value = String.Empty;
            foreach (var tag in tagsList)
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = tag.ToString();
                }
                else
                {
                    value = String.Format("{0}, {1}", value, tag);
                }
            }
            propItem.Id = 0x9286;
            propItem.Type = 2;
            propItem.Value = System.Text.Encoding.UTF8.GetBytes(value + "\0");
            propItem.Len = propItem.Value.Length;
            image.SetPropertyItem(propItem);
        }   
    }
}
