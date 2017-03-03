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
    public partial class PanelTaskAutobookYearlyPlan : HPanelTepCommon
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
            /*PERIOD, // идентификаторы периодов расчетов, использующихся на форме
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов*/
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //    , DENY_COMP_CALCULATED,
            //DENY_PARAMETER_CALCULATED // запрещенных для расчета
            //    , DENY_COMP_VISIBLED,
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Отображение значений в табличном представлении(план)
        /// </summary>
        protected DataGridViewAutobookYearlyPlan m_dgvValues;
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
            return new PanelManagementAutobookYearlyPlan();
        }

        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementAutobookYearlyPlan PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementAutobookYearlyPlan;
            }
        }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskAutobookYarlyPlanCalculate();
        }

        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementAutobookYearlyPlan : PanelManagementTaskCalculate //HPanelCommon
        {
            /// <summary>
            /// Перечисление - идентификаторы элементов управления
            /// </summary>
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1
                , BUTTON_SAVE, BUTTON_LOAD
                , CHKBX_EDIT
                    , COUNT
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
            public PanelManagementAutobookYearlyPlan()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels) //4, 3
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = new Control();
                int posRow = -1; // позиция по оси "X" при позиционировании элемента управления

                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                SuspendLayout();

                posRow = 6;
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
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Признак Корректировка_включена/корректировка_отключена 
                ctrl = new CheckBox();
                ctrl.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
                ctrl.Text = @"Корректировка значений разрешена";
                ctrl.Dock = DockStyle.Top;
                ctrl.Enabled = false;
                (ctrl as CheckBox).Checked = true;
                Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);

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
                else
                    ;
            }
            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, ItemCheckEventArgs ev)
            {
                throw new NotImplementedException();
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
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            int posRow = -1; // позиция по оси "X" при позиционировании элемента управления   
            int posColdgvValues = 4;

            SuspendLayout();

            posRow = 0;

            m_dgvValues = new DataGridViewAutobookYearlyPlan(INDEX_CONTROL.DGV_PLANEYAR.ToString());
            m_dgvValues.Dock = DockStyle.Fill;
            m_dgvValues.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dgvValues.AllowUserToResizeRows = false;
            m_dgvValues.AddColumn("Выработка, тыс. кВтч", false, "Output");

            m_dgvValues.Columns["DATE"].Visible = false;
            foreach (DataGridViewColumn column in m_dgvValues.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_dgvValues.CellEndEdit += dgvYear_CellEndEdit;
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
            tlpYear.Controls.Add(m_dgvValues, 0, 2);
            Controls.Add(tlpYear, 1, posRow);
            SetColumnSpan(tlpYear, 9); SetRowSpan(tlpYear, 10);
            //
            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, posColdgvValues);
            SetRowSpan(PanelManagement, RowCount);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4, 10);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(HPanelTepCommon_btnUpdate_Click);
            //(btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
            //    new EventHandler(hPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);

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
                m_dgvValues.FillTableEdit((int)Session.m_Id);
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

            ID_PERIOD idProfilePeriod;
            ID_TIMEZONE idProfileTimezone;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            //m_dictTableDictPrj = new DataTable[(int)ID_DBTABLE.COUNT];
            int role = HTepUsers.Role;

            for (INDEX_ID id = INDEX_ID.ALL_COMPONENT; id < INDEX_ID.COUNT; id++)
                switch (id) {
                    /*case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH, (int)ID_PERIOD.YEAR };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;*/
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
            initialize(new ID_DBTABLE[] { /*ID_DBTABLE.PERIOD*/ }, out err, out errMsg);

            m_dictTableDictPrj.FilterDbTableTimezone = DictionaryTableDictProject.DbTableTimezone.Msk;
            m_dictTableDictPrj.FilterDbTableTime = DictionaryTableDictProject.DbTableTime.Month;
            m_dictTableDictPrj.FilterDbTableCompList = DictionaryTableDictProject.DbTableCompList.Tec | DictionaryTableDictProject.DbTableCompList.Tg;

            foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP].Rows) {
                id_comp = (int)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);

                m_dgvValues.AddIdComp(id_comp, "Output");
            }

            m_dgvValues.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);

            try{
                if (m_dictProfile.GetObjects(((int)ID_PERIOD.YEAR).ToString(), ((int)INDEX_CONTROL.DGV_PLANEYAR).ToString()).Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                    if (int.Parse(m_dictProfile.GetObjects(((int)ID_PERIOD.YEAR).ToString(), ((int)INDEX_CONTROL.DGV_PLANEYAR).ToString()).Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                    else
                        (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                else
                    (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                if ((Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                    m_dgvValues.AddBRead(false);
                else
                    m_dgvValues.AddBRead(true);

                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                else
                    (Controls.Find(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutoBookYarlyPlan::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (err == 0) {
                try {
                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = (ID_TIMEZONE)Enum.Parse(typeof(ID_TIMEZONE), m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]);
                    PanelManagement.FillValueTimezone (m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , (ID_TIMEZONE)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]));
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)Enum.Parse(typeof(ID_PERIOD), m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]);
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , (ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
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

                        m_dgvValues.ShowValues(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]);
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
                TaskCalculateType
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
                    HandlerDb.CreateSession(m_Id
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
                    m_Id
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

            m_dgvValues.ClearRows();
            DateTime dtNew = new DateTime(PanelManagement.DatetimeRange.Begin.Year, 1, 1); //??? почему 'Begin', а не 'End'
            //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
            //заполнение представления
            for (int i = 0; i < GetMonth.Count(); i++)
            {
                m_dgvValues.AddRow(new DataGridViewAutobookYearlyPlan.ROW_PROPERTY()
                {
                    m_idAlg = id_alg
                    //, m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                    , m_Value = GetMonth[i]
                    , m_vsRatio = ratio
                    , m_vsRound = round
                });

                m_dgvValues.Rows[i].Cells["DATE"].Value = dtNew.ToShortDateString();
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
            ID_TIMEZONE idTimezone = PanelManagement.IdTimezone;

            Session.SetCurrentTimeZone(idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Зарегистрировать/отменить регистрацию обработчика события изменение даты/времени
        /// </summary>
        /// <param name="active">Признак регистрации/отмены регистрации</param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementAutobookYearlyPlan.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
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
                m_dgvValues.ClearRows();
                //// удалить все столбцы
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                m_dgvValues.ClearValues();

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

            for (int i = 0; i < m_dgvValues.Rows.Count; i++)
            {
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = getStructurInval(dtrPer[i], out err);
                dr_saveValue = valuesFence.Select(string.Format(m_TableEdit.Locale, "WR_DATETIME = '{0:o}'", m_dgvValues.Rows[i].Cells["DATE"].Value));

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
                return m_dgvValues.FillTableEdit((int)Session.m_Id);
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
        //        m_dgvValues.Rows[i].Cells["DATE"].Value = dtNew.ToShortDateString();
        //        dtNew = dtNew.AddMonths(1);
        //    }

        //    m_dgvValues.Rows[dtBegin.Month - 1].Selected = true;
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
