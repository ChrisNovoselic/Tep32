﻿using HClassLibrary;
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
        /// Лист с группами хидеров отображения
        /// </summary>
        protected static List<List<string>> s_listGroupHeaders = new List<List<string>> {
            // группа №1
            new List<string> { "Острый пар", "Горячий промперегрев", "ХПП" }
            // группа №2
            , new List<string> { "Питательная вода", "Продувка", "Конденсатор", "Холодный воздух", "Горячий воздух", "Кислород", "VI отбор", "VII отбор" }
            // группа №3
            , new List<string> { "Уходящие газы", "", "", "", "РОУ", "Сетевая вода", "Выхлоп ЦНД" }
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
        ///класс для обработки данных
        /// </summary>
        public class VedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public VedomostBlCalculate()
                : base()
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
            HandlerDb.ModeDataDateTime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;
            
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
                    Start = PanelManagement.DatetimeRange.Begin
                    ,
                    Increment = TimeSpan.FromDays(1)
                });
                dgv.ResizeControls();
                dgv.ConfigureColumns();

                pictureBox = new PictureBoxVedomostBl();
                pictureBox.AddControl(dgv);
                //??? панель одновременно содержит все picureBox-ы
                (findControl(INDEX_CONTROL.PANEL_PICTUREBOX.ToString()) as Panel).Controls.Add(pictureBox);

                //возможность_редактирвоания_значений
                try {
                    if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.MONTH, PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == true)
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox).Checked =
                            (MODE_CORRECT)Enum.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.MONTH, PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == MODE_CORRECT.ENABLE;
                    else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox).Checked = false;

                    if ((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox).Checked == true)
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
                if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == true)
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled =
                        (MODE_CORRECT)MODE_CORRECT.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == MODE_CORRECT.ENABLE;
                else
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
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

            m_listDataGridViewVedomostBl.Add(new DataGridViewVedomostBl(obj));
        }
        #endregion

        ///// <summary>
        ///// Получение визуальных настроек 
        ///// для отображения данных на форме
        ///// </summary>
        ///// <param name="idComp">идКомпонента</param>
        ///// <returns>словарь настроечных данных</returns>
        //private Dictionary<string, List<int>> getVisualSettingsOfIdComponent(int idComp)
        //{
        //    Dictionary<string, List<int>> dictSettRes = new Dictionary<string, List<int>>();

        //    int err = -1
        //     , id_alg = -1;
        //    Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
        //    List<int> ratio = new List<int>()
        //    , round = new List<int>();
        //    string n_alg = string.Empty;            

        //    dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
        //       , new int[] {
        //            m_Id
        //            , idComp }
        //       , out err);

        //    IEnumerable<DataRow> listParameter = ListParameter.Select(x => x).Where(x => (int)x["ID_COMP"] == idComp);

        //    foreach (DataRow r in listParameter) {
        //        id_alg = (int)r[@"ID_ALG"];
        //        n_alg = r[@"N_ALG"].ToString().Trim();
        //        //// не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
        //        //if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
        //        //    // добавить в список идентификатор параметра алгоритма расчета
        //        //    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);
        //        //else
        //        //    ;

        //        // получить значения для настройки визуального отображения
        //        if (dictVisualSettings.ContainsKey(n_alg) == true) {
        //        // установленные в проекте
        //            ratio.Add(dictVisualSettings[n_alg.Trim()].m_ratio);
        //            round.Add(dictVisualSettings[n_alg.Trim()].m_round);
        //        } else {
        //        // по умолчанию
        //            ratio.Add(HTepUsers.s_iRatioDefault);
        //            round.Add(HTepUsers.s_iRoundDefault);
        //        }
        //    }
        //    dictSettRes.Add("ratio", ratio);
        //    dictSettRes.Add("round", round);

        //    return dictSettRes;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="active"></param>
        //protected void activateDateTimeRangeValue_OnChanged(bool active)
        //{
        //    if (!(PanelManagement == null))
        //        if (active == true)
        //            PanelManagement.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
        //        else
        //            if (active == false)
        //                PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
        //            else
        //                throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        //}

        ///// <summary>
        ///// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        ///// </summary>
        ///// <param name="obj">Объект, инициировавший событие</param>
        ///// <param name="ev">Аргумент события</param>
        //private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        //{
        //    int //err = -1,
        //      id_alg = -1;
        //    DataGridViewVedomostBl dgv = ActiveDataGridView;
        //    string n_alg = string.Empty;
        //    DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);

        //    //settingDateRange();
        //    Session.SetDatetimeRange(dtBegin, dtEnd);

        //    if (m_bflgClear) {
        //        clear();

        //        if (dgv.Rows.Count != 0)
        //            dgv.ClearRows();
        //        else
        //            ;

        //        for (int i = 0; i < DaysInMonth + 1; i++)
        //            dgv.AddRow(dt.AddDays(i), !(i < DaysInMonth));
        //    } else
        //        ;

        //    dgv.Rows[dtBegin.Day - 1].Selected = true;
        //    s_currentOffSet = (int)Session.m_curOffsetUTC.TotalMinutes;
        //}

        ///// <summary>
        ///// Установка длительности периода 
        ///// </summary>
        //private void settingDateRange()
        //{
        //    int cntDays,
        //        today = 0;

        //    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //      (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

        //    PanelManagementVedomostBl.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

        //}

        ///// <summary>
        ///// загрузка/обновление данных
        ///// </summary>
        //private void updateDataValues()
        //{
        //    int err = -1
        //        , cnt = Session.CountBasePeriod
        //        , iRegDbConn = -1;
        //    string errMsg = string.Empty;
        //    DateTimeRange[] dtrGet;

        //    if (!WhichBlIsSelected)
        //        dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
        //    else
        //        dtrGet = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

        //    clear();
        //    m_handlerDb.RegisterDbConnection(out iRegDbConn);

        //    if (!(iRegDbConn < 0))
        //    {
        //        // установить значения в таблицах для расчета, создать новую сессию
        //        setValues(dtrGet, out err, out errMsg);

        //        if (err == 0)
        //        {
        //            if (m_TableOrigin.Rows.Count > 0)
        //            {
        //                // создать копии для возможности сохранения изменений
        //                setValues();
        //                // отобразить значения
        //                ActiveDataGridView.ShowValues(m_arTableOrigin[(int)Session.m_ViewValues], Session.m_ViewValues);
        //                //сохранить готовые значения в таблицу
        //                m_arTableEdit[(int)Session.m_ViewValues] = valuesFence();
        //            }
        //            else
        //                deleteSession();
        //        }
        //        else
        //        {
        //            // в случае ошибки "обнулить" идентификатор сессии
        //            deleteSession();
        //            throw new Exception(@"PanelTaskVedomostBl::updatedataValues() - " + errMsg);
        //        }
        //    }
        //    else
        //        deleteSession();

        //    if (!(iRegDbConn > 0))
        //        m_handlerDb.UnRegisterDbConnection();
        //}

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            // отобразить значения
            ActiveDataGridView.ShowValues(m_arTableOrigin[(int)Session.m_ViewValues], Session.m_ViewValues);
            ////сохранить готовые значения в таблицу
            //m_arTableEdit[(int)Session.m_ViewValues] = valuesFence();
        }

        protected override void handlerDbTaskCalculate_onCalculateCompleted(HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(object obj)
        {
            throw new NotImplementedException();
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

            __handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME, QUALITY"
                    , unCol
                    , origin
                    , edit
                    , out err);
        }

        ///// <summary>
        ///// получение значений
        ///// создание сессии
        ///// </summary>
        ///// <param name="arQueryRanges"></param>
        ///// <param name="err">номер ошибки</param>
        ///// <param name="strErr">текст ошибки</param>
        //private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        //{
        //    err = 0;
        //    strErr = string.Empty;
        //    //Создание сессии
        //    Session.New();
        //    if (Session.m_ViewValues == HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE)
        //        //Запрос для получения архивных данных
        //        m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutvalArch(TaskCalculateType, HandlerDb.GetDateTimeRangeValuesVarArchive(), out err);
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar(
        //        TaskCalculateType
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err);
        //    //Проверить признак выполнения запроса
        //    if (err == 0)
        //    {
        //        //Проверить признак выполнения запроса
        //        if (err == 0)
        //            //Начать новую сессию расчета
        //            //, получить входные для расчета значения для возможности редактирования
        //            HandlerDb.CreateSession(m_Id
        //                , Session.CountBasePeriod
        //                , m_dictTableDictPrj[ID_DBTABLE.COMP_LIST]
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
        //    m_arTableEdit[(int)Session.m_ViewValues] =
        //        m_arTableOrigin[(int)Session.m_ViewValues].Clone();
        //}

        ///// <summary>
        ///// формирование таблицы данных
        ///// </summary>
        //private DataTable valuesFence()
        //{ //сохранить вх. знач. в DataTable
        //    return ActiveDataGridView.FillTableToSave(m_TableOrigin, (int)Session.m_Id, Session.m_ViewValues);
        //}

        /// <summary>
        /// ??? проверка выборки блока(для 1 и 6)
        /// </summary>
        public bool WhichBlIsSelected
        {
            get { return s_flagBl; }

            set { s_flagBl = value; }
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
        //        , HandlerDb.GetQueryParameters(Type)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        //, HandlerDb.GetQueryModeDev()
        //        //// единицы измерения
        //        //, m_handlerDb.GetQueryMeasures()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio()
        //    };

        //    return arRes;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err">номер ошибки</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// удачное заврешение UpdateInsertDelete
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Copy();
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

        ///// <summary>
        ///// Сохранение входных знчений
        ///// </summary>
        ///// <param name="err">номер ошибки</param>
        //private void saveInvalValue(out int err)
        //{
        //    DateTimeRange[] dtrPer;

        //    if (!WhichBlIsSelected)
        //        dtrPer = HandlerDb.getDateTimeRangeVariableValues();
        //    else
        //        dtrPer = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

        //    sortingDataToTable(m_arTableOrigin[(int)Session.m_ViewValues]
        //        , m_arTableEdit[(int)Session.m_ViewValues]
        //        , HandlerDb.GetNameTableOut(dtrPer[0].Begin)
        //        , @"ID"
        //        , out err
        //    );
        //}

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



