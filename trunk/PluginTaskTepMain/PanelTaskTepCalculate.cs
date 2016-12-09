using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Drawing;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskTepMain
{
    public abstract partial class PanelTaskTepCalculate : HPanelTepCommon
    {
        /// <summary>
        /// Массив списков идентификаторов компонентов ТЭЦ/параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            , PERIOD, TIMEZONE, COMPONENT, PARAMETER //_IN, PARAMETER_OUT
            , MODE_DEV/*, MEASURE*/, RATIO
                , COUNT
        }
        ///// <summary>
        ///// Индекс используесмых на панели значений
        ///// </summary>
        //protected enum INDEX_USE_VALUES { UNKNOWN = -1, IN, OUT_NORM, OUT_MKT
        //    , COUNT }
        ///// <summary>
        ///// Составной признак, указывающий на индексы, используемых на панели значений
        ///// </summary>
        //HMark m_markUseValues;        
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return Session.m_LoadValues == TepCommon.HandlerDbTaskCalculate.SESSION.INDEX_LOAD_VALUES.SOURCE ? ID_PERIOD.HOUR : Session.m_currIdPeriod; } }        
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected abstract PanelManagementTaskTepCalculate createPanelManagement();
        
        private PanelManagementTaskTepCalculate _panelManagement;
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementTaskTepCalculate PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement ();
                else
                    ;

                return _panelManagement;
            }
        }

        /// <summary>
        /// Отображение значений в табличном представлении
        /// </summary>
        protected DataGridViewTEPCalculate m_dgvValues;
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
                    //_currIdPeriod == ID_PERIOD.HOUR ?
                    //    (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0 :
                    //_currIdPeriod == ID_PERIOD.DAY ?
                    //    (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalDays - 0 :
                    //    24
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

        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1
            , PERIOD // идентификаторы периодов расчетов, использующихся на форме
            , TIMEZONE // идентификаторы (целочисленные, из БД системы) часовых поясов
            , ALL_COMPONENT, ALL_NALG // все идентификаторы компонентов ТЭЦ/параметров
            , DENY_COMP_CALCULATED, DENY_PARAMETER_CALCULATED // запрещенных для расчета
            , DENY_COMP_VISIBLED, DENY_PARAMETER_VISIBLED // запрещенных для отображения
                , COUNT
        }
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="iFunc">Объект для связи с сервером (внешней, вызывающей программой)</param>
        /// <param name="strNameTableAlg">Строка - наименование таблицы с параметрами алгоритма расчета</param>
        /// <param name="strNameTablePut">Строка - наименование таблицы с параметрами, детализированных до принадлежности к компоненту станции (оборудования)</param>
        /// <param name="strNameTableValues">Строка - наименование таблицы со значениями</param>
        protected PanelTaskTepCalculate(IPlugIn iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc)
        {
            Type = type;

            HandlerDb.IdTask = ID_TASK.TEP;

            InitializeComponents();

            Session.m_IdFpanel = m_id_panel;
            Session.SetRangeDatetime(PanelManagementTaskTepCalculate.s_dtDefault, PanelManagementTaskTepCalculate.s_dtDefault.AddHours(1));
        }

        protected TepCommon.HandlerDbTaskCalculate HandlerDb { get { return m_handlerDb as TepCommon.HandlerDbTaskCalculate; } }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskTepCalculate();
        }

        private void InitializeComponents ()
        {
        }

        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];
            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR/*, (int)ID_PERIOD.SHIFTS*/, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
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

            if (err == 0)
            {
                try
                {
                    m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Clear();

                    initialize();

                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelTaskTepCalculate.PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);

                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 0; //??? требуется прочитать из [profile]

                    //// отобразить значения
                    //updateDataValues();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskTepValues::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
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
                    case INDEX_TABLE_DICTPRJ.MODE_DEV:
                        errMsg = @"Получение идентификаторов режимов работы оборудования";
                        break;
                    //case INDEX_TABLE_DICTPRJ.MEASURE:
                    //    errMsg = @"Получение информации по единицам измерения";
                    //    break;
                    default:
                        errMsg = @"Неизвестная ошибка";
                        break;
                }
        }

        protected abstract void initialize();
        /// <summary>
        /// Установить признак активности панель при выборе ее пользователем
        /// </summary>
        /// <param name="activate">Признак активности</param>
        /// <returns>Результат выполнения - был ли установлен признак</returns>
        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            if (IsFirstActivated == true)
                ;
            else
                ;

            return bRes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged (bool active)
        {
            if (! (PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementTaskTepCalculate.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                    else
                        ;
            else
                throw new Exception(@"PanelTaskTepCalculate::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }
        /// <summary>
        /// Массив запросов к БД по получению словарных и проектных значений
        /// </summary>
        private string[] getQueryDictPrj()
        {
            string[] arRes = null;

            arRes = new string[]
            {
                //PERIOD
                HandlerDb.GetQueryTimePeriods (m_strIdPeriods)
                //TIMEZONE
                , HandlerDb.GetQueryTimezones (m_strIdTimezones)
                // список компонентов
                , HandlerDb.GetQueryCompList ()
                // параметры расчета
                , HandlerDb.GetQueryParameters (Type)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                , HandlerDb.GetQueryModeDev ()
                //// единицы измерения
                //, m_handlerDb.GetQueryMeasures ()
                // коэффициенты для единиц измерения
                , HandlerDb.GetQueryRatio ()
            };

            return arRes;
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            PanelManagement.SetPeriod(Session.m_currIdPeriod);
            Session.SetRangeDatetime(PanelManagement.RangeDateTime);
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
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            //??? перед очисткой или после (не требуются ли предыдущий диапазон даты/времени)
            Session.SetRangeDatetime(dtBegin, dtEnd);

            // очистить содержание представления
            clear();

            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }
        /// <summary>
        /// Удалить сессию (+ очистить реквизиты сессии)
        /// </summary>
        protected virtual void deleteSession()
        {
            int err = -1;

            HandlerDb.DeleteSession(out err);
        }
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected virtual void clear(int iCtrl = (int)PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE indxCtrl = (PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE)iCtrl;

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

                cbx = Controls.Find(PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementTaskTepCalculate.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                m_dgvValues.ClearRows();
                m_dgvValues.ClearColumns();
            }
            else
            // очистить содержание представления
                m_dgvValues.ClearValues();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки "Результирующее действие - К макету"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected abstract void btnRunRes_onClick(object obj, EventArgs ev);

        ///// <summary>
        ///// Инициировать подготовку к расчету
        /////  , выполнить расчет
        /////  , актуализировать таблицы с временными значениями
        ///// </summary>
        ///// <param name="type">Тип требуемого расчета</param>
        //protected abstract void btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE type);

        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при галичии ошибки</param>
        protected abstract void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr);        
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected abstract class DataGridViewTEPCalculate : DataGridView
        {
            public DataGridViewTEPCalculate()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                this.Dock = DockStyle.Fill;

                MultiSelect = false;
                SelectionMode = DataGridViewSelectionMode.CellSelect;
                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                AllowUserToOrderColumns = false;
                AllowUserToResizeRows = false;
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders | DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            }
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ID_ALG, SYMBOL, COUNT }
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
                    public enum INDEX_CELL_PROPERTY : uint { CALC_DENY, IS_NAN }
                    /// <summary>
                    /// Признак запрета расчета
                    /// </summary>
                    public bool m_bCalcDeny;
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idParameter, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality, bool bCalcDeny)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                        m_bCalcDeny = bCalcDeny;
                    }

                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }
                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// Пояснения к параметру в алгоритме расчета
                /// </summary>
                public string m_strHeaderText
                    , m_strToolTipText
                    , m_strMeasure
                    , m_strSymbol;

                ///// <summary>
                ///// Признак отображения строки
                ///// </summary>
                //public bool m_bVisibled;

                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRound;

                public HDataGridViewCell[] m_arPropertiesCells;

                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];
                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT, false);
                }
            }
            /// <summary>
            /// Перечисления для индексирования массива со значениями цветов для фона ячеек
            /// </summary>
            protected enum INDEX_COLOR : uint
            {
                EMPTY, VARIABLE, DEFAULT, CALC_DENY, NAN, PARTIAL, NOT_REC, LIMIT,
                USER
                    , COUNT
            }
            /// <summary>
            /// Массив со значениями цветов для фона ячеек
            /// </summary>
            protected static Color[] s_arCellColors = new Color[(int)INDEX_COLOR.COUNT] { Color.Gray //EMPTY
                , Color.White //VARIABLE
                , Color.Yellow //DEFAULT
                , Color.LightGray //CALC_DENY
                , Color.White //NAN
                , Color.BlueViolet //PARTIAL
                , Color.Sienna //NOT_REC
                , Color.Red //LIMIT
                , Color.White //USER
            };

            protected struct RATIO
            {
                public int m_id;

                public int m_value;

                public string m_nameRU
                    , m_nameEN
                    , m_strDesc;
            }

            protected Dictionary<int, RATIO> m_dictRatio;

            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO()
                    {
                        m_id = (int)r[@"ID"]
                        , m_value = (int)r[@"VALUE"]
                        , m_nameRU = (string)r[@"NAME_RU"]
                        , m_nameEN = (string)r[@"NAME_RU"]
                        , m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }

            public abstract void AddColumn(int id_comp, string text, bool bVisibled);

            public abstract void AddRow(ROW_PROPERTY rowProp);

            public abstract void ShowValues(DataTable values, DataTable parameter/*, bool bUseRatio = true*/);

            public abstract void ClearColumns();

            public abstract void ClearRows();

            public abstract void ClearValues();

            //public abstract void UpdateStructure(int id_item/*, int id_par*/, PanelTaskTepValues.INDEX_ID indxDeny, bool bItemChecked);
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepCalculate : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END
                    , COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            public static DateTime s_dtDefault = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);

            public PanelManagementTaskTepCalculate()
                : base(8, 21)
            {
                InitializeComponents();

                //HDateTimePicker hdtpEnd = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                //m_dtRange = new DateTimeRange((Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value
                //    , hdtpEnd.Value);
                ////Назначить обработчик события - изменение дата/время начала периода
                //hdtpBegin.ValueChanged += new EventHandler(hdtpBegin_onValueChanged);
                //Назначить обработчик события - изменение дата/время окончания периода
                // при этом отменить обработку события - изменение дата/время начала периода
                // т.к. при изменении дата/время начала периода изменяется и дата/время окончания периода
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            private void InitializeComponents()
            {
                Control ctrl = null;         

                SuspendLayout();

                initializeLayoutStyle();

                ctrl = new ComboBox();
                ctrl.Name = INDEX_CONTROL_BASE.CBX_PERIOD.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //??? точное размещенеие в коде целевого класса
                this.Controls.Add(ctrl); //??? добавлять для возможности последующего поиска

                ctrl = new ComboBox();
                ctrl.Name = INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl); //??? добавлять для возможности последующего поиска (без указания столбца, строки)

                ctrl = new HDateTimePicker(s_dtDefault, null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl); //??? добавлять для возможности последующего поиска (без указания столбца, строки)

                ctrl = new HDateTimePicker(s_dtDefault.AddHours(1), Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl); //??? добавлять для возможности последующего поиска (без указания столбца, строки)

                ResumeLayout(false);
                PerformLayout();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            ///// <summary>
            ///// Обработчик события - изменение дата/время начала периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //private void hdtpBegin_onValueChanged(object obj, EventArgs ev)
            //{
            //    m_dtRange.Set((obj as HDateTimePicker).Value, m_dtRange.End);

            //    DateTimeRangeValue_Changed(this, EventArgs.Empty);
            //}

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEnd = obj as HDateTimePicker;
                //m_dtRange.Set(hdtpEnd.LeadingValue, hdtpEnd.Value);

                if (!(DateTimeRangeValue_Changed == null))
                    DateTimeRangeValue_Changed(hdtpEnd.LeadingValue, hdtpEnd.Value);
                else
                    ;
            }

            public DateTimeRange RangeDateTime
            {
                get {
                    return new DateTimeRange((Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value
                        , (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value);
                }
            }

            public void SetPeriod(ID_PERIOD idPeriod)
            {
                HDateTimePicker hdtpBegin = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                    , hdtpEnd = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                //Выполнить запрос на получение значений для заполнения 'DataGridView'
                switch (idPeriod)
                {
                    case ID_PERIOD.HOUR:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , DateTime.Now.Hour
                            , 0
                            , 0).AddHours(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddHours(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.HOUR;
                        break;
                    //case ID_PERIOD.SHIFTS:
                    //    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    //    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    //    break;
                    case ID_PERIOD.DAY:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , 0
                            , 0
                            , 0).AddDays(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddDays(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.DAY;
                        break;
                    case ID_PERIOD.MONTH:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , 1
                            , 0
                            , 0
                            , 0).AddMonths(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddMonths(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.MONTH;
                        break;
                    case ID_PERIOD.YEAR:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , 1
                            , 1
                            , 0
                            , 0
                            , 0).AddYears(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddYears(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.YEAR;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
