using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;

namespace PluginTaskReaktivka
{
    public partial class PanelTaskReaktivka : HPanelTepCommon
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
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1,
            PERIOD, TIMEZONE,
            COMPONENT,
            //PARAMETER, 
            //, MODE_DEV/*, MEASURE*/,
            RATIO,
            COUNT
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
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected HandlerDbTaskCalculate.INDEX_TABLE_VALUES m_ViewValues;
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
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskReaktivka(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.REAKTIVKA;

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();
            Session.SetRangeDatetime(PanelManagementReaktivka.s_dtDefaultAU, PanelManagementReaktivka.s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// Конструктор
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

            Controls.Add(PanelManagementReak, 0, posRow);
            SetColumnSpan(PanelManagementReak, 4); SetRowSpan(PanelManagementReak, 9);

            Controls.Add(m_dgvReak, 5, posRow);
            SetColumnSpan(m_dgvReak, 9); SetRowSpan(m_dgvReak, 10);

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
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void m_dgvReak_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            m_dgvReak.SumValue(e.ColumnIndex, e.RowIndex);
            if(m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] != null)
            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = valuesFence;
        }

        /// <summary>
        /// Обработчик события изменения значения в ячейке
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void m_dgvReak_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {

        }

        /// <summary>
        /// Обработчик события - нажатие клавиши ЭКСПОРТ
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void PanelTaskReaktivka_ClickExport(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();//
            m_rptExcel.CreateExcel(m_dgvReak, Session.m_rangeDatetime);
        }

        /// <summary>
        /// инициализация параметров вкладки
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            Control ctrl = null;
            ID_PERIOD idPeriod = ID_PERIOD.UNKNOWN;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            int role = (int)HTepUsers.Role;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[] {
                        //INDEX_ID.DENY_COMP_CALCULATED,
                        INDEX_ID.DENY_COMP_VISIBLED
                    };
            bool[] arChecked = new bool[arIndxIdToAdd.Length];

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
                id_comp = (int)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
                strItem = ((string)r[@"DESCRIPTION"]).Trim();
                // установить признак отображения компонента станции
                arChecked[0] = m_arListIds[(int)INDEX_ID.DENY_COMP_VISIBLED].IndexOf(id_comp) < 0;
                (PanelManagementReak as PanelManagementReaktivka).AddComponent(id_comp
                    , strItem
                    , arIndxIdToAdd
                    , arChecked);

                if (m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows.Count + 2 > m_dgvReak.Columns.Count)
                    m_dgvReak.AddColumn(id_comp, strItem, strItem, true, arChecked[0]);
                else
                    ;
            }
            //возможность_редактирвоания_значений
            try
            {
                if ((m_dictProfile.Objects.ContainsKey(((int)ID_PERIOD.MONTH).ToString()) == true)
                    && (m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects.ContainsKey(((int)INDEX_CONTROL.DGV_DATA).ToString()) == true)
                    && (m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)INDEX_CONTROL.DGV_DATA).ToString()].Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true))
                {
                    (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked =
                        int.Parse(m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)INDEX_CONTROL.DGV_DATA).ToString()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == 1;
                }
                else
                    (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                if ((Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                    m_dgvReak.AddBRead(false);
                else
                    m_dgvReak.AddBRead(true);

                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                    (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled =
                        int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == 1;
                else
                    (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;

                //Установить обработчик события - добавить параметр
                //eventAddNAlgParameter += new DelegateObjectFunc((PanelManagement as PanelManagementTaskTepValues).OnAddParameter);
                // установить единый обработчик события - изменение состояния признака участие_в_расчете/видимость
                // компонента станции для элементов управления
                (PanelManagementReak as PanelManagementReaktivka).ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.DENY_COMP_VISIBLED });
                //
                m_dgvReak.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

                if (err == 0)
                {                
                    //m_bflgClear = !m_bflgClear;
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]);
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementReaktivka.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    idPeriod = (ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]);
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = m_arListIds[(int)INDEX_ID.PERIOD].IndexOf((int)idPeriod);
                    Session.SetCurrentPeriod(idPeriod);
                    (PanelManagementReak as PanelManagementReaktivka).SetPeriod(idPeriod);
                    (ctrl as ComboBox).Enabled = false;                
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
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelTaskReaktivka::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
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
        /// Зарегистрировать/отменить обработчик события 'DateTimeRangeValue_Changed' от составного календаря
        /// </summary>
        /// <param name="active">Признак регистрации/отмены обработчика</param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagementReak == null))
                if (active == true)
                    PanelManagementReak.DateTimeRangeValue_Changed += new PanelManagementReaktivka.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagementReak.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                    else
                        ;
            else
                throw new Exception(@"PanelTaskReaktivka::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
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

            Session.SetRangeDatetime(dtBegin, dtEnd);

            if (m_bflgClear == true)
            {
                clear();
                dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
                    , new int[] { m_id_panel, (int)Session.m_currIdPeriod }
                    , out err
                );

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
                            //, m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            , m_Value = dt.AddDays(i).ToShortDateString()
                            , m_vsRatio = ratio
                            , m_vsRound = round
                        });
                    else
                        m_dgvReak.AddRow(new DGVReaktivka.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            //, m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            , m_Value = "ИТОГО"
                            , m_vsRatio = ratio
                            , m_vsRound = round
                        }
                        , DaysInMonth);
                }
            } else
                ; //??? ничего очищать не надо, но и ничего не делать

            m_dgvReak.Rows[dtBegin.Day - 1].Selected = true;
            m_currentOffSet = Session.m_curOffsetUTC;
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
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
              m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;

            DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = 
                HandlerDb.GetDataOutval(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, dtR, out err);
            //HandlerDb.GetInVal(Type
            //, dtR
            //, ActualIdPeriod
            //, out err);

            m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                HandlerDb.SaveValues(m_TableOrigin, valuesFence, (int)Session.m_currIdTimezone, out err);

            saveInvalValue(out err);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION;

            onButtonLoadClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
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
                GC.Collect();
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
                else; //throw new Exception (@"");
            else
                if (ev.m_newCheckState == CheckState.Checked)
                if (!(m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0))
                    m_arListIds[(int)ev.m_indxIdDeny].Remove(idItem);
                else; //throw new Exception (@"");
            else;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //Изменить структуру 'DataGridView'          
            (m_dgvReak as DGVReaktivka).UpdateStructure(ev);
        }

        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
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

            clear();
            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (m_arTableOrigin[(int)m_ViewValues].Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();
                        // отобразить значения
                        m_dgvReak.ShowValues(m_arTableOrigin[(int)m_ViewValues]);
                        //
                        m_arTableEdit[(int)m_ViewValues] = valuesFence;
                    }
                    else {
                        deleteSession();
                        throw new Exception(@"PanelTaskreaktivka::updatedataValues() - " + errMsg);
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
                deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
        }

        /// <summary>
        /// Проверка выбранного диапазона
        /// </summary>
        /// <param name="dtRange">диапазон дат</param>
        /// <returns>флагп проверки</returns>
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
            if (m_ViewValues == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE)
                //Запрос для получения архивных данных
                m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = HandlerDb.GetDataOutvalArch(Type, HandlerDb.GetDateTimeRangeValuesVarArchive(), out err);
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
        /// формирование таблицы данных
        /// </summary>
        private DataTable valuesFence
        {
            get
            { //сохранить вх. знач. в DataTable
                return m_dgvReak.GetValue(m_TableOrigin, (int)Session.m_Id,m_ViewValues);
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

            strRes = HandlerDbValues.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

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
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            deleteSession();

            base.Stop();
        }
    }
}
