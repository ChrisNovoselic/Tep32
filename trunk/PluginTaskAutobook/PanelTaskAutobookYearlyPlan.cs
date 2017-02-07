using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using HClassLibrary;
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
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
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
        /// Объект для работы с БД (чтение, сохранение значений)
        /// </summary>
        protected HandlerDbTaskAutobookYarlyPlanCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskAutobookYarlyPlanCalculate; } }
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_PLANEYAR = 2,
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
        ///// <summary>
        ///// Таблицы со значениями словарных, проектных данных
        ///// </summary>
        //protected DataTable[] m_dictTableDictPrj;
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
        ///// <summary>
        ///// 
        ///// </summary>
        //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, 1, 1);
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementAutobook();
        }

        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementAutobook PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementAutobook;
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

                    public HDataGridViewCell(int idParameter, HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
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
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT);
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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nameDGV"></param>
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
                //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

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

            public void AddBRead(bool bRead)
            {
                foreach (HDataGridViewColumn col in Columns)
                        col.ReadOnly = bRead;
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
            public void ShowValues(DataTable tbOrigin)
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
                                    dblVal *= Math.Pow(10F, -1 * vsRatioValue);

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
                                        , valueToRes *= Math.Pow(10F, 1 * vsRatioValue)
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
        protected class PanelManagementAutobook : PanelManagementTaskCalculate //HPanelCommon
        {
            /// <summary>
            /// 
            /// </summary>
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD,
                TXTBX_EMAIL,
                CHKBX_EDIT,
                COUNT
            }

            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementAutobook()
                : base() //4, 3
            {
                InitializeComponents();
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
                SetPositionPeriod(new System.Drawing.Point(0, posRow), new Size(this.ColumnCount / 2, 1));

                //Период расчета - значение
                SetPositionTimezone(new System.Drawing.Point(0, posRow = posRow + 1), new Size(this.ColumnCount / 2, 1));

                //Дата/время начала периода расчета
                posRow = SetPositionDateTimePicker(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount, 4));

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                //ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                //indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                //ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString();
                //indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                //ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка 
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                ctrlBSend.Enabled = false;
                ctrlBSend.Visible = false;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 1, 0);
                tlpButton.Controls.Add(ctrlBSend, 0, 1);
                //tlpButton.Controls.Add(ctrlTxt, 1, 1);
                Controls.Add(tlpButton, 0, posRow = posRow + 2);
                SetColumnSpan(tlpButton, 4); SetRowSpan(tlpButton, 2);
                //Признак Корректировка_включена/корректировка_отключена 
                CheckBox cBox = new CheckBox();
                cBox.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookYearlyPlan(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();

            //Session.SetDatetimeRange(s_dtDefaultAU, s_dtDefaultAU.AddMonths(1));
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
            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, posColdgvTEPValues); SetRowSpan(PanelManagement, posRow = posRow + 6);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4, 10);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(hPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);

        }

        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }

        /// <summary>
        /// обработчик события - конец редактирования занчения в ячейке
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void dgvYear_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
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

            //m_dictTableDictPrj = new DataTable[(int)ID_DBTABLE.COUNT];
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
            // PERIOD, TIMIZONE, COMP, PARAMETER(OUT_VALUES), MEASURE, RATIO
            initialize(new ID_DBTABLE[] { ID_DBTABLE.PERIOD }, out err, out errMsg);

            foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP].Rows)
            {
                id_comp = (int)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);

                m_dgvYear.AddIdComp(id_comp, "Output");
            }

            m_dgvYear.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);

            try
            {
                if (m_dictProfile.Objects[((int)ID_PERIOD.YEAR).ToString()].Objects[((int)INDEX_CONTROL.DGV_PLANEYAR).ToString()].Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Objects[((int)ID_PERIOD.YEAR).ToString()].Objects[((int)INDEX_CONTROL.DGV_PLANEYAR).ToString()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                    else
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                }
                else
                    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                if ((Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                    m_dgvYear.AddBRead(false);
                else
                    m_dgvYear.AddBRead(true);

                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                }
                else
                    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelTaskAutoBookYarlyPlan::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (err == 0)
            {
                try
                {
                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone (m_dictTableDictPrj[ID_DBTABLE.TIMEZONE], (ID_TIMEZONE)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]));                    
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.PERIOD], (ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()])));
                    Session.SetCurrentPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
                    PanelManagement.SetModeDatetimeRange();

                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
              m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        }

        /// <summary>
        ///  Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">номер ошибки</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(GetNameTableIn(PanelManagement.DatetimeRange.Begin) //??? почему 'Begin', а не 'End'
                , @"DATE_TIME"
                , @"ID"
                , m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                , out err
            );
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = Session.CountBasePeriod
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
                    if (m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();

                        m_dgvYear.ShowValues(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]);
                    }
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskAutobookYearlyPlan::updatedataValues() - " + errMsg);
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
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
                (
                Type
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
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
                        , Session.CountBasePeriod
                        , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
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
            //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
            //         m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
        }

        ///// <summary>
        ///// формирование запросов 
        ///// для справочных данных
        ///// </summary>
        ///// <returns>запрос</returns>
        //private string[] getQueryDictPrj()
        //{
        //    string[] arRes = null;

        //    arRes = new string[]
        //    {
        //        //PERIOD
        //        HandlerDb.GetQueryTimePeriods(m_strIdPeriods)
        //        //TIMEZONE
        //        , HandlerDb.GetQueryTimezones(m_strIdTimezones)
        //        // список компонентов
        //        , HandlerDb.GetQueryComp(Type)
        //        // параметры расчета
        //        , HandlerDb.GetQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        //, HandlerDb.GetQueryModeDev()
        //        //// единицы измерения
        //        , m_handlerDb.GetQueryMeasures()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio()
        //    };

        //    return arRes;
        //}

        ///// <summary>
        ///// Установка длительности периода 
        ///// </summary>
        //private void settingDateRange()
        //{
        //    int cntDays,
        //        today = 0;

        //    PanelManagementYear.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //      (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today).AddMonths(11);

        //    PanelManagementYear.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
        //}

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
            Session.SetCurrentPeriod(PanelManagement.IdPeriod);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            PanelManagement.SetModeDatetimeRange();
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
                else
                    ;
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
            DateTime dtNew = new DateTime(PanelManagement.DatetimeRange.Begin.Year, 1, 1); //??? почему 'Begin', а не 'End'
            //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
            //заполнение представления
            for (int i = 0; i < GetMonth.Count(); i++)
            {
                m_dgvYear.AddRow(new DGVAutoBook.ROW_PROPERTY()
                {
                    m_idAlg = id_alg
                    //, m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                    , m_Value = GetMonth[i]
                    , m_vsRatio = ratio
                    , m_vsRound = round
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
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Зарегистрировать/отменить регистрацию обработчика события изменение даты/времени
        /// </summary>
        /// <param name="active">Признак регистрации/отмены регистрации</param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
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

                listRes = m_dictTableDictPrj[ID_DBTABLE.COMP].Select().ToList<DataRow>();

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
            //settingDateRange();
            Session.SetDatetimeRange(dtBegin, dtEnd);
            ////заполнение представления
            //changeDateInGrid(dtBegin);
        }

        /// <summary>
        /// Очистить содержание представления
        /// </summary>
        /// <param name="bClose">Признак снятия с отображения(закрытия) вкладки</param>
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true) {
                // удалить все строки
                m_dgvYear.ClearRows();
                //// удалить все столбцы
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                m_dgvYear.ClearValues();

            base.clear(bClose);
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
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = getStructurInval(dtrPer[i], out err);
                dr_saveValue = valuesFence.Select(string.Format(m_TableEdit.Locale, "WR_DATETIME = '{0:o}'", m_dgvYear.Rows[i].Cells["DATE"].Value));

                if (dr_saveValue.Count() > 0) {
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                        HandlerDb.SavePlanValue(m_TableOrigin, dr_saveValue, (int)Session.m_currIdTimezone, out err);

                    //s_dtDefaultAU = dtrPer[i].Begin.AddMonths(1);
                    base.HPanelTepCommon_btnSave_Click(obj, ev);
                }
                else
                    ;
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
                + " WHERE ID_TIME = " + (int)Session.ActualIdPeriod;

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

            strRes = HandlerDbValues.s_dictDbTables[ID_DBTABLE.INVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

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
            Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        ///// <summary>
        ///// Изменение года)
        ///// </summary>
        ///// <param name="dtBegin">дата</param>
        //private void changeDateInGrid(DateTime dtBegin)
        //{
        //    (Controls.Find(INDEX_CONTROL.LABEL_YEARPLAN.ToString(), true)[0] as Label).Text =
        //        string.Format(@"Плановая выработка электроэнергии на {0} год.", dtBegin.Year);

        //    DateTime dtNew = new DateTime(dtBegin.Year, 1, 1);
        //    //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
        //    //заполнение представления
        //    for (int i = 0; i < GetMonth.Count(); i++)
        //    {
        //        m_dgvYear.Rows[i].Cells["DATE"].Value = dtNew.ToShortDateString();
        //        dtNew = dtNew.AddMonths(1);
        //    }

        //    m_dgvYear.Rows[dtBegin.Month - 1].Selected = true;
        //}

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
