﻿using System;
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
        public enum ID_QUALITY_VALUE { NOT_REC = -3, PARTIAL, DEFAULT, SOURCE, USER }

        private ID_TASK _iIdTask;
        /// <summary>
        /// Идентификатор задачи
        /// </summary>
        public ID_TASK IdTask { get { return _iIdTask; } set { if (!(_iIdTask == value)) { _iIdTask = value; createTaskCalculate(); } else ; } }
        /// <summary>
        /// Объект для произведения расчетов
        /// </summary>
        TaskCalculate m_taskCalculate;

        public HandlerDbTaskCalculate(ID_TASK idTask = ID_TASK.UNKNOWN)
            : base()
        {
            IdTask = idTask;
        }

        private void createTaskCalculate ()
        {
            if (!(m_taskCalculate == null))
                m_taskCalculate = null;
            else
                ;

            switch (IdTask)
            {
                case ID_TASK.TEP:
                    m_taskCalculate = new TaskTepCalculate();
                    break;
                default:
                    break;
            }
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
            , ref DataTable tableArchiveValues, ref DataTable tableSessionValues, ref DataTable tableDefValues
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
            foreach (DataRow rValVar in tableSessionValues.Rows)
            {
                rowsSel = tableDefValues.Select(@"ID_PUT=" + rValVar[@"ID"]);
                foreach (DataRow rToRemove in rowsSel)
                    tableDefValues.Rows.Remove(rToRemove);
            }
            // вставить строки из таблицы со значениями "по умолчанию"
            foreach (DataRow rValDef in tableDefValues.Rows)
            {
                rowsSel = tablePars.Select(@"ID=" + rValDef[@"ID_PUT"]);
                if (rowsSel.Length == 1)
                {
                    iAVG = (Int16)rowsSel[0][@"AVG"];

                    tableSessionValues.Rows.Add(new object[]
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

            if ((tableSessionValues.Columns.Count > 0)
                && (tableSessionValues.Rows.Count > 0))
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

                arTypeColumns = new Type[tableSessionValues.Columns.Count];
                arNameColumns = new string[tableSessionValues.Columns.Count];
                foreach (DataColumn c in tableSessionValues.Columns)
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

                foreach (DataRow r in tableSessionValues.Rows)
                {
                    strQuery += @"(";

                    foreach (DataColumn c in tableSessionValues.Columns)
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
                strQuery = @"SELECT [ID_PUT] as [ID], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]"
                    + @" FROM [inval]"
                    + @" WHERE [ID_SESSION]=" + idSession;
                tableSessionValues = DbTSQLInterface.Select(ref _dbConnection, strQuery, null, null, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Удалить запись о параметрах сессии расчета (по триггеру - все входные и выходные значения)
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="err">признак выполнения функции</param>
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
        /// Обновить значения
        /// </summary>
        /// <param name="indxDbTable"></param>
        /// <param name="tableOriginInValues"></param>
        /// <param name="tableEditInValues"></param>
        /// <param name="err"></param>
        public void UpdateSession(INDEX_DBTABLE_NAME indxDbTable
            , DataTable tableOriginInValues
            , DataTable tableEditInValues
            , out int err)
        {
            err = -1;

            RecUpdateInsertDelete(s_NameDbTables[(int)indxDbTable], @"ID, ID_SESSION", tableOriginInValues, tableEditInValues, out err);
        }

        private string querySession { get { return @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @"] WHERE [ID_USER]=" + HTepUsers.Id; } }

        private string getQueryValuesVar (long idSession, TYPE type)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;
            // аналог в 'getQueryValuesVar'
            whereParameters = getWhereRangeOutPut(type);
            if (whereParameters.Equals(string.Empty) == false)
                whereParameters = @" AND " + whereParameters;
            else
                ;

            strRes = @"SELECT * FROM " + getNameDbTable (type, TABLE_CALCULATE_REQUIRED.VALUE) + @" WHERE [ID_SESSION]=" + (int)idSession
                + whereParameters;

            return strRes;
        }
        /// <summary>
        /// Строка - условие для TSQL-запроса для указания диапазона идентификаторов
        ///  выходных параметров алгоритма расчета
        /// </summary>
        private string getWhereRangeOutPut(TYPE type, string strNameFieldId = @"ID")
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

                    strRes = @"[" + strNameFieldId + @"] BETWEEN " + (int)(idRecStart - 1) + @" AND " + (int)(idRecEnd - 1);
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
            // аналог в 'getQueryValuesVar'
            whereParameters = getWhereRangeOutPut (type);
            if (whereParameters.Equals(string.Empty) == false)
                whereParameters = @" AND a." + whereParameters;
            else
                ;
            
            strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                    + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                    + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                + @" FROM [dbo].[" + getNameDbTable (type, TABLE_CALCULATE_REQUIRED.PUT) + @"] as p"
                    + @" JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] as a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)_iIdTask + whereParameters
                    + @" JOIN [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MEASURE] + @"] as m ON a.ID_MEASURE = m.ID";

            return strRes;
        }
        /// <summary>
        /// Возвратить наименование таблицы 
        /// </summary>
        /// <param name="type">Тип панели/расчета</param>
        /// <param name="req">Индекс таблицы, требуемой при расчете</param>
        /// <returns>Наименование таблицы</returns>
        private static string getNameDbTable(TYPE type, TABLE_CALCULATE_REQUIRED req)
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
        private string getQueryValuesVar(long idSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , TYPE type
            , DateTimeRange[] arQueryRanges)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;
            // аналог в 'GetQueryParameters'
            whereParameters = getWhereRangeOutPut(type);
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
                    + @" FROM [dbo].[" + getNameDbTable (type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
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
        public DataTable GetValuesVar(long idSession
            , TYPE type
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryValuesVar(idSession
                    , type)
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
        public DataTable GetValuesVar(long idToSession
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , TYPE type
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryValuesVar(idToSession
                    , idPeriod
                    , cntBasePeriod
                    , type
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
        private TaskTepCalculate.ListDATATABLE prepareTepCalculateValues(TYPE type, out int err)
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
                    tableVal = Select(GetQueryParameters(TYPE.IN_VALUES), out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.IN_PARAMETER, m_table = tableVal.Copy() });
                    // получить входные значения для сессии
                    tableVal = GetValuesVar(idSession, TYPE.IN_VALUES, out err);
                    listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.IN_VALUES, m_table = tableVal.Copy() });

                    if (type == TYPE.OUT_VALUES)
                    {// дополнительно получить описание выходных-нормативных параметров в алгоритме расчета
                        tableVal = Select(GetQueryParameters(TYPE.OUT_TEP_NORM_VALUES), out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_NORM_PARAMETER, m_table = tableVal.Copy() });
                        //
                        tableVal = GetValuesVar(idSession, TYPE.OUT_TEP_NORM_VALUES, out err);
                        listRes.Add(new TaskTepCalculate.DATATABLE() { m_indx = TaskTepCalculate.INDEX_DATATABLE.OUT_NORM_VALUES, m_table = tableVal.Copy() });
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
        public void TepCalculateNormative()
        {
            int err = -1
                , iRegDbConn = -1;

            TaskTepCalculate.ListDATATABLE listDataTables = null;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // подготовить таблицы для расчета
                listDataTables = prepareTepCalculateValues(TYPE.OUT_TEP_NORM_VALUES, out err);
                if (err == 0)
                {
                    // произвести вычисления
                    (m_taskCalculate as TaskTepCalculate).CalculateNormative(listDataTables);
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

            TaskTepCalculate.ListDATATABLE listDataTables = null;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // подготовить таблицы для расчета
                listDataTables = prepareTepCalculateValues(TYPE.OUT_VALUES, out err);
                // произвести вычисления
                (m_taskCalculate as TaskTepCalculate).CalculateMaket(listDataTables);
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
