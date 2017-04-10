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

        public enum STATE_VALUE { ORIGINAL, EDIT }
        /// <summary>
        /// Наименования таблиц в БД, необходимых для расчета (длина = INDEX_DBTABLE_NAME.COUNT)
        /// </summary>
        public static Dictionary<ID_DBTABLE, DB_TABLE> s_dictDbTables = new Dictionary<ID_DBTABLE, DB_TABLE> () {
            { ID_DBTABLE.TIME           , new DB_TABLE () { m_name = @"time"        , m_description = @"" } }
            , { ID_DBTABLE.TIMEZONE     , new DB_TABLE () { m_name = @"timezones"   , m_description = @"" } }
            //, { ID_DBTABLE.PERIOD       , new DB_TABLE () { m_name = @"period"      , m_description = @"" } }
            , { ID_DBTABLE.COMP_LIST    , new DB_TABLE () { m_name = @"comp_list"   , m_description = @"" } }
            , { ID_DBTABLE.COMP         , new DB_TABLE () { m_name = @"comp"        , m_description = @"" } }
            , { ID_DBTABLE.COMP_VALUES  , new DB_TABLE () { m_name = @"comp_values" , m_description = @"" } }
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
            , { ID_DBTABLE.ROLES_UNIT   , new DB_TABLE () { m_name = @"roles_unit"  , m_description = @"" } }
            , { ID_DBTABLE.USERS        , new DB_TABLE () { m_name = @"users"       , m_description = @"" } }
            ,
        };
        /// <summary>
        /// Словарь для хранения таблиц со словарно-проектными значениями
        /// ??? public временно
        /// </summary>
        public class DictionaryTableDictProject : Dictionary<ID_DBTABLE, DataTable>
        {
            public enum Error
            {
                ExFilterBuilder = -12
                , ExRowRemove
                , IdDbTableUnknown = -2
                , Any = -1
                , Not = 0
                , WFilterNoApplied
            }

            public class TSQLWhereItem
            {
                [Flags]
                public enum RULE
                {
                    NotSet
                    , Equale
                    , Between
                    , NotEquale
                    , Above
                    , Below
                }

                private string _nameField;

                private object[] _limits;

                public string Value { get; }

                public RULE Rules;

                public int Ready { get; }

                public TSQLWhereItem(string nameField, RULE rules, params object[] limits)
                {
                    //int lim_max = -1
                    //    , range = -1;

                    _nameField = nameField;
                    if ((!(limits == null))
                        && (limits.Length > 0)) {
                        if (limits.Length == 1) { // единственный объект
                            if (limits[0] is Array) {
                            // предполагаем, что все объекты переданы в массиве единственного параметра
                                //Array.Copy(limits[0] as Array, _limits, (limits[0] as Array).Length);
                                _limits = new object[(limits[0] as Array).Length];
                                (limits[0] as Array).CopyTo(_limits, 0);
                            } else {
                            // предполагаем, что единственный объект простой, пригодный для использования в запросе для ограничения
                                _limits = new object[limits.Length];
                                limits.CopyTo(_limits, 0);
                            }
                        } else { // объектов более, чем 1
                        // предполагаем, что все объекты простые, пригодные для использования в запросе для ограничения
                            _limits = new object[limits.Length];
                            limits.CopyTo(_limits, 0);
                        }
                    } else
                        ;
                    Rules = rules;

                    Ready = 0;

                    //lim_max = (int)_limit;
                    //range = 1;
                    //while ((lim_max / range) > 0) range *= 10;
                    //lim_max += range;

                    foreach (RULE rule in Enum.GetValues(typeof(RULE)))
                        if ((rule & Rules) == rule)
                            switch (rule) {
                                case RULE.Equale:
                                    Value = string.Format(@"{0}={1}", _nameField, (int)_limits[0]);
                                    break;
                                case RULE.Between:
                                    Value = string.Format(@"{0}>{1} AND {0}<{2}", _nameField, (int)_limits[0], (int)_limits[1]);
                                    break;
                                default:
                                    break;
                            }
                        else
                            ;
                }
            }

            public class ListTSQLWhere : List<TSQLWhereItem>//, IDisposable
            {
                public ID_DBTABLE IdDbTable;

                public ListTSQLWhere(ID_DBTABLE idDbTable)
                {
                    IdDbTable = idDbTable;
                }

                public string Value
                {
                    get {
                        string strRes = string.Empty;

                        foreach (TSQLWhereItem item in this) {
                            if (item.Ready == 0) {
                                if (this.IndexOf(item) == 0)
                                    if (Count == 1)
                                        strRes = item.Value;
                                    else
                                        strRes = string.Format(@"({0})", item.Value);
                                else
                                    strRes += string.Format(@" OR ({0})", item.Value);
                            } else
                                ;
                        }

                        return strRes;
                    }
                }

                //public void Dispose()
                //{
                //}
            }

            public DictionaryTableDictProject()
            {
            }

            public virtual Error SetDbTableFilter(ListTSQLWhere where)
            {
                Error iRes = Error.Not; // ошибка

                ID_DBTABLE idDbTable = where.IdDbTable;
                string filter = where.Value;
                List<DataRow> rowsToTake = null
                    , rowsToRemove = null
                    ;

                try {
                    rowsToTake = new List<DataRow>();
                    rowsToRemove = new List<DataRow>();

                    if ((!(idDbTable == ID_DBTABLE.UNKNOWN))
                        && (string.IsNullOrEmpty(filter) == false))
                        if (ContainsKey(idDbTable) == true) {
                            // получить удерживаемые строки
                            rowsToTake =
                                this[idDbTable].Select(filter).ToList()
                                ;
                            // получить удаляемые строки                            
                            foreach (DataRow row in this[idDbTable].Rows)
                                foreach (DataRow rowToTake in rowsToTake)                                    
                                    // удалению все строки, отличающиеся от удерживаемых
                                    if ((rowsToTake.IndexOf(row) < 0)
                                        && (rowsToRemove.IndexOf(row) < 0))
                                        rowsToRemove.Add(row);
                                    else
                                        ;

                            iRes = Error.Not; // Ok
                        } else
                            iRes = Error.IdDbTableUnknown;
                    else
                        iRes = Error.WFilterNoApplied; // фильтр не применен - не найден обработчик
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"SetDbTableFilter (DbFilter={0}, idDbTable={1})..."
                        , where.ToString(), idDbTable)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }

                try {
                    if (iRes == Error.Not) {
                    // удление строк "к удалению"
                        foreach (DataRow rowToRemove in rowsToRemove)
                            this[idDbTable].Rows.Remove(rowToRemove);
                    } else
                        ;

                    this[idDbTable].AcceptChanges();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"SetDbTableFilter (DbFilter={0}, idDbTable={1}) - строк для сохранения={2}..."
                        , where.ToString(), idDbTable, rowsToTake.Count)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = Error.ExRowRemove; // исключение при удалении строк
                }

                return iRes;
            }
        }
        /// <summary>
        /// Словарь с таблицами словарно-проектных значений
        /// ??? public временно
        /// </summary>
        public DictionaryTableDictProject m_dictTableDictPrj;
        /// <summary>
        /// Проверить возможность добавления таблиц в словарь словарно-проектных величин
        ///  , при необходимости обеспечить такую возможность
        /// </summary>
        public void DictTableDictPrjValidate()
        {
            if ((!(m_dictTableDictPrj == null))
                && (m_dictTableDictPrj.Count > 0)) {
                Logging.Logg().Warning(@"HPanelTepCommon::initialize () - словарно-проектные таблицы повторная инициализация...", Logging.INDEX_MESSAGE.NOT_SET);

                Clear();
            } else
                m_dictTableDictPrj = new DictionaryTableDictProject();
        }
        /// <summary>
        /// Очистить все словари/списки со словарно-проектными величинами
        /// </summary>
        public virtual void Clear()
        {
            m_dictTableDictPrj.Clear();
        }
        /// <summary>
        /// Добавить таблицу в словарь со словарно-проектными величинами
        /// </summary>
        /// <param name="id">Идентификатор таблицы БД</param>
        /// <param name="err">Признак ошибки при чтении из БД данных таблицы</param>
        public virtual void AddTableDictPrj(ID_DBTABLE id, out int err)
        {
            addTableDictPrj(id, GetDataTable(id, out err));
        }
        /// <summary>
        /// Добавить таблицу в словарь со словарно-проектными величинами
        /// </summary>
        /// <param name="idDbTable">дентификатор таблицы БД</param>
        /// <param name="table">Таблица с данными для добавления</param>
        protected void addTableDictPrj(ID_DBTABLE idDbTable, DataTable table)
        {
            if (m_dictTableDictPrj.ContainsKey(idDbTable) == false) {
                m_dictTableDictPrj.Add(idDbTable, table);
            } else
                ;
        }
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
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
                    //Console.WriteLine(string.Format(@"HandlerDbTaskCalculate::RegisterDbConnection () - _iListenerId={0}...", _iListenerId))
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
            if (isRegisterDbConnection == true) {
                //Console.WriteLine(string.Format(@"HandlerDbTaskCalculate::UnRegisterDbConnection () - _iListenerId={0}...", _iListenerId));

                DbSources.Sources().UnRegister(_iListenerId);
                _dbConnection = null;
                _iListenerId = -1;
            } else
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
                //case ID_DBTABLE.PERIOD:
                    err = -1;
                    break;
                case ID_DBTABLE.IN_PARAMETER:
                case ID_DBTABLE.OUT_PARAMETER:
                    err = 1;
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
