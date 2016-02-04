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
    public partial class HandlerDbTaskCalculate : HHandlerDb
    {
        public enum TABLE_CALCULATE_REQUIRED : short { UNKNOWN = -1, ALG, PUT, VALUE, COUNT }
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
        };
        /// <summary>
        /// Перечисление - идентификаторы состояния полученных из БД значений
        /// </summary>
        public enum ID_QUALITY_VALUE { NOT_REC = -3, PARTIAL, DEFAULT, SOURCE, USER }

        TaskTepCalculate m_taskTepCalculate;

        public HandlerDbTaskCalculate()
            : base()
        {
            m_taskTepCalculate = new TaskTepCalculate();
        }

        public override void StartDbInterfaces()
        {
            throw new NotImplementedException();
        }

        public override void ClearValues()
        {
            throw new NotImplementedException();
        }

        public void Load(ID_PERIOD idTime)
        {
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

        private ConnectionSettings _connSett;
        
        private int _iListenerId;

        private DbConnection _dbConnection;

        private bool isRegisterDbConnection { get { return (_iListenerId > 0) && (!(_dbConnection == null)) && (_dbConnection.State == ConnectionState.Open); } }

        public void InitConnectionSettings(ConnectionSettings connSett)
        {
            _connSett = connSett;
        }
        /// <summary>
        /// Зарегистрировать соединение с БД
        /// </summary>
        /// <param name="err">Признак выполнения операции (0 - ошибок нет, 1 - регистрация уже произведена, -1 - ошибка)</param>
        public void RegisterDbConnection (out int err)
        {
            err = -1;
            // проверить требуется ли регистрация
            if (isRegisterDbConnection == false)
            {
                _iListenerId = DbSources.Sources().Register(_connSett, false, CONN_SETT_TYPE.MAIN_DB.ToString());
                _dbConnection = DbSources.Sources().GetConnection(_iListenerId, out err);

                if (!(err == 0))
                {
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::RegisterDbConnection () - ошибка соединения с БД...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    ;
            }
            else
                // регистрация не требуется
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
        /// Создать новую сессию для расчета
        ///  - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        public void CreateSession(int idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , ID_TIMEZONE idTimezone
            , DataTable tablePars
            , ref DataTable tableInValues, ref DataTable tableDefValues
            , DateTimeRange []dtRanges
            , out int err, out string strErr)
        {            
            err = 0;
            strErr = string.Empty;

            int iAVG = -1;
            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsSel = null;
            Type[] arTypeColumns = null;
            DateTimeRange dtRange = new DateTimeRange (dtRanges[0].Begin, dtRanges[dtRanges.Length - 1].End);

            // удалить строки из таблицы со значениями "по умолчанию"
            foreach (DataRow rValVar in tableInValues.Rows)
            {
                rowsSel = tableDefValues.Select(@"ID_INPUT=" + rValVar[@"ID"]);
                foreach (DataRow rToRemove in rowsSel)
                    tableDefValues.Rows.Remove(rToRemove);
            }
            // вставить строки из таблицы со значениями "по умолчанию"
            foreach (DataRow rValDef in tableDefValues.Rows)
            {
                rowsSel = tablePars.Select(@"ID=" + rValDef[@"ID_INPUT"]);
                if (rowsSel.Length == 1)
                {
                    iAVG = (Int16)rowsSel[0][@"AVG"];

                    tableInValues.Rows.Add(new object[]
                                    {
                                        rValDef[@"ID_INPUT"]
                                        //, HUsers.Id //ID_USER
                                        , -1 //ID_SOURCE
                                        , idSession //ID_SESSION
                                        , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT //QUALITY
                                        , (iAVG == 0) ? cntBasePeriod * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
                                        , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                    }
                    );
                }
                else
                    ; // по идентификатору найден не единственный парпметр расчета
            }

            if ((tableInValues.Columns.Count > 0)
                && (tableInValues.Rows.Count > 0))
            {
                // подготовить содержание запроса при вставке значений, идентифицирующих новую сессию
                strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @" ("
                    + @"[ID_CALCULATE]"
                    + @", [ID_TASK]"
                    + @", [ID_USER]"
                    + @", [ID_TIME]"
                    + @", [ID_TIMEZONE]"
                    + @", [DATETIME_BEGIN]"
                    + @", [DATETIME_END]) VALUES ("
                    ;

                strQuery += idSession;
                strQuery += @"," + (int)ID_TASK.TEP;
                strQuery += @"," + HTepUsers.Id;
                strQuery += @"," + (int)idPeriod;
                strQuery += @"," + (int)idTimezone;
                strQuery += @",'" + dtRange.Begin.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"
                strQuery += @",'" + dtRange.End.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"

                strQuery += @")";

                //Вставить в таблицу БД новый идентификтор сессии
                DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);

                // подготовить содержание запроса при вставке значений во временную таблицу для расчета
                strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

                arTypeColumns = new Type[tableInValues.Columns.Count];
                arNameColumns = new string[tableInValues.Columns.Count];
                foreach (DataColumn c in tableInValues.Columns)
                {
                    arTypeColumns[c.Ordinal] = c.DataType;
                    if (c.ColumnName.Equals(@"ID") == true)
                        strNameColumn = @"ID_INPUT";
                    else
                        strNameColumn = c.ColumnName;
                    arNameColumns[c.Ordinal] = strNameColumn;
                    strQuery += strNameColumn + @",";
                }
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);

                strQuery += @") VALUES ";

                foreach (DataRow r in tableInValues.Rows)
                {
                    strQuery += @"(";

                    foreach (DataColumn c in tableInValues.Columns)
                        strQuery += DbTSQLInterface.ValueToQuery(r[c.Ordinal], arTypeColumns[c.Ordinal]) + @",";

                    // исключить лишнюю запятую
                    strQuery = strQuery.Substring(0, strQuery.Length - 1);

                    strQuery += @"),";
                }
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);
                //Вставить во временную таблицу в БД входные для расчета значения
                DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
                // получить входные для расчета значения для возможности редактирования
                tableInValues = DbTSQLInterface.Select(ref _dbConnection, @"SELECT [ID_INPUT] as [ID],[ID_SOURCE],[ID_SESSION],[QUALITY],[VALUE],[WR_DATETIME] FROM [inval] WHERE [ID_SESSION]=" + idSession, null, null, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public void DeleteSession(int idSession, out int err)
        {
            err = -1;

            int iRegDbConn = -1;
            string strQuery = string.Empty;

            if (idSession > 0)
            {
                RegisterDbConnection(out iRegDbConn);

                if (!(iRegDbConn < 0))
                {
                    strQuery = @"DELETE FROM [dbo].[" + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @"]"
                        + @" WHERE [ID_CALCULATE]=" + idSession;

                    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
                }
                else
                    ;

                if (!(iRegDbConn > 0))
                {
                    UnRegisterDbConnection ();
                }
                else
                    ;
            }
            else
                ;
        }

        private string querySession { get { return @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @"] WHERE [ID_USER]=" + HTepUsers.Id; } }

        private string getQueryValues (long idSession, INDEX_DBTABLE_NAME indxDbTableName)
        {
            return @"SELECT * FROM " + s_NameDbTables[(int)indxDbTableName] + @" WHERE [IS_SESSION]=" + (int)idSession;
        }

        /// <summary>
        /// Строка - условие для TSQL-запроса для указания диапазона идентификаторов
        ///  выходных параметров алгоритма расчета
        /// </summary>
        private string getWhereRangeOutPut(TYPE type)
        {
            string strRes = string.Empty;

            ID_START_RECORD idRecStart = ID_START_RECORD.ALG
                , idRecEnd = ID_START_RECORD.PUT;

            switch (type)
            {
                case TYPE.IN_VALUES:
                    break;
                case TYPE.OUT_TEP_NORM_VALUES:
                case TYPE.OUT_VALUES:
                    idRecStart = type == TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                        type == TYPE.OUT_VALUES ? ID_START_RECORD.ALG :
                            ID_START_RECORD.ALG;
                    idRecEnd = type == TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.PUT :
                        type == TYPE.OUT_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                            ID_START_RECORD.PUT;

                    strRes = @"[ID] BETWEEN " + (int)(idRecStart - 1) + @" AND " + (int)(idRecEnd - 1);
                    break;
                default:
                    break;
            }
            //

            return strRes;
        }

        public string GetQueryParameters(TYPE type)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;
            
            whereParameters = getWhereRangeOutPut (type);
            if (whereParameters.Equals(string.Empty) == false)
                whereParameters = @" AND a." + whereParameters;
            else
                ;
            
            strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                    + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                    + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                + @" FROM [dbo].[" + GetNameDbTable (type, TABLE_CALCULATE_REQUIRED.PUT) + @"] as p"
                    + @" JOIN [dbo].[" + GetNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] as a ON a.ID_TASK = 1 AND a.ID = p.ID_ALG" + whereParameters
                    + @" JOIN [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MEASURE] + @"] as m ON a.ID_MEASURE = m.ID";

            return strRes;
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
        /// <summary>
        /// Возвратить наименование таблицы 
        /// </summary>
        /// <param name="type">Тип панели/расчета</param>
        /// <param name="req">Индекс таблицы, требуемой при расчете</param>
        /// <returns>Наименование таблицы</returns>
        public static string GetNameDbTable(TYPE type, TABLE_CALCULATE_REQUIRED req)
        {
            INDEX_DBTABLE_NAME indx = INDEX_DBTABLE_NAME.UNKNOWN;

            switch (type)
            {
                case TYPE.IN_VALUES:
                    switch (req)
                    {
                        case TABLE_CALCULATE_REQUIRED.ALG:
                            indx = INDEX_DBTABLE_NAME.INALG;
                            break;
                        case TABLE_CALCULATE_REQUIRED.PUT:
                            indx = INDEX_DBTABLE_NAME.INPUT;
                            break;
                        case TABLE_CALCULATE_REQUIRED.VALUE:
                            indx = INDEX_DBTABLE_NAME.INVALUES;
                            break;
                        default:
                            break;
                    }
                    break;
                case TYPE.OUT_TEP_NORM_VALUES:
                case TYPE.OUT_VALUES:
                    switch (req)
                    {
                        case TABLE_CALCULATE_REQUIRED.ALG:
                            indx = INDEX_DBTABLE_NAME.OUTALG;
                            break;
                        case TABLE_CALCULATE_REQUIRED.PUT:
                            indx = INDEX_DBTABLE_NAME.OUTPUT;
                            break;
                        case TABLE_CALCULATE_REQUIRED.VALUE:
                            indx = INDEX_DBTABLE_NAME.OUTVALUES;
                            break;
                        default:
                            break;
                    }
                    break;
                case TYPE.OUT_TEP_REALTIME:
                    switch (req)
                    {
                        case TABLE_CALCULATE_REQUIRED.ALG:
                            indx = INDEX_DBTABLE_NAME.INALG;
                            break;
                        case TABLE_CALCULATE_REQUIRED.PUT:
                            indx = INDEX_DBTABLE_NAME.INPUT;
                            break;
                        case TABLE_CALCULATE_REQUIRED.VALUE:
                            indx = INDEX_DBTABLE_NAME.INVALUES;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return s_NameDbTables[(int)indx];
        }

        //private string getQueryOutNormativeValues { get { return @""; } }

        //private string queryOutMaketValues { get { return @""; } }
        /// <summary>
        /// Запрос для получения значений "по умолчанию"
        /// </summary>
        private string getQueryValuesDef(ID_PERIOD idPeriod)
        {
            string strRes = string.Empty;

            strRes = @"SELECT"
                + @" *"
                + @" FROM [dbo].[" + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVAL_DEF] + @"] v"
                + @" WHERE [ID_TIME] = " + (int)idPeriod //(int)_currIdPeriod
                    ;

            return strRes;
        }        
        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        /// </summary>
        private string getQueryValuesVar(int idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , string strNameTableAlg
            , string strNameTablePut
            , string strNameTableValues
            , DateTimeRange[] arQueryRanges)
        {
            string strRes = string.Empty;

            int i = -1;
            bool bLastItem = false
                , bEquDatetime = false;

            for (i = 0; i < arQueryRanges.Length; i++)
            {
                bLastItem = !(i < (arQueryRanges.Length - 1));

                strRes += @"SELECT v.ID_INPUT, v.ID_TIME, v.ID_TIMEZONE"
                        + @", v.ID_SOURCE"
                        + @", " + idSession + @" as [ID_SESSION], v.QUALITY"
                        + @", [VALUE]"
                    //+ @", GETDATE () as [WR_DATETIME]"
                    + @" FROM [dbo].[" + strNameTableValues + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                    + @" WHERE v.[ID_TIME] = " + (int)idPeriod //???ID_PERIOD.HOUR //??? _currIdPeriod
                    ;
                // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                // 'Begin' == 'End'
                if (bLastItem == true)
                    bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);
                else
                    ;

                if (bEquDatetime == false)
                    strRes += @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                        + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";
                else
                    strRes += @" AND [DATE_TIME] = '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                if (bLastItem == false)
                    strRes += @" UNION ";
                else
                    ;
            }

            strRes = @"SELECT p.ID"
                //+ @", " + HTepUsers.Id + @" as [ID_USER]"
                    + @", v.ID_SOURCE"
                    + @", " + idSession + @" as [ID_SESSION]"
                    + @", CASE"
                        + @" WHEN COUNT (*) = " + cntBasePeriod + @" THEN MIN(v.[QUALITY])"
                        + @" WHEN COUNT (*) = 0 THEN " + (int)ID_QUALITY_VALUE.NOT_REC
                            + @" ELSE " + (int)ID_QUALITY_VALUE.PARTIAL
                        + @" END as [QUALITY]"
                    + @", CASE"
                        + @" WHEN m.[AVG] = 0 THEN SUM (v.[VALUE])"
                        + @" WHEN m.[AVG] = 1 THEN AVG (v.[VALUE])"
                            + @" ELSE MIN (v.[VALUE])"
                        + @" END as [VALUE]"
                    + @", GETDATE () as [WR_DATETIME]"
                + @" FROM (" + strRes + @") as v"
                    + @" LEFT JOIN [dbo].[" + strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
                    + @" LEFT JOIN [dbo].[" + strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
                    + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                + @" GROUP BY v.ID_INPUT, v.ID_SOURCE, v.ID_TIME, v.ID_TIMEZONE, v.QUALITY"
                    + @", a.ID_MEASURE, a.N_ALG"
                    + @", p.ID, p.ID_ALG, p.ID_COMP, p.MAXVALUE, p.MINVALUE"
                    + @", m.[AVG]"
                ;

            return strRes;
        }

        public DataTable GetValuesDef(ID_PERIOD idPeriod, out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection, getQueryValuesDef(idPeriod), null, null, out err);

            return tableRes;
        }

        public DataTable GetValuesVar(int idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , string strNameTableAlg
            , string strNameTablePut
            , string strNameTableValues
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryValuesVar(idSession
                    , idPeriod
                    , cntBasePeriod
                    , strNameTableAlg
                    , strNameTablePut
                    , strNameTableValues
                    , arQueryRanges)
                , null, null
                , out err);

            return tableRes;
        }

        public long GetIdSession(out int err)
        {
            err = -1;
            long iRes = -1;

            DataTable tableSession = null;
            int iRegDbConn = -1
                , iCntSession = -1;

            RegisterDbConnection(out iRegDbConn);

            if (! (iRegDbConn < 0))
            {
                // прочитать параметры сессии для текущего пользователя
                tableSession = DbTSQLInterface.Select(ref _dbConnection, querySession, null, null, out err);
                // получить количество зарегистрированных сессий для пользователя
                iCntSession = tableSession.Rows.Count;

                if ((err == 0)
                    && (iCntSession == 1))
                    iRes = (long)tableSession.Rows[0][@"ID_CALCULATE"];
                else
                    if (err == 0)
                        switch (iCntSession)
                        {
                            case 0:
                                err = -101;
                                break;
                            default:
                                err = -102;
                                break;
                        }
                    else
                        ; // ошибка получения параметров сессии
            }
            else
                ;

            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
            
            return iRes;
        }
        /// <summary>
        /// Подготовить таблицы для проведения расчета
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении функции</param>
        /// <returns>Массив таблиц со значенями для расчета</returns>
        private TaskTepCalculate.DATATABLE[] prepareTepCalculateValues(TYPE type, out int err)
        {
            List <TaskTepCalculate.DATATABLE> listRes = new List<TaskTepCalculate.DATATABLE> ();
            err = -1;

            long idSession = -1;
            DataTable tableVal = null;

            if (isRegisterDbConnection == true)
            {
                // получить количество зарегистрированных сессий для пользователя
                idSession = GetIdSession (out err);
                if (err == 0)
                {
                    // прочитать входные значения для сессии
                    tableVal = DbTSQLInterface.Select(ref _dbConnection, getQueryValues(idSession, INDEX_DBTABLE_NAME.INVALUES), null, null, out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.IN_VALUES, m_table = tableVal.Copy() });
                    // прочитать выходные-нормативы значения для сессии
                    tableVal = DbTSQLInterface.Select(ref _dbConnection, getQueryValues(idSession, INDEX_DBTABLE_NAME.OUTVALUES), null, null, out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_NORM_VALUES, m_table = tableVal.Copy() });
                }
                else
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::prepareTepCalculateValues () - при получении идентифкатора сессии расчета...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                ; // ошибка при регистрации соединения с БД

            return listRes.ToArray<TaskTepCalculate.DATATABLE> ();
        }

        public void TepCalculateNormative()
        {
            int err = -1
                , iRegDbConn = -1;

            TaskTepCalculate.DATATABLE[] arDataTables = null;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // подготовить таблицы для расчета
                arDataTables = prepareTepCalculateValues(TYPE.OUT_TEP_NORM_VALUES, out err);
                if (err == 0)
                {
                    // произвести вычисления
                    m_taskTepCalculate.CalculateNormative(arDataTables);
                    // сохранить результаты вычисления
                    saveResult(out err);
                }
                else
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::TepCalculateNormative () - при подготовке данных для расчета...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::TepCalculateNormative () - при регистрации соединения...", Logging.INDEX_MESSAGE.NOT_SET);

            // отмена регистрации БД - только, если регистрация произведена в текущем контексте
            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }

        public void TepCalculateMaket()
        {
            int err = -1
                , iRegDbConn = -1;

            TaskTepCalculate.DATATABLE[] arDataTables = null;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // подготовить таблицы для расчета
                arDataTables = prepareTepCalculateValues(TYPE.OUT_VALUES, out err);
                // произвести вычисления
                m_taskTepCalculate.CalculateMaket(arDataTables);
                // сохранить результаты вычисления
                saveResult(out err);
            }
            else
                ;

            // отмена регистрации БД - только, если регистрация произведена в текущем контексте
            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }

        private void saveResult (out int err)
        {
            err = -1;

            DbTSQLInterface.ExecNonQuery(ref _dbConnection, @"", null, null, out err);
        }
    }
}
