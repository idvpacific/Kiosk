using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using System.CodeDom;
using System.Security.Cryptography;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using Newtonsoft.Json;

namespace IDV_Reader
{
    public partial class Main : Form
    {
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Public Define :
        string FileLog = "";
        private Control THC;
        bool Cam_Init_1 = false;
        bool Cam_Init_2 = true;
        bool Cam_Init_3 = false;
        bool Init_Activity_Complete = false;
        bool FirstLoad = true;
        bool ExitLoop = false;
        string IPAddress = "";
        bool SystemPause = false;
        string IDVBS = "";
        bool Doc_In = false;
        bool Info_Confirm = false;
        bool Finish_Confirm = false;
        bool PageProcc_Show = false;
        bool PageDVLAWait = false;
        //--------------------------------------------------------
        bool ReConfig = true;
        int Reconfig_Click = 0;
        string ServerAddress = "";
        string CompanyID = "";
        string DealerID = "";
        string UserID = "";
        string Auth_Username = "";
        string Auth_Password = "";
        int DeviceID = 0; // 1 : Passport | 2 : Driving Lic
        int DVLA_En = 0;
        int VirtualKeyboad = 0;
        bool KeyboardActived = false;
        public string LastResKeyB = "";
        public bool WaitForKB = false;
        int SBtnSelect = 0;
        int WBSiteAdd = 0;
        bool UserCancel = false;
        bool DontShowKeyboard = false;
        //--------------------------------------------------------
        ListBox SelectedPluginList = new ListBox();
        ListBox AvailablePluginList = new ListBox();
        ListBox RFIDAvailDataItems = new ListBox();
        ListBox RFIDSelectDataItems = new ListBox();
        ListBox UHFAvailDataItems = new ListBox();
        ListBox UHFSelectDataItems = new ListBox();
        //--------------------------------------------------------
        int Activity_Now = 0;
        bool SystemRunNew = false;
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Main Form Events :
        public Main()
        {
            InitializeComponent();
            THC = new Control();
            THC.CreateControl();
        }

