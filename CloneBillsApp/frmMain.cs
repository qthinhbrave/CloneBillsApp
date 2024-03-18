using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using CloneBillsApp.Class;
using CloneBillsApp.Class.AppData;
using CloneBillsApp.Class.VacsMapApp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CloneBillsApp
{
    public partial class frmMain : Form
    {
        private clsSourceInfo sourceInfo;
        private clsLocalDestinationInfo localInfo;
        private clsGoogleInfo googleInfo;
        private clsGoogleApiSevice _api;

        public frmMain()
        {
            InitializeComponent();

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

            myBackgroundWorker.DoWork += myBackgroundWorker_DoWork;
            myBackgroundWorker.ProgressChanged += myBackgroundWorker_ProgressChanged;
            myBackgroundWorker.RunWorkerCompleted += myBackgroundWorker_RunWorkerCompleted;

            myBackgroundWorker.WorkerReportsProgress = true;
            myBackgroundWorker.WorkerSupportsCancellation = true;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            notifyIcon.BalloonTipTitle = "クラウドアップローダ";
            notifyIcon.BalloonTipText = "クラウドアップローダ";
            notifyIcon.Text = "クラウドアップローダ";

            //オプション設定ロード
            if (!clsOptionSetting.Load())
            {
                //ロード失敗
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "オプション設定のロードに失敗しました。\nアプリケーションを終了します。");
                //アプリケーション終了
                Application.Exit();
                return;
            }

            //ログ出力先更新
            /*
            try
            {
                clsCommon.ChangeLogPath(true);
                clsLogger.Info("アプリケーション開始");
            }
            catch (Exception ex)
            {
                //更新処理失敗
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "ログ出力先の更新に失敗しました。\nアプリケーションを終了します。\n" + ex.Message);
                //アプリケーション終了
                Application.Exit();
                return;
            }
            */

            //文字列リソースロード
            try
            {
                clsCommon.LoadStrings();
            }
            catch (Exception ex)
            {
                //ログ出力
                clsLogger.Fatal(ex);
                //ロード失敗
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "文字列リソースのロードに失敗しました。\nアプリケーションを終了します。");
                //アプリケーション終了
                Application.Exit();
                return;
            }

            // Init DataSource for controls
            cbbHours.DataSource = clsCommon.Hours;
            cbbMinutes.DataSource = clsCommon.Minutes;
            LoadTimeUpload();
            sourceInfo = new clsSourceInfo();
            localInfo = new clsLocalDestinationInfo();
            googleInfo = new clsGoogleInfo();
            LoadUploadPath();

            // Check source-dest path is empty
            /*if (String.IsNullOrEmpty(txtSourcePath.Text) || String.IsNullOrEmpty(txtDestinationInfo.Text))
            {
                btnConfig_Click(sender, e);
            }*/
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            clsLogger.Info("アプリケーション終了");
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
                case Keys.F1:
                    btnF1_Click(sender, e);
                    break;
                case Keys.F2:
                    btnF2_Click(sender, e);
                    break;
                case Keys.F3:
                    btnF3_Click(sender, e);
                    break;
                case Keys.F4:
                    btnF4_Click(sender, e);
                    break;
                case Keys.F5:
                    btnConfig_Click(sender, e);
                    break;
            }
        }

        /// <summary>
        /// Add TimeUpload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnF1_Click(object sender, EventArgs e)
        {
            string hour = cbbHours.Text;
            string minute = cbbMinutes.Text;

            if (hour == "")
            {
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_value_is_not_selected", "時"));
                return;
            }

            if (minute == "")
            {
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_value_is_not_selected", "分"));
                return;
            }

            if (clsTimeUpload.AddTimeUpload(hour, minute))
            {
                // reload TimeUpload list
                LoadTimeUpload();
                cbbHours.SelectedIndex = -1;
                cbbMinutes.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Delete TimeUpload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnF2_Click(object sender, EventArgs e)
        {
            if (lbxTimeUpload.SelectedItems != null && lbxTimeUpload.SelectedItems.Count == 1)
            {
                clsTimeUpload objTimeUpload = (clsTimeUpload)lbxTimeUpload.SelectedItems[0];
                DialogResult objResult = clsCommon.ShowMessage(clsCommon.enmMessageType.CONFIRM, clsCommon.GetString("confirm_delTimeUpload", objTimeUpload.TIME_VALUE));
                if (objResult == DialogResult.OK)
                {
                    if (objTimeUpload.DeleteTimeUpload())
                    {
                        // reload TimeUpload list
                        LoadTimeUpload();
                    }
                }
            }
        }
        /// <summary>
        /// Hide application, 
        ///   it becomes an icon in the system task tray.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnF3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Manual execute upload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnF4_Click(object sender, EventArgs e)
        {
            DialogResult objResult = clsCommon.ShowMessage(clsCommon.enmMessageType.CONFIRM, clsCommon.GetString("confirm_RunUploadManual"));
            if (objResult == DialogResult.OK)
            {
                if (googleInfo.IsActive)
                {
                    _api = new clsGoogleApiSevice(googleInfo.ClientId, googleInfo.ClientSecret);
                    // Ckeck authen
                    await _api.LoginAsync();
                    if (!_api.CheckLogin())
                    {
                        clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_google_login_fail"));
                    }
                }

                btnF4.Enabled = false;
                myBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Show OptionConfig setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfig_Click(object sender, EventArgs e)
        {
            // Show config
            frmConfig frm = new frmConfig(sourceInfo, localInfo, googleInfo);
            frm.ShowDialog();

            // Reload info
            if (frm.IsUpdateInfo)
            {
                clsOptionSetting.Load();
                LoadUploadPath();
                // reload TimeUpload list
                LoadTimeUpload();
            }
        }

        private void LoadTimeUpload()
        {
            try
            {
                lbxTimeUpload.DataSource = clsTimeUpload.GetTimeUpload();
                lbxTimeUpload.DisplayMember = "TIME_VALUE";
                lbxTimeUpload.ValueMember = "TIME_ID";
                lbxTimeUpload.SelectedIndex = -1;
            }
            catch(Exception ex)
            {
                clsLogger.Err(ex.Message);
            }
        }

        private void LoadUploadPath()
        {
            txtSourcePath.Text = sourceInfo.Path;
            if (localInfo.IsActive)
            {
                txtDestinationInfo.Text = localInfo.Path;
            }
            else if (googleInfo.IsActive)
            {
                txtDestinationInfo.Text = googleInfo.ToString();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized) 
            { 
                this.Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
            else
            {
                notifyIcon.Visible = false;
            }
        }

        private void mnuItemClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region BackgroundWorker

        void myBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Change the status of the buttons on the UI accordingly
            btnF4.Enabled = true;
        }

        void myBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string text = e.UserState.ToString();
            listBox2.Items.Add(text);
        }

        void myBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // EXECUTE_UPLOAD
                clsUpload upload = new clsUpload();
                upload.sourceInfo = sourceInfo;
                upload.localInfo = localInfo;
                upload.googleInfo = googleInfo;
                if (googleInfo.IsActive)
                {
                    upload._api = this._api;
                }
                upload.UploadFile(myBackgroundWorker);
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }
        }

        #endregion
    }
}
