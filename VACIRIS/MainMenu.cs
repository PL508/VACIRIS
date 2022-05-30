using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Firebase.Storage;
using System.Threading.Tasks;
using System.Threading;
using FireSharp.Config;
using FireSharp.Response;
using System.IO.Compression;
using System.Net;

namespace VACIRIS
{
    public partial class MainMenu : Form
    {
        public static string fileOpenPath = string.Empty;

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
        public bool isMinimized = false;
        login cur_form;
        private IconButton currentBtn;
        private Panel leftBorderBtn;
        private Form currentChildForm;

        public MainMenu(login frmlogin)
        {
            cur_form = frmlogin;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            UserLabel.Text = Truncate(login.username, 26);
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 50);
            panelMenu.Controls.Add(leftBorderBtn);
            ActivateButton(dbBtn, RGBColors.color1);
            OpenChildForm(new dashboard());
        }

        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(172, 126, 241);
            public static Color color2 = Color.FromArgb(249, 118, 176);
            public static Color color3 = Color.FromArgb(253, 138, 114);
            public static Color color4 = Color.FromArgb(24, 161, 251);
            public static Color color5 = Color.FromArgb(4, 204, 20);
        }

        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null)
            {
                DisableButton();
                //Button
                currentBtn = (IconButton)senderBtn;
                currentBtn.BackColor = Color.FromArgb(37, 36, 81);
                currentBtn.ForeColor = color;
                currentBtn.TextAlign = ContentAlignment.MiddleCenter;
                currentBtn.IconColor = color;
                currentBtn.TextImageRelation = TextImageRelation.TextBeforeImage;
                currentBtn.ImageAlign = ContentAlignment.MiddleRight;
                //Left border button
                leftBorderBtn.BackColor = color;
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();
                //Current Child Form Icon
                iconCurrentChildForm.IconChar = currentBtn.IconChar;
                iconCurrentChildForm.IconColor = color;
            }
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
                currentChildForm.Dispose();
            }
            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelDesktop.Controls.Add(childForm);
            panelDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
            labelCurrentChildForm.Text = childForm.Text;
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = Color.FromArgb(39, 49, 71);
                currentBtn.ForeColor = Color.White;
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;
                currentBtn.IconColor = Color.White; 
                currentBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
                currentBtn.ImageAlign = ContentAlignment.MiddleLeft;
            }
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            cur_form.Show();
            this.Dispose();
        }

        private void MainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void dbBtn_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color1);
            OpenChildForm(new dashboard());
        }

        private void regBtn_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color2);
            OpenChildForm(new face_registration());
        }

        private void detectBtn_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color3);
            OpenChildForm(new face_recognition());
        }

        private void settingBtn_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);
            OpenChildForm(new settings());
        }

        private void helpBtn_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color5);
            OpenChildForm(new help());
        }

        private void Update()
        {
            string startPath = "deepface/images";
            string zipPath = "deepface/images.zip";
            if (File.Exists(zipPath) == true) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(startPath, zipPath, 0, true);

            var stream = File.Open("deepface/images.zip", FileMode.Open);

            var task = new FirebaseStorage("vaciris-4a6d4.appspot.com").Child("images.zip").PutAsync(stream);
        }
    }
}
