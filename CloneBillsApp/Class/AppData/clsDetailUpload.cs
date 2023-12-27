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
    public class clsDetailUpload
    {
        public int DetailID { get; set; }
        public int HistoryID { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public bool Result { get; set; }
        public string Remark { get; set; }

        public clsDetailUpload() { }

        public bool Insert()
        {
            bool result = true;
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                objDbHelper.Open();

                Hashtable objParams = new Hashtable();
                objParams["HISTORY_ID"] = HistoryID;
                objParams["FILE_NAME"] = FileName;
                objParams["PATH"] = Path;
                objParams["SIZE"] = Size;
                objParams["RESULT"] = Result;
                objParams["REMARK"] = Remark;
                clsDatabaseCommand objCommand;

                //
                objCommand = objDbHelper.CreateInsertCommand("SQL.InsertDetailUpload", objParams);
                objDbHelper.Insert(objCommand);
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


    }
}
