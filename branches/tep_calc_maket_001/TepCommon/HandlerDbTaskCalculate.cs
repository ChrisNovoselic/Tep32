using System;
using System.Globalization;
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
    public partial class HandlerDbTaskCalculate : HandlerDbValues
    {
        /// <summary>
        /// Перечисление - индексы таблиц для значений
        ///  , собранных в автоматическом режиме
        ///  , "по умолчанию"
        /// </summary>
        public enum INDEX_TABLE_VALUES : int
        {
            UNKNOWN = -1, ARCHIVE, SESSION, DEFAULT
                , COUNT
        }

        public enum TABLE_CALCULATE_REQUIRED : short { UNKNOWN = -1, ALG, PUT, VALUE
            , COUNT }        
        /// <summary>
        /// Перечисление - идентификаторы состояния полученных из БД значений
        /// </summary>
        public enum ID_QUALITY_VALUE { NOT_REC = -3, PARTIAL, DEFAULT, SOURCE, USER, CALCULATED }

        private const int MAX_ROWCOUNT_TO_INSERT = 666;

        private ID_TASK _iIdTask;
        /// <summary>
        /// Идентификатор задачи
        /// </summary>
        public ID_TASK IdTask { get { return _iIdTask; } set { if (!(_iIdTask == value)) { _iIdTask = value; createTaskCalculate(); } else ; } }
        /// <summary>
        /// Объект для произведения расчетов
        /// </summary>
        private TaskCalculate m_taskCalculate;

        public HandlerDbTaskCalculate(ID_TASK idTask = ID_TASK.UNKNOWN)
            : base()
        {
            IdTask = idTask;
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        private void createTaskCalculate (/*ID_TASK idTask*/)
        {
            if (!(m_taskCalculate == null))
                m_taskCalculate = null;
            else
                ;

            //if (!(type == TaskCalculate.TYPE.UNKNOWN))
                switch (IdTask)
                {
                    case ID_TASK.TEP:
                        m_taskCalculate = new TaskTepCalculate();
                        break;
                    default:
                        break;
                }
            //else ;
        }
        /// <summary>
        /// Создать новую сессию для расчета
        ///  - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="idSession">Идентификатор сессии</param>
        /// <param name="idPeriod">Идентификатор периода расчета</param>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="idTimezone">Идентификатор часового пояса</param>
        /// <param name="tablePars">Таблица характеристик входных параметров</param>
        /// <param name="tableSessionValues">Таблица значений входных параметров</param>
        /// <param name="tableDefValues">Таблица значений по умолчанию входных параметров</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        public void CreateSession(int idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , ID_TIMEZONE idTimezone
            , DataTable tablePars
            , ref DataTable [] arTableValues
            , DateTimeRange dtRange
            , out int err, out string strErr)
        {            
            err = 0;
            strErr = string.Empty;

            int iAVG = -1;
            string strQuery = string.Empty;            
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsSel = null;

            // удалить строки из таблицы со значениями "по умолчанию"
            foreach (DataRow rValVar in arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows)
            {
                rowsSel = arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_PUT=" + rValVar[@"ID"]);
                foreach (DataRow rToRemove in rowsSel)
                    arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Remove(rToRemove);
            }
            // вставить строки из таблицы со значениями "по умолчанию"
            foreach (DataRow rValDef in arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows)
            {
                rowsSel = tablePars.Select(@"ID=" + rValDef[@"ID_PUT"]);
                if (rowsSel.Length == 1)
                {
                    iAVG = (Int16)rowsSel[0][@"AVG"];

                    arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Add(new object[]
                        {
                            rValDef[@"ID_PUT"]
                            //, HUsers.Id //ID_USER
                            //, -1 //ID_SOURCE
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

            if ((arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Columns.Count > 0)
                && (arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0))
            {
                //Вситвить строку с идентификатором новой сессии
                insertIdSession(idSession, idPeriod, cntBasePeriod, idTimezone, dtRange, out err);
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertInValues(arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);
                //Вставить строки в таблицу БД со выходными значениями для расчета
                insertOutValues(idSession, out err);

                // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                // получить входные для расчета значения для возможности редактирования
                strQuery = @"SELECT [ID_PUT] as [ID], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]"
                    + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                    + @" WHERE [ID_SESSION]=" + idSession;
                arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Вставить в таблицу БД идентификатор новой сессии
        /// </summary>
        /// <param name="id">Идентификатор сессии</param>
        /// <param name="idPeriod">Идентификатор периода расчета</param>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="idTimezone">Идентификатор часового пояса</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertIdSession(int id
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , ID_TIMEZONE idTimezone
            , DateTimeRange dtRange
            , out int err)
        {
            err = -1;

            string strQuery = string.Empty;

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

            strQuery += id;
            strQuery += @"," + (int)ID_TASK.TEP;
            strQuery += @"," + HTepUsers.Id;
            strQuery += @"," + (int)idPeriod;
            strQuery += @"," + (int)idTimezone;
            strQuery += @",'" + dtRange.Begin.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"
            strQuery += @",'" + dtRange.End.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"

            strQuery += @")";

            //Вставить в таблицу БД строку с идентификтором новой сессии
            DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
        }
        /// <summary>
        /// Вставить значения в таблицу для временных входных значений
        /// </summary>
        /// <param name="tableInValues">Таблица со значениями для вставки</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertInValues(DataTable tableInValues, out int err)
        {
            err = -1;

            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;

            // подготовить содержание запроса при вставке значений во временную таблицу для расчета
            strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

            arTypeColumns = new Type[tableInValues.Columns.Count];
            arNameColumns = new string[tableInValues.Columns.Count];
            foreach (DataColumn c in tableInValues.Columns)
            {
                arTypeColumns[c.Ordinal] = c.DataType;
                if (c.ColumnName.Equals(@"ID") == true)
                    strNameColumn = @"ID_PUT";
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
        }
        /// <summary>
        /// Вставить значения в таблицу для временных выходных значений сессии расчета
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        private void insertOutValues(int idSession, out int err)
        {
            err = -1;

            if (IdTask == ID_TASK.TEP)
                insertOutValues (idSession, TaskCalculate.TYPE.OUT_TEP_NORM_VALUES, out err);
            else
                ;

            if (err == 0)
                insertOutValues (idSession, TaskCalculate.TYPE.OUT_VALUES, out err);
            else
                ;
        }
        /// <summary>
        /// Вставить значения в таблицу для временных выходных значений сессии расчета
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="typeCalc">Тип расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        private void insertOutValues(int idSession, TaskCalculate.TYPE typeCalc, out int err)
        {
            err = -1;

            DataTable tableParameters = null;
            string strBaseQuery = string.Empty
                , strQuery = string.Empty;
            int iRowCounterToInsert = -1;

            strQuery = GetQueryParameters(typeCalc);
            tableParameters = Select(strQuery, out err);

            strBaseQuery =
            strQuery =
                @"INSERT INTO " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @" VALUES ";

            iRowCounterToInsert = 0;
            foreach (DataRow rPar in tableParameters.Rows)
            {
                if (iRowCounterToInsert > MAX_ROWCOUNT_TO_INSERT)
                {
                    // исключить лишнюю запятую
                    strQuery = strQuery.Substring(0, strQuery.Length - 1);
                    // вставить строки в таблицу
                    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);

                    if (!(err == 0))
                    // при ошибке - не продолжать
                        break;
                    else
                        ;

                    strQuery = strBaseQuery;
                    iRowCounterToInsert = 0;
                }
                else
                    ;

                strQuery += @"(";
                
                strQuery += idSession + @"," //ID_SEESION
                    + rPar[@"ID"] + @"," //ID_PUT
                    + 0.ToString() + @"," //QUALITY
                    + 0.ToString() + @"," //VALUE
                    + @"GETDATE()"
                    ;

                strQuery += @"),";

                iRowCounterToInsert++;
            }

            if (err == 0)
            {
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);
                // вставить строки в таблицу
                DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
            }
            else
                ; // при ошибке - не продолжать
        }
        /// <summary>
        /// Удалить запись о параметрах сессии расчета (по триггеру - все входные и выходные значения)
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        public void DeleteSession(int idSession, out int err)
        {
            err = -1;

            int iRegDbConn = -1; // признак регистрации соединения с БД
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
        /// <summary>
        /// Обновить значения во временой таблице
        /// </summary>
        /// <param name="indxDbTable">Индекс таблицы в списке таблиц БД</param>
        /// <param name="tableOriginValues">Таблица с исходными значениями</param>
        /// <param name="tableEditValues">Таблица с измененными значениями</param>
        /// <param name="err">Идентификатор ошибки при выполнении операции</param>
        public void UpdateSession(INDEX_DBTABLE_NAME indxDbTable
            , DataTable tableOriginValues
            , DataTable tableEditValues
            , out int err)
        {
            err = -1;

            RecUpdateInsertDelete(s_NameDbTables[(int)indxDbTable], @"ID, ID_SESSION", tableOriginValues, tableEditValues, out err);
        }
        /// <summary>
        /// Возвратить строку запроса для получения текущего идентификатора сессии расчета
        /// </summary>
        private string querySession { get { return @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @"] WHERE [ID_USER]=" + HTepUsers.Id; } }
        /// <summary>
        /// Возвратить строку запроса для получения
        /// </summary>
        /// <param name="idSession">Идентификатор сессии</param>
        /// <param name="type">Тип значений (входные, выходные-нормативы - только для ТЭП, выходные)</param>
        /// <returns>Строка - содержание запроса</returns>
        private string getQueryValuesVar (long idSession, TaskCalculate.TYPE type)
        {
            string strRes = string.Empty
                , strJoinValues = string.Empty;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                strJoinValues = getRangeAlg(type);
                if (strJoinValues.Equals(string.Empty) == false)
                    strJoinValues = @" JOIN [" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT AND p.ID_ALG" + strJoinValues;
                else
                    ;

                strRes = @"SELECT v.* FROM " + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @" as v"
                    + strJoinValues
                    + @" WHERE [ID_SESSION]=" + (int)idSession;
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }

        private string getRangeAlg(TaskCalculate.TYPE type)
        {
            string strRes = string.Empty;

            ID_START_RECORD idRecStart = ID_START_RECORD.ALG
                , idRecEnd = ID_START_RECORD.PUT;

            switch (type)
            {
                case TaskCalculate.TYPE.IN_VALUES:
                    break;
                case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                case TaskCalculate.TYPE.OUT_VALUES:
                    idRecStart = type == TaskCalculate.TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                        type == TaskCalculate.TYPE.OUT_VALUES ? ID_START_RECORD.ALG :
                            ID_START_RECORD.ALG;
                    idRecEnd = type == TaskCalculate.TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.PUT :
                        type == TaskCalculate.TYPE.OUT_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                            ID_START_RECORD.PUT;

                    strRes = @" BETWEEN " + (int)(idRecStart - 1) + @" AND " + (int)(idRecEnd - 1);
                    break;
                default:
                    break;
            }

            return strRes;
        }
        /// <summary>
        /// Строка - условие для TSQL-запроса для указания диапазона идентификаторов
        ///  выходных параметров алгоритма расчета
        /// </summary>
        private string getWhereRangeAlg(TaskCalculate.TYPE type, string strNameFieldId = @"ID")
        {
            string strRes = string.Empty;

            switch (type)
            {
                case TaskCalculate.TYPE.IN_VALUES:
                    break;
                case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                case TaskCalculate.TYPE.OUT_VALUES:
                    strRes = @"[" + strNameFieldId + @"]" + getRangeAlg (type);
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetQueryParameters(TaskCalculate.TYPE type/* = TaskCalculate.TYPE.UNKNOWN*/)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;

            //if (type == TaskCalculate.TYPE.UNKNOWN)
            //    type = m_taskCalculate.Type;
            //else
            //    ;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                // аналог в 'getQueryValuesVar'
                whereParameters = getWhereRangeAlg(type);
                if (whereParameters.Equals(string.Empty) == false)
                    whereParameters = @" AND a." + whereParameters;
                else
                    ;

                strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                        + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                        + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] as p"
                        + @" JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] as a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask + whereParameters
                        + @" JOIN [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MEASURE] + @"] as m ON a.ID_MEASURE = m.ID";
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::GetQueryParameters () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Возвратить наименование таблицы 
        /// </summary>
        /// <param name="type">Тип панели/расчета</param>
        /// <param name="req">Индекс таблицы, требуемой при расчете</param>
        /// <returns>Наименование таблицы</returns>
        private static string getNameDbTable(TaskCalculate.TYPE type, TABLE_CALCULATE_REQUIRED req)
        {
            INDEX_DBTABLE_NAME indx = INDEX_DBTABLE_NAME.UNKNOWN;

            indx = TaskCalculate.GetIndexNameDbTable(type, req);

            return s_NameDbTables[(int)indx];
        }
        ///// <summary>
        ///// Возвратить наименование таблицы 
        ///// </summary>
        ///// <param name="req">Индекс таблицы, требуемой при расчете</param>
        ///// <returns>Наименование таблицы</returns>
        //private string getNameDbTable(TABLE_CALCULATE_REQUIRED req)
        //{
        //    return getNameDbTable(m_taskCalculate.Type, req);
        //}
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
        ///  , структура таблицы совместима с [inval], [outval]
        /// </summary>
        private string getQueryValuesVar(TaskCalculate.TYPE type
            , long idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                // аналог в 'GetQueryParameters'
                whereParameters = getWhereRangeAlg(type);
                if (whereParameters.Equals(string.Empty) == false)
                    whereParameters = @" AND a." + whereParameters;
                else
                    ;

                int i = -1;
                bool bLastItem = false
                    , bEquDatetime = false;

                for (i = 0; i < arQueryRanges.Length; i++)
                {
                    bLastItem = !(i < (arQueryRanges.Length - 1));

                    strRes += @"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
                            + @", " + idSession + @" as [ID_SESSION]"
                            + @", m.[AVG]"
                        //+ @", GETDATE () as [WR_DATETIME]"
                        + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                            + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT"
                            + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)_iIdTask + whereParameters
                            + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
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
                        strRes += @" UNION ALL";
                    else
                        ;
                }

                strRes = @"SELECT v.ID_PUT as [ID]"
                        + @", " + idSession + @" as [ID_SESSION]"
                        + @", CASE"
                            + @" WHEN COUNT (*) = " + cntBasePeriod + @" THEN MIN(v.[QUALITY])"
                            + @" WHEN COUNT (*) = 0 THEN " + (int)ID_QUALITY_VALUE.NOT_REC
                                + @" ELSE " + (int)ID_QUALITY_VALUE.PARTIAL
                            + @" END as [QUALITY]"
                        + @", CASE"
                            + @" WHEN v.[AVG] = 0 THEN SUM (v.[VALUE])"
                            + @" WHEN v.[AVG] = 1 THEN AVG (v.[VALUE])"
                                + @" ELSE MIN (v.[VALUE])"
                            + @" END as [VALUE]"
                        + @", GETDATE () as [WR_DATETIME]"
                    + @" FROM (" + strRes + @") as v"
                    + @" GROUP BY v.ID_PUT"
                        + @", v.[AVG]"
                    ;
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значенями по умолчанию
        /// </summary>
        /// <param name="idPeriod">Идентификатор </param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Объект-таблица со значенями по умолчанию</returns>
        public DataTable GetValuesDef(ID_PERIOD idPeriod, out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection, getQueryValuesDef(idPeriod), null, null, out err);

            return tableRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значениями из таблицы с временными для расчета
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="type">Тип значений (входные, выходные)</param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Объект-таблица</returns>
        public DataTable GetValuesVar(TaskCalculate.TYPE type
            , long idSession
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryValuesVar(idSession, type)
                , null, null
                , out err);

            return tableRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значениями из таблицы с сохраняемыми значениями из источников информации
        /// </summary>
        /// <param name="idSession">Идентификатор сессии - назначаемый</param>
        /// <param name="idPeriod">Идентификатор периода расчета</param>
        /// <param name="cntBasePeriod">Количество периодов расчета в интервале запрашиваемых данных</param>
        /// <param name="arQueryRanges">Массив диапазонов даты/времени - интервал(ы) заправшиваемых данных</param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Таблица со значенями</returns>
        public DataTable GetValuesVar(TaskCalculate.TYPE type
            , long idToSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryValuesVar(type
                    , idToSession
                    , idPeriod
                    , cntBasePeriod
                    , arQueryRanges)
                , null, null
                , out err);

            return tableRes;
        }
        /// <summary>
        /// Возвратить идентификатор сессии расчета
        /// </summary>
        /// <param name="err">Признак выполнении функции</param>
        /// <returns>Идентификатор рпасчета сессии</returns>
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
        private TaskTepCalculate.ListDATATABLE prepareTepCalculateValues(TaskCalculate.TYPE type, out int err)
        {
            TaskTepCalculate.ListDATATABLE listRes = new TaskTepCalculate.ListDATATABLE ();
            err = -1;

            long idSession = -1;
            DataTable tableVal = null;

            if (isRegisterDbConnection == true)
            {
                // получить количество зарегистрированных сессий для пользователя
                idSession = GetIdSession (out err);
                if (err == 0)
                {
                    // получить таблицу со значеняими нормативных графиков
                    tableVal = GetDataTable (INDEX_DBTABLE_NAME.FTABLE, out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.FTABLE, m_table = tableVal.Copy() });
                    // получить описание входных парметров в алгоритме расчета
                    tableVal = Select(GetQueryParameters(TaskCalculate.TYPE.IN_VALUES), out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.IN_PARAMETER, m_table = tableVal.Copy() });
                    // получить входные значения для сессии
                    tableVal = GetValuesVar(TaskCalculate.TYPE.IN_VALUES, idSession, out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.IN_VALUES, m_table = tableVal.Copy() });

                    if (IdTask == ID_TASK.TEP)
                    {
                        // получить описание выходных-нормативных парметров в алгоритме расчета
                        tableVal = Select(GetQueryParameters(TaskCalculate.TYPE.OUT_TEP_NORM_VALUES), out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_NORM_PARAMETER, m_table = tableVal.Copy() });
                        // получить выходные-нормативные значения для сессии
                        tableVal = GetValuesVar(TaskCalculate.TYPE.OUT_TEP_NORM_VALUES, idSession, out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_NORM_VALUES, m_table = tableVal.Copy() });
                    }
                    else
                        ;

                    if (type == TaskCalculate.TYPE.OUT_VALUES)
                    {// дополнительно получить описание выходных-нормативных параметров в алгоритме расчета
                        tableVal = Select(GetQueryParameters(TaskCalculate.TYPE.OUT_VALUES), out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_PARAMETER, m_table = tableVal.Copy() });
                        // получить выходные значения для сессии
                        tableVal = GetValuesVar(TaskCalculate.TYPE.OUT_VALUES, idSession, out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_VALUES, m_table = tableVal.Copy() });
                    }
                    else
                        ;
                    
                }
                else
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::prepareTepCalculateValues () - при получении идентифкатора сессии расчета...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                ; // ошибка при регистрации соединения с БД

            return listRes;
        }
        /// <summary>
        /// Расчитать выходные-нормативные значения для задачи "Расчет ТЭП"
        ///  , сохранить значения во временной таблице для возможности предварительного просмотра результата
        /// </summary>
        public void Calculate(TaskCalculate.TYPE type)
        {
            int err = -1
                , iRegDbConn = -1;
            DataTable tableOrigin = null
                , tableCalcRes = null;

            TaskTepCalculate.ListDATATABLE listDataTables = null;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                switch (IdTask)
                {
                    case ID_TASK.TEP:
                        // подготовить таблицы для расчета
                        listDataTables = prepareTepCalculateValues(type, out err);
                        if (err == 0)
                        {
                            // произвести вычисления
                            switch (type)
                            {
                                case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                                    tableOrigin = listDataTables.FindDataTable(TaskCalculate.INDEX_DATATABLE.OUT_NORM_VALUES);
                                    tableCalcRes = (m_taskCalculate as TaskTepCalculate).CalculateNormative(listDataTables);
                                    break;
                                case TaskCalculate.TYPE.OUT_VALUES:
                                    tableCalcRes = (m_taskCalculate as TaskTepCalculate).CalculateMaket(listDataTables);
                                    break;
                                default:
                                    break;
                            }                            
                            // сохранить результаты вычисления
                            saveResult(tableOrigin, tableCalcRes, out err);
                        }
                        else
                            Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при подготовке данных для расчета...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                    default:
                        Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - неизвестный тип задачи расчета...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при регистрации соединения...", Logging.INDEX_MESSAGE.NOT_SET);

            // отмена регистрации БД - только, если регистрация произведена в текущем контексте
            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }

        private void saveResult (DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = -1;

            DataTable tableEdit = new DataTable();
            DataRow[] rowSel = null;

            tableEdit = tableOrigin.Clone();

            foreach (DataRow r in tableOrigin.Rows)
            {
                rowSel = tableRes.Select(@"ID=" + r[@"ID_PUT"]);

                if (rowSel.Length == 1)
                {
                    tableEdit.Rows.Add(new object[] {
                        //r[@"ID"],
                        r[@"ID_SESSION"]
                        , r[@"ID_PUT"]
                        , rowSel[0][@"QUALITY"]
                        , rowSel[0][@"VALUE"]                        
                        , HDateTime.ToMoscowTimeZone ().ToString (CultureInfo.InvariantCulture)
                    });
                }
                else
                    ; //??? ошибка
            }

            RecUpdateInsertDelete(s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES], @"ID_PUT", tableOrigin, tableEdit, out err);
        }
    }
}
