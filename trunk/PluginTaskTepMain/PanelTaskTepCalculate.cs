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
using System.Reflection;

namespace PluginTaskTepMain
{
    public abstract partial class PanelTaskTepCalculate : HPanelTepCommon
    {
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementTaskCalculate PanelManagement
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
        
        ///// <summary>
        ///// Таблицы со значениями словарных, проектных данных
        ///// </summary>
        //protected DataTable[] m_dictTableDictPrj;
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1
            /*, PERIOD // идентификаторы периодов расчетов, использующихся на форме
            , TIMEZONE // идентификаторы (целочисленные, из БД системы) часовых поясов
            , ALL_COMPONENT*/, ALL_NALG // все идентификаторы компонентов ТЭЦ/параметров
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
            TaskCalculateType = type;

            HandlerDb.IdTask = ID_TASK.TEP;

            InitializeComponents();

            Session.m_IdFpanel = m_Id;
            Session.SetDatetimeRange(PanelManagementTaskCalculate.s_dtDefault, PanelManagementTaskCalculate.s_dtDefault.AddHours(1));
        }

        protected TepCommon.HandlerDbTaskCalculate HandlerDb { get { return m_handlerDb as TepCommon.HandlerDbTaskCalculate; } }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskTepCalculate();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents ()
        {
        }

        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];
            //for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
            //    switch (id)
            //    {
            //        case INDEX_ID.PERIOD:
            //            m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR/*, (int)ID_PERIOD.SHIFTS*/, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
            //            break;
            //        case INDEX_ID.TIMEZONE:
            //            m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
            //            break;
            //        default:
            //            //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
            //            m_arListIds[(int)id] = new List<int>();
            //            break;
            //    }

            //HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            string strItem = string.Empty;
            int i = -1;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMEZONE, COMP_LIST, PARAMETERS(Type), MODE_DEV, RATIO
            initialize(new ID_DBTABLE[] {
                    ID_DBTABLE.TIME
                    , ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , TaskCalculateType == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? ID_DBTABLE.IN_PARAMETER :
                        TaskCalculateType == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES ? ID_DBTABLE.OUT_PARAMETER :
                            TaskCalculateType == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES ? ID_DBTABLE.OUT_PARAMETER :
                                TaskCalculateType == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME ? ID_DBTABLE.OUT_PARAMETER :
                                    ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.MODE_DEV
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );
            m_dictTableDictPrj.FilterDbTableTimezone = DictionaryTableDictProject.DbTableTimezone.Msk;
            m_dictTableDictPrj.FilterDbTableTime = DictionaryTableDictProject.DbTableTime.Hour
                | DictionaryTableDictProject.DbTableTime.Day
                | DictionaryTableDictProject.DbTableTime.Month;
            m_dictTableDictPrj.FilterDbTableCompList = DictionaryTableDictProject.DbTableCompList.Tec | DictionaryTableDictProject.DbTableCompList.Tg;

            if (err == 0)
                try {
                    //m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Clear();

                    initialize();

                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , ID_PERIOD.HOUR); //??? активный период требуется прочитать из [profile]
                    Session.SetCurrentPeriod(PanelManagement.IdPeriod);
                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , ID_TIMEZONE.MSK); //??? активный пояс требуется прочитать из [profile]
                    setCurrentTimeZone(PanelManagement.IdTimezone);

                    //// отобразить значения
                    //updateDataValues();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskTepValues::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
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
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
            Session.SetCurrentPeriod(PanelManagement.IdPeriod);
            setCurrentTimeZone(PanelManagement.IdTimezone);
            //??? перед очисткой или после (не требуются ли предыдущий диапазон даты/времени)
            Session.SetDatetimeRange(PanelManagement.DatetimeRange);

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
        protected virtual void panelManagement_onPeriodChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ID_TIMEZONE idTimezone)
        {
            Session.SetCurrentTimeZone(idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)idTimezone)[0][@"OFFSET_UTC"]);
        }
        ///// <summary>
        ///// Массив запросов к БД по получению словарных и проектных значений
        ///// </summary>
        //private string[] getQueryDictPrj()
        //{
        //    string[] arRes = null;

        //    arRes = new string[]
        //    {
        //        //PERIOD
        //        HandlerDb.GetQueryTimePeriods (m_strIdPeriods)
        //        //TIMEZONE
        //        , HandlerDb.GetQueryTimezones (m_strIdTimezones)
        //        // список компонентов
        //        , HandlerDb.GetQueryCompList ()
        //        // параметры расчета
        //        , HandlerDb.GetQueryParameters (Type)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        , HandlerDb.GetQueryModeDev ()
        //        //// единицы измерения
        //        //, m_handlerDb.GetQueryMeasures ()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio ()
        //    };

        //    return arRes;
        //}        
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(bool bClose = false)
        {
            deleteSession();
            //??? повторная проверка
            if (bClose == true) {
                PanelManagement.Clear(); // прежде удаления элементов из списка отменить регистрацию обработки событий "изменение текущ./индекса"

                m_dgvValues.ClearRows();
                m_dgvValues.ClearColumns();
            }
            else
            // очистить содержание представления
                m_dgvValues.ClearValues();

            base.clear(bClose);
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
        protected abstract class DataGridViewTEPCalculate : DataGridViewValues
        {
            public DataGridViewTEPCalculate()
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
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

            public abstract void AddColumn(int id_comp, string text, bool bVisibled);

            public abstract void AddRow(ROW_PROPERTY rowProp);

            public abstract void ShowValues(DataTable values, DataTable parameter/*, bool bUseRatio = true*/);

            public abstract void ClearColumns();

            public abstract void ClearRows();

            public abstract void ClearValues();

            //public abstract void UpdateStructure(int id_item/*, int id_par*/, PanelTaskTepValues.INDEX_ID indxDeny, bool bItemChecked);
        }
    }
}