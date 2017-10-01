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
        public ImageViewer()
        {
            ImageControllerSingleton pictureController = ImageControllerSingleton.Instance;
            pictureController.Init(this);
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
        public Image CurrentImage { set; get; }
        public string openedFileName { set; get; }
        public List<string> TagsList
        {
            get
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
            set
            {
                tagsTextBoxList.Clear();
                tagsFlowLayoutPanel.Controls.Clear();
                foreach (var tag in value)
                {
                    TextBox tb = CreateTagTextBox();
                    tb.Text = tag;
                    tagsTextBoxList.Add(tb);
                    tagsFlowLayoutPanel.Controls.Add(tb);
                }
            }
        }

        private TextBox CreateTagTextBox()
        {
            TextBox tb = new TextBox();
            tb.Font = new Font(new FontFamily("Segoe Print"), 12);
            tb.Width = tagsFlowLayoutPanel.Width - 10;
            tb.KeyDown += TagTextBox_KeyDown;
            return tb;
        }

        private void TagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null && e.KeyData == Keys.Enter)
            {
                addTagButton_Click(sender, e);
                tagsTextBoxList.Last().Focus();
            }
        }

        public void ShowImage()
        {
            if (CurrentImage != null)
            {
                pictureBox.Image = CurrentImage;
            }
        }
        public void ErrorMessage(string msg)
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

        private string GetPossibleFileFormatListOpen(List<string> possibleFileFormatList)
        {
            string resultString = "(";
            foreach (var listItem in possibleFileFormatList)
            {
                resultString = String.Concat(resultString, String.Format("*{0},", listItem));
            }
            resultString = String.Format("{0})|", resultString.Substring(0, resultString.Length - 1));
            foreach (var listItem in possibleFileFormatList)
            {
                resultString = String.Concat(resultString, String.Format("*{0};", listItem));
            }
            resultString = resultString.Substring(0, resultString.Length-1);
            return resultString;
        }

        private string GetPossibleFileFormatListSave(List<string> possibleFileFormatList)
        {
            string resultString = String.Empty;
            foreach (var listItem in possibleFileFormatList)
            {
                resultString = String.Concat(resultString, String.Format("(*{0})|*{0}|", listItem));
            }
            resultString = resultString.Substring(0, resultString.Length - 1);
            return resultString;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = GetPossibleFileFormatListOpen(PossibleFileFormatList);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                OnPictureOpen(ofd.FileName);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = GetPossibleFileFormatListSave(PossibleFileFormatList);
            sfd.FileName = openedFileName; 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                OnPictureSave(sfd.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void addTagButton_Click(object sender, EventArgs e)
        {
            if (tagsTextBoxList.Count<10)
            {
                TextBox tb = CreateTagTextBox();
                tagsTextBoxList.Add(tb);
                tagsFlowLayoutPanel.Controls.Add(tagsTextBoxList.Last());
            }
        }
    }
}
