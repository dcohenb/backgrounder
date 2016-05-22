using System;
using System.Windows.Forms;

namespace backgrounder
{
    public partial class AboutForm : Form
    {

        public AboutForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load_1(object sender, EventArgs e)
        {
            rotationTime.Value = Properties.Settings.Default.RotationTime;
        }
        
        private void rotationTime_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RotationTime = int.Parse(rotationTime.Value.ToString());
            Properties.Settings.Default.Save();
        }
    }
}
