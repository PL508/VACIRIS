using System;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;

namespace VACIRIS
{
    public partial class settings : Form
    {
        public settings()
        {
            InitializeComponent();
        }

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

        FilterInfoCollection filter;
        VideoCaptureDevice device;
        public static int choose = 0;
        public static int max_img = 50;
        public static int theme = 1;

        private void settings_Load(object sender, EventArgs e)
        {
            maxImages.Value = max_img;
            string date = File.ReadAllText("dt_date.txt");
            update_day.Text = "Last checked: " + date;
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo device in filter)
            {
                cboDevices.Items.Add(device.Name);
            }
            cboDevices.SelectedIndex = choose;
            device = new VideoCaptureDevice();
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            testBtn.Visible = false;
            device = new VideoCaptureDevice(filter[choose].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.Start();
        }

        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictCam.Image = bitmap; 
        }

        private void settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device.IsRunning)
            {
                device.Stop();
            }
        }

        private void cboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            choose = cboDevices.SelectedIndex;
            if (device != null && device.IsRunning)
            {
                device.Stop();
                device = new VideoCaptureDevice(filter[choose].MonikerString);
                device.NewFrame += Device_NewFrame;
                device.Start();
            }
        }

        private void maxImages_ValueChanged(object sender, EventArgs e)
        {
            max_img = ((int)maxImages.Value);
        }

        private void sysSetCB_CheckedChanged(object sender, EventArgs e)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var registryValueObject = key?.GetValue("AppsUseLightTheme");

            if (registryValueObject == null) theme = 0;
            else theme = 1;
        }

        private void updateData()
        {
            if (CheckInternetConnection())
            {
                string[] new_date = {
                    DateTime.Now.ToString()
                };
                update_day.Text = "Last checked: " + DateTime.Now.ToString();

                File.WriteAllLines("dt_date.txt", new_date);
            }
            else
            {
                string date = File.ReadAllText("dt_date.txt");
                update_day.Text = "Last checked: " + date;
            }
            return;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string startPath = "deepface/database";
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                if (CheckInternetConnection())
                {
                    wc.DownloadFile("https://firebasestorage.googleapis.com/v0/b/vaciris-4a6d4.appspot.com/o/representations_arcface.pkl?alt=media&token=7af17a1e-ed31-4ed1-b071-877d8ddfbb55", startPath + "/representations_arcface.pkl");
                    updateData();
                    MessageBox.Show("Successfully updated.", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("A file error has occurred.\nPlease check your network connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
