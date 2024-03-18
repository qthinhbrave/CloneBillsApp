using CloneBillsApp.Class;
using CloneBillsApp.Class.AppData;
using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloneBillsApp
{
    public partial class frmConfig : Form
    {
        public bool IsUpdateInfo = false;
        private clsSourceInfo clsSourceInfo;
        private clsLocalDestinationInfo clsLocalDestinationInfo;
        private clsGoogleInfo clsGoogleInfo;

        public frmConfig()
        {
            InitializeComponent();
        }

        public frmConfig(clsSourceInfo clsSourceInfo, clsLocalDestinationInfo clsLocalDestinationInfo, clsGoogleInfo clsGoogleInfo)
        {
            InitializeComponent();

            this.clsSourceInfo = clsSourceInfo;
            this.clsLocalDestinationInfo = clsLocalDestinationInfo;
            this.clsGoogleInfo = clsGoogleInfo;
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

            txtSourcePath.Text = clsSourceInfo.Path;

            radioButton1.Checked = true;//clsLocalDestinationInfo.IsActive;
            txtLocalPath.Text = clsLocalDestinationInfo.Path;

            /*radioButton2.Checked = clsGoogleInfo.IsActive;
            txtClientID.Text = clsGoogleInfo.ClientId;
            txtClientSecret.Text = clsGoogleInfo.ClientSecret;*/
        }

        /// <summary>
        /// Keyup Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyEvent(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    btnEsc_Click(sender, e);
                    break;
                case Keys.F12:
                    btnSave_Click(sender, e);
                    break;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                txtLocalPath.ReadOnly = false;
                txtLocalPath.Text = clsLocalDestinationInfo.Path;
                browseDestination.Enabled = true;
            }
            else
            {
                txtLocalPath.ReadOnly = true;
                //txtLocalPath.Text = "";
                browseDestination.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                txtClientID.ReadOnly = false;
                txtClientID.Text = clsGoogleInfo.ClientId;

                txtClientSecret.ReadOnly = false;
                txtClientSecret.Text = clsGoogleInfo.ClientSecret;
            }
            else
            {
                txtClientID.ReadOnly = true;
                //txtClientID.Text = "";
                txtClientSecret.ReadOnly = true;
                //txtClientSecret.Text = "";
            }
        }

        /// <summary>
        /// Choose source location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseSource_Click(object sender, EventArgs e)
        {
            txtSourcePath.Text = SelectFolder();
        }

        /// <summary>
        /// Choose local path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseLocalPath_Click(object sender, EventArgs e)
        {
            txtLocalPath.Text= SelectFolder();
        }

        /// <summary>
        /// Select and read JsonKey file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseJsonKey_Click(object sender, EventArgs e)
        {
            SelectFile();
        }

        /// <summary>
        /// Escape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEsc_Click(object sender, EventArgs e)
        {
            DialogResult modalForm = clsCommon.ShowMessage(clsCommon.enmMessageType.CONFIRM, clsCommon.GetString("confirm_CloseConfigForm"));
            if (modalForm == DialogResult.OK)
            {
                this.Close(); // close the connection setup form
            }
        }

        /// <summary>
        /// Save configs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSave_Click(object sender, EventArgs e)
        {
            bool ok = true;
            // 1. Check save Source path
            if (clsSourceInfo.Path != txtSourcePath.Text)
            {
                if (clsSourceInfo.Save(txtSourcePath.Text))
                {
                    IsUpdateInfo = true;
                }
                else
                { ok = false; }
            }

            // 2. Check save LocalDestinationInfo
            if (clsLocalDestinationInfo.IsActive != radioButton1.Checked ||
                clsLocalDestinationInfo.Path != txtLocalPath.Text)
            {
                if (clsLocalDestinationInfo.Save(radioButton1.Checked, txtLocalPath.Text))
                {
                    IsUpdateInfo = true;
                }
                else
                { ok = false; }
            }

            // 3. Check save GoogleInfo
            if (clsGoogleInfo.IsActive != radioButton2.Checked ||
                clsGoogleInfo.ClientId != txtClientID.Text ||
                clsGoogleInfo.ClientSecret != rtbJsonKey.Text)
            {
                if (!radioButton2.Checked)
                {
                    // only save to DB
                    if (clsGoogleInfo.Save(radioButton2.Checked, txtClientID.Text, txtClientSecret.Text))
                    {
                        IsUpdateInfo = true;
                    }
                    else
                    { ok = false; }
                }
                else
                {
                    clsGoogleApiSevice _api = new clsGoogleApiSevice(txtClientID.Text, txtClientSecret.Text);
                    // Ckeck authen
                    await _api.LoginAsync();
                    if (_api.CheckLogin())
                    {
                        if (clsGoogleInfo.Save(radioButton2.Checked, txtClientID.Text, txtClientSecret.Text))
                        {
                            IsUpdateInfo = true;
                        }
                        else
                        { ok = false; }
                    }
                }
            }
            if (ok)
            {
                clsCommon.ShowMessage(clsCommon.enmMessageType.INFO, clsCommon.GetString("info_update_option_setting"));
            }
        }

        private string SelectFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }
            return String.Empty;
        }

        private string SelectFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sr = new StreamReader(openFileDialog1.FileName);
                    SetText(sr.ReadToEnd());
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
            return String.Empty;
        }

        private void SetText(string text)
        {
            rtbJsonKey.Text = text;
        }

    }
}
