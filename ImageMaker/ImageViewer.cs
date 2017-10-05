using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ImageMaker
{
    public partial class ImageViewer : Form, IImageViewer
    {
        enum Operation
        {
            Open,
            Save
        }
        public ImageViewer()
        {
            ImageControllerSingleton imageController = ImageControllerSingleton.Instance;
            imageController.Init(this);
            InitializeComponent();
        }

        private List<string> possibleFileFormatList = new List<string>();
        private List<TextBox> tagsTextBoxList = new List<TextBox>();

        public List<string> PossibleFileFormatList
        {
            set
            {
                possibleFileFormatList = value;
            }
            get
            {
                return possibleFileFormatList;
            }
        }
        public string OpenedFileName { set; get; }
        public List<string> GetTags()
        {
            List<string> tagsStringList = new List<string>();
            foreach (var tagTextBox in tagsTextBoxList)
            {
                if (!String.IsNullOrEmpty(tagTextBox.Text))
                {
                    tagsStringList.Add(tagTextBox.Text);
                }
            }
            return tagsStringList;
        }
        public void SetTags(List<string> tagsList)
        {
            tagsTextBoxList.Clear();
            tagsFlowLayoutPanel.Controls.Clear();
            foreach (var tag in tagsList)
            {
                TextBox tagTextBox = CreateFormatedTextBox();
                tagTextBox.Text = tag;
                tagsTextBoxList.Add(tagTextBox);
                tagsFlowLayoutPanel.Controls.Add(tagTextBox);
            }
        }      

        private TextBox CreateFormatedTextBox()
        {
            TextBox tagTextBox = new TextBox();
            tagTextBox.Font = new Font(new FontFamily("Segoe Print"), 12);
            tagTextBox.Width = tagsFlowLayoutPanel.Width - 10;
            tagTextBox.KeyDown += TagTextBox_KeyDown;
            return tagTextBox;
        }

        public void ShowImage(Image image)
        {
            pictureBox.Image = image;
        }
        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }


        public event ImageOpenHandler ImageOpenEvent;
        public event ImageSaveHandler ImageSaveEvent;

        private void OnPictureOpen (string filePath)
        {
            if (ImageOpenEvent != null)
            {
                ImageOpenEvent(filePath);
            }
        }

        private void OnPictureSave (string filePath)
        {
            if (ImageSaveEvent != null)
            {
                ImageSaveEvent(filePath);
            }
        }

        private string GetPossibleFileFormatList(List<string> possibleFileFormatList, Operation operation)
        {
            string resultString = String.Empty;
            if (operation.Equals(Operation.Open))
            {
                resultString = "(";
                foreach (var listItem in possibleFileFormatList)
                {
                    resultString = String.Concat(resultString, String.Format("*{0},", listItem));
                }
                resultString = String.Format("{0})|", resultString.Substring(0, resultString.Length - 1));
                foreach (var listItem in possibleFileFormatList)
                {
                    resultString = String.Concat(resultString, String.Format("*{0};", listItem));
                }
            }
            else if (operation.Equals(Operation.Save))
            {
                foreach (var listItem in possibleFileFormatList)
                {
                    resultString = String.Concat(resultString, String.Format("(*{0})|*{0}|", listItem));
                }
            }
            resultString = resultString.Substring(0, resultString.Length-1);
            return resultString;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = GetPossibleFileFormatList(PossibleFileFormatList,Operation.Open);
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OnPictureOpen(openFileDialog.FileName);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = GetPossibleFileFormatList(PossibleFileFormatList,Operation.Save);
            saveFileDialog.FileName = OpenedFileName;
            for (int  i=0; i<possibleFileFormatList.Count;i++)
            {
                if (possibleFileFormatList[i].Contains(OpenedFileName.Split('.').Last()))
                {
                    saveFileDialog.FilterIndex = i+1;
                }
            }
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                OnPictureSave(saveFileDialog.FileName);
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AddTagButton_Click(object sender, EventArgs e)
        {
            if (tagsTextBoxList.Count<10)
            {
                TextBox tb = CreateFormatedTextBox();
                tagsTextBoxList.Add(tb);
                tagsFlowLayoutPanel.Controls.Add(tagsTextBoxList.Last());
                tagsTextBoxList.Last().Focus();
            }
        }

        private void TagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null && e.KeyData == Keys.Enter)
            {
                AddTagButton_Click(sender, e);
            }
        }

    }
}
