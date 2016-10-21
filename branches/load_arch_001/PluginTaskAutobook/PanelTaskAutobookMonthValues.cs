using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace PluginTaskAutobook
{
    public class PanelTaskAutobookMonthValues : HPanelTepCommon
    {
        /// <summary>
        /// 
        /// </summary>
        public static int vsRatio;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        bool m_bflgClear = true;
        //public event DelegateBoolFunc EvtChangeRow;
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected HandlerDbTaskCalculate.INDEX_TABLE_VALUES m_ViewValues;
        /// <summary>
        /// 
        /// </summary>
        protected TaskAutobookCalculate AutoBookCalc;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1,
            PERIOD, TIMEZONE,
            COMPONENT, PARAMETER, RATIO,
            COUNT
        }     
        /// <summary>
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Перечисление - столбцы отображения
        /// </summary>
        public enum INDEX_GTP : int
        {
            UNKNOW = -1,
            GTP12, GTP36,
            TEC,
            CorGTP12, CorGTP36,
            COUNT
        }
        /// <summary>
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        public enum INDEX_VIEW_VALUES : short
        {
            UNKNOWN = -1,
            ARCHIVE, SOURCE,
            COUNT
        }
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL { UNKNOWN = -1, LABEL_DESC=1, DGV_DATA=3 }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD, // идентификаторы периодов расчетов, использующихся на форме               
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов                   
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //    , DENY_COMP_CALCULATED,//DENY_PARAMETER_CALCULATED // запрещенных для расчета
            DENY_COMP_VISIBLED, //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskAutobookMonthValuesCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskAutobookMonthValuesCalculate; } }
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// Часовой пояс(часовой сдвиг)
        /// </summary>
        protected static int m_currentOffSet;
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        private PanelManagementAutobook createPanelManagement()
        {
            return new PanelManagementAutobook();
        }
        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        public DGVAutoBook m_dgvAB;
        /// <summary>
        /// to Outlook
        /// </summary>
        protected ReportsToNSS m_rptsNSS;
        /// <summary>
        /// to Excel
        /// </summary>
        protected ReportExcel m_rptExcel;
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        private PanelManagementAutobook _panelManagement;
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementAutobook PanelManagementAB
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement;
            }
        }

        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Отправить", @"Сохранить", @"Загрузить" };

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskAutobookMonthValuesCalculate();
        }

        /// <summary>
        /// Класс для грида
        /// </summary>
        public class DGVAutoBook : DataGridView
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;

            /// <summary>
            /// Структура для описания добавляемых строк
            /// </summary>
            public class ROW_PROPERTY
            {
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { IS_NAN }
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idParameter, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                    }

                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }
                /// <summary>
                /// Пояснения к параметру в алгоритме расчета
                /// </summary>
                public string m_strMeasure
                    , m_Value;
                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRound;

                public HDataGridViewCell[] m_arPropertiesCells;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="cntCols"></param>
                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];
                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT);
                }
            }

            public DGVAutoBook(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            private void InitializeComponents(string nameDGV)
            {
                Name = nameDGV;
                Dock = DockStyle.Fill;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
                //Отменить возможность добавления строк
                AllowUserToAddRows = false;
                //Отменить возможность удаления строк
                AllowUserToDeleteRows = false;
                //Отменить возможность изменения порядка следования столбцов строк
                AllowUserToOrderColumns = false;
                //Не отображать заголовки строк
                RowHeadersVisible = false;
                //Ширина столбцов под видимую область
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                AddColumn(-2, string.Empty, "ALG", true, false);
                AddColumn(-1, "Дата", "DATE", true, true);
            }

            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_iIdComp;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            public void AddColumn(string txtHeader, bool bRead, string nameCol)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    //column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="idPut">индентификатор источника</param>
            public void AddColumn(int idPut, string txtHeader, string nameCol, bool bRead, bool bVisibled)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (HDataGridViewColumn col in Columns)
                        if ((col.m_iIdComp > 0)
                            && (col.m_iIdComp < 1000))
                        {
                            indxCol = Columns.IndexOf(col);
                            break;
                        }

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = idPut };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (idPut < 0)
                            // для служебных столбцов
                            //column.Frozen = true
                            ;
                    }

                    column.ReadOnly = bRead;
                    column.HeaderText = txtHeader;
                    column.Name = nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = bVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + idPut + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Установка идПута для столбца
            /// </summary>
            /// <param name="idPut">номер пута</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddIdComp(int idPut, string nameCol)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.m_iIdComp = idPut;
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bRead">true/false</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddBRead(bool bRead, string nameCol)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.ReadOnly = bRead;
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow(ROW_PROPERTY rowProp)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

                if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
                    m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
                // инициализировать значения в служебных ячейках
                m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
            }

            /// <summary>
            /// 
            /// </summary>
            protected struct RATIO
            {
                public int m_id;
                public int m_value;
                public string m_nameRU
                    , m_nameEN
                    , m_strDesc;
            }

            /// <summary>
            /// 
            /// </summary>
            protected Dictionary<int, RATIO> m_dictRatio;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tblRatio"></param>
            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO()
                    {
                        m_id = (int)r[@"ID"]
                        ,
                        m_value = (int)r[@"VALUE"]
                        ,
                        m_nameRU = (string)r[@"NAME_RU"]
                        ,
                        m_nameEN = (string)r[@"NAME_RU"]
                        ,
                        m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }

            /// <summary>
            /// 
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            public void ClearValues()
            {
                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="planOnMonth">план на месяц</param>
            /// <param name="typeValues">тип данных</param>
            public void ShowValues(DataTable[] tbOrigin
                , DataTable planOnMonth
                , HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
            {
                int idAlg = -1
                  , vsRatioValue = -1
                  , corOffset = 0;
                DataRow[] dr_CorValues, dr_Values = null;
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                bool bflg = false;
                double dblVal = -1F;
                //
                ClearValues();
                //заполнение плана
                if (planOnMonth.Rows.Count > 0)
                    planInMonth(planOnMonth.Rows[0]["VALUE"].ToString(),
                      Convert.ToDateTime(planOnMonth.Rows[0]["WR_DATETIME"].ToString()));
                //заполнение столбцов с корр. знач.
                foreach (DataGridViewRow row in Rows)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.Index > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        {
                            dr_CorValues = formingValue(tbOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                               , row.Cells["DATE"].Value.ToString(), m_currentOffSet, col.m_iIdComp);
                            idAlg = (int)row.Cells["ALG"].Value;

                            if (dr_CorValues != null)
                                for (int t = 0; t < dr_CorValues.Count(); t++)
                                {
                                    dblVal = Convert.ToDouble(dr_CorValues[t]["VALUE"]);
                                    vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                    dblVal *= Math.Pow(10F, 1 * vsRatioValue);

                                    row.Cells[col.Index].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, CultureInfo.InvariantCulture);
                                }

                            if ((int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION == (int)typeValues)
                                corOffset = 1;

                            dr_Values = formingValue(tbOrigin[(int)typeValues],
                               row.Cells["DATE"].Value.ToString()
                               , (m_currentOffSet * corOffset)
                               , col.m_iIdComp);

                            if (dr_Values != null)
                                if (dr_Values.Count() > 0)
                                    //заполнение столбцов ГТП,ТЭЦ
                                    for (int p = 0; p < dr_Values.Count(); p++)
                                        if (row.Cells["DATE"].Value.ToString() ==
                                        Convert.ToDateTime(dr_Values[p]["WR_DATETIME"]).AddMinutes(m_currentOffSet).ToShortDateString())
                                        {
                                            dblVal = correctingValues(Math.Pow(10F,-1* vsRatioValue)
                                                , dr_Values[p]["VALUE"]
                                                , col.Name
                                                , ref bflg
                                                , row
                                                , typeValues);

                                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                            dblVal *= Math.Pow(10F, 1 * vsRatioValue);

                                            row.Cells[col.Index].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                                CultureInfo.InvariantCulture);
                                        }

                            //if (dr_CorValues != null)
                            //    if (dr_CorValues.Count() > 0 &&   dr_Values == null)
                            //{
                            //    editCells(row);
                            //}                        

                        }
                    }
                    fillCells(row);
                }
            }

            /// <summary>
            ///Корректировка знач.
            /// </summary>
            /// <param name="pow"></param>
            /// <param name="rowValue">значение</param>
            /// <param name="namecol">имя столбца</param>
            /// <param name="bflg">признак корректировки</param>
            /// <param name="row">тек.строка</param>
            /// <param name="typeValues">тип загружаеммых данных(архивные/текущие)</param>
            /// <returns></returns>
            private double correctingValues(double pow
                , object rowValue
                , string namecol
                , ref bool bflg
                , DataGridViewRow row
                , HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
            {
                double valRes = 0
                    , signValues = 1;

                switch (typeValues)
                {
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE:
                        signValues = -1;
                        break;
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION:
                        break;
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                switch (namecol)
                {
                    case "GTP12":
                        if (double.TryParse(row.Cells["CorGTP12"].Value.ToString(), out valRes))
                        {
                            valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "GTP36":
                        if (double.TryParse(row.Cells["CorGTP36"].Value.ToString(), out valRes))
                        {
                            valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "TEC":
                        if (bflg)
                        {
                            valRes = double.Parse(row.Cells["GTP12"].Value.ToString()) * pow
                                + double.Parse(row.Cells["GTP36"].Value.ToString()) * pow;
                            bflg = false;
                        }
                        else
                            valRes = (double)rowValue;
                        break;
                    default:
                        break;
                }
                return valRes;
            }

            /// <summary>
            /// Вычисление месячного плана
            /// </summary>
            /// <param name="value">значение</param>
            /// <param name="date">дата</param>
            private void planInMonth(string value, DateTime date)
            {
                int idAlg
                     , vsRatioValue = -1;
                double planDay
                   , dbValue
                    , increment = 0;

                idAlg = (int)Rows[0].Cells["ALG"].Value;
                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                planDay = (Convert.ToSingle(value)
                   / DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month));

                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    increment = increment + planDay;
                    dbValue = increment * Math.Pow(10F, 1 * vsRatioValue);
                    Rows[i].Cells["PlanSwen"].Value = dbValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                             CultureInfo.InvariantCulture);
                }

                dbValue = float.Parse(value) * Math.Pow(10F, 1 * vsRatioValue);

                Rows[DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month) - 1].Cells["PlanSwen"].Value =
                    dbValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Редактирование значений ввиду новых корр. значений
            /// </summary>
            /// <param name="e"></param>
            /// <param name="colName">имя столбца, 
            /// который попадает под корректировку</param>
            public void editCells(DataGridViewCellParsingEventArgs e, string colName)
            {
                double valueNew,//новое знач.
                valueCor,//первичное знач.
                valueCell = 0,//знач. ячейки
                value = 0;

                if (e.Value.ToString() == string.Empty)
                    valueNew = 0;
                else
                    valueNew = AsParseToF(e.Value.ToString());

                valueCor = AsParseToF(Rows[e.RowIndex].Cells[Columns[e.ColumnIndex].Name].Value.ToString());

                switch (Columns[e.ColumnIndex].Name)
                {
                    case "CorGTP12":

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), out valueCell);

                        break;
                    case "CorGTP36":
                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), out valueCell);
                        break;
                }

                if (valueCell != 0)
                {
                    Rows[e.RowIndex].Cells[INDEX_GTP.TEC.ToString()].Value = AsParseToF(Rows[e.RowIndex].Cells[colName].Value.ToString())
                               + valueCell;

                    for (int i = e.RowIndex; i < Rows.Count; i++)
                        if (double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                            fillCells(Rows[i]);
                        else
                            break;
                }
            }

            /// <summary>
            /// Редактирование значений ввиду новых корр. значений
            /// </summary>
            /// <param name="row">редактируемая строка</param>
            public void editCells(DataGridViewRow row)
            {
                double valueCor,//новое знач.
                valueCell = 0,//знач. ячейки
                value = 0;

                foreach (DataGridViewColumn col in row.DataGridView.Columns)
                {
                    switch (col.Name)
                    {
                        case "CorGTP12":

                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCor + valueCell;
                            break;

                        case "CorGTP36":

                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCor + valueCell;
                            break;
                    }
                }

                Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value = double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString())
                           + double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString());

                for (int i = row.Index; i < Rows.Count; i++)
                    if (double.TryParse(Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                        fillCells(Rows[i]);
                    else
                        break;
            }

            /// <summary>
            /// Вычисление параметров нараст.ст.
            /// и заполнение грида
            /// </summary>
            /// <param name="row">строка</param>
            private void fillCells(DataGridViewRow row)
            {
                int value
                    , swenValue = 0;

                if (int.TryParse(row.Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                {
                    if (row.Index == 0)
                        row.Cells["StSwen"].Value = value;
                    else
                    {
                        int.TryParse(row.DataGridView.Rows[row.Index - 1].Cells["StSwen"].Value.ToString(), out swenValue);
                        row.Cells["StSwen"].Value = value + swenValue;
                    }

                    countDeviation(row);
                }
            }

            /// <summary>
            /// Вычисление отклонения от плана
            /// </summary>
            /// <param name="row">строка</param>
            public void countDeviation(DataGridViewRow row)
            {
                int _number = 0;

                if (row.Cells["StSwen"].Value == null)
                    row.Cells["DevOfPlan"].Value = "";
                else
                    if (int.TryParse(row.Cells["PlanSwen"].Value.ToString(), out _number))
                    row.Cells["DevOfPlan"].Value = Convert.ToSingle(row.Cells["StSwen"].Value) - _number;
                else
                    row.Cells["DevOfPlan"].Value = Convert.ToSingle(row.Cells["StSwen"].Value) - 0;
            }

            /// <summary>
            /// Отбор строк по дате и идПуту
            /// </summary>
            /// <param name="dtOrigin">таблица значений</param>
            /// <param name="date">дата</param>
            /// <param name="idPut">идЭлемента</param>
            /// <returns>набор строк</returns>
            private DataRow[] formingValue(DataTable dtOrigin, string date, int idPut)
            {
                DateTime dateOffSet;
                DataRow[] dr_idCorPut = null;

                var m_enumResIDPUT = (from r in dtOrigin.AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    dateOffSet = m_enumResIDPUT.ElementAt(i).DATE_TIME;

                    if (date == dateOffSet.ToShortDateString())
                    {
                        dr_idCorPut = dtOrigin.Select(
                        string.Format(dtOrigin.Locale
                        , "WR_DATETIME = '{0:o}' AND ID_PUT = {1}", m_enumResIDPUT.ElementAt(i).DATE_TIME, idPut));
                        break;
                    }
                }
                return dr_idCorPut;
            }

            /// <summary>
            /// Отбор строк по дате и идПуту
            /// </summary>
            /// <param name="dtOrigin">таблица значений</param>
            /// <param name="date">дата</param>
            /// <param name="offSet">часовая разница</param>
            /// <param name="idPut">идЭлемента</param>
            /// <returns>набор строк</returns>
            private DataRow[] formingValue(DataTable dtOrigin, string date, int offSet, int idPut)
            {
                DateTime dateOffSet;
                DataRow[] dr_idCorPut = null;

                var m_enumResIDPUT = (from r in dtOrigin.AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    dateOffSet = m_enumResIDPUT.ElementAt(i).DATE_TIME.AddMinutes(offSet);

                    if (date == dateOffSet.ToShortDateString())
                    {
                        dr_idCorPut = dtOrigin.Select(
                        string.Format(dtOrigin.Locale
                        , "WR_DATETIME = '{0:o}' AND ID_PUT = {1}", m_enumResIDPUT.ElementAt(i).DATE_TIME, idPut));
                        break;
                    }
                }
                return dr_idCorPut;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="e">переменная с данными события</param>
            /// <returns></returns>
            public DataTable FillTableCorValue(int offset, DataGridViewCellParsingEventArgs e)
            {
                double valueToRes;
                int idComp = 0
                     , idAlg
                     , vsRatioValue = -1;
                DateTime timeRes;
                HDataGridViewColumn cols = (HDataGridViewColumn)Columns[e.ColumnIndex];

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });


                for (int i = 0; i < Rows.Count; i++)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.Index > (int)INDEX_GTP.GTP36 & col.Index < (int)INDEX_GTP.CorGTP36)
                        {
                            idAlg = (int)Rows[0].Cells["ALG"].Value;
                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                            if (cols.m_iIdComp == col.m_iIdComp &&
                                Rows[i].Cells["Date"].Value == Rows[e.RowIndex].Cells["Date"].Value)
                            {

                                valueToRes = AsParseToF(e.Value.ToString()) * Math.Pow(10F, -1 * vsRatioValue);
                                //double.Parse(e.Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture) //
                                idComp = cols.m_iIdComp;
                            }
                            else
                                if (double.TryParse(Rows[i].Cells[col.Index].Value.ToString(), out valueToRes))
                            {
                                valueToRes *= Math.Pow(10F, -1 * vsRatioValue);
                                idComp = col.m_iIdComp;
                            }
                            else
                                valueToRes = -1;

                            timeRes = Convert.ToDateTime(Rows[i].Cells["Date"].Value.ToString());
                            //при -1 не нужно записывать значение в таблицу
                            if (valueToRes > -1)
                                dtSourceEdit.Rows.Add(new object[]
                                {
                                    idComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes
                                    , timeRes.AddMinutes(-offset).ToString("F",dtSourceEdit.Locale)
                                    , i
                                });
                        }
                    }
                }
                return dtSourceEdit;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <returns>таблица значений</returns>
            public DataTable FillTableCorValue(int offset)
            {
                int idAlg
                    , vsRatioValue = -1;
                double valueToRes;
                DateTime dtRes;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                    });

                foreach (DataGridViewRow row in Rows)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.Index > (int)INDEX_GTP.GTP36 & col.Index < (int)INDEX_GTP.CorGTP36)
                        {
                            idAlg = (int)row.Cells["ALG"].Value;
                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                            dtRes = Convert.ToDateTime(row.Cells["Date"].Value.ToString());

                            if (double.TryParse(row.Cells[col.Index].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueToRes))
                                valueToRes *= Math.Pow(10F, -1 * vsRatioValue);
                            else
                                valueToRes = -1;

                            if (valueToRes > -1)
                                dtSourceEdit.Rows.Add(new object[]
                                {
                                    col.m_iIdComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes
                                    , dtRes.AddMinutes(-offset).ToString("F",dtSourceEdit.Locale)
                                    , row.Cells["Date"].Value.ToString()
                                });
                        }
                    }
                }
                return dtSourceEdit;
            }

            /// <summary>
            /// Формирование таблицы вых. значений
            /// </summary>
            /// <returns></returns>
            public DataTable FillTableValueDay()
            {
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                int idAlg
                   , vsRatioValue = -1;
                double valueToRes = 0F;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                    });

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.Index > (int)INDEX_GTP.CorGTP12 & col.Index <= (int)INDEX_GTP.COUNT + 1)
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (Convert.ToDateTime(row.Cells["Date"].Value) <= DateTime.Now.Date)
                            {
                                idAlg = (int)row.Cells["ALG"].Value;
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                vsRatio = vsRatioValue;

                                if (double.TryParse(row.Cells[col.Index].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueToRes))
                                {
                                    valueToRes *= Math.Pow(10F, -1 * vsRatioValue);

                                    dtSourceEdit.Rows.Add(new object[]
                                    {
                                        col.m_iIdComp
                                        , -1
                                        , 1.ToString()
                                        , valueToRes
                                        , Convert.ToDateTime(row.Cells["Date"].Value.ToString()).ToString("F",dtSourceEdit.Locale)
                                        , row.Cells["Date"].Value.ToString()
                                    });
                                }
                            }
                        }
                }
                return dtSourceEdit;
            }
        }

        /// <summary>
        /// калькулятор значений
        /// </summary>
        public class TaskAutobookCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            public DataTable[] calcTable;
            /// <summary>
            /// выходные значения
            /// </summary>
            public List<string> value;

            /// <summary>
            /// 
            /// </summary>
            public TaskAutobookCalculate()
            {
                calcTable = new DataTable[(int)INDEX_GTP.COUNT];
                value = new List<string>((int)INDEX_GTP.COUNT);
            }

            /// <summary>
            /// Суммирование значений ТГ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            /// <returns>отредактированое значение</returns>
            private double sumTG(DataTable tb_gtp)
            {
                double value = 0;

                foreach (DataRow item in tb_gtp.Rows)
                    value += Convert.ToDouble(item[@"VALUE"].ToString());

                return value;
            }

            /// <summary>
            /// разбор данных по гтп
            /// </summary>
            /// <param name="dtOrigin">таблица с данными</param>
            /// /// <param name="dtOut">таблица с параметрами</param>
            public void getTable(DataTable[] dtOrigin, DataTable dtOut)
            {
                int i = 0
                , count = 0;

                calcTable[(int)INDEX_GTP.GTP12] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                calcTable[(int)INDEX_GTP.TEC] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                calcTable[(int)INDEX_GTP.GTP36] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                //
                var m_enumDT = (from r in dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].AsEnumerable()
                                orderby r.Field<DateTime>("WR_DATETIME")
                                select new
                                {
                                    DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                }).Distinct();

                for (int j = 0; j < m_enumDT.Count(); j++)
                {
                    i = 0;
                    calcTable[(int)INDEX_GTP.GTP12].Rows.Clear();
                    calcTable[(int)INDEX_GTP.GTP36].Rows.Clear();

                    DataRow[] drOrigin =
                        dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].
                        Select(String.Format(dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Locale
                        , "WR_DATETIME = '{0:o}'", m_enumDT.ElementAt(j).DATE_TIME));

                    foreach (DataRow row in drOrigin)
                    {
                        if (i < 2)
                        {
                            calcTable[(int)INDEX_GTP.GTP12].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        }
                        else
                            calcTable[(int)INDEX_GTP.GTP36].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        i++;
                    }

                    calculate(calcTable);

                    for (int t = 0; t < value.Count(); t++)
                    {
                        calcTable[(int)INDEX_GTP.TEC].Rows.Add(new object[]
                        {
                            dtOut.Rows[t]["ID"]
                            ,dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["ID_SESSION"]
                            ,dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["QUALITY"]
                            ,value[t]
                            ,Convert.ToDateTime(String.Format(dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Locale
                            ,m_enumDT.ElementAt(j).DATE_TIME.ToString()))
                            ,count
                        });
                        count++;
                    }
                }
            }

            /// <summary>
            /// Вычисление парамтеров ГТП и ТЭЦ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            private void calculate(DataTable[] tb_gtp)
            {
                double fTG12 = 0
                    , fTG36 = 0
                    , fTec = 0;

                if (value.Count() > 0)
                    value.Clear();

                for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_GTP.GTP12:
                            fTG12 = sumTG(tb_gtp[i]);
                            value.Add(fTG12.ToString());
                            break;
                        case (int)INDEX_GTP.GTP36:
                            fTG36 = sumTG(tb_gtp[i]);
                            value.Add(fTG36.ToString());
                            break;
                        case (int)INDEX_GTP.TEC:
                            fTec = fTG12 + fTG36;
                            value.Add(fTec.ToString());
                            break;
                        default:
                            break;
                    }
                }
            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Класс по работе с формированием 
        /// и отправкой отчета NSS
        /// </summary>
        public class ReportsToNSS
        {
            /// <summary>
            /// Экземпляр класса
            /// </summary>
            CreateMessage m_crtMsg;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            public ReportsToNSS()
            {
                m_crtMsg = new CreateMessage();
            }

            /// <summary>
            /// Класс создания письма
            /// </summary>
            private class CreateMessage
            {
                /// <summary>
                /// 
                /// </summary>
                Outlook.Application m_oApp;

                /// <summary>
                /// конструктор(основной)
                /// </summary>
                public CreateMessage()
                {
                    m_oApp = new Outlook.Application();
                }

                /// <summary>
                /// Формирование письма
                /// </summary>
                /// <param name="subject">тема письма</param>
                /// <param name="body">тело сообщения</param>
                /// <param name="to">кому/куда</param>
                public void FormingMessage(string subject, string body, string to)
                {
                    try
                    {
                        Outlook.MailItem newMail = (Outlook.MailItem)m_oApp.CreateItem(Outlook.OlItemType.olMailItem);
                        newMail.To = to;
                        newMail.Subject = subject;
                        newMail.Body = body;
                        newMail.Importance = Outlook.OlImportance.olImportanceNormal;
                        newMail.Display();
                        sendMail(newMail);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="mail"></param>
                private void sendMail(Outlook.MailItem mail)
                {
                    //отправка
                    mail.Send();
                }

                /// <summary>
                /// Прикрепление файла к письму
                /// </summary>
                /// <param name="mail"></param>
                private void AddAttachment(Outlook.MailItem mail)
                {
                    OpenFileDialog attachment = new OpenFileDialog();

                    attachment.Title = "Select a file to send";
                    attachment.ShowDialog();

                    if (attachment.FileName.Length > 0)
                    {
                        mail.Attachments.Add(
                            attachment.FileName,
                            Outlook.OlAttachmentType.olByValue,
                            1,
                            attachment.FileName);
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                private void closeOutlook()
                {
                    m_oApp.Quit();
                    Marshal.ReleaseComObject(m_oApp);
                    GC.Collect();
                }
            }

            /// <summary>
            /// Содание тела сообщения
            /// </summary>
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtSend">дата</param>
            private void createBodyToSend(ref string sbjct
                , ref string bodyMsg
                , DataTable sourceTable
                , DateTime dtSend)
            {
                DataRow[] drReportDay;
                DateTime reportDate;

                reportDate = dtSend.AddHours(6).Date;//??
                drReportDay =
                    sourceTable.Select(string.Format(sourceTable.Locale, @"WR_DATETIME = '{0:o}'", reportDate));

                if ((double)drReportDay.Length != 0)
                {
                    bodyMsg = @"BEGIN " + "\r\n"
                        + @"(DATE):" + reportDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "\r\n"
                        + @"(01): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.TEC]["VALUE"].ToString())) + ":\r\n"
                        + @"(02): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.GTP12]["VALUE"].ToString())) + ":\r\n"
                        + @"(03): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.GTP36]["VALUE"].ToString())) + ":\r\n"
                        + @"END ";
                    /*bodyMsg = @"Дата " + reportDate.ToShortDateString() + ".\r\n"
                        + @"Станция, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ";\r\n"
                        + @"Блоки 1-2, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ";\r\n"
                        + @"Блоки 3-6, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]);*/

                    sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
                }
            }

            /// <summary>
            /// Редактирование значения
            /// </summary>
            /// <param name="val">значение</param>
            /// <returns>измененное знач.</returns>
            private string fewerValue(double val)
            {
                return Convert.ToString(val * Math.Pow(10F, 1 * vsRatio));
            }

            /// <summary>
            /// Создание. Подготовка. Отправка письма.
            /// </summary>
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtSend">выбранный промежуток</param>
            /// <param name="to">получатель</param>
            public void SendMailToNSS(DataTable sourceTable, DateTime dtSend, string to)
            {
                string bodyMsg = string.Empty
                 , sbjct = string.Empty;

                createBodyToSend(ref sbjct, ref bodyMsg, sourceTable, dtSend);

                if (sbjct != "")
                    m_crtMsg.FormingMessage(sbjct, bodyMsg, to);
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            /// <summary>
            /// Экземпляп приложения Excel
            /// </summary>
            private Excel.Application m_excApp;
            /// <summary>
            /// Экземпляр книги Excel
            /// </summary>
            private Excel.Workbook m_workBook;
            /// <summary>
            /// Экземпляр листа Excel
            /// </summary>
            private Excel.Worksheet m_wrkSheet;
            private object _missingObj = Missing.Value;

            /// <summary>
            /// 
            /// </summary>
            protected enum INDEX_DIVISION : int
            {
                UNKNOW = -1,
                SEPARATE_CELL,
                ADJACENT_CELL
            }

            /// <summary>
            /// конструктор(основной)
            /// </summary>
            public ReportExcel()
            {
                m_excApp = new Excel.Application();
                m_excApp.Visible = false;
            }

            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgView">отрбражение данных</param>
            /// <param name="dtRange">дата</param>
            public void CreateExcel(DataGridView dgView, DateTimeRange dtRange)
            {
                if (addWorkBooks())
                {
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Autobook");
                    int indxRow = 1;

                    try
                    {
                        for (int i = 0; i < dgView.Columns.Count; i++)
                        {
                            if (dgView.Columns[i].HeaderText != "")
                            {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxRow];

                                foreach (Excel.Range cell in colRange.Cells)
                                    if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                                    {
                                        fillSheetExcel(colRange, dgView, i, cell.Row);
                                        break;
                                    }
                                indxRow++;
                            }
                        }
                        setPlanMonth(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        Marshal.ReleaseComObject(m_excApp);
                    }
                    catch (Exception e)
                    {
                        CloseExcel();
                        MessageBox.Show("Ошибка экспорта данных!" + e);
                    }
                }
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                //string pathToTemplate = @"D:\MyProjects\C.Net\TEP32\Tep\bin\Debug\Template\TemplateAutobook.xlsx";
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateAutobook.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
                    CloseExcel();
                    bflag = false;
                    MessageBox.Show("Отсутствует шаблон для отчета Excel");
                }
                return bflag;
            }

            /// <summary>
            /// Обработка события - закрытие экселя
            /// </summary>
            /// <param name="Cancel"></param>
            void workBook_BeforeClose(ref bool Cancel)
            {
                CloseExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                CloseExcel();
            }

            /// <summary>
            /// Добавление плана и месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setPlanMonth(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                Excel.Range exclRPL = exclWrksht.get_Range("C5");
                Excel.Range exclRMonth = exclWrksht.get_Range("A4");
                exclRPL.Value2 = dgv.Rows[dgv.Rows.Count - 1].Cells[@"PlanSwen"].Value;
                exclRMonth.Value2 = HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
            }

            /// <summary>
            /// Деление 
            /// </summary>
            /// <param name="headerTxt">строка</param>
            /// <returns>часть строки</returns>
            private string splitString(string headerTxt)
            {
                string[] spltHeader = headerTxt.Split(',');

                if (spltHeader.Length > (int)INDEX_DIVISION.ADJACENT_CELL)
                    return spltHeader[(int)INDEX_DIVISION.ADJACENT_CELL].TrimStart();
                else
                    return spltHeader[(int)INDEX_DIVISION.SEPARATE_CELL];
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне
            /// </summary>
            /// <param name="cellRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range cellRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < cellRange.Rows.Count; i++)
                    if (((Excel.Range)cellRange.Cells[i]).Value == null &&
                        ((Excel.Range)cellRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        row = i;
                        break;
                    }

                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    cellRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                    row++;
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            public void CloseExcel()
            {
                //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                m_excApp = null;
                m_workBook = null;
                m_wrkSheet = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookMonthValues(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            AutoBookCalc = new TaskAutobookCalculate();

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <returns>кол-во дней</returns>
        protected int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsParseToF(string value)
        {
            int _indxChar = 0;
            string _sepReplace = string.Empty;
            bool bFlag = true;
            //char[] _separators = { ' ', ',', '.', ':', '\t'};
            //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();
            float fValue = 0;

            foreach (char item in value.ToCharArray())
            {
                if (!char.IsDigit(item))
                    if (char.IsLetter(item))
                        value = value.Remove(_indxChar, 1);
                    else
                        _sepReplace = value.Substring(_indxChar, 1);
                else
                    _indxChar++;

                switch (_sepReplace)
                {
                    case ".":
                    case ",":
                    case " ":
                    case ":":
                        float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fValue);
                        bFlag = false;
                        break;
                }
            }

            if (bFlag)
                try
                {
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue))
                        fValue = 0;
                }
                catch (Exception)
                {
                    if (value.ToString() == "")
                        ;
                }


            return fValue;
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementAutobook : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL, CALENDAR,
                CBX_PERIOD, CBX_TIMEZONE,
                HDTP_BEGIN, HDTP_END,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CHKBX_EDIT,
                COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            /// <summary>
            /// 
            /// </summary>
            public PanelManagementAutobook()
                : base(4, 3)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            /// <summary>
            /// 
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                SuspendLayout();
                posRow = 0;
                //Период расчета - подпись
                Label lblCalcPer = new Label();
                lblCalcPer.Text = "Период расчета";
                //Период расчета - значение
                ComboBox cbxCalcPer = new ComboBox();
                cbxCalcPer.Name = INDEX_CONTROL_BASE.CBX_PERIOD.ToString();
                cbxCalcPer.DropDownStyle = ComboBoxStyle.DropDownList;
                //Часовой пояс расчета - подпись
                Label lblCalcTime = new Label();
                lblCalcTime.Text = "Часовой пояс расчета";
                //Часовой пояс расчета - значение
                ComboBox cbxCalcTime = new ComboBox();
                cbxCalcTime.Name = INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString();
                cbxCalcTime.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxCalcTime.Enabled = false;
                //
                TableLayoutPanel tlp = new TableLayoutPanel();
                tlp.AutoSize = true;
                tlp.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlp.Controls.Add(lblCalcPer, 0, 0);
                tlp.Controls.Add(cbxCalcPer, 0, 1);
                tlp.Controls.Add(lblCalcTime, 1, 0);
                tlp.Controls.Add(cbxCalcTime, 1, 1);
                tlp.AutoSize = true;
                tlp.Dock = DockStyle.Fill;
                this.Controls.Add(tlp, 0, posRow);
                SetColumnSpan(tlp, 4); this.SetRowSpan(tlp, 1);
                //
                TableLayoutPanel tlpValue = new TableLayoutPanel();
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, 35F));
                tlpValue.Dock = DockStyle.Fill;
                tlpValue.AutoSize = true;
                //tlpValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                ////Дата/время начала периода расчета - подпись
                Label lBeginCalcPer = new Label();
                lBeginCalcPer.Dock = DockStyle.Bottom;
                lBeginCalcPer.Text = @"Дата/время начала периода расчета:";
                ////Дата/время начала периода расчета - значения
                int cntDays = DateTime.DaysInMonth(s_dtDefaultAU.Year, s_dtDefaultAU.Month);
                int today = s_dtDefaultAU.Day;

                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(-(today - 1)), null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                tlpValue.Controls.Add(lBeginCalcPer, 0, 0);
                tlpValue.Controls.Add(ctrl, 0, 1);
                //Дата/время  окончания периода расчета - подпись
                Label lEndPer = new Label();
                lEndPer.Dock = DockStyle.Top;
                lEndPer.Text = @"Дата/время окончания периода расчета:";
                //Дата/время  окончания периода расчета - значение
                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(cntDays - today)
                    , tlpValue.Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);
                Controls.Add(tlpValue, 0, posRow = posRow + 1);
                this.SetColumnSpan(tlpValue, 4); this.SetRowSpan(tlpValue, 1);
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL_BASE.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка - Отправить
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL_BASE.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                //ctrlBSend.Enabled = false;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL_BASE.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //Кнопка - Экспорт
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;
                //Поле с почтой
                TextBox ctrlTxt = new TextBox();
                ctrlTxt.Name = INDEX_CONTROL_BASE.TXTBX_EMAIL.ToString();
                //ctrlTxt.Text = @"Pasternak_AS@sibeco.su";
                ctrlTxt.Dock = DockStyle.Top;
                //Календарь
                DateTimePicker dtPickerCalendar = new DateTimePicker();
                dtPickerCalendar.Name = INDEX_CONTROL_BASE.CALENDAR.ToString();
                dtPickerCalendar.Dock = DockStyle.Fill;
                dtPickerCalendar.DropDownAlign = LeftRightAlignment.Right;
                dtPickerCalendar.Format = DateTimePickerFormat.Custom;
                dtPickerCalendar.Width = 125;
                dtPickerCalendar.CustomFormat = "dd MMM, yyyy";
                //табло кнопок
                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 0, 1);
                tlpButton.Controls.Add(ctrlTxt, 1, 0);
                tlpButton.Controls.Add(ctrlBSend, 1, 2);
                tlpButton.Controls.Add(ctrlExp, 0, 2);
                tlpButton.Controls.Add(dtPickerCalendar, 1, 1);
                Controls.Add(tlpButton, 0, posRow = posRow + 2);
                SetColumnSpan(tlpButton, 4); SetRowSpan(tlpButton, 3);
                //Признак Корректировка_включена/корректировка_отключена 
                CheckBox cBox = new CheckBox();
                cBox.Name = INDEX_CONTROL_BASE.CHKBX_EDIT.ToString();
                cBox.Text = @"Корректировка значений разрешена";
                cBox.Dock = DockStyle.Top;
                cBox.Enabled = false;
                cBox.Checked = true;
                Controls.Add(cBox, 0, posRow = posRow + 1);
                SetColumnSpan(cBox, 4); SetRowSpan(cBox, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;

                if (!(DateTimeRangeValue_Changed == null))
                    DateTimeRangeValue_Changed(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
            }

            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name="idPeriod"></param>
            public void SetPeriod(ID_PERIOD idPeriod)
            {
                HDateTimePicker hdtpBtimePer = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                , hdtpEndtimePer = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;

                int cntDays = DateTime.DaysInMonth(hdtpBtimePer.Value.Year,
                   hdtpBtimePer.Value.Month);
                int today = hdtpBtimePer.Value.Day;

                //Выполнить запрос на получение значений для заполнения 'DataGridView'
                switch (idPeriod)
                {
                    case ID_PERIOD.HOUR:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , DateTime.Now.Hour
                            , 0
                            , 0).AddHours(-1);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddHours(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.HOUR;
                        break;
                    //case ID_PERIOD.SHIFTS:
                    //    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    //    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    //    break;
                    case ID_PERIOD.DAY:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , 0
                            , 0
                            , 0);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddDays(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.DAY;
                        break;
                    case ID_PERIOD.MONTH:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , 1
                            , 0
                            , 0
                            , 0);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddDays(cntDays - 1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.MONTH;
                        break;
                    case ID_PERIOD.YEAR:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , 1
                            , 1
                            , 0
                            , 0
                            , 0).AddYears(-1);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddYears(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.YEAR;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// инициализация объектов
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvTEPValues = 4;

            SuspendLayout();

            posRow = 0;
            m_dgvAB = new DGVAutoBook(INDEX_CONTROL.DGV_DATA.ToString());
            m_dgvAB.Dock = DockStyle.Fill;
            m_dgvAB.Name = INDEX_CONTROL.DGV_DATA.ToString();
            m_dgvAB.AllowUserToResizeRows = false;
            m_dgvAB.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dgvAB.AddColumn("Корректировка ПТО,\r\nБлоки 1-2", true, INDEX_GTP.CorGTP12.ToString());
            m_dgvAB.AddColumn("Корректировка ПТО,\r\nБлоки 3-6", true, INDEX_GTP.CorGTP36.ToString());
            m_dgvAB.AddColumn("Блоки 1-2", true, INDEX_GTP.GTP12.ToString());
            m_dgvAB.AddColumn("Блоки 3-6", true, INDEX_GTP.GTP36.ToString());
            m_dgvAB.AddColumn("Станция,\r\nсутки", true, INDEX_GTP.TEC.ToString());
            m_dgvAB.AddColumn("Станция,\r\nнараст.", true, "StSwen");
            m_dgvAB.AddColumn("План нараст.", true, "PlanSwen");
            m_dgvAB.AddColumn("Отклонение от плана", true, "DevOfPlan");
            Controls.Add(m_dgvAB, 4, posRow);
            SetColumnSpan(m_dgvAB, 9); SetRowSpan(m_dgvAB, 10);
            //
            Controls.Add(PanelManagementAB, 0, posRow);
            SetColumnSpan(PanelManagementAB, posColdgvTEPValues);
            SetRowSpan(PanelManagementAB, RowCount);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskAutobookMonthValue_btnsend_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskAutobookMonthValues_btnexport_Click);
            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CALENDAR.ToString(), true)[0] as Button).

            m_dgvAB.CellParsing += dgvAB_CellParsing;
            //m_dgvAB.SelectionChanged += dgvAB_SelectionChanged;
        }

        /// <summary>
        /// обработка события - Выбор строки
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        void dgvAB_SelectionChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < (sender as DataGridView).SelectedRows.Count; i++)
                if ((sender as DataGridView).SelectedRows[i].Cells["Date"].Value != null)
                {
                    DateTime dtRow = Convert.ToDateTime((sender as DataGridView).SelectedRows[i].Cells["Date"].Value);
                    HDateTimePicker datetimePicker =
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                    m_bflgClear = false;
                    datetimePicker.Value = dtRow;
                }
        }

        /// <summary>
        /// обработка ЭКСПОРТА(временно)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void PanelTaskAutobookMonthValues_btnexport_Click(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();
            m_rptExcel.CreateExcel(m_dgvAB, Session.m_rangeDatetime);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void PanelTaskAutobookMonthValue_btnsend_Click(object sender, EventArgs e)
        {
            m_rptsNSS = new ReportsToNSS();

            string toSend = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.TXTBX_EMAIL.ToString(), true)[0] as TextBox).Text;
            DateTime dtSend = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CALENDAR.ToString(), true)[0] as DateTimePicker).Value;
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = m_dgvAB.FillTableValueDay();
            //
            m_rptsNSS.SendMailToNSS(m_TableEdit, dtSend, toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void dgvAB_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            int numMonth = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month
                , day = m_dgvAB.Rows.Count;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            switch (m_dgvAB.Columns[e.ColumnIndex].Name)
            {
                case "CorGTP12":
                    //корректировка значений
                    m_dgvAB.editCells(e, INDEX_GTP.GTP12.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = m_dgvAB.FillTableCorValue(Session.m_curOffsetUTC, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = m_dgvAB.FillTableValueDay();
                    break;
                case "CorGTP36":
                    //корректировка значений
                    m_dgvAB.editCells(e, INDEX_GTP.GTP36.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = m_dgvAB.FillTableCorValue(Session.m_curOffsetUTC, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = m_dgvAB.FillTableValueDay();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            deleteSession();

            base.Stop();
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr"></param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = HandlerDb.GetDataOutval(arQueryRanges, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            //Получение значений корр. input
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                HandlerDb.GetCorInPut(Type
                , arQueryRanges
                , ActualIdPeriod
                , out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , CountBasePeriod
                        , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
                        , ref m_arTableOrigin
                        , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                        , out err, out strErr);
                else
                    strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
                        + @" по " + Session.m_rangeDatetime.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
                    + @" по " + Session.m_rangeDatetime.End.ToString();
        }

        /// <summary>
        /// copy
        /// </summary>
        private void setValues()
        {
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Clone();
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                = m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]
              = m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE].Clone();
        }

        /// <summary>
        /// Проверка выбранного диапазона
        /// </summary>
        /// <param name="dtRange">диапазон дат</param>
        /// <returns></returns>
        private bool rangeCheking(DateTimeRange[] dtRange)
        {
            bool bflag = false;

            for (int i = 0; i < dtRange.Length; i++)
                if (dtRange[i].Begin.Month > DateTime.Now.Month)
                    if (dtRange[i].End.Year >= DateTime.Now.Year)
                        bflag = true;

            return bflag;
        }

        /// <summary>
        /// Загрузка сырых значений
        /// </summary>
        /// <param name="typeValues">тип загружаемых значений</param>
        private void updateDataValues(HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
        {
            int err = -1
                , cnt = CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            //if (rangeCheking(dtrGet))
            //    MessageBox.Show("Выбранный диапазон месяцев неверен");
            //else
            //{
                clear();
                m_handlerDb.RegisterDbConnection(out iRegDbConn);

                if (!(iRegDbConn < 0))
                {
                    // установить значения в таблицах для расчета, создать новую сессию
                    setValues(dtrGet, out err, out errMsg);

                    if (err == 0)
                    {
                        if (m_TableOrigin.Rows.Count > 0)
                        {
                            // создать копии для возможности сохранения изменений
                            setValues();
                            //вычисление значений
                            AutoBookCalc.getTable(m_arTableOrigin, HandlerDb.GetOutPut(out err));
                            //
                            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                                AutoBookCalc.calcTable[(int)INDEX_GTP.TEC].Copy();
                            //запись выходных значений во временную таблицу
                            HandlerDb.insertOutValues(out err, AutoBookCalc.calcTable[(int)INDEX_GTP.TEC]);
                            // отобразить значения
                            m_dgvAB.ShowValues(m_arTableOrigin
                                , HandlerDb.GetPlanOnMonth(Type
                                , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
                                , ActualIdPeriod
                                , out err)
                                , typeValues);
                            //формирование таблиц на основе грида
                            valuesFence();
                        }
                        else
                            deleteSession();
                    }
                    else
                    {
                        // в случае ошибки "обнулить" идентификатор сессии
                        deleteSession();
                        throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                    }
                }
                else
                    deleteSession();

                if (!(iRegDbConn > 0))
                    m_handlerDb.UnRegisterDbConnection();
            //}
        }

        /// <summary>
        /// Загрузка архивных значений
        /// </summary>
        /// <param name="typeValues">тип загружаемых значений</param>
        private void loadArchValues(HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
        {
            int err = -1,
                cnt = CountBasePeriod,
                iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            clear();
            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (true)
                    {
                        //запись выходных значений во временную таблицу
                        //HandlerDb.insertOutValues(out err, m_arTableOrigin[(int)typeValues]);
                        // отобразить значения
                        m_dgvAB.ShowValues(m_arTableOrigin
                            , HandlerDb.GetPlanOnMonth(Type
                            , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
                            , ActualIdPeriod
                            , out err)
                            , typeValues);
                        //формирование таблиц на основе грида
                        valuesFence();
                    }
                    else;
                }
                else
                {
                    deleteSession();
                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            }
            else deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
        }

        /// <summary>
        /// формирование таблиц данных
        /// </summary>
        private void valuesFence()
        {
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
            //сохранить вых. знач. в DataTable
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                m_dgvAB.FillTableValueDay();
            //сохранить вх.корр. знач. в DataTable
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_dgvAB.FillTableCorValue(Session.m_curOffsetUTC);
        }

        /// <summary>
        /// Количество базовых периодов
        /// </summary>
        protected int CountBasePeriod
        {
            get
            {
                int iRes = -1;
                ID_PERIOD idPeriod = ActualIdPeriod;

                iRes =
                    idPeriod == ID_PERIOD.HOUR ?
                        (int)(Session.m_rangeDatetime.End - Session.m_rangeDatetime.Begin).TotalHours - 0 :
                        idPeriod == ID_PERIOD.DAY ?
                            (int)(Session.m_rangeDatetime.End - Session.m_rangeDatetime.Begin).TotalDays - 0 :
                            24;

                return iRes;
            }
        }

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev"></param>
        private void HPanelAutobook_btnHistory_Click(object obj, EventArgs ev)
        {
            m_ViewValues = HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE;
            onButtonLoadClick();
        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            switch (m_ViewValues)
            {
                case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.UNKNOWN:
                    break;
                case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION:
                    updateDataValues(m_ViewValues);
                    break;
                case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE:
                    loadArchValues(m_ViewValues);
                    break;
                case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION;
            onButtonLoadClick();
        }

        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }

        /// <summary>
        /// Активация/Деактивация вкладки
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public override bool Activate(bool activate)
        {
            bool bRes = false;
            int err = -1;

            bRes = base.Activate(activate);

            if (bRes == true)
                if (activate == true)
                    HandlerDb.InitSession(out err);

            return bRes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1
                , tCount = 0;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            INDEX_ID[] arIndxIdToAdd =
                new INDEX_ID[]
                {
                        //INDEX_ID.DENY_COMP_CALCULATED,
                        INDEX_ID.DENY_COMP_VISIBLED
                };

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = HTepUsers.Role;

            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    case INDEX_ID.ALL_COMPONENT:
                        m_arListIds[(int)id] = new List<int> { };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }

            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj = getQueryDictPrj();
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrjs[i] = m_handlerDb.Select(arQueryDictPrj[i], out err);

                if (!(err == 0))
                    break;
            }

            bool[] arChecked = new bool[arIndxIdToAdd.Length];
            Array namePut = Enum.GetValues(typeof(INDEX_GTP));

            foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
            {
                id_comp = (int)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);

                m_dgvAB.AddIdComp(id_comp, namePut.GetValue(tCount).ToString());
                tCount++;
            }

            try
            {
                if (m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)INDEX_CONTROL.DGV_DATA).ToString()].Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)INDEX_CONTROL.DGV_DATA).ToString()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                    else
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                }
                else
                    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                if ((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                        for (int j = (int)INDEX_GTP.CorGTP12; j < (int)INDEX_GTP.COUNT; j++)
                            m_dgvAB.AddBRead(false, namePut.GetValue(j).ToString());
              
            }
            catch(Exception)
            {

            }


            try
            {
                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                }
                else
                    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            }
            catch (Exception)
            {

            }

            m_dgvAB.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

            if (err == 0)
            {
                try
                {
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]);
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
                    Session.SetCurrentPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
                    (PanelManagementAB as PanelManagementAutobook).SetPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
                    (ctrl as ComboBox).Enabled = false;

                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.TXTBX_EMAIL.ToString(), true)[0];
                    //из profiles
                    ctrl.Text = m_dictProfile.Objects[((int)PanelManagementAutobook.INDEX_CONTROL_BASE.TXTBX_EMAIL).ToString()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.MAIL).ToString()];
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                switch ((INDEX_TABLE_DICTPRJ)i)
                {
                    case INDEX_TABLE_DICTPRJ.PERIOD:
                        errMsg = @"Получение интервалов времени для периода расчета";
                        break;
                    case INDEX_TABLE_DICTPRJ.TIMEZONE:
                        errMsg = @"Получение списка часовых поясов";
                        break;
                    case INDEX_TABLE_DICTPRJ.COMPONENT:
                        errMsg = @"Получение списка компонентов станции";
                        break;
                    case INDEX_TABLE_DICTPRJ.PARAMETER:
                        errMsg = @"Получение строковых идентификаторов параметров в алгоритме расчета";
                        break;
                    //case INDEX_TABLE_DICTPRJ.MODE_DEV:
                    //    errMsg = @"Получение идентификаторов режимов работы оборудования";
                    //    break;
                    //case INDEX_TABLE_DICTPRJ.MEASURE:
                    //    errMsg = @"Получение информации по единицам измерения";
                    //    break;
                    default:
                        errMsg = @"Неизвестная ошибка";
                        break;
                }
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            setCurrentTimeZone(obj as ComboBox);
            // очистить содержание представления
            clear();
            m_currentOffSet = Session.m_curOffsetUTC;
        }

        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            int err = -1
             , id_alg = -1
             , ratio = -1
             , round = -1;
            string n_alg = string.Empty;
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);
            Session.SetRangeDatetime(dtBegin, dtEnd);
            // очистить содержание представления
            if (m_bflgClear)
            {
                clear();

                dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
                    , new int[] {
                        m_id_panel
                        , (int)Session.m_currIdPeriod }
                        , out err);

                IEnumerable<DataRow> listParameter = ListParameter.Select(x => x);

                foreach (DataRow r in listParameter)
                {
                    id_alg = (int)r[@"ID_ALG"];
                    n_alg = r[@"N_ALG"].ToString().Trim();
                    // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                    if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                        // добавить в список идентификатор параметра алгоритма расчета
                        m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);
                }

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg.Trim()) == true)
                {// установленные в проекте
                    ratio = dictVisualSettings[n_alg.Trim()].m_ratio;
                    round = dictVisualSettings[n_alg.Trim()].m_round;
                }
                else
                {// по умолчанию
                    ratio = HTepUsers.s_iRatioDefault;
                    round = HTepUsers.s_iRoundDefault;
                }

                m_dgvAB.ClearRows();
                //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
                //заполнение представления
                for (int i = 0; i < DaysInMonth; i++)
                {
                    m_dgvAB.AddRow(new DGVAutoBook.ROW_PROPERTY()
                    {
                        m_idAlg = id_alg
                                ,
                        //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        //,
                        m_Value = dt.AddDays(i).ToShortDateString()
                                ,
                        m_vsRatio = ratio
                                ,
                        m_vsRound = round
                    });
                }
            }
            m_currentOffSet = Session.m_curOffsetUTC;
            m_bflgClear = true;
        }

        /// <summary>
        /// Установка длительности периода 
        /// </summary>
        private void settingDateRange()
        {
            int cntDays,
                today = 0;

            PanelManagementAB.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
              (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
                (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

            PanelManagementAB.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

            //m_bflgDTRange = false;

        }

        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Select().ToList<DataRow>();

                return listRes;
            }
        }

        /// <summary>
        /// очистка грида
        /// </summary>
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected void clear(int iCtrl = (int)INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;

            deleteSession();
            //??? повторная проверка
            if (bClose == true)
            {
                if (!(m_arTableDictPrjs == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                    {
                        if (!(m_arTableDictPrjs[i] == null))
                        {
                            m_arTableDictPrjs[i].Clear();
                            m_arTableDictPrjs[i] = null;
                        }
                    }

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                m_dgvAB.ClearRows();
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                m_dgvAB.ClearValues();
        }

        /// <summary>
        /// удаление сессии и очистка таблиц 
        /// с временными данными
        /// </summary>
        protected void deleteSession()
        {
            int err = -1;

            HandlerDb.DeleteSession(out err);
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            int idTimezone = m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex];

            Session.SetCurrentTimeZone((ID_TIMEZONE)idTimezone
                , (int)m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagementAB as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);

            // очистить содержание представления
            clear();
        }

        /// <summary>
        /// обработку события - изменение начала/окончания даты/времени
        /// </summary>
        /// <param name="active">параметр активации</param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagementAB == null))
                if (active == true)
                    PanelManagementAB.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagementAB.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }

        /// <summary>
        /// формирование запросов 
        /// для справочных данных
        /// </summary>
        /// <returns>запрос</returns>
        private string[] getQueryDictPrj()
        {
            string[] arRes = null;

            arRes = new string[]
            {
                //PERIOD
                HandlerDb.GetQueryTimePeriods(m_strIdPeriods)
                //TIMEZONE
                , HandlerDb.GetQueryTimezones(m_strIdTimezones)
                // список компонентов
                , HandlerDb.GetQueryComp(Type)
                // параметры расчета
                //, HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                //, HandlerDb.GetQueryModeDev()
                //// единицы измерения
                , m_handlerDb.GetQueryMeasures()
                // коэффициенты для единиц измерения
                , HandlerDb.GetQueryRatio()
            };

            return arRes;
        }

        /// <summary>
        /// Строка для запроса информации по периодам расчетов
        /// </summary>        
        protected string m_strIdPeriods
        {
            get
            {
                string strRes = string.Empty;

                for (int i = 0; i < m_arListIds[(int)INDEX_ID.PERIOD].Count; i++)
                    strRes += m_arListIds[(int)INDEX_ID.PERIOD][i] + @",";
                strRes = strRes.Substring(0, strRes.Length - 1);

                return strRes;
            }
        }

        /// <summary>
        /// Строка для запроса информации по часовым поясам
        /// </summary>        
        protected string m_strIdTimezones
        {
            get
            {
                string strRes = string.Empty;

                for (int i = 0; i < m_arListIds[(int)INDEX_ID.TIMEZONE].Count; i++)
                    strRes += m_arListIds[(int)INDEX_ID.TIMEZONE][i] + @",";
                strRes = strRes.Substring(0, strRes.Length - 1);

                return strRes;
            }
        }

        /// <summary>
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;
            //сбор значений
            valuesFence();
            Control ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];

            m_arTableOrigin[(int)m_ViewValues] = getStructurOutval(
                GetNameTableOut((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value), out err);

            m_arTableEdit[(int)m_ViewValues] = HandlerDb.SaveResOut(m_arTableOrigin[(int)m_ViewValues]
                , m_arTableEdit[(int)m_ViewValues]
                , (ctrl as ComboBox).SelectedIndex
                , out err);

            base.HPanelTepCommon_btnSave_Click(obj, ev);

            if (m_TableEdit.Rows.Count > 0)
                //save вх. значений
                saveInvalValue((ctrl as ComboBox).SelectedIndex, out err);
        }

        /// <summary>
        /// получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(string nameTable, out int err)
        {
            string strRes = string.Empty;

            strRes = "SELECT * FROM " + nameTable;

            return HandlerDb.Select(strRes, out err);
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

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        private string getNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = HandlerDbValues.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;
            //
            sortingDataToTable(HandlerDb.GetDataOutvalFull(HandlerDb.getDateTimeRangeExtremeVal(), out err)
                , m_TableEdit
                , GetNameTableOut((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value)
                , @""
                , out err);
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="m_origin">оригинальная таблица</param>
        /// <param name="m_edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable m_origin, DataTable m_edit, string unCol, out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME"
                    , unCol
                    , m_origin
                    , m_edit
                    , out err);
        }

        /// <summary>
        /// разбор данных по разным табилца(взависимости от месяца)
        /// </summary>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="unCol">столбец, неучаствующий в InsertUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void sortingDataToTable(DataTable origin
            , DataTable edit
            , string nameTable
            , string unCol
            , out int err)
        {
            string nameTableExtrmRow = string.Empty
                          , nameTableNew = string.Empty;
            DataTable editTemporary = new DataTable()
                , originTemporary = new DataTable();

            err = -1;
            editTemporary = edit.Clone();
            originTemporary = origin.Clone();
            nameTableNew = nameTable;

            foreach (DataRow row in edit.Rows)
            {
                nameTableExtrmRow = extremeRow(row["DATE_TIME"].ToString(), nameTableNew);

                if (nameTableExtrmRow != nameTableNew)
                {
                    foreach (DataRow rowOrigin in origin.Rows)
                        if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month != Convert.ToDateTime(row["DATE_TIME"]).Month)
                            originTemporary.Rows.Add(rowOrigin.ItemArray);

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);//сохранение данных

                    nameTableNew = nameTableExtrmRow;
                    editTemporary.Rows.Clear();
                    originTemporary.Rows.Clear();
                    editTemporary.Rows.Add(row.ItemArray);
                }
                else
                    editTemporary.Rows.Add(row.ItemArray);
            }

            if (editTemporary.Rows.Count > 0)
            {
                foreach (DataRow rowOrigin in origin.Rows)
                    if (extremeRow(Convert.ToDateTime(rowOrigin["DATE_TIME"]).ToString(), nameTableNew) == nameTableNew)
                        originTemporary.Rows.Add(rowOrigin.ItemArray);

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);//сохранение данных
            }
        }

        /// <summary>
        /// Нахождение имени таблицы для крайних строк
        /// </summary>
        /// <param name="strDate">дата</param>
        /// <param name="nameTable">изначальное имя таблицы</param>
        /// <returns>имя таблицы</returns>
        private static string extremeRow(string strDate, string nameTable)
        {
            DateTime dtStr = Convert.ToDateTime(strDate);
            string m_nametable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + m_nametable;
        }

        /// <summary>
        /// Сохранение входных знчений(корр. величины)
        /// </summary>
        /// <param name="err"></param>
        private void saveInvalValue(int timeZone, out int err)
        {
            //err = -1;
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                HandlerDb.GetInPutID(Type, dtrPer, ActualIdPeriod, out err);

            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            HandlerDb.SaveResInval(m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
            , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], timeZone, out err);

            sortingDataToTable(m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , getNameTableIn(dtrPer[0].Begin)
                , @"ID"
                , out err);
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }
    }
}