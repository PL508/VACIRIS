using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

namespace VACIRIS
{
    public partial class login : Form
    {
        public static string username = "";

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

        private void loginForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Username != "")
            {
                txtUsername.Text = Properties.Settings.Default.Username;
                txtPassword.Text = Properties.Settings.Default.Password;
            }
        }

        public login()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            this.ActiveControl = txtUsername;
            txtUsername.Focus();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            if(CheckInternetConnection())
            {
                if ((txtUsername.Text == "admin" && txtPassword.Text == "admin")
                || (txtUsername.Text == "tuanafk2006@gmail.com" && txtPassword.Text == "admin")
                || (txtUsername.Text == "phuclequang2006@gmail.com" && txtPassword.Text == "admin")
                || (txtUsername.Text == "admin@VACIRIS" && txtPassword.Text == "admin"))
                {
                    if (checkBoxRM.Checked == true)
                    {
                        Properties.Settings.Default.Username = txtUsername.Text;
                        Properties.Settings.Default.Password = txtPassword.Text;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default.Username = "";
                        Properties.Settings.Default.Password = "";
                        Properties.Settings.Default.Save();
                    }
                    username = txtUsername.Text;
                    MainMenu MM = new MainMenu(this);
                    MM.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("No internet connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Contact your administrator to reset your account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void loginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}