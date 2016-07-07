using System;
using System.Globalization;
//using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Data;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;


namespace PluginTaskReaktivka
{
    public class HandlerDbTaskReaktivkaCalculate : HandlerDbTaskCalculate
    {
        protected override void createTaskCalculate()
        {
            base.createTaskCalculate();
        }

        protected override void calculate(TaskCalculate.TYPE type, out int err)
        {
            err = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetQueryComp(TaskCalculate.TYPE type)
        {
            string strRes = string.Empty;

            strRes = @"SELECT a.[ID] as ID_ALG, p.[ID], p.[ID_COMP], cl.[DESCRIPTION], a.[N_ALG] "
            + @"FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + "] a "
            + @"LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + "] p "
            + @"ON a.ID = p.ID_ALG "
            + @"LEFT JOIN " + "[" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.COMP_LIST] + @"] cl "
            + @"ON cl.ID = p.ID_COMP "
            + @"WHERE a.ID_TASK  = " + (int)IdTask;

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

        /// <summary>
        /// Получение данныз из profiles
        /// </summary>
        /// <param name="IdTab">Ид панели</param>
        /// <returns>таблица данных</returns>
        public DataTable GetProfilesContext()
        {
            string query = string.Empty;
            int err = -1;

            query = @"SELECT * "
                + @"FROM [TEP_NTEC_5].[dbo].[profiles] "
                + @"WHERE ID_EXT = " + HTepUsers.Role;

            return Select(query, out err);
        }

        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        ///  , структура таблицы совместима с [inval], [outval]
        /// </summary>
        /// <param name="type">тип задачи</param>
        /// <param name="idPeriod">период</param>
        /// <param name="cntBasePeriod">период(день,месяц,год)</param>
        /// <param name="arQueryRanges">диапазон времени запроса</param>
        /// <returns>строка запроса</returns>
        public override string GetQueryValuesVar(TaskCalculate.TYPE type, ID_PERIOD idPeriod
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
                            + @", CONVERT(varchar, [DATE_TIME], 127) as [EXTENDED_DEFINITION] "
                            + @"FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] a "
                            + @"LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p "
                            + @"ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask + " "
                            + @"LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                            + arQueryRanges[i].End.ToString(@"yyyyMM") + @"] v "
                            + @"ON p.ID = v.ID_PUT "
                            + @"WHERE v.[ID_TIME] = " + (int)idPeriod //+ " AND [ID_SOURCE] > 0 "
                            + @" AND ID_TIMEZONE = " + (int)_Session.m_currIdTimezone
                        ;
                    // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                    // 'Begin' == 'End'
                    if (bLastItem == true)
                        bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);

                    if (bEquDatetime == false)
                        strRes += @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                      + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                    if (bLastItem == false)
                        strRes += @" UNION ALL ";
                }

                strRes = "" + @"SELECT v.ID_PUT"
                    + @", " + _Session.m_Id + @" as [ID_SESSION]"
                    + @", [QUALITY]"
                    + @", [VALUE]"
                    + @", [DATE_TIME] as [WR_DATETIME]"
                    + @", [EXTENDED_DEFINITION] "
                    + @"FROM (" + strRes + @") as v "
                    + @"ORDER BY v.ID_PUT,v.DATE_TIME";
            }
            else
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
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
        /// получение временного диапазона 
        /// для всех значений
        /// </summary>
        /// <returns>диапазон дат</returns>
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
                                   new DateTime(arRangesRes[i - 1].End.Year,arRangesRes[i - 1].End.AddMonths(1).Month,DateTime.DaysInMonth(arRangesRes[i - 1].End.Year,arRangesRes[i - 1].End.AddMonths(1).Month)));
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

        /// <summary>
        /// 
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

                for (int j = 0; j < tableOrigin.Rows.Count; j++)
                    if (rowSel == tableOrigin.Rows[j]["ID_PUT"].ToString())
                        if (dtRes == Convert.ToDateTime(tableOrigin.Rows[j]["DATE_TIME"]))
                            if (tableOrigin.Rows[j]["Value"].ToString() == tableRes.Rows[i]["VALUE"].ToString())
                            {
                                idUser = (int)tableOrigin.Rows[j]["ID_USER"];
                                idSource = (int)tableOrigin.Rows[j]["ID_SOURCE"];
                                break;
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
        /// Получение корр. входных значений
        /// из INVAL
        /// </summary>
        /// <param name="type">тип задачи</param>
        /// <param name="arQueryRanges"></param>
        /// <param name="idPeriod">тек. период</param>
        /// <param name="err">Индентификатор ошибки</param>
        /// <returns>таблица значений</returns>
        public DataTable GetInVal(TaskCalculate.TYPE type
            , DateTimeRange[] arQueryRanges
            , ID_PERIOD idPeriod
            , out int err)
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
                    + @" AND v.ID_TIME = " + (int)idPeriod
                    + @" AND ID_TIMEZONE = " + (int)_Session.m_currIdTimezone;

                if (bLastItem == false)
                    strQuery += @" UNION ALL ";
            }
            strQuery += @" ORDER BY [DATE_TIME] ";

            return Select(strQuery, out err);
        }
    }
}
