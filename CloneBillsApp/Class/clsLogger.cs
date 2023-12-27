using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class
{
    using log4net;
    using log4net.Appender;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    namespace VacsMapApp
    {
        /// <summary>
        /// ログ出力クラス
        /// </summary>
        public class clsLogger
        {
            private static ILog m_CommonLogger = LogManager.GetLogger("applog");

            /// <summary>
            /// ログ出力先変更(汎用ログ)
            /// </summary>
            /// <param name="strPath"></param>
            public static void ChangeCommonLogPath(string strPath, bool isInit)
            {
                Logger objLogger = (Logger)m_CommonLogger.Logger;
                var x = objLogger.GetHashCode();
                RollingFileAppender objAppender = objLogger.GetAppender("applog") as RollingFileAppender;
                string oldPath = objAppender.File;
                objAppender.File = strPath;
                objAppender.ActivateOptions();
                string newPath = objAppender.File;

                try
                {
                    //削除
                    if (isInit && NormalizePath(oldPath) != NormalizePath(newPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                catch (Exception ex)
                {
                    //削除に失敗してもスルー
                }
            }

            private static string NormalizePath(string path)
            {
                return Path.GetFullPath(new Uri(path).LocalPath)
                           .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           .ToUpperInvariant();
            }

            /// <summary>
            /// 過去ログファイル削除
            /// </summary>
            public static bool CleanupLogFile()
            {
                try
                {
                    ILoggerRepository objRepository = LogManager.GetAllRepositories().FirstOrDefault();
                    List<IAppender> lstAppender = objRepository.GetAppenders().Where(x => x.GetType() == typeof(RollingFileAppender)).ToList();
                    foreach (IAppender objAppender in lstAppender)
                    {
                        RollingFileAppender objRollingAppender = objAppender as RollingFileAppender;

                        DateTime objDate = DateTime.Now.AddDays(-objRollingAppender.MaxSizeRollBackups);

                        string strFilePath = Path.GetDirectoryName(objRollingAppender.File);
                        DirectoryInfo objDirInfo = new DirectoryInfo(strFilePath);
                        string strFormat = objRollingAppender.Name + "*.log";

                        FileInfo[] aryFileInfo = objDirInfo.GetFiles(strFormat);
                        foreach (FileInfo objFileInfo in aryFileInfo)
                        {
                            if (objFileInfo.CreationTime < objDate)
                            {
                                try
                                {
                                    objFileInfo.Delete();
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Fatal(ex);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// ログ出力(致命的エラー)
            /// </summary>
            /// <param name="ex"></param>
            public static void Fatal(Exception ex)
            {
                m_CommonLogger.Fatal(ex);
            }
            /// <summary>
            /// ログ出力(エラー)
            /// </summary>
            /// <param name="ex"></param>
            public static void Err(string message)
            {
                m_CommonLogger.Error(message);
            }

            /// <summary>
            /// ログ出力(警告)
            /// </summary>
            /// <param name="ex"></param>
            public static void Warn(string message)
            {
                m_CommonLogger.Warn(message);
            }

            /// <summary>
            /// ログ出力(情報)
            /// </summary>
            /// <param name="ex"></param>
            public static void Info(string message)
            {
                m_CommonLogger.Info(message);
            }
        }
    }
}
