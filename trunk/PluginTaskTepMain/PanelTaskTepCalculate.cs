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
        protected DataGridViewTaskTepCalculate m_dgvValues;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="iFunc">Объект для связи с сервером (внешней, вызывающей программой)</param>
        /// <param name="strNameTableAlg">Строка - наименование таблицы с параметрами алгоритма расчета</param>
        /// <param name="strNameTablePut">Строка - наименование таблицы с параметрами, детализированных до принадлежности к компоненту станции (оборудования)</param>
        /// <param name="strNameTableValues">Строка - наименование таблицы со значениями</param>
        protected PanelTaskTepCalculate(IPlugIn iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc, type)
        {
            HandlerDb.IdTask = ID_TASK.TEP;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDateTime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;

            InitializeComponents();

            Session.m_IdFpanel = m_Id;
            Session.SetDatetimeRange(PanelManagementTaskCalculate.s_dtDefault, PanelManagementTaskCalculate.s_dtDefault.AddHours(1));
        }
        /// <summary>
        /// Объект доступа к данным
        /// </summary>
        protected HandlerDbTaskTepCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskTepCalculate; } }

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

            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            string strItem = string.Empty;
            int i = -1;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMEZONE, COMP_LIST, PARAMETERS(Type), MODE_DEV, RATIO
            initialize(new ID_DBTABLE[] {
                    ID_DBTABLE.TIME
                    , ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , IsInParameters == true ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , IsOutParameters == true ? ID_DBTABLE.OUT_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.MODE_DEV
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );

            HandlerDb.FilterDbTableTimezone = TepCommon.HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = TepCommon.HandlerDbTaskCalculate.DbTableTime.Month
                | TepCommon.HandlerDbTaskCalculate.DbTableTime.Day
                | TepCommon.HandlerDbTaskCalculate.DbTableTime.Hour;
            HandlerDb.FilterDbTableCompList = TepCommon.HandlerDbTaskCalculate.DbTableCompList.Tec | TepCommon.HandlerDbTaskCalculate.DbTableCompList.Tg;

            if (err == 0)
                try {
                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , ID_PERIOD.DAY); //??? активный период требуется прочитать из [profile]
                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , ID_TIMEZONE.MSK); //??? активный пояс требуется прочитать из [profile]
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
            base.panelManagement_Period_onChanged();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);
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

        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(bool bClose = false)
        {
            HandlerDb.Clear();
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
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected abstract class DataGridViewTaskTepCalculate : DataGridViewValues
        {
            public DataGridViewTaskTepCalculate() : base (ModeData.NALG)
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

            protected abstract void addColumn(TepCommon.HandlerDbTaskCalculate.TECComponent comp, ModeAddColumn mode);

            //public abstract void AddRow(NALG_PARAMETER nAlgParameter);

            public abstract void ShowValues(DataTable values/*, DataTable parameter, bool bUseRatio = true*/);

            public abstract void ClearColumns();

            //public abstract void ClearRows();

            //public abstract void ClearValues();

            //public abstract void UpdateStructure(int id_item/*, int id_par*/, PanelTaskTepValues.INDEX_ID indxDeny, bool bItemChecked);
        }
    }
}