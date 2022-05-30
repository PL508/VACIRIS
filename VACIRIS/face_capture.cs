using System;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VACIRIS
{
    public partial class face_capture : Form
    {
        string dir = "";
        int max_img;
        public face_capture(string di, int max)
        {
            dir = di;
            max_img = max;
            InitializeComponent();
        }

        FilterInfoCollection filter;
        VideoCaptureDevice device;

        static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt2.xml");

        private void FaceCapture_Load(object sender, EventArgs e)
        {
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            device = new VideoCaptureDevice(filter[settings.choose].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.Start();
        }

        public int x, y, w, h, i = 0;

        public static Bitmap cropAtRect(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }

        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // 640, 360
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            Image<Bgr, byte> grayImage = new Image<Bgr, byte>(bitmap);
            Rectangle[] rectangles = cascadeClassifier.DetectMultiScale(grayImage, 1.3, 4);
            foreach (var rectangle in rectangles)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    x = rectangle.X; y = rectangle.Y;
                    w = rectangle.Width; h = rectangle.Height;
                    if (mainPanel.InvokeRequired)
                    {
                        if (i <= max_img)
                        {
                            i++;
                            mainPanel.Invoke(new MethodInvoker(delegate {
                                var bmp = new Bitmap(x + w, y + h);
                                mainPanel.DrawToBitmap(bmp, new Rectangle(0, 0, x + w, y + h));

                                bmp = cropAtRect(bmp, new Rectangle(x, y, x + w, y + h));
                                bmp = cropAtRect(bmp, new Rectangle(0, 0, w, h));

                                string path = dir + i.ToString() + ".jpg";
                                bmp.Save(path);
                            }));
                        }
                        else
                        {
                            if (File.Exists(dir + "1.jpg")) File.Delete(dir + "1.jpg"); 
                            Font arialFont = new Font("Arial", 20, FontStyle.Bold);
                            PointF location = new PointF(10f, 10f);
                            graphics.DrawString("Successfully scanned.\nPlease close the window.", arialFont, Brushes.Green, location);
                        }
                    }
                }
            }
            picBox.Image = bitmap;
        }

        private void FaceCapture_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(device.IsRunning)
            {
                device.Stop();
            }
        }
    }
}
