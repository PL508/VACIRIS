using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Media;

namespace VACIRIS
{
    public partial class dashboard : Form
    {
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

        private void updateData()
        {
            StreamReader r = new StreamReader("owid-covid-latest.json");

            var jsonData = JObject.Parse(r.ReadToEnd());

            var dictionaryTokens = jsonData.Children().Select(u => u as JProperty).Where(v => v.Value["location"].ToString().Contains("Vietnam")).ToDictionary(k => k.Name, v => v.Value);

            string total_cases = dictionaryTokens["VNM"]["total_cases"].ToString();
            string new_cases = dictionaryTokens["VNM"]["new_cases"].ToString();
            string new_deaths = dictionaryTokens["VNM"]["new_deaths"].ToString();
            string total_vaccinations = dictionaryTokens["VNM"]["total_vaccinations"].ToString();
            string people_vaccinated = dictionaryTokens["VNM"]["people_vaccinated"].ToString();
            string people_fully_vaccinated = dictionaryTokens["VNM"]["people_fully_vaccinated"].ToString();

            string totalcases = total_cases;
            string newcases = new_cases;
            string newdeaths = new_deaths;
            string vaccinations = total_vaccinations;
            string vaccinated = people_vaccinated;
            string fully_vaccinated = people_fully_vaccinated;

            if (CheckInternetConnection())
            {
                string[] new_date = {
                    DateTime.Now.ToString()
                };
                update_day.Text = "Newest update " + DateTime.Now.ToString();
                
                File.WriteAllLines("db_date.txt", new_date);
            }
            else
            {
                string date = File.ReadAllText("db_date.txt");
                update_day.Text = "Newest update " + date;
            }
            label_totalcases.Text = totalcases;
            label_newcases.Text = newcases;
            label_deaths.Text = newdeaths;
            label_totalvaccinations.Text = vaccinations;
            label_vaccinated.Text = vaccinated;
            label_fullyvaccinated.Text = fully_vaccinated;
            
            r.Close();
            return;
        }

        public dashboard()
        {
            InitializeComponent();
            updateData();
        }

        private void prevention_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.who.int/health-topics/coronavirus#tab=tab_2");
        }

        private void symptomps_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.who.int/health-topics/coronavirus#tab=tab_3");
        }

        private void case_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://news.google.com/covid19/map");
        }

        private void reload_button_Click(object sender, EventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                if (CheckInternetConnection())
                {
                    wc.DownloadFile("https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/latest/owid-covid-latest.json", "owid-covid-latest.json");
                    updateData();
                    SystemSounds.Exclamation.Play();
                }
                else
                {
                    MessageBox.Show("A file error has occurred.\nPlease check your network connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
