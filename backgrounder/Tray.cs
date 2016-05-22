using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

namespace backgrounder
{
    public partial class Tray : Form
    {
        AboutForm about;
        Timer replaceTimer;

        public Tray()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            nextBackground();
        }

        private void settingsFormClosed(object sender, FormClosedEventArgs e)
        {
            about = null;
        }

        private void openAboutForm()
        {
            if (about == null)
            {
                about = new AboutForm();
                about.Show();
                about.FormClosed += settingsFormClosed;
            }
            else
            {
                about.Focus();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openAboutForm();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            openAboutForm();
        }

        private void nextBackground()
        {
            string result = httpRequest("https://y01v4lkf97.execute-api.us-west-2.amazonaws.com/prod/bg");        

            if (result != null)
            {
                result = result.Replace("\"", ""); 
                Dictionary<string, string> dictionary = queryStringParse(result);

                string link = "";
                if (dictionary["link"] != null)
                {
                    link = dictionary["link"].ToString();
                }
                
                if(link != "")
                {
                    Uri backgroundURI = new Uri(link);

                    using (WebClient webClient = new WebClient())
                    {
                        string BackgroundID = dictionary["id"].ToString();
                        string ImagePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%TEMP%") + "\\" + BackgroundID);

                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => DownloadFileCallback(sender, e, ImagePath, BackgroundID));
                        webClient.DownloadFileAsync(backgroundURI, ImagePath);
                    }
                }
                else
                {
                    Debug.WriteLine("No bg returned");
                    nextBackground();
                }
            }
            else
            {
                resetReplaceTimer();
            }
        }

        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e, String DownloadPath, string BackgroundID)
        {
            Wallpaper.Set(new Uri(DownloadPath), Wallpaper.Style.Stretched);

            string OldPath = Properties.Settings.Default.CurrentBackground;

            GC.Collect();
            if (OldPath != null && File.Exists(OldPath))
            {
                Debug.WriteLine("Delete: " + OldPath);
                File.Delete(OldPath);
            }

            Debug.WriteLine("New Image: " + DownloadPath);
            Properties.Settings.Default.CurrentBackgroundID = BackgroundID;
            Properties.Settings.Default.CurrentBackground = DownloadPath;
            Properties.Settings.Default.Save();

            resetReplaceTimer();
        }

        private void resetReplaceTimer()
        {
            if (replaceTimer != null)
            {
                replaceTimer.Stop();
                replaceTimer = null;
            }

            replaceTimer = new Timer();
            replaceTimer.Tick += new EventHandler(OnTimedEvent);
            replaceTimer.Interval = (int)(Properties.Settings.Default.RotationTime * (60 * 1000));
            replaceTimer.Start();
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            replaceTimer.Stop();
            replaceTimer = null;
            nextBackground();
        }

        private void nextBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nextBackground();
        }

        private string httpRequest(string endpoint)
        {
            Debug.WriteLine("Http Call :: " + endpoint);
            try
            {
                WebRequest webRequest = WebRequest.Create(endpoint);
                using (WebResponse response = webRequest.GetResponse())
                using (Stream content = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(content))
                {
                    String strContent = reader.ReadToEnd();
                    Debug.WriteLine("Http Call response :: " + strContent);
                    return strContent;
                }
            }
            catch (WebException webExcp)
            {
                Debug.WriteLine("Http Call failed :: WebException");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Http Call failed :: Exception");
                return null;
            }
        }

        private Dictionary<string, string> queryStringParse(string str)
        {
            var dictionary = new Dictionary<string, string>();

            // Query String Breakdown
            foreach (string pair in str.Split('&'))
            {
                string[] n = pair.Split('=');
                dictionary.Add(n[0], n[1]);
            }

            return dictionary;
        }
    }
}
