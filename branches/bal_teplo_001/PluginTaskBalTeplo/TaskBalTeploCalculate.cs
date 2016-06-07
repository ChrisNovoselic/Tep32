using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskBalTeplo
{
    public class TaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate
    {
        public override string GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;

            //if (type == TaskCalculate.TYPE.UNKNOWN)
            //    type = m_taskCalculate.Type;
            //else
            //    ;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                        + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                        + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] as p"
                        + @" JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] as a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask
                        + whereParameters
                        + @" JOIN [dbo].[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.MEASURE] + @"] as m ON a.ID_MEASURE = m.ID ORDER BY ID";
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::GetQueryParameters () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();

             m_taskCalculate = new HandlerDbTaskCalculate.TaskBalTeploCalculate();
        }

        protected override void calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, out int err)
        {
            err = -1;

            DataTable tableOrigin = null
                , tableCalcRes = null;

            TepCommon.HandlerDbTaskCalculate.TaskCalculate.ListDATATABLE listDataTables = null;

            // подготовить таблицы для расчета
            listDataTables = prepareTepCalculateValues(type, out err);
            if (err == 0)
            {
                // произвести вычисления
                switch (type)
                {
                    case TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES:
                        tableCalcRes = (m_taskCalculate as HandlerDbTaskCalculate.TaskBalTeploCalculate).CalculateOut(listDataTables);
                        break;
                    default:
                        break;
                }
                // сохранить результаты вычисления
                //saveResult(tableOrigin, tableCalcRes, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при подготовке данных для расчета...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        private const int MAX_ROWCOUNT_TO_INSERT = 666;

        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        ///  , структура таблицы совместима с [inval], [outval]
        /// </summary>
        /// <param name="type"></param>
        /// <param name="idPeriod">период</param>
        /// <param name="cntBasePeriod"></param>
        /// <param name="arQueryRanges">диапазон времени запроса</param>
        /// <returns></returns>
        public override string getQueryValuesVar(TaskCalculate.TYPE type, ID_PERIOD idPeriod
            , int cntBasePeriod, DateTimeRange[] arQueryRanges)
        {
            string strRes = string.Empty
            , whereParameters = string.Empty;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                // аналог в 'GetQueryParameters'
                //whereParameters = getWhereRangeAlg(type);
                //if (whereParameters.Equals(string.Empty) == false)
                //    whereParameters = @" AND a." + whereParameters;
                //else
                //    ;

                int i = -1;
                bool bLastItem = false
                    , bEquDatetime = false;

                for (i = 0; i < arQueryRanges.Length; i++)
                {
                    bLastItem = !(i < (arQueryRanges.Length - 1));

                    strRes += @"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
                            + @", " + _Session.m_Id + @" as [ID_SESSION]"
                            + @",[DATE_TIME]"
                            + @", m.[AVG]"
                             + @", ROW_NUMBER() OVER(ORDER BY v.ID_PUT) as [EXTENDED_DEFINITION] "
                        //+ @", GETDATE () as [WR_DATETIME]"
                        + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                        + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                            + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT"
                            + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG)
                            + @"] a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask + whereParameters
                            + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                        + @" WHERE v.[ID_TIME] = " + (int)idPeriod + " AND ID_SOURCE > 0 " //???ID_PERIOD.HOUR //??? _currIdPeriod
                        ;
                    // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                    // 'Begin' == 'End'
                    if (bLastItem == true)
                        bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);
                    else
                        ;

                    if (bEquDatetime == false)
                        strRes += @" AND [DATE_TIME] >= '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                      + @" AND [DATE_TIME] < '" + arQueryRanges[i].End.AddDays(1).ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                    if (bLastItem == false)
                        strRes += @" UNION ALL ";
                    else
                        ;
                }

                strRes = " " + @" SELECT v.ID_PUT" // as [ID]"
                        + @", " + _Session.m_Id + @" as [ID_SESSION]"
                        + @", [QUALITY]"
                        + ",[VALUE]"
                         + ",[DATE_TIME] as [WR_DATETIME]"
                          + @",[EXTENDED_DEFINITION]"
                    + @" FROM (" + strRes + @") as v"
                    + @" ORDER BY  v.ID_PUT,v.DATE_TIME"
                    ;
            }
            else
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }

        /// <summary>
        /// Возвратить объект-таблицу со значенями по умолчанию
        /// </summary>
        /// <param name="idPeriod">Идентификатор </param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Объект-таблица со значенями по умолчанию</returns>
        public override DataTable GetValuesDef(ID_PERIOD idPeriod, out int err)
        {
            DataTable tableRes = new DataTable();

            string query = @"SELECT  d.[ID_PUT],d.[ID_TIME],d.[VALUE],d.[WR_ID_USER],d.[WR_DATETIME] from [TEP_NTEC_5].[dbo].[inalg] a left join [TEP_NTEC_5].[dbo].[input] i on a.id=i.ID_ALG inner join inval_def d on d.ID_PUT=i.ID where a.ID_TASK=2 and d.[ID_TIME] = " + (int)idPeriod;

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection, query, null, null, out err);

            return tableRes;
        }

        /// <summary>
        /// Получение корр. входных значений
        /// из INVAL
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arQueryRanges"></param>
        /// <param name="idPeriod">тек. период</param>
        /// <param name="err"></param>
        /// <returns>таблица занчений</returns>
        public DataTable getCorInPut(TaskCalculate.TYPE type
            , DateTimeRange[] arQueryRanges
            , ID_PERIOD idPeriod
            , out int err)
        {
            string strQuery = string.Empty;

            for (int i = 0; i < arQueryRanges.Length; i++)
            {
                strQuery = "SELECT  p.ID as ID_PUT"
                    + @", " + _Session.m_Id + @" as [ID_SESSION]"
                    + @", v.QUALITY as QUALITY, v.VALUE as VALUE"
                    + @",v.DATE_TIME as WR_DATETIME,  ROW_NUMBER() OVER(ORDER BY p.ID) as [EXTENDED_DEFINITION] "
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
                    + @" ON a.ID = p.ID_ALG"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                    + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v "
                    + @" ON v.ID_PUT = p.ID"
                    + @" WHERE  ID_TASK = " + (int)IdTask
                    + @" AND [DATE_TIME] >= '" + arQueryRanges[i].Begin.AddHours(-arQueryRanges[i].Begin.Hour).ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                    + @" AND [DATE_TIME] < '" + arQueryRanges[i].End.AddHours(-arQueryRanges[i].Begin.Hour).ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                    + @" AND v.ID_TIME = " + (int)idPeriod + " AND v.ID_SOURCE = 0";
            }
            return Select(strQuery, out err);
        }

        /// <summary>
        /// Получение плановых значений
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arQueryRanges">отрезок времени</param>
        /// <param name="idPeriod">период времени</param>
        /// <param name="err"></param>
        /// <returns>таблица значений</returns>
        public DataTable getPlanOnMonth(TaskCalculate.TYPE type
            , DateTimeRange[] arQueryRanges
            , ID_PERIOD idPeriod, out int err)
        {
            string strQuery = string.Empty;

            for (int i = 0; i < arQueryRanges.Length; i++)
            {
                strQuery = "SELECT  p.ID as ID_PUT"
                        + @", " + _Session.m_Id + @" as [ID_SESSION]"
                        + @", v.QUALITY as QUALITY, v.VALUE as VALUE"
                          + @",v.DATE_TIME as WR_DATETIME,  ROW_NUMBER() OVER(ORDER BY p.ID) as [EXTENDED_DEFINITION] "
                          + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
                           + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
                          + @" ON a.ID = p.ID_ALG"
                           + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                           + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v "
                          + @" ON v.ID_PUT = p.ID"
                          + @" WHERE  ID_TASK = " + (int)IdTask
                          + @" AND [DATE_TIME] >= '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                          + @" AND [DATE_TIME] < '" + arQueryRanges[i].End.AddMonths(1).ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                    + @" AND v.ID_TIME = 24";
            }

            return Select(strQuery, out err);
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

        /// <summary>
        /// Создать новую сессию для расчета
        ///  - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="tablePars">Таблица характеристик входных параметров</param>
        /// <param name="tableSessionValues">Таблица значений входных параметров</param>
        /// <param name="tableDefValues">Таблица значений по умолчанию входных параметров</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        public override void CreateSession(int cntBasePeriod
            , DataTable tablePars
            , ref DataTable[] arTableValues
            , DateTimeRange dtRange, out int err
            , out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            string strQuery = string.Empty;

            if ((arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Columns.Count > 0)
                && (arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0))
            {
                //Вставить строку с идентификатором новой сессии
                insertIdSession(cntBasePeriod, out err);
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertInValues(arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

                // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                // получить входные для расчета значения для возможности редактирования
                strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                    + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
            }
            else
            {
                if (arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Count > 0)
                {
                    //Вставить строку с идентификатором новой сессии
                    insertIdSession(cntBasePeriod, out err);
                    //Вставить строки в таблицу БД со входными значениями для расчета
                    insertDefInValues(arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

                    // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                    arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                    // получить входные для расчета значения для возможности редактирования
                    strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                        + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                        + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                    arTableValues[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
                }
                else
                    Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
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
        private void insertIdSession(
            int cntBasePeriod
            , out int err)
        {
            err = -1;

            string strQuery = string.Empty;

            // подготовить содержание запроса при вставке значений, идентифицирующих новую сессию
            strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @" ("
                + @"[ID_CALCULATE]"
                + @", [ID_TASK]"
                + @", [ID_USER]"
                + @", [ID_TIME]"
                + @", [ID_TIMEZONE]"
                + @", [DATETIME_BEGIN]"
                + @", [DATETIME_END]) VALUES ("
                ;

            strQuery += _Session.m_Id;
            strQuery += @"," + (Int32)IdTask;
            strQuery += @"," + HTepUsers.Id;
            strQuery += @"," + (int)_Session.m_currIdPeriod;
            strQuery += @"," + (int)_Session.m_currIdTimezone;
            strQuery += @",'" + _Session.m_rangeDatetime.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture)  // @"yyyyMMdd HH:mm:ss"
            strQuery += @",'" + _Session.m_rangeDatetime.End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture) ; // @"yyyyMMdd HH:mm:ss"

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
            strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

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
                {
                    strQuery += DbTSQLInterface.ValueToQuery(r[c.Ordinal], arTypeColumns[c.Ordinal]) + @",";
                }

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
        /// Вставить значения в таблицу для временных входных значений по умолчанию
        /// </summary>
        /// <param name="tableInValues">Таблица со значениями для вставки</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertDefInValues(DataTable tableInValues, out int err)
        {
            err = -1;

            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;
            string[] col_name = {"ID_SESSION","ID_PUT","QUALITY","VALUE","WR_DATETIME","EXTENDED_DEFINITION"};

            // подготовить содержание запроса при вставке значений во временную таблицу для расчета
            strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

            arTypeColumns = new Type[tableInValues.Columns.Count];
            arNameColumns = new string[tableInValues.Columns.Count];
            foreach (string c in col_name)
            {
                strNameColumn = c;
                strQuery += strNameColumn + @",";
            }
            // исключить лишнюю запятую
            strQuery = strQuery.Substring(0, strQuery.Length - 1);

            strQuery += @") VALUES ";

            foreach (DataRow r in tableInValues.Rows)
            {
                strQuery += @"(";
                strQuery += _Session.m_Id + @",";

                foreach (DataColumn c in tableInValues.Columns)
                {
                    if (c.ColumnName == col_name[1])
                    {
                        strQuery += r[c].ToString() + @",";
                        strQuery += "0" + @",";
                    }
                    if (c.ColumnName == col_name[3])
                    {
                        strQuery += r[c].ToString() + @",";
                    }
                    if (c.ColumnName == col_name[4])
                    {
                        strQuery += "'"+r[c].ToString() + @"',";
                        strQuery += "'"+DateTime.Now.ToString() + @"',";
                    }
                }

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
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        public void insertOutValues(out int err, DataTable tableRes)
        {
            err = -1;

            if (IdTask == ID_TASK.AUTOBOOK)
                insertOutValues(_Session.m_Id, TaskCalculate.TYPE.OUT_TEP_NORM_VALUES, out err, tableRes);
            else
                ;
            //if (err == 0)
            //    insertOutValues(_Session.m_Id, TaskCalculate.TYPE.OUT_VALUES, out err);
            //else
            //    ;
        }

        /// <summary>
        /// Вставить значения в таблицу для временных выходных значений сессии расчета
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="typeCalc">Тип расчета</param>
        /// <param name="tableRes">таблица с данными</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        private void insertOutValues(long idSession, TaskCalculate.TYPE typeCalc, out int err, DataTable tableRes)
        {
            err = 0;
            string strBaseQuery = string.Empty
                , strQuery = string.Empty;
            int iRowCounterToInsert = -1;

            strBaseQuery =
            strQuery =
                @"INSERT INTO " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @" VALUES ";

            if (true)
            {
                iRowCounterToInsert = 0;
                foreach (DataRow rowSel in tableRes.Rows)
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
                      + rowSel[@"ID_PUT"] + @"," //ID_PUT
                      + rowSel[@"QUALITY"] + @"," //QUALITY
                      + rowSel[@"VALUE"] + @"," + //VALUE
                    "'" + rowSel[@"WR_DATETIME"] + "',"
                      + rowSel[@"EXTENDED_DEFINITION"]
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
        }

        /// <summary>
        /// Получение корр. PUT's
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arQueryRanges"></param>
        /// <param name="idPeriod">период</param>
        /// <param name="err"></param>
        /// <returns></returns>
        public DataTable getInPut(TaskCalculate.TYPE type
            , DateTimeRange[] arQueryRanges
            , ID_PERIOD idPeriod
            , out int err)
        {
            string strQuery = string.Empty;

            for (int i = 0; i < arQueryRanges.Length; i++)
            {
                strQuery += @"SELECT DISTINCT v.ID,v.ID_PUT, v.ID_USER, v.ID_SOURCE,v.DATE_TIME, v.ID_TIME"
                    + ", v.ID_TIMEZONE,v.QUALITY,v.VALUE,v.WR_DATETIME"
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
                    + @" ON a.ID = p.ID_ALG"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                    + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v "
                    + @" ON v.ID_PUT = p.ID"
                    + @" WHERE  ID_TASK = " + (int)IdTask
                    + @" AND v.ID_TIME = " + (int)idPeriod + " AND v.ID_SOURCE = 0"
                    + @" ORDER BY ID";
            }
            return Select(strQuery, out err);
        }

        /// <summary>
        /// Вых. PUT's
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица значений</returns>
        public DataTable getOutPut(out int err)
        {
            DataTable tableParameters = null;
            string strQuery = string.Empty;

            strQuery = GetQueryParameters(TaskCalculate.TYPE.OUT_TEP_NORM_VALUES);

            return tableParameters = Select(strQuery, out err);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public DataTable OutValues(out int err)
        {
            string strQuery;
            strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"]"
                + @" WHERE [ID_SESSION]=" + _Session.m_Id;

            return Select(strQuery, out err);
        }

        /// <summary>
        /// Формирование таблицы для сохранения значений OUT
        /// </summary>
        /// <param name="tableOrigin">первичная таблица</param>
        /// <param name="tableRes">таблица с параметрами</param>
        /// <param name="err"></param>
        /// <returns>таблицу значений</returns>
        public DataTable saveResOut(DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = -1;
            DataTable tableEdit = new DataTable();
            string rowSel = null;
            tableEdit = tableOrigin.Clone();//копия структуры

            if (tableRes != null)
            {
                //foreach (DataGridViewRow r in dgvRes.Rows)
                //{
                for (int i = 0; i < tableRes.Rows.Count; i++)
                {
                    //if (r.Cells[namePut.GetValue(i).ToString()].Value != null)
                    //{
                    rowSel = tableRes.Rows[i]["ID_PUT"].ToString();

                    tableEdit.Rows.Add(new object[] 
                                {
                                    DbTSQLInterface.GetIdNext(tableEdit, out err)
                                    ,rowSel
                                    ,HUsers.Id.ToString()
                                    , 0.ToString()
                                    ,Convert.ToDateTime(tableRes.Rows[i]["WR_DATETIME"].ToString()).AddDays(1).ToString(CultureInfo.InvariantCulture)
                                    , ID_PERIOD.DAY
                                    , ID_TIMEZONE.NSK
                                    , 1.ToString()
                                    , tableRes.Rows[i]["VALUE"]               
                                    , DateTime.Now
                                });
                    //}
                }
                //}
            }
            else ;

            return tableEdit;
        }

        /// <summary>
        /// Формирование таблицы для сохранения значений IN
        /// </summary>
        /// <param name="tableOrigin">первичная таблица</param>
        /// <param name="tableRes">таблица с параметрами</param>
        /// <param name="err"></param>
        /// <returns>таблицу значений</returns>
        public DataTable saveResInval(DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = -1;
            DataTable tableEdit = new DataTable();
            string rowSel = null;
            tableEdit = tableOrigin.Clone();//копия структуры

            if (tableRes != null)
            {
                //foreach (DataGridViewRow r in dgvRes.Rows)
                //{
                for (int i = 0; i < tableRes.Rows.Count; i++)
                {
                    rowSel = tableRes.Rows[i]["ID_PUT"].ToString();

                    tableEdit.Rows.Add(new object[] 
                                {
                                    DbTSQLInterface.GetIdNext(tableEdit, out err)
                                    , rowSel
                                    , HUsers.Id.ToString()
                                    , 0.ToString()
                                    , Convert.ToDateTime(tableRes.Rows[i]["WR_DATETIME"].ToString()).ToString(CultureInfo.InvariantCulture)
                                    , ID_PERIOD.DAY
                                    , ID_TIMEZONE.NSK
                                    , 1.ToString()
                                    , tableRes.Rows[i]["VALUE"]            
                                    , DateTime.Now
                                });
                }
                //}
            }
            return tableEdit;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IdTab"></param>
        /// <returns></returns>
        public DataTable GetProfilesContext(int IdTab)
        {
            string query = string.Empty;
            int err = -1;

            query = @"SELECT VALUE,ID_CONTEXT"
                + @" FROM [TEP_NTEC_5].[dbo].[profiles]"
                + @" WHERE ID_TAB = " + IdTab
                + " AND ID_EXT = " + HUsers.Id;

            return Select(query, out err);
        }

        public override string GetQueryCompList()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.COMP_LIST] + @"]";

            return strRes;
        }

        public string GetQueryNAlgList()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM "+s_NameDbTables[(int)INDEX_DBTABLE_NAME.INALG]+" where ID_TASK=2";

            return strRes;
        }

        public string GetQueryNAlgOutList()
        {
            string strRes = string.Empty;

            strRes = @"SELECT * FROM " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTALG] + " where ID_TASK=2";

            return strRes;
        }
    }

    public partial class HandlerDbTaskCalculate : TepCommon.HandlerDbValues
    {
        
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public partial class TaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Признак расчета ТЭП-оперативно
            /// </summary>
            protected /*override */bool isRealTime { get { return !(m_indxCompRealTime == INDX_COMP.UNKNOWN); } }
            //protected virtual bool isRealTime { get { return _type == TYPE.OUT_TEP_REALTIME; } }

            private bool isRealTimeBL1456
            {
                get
                {
                    return (m_indxCompRealTime == INDX_COMP.iBL1)
                        || (m_indxCompRealTime == INDX_COMP.iBL4)
                        || (m_indxCompRealTime == INDX_COMP.iBL5)
                        || (m_indxCompRealTime == INDX_COMP.iBL6);
                }
            }
            /// <summary>
            /// ???
            /// </summary>
            int n_blokov
                , n_blokov1;
            /// <summary>
            /// Перечисления индексы для массива идентификаторов компонентов оборудования ТЭЦ
            /// </summary>
            private enum INDX_COMP : short
            {
                UNKNOWN = -1
                    , iBL1, iBL2, iBL3, iBL4, iBL5, iBL6,
                iOP1,iOP2,iOP3,iOP4,iOP5,iOP6,
                iPP1,iPP2,iPP3,iPP4,iPP5,iPP6,iPP7,iPP8,
                iST
                    , COUNT
            };
            /// <summary>
            /// Константы - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private const int BL1 = 1029
                , BL2 = 1030
                , BL3 = 1031
                , BL4 = 1032
                , BL5 = 1033
                , BL6 = 1034
                , OP1 = 2001
                , OP2 = 2002
                , OP3 = 2003
                , OP4 = 2004
                , OP5 = 2005
                , OP6 = 2006
                , PP1 = 3001
                , PP2 = 3002
                , PP3 = 3003
                , PP4 = 3004
                , PP5 = 3005
                , PP6 = 3006
                , PP7 = 3007
                , PP8 = 3008
                    , ST = 5;
            /// <summary>
            /// Массив - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private readonly int[] ID_COMP =
            {
                BL1, BL2, BL3, BL4, BL5, BL6
                ,OP1,OP2,OP3,OP4,OP5,OP6
                ,PP1,PP2,PP3,PP4,PP5,PP6,PP7,PP8
                    , ST
            };
            /// <summary>
            /// Индекс целевого компонента ТЭЦ при расчете ТЭП-оперативно
            /// </summary>
            private INDX_COMP m_indxCompRealTime;
            /// <summary>
            /// Объект, обеспечивающий вычисление нормативных значений при работе оборудования ТЭЦ
            /// </summary>
            private FTable fTable;
            /// <summary>
            /// Словарь с расчетными НОРМативными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            private P_ALG Norm;
            /// <summary>
            /// Перечисление - режимы работы оборудования
            /// </summary>
            private enum MODE_DEV : short
            {
                UNKNOWN = -1, COND_1 = 1, ELEKTRO2_2, ELEKTRO1_2a,
                TEPLO_3
                    , COUNT
            }
            /// <summary>
            /// Словарь - режимы работы для компонентов станции
            /// </summary>
            private Dictionary<int, MODE_DEV> _modeDev;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskBalTeploCalculate()
                : base()
            {
                m_indxCompRealTime = INDX_COMP.UNKNOWN;

                In = new P_ALG();
                Norm = new P_ALG();
                Out = new P_ALG();

                fTable = new FTable();
            }

            protected override int initValues(ListDATATABLE listDataTables)
            {
                int iRes = -1;

                // инициализация нормативных значений для оборудования
                fTable.Set(listDataTables.FindDataTable(INDEX_DATATABLE.FTABLE));
                // инициализация входных значений
                iRes = initInValues(listDataTables.FindDataTable(INDEX_DATATABLE.IN_PARAMETER)
                    , listDataTables.FindDataTable(INDEX_DATATABLE.IN_VALUES));

                return iRes;
            }

            private int initInValues(DataTable tablePar, DataTable tableVal)
            {
                int iRes = 0;

                MODE_DEV mDev = MODE_DEV.UNKNOWN;

                iRes = initValues(In, tablePar, tableVal);

                if (In.ContainsKey(@"74") == true)
                {
                    _modeDev = new Dictionary<int, MODE_DEV>();

                    for (int i = (int)INDX_COMP.iBL1; (i < (int)INDX_COMP.COUNT) && (iRes == 0); i++)
                    {
                        switch ((int)In[@"74"][ID_COMP[i]].value)
                        {
                            case 1: //[MODE_DEV].1 - Конденсационный
                                mDev = MODE_DEV.COND_1;
                                break;
                            case 2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                mDev = MODE_DEV.ELEKTRO2_2;
                                break;
                            case 3: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                mDev = MODE_DEV.ELEKTRO1_2a;
                                break;
                            case 4: //[MODE_DEV].3 - По тепл. граф.
                                mDev = MODE_DEV.TEPLO_3;
                                break;
                            default:
                                iRes = -1;

                                //logErrorUnknownModeDev(@"InitInValues", ID_COMP[i]);
                                break;
                        }

                        if ((_modeDev.ContainsKey(i) == false)
                            && (!(mDev == MODE_DEV.UNKNOWN)))
                            _modeDev.Add(i, mDev);
                        else
                            ;
                    }
                }
                else
                {
                    iRes = -1;

                    Logging.Logg().Error(@"TaskTepCalculate::initInValues () - во входной таблице не установлен режим оборудования...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return iRes;
            }

            private int initOutValues(DataTable tablePar, DataTable tableVal)
            {
                int iRes = -1;

                iRes = initValues(Out, tablePar, tableVal);

                return iRes;
            }

            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateOut(ListDATATABLE listDataTables)
            {
                int iInitValuesRes = -1;

                DataTable tableRes = null;

                iInitValuesRes = initValues(listDataTables);

                if (iInitValuesRes == 0)
                {
                    //// расчет
                    //foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Norm)
                    //    pAlg.Value[ST].value = calculateNormative(pAlg.Key);

                    //foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Out)
                    //    pAlg.Value[ST].value = calculateMaket(pAlg.Key);

                    //// преобразование в таблицу
                    //tableRes = resultToTable(Out);
                }
                else
                    ; // ошибка при инициализации параметров, значений

                return tableRes;
            }

            private int calculateOut()
            {
                int iRes = 0;

                return iRes;
            }

            private DataTable resultToTable(P_ALG pAlg)
            {
                DataTable tableRes = new DataTable();

                tableRes.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"ID", typeof(int))
                    , new DataColumn (@"QUALITY", typeof(short))
                    , new DataColumn (@"VALUE", typeof(float))                    
                });

                foreach (P_ALG.P_PUT pPut in pAlg.Values)
                    foreach (P_ALG.P_PUT.P_VAL val in pPut.Values)
                        tableRes.Rows.Add(new object[]
                            {
                                val.m_iId //ID_PUT
                                , val.m_sQuality //QUALITY
                                , val.value //VALUE
                            });

                return tableRes;
            }
        }
    }
}
