using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskAutobook
{
    public class PanelTaskAutobookYearlyPlan : HPanelTepCommon
    {
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// 
        /// </summary>
        protected enum PROFILE_INDEX
        {
            UNKNOW = -1,
            TIMEZONE = 101, MAIL, PERIOD,
            RATIO = 201, ROUND, EDIT_COLUMN = 204,
        }
        /// <summary>
        /// 
        /// </summary>
        public static string[] GetMonth =
        {
            "Январь", "Февраль", "Март", "Апрель",
            "Май", "Июнь", "Июль", "Август", "Сентябрь",
            "Октябрь", "Ноябрь", "Декабрь"//, "Январь сл. года"
        };
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskAutobookYarlyPlanCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskAutobookYarlyPlanCalculate; } }
        /// <summary>
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        protected enum INDEX_VIEW_VALUES : short
        {
            UNKNOWN = -1, SOURCE,
            ARCHIVE, COUNT
        }
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_PLANEYAR,
            LABEL_DESC, LABEL_YEARPLAN
        }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD, // идентификаторы периодов расчетов, использующихся на форме
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //    , DENY_COMP_CALCULATED,
            //DENY_PARAMETER_CALCULATED // запрещенных для расчета
            //    , DENY_COMP_VISIBLED,
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            , PERIOD, TIMEZONE, COMPONENT,
            PARAMETER, //_IN, PARAMETER_OUT
            MODE_DEV/*, MEASURE*/,
            RATIO,
            COUNT
        }
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.MONTH : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected INDEX_VIEW_VALUES m_ViewValues;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Отображение значений в табличном представлении(план)
        /// </summary>
        protected DGVAutoBook m_dgvYear;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        private PanelManagementAutobook createPanelManagement()
        {
            return new PanelManagementAutobook();
        }

        private PanelManagementAutobook _panelManagement;
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementAutobook PanelManagementYear
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement;
            }
        }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskAutobookYarlyPlanCalculate();
        }

        /// <summary>
        /// Панель отображения значений 
        /// и их обработки
        /// </summary>
        protected class DGVAutoBook : DataGridView
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, MONTH_NAME, COUNT }
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

            /// <summary>
            /// основной конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DGVAutoBook(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            private void InitializeComponents(string nameDGV)
            {
                this.Name = nameDGV;
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
                //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                AddColumn(-3, string.Empty, "ALG", true, false);
                AddColumn(-2, "Дата", "DATE", true, false);
                AddColumn(-1, "Месяц", "Month", true, true);
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
            /// <param name="bRead"></param>
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
            /// <param name="id_comp">номер компонента</param>
            /// <param name="txtHeader">заголовок столбца</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="bRead">"только чтение"</param>
            /// <param name="bVisibled">видимость столбца</param>
            public void AddColumn(int id_comp, string txtHeader, string nameCol, bool bRead, bool bVisibled)
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
                        else
                            ;

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = id_comp, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (id_comp < 0)
                        {// для служебных столбцов
                            if (bVisibled == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }
                            column.Frozen = true;
                            column.ReadOnly = true;
                        }
                    }

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
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + id_comp + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow()
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                i = Rows.Add(row);
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
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.MONTH_NAME].Value = rowProp.m_Value;
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
                foreach (HDataGridViewColumn c in Columns)
                    if (c.m_iIdComp > 0)
                        foreach (DataGridViewRow row in Rows)
                            row.Cells[c.Index].Value = null;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            public void ShowValues(DataTable tbOrigin, DataGridView dgvView)
            {
                double dblVal = -1F;
                int idAlg = -1
                    , vsRatioValue = -1;

                ClearValues();

                foreach (HDataGridViewColumn col in Columns)
                    if (col.Index > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                            for (int j = 0; j < tbOrigin.Rows.Count; j++)
                            {
                                idAlg = (int)row.Cells["ALG"].Value;

                                if (row.Cells["Month"].Value.ToString() ==
                                    GetMonth.ElementAt(Convert.ToDateTime(tbOrigin.Rows[j]["WR_DATETIME"]).AddMonths(-1).Month - 1))
                                {
                                    double.TryParse(tbOrigin.Rows[j]["VALUE"].ToString(), out dblVal);
                                    vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                    dblVal *= Math.Pow(10F, 1 * vsRatioValue);

                                    row.Cells[col.Index].Value =
                                     dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
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
            /// Формирвоание значений
            /// </summary>
            /// <param name="idSession">номер сессии</param>
            public DataTable FillTableEdit(int idSession)
            {
                int i = 0
                    , idAlg = -1
                    , vsRatioValue = -1;
                double valueToRes;

                DataTable editTable = new DataTable();
                editTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_iIdComp > 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            idAlg = (int)row.Cells["ALG"].Value;
                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                            if (row.Cells[col.Index].Value != null)
                                if (double.TryParse(row.Cells[col.Index].Value.ToString(), out valueToRes))
                                    editTable.Rows.Add(new object[]
                                    {
                                        col.m_iIdComp
                                        , idSession
                                        , 1.ToString()
                                        , valueToRes *= Math.Pow(10F, -1 * vsRatioValue)
                                        , Convert.ToDateTime(row.Cells["DATE"].Value.ToString()).ToString("F",editTable.Locale)
                                        , i
                                    });
                            i++;
                        }
                return editTable;
            }
        }

        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementAutobook : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                    , BUTTON_SEND, BUTTON_SAVE,
                BUTTON_LOAD,
                TXTBX_EMAIL
                , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN,
                HDTP_END
                    , COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            public PanelManagementAutobook()
                : base(4, 3)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                //int posColdgvTEPValues = 6;
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
                tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlp.Controls.Add(lblCalcPer, 0, 0);
                tlp.Controls.Add(cbxCalcPer, 0, 1);
                tlp.Controls.Add(lblCalcTime, 1, 0);
                tlp.Controls.Add(cbxCalcTime, 1, 1);
                this.Controls.Add(tlp, 0, posRow);
                this.SetColumnSpan(tlp, 4); this.SetRowSpan(tlp, 1);
                //
                TableLayoutPanel tlpValue = new TableLayoutPanel();
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.Dock = DockStyle.Fill;
                tlpValue.AutoSize = true;
                tlpValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                ////Дата/время начала периода расчета - подпись
                Label lBeginCalcPer = new Label();
                lBeginCalcPer.Dock = DockStyle.Bottom;
                lBeginCalcPer.Text = @"Дата/время начала периода расчета:";
                ////Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker(s_dtDefaultAU, null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                tlpValue.Controls.Add(lBeginCalcPer, 0, 0);
                tlpValue.Controls.Add(ctrl, 0, 1);
                //Дата/время  окончания периода расчета - подпись
                Label lEndPer = new Label();
                lEndPer.Dock = DockStyle.Top;
                lEndPer.Text = @"Дата/время  окончания периода расчета:";
                //Дата/время  окончания периода расчета - значение
                ctrl = new HDateTimePicker(s_dtDefaultAU.AddMonths(1)
                    , tlpValue.Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);
                this.Controls.Add(tlpValue, 0, posRow = posRow + 1);
                this.SetColumnSpan(tlpValue, 4); this.SetRowSpan(tlpValue, 1);
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL_BASE.BUTTON_LOAD.ToString();
                //ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                //indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                //ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString();
                //indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                //ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка 
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL_BASE.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                ctrlBSend.Enabled = false;
                ctrlBSend.Visible = false;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL_BASE.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 1, 0);
                tlpButton.Controls.Add(ctrlBSend, 0, 1);
                //tlpButton.Controls.Add(ctrlTxt, 1, 1);
                this.Controls.Add(tlpButton, 0, posRow = posRow + 2);
                this.SetColumnSpan(tlpButton, 4); this.SetRowSpan(tlpButton, 2);

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
            /// 
            /// </summary>
            /// <param name="idPeriod"></param>
            public void SetPeriod(ID_PERIOD idPeriod)
            {
                HDateTimePicker hdtpBtimePer = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                , hdtpEndtimePer = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
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
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddMonths(1);
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
                            , 0).AddYears(0);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddMonths(11);
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
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookYearlyPlan(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;

            m_arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddMonths(1));
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

            m_dgvYear = new DGVAutoBook(INDEX_CONTROL.DGV_PLANEYAR.ToString());
            m_dgvYear.Dock = DockStyle.Fill;
            m_dgvYear.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dgvYear.AllowUserToResizeRows = false;
            m_dgvYear.AddColumn("Выработка, тыс. кВтч", false, "Output");

            m_dgvYear.Columns["DATE"].Visible = false;
            foreach (DataGridViewColumn column in m_dgvYear.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_dgvYear.CellEndEdit += dgvYear_CellEndEdit;
            //
            Label lblyearDGV = new Label();
            lblyearDGV.Dock = DockStyle.Top;
            lblyearDGV.Text = @"Плановая выработка электроэнергии на "
                + DateTime.Now.Year + " год.";
            lblyearDGV.Name = INDEX_CONTROL.LABEL_YEARPLAN.ToString();
            Label lblTEC = new Label();
            lblTEC.Dock = DockStyle.Top;
            lblTEC.Text = @"Новосибирская ТЭЦ-5";
            //
            TableLayoutPanel tlpYear = new TableLayoutPanel();
            tlpYear.Dock = DockStyle.Fill;
            tlpYear.AutoSize = true;
            tlpYear.AutoSizeMode = AutoSizeMode.GrowOnly;
            tlpYear.Controls.Add(lblyearDGV, 0, 0);
            tlpYear.Controls.Add(lblTEC, 0, 1);
            tlpYear.Controls.Add(m_dgvYear, 0, 2);
            Controls.Add(tlpYear, 1, posRow);
            SetColumnSpan(tlpYear, 9); SetRowSpan(tlpYear, 10);
            //
            Controls.Add(PanelManagementYear, 0, posRow);
            SetColumnSpan(PanelManagementYear, posColdgvTEPValues); SetRowSpan(PanelManagementYear, posRow = posRow + 6);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4, 10);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(hPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);

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
        /// обработчик события - конец редактирования занчения в ячейке
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void dgvYear_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                m_dgvYear.FillTableEdit((int)Session.m_Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">сообщение ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = HTepUsers.Role;

            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH, (int)ID_PERIOD.YEAR };
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

            foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
            {
                id_comp = (Int32)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);

                m_dgvYear.AddIdComp(id_comp, "Output");
            }

            m_dgvYear.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

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
                    (ctrl as ComboBox).SelectedIndex = int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.TIMEZONE).ToString()]);
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    Session.SetCurrentPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    (PanelManagementYear as PanelManagementAutobook).SetPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    (ctrl as ComboBox).Enabled = false;

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
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
              m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }

        /// <summary>
        ///  Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">номер ошибки</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(GetNameTableIn(s_dtDefaultAU)
            , @"DATE_TIME"
            , @"ID"
            , m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            , out err);
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
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            m_handlerDb.RegisterDbConnection(out iRegDbConn);
            clear();

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();

                        m_dgvYear.ShowValues(
                            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                            , m_dgvYear);
                    }
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            }
            else
                //удалить сессию
                deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
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
                if (dtRange[i].End.Month > DateTime.Now.Month)
                    if (dtRange[i].End.Year >= DateTime.Now.Year)
                        bflag = true;

            return bflag;
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges">массив временных отрезков</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr">текст ошибки</param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );

            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    // ,получить входные для расчета значения для возможности редактирования
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
            //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            //         m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
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
                , HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
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
        /// Установка длительности периода 
        /// </summary>
        private void settingDateRange()
        {
            int cntDays,
                today = 0;

            PanelManagementYear.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
              (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today).AddMonths(11);

            PanelManagementYear.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
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
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            int err = -1
                , id_alg = -1
                , ratio = -1
                , round = -1;
            string n_alg = string.Empty;
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();

            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagementYear as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);
            // очистить содержание представления
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
                n_alg = r[@"N_ALG"].ToString();
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
            m_dgvYear.ClearRows();
            DateTime dtNew = new DateTime(s_dtDefaultAU.Year, 1, 1);
            //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
            //заполнение представления
            for (int i = 0; i < GetMonth.Count(); i++)
            {
                m_dgvYear.AddRow(new DGVAutoBook.ROW_PROPERTY()
                {
                    m_idAlg = id_alg
                    ,
                    //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                    //,
                    m_Value = GetMonth[i]
                    ,
                    m_vsRatio = ratio
                    ,
                    m_vsRound = round
                });

                m_dgvYear.Rows[i].Cells["DATE"].Value = dtNew.ToShortDateString();
                dtNew = dtNew.AddMonths(1);
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
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagementYear == null))
                if (active == true)
                    PanelManagementYear.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagementYear.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
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
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            // очистить содержание представления
            clear();
            settingDateRange();
            Session.SetRangeDatetime(dtBegin, dtEnd);
            //заполнение представления
            changeDateInGrid(dtBegin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected void clear(int iCtrl = (int)PanelTaskAutobookYearlyPlan.INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            PanelTaskAutobookYearlyPlan.INDEX_CONTROL indxCtrl = (PanelTaskAutobookYearlyPlan.INDEX_CONTROL)iCtrl;

            deleteSession();
            //??? повторная проверка
            if (bClose == true)
            {
                if (!(m_arTableDictPrjs == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                        if (!(m_arTableDictPrjs[i] == null))
                        {
                            m_arTableDictPrjs[i].Clear();
                            m_arTableDictPrjs[i] = null;
                        }

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                m_dgvYear.ClearRows();
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                m_dgvYear.ClearValues();
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
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;
            DataRow[] dr_saveValue;
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeToSave();

            for (int i = 0; i < m_dgvYear.Rows.Count; i++)
            {
                m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = getStructurInval(dtrPer[i], out err);
                dr_saveValue = valuesFence.Select(String.Format(m_TableEdit.Locale, "WR_DATETIME = '{0:o}'", m_dgvYear.Rows[i].Cells["DATE"].Value));

                if (dr_saveValue.Count() > 0)
                {
                    m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                        HandlerDb.SavePlanValue(m_TableOrigin, dr_saveValue, (int)Session.m_currIdTimezone, out err);

                    s_dtDefaultAU = dtrPer[i].Begin.AddMonths(1);
                    base.HPanelTepCommon_btnSave_Click(obj, ev);
                }
            }
        }

        /// <summary>
        /// формирование таблицы данных
        /// </summary>
        private DataTable valuesFence
        {
            get
            { //сохранить вх. знач. в DataTable
                return m_dgvYear.FillTableEdit((int)Session.m_Id);
            }
        }

        /// <summary>
        /// получает структуру таблицы 
        /// INVAL_XXXXXX???
        /// </summary>
        /// <param name="arQueryRanges">временной промежуток</param>
        /// <param name="err">Индентификатор ошибки</param>
        /// <returns>таблица</returns>
        private DataTable getStructurInval(DateTimeRange arQueryRanges, out int err)
        {
            string strRes = string.Empty;

            strRes += "SELECT * FROM "
                + GetNameTableIn(arQueryRanges.End)
                + " WHERE ID_TIME = " + (int)ActualIdPeriod;

            return HandlerDb.Select(strRes, out err);
        }

        /// <summary>
        /// Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            updateDataValues();
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        /// <summary>
        /// Изменение года)
        /// </summary>
        /// <param name="dtBegin">дата</param>
        private void changeDateInGrid(DateTime dtBegin)
        {
            (Controls.Find(INDEX_CONTROL.LABEL_YEARPLAN.ToString(), true)[0] as Label).Text =
             @"Плановая выработка электроэнергии на "
                + dtBegin.Year + " год.";

            m_dgvYear.Rows[dtBegin.Month - 1].Selected = true;
        }

        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            deleteSession();

            base.Stop();
        }
    }
}
