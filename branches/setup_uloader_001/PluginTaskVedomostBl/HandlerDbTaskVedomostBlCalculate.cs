using HClassLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    public class HandlerDbTaskVedomostBlCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект для расчета выходных значений
        /// </summary>
        protected override void createTaskCalculate()
        {
            base.createTaskCalculate();
        }

        /// <summary>
        /// Рассчитать выходные значения
        /// </summary>
        /// <param name="type">Тип расчета</param>
        /// <param name="tableOrigin">Оригинальная таблица</param>
        /// <param name="tableCalc">Выходная таблмца с рассчитанными значениями</param>
        /// <param name="err">Признак ошибки при выполнении метода</param>
        protected override void calculate(TaskCalculate.TYPE type, out DataTable tableOrigin, out DataTable tableCalc, out int err)
        {
            tableOrigin = new DataTable();
            tableCalc = new DataTable();
            err = 0;
        }

        /// <summary>
        /// получение временного диапазона 
        /// для блоков 1,6
        /// </summary>
        /// <returns>диапазон дат</returns>
        public DateTimeRange[] GetDateTimeRangeValuesVarExtremeBL()
        {
            DateTimeRange[] arRangesRes = null;
            int i = -1;
            bool bEndMonthBoudary = false;
            DateTime dtBegin =
                _Session.m_DatetimeRange.Begin.AddHours(-(TimeZoneInfo.Local.BaseUtcOffset + TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").BaseUtcOffset).Hours - 2),
            dtEnd = _Session.m_DatetimeRange.End.AddHours(-(TimeZoneInfo.Local.BaseUtcOffset + TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").BaseUtcOffset).Hours - 2).AddDays(1);

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
                        {
                            // предыдущих значений нет
                            //arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                            arRangesRes[i] = new DateTimeRange(dtBegin, dtBegin.AddDays(1));
                        }
                        else
                            if (i == arRangesRes.Length - 1)
                            // крайний элемент массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                        else
                            // для элементов в "середине" массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End,
                               new DateTime(arRangesRes[i - 1].End.Year, arRangesRes[i - 1].End.AddMonths(1).Month, DateTime.DaysInMonth(arRangesRes[i - 1].End.Year, arRangesRes[i - 1].End.AddMonths(1).Month)));
            //HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
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

        ///// <summary>
        ///// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        /////  , структура таблицы совместима с [inval], [outval]
        ///// </summary>
        ///// <param name="type">тип задачи</param>
        ///// <param name="idPeriod">период</param>
        ///// <param name="cntBasePeriod">период(день,месяц,год)</param>
        ///// <param name="arQueryRanges">диапазон времени запроса</param>
        ///// <returns>строка запроса</returns>
        //public override string getQueryVariableValues(TaskCalculate.TYPE type, ID_PERIOD idPeriod
        //    , int cntBasePeriod, DateTimeRange[] arQueryRanges)
        //{
        //    string strRes = string.Empty
        //    , whereParameters = string.Empty;

        //    if (!(type == TaskCalculate.TYPE.UNKNOWN))
        //    {
        //        // аналог в 'GetQueryParameters'
        //        //whereParameters = getWhereRangeAlg(type);
        //        //if (whereParameters.Equals(string.Empty) == false)
        //        //    whereParameters = @" AND a." + whereParameters;
        //        //else
        //        //    ;

        //        int i = -1;
        //        bool bLastItem = false
        //            , bEquDatetime = false;

        //        for (i = 0; i < arQueryRanges.Length; i++)
        //        {
        //            bLastItem = !(i < (arQueryRanges.Length - 1));

        //            strRes += @"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
        //                    + @", " + _Session.m_Id + @" as [ID_SESSION]"
        //                    + @", [DATE_TIME]"
        //                    + @", CONVERT(varchar, [DATE_TIME], 127) as [EXTENDED_DEFINITION] "
        //                    + @"FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] a "
        //                    + @"LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p "
        //                    + @"ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask + " "
        //                    + @"LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
        //                    + arQueryRanges[i].End.ToString(@"yyyyMM") + @"] v "
        //                    + @"ON p.ID = v.ID_PUT "
        //                    + @"WHERE v.[ID_TIME] = " + (int)ID_PERIOD.DAY //+ " AND [ID_SOURCE] > 0 "
        //                    + @" AND ID_TIMEZONE = " + (int)_Session.CurrentIdTimezone
        //                    + @" AND p.ID_COMP = " + PanelTaskVedomostBl.s_delegateGetIdActiveComponent()
        //                ;
        //            // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
        //            // 'Begin' == 'End'
        //            if (bLastItem == true)
        //                bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);

        //            if (bEquDatetime == false)
        //                strRes += @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
        //              + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

        //            if (bLastItem == false)
        //                strRes += @" UNION ALL ";
        //        }

        //        strRes = "" + @"SELECT v.ID_PUT"
        //            + @", " + _Session.m_Id + @" as [ID_SESSION]"
        //            + @", [QUALITY]"
        //            + @", [VALUE]"
        //            + @", [DATE_TIME] as [WR_DATETIME]"
        //            + @", [EXTENDED_DEFINITION] "
        //            + @"FROM (" + strRes + @") as v "
        //            + @"ORDER BY v.ID_PUT,v.DATE_TIME";
        //    }
        //    else
        //        Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

        //    return strRes;
        //}

        ///// <summary>
        ///// Формирование запроса на получение
        ///// имен заголовков
        ///// </summary>
        ///// <returns>строка запроса</returns>
        //public DataTable GetHeaderDGV()
        //{
        //    string query = string.Empty;
        //    int err = -1;

        //    query = @"SELECT * "
        //        + @"FROM [inalg] a "
        //        + @"LEFT JOIN [input] p "
        //        + @"ON a.ID = p.ID_ALG "
        //        + @"WHERE a.ID_TASK = " + (int)IdTask
        //        + @" ORDER BY p.ID"
        //        ;

        //    return Select(query, out err);
        //}

        /// <summary>
        /// Формирование запроса на получение компонентов
        /// </summary>
        /// <returns>строка запроса</returns>
        public string GetQueryComp(TaskCalculate.TYPE type)
        {
            string strRes = string.Empty;

            strRes = @"SELECT l.[ID] , l.[ID_COMP], l.[DESCRIPTION] "
            + @"FROM  [" + s_dictDbTables[ID_DBTABLE.COMP_LIST].m_name + @"] l "
            + @"LEFT JOIN [comp] c "
            + @"ON c.ID = l.ID_COMP "
            + @"WHERE c.ID > 500 AND c.ID < 2000";

            return strRes;
        }

        /// <summary>
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="tableOrigin">первоначальная таблица</param>
        /// <param name="tableRes">измененная таблица</param>
        /// <param name="timezone">индентификатор таймзоны</param>
        /// <param name="err">номер ошибки</param>
        /// <returns>таблица данных</returns>
        public DataTable SaveValues(DataTable tableOrigin, DataTable tableRes, int timezone, out int err)
        {
            err = -1;
            DataTable tableEdit = new DataTable();
            DateTime dtRes;
            string rowSel = null;
            int idUser = 0
                , idSource = 0;

            tableEdit = tableOrigin.Clone();//копия структуры

            for (int i = 0; i < tableRes.Rows.Count; i++)
            {
                rowSel = tableRes.Rows[i]["ID_PUT"].ToString();
                dtRes = Convert.ToDateTime(tableRes.Rows[i]["WR_DATETIME"]);

                if (tableOrigin.Rows.Count > 0)
                {
                    for (int j = 0; j < tableOrigin.Rows.Count; j++)
                        if (rowSel == tableOrigin.Rows[j]["ID_PUT"].ToString())
                            if (dtRes.ToShortDateString() == Convert.ToDateTime(tableOrigin.Rows[j]["DATE_TIME"]).ToShortDateString())
                                if (tableOrigin.Rows[j]["Value"].ToString() == tableRes.Rows[i]["VALUE"].ToString())
                                {
                                    idUser = (int)tableOrigin.Rows[j]["ID_USER"];
                                    idSource = (int)tableOrigin.Rows[j]["ID_SOURCE"];
                                    break;
                                }
                }
                else
                {
                    idUser = HUsers.Id;
                    idSource = 0;
                }

                tableEdit.Rows.Add(new object[]
                {
                    DbTSQLInterface.GetIdNext(tableEdit, out err)
                    , rowSel
                    , idUser.ToString()
                    , idSource.ToString()
                    , dtRes.ToString(CultureInfo.InvariantCulture)
                    , ID_PERIOD.DAY
                    , timezone
                    , tableRes.Rows[i]["QUALITY"].ToString()
                    , tableRes.Rows[i]["VALUE"].ToString()
                    , DateTime.Now
                });
            }
            return tableEdit;
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableOut(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        ///// <summary>
        ///// Получение имени таблицы вх.зн. в БД
        ///// </summary>
        ///// <param name="dtInsert">дата</param>
        ///// <returns>имя таблицы</returns>
        //private string getNameTableIn(DateTime dtInsert)
        //{
        //    string strRes = string.Empty;

        //    if (dtInsert == null)
        //        throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

        //    strRes = HandlerDbValues.s_dictDbTables[ID_DBTABLE.INVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

        //    return strRes;
        //}


        ///// <summary>
        ///// Получение вых.знач.
        ///// </summary>
        ///// <param name="dtRange">диапазон временной</param>
        ///// <param name="err">Индентификатор ошибки</param>
        ///// <returns>таблица данных</returns>
        //public DataTable GetDataOutvalArch(TaskCalculate.TYPE type, DateTimeRange[] dtRange, out int err)
        //{
        //    string strQuery = string.Empty;
        //    bool bLastItem = false;

        //    for (int i = 0; i < dtRange.Length; i++)
        //    {
        //        bLastItem = !(i < (dtRange.Length - 1));

        //        strQuery += @"SELECT [ID_PUT], [QUALITY], [VALUE], [DATE_TIME] as [WR_DATETIME]"
        //            + @" , CONVERT(varchar, [DATE_TIME], 127) as [EXTENDED_DEFINITION]"
        //            + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
        //            + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
        //            + @" ON a.ID = p.ID_ALG"
        //            + @" LEFT JOIN [dbo].[outval_" + dtRange[i].End.ToString(@"yyyyMM") + @"] o"
        //            + @" ON o.ID_PUT = p.ID"
        //            + @" WHERE  [ID_TASK] = " + (int)IdTask
        //            + @" AND [DATE_TIME] > '" + dtRange[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
        //            + @" AND [DATE_TIME] <= '" + dtRange[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
        //            + @" AND [ID_TIMEZONE] = " + (int)_Session.CurrentIdTimezone
        //            + @" AND [ID_TIME] = " + (int)ID_PERIOD.DAY
        //            + @" AND [QUALITY] > 0";

        //        if (bLastItem == false)
        //            strQuery += @" UNION ALL ";
        //    }
        //    return Select(strQuery, out err);
        //}

        /// <summary>
        /// получение временного диапазона 
        /// для архивных значений
        /// </summary>
        /// <returns>диапазон дат</returns>
        protected DateTimeRange[] getDateTimeRangeValuesVarArchive()
        {
            DateTimeRange[] arRangesRes = null;
            int i = -1;
            bool bEndMonthBoudary = false;

            DateTime dtBegin = _Session.m_DatetimeRange.Begin.AddDays(-_Session.m_DatetimeRange.Begin.Day).AddMinutes(-1 * _Session.m_curOffsetUTC.TotalMinutes)
                , dtEnd = _Session.m_DatetimeRange.End.AddMinutes(-1 * _Session.m_curOffsetUTC.TotalMinutes).AddDays(1);
            //AddDays(-(DateTime.DaysInMonth(_Session.m_rangeDatetime.Begin.Year, _Session.m_rangeDatetime.Begin.Month) - 1));

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
                        {
                            // предыдущих значений нет
                            //arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                            arRangesRes[i] = new DateTimeRange(dtBegin, dtBegin.AddDays(1));
                        }
                        else
                            if (i == arRangesRes.Length - 1)
                            // крайний элемент массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                        else
                            // для элементов в "середине" массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End,// HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
                                               new DateTime(arRangesRes[i - 1].End.Year, arRangesRes[i - 1].End.AddMonths(1).Month, DateTime.DaysInMonth(arRangesRes[i - 1].End.Year, arRangesRes[i - 1].End.AddMonths(1).Month)));
            else
                if (bEndMonthBoudary == true)
                // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц ('diffMonth' всегда > 0)
                // + использование следующей за 'dtEnd' таблицы
                for (i = 0; i < arRangesRes.Length; i++)
                    if (i == 0)
                        // предыдущих значений нет
                        arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin).AddDays(-1));
                    else
                        if (i == arRangesRes.Length - 1)
                        // крайний элемент массива
                        arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End.AddDays(-1), dtEnd);
                    else
                        // для элементов в "середине" массива
                        arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
            else
                ;

            return arRangesRes;
        }

        /// <summary>
        /// Получение входных значений
        /// из INVAL???
        /// </summary>
        /// <param name="type">тип задачи</param>
        /// <param name="arQueryRanges">диапазон запроса</param>
        /// <param name="idPeriod">тек. период</param>
        /// <param name="err">Индентификатор ошибки</param>
        /// <returns>таблица значений</returns>
        /// <returns></returns>
        internal DataTable GetInVal(TaskCalculate.TYPE type, DateTimeRange[] arQueryRanges, ID_PERIOD actualIdPeriod, out int err)
        {
            string strQuery = string.Empty;
            bool bLastItem = false;

            for (int i = 0; i < arQueryRanges.Length; i++)
            {
                bLastItem = !(i < (arQueryRanges.Length - 1));

                strQuery += @"SELECT v.ID, v.ID_PUT, v.ID_USER, v.ID_SOURCE, v.DATE_TIME, v.ID_TIME"
                    + ", v.ID_TIMEZONE, v.QUALITY, v.VALUE, v.WR_DATETIME"
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p"
                    + @" ON a.ID = p.ID_ALG"
                    + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                    + arQueryRanges[i].End.ToString(@"yyyyMM") + @"] v"
                    + @" ON v.ID_PUT = p.ID"
                    + @" WHERE  ID_TASK = " + (int)IdTask
                    + @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                    + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                    + @" AND v.ID_TIME = " + (int)ID_PERIOD.DAY
                    + @" AND [ID_TIMEZONE] = " + (int)_Session.CurrentIdTimezone;

                if (bLastItem == false)
                    strQuery += @" UNION ALL ";
            }
            strQuery += @" ORDER BY [DATE_TIME] ";

            return Select(strQuery, out err);
        }

        public override DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err)
        {
            throw new NotImplementedException();
        }

        protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        {
            throw new NotImplementedException();
        }
    }
}