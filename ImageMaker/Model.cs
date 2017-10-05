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
    public class Model:IModel
    {
        private List<string> possibleFileFormatList;
        private FileInfo currentFileInfo;
        private Image currentImage;

        public event ImageOpenedHandler ImageOpenedEvent;
        public event ImageSavedHandler ImageSavedEvent;
        public event ExceptionHandler ExceptionEvent;

        public List<string> PossibleFileFormatList
        {
            get
            {
                return possibleFileFormatList;
            }
        }
        public FileInfo CurrentFileInfo
        {
            get
            {
                return currentFileInfo;
            }
        }
        public Image CurrenImage
        {
            get
            {
                return currentImage;
            }
        }

        public List<string> Tags { set; get; }
        
        public Model()
        {
            possibleFileFormatList = new List<string> { ".jpg", ".png", ".bin" };
        }

        public void OpenImage(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    currentFileInfo = new FileInfo(filePath);
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        MemoryStream memoryStream = new MemoryStream((int)fileStream.Length);
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        if (currentFileInfo.Extension.Equals(".bin"))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            currentImage = (Image)formatter.Deserialize(memoryStream);
                        }
                        else
                        {
                            currentImage = Image.FromStream(memoryStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionEvent(String.Format("Cannot open file. Exception occurred:{0}", ex.Message));
                }
                ExtractImageTags();
                OnImageOpened();
            }
        }

        public void SaveImage(string filePath)
        {
            FileInfo savePictureFileInfo = new FileInfo(filePath);
            try
            {
                AttachTagsToImage();
                switch (savePictureFileInfo.Extension)
                {
                    case ".png":
                        currentImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".jpg":
                        currentImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".bin":
                        SaveImageToBinary(filePath);
                        break;
                    default: break;
                }
            }
            catch(Exception ex)
            {
                ExceptionEvent(String.Format("Cannot save file. Exception occurred:{0}", ex.Message));
            }
            OnImageSaved();
        }

        private void OnImageOpened()
        {
            if (ImageOpenedEvent!=null)
            {
                ImageOpenedEvent();
            }
        }

        private void OnImageSaved()
        {
            if (ImageSavedEvent != null)
            {
                ImageSavedEvent();
            }
        }

        private void OnException(string exMessage)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(exMessage);
            }
        }

        private void SaveImageToBinary(string filePath)
        {
            using (FileStream fileStream = File.Create(filePath))
            {
                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                currentImage.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                Image tempImage = Image.FromStream(memoryStream);
                formatter.Serialize(fileStream, tempImage);
            }
        }

        private void ExtractImageTags()
        {
            try
            {
                PropertyItem tagPropertyItem = GetPropertyItemByID(0x9286);
                string tagString = String.Empty;
                if (tagPropertyItem != null)
                {
                    switch (currentFileInfo.Extension)
                    {
                        case ".jpg":
                            List<byte> tagByteList = new List<byte>();
                            for (int i = 0; i < tagPropertyItem.Value.Length; i += 2)
                            {
                                tagByteList.Add(tagPropertyItem.Value[i]);
                            }
                            byte[] tagByteArray = tagByteList.ToArray();
                            tagString = System.Text.Encoding.Default.GetString(tagByteArray);
                            break;
                        default:
                            tagString = System.Text.Encoding.Default.GetString(tagPropertyItem.Value);
                            break;
                    }
                }
                Tags = tagString.Split('|').ToList();
            }
            catch (Exception ex)
            {
                ExceptionEvent(String.Format("Cannot extract tags from file. Exception occurred:{0}", ex.Message));
            }
        }

        private PropertyItem GetPropertyItemByID(int id)
        {
            return currentImage.PropertyItems.Select(x => x).FirstOrDefault(x => x.Id == id);
        }

        private void AttachTagsToImage()
        {
            PropertyItem propItem = currentImage.PropertyItems[0];
            string value = String.Empty;
            foreach (var tag in Tags)
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = tag.ToString();
                }
                else
                {
                    value = String.Format("{0}|{1}", value, tag);
                }
            }
            propItem.Id = 0x9286;
            propItem.Type = 2;
            propItem.Value = System.Text.Encoding.UTF8.GetBytes(value + "\0");
            propItem.Len = propItem.Value.Length;
            currentImage.SetPropertyItem(propItem);
        }
    }
}
