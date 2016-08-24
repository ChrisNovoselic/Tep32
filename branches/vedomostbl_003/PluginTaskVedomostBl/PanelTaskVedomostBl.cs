using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Windows.Controls;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using Excel = Microsoft.Office.Interop.Excel;
using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskVedomostBl
{
    public class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        static bool m_bflgClear = false;
        /// <summary>
        /// 
        /// </summary>
        Dictionary<int, Dictionary<int, Dictionary<int, string>>> dict;// = new Dictionary<int, Dictionary<int, Dictionary<int, string>>>();
        /// <summary>
        /// Листы с хидерами грида
        /// </summary>
        public static List<string> m_listGroupSett_1 = new List<string>
        {
         "Острый пар", "Горячий промперегрев", "ХПП"
        };
        public static List<string> m_listGroupSett_2 = new List<string>
        {
         "Питательная вода","Продувка", "Конденсатор", "Холодный воздух"
         , "Горячий воздух", "Кислород", "VI отбор", "VII отбор"
        };
        public static List<string> m_listGroupSett_3 = new List<string>
        {
          "Уходящие газы","","" ,"","РОУ", "Сетевая вода", "Выхлоп ЦНД"
        };
        /// <summary>
        /// Лист с группами хидеров отображения
        /// </summary>
        public static List<List<string>> m_listHeader = new List<List<string>> { m_listGroupSett_1, m_listGroupSett_2, m_listGroupSett_3 };
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_DATA_B1, DGV_DATA_B2, DGV_DATA_B3,
            DGV_DATA_B4, DGV_DATA_B5, DGV_DATA_B6,
            RADIOBTN_BLK1, RADIOBTN_BLK2, RADIOBTN_BLK3,
            RADIOBTN_BLK4, RADIOBTN_BLK5, RADIOBTN_BLK6,
            LABEL_DESC, TBLP_HGRID,
            COUNT
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
            //DENY_COMP_CALCULATED, 
            DENY_COMP_VISIBLED,
            BLOCK_VISIBLED, HGRID_VISIBLE,
            //DENY_PARAMETER_CALCULATED, // запрещенных для расчета
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1,
            PERIOD, TIMEZONE,
            COMPONENT, PARAMETER,
            //, MODE_DEV/*, MEASURE*/,
            RATIO,
            COUNT
        }
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskVedomostBlCalculate; } }
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == HandlerDbTaskVedomostBlCalculate.INDEX_TABLE_VALUES.SESSION ? ID_PERIOD.MONTH : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate.INDEX_TABLE_VALUES m_ViewValues;
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        private PanelManagementVedomost _panelManagement;
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementVedomost PanelManagementVed
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement;
            }
        }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        private PanelManagementVedomost createPanelManagement()
        {
            return new PanelManagementVedomost();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskVedomostBlCalculate();
        }
        /// <summary>
        /// 
        /// </summary>
        protected DGVVedomostBl m_dgvVedomst;

        /// <summary>
        /// 
        /// </summary>
        protected System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }

        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementVedomost : HPanelCommon
        {
            /// <summary>
            /// подсказка
            /// </summary>
            ToolTip tlTipGrp = new ToolTip();
            /// <summary>
            /// текст подсказки
            /// </summary>
            string[] toolTipText;
            private int toolTipIndex;
            /// <summary>
            /// Перечисление контролов панели
            /// </summary>
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL,
                CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CLBX_COMP_VISIBLED, CLBX_COMP_CALCULATED, CLBX_COL_VISIBLED,
                CHKBX_EDIT, TBLP_BLK, TOOLTIP_GRP,
                COUNT
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dtBegin"></param>
            /// <param name="dtEnd"></param>
            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);
            /// <summary>
            /// Класс аргумента для события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public class ItemCheckedParametersEventArgs : EventArgs
            {
                /// <summary>
                /// Индекс в списке идентификаторов
                ///  для получения ключа в словаре со значениями
                /// </summary>
                public INDEX_ID m_indxIdDeny;
                /// <summary>
                /// Идентификатор в алгоритме расчета
                /// </summary>
                public int m_idItem;
                /// <summary>
                /// Состояние элемента, связанного с компонентом/параметром_расчета
                /// </summary>
                public CheckState m_newCheckState;

                public ItemCheckedParametersEventArgs(int idItem, INDEX_ID indxIdDeny, CheckState newCheckState)
                    : base()
                {
                    m_idItem = idItem;
                    m_indxIdDeny = indxIdDeny;
                    m_newCheckState = newCheckState;
                }
            }

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;
            /// <summary>
            /// Тип обработчика события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            /// <param name="ev">Аргумент события</param>
            public delegate void ItemCheckedParametersEventHandler(ItemCheckedParametersEventArgs ev);
            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;
            /// <summary>
            /// 
            /// </summary>
            public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

            public PanelManagementVedomost()
                : base(4, 3)
            {
                InitializeComponents();
                toolTipText = new string[m_listHeader.Count];
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            /// <summary>
            /// 
            /// </summary>
            private void InitializeComponents()
            {
                //initializeLayoutStyle();
                ToolTip tlTipHeader = new ToolTip();
                tlTipHeader.AutoPopDelay = 5000;
                tlTipHeader.InitialDelay = 1000;
                tlTipHeader.ReshowDelay = 500;
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
                cbxCalcPer.Enabled = false;
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
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
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
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);
                this.Controls.Add(tlpValue, 0, posRow = posRow + 1);
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
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL_BASE.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;

                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 1, 0);
                tlpButton.Controls.Add(ctrlExp, 0, 2);
                this.Controls.Add(tlpButton, 0, posRow = posRow + 2);
                this.SetColumnSpan(tlpButton, 4); this.SetRowSpan(tlpButton, 2);
                //Признаки включения/исключения для отображения компонента
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Выбрать блок для отображения";
                TableLayoutPanel tlpChk = new TableLayoutPanel();
                tlpChk.Controls.Add(ctrl, 0, 0);
                //
                ctrl = new TableLayoutPanelkVed();
                //ctrl = new CheckedListBoxTaskReaktivka();
                ctrl.Name = INDEX_CONTROL_BASE.TBLP_BLK.ToString();
                ctrl.Dock = DockStyle.Top;
                tlpChk.Controls.Add(ctrl, 0, 1);

                //Признак для включения/исключения для отображения столбца(ов)
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить столбец(ы) для отображения";
                tlpChk.Controls.Add(ctrl, 0, 2);
                //
                ctrl = new CheckedListBoxTaskVed();
                ctrl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.showCheckBoxToolTip); ;
                ctrl.Name = INDEX_CONTROL_BASE.CLBX_COL_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Top;
                (ctrl as CheckedListBoxTaskVed).CheckOnClick = true;
                tlpChk.Controls.Add(ctrl, 0, 3);
                tlpChk.Dock = DockStyle.Fill;
                tlpChk.AutoSize = true;
                tlpChk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpChk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
                tlpChk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
                this.Controls.Add(tlpChk, 0, posRow = posRow + 4);
                this.SetColumnSpan(tlpChk, 4); this.SetRowSpan(tlpChk, 2);

                //Признак Корректировка_включена/корректировка_отключена 
                CheckBox cBox = new CheckBox();
                cBox.Name = INDEX_CONTROL_BASE.CHKBX_EDIT.ToString();
                cBox.Text = @"Корректировка значений разрешена";
                cBox.Dock = DockStyle.Top;
                cBox.Enabled = false;
                cBox.Checked = true;
                this.Controls.Add(cBox, 0, posRow = posRow + 1);
                this.SetColumnSpan(cBox, 4); this.SetRowSpan(cBox, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void showCheckBoxToolTip(object sender, MouseEventArgs e)
            {
                CheckedListBoxTaskVed chkVed = (this.Controls.Find(INDEX_CONTROL_BASE.CLBX_COL_VISIBLED.ToString(), true)[0] as CheckedListBoxTaskVed);

                if (toolTipIndex != chkVed.IndexFromPoint(e.Location))
                {
                    toolTipIndex = chkVed.IndexFromPoint(chkVed.PointToClient(MousePosition));
                    if (toolTipIndex > -1)
                    {
                        //Свич по элементам находящимся в чеклистбоксе
                        switch (chkVed.Items[toolTipIndex].ToString())
                        {
                            case "Группа 1":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 2":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 3":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
                                break;
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cols"></param>
            /// <param name="rows"></param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                //m_bflgClear = true;
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
                , hdtpEndtimePer = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;

                int cntDays = DateTime.DaysInMonth(hdtpBtimePer.Value.Year, hdtpBtimePer.Value.Month);
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

            /// <summary>
            /// Интерфейс для всех элементов управления с компонентами станции, параметрами расчета
            /// </summary>
            protected interface IControl
            {
                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                int SelectedId { get; }
                ///// <summary>
                ///// Добавить элемент в список
                ///// </summary>
                ///// <param name="text">Текст подписи элемента</param>
                ///// <param name="id">Идентификатор элемента</param>
                ///// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                //void AddItem(int id, string text, bool bChecked);
                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                void ClearItems();
            }
            /// <summary>
            /// Класс для размещения элементов (блоков) выбора отображения значений
            /// </summary>
            protected class TableLayoutPanelkVed : TableLayoutPanel
            {
                RadioButton[] arRb;
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                /// <summary>
                /// 
                /// </summary>
                public TableLayoutPanelkVed()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId
                {
                    get
                    {
                        int cnt = 0;

                        foreach (RadioButton rb in Controls)
                        {
                            if (rb.Checked == true)
                                break;
                            else
                                cnt++;
                        }
                        return m_listId[cnt];
                    }
                }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItems(int[] id, string[] text, bool[] bChecked, RadioButton[] rb)
                {
                    int indx = -1
                       , col = -1
                       , row = -1;
                    if (arRb == null)
                        arRb = rb;
                    RowCount = 1;
                    ColumnCount = 3;
                    RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
                    ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
                    ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
                    ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));

                    for (int i = 0; i < arRb.Length; i++)
                    {
                        arRb[i].Text = text[i];
                        arRb[i].Checked = bChecked[i];
                        arRb[i].CheckedChanged += TableLayoutPanelkVed_CheckedChanged;
                        m_listId.Add(id[i]);

                        if (RowCount * ColumnCount < arRb.Length)
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() => RowCount++));
                                Invoke(new Action(() => RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F))));
                            }
                            else
                            {
                                if (ColumnCount > RowCount)
                                {
                                    RowCount++;
                                    RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
                                }
                                else
                                {
                                    ColumnCount++;
                                    ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
                                }
                            }
                        }

                        indx = i;
                        if (!(indx < arRb.Length))
                            //indx += (int)(indx / RowCount);

                            row = (int)(indx / RowCount);
                        col = indx % (RowCount - 0);

                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Controls.Add(arRb[i], col, row)));
                            Invoke(new Action(() => AutoScroll = true));
                        }
                        else
                        {
                            Controls.Add(arRb[i], col, row);
                            //AutoScroll = true;
                        }
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                public void TableLayoutPanelkVed_CheckedChanged(object sender, EventArgs e)
                {
                    string nameControl;
                    int id = SelectedId;
                    nameControl = (sender as RadioButton).Name;//double
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Controls.Clear();
                    m_listId.Clear();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    //strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }

            }

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskVed : CheckedListBox, IControl
            {
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                public CheckedListBoxTaskVed()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId { get { return m_listId[SelectedIndex]; } }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                    Items.Add(text, bChecked);
                    m_listId.Add(id);

                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Items.Clear();
                    m_listId.Clear();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }
            }

            /// <summary>
            /// Добавить элемент компонент станции в списки
            /// , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(int id_comp, string text, List<string> textToolTip, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            {
                Control ctrl = null;
                toolTipText[id_comp] = fromationToolTipText(textToolTip);

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskVed).AddItem(id_comp, text, arChecked[id_comp]);

                    else
                        Logging.Logg().Error(@"PanelManagementTaskVed::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="listText"></param>
            /// <returns></returns>
            private string fromationToolTipText(List<string> listText)
            {
                string strTextToolTip = string.Empty;

                foreach (var item in listText)
                {
                    if (strTextToolTip != string.Empty)
                        strTextToolTip += ", ";
                    strTextToolTip += item;
                }

                return strTextToolTip;
            }

            /// <summary>
            /// Добавить элемент компонент станции в списки
            /// , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponentRB(int[] id_comp,
                string[] text,
                INDEX_ID[] arIndexIdToAdd,
                bool[] arChecked,
                RadioButton[] rb)
            {
                Control ctrl = null;

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as TableLayoutPanelkVed).AddItems(id_comp, text, arChecked, rb);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskVed::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Найти элемент управления на панели по индексу идентификатора
            /// </summary>
            /// <param name="id">Индекс идентификатора, используемого для заполнения элемента управления</param>
            /// <returns>Дочерний элемент управления</returns>
            protected Control find(INDEX_ID id)
            {
                Control ctrlRes = null;

                ctrlRes = find(getIndexControlOfIndexID(id));

                return ctrlRes;
            }

            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns>элемент панели</returns>
            protected Control find(INDEX_CONTROL_BASE indxCtrl)
            {
                Control ctrlRes = null;

                ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];

                return ctrlRes;
            }

            /// <summary>
            /// Возвратить идентификатор элемента управления по идентификатору
            ///  , используемого для его заполнения
            /// </summary>
            /// <param name="indxId"></param>
            /// <returns>индекс элемента панели</returns>
            protected INDEX_CONTROL_BASE getIndexControlOfIndexID(INDEX_ID indxId)
            {
                INDEX_CONTROL_BASE indxRes = INDEX_CONTROL_BASE.UNKNOWN;

                switch (indxId)
                {
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        indxRes = INDEX_CONTROL_BASE.CLBX_COL_VISIBLED;
                        break;
                    case INDEX_ID.HGRID_VISIBLE:
                        indxRes = INDEX_CONTROL_BASE.CLBX_COL_VISIBLED;
                        break;
                    case INDEX_ID.BLOCK_VISIBLED:
                        indxRes = INDEX_CONTROL_BASE.TBLP_BLK;
                        break;
                    default:
                        break;
                }

                return indxRes;
            }

            /// <summary>
            /// Очистить
            /// </summary>
            public void Clear()
            {
                INDEX_ID[] arIndxIdToClear = new INDEX_ID[] { INDEX_ID.DENY_COMP_VISIBLED };

                ActivateCheckedHandler(false, arIndxIdToClear);

                Clear(arIndxIdToClear);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="arIdToClear"></param>
            public void Clear(INDEX_ID[] arIdToClear)
            {
                for (int i = 0; i < arIdToClear.Length; i++)
                    clear(arIdToClear[i]);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="idToClear"></param>
            private void clear(INDEX_ID idToClear)
            {
                (find(idToClear) as IControl).ClearItems();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bActive"></param>
            /// <param name="arIdToActivate"></param>
            public void ActivateCheckedHandler(bool bActive, INDEX_ID[] arIdToActivate)
            {
                for (int i = 0; i < arIdToActivate.Length; i++)
                    activateCheckedHandler(bActive, arIdToActivate[i]);
            }

            /// <summary>
            /// событие активации
            /// </summary>
            /// <param name="bActive"></param>
            /// <param name="idToActivate"></param>
            protected virtual void activateCheckedHandler(bool bActive, INDEX_ID idToActivate)
            {
                INDEX_CONTROL_BASE indxCtrl = INDEX_CONTROL_BASE.UNKNOWN;
                CheckedListBox clbx = null;

                indxCtrl = getIndexControlOfIndexID(idToActivate);

                if (!(indxCtrl == INDEX_CONTROL_BASE.UNKNOWN))
                {
                    clbx = (Controls.Find(indxCtrl.ToString(), true)[0] as CheckedListBox);

                    if (bActive == true)
                        clbx.ItemCheck += new ItemCheckEventHandler(onItemCheck);
                    else
                        clbx.ItemCheck -= onItemCheck;
                }
            }

            /// <summary>
            /// Обработчик события - изменение состояния элемента списка
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected void onItemCheck(object obj, ItemCheckEventArgs ev)
            {
                itemCheck((obj as IControl).SelectedId, getIndexIdOfControl(obj as Control), ev.NewValue);
            }

            /// <summary>
            /// Получение ИД контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>индекс</returns>
            protected INDEX_ID getIndexIdOfControl(Control ctrl)
            {
                INDEX_CONTROL_BASE id = INDEX_CONTROL_BASE.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
                INDEX_ID indxRes = INDEX_ID.UNKNOWN;

                try
                {
                    //Определить идентификатор
                    id = getIndexControl(ctrl);
                    // , соответствующий изменившему состояние элементу 'CheckedListBox'
                    switch (id)
                    {
                        case INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED:
                            indxRes = id == INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        case INDEX_CONTROL_BASE.CLBX_COL_VISIBLED:
                            indxRes = id == INDEX_CONTROL_BASE.CLBX_COL_VISIBLED ? INDEX_ID.HGRID_VISIBLE : INDEX_ID.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelManagementTaskTepValues::onItemCheck () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return indxRes;
            }

            /// <summary>
            /// Получение индекса контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>имя индекса контрола на панели</returns>
            protected INDEX_CONTROL_BASE getIndexControl(Control ctrl)
            {
                INDEX_CONTROL_BASE indxRes = INDEX_CONTROL_BASE.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (strId.Equals(INDEX_CONTROL_BASE.CLBX_COL_VISIBLED.ToString()) == true)
                    indxRes = INDEX_CONTROL_BASE.CLBX_COL_VISIBLED;
                else if (strId.Equals(INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED.ToString()) == true)
                    indxRes = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED;
                else
                    throw new Exception(@"PanelTaskTepValues::getIndexControl () - не найден объект 'CheckedListBox'...");

                return indxRes;
            }

            /// <summary>
            /// Инициировать событие - изменение признака элемента
            /// </summary>
            /// <param name="address">Адрес элемента</param>
            /// <param name="checkState">Значение признака элемента</param>
            protected void itemCheck(int idItem, INDEX_ID indxIdDeny, CheckState checkState)
            {
                ItemCheck(new ItemCheckedParametersEventArgs(idItem, indxIdDeny, checkState));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected class DGVVedomostBl : DataGridView
        {
            /// <summary>
            /// 
            /// </summary>
            public int m_idCompDGV;
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DGVVedomostBl(string nameDGV, bool IsHeader)
            {
                InitializeComponents(nameDGV, IsHeader);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nameDGV"></param>
            private void InitializeComponents(string nameDGV, bool bIsHeader)
            {
                this.Name = nameDGV;
                Dock = DockStyle.Fill;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
                //Запрет изменения размера строк
                AllowUserToResizeRows = false;
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
                //HorizontalScrollBar.Enabled = true;
                //this.ScrollBars = ScrollBars.Horizontal;

                if (!bIsHeader)
                {
                    AddColumns(-2, string.Empty, "ALG", false);
                    AddColumns(-1, "Дата", "Date", true );
                }
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
                /// <summary>
                /// 
                /// </summary>
                public string topHeader;
            }

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
            /// 
            /// </summary>
            /// <param name="idHeader"></param>
            /// <param name="headerText"></param>
            /// <param name="nameCol"></param>
            /// <param name="bVisible"></param>
            public void AddColumns(int idHeader, string nameCol, string headerText, bool bVisible)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = idHeader, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    //column.Frozen = true;
                    column.Visible = bVisible;
                    column.ReadOnly = false;
                    column.Name = nameCol;
                    column.HeaderText = headerText;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    //Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="idHeader"></param>
            /// <param name="topHeader"></param>
            /// <param name="headerText"></param>
            /// <param name="nameCol"></param>
            /// <param name="bVisible"></param>
            public void AddColumns(int idHeader, string topHeader, string nameCol, string headerText, bool bVisible)
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

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = idHeader, m_bCalcDeny = false, topHeader = topHeader };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (idHeader < 0)
                        {// для служебных столбцов
                            if (bVisible == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }
                            column.Frozen = true;
                            column.ReadOnly = true;
                        }
                    }

                    column.HeaderText = headerText;
                    column.Name = nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = bVisible;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    //Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + id_comp + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Удаление набора строк
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
            }

            /// <summary>
            /// Очищение отображения от значений
            /// </summary>
            public void ClearValues()
            {
                //CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;

                //??? если установить 'true' - редактирование невозможно
                //ReadOnly = false;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
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
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow(ROW_PROPERTY rowProp, int DaysInMonth)
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
                // инициализировать значения в служебных ячейках
                //m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);

                if (i == DaysInMonth)
                    foreach (HDataGridViewColumn col in Columns)
                        Rows[i].Cells[col.Index].ReadOnly = true;//блокировка строк
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
        }

        protected class DataWorkClass : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            public static Dictionary<int, Dictionary<int, Dictionary<int, string>>> dict;

            public DataWorkClass() : base()
            {
 
            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            private class parsingData
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="dt"></param>
                public parsingData(DataTable dt)
                {
 
                }

                private void compilingDict()
                {


                }

                private void disaggregationToParts(DataTable dtPars)
                {
                    Dictionary<int, string> dictLowerLvl = new Dictionary<int, string>();
                    int cntAr = 0;
                    List<List<string>> arList = new List<List<string>> { };

                    foreach (DataRow row in dtPars.Rows)
                    {
                        arList[cntAr] = row["N_ALG"].ToString().Split('.', ',').ToList();
                       cntAr++;
                    }
                }

                private void createDict()
                {
                    dict = new Dictionary<int,Dictionary<int,Dictionary<int,string>>> {};
                    //dict.Add(,
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskVedomostBl(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control();
            TableLayoutPanel tblPanelHGrid = new TableLayoutPanel();
            FlowLayoutPanel flPanelHG = new FlowLayoutPanel();
            tblPanelHGrid.Name = INDEX_CONTROL.TBLP_HGRID.ToString();
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить" 

            SuspendLayout();
            //создание грида со значениями
            for (int i = (int)INDEX_CONTROL.DGV_DATA_B1; i < (int)INDEX_CONTROL.RADIOBTN_BLK1; i++)
            {
                ctrl = new DGVVedomostBl(namePut.GetValue(i).ToString(), false);
                ctrl.Name = namePut.GetValue(i).ToString();
                ctrl.Enabled = false;
                ctrl.Visible = false;

                //this.Controls.Add(ctrl, 5, posRow + 1);
                //this.SetColumnSpan(ctrl, 9); this.SetRowSpan(ctrl, 10);
            }

            this.Controls.Add(PanelManagementVed, 0, posRow);
            this.SetColumnSpan(PanelManagementVed, 4); this.SetRowSpan(PanelManagementVed, 13);

            for (int i = 0; i < m_listHeader.Count; i++)
            {
                //добавление столбцов
                for (int j = 0; j < m_listHeader[i].Count; j++)
                {

                }
            }

            (tblPanelHGrid as TableLayoutPanel).AutoScroll = true;
            this.Controls.Add(tblPanelHGrid, 5, posRow);
            this.SetColumnSpan(tblPanelHGrid, 9); this.SetRowSpan(tblPanelHGrid, 1);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
            //(Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += PanelTaskReaktivka_ClickExport;
            (PanelManagementVed as PanelManagementVedomost).ItemCheck += new PanelManagementVedomost.ItemCheckedParametersEventHandler(panelManagement_ItemCheck);
            (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).CheckedChanged += PanelManagementVedomost_CheckedChanged;
        }

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_ItemCheck(PanelManagementVedomost.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //Изменить признак состояния компонента ТЭЦ/параметра алгоритма расчета
            if (ev.m_newCheckState == CheckState.Unchecked)
                if (m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0)
                    m_arListIds[(int)ev.m_indxIdDeny].Add(idItem);
                else ; //throw new Exception (@"");
            else
                if (ev.m_newCheckState == CheckState.Checked)
                    if (!(m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0))
                        m_arListIds[(int)ev.m_indxIdDeny].Remove(idItem);
                    else ; //throw new Exception (@"");
                else ;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //Изменить структуру 'HDataGRidVIew's'          
            placementHGridViewOnTheForm(ev);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelManagementVedomost_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void placementHGridViewOnTheForm(PanelManagementVedomost.ItemCheckedParametersEventArgs item)
        {
            bool bItemChecked = item.m_newCheckState == CheckState.Checked ? true :
                  item.m_newCheckState == CheckState.Unchecked ? false :
                      false;
            //Поиск индекса элемента отображения
            switch (item.m_indxIdDeny)
            {
                case INDEX_ID.HGRID_VISIBLE:

                    break;
                default:
                    break;
            }
            //Controls.Find(INDEX_CONTROL.DGV_HEADER_GRP_1)
        }

        /// <summary>
        /// Инициализация радиобаттанов
        /// </summary>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        private void initializeRB(DataTable[] m_arTableDictPrjs, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            Control cntrl;
            string[] arstrItem;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            RadioButton[] arRadioBtn;
            int[] arId_comp;
            int rbCnt = (int)INDEX_CONTROL.RADIOBTN_BLK1;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[] 
            {
                INDEX_ID.BLOCK_VISIBLED
            };

            cntrl = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.TBLP_BLK.ToString(), true)[0];
            //инициализация массивов
            bool[] arChecked = new bool[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count];
            arRadioBtn = new RadioButton[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count];
            arId_comp = new int[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count];
            arstrItem = new string[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count];
            arRadioBtn = new RadioButton[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count];
            //создание списка гридов по блокам
            foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
            {
                //инициализация радиобаттанов
                arRadioBtn[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = new RadioButton();
                arRadioBtn[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1].Name = namePut.GetValue(rbCnt).ToString();

                arId_comp[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = int.Parse(r[@"ID"].ToString());
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(int.Parse(r[@"ID"].ToString()));
                arstrItem[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = ((string)r[@"DESCRIPTION"]).Trim();
                if (rbCnt == (int)INDEX_CONTROL.RADIOBTN_BLK1)
                    arChecked[0] = true;
                else
                    arChecked[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = false;

                if (arId_comp[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count - 1] != 0)
                    //добавление радиобатонов на форму
                    (PanelManagementVed as PanelManagementVedomost).AddComponentRB(arId_comp
                              , arstrItem
                              , arIndxIdToAdd
                              , arChecked
                              , arRadioBtn);

                rbCnt++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int i = -1,
                id_comp,
                idPer = int.Parse(HTepUsers.GetProfileUser_Tab(m_id_panel).Select("ID_UNIT = " + (int)HTepUsers.ID_ALLOWED.PERIOD_IND + " AND ID_EXT = " + HTepUsers.Role)[0]["VALUE"].ToString())
            , rbCnt = (int)INDEX_CONTROL.RADIOBTN_BLK1;
            Control ctrl = null;
            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = (int)HTepUsers.Role;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[]
            {
                INDEX_ID.HGRID_VISIBLE
            };

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
            //Заполнить элементы управления с компонентами станции
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrjs[i] = m_handlerDb.Select(arQueryDictPrj[i], out err);

                if (!(err == 0))
                    break;
            }
            (PanelManagementVed as PanelManagementVedomost).Clear();
            //
            initializeRB(m_arTableDictPrjs, out err, out errMsg);

            bool[] arChecked = new bool[m_listHeader.Count];

            foreach (var list in m_listHeader)
            {
                id_comp = m_listHeader.IndexOf(list);
                //m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_comp);
                strItem = "Группа " + (id_comp + 1);
                // установить признак отображения компонента станции
                arChecked[0] = m_arListIds[(int)INDEX_ID.HGRID_VISIBLE].IndexOf(id_comp) < 0;
                (PanelManagementVed as PanelManagementVedomost).AddComponent(id_comp
                    , strItem
                    , list
                    , arIndxIdToAdd
                    , arChecked);
            }

            (PanelManagementVed as PanelManagementVedomost).ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.HGRID_VISIBLE });

            //m_dgvReak.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

            if (err == 0)
            {
                try
                {
                    if (m_bflgClear == false)
                        m_bflgClear = true;
                    else
                        m_bflgClear = false;
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = int.Parse(HTepUsers.GetProfileUser_Tab(m_id_panel).Select("ID_UNIT = " + (int)HTepUsers.ID_ALLOWED.QUERY_TIMEZONE + " AND ID_EXT = " + HTepUsers.Role)[0]["VALUE"].ToString());//??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);

                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][2]);//??
                    (PanelManagementVed as PanelManagementVedomost).SetPeriod((ID_PERIOD)idPer);
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
        /// 
        /// </summary>
        /// <param name="dgvBl"></param>
        protected void filingDictHeader(DataGridView dgvBl)
        {
            DataTable dtHtext = HandlerDb.GetHeaderDGV();

            foreach (var item in m_listHeader)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    //dgvBl.Add
                }
            }
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            if (m_bflgClear)
            {
                //Установить новое значение для текущего периода
                setCurrentTimeZone(obj as ComboBox);
                // очистить содержание представления
                clear();
            }
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
            if (!(PanelManagementVed == null))
                if (active == true)
                    PanelManagementVed.DateTimeRangeValue_Changed += new PanelManagementVedomost.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagementVed.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                    else
                        throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagementVed as PanelManagementVedomost).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);
            if (m_bflgClear)
                // очистить содержание представления
                clear();
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
            //settingDateRange();
            //Session.SetRangeDatetime(dtBegin, dtEnd);

            //if (m_bflgClear)
            //{
            //    clear();
            //    dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
            //      , new int[] {
            //        m_id_panel
            //        , (int)Session.m_currIdPeriod }
            //      , out err);

            //    IEnumerable<DataRow> listParameter = ListParameter.Select(x => x);

            //    foreach (DataRow r in listParameter)
            //    {
            //        id_alg = (int)r[@"ID_ALG"];
            //        n_alg = r[@"N_ALG"].ToString().Trim();
            //        // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
            //        if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
            //            // добавить в список идентификатор параметра алгоритма расчета
            //            m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);
            //    }

            //    // получить значения для настройки визуального отображения
            //    if (dictVisualSettings.ContainsKey(n_alg) == true)
            //    {// установленные в проекте
            //        ratio = dictVisualSettings[n_alg.Trim()].m_ratio;
            //        round = dictVisualSettings[n_alg.Trim()].m_round;
            //    }
            //    else
            //    {// по умолчанию
            //        ratio = HTepUsers.s_iRatioDefault;
            //        round = HTepUsers.s_iRoundDefault;
            //    }

            //    m_dgvReak.ClearRows();

            //    for (int i = 0; i < DaysInMonth + 1; i++)
            //    {
            //        if (m_dgvReak.Rows.Count != DaysInMonth)
            //            m_dgvReak.AddRow(new DGVReaktivka.ROW_PROPERTY()
            //            {
            //                m_idAlg = id_alg
            //                ,
            //                //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
            //                //,
            //                m_Value = dt.AddDays(i).ToShortDateString()
            //                ,
            //                m_vsRatio = ratio
            //                ,
            //                m_vsRound = round
            //            });
            //        else
            //            m_dgvReak.AddRow(new DGVReaktivka.ROW_PROPERTY()
            //            {
            //                m_idAlg = id_alg
            //                ,
            //                //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
            //                //,
            //                m_Value = "ИТОГО"
            //                ,
            //                m_vsRatio = ratio
            //                ,
            //                m_vsRound = round
            //            }
            //            , DaysInMonth);
            //    }
            //}

            //m_dgvReak.Rows[dtBegin.Day - 1].Selected = true;
            //m_currentOffSet = Session.m_curOffsetUTC;
        }

        /// <summary>
        /// очистка грида
        /// </summary>
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected virtual void clear(int iCtrl = (int)INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;

            deleteSession();
            //??? повторная проверка
            if (bClose == true)
            {
                //(PanelManagementReak as PanelManagmentReaktivka).Clear();

                if (!(m_arTableDictPrjs == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                    {
                        if (!(m_arTableDictPrjs[i] == null))
                        {
                            m_arTableDictPrjs[i].Clear();
                            m_arTableDictPrjs[i] = null;
                        }
                    }

                cbx = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                //for (int i = 0; i < length; i++)
                //{
                //    //m_dgvReak.ClearRows();
                //}
                //dgvReak.ClearColumns();
            }
            else
            {
                //for (int i = 0; i < length; i++)
                //{
                //    // очистить содержание представления
                //    //m_dgvReak.ClearValues();
                //}
                ;
            }
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
                , HandlerDb.GetQueryComp()
                // параметры расчета
                , HandlerDb.GetQueryParameters(Type)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                //, HandlerDb.GetQueryModeDev()
                //// единицы измерения
                //, m_handlerDb.GetQueryMeasures()
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
        /// 
        /// </summary>
        /// <param name="err"></param>
        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;

            //DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVar();

            //m_arTableOrigin[(int)m_ViewValues] =
            //HandlerDb.GetInVal(Type
            //, dtR
            //, ActualIdPeriod
            //, m_ViewValues
            //, out err);

            //m_arTableEdit[(int)m_ViewValues] =
            //    HandlerDb.SaveValues(m_arTableOrigin[(int)m_ViewValues], valuesFence(), (int)Session.m_currIdTimezone, out err);

            //saveInvalValue(out err);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION;

            onButtonLoadClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
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
            //updateDataValues();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 21;
            register(21, typeof(PanelTaskVedomostBl), @"Задача", @"Ведомости эн./блоков");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