        [Obsolete]
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                this.Top = 0;
                this.Left = 0;
                this.WindowState = FormWindowState.Maximized;
                timer1.Enabled = false;
                timer1.Stop();
                timer2.Enabled = false;
                timer2.Stop();
                Lbl_ErrorPublic.Text = "";
                SystemRunNew = false;
                IDVBS = ConfigurationSettings.AppSettings["IDV_BaseAddress"].ToString().Trim();
                if (ConfigurationSettings.AppSettings["CNG_Logo_IDDRIVER"].ToString().Trim() == "1") { PanBObj1.Visible = true; } else { PanBObj1.Visible = false; }
                if (ConfigurationSettings.AppSettings["CNG_Logo_Kia"].ToString().Trim() == "1") { PIC_LG.Image = LG_KIA.Image; }
                if (ConfigurationSettings.AppSettings["CNG_Logo_Toyota"].ToString().Trim() == "1") { PIC_LG.Image = LG_TOYOTA.Image; }
                if (ConfigurationSettings.AppSettings["CNG_Logo_BENZ"].ToString().Trim() == "1") { PIC_LG.Image = LG_BENZ.Image; }
                if (ConfigurationSettings.AppSettings["CNG_DVLA"].ToString().Trim() == "1") { DVLA_En = 1; }
                if (ConfigurationSettings.AppSettings["CNG_Keyboard"].ToString().Trim() == "1") { VirtualKeyboad = 1; }
                if (System.IO.Directory.Exists(Application.StartupPath + @"\Logs") == false) { System.IO.Directory.CreateDirectory(Application.StartupPath + @"\Logs"); }
                FileLog = Application.StartupPath + @"\Logs\" + DateTime.Now.ToString("ddMMyyyy-HHmm") + ".txt";
                // Load Config File :
                ReConfig = true;
                try
                {
                    ServerAddress = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "SA", "").ToString().Trim();
                    CompanyID = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "CI", "").ToString().Trim();
                    DealerID = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "DI", "").ToString().Trim();
                    UserID = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "UI", "").ToString().Trim();
                    Auth_Username = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "AU", "").ToString().Trim();
                    Auth_Password = Interaction.GetSetting("IDV_Pacific", "Document_Reader", "AP", "").ToString().Trim();
                    DeviceID = int.Parse(Interaction.GetSetting("IDV_Pacific", "Document_Reader", "DVI", "0").ToString().Trim());
                    ServerAddress = ServerAddress.Trim();
                    CompanyID = CompanyID.Trim();
                    DealerID = DealerID.Trim();
                    UserID = UserID.Trim();
                    if ((DeviceID < 1) || (DeviceID > 10)) { DeviceID = 0; }
                    if ((ServerAddress != "") && (CompanyID != "") && (DealerID != "") && (UserID != "") && (Auth_Username != "") && (Auth_Password != "") && (DeviceID != 0))
                    {
                        ServerAddress = GetUnEncData(ServerAddress);
                        CompanyID = GetUnEncData(CompanyID);
                        DealerID = GetUnEncData(DealerID);
                        UserID = GetUnEncData(UserID);
                        Auth_Username = GetUnEncData(Auth_Username);
                        Auth_Password = GetUnEncData(Auth_Password);
                        if ((ServerAddress != "") && (CompanyID != "") && (DealerID != "") && (UserID != "") && (Auth_Username != "") && (Auth_Password != "") && (DeviceID != 0))
                        {
                            ReConfig = false;
                        }
                    }
                    else
                    {
                        ReConfig = true;
                    }
                }
                catch (Exception) { ReConfig = true; }
                // Test Config File :
                if (ReConfig == false)
                {
                    if (DeviceID == 1)
                    {
                        Ins_Pic.Image = C1_INS.Image;
                        Ins_Text.Text = "Please insert your passport in the correct way";
                        Proc_Pic.Image = C1_PRO.Image;
                        Proc_Text.Text = "Processing your passport information";
                    }
                    if (DeviceID == 2)
                    {
                        Ins_Pic.Image = C2_INS.Image;
                        Ins_Text.Text = "Please insert your driving licence in scanner";
                        Proc_Pic.Image = C2_PRO.Image;
                        Proc_Text.Text = "Processing your driving licence information";
                    }
                }
                try { IPAddress = GetUser_IP(); } catch (Exception) { }
                AddLog("-------------------------------------------------------------------------" + "\r\n");
                AddLog("- Run Software -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                AddLog("-------------------------------------------------------------------------" + "\r\n");
                Btn_BTD.Visible = false;
                Btn_CC.Visible = false;
                Btn_CF.Visible = false;
                Btn_BTD.Top = 13;
                Btn_CC.Top = 13;
                Btn_CF.Top = 13;
                int Btn_Count = 0;
                string BtnSC = ConfigurationSettings.AppSettings["SYS_Btn_Scanner"].ToString().Trim();
                string BtnCC = ConfigurationSettings.AppSettings["SYS_Btn_CarConfig"].ToString().Trim();
                string BtnCF = ConfigurationSettings.AppSettings["SYS_Btn_Finance"].ToString().Trim();
                if ((BtnSC != "0") && (BtnSC != "1")) { BtnSC = "0"; }
                if ((BtnCC != "0") && (BtnCC != "1")) { BtnCC = "0"; }
                if ((BtnCF != "0") && (BtnCF != "1")) { BtnCF = "0"; }
                if (BtnSC == "1") { Btn_BTD.Visible = true; Btn_Count++; }
                if (BtnCC == "1") { Btn_CC.Visible = true; Btn_Count++; }
                if (BtnCF == "1") { Btn_CF.Visible = true; Btn_Count++; }
                int BtnNL = 0;
                if (Btn_Count > 0)
                {
                    int WidthDiff = Btn_Count * 570;
                    int widthSpace = (int)((Pnl_Btn.Width - WidthDiff) / (Btn_Count + 1));
                    BtnNL = widthSpace;
                    if (BtnCF == "1")
                    {
                        Btn_CF.Left = BtnNL;
                        BtnNL = BtnNL + Btn_CF.Width + widthSpace;
                    }
                    if (BtnCC == "1")
                    {
                        Btn_CC.Left = BtnNL;
                        BtnNL = BtnNL + Btn_CC.Width + widthSpace;
                    }
                    if (BtnSC == "1")
                    {
                        Btn_BTD.Left = BtnNL;
                        BtnNL = BtnNL + Btn_BTD.Width + widthSpace;
                    }
                }
                button11.Visible = false;
                button12.Visible = false;
                button13.Visible = false;
                if (ConfigurationSettings.AppSettings["Btn_Cancel_WaitForDoc"].ToString().Trim() == "1") { button11.Visible = true; }
                if (ConfigurationSettings.AppSettings["Btn_Cancel_EmailPhone"].ToString().Trim() == "1") { button12.Visible = true; }
                if (ConfigurationSettings.AppSettings["Btn_Cancel_Confirm"].ToString().Trim() == "1") { button13.Visible = true; }
                timer1.Interval = 1000;
                timer1.Enabled = true;
                timer1.Start();
            }
            catch (Exception)
            {
                this.Close();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                timer1.Stop();
                timer1.Enabled = false;
                timer2.Stop();
                timer2.Enabled = false;
                MMM.Readers.FullPage.Reader.Shutdown();
                AddLog("-------------------------------------------------------------------------" + "\r\n");
                AddLog("- Close Software -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm" + "\r\n"));
                AddLog("-------------------------------------------------------------------------" + "\r\n");
            }
            catch (System.DllNotFoundException)
            { }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Object Event :
        private void Splash_Btn_Exit_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception)
            { }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception) { }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            try
            {
                Activity_Now = 1;
                SBtnSelect = 0;
            }
            catch (Exception) { }
        }

        private void Splash_Btn_Retry_Click(object sender, EventArgs e)
        {
            try
            {
                Lbl_ErrorPublic.Text = "";
                Splash_Btn_Exit.Visible = false;
                Splash_Btn_Retry.Visible = false;
                Splash_Lbl_1.Text = "Initial System Startup ...";
                Splash_Lbl_2.Text = "";
                Application.DoEvents();
                timer1.Enabled = true;
                timer1.Interval = 1000;
                timer1.Start();
            }
            catch (Exception) { }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Function :
        private void AddLog(string LogDesc)
        {
            try
            {
                if (LogDesc == null) { return; }
                File.AppendAllText(FileLog, LogDesc);
                if (IDVBS != "")
                {
                    string LD = "";
                    if (LogDesc.IndexOf("----------") > 0) { return; }
                    int FindFND = 0;
                    FindFND = LogDesc.IndexOf("->");
                    if (FindFND < 0) { FindFND = LogDesc.Length; }
                    try
                    {
                        LogDesc = LogDesc.Substring(0, FindFND);
                    }
                    catch (Exception)
                    {
                        LogDesc = LogDesc.Substring(0, FindFND - 1);
                    }
                    LD = LogDesc.Replace("\r\n", "");
                    LD = LogDesc.Replace("->", "");
                    LD = LD.Replace("-", "");
                    LD = LD.Replace("  ", " ");
                    LD = LD.Replace("  ", " ");
                    LD = LD.Trim();
                    Scanner_HttpPost_Log("AddLog", "C=" + CompanyID + "&D=" + DealerID + "&U=" + UserID + "&IP=" + IPAddress + "&I=" + DeviceID.ToString() + "&T=" + LD);
                }
            }
            catch (Exception)
            { }
        }

        private void Centerlizer()
        {
            try
            {
                foreach (var PNL in Controls.OfType<Panel>())
                {
                    if (PNL.Name.Substring(0, 3) == "PL_")
                    {
                        PNL.Left = (this.Width / 2) - (PNL.Width / 2);
                        PNL.Top = (this.Height / 2) - (PNL.Height / 2);
                    }
                }
            }
            catch (Exception)
            { }
        }

        private void ClearAll()
        {
            try
            {
                foreach (var PNL in Controls.OfType<Panel>())
                {
                    if (PNL.Name.Substring(0, 3) == "PL_")
                    {
                        PNL.Visible = false;
                        PNL.Enabled = false;
                    }
                }
                Lbl_ErrorPublic.Text = "";
                Splash_Lbl_1.Text = "";
                Splash_Lbl_2.Text = "";
            }
            catch (Exception)
            { }
        }

        private void ClearAll2()
        {
            try
            {
                foreach (var PNL in Controls.OfType<Panel>())
                {
                    if (PNL.Name.Substring(0, 3) == "PL_")
                    {
                        PNL.Visible = false;
                    }
                }
            }
            catch (Exception)
            { }
        }
        public string Get_Date()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                string Txt = DateTime.Now.ToString("dd/MM/yyyy");
                return Txt.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void Wait(int time)
        {
            Thread thread = new Thread(delegate ()
            {
                System.Threading.Thread.Sleep(time);
            });
            thread.Start();
            while (thread.IsAlive)
            {
                Application.DoEvents();
            }

        }

        public bool ExpiredDateCheck(string BaseDate, string MaxDate)
        {
            try
            {
                bool Res = false;
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                DateTime BDate = DateTime.Parse(BaseDate);
                DateTime MDate = DateTime.Parse(MaxDate);
                if (BDate.Date >= MDate.Date)
                {
                    Res = true;
                }
                return Res;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Encrypt(string PlainText, string SecurityKey)

        {
            try
            {
                byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);
                MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
                byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
                objMD5CryptoService.Clear();
                var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
                objTripleDESCryptoService.Key = securityKeyArray;
                objTripleDESCryptoService.Mode = CipherMode.ECB;
                objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
                var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
                byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
                objTripleDESCryptoService.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string Decrypt(string CipherText, string SecurityKey)
        {
            try
            {
                byte[] toEncryptArray = Convert.FromBase64String(CipherText);
                MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
                byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
                objMD5CryptoService.Clear();
                var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
                objTripleDESCryptoService.Key = securityKeyArray;
                objTripleDESCryptoService.Mode = CipherMode.ECB;
                objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
                var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
                byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                objTripleDESCryptoService.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string Make_Security_Code(int Count)
        {
            string FB = "";
            try
            {
                const string valid = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < Count--)
                { res.Append(valid[rnd.Next(valid.Length)]); }
                FB = res.ToString();
            }
            catch
            { FB = ""; }
            return FB;
        }

        public string GetUser_IP()
        {
            try
            {
                string InternetIP = "";
                InternetIP = new WebClient().DownloadString("http://icanhazip.com");
                return InternetIP;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetUnEncData(string InVal)
        {
            try
            {
                string LastRes = "";
                LastRes = Decrypt(InVal.Substring(0, InVal.Length - 10), InVal.Substring((InVal.Length - 10), 10));
                LastRes = LastRes.Trim();
                return LastRes;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Boolean InternetAvailable()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (client.OpenRead("http://www.google.com/"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ApplicationLic()
        {
            bool Res = false;
            try
            {
                string ResV = "";
                try
                {
                    ResV = Scanner_HttpPost("WA_02_AppLic", "CID=" + CompanyID + "&ATU=" + Auth_Username + "&ATP=" + Auth_Password);
                }
                catch (Exception) { }
                if (ResV.Trim().ToUpper() != "ERR")
                {
                    ResV = ResV.Replace("\"", "").Trim();
                    string RSV = ResV.Substring(0, ResV.Length - 10);
                    string ENC = ResV.Substring(ResV.Length - 10, 10);
                    string PSC = Decrypt(RSV, ENC).Trim().ToUpper();
                    if (PSC == "EMASOK")
                    {
                        Res = true;
                    }
                    else
                    {
                        Res = false;
                    }
                }
                else
                {
                    Res = false;
                }
            }
            catch (Exception)
            {
                Res = false;
            }
            return Res;
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Timer Tick :
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                bool LicActive = false;
                bool InternetActive = false;
                timer1.Enabled = false;
                timer1.Stop();
                Centerlizer();
                ClearAll();
                PL_01_Splash.Enabled = true;
                PL_01_Splash.Visible = true;
                Splash_Lbl_1.Text = "Initial System Startup ...";
                Splash_Lbl_2.Text = "";
                Splash_Btn_Exit.Visible = false;
                Splash_Btn_Retry.Visible = false;
                PL_01_Splash.Visible = true;
                Application.DoEvents();
                if (ReConfig == false)
                {
                    InternetActive = false;
                    InternetActive = InternetAvailable();
                    if (InternetActive == true)
                    {
                        IPAddress = "";
                        IPAddress = GetUser_IP();
                        if (IPAddress != "")
                        {
                            LicActive = false;
                            LicActive = ApplicationLic();
                            if (LicActive == true)
                            {
                                timer2.Interval = 1000;
                                timer2.Start();
                            }
                            else
                            {
                                Splash_Lbl_1.Text = "Your application has the right to suspend";
                                Splash_Lbl_2.Text = "Please contact your administrator";
                                Splash_Btn_Exit.Visible = true;
                                Splash_Btn_Retry.Visible = true;
                            }
                        }
                        else
                        {
                            Splash_Lbl_1.Text = "It is not possible to identify device IP address";
                            Splash_Lbl_2.Text = "Please check yout internet connection";
                            Splash_Btn_Exit.Visible = true;
                            Splash_Btn_Retry.Visible = true;
                        }
                    }
                    else
                    {
                        Splash_Lbl_1.Text = "It is not possible to connect to the central server";
                        Splash_Lbl_2.Text = "Please check yout internet connection";
                        Splash_Btn_Exit.Visible = true;
                        Splash_Btn_Retry.Visible = true;
                    }
                }
                else
                {
                    timer2.Interval = 1000;
                    timer2.Start();
                }
            }
            catch (Exception) { }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            Lbl_ErrorPublic.Text = "";
            Splash_Lbl_1.Text = "Initial System Startup ...";
            Splash_Lbl_2.Text = "";
            Splash_Btn_Retry.Visible = false;
            Splash_Btn_Exit.Visible = false;
            Cam_Init_1 = false;
            Cam_Init_2 = true;
            Cam_Init_3 = false;
            Init_Activity_Complete = false;
            FirstLoad = true;
            int ReloadInit = 0;
            Application.DoEvents();
            try
            {
                AddLog("- System Log : " + "Connect To Camera Driver" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                MMM.Readers.FullPage.Reader.EnableLogging(true, 1, -1, "IDV_Reader.log");
                MMM.Readers.ErrorCode lResult = MMM.Readers.ErrorCode.NO_ERROR_OCCURRED;
                Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(OnPowerModeChanged);
                lResult = MMM.Readers.FullPage.Reader.Initialise(new MMM.Readers.FullPage.DataDelegate(DataCallbackThreadHelper), new MMM.Readers.FullPage.EventDelegate(EventCallbackThreadHelper), new MMM.Readers.ErrorDelegate(ErrorCallbackThreadHelper), new MMM.Readers.FullPage.CertificateDelegate(CertificateCallbackThreadHelper), true, false);
                if (lResult != MMM.Readers.ErrorCode.NO_ERROR_OCCURRED)
                {
                    Splash_Lbl_1.Text = "Error! Initialisation Failed";
                    Splash_Lbl_2.Text = lResult.ToString();
                    ReloadInit = 1;
                    AddLog("- System Log : " + "Initialise Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                }
                MMM.Readers.FullPage.Reader.SetWarningCallback(new MMM.Readers.WarningDelegate(WarningCallbackThreadHelper));
            }
            catch (System.DllNotFoundException except)
            {
                Splash_Lbl_1.Text = "Error! Initialisation Failed";
                AddLog("- System Log : " + "Initialise Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                Splash_Lbl_2.Text = except.Message;
                ReloadInit = 1;
            }
            if (ReloadInit == 0)
            {
                while (Init_Activity_Complete == false)
                {
                    if ((Cam_Init_1 == true) && (Cam_Init_2 == true) && (Cam_Init_3 == true)) { Init_Activity_Complete = true; }
                    Application.DoEvents();
                }
                if ((Cam_Init_1 == true) && (Cam_Init_2 == true) && (Cam_Init_3 == true))
                {
                    MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                    Splash_Lbl_1.Text = "Congratulations! Initialised Successfully";
                    AddLog("- System Log : " + "Congratulations! Initialise Successfully" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    if (DeviceID == 1)
                    {
                        AddLog("- System Log : " + "Passport Config Reload" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        Splash_Lbl_2.Text = "Applying RFID Chip Setting ...";
                        AddLog("- System Log : " + "Applying RFID Chip Setting" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    }
                    else
                    {
                        AddLog("- System Log : " + "Driving Lic Config Reload" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        Splash_Lbl_2.Text = "Applying Scanner Setting ...";
                        AddLog("- System Log : " + "Applying Scanner Setting" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    }
                    Application.DoEvents();
                    MMM.Readers.FullPage.ReaderState state = MMM.Readers.FullPage.Reader.GetState();
                    if (state == MMM.Readers.FullPage.ReaderState.READER_NOT_INITIALISED || state == MMM.Readers.FullPage.ReaderState.READER_ERRORED || state == MMM.Readers.FullPage.ReaderState.READER_FATAL_ERRORED || state == MMM.Readers.FullPage.ReaderState.READER_TERMINATED || state == MMM.Readers.FullPage.ReaderState.READER_READING)
                    {
                        if (DeviceID == 1)
                        {
                            Splash_Lbl_1.Text = "Error! RFID Chip Setting Failed";
                        }
                        else
                        {
                            Splash_Lbl_1.Text = "Error! Scanner Setting Failed";
                        }
                        Splash_Lbl_2.Text = "Scanner not respone to system request. Please restart software [ Code:101 ]";
                        Splash_Btn_Exit.Visible = true;
                        Splash_Btn_Retry.Visible = false;
                        AddLog("- System Log : " + "Error! RFID Chip Setting Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        AddLog("- System Log : " + "Scanner not respone to system request. Please restart software [ Code:101 ]" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    }
                    else
                    {
                        DataToSend_Reload();
                        if (DeviceID == 1)
                        {
                            RFIDSelectAll();
                        }
                        else
                        {
                            RFIDDeSelectAll();
                        }
                        Application.DoEvents();
                        MMM.Readers.FullPage.ReaderSettings lSettings;
                        if (MMM.Readers.FullPage.Reader.GetSettings(out lSettings) == MMM.Readers.ErrorCode.NO_ERROR_OCCURRED)
                        {
                            if (DeviceID == 1)
                            {
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.DOCMARKERS;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.CODELINE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.CHECKSUM;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.IRIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.VISIBLEIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.UVIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.PHOTOIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.SMARTCARD;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.SECURITYCHECK;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.SWIPE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.AAMVA;
                                lSettings.puDataToSend.special &= ~MMM.Readers.FullPage.DataSendSet.Flags.SECURITYCHECK;
                                lSettings.puDataToSend.special &= ~MMM.Readers.FullPage.DataSendSet.Flags.IRIMAGE;
                                lSettings.puDataToSend.special |= MMM.Readers.FullPage.DataSendSet.Flags.VISIBLEIMAGE;
                                lSettings.puCameraSettings.ir.puUseAntiGlare = true;
                                lSettings.puCameraSettings.vis.puUseAntiGlare = true;
                                lSettings.puCameraSettings.ir.puUseSequentialImaging = false;
                                lSettings.puCameraSettings.vis.puUseSequentialImaging = false;
                                lSettings.puCameraSettings.flap = 0;
                                lSettings.puCameraSettings.flap |= 0x08;
                                lSettings.puCameraSettings.ir.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puCameraSettings.vis.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puCameraSettings.uv.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puImageSettings.useVisibleForBarcode = 0;
                                lSettings.puImageSettings.useVisibleForBarcode = 1;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.BARCODEIMAGE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.UHF;
                                lSettings.puDataToSend.progress = 0;
                                lSettings.puDataToSend.imageFormat = MMM.Readers.FullPage.ImageFormats.RTE_JPEG;
                                lSettings.puCameraSettings.scaleFactor = 100;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode |= (int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EPASSPORT;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EID;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EDL;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.IDL;
                                StoreUHFOrder(ref lSettings.puDataToSend.uhf);
                                StoreRFIDOrder(ref lSettings.puDataToSend.rfid);
                                StorePlugins();
                            }
                            else
                            {
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.DOCMARKERS;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.CODELINE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.CHECKSUM;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.IRIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.VISIBLEIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.UVIMAGE;
                                lSettings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.PHOTOIMAGE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.SMARTCARD;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.SECURITYCHECK;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.SWIPE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.AAMVA;
                                lSettings.puDataToSend.special &= ~MMM.Readers.FullPage.DataSendSet.Flags.SECURITYCHECK;
                                lSettings.puDataToSend.special &= ~MMM.Readers.FullPage.DataSendSet.Flags.IRIMAGE;
                                lSettings.puDataToSend.special |= MMM.Readers.FullPage.DataSendSet.Flags.VISIBLEIMAGE;
                                lSettings.puCameraSettings.ir.puUseAntiGlare = true;
                                lSettings.puCameraSettings.vis.puUseAntiGlare = true;
                                lSettings.puCameraSettings.ir.puUseSequentialImaging = false;
                                lSettings.puCameraSettings.vis.puUseSequentialImaging = false;
                                lSettings.puCameraSettings.flap = 0;
                                lSettings.puCameraSettings.flap |= 0x20;
                                lSettings.puCameraSettings.ir.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puCameraSettings.vis.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puCameraSettings.uv.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_HARDWARE_AMBIENT_REMOVAL;
                                lSettings.puDataToSend.compression = 90;
                                lSettings.puImageSettings.useVisibleForBarcode = 0;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.BARCODEIMAGE;
                                lSettings.puDataToSend.send &= ~MMM.Readers.FullPage.DataSendSet.Flags.UHF;
                                lSettings.puDataToSend.progress = 0;
                                lSettings.puDataToSend.imageFormat = MMM.Readers.FullPage.ImageFormats.RTE_JPEG;
                                lSettings.puCameraSettings.scaleFactor = 75;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EPASSPORT;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EID;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.EDL;
                                lSettings.puRFIDSettings.puRFProcessSettings.puRFApplicationMode &= ~(int)MMM.Readers.Modules.RF.RF_APPLICATION_MODE.IDL;
                                StoreUHFOrder(ref lSettings.puDataToSend.uhf);
                                StoreRFIDOrder(ref lSettings.puDataToSend.rfid);
                                StorePlugins();
                            }
                            FirstLoad = false;
                            Wait(2000);
                            if (DeviceID == 1)
                            {
                                if (textBox1.Text.IndexOf("DOC_ON_WINDOW") > 0) { Wait(4000); }
                            }
                            else
                            {
                                if (textBox1.Text.IndexOf("DOC_ON_WINDOW") > 0) { Wait(10000); }
                            }
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_ASLEEP, true);
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                            MMM.Readers.ErrorCode EE = MMM.Readers.FullPage.Reader.UpdateSettings(lSettings);
                            if (DeviceID == 1)
                            {
                                Splash_Lbl_2.Text = "RFID Chip Setting Ready to use";
                                AddLog("- System Log : " + "RFID Chip Setting Ready to use" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                            }
                            else
                            {
                                Splash_Lbl_2.Text = "Scanner Setting Ready to use";
                                AddLog("- System Log : " + "Scanner Setting Ready to use" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                            }
                            Application.DoEvents();
                            Wait(1000);
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_ASLEEP, true);
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                            if (ReConfig == false)
                            {
                                SystemPause = false;
                                timer3.Enabled = true;
                                timer3.Interval = 3000;
                                timer3.Start();
                                timer3_Tick(null, null);
                                DoScan();
                            }
                            else
                            {
                                DoConfig();
                            }
                        }
                        else
                        {
                            if (DeviceID == 1)
                            {
                                Splash_Lbl_1.Text = "Error! RFID Chip Setting Failed";
                                Splash_Lbl_2.Text = "Scanner not respone to system request. Please restart software [ Code:102 ]";
                                AddLog("- System Log : " + "Error! RFID Chip Setting Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                            }
                            else
                            {
                                Splash_Lbl_1.Text = "Error! Scanner Setting Failed";
                                Splash_Lbl_2.Text = "Scanner not respone to system request. Please restart software [ Code:102 ]";
                                AddLog("- System Log : " + "Error! Scanner Setting Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                            }
                            Splash_Btn_Exit.Visible = true;
                            Splash_Btn_Retry.Visible = false;
                            AddLog("- System Log : " + "Scanner not respone to system request. Please restart software [ Code:102 ]" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        }
                    }
                }
                else
                {
                    Splash_Lbl_1.Text = "Error! Initialise Failed [Err3]";
                    Splash_Lbl_2.Text = "Please Retry Initialise System";
                    AddLog("- System Log : " + "Initialise Failed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    AddLog("- System Log : " + "Please Retry Initialise System" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    Splash_Btn_Exit.Visible = true;
                    Splash_Btn_Retry.Visible = false;
                }
            }
            else
            {
                Splash_Btn_Exit.Visible = true;
                Splash_Btn_Retry.Visible = false;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            bool EVOK = false;
            try
            {
                EVOK = InternetAvailable();
            }
            catch (Exception)
            {
                EVOK = false;
            }
            if (EVOK == false)
            {
                if (PL_NOIN.Visible == false)
                {
                    Activity_Now = 1;
                    SBtnSelect = 0;
                    SystemPause = true;
                    PL_NOIN.Left = (this.Width / 2) - (PL_NOIN.Width / 2);
                    PL_NOIN.Top = (this.Height / 2) - (PL_NOIN.Height / 2);
                    PL_NOIN.BringToFront();
                    PL_NOIN.Visible = true;
                }
            }
            else
            {
                if (PL_NOIN.Visible == true)
                {
                    SystemPause = false;
                    PL_NOIN.Visible = false;
                    Activity_Now = 1;
                    SBtnSelect = 0;
                }
            }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Send To Server Function :
        private void SendToServer(int StatusCode)
        {
            try
            {
                string BaseID = "0";
                string Document_ID = "1";
                if (lblDocumentType.Text.Trim().ToLower() != "passport") { Document_ID = "2"; } else { Document_ID = "1"; }
                AddLog("- Scan Log : " + "Send To Server - Start" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                if (LBLD_1.Text.Trim() == "") { StatusCode = 2; }
                if (LBLD_2.Text.Trim() == "") { StatusCode = 2; }
                BaseID = Scanner_HttpPost("BaseInfo", "UserCode=" + UserID + "&SDK_Code=1&Document_ID=" + Document_ID + "&FirstName=" + LBLD_1.Text.Trim() + "&LastName=" + LBLD_2.Text.Trim() + "&StatusCode=" + StatusCode.ToString() + "&DocNo=" + LBLD_5.Text.Trim());
                BaseID = BaseID.Replace("\"", "").Trim();
                if (BaseID != "ERR")
                {
                    AddLog("- Scan Log : " + "Base ID = " + BaseID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    string R1 = Scanner_HttpPost("GA_01_MRZ_Data", "BaseID=" + BaseID + "&Document_Type=" + lblDocumentType.Text.Trim() + "&Forenames=" + lblForenames.Text.Trim() + "&Surname=" + lblSurname.Text.Trim() + "&Nationality=" + lblNationality.Text.Trim() + "&Sex=" + lblSex.Text.Trim() + "&Date_of_Birth=" + lblDateOfBirth.Text.Trim() + "&Document_Number=" + lblDocumentNumber.Text.Trim() + "&MRZ=" + richTextBoxCodeline.Text.Trim() + "&ExpireDate=" + lblMRZ_ExpireDate.Text.Trim());
                    AddLog("- Scan Log : " + "G1 = " + R1 + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    string R2 = Scanner_HttpPost("GA_02_ChipData", "BaseID=" + BaseID + "&Forenames=" + lblRFForenames.Text.Trim() + "&Surname=" + lblRFSurname.Text.Trim() + "&Nationality=" + lblRFNationality.Text.Trim() + "&Sex=" + lblRFSex.Text.Trim() + "&Date_of_Birth=" + lblRFDateOfBirth.Text.Trim() + "&Document_Number=" + lblRFDocNumber.Text.Trim() + "&MRZ=" + lblRFCodeline.Text.Trim() + "&Air_Baud_Rate=" + lblAirBaudRate.Text.Trim() + "&Chip_ID=" + lblChipId.Text.Trim() + "&BAC=" + lblBACStatus.Text.Trim() + "&SAC=" + lblSACStatus.Text.Trim() + "&ExpireDate=" + lblRF_ExpireDate.Text.Trim());
                    AddLog("- Scan Log : " + "G2 = " + R2 + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    string R3 = "";
                    foreach (ListViewItem anItem in validatedList.Items)
                    {
                        R3 = "";
                        R3 = Scanner_HttpPost("GA_03_Validate", "BaseID=" + BaseID + "&Item_Code=" + anItem.SubItems[0].Text.Trim() + "&Validate=" + anItem.SubItems[1].Text.Trim());
                        AddLog("- Scan Log : " + "G3 = " + anItem.SubItems[0].Text.Trim() + " -< " + R3 + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    }
                    AddLog("- Scan Log : " + "G3 = " + R3 + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                    for (int i = 1; i < 6; i++)
                    {
                        switch (i)
                        {
                            case 1:
                                {
                                    if (photoImage.Image != null)
                                    {
                                        if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                        photoImage.Image.Save(Application.StartupPath + @"\Photos\FC.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                        var client = new RestClient(ServerAddress + "/api/GA_04_PhotoUploader?BaseID=" + BaseID);
                                        client.Timeout = -1;
                                        var request = new RestRequest(Method.POST);
                                        request.AddHeader("Content-Type", "multipart/form-data");
                                        request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\FC.jpeg");
                                        IRestResponse response = client.Execute(request);
                                        if (File.Exists(Application.StartupPath + @"\Photos\FC.jpeg") == true) { File.Delete(Application.StartupPath + @"\Photos\FC.jpeg"); }
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (irImage.Image != null)
                                    {
                                        if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                        irImage.Image.Save(Application.StartupPath + @"\Photos\IR.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                        var client = new RestClient(ServerAddress + "/api/GA_04_PhotoUploader?BaseID=" + BaseID);
                                        client.Timeout = -1;
                                        var request = new RestRequest(Method.POST);
                                        request.AddHeader("Content-Type", "multipart/form-data");
                                        request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\IR.jpeg");
                                        IRestResponse response = client.Execute(request);
                                        if (File.Exists(Application.StartupPath + @"\Photos\IR.jpeg") == true) { File.Delete(Application.StartupPath + @"\Photos\IR.jpeg"); }
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    if (uvImage.Image != null)
                                    {
                                        if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                        uvImage.Image.Save(Application.StartupPath + @"\Photos\UV.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                        var client = new RestClient(ServerAddress + "/api/GA_04_PhotoUploader?BaseID=" + BaseID);
                                        client.Timeout = -1;
                                        var request = new RestRequest(Method.POST);
                                        request.AddHeader("Content-Type", "multipart/form-data");
                                        request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\UV.jpeg");
                                        IRestResponse response = client.Execute(request);
                                        if (File.Exists(Application.StartupPath + @"\Photos\UV.jpeg") == true) { File.Delete(Application.StartupPath + @"\Photos\UV.jpeg"); }
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    if (rfImage.Image != null)
                                    {
                                        if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                        rfImage.Image.Save(Application.StartupPath + @"\Photos\RF.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                        var client = new RestClient(ServerAddress + "/api/GA_04_PhotoUploader?BaseID=" + BaseID);
                                        client.Timeout = -1;
                                        var request = new RestRequest(Method.POST);
                                        request.AddHeader("Content-Type", "multipart/form-data");
                                        request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\RF.jpeg");
                                        IRestResponse response = client.Execute(request);
                                        if (File.Exists(Application.StartupPath + @"\Photos\RF.jpeg") == true) { File.Delete(Application.StartupPath + @"\Photos\RF.jpeg"); }
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    if (visibleImage.Image != null)
                                    {
                                        if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                        visibleImage.Image.Save(Application.StartupPath + @"\Photos\NM.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                        var client = new RestClient(ServerAddress + "/api/GA_04_PhotoUploader?BaseID=" + BaseID);
                                        client.Timeout = -1;
                                        var request = new RestRequest(Method.POST);
                                        request.AddHeader("Content-Type", "multipart/form-data");
                                        request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\NM.jpeg");
                                        IRestResponse response = client.Execute(request);
                                        if (File.Exists(Application.StartupPath + @"\Photos\NM.jpeg") == true) { File.Delete(Application.StartupPath + @"\Photos\NM.jpeg"); }
                                    }
                                    break;
                                }
                        }
                    }
                    if (Directory.Exists(Application.StartupPath + @"\Photos") == true) { Directory.Delete(Application.StartupPath + @"\Photos", true); }
                }
                else { AddLog("- Scan Log : " + "Send To Server - Error - BaseID" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n"); }
            }
            catch (Exception) { AddLog("- Scan Log : " + "Send To Server - Error" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n"); }
            AddLog("- Scan Log : " + "Send To Server - End" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
        }

        public string Scanner_HttpPost(string API, string DValue)
        {
            string Res = "ERR";
            try
            {
                string SRVR = ServerAddress + "/";
                var client = new RestClient(SRVR + "api/" + API + "?" + DValue);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                IRestResponse response = client.Execute(request);
                Res = response.Content.ToString().Trim();
            }
            catch (Exception) { Res = "ERR"; }
            return Res;
        }

        public void Scanner_HttpPost_Log(string API, string DValue)
        {
            if (IDVBS == "") { return; }
            try
            {
                string SRVR = IDVBS + "/";
                var client = new RestClient(SRVR + "api/" + API + "?" + DValue);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                client.Execute(request);
            }
            catch (Exception) { }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Base Win App Function :
        private void DoScan()
        {
            try
            {
                ExitLoop = false;
                SBtnSelect = 0;
                Centerlizer();
                if (DeviceID == 1)
                {
                    pictureBox4.Visible = true;
                    pictureBox4.Enabled = true;
                    pictureBox5.Visible = true;
                    pictureBox5.Enabled = true;
                }
                else
                {
                    pictureBox4.Visible = false;
                    pictureBox4.Enabled = false;
                    pictureBox5.Visible = false;
                    pictureBox5.Enabled = false;
                }
                //----------------------------------------------------------------
                Activity_Now = 1;
                SBtnSelect = 0;
                WBSiteAdd = 0;
                UserCancel = false;
                // 1 : Show First Page And Wait For Insert Document .
                // 2 : Readding Data And Send to Webservice .
                // 3 : Show Result Tick .
                // 4 : Wait For Remove Document From Scanner .
                // 5 : Wait For Response From Scanner .
                //----------------------------------------------------------------
                SystemRunNew = true;
                while (ExitLoop == false)
                {
                    if (SBtnSelect == 0)
                    {
                        if (Pnl_Btn.Visible == false)
                        {
                            Activity_Now = 1;
                            SBtnSelect = 0;
                            UserCancel = false;
                            SystemPause = false;
                            Finish_Confirm = false;
                            KeyboardActived = false;
                            label28.Visible = false;
                            label30.Visible = false;
                            PL_01_Splash.Visible = false;
                            PL_02_Splash.Visible = false;
                            PL_03_Splash.Visible = false;
                            PL_04_Splash.Visible = false;
                            PL_Error.Visible = false;
                            PL_Show_Info.Visible = false;
                            PL_Succ.Visible = false;
                            PL_Submit.Visible = false;
                            PL_DVLA.Visible = false;
                            PL_DVLA_WT.Visible = false;
                            PL_DVLA_OK.Visible = false;
                            PL_02_Splash.Visible = false;
                            PnlWeb.Visible = false;
                            Pnl_Btn.Left = (this.Width / 2) - (Pnl_Btn.Width / 2);
                            Pnl_Btn.Top = (this.Height / 2) - (Pnl_Btn.Height / 2) + 37;
                            Pnl_Btn.Enabled = true;
                            Pnl_Btn.Visible = true;
                        }
                    }
                    else
                    {
                        if (SBtnSelect == 1)
                        {
                            if (PnlWeb.Visible == false)
                            {
                                label28.Visible = false;
                                label30.Visible = false;
                                PL_01_Splash.Visible = false;
                                PL_02_Splash.Visible = false;
                                PL_03_Splash.Visible = false;
                                PL_04_Splash.Visible = false;
                                PL_Error.Visible = false;
                                PL_Show_Info.Visible = false;
                                PL_Succ.Visible = false;
                                PL_Submit.Visible = false;
                                PL_DVLA.Visible = false;
                                PL_DVLA_WT.Visible = false;
                                PL_DVLA_OK.Visible = false;
                                PL_02_Splash.Visible = false;
                                Pnl_Btn.Visible = false;
                                WB.Top = 10;
                                WB.Left = 10;
                                WB.Width = 1838;
                                WB.Height = 852;
                                WBL.Top = (PnlWeb.Height / 2) - (WBL.Height / 2);
                                WBL.Left = (PnlWeb.Width / 2) - (WBL.Width / 2);
                                WB.Visible = false;
                                WBL.Visible = false;
                                LoadWebPage(WBSiteAdd);
                                PnlWeb.Width = 1858;
                                PnlWeb.Height = 872;
                                PnlWeb.Left = (this.Width / 2) - (PnlWeb.Width / 2);
                                PnlWeb.Top = (this.Height / 2) - (PnlWeb.Height / 2) + 37;
                                WBL.Top = (PnlWeb.Height / 2) - (WBL.Height / 2);
                                WBL.Left = (PnlWeb.Width / 2) - (WBL.Width / 2);
                                PnlWeb.Enabled = true;
                                PnlWeb.Visible = true;
                            }
                        }
                        else
                        {
                            if (SystemPause == false)
                            {
                                switch (Activity_Now)
                                {
                                    case 1:
                                        {
                                            ClearAll2();
                                            DontShowKeyboard = false;
                                            UserCancel = false;
                                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                                            label28.Visible = false;
                                            label30.Visible = false;
                                            PL_01_Splash.Visible = false;
                                            PL_02_Splash.Visible = false;
                                            PL_03_Splash.Visible = false;
                                            PL_04_Splash.Visible = false;
                                            PL_Error.Visible = false;
                                            PL_Show_Info.Visible = false;
                                            PL_Succ.Visible = false;
                                            PL_Submit.Visible = false;
                                            PL_DVLA.Visible = false;
                                            PL_DVLA_WT.Visible = false;
                                            PL_DVLA_OK.Visible = false;
                                            PL_02_Splash.Enabled = true;
                                            PL_02_Splash.Visible = true;
                                            Application.DoEvents();
                                            AddLog("- Scan Log : " + "Wait For Document" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                            Wait(1000);
                                            Activity_Now = 5;
                                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_ENABLED, true);
                                            break;
                                        }
                                    //-------------------------------------------------------------------------------------------------------
                                    case 2:
                                        {
                                            if (DeviceID == 1)
                                            {
                                                label28.Visible = false;
                                                label30.Visible = false;
                                                PL_01_Splash.Visible = false;
                                                PL_02_Splash.Visible = false;
                                                PL_03_Splash.Visible = false;
                                                PL_04_Splash.Visible = false;
                                                PL_Error.Visible = false;
                                                PL_Show_Info.Visible = false;
                                                PL_Succ.Visible = false;
                                                PL_Submit.Visible = false;
                                                PL_DVLA.Visible = false;
                                                PL_DVLA_WT.Visible = false;
                                                PL_DVLA_OK.Visible = false;
                                                PL_03_Splash.Enabled = true;
                                                PL_03_Splash.Visible = true;
                                            }
                                            else
                                            {
                                                Finish_Confirm = false;
                                                PageProcc_Show = true;
                                                TXT_Email.Text = "";
                                                TXT_Phone.Text = "";
                                                Btn_Finished.Enabled = true;
                                                label28.Visible = false;
                                                label30.Visible = false;
                                                PL_01_Splash.Visible = false;
                                                PL_02_Splash.Visible = false;
                                                PL_03_Splash.Visible = false;
                                                PL_04_Splash.Visible = false;
                                                PL_Error.Visible = false;
                                                PL_Show_Info.Visible = false;
                                                PL_Succ.Visible = false;
                                                PL_Submit.Visible = false;
                                                PL_DVLA.Visible = false;
                                                PL_DVLA_WT.Visible = false;
                                                PL_DVLA_OK.Visible = false;
                                                PL_Submit.Enabled = true;
                                                label37.ForeColor = Color.Black;
                                                label38.ForeColor = Color.Black;
                                                PL_Submit.Visible = true;
                                            }
                                            AddLog("- Scan Log : " + "Proccess Document" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                            Activity_Now = 5;
                                            break;
                                        }
                                    //-------------------------------------------------------------------------------------------------------
                                    case 3:
                                        {
                                            int StatusCode = 0;
                                            AddLog("- Scan Log : " + "Proccess End" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                            if (DeviceID == 1)
                                            {
                                                pictureBox3.Image = null;
                                                LBLD_1.Text = "";
                                                LBLD_2.Text = "";
                                                LBLD_3.Text = "";
                                                LBLD_4.Text = "";
                                                LBLD_5.Text = "";
                                                T_OK.Visible = false;
                                                T_Error.Visible = false;
                                                try
                                                { if (rfImage.Image != null) { pictureBox3.Image = rfImage.Image; } else { if (photoImage.Image != null) { pictureBox3.Image = photoImage.Image; } } }
                                                catch (Exception) { }
                                                try
                                                {
                                                    if ((lblRFForenames.Text != null) && (lblRFForenames.Text != "")) { LBLD_1.Text = lblRFForenames.Text.Trim().ToUpper(); }
                                                    if (LBLD_1.Text.Trim() == "") { LBLD_1.Text = lblForenames.Text.Trim().ToUpper(); }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    if ((lblRFSurname.Text != null) && (lblRFSurname.Text != "")) { LBLD_2.Text = lblRFSurname.Text.Trim().ToUpper(); }
                                                    if (LBLD_2.Text.Trim() == "") { LBLD_2.Text = lblSurname.Text.Trim().ToUpper(); }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    if ((lblRFNationality.Text != null) && (lblRFNationality.Text != "")) { LBLD_3.Text = lblRFNationality.Text.Trim().ToUpper(); }
                                                    if (LBLD_3.Text.Trim() == "") { LBLD_3.Text = lblNationality.Text.Trim().ToUpper(); }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    if ((lblRFSex.Text != null) && (lblRFSex.Text != "")) { LBLD_4.Text = lblRFSex.Text.Trim().ToUpper(); }
                                                    if (LBLD_4.Text.Trim() == "") { LBLD_4.Text = lblSex.Text.Trim().ToUpper(); }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    if ((lblRFDocNumber.Text != null) && (lblRFDocNumber.Text != "")) { LBLD_5.Text = lblRFDocNumber.Text.Trim().ToUpper(); }
                                                    if (LBLD_5.Text.Trim() == "") { LBLD_5.Text = lblDocumentNumber.Text.Trim().ToUpper(); }
                                                }
                                                catch (Exception) { }
                                                int WIDEROK = 0;
                                                try
                                                {
                                                    if (lblRFSurname.Text.Trim().ToUpper() != lblSurname.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblRFForenames.Text.Trim().ToUpper() != lblForenames.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblRFNationality.Text.Trim().ToUpper() != lblNationality.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblRFSex.Text.Trim().ToUpper() != lblSex.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblRFDateOfBirth.Text.Trim().ToUpper() != lblDateOfBirth.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblRFDocNumber.Text.Trim().ToUpper() != lblDocumentNumber.Text.Trim().ToUpper()) { WIDEROK = 1; }
                                                    if (lblBACStatus.Text.Trim().ToUpper() != "TS_SUCCESS") { WIDEROK = 1; }
                                                }
                                                catch (Exception)
                                                {
                                                    WIDEROK = 1;
                                                }
                                                if (WIDEROK == 1) { T_Error.Visible = true; StatusCode = 2; } else { T_OK.Visible = true; StatusCode = 1; }
                                                try
                                                {
                                                    if (WIDEROK == 1)
                                                    {
                                                        if ((lblRFSurname.Text.Trim() == "") && (lblSurname.Text.Trim() != "")) { StatusCode = 3; }
                                                        if ((lblRFForenames.Text.Trim() == "") && (lblForenames.Text.Trim() != "")) { StatusCode = 3; }
                                                        if ((lblRFNationality.Text.Trim() == "") && (lblNationality.Text.Trim() != "")) { StatusCode = 3; }
                                                        if ((lblRFSex.Text.Trim() == "") && (lblSex.Text.Trim() != "")) { StatusCode = 3; }
                                                        if ((lblRFDateOfBirth.Text.Trim() == "") && (lblDateOfBirth.Text.Trim() != "")) { StatusCode = 3; }
                                                        if ((lblRFDocNumber.Text.Trim() == "") && (lblDocumentNumber.Text.Trim() != "")) { StatusCode = 3; }
                                                    }
                                                }
                                                catch (Exception) { }
                                                string EXODATE = "";
                                                try
                                                {
                                                    if (lblRF_ExpireDate.Text.Trim() != "") { EXODATE = lblRF_ExpireDate.Text.Trim(); }
                                                    if (EXODATE == "")
                                                    {
                                                        if (lblMRZ_ExpireDate.Text.Trim() != "") { EXODATE = lblMRZ_ExpireDate.Text.Trim(); }
                                                    }
                                                }
                                                catch (Exception) { EXODATE = ""; }
                                                if (StatusCode == 1)
                                                {
                                                    if (EXODATE.Trim() != "")
                                                    {
                                                        // Hamid Said When Expires SHoulbe be in Refered .
                                                        if (ExpiredDateCheck(EXODATE, Get_Date()) == false) { StatusCode = 3; WIDEROK = 1; }
                                                    }
                                                    else
                                                    {
                                                        StatusCode = 3; // No Expire Date => Reffered
                                                        WIDEROK = 1;
                                                    }
                                                }
                                                if (WIDEROK == 1) { T_Error.Visible = true; } else { T_OK.Visible = true; }
                                                if (LBLD_1.Text.Trim() == "") { LBLD_1.Text = "NA"; }
                                                if (LBLD_2.Text.Trim() == "") { LBLD_2.Text = "NA"; }
                                                if (LBLD_3.Text.Trim() == "") { LBLD_3.Text = "NA"; }
                                                if (LBLD_4.Text.Trim() == "") { LBLD_4.Text = "NA"; }
                                                if (LBLD_5.Text.Trim() == "") { LBLD_5.Text = "NA"; }
                                                if (lblDocumentType.Text.Trim().ToLower() == "unknown document") { lblDocumentType.Text = "Passport"; }
                                                SendToServer(StatusCode);
                                                PL_01_Splash.Visible = false;
                                                PL_02_Splash.Visible = false;
                                                PL_03_Splash.Visible = false;
                                                PL_04_Splash.Visible = true;
                                                Activity_Now = 5;
                                            }
                                            else
                                            {
                                                Application.DoEvents();
                                                SystemPause = true;
                                                AddLog("- Scan Log : " + "Proccess End" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                // Create Application :
                                                AddLog("- Scan Log : " + "Create New Application" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                string APPID = "";
                                                APPID = Scanner_HttpPost("DL_01_CreateApplication", "CID=" + CompanyID + "&DID=" + DealerID + "&UID=" + UserID + "&IP=" + IPAddress + "&UNM=" + Auth_Username + "&PAS=" + Auth_Password);
                                                Application.DoEvents();
                                                APPID = APPID.Replace("\"", "").Trim();
                                                if (APPID.IndexOf("RR_") < 0)
                                                {
                                                    AddLog("- Scan Log : " + "Application Created - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                    // Send Image To Server :
                                                    for (int i = 1; i <= 7; i++)
                                                    {
                                                        Application.DoEvents();
                                                        if (UserCancel == true) { break; }
                                                        string ImageNameLog = "";
                                                        PictureBox PBL = new PictureBox();
                                                        switch (i)
                                                        {
                                                            case 1: { PBL = photoImage; ImageNameLog = "FacePhoto"; break; }
                                                            case 2: { PBL = visibleImage; ImageNameLog = "IR Front"; break; }
                                                            case 3: { PBL = visibleImageRear; ImageNameLog = "IR Back"; break; }
                                                            case 4: { PBL = irImage; ImageNameLog = "Normal Front"; break; }
                                                            case 5: { PBL = irImageRear; ImageNameLog = "Normal Back"; break; }
                                                            case 6: { PBL = uvImage; ImageNameLog = "UV Front"; break; }
                                                            case 7: { PBL = uvImageRear; ImageNameLog = "UV Back"; break; }
                                                        }
                                                        if (PBL.Image != null)
                                                        {
                                                            try
                                                            {
                                                                AddLog("- Scan Log : Starting Upload " + " - " + ImageNameLog + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                if (Directory.Exists(Application.StartupPath + @"\Photos") == false) { Directory.CreateDirectory(Application.StartupPath + @"\Photos"); }
                                                                PBL.Image.Save(Application.StartupPath + @"\Photos\" + i.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                                                                var client = new RestClient(ServerAddress + "/api/DL_02_UploadImage?AppID=" + APPID + "&ImageCode=" + i.ToString());
                                                                client.Timeout = -1;
                                                                var request = new RestRequest(Method.POST);
                                                                request.AddHeader("Content-Type", "multipart/form-data");
                                                                request.AddFile("files[testfile1.pot]", Application.StartupPath + @"\Photos\" + i.ToString() + ".jpg");
                                                                IRestResponse IRRES = client.Execute(request);
                                                                if (File.Exists(Application.StartupPath + @"\Photos\" + i.ToString() + ".jpg") == true) { File.Delete(Application.StartupPath + @"\Photos\" + i.ToString() + ".jpg"); }
                                                                Application.DoEvents();
                                                                if (IRRES.ToString().Replace("\"", "").Trim().ToUpper() == "OK")
                                                                {
                                                                    AddLog("- Scan Log : Image [ " + ImageNameLog + " ] Uploaded Successful" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                }
                                                                else
                                                                {
                                                                    AddLog("- Scan Log : Image Upload Failed" + " - " + ImageNameLog + " : " + IRRES.ToString().Replace("\"", "").Trim() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                }
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                AddLog("- Scan Log : Image Upload Failed" + " - " + ImageNameLog + " : " + e.Message.Trim() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            AddLog("- Scan Log : Image Upload Failed" + " - " + ImageNameLog + " : Not Founded" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        }
                                                        Application.DoEvents();
                                                    }
                                                    Application.DoEvents();
                                                    // Clear Image Temp :
                                                    if (Directory.Exists(Application.StartupPath + @"\Photos") == true)
                                                    {
                                                        try
                                                        {
                                                            Directory.Delete(Application.StartupPath + @"\Photos", true);
                                                            AddLog("- Scan Log : Local Image Temp Folder Removed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        }
                                                        catch (Exception)
                                                        {
                                                            AddLog("- Scan Log : Local Image Temp Folder Removing Error" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        }
                                                    }
                                                    Application.DoEvents();
                                                    if (UserCancel == true) { break; }
                                                    // Publish Application :
                                                    AddLog("- Scan Log : " + "Publish Application - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                    Application.DoEvents();
                                                    string ResPublish = "";
                                                    ResPublish = Scanner_HttpPost("DL_03_Publish", "CID=" + CompanyID + "&DID=" + DealerID + "&UID=" + UserID + "&APPID=" + APPID + "&UNM=" + Auth_Username + "&PAS=" + Auth_Password);
                                                    Application.DoEvents();
                                                    ResPublish = ResPublish.Replace("\"", "").Trim();
                                                    if (ResPublish.IndexOf("RR_") < 0)
                                                    {
                                                        if (UserCancel == true) { break; }
                                                        AddLog("- Scan Log : " + "Get Application Status - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        Application.DoEvents();
                                                        // Wait for Response :
                                                        bool ErroPageShow = false;
                                                        bool EditPageShow = false;
                                                        bool WaitForRes = true;
                                                        int WaitMin = 0;
                                                        string AppStatusCode = "0";
                                                        while (WaitForRes == true)
                                                        {
                                                            if (UserCancel == true) { break; }
                                                            Wait(1000);
                                                            if (UserCancel == true) { break; }
                                                            Wait(1000);
                                                            if (UserCancel == true) { break; }
                                                            Wait(1000);
                                                            if (UserCancel == true) { break; }
                                                            Wait(1000);
                                                            if (UserCancel == true) { break; }
                                                            Wait(1000);
                                                            if (UserCancel == true) { break; }
                                                            AppStatusCode = Scanner_HttpPost("DL_04_ApplicationStatus", "APPID=" + APPID);
                                                            AppStatusCode = AppStatusCode.Replace("\"", "").Trim().ToString();
                                                            if ((AppStatusCode != "") && (AppStatusCode != "0") && (AppStatusCode != "1") && (AppStatusCode != "2"))
                                                            {
                                                                WaitForRes = false;
                                                                ErroPageShow = false;
                                                                //if (AppStatusCode == "8") { EditPageShow = true; }
                                                                EditPageShow = true;
                                                            }
                                                            else
                                                            {
                                                                WaitMin++;
                                                                if (WaitMin >= 20) { WaitForRes = false; ErroPageShow = true; EditPageShow = false; }
                                                            }
                                                        }
                                                        AddLog("- Scan Log : " + "Application Last Status : " + AppStatusCode + " - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        Application.DoEvents();
                                                        if (UserCancel == true) { break; }
                                                        if (ErroPageShow == false)
                                                        {
                                                            if (EditPageShow == true)
                                                            {
                                                                // Get Response :
                                                                AddLog("- Scan Log : " + "Get Application Result - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                string FieldResult = "";
                                                                List<ResultItems> ResITM = new List<ResultItems>();
                                                                if (UserCancel == true) { break; }
                                                                try
                                                                {
                                                                    FieldResult = Scanner_HttpPost("DL_05_Result", "APPID=" + APPID);
                                                                    FieldResult = FieldResult.Trim();
                                                                    if (FieldResult.Substring(0, 1) == "\"") { FieldResult = FieldResult.Substring(1, FieldResult.Length - 1); }
                                                                    FieldResult = FieldResult.Trim();
                                                                    if (FieldResult.Substring(FieldResult.Length - 1, 1) == "\"") { FieldResult = FieldResult.Substring(0, FieldResult.Length - 1); }
                                                                    FieldResult = FieldResult.Replace("\\", "");
                                                                    FieldResult = FieldResult.Trim();
                                                                    ResITM = JsonConvert.DeserializeObject<List<ResultItems>>(FieldResult);
                                                                }
                                                                catch (Exception) { }
                                                                if (UserCancel == true) { break; }
                                                                AddLog("- Scan Log : " + "Create Dynamic Object For Edit - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                LBL_1.Text = "Firstname :"; LBL_1.Visible = true;
                                                                LBL_2.Text = "Surname :"; LBL_2.Visible = true;
                                                                LBL_3.Text = "Date of Birth :"; LBL_3.Visible = true;
                                                                LBL_4.Text = "Licence Number :"; LBL_4.Visible = true;
                                                                LBL_5.Text = "Licence Expiry :"; LBL_5.Visible = true;
                                                                LBL_6.Text = "Address :"; LBL_6.Visible = true;
                                                                LBL_1.ForeColor = Color.Black;
                                                                LBL_2.ForeColor = Color.Black;
                                                                LBL_3.ForeColor = Color.Black;
                                                                LBL_4.ForeColor = Color.Black;
                                                                LBL_5.ForeColor = Color.Black;
                                                                LBL_6.ForeColor = Color.Black;
                                                                label47.Visible = false;
                                                                TXT_1.Text = ""; TXT_1.Visible = true;
                                                                TXT_2.Text = ""; TXT_2.Visible = true;
                                                                TXT_3.Text = ""; TXT_3.Visible = true;
                                                                TXT_4.Text = ""; TXT_4.Visible = true;
                                                                TXT_5.Text = ""; TXT_5.Visible = true;
                                                                TXT_6.Text = ""; TXT_6.Visible = true;
                                                                int DTIT = 0;
                                                                foreach (ResultItems RW in ResITM)
                                                                {
                                                                    if (UserCancel == true) { break; }
                                                                    switch (RW.ID.ToString().Trim())
                                                                    {
                                                                        case "1": { TXT_1.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                        case "2": { TXT_2.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                        case "3": { TXT_3.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                        case "4": { TXT_4.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                        case "5": { TXT_5.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                        case "6": { TXT_6.Text = RW.Value.Trim(); DTIT = 1; break; }
                                                                    }
                                                                }
                                                                if (DTIT == 0)
                                                                {
                                                                    TXT_1.Text = "";
                                                                    TXT_2.Text = "";
                                                                    TXT_3.Text = "";
                                                                    TXT_4.Text = "";
                                                                    TXT_5.Text = "";
                                                                    TXT_6.Text = "";
                                                                    DTIT = 1;
                                                                }
                                                                //----------------------------------------------------------------------------------------------------
                                                                // Accuant Date / Time remove :
                                                                try
                                                                {
                                                                    if (UserCancel == true) { break; }
                                                                    if (LBL_1.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_1.Text = TXT_1.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_2.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_2.Text = TXT_2.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_3.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_3.Text = TXT_3.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_4.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_4.Text = TXT_4.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_5.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_5.Text = TXT_5.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_6.Text.ToLower().Trim() == ("Date of Birth :").ToLower().Trim()) { TXT_6.Text = TXT_6.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_1.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_1.Text = TXT_1.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_2.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_2.Text = TXT_2.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_3.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_3.Text = TXT_3.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_4.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_4.Text = TXT_4.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_5.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_5.Text = TXT_5.Text.Split(' ')[0].ToString().Trim(); }
                                                                    if (LBL_6.Text.ToLower().Trim() == ("Licence Expiry :").ToLower().Trim()) { TXT_6.Text = TXT_6.Text.Split(' ')[0].ToString().Trim(); }
                                                                }
                                                                catch (Exception)
                                                                { }
                                                                //----------------------------------------------------------------------------------------------------
                                                                // Show Result Page :
                                                                if (UserCancel == true) { break; }
                                                                AddLog("- Scan Log : " + "Show Application Status - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                PageProcc_Show = false;
                                                                if (UserCancel == true) { break; }
                                                                while (Finish_Confirm == false) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                if (UserCancel == true) { break; }
                                                                button7.Enabled = true;
                                                                label28.Visible = false;
                                                                label30.Visible = false;
                                                                PL_01_Splash.Visible = false;
                                                                PL_02_Splash.Visible = false;
                                                                PL_03_Splash.Visible = false;
                                                                PL_04_Splash.Visible = false;
                                                                PL_Error.Visible = false;
                                                                PL_Show_Info.Visible = false;
                                                                PL_DVLA.Visible = false;
                                                                PL_DVLA_WT.Visible = false;
                                                                PL_DVLA_OK.Visible = false;
                                                                PL_Succ.Visible = false;
                                                                PL_Submit.Visible = false;




                                                                PL_Show_Info.Enabled = true;
                                                                PL_Show_Info.Visible = true;
                                                                if (UserCancel == true) { break; }
                                                                // Wait For Accept Response :
                                                                KeyboardActived = false;
                                                                Info_Confirm = false;
                                                                if (UserCancel == true) { break; }
                                                                while (Info_Confirm == false) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                if (UserCancel == true) { break; }
                                                                KeyboardActived = true;
                                                                button7.Enabled = false;
                                                                PL_Show_Info.Enabled = false;
                                                                AddLog("- Scan Log : " + "Accept Application Result By User - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                // Send Last Changes :
                                                                AddLog("- Scan Log : " + "Save Application Last Result - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                if (TXT_1.Visible == true) { var T1 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=1&Key=" + (LBL_1.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_1.Text.Trim()); }
                                                                if (TXT_2.Visible == true) { var T2 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=2&Key=" + (LBL_2.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_2.Text.Trim()); }
                                                                Application.DoEvents();
                                                                if (TXT_3.Visible == true) { var T3 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=3&Key=" + (LBL_3.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_3.Text.Trim()); }
                                                                if (TXT_4.Visible == true) { var T4 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=4&Key=" + (LBL_4.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_4.Text.Trim()); }
                                                                Application.DoEvents();
                                                                if (TXT_5.Visible == true) { var T5 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=5&Key=" + (LBL_5.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_5.Text.Trim()); }
                                                                if (TXT_6.Visible == true) { var T6 = Scanner_HttpPost("DL_06_SaveResult", "APPID=" + APPID + "&DocID=6&Key=" + (LBL_6.Text.Replace(":", "").Replace("*", "").Trim()).Trim() + "&Value=" + TXT_6.Text.Trim()); }
                                                                Application.DoEvents();
                                                                // Show Phone And Email Page :
                                                                //TXT_Email.Text = "";
                                                                //TXT_Phone.Text = "";
                                                                //Btn_Finished.Enabled = true;
                                                                //label28.Visible = false;
                                                                //label30.Visible = false;
                                                                //PL_01_Splash.Visible = false;
                                                                //PL_02_Splash.Visible = false;
                                                                //PL_03_Splash.Visible = false;
                                                                //PL_04_Splash.Visible = false;
                                                                //PL_Error.Visible = false;
                                                                //PL_Show_Info.Visible = false;
                                                                //PL_Succ.Visible = false;
                                                                //PL_Submit.Visible = false;
                                                                //PL_Submit.Enabled = true;
                                                                //PL_Submit.Visible = true;
                                                                //Finish_Confirm = false;
                                                                //while (Finish_Confirm == false) { Application.DoEvents(); }
                                                                //Btn_Finished.Enabled = false;
                                                                //PL_Submit.Enabled = false;
                                                                // Send Email And Phone To Server :
                                                                if (UserCancel == true) { break; }
                                                                var TL = Scanner_HttpPost("DL_08_EMPH", "APPID=" + APPID + "&E=" + TXT_Email.Text.Trim() + "&P=" + TXT_Phone.Text.Trim());
                                                                // Send Call Back URl :
                                                                if (UserCancel == true) { break; }
                                                                CallBackURL_Post(APPID);
                                                                Application.DoEvents();
                                                                if (DVLA_En == 1)
                                                                {
                                                                    // DVLA Form :
                                                                    label28.Visible = false;
                                                                    label30.Visible = false;
                                                                    PL_01_Splash.Visible = false;
                                                                    PL_02_Splash.Visible = false;
                                                                    PL_03_Splash.Visible = false;
                                                                    PL_04_Splash.Visible = false;
                                                                    PL_Error.Visible = false;
                                                                    PL_Show_Info.Visible = false;
                                                                    PL_Submit.Visible = false;
                                                                    textBox6.Text = "";
                                                                    textBox7.Text = "";
                                                                    PL_DVLA_WT.Visible = false;
                                                                    PL_DVLA_OK.Visible = false;
                                                                    PageDVLAWait = true;
                                                                    label39.Enabled = true;
                                                                    label40.Enabled = true;
                                                                    label41.Enabled = true;
                                                                    label42.Enabled = true;
                                                                    label43.Enabled = true;
                                                                    textBox6.Enabled = true;
                                                                    textBox7.Enabled = true;
                                                                    button8.Enabled = true;
                                                                    PL_DVLA.Visible = true;
                                                                    PL_DVLA.Enabled = true;
                                                                    Application.DoEvents();
                                                                    while (PageDVLAWait == true) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                    if (UserCancel == true) { break; }
                                                                    label39.Enabled = false;
                                                                    label40.Enabled = false;
                                                                    label41.Enabled = false;
                                                                    label42.Enabled = false;
                                                                    label43.Enabled = false;
                                                                    textBox6.Enabled = false;
                                                                    textBox7.Enabled = false;
                                                                    button8.Enabled = false;
                                                                    PL_DVLA_WT.Visible = true;
                                                                    PL_DVLA_WT.Enabled = true;
                                                                    WGPLWT.Visible = true;
                                                                    WGPLWT.Enabled = true;
                                                                    Application.DoEvents();
                                                                    Wait(3000);
                                                                    PL_DVLA.Visible = false;
                                                                    PL_DVLA_WT.Visible = false;
                                                                    PL_DVLA_OK.Visible = true;
                                                                    PL_DVLA_OK.Enabled = true;
                                                                    PageDVLAWait = true;
                                                                    Application.DoEvents();
                                                                    while (PageDVLAWait == true) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                    if (UserCancel == true) { break; }
                                                                }
                                                                // Show Thanks Page :
                                                                AddLog("- Scan Log : " + "End Application With Accept - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                label28.Visible = false;
                                                                label30.Visible = false;
                                                                PL_01_Splash.Visible = false;
                                                                PL_02_Splash.Visible = false;
                                                                PL_03_Splash.Visible = false;
                                                                PL_04_Splash.Visible = false;
                                                                PL_Error.Visible = false;
                                                                PL_Show_Info.Visible = false;
                                                                PL_Submit.Visible = false;
                                                                PL_DVLA.Visible = false;
                                                                PL_DVLA_WT.Visible = false;
                                                                PL_DVLA_OK.Visible = false;
                                                                PL_Succ.Enabled = true;
                                                                PL_Succ.Visible = true;
                                                            }
                                                            else
                                                            {
                                                                // Show Phone And Email Page :
                                                                //TXT_Email.Text = "";
                                                                //TXT_Phone.Text = "";
                                                                //Btn_Finished.Enabled = true;
                                                                //label28.Visible = false;
                                                                //label30.Visible = false;
                                                                //PL_01_Splash.Visible = false;
                                                                //PL_02_Splash.Visible = false;
                                                                //PL_03_Splash.Visible = false;
                                                                //PL_04_Splash.Visible = false;
                                                                //PL_Error.Visible = false;
                                                                //PL_Show_Info.Visible = false;
                                                                //PL_Succ.Visible = false;
                                                                //PL_Submit.Visible = false;
                                                                //PL_Submit.Enabled = true;
                                                                //PL_Submit.Visible = true;
                                                                //Finish_Confirm = false;
                                                                if (UserCancel == true) { break; }
                                                                while (Finish_Confirm == false) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                if (UserCancel == true) { break; }
                                                                //Btn_Finished.Enabled = false;
                                                                // Send Email And Phone To Server :
                                                                var TL = Scanner_HttpPost("DL_08_EMPH", "APPID=" + APPID + "&E=" + TXT_Email.Text.Trim() + "&P=" + TXT_Phone.Text.Trim());
                                                                // Send Call Back URl :
                                                                if (UserCancel == true) { break; }
                                                                CallBackURL_Post(APPID);
                                                                KeyboardActived = false;
                                                                Application.DoEvents();
                                                                if (DVLA_En == 1)
                                                                {
                                                                    // DVLA Form :
                                                                    label28.Visible = false;
                                                                    label30.Visible = false;
                                                                    PL_01_Splash.Visible = false;
                                                                    PL_02_Splash.Visible = false;
                                                                    PL_03_Splash.Visible = false;
                                                                    PL_04_Splash.Visible = false;
                                                                    PL_Error.Visible = false;
                                                                    PL_Show_Info.Visible = false;
                                                                    PL_Submit.Visible = false;
                                                                    textBox6.Text = "";
                                                                    textBox7.Text = "";
                                                                    PL_DVLA_WT.Visible = false;
                                                                    PL_DVLA_OK.Visible = false;
                                                                    PageDVLAWait = true;
                                                                    label39.Enabled = true;
                                                                    label40.Enabled = true;
                                                                    label41.Enabled = true;
                                                                    label42.Enabled = true;
                                                                    label43.Enabled = true;
                                                                    textBox6.Enabled = true;
                                                                    textBox7.Enabled = true;
                                                                    button8.Enabled = true;
                                                                    PL_DVLA.Visible = true;
                                                                    PL_DVLA.Enabled = true;
                                                                    Application.DoEvents();
                                                                    while (PageDVLAWait == true) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                    if (UserCancel == true) { break; }
                                                                    label39.Enabled = false;
                                                                    label40.Enabled = false;
                                                                    label41.Enabled = false;
                                                                    label42.Enabled = false;
                                                                    label43.Enabled = false;
                                                                    textBox6.Enabled = false;
                                                                    textBox7.Enabled = false;
                                                                    button8.Enabled = false;
                                                                    PL_DVLA_WT.Visible = true;
                                                                    PL_DVLA_WT.Enabled = true;
                                                                    WGPLWT.Visible = true;
                                                                    WGPLWT.Enabled = true;
                                                                    Application.DoEvents();
                                                                    Wait(3000);
                                                                    PL_DVLA.Visible = false;
                                                                    PL_DVLA_WT.Visible = false;
                                                                    PL_DVLA_OK.Visible = true;
                                                                    PL_DVLA_OK.Enabled = true;
                                                                    PageDVLAWait = true;
                                                                    Application.DoEvents();
                                                                    while (PageDVLAWait == true) { if (UserCancel == true) { break; } Application.DoEvents(); }
                                                                    if (UserCancel == true) { break; }
                                                                }
                                                                // Show Thanks Page :
                                                                if (UserCancel == true) { break; }
                                                                AddLog("- Scan Log : " + "End Application Without Accept - AppID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                                Application.DoEvents();
                                                                label28.Visible = false;
                                                                label30.Visible = false;
                                                                PL_01_Splash.Visible = false;
                                                                PL_02_Splash.Visible = false;
                                                                PL_03_Splash.Visible = false;
                                                                PL_04_Splash.Visible = false;
                                                                PL_Error.Visible = false;
                                                                PL_Show_Info.Visible = false;
                                                                PL_Submit.Visible = false;
                                                                PL_DVLA.Visible = false;
                                                                PL_DVLA_WT.Visible = false;
                                                                PL_DVLA_OK.Visible = false;
                                                                PL_Succ.Enabled = true;
                                                                PL_Succ.Visible = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            AddLog("- Scan Log : " + "Application Validate Failed - ID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                            // Show Error Page :
                                                            CallBackURL_Post(APPID);
                                                            Application.DoEvents();
                                                            label32.Text = "The server was not responsive, please try again";
                                                            label28.Visible = false;
                                                            label30.Visible = false;
                                                            PL_01_Splash.Visible = false;
                                                            PL_02_Splash.Visible = false;
                                                            PL_03_Splash.Visible = false;
                                                            PL_04_Splash.Visible = false;
                                                            PL_Error.Visible = false;
                                                            PL_Show_Info.Visible = false;
                                                            PL_Succ.Visible = false;
                                                            PL_Submit.Visible = false;
                                                            PL_Error.Enabled = true;
                                                            PL_Error.Visible = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        AddLog("- Scan Log : " + "Publish Application Failed - ID : " + APPID + " - Code : " + ResPublish + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                        // Show Error Page :
                                                        CallBackURL_Post(APPID);
                                                        Application.DoEvents();
                                                        label32.Text = "Oops! Something went wrong ...";
                                                        label28.Visible = false;
                                                        label30.Visible = false;
                                                        PL_01_Splash.Visible = false;
                                                        PL_02_Splash.Visible = false;
                                                        PL_03_Splash.Visible = false;
                                                        PL_04_Splash.Visible = false;
                                                        PL_Error.Visible = false;
                                                        PL_Show_Info.Visible = false;
                                                        PL_Succ.Visible = false;
                                                        PL_Submit.Visible = false;
                                                        PL_Error.Enabled = true;
                                                        PL_Error.Visible = true;
                                                    }
                                                }
                                                else
                                                {
                                                    AddLog("- Scan Log : " + "Create New Application Failed - Code : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                                    // Show Error Page :
                                                    label32.Text = "Oops! Something went wrong ...";
                                                    label28.Visible = false;
                                                    label30.Visible = false;
                                                    PL_01_Splash.Visible = false;
                                                    PL_02_Splash.Visible = false;
                                                    PL_03_Splash.Visible = false;
                                                    PL_04_Splash.Visible = false;
                                                    PL_Error.Visible = false;
                                                    PL_Show_Info.Visible = false;
                                                    PL_Succ.Visible = false;
                                                    PL_Submit.Visible = false;
                                                    PL_Error.Enabled = true;
                                                    PL_Error.Visible = true;
                                                }
                                                Activity_Now = 5;
                                                if (Doc_In == true)
                                                {
                                                    label28.Visible = true;
                                                    label30.Visible = true;
                                                    Activity_Now = 5;
                                                }
                                                else
                                                {
                                                    label28.Visible = false;
                                                    label30.Visible = false;
                                                    Wait(3000);
                                                    Activity_Now = 1;
                                                    SBtnSelect = 0;
                                                }
                                                SystemPause = false;
                                            }
                                            break;
                                        }
                                    //-------------------------------------------------------------------------------------------------------
                                    case 4:
                                        {
                                            Wait(1000);
                                            Activity_Now = 1;
                                            SBtnSelect = 0;
                                            AddLog("- Scan Log : " + "Doc Removed" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                                            break;
                                        }
                                        //-------------------------------------------------------------------------------------------------------
                                }
                            }
                        }
                    }
                    Application.DoEvents();
                }
            }
            catch (Exception)
            {
                Pnl_Btn.Visible = false;
                label28.Visible = false;
                label30.Visible = false;
                PL_01_Splash.Visible = false;
                PL_02_Splash.Visible = false;
                PL_03_Splash.Visible = false;
                PL_04_Splash.Visible = false;
                PL_Error.Visible = false;
                PL_Succ.Visible = false;
                PL_Show_Info.Visible = false;
                PL_Submit.Visible = false;
                PL_Error.Visible = true;
                PL_Error.Enabled = true;
            }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        private void CallBackURL_Post(string APPID)
        {
            try
            {
                AddLog("- Scan Log : " + "Sending Callback URL - ID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                var CBU = Scanner_HttpPost("DL_07_CallBackURL", "APPID=" + APPID);
            }
            catch (Exception)
            {
                AddLog("- Scan Log : " + "Callback URL Send Failed - ID : " + APPID + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Scanner Function And Events :
        private void DataToSend_Reload()
        {
            MMM.Readers.FullPage.ReaderSettings lSettings;
            MMM.Readers.ErrorCode lErrorCode = MMM.Readers.FullPage.Reader.GetSettings(out lSettings);
            if (lErrorCode == MMM.Readers.ErrorCode.NO_ERROR_OCCURRED)
            {
                if ((lSettings.puDataToSend.special & MMM.Readers.FullPage.DataSendSet.Flags.UVIMAGE) != 0) { lSettings.puCameraSettings.uv.puAmbientRemoval = MMM.Readers.Modules.Camera.AmbientRemovalMethod.MRAS_BASIC_AMBIENT_REMOVAL; }
                PopulateUHFLists(lSettings.puDataToSend.uhf);
                PopulateRFIDLists(lSettings.puDataToSend.rfid);
                PopulatePluginLists();
            }
        }

        private void PopulatePluginLists()
        {
            int lPluginNameLength = 1;
            for (int i = 0; (lPluginNameLength > 0); i++)
            {
                String lPluginName = "";
                MMM.Readers.ErrorCode lErrorCode =
                    MMM.Readers.FullPage.Reader.GetPluginName(ref lPluginName, i);

                lPluginNameLength = lPluginName.Length;

                if (lPluginNameLength > 0)
                {
                    bool lEnabled = false;

                    MMM.Readers.FullPage.Reader.IsPluginEnabled(lPluginName, ref lEnabled);
                    if (lEnabled)
                        SelectedPluginList.Items.Add(lPluginName);
                    else
                        AvailablePluginList.Items.Add(lPluginName);
                }
            }
        }

        private void StorePlugins()
        {
            foreach (String lPluginName in AvailablePluginList.Items)
                MMM.Readers.FullPage.Reader.EnablePlugin(lPluginName, false);

            foreach (String lPluginName in SelectedPluginList.Items)
                MMM.Readers.FullPage.Reader.EnablePlugin(lPluginName, true);
        }

        private void PopulateUHFLists(MMM.Readers.FullPage.UHFDataToSend aDataToSend)
        {
            ListBox lCurrentList = UHFAvailDataItems;
            for (int lOrder = 0; lOrder < 3; lOrder++)
            {
                if (lOrder > 0)
                    lCurrentList = UHFSelectDataItems;

                if (aDataToSend.puEPC == lOrder)
                    lCurrentList.Items.Add("EPC Tag");

                if (aDataToSend.puTagID == lOrder)
                    lCurrentList.Items.Add("Tag ID");
            }
        }

        private void StoreUHFOrder(ref MMM.Readers.FullPage.UHFDataToSend aDataToSend)
        {
            for (int lListCtrlIndex = 0; lListCtrlIndex < 2; lListCtrlIndex++)
            {
                ListBox lListbox = (lListCtrlIndex == 0) ? UHFAvailDataItems : UHFSelectDataItems;

                for (int lIndex = 0; lIndex < lListbox.Items.Count; lIndex++)
                {
                    if (!(lListbox.Items[lIndex] is String))
                        continue;

                    int lOrder = (lListCtrlIndex > 0) ? lIndex + 1 : 0;
                    String lItem = lListbox.Items[lIndex] as String;

                    if (lItem == "EPC Tag")
                        aDataToSend.puEPC = lOrder;

                    else if (lItem == "Tag ID")
                        aDataToSend.puTagID = lOrder;
                }
            }
        }

        private void PopulateRFIDLists(MMM.Readers.FullPage.RFIDDataToSend aDataToSend)
        {
            ListBox lCurrentList = RFIDAvailDataItems;
            for (int lOrder = 0; lOrder < 50; lOrder++)
            {
                if (lOrder > 0)
                    lCurrentList = RFIDSelectDataItems;

                if (aDataToSend.puEFComFile == lOrder)
                    lCurrentList.Items.Add("EF.COM file");

                if (aDataToSend.puEFSodFile == lOrder)
                    lCurrentList.Items.Add("EF.SOD file");

                if (aDataToSend.puAirBaudRate == lOrder)
                    lCurrentList.Items.Add("Air Baud Rate");

                if (aDataToSend.puActiveAuthentication == lOrder)
                    lCurrentList.Items.Add("Active Authentication");

                if (aDataToSend.puValidateDocSignerCert == lOrder)
                    lCurrentList.Items.Add("Validate D/S Cert.");

                if (aDataToSend.puChipID == lOrder)
                    lCurrentList.Items.Add("Chip ID");

                if (aDataToSend.puDG1MRZData == lOrder)
                    lCurrentList.Items.Add("DG1 MRZ");

                if (aDataToSend.puDG1DataEDL == lOrder)
                    lCurrentList.Items.Add("DG1 Data eDL");

                if (aDataToSend.puDG2FaceJPEG == lOrder)
                    lCurrentList.Items.Add("DG2 Photo");

                if (aDataToSend.puDG3Fingerprints == lOrder)
                    lCurrentList.Items.Add("DG3 Fingerprints");

                if (aDataToSend.puDG6FaceJPEG == lOrder)
                    lCurrentList.Items.Add("DG6 eDL Photo");

                if (aDataToSend.puDG7Fingerprints == lOrder)
                    lCurrentList.Items.Add("DG7 eDL Fingerprints");

                if (aDataToSend.puCrosscheckEFComEFSod == lOrder)
                    lCurrentList.Items.Add("Crosscheck EF.COM / EF.SOD");

                for (int lDataGroup = 0; lDataGroup < 16; lDataGroup++)
                {
                    if (aDataToSend.puDGFile[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0} file", lDataGroup + 1));

                    if (aDataToSend.puValidateDG[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0} validate", lDataGroup + 1));
                }

                for (int lDataGroup = 0; lDataGroup < 22; lDataGroup++)
                {
                    if (aDataToSend.puDGFileEID[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0}_eID file", lDataGroup + 1));

                    if (aDataToSend.puValidateDGEID[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0}_eID validate", lDataGroup + 1));
                }

                for (int lDataGroup = 0; lDataGroup < 14; lDataGroup++)
                {
                    if (aDataToSend.puDGFileEDL[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0}_eDL file", lDataGroup + 1));

                    if (aDataToSend.puValidateDGEDL[lDataGroup + 1] == lOrder)
                        lCurrentList.Items.Add(String.Format("DG{0}_eDL validate", lDataGroup + 1));
                }

                if (aDataToSend.puValidateSignature == lOrder)
                    lCurrentList.Items.Add("Validate Signature");

                if (aDataToSend.puValidateSignedAttrs == lOrder)
                    lCurrentList.Items.Add("Validate Signed Attrs");

                if (aDataToSend.puGetBACStatus == lOrder)
                    lCurrentList.Items.Add("BAC Status");

                if (aDataToSend.puGetSACStatus == lOrder)
                    lCurrentList.Items.Add("SAC Status");
            }
        }

        private void StoreRFIDOrder(ref MMM.Readers.FullPage.RFIDDataToSend aDataToSend)
        {
            // To avoid having this switch twice, we'll just go around this for both list controls with a loop.
            // For the available list control, we want to set the order number to 0 for all items in it. For the
            // selected list control, we want to set the order number to the index in the list, plus 1.
            for (int lListCtrlIndex = 0; lListCtrlIndex < 2; lListCtrlIndex++)
            {
                ListBox lListbox = (lListCtrlIndex == 0) ? RFIDAvailDataItems : RFIDSelectDataItems;

                for (int lIndex = 0; lIndex < lListbox.Items.Count; lIndex++)
                {
                    if (!(lListbox.Items[lIndex] is String))
                        continue;

                    int lOrder = (lListCtrlIndex > 0) ? lIndex + 1 : 0;
                    String lItem = lListbox.Items[lIndex] as String;

                    if (lItem == "EF.COM file")
                        aDataToSend.puEFComFile = lOrder;

                    else if (lItem == "EF.SOD file")
                        aDataToSend.puEFSodFile = lOrder;

                    else if (lItem == "Air Baud Rate")
                        aDataToSend.puAirBaudRate = lOrder;

                    else if (lItem == "Active Authentication")
                        aDataToSend.puActiveAuthentication = lOrder;

                    else if (lItem == "Validate D/S Cert.")
                        aDataToSend.puValidateDocSignerCert = lOrder;

                    else if (lItem == "Chip ID")
                        aDataToSend.puChipID = lOrder;

                    else if (lItem == "DG1 MRZ")
                        aDataToSend.puDG1MRZData = lOrder;

                    else if (lItem == "DG1 Data eDL")
                        aDataToSend.puDG1DataEDL = lOrder;

                    else if (lItem == "DG2 Photo")
                        aDataToSend.puDG2FaceJPEG = lOrder;

                    else if (lItem == "DG3 Fingerprints")
                        aDataToSend.puDG3Fingerprints = lOrder;

                    else if (lItem == "DG6 eDL Photo")
                        aDataToSend.puDG6FaceJPEG = lOrder;

                    else if (lItem == "DG7 eDL Fingerprints")
                        aDataToSend.puDG7Fingerprints = lOrder;

                    else if (lItem == "Crosscheck EF.COM / EF.SOD")
                        aDataToSend.puCrosscheckEFComEFSod = lOrder;

                    else if (lItem == "Validate Signature")
                        aDataToSend.puValidateSignature = lOrder;

                    else if (lItem == "Validate Signed Attrs")
                        aDataToSend.puValidateSignedAttrs = lOrder;

                    else if (lItem == "BAC Status")
                        aDataToSend.puGetBACStatus = lOrder;

                    else if (lItem == "SAC Status")
                        aDataToSend.puGetSACStatus = lOrder;

                    else if (lItem.Contains("DG") && !(lItem.Contains("_eID") || lItem.Contains("_eDL")))
                    {
                        String lDGString = "";
                        foreach (Char lChar in lItem)
                        {
                            if (Char.IsNumber(lChar))
                                lDGString += lChar;
                        }
                        int lDataGroup = Int32.Parse(lDGString);

                        if (lItem.Contains("file") && lDataGroup <= aDataToSend.puDGFile.Length)
                            aDataToSend.puDGFile[lDataGroup] = lOrder;
                        else if (lItem.Contains("validate") && lDataGroup <= aDataToSend.puDGFile.Length)
                            aDataToSend.puValidateDG[lDataGroup] = lOrder;
                    }

                    else if (lItem.Contains("_eID"))
                    {
                        String lDGString = "";
                        foreach (Char lChar in lItem)
                        {
                            if (Char.IsNumber(lChar))
                                lDGString += lChar;
                        }
                        int lDataGroup = Int32.Parse(lDGString);

                        if (lItem.Contains("file") && lDataGroup <= aDataToSend.puDGFileEID.Length)
                            aDataToSend.puDGFileEID[lDataGroup] = lOrder;
                        else if (lItem.Contains("validate") && lDataGroup <= aDataToSend.puDGFileEID.Length)
                            aDataToSend.puValidateDGEID[lDataGroup] = lOrder;
                    }
                    else if (lItem.Contains("_eDL"))
                    {
                        String lDGString = "";
                        foreach (Char lChar in lItem)
                        {
                            if (Char.IsNumber(lChar))
                                lDGString += lChar;
                        }
                        int lDataGroup = Int32.Parse(lDGString);

                        if (lItem.Contains("file") && lDataGroup <= aDataToSend.puDGFileEDL.Length)
                            aDataToSend.puDGFileEDL[lDataGroup] = lOrder;
                        else if (lItem.Contains("validate") && lDataGroup <= aDataToSend.puDGFileEDL.Length)
                            aDataToSend.puValidateDGEDL[lDataGroup] = lOrder;
                    }
                }
            }
        }

        private void RFIDSelectAll()
        {
            Object[] lRawItems = new Object[RFIDAvailDataItems.Items.Count];
            RFIDAvailDataItems.Items.CopyTo(lRawItems, 0);
            RFIDAvailDataItems.Items.Clear();
            foreach (Object lRawItem in lRawItems) { if (lRawItem is String) { RFIDSelectDataItems.Items.Add(lRawItem); } }
        }

        private void RFIDDeSelectAll()
        {
            Object[] lRawItems = new Object[RFIDAvailDataItems.Items.Count];
            RFIDAvailDataItems.Items.CopyTo(lRawItems, 0);
            RFIDAvailDataItems.Items.Clear();
            RFIDSelectDataItems.Items.Clear();
        }

        void DataCallbackThreadHelper(MMM.Readers.FullPage.DataType aDataType, object aData)
        {
            if (THC.InvokeRequired)
            {
                THC.Invoke(
                    new MMM.Readers.FullPage.DataDelegate(DataCallback),
                    new object[] { aDataType, aData }
                );
            }
            else
            {
                DataCallback(aDataType, aData);
            }
        }

        void HighlightCodelineCheckDigits(MMM.Readers.CodelineData aCodeline)
        {
            for (int loop = 0; loop < aCodeline.CheckDigitDataListCount; loop++)
            {
                MMM.Readers.CodelineCheckDigitData lCDData = aCodeline.CheckDigitDataList[loop];
                int lIndex = lCDData.puCodelinePos;
                for (int line = 1; line < lCDData.puCodelineNumber; line++)
                {
                    switch (line)
                    {
                        case 1:
                            lIndex += aCodeline.Line1.Length;
                            ++lIndex;
                            break;
                        case 2:
                            lIndex += aCodeline.Line2.Length;
                            ++lIndex;
                            break;
                    }
                }
                richTextBoxCodeline.Select(lIndex, 1);
                if (lCDData.puValueExpected == lCDData.puValueRead)
                    richTextBoxCodeline.SelectionColor = Color.Green;
                else
                    richTextBoxCodeline.SelectionColor = Color.Red;
                richTextBoxCodeline.DeselectAll();
            }
        }

        void DataCallback(MMM.Readers.FullPage.DataType aDataType, object aData)
        {
            try
            {
                LogDataItem(aDataType, aData);
                if (aData != null)
                {
                    switch (aDataType)
                    {
                        case MMM.Readers.FullPage.DataType.CD_CODELINE_DATA:
                            {
                                MMM.Readers.CodelineData codeline = (MMM.Readers.CodelineData)aData;
                                richTextBoxCodeline.Text = codeline.Line1 + "\n" + codeline.Line2 + "\n" + codeline.Line3;
                                HighlightCodelineCheckDigits(codeline);
                                lblSurname.Text = codeline.Surname;
                                lblForenames.Text = codeline.Forenames;
                                lblNationality.Text = codeline.Nationality;
                                lblSex.Text = codeline.Sex;
                                lblDateOfBirth.Text = codeline.DateOfBirth.Day.ToString("00") + "/" + codeline.DateOfBirth.Month.ToString("00") + "/" + codeline.DateOfBirth.Year.ToString("00");
                                lblDocumentNumber.Text = codeline.DocNumber;
                                lblDocumentType.Text = codeline.DocType;
                                lblMRZ_ExpireDate.Text = codeline.ExpiryDate.Day.ToString("00") + "/" + codeline.ExpiryDate.Month.ToString("00") + "/" + "20" + codeline.ExpiryDate.Year.ToString("00");
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEIR:
                            {
                                irImage.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEVIS:
                            {
                                visibleImage.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEPHOTO:
                            {
                                photoImage.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEUV:
                            {
                                uvImage.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEIRREAR:
                            {
                                irImageRear.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEVISREAR:
                            {
                                visibleImageRear.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_IMAGEUVREAR:
                            {
                                uvImageRear.Image = aData as Bitmap;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCAIRBAUD:
                            {
                                lblAirBaudRate.Text = aData as string;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_BACKEY_CORRECTION:
                            {
                                bool lTemp = THC.InvokeRequired;
                                {
                                    System.Text.StringBuilder lStringBuilder = aData as System.Text.StringBuilder;
                                    if (lStringBuilder != null)
                                    {
                                        //FormBACKeyCorrection lForm = new FormBACKeyCorrection();
                                        //lForm.SetCodeline(lStringBuilder.ToString());
                                        //if (lForm.ShowDialog() == DialogResult.OK)
                                        //{
                                        //    lStringBuilder.Replace(
                                        //        lStringBuilder.ToString(),
                                        //        lForm.GetCodeline()
                                        //    );
                                        //}
                                    }
                                }
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_CODELINE_DATA:
                            {
                                MMM.Readers.CodelineData codeline = (MMM.Readers.CodelineData)aData;
                                lblRFCodeline.Text = codeline.Data;
                                lblRFSurname.Text = codeline.Surname;
                                lblRFForenames.Text = codeline.Forenames;
                                lblRFNationality.Text = codeline.Nationality;
                                lblRFSex.Text = codeline.Sex;
                                lblRFDateOfBirth.Text = codeline.DateOfBirth.Day.ToString("00") + "/" + codeline.DateOfBirth.Month.ToString("00") + "/" + codeline.DateOfBirth.Year.ToString("00");
                                lblRFDocNumber.Text = codeline.DocNumber;
                                lblRF_ExpireDate.Text = codeline.ExpiryDate.Day.ToString("00") + "/" + codeline.ExpiryDate.Month.ToString("00") + "/" + "20" + codeline.ExpiryDate.Year.ToString("00");
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_PHOTO:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_EDL_PHOTO:
                            {
                                byte[] lInputBuffer = aData as byte[];
                                try
                                {
                                    System.IO.Stream streamBuffer = new System.IO.MemoryStream();
                                    streamBuffer.Write(lInputBuffer, 0, lInputBuffer.Length);
                                    streamBuffer.Seek(0, System.IO.SeekOrigin.Begin);
                                    rfImage.Image = new Bitmap(streamBuffer);
                                }
                                catch (Exception)
                                {
                                    byte[] lOutputBuffer = null;
                                    MMM.Readers.Modules.Imaging.ConvertFormat(MMM.Readers.FullPage.ImageFormats.RTE_BMP, lInputBuffer, out lOutputBuffer);
                                    if (lOutputBuffer != null)
                                    {
                                        try
                                        {
                                            System.IO.Stream j2kStream = new System.IO.MemoryStream();
                                            j2kStream.Write(lOutputBuffer, 0, lOutputBuffer.Length);
                                            j2kStream.Seek(0, System.IO.SeekOrigin.Begin);
                                            rfImage.Image = new Bitmap(j2kStream);
                                        }
                                        catch (Exception)
                                        {
                                            //System.Windows.Forms.MessageBox.Show(decodeExcept.ToString());
                                        }
                                    }
                                }
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCCHIPID:
                            {
                                lblChipId.Text = aData as string;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG15_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG16_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG15_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG16_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG17_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG18_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG19_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG20_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG21_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG22_VALIDATE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_VALIDATE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNEDATTRS_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNEDATTRS_VALIDATE_CARD_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNEDATTRS_VALIDATE_CHIP_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNATURE_VALIDATE:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNATURE_VALIDATE_CARD_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCSIGNATURE_VALIDATE_CHIP_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_VALIDATE_DOC_SIGNER_CERT:
                            {
                                ListViewItem thisItem = validatedList.Items.Add(aDataType.ToString());

                                MMM.Readers.Modules.RF.ValidationCode lValidationResult = (MMM.Readers.Modules.RF.ValidationCode)aData;
                                thisItem.SubItems.Add(lValidationResult.ToString());
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_ACTIVE_AUTHENTICATION:
                        case MMM.Readers.FullPage.DataType.CD_SCBAC_STATUS:
                        case MMM.Readers.FullPage.DataType.CD_SAC_STATUS:
                        case MMM.Readers.FullPage.DataType.CD_SCTERMINAL_AUTHENTICATION_STATUS:
                        case MMM.Readers.FullPage.DataType.CD_SCCHIP_AUTHENTICATION_STATUS:
                            {
                                ListViewItem thisItem = validatedList.Items.Add(aDataType.ToString());

                                MMM.Readers.Modules.RF.TriState lState = (MMM.Readers.Modules.RF.TriState)aData;
                                thisItem.SubItems.Add(lState.ToString());
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG15_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG16_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG15_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG16_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG17_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG18_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG19_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG20_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG21_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG22_FILE_EID:
                        case MMM.Readers.FullPage.DataType.CD_SCEF_COM_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCEF_SOD_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCEF_CVCA_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCEF_CARD_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCEF_CHIP_SECURITY_FILE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_FILE_EDL:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_FILE_EDL:
                            {
                                byte[] lFileData = aData as byte[];

                                ListViewItem thisItem = dataFileList.Items.Add(aDataType.ToString());
                                thisItem.SubItems.Add(lFileData.Length.ToString() + " bytes");
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_UHF_EPC:
                            {
                                String lEPCString = "";
                                foreach (byte lByte in (byte[])aData)
                                {
                                    if (lEPCString.Length != 0)
                                        lEPCString += "-";
                                    lEPCString += lByte.ToString("X4");
                                }
                                //EPCField.Text = lEPCString;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_UHF_TAGID:
                            {
                                if (aData is MMM.Readers.FullPage.UHFTagIDData)
                                {
                                    //DisplayUHFTagID(
                                    //    (MMM.Readers.FullPage.UHFTagIDData)aData);
                                }
                                break;
                            }
                        #region eID
                        // eID related data for reference
                        case MMM.Readers.FullPage.DataType.CD_SCDG2_EID_ISSUING_ENTITY:
                            {
                                MMM.Readers.FullPage.EIDASIssuingEntity data = (MMM.Readers.FullPage.EIDASIssuingEntity)aData;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG3_EID_VALIDITY_PERIOD:
                            {
                                MMM.Readers.FullPage.EIDASValidityPeriod data = (MMM.Readers.FullPage.EIDASValidityPeriod)aData;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG9_EID_PLACE_OF_BIRTH:
                            {
                                MMM.Readers.FullPage.EIDASGeneralPlace data = (MMM.Readers.FullPage.EIDASGeneralPlace)aData;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG17_EID_PLACE_OF_RESIDENCE:
                            {
                                MMM.Readers.FullPage.EIDASPlaceOfResidence data = (MMM.Readers.FullPage.EIDASPlaceOfResidence)aData;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG12_EID_OPTIONAL_DATA_R:
                        case MMM.Readers.FullPage.DataType.CD_SCDG14_EID_WRITTEN_SIGNATURE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG18_EID_MUNICIPALITY_ID:
                        case MMM.Readers.FullPage.DataType.CD_SCDG21_EID_OPTIONAL_DATA_RW:
                            {
                                byte[] data = (byte[])aData;
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_EID_DOCUMENT_TYPE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG4_EID_GIVEN_NAMES:
                        case MMM.Readers.FullPage.DataType.CD_SCDG5_EID_FAMILY_NAMES:
                        case MMM.Readers.FullPage.DataType.CD_SCDG6_EID_NOM_DE_PLUME:
                        case MMM.Readers.FullPage.DataType.CD_SCDG7_EID_ACADEMIC_TITLE:
                        case MMM.Readers.FullPage.DataType.CD_SCDG8_EID_DATE_OF_BIRTH:
                        case MMM.Readers.FullPage.DataType.CD_SCDG10_EID_NATIONALITY:
                        case MMM.Readers.FullPage.DataType.CD_SCDG11_EID_SEX:
                        case MMM.Readers.FullPage.DataType.CD_SCDG13_EID_BIRTH_NAME:
                        case MMM.Readers.FullPage.DataType.CD_SCDG19_EID_RESIDENCE_PERMIT_1:
                        case MMM.Readers.FullPage.DataType.CD_SCDG20_EID_RESIDENCE_PERMIT_2:
                            {
                                string test = aData.ToString();
                                break;
                            }
                        #endregion
                        case MMM.Readers.FullPage.DataType.CD_SCDG1_EDL_DATA:
                            {
                                MMM.Readers.FullPage.EDLDataGroup1Data data = (MMM.Readers.FullPage.EDLDataGroup1Data)aData;
                                break;
                            }
                    }
                    switch (aDataType)
                    {
                        case MMM.Readers.FullPage.DataType.CD_SCBAC_STATUS:
                            {
                                MMM.Readers.Modules.RF.TriState lState = (MMM.Readers.Modules.RF.TriState)aData;
                                this.lblBACStatus.Text = lState.ToString();
                                break;
                            }
                        case MMM.Readers.FullPage.DataType.CD_SAC_STATUS:
                            {
                                MMM.Readers.Modules.RF.TriState lState = (MMM.Readers.Modules.RF.TriState)aData;
                                this.lblSACStatus.Text = lState.ToString();
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Lbl_ErrorPublic.Text = e.ToString();
            }
        }

        void EventCallbackThreadHelper(MMM.Readers.FullPage.EventCode aEventType)
        {
            if (THC.InvokeRequired)
            {
                THC.Invoke(
                    new MMM.Readers.FullPage.EventDelegate(EventCallback),
                    new object[] { aEventType }
                );
            }
            else
            {
                EventCallback(aEventType);
            }
        }

        void EventCallback(MMM.Readers.FullPage.EventCode aEventType)
        {
            try
            {
                LogEvent(aEventType);
                switch (aEventType)
                {
                    case MMM.Readers.FullPage.EventCode.SETTINGS_INITIALISED:
                        {
                            MMM.Readers.FullPage.ReaderSettings settings;
                            MMM.Readers.ErrorCode errorCode = MMM.Readers.FullPage.Reader.GetSettings(out settings);
                            if (errorCode == MMM.Readers.ErrorCode.NO_ERROR_OCCURRED)
                            {
                                settings.puDataToSend.send |= MMM.Readers.FullPage.DataSendSet.Flags.DOCMARKERS;
                                settings.puDataToSend.special = MMM.Readers.FullPage.DataSendSet.Flags.VISIBLEIMAGE | MMM.Readers.FullPage.DataSendSet.Flags.IRIMAGE;
                                MMM.Readers.FullPage.Reader.UpdateSettings(settings);
                                MMM.Readers.FullPage.Reader.EnableLogging(true, settings.puLoggingSettings.logLevel, (int)settings.puLoggingSettings.logMask, "HLNonBlockingExample.Net.log");
                                Cam_Init_1 = true;
                            }
                            else
                            {
                                Lbl_ErrorPublic.Text = "GetSettings failure, check for Settings structure mis-match. Error:" + errorCode.ToString();
                                Init_Activity_Complete = true;
                            }
                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.DEVICE_CONNECTED:
                        {
                            Cam_Init_2 = true;
                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.PLUGINS_INITIALISED:
                        {
                            int lIndex = 0;
                            string lPluginName = "";
                            while (MMM.Readers.FullPage.Reader.GetPluginName(ref lPluginName, lIndex) == MMM.Readers.ErrorCode.NO_ERROR_OCCURRED && lPluginName.Length > 0)
                            {
                                ++lIndex;
                            }
                            Cam_Init_3 = true;
                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.DOC_REMOVED:
                        {
                            if (SystemRunNew == true)
                            {
                                if (MMM.Readers.FullPage.Reader.GetState() != MMM.Readers.FullPage.ReaderState.READER_DISABLED)
                                {
                                    MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                                }
                                Activity_Now = 4;
                            }
                            Doc_In = false;
                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.DOC_ON_WINDOW:
                        {
                            if (FirstLoad == true)
                            {
                                if (MMM.Readers.FullPage.Reader.GetState() != MMM.Readers.FullPage.ReaderState.READER_DISABLED)
                                {
                                    MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_DISABLED, true);
                                    return;
                                }
                            }
                            if (SystemRunNew == true)
                            {
                                Activity_Now = 2;
                            }
                            Doc_In = true;
                            Clear();
                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.READING_DATA:
                        {

                            break;
                        }
                    case MMM.Readers.FullPage.EventCode.END_OF_DOCUMENT_DATA:
                        {
                            if (SystemRunNew == true)
                            {
                                Activity_Now = 3;
                            }
                            break;
                        }
                }
                UpdateState(MMM.Readers.FullPage.Reader.GetState());
            }
            catch (Exception e)
            {
                LogError(0, e.Message);
            }
        }

        void Clear()
        {
            richTextBoxCodeline.Text = "";
            lblSurname.Text = "";
            lblForenames.Text = "";
            lblNationality.Text = "";
            lblSex.Text = "";
            lblDateOfBirth.Text = "";
            lblDocumentNumber.Text = "";
            lblDocumentType.Text = "";
            irImage.Image = null;
            visibleImage.Image = null;
            photoImage.Image = null;
            uvImage.Image = null;
            irImageRear.Image = null;
            visibleImageRear.Image = null;
            uvImageRear.Image = null;
            lblRFCodeline.Text = "";
            lblRFSurname.Text = "";
            lblRFForenames.Text = "";
            lblRFNationality.Text = "";
            lblRFSex.Text = "";
            lblRFDateOfBirth.Text = "";
            lblRFDocNumber.Text = "";
            rfImage.Image = null;
            validatedList.Items.Clear();
            dataFileList.Items.Clear();
            lblChipId.Text = "";
            lblAirBaudRate.Text = "";
            lblBACStatus.Text = "";
            lblSACStatus.Text = "";
            lblMRZ_ExpireDate.Text = "";
            lblRF_ExpireDate.Text = "";
        }

        void ErrorCallbackThreadHelper(MMM.Readers.ErrorCode aErrorCode, string aErrorMessage)
        {
            if (THC.InvokeRequired)
            {
                THC.Invoke(
                    new MMM.Readers.ErrorDelegate(ErrorCallback),
                    new object[] { aErrorCode, aErrorMessage }
                );
            }
            else
            {
                ErrorCallback(aErrorCode, aErrorMessage);
            }
        }

        void ErrorCallback(MMM.Readers.ErrorCode aErrorCode, string aErrorMessage)
        {
            switch (aErrorCode)
            {
                case MMM.Readers.ErrorCode.ERROR_READER_NOT_CONNECTED:
                    {
                        Cam_Init_1 = false;
                        Cam_Init_2 = false;
                        Cam_Init_3 = false;
                        Init_Activity_Complete = true;
                        break;
                    }
            }
            if (aErrorMessage.IndexOf("Datagroup") < 0) { Lbl_ErrorPublic.Text = aErrorMessage.ToString(); }
            LogError(aErrorCode, aErrorMessage);
        }

        void WarningCallbackThreadHelper(MMM.Readers.WarningCode aWarningCode, string aWarningMessage)
        {
            if (THC.InvokeRequired)
            {
                THC.Invoke(
                    new MMM.Readers.WarningDelegate(WarningCallback),
                    new object[] { aWarningCode, aWarningMessage }
                );
            }
            else
            {
                WarningCallback(aWarningCode, aWarningMessage);
            }
        }

        void WarningCallback(MMM.Readers.WarningCode aWarningCode, string aWarningMessage)
        {
            LogWarning(aWarningCode, aWarningMessage);
        }

        bool CertificateCallbackThreadHelper(byte[] aCertIdentifier, MMM.Readers.Modules.RF.CertType aCertType, out byte[] aCertBuffer)
        {
            if (THC.InvokeRequired)
            {
                aCertBuffer = null;
                object[] lParams = new object[]
                {
                    aCertIdentifier, aCertType, aCertBuffer
                };
                bool lResult = (bool)THC.Invoke(new MMM.Readers.FullPage.CertificateDelegate(CertificateCallback), lParams);
                aCertBuffer = (byte[])lParams[2];
                return lResult;
            }
            else
            {
                return CertificateCallback(aCertIdentifier, aCertType, out aCertBuffer);
            }
        }

        bool CertificateCallback(byte[] aCertIdentifier, MMM.Readers.Modules.RF.CertType aCertType, out byte[] aCertBuffer)
        {
            bool lSuccess = false;
            OpenFileDialog fileSelector = new OpenFileDialog();
            aCertBuffer = null;
            fileSelector.Title = "Open external certificate: " + aCertType.ToString();
            fileSelector.InitialDirectory = "c:\\certs\\";
            fileSelector.Filter = "Certs and keys|*.cer;*.der;*.cvcert;*.pkcs8;*.bin|All files (*.*)|*.*";
            fileSelector.FilterIndex = 1;
            fileSelector.RestoreDirectory = true;
            if (fileSelector.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.Stream fs = null;
                    if ((fs = fileSelector.OpenFile()) != null)
                    {
                        aCertBuffer = new byte[fs.Length];
                        fs.Read(aCertBuffer, 0, aCertBuffer.Length);
                        fs.Close();
                        lSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    Lbl_ErrorPublic.Text = "Error: Could not read file from disk. Original error: " + ex.Message;
                }
            }
            return lSuccess;
        }

        void LogDataItem(MMM.Readers.FullPage.DataType aDataType, object aData)
        {
            if (aDataType == MMM.Readers.FullPage.DataType.CD_SWIPE_MSR_DATA)
            {
                MMM.Readers.Modules.Swipe.MsrData msrData = (MMM.Readers.Modules.Swipe.MsrData)aData;
                LogDataItem("MSR_TRACK_1", msrData.Track1);
                LogDataItem("MSR_TRACK_2", msrData.Track2);
                LogDataItem("MSR_TRACK_3", msrData.Track3);
            }
            else if (aDataType == MMM.Readers.FullPage.DataType.CD_AAMVA_DATA)
            {
                MMM.Readers.AAMVAData aamvaData = (MMM.Readers.AAMVAData)aData;
                LogDataItem("AAMVA_FULL_NAME", aamvaData.Parsed.FullName);
                LogDataItem("AAMVA_LICENCE_NUMBER", aamvaData.Parsed.LicenceNumber);
            }
            else if (aDataType > MMM.Readers.FullPage.DataType.CD_PLUGIN)
            {
                MMM.Readers.FullPage.PluginData pluginData = (MMM.Readers.FullPage.PluginData)aData;
                ListViewItem thisItem = dataFileList.Items.Add(pluginData.puDataFormat.ToString());
                string lInfo = pluginData.puFeatureName + " " + pluginData.puFieldName + ": ";
                if (pluginData.puData is string)
                    LogDataItem(aDataType.ToString(), lInfo + (string)pluginData.puData);
                else if (pluginData.puData is byte[])
                    LogDataItem(aDataType.ToString(), lInfo + ((byte[])pluginData.puData).Length + " bytes");
                else
                    LogDataItem(aDataType.ToString(), lInfo + aData);
            }
            else
            {
                LogDataItem(aDataType.ToString(), aData);
            }
        }

        void LogDataItem(string aDataType, object aData)
        {
            AddLog("- LogDataItem : " + aData.ToString() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            textBox1.Text += "- LogDataItem : " + aData.ToString() + "\r\n";
        }

        void LogWarning(MMM.Readers.WarningCode aWarningCode, string aWarningMessage)
        {
            AddLog("- LogWarning : " + aWarningMessage.ToString() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            textBox1.Text += "- LogWarning : " + aWarningMessage.ToString() + "\r\n";
        }

        void LogError(MMM.Readers.ErrorCode aErrorCode, string aErrorMessage)
        {
            AddLog("- LogError : " + aErrorMessage.ToString() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            textBox1.Text += "- LogError : " + aErrorMessage.ToString() + "\r\n";
        }

        void LogEvent(MMM.Readers.FullPage.EventCode aEventType)
        {
            AddLog("- LogEvent : " + aEventType.ToString() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            textBox1.Text += "- LogEvent : " + aEventType.ToString() + "\r\n";
        }

        void UpdateState(MMM.Readers.FullPage.ReaderState state)
        {
            AddLog("- UpdateState : " + state.ToString() + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
            textBox1.Text += "- UpdateState : " + state.ToString() + "\r\n";
        }

        private MMM.Readers.FullPage.ReaderState prPreviousState = MMM.Readers.FullPage.ReaderState.READER_DISABLED;

        private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    {
                        AddLog("- PowerModeChanged : " + "Resume" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        System.Threading.Thread.Sleep(5000);
                        MMM.Readers.FullPage.Reader.SetState(prPreviousState, true);
                        UpdateState(prPreviousState);
                        break;
                    }
                case Microsoft.Win32.PowerModes.Suspend:
                    {
                        AddLog("- PowerModeChanged : " + "Suspend" + " -> " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm") + "\r\n");
                        MMM.Readers.FullPage.ReaderState lCurrentState = MMM.Readers.FullPage.Reader.GetState();
                        prPreviousState = lCurrentState;
                        if ((lCurrentState != MMM.Readers.FullPage.ReaderState.READER_NOT_INITIALISED) && (lCurrentState != MMM.Readers.FullPage.ReaderState.READER_ERRORED) && (lCurrentState != MMM.Readers.FullPage.ReaderState.READER_TERMINATED) && (lCurrentState != MMM.Readers.FullPage.ReaderState.READER_SUSPENDED))
                        {
                            MMM.Readers.FullPage.Reader.SetState(MMM.Readers.FullPage.ReaderState.READER_SUSPENDED, true);
                            UpdateState(MMM.Readers.FullPage.ReaderState.READER_SUSPENDED);
                            do
                            {
                                System.Threading.Thread.Sleep(10);
                                lCurrentState = MMM.Readers.FullPage.Reader.GetState();
                            }
                            while (lCurrentState == prPreviousState);
                        }
                    }
                    break;
            }
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        // Config Section :
        private void DoConfig()
        {
            try
            {
                foreach (var PNL in Controls.OfType<Panel>())
                {
                    if (PNL.Name.Substring(0, 3) == "PL_")
                    {
                        PNL.Visible = false;
                        PNL.Enabled = false;
                    }
                }
                if ((ServerAddress != "") || (CompanyID != "") || (DealerID != "") || (UserID != "") || (DeviceID != 0))
                {
                    textBox2.Text = "";
                    PL_Password.Visible = true;
                    PL_Password.Enabled = true;
                }
                else
                {
                    textBox5.Text = "";
                    textBox4.Text = "";
                    textBox3.Text = "";
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                    textBox3.Text = UserID.Trim();
                    textBox4.Text = DealerID.Trim();
                    if (DeviceID == 2)
                    {
                        radioButton1.Checked = false;
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        radioButton1.Checked = true;
                        radioButton2.Checked = false;
                    }
                    radioButton1_CheckedChanged(null, null);
                    PL_Config.Visible = true;
                    PL_Config.Enabled = true;
                }
            }
            catch (Exception) { }
        }

        private void Splash_PB_1_Click(object sender, EventArgs e)
        {
            try
            {
                Reconfig_Click++;
                if (Reconfig_Click >= 10) { ReConfig = true; }
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to reset the system settings? If so it is not possible to recover the settings", "Configuration Reset ...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "SA", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "CI", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "DI", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "UI", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "AU", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "AP", "");
                    Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "DVI", "0");
                    PL_Password.Visible = false;
                    PL_Password.Enabled = false;
                    textBox5.Text = "";
                    textBox3.Text = "";
                    textBox4.Text = "";
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                    PL_Config.Visible = true;
                    PL_Config.Enabled = true;
                }
            }
            catch (Exception) { }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radioButton1.Checked == true)
                {
                    textBox3.Enabled = true;
                    textBox3.Text = "";
                    textBox3.Text = UserID;
                }
                else
                {
                    textBox3.Enabled = false;
                    textBox3.Text = "";
                    string[] SCNSER = null;
                    MMM.Readers.ErrorCode ERC = new MMM.Readers.ErrorCode();
                    ERC = MMM.Readers.FullPage.Reader.GetConnectedScanners(ref SCNSER);
                    textBox3.Text = SCNSER[0].ToString().Trim();
                }
            }
            catch (Exception)
            { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                textBox5.Text = "";
                var filePath = string.Empty;
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "IDV Config files (*.IDV)|*.IDV";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK) { filePath = openFileDialog.FileName; }
                }
                textBox5.Text = filePath.ToString().Trim();
            }
            catch (Exception)
            { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = textBox3.Text.Trim();
                textBox4.Text = textBox4.Text.Trim();
                textBox5.Text = textBox5.Text.Trim();
                if (File.Exists(textBox5.Text) == true)
                {
                    if (textBox4.Text != "")
                    {
                        if (textBox3.Text != "")
                        {
                            string FileConfig = "";
                            FileConfig = File.ReadAllText(textBox5.Text);
                            FileConfig = FileConfig.Replace("\r\n", "");
                            FileConfig = FileConfig.Substring(1200, FileConfig.Length - 1200);
                            FileConfig = FileConfig.Substring(0, FileConfig.Length - 2300);
                            string Enc1 = FileConfig.Substring(0, 10);
                            FileConfig = FileConfig.Substring(10, FileConfig.Length - 10);
                            string Enc2 = FileConfig.Substring(FileConfig.Length - 15, 15);
                            string TextBody = FileConfig.Substring(0, FileConfig.Length - 15);
                            TextBody = Decrypt(TextBody, Enc2);
                            TextBody = Decrypt(TextBody, Enc1);
                            string[] Config_Sep = TextBody.Trim().Split('-');
                            if (Config_Sep.Length == 12)
                            {
                                if (Config_Sep[0] == "IDV")
                                {
                                    if (Config_Sep[6] == "EMAS")
                                    {
                                        if (Config_Sep[11] == "PACIFIC")
                                        {
                                            string DFS = ""; string DFSS = "";
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(Config_Sep[5].ToString().Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "SA", DFS);
                                            Wait(200);
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(Config_Sep[1].ToString().Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "CI", DFS);
                                            Wait(200);
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(textBox4.Text.Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "DI", DFS);
                                            Wait(200);
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(textBox3.Text.Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "UI", DFS);
                                            Wait(200);
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(Config_Sep[9].ToString().Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "AU", DFS);
                                            Wait(200);
                                            DFS = ""; DFSS = ""; DFSS = Make_Security_Code(10); DFS = Encrypt(Config_Sep[10].ToString().Trim(), DFSS); DFS = DFS + DFSS;
                                            Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "AP", DFS);
                                            if (radioButton1.Checked == true)
                                            {
                                                Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "DVI", "1");
                                            }
                                            else
                                            {
                                                Interaction.SaveSetting("IDV_Pacific", "Document_Reader", "DVI", "2");
                                            }
                                            MessageBox.Show("Dear User ...\r\nApplication is configured were successfully. The configuration file for " + Config_Sep[2].ToString().Trim() + " was successfully installed.\r\nYou need to restart the application to apply the changes, The software will now close automatically.", "Congratulations ...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            MMM.Readers.FullPage.Reader.Shutdown();
                                            Close();
                                        }
                                        else
                                        {
                                            MessageBox.Show("Dear User ...\r\nIDV config file is not valid", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Dear User ...\r\nIDV config file is not valid", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Dear User ...\r\nIDV config file is not valid", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Dear User ...\r\nIDV config file is not valid", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Dear User ...\r\nPlease before saving the config, Enter user id for your scanner", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Dear User ...\r\nPlease before saving the config, Enter dealer id for your branch", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Dear User ...\r\nPlease before saving the config, Select IDV config file from your system. If you don't have the file contact support.", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Dear User ...\r\nError when configuring your application settings", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Text.Trim() != "")
                {
                    string Res = "ERR";
                    try
                    {
                        Res = Scanner_HttpPost("WA_01_Login", "CID=" + CompanyID + "&ATU=" + Auth_Username + "&ATP=" + Auth_Password + "&ConPass=" + textBox2.Text.Trim());
                    }
                    catch (Exception) { }
                    if (Res.Trim().ToUpper() != "ERR")
                    {
                        Res = Res.Replace("\"", "").Trim();
                        string RSV = Res.Substring(0, Res.Length - 10);
                        string ENC = Res.Substring(Res.Length - 10, 10);
                        string PSC = Decrypt(RSV, ENC).Trim().ToUpper();
                        if (PSC == "EMASOK")
                        {
                            foreach (var PNL in Controls.OfType<Panel>())
                            {
                                if (PNL.Name.Substring(0, 3) == "PL_")
                                {
                                    PNL.Visible = false;
                                    PNL.Enabled = false;
                                }
                            }
                            textBox5.Text = "";
                            textBox4.Text = "";
                            textBox3.Text = "";
                            radioButton1.Checked = true;
                            radioButton2.Checked = false;
                            textBox3.Text = UserID.Trim();
                            textBox4.Text = DealerID.Trim();
                            if (DeviceID == 2)
                            {
                                radioButton1.Checked = false;
                                radioButton2.Checked = true;
                            }
                            else
                            {
                                radioButton1.Checked = true;
                                radioButton2.Checked = false;
                            }
                            radioButton1_CheckedChanged(null, null);
                            PL_Config.Visible = true;
                            PL_Config.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Dear User ...\r\nYour request for admin panel access was denied by the administrator.Please contact yout administrator", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Dear User ...\r\nYour request for admin panel access was denied by the administrator.Please contact yout administrator", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Dear User ...\r\nPlease enter administrator login password", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Dear User ...\r\nAn error occurred when login administrator", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int OKC = 1;
                if (ConfigurationSettings.AppSettings["PNL_Confirm_Force"].ToString().Trim() == "1")
                {
                    if (TXT_1.Text.Trim() == "") { if (LBL_1.Text.IndexOf("*") < 1) { LBL_1.Text = " * " + LBL_1.Text; } LBL_1.ForeColor = Color.Maroon; OKC = 0; }
                    if (TXT_2.Text.Trim() == "") { if (LBL_2.Text.IndexOf("*") < 1) { LBL_2.Text = " * " + LBL_2.Text; } LBL_2.ForeColor = Color.Maroon; OKC = 0; }
                    if (TXT_3.Text.Trim() == "") { if (LBL_3.Text.IndexOf("*") < 1) { LBL_3.Text = " * " + LBL_3.Text; } LBL_3.ForeColor = Color.Maroon; OKC = 0; }
                    if (TXT_4.Text.Trim() == "") { if (LBL_4.Text.IndexOf("*") < 1) { LBL_4.Text = " * " + LBL_4.Text; } LBL_4.ForeColor = Color.Maroon; OKC = 0; }
                    if (TXT_5.Text.Trim() == "") { if (LBL_5.Text.IndexOf("*") < 1) { LBL_5.Text = " * " + LBL_5.Text; } LBL_5.ForeColor = Color.Maroon; OKC = 0; }
                    if (TXT_6.Text.Trim() == "") { if (LBL_6.Text.IndexOf("*") < 1) { LBL_6.Text = " * " + LBL_6.Text; } LBL_6.ForeColor = Color.Maroon; OKC = 0; }
                }
                if (OKC == 1) { Info_Confirm = true; } else { label47.Visible = true; }
            }
            catch (Exception) { }
        }

        private void Btn_Finished_Click(object sender, EventArgs e)
        {
            try
            {
                TXT_Email.Text = TXT_Email.Text.Trim();
                TXT_Phone.Text = TXT_Phone.Text.Trim();
                if ((TXT_Email.Text != "") && (TXT_Phone.Text != ""))
                {
                    button7.Enabled = false;
                    if (PageProcc_Show == true)
                    {
                        KeyboardActived = true;
                        label28.Visible = false;
                        label30.Visible = false;
                        PL_01_Splash.Visible = false;
                        PL_02_Splash.Visible = false;
                        PL_03_Splash.Visible = false;
                        PL_04_Splash.Visible = false;
                        PL_Error.Visible = false;
                        PL_Show_Info.Visible = false;
                        PL_Succ.Visible = false;
                        PL_Submit.Visible = false;
                        PL_03_Splash.Enabled = true;
                        PL_03_Splash.Visible = true;
                    }
                    Finish_Confirm = true;
                    KeyboardActived = false;
                }
                else
                {
                    label37.ForeColor = Color.DarkRed;
                    label38.ForeColor = Color.DarkRed;
                }
            }
            catch (Exception) { }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if ((textBox6.Text.Trim() != "") || (textBox7.Text.Trim() != ""))
            {
                PageDVLAWait = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PageDVLAWait = false;
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        private void TextB_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Test Click");
        }
        private void TextB_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Test Enter");
        }
        private void ShowKeybord(TextBox TX, string Header)
        {
            if (VirtualKeyboad != 1) { return; }
            if (KeyboardActived == true) { return; }
            if (UserCancel == true) { return; }
            if (DontShowKeyboard == true) { return; }
            KeyboardActived = true;
            KeyboardF Frm = new KeyboardF();
            LastResKeyB = TX.Text;
            Frm.MainText = TX.Text;
            Frm.Header = Header;
            Frm.MainFB = this;
            WaitForKB = true;
            Frm.Show();
            Frm.ApplyData();
            while (WaitForKB == true) { Application.DoEvents(); }
            TX.Text = LastResKeyB.Trim();
            KeyboardActived = false;
            this.Focus();
            PanB1.Focus();
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Administrator Password");
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Administrator Password");
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Config File Address");
        }

        private void textBox5_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Config File Address");
        }

        private void textBox4_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Dealer ID");
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Dealer ID");
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "User ID");
        }

        private void textBox3_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "User ID");
        }

        private void textBox7_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "DVLA Code");
        }

        private void textBox7_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "DVLA Code");
        }

        private void textBox6_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "National Insurance");
        }

        private void textBox6_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "National Insurance");
        }

        private void TXT_Email_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Please Enter Your Email Address");
        }

        private void TXT_Email_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Please Enter Your Email Address");
        }

        private void TXT_Phone_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Please Enter Your Phone Number");
        }

        private void TXT_Phone_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, "Please Enter Your Phone Number");
        }

        private void TXT_1_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_1.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_1_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_1.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_2_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_2.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_2_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_2.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_3_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_3.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_3_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_3.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_4_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_4.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_4_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_4.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_5_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_5.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_5_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_5.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_6_Click(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_6.Text.Replace(":", "").Replace("  ", "").Trim());
        }

        private void TXT_6_Enter(object sender, EventArgs e)
        {
            ShowKeybord((TextBox)sender, LBL_6.Text.Replace(":", "").Replace("  ", "").Trim());
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        private void Btn_BTD_Click(object sender, EventArgs e)
        {
            Pnl_Btn.Visible = false;
            SBtnSelect = 2;
            Application.DoEvents();
        }

        private void Btn_CC_Click(object sender, EventArgs e)
        {
            Pnl_Btn.Visible = false;
            WBSiteAdd = 0;
            SBtnSelect = 1;
            Application.DoEvents();
        }

        private void LoadWebPage(int WIDSA)
        {
            WB.Top = 10;
            WB.Left = 10;
            WB.Width = 1838;
            WB.Height = 852;
            WBL.Top = (PnlWeb.Height / 2) - (WBL.Height / 2);
            WBL.Left = (PnlWeb.Width / 2) - (WBL.Width / 2);
            WB.Visible = false;
            WBL.Visible = true;
            Application.DoEvents();
            if (WIDSA == 1)
            {
                string ADDCC = ConfigurationSettings.AppSettings["KIA_CFCal"].ToString().Trim();
                WB.Navigate(ADDCC);
            }
            else
            {
                string ADDCC = ConfigurationSettings.AppSettings["KIA_CCustom"].ToString().Trim();
                WB.Navigate(ADDCC);
            }
        }

        private void WB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                dynamic htmldoc = WB.Document.DomDocument as dynamic;
                dynamic node = htmldoc.getElementById("header") as dynamic;
                node.parentNode.removeChild(node);
                dynamic node2 = htmldoc.getElementById("footer") as dynamic;
                node2.parentNode.removeChild(node2);
                foreach (HtmlElement el in WB.Document.GetElementById("content").GetElementsByTagName("div"))
                {
                    try
                    {
                        if (el.OuterHtml.ToLower().IndexOf("content_title") > 0)
                        {
                            el.OuterHtml = "";
                            break;
                        }
                    }
                    catch (Exception)
                    { }
                }
                WB.Document.GetElementById("container").SetAttribute("id", "emas");
                if (WBSiteAdd == 1)
                {

                }
            }
            catch (Exception)
            { }
            WB.Top = 10;
            WB.Left = 10;
            WB.Width = 1838;
            WB.Height = 852;
            WBL.Top = (PnlWeb.Height / 2) - (WBL.Height / 2);
            WBL.Left = (PnlWeb.Width / 2) - (WBL.Width / 2);
            WBL.Visible = false;
            WB.Visible = true;
        }

        private void WB_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            WB.Top = 10;
            WB.Left = 10;
            WB.Width = 1838;
            WB.Height = 852;
            WBL.Top = (PnlWeb.Height / 2) - (WBL.Height / 2);
            WBL.Left = (PnlWeb.Width / 2) - (WBL.Width / 2);
            WB.Visible = false;
            WBL.Visible = true;
            Application.DoEvents();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PnlWeb.Visible = false;
            WBSiteAdd = 0;
            SBtnSelect = 0;
            Application.DoEvents();
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            Pnl_Btn.Visible = false;
            WBSiteAdd = 1;
            SBtnSelect = 1;
            Application.DoEvents();
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                DontShowKeyboard = true;
                SystemPause = true;
                Activity_Now = 1;
                SBtnSelect = 0;
                UserCancel = true;
                KeyboardActived = true;
                Finish_Confirm = true;
            }
            catch (Exception)
            { }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            DontShowKeyboard = true;
            SystemPause = true;
            Activity_Now = 1;
            SBtnSelect = 0;
            UserCancel = true;
            KeyboardActived = true;
            Finish_Confirm = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DontShowKeyboard = true;
            SystemPause = true;
            Activity_Now = 1;
            SBtnSelect = 0;
            UserCancel = true;
            KeyboardActived = true;
            Finish_Confirm = true;
        }
        //-------------------------------------------------------------------------------------\\
        //-------------------------------------------------------------------------------------//
    }
}
