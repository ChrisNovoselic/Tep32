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
            get {
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
            , ALL_COMPONENT, ALL_NALG // все идентификаторы компонентов ТЭЦ/параметров
            */, DENY_COMP_CALCULATED, DENY_PARAMETER_CALCULATED // запрещенных для расчета
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

                    //initialize();

                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , ID_PERIOD.DAY); //??? активный период требуется прочитать из [profile]
                    Session.CurrentIdPeriod = PanelManagement.IdPeriod;
                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , ID_TIMEZONE.MSK); //??? активный пояс требуется прочитать из [profile]
                    Session.CurrentIdTimezone = PanelManagement.IdTimezone;
                        //, (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)PanelManagement.IdTimezone)[0][@"OFFSET_UTC"]);

                    //// отобразить значения
                    //updateDataValues();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskTepValues::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        //protected abstract void initialize();
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
        protected override void panelManagement_DatetimeRangeChanged()
        {
            base.panelManagement_DatetimeRangeChanged();
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
        protected override void panelManagement_PeriodChanged()
        {
            base.panelManagement_PeriodChanged();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void onAddNAlgParameter(NALG_PARAMETER obj)
        {
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void onAddPutParameter(PUT_PARAMETER obj)
        {
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void onAddComponent(TECComponent obj)
        {
        }
        #endregion

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
            public DataGridViewTEPCalculate() : base (ModeData.NALG)
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
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //protected enum INDEX_SERVICE_COLUMN : uint { ID_ALG, SYMBOL, COUNT }            
            /// <summary>
            /// Перечисления для индексирования массива со значениями цветов для фона ячеек
            /// </summary>
            protected enum INDEX_COLOR : uint
            {
                EMPTY, VARIABLE, DEFAULT, DISABLED, NAN, PARTIAL, NOT_REC, LIMIT,
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

            [Flags]
            protected enum ModeAddColumn
            {
                NotSet
                , Insert = 1 // вставляемый (ТГ, в ~ от идентификатора)
                , Service = 2 // сервисный/добавялемый
                , Begined = 4 // всегда 1-ый (за сервисными)
                , Visibled = 8 // отображаемый
            }

            protected abstract void addColumn(TECComponent comp, ModeAddColumn mode);

            //public abstract void AddRow(NALG_PARAMETER nAlgParameter);

            public abstract void ShowValues(DataTable values/*, DataTable parameter, bool bUseRatio = true*/);

            public abstract void ClearColumns();

            //public abstract void ClearRows();

            //public abstract void ClearValues();

            //public abstract void UpdateStructure(int id_item/*, int id_par*/, PanelTaskTepValues.INDEX_ID indxDeny, bool bItemChecked);
        }
    }
}