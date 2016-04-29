using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;
using System.Drawing;
using System.Data;

namespace PluginTaskBalTeplo
{
    public class PanelTaskBalTeplo : HPanelTepCommon
    {
        DataTable[] m_arr_InVal;
        DataTable[] m_arr_OutVal;
        DataTable m_calculate_outval;

        enum INDEX_TABLE { Origin, Edit };

        PanelTEP PanelTep_Block;
        PanelTEP PanelTep_Vivod;
        PanelTEP PanelTep_Teplo;
        //PanelManagementAutobook PanelManagement;

        public enum ViewCompMode { Block, Output };

        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_CONTEXT
        {
            ID_CON = 10
        }

        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_DATA,
            DGV_PLANEYAR
                , LABEL_DESC
        }

        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;

        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD // идентификаторы периодов расчетов, использующихся на форме
                ,
            TIMEZONE // идентификаторы (целочисленные, из БД системы) часовых поясов
                //    , ALL_COMPONENT,
                //ALL_NALG // все идентификаторы компонентов ТЭЦ/параметров
                //    , DENY_COMP_CALCULATED,
                //DENY_PARAMETER_CALCULATED // запрещенных для расчета
                //    , DENY_COMP_VISIBLED,
                //DENY_PARAMETER_VISIBLED // запрещенных для отображения
                , COUNT
        }

        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            , PERIOD, TIMEZONE, COMPONENT,
            PARAMETER //_IN, PARAMETER_OUT
                , MODE_DEV/*, MEASURE*/,
            RATIO
                , COUNT
        }

        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;

        /// <summary>
        /// 
        /// </summary>
        protected TaskBalTeploCalculate BalTeploCalc;


        private void InitializeComponent()
        {
            SuspendLayout();

            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvTEPValues = 4;

            posRow = 0;
            //PanelManagement = new PanelManagementAutobook();
            PanelTep_Block = new PanelTEP();
            PanelTep_Vivod = new PanelTEP();
            PanelTep_Teplo = new PanelTEP();
            PanelManagement.Dock = DockStyle.Fill;
            PanelTep_Block.Dock = DockStyle.Fill;
            PanelTep_Vivod.Dock = DockStyle.Fill;
            PanelTep_Teplo.Dock = DockStyle.Fill;

            PanelTep_Block.Visible = true;
            PanelTep_Vivod.Visible = false;
            //
            this.Controls.Add(PanelManagement, 0, posRow);
            this.SetColumnSpan(PanelManagement, posColdgvTEPValues);
            this.SetRowSpan(PanelManagement, 10);//this.RowCount); 
            //
            this.Controls.Add(PanelTep_Block, posColdgvTEPValues, 0);
            this.SetColumnSpan(PanelTep_Block, 9);
            this.SetRowSpan(PanelTep_Block, 7);//this.RowCount);
            //
            this.Controls.Add(PanelTep_Vivod, 4, 0);
            this.SetColumnSpan(PanelTep_Vivod, 9);
            this.SetRowSpan(PanelTep_Vivod, 7);//this.RowCount);
            //
            this.Controls.Add(PanelTep_Teplo, posColdgvTEPValues, 7);
            this.SetColumnSpan(PanelTep_Teplo, 9);
            this.SetRowSpan(PanelTep_Teplo, 3);//this.RowCount);

            ResumeLayout(false);
            PerformLayout();

            PanelManagement.RadioButtonModeChanged += new PanelManagementAutobook.RadioButtonMode_Event_Handler(PanelManagement_ViewComponentMode_SelectionChanged);
            
            Button btn = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskAutobookMonthValue_btnsend_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskAutobookMonthValues_btnexport_Click);
        }


        #region AutoBook
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
                            24
                            ;

                return iRes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;

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
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected INDEX_VIEW_VALUES m_ViewValues;

        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }

        protected override HandlerDbValues createHandlerDb()
        {
            return new TaskBalTeploCalculate();
        }

        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }

        /// <summary>
        /// 
        /// </summary>
        protected TaskBalTeploCalculate HandlerDb { get { return m_handlerDb as TaskBalTeploCalculate; } }

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
        protected PanelManagementAutobook PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();
                else
                    ;

                return _panelManagement;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected void clear(int iCtrl = (int)INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;

            deleteSession();
            //??? повторная проверка
            if (bClose == true)
            {
                if (!(m_arTableDictPrjs == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                    {
                        if (!(m_arTableDictPrjs[i] == null))
                        {
                            m_arTableDictPrjs[i].Clear();
                            m_arTableDictPrjs[i] = null;
                        }
                        else
                            ;
                    }
                else
                    ;

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                //dgvAB.ClearRows();
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                //dgvAB.ClearValues()
                ;
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
                , HandlerDb.GetQueryCompList()
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
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagement as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);

            // очистить содержание представления
            clear();
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
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
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                    else
                        ;
            else
                throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
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
            Session.SetRangeDatetime(dtBegin, dtEnd);
            //заполнение представления
            fillDaysGrid(dtBegin, dtBegin.Month);
        }

        /// <summary>
        /// заполнение грида датами
        /// </summary>
        /// <param name="date">тек.дата</param>
        /// <param name="numMonth">номер месяца</param>
        private void fillDaysGrid(DateTime date, int numMonth)
        {
            //DateTime dt = new DateTime(date.Year, date.Month, 1);
            //dgvAB.ClearRows();

            //for (int i = 0; i < DayIsMonth; i++)
            //{
            //    dgvAB.AddRow();
            //    dgvAB.Rows[i].Cells[0].Value = dt.AddDays(i).ToShortDateString();
            //}
            //dgvAB.Rows[date.Day - 1].Selected = true;

        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <param name="numMonth">номер месяца</param>
        /// <returns>кол-во дней</returns>
        public int DayIsMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValues_btnexport_Click(object sender, EventArgs e)
        {
            //rptExcel.CreateExcel(dgvAB);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValue_btnsend_Click(object sender, EventArgs e)
        {
            int err = -1;
            string toSend = (Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0] as TextBox).Text;

            //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            //    dgvAB.FillTableValueDay(HandlerDb.OutValues(out err), dgvAB, HandlerDb.getOutPut(out err));
            //rptsNSS.SendMailToNSS(m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            //, HandlerDb.GetDateTimeRangeValuesVar(), toSend);
        }

        /// <summary>
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            //m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = getStructurOutval(out err);
            //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            //HandlerDb.saveResOut(m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            //, m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

            //base.HPanelTepCommon_btnSave_Click(obj, ev);

            //saveInvalValue(out err);
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            //m_ViewValues = INDEX_VIEW_VALUES.SOURCE;

            onButtonLoadClick();

        }

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelAutobook_btnHistory_Click(object obj, EventArgs ev)
        {
            //m_ViewValues = INDEX_VIEW_VALUES.ARCHIVE;

            //onButtonLoadClick();
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
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1
                , iRegDbConn = -1;
            string errMsg = string.Empty;

            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg);

                //if (err == 0)
                //{
                //    if (m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0)
                //    {
                //        // создать копии для возможности сохранения изменений
                //        setValues();
                //        //вычисление значений
                //        AutoBookCalc.getTable(m_arTableOrigin, HandlerDb.getOutPut(out err));
                //        //
                //        m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                //            AutoBookCalc.calcTable[(int)INDEX_GTP.TEC].Copy();
                //        //запись выходных значений во временную таблицу
                //        HandlerDb.insertOutValues(out err, AutoBookCalc.calcTable[(int)INDEX_GTP.TEC]);
                //        // отобразить значения
                //        dgvAB.ShowValues(m_arTableOrigin
                //            , dgvAB, HandlerDb.getPlanOnMonth(
                //            Type
                //            , HandlerDb.GetDateTimeRangeValuesVar()
                //            , ActualIdPeriod
                //            , out err));
                //        //сохранить вых. знач. в DataTable
                //        m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                //            dgvAB.FillTableValueDay(HandlerDb.OutValues(out err)
                //               , dgvAB
                //               , HandlerDb.getOutPut(out err));
                //        //сохранить вых.корр. знач. в DataTable
                //        m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                //            dgvAB.FillTableCorValue(HandlerDb.OutValues(out err), dgvAB);
                //    }
                //    else ;
                //}
                //else
                //{
                //    // в случае ошибки "обнулить" идентификатор сессии
                //    deleteSession();
                //    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                //}
                ////удалить сессию
                ////deleteSession();
            }
            else
                ;

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
            else
                ;
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr"></param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            //изменение начальной даты
            if (arQueryRanges.Count() > 1)
                arQueryRanges[1] = new DateTimeRange(arQueryRanges[1].Begin.AddDays(-(arQueryRanges[1].Begin.Day - 1))
                    , arQueryRanges[1].End.AddDays(-(arQueryRanges[1].End.Day - 2)));
            else
                arQueryRanges[0] = new DateTimeRange(arQueryRanges[0].Begin.AddDays(-(arQueryRanges[0].Begin.Day - 1))
                    , arQueryRanges[0].End.AddDays(DayIsMonth - arQueryRanges[0].End.Day));
            //Запрос для получения архивных данных
            //m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            ////Запрос для получения автоматически собираемых данных
            //m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
            //    (
            //    Type
            //    , ActualIdPeriod
            //    , CountBasePeriod
            //    , arQueryRanges
            //   , out err
            //    );
            ////Получение значений корр. input
            //m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            //    HandlerDb.getCorInPut(Type
            //    , arQueryRanges
            //    , ActualIdPeriod
            //    , out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                {
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    //HandlerDb.CreateSession(
                    //    CountBasePeriod
                    //    , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
                    //    , ref m_arTableOrigin
                    //    , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                    //    , out err, out strErr);
                }
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
            //    m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
            //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            //    = m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
        }

        /// <summary>
        /// получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(out int err)
        {
            string strRes = string.Empty;

            strRes = "SELECT * FROM "
                + GetNameTableOut((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value);

            return HandlerDb.Select(strRes, out err);
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableOut(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        #endregion


        public PanelTaskBalTeplo(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            BalTeploCalc = new TaskBalTeploCalculate();

            InitializeComponent();

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));

            addLabelDesc("panelDesc", 4, 10);

            //getInval();
        }

        private void getInval()
        {
            int err = -1;
            //m_arr_InVal[(int)INDEX_TABLE.Origin] = HandlerDb.GetValuesVar(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, ID_PERIOD.DAY, 1, HandlerDb.GetDateTimeRangeValuesVar(), out err);

        }

        private void calculateOutVal()
        {

        }

        private void getOutVal()
        {
            int err = -1;
            m_arr_OutVal[(int)INDEX_TABLE.Origin] = m_handlerDb.Select("", out err);
        }

        private void fillDGV()
        {
        }

        #region Перегруженные
        /// <summary>
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];
            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            string strItem = string.Empty;
            int i = -1;
            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj = getQueryDictPrj();
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrjs[i] = m_handlerDb.Select(arQueryDictPrj[i], out err);

                if (!(err == 0))
                    break;
                else
                    ;
            }
            ////Назначить обработчик события - изменение дата/время начала периода
            //hdtpBegin.ValueChanged += new EventHandler(hdtpBegin_onValueChanged);
            //Назначить обработчик события - изменение дата/время окончания периода
            // при этом отменить обработку события - изменение дата/время начала периода
            // т.к. при изменении дата/время начала периода изменяется и дата/время окончания периода
            // (Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);

            if (err == 0)
            {
                try
                {
                    //initialize();
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 1; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][1]);//??
                    (PanelManagement as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
                    (ctrl as ComboBox).Enabled = false;

                    ctrl = Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0];
                    DataTable tb = HandlerDb.GetProfilesContext(m_id_panel);
                    //из profiles
                    for (int j = 0; j < tb.Rows.Count; j++)
                        if (Convert.ToInt32(tb.Rows[j]["ID_CONTEXT"]) == (int)INDEX_CONTEXT.ID_CON)
                            ctrl.Text = tb.Rows[j]["VALUE"].ToString().TrimEnd();
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

        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }
        #endregion

        protected void PanelManagement_ViewComponentMode_SelectionChanged(object sender, PanelManagementAutobook.RadioButtonMode_Event_Args e)
        {
            if (e.mode == ViewCompMode.Block)
            {
                PanelTep_Vivod.Visible = false;
                PanelTep_Block.Visible = true;
            }
            else
                if (e.mode == ViewCompMode.Output)
                {
                    PanelTep_Block.Visible = false;
                    PanelTep_Vivod.Visible = true;
                }
        }

        protected class PanelTEP : TableLayoutPanel
        {
            public PanelTEP()
                : base()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                this.RowCount = 1;
                this.ColumnCount = 1;

                DateGridView_TEP dgv = new DateGridView_TEP();
                dgv.Dock = DockStyle.Fill;
                this.Controls.Add(dgv, 0, 0);
            }

            public void FillDGV(DataTable dt)
            {
                
            }

            protected class DateGridView_TEP : DataGridView
            {
                public DateGridView_TEP()
                    : base()
                {
                    InitializeComponent();
                }

                private void InitializeComponent()
                {

                }
            }
        
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementAutobook : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                    , BUTTON_SEND, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT
                    ,
                TXTBX_EMAIL
                    , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN,
                HDTP_END
                                , MENUITEM_UPDATE,
                MENUITEM_HISTORY
                    , RB_PANEL
                ,COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            public PanelManagementAutobook()
                : base(6, 21)
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

                //*****************//
                this.Controls.Add(tlp, 0, posRow);
                this.SetColumnSpan(tlp, 4); this.SetRowSpan(tlp, 3);
                //*****************//

                //
                TableLayoutPanel tlpValue = new TableLayoutPanel();
                //tlpValue.ColumnStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                
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
                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(1)
                    , tlpValue.Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);

                //*****************//
                this.Controls.Add(tlpValue, 0, posRow = posRow + 3);
                this.SetColumnSpan(tlpValue, 4); this.SetRowSpan(tlpValue, 7);
                //*****************//

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
                //Кнопка - импортировать
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL_BASE.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                //ctrlBSend.Enabled = false;
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
                //Поле с почтой
                TextBox ctrlTxt = new TextBox();
                ctrlTxt.Name = INDEX_CONTEXT.ID_CON.ToString();
                //ctrlTxt.Text = @"Pasternak_AS@sibeco.su";
                ctrlTxt.Dock = DockStyle.Top;

                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBSend, 1, 0);
                tlpButton.Controls.Add(ctrlBsave, 0, 1);
                tlpButton.Controls.Add(ctrlTxt, 1, 1);
                tlpButton.Controls.Add(ctrlExp, 0, 2);

                //*****************//
                this.Controls.Add(tlpButton, 0, posRow = posRow + 7);
                this.SetColumnSpan(tlpButton, 4); this.SetRowSpan(tlpButton, 5);
                //*****************//

                PanelRadioButton RB_PANEL = new PanelRadioButton();
                RB_PANEL.Dock = DockStyle.Fill;
                RB_PANEL.AutoSize = true;
                RB_PANEL.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                RB_PANEL.RadioButtonModeChanged += new PanelRadioButton.RadioButtonMode_Event_Handler(RadioBtn_SelectionChanged);

                //*****************//
                this.Controls.Add(RB_PANEL, 0, posRow = posRow + 5);
                this.SetColumnSpan(RB_PANEL, 4); this.SetRowSpan(RB_PANEL, 6);
                //*****************//

                this.RowStyles.Clear();

                float val = (float)100 / this.RowCount;

                for (int i = 0; i < this.RowCount; i++)
                {
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, val));
                }

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
            /// Установка периода
            /// </summary>
            /// <param name="idPeriod"></param>
            public void SetPeriod(ID_PERIOD idPeriod)
            {
                HDateTimePicker hdtpBtimePer = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                , hdtpEndtimePer = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
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

            private void RadioBtn_SelectionChanged(object sender, PanelRadioButton.RadioButtonMode_Event_Args e)
            {
                if (RadioButtonModeChanged != null)
                {
                    RadioButtonModeChanged(sender, new RadioButtonMode_Event_Args(e.mode));
                }
            }

            public class PanelRadioButton : TableLayoutPanel
            {
                public ViewCompMode Set_ViewCompMode;

                enum INDEX_CTRL { rb_block, rb_output };
                public PanelRadioButton()
                    : base()
                {
                    InitializeComponent();
                }

                private void InitializeComponent()
                {
                    SuspendLayout();

                    this.ColumnCount = 1;
                    this.RowCount = 2;

                    float val = (float)100 / this.ColumnCount;
                    //Добавить стили "ширина" столлбцов
                    for (int s = 0; s < this.ColumnCount - 0; s++)
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, val));

                    val = (float)100 / this.RowCount;
                    //Добавить стили "высота" строк
                    for (int s = 0; s < this.RowCount - 0; s++)
                        this.RowStyles.Add(new RowStyle(SizeType.Percent, val));

                    Control ctrl = new Control();
                    ctrl = new RadioButton();
                    ctrl.Dock = DockStyle.Fill;
                    ctrl.Text = "Блоки";
                    ctrl.Name = INDEX_CTRL.rb_block.ToString();
                    ((RadioButton)ctrl).Checked = true;
                    ((RadioButton)ctrl).CheckedChanged += new EventHandler(rb_CheckedChanged);
                    this.Controls.Add(((RadioButton)ctrl), 0, 0);
                    
                    ctrl = new RadioButton();
                    ctrl.Dock = DockStyle.Fill;
                    ctrl.Text = "Вывода";
                    ctrl.Name = INDEX_CTRL.rb_output.ToString();
                    ((RadioButton)ctrl).CheckedChanged += new EventHandler(rb_CheckedChanged);
                    this.Controls.Add(((RadioButton)ctrl), 0, 1);

                    ResumeLayout(false);
                    PerformLayout();
                }

                private void rb_CheckedChanged(object sender, EventArgs e)
                {
                    if (((RadioButton)sender).Name == INDEX_CTRL.rb_block.ToString() && ((RadioButton)sender).Checked == true)
                    {
                        Set_ViewCompMode = ViewCompMode.Block;
                        if (RadioButtonModeChanged != null)
                        {
                            RadioButtonModeChanged(this, new RadioButtonMode_Event_Args(Set_ViewCompMode));
                        }
                    }
                    else
                        if (((RadioButton)sender).Name == INDEX_CTRL.rb_output.ToString() && ((RadioButton)sender).Checked == true)
                        {
                            Set_ViewCompMode = ViewCompMode.Output;
                            if (RadioButtonModeChanged != null)
                            {
                                RadioButtonModeChanged(this, new RadioButtonMode_Event_Args(Set_ViewCompMode));
                            }
                        }

                    
                }

                public class RadioButtonMode_Event_Args : EventArgs
                {
                    public ViewCompMode mode;

                    public RadioButtonMode_Event_Args(ViewCompMode Mode)
                        : base()
                    {
                        mode = Mode;
                    }
                }

                public delegate void RadioButtonMode_Event_Handler(object sender, RadioButtonMode_Event_Args e);

                public RadioButtonMode_Event_Handler RadioButtonModeChanged;
            }

            public class RadioButtonMode_Event_Args : EventArgs
            {
                public ViewCompMode mode;

                public RadioButtonMode_Event_Args(ViewCompMode Mode)
                    : base()
                {
                    mode = Mode;
                }
            }

            public delegate void RadioButtonMode_Event_Handler(object sender, RadioButtonMode_Event_Args e);

            public RadioButtonMode_Event_Handler RadioButtonModeChanged;
        }
        
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 19;
            register(19, typeof(PanelTaskBalTeplo), @"Задача", @"Баланс тепла");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

