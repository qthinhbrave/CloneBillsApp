using CloneBillsApp.Class.AppData;
using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CloneBillsApp.Class
{
    public class clsCommon
    {
        //アプリケーション名
        public const string APP_NAME = "CloneBillsApp";

        //二重起動防止Mutex名
        public const string APP_MUTEX_NAME = "CloneBillsApp";

        public enum enmUploadType
        {
            LOCAL,
            GOOGLE
        }

        //メッセージボックス種別
        public enum enmMessageType
        {
            ERROR,
            WARN,
            INFO,
            CONFIRM
        }

        //文字列リソースXML
        private const string STRINGS_FILE_PATH = @".\string.xml";
        private static XElement m_objStringsXElem;

        /// <summary>
        /// 文字列リソースロード
        /// </summary>
        public static void LoadStrings()
        {
            m_objStringsXElem = XElement.Load(STRINGS_FILE_PATH);
        }


        public static ObservableCollection<string> Hours
        {
            get
            {
                var ret = new ObservableCollection<string>();
                ret.Add("");
                for (int i = 0; i <= 23; i++)
                {
                    ret.Add(string.Format("{0:D2}", i));
                }
                return ret;
            }
        }

        public static ObservableCollection<string> Minutes
        {
            get
            {
                var ret = new ObservableCollection<string>();
                ret.Add("");
                for (int i = 0; i <= 59; i++)
                {
                    ret.Add(string.Format("{0:D2}", i));
                }
                return ret;
            }
        }

        /// <summary>
        /// メッセージ表示
        /// </summary>
        /// <param name="iMessageType"></param>
        /// <param name="strMessage"></param>
        public static DialogResult ShowMessage(enmMessageType iMessageType, string strMessage)
        {
            DialogResult objResult = DialogResult.None;
            switch (iMessageType)
            {
                case enmMessageType.ERROR:
                    objResult = MessageBox.Show(strMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case enmMessageType.WARN:
                    objResult = MessageBox.Show(strMessage, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case enmMessageType.INFO:
                    objResult = MessageBox.Show(strMessage, "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case enmMessageType.CONFIRM:
                    objResult = MessageBox.Show(strMessage, "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    break;
            }
            return objResult;
        }

        #region Log
        /// <summary>
        /// ログ出力先更新
        /// </summary>
        public static bool ChangeLogPath(bool isInit)
        {
            try
            {
                string strPath;
                strPath = clsOptionSetting.GetString_Value1("LOG_FILE", "C_OUTPUT_PATH");
                if (!strPath.EndsWith("\\"))
                {
                    strPath += "\\";
                }
                clsLogger.ChangeCommonLogPath(strPath, isInit);
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
                return false;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// 文字列リソース取得
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static string GetString(string strKey, params object[] aryEmbed)
        {
            try
            {
                List<string> lstValues = (from objStrings in m_objStringsXElem.Elements("string")
                                          where (string)objStrings.Attribute("name") == strKey
                                          select objStrings.Value).ToList();
                if (lstValues.Count == 1)
                {
                    if (aryEmbed.Length == 0)
                    {
                        return lstValues[0];
                    }
                    return string.Format(lstValues[0], aryEmbed);
                }
            }
            catch
            {
            }
            return "";
        }

        public static void DirectorySearch(string dir, ref List<string> files)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    DirectorySearch(d, ref files);
                }
            }
            catch (System.Exception ex)
            {
                clsLogger.Err(ex.Message);
            }
        }
    }
}
