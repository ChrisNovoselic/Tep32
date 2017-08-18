using HClassLibrary;
using InterfacePlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    public partial class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// ??? переменная с текущем отклоненеим от UTC
        /// </summary>
        private static int s_currentOffSet;
        /// <summary>
        /// Для обозначения выбора 1 или 6 блоков
        /// </summary>
        private static bool s_flagBl = true;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        private bool m_bflgClear = false;
        /// <summary>
        /// ??? экземпляр делегата(возврат PictureBox для активного представления)
        /// </summary>
        public static Func<PictureBox> s_delegateGetActivePictureBox;
        /// <summary>
        /// ??? экземпляр делегата(возврат Ид)
        /// </summary>
        public static Func<int> s_delegateGetIdActiveComponent;        
        /// <summary>
        /// Список представлений для каждого из компонентов станции 
        /// </summary>
        private List<DataGridViewVedomostBl> m_listDataGridViewVedomostBl;
        /// <summary>
        /// Список с группами заголовков отображения
        /// </summary>
        protected static List<List<string>> s_listGroupHeaders = new List<List<string>> {
            // группа №1
            new List<string> { /*[1]*/ "Острый пар", /*[2]*/"Горячий промперегрев", /*[3]*/"ХПП" }
            // группа №2
            , new List<string> { /*[4]*/"Питательная вода", /*[5]*/"Продувка", /*[6]*/"Конденсатор", /*[7]*/"Холодный воздух", /*[8]*/"Горячий воздух", /*[9]/"Кислород", /*[10]*/"VI отбор", /*[11]*/"VII отбор" }
            // группа №3
            , new List<string> { /*[12]*/"Уходящие газы", /*[13]*/"", /*[14]*/"", /*[15]*/"", /*[16]*/"РОУ", /*[17]*/"Сетевая вода", /*[18]*/"Выхлоп ЦНД" }
        };
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , LABEL_DESC, TBLP_HGRID, /*PICTURE_BOXDGV,*/ PANEL_PICTUREBOX
                , COUNT
        }
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Объект для обращения к БД
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskVedomostBlCalculate; } }
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementVedomostBl PanelManagement
        {
            get {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementVedomostBl;
            }
        }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementVedomostBl();
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
        protected ReportExcel m_rptExcel;
        ///// <summary>
        ///// ??? почему статик Экземпляр класса обрабокти данных
        ///// </summary>
        //private static VedomostBlCalculate s_VedCalculate;
        /// <summary>
        /// ???
        /// </summary>
        protected class PictureBoxVedomostBl : PictureBox
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="viewActive">активный грид</param>
            public PictureBoxVedomostBl()
            {
                InitializeComponents();
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            private void InitializeComponents()
            {                
            }

            public void AddControl(DataGridViewVedomostBl dgv)
            {
                int height = -1;

                Tag = dgv.IdComponent;

                height = (dgv.Rows.Count) * dgv.Rows[0].Height + DataGridViewVedomostBl.HEIGHT_HEADER;

                this.Size = new Size(dgv.Width - DataGridViewVedomostBl.PADDING_COLUMN, height);

                Controls.Add(dgv);
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskVedomostBl(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.VEDOM_BL;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDatetime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;
            
            m_listDataGridViewVedomostBl = new List<DataGridViewVedomostBl>();

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();            

            //???
            s_delegateGetActivePictureBox = new Func<PictureBox>(GetActivePictureBox);
            //s_getDGV = new Func<DataGridView>(GetDGVOfIdComp);
            s_delegateGetIdActiveComponent = new Func<int>(GetIdActiveComponent);
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
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

            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, 4); SetRowSpan(PanelManagement, 13);
            //контейнеры для DGV
            //PictureBox pictureBox = new PictureBox();
            //pictureBox.Name = INDEX_CONTROL.PICTURE_BOXDGV.ToString();
            //pictureBox.TabStop = false;
            //
            Panel panelPictureBox = new Panel();
            panelPictureBox.Name = INDEX_CONTROL.PANEL_PICTUREBOX.ToString();
            panelPictureBox.Dock = DockStyle.Fill;
            (panelPictureBox as Panel).AutoScroll = true;
            Controls.Add(panelPictureBox, 5, posRow); // 5
            SetColumnSpan(panelPictureBox, 9); SetRowSpan(panelPictureBox, 10); // 9, 10
            //
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnHistory_onClick);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += panelManagemenet_btnExportExcel_onClick;
            //PanelManagement.ItemCheck += new PanelManagementTaskCalculate.ItemCheckedParametersEventHandler(panelManagement_onItemCheck);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox).CheckedChanged += panelManagement_ModeEnable_onChanged;
        }

        /// <summary>
        /// Обработчик события - Кнопка экспорта даных в Excel
        /// </summary>
        /// <param name="sender">объект, вызвавщий событие</param>
        /// <param name="e">Аргумент события, описывающий состояние элемента</param>
        private void panelManagemenet_btnExportExcel_onClick(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();
            m_rptExcel.CreateExcel(ActiveDataGridView, Session.m_DatetimeRange);
        }

        //private class ItemCheckedVedomostBlParametersEventArgs : PanelManagementVedomostBl.ItemCheckedParametersEventArgs
        //{
        //    public ItemCheckedVedomostBlParametersEventArgs(int id, CheckState newCheckState)
        //        : base (id, TYPE.VISIBLE, newCheckState)
        //    {
        //    }

        //    public int IndexHeader { get { return m_idComp; } }

        //    new public bool IsNAlg { get { return false; } }

        //    new public bool IsComponent { get { return false; } }

        //    new public bool IsPut { get { return false; } }
        //}

        /// <summary>
        /// Обработчик события - изменение отображения кол-во групп заголовка
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev)
        {
            //??? где сохраняются изменения. только на элементе управления?
            ;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            ;
            //Изменить структуру 'HDataGRidVIew's'          
            bool bItemChecked = ev.NewCheckState == CheckState.Checked ? true :
                  ev.NewCheckState == CheckState.Unchecked ? false : false;
            DataGridViewVedomostBl dgv = ActiveDataGridView;

            if (ev.m_type == PanelManagementTaskCalculate.ItemCheckedParametersEventArgs.TYPE.VISIBLE) {
                //if (ev.IsComponent == true) {
                    dgv.SetHeaderVisibled(s_listGroupHeaders[ev.m_idComp], bItemChecked);
                    dgv.ResizeControls();
                //} else
                ////??? другие случаи
                //    ;
            } else
            //??? ENABLE
                ;
        }

        /// <summary>
        /// Обработчик события - Признак Корректировка_включена/корректировка_отключена 
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void panelManagement_ModeEnable_onChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Нахожджение активного DGV
        /// </summary>
        /// <returns>активная вьюха на панели</returns>
        private DataGridViewVedomostBl ActiveDataGridView
        {
            get {
                Control ctrlRes = new Control();

                foreach (PictureBoxVedomostBl item in findControl(INDEX_CONTROL.PANEL_PICTUREBOX.ToString()).Controls)
                    if (item.Visible == true) {
                        ctrlRes = item.Controls[0];

                        break;
                    } else
                        ;

                return ctrlRes as DataGridViewVedomostBl;
            }
        }

        /// <summary>
        /// Возвращает PictureBox по идентификатору активного представления (компонента)
        /// </summary>
        /// <param name="idComp">Идентификатор компонента, установленный также идентификатором и для представления</param>
        /// <returns>Объект активного PictureBox</returns>
        public PictureBox GetActivePictureBox()
        {
            int cnt = 0
                , outCnt = 0
                , idComp = -1;
            PictureBox cntrl = new PictureBox();

            idComp = ActiveDataGridView.IdComponent;

            foreach (PictureBoxVedomostBl item in findControl(INDEX_CONTROL.PANEL_PICTUREBOX.ToString()).Controls)
            {
                if (idComp == (int)item.Tag)
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
        /// Возвращает idComp
        /// </summary>
        /// <returns>индентификатор объекта</returns>
        public int GetIdActiveComponent()
        {
            return ActiveDataGridView.IdComponent;
        }

        /// <summary>
        /// Инициализация сетки данных
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeDataGridView(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            PictureBoxVedomostBl pictureBox;
            TimeSpan tsOffsetUTC = TimeSpan.MinValue;

            tsOffsetUTC = TimeSpan.FromDays(1) - Session.m_curOffsetUTC;

            //создание грида со значениями
            //for (int i = 0; i < m_listTECComponent.Count; i++)
            foreach (DataGridViewVedomostBl dgv in m_listDataGridViewVedomostBl)
            {
                //dgv.DatetimeStamp = new DataGridViewValues.DateTimeStamp() {
                //    Start = PanelManagement.DatetimeRange.Begin
                //    , Increment = TimeSpan.FromDays(1)
                //};
                dgv.AddColumns(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter.FindAll(put => { return put.IdComponent == dgv.IdComponent; }));
                dgv.AddRows(new DataGridViewValues.DateTimeStamp() {
                    Start = PanelManagement.DatetimeRange.Begin + tsOffsetUTC
                    , Increment = TimeSpan.FromDays(1)
                    , ModeDataDatetime = HandlerDb.ModeDataDatetime
                });
                dgv.ResizeControls();
                dgv.ConfigureColumns();

                pictureBox = new PictureBoxVedomostBl();
                pictureBox.AddControl(dgv);
                //??? панель одновременно содержит все picureBox-ы
                (findControl(INDEX_CONTROL.PANEL_PICTUREBOX.ToString()) as Panel).Controls.Add(pictureBox);

                //возможность_редактирвоания_значений
                try {
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox).Checked =
                        m_dictProfile.GetBooleanAttribute(ID_PERIOD.MONTH, PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE, HTepUsers.ID_ALLOWED.ENABLED_ITEM);

                    if ((findControl(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString()) as CheckBox).Checked == true)
                        for (int t = 0; t < dgv.RowCount; t++)
                            dgv.ReadOnlyColumns = false;
                    else
                        ;
                } catch (Exception e) {
                //???
                    Logging.Logg().Exception (e, string.Format(@"PanelVedomostBl::InitializeDataGridView () - ошибки проверки возможности редактирования ячеек..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        /// <summary>
        /// Инициализация объектов формы
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_TIMEZONE idProfileTimezone = ID_TIMEZONE.UNKNOWN;
            ID_PERIOD idProfilePeriod = ID_PERIOD.UNKNOWN;
            string strItem = string.Empty;
            Control ctrl = null;

            // ВАЖНО! Обязательно до инициализации таблиц проекта (сортировка призойдет при вызове этой функции).
            HandlerDb.ModeNAlgSorting = HandlerDbTaskCalculate.MODE_NALG_SORTING.NotSortable;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMWZONE, COMP, PARAMETER, RATIO
            initialize
            //m_markTableDictPrj = new HMark
                (new ID_DBTABLE[] { /*ID_DBTABLE.PERIOD
                    , */ID_DBTABLE.TIME, ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , IsInParameters == true ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , IsOutParameters == true ? ID_DBTABLE.OUT_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );

            HandlerDb.FilterDbTableTimezone = TepCommon.HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = TepCommon.HandlerDbTaskCalculate.DbTableTime.Month;
            HandlerDb.FilterDbTableCompList = HandlerDbTaskCalculate.DbTableCompList.Tg;

            //активность_кнопки_сохранения
            try {
                (findControl(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString()) as Button).Enabled =
                    m_dictProfile.GetBooleanAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL);
            } catch (Exception e) {
                // ???
                Logging.Logg().Exception(e, string.Format(@"PanelTaskVedomostBl::initialize () - BUTTON_SAVE.Enabled..."), Logging.INDEX_MESSAGE.NOT_SET);

                err = -2;
            }

            if (err == 0) {
                try {
                    //???
                    m_bflgClear = !m_bflgClear;

                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = ID_TIMEZONE.MSK;
                    PanelManagement.FillValueTimezone(HandlerDb.m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , idProfileTimezone);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)int.Parse(m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.PERIOD));
                    PanelManagement.FillValuePeriod(HandlerDb.m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , idProfilePeriod);

                    PanelManagement.AllowUserTimezoneChanged = false;
                    PanelManagement.AllowUserPeriodChanged = false;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), @"...", Logging.INDEX_MESSAGE.NOT_SET);
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

        //protected override void panelManagement_OnEventDetailChanged(object obj)
        //{
        //    base.panelManagement_OnEventDetailChanged(obj);
        //}
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение даты/времени, диапазона даты/времени)
        /// </summary>
        protected override void panelManagement_DatetimeRange_onChanged()
        {
            base.panelManagement_DatetimeRange_onChanged();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_TimezoneChanged()
        {
            base.panelManagement_TimezoneChanged();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            int err = -1;
            string errMsg = string.Empty;

            // удалить все представления за указанный ранее период
            m_listDataGridViewVedomostBl.Clear();

            base.panelManagement_Period_onChanged();
            //Закончилось перечисление компонентов, параметров алгоритма расчета...

            //??? Dgv's
            initializeDataGridView(out err, out errMsg);
            //Переключатели для выбора компонентов(эн./блоков, котлов)
            PanelManagement.AddComponent(HandlerDb.ListTECComponent, out err, out errMsg);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);

            m_listDataGridViewVedomostBl.ForEach(dgv => { dgv.AddNAlgParameter(obj); });
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);

            m_listDataGridViewVedomostBl.Add(new DataGridViewVedomostBl(obj, HandlerDb.GetValueAsRatio));
        }
        #endregion        

        protected override void handlerDbTaskCalculate_onEventCompleted(HandlerDbTaskCalculate.EVENT evt, TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            string msgToStatusStrip = string.Empty;

            switch (evt) {
                case HandlerDbTaskCalculate.EVENT.SET_VALUES:
                    msgToStatusStrip = string.Format(@"Получение значений из БД");
                    break;
                case HandlerDbTaskCalculate.EVENT.CALCULATE:
                    break;
                case HandlerDbTaskCalculate.EVENT.EDIT_VALUE:
                    break;
                case HandlerDbTaskCalculate.EVENT.SAVE_CHANGES:
                    break;
                default:
                    break;
            }

            dataAskedHostMessageToStatusStrip(res, msgToStatusStrip);

            if ((res == TepCommon.HandlerDbTaskCalculate.RESULT.Ok)
                || (res == TepCommon.HandlerDbTaskCalculate.RESULT.Warning))
                switch (evt) {
                    case HandlerDbTaskCalculate.EVENT.SET_VALUES: // отображать значения при отсутствии ошибок
                        ActiveDataGridView.ShowValues(HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , out err);
                        break;
                    case HandlerDbTaskCalculate.EVENT.CALCULATE:
                        break;
                    case HandlerDbTaskCalculate.EVENT.EDIT_VALUE:
                        break;
                    case HandlerDbTaskCalculate.EVENT.SAVE_CHANGES:
                        break;
                    default:
                        break;
                }
            else
                ;
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(HandlerDbTaskCalculate.CalculateProccessEventArgs ev)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// ??? проверка выборки блока(для 1 и 6)
        /// </summary>
        public bool WhichBlIsSelected
        {
            get { return s_flagBl; }

            set { s_flagBl = value; }
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            int err = -1;
            //DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVarArchive();

            //m_arTableOrigin[(int)Session.m_ViewValues] =
            //    HandlerDb.GetDataOutval(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, dtR, out err);
            ////HandlerDb.GetInVal(Type
            ////, dtR
            ////, ActualIdPeriod
            ////, out err);

            //m_arTableEdit[(int)Session.m_ViewValues] =
            //HandlerDb.SaveValues(m_arTableOrigin[(int)Session.m_ViewValues]
            //    , valuesFence()
            //    , (int)Session.CurrentIdTimezone
            //    , out err);

            //saveInvalValue(out err);
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        private void panelTepCommon_btnHistory_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
        }
    }

    /// <summary>
    /// Класс для взамодействия с основным приложением (вызывающая программа)
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



