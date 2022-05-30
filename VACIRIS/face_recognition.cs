using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using FireSharp;
using FireSharp.Response;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using FireSharp.Config;
using System.Threading;
using System.Media;
using System.Net;

namespace VACIRIS
{
    public partial class face_recognition : Form
    {
        public face_recognition()
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

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "2vbfIn8LCaXR1HvSBjD71JX5Zw5eJGX5YxFfT2ND",
            BasePath = "https://vaciris-4a6d4-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };

        IFirebaseClient client;

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            Directory.Delete(target_dir, false);
        }

        private void face_recognition_Load(object sender, EventArgs e)
        {
            if (MainMenu.fileOpenPath != "") OpenFile(MainMenu.fileOpenPath);
            try
            {
                client = new FirebaseClient(ifc);
            }
            catch
            {
                MessageBox.Show("A file error has occurred.\nPlease check your network connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void face_recognition_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.S))
            {
                MessageBox.Show("Successfully saved.", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string filepath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if(Directory.Exists(filepath + @"\deepface\rtdata\") == true)
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(filepath + @"\deepface\rtdata\");
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            Directory.CreateDirectory(filepath + @"\deepface\rtdata\");
            face_capture FC = new face_capture(filepath + @"\deepface\rtdata\", settings.max_img);
            FC.ShowDialog();

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = filepath + @"\Python39\python.exe";
            start.WorkingDirectory = "deepface";
            start.Arguments = filepath + @"\deepface\faces.py";
            start.UseShellExecute = true;
            using (Process process = Process.Start(start))
            {
                process.WaitForExit();
            }
            List<String> collect = new List<string>();
            collect = File.ReadAllLines("deepface/collect.txt").ToList();

            Dictionary<string, int> list = new Dictionary<string, int>();

            int res = 0; string res_path = "";

            foreach (string i in collect)
            {
                list.TryGetValue(i, out int currentCount);
                list[i] = currentCount + 1;
            }
            foreach (string i in collect) {res = Math.Max(res, list[i]);}
            foreach (string i in collect)
            {
                if (res == list[i])
                {
                    res_path = i;
                    break;
                }
            }
            if (res_path != "null" && res_path != "" && CheckInternetConnection() == true)
            {
                int temp = settings.max_img / 2 + 1;
                LiveUpdate(res_path);
                picBox.ImageLocation = "deepface/rtdata/" + temp.ToString() + ".jpg";
                MessageBox.Show("Successfully detected.", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                picBox.ImageLocation = "/error.jpg";
                txtName.Text = "Unknown"; txtBirth.Text = "Unknown"; txtGender.Text = "Unknown";
                txtPhone.Text = "Unknown"; txtCCCD.Text = "Unknown"; txtVType.Text = "Unknown";
                txtVDate.Text = "Unknown"; txtInjections.Text = "Unknown"; txtVAddress.Text = "Unknown";
                MessageBox.Show("No face was found.\nPlease check your device and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LiveUpdate(string path)
        {
            var res = (dynamic) null;
            while(res == null) res = client.Get(path);
            UserData ud = res.ResultAs<UserData>();
            txtName.Text = ud.fullname; txtBirth.Text = ud.date_of_birth;
            txtGender.Text = ud.gender; txtPhone.Text = ud.phone_number; txtCCCD.Text = ud.CCCD;
            txtInjections.Text = ud.number_of_injections; txtVDate.Text = ud.vaccination_date; 
            txtVType.Text = ud.vaccine_type; txtVAddress.Text = ud.vaccination_address;
            string[] atr = new string[] {
                DateTime.Now.ToString(), ud.fullname, ud.date_of_birth, ud.gender,
                ud.phone_number, ud.CCCD, ud.address, ud.vaccine_type, ud.vaccination_date,
                ud.number_of_injections, ud.lotNum, ud.vaccination_address
            };
            GridView.Rows.Add(atr);
        }

        private void OpenFile(string Path)
        {
            try {
                GridView.Rows.Clear();
                string[] lines = File.ReadAllLines(Path);
                string[] data;
                for (int i = 0; i < lines.Length; i++)
                {
                    data = lines[i].ToString().Split(',');
                    string[] row = new string[data.Length];

                    for (int j = 0; j < data.Length; j++) row[j] = data[j].Trim();

                    GridView.Rows.Add(row);
                }
                chOpen.Text = "File is opening";
            }
            catch {GridView.Rows.Clear();}
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MainMenu.fileOpenPath = openFileDialog.FileName;
                    OpenFile(MainMenu.fileOpenPath);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if(chOpen.Text == "File is opening")
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = "c:\\";
                    saveFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv|All file (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (TextWriter tw = new StreamWriter(saveFileDialog.FileName))
                        {
                            for (int i = 0; i < GridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < GridView.Columns.Count; j++)
                                {
                                    tw.Write($"{GridView.Rows[i].Cells[j].Value.ToString() + ","}");
                                }
                                tw.WriteLine();
                            }
                            GridView.Rows.Clear();
                            MainMenu.fileOpenPath = string.Empty;
                            chOpen.Text = "File is not open";
                            MessageBox.Show("Successfully exported to " + saveFileDialog.FileName + ".", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            else
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = "c:\\";
                    saveFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SystemSounds.Exclamation.Play();
                        MainMenu.fileOpenPath = saveFileDialog.FileName;
                        using (TextWriter tw = new StreamWriter(MainMenu.fileOpenPath))
                        {
                            for (int i = 0; i < GridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < GridView.Columns.Count; j++)
                                {
                                    tw.Write($"{GridView.Rows[i].Cells[j].Value.ToString() + ","}");
                                }
                                tw.WriteLine();
                            }
                        }
                        chOpen.Text = "File is opening";
                    }
                }
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = "c:\\";
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    GridView.Rows.Clear();
                    MainMenu.fileOpenPath = saveFileDialog.FileName;
                    chOpen.Text = "File is opening";
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (chOpen.Text == "File is opening")
            {
                SystemSounds.Exclamation.Play();
                using (TextWriter tw = new StreamWriter(MainMenu.fileOpenPath))
                {
                    for (int i = 0; i < GridView.Rows.Count; i++)
                    {
                        for (int j = 0; j < GridView.Columns.Count; j++)
                        {
                            tw.Write($"{GridView.Rows[i].Cells[j].Value.ToString() + ","}");
                        }
                        tw.WriteLine();
                    }
                }
                chOpen.Text = "File is opening";
            }
            else
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = "c:\\";
                    saveFileDialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SystemSounds.Exclamation.Play();
                        MainMenu.fileOpenPath = saveFileDialog.FileName;
                        using (TextWriter tw = new StreamWriter(MainMenu.fileOpenPath))
                        {
                            for (int i = 0; i < GridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < GridView.Columns.Count; j++)
                                {
                                    tw.Write($"{GridView.Rows[i].Cells[j].Value.ToString() + ","}");
                                }
                                tw.WriteLine();
                            }
                        }
                        chOpen.Text = "File is opening";
                    }
                }
            }
        }
    }
}
