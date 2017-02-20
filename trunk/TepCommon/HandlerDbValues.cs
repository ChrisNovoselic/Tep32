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
        public struct DB_TABLE
        {
            public string m_name;

            public string m_description;
        }
        /// <summary>
        /// Наименования таблиц в БД, необходимых для расчета (длина = INDEX_DBTABLE_NAME.COUNT)
        /// </summary>
        public static Dictionary<ID_DBTABLE, DB_TABLE> s_dictDbTables = new Dictionary<ID_DBTABLE, DB_TABLE> () {
            { ID_DBTABLE.TIME           , new DB_TABLE () { m_name = @"time"        , m_description = @"" } }
            , { ID_DBTABLE.TIMEZONE     , new DB_TABLE () { m_name = @"timezones"   , m_description = @"" } }
            , { ID_DBTABLE.PERIOD       , new DB_TABLE () { m_name = @"period"      , m_description = @"" } }
            , { ID_DBTABLE.COMP_LIST    , new DB_TABLE () { m_name = @"comp_list"   , m_description = @"" } }
            , { ID_DBTABLE.COMP         , new DB_TABLE () { m_name = @"comp"        , m_description = @"" } }
            , { ID_DBTABLE.MODE_DEV     , new DB_TABLE () { m_name = @"mode_dev"    , m_description = @"" } }
            , { ID_DBTABLE.RATIO        , new DB_TABLE () { m_name = @"ratio"       , m_description = @"" } }
            , { ID_DBTABLE.MEASURE      , new DB_TABLE () { m_name = @"measure"     , m_description = @"" } }
            , { ID_DBTABLE.SESSION      , new DB_TABLE () { m_name = @"session"     , m_description = @"" } }
            , { ID_DBTABLE.INALG        , new DB_TABLE () { m_name = @"inalg"       , m_description = @"" } }
            , { ID_DBTABLE.INPUT        , new DB_TABLE () { m_name = @"input"       , m_description = @"" } }
            , { ID_DBTABLE.INVALUES     , new DB_TABLE () { m_name = @"inval"       , m_description = @"" } }
            , { ID_DBTABLE.INVAL_DEF    , new DB_TABLE () { m_name = @"inval_def"   , m_description = @"" } }
            , { ID_DBTABLE.OUTALG       , new DB_TABLE () { m_name = @"outalg"      , m_description = @"" } }
            , { ID_DBTABLE.OUTPUT       , new DB_TABLE () { m_name = @"output"      , m_description = @"" } }
            , { ID_DBTABLE.OUTVALUES    , new DB_TABLE () { m_name = @"outval"      , m_description = @"" } }
            , { ID_DBTABLE.IN_PARAMETER , new DB_TABLE () { m_name = @"???"         , m_description = @"" } }
            , { ID_DBTABLE.OUT_PARAMETER, new DB_TABLE () { m_name = @"???"         , m_description = @"" } }
            , { ID_DBTABLE.FTABLE       , new DB_TABLE () { m_name = @"ftable"      , m_description = @"" } }
            , { ID_DBTABLE.PLUGINS      , new DB_TABLE () { m_name = @"plugins"     , m_description = @"" } }
            , { ID_DBTABLE.TASK         , new DB_TABLE () { m_name = @"task"        , m_description = @"" } }
            , { ID_DBTABLE.FPANELS      , new DB_TABLE () { m_name = @"fpanels"     , m_description = @"" } }
            ,
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
        /// <param name="idTableDb">Индекс таблицы в перечне таблиц БД</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        /// <returns>Таблица - результат запроса - значения таблицы БД</returns>
        public DataTable GetDataTable(ID_DBTABLE idTableDb, out int err)
        {
            err = 0; // успех

            DataTable tblRes = null;

            switch (idTableDb) {
                case ID_DBTABLE.UNKNOWN:
                case ID_DBTABLE.PERIOD:
                    err = -1;
                    break;                
                default:
                    if (s_dictDbTables.ContainsKey(idTableDb) == false) {
                        err = -2;
                    } else
                        if (!(s_dictDbTables[idTableDb].m_name.IndexOf(@"?") < 0)) {
                            err = -3;
                        } else
                            tblRes = GetDataTable(s_dictDbTables[idTableDb].m_name, out err);                        
                    break;                
            }

            if (err < 0)
                tblRes = new DataTable();
            else
                ;

            return tblRes;
        }
        /// <summary>
        /// Обновить значения в БД в соответствии с внесенными изменениями (различия между оригинальной и редактируемой таблицой)
        /// </summary>
        /// <param name="nameTable">имя таблицы в бд</param>
        /// <param name="strKeyFields">ключевые поля для вставки</param>
        /// <param name="unchangeableColumn">столбцы, не учавствующие в запросе</param>
        /// <param name="tblOrigin">оригинальная таблица</param>
        /// <param name="tblEdit">отредактированная таблица</param>
        /// <param name="err">номер ошибки</param>
        public void RecUpdateInsertDelete(string nameTable
            , string strKeyFields
            , string unchangeableColumn
            , DataTable tblOrigin, DataTable tblEdit
            , out int err)
        {
            err = -1;

            int iRegDbConn = -1;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                DbTSQLInterface.RecUpdateInsertDelete(ref _dbConnection, nameTable, strKeyFields, unchangeableColumn, tblOrigin, tblEdit, out err);
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

        protected virtual string getQueryTimePeriods(params int[]ids)
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_dictDbTables[ID_DBTABLE.TIME].m_name + @"] WHERE [ID] IN (" + String.Join(@",", ids.Cast<string>().ToArray()) + @")";

            return strRes;
        }

        protected virtual string getQueryTimezones(params int[] ids)
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_dictDbTables[ID_DBTABLE.TIMEZONE].m_name + @"] WHERE [ID] IN (" + String.Join(@",", ids.Cast<string>().ToArray()) + @")";

            return strRes;
        }

        protected virtual string getQueryCompList()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_dictDbTables[ID_DBTABLE.COMP_LIST].m_name + @"] "
                    + @"WHERE ([ID] = 5 AND [ID_COMP] = 1)"
                        + @" OR ([ID_COMP] = 1000)";

            return strRes;
        }

        protected virtual string getQueryModeDev()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_dictDbTables[ID_DBTABLE.MODE_DEV].m_name + @"]";

            return strRes;
        }

        protected virtual string getQueryMeasures()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_dictDbTables[ID_DBTABLE.MEASURE].m_name + @"]";

            return strRes;
        }

        protected virtual string getQueryRatio()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [dbo].[" + s_dictDbTables[ID_DBTABLE.RATIO].m_name + @"]";

            return strRes;
        }
    }
}
