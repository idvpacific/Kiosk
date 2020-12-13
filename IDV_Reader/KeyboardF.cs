using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDV_Reader
{
    public partial class KeyboardF : Form
    {
        public string MainText;
        public string Header;
        public Main MainFB;
        public KeyboardF()
        {
            InitializeComponent();
        }

        private void KeyboardF_Load(object sender, EventArgs e)
        {
            ApplyData();
        }

        public void ApplyData()
        {
            label1.Text = "";
            textBox1.Text = "";
            textBox1.Text = MainText.Trim();
            label1.Text = Header.Trim();
            try
            {
                textBox1.SelectionStart = textBox1.Text.Length;
            }
            catch (Exception)
            {
                textBox1.SelectionStart = 0;
            }
        }

        private void button44_Click(object sender, EventArgs e)
        {
            MainFB.LastResKeyB = MainText;
            MainFB.WaitForKB = false;
            Close();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            MainFB.LastResKeyB = textBox1.Text.Trim();
            MainFB.WaitForKB = false;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Focus();
                string SendKey = ((Button)sender).Text[0].ToString().ToUpper();
                SendKeys.SendWait(SendKey);
            }
            catch (Exception)
            { }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Focus();
                SendKeys.SendWait("{BACKSPACE}");
            }
            catch (Exception)
            { }
        }

        private void button45_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Focus();
                SendKeys.SendWait(" ");
            }
            catch (Exception)
            { }
        }
    }
}
