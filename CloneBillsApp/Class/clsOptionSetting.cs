using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class
{
    public class clsOptionSetting
    {
        private static DataTable m_objOptionTable = null;

        public static void CreateDB()
        {
            var createDBSQL = ".\\Db\\CloneBill.db";
            SQLiteConnection.CreateFile(createDBSQL);
        }
        /// <summary>
        /// オプション設定値ロード
        /// </summary>
        public static bool Load()
        {
            clsDatabaseHelper objDbHelper = new clsDatabaseHelper();
            try
            {
                objDbHelper.Open();
                clsDatabaseCommand objCommand = objDbHelper.CreateSelectCommand("SQL.SelectOptions", "MST_OPTION", null);
                m_objOptionTable = objDbHelper.ExecuteQuery(objCommand);
                return true;
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }
            finally
            {
                objDbHelper.Close();
            }
            return false;
        }

        /// <summary>
        /// オプション設定値取得(文字列型)
        /// </summary>
        /// <param name="strKey1"></param>
        /// <param name="strKey2"></param>
        /// <returns></returns>
        public static string GetString_Value1(string strKey1, string strKey2)
        {
            try
            {
                DataRow objOptionRow = m_objOptionTable.AsEnumerable().Where(objRow => objRow.Field<string>("KEY_1").Equals(strKey1) && objRow.Field<string>("KEY_2").Equals(strKey2)).CopyToDataTable().Rows[0];
                return objOptionRow["Value_1"].ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public static string GetString_Value2(string strKey1, string strKey2)
        {
            try
            {
                DataRow objOptionRow = m_objOptionTable.AsEnumerable().Where(objRow => objRow.Field<string>("KEY_1").Equals(strKey1) && objRow.Field<string>("KEY_2").Equals(strKey2)).CopyToDataTable().Rows[0];
                return objOptionRow["Value_2"].ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public static bool GetBool_Value3(string strKey1, string strKey2)
        {
            try
            {
                DataRow objOptionRow = m_objOptionTable.AsEnumerable().Where(objRow => objRow.Field<string>("KEY_1").Equals(strKey1) && objRow.Field<string>("KEY_2").Equals(strKey2)).CopyToDataTable().Rows[0];
                return bool.Parse(objOptionRow["Value_3"].ToString());
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// Save configs
        /// </summary>
        /// <param name="key_1"></param>
        /// <param name="key_2"></param>
        /// <param name="value_1"></param>
        /// <param name="value_2"></param>
        /// <param name="value_3"></param>
        /// <returns></returns>
        public static bool Save(string key_1, string key_2, string value_1, string value_2 = "NULL", string value_3 = "NULL")
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
                objParams["KEY_1"] = key_1;
                objParams["KEY_2"] = key_2;
                objParams["Value_1"] = value_1;
                if (value_2 != "NULL") objParams["Value_2"] = value_2;
                if (value_3 != "NULL") objParams["Value_3"] = value_3;

                //SQLコマンド
                clsDatabaseCommand objCommand = objDbHelper.CreateUpdateCommand("SQL.UpdateOption", objParams);
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
