using IBatisNet.DataMapper.MappedStatements;
using IBatisNet.DataMapper.Scope;
using IBatisNet.DataMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class
{
    public class clsDatabaseCommand
    {
        private ISqlMapper m_objMapper;
        private IDbCommand m_objCommand;
        private RequestScope m_objScope;

        /// <summary>
        /// SQLコマンドタイプ
        /// </summary>
        public enum enmCommandType
        {
            SELECT,
            INSERT,
            UPDATE,
            DELETE,
            OTHER
        }
        public enmCommandType SqlCommandType { get; private set; }

        /// <summary>
        /// 指定されたテーブル名
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// SQLステートメントID
        /// </summary>
        public string StatementName { get; private set; }

        /// <summary>
        /// SQLパラメータ
        /// </summary>
        public object Parameters { get; private set; }

        /// <summary>
        /// SQLコマンドオブジェクトのコンストラクタ
        /// </summary>
        /// <param name="iCommandType"></param>
        /// <param name="objDbHelper"></param>
        /// <param name="objMapper"></param>
        /// <param name="strStatementName"></param>
        /// <param name="strTableName"></param>
        /// <param name="objParams"></param>
        /// <remarks>clsDatabaseHelper.createCommandを使ってインスタンスを取得して下さい</remarks>
        public clsDatabaseCommand(enmCommandType iCommandType, ISqlMapper objMapper, string strStatementName, string strTableName, object objParams)
        {
            m_objMapper = objMapper;

            SqlCommandType = iCommandType;
            StatementName = strStatementName;
            TableName = strTableName;

            if (objParams == null)
            {
                Parameters = new Hashtable();
            }
            else
            {
                Parameters = objParams;
            }

            IMappedStatement objStatement = objMapper.GetMappedStatement(strStatementName);
            m_objScope = objStatement.Statement.Sql.GetRequestScope(objStatement, Parameters, objMapper.LocalSession);
            objStatement.PreparedCommand.Create(m_objScope, objMapper.LocalSession, objStatement.Statement, Parameters);
            m_objCommand = objMapper.LocalSession.CreateCommand(CommandType.Text);
            m_objCommand.CommandText = m_objScope.IDbCommand.CommandText;

            foreach (IDbDataParameter pa in m_objScope.IDbCommand.Parameters)
            {
                m_objCommand.Parameters.Add(pa);
            }
        }

        /// <summary>
        /// SQL文字列を返す
        /// </summary>
        /// <returns></returns>
        public string GetSql()
        {
            return m_objScope.PreparedStatement.PreparedSql;
        }

        /// <summary>
        /// DataAdapterを取得する
        /// </summary>
        /// <returns></returns>
        public IDbDataAdapter GetAdapter()
        {
            IDbDataAdapter objAdapter = m_objMapper.LocalSession.CreateDataAdapter(m_objCommand);
            return objAdapter;
        }

    }
}
