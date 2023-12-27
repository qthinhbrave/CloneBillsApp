using IBatisNet.DataMapper.Configuration;
using IBatisNet.DataMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class
{
    public class clsDatabaseHelper
    {
        private ISqlMapper m_objMapper;

        public clsDatabaseHelper()
        {
            DomSqlMapBuilder builder = new DomSqlMapBuilder();
            this.m_objMapper = builder.Configure(ConfigurationManager.AppSettings["iBatisConfigPath"]);
        }

        /// <summary>
        /// SELECT SQLコマンドオブジェクトを生成する
        /// </summary>
        /// <param name="strStatementName"></param>
        /// <param name="strTableName"></param>
        /// <param name="objParams"></param>
        /// <returns></returns>
        public clsDatabaseCommand CreateSelectCommand(string strStatementName, string strTableName, object objParams)
        {
            return new clsDatabaseCommand(clsDatabaseCommand.enmCommandType.SELECT, this.m_objMapper, strStatementName, strTableName, objParams);
        }

        /// <summary>
        /// INSERT SQLコマンドオブジェクトを生成する
        /// </summary>
        /// <param name="strStatementName"></param>
        /// <param name="objParams"></param>
        /// <returns></returns>
        public clsDatabaseCommand CreateInsertCommand(string strStatementName, Hashtable objParams)
        {
            return new clsDatabaseCommand(clsDatabaseCommand.enmCommandType.INSERT, this.m_objMapper, strStatementName, "", objParams);
        }

        /// <summary>
        /// UPDATE SQLコマンドオブジェクトを生成する
        /// </summary>
        /// <param name="strStatementName"></param>
        /// <param name="objParams"></param>
        /// <returns></returns>
        public clsDatabaseCommand CreateUpdateCommand(string strStatementName, Hashtable objParams)
        {
            return new clsDatabaseCommand(clsDatabaseCommand.enmCommandType.UPDATE, this.m_objMapper, strStatementName, "", objParams);
        }

        /// <summary>
        /// DELETE SQLコマンドオブジェクトを生成する
        /// </summary>
        /// <param name="strStatementName"></param>
        /// <param name="String"></param>
        /// <param name="objParams"></param>
        /// <param name="Hashtable"></param>
        /// <returns></returns>
        public clsDatabaseCommand CreateDeleteCommand(String strStatementName, Hashtable objParams)
        {
            return new clsDatabaseCommand(clsDatabaseCommand.enmCommandType.DELETE, this.m_objMapper, strStatementName, "", objParams);
        }

        /// <summary>
        /// SELECT文を実行しDataTableを返す
        /// </summary>
        /// <param name="objCommand"></param>
        /// <param name="clsDatabaseCommand"></param>
        /// <returns></returns>
        public DataTable ExecuteQuery(clsDatabaseCommand objCommand)
        {
            //clsLogger.Debug("ExecuteQuery() - start");
            //clsLogger.Debug(objCommand.GetSql());

            IDbDataAdapter objAdapter = objCommand.GetAdapter();

            //20210329 Timeoutエラー対応
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["SQLCommandTimeout"]) == false)
            {
                objAdapter.SelectCommand.CommandTimeout = Int32.Parse(ConfigurationManager.AppSettings["SQLCommandTimeout"]);
            }
            DataSet objDataSet = new DataSet();
            objAdapter.Fill(objDataSet);
            DataTable objDataTable = objDataSet.Tables[0];
            objDataTable.TableName = objCommand.TableName;

            //clsLogger.Debug("ExecuteQuery() - end");
            return objDataTable;
        }

        /// <summary>
        /// INSERT/UPDATE/DELETE文を実行する
        /// </summary>
        /// <param name="objCommand"></param>
        /// <param name="clsDatabaseCommand"></param>
        /// <returns></returns>
        public Object ExecuteNonQuery(clsDatabaseCommand objCommand)
        {
            switch (objCommand.SqlCommandType)
            {
                case clsDatabaseCommand.enmCommandType.INSERT:
                    return this.Insert(objCommand);

                case clsDatabaseCommand.enmCommandType.UPDATE:
                    this.Update(objCommand);
                    return null;

                case clsDatabaseCommand.enmCommandType.DELETE:
                    this.Delete(objCommand);
                    return null;

                default:
                    throw new ArgumentException("不正なSQLタイプのコマンドが指定されました。:" + objCommand.SqlCommandType.ToString());
            }
        }

        /// <summary>
        /// INSERTコマンドを実行する
        /// </summary>
        /// <param name="objCommand"></param>
        /// <param name="clsDatabaseCommand"></param>
        /// <returns></returns>
        public object Insert(clsDatabaseCommand objCommand)
        {
            Object objKey = m_objMapper.Insert(objCommand.StatementName, objCommand.Parameters);
            return objKey;
        }

        /// <summary>
        /// UPDATEコマンドを実行する
        /// </summary>
        /// <param name="objCommand"></param>
        /// <param name="clsDatabaseCommand"></param>
        public void Update(clsDatabaseCommand objCommand)
        {
            m_objMapper.Update(objCommand.StatementName, objCommand.Parameters);
        }

        /// <summary>
        /// DELETEコマンドを実行する
        /// </summary>
        /// <param name="objCommand"></param>
        /// <param name="clsDatabaseCommand"></param>
        public void Delete(clsDatabaseCommand objCommand)
        {
            m_objMapper.Delete(objCommand.StatementName, objCommand.Parameters);
        }

        /// <summary>
        /// データベースに接続する
        /// </summary>
        public void Open()
        {
            if (!m_objMapper.IsSessionStarted)
            {
                m_objMapper.OpenConnection();
            }
        }

        /// <summary>
        /// データベースの接続を解除する
        /// </summary>
        public void Close()
        {
            if (m_objMapper.IsSessionStarted)
            {
                m_objMapper.CloseConnection();
            }
        }

        /// <summary>
        /// トランザクションを開始する
        /// </summary>
        public void Begin()
        {
            m_objMapper.BeginTransaction(false);
        }

        /// <summary>
        /// トランザクションをコミットする
        /// </summary>
        public void Commit()
        {
            m_objMapper.CommitTransaction(false);
        }

        /// <summary>
        /// トランザクションをロールバックする
        /// </summary>
        public void Rollback()
        {
            m_objMapper.RollBackTransaction(false);
        }
    }
}
