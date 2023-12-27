using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsHistoryUpload
    {
        public int HistoryID { get; set; }
        public DateTime RecordDT { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public int NumberUploaded { get; set; }
        public int IntervalSec { get; set; }

        public clsHistoryUpload()
        {

        }

        public bool Insert(string fromLocation, string toLocation)
        {
            bool result = true;
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                objDbHelper.Open();

                Hashtable objParams = new Hashtable();
                objParams["FROM_LOCATION"] = fromLocation;
                objParams["TO_LOCATION"] = toLocation;
                clsDatabaseCommand objCommand;

                //
                objCommand = objDbHelper.CreateInsertCommand("SQL.InsertHistoryUpload", objParams);
                objDbHelper.Insert(objCommand);

                //
                objParams.Clear();
                objCommand = objDbHelper.CreateSelectCommand("SQL.SelectHistoryUploadLastId", "HISTORY_UPLOAD", null);
                var dt = objDbHelper.ExecuteQuery(objCommand);
                HistoryID = (int)dt.Rows[0].Field<long>("HISTORY_ID");
                RecordDT = DateTime.Parse(dt.Rows[0].Field<string>("RECORD_DT"));
                FromLocation = dt.Rows[0].Field<string>("FROM_LOCATION");
                toLocation = dt.Rows[0].Field<string>("TO_LOCATION");
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
                result = false;
            }
            finally
            {
                objDbHelper.Close();
            }
            return result;
        }

        public bool UpdateEndUpload()
        {
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                //DBオープン
                objDbHelper.Open();

                //トランザクション開始
                objDbHelper.Begin();

                //パラメータ
                Hashtable objParams = new Hashtable();
                objParams["NUMBER_UPLOADED"] = NumberUploaded;
                objParams["INTERVAL_SEC"] = IntervalSec;
                objParams["HISTORY_ID"] = HistoryID;

                //SQLコマンド
                clsDatabaseCommand objCommand = objDbHelper.CreateUpdateCommand("SQL.UpdateHistoryUpload", objParams);
                objDbHelper.Update(objCommand);

                //コミット
                objDbHelper.Commit();
            }
            catch (Exception ex)
            {
                objDbHelper.Rollback();
                clsLogger.Err(ex.Message);
                return false;
            }
            finally
            {
                objDbHelper.Close();
            }
            return true;
        }
    }
}
