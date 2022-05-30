using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using FireSharp;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp.Config;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VACIRIS
{
    public partial class face_registration : Form
    {
        public face_registration()
        {
            InitializeComponent();
        }

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "2vbfIn8LCaXR1HvSBjD71JX5Zw5eJGX5YxFfT2ND",
            BasePath = "https://vaciris-4a6d4-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };

        IFirebaseClient client;

        private void face_registration_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FirebaseClient(ifc);
            }
            catch
            {
                MessageBox.Show("A file error has occurred.\nPlease check your network connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        private void submit_button_Click(object sender, EventArgs e)
        {
            if (fullname.Text == "" || date_of_birth.Text == "" || gender.Text == ""
                || phone_number.Text == "" || CCCD.Text == "" || vaccine_type.Text == ""
                || vaccination_date.Text == "" || vaccination_address.Text == ""
                || number_of_injections.Text == "")
            {
                MessageBox.Show("A file error has occurred.\nPlease carefully check the required information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string filepath = "deepface/images/" + phone_number.Text + "_" + CCCD.Text;
                if (Directory.Exists(filepath) == true)
                {
                    int fileCount = Directory.GetFiles(filepath, "*.jpg*", SearchOption.AllDirectories).Length;
                    if (fileCount >= 25)
                    {
                        if (address.Text == "") address.Text = "NULL";
                        if (lotNum.Text == "") lotNum.Text = "NULL";
                        UserData ud = new UserData
                        {
                            fullname = fullname.Text.ToString(),
                            date_of_birth = date_of_birth.Text.ToString(),
                            gender = gender.Text.ToString(),
                            phone_number = phone_number.Text.ToString(),
                            CCCD = CCCD.Text.ToString(),
                            address = address.Text.ToString(),
                            vaccine_type = vaccine_type.Text.ToString(),
                            vaccination_date = vaccination_date.Text.ToString(),
                            number_of_injections = number_of_injections.Text.ToString(),
                            lotNum = lotNum.Text.ToString(), vaccination_address = vaccination_address.Text.ToString()
                        };
                        string Path = phone_number.Text.ToString() + "_" + CCCD.Text.ToString();
                        var set = client.Set(Path, ud);
                        if (address.Text == "NULL") address.Text = "";
                        if (lotNum.Text == "NULL") lotNum.Text = "";
                        MessageBox.Show("Successfully saved.", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("A file error has occurred.\nPlease carefully check the required information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No face was found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void clear_content_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to continue?", "VACIRIS",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if(phone_number.Text != "" && CCCD.Text != "")
                {
                    string filepath = "deepface/images/" + phone_number.Text + "_" + CCCD.Text + "/";
                    if (Directory.Exists(filepath) == true)
                    {
                        bool result = await check(phone_number.Text + "_" + CCCD.Text);
                        if (result == false) DeleteDirectory(filepath);
                    }
                }
                fullname.Text = ""; date_of_birth.Text = ""; gender.Text = ""; phone_number.Text = "";
                CCCD.Text = ""; address.Text = ""; lotNum.Text = ""; vaccine_type.Text = "";
                vaccination_date.Text = ""; vaccination_address.Text = ""; number_of_injections.Text = "";
                MessageBox.Show("Successfully cleared.", "VACIRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public async Task<bool> check(string Path)
        {
            FirebaseResponse res = await client.GetAsync(Path);
            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.Body.ToString());
            string k;
            try {k = data.ElementAt(5).Value;}
            catch {k = "HAHA";}
            if (k == "HAHA") return false;
            else return true;
        }

        private void face_scanning_Click(object sender, EventArgs e)
        {
            if (fullname.Text == "" || date_of_birth.Text == "" || gender.Text == ""
                || phone_number.Text == "" || CCCD.Text == "" || vaccine_type.Text == ""
                || vaccination_date.Text == "" || vaccination_address.Text == ""
                || number_of_injections.Text == "")
            {
                MessageBox.Show("A file error has occurred.\nPlease carefully check the required information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string new_dir = "deepface/images/" + phone_number.Text + "_" + CCCD.Text + "/";
                Directory.CreateDirectory(new_dir);
                face_capture FC = new face_capture(new_dir, 40);
                FC.ShowDialog();
            }
        }
    }
}
