using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using Excel = Microsoft.Office.Interop.Excel;
using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskReaktivka
{
    public class PanelTaskReaktivka : HPanelTepCommon
    {
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        static bool m_bflgClear = false;
        /// <summary>
        /// Часовой пояс(часовой сдвиг)
        /// </summary>
        protected static int m_currentOffSet;
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_DATA, LABEL_DESC
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
            //DENY_PARAMETER_CALCULATED, // запрещенных для расчета
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            , PERIOD, TIMEZONE,
            COMPONENT,
            //PARAMETER, 
            //, MODE_DEV/*, MEASURE*/,
            RATIO
               , COUNT
        }
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskReaktivkaCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskReaktivkaCalculate; } }
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
        /// 
        /// </summary>
        protected ReportExcel m_rptExcel;
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected INDEX_VIEW_VALUES m_ViewValues;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
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
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        private PanelManagementReaktivka _panelManagement;
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementReaktivka PanelManagementReak
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
        private PanelManagementReaktivka createPanelManagement()
        {
            return new PanelManagementReaktivka();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskReaktivkaCalculate();
        }
        /// <summary>
        /// Экземпляр класса отображения данных
        /// </summary>
        DGVReaktivka m_dgvReak;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskReaktivka(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.REAKTIVKA;

            m_arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();
            Session.SetRangeDatetime(PanelManagementReaktivka.s_dtDefaultAU, PanelManagementReaktivka.s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            m_dgvReak = new DGVReaktivka(INDEX_CONTROL.DGV_DATA.ToString());

            foreach (DataGridViewColumn column in m_dgvReak.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    

            SuspendLayout();

            this.Controls.Add(PanelManagementReak, 0, posRow);
            this.SetColumnSpan(PanelManagementReak, 4);
            this.SetRowSpan(PanelManagementReak, 9);

            this.Controls.Add(m_dgvReak, 5, posRow);
            this.SetColumnSpan(m_dgvReak, 9);
            this.SetRowSpan(m_dgvReak, 10);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += PanelTaskReaktivka_ClickExport;
            (PanelManagementReak as PanelManagementReaktivka).ItemCheck += new PanelManagementReaktivka.ItemCheckedParametersEventHandler(panelManagement_ItemCheck);
            m_dgvReak.CellEndEdit += m_dgvReak_CellEndEdit;
            //m_dgvReak.CellParsing += m_dgvReak_CellParsing;
        }

        /// <summary>
        /// Обработчик события - окончание редактирования отображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_dgvReak_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            m_dgvReak.SumValue(e.ColumnIndex, e.RowIndex);
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = valuesFence;
        }

        /// <summary>
        /// Обработчик события изменения значения в ячейке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_dgvReak_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {

        }

        /// <summary>
        /// Обработчик события - нажатие клавиши ЭКСПОРТ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskReaktivka_ClickExport(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();//
            m_rptExcel.CreateExcel(m_dgvReak, Session.m_rangeDatetime);
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
            int i = -1
                , id_comp = -1;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = (int)HTepUsers.Role;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[] {
                        //INDEX_ID.DENY_COMP_CALCULATED,
                        INDEX_ID.DENY_COMP_VISIBLED
                    };
            bool[] arChecked = new bool[arIndxIdToAdd.Length];
            //
            DataRow[] drEdtCol =
                HTepUsers.GetProfileUser_Tab(m_id_panel).Select("ID_UNIT = " + (int)HTepUsers.ID_ALLOWED.EDIT_COLUMN + " AND ID_EXT = " + HTepUsers.Role);
            DataRow[] drTZ =
               HTepUsers.GetProfileUser_Tab(m_id_panel).Select("ID_UNIT = " + (int)HTepUsers.ID_ALLOWED.QUERY_TIMEZONE + " AND ID_EXT = " + HTepUsers.Role);

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
            (PanelManagementReak as PanelManagementReaktivka).Clear();

            foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
            {
                id_comp = (Int32)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
                strItem = ((string)r[@"DESCRIPTION"]).Trim();
                // установить признак участия в расчете компонента станции
                //arChecked[0] = m_arListIds[(int)INDEX_ID.DENY_COMP_CALCULATED].IndexOf(id_comp) < 0;
                // установить признак отображения компонента станции
                arChecked[0] = m_arListIds[(int)INDEX_ID.DENY_COMP_VISIBLED].IndexOf(id_comp) < 0;
                (PanelManagementReak as PanelManagementReaktivka).AddComponent(id_comp
                    , strItem
                    , arIndxIdToAdd
                    , arChecked);

                if (m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count + 2 > m_dgvReak.Columns.Count)
                    m_dgvReak.AddColumn(id_comp, strItem, strItem, false, arChecked[0]);
            }
            //Установить обработчик события - добавить параметр
            //eventAddNAlgParameter += new DelegateObjectFunc((PanelManagement as PanelManagementTaskTepValues).OnAddParameter);
            // установить единый обработчик события - изменение состояния признака участие_в_расчете/видимость
            // компонента станции для элементов управления
            (PanelManagementReak as PanelManagementReaktivka).ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.DENY_COMP_VISIBLED });

            m_dgvReak.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

            if (err == 0)
            {
                try
                {
                    if (m_bflgClear == false)
                        m_bflgClear = true;
                    else
                        m_bflgClear = false;
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = Convert.ToInt32(drTZ[0]["VALUE"].ToString()); //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);

                    (ctrl as ComboBox).SelectedIndex = 1; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][1]);//??
                    (PanelManagementReak as PanelManagementReaktivka).SetPeriod(ID_PERIOD.MONTH);
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
                    //case INDEX_TABLE_DICTPRJ.PARAMETER:
                    //    errMsg = @"Получение строковых идентификаторов параметров в алгоритме расчета";
                    //    break;
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
            if (!(PanelManagementReak == null))
                if (active == true)
                    PanelManagementReak.DateTimeRangeValue_Changed += new PanelManagementReaktivka.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagementReak.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
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
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagementReak as PanelManagementReaktivka).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);
            if (m_bflgClear)
                // очистить содержание представления
                clear();
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
                //, HandlerDb.GetQueryParameters(Type)
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

                cbx = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                m_dgvReak.ClearRows();
                //dgvReak.ClearColumns();
            }
            else
                // очистить содержание представления
                m_dgvReak.ClearValues();
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
            settingDateRange();
            Session.SetRangeDatetime(dtBegin, dtEnd);

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
                if (dictVisualSettings.ContainsKey(n_alg) == true)
                {// установленные в проекте
                    ratio = dictVisualSettings[n_alg.Trim()].m_ratio;
                    round = dictVisualSettings[n_alg.Trim()].m_round;
                }
                else
                {// по умолчанию
                    ratio = HTepUsers.s_iRatioDefault;
                    round = HTepUsers.s_iRoundDefault;
                }

                m_dgvReak.ClearRows();

                for (int i = 0; i < DaysInMonth + 1; i++)
                {
                    if (m_dgvReak.Rows.Count != DaysInMonth)
                        m_dgvReak.AddRow(new DGVReaktivka.ROW_PROPERTY()
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
                    else
                        m_dgvReak.AddRow(new DGVReaktivka.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = "ИТОГО"
                            ,
                            m_vsRatio = ratio
                            ,
                            m_vsRound = round
                        }
                        , DaysInMonth);
                }
            }

            m_dgvReak.Rows[dtBegin.Day - 1].Selected = true;
            m_currentOffSet = Session.m_curOffsetUTC;
        }


        /// <summary>
        /// Установка длительности периода 
        /// </summary>
        private void settingDateRange()
        {
            int cntDays,
                today = 0;

            PanelManagementReak.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
              (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

            cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
  (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            today = (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
                (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

            PanelManagementReak.DateTimeRangeValue_Changed += new PanelManagementReaktivka.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

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
            err = -1;
            //DateTimeRange[] dtRangeArr = HandlerDb.GetDateTimeRangeValuesVar();

            //m_handlerDb.RecUpdateInsertDelete(getNameTableIn(dtRangeArr[0].Begin)
            //        , @"ID_PUT, DATE_TIME"
            //        , @""
            //        , m_TableOrigin
            //        , m_TableEdit
            //        , out err);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
              m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;

            DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.GetInVal(Type
            , dtR
            , ActualIdPeriod
            , out err);

            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                HandlerDb.SaveValues(m_TableOrigin, valuesFence, (int)Session.m_currIdTimezone, out err);

            saveInvalValue(out err);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.ARCHIVE;

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
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementReaktivka : HPanelCommon
        {
            /// <summary>
            /// 
            /// </summary>
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL,
                CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CLBX_COMP_VISIBLED, CLBX_COMP_CALCULATED,
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

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            public PanelManagementReaktivka()
                : base(4, 3)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            private void InitializeComponents()
            {
                //initializeLayoutStyle();
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

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить компонент для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                ctrl = new CheckedListBoxTaskReaktivka();
                ctrl.Name = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Top;
                (ctrl as CheckedListBoxTaskReaktivka).CheckOnClick = true;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 2);

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
                m_bflgClear = true;
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
                , hdtpEndtimePer = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;

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
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskReaktivka : CheckedListBox, IControl
            {
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                public CheckedListBoxTaskReaktivka()
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
            /// Добавить элемент компонент станции в списки
            /// , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(int id_comp, string text, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            {
                Control ctrl = null;

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskReaktivka).AddItem(id_comp, text, arChecked[i]);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
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
                        indxRes = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED;
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
            /// 
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

                if (strId.Equals(INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED.ToString()) == true)
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
        protected class DGVReaktivka : DataGridView
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DGVReaktivka(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nameDGV"></param>
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

                AddColumn(-2, string.Empty, "ALG", true, false);
                AddColumn(-1, "Дата", "Date", true, true);
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
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="idPut">индентификатор источника</param>
            public void AddColumn(string txtHeader, string nameCol, bool bRead, bool bVisibled)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    column.Frozen = true;
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

            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public void UpdateStructure(PanelManagementReaktivka.ItemCheckedParametersEventArgs item)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                int indx = -1
                    , cIndx = -1
                    , rKey = -1;
                bool bItemChecked = item.m_newCheckState == CheckState.Checked ? true :
                    item.m_newCheckState == CheckState.Unchecked ? false :
                        false;

                //Поиск индекса элемента отображения
                switch (item.m_indxIdDeny)
                {
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        // найти индекс столбца (компонента) - по идентификатору
                        foreach (HDataGridViewColumn c in Columns)
                            if (c.m_iIdComp == item.m_idItem)
                            {
                                indx = Columns.IndexOf(c);
                                break;
                            }
                            else
                                ;
                        break;
                    default:
                        break;
                }

                if (!(indx < 0))
                {
                    switch (item.m_indxIdDeny)
                    {
                        //case INDEX_ID.DENY_COMP_CALCULATED:
                        //    cIndx = indx;
                        //    // для всех ячеек в столбце
                        //    foreach (DataGridViewRow r in Rows)
                        //    {
                        //        indx = Rows.IndexOf(r);
                        //        if (getClrCellToComp(cIndx, indx, bItemChecked, out clrCell) == true)
                        //            r.Cells[cIndx].Style.BackColor = clrCell;
                        //        else
                        //            ;
                        //    }
                        //    (Columns[cIndx] as HDataGridViewColumn).m_bCalcDeny = !bItemChecked;
                        //    break;
                        //case INDEX_ID.DENY_PARAMETER_CALCULATED:
                        //    rKey = (int)Rows[indx].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;
                        //    // для всех ячеек в строке
                        //    foreach (DataGridViewCell c in Rows[indx].Cells)
                        //    {
                        //        cIndx = Rows[indx].Cells.IndexOf(c);
                        //        if (getClrCellToParameter(cIndx, indx, bItemChecked, out clrCell) == true)
                        //            c.Style.BackColor = clrCell;
                        //        else
                        //            ;

                        //        m_dictPropertiesRows[rKey].m_arPropertiesCells[cIndx].m_bCalcDeny = !bItemChecked;
                        //    }
                        //    break;
                        case INDEX_ID.DENY_COMP_VISIBLED:
                            cIndx = indx;
                            // для всех ячеек в столбце
                            Columns[cIndx].Visible = bItemChecked;
                            break;
                        //case INDEX_ID.DENY_PARAMETER_VISIBLED:
                        //    // для всех ячеек в строке
                        //    Rows[indx].Visible = bItemChecked;
                        //    break;
                        //default:
                        //    break;
                    }
                }
                else
                    ; // нет элемента для изменения стиля
            }

            /// <summary>
            /// Отображение значений
            /// </summary>
            /// <param name="source">таблица с даными</param>
            public void ShowValues(DataTable source)
            {
                int idAlg = -1
                   , idParameter = -1
                   , iQuality = -1
                   , iCol = 0//, iRow = 0
                   , vsRatioValue = -1
                   , iRowCount = 0;
                double dblVal = -1F,
                    dbSumVal = 0;
                DataRow[] parameterRows = null;

                var enumTime = (from r in source.AsEnumerable()
                                orderby r.Field<DateTime>("WR_DATETIME")
                                select new
                                {
                                    WR_DATETIME = r.Field<DateTime>("WR_DATETIME"),
                                }).Distinct();

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                            {
                                iRowCount++;
                                idAlg = (int)row.Cells["ALG"].Value;
                                parameterRows =
                                source.Select(String.Format(source.Locale, "ID_PUT = " + col.m_iIdComp));

                                for (int i = 0; i < parameterRows.Count(); i++)
                                {
                                    if (Convert.ToDateTime(parameterRows[i][@"WR_DATETIME"]).AddMinutes(m_currentOffSet).ToShortDateString() ==
                                        row.Cells["Date"].Value.ToString())
                                    {
                                        idParameter = (int)parameterRows[i][@"ID_PUT"];
                                        dblVal = ((double)parameterRows[i][@"VALUE"]);
                                        iQuality = (int)parameterRows[i][@"QUALITY"];

                                        row.Cells[iCol].ReadOnly = double.IsNaN(dblVal);
                                        vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                                        dblVal *= Math.Pow(10F, -1 * vsRatioValue);

                                        row.Cells[iCol].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                            System.Globalization.CultureInfo.InvariantCulture);
                                        dbSumVal += dblVal;
                                    }
                                }
                            }
                            else
                                row.Cells[iCol].Value = dbSumVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                    System.Globalization.CultureInfo.InvariantCulture);
                        }

                    iCol++;
                    dbSumVal = 0;
                    iRowCount = 0;
                }
            }

            /// <summary>
            /// Перерасчет суммы по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <param name="indxRow">индекс строки</param>
            /// <param name="newValue">новое значение</param>
            public void SumValue(int indxCol, int indxRow)
            {
                int idAlg = -1;
                double sumValue = 0F
                    , value;

                idAlg = (int)Rows[indxRow].Cells[0].Value;

                foreach (DataGridViewRow row in Rows)
                    if (Rows.Count - 1 != row.Index)
                        if (double.TryParse(row.Cells[indxCol].Value.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                            sumValue += value;
                        else ;
                    else
                        row.Cells[indxCol].Value = sumValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                    System.Globalization.CultureInfo.InvariantCulture);

                formatCell();
            }

            /// <summary>
            /// Сохранение значений отображения в табилцу
            /// </summary>
            /// <returns>таблица с данными</returns>
            public DataTable GetValue(DataTable dtSourceOrg, int idSession)
            {
                int i = 0,
                    idAlg = -1,
                     vsRatioValue = -1
                     , quality = -1;
                double valueToRes = 0;
                DateTime dtVal;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.m_iIdComp > 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (row.Cells[col.Index].Value != null)
                                    if (row.Cells[col.Index].Value.ToString() != "")
                                    {
                                        idAlg = (int)row.Cells["ALG"].Value;//??
                                        valueToRes = Convert.ToDouble(row.Cells[col.Index].Value.ToString().Replace('.', ','));
                                        vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                                        valueToRes *= Math.Pow(10F, vsRatioValue);
                                        dtVal = Convert.ToDateTime(row.Cells["Date"].Value.ToString());

                                        quality = diffRowsInTables(dtSourceOrg, valueToRes, i);

                                        dtSourceEdit.Rows.Add(new object[] 
                                    {
                                        col.m_iIdComp
                                        , idSession
                                        , quality
                                        , valueToRes                 
                                        , dtVal.AddMinutes(-m_currentOffSet).ToString("F",dtSourceEdit.Locale)
                                        , i
                                    });
                                        i++;
                                    }
                        }
                }

                dtSourceEdit = sortingTable(dtSourceEdit, "WR_DATETIME, ID_PUT");

                return dtSourceEdit;
            }

            /// <summary>
            /// Форматирование значений
            /// </summary>
            private void formatCell()
            {
                int idAlg = -1
                     , vsRatioValue = -1,
                     iCol = 0;
                double dblVal = 1F;

                foreach (HDataGridViewColumn column in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (double.TryParse(row.Cells[iCol].Value.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out dblVal))
                                {
                                    //dblVal = double.Parse(row.Cells[iCol].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    idAlg = (int)row.Cells["ALG"].Value;
                                    vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                    row.Cells[iCol].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                                System.Globalization.CultureInfo.InvariantCulture);
                                }
                        }
                    iCol++;
                }
            }

            /// <summary>
            /// соритровка таблицы по столбцу
            /// </summary>
            /// <param name="table">таблица для сортировки</param>
            /// <param name="sortStr">имя столбца/ов для сортировки</param>
            /// <returns></returns>
            private DataTable sortingTable(DataTable table, string colSort)
            {
                DataView dView = table.DefaultView;
                string sortExpression = string.Format(colSort);
                dView.Sort = sortExpression;
                table = dView.ToTable();

                return table;
            }

            /// <summary>
            /// Проверка на изменение значения
            /// </summary>
            /// <param name="origin">оригинальная таблица</param>
            /// <param name="editValue">значение</param>
            /// <param name="i">номер строки</param>
            /// <returns>показатель изменения</returns>
            private int diffRowsInTables(DataTable origin, double editValue, int i)
            {
                int quality = 0;

                origin = sortingTable(origin, "WR_DATETIME, ID_PUT");

                if (origin.Rows[i]["Value"].ToString() != editValue.ToString())
                    quality = 1;

                return quality;
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            private Excel.Application m_excApp;
            private Excel.Workbook m_workBook;
            private Excel.Worksheet m_wrkSheet;
            private object _missingObj = System.Reflection.Missing.Value;

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
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Reaktivka");
                    int indxCol = 1;

                    try
                    {
                        for (int i = 0; i < dgView.Columns.Count; i++)
                        {
                            if (dgView.Columns[i].HeaderText != "")
                            {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxCol];

                                foreach (Excel.Range cell in colRange.Cells)
                                    if (Convert.ToString(cell.Value) != "")
                                        if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                                        {
                                            fillSheetExcel(colRange, dgView, i, cell.Row);
                                            break;
                                        }
                                indxCol++;
                            }
                        }
                        //
                        setSignature(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        closeExcel();
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);
                    }
                    catch (Exception e)
                    {
                        closeExcel();
                        MessageBox.Show("Ошибка экспорта данных!");
                    }
                }
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateReaktivka.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
                    closeExcel();
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
                closeExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                closeExcel();
            }

            /// <summary>
            /// Добавление подписи месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setSignature(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                Excel.Range exclTEC = exclWrksht.get_Range("B2");
                Excel.Range exclRMonth = exclWrksht.get_Range("A2");
                //exclRPL.Value2 = dgv.Rows[dgv.Rows.Count - 1].Cells[@"PlanSwen"].Value;
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
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        row = i;
                        break;
                    }

                for (int j = 0; j < dgv.Rows.Count; j++)
                    if (dgv.Rows.Count - 1 != j)
                    {
                        colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                        row++;
                    }
                    else
                    {
                        deleteNullRow(colRange, row);

                        if (Convert.ToString(((Excel.Range)colRange.Cells[row]).Value) == "")
                            colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                    }

            }

            /// <summary>
            /// Удаление пустой строки
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="row">номер строки</param>
            private void deleteNullRow(Excel.Range colRange, int row)
            {
                Excel.Range rangeCol = (Excel.Range)m_wrkSheet.Columns[1];

                while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "")
                {
                    Excel.Range rangeRow = (Excel.Range)m_wrkSheet.Rows[row];
                    rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            private void closeExcel()
            {
                //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                m_excApp = null;
                m_workBook = null;
                m_wrkSheet = null;
                System.GC.Collect();
            }
        }

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_ItemCheck(PanelManagementReaktivka.ItemCheckedParametersEventArgs ev)
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
            //Изменить структуру 'DataGridView'          
            (m_dgvReak as DGVReaktivka).UpdateStructure(ev);
        }

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
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            if (rangeCheking(dtrGet))
                MessageBox.Show("Выбранный диапазон месяцев неверен");
            else
            {
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
                            m_dgvReak.ShowValues(m_TableOrigin);
                            //
                            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = valuesFence;
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
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
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
                    HandlerDb.CreateSession(
                        CountBasePeriod
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
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
             m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
        }

        /// <summary>
        /// формирование таблицы данных
        /// </summary>
        private DataTable valuesFence
        {
            get
            { //сохранить вх. знач. в DataTable
                return m_dgvReak.GetValue(m_TableOrigin, (int)Session.m_Id);
            }
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

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Сохранение входных знчений
        /// </summary>
        /// <param name="err">номер ошибки</param>
        private void saveInvalValue(out int err)
        {
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeValuesVar();

            sortingDataToTable(m_TableOrigin
                , m_TableEdit
                , getNameTableIn(dtrPer[0].Begin)
                , @"ID"
                , out err);
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable origin, DataTable edit, string unCol, out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME"
                    , unCol
                    , origin
                    , edit
                    , out err);
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
            string newNameTable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + newNameTable;
        }

        /// <summary>
        /// разбор данных по разным табилца(взависимости от месяца)
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
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

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);

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

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <returns></returns>
        private DataTable diffRowsInTables(DataTable origin, DataTable edit)
        {
            for (int i = 0; i < origin.Rows.Count; i++)
                for (int j = 0; j < edit.Rows.Count; j++)
                    if (origin.Rows[i]["Value"].Equals(edit.Rows[j]["Value"]))
                        edit.Rows.RemoveAt(j);

            return edit;
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
        /// Поиск ид панели
        /// </summary>
        /// <returns>key</returns>
        private int findMyIDTab()
        {
            int Res = 0;
            Dictionary<int, Type> dictRegId = (_iFuncPlugin as PlugInBase).GetRegisterTypes();

            foreach (var item in dictRegId)
                if (item.Value == this.GetType())
                    Res = item.Key;

            return Res;
        }
    }
}
