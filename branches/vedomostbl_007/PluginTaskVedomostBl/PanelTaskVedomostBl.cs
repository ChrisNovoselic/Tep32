//using Excel = Microsoft.Office.Interop.Excel;
using HClassLibrary;
using InterfacePlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    public class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// 
        /// </summary>
        static int m_currentOffSet;
        /// <summary>
        /// 
        /// </summary>
        static bool flagBl = true;
        /// <summary>
        /// Делегат 
        /// </summary>
        /// <param name="id">ид грида</param>
        /// <returns>picture</returns>
        public delegate PictureBox DelgetPictureOfIdComp(int id);
        /// <summary>
        /// Делегат 
        /// </summary>
        /// <returns>грид</returns>
        public delegate DataGridView DelgetDataGridViewOfIdComp();
        /// <summary>
        /// экземпляр делегата
        /// </summary>
        static public DelgetPictureOfIdComp m_getPicture;
        static public DelgetDataGridViewOfIdComp m_getDGV;
        static public IntDelegateFunc m_getIdComp;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        static bool m_bflgClear = false;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Массив словарей для составления хидеров каждого блока(ТГ)
        /// </summary>
        static Dictionary<int, List<string[]>> m_dict;
        /// <summary>
        /// Листы с хидерами грида
        /// </summary>
        protected static List<string> m_listGroupSett_1 = new List<string>
        {
         "Острый пар", "Горячий промперегрев", "ХПП"
        };
        protected static List<string> m_listGroupSett_2 = new List<string>
        {
         "Питательная вода","Продувка", "Конденсатор", "Холодный воздух"
         , "Горячий воздух", "Кислород", "VI отбор", "VII отбор"
        };
        protected static List<string> m_listGroupSett_3 = new List<string>
        {
          "Уходящие газы","","" ,"","РОУ", "Сетевая вода", "Выхлоп ЦНД"
        };
        /// <summary>
        /// Лист с группами хидеров отображения
        /// </summary>
        protected static List<List<string>> m_listHeader = new List<List<string>> { m_listGroupSett_1, m_listGroupSett_2, m_listGroupSett_3 };
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
            LABEL_DESC, TBLP_HGRID, PICTURE_BOXDGV, PANEL_PICTUREDGV,
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
        /// 
        /// </summary>
        protected enum PROFILE_INDEX
        {
            UNKNOW = -1,
            TIMEZONE = 101, MAIL, PERIOD,
            RATIO = 201, ROUND, EDIT_COLUMN = 204,
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
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskVedomostBlCalculate; } }
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION ? ID_PERIOD.MONTH : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected HandlerDbTaskCalculate.INDEX_TABLE_VALUES m_ViewValues;
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
        protected PictureVedBl m_pictureVedBl;
        /// <summary>
        /// 
        /// </summary>
        static VedomostBlCalculate m_VedCalculate;
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableEdit
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
            /// <summary>
            /// индекс подсказки
            /// </summary>
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
                PICTURE_BOXDGV, PANEL_PICTUREDGV,
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
            /// <summary>
            /// 
            /// </summary>
            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;
            /// <summary>
            /// Тип обработчика события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            /// <param name="ev">Аргумент события</param>
            public delegate void ItemCheckedParametersEventHandler(ItemCheckedParametersEventArgs ev);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="idComp"></param>
            public delegate DataGridView GetDgvOfId(int idComp);
            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;
            /// <summary>
            /// 
            /// </summary>
            public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            /// <summary>
            /// 
            /// </summary>
            public PanelManagementVedomost()
                : base(4, 3)
            {
                try
                {
                    InitializeComponents();
                    toolTipText = new string[m_listHeader.Count];
                    (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

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
                //Признаки включения/исключения для отображения блока(ТГ)
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Выбрать блок для отображения:";
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
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить столбец(ы) для отображения:";
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
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
                Controls.Add(tlpChk, 0, posRow = posRow + 4);
                SetColumnSpan(tlpChk, 4); SetRowSpan(tlpChk, 2);
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
            /// обработчик события - отображения всплывающей подсказки по группам
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
                    RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

                    for (int i = 0; i < arRb.Length; i++)
                    {
                        arRb[i].CheckedChanged += TableLayoutPanelkVed_CheckedChanged;
                        arRb[i].Text = text[i];
                        m_listId.Add(id[i]);
                        arRb[i].Checked = bChecked[i];

                        if (RowCount * ColumnCount < arRb.Length)
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() => RowCount++));
                                Invoke(new Action(() => RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 20F))));
                            }
                            else
                            {
                                if (ColumnCount > RowCount)
                                {
                                    RowCount++;
                                    RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
                                }
                                else
                                {
                                    ColumnCount++;
                                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                                }
                            }
                        }

                        indx = i;
                        if (!(indx < arRb.Length))
                            //indx += (int)(indx / RowCount);

                            row = indx / RowCount;
                        col = indx % (RowCount - 0);

                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Controls.Add(arRb[i], col, row)));
                            Invoke(new Action(() => AutoScroll = true));
                        }
                        else
                            Controls.Add(arRb[i], col, row);
                    }
                }

                /// <summary>
                /// Обработчик события - переключение блока(ТГ)
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                public void TableLayoutPanelkVed_CheckedChanged(object sender, EventArgs e)
                {
                    int id = SelectedId;
                    PictureBox pictrure;

                    if ((sender as RadioButton).Checked == true)
                    {
                        pictrure = m_getPicture(id);
                        pictrure.Visible = true;
                        pictrure.Enabled = true;
                    }
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Controls.Clear();
                    m_listId.Clear();
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
                /// <summary>
                /// 
                /// </summary>
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
            ///, в соответствии с 'arIndexIdToAdd'
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
            /// Формирование текста всплывающей подсказки 
            /// для групп
            /// </summary>
            /// <param name="listText">перечень заголовков, входящих в группу</param>
            /// <returns>текст всплывающей подсказки</returns>
            private string fromationToolTipText(List<string> listText)
            {
                string strTextToolTip = string.Empty;

                foreach (var item in listText)
                {
                    if (strTextToolTip != string.Empty)
                        if (item != "")
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
                        Logging.Logg().Error(@"PanelManagementTaskVed::AddComponentRB () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
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
                CheckedListBox clbx = (Controls.Find(INDEX_CONTROL_BASE.CLBX_COL_VISIBLED.ToString(), true)[0] as CheckedListBox);

                itemCheck((obj as IControl).SelectedId, getIndexIdOfControl(obj as Control), ev.NewValue);

                //if (clbx.CheckedItems.Count == 0)
                //    clbx.SetItemChecked((obj as IControl).SelectedId + 1 == clbx.Items.Count ? 0 : (obj as IControl).SelectedId + 1, true);
                //else if (clbx.CheckedItems.Count == 1)
                //    if (clbx.CheckedItems.Contains(clbx.CheckedItems[(obj as IControl).SelectedId]))
                //        ;
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
        /// класс пикчи
        /// </summary>
        protected class PictureVedBl : PictureBox
        {
            /// <summary>
            /// ид Пикчи
            /// </summary>
            public int m_idCompPicture;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="viewActive"></param>
            public PictureVedBl(DGVVedomostBl viewActive)
            {
                InitializeComponents(viewActive);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="viewActive"></param>
            private void InitializeComponents(DGVVedomostBl viewActive)
            {
                int _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

                Size = new Size(viewActive.Width - 10, _drwH);
                m_idCompPicture = viewActive.m_idCompDGV;
                Controls.Add(viewActive);
            }
        }

        /// <summary>
        /// класс вьюхи
        /// </summary>
        protected class DGVVedomostBl : DataGridView
        {
            /// <summary>
            /// ширина и высота
            /// </summary>
            static int m_drwW,
                m_drwH = m_listHeader.Count;
            /// <summary>
            /// 
            /// </summary>
            Rectangle recParentCol;
            /// <summary>
            /// словарь названий заголовков 
            /// верхнего и среднего уровней
            /// </summary>
            static Dictionary<int, List<string>> headerTop = new Dictionary<int, List<string>>(),
                headerMiddle = new Dictionary<int, List<string>>();
            /// <summary>
            /// соотношение заголовков
            /// </summary>
            static Dictionary<int, int[]> m_arIntTopHeader = new Dictionary<int, int[]> { },
            m_arMiddleCol = new Dictionary<int, int[]> { };
            /// <summary>
            /// 
            /// </summary>
            public enum INDEX_HEADER
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }
            /// <summary>
            /// 
            /// </summary>
            public int m_idCompDGV;
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG = 0, DATE, COUNT }
            /// <summary>
            /// 
            /// </summary>
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;
            private Dictionary<int, COLUMN_PROPERTY> m_dictPropertyColumns;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DGVVedomostBl(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="nameDGV">имя окна отображения данных</param>
            private void InitializeComponents(string nameDGV)
            {
                Name = nameDGV;
                Dock = DockStyle.None;
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
                //
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                AllowUserToResizeColumns = false;
                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
                ColumnHeadersHeight = ColumnHeadersHeight * m_drwH;//высота от нижнего(headerText)
                ScrollBars = ScrollBars.None;

                AddColumns(-2, "ALG", string.Empty, false);
                AddColumns(-1, "Date", "Дата", true);
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
                /// признак общей группы
                /// </summary>
                public string m_topHeader;
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
            /// Структура для описания добавляемых столбцов
            /// </summary>
            public class COLUMN_PROPERTY
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
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// признак агрегации
                /// </summary>
                public int m_Avg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRound;
                /// <summary>
                /// Имя колонки
                /// </summary>
                public string nameCol;
                /// <summary>
                /// Текст в колонке
                /// </summary>
                public string hdrText;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public string topHeader;
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="nameCol">имя колонки</param>
            /// <param name="headerText">текст заголовка</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, string nameCol, string headerText, bool bVisible)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = idHeader, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    column.Frozen = true;
                    column.Visible = bVisible;
                    column.ReadOnly = false;
                    column.Name = nameCol;
                    column.HeaderText = headerText;
                    column.DefaultCellStyle.Alignment = alignText;
                    //column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DGVVedBl::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="col_prop">Структура для описания добавляемых столбцов</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, COLUMN_PROPERTY col_prop, bool bVisible)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    if (m_dictPropertyColumns == null)
                        m_dictPropertyColumns = new Dictionary<int, COLUMN_PROPERTY>();

                    if (!m_dictPropertyColumns.ContainsKey(col_prop.m_idAlg))
                        m_dictPropertyColumns.Add(col_prop.m_idAlg, col_prop);
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    //foreach (HDataGridViewColumn col in Columns)
                    //    if ((col.m_iIdComp > 0)
                    //        && (col.m_iIdComp < 1000))
                    //    {
                    //        indxCol = Columns.IndexOf(col);
                    //        break;
                    //    }

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_topHeader = col_prop.topHeader, m_iIdComp = idHeader };
                    alignText = DataGridViewContentAlignment.MiddleRight;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        //if (idHeader < 0)
                        //{// для служебных столбцов
                        if (bVisible == true)
                        {// только для столбца с [SYMBOL]
                            alignText = DataGridViewContentAlignment.MiddleLeft;
                        }
                        column.Frozen = true;
                        column.ReadOnly = true;
                        //}
                    }

                    column.HeaderText = col_prop.hdrText;
                    column.Name = col_prop.nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.Visible = bVisible;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewVedBl::AddColumn (idHeader=" + idHeader + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
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
            /// <param name="rowProp"></param>
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
            /// <param name="rowProp"></param>
            /// <param name="DaysInMonth"></param>
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

            /// <summary>
            /// Подготовка параметров к рисовке хидера
            /// </summary>
            /// <param name="dgv">активное окно отображения данных</param>
            public void dgvConfigCol(DataGridView dgv)
            {
                int cntCol = 0;
                formingTitleLists((dgv as DGVVedomostBl).m_idCompDGV);

                formRelationsHeading((dgv as DGVVedomostBl).m_idCompDGV);

                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible == true)
                        cntCol++;

                m_drwW = cntCol * dgv.Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width +
                    dgv.Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width / m_listHeader.Count;

                dgv.Paint += new PaintEventHandler(dataGridView1_Paint);
            }

            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            /// <param name="idTG">номер идТГ</param>
            private void formingTitleLists(int idTG)
            {
                string _oldItem = string.Empty;
                List<string> _listTop = new List<string>(),
                    _listMiddle = new List<string>();

                if (headerTop.ContainsKey(idTG))
                    headerTop.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_iIdComp >= 0)
                        if (col.Visible == true)
                            if (col.m_topHeader != "")
                                if (col.m_topHeader != _oldItem)
                                {
                                    _oldItem = col.m_topHeader;
                                    _listTop.Add(col.m_topHeader);
                                }
                                else;
                            else
                                _listTop.Add(col.m_topHeader);
                        else;
                    else;

                headerTop.Add(idTG, _listTop);

                if (headerMiddle.ContainsKey(idTG))
                    headerMiddle.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_iIdComp >= 0)
                        if (col.Visible == true)
                            if (col.Name != _oldItem)
                            {
                                _oldItem = col.Name;
                                _listMiddle.Add(col.Name);
                            }

                headerMiddle.Add(idTG, _listMiddle);
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            /// <param name="idDgv">номер окна отображения</param>
            private void formRelationsHeading(int idDgv)
            {
                string _oldItem = string.Empty;
                int _indx = 0,
                    _untdColM = 0;
                int[] _arrIntTop = new int[headerTop[idDgv].Count()],
                    _arrIntMiddle = new int[headerMiddle[idDgv].Count()];

                if (m_arIntTopHeader.ContainsKey(idDgv))
                    m_arIntTopHeader.Remove(idDgv);

                foreach (var item in headerTop[idDgv])
                {
                    int untdCol = 0;
                    foreach (HDataGridViewColumn col in Columns)
                        if (col.Visible == true)
                            if (col.m_topHeader == item)
                                if (!(item == ""))
                                    untdCol++;
                                else
                                {
                                    untdCol = 1;
                                    break;
                                }
                    _arrIntTop[_indx] = untdCol;
                    _indx++;
                }

                m_arIntTopHeader.Add(idDgv, _arrIntTop);
                _indx = 0;

                if (m_arMiddleCol.ContainsKey(idDgv))
                    m_arMiddleCol.Remove(idDgv);

                foreach (var item in headerMiddle[idDgv])
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_iIdComp > -1)
                            if (item == col.Name)
                                _untdColM++;
                            else
                                if (_untdColM > 0)
                                break;
                    }
                    _arrIntMiddle[_indx] = _untdColM;
                    _indx++;
                    _untdColM = 0;
                }
                m_arMiddleCol.Add(idDgv, _arrIntMiddle);
            }

            /// <summary>
            /// Скрыть/показать столбцы из списка групп
            /// </summary>
            /// <param name="dgvActive"></param>
            /// <param name="listHeaderTop"></param>
            /// <param name="isCheck"></param>
            public void HideColumns(DataGridView dgv, List<string> listHeaderTop, bool isCheck)
            {
                foreach (var item in listHeaderTop)
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_topHeader == item)
                            if (isCheck)
                                col.Visible = true;
                            else
                                col.Visible = false;
                    }

                dgvConfigCol(dgv);
            }

            /// <summary>
            /// обработчик события перерисовки грида(построение шапки заголовка)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void dataGridView1_Paint(object sender, PaintEventArgs e)
            {
                int _indxCol = 0;
                Rectangle _r1 = new Rectangle();
                Rectangle _r2 = new Rectangle();
                Pen pen = new Pen(Color.Black);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                m_drwH = 3;
                //
                for (int i = 0; i < Columns.Count; i++)
                    if (GetCellDisplayRectangle(i, -1, true).Height > 0 & GetCellDisplayRectangle(i, -1, true).X > 0)
                    {
                        recParentCol = GetCellDisplayRectangle(i, -1, true);
                        _r1 = recParentCol;
                        _r2 = recParentCol;
                        break;
                    }

                m_drwH = _r1.Height / m_drwH;

                foreach (var item in headerMiddle[(sender as DGVVedomostBl).m_idCompDGV])
                {
                    //get the column header cell
                    _r1.Width = m_arMiddleCol[(sender as DGVVedomostBl).m_idCompDGV][headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item)]
                        * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    _r1.Height = m_drwH + 3;//??? 

                    if (headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item) - 1 > -1)
                        _r1.X = _r1.X + m_arMiddleCol[(sender as DGVVedomostBl).m_idCompDGV][headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item) - 1]
                            * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        _r1.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                        _r1.Y = _r1.Y + _r1.Height;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), _r1);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      _r1,
                      format);
                    e.Graphics.DrawRectangle(pen, _r1);
                }

                foreach (var item in headerTop[(sender as DGVVedomostBl).m_idCompDGV])
                {
                    //get the column header cell
                    _r2.Width = m_arIntTopHeader[(sender as DGVVedomostBl).m_idCompDGV][_indxCol] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    _r2.Height = m_drwH + 2;//??? 

                    if (_indxCol - 1 > -1)
                        _r2.X = _r2.X + m_arIntTopHeader[(sender as DGVVedomostBl).m_idCompDGV][_indxCol - 1] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        _r2.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                        _r2.Y += _r2.Y;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), _r2);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      _r2,
                      format);
                    e.Graphics.DrawRectangle(pen, _r2);
                    _indxCol++;
                }

                //(sender as DGVVedomostBl).Paint -= new PaintEventHandler(dataGridView1_Paint);
            }

            /// <summary>
            /// обработчик события - перерисовки ячейки
            /// </summary>
            /// <param name="sender"></param>0
            /// <param name="e"></param>
            static void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
            {
                if (e.RowIndex == -1 && e.ColumnIndex > -1)
                {
                    e.PaintBackground(e.CellBounds, false);

                    Rectangle r2 = e.CellBounds;
                    r2.Y += e.CellBounds.Height / m_drwH;
                    r2.Height = e.CellBounds.Height / m_drwH;
                    e.PaintContent(r2);
                    e.Handled = true;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tableOrigin"></param>
            public void ShowValues(DataTable tableOrigin)
            {
                DataTable _dtOriginVal = new DataTable();
                int idAlg = -1
                   , idParameter = -1
                   , iQuality = -1
                   , _hoursOffSet
                   , iCol = 0//, iRow = 0
                   , _vsRatioValue = -1
                   , iRowCount = 0;
                double dblVal = -1F,
                    dbSumVal = 0;
                DataRow[] parameterRows = null;

                _dtOriginVal = tableOrigin.Copy();

                if (flagBl)
                    _hoursOffSet = -1 * (DateTimeOffset.Now.Offset.Hours + 10);
                else
                    _hoursOffSet = (m_currentOffSet / 60);

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                            {
                                idAlg = col.m_iIdComp;
                                _vsRatioValue = m_dictPropertyColumns[idAlg].m_vsRatio;

                                if (Convert.ToDateTime(_dtOriginVal.Rows[row.Index][@"WR_DATETIME"]).AddHours(_hoursOffSet).ToShortDateString() ==
                                        row.Cells["Date"].Value.ToString())
                                {
                                    dblVal = ((double)_dtOriginVal.Rows[row.Index][@"VALUE"]);
                                    row.Cells[iCol].Value = dblVal.ToString(@"F" + m_dictPropertyColumns[idAlg].m_vsRound,
                                           CultureInfo.InvariantCulture);
                                }
                            }
                            else
                            if (m_dictPropertyColumns[idAlg].m_Avg == 0)
                                row.Cells[iCol].Value = sumVal(col.Index).ToString(@"F" + m_dictPropertyColumns[idAlg].m_vsRound, CultureInfo.InvariantCulture);
                            else
                                row.Cells[iCol].Value = avgVal(col.Index).ToString(@"F" + m_dictPropertyColumns[idAlg].m_vsRound, CultureInfo.InvariantCulture);
                        }

                    iCol++;
                }
            }

            /// <summary>
            /// Получение суммы по столбцу
            /// </summary>
            /// <param name="indxCol"></param>
            /// <returns></returns>
            private double sumVal(int indxCol)
            {
                double _sumValue = 0F;

                try
                {
                    foreach (DataGridViewRow row in Rows)
                        if (Rows.Count - 1 != row.Index)
                            _sumValue += m_VedCalculate.AsParseToF(row.Cells[indxCol].Value.ToString());
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка суммирования столбца!");
                }
             

                return _sumValue;
            }

            /// <summary>
            /// Получение среднего по столбцу
            /// </summary>
            /// <param name="indxCol"></param>
            /// <returns></returns>
            private double avgVal(int indxCol)
            {
                int cntNum = 0;
                double _avgValue = 0F
                   , _sumValue = 0F;

                try
                {
                    foreach (DataGridViewRow row in Rows)
                        _sumValue += m_VedCalculate.AsParseToF(row.Cells[indxCol].Value.ToString());
                    cntNum++;
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка усреднения столбца!");
                }
            

                return _avgValue = _sumValue / cntNum;
            }
        }

        /// <summary>
        ///класс для обработки данных
        /// </summary>
        public class VedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            private parsingData _pData;
            /// <summary>
            /// индекс уровней хидеров
            /// </summary>
            protected enum lvlHeader
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }

            public VedomostBlCalculate()
                : base()
            {

            }

            /// <summary>
            /// Создание словаря заголвоков для каждого блока(ТГ)
            /// </summary>
            /// <param name="dtSource"></param>
            public List<string[]> CreateDictHeader(DataTable dtSource, int param)
            {
                _pData = new parsingData(dtSource, param);

                return compilingDict(_pData.ListParam, dtSource.Select("ID_COMP = " + param));
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
            /// сборка и компановка словаря
            /// </summary>
            /// <param name="arlistStr">лист парамтеров</param>
            /// <param name="dtPars">таблица с данными</param>
            private List<string[]> compilingDict(List<List<string>> arlistStr, DataRow[] dtPars)
            {
                int cntHeader = 0;
                string[] _arStrHeader;
                List<string[]> listHeader = new List<string[]> { };

                var enumHeader = (from r in dtPars.AsEnumerable()
                                  orderby r.Field<int>("ID")
                                  select new
                                  {
                                      NAME_SHR = r.Field<string>("NAME_SHR"),
                                  }).Distinct();

                listHeader.Clear();

                for (int j = 0; j < arlistStr.Count; j++)
                {
                    if (arlistStr[j].Count < 3)
                        _arStrHeader = new string[arlistStr[j].Count + 1];
                    else
                        _arStrHeader = new string[arlistStr[j].Count];

                    bool bflagStopfor = false;
                    cntHeader = 0;

                    for (int i = arlistStr[j].Count - 1; i > -1; i--)
                    {
                        switch (i)
                        {
                            case (int)lvlHeader.TOP:
                                for (int t = 0; t < m_listHeader.Count; t++)
                                {
                                    for (int n = 0; n < m_listHeader[t].Count; n++)
                                    {
                                        cntHeader++;
                                        if (int.Parse(arlistStr[j].ElementAt((int)lvlHeader.TOP)) == cntHeader)
                                        {
                                            _arStrHeader[i] = m_listHeader[t][n];
                                            listHeader.Add(_arStrHeader);
                                            bflagStopfor = true;
                                            break;
                                        }
                                    }

                                    if (bflagStopfor)
                                        break;
                                }
                                break;
                            case (int)lvlHeader.MIDDLE:

                                if (arlistStr[j].Count < 3)
                                    _arStrHeader[i + 1] = "";

                                _arStrHeader[(int)lvlHeader.MIDDLE] = dtPars[j]["NAME_SHR"].ToString().Trim();
                                break;
                            case (int)lvlHeader.LOW:
                                _arStrHeader[i] = dtPars[j]["DESCRIPTION"].ToString().Trim();
                                break;
                            default:
                                break;
                        }
                    }
                }

                return listHeader;
            }

            /// <summary>
            /// 
            /// </summary>
            private class DataWorkClass
            {

            }

            /// <summary>
            /// преобразование числа в нужный формат отображения
            /// </summary>
            /// <param name="value">число</param>
            /// <returns>преобразованное число</returns>
            public float AsParseToF(string value)
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
                        fValue = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        if (value.ToString() == "")
                            fValue = 0;
                    }


                return fValue;
            }

            /// <summary>
            /// класс для формирования листа с параметрами 
            /// для формирования заголовков
            /// </summary>
            private class parsingData
            {
                /// <summary>
                /// набор листов с параметрами группировки
                /// </summary>
                private List<List<string>> arList;

                /// <summary>
                /// конструктор с параметрами
                /// </summary>
                /// <param name="dt">таблица с данными</param>
                /// <param name="param">параметр для выборки</param>
                public parsingData(DataTable dt, int param)
                {
                    disaggregationToParts(dt.Select("ID_COMP = " + param));
                }

                /// <summary>
                /// формирование листа параметров вида x.y.z,
                /// где x - TopHeader, y - MiddleHeader, y - LowHeader
                /// </summary>
                /// <param name="dtPars">таблица с данными</param>
                private void disaggregationToParts(DataRow[] dtPars)
                {
                    arList = new List<List<string>>(dtPars.Count());

                    foreach (DataRow row in dtPars)
                    {
                        List<string> list = new List<string>();
                        list = row["N_ALG"].ToString().Split('.', ',').ToList();
                        arList.Add(list);
                    }
                }

                /// <summary>
                /// возвращает лист с парметрами 
                /// для построения словаря заголовков
                /// </summary>
                public List<List<string>> ListParam
                {
                    get
                    {
                        return arList;
                    }
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
            m_VedCalculate = new VedomostBlCalculate();
            HandlerDb.IdTask = ID_TASK.VEDOM_BL;
            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
            m_dict = new Dictionary<int, List<string[]>> { };

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            InitializeComponent();
            m_getPicture = new DelgetPictureOfIdComp(GetPictureOfIdComp);
            m_getDGV = new DelgetDataGridViewOfIdComp(GetDGVOfIdComp);
            m_getIdComp = new IntDelegateFunc(GetIdComp);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control();
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить" 

            SuspendLayout();

            Controls.Add(PanelManagementVed, 0, posRow);
            SetColumnSpan(PanelManagementVed, 4); SetRowSpan(PanelManagementVed, 13);
            //контейнеры для DGV
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = INDEX_CONTROL.PICTURE_BOXDGV.ToString();
            pictureBox.TabStop = false;
            //
            Panel m_paneL = new Panel();
            m_paneL.Name = INDEX_CONTROL.PANEL_PICTUREDGV.ToString();
            m_paneL.Dock = DockStyle.Fill;
            (m_paneL as Panel).AutoScroll = true;
            Controls.Add(m_paneL, 5, posRow);
            SetColumnSpan(m_paneL, 9); SetRowSpan(m_paneL, 10);
            //
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
        /// Обработчик события - изменение отображения кол-во групп заголовка
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
                else; //throw new Exception (@"");
            else
                if (ev.m_newCheckState == CheckState.Checked)
                if (!(m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0))
                    m_arListIds[(int)ev.m_indxIdDeny].Remove(idItem);
                else; //throw new Exception (@"");
            else;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //Изменить структуру 'HDataGRidVIew's'          
            placementHGridViewOnTheForm(ev);
        }

        /// <summary>
        /// Обработчик события - Признак Корректировка_включена/корректировка_отключена 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelManagementVedomost_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Изменить структуру 'HDataGRidVIew's'
        /// </summary>
        /// <param name="item"></param>
        private void placementHGridViewOnTheForm(PanelManagementVedomost.ItemCheckedParametersEventArgs item)
        {
            bool bItemChecked = item.m_newCheckState == CheckState.Checked ? true :
                  item.m_newCheckState == CheckState.Unchecked ? false :
                      false;
            DGVVedomostBl cntrl = (getActiveView() as DGVVedomostBl);
            //Поиск индекса элемента отображения
            switch (item.m_indxIdDeny)
            {
                case INDEX_ID.HGRID_VISIBLE:
                    cntrl.HideColumns(cntrl as DataGridView, m_listHeader[item.m_idItem], bItemChecked);
                    ReSizeControls(cntrl as DataGridView);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Нахожджение активного DGV
        /// </summary>
        /// <returns>активная вьюха</returns>
        private DataGridView getActiveView()
        {
            Control cntrl = new Control();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                if (item.Visible == true)
                    foreach (DataGridView dgv in item.Controls)
                        cntrl = dgv;

            return (cntrl as DataGridView);
        }

        /// <summary>
        /// Настройка размеров контролов отображения
        /// </summary>
        private void ReSizeControls(DataGridView viewActive)
        {
            int cntCol = 0;

            for (int j = 1; j < viewActive.ColumnCount; j++)
                viewActive.Columns[j].Width = 65;

            foreach (DataGridViewColumn col in viewActive.Columns)
                if (col.Visible == true)
                    cntCol++;

            int _drwW = cntCol * viewActive.Columns[2].Width + 10
                , _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

            GetPictureOfIdComp((viewActive as DGVVedomostBl).m_idCompDGV).Size = new Size(_drwW + 2, _drwH);
            viewActive.Size = new Size(_drwW + 2, _drwH);
        }

        /// <summary>
        /// Обработчик события - добавления строк в грид
        /// (для изменение размера контролов)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGVVedomostBl_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ReSizeControls(sender as DataGridView);
        }

        /// <summary>
        /// Возвращает пикчу по номеру
        /// </summary>
        /// <param name="idComp">ид номер грида</param>
        public PictureBox GetPictureOfIdComp(int idComp)
        {
            int cnt = 0,
                outCnt = 0;
            PictureBox cntrl = new PictureBox();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
            {
                if (idComp == item.m_idCompPicture)
                {
                    outCnt = cnt;
                    cntrl = (item as PictureBox);
                }
                else
                {
                    (item as PictureBox).Visible = false;
                    (item as PictureBox).Enabled = false;
                }
                cnt++;
            }

            if (outCnt == 0 || outCnt == 5)
                WhichBlIsSelected = true;
            else
                WhichBlIsSelected = false;

            return cntrl;
        }

        /// <summary>
        /// Возвращает по номеру
        /// </summary>
        public DataGridView GetDGVOfIdComp()
        {
            DataGridView cntrl = new DataGridView();

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                foreach (DGVVedomostBl item in picture.Controls)
                    if (item.Visible == true)
                        cntrl = (item as DataGridView);

            return cntrl;
        }

        /// <summary>
        /// Возвращает idComp
        /// </summary>
        public int GetIdComp()
        {
            int _idComp = 0;

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                foreach (DGVVedomostBl item in picture.Controls)
                    if (item.Visible == true)
                        _idComp = item.m_idCompDGV;

            return _idComp;
        }

        /// <summary>
        /// Настройка размеров формы отображения данных
        /// </summary>
        /// <param name="dgv">активное окно отображения данных</param>
        public void SizeDgv(object dgv)
        {
            (dgv as DGVVedomostBl).dgvConfigCol(dgv as DataGridView);
        }

        /// <summary>
        /// Инициализация радиобаттанов
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeRB(Array namePut, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string[] arstrItem;
            RadioButton[] arRadioBtn;
            int[] arId_comp;
            int rbCnt = (int)INDEX_CONTROL.RADIOBTN_BLK1;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[]
            {
                INDEX_ID.BLOCK_VISIBLED
            };
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
                try
                {
                    if (arId_comp[m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count - 1] != 0)
                        //добавление радиобатонов на форму
                        (PanelManagementVed as PanelManagementVedomost).AddComponentRB(arId_comp
                                  , arstrItem
                                  , arIndxIdToAdd
                                  , arChecked
                                  , arRadioBtn);
                    rbCnt++;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initializeRB () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        /// <summary>
        /// Инициализация сетки данных
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        private void initializeDGV(Array namePut, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            Control ctrl = null;
            DateTime _dtRow = new DateTime(s_dtDefaultAU.Year, s_dtDefaultAU.Month, 1);
            DataTable dtComponentId = HandlerDb.GetHeaderDGV();//получение ид компонентов    

            //создание грида со значениями
            for (int j = (int)INDEX_CONTROL.DGV_DATA_B1; j < (int)INDEX_CONTROL.RADIOBTN_BLK1; j++)
            {
                ctrl = new DGVVedomostBl(namePut.GetValue(j).ToString());
                ctrl.Name = namePut.GetValue(j).ToString();
                (ctrl as DGVVedomostBl).m_idCompDGV = int.Parse(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows[j]["ID"].ToString());

                filingDictHeader(dtComponentId, (ctrl as DGVVedomostBl).m_idCompDGV);

                Dictionary<string, List<int>> _dictVisualSett = visualSettingsCol((ctrl as DGVVedomostBl).m_idCompDGV);

                for (int k = 0; k < m_dict[(ctrl as DGVVedomostBl).m_idCompDGV].Count; k++)
                {
                    int idPar = int.Parse(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select("ID_COMP = " + (ctrl as DGVVedomostBl).m_idCompDGV)[k]["ID_ALG"].ToString());
                    int _avg = int.Parse(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select("ID_COMP = " + (ctrl as DGVVedomostBl).m_idCompDGV)[k]["AVG"].ToString());

                    (ctrl as DGVVedomostBl).AddColumns(idPar, new DGVVedomostBl.COLUMN_PROPERTY
                    {
                        topHeader = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.TOP].ToString(),
                        nameCol = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.MIDDLE].ToString(),
                        hdrText = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.LOW].ToString(),
                        m_idAlg = idPar,//(ctrl as DGVVedomostBl).m_idCompDGV,
                        m_vsRatio = _dictVisualSett["ratio"][k],
                        m_vsRound = _dictVisualSett["round"][k],
                        m_Avg = _avg
                    }
                       , true);
                }

                for (int i = 0; i < DaysInMonth + 1; i++)
                    if ((ctrl as DGVVedomostBl).Rows.Count != DaysInMonth)
                        (ctrl as DGVVedomostBl).AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            //m_idAlg = id_alg
                            //,
                            m_Value = _dtRow.AddDays(i).ToShortDateString()
                        });
                    else
                    {
                        (ctrl as DGVVedomostBl).RowsAdded += DGVVedomostBl_RowsAdded;
                        (ctrl as DGVVedomostBl).AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            //m_idAlg = id_alg
                            //,
                            m_Value = "ИТОГО"
                        }
                       , DaysInMonth);
                    }

                SizeDgv(ctrl);
                m_pictureVedBl = new PictureVedBl(ctrl as DGVVedomostBl);
                (Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0] as Panel).Controls.Add(m_pictureVedBl);
            }
        }

        /// <summary>
        /// Инициализация объектов формы
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
                id_comp;
            Control ctrl = null;
            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = HTepUsers.Role;

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

            bool[] arChecked = new bool[m_listHeader.Count];
            //
            foreach (var list in m_listHeader)
            {
                id_comp = m_listHeader.IndexOf(list);
                //m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_comp);
                strItem = "Группа " + (id_comp + 1);
                // установить признак отображения группы столбцов
                arChecked[id_comp] = true;
                (PanelManagementVed as PanelManagementVedomost).AddComponent(id_comp
                    , strItem
                    , list
                    , arIndxIdToAdd
                    , arChecked);
            }
            //
            (PanelManagementVed as PanelManagementVedomost).ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.HGRID_VISIBLE });
            //Dgv's
            initializeDGV(namePut, out err, out errMsg);//???
            //радиобаттаны
            initializeRB(namePut, out err, out errMsg);

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
                    (ctrl as ComboBox).SelectedIndex = int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.TIMEZONE).ToString()]);
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    Session.SetCurrentPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    (PanelManagementVed as PanelManagementVedomost).SetPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)PROFILE_INDEX.PERIOD).ToString()]));
                    (ctrl as ComboBox).Enabled = false;

                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
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

            //m_dgvReak.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);
        }

        /// <summary>
        /// Получение визуальных настроек 
        /// для отображения данных на форме
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<int>> visualSettingsCol(int idComp)
        {
            int err = -1
             , id_alg = -1;
            List<int> ratio = new List<int>()
            , round = new List<int>();
            string n_alg = string.Empty;

            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            Dictionary<string, List<int>> _dictSett = new Dictionary<string, List<int>>();

            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
               , new int[] {
                    m_id_panel
                    , idComp }
               , out err);

            IEnumerable<DataRow> listParameter = ListParameter.Select(x => x).Where(x => (int)x["ID_COMP"] == idComp);

            foreach (DataRow r in listParameter)
            {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();
                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                    // добавить в список идентификатор параметра алгоритма расчета
                    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg) == true)
                {// установленные в проекте
                    ratio.Add(dictVisualSettings[n_alg.Trim()].m_ratio);
                    round.Add(dictVisualSettings[n_alg.Trim()].m_round);
                }
                else
                {// по умолчанию
                    ratio.Add(HTepUsers.s_iRatioDefault);
                    round.Add(HTepUsers.s_iRoundDefault);
                }
            }
            _dictSett.Add("ratio", ratio);
            _dictSett.Add("round", round);

            return _dictSett;
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <returns>кол-во дней</returns>
        public int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// Заполнение словаря[x] заголовков
        /// </summary>
        /// <param name="dt">табилца парамтеров</param>
        /// <param name="paramBl">параметр(идТГ)</param>
        protected void filingDictHeader(DataTable dt, int paramBl)
        {
            m_dict.Add(paramBl, m_VedCalculate.CreateDictHeader(dt, paramBl));//cловарь заголовков
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
             , id_alg = -1;
            DGVVedomostBl _dgv = (getActiveView() as DGVVedomostBl);
            string n_alg = string.Empty;
            DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);

            settingDateRange();
            Session.SetRangeDatetime(dtBegin, dtEnd);

            if (m_bflgClear)
            {
                clear();

                if (_dgv.Rows.Count != 0)
                    _dgv.ClearRows();

                for (int i = 0; i < DaysInMonth + 1; i++)
                {
                    if (_dgv.Rows.Count != DaysInMonth)
                        _dgv.AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = dt.AddDays(i).ToShortDateString()
                        });
                    else
                        _dgv.AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = "ИТОГО"
                        }
                        , DaysInMonth);
                }
            }

            _dgv.Rows[dtBegin.Day - 1].Selected = true;
            m_currentOffSet = Session.m_curOffsetUTC;
        }

        /// <summary>
        /// Установка длительности периода 
        /// </summary>
        private void settingDateRange()
        {
            int cntDays,
                today = 0;

            PanelManagementVed.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
              (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
  (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementVedomost.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

            PanelManagementVed.DateTimeRangeValue_Changed += new PanelManagementVedomost.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

        }

        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select().ToList<DataRow>();

                return listRes;
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
            DateTimeRange[] dtrGet;

            if (!WhichBlIsSelected)
                dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
            else
                dtrGet = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

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
                        // отобразить значения
                        (getActiveView() as DGVVedomostBl).ShowValues(m_TableOrigin);
                        //
                        //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = valuesFence;
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
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
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
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , CountBasePeriod
                        , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT]
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
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
             m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool WhichBlIsSelected
        {
            get { return flagBl; }

            set { flagBl = value; }
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
                , HandlerDb.GetQueryComp(Type)
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
        /// <summary>
        /// 
        /// </summary>
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
            updateDataValues();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

