using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.MonthCalendar;

namespace CloneBillsApp.Class.AppData
{
    public class clsTimeUpload
    {
        public int TIME_ID { get; set; }

        public string TIME_VALUE { get; set; }

        public static List<clsTimeUpload> GetTimeUpload()
        {
            List<clsTimeUpload> lstTimeUploads = new List<clsTimeUpload>();
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                objDbHelper.Open();

                //Hashtable objParams = new Hashtable();
                clsDatabaseCommand objCommand;

                //
                objCommand = objDbHelper.CreateSelectCommand("SQL.SelectTimeUploads", "MST_TIME_UPLOAD", null);
                DataTable objTimeUploadTable = objDbHelper.ExecuteQuery(objCommand);

                //
                foreach (DataRow objDataRow in objTimeUploadTable.Rows)
                {
                    clsTimeUpload objArea = new clsTimeUpload()
                    {
                        TIME_ID = int.Parse(objDataRow.ItemArray[0].ToString()),
                        TIME_VALUE = objDataRow.ItemArray[1].ToString(),
                    };
                    lstTimeUploads.Add(objArea);
                }
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_failed_SelectTimeUploads", "時間設定"));
            }
            finally
            {
                objDbHelper.Close();
            }
            return lstTimeUploads;
        }

        public static bool AddTimeUpload(string hour, string minute)
        {
            bool result = true;
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                objDbHelper.Open();

                Hashtable objParams = new Hashtable();
                objParams["TIME_VALUE"] = hour + ":" + minute;
                clsDatabaseCommand objCommand;

                //
                objCommand = objDbHelper.CreateInsertCommand("SQL.InsertTimeUpload", objParams);
                objDbHelper.Insert(objCommand);

                // 
                clsTaskService task = new clsTaskService(objParams["TIME_VALUE"].ToString());
                clsTaskScheduler.CreateTask(task);
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_failed_InsertTimeUpload", "時間設定"));
                result = false;
            }
            finally
            {
                objDbHelper.Close();
            }
            return result;
        }

        public bool DeleteTimeUpload()
        {
            bool result = true;
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                //DBオープン
                objDbHelper.Open();

                //トランザクション開始
                objDbHelper.Begin();
                Hashtable objParams = new Hashtable();
                objParams["TIME_ID"] = this.TIME_ID;
                clsDatabaseCommand objCommand;

                //
                objCommand = objDbHelper.CreateDeleteCommand("SQL.DeleteTimeUpload", objParams);
                objDbHelper.Delete(objCommand);

                //
                clsTaskService task = new clsTaskService(TIME_VALUE);
                clsTaskScheduler.DeleteTask(task.TaskName);

                //コミット
                objDbHelper.Commit();
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
                clsCommon.ShowMessage(clsCommon.enmMessageType.ERROR, clsCommon.GetString("err_failed_DeleteTimeUpload", "時間設定"));
                result = false;
            }
            finally
            {
                objDbHelper.Close();
            }
            return result;
        }
    }
}
