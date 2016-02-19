using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public partial class HandlerDbValues : HHandlerDb
    {
        /// <summary>
        /// Наименования таблиц в БД, необходимых для расчета (длина = INDEX_DBTABLE_NAME.COUNT)
        /// </summary>
        public static string[] s_NameDbTables = {
            @"time"
            , @"timezones"
            , @"comp_list"
            , @"mode_dev"
            , @"ratio"
            , @"measure"
            , @"session"
            , @"inalg"
            , @"input"
            , @"inval"
            , @"inval_def"
            , @"outalg"
            , @"output"
            , @"outval"
            , @"ftable"
        };

        public HandlerDbValues()
            : base()
        {
        }

        public override void StartDbInterfaces()
        {
            throw new NotImplementedException();
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            throw new NotImplementedException();
        }

        protected override int StateRequest(int state)
        {
            throw new NotImplementedException();
        }

        protected override int StateResponse(int state, object obj)
        {
            throw new NotImplementedException();
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected ConnectionSettings _connSett;

        protected int _iListenerId;

        protected DbConnection _dbConnection;

        protected bool isRegisterDbConnection { get { return (_iListenerId > 0) && (!(_dbConnection == null)) && (_dbConnection.State == ConnectionState.Open); } }

        public void InitConnectionSettings(ConnectionSettings connSett)
        {
            _connSett = connSett;
        }
        /// <summary>
        /// Зарегистрировать соединение с БД
        /// </summary>
        /// <param name="err">Признак выполнения операции (0 - ошибок нет, 1 - регистрация уже произведена, -1 - ошибка)</param>
        public void RegisterDbConnection(out int err)
        {
            err = -1;
            // проверить требуется ли регистрация
            if (isRegisterDbConnection == false)
            {
                // получить идентификатор регистрации для соединения с БД
                _iListenerId = DbSources.Sources().Register(_connSett, false, CONN_SETT_TYPE.MAIN_DB.ToString());
                // получить объект соединения с БД
                _dbConnection = DbSources.Sources().GetConnection(_iListenerId, out err);

                if (!(err == 0))
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::RegisterDbConnection () - ошибка соединения с БД...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
            }
            else
                // регистрация не требуется - признак для оповещения вызвавшего контекста
                err = 1;
        }
        /// <summary>
        /// Отменить регистрацию соединения с БД
        /// </summary>
        public void UnRegisterDbConnection()
        {
            // проверить требуется ли регистрация
            if (isRegisterDbConnection == true)
            {
                DbSources.Sources().UnRegister(_iListenerId);
                _dbConnection = null;
                _iListenerId = -1;
            }
            else
                ;
        }
        /// <summary>
        /// Объект с установленным соединением с БД
        /// </summary>
        public DbConnection DbConnection
        {
            get { return _dbConnection; }
        }
        /// <summary>
        /// Объект с параметрами для установления соединения с БД
        /// </summary>
        public ConnectionSettings ConnectionSettings
        {
            get { return _connSett; }
        }
        /// <summary>
        /// Выполнить запрос с возвращением результата в виде таблицы
        /// </summary>
        /// <param name="query">Содержание запроса к БД</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        /// <returns>Таблица - результат запроса</returns>
        public DataTable Select(string query, out int err)
        {
            err = -1;

            DataTable tableRes = new DataTable();

            int iRegDbConn = -1;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
                tableRes = DbTSQLInterface.Select(ref _dbConnection, query, null, null, out err);
            else
                ;

            if (!(iRegDbConn > 0))
            {
                UnRegisterDbConnection();
            }
            else
                ;

            return tableRes;
        }
        /// <summary>
        /// Возвратить значения одной таблицы
        /// </summary>
        /// <param name="strNameTable">Наименование таблицы</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        /// <returns>Таблица - результат запроса - значения таблицы БД</returns>
        public DataTable GetDataTable(string strNameTable, out int err)
        {
            return Select(@"SELECT * FROM [" + strNameTable + @"]", out err);
        }
        /// <summary>
        /// Возвратить значения одной таблицы по индексу
        /// </summary>
        /// <param name="indxTable">Индекс таблицы в перечне таблиц БД</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        /// <returns>Таблица - результат запроса - значения таблицы БД</returns>
        public DataTable GetDataTable(INDEX_DBTABLE_NAME indxTable, out int err)
        {
            return GetDataTable(s_NameDbTables[(int)indxTable], out err);
        }

        public void RecUpdateInsertDelete(string nameTable, string strKeyFields, DataTable tblOrigin, DataTable tblEdit, out int err)
        {
            err = -1;

            int iRegDbConn = -1;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                DbTSQLInterface.RecUpdateInsertDelete(ref _dbConnection, nameTable, strKeyFields, tblOrigin, tblEdit, out err);
            }
            else
                ;

            if (!(iRegDbConn > 0))
            {
                UnRegisterDbConnection();
            }
            else
                ;
        }

        public string GetQueryTimePeriods(string strIds)
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.TIME] + @"] WHERE [ID] IN (" + strIds + @")";

            return strRes;
        }

        public string GetQueryTimezones(string strIds)
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.TIMEZONE] + @"] WHERE [ID] IN (" + strIds + @")";

            return strRes;
        }

        public string GetQueryCompList()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.COMP_LIST] + @"] "
                    + @"WHERE ([ID] = 5 AND [ID_COMP] = 1)"
                        + @" OR ([ID_COMP] = 1000)";

            return strRes;
        }

        public string GetQueryModeDev()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MODE_DEV] + @"]";

            return strRes;
        }

        public string GetQueryMeasures()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MEASURE] + @"]";

            return strRes;
        }

        public string GetQueryRatio()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.RATIO] + @"]";

            return strRes;
        }
    }
}
