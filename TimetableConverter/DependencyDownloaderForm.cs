using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimetableConverter
{
    public partial class frmDependencyDownloader : Form
    {
        // Declare debugging boolean
        bool debug = false;

        /// <summary>
        /// Form Initializer
        public frmDependencyDownloader()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Downloads Dependencies
        /// </summary>(
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownload_Click(object sender, EventArgs e)
        {
            // Check / Download dependencies as needed
            if (debug)
            {
                checkAndDownloadDependency("chromedriver.exe");
            }
            else
            {
                checkAndDownloadDependency("phantomjs.exe");
            }

            checkAndDownloadDependency("DDay.iCal.dll");
            checkAndDownloadDependency("WebDriver.dll");

            // Show Conversion Form
            Form frm = new frmTimetableConverter();
            frm.Show();

            this.Hide();
        }

        /// <summary>
        /// Checks if dependency is available, Downloads it if not
        /// </summary>
        /// <param name="dependency">Dependency to check for</param>
        private void checkAndDownloadDependency(string dependency)
        {
            if (!checkDependency(dependency))
            {
                new System.Net.WebClient().DownloadFile("http://vps.q0r.ca/tc/", dependency);
                tbxMain.AppendText("Dependency \"" + dependency + "\" downloaded successfully!");

                string[] splitName = dependency.Split('.');

                string extension = splitName[splitName.Length - 1];

                if (extension == "dll")
                {
                    Assembly assembly = Assembly.LoadFrom(dependency);

                    AppDomain.CurrentDomain.Load(assembly.GetName());
                }
            }
        }

        /// <summary>
        /// Checks if dependency is available
        /// </summary>
        /// <param name="dependency">Dependency to check for</param>
        /// <returns>Whether the dependency is found or not</returns>
        private bool checkDependency(string dependency)
        {
            return File.Exists(dependency);
        }

        /// <summary>
        /// Event fired when form has "Activated"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmDependencyDownloader_Activated(object sender, EventArgs e)
        {
            if ((debug ? checkDependency("chromedriver.exe") : checkDependency("phantomjs.exe"))
                && checkDependency("DDay.iCal.dll")
                && checkDependency("WebDriver.dll"))
            {
                Form frm = new frmTimetableConverter();
                frm.Show();

                this.Hide();
            }
        }
    }
}
