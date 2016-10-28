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
        /// <summary>
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        public enum INDEX_VIEW_VALUES : short
        {
            UNKNOWN = -1, SOURCE,
            ARCHIVE, COUNT
        }

        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        public INDEX_VIEW_VALUES m_ViewValues;

        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        public ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.DAY : _Session.m_currIdPeriod; } }


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

        public override string GetQueryValuesVar(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            string strRes = string.Empty
               , strJoinValues = string.Empty;

            if (!(type == TaskCalculate.TYPE.UNKNOWN))
            {
                //strJoinValues = getRangeAlg(type);
                if (strJoinValues.Equals(string.Empty) == false)
                    strJoinValues = @" JOIN [" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT AND p.ID_ALG" + strJoinValues;
                else
                    ;

                strRes = @"SELECT v.* FROM " + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @" as v"
                    + strJoinValues
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

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
                , tableOriginIn = null
                , tableCalcRes = null
                , tableCalcResIn = null;

            TepCommon.HandlerDbTaskCalculate.TaskCalculate.ListDATATABLE listDataTables = null;

            // подготовить таблицы для расчета
            listDataTables = prepareTepCalculateValues(type, out err);

            if (err == 0)
            {
                // произвести вычисления
                switch (type)
                {
                    case TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES:
                        tableOrigin = listDataTables.FindDataTable(TepCommon.HandlerDbTaskCalculate.TaskCalculate.INDEX_DATATABLE.OUT_VALUES);
                        tableOriginIn = listDataTables.FindDataTable(TepCommon.HandlerDbTaskCalculate.TaskCalculate.INDEX_DATATABLE.IN_VALUES);
                        DataTable[] tableRes = (m_taskCalculate as HandlerDbTaskCalculate.TaskBalTeploCalculate).CalculateOut(listDataTables);
                        tableCalcRes = tableRes[1];
                        tableCalcResIn = tableRes[0];
                        break;
                    default:
                        break;
                }
                // сохранить результаты вычисления
                saveResult(tableOrigin, tableCalcRes, out err);
                saveResultIn(tableOriginIn, tableCalcResIn, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при подготовке данных для расчета...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        /// <summary>
        /// Сохранить результаты вычислений в таблице для временных значений
        /// </summary>
        /// <param name="tableOrigin">??? Таблица с оригинальными значениями</param>
        /// <param name="tableRes">??? Таблица с оригинальными значениями</param>
        /// <param name="err">Признак выполнения операции сохранения</param>
        protected void saveResultIn(DataTable tableOrigin, DataTable tableRes, out int err)
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
                        , HDateTime.ToMoscowTimeZone ().ToString (CultureInfo.InvariantCulture)
                    });
                }
                else
                    ; //??? ошибка
            }

            RecUpdateInsertDelete(s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES], @"ID_PUT, ID_SESSION", string.Empty, tableOrigin, tableEdit, out err);
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
        public override string GetQueryValuesVar(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, ID_PERIOD idPeriod, int cntBasePeriod, DateTimeRange[] arQueryRanges)
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

        public DataTable GetValuesDayVar(TaskCalculate.TYPE type
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , GetQueryValuesDayVar(type
                    , idPeriod
                    , cntBasePeriod
                    , arQueryRanges)
                , null, null
                , out err);

            return tableRes;
        }

        public string GetQueryValuesDayVar(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, ID_PERIOD idPeriod, int cntBasePeriod, DateTimeRange[] arQueryRanges)
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

                    strRes += @"SELECT [ID_PUT], " + _Session.m_Id + @" as [ID_SESSION]"
                            + @", [QUALITY], SUM([VALUE]) as [VALUE]"
                            + @", Convert(DateTime, '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"') as [WR_DATETIME]"
                            + @", Convert(bigint,0 ) as [EXTENDED_DEFINITION]"

                        + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_"
                        + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"]"

                        + @" WHERE [ID_TIME] = 13 AND ID_SOURCE > 0 " //???ID_PERIOD.HOUR //??? _currIdPeriod
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
                    strRes += @" group by ID_PUT,QUALITY";
                }
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
        public DataTable GetValuesDefAll(ID_PERIOD idPeriod, INDEX_DBTABLE_NAME db_type, out int err)
        {
            DataTable tableRes = new DataTable();
            string query = string.Empty;
            if (db_type == INDEX_DBTABLE_NAME.INVALUES)
            {
                query = @"SELECT  d.[ID_PUT],d.[ID_TIME],d.[VALUE],d.[WR_ID_USER],d.[WR_DATETIME] FROM [inalg] a LEFT JOIN [input] i on a.id=i.ID_ALG INNER JOIN inval_def d on d.ID_PUT=i.ID WHERE a.ID_TASK=2 and d.[ID_TIME] = " + (int)idPeriod;
            }
            if (db_type == INDEX_DBTABLE_NAME.OUTVALUES)
            {
                query = @"SELECT  d.[ID_PUT],d.[ID_TIME],d.[VALUE],d.[WR_ID_USER],d.[WR_DATETIME] "
+ @"FROM [outalg] a LEFT JOIN [output] i on a.id=i.ID_ALG INNER JOIN inval_def d on d.ID_PUT=i.ID "
+ @"WHERE a.ID_TASK = 2 AND d.[ID_TIME] = " + (int)idPeriod;
            }
            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection, query, null, null, out err);

            return tableRes;
        }

        public DataTable GetValuesArch(INDEX_DBTABLE_NAME type, out int err)
        {
            string strRes = string.Empty;
            DataTable res = new DataTable();
            string from = s_NameDbTables[(int)type] + @"_" + _Session.m_rangeDatetime.Begin.Year.ToString() + _Session.m_rangeDatetime.Begin.Month.ToString(@"00");
            DateTimeRange[] dt_range = GetDateTimeRangeValuesVar();
            strRes = "SELECT * FROM "
                + from
                + " WHERE DATE_TIME>='" + dt_range[0].Begin.ToString() + "' AND DATE_TIME<= '" + dt_range[0].End.ToString() + "' AND ID_USER=" + HUsers.Id.ToString();

            res = Select(strRes, out err).Copy();
            res.Columns.Remove("ID");
            return res;
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
        /// <param name="idFPanel">Идентификатор панели на замену [ID_TASK]</param>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="tablePars">Таблица характеристик входных параметров</param>
        /// <param name="tableSessionValues">Таблица значений входных параметров</param>
        /// <param name="tableDefValues">Таблица значений по умолчанию входных параметров</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        public void CreateSession(int idFPanel
            , int cntBasePeriod
            , DataTable tablePars
            , ref DataTable[] arTableValuesIn
            , ref DataTable[] arTableValuesOut
            , DateTimeRange dtRange, out int err
            , out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            string strQuery = string.Empty;

            if (m_ViewValues == INDEX_VIEW_VALUES.SOURCE)
                CS_Source(idFPanel
                    , cntBasePeriod
                    , tablePars
                    , ref arTableValuesIn
                    , ref arTableValuesOut
                    , dtRange, out err
                    , out strErr);
            if (m_ViewValues == INDEX_VIEW_VALUES.ARCHIVE)
                CS_Archive(idFPanel
                    , cntBasePeriod
                    , tablePars
                    , ref arTableValuesIn
                    , ref arTableValuesOut
                    , dtRange, out err
                    , out strErr);
        }

        private void CS_Archive(int idFPanel
            , int cntBasePeriod
            , DataTable tablePars
            , ref DataTable[] arTableValuesIn
            , ref DataTable[] arTableValuesOut
            , DateTimeRange dtRange, out int err
            , out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            string strQuery = string.Empty;

            if ((arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE].Columns.Count > 0)
                && (arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE].Rows.Count > 0))
            {
                //Вставить строку с идентификатором новой сессии
                insertIdSession(idFPanel, cntBasePeriod, out err);
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertInValues(arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE], out err);

                // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                // получить входные для расчета значения для возможности редактирования
                strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                    + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);

                if ((arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE].Columns.Count > 0)
                    && (arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE].Rows.Count > 0))
                {
                    //Вставить строки в таблицу БД со входными значениями для расчета
                    insertOutValues(out err, arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]);

                    // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                    arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                    // получить входные для расчета значения для возможности редактирования
                    strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]" // as [ID]
                        + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"]"
                        + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                    arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
                }
            }
            else
            {
                CS_Source(idFPanel
                    , cntBasePeriod
                    , tablePars
                    , ref arTableValuesIn
                    , ref arTableValuesOut
                    , dtRange, out err
                    , out strErr);
            }
        }

        private void CS_Source(int idFPanel
            , int cntBasePeriod
            , DataTable tablePars
            , ref DataTable[] arTableValuesIn
            , ref DataTable[] arTableValuesOut
            , DateTimeRange dtRange, out int err
            , out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            string strQuery = string.Empty;


            if ((arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Columns.Count > 0)
                && (arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0))
            {
                //Вставить строку с идентификатором новой сессии
                insertIdSession(idFPanel, cntBasePeriod, out err);
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertInValues(arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

                // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                // получить входные для расчета значения для возможности редактирования
                strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                    + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
            }
            else
            {
                if (arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Count > 0)
                {
                    //Вставить строку с идентификатором новой сессии
                    insertIdSession(idFPanel, cntBasePeriod, out err);
                    //Вставить строки в таблицу БД со входными значениями для расчета
                    insertDefInValues(arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

                    // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                    arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                    // получить входные для расчета значения для возможности редактирования
                    strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                        + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
                        + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                    arTableValuesIn[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
                }
                else
                    Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if ((arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Columns.Count > 0)
                && (arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0))
            {
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertOutValues(out err, arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]);

                // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                // получить входные для расчета значения для возможности редактирования
                strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]" // as [ID]
                    + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"]"
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
            }
            else
            {
                if (arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Count > 0)
                {
                    //Вставить строки в таблицу БД со входными значениями для расчета
                    insertDefOutValues(arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

                    // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
                    arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
                    // получить входные для расчета значения для возможности редактирования
                    strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]" // as [ID]
                        + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"]"
                        + @" WHERE [ID_SESSION]=" + _Session.m_Id;
                    arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);

                }
                else
                    if (arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count == 0)
                {
                    string strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                        + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                        + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                        + @" FROM [dbo].[output] as p"
                        + @" JOIN [dbo].[outalg] as a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask
                        + @" JOIN [dbo].[measure] as m ON a.ID_MEASURE = m.ID ORDER BY ID";
                    DataTable param = Select(strRes, out err);
                    foreach (DataRow r in param.Rows)
                    {
                        arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Add(new object[] { r["ID"], "19", "0", "0", DateTime.Now });
                    }
                    insertDefOutValues(arTableValuesOut[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

                }
                Logging.Logg().Error(@"TepCommon.HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        ///// <summary>
        ///// Вставить в таблицу БД идентификатор новой сессии
        ///// </summary>
        ///// <param name="id">Идентификатор сессии</param>
        ///// <param name="idPeriod">Идентификатор периода расчета</param>
        ///// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        ///// <param name="idTimezone">Идентификатор часового пояса</param>
        ///// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        ///// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        //private void insertIdSession(
        //    int cntBasePeriod
        //    , out int err)
        //{
        //    err = -1;

        //    string strQuery = string.Empty;

        //    // подготовить содержание запроса при вставке значений, идентифицирующих новую сессию
        //    strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @" ("
        //        + @"[ID_CALCULATE]"
        //        + @", [ID_TASK]"
        //        + @", [ID_USER]"
        //        + @", [ID_TIME]"
        //        + @", [ID_TIMEZONE]"
        //        + @", [DATETIME_BEGIN]"
        //        + @", [DATETIME_END]) VALUES ("
        //        ;

        //    strQuery += _Session.m_Id;
        //    strQuery += @"," + (Int32)IdTask;
        //    strQuery += @"," + HTepUsers.Id;
        //    strQuery += @"," + (int)_Session.m_currIdPeriod;
        //    strQuery += @"," + (int)_Session.m_currIdTimezone;
        //    strQuery += @",'" + _Session.m_rangeDatetime.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture)  // @"yyyyMMdd HH:mm:ss"
        //    strQuery += @",'" + _Session.m_rangeDatetime.End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture) ; // @"yyyyMMdd HH:mm:ss"

        //    strQuery += @")";

        //    //Вставить в таблицу БД строку с идентификтором новой сессии
        //    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
        //}

        /// <summary>
        /// Вставить значения в таблицу для временных входных значений
        /// </summary>
        /// <param name="tableInValues">Таблица со значениями для вставки</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        public void insertInValues(DataTable tableInValues, out int err)
        {
            err = -1;

            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;
            string[] col_names = { "ID_SESSION"
      ,"ID_PUT"
      ,"QUALITY"
      ,"VALUE"
      ,"WR_DATETIME"
      ,"EXTENDED_DEFINITION"};

            // подготовить содержание запроса при вставке значений во временную таблицу для расчета
            strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

            arTypeColumns = new Type[tableInValues.Columns.Count];
            arNameColumns = new string[tableInValues.Columns.Count];
            foreach (string c in col_names)
            {
                strQuery += c + @",";
            }
            // исключить лишнюю запятую
            strQuery = strQuery.Substring(0, strQuery.Length - 1);

            strQuery += @") VALUES ";

            foreach (DataRow r in tableInValues.Rows)
            {
                strQuery += @"(";
                int i_col = 0;
                strQuery += _Session.m_Id + @",";
                i_col++;

                foreach (string str in col_names)
                {
                    foreach (DataColumn c in tableInValues.Columns)
                    {
                        if (c.ColumnName == str & c.ColumnName != "ID_SESSION")
                        {
                            strQuery += DbTSQLInterface.ValueToQuery(r[c.Ordinal], c.DataType) + @",";
                            i_col++;
                        }

                    }

                }
                if (col_names.Length - i_col == 1)
                {
                    strQuery += "'" + DateTime.Now.ToString() + @"',";
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
            string[] col_name = { "ID_SESSION", "ID_PUT", "QUALITY", "VALUE", "WR_DATETIME", "EXTENDED_DEFINITION" };

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
                        strQuery += "'" + r[c].ToString() + @"',";
                        strQuery += "'" + DateTime.Now.ToString() + @"',";
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

            if (IdTask == ID_TASK.BAL_TEPLO)
                insertOutValues(_Session.m_Id, TaskCalculate.TYPE.OUT_VALUES, out err, tableRes);
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
                      + rowSel[@"VALUE"].ToString().Replace(',', '.') + @"," + //VALUE
                    "'" + rowSel[@"WR_DATETIME"] + @"'," +
                    "'" + HDateTime.ToMoscowTimeZone().ToString()
                      ;

                    strQuery += @"'),";

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
        /// Вставить значения в таблицу для временных входных значений по умолчанию
        /// </summary>
        /// <param name="tableOutValues">Таблица со значениями для вставки</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertDefOutValues(DataTable tableOutValues, out int err)
        {
            err = -1;

            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;
            string[] col_name = { "ID_SESSION", "ID_PUT", "QUALITY", "VALUE", "WR_DATETIME" };

            // подготовить содержание запроса при вставке значений во временную таблицу для расчета
            strQuery = @"INSERT INTO " + TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @" (";

            arTypeColumns = new Type[tableOutValues.Columns.Count];
            arNameColumns = new string[tableOutValues.Columns.Count];
            foreach (string c in col_name)
            {
                strNameColumn = c;
                strQuery += strNameColumn + @",";
            }
            // исключить лишнюю запятую
            strQuery = strQuery.Substring(0, strQuery.Length - 1);

            strQuery += @") VALUES ";

            foreach (DataRow r in tableOutValues.Rows)
            {
                strQuery += @"(";
                strQuery += _Session.m_Id + @",";

                foreach (DataColumn c in tableOutValues.Columns)
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
                        strQuery += "GETDATE(),";
                        //strQuery += "'" + DateTime.Now.ToString() + @"',";
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

        private int getNextIdDB_in(DateTime date)
        {
            int id = -1,
                err = -1;
            string month = string.Empty;
            if (date.Month < 10)
                month = "0" + date.Month.ToString();
            else
                month = date.Month.ToString();
            DataTable res = Select("SELECT TOP 1 ID FROM inval_" + date.Year.ToString() + month + " order by ID desc", out err);
            if (res.Rows.Count == 0)
            {
                id = 0;
            }
            else
                id = Convert.ToInt32(res.Rows[0][0]);
            return id;
        }
        private int getNextIdDB_out(DateTime date)
        {
            int id = -1,
                err = -1;
            string month = string.Empty;
            if (date.Month < 10)
                month = "0" + date.Month.ToString();
            else
                month = date.Month.ToString();
            DataTable res = Select("SELECT TOP 1 ID FROM outval_" + date.Year.ToString() + month + " order by ID desc", out err);
            if (res.Rows.Count == 0)
            {
                id = 0;
            }
            else
                id = Convert.ToInt32(res.Rows[0][0]);
            return id;
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
            err = 0;
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
                                rowSel
                                , HUsers.Id.ToString()
                                , 0.ToString()
                                , (_Session.m_rangeDatetime.Begin - getOffsetMoscowToUTC).ToString(CultureInfo.InvariantCulture)
                                , ID_PERIOD.DAY
                                , ID_TIMEZONE.MSK
                                , 1.ToString()
                                , tableRes.Rows[i]["VALUE"]
                                , DateTime.Now
                            });
                    //}
                }
                //}
            }
            else;

            return tableEdit;
        }

        /// <summary>
        /// Смещение по времени до Москвы
        /// </summary>
        private TimeSpan getOffsetToMoscow
        {
            get
            {
                TimeSpan offset = TimeZoneInfo.Local.BaseUtcOffset - TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").BaseUtcOffset;
                
                return offset;
            }
            set
            {
            }
        }
        /// <summary>
        /// Смещение по времени Москвы до UTC
        /// </summary>
        private TimeSpan getOffsetMoscowToUTC
        {
            get
            {
                TimeSpan offset = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").BaseUtcOffset;
                
                return offset;
            }
            set
            {
            }
        }
        /// <summary>
        /// Смещение по времени до UTC
        /// </summary>
        private TimeSpan getOffsetToUTC
        {
            get
            {
                TimeSpan offset = TimeZoneInfo.Local.BaseUtcOffset;

                return offset;
            }
            set
            {
            }
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
            err = 0;
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
                                rowSel
                                , HUsers.Id.ToString()
                                , 0.ToString()
                                , (_Session.m_rangeDatetime.Begin - getOffsetMoscowToUTC).ToString(CultureInfo.InvariantCulture)
                                , ID_PERIOD.DAY
                                , ID_TIMEZONE.MSK
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
            DataTable dt = new DataTable();
            //dt = HTepUsers.GetProfileUser_Tab(IdTab);????

            return dt;
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

            strRes = @"SELECT * FROM " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INALG] + " where ID_TASK=2";

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
            protected override int initValues(TepCommon.HandlerDbTaskCalculate.TaskCalculate.ListDATATABLE listDataTables)
            {
                initValues(Out, listDataTables.FindDataTable(INDEX_DATATABLE.OUT_PARAMETER), listDataTables.FindDataTable(INDEX_DATATABLE.OUT_VALUES));
                initValues(In, listDataTables.FindDataTable(INDEX_DATATABLE.IN_PARAMETER), listDataTables.FindDataTable(INDEX_DATATABLE.IN_VALUES));
                return 0;
            }
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
                iOP1, iOP2, iOP3, iOP4, iOP5, iOP6,
                iPP1, iPP2, iPP3, iPP4, iPP5, iPP6, iPP7, iPP8,
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

            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable[] CalculateOut(ListDATATABLE listDataTables)
            {
                int iInitValuesRes = -1;

                DataTable tableRes = null;
                DataTable tableResIn = null;

                iInitValuesRes = initValues(listDataTables);

                if (iInitValuesRes == 0)
                {
                    var items = from pair in In
                                orderby pair.Key ascending
                                select pair;
                    // расчет
                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items)
                    {
                        //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                        calculateIn(pAlg.Key);
                    }
                    // преобразование в таблицу
                    tableResIn = resultToTable(In);

                    items = from pair in Out
                            orderby pair.Key ascending
                            select pair;

                    // расчет
                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items)
                    {
                        //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                        calculateOut(pAlg.Key);
                    }
                    // преобразование в таблицу
                    tableRes = resultToTable(Out);
                }
                else
                    ; // ошибка при инициализации параметров, значений
                return new DataTable[] { tableResIn, tableRes };
            }

            private float calculateOut(string nAlg)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная велечина
                float sum = 0,
                    sum1 = 0;
                int i = -1;
                switch (nAlg)
                {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            double str = 9.771 * Math.Pow(10, -4) + 1.774 * Math.Pow(10, -5) * In["1.2"][ID_COMP[i]].value / 100
                                + 2.52 * Math.Pow(10, -5) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 2) + 2.96 * Math.Pow(10, -6) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100 - 1.5), 3) * In["1.2"][ID_COMP[i]].value / 100
                                + (3.225 * Math.Pow(10, -6) + 1.3436 * Math.Pow(10, -6) * In["1.2"][ID_COMP[i]].value / 100 + 1.684 * Math.Pow(10, -8) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 6)
                                + 1.432 * Math.Pow(10, -7) * Math.Pow((1 / (In["1.2"][ID_COMP[i]].value / 100 + 0.5)), 3)) * ((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10)
                                + (3.7 * Math.Pow(10, -8) + 3.588 * Math.Pow(10, -8) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 3) - 4.05 * Math.Pow(10, -13) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 9)) * Math.Pow(((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10), 2) +
                                +1.1766 * Math.Pow(10, -13) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 12) * Math.Pow(((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10), 4);

                            Out[nAlg][ID_COMP[i]].value = (float)str * 10000;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        }
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            double str = In["1.1"][ID_COMP[i]].value;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            double p = In["1.4"][ID_COMP[i]].value;
                            double t = In["1.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            double p = In["1.4"][ID_COMP[i]].value;//Индекс обратного давления
                            double t = In["1.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = 0/*(float)str*/;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In["1.1"][ID_COMP[i]].value * (Out["1.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            double p = In["2.5"][ID_COMP[i]].value;
                            double t = In["2.3"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            double p = In["2.6"][ID_COMP[i]].value;
                            double t = In["2.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In["7.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In["2.1"][ID_COMP[i]].value * (Out["2.1"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 3.1
                    case @"3.1": //Тепло с подпиткой сумма суммы тепла по блокам и тепла с подпиткой теплосети
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double str = 0;
                            Out[nAlg][ID_COMP[i]].value = In["7.1"][ID_COMP[i]].value;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 3.2
                    case @"3.2": //Энтальпия тепла
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double p = In["2.6"][ID_COMP[i]].value;
                            double t = In["2.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 3.3
                    case @"3.3": //Тепло по блокам с подпиткой
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double str = 0;
                            Out[nAlg][ID_COMP[i]].value = Out["1.5"][ID_COMP[(int)INDX_COMP.iST]].value + Out["3.1"][ID_COMP[i]].value;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 4.1
                    case @"4.1": //Q БД тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out["2.3"][ID_COMP[(int)INDX_COMP.iST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        nAlg = "4.3";
                        fRes = 0;
                        goto entalp;//????
                        break;
                    #endregion

                    #region 4.2
                    case @"4.2": //Q расч тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = (In["4.1"][ID_COMP[i]].value * (Out["4.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 4.3
                    case @"4.3": //Энтальпия тс
                    entalp://????
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double p = In["4.4"][ID_COMP[i]].value;
                            double t = In["4.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 5.1
                    case @"5.1": //Тепло вывода F1              
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out["2.4"][ID_COMP[(int)INDX_COMP.iST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 5.2
                    case @"5.2": //Тепло вывода F2              
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            fRes += In["2.1"][ID_COMP[i]].value * Out["2.1"][ID_COMP[i]].value - In["2.2"][ID_COMP[i]].value * Out["2.2"][ID_COMP[i]].value;
                        }
                        Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / 1000;
                        break;
                    #endregion

                    #region 5.3
                    case @"5.3": //Небаланс                     
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++)
                        {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = (In["2.1"][ID_COMP[(int)INDX_COMP.iST]].value - In["2.2"][ID_COMP[(int)INDX_COMP.iST]].value - In["4.1"][ID_COMP[(int)INDX_COMP.iST]].value) / In["2.1"][ID_COMP[(int)INDX_COMP.iST]].value * 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Q бд
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = In["7.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;
                        }
                        nAlg = "6.3";
                        fRes = 0;
                        goto entpr;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Q расч
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In["6.1"][ID_COMP[i]].value * (Out["6.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Энтальпия пр
                    entpr:
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            double p = In["6.5"][ID_COMP[i]].value;
                            double t = In["6.3"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP1);

                        }
                        nAlg = "6.4";
                        fRes = 0;
                        goto entob;
                        break;
                    #endregion

                    #region 6.4
                    case @"6.4": //Энтальпия обр
                    entob:
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            double p = In["6.6"][ID_COMP[i]].value;//Индекс обратного давления
                            double t = In["6.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP1);

                        }
                        break;
                    #endregion

                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateMaket (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
                return fRes;
            }

            private float calculateIn(string nAlg)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная велечина
                float sum = 0,
                    sum1 = 0;
                int i = -1;
                double str = 0;
                switch (nAlg)
                {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            str = str + In["1.1"][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.1"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            str = str + In["1.2"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.2"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            str = str + In["1.3"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.3"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            str = str + In["1.4"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.4"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++)
                        {
                            str = str + In["1.5"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.5"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.5
                    case @"2.5": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.6
                    case @"2.6": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP1; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.4
                    case @"6.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.5
                    case @"6.5": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.6
                    case @"6.6": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP1; i < (int)INDX_COMP.iST; i++)
                        {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP1 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    default:
                        break;
                }
                return fRes;
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
