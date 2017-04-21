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
        protected HandlerDbTaskAutobookYarlyPlanCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskAutobookYarlyPlanCalculate; } }
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_VALUES = 2,
            LABEL_DESC, LABEL_YEARPLAN
        }
        /// <summary>
        /// Отображение значений в табличном представлении(план)
        /// </summary>
        protected DataGridViewAutobookYearlyPlan m_dgvValues;
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
                , BUTTON_LOAD, BUTTON_SAVE
                , DGV_VALUES
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
                //initializeLayoutStyle();
                Control ctrl = new Control();
                ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                                 //int posColdgvTEPValues = 6;
                SuspendLayout();

                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2);
                SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2);
                SetRowSpan(ctrl, 1);
                //Признак редактирование_разрешено/редактирование_запрещено 
                ctrl = new CheckBox();
                ctrl.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
                ctrl.Text = @"Редактирование разрешено";
                ctrl.Dock = DockStyle.Top;
                ctrl.Enabled = false;
                (ctrl as CheckBox).Checked = true;
                Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount);
                SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                throw new NotImplementedException();
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                // не требуется, т.к. отсутствуют необходимые элементы управления
            }
        }

        /// <summary>
        /// Класс панели - ИРЗ Учет активной электроэнергии - плановые значения (месяц-год)
        /// </summary>
        /// <param name="iFunc">Объект для взаимодействия с вызывающей программой</param>
        public PanelTaskAutobookYearlyPlan(IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDatetime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;

            InitializeComponent();

            m_dgvValues.EventCellValueChanged += dgvValues_onEventCellValueChanged;
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            m_dgvValues = new DataGridViewAutobookYearlyPlan(INDEX_CONTROL.DGV_VALUES.ToString());

            foreach (DataGridViewColumn column in m_dgvValues.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            Control ctrl = new Control();
            ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"
            int posColdgvValues = 4
                , heightRowdgvValues = 10;

            SuspendLayout();

            Controls.Add(PanelManagement, 0, posRow = posRow);
            SetColumnSpan(PanelManagement, posColdgvValues);
            SetRowSpan(PanelManagement, RowCount);

            Controls.Add(m_dgvValues, posColdgvValues, posRow);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvValues);
            SetRowSpan(m_dgvValues, heightRowdgvValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (findControl(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_LOAD.ToString()) as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);            
            (findControl(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString()) as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);
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
            bool bEnabled = true;

            int role = HTepUsers.Role;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMIZONE, COMP, PARAMETER(OUT_VALUES), MEASURE, RATIO
            initialize(new ID_DBTABLE[] { ID_DBTABLE.TIMEZONE, ID_DBTABLE.TIME, ID_DBTABLE.IN_PARAMETER, ID_DBTABLE.COMP_LIST, ID_DBTABLE.RATIO }, out err, out errMsg);

            HandlerDb.FilterDbTableTimezone = TepCommon.HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = TepCommon.HandlerDbTaskCalculate.DbTableTime.Year;
            HandlerDb.FilterDbTableCompList = TepCommon.HandlerDbTaskCalculate.DbTableCompList.Tec;

            m_dgvValues.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);

            try {
                if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.YEAR, INDEX_CONTROL.DGV_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == true)
                    bEnabled =
                        (MODE_CORRECT)Enum.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.YEAR, INDEX_CONTROL.DGV_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == MODE_CORRECT.ENABLE;
                else
                    bEnabled = false;

                (findControl(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.CHKBX_EDIT.ToString()) as CheckBox).Checked =
                m_dgvValues.ReadOnly =
                    bEnabled;

                bEnabled = true; // значение по умолчанию для кнопки "Сохранить"
                if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == true)
                    bEnabled =
                        (MODE_CORRECT)Enum.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == MODE_CORRECT.ENABLE;
                else
                    bEnabled = false;

                (findControl(PanelManagementAutobookYearlyPlan.INDEX_CONTROL.BUTTON_SAVE.ToString()) as Button).Enabled =
                    bEnabled;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutoBookYarlyPlan::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (err == 0) {
                try {
                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = (ID_TIMEZONE)Enum.Parse(typeof(ID_TIMEZONE), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.TIMEZONE));
                    PanelManagement.FillValueTimezone (m_dictTableDictPrj[ID_DBTABLE.TIMEZONE], idProfileTimezone);
                        //, (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(string.Format(@"ID={0}", (int)idProfileTimezone))[0][@"OFFSET_UTC"]);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)Enum.Parse(typeof(ID_PERIOD), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.PERIOD));
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME], idProfilePeriod);

                    PanelManagement.AllowUserPeriodChanged = false;
                    PanelManagement.AllowUserTimezoneChanged = false;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            } else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        #region Обработка измнения значений основных элементов управления на панели управления 'PanelManagement'
        /// <summary>
        /// Обработчик события при изменении значения
        ///  одного из основных элементов управления на панели управления 'PanelManagement'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            base.panelManagement_EventIndexControlBase_onValueChanged(obj);

            if (obj is Enum)
                ; // switch ()
            else
                ;
        }

        private void addValueRows()
        {
            TimeSpan tsOffsetUTC = /*TimeSpan.FromDays(1)*/ - Session.m_curOffsetUTC;

            m_dgvValues.AddRows(new DataGridViewValues.DateTimeStamp() {
                Start = PanelManagement.DatetimeRange.Begin + tsOffsetUTC
                , Increment = TimeSpan.MaxValue // значение неизвестно (в каждом месяце - разное кол-во суток), но является признаком для определения рассчитываемого значения
                , ModeDataDatetime = HandlerDb.ModeDataDatetime
            });
        }
        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(HPanelTepCommon.PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение даты/времени, диапазона даты/времени)
        /// </summary>
        protected override void panelManagement_DatetimeRange_onChanged()
        {
            base.panelManagement_DatetimeRange_onChanged();

            addValueRows();
        }

        private void buildStructureValues()
        {
            m_dgvValues.AddColumns(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter);

            addValueRows();
            
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_TimezoneChanged()
        {
            base.panelManagement_TimezoneChanged();

            buildStructureValues();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            base.panelManagement_Period_onChanged();

            buildStructureValues();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);

            m_dgvValues.AddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);

            m_dgvValues.AddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(TepCommon.HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);
        }
        #endregion

        ///// <summary>
        ///// загрузка/обновление данных
        ///// </summary>
        //private void updateDataValues()
        //{
        //    int err = -1
        //        , cnt = Session.CountBasePeriod
        //        , iRegDbConn = -1;
        //    string errMsg = string.Empty;
        //    DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

        //    m_handlerDb.RegisterDbConnection(out iRegDbConn);
        //    clear();

        //    if (!(iRegDbConn < 0))
        //    {
        //        // установить значения в таблицах для расчета, создать новую сессию
        //        setValues(dtrGet, out err, out errMsg);

        //        if (err == 0)
        //        {
        //            if (m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows.Count > 0)
        //            {
        //                // создать копии для возможности сохранения изменений
        //                setValues();

        //                m_dgvValues.ShowValues(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]);
        //            }
        //        }
        //        else
        //        {
        //            // в случае ошибки "обнулить" идентификатор сессии
        //            deleteSession();
        //            throw new Exception(@"PanelTaskAutobookYearlyPlan::updatedataValues() - " + errMsg);
        //        }
        //    }
        //    else
        //        //удалить сессию
        //        deleteSession();

        //    if (!(iRegDbConn > 0))
        //        m_handlerDb.UnRegisterDbConnection();
        //}

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            dataAskedHostMessageToStatusStrip(res, string.Format(@"Получение значений из БД"));

            if ((res == TepCommon.HandlerDbTaskCalculate.RESULT.Ok)
                || (res == TepCommon.HandlerDbTaskCalculate.RESULT.Warning))
            // отображать значения при отсутствии ошибок
                m_dgvValues.ShowValues(((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
                        ? HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            : new List<TepCommon.HandlerDbTaskCalculate.VALUE>()
                    , ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                        ? HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            : new List<TepCommon.HandlerDbTaskCalculate.VALUE>()
                    , out err);
            else
                ;
        }

        protected override void handlerDbTaskCalculate_onCalculateCompleted(TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(object obj)
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Проверка выбранного диапазона
        ///// </summary>
        ///// <param name="dtRange">диапазон дат</param>
        ///// <returns></returns>
        //private bool rangeCheking(DateTimeRange[] dtRange)
        //{
        //    bool bflag = false;

        //    for (int i = 0; i < dtRange.Length; i++)
        //        if (dtRange[i].End.Month > DateTime.Now.Month)
        //            if (dtRange[i].End.Year >= DateTime.Now.Year)
        //                bflag = true;

        //    return bflag;
        //}

        ///// <summary>
        ///// получение значений
        ///// создание сессии
        ///// </summary>
        ///// <param name="arQueryRanges">массив временных отрезков</param>
        ///// <param name="err">номер ошибки</param>
        ///// <param name="strErr">текст ошибки</param>
        //private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        //{
        //    err = 0;
        //    strErr = string.Empty;
        //    //Создание сессии
        //    Session.New();
        //    //Запрос для получения архивных данных
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = new DataTable();
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //        (
        //        TaskCalculateType
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err
        //        );

        //    //Проверить признак выполнения запроса
        //    if (err == 0)
        //    {
        //        //Проверить признак выполнения запроса
        //        if (err == 0)
        //            //Начать новую сессию расчета
        //            // ,получить входные для расчета значения для возможности редактирования
        //            HandlerDb.CreateSession(m_Id
        //                , Session.CountBasePeriod
        //                , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
        //                , ref m_arTableOrigin
        //                , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
        //                , out err, out strErr);
        //        else
        //            strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
        //                + @" по " + Session.m_rangeDatetime.End.ToString();
        //    }
        //    else
        //        strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
        //            + @" по " + Session.m_rangeDatetime.End.ToString();
        //}

        ///// <summary>
        ///// copy
        ///// </summary>
        //private void setValues()
        //{
        //    //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //    //         m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
        //    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
        //        m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
        //}

        /// <summary>
        /// Очистить содержание представления
        /// </summary>
        /// <param name="bClose">Признак снятия с отображения(закрытия) вкладки</param>
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true) {
                // удалить все строки
                m_dgvValues.Clear();
            } else
                // очистить содержание представления
                m_dgvValues.ClearValues();

            base.clear(bClose);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки "Сохранить" - сохранение значений в БД
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие(кнопка)</param>
        /// <param name="ev">Аргумент события(пустой)</param>
        protected override void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            base.panelTepCommon_btnSave_onClick(obj, ev);
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
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
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
            HandlerDb.Stop();

            base.Stop();
        }
    }
}
