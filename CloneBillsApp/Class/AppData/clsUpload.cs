﻿using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsUpload
    {
        public int Count { get; set; }
        public clsHistoryUpload obj {  get; set; }
        public clsSourceInfo sourceInfo { get; set; }
        public clsLocalDestinationInfo localInfo { get; set; }
        public clsGoogleInfo googleInfo { get; set; }
        public clsGoogleApiSevice _api { get; set; }

        public clsUpload()
        {
            obj = new clsHistoryUpload();
        }

        public void UploadFile(BackgroundWorker bgw)
        {
            string fromLocation = sourceInfo.Path;
            string toLocation = "";
            List<string> files = new List<string>();
            List<Task> TaskList = new List<Task>();

            // Marking the start time 
            DateTime start = DateTime.Now;
            //localInfo.IsActive = false;
            //googleInfo.IsActive = false;

            if (localInfo.IsActive)
            {
                toLocation = localInfo.Path;
            }
            else if (googleInfo.IsActive)
            {
                toLocation = googleInfo.ToString();
            }
            else
            {
                toLocation = "BOX";
            }

            try
            {
                if (obj.Insert(fromLocation, toLocation))
                {

                    clsLogger.Info("実行LOG_BEGIN。。。");
                    if (bgw != null) bgw.ReportProgress(1, clsCommon.GetString("info_execute_upload_begin", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff")));

                    // EXECUTE UPLOAD
                    clsCommon.DirectorySearch(fromLocation, ref files);
                    clsBoxApiService obj = new clsBoxApiService();
                    foreach (string file in files)
                    {
                        if (localInfo.IsActive)
                        {
                            string destFileName = file.Replace(fromLocation, toLocation);
                            string dir = Path.GetDirectoryName(destFileName);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            if (!File.Exists(destFileName))
                            {
                                TaskList.Add(LocalUpload(file, destFileName, bgw));
                            }
                        }
                        else if (googleInfo.IsActive)
                        {
                            string folderId = null;
                            string subPath = Path.GetDirectoryName(file).Replace(sourceInfo.Path, "");
                            string fileName = Path.GetFileName(file);
                            string toPath = String.Format("{0}{1}", clsCommon.APP_NAME, subPath);
                            //clsLogger.Info("toPath: " + toPath);
                            folderId = _api.CreateDirectoryIfNotExist(toPath);
                            if (folderId != null && folderId != "")
                            {
                                if (!_api.ExistsFile(folderId, fileName))
                                {
                                    TaskList.Add(GoogleUpload(_api, file, folderId, bgw));
                                }
                            }
                        }
                        //else
                        //{
                        //    TaskList.Add(obj.BOX_Upload(file));
                        //}
                    }

                    //clsLogger.Info("Task.WhenAll。。。");
                    //Task.WhenAll(TaskList.ToArray()).Wait();
                    clsLogger.Info("Task.WaitAll。。。");
                    Task.WaitAll(TaskList.ToArray());
                    clsLogger.Info("Task.Finished。");
                }
            }
            catch(Exception ex)
            {
                clsLogger.Err(ex.Message);
            }
            finally
            {
                // Marking the end time 
                DateTime end = DateTime.Now;

                // Total Duration  
                TimeSpan ts = (end - start);

                if (obj.HistoryID > 0)
                {
                    // Update end time
                    obj.NumberUploaded = Count;
                    obj.IntervalSec = (int)ts.TotalSeconds;
                    obj.UpdateEndUpload();
                }
                clsLogger.Info("実行LOG_END。");
                if (bgw != null) bgw.ReportProgress(100, clsCommon.GetString("info_execute_upload_end", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff")));
            }
        }

        private Task LocalUpload(string file, string destFileName, BackgroundWorker bgw)
        {
            return Task.Run(() =>
            {
                try
                {
                    clsLogger.Info("Moving " + file);
                    if (bgw != null) bgw.ReportProgress(1, file);

                    // Copy
                    File.Copy(file, destFileName, true);

                    Count++;
                    LogDetail(file, null);
                }
                catch(Exception ex)
                {
                    clsLogger.Err(ex.Message);
                    LogDetail(file, ex);
                }
                finally
                {
                    clsLogger.Info("Moved " + file);
                }
            });
        }

        private Task GoogleUpload(clsGoogleApiSevice _api, string file, string folderId, BackgroundWorker bgw)
        {
            return Task.Run(async() =>
            {
                try
                {
                    clsLogger.Info("Moving " + file);
                    if (bgw != null) bgw.ReportProgress(1, file);

                    // Copy
                    var fileId = await _api.UploadFileAsync(file, CancellationToken.None, folderId);
                    
                    Count++;
                    LogDetail(file, null);
                }
                catch (Exception ex)
                {
                    clsLogger.Err(ex.Message);
                    LogDetail(file, ex);
                }
                finally
                {
                    clsLogger.Info("Moved " + file);
                }
            });
        }

        private void LogDetail(string file, Exception ex)
        {
            clsDetailUpload detail = new clsDetailUpload();
            detail.HistoryID = obj.HistoryID;
            FileInfo oFileInfo = new FileInfo(file);
            if (oFileInfo.Exists)
            {
                detail.FileName = oFileInfo.Name;
                detail.Path = oFileInfo.DirectoryName;
                detail.Size = oFileInfo.Length;
            }

            if (ex == null)
            {
                detail.Result = true;
            }
            else
            {
                detail.Result = false;
                detail.Remark = ex.Message;
            }
            detail.Insert();
        }
    }
}
