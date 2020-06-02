using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDVR
{
    public partial class IDVR : Form
    {
        public IDVR()
        {
            InitializeComponent();
        }

        private void IDVR_Load(object sender, EventArgs e)
        {
            try
            {
                Width = 0;
                Height = 0;
                Visible = false;
                timer1.Enabled = true;
                timer1.Interval = 100;
                timer1.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                timer1.Stop();
                //foreach (var process in Process.GetProcessesByName("IDV Document Reader"))
                foreach (var process in Process.GetProcesses())
                {
                    if (process.ProcessName == "IDV_Reader")
                    {
                        process.Kill();
                    }
                }
                Process PCS = new Process();
                PCS.StartInfo.FileName = Application.StartupPath + "\\IDVR\\" + "IDV_Reader.exe";
                PCS.Start();
            }
            catch (Exception) { }
            Close();
        }
    }
}
