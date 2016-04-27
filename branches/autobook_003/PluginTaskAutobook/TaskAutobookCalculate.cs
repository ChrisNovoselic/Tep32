using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Windows.Forms; //???

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskAutobook
{
    /// <summary>
    /// DayAutoBook
    /// </summary>
    public class HandlerDbTaskAutobookMonthValuesCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();

            //??? m_taskCalculate = new TaskAutobookCalculate();
        }

        protected override void calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, out int err)
        {
            err = 0;
        }

        private const int MAX_ROWCOUNT_TO_INSERT = 666;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DateTimeRange[] GetDateTimeRangeValuesVar()
        {
            DateTimeRange[] arRangesRes = null;
            int i = -1;
            bool bEndMonthBoudary = false;

            DateTime dtBegin = _Session.m_rangeDatetime.Begin.AddDays(-_Session.m_rangeDatetime.Begin.Day).AddMinutes(-1 * _Session.m_curOffsetUTC)
                , dtEnd = _Session.m_rangeDatetime.End.AddMinutes(-1 * _Session.m_curOffsetUTC).AddDays(-1);

            arRangesRes = new DateTimeRange[(dtEnd.Month - dtBegin.Month) + 12 * (dtEnd.Year - dtBegin.Year) + 1];
            bEndMonthBoudary = HDateTime.IsMonthBoundary(dtEnd);
            if (bEndMonthBoudary == false)
                if (arRangesRes.Length == 1)
                    // самый простой вариант - один элемент в массиве - одна таблица
                    arRangesRes[0] = new DateTimeRange(dtBegin, dtEnd);
                else
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц
                    for (i = 0; i < arRangesRes.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                        else
                            if (i == arRangesRes.Length - 1)
                                // крайний элемент массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                            else
                                // для элементов в "середине" массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
            else
                if (bEndMonthBoudary == true)
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц ('diffMonth' всегда > 0)
                    // + использование следующей за 'dtEnd' таблицы
                    for (i = 0; i < arRangesRes.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                        else
                            if (i == arRangesRes.Length - 1)
                                // крайний элемент массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                            else
                                // для элементов в "середине" массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
                else
                    ;

            return arRangesRes;
        }

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
                            + @", [DATE_TIME]"
                            + @", m.[AVG]"
                            + @", CONVERT(varchar, [DATE_TIME], 112) as [EXTENDED_DEFINITION] "
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
                        strRes += @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                      + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                    if (bLastItem == false)
                        strRes += @" UNION ALL ";
                    else
                        ;
                }

                strRes = " " + @"SELECT v.ID_PUT" // as [ID]"
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
                        + @",v.DATE_TIME as WR_DATETIME, ROW_NUMBER() OVER(ORDER BY p.ID) as [EXTENDED_DEFINITION]"
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
        ///  Создать новую сессию для расчета
        /// - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="tablePars">Таблица характеристик входных параметров</param>
        /// <param name="arTableValues"></param>
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
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
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
                        , rowSel
                        , HUsers.Id.ToString()
                        , 0.ToString()
                        , Convert.ToDateTime(tableRes.Rows[i]["WR_DATETIME"].ToString()).AddDays(1).ToString(CultureInfo.InvariantCulture)
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
    }

    /// <summary>
    /// PlanAutoBook
    /// </summary>
    public class HandlerDbTaskAutobookYarlyPlanCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();

            //??? m_taskCalculate = new TaskAutobookCalculate();
        }

        protected override void calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, out int err)
        {
            err = 0;
        }

        private const int MAX_ROWCOUNT_TO_INSERT = 666;

        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        ///  , структура таблицы совместима с [inval], [outval]
        /// </summary>
        /// <param name="type"></param>
        /// <param name="idPeriod">ид периода</param>
        /// <param name="cntBasePeriod">период</param>
        /// <param name="arQueryRanges">диапазон времени запроса</param>
        /// <returns></returns>
        public override string getQueryValuesVar(TaskCalculate.TYPE type
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges)
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
                    if (arQueryRanges[i].Begin < DateTime.Now)
                    {
                        bLastItem = !(i < (arQueryRanges.Length - 1));

                        strRes += @"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
                                + @", " + _Session.m_Id + @" as [ID_SESSION]"
                                + @",[DATE_TIME]"
                                + @", m.[AVG]"
                                + @", [EXTENDED_DEFINITION] = " + i
                            //+ @", GETDATE () as [WR_DATETIME]"
                            + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                                + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT"
                                + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] a ON a.ID = p.ID_ALG AND a.ID_TASK = "
                                + (int)IdTask + whereParameters
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
                            strRes += @" AND [DATE_TIME] >= '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                          + @" AND [DATE_TIME] < '" + arQueryRanges[i].End.AddDays(1).ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                        if (bLastItem == false)
                            strRes += @" UNION ALL ";
                        else
                            ;
                    }
                    else
                    {
                        // исключить лишнюю запятую
                        strRes = strRes.Substring(0, strRes.Length - (" UNION ALL ".Length - 1));
                        break;
                    }
                }

                strRes = " " + @" SELECT v.ID_PUT" // as [ID]"
                        + @", " + _Session.m_Id + @" as [ID_SESSION]"
                        + @", [QUALITY]"
                        + ",[VALUE]"
                         + ",[DATE_TIME] as [WR_DATETIME]"
                         + @",[EXTENDED_DEFINITION]"
                    + @" FROM (" + strRes + @") as v"
                    + @" ORDER BY  v.ID_PUT,v.DATE_TIME";
            }
            else
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
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
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Вставить в таблицу БД идентификатор новой сессии
        /// </summary>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertIdSession(int cntBasePeriod, out int err)
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
        /// Формирование списка отрезков времени
        /// </summary>
        /// <returns></returns>
        public override DateTimeRange[] GetDateTimeRangeValuesVar()
        {
            DateTimeRange[] arRangesRes = null;
            int i = -1,
            startMont = _Session.m_rangeDatetime.Begin.Month - 1;

            bool bEndMonthBoudary = false;

            DateTime dtBegin = _Session.m_rangeDatetime.Begin.AddMonths(-startMont)
                , dtEnd = _Session.m_rangeDatetime.End.AddDays(1).AddMonths(-1);
            arRangesRes = new DateTimeRange[(dtEnd.Month - dtBegin.Month) + 12 * (dtEnd.Year - dtBegin.Year) + 1];

            bEndMonthBoudary = HDateTime.IsMonthBoundary(dtEnd);
            if (bEndMonthBoudary == false)
                if (arRangesRes.Length == 1)
                    // самый простой вариант - один элемент в массиве - одна таблица
                    arRangesRes[0] = new DateTimeRange(dtBegin, dtEnd);
                else
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц
                    for (i = 0; i < arRangesRes.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin).AddDays(1));
                        else
                            if (i == arRangesRes.Length - 1)
                                // крайний элемент массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd.AddMonths(1));
                            else
                                // для элементов в "середине" массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End).AddDays(1));
            else
                if (bEndMonthBoudary == true)
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц ('diffMonth' всегда > 0)
                    // + использование следующей за 'dtEnd' таблицы
                    for (i = 0; i < arRangesRes.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                        else
                            if (i == arRangesRes.Length - 1)
                                // крайний элемент массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd.AddMonths(1));
                            else
                                // для элементов в "середине" массива
                                arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
                else
                    ;

            return arRangesRes;
        }

        /// <summary>
        /// Формирование списка значений 
        /// для сохранения в БД
        /// </summary>
        /// <param name="tableOrigin">таблица значений</param>
        /// <param name="rowRes">строка значений</param>
        /// <param name="err"></param>
        /// <returns>таблица значений</returns>
        public DataTable savePlanValue(DataTable tableOrigin, DataRow rowRes, out int err)
        {
            err = -1;
            double ResValue;
            DataTable tableEdit = new DataTable();
            string rowSel = null;
            tableEdit = tableOrigin.Clone();//копия структуры

            //if (tableRes != null)
            //{
            //for (int i = 0; i < tableRes.Rows.Count; i++)
            //{
            //foreach (DataGridViewRow r in dgvRes.Rows)
            //{
            //    if (r.Cells["DateTime"].Value.ToString() ==
            //        Convert.ToDateTime(tableRes.Rows[i]["WR_DATETIME"]).ToShortDateString())
            //    {
            rowSel = rowRes["ID_PUT"].ToString();
            ResValue = Convert.ToDouble(rowRes["VALUE"]);

            tableEdit.Rows.Add(new object[] 
            {
                DbTSQLInterface.GetIdNext(tableOrigin, out err)
                , rowSel
                , HUsers.Id.ToString()
                , 0.ToString()
                , Convert.ToDateTime(rowRes["WR_DATETIME"].ToString()).AddMonths(1).ToString(CultureInfo.InvariantCulture)
                , ID_PERIOD.MONTH
                , ID_TIMEZONE.NSK
                , 1.ToString()
                , ResValue         
                , DateTime.Now
            });
            //break;
            //}
            //}
            //}
            //}
            return tableEdit;
        }

        /// <summary>
        /// Получение ID_PUT
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arQueryRanges">отрезок времени</param>
        /// <param name="idPeriod">период времени</param>
        /// <param name="err"></param>
        /// <returns>таблица с put'ами</returns>
        public DataTable getPlan(TaskCalculate.TYPE type
            , DateTime arQueryDatetime
            , ID_PERIOD idPeriod, out int err)
        {
            string strQuery = string.Empty;
            int i = 0;

            strQuery = @"SELECT ID_PUT, ID_TIME,DATE_TIME"
              + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
              + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
              + @" ON a.ID = p.ID_ALG"
              + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                         + arQueryDatetime.ToString(@"yyyyMM") + @"] v "
                         + @" ON p.ID = v.ID_PUT"
                         + @" WHERE  ID_TASK = " + (int)IdTask
                         + @" AND v.ID_TIME = " + (int)idPeriod;

            return Select(strQuery, out err);
        }

        ///// <summary>
        ///// Получение вх.зн.
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="idPeriod"></param>
        ///// <param name="err"></param>
        ///// <returns>таблица значений</returns>
        //public DataTable getAllInval(TaskCalculate.TYPE type
        //    , ID_PERIOD idPeriod, out int err)
        //{
        //    string strQuery = string.Empty;
        //    int i = 0;

        //    strQuery = @"SELECT ID_PUT, ID_TIME,DATE_TIME"
        //      + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
        //      + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
        //      + @" ON a.ID = p.ID_ALG"
        //      + @" WHERE  ID_TASK = " + (int)IdTask;

        //    return Select(strQuery, out err);
        //}
    }
}
