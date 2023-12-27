using CloneBillsApp.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloneBillsApp.Class.VacsMapApp;
using CloneBillsApp.Class.Constants;
using CloneBillsApp.Class.AppData;

namespace CloneBillsApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            clsLogger.Info("Aplication start...");
            if (args.Length == 0)
            {
                //二重起動防止Mutex
                string strMutexName = clsCommon.APP_MUTEX_NAME;
                Mutex objMutex = new Mutex(false, strMutexName);

                bool hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = objMutex.WaitOne(0, false);
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }
                    if (hasHandle == false)
                    {
                        MessageBox.Show("多重起動できません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //未処理例外イベントハンドラ
                    Application.ThreadException += new ThreadExceptionEventHandler(OnThreadException);
                    Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMain());
                }
                catch (Exception ex)
                {
                    //ログ出力
                    clsLogger.Fatal(ex);
                    //予期せぬ例外
                    clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "予期せぬエラーが発生しました。\nアプリケーションを終了します。\n" + ex.Message);
                    Application.Exit();
                    return;
                }
                finally
                {
                    if (hasHandle)
                    {
                        objMutex.ReleaseMutex();
                    }
                    objMutex.Close();
                    clsLogger.Info("Aplication close.");
                }
            }
            else if (args.Length == 1 && args[0] == OptionKeys.EXE_ARGUMENT)
            {
                // EXECUTE_UPLOAD
                if (clsOptionSetting.Load())
                {
                    clsUpload upload = new clsUpload();
                    upload.sourceInfo = new clsSourceInfo();
                    upload.localInfo = new clsLocalDestinationInfo();
                    upload.googleInfo = new clsGoogleInfo();
                    if (upload.googleInfo.IsActive)
                    {
                        var task = InitGoogleApiSevice(upload.googleInfo.ClientId, upload.googleInfo.ClientSecret);
                        if (task.Result != null)
                        {
                            var _api = task.Result;
                            upload._api = _api;
                            upload.UploadFile(null);
                        }
                    }
                    else
                    {
                        upload.UploadFile(null);
                    }
                }

                clsLogger.Info("Aplication close.");
            }
        }

        /// <summary>
        /// 未処理例外イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //ログ出力
            clsLogger.Fatal(e.Exception);
            //予期せぬ例外
            clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "予期せぬエラーが発生しました。\nアプリケーションを終了します。\n" + e.Exception.Message);
            Application.Exit();
            return;
        }

        /// <summary>
        /// 未処理例外イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                //ログ出力
                clsLogger.Fatal(ex);
                //予期せぬ例外
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, "予期せぬエラーが発生しました。\nアプリケーションを終了します。\n" + ex.Message);
                Application.Exit();
                return;
            }
        }

        public static async Task<clsGoogleApiSevice> InitGoogleApiSevice(string clientId, string clientSecret)
        {
            var _api = new clsGoogleApiSevice(clientId, clientSecret);
            // Ckeck authen
            await _api.LoginAsync();
            if (!_api.CheckLogin())
            {
                clsLogger.Err(clsCommon.GetString("err_google_login_fail"));
                return null;
            }
            return _api;
        }
    }
}
