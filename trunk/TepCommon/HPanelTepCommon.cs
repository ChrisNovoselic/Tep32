using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;
using System.Globalization;
using static TepCommon.HandlerDbTaskCalculate;

namespace TepCommon
{
    public abstract partial class HPanelTepCommon : HPanelCommon
    {
        /// <summary>
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Тип(ы) расчетов, выполняемых на вкладке (м.б. установлены смешанные)
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE TaskCalculateType;
        /// <summary>
        /// Список значений, загруженных из БД
        /// </summary>
        protected Dictionary<KEY_VALUES, List<VALUES>> m_dictValues;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="plugIn">Объект для связи с вызывающей программой</param>
        /// <param name="type">Тип(ы) расчетов, выполняемых на вкладке</param>
        public HPanelTepCommon(IPlugIn plugIn, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base (plugIn)
        {
            TaskCalculateType = type;

            m_dictValues = new Dictionary<KEY_VALUES, List<VALUES>>();

            eventAddNAlgParameter += new Action<NALG_PARAMETER>(onAddNAlgParameter);

            eventAddPutParameter += new Action<PUT_PARAMETER>(onAddPutParameter);

            eventAddComponent += new Action<TECComponent>(onAddComponent);

            eventSetValuesCompleted += new Action(onSetValuesCompleted);
        }
        /// <summary>
        /// Поле
        /// </summary>
        private PanelManagementTaskCalculate __panelManagement;
        /// <summary>
        /// Свойство для обращения к панели управления
        ///  для автоматического назначения обработчиков событий
        ///  для реализации шаблона Singleton
        /// </summary>
        protected PanelManagementTaskCalculate _panelManagement
        {
            get { return __panelManagement; }

            set {
                if (__panelManagement == null) {
                    __panelManagement = value;
                    // обработчик события при изменении значений в основных элементах управления
                    __panelManagement.EventIndexControlBaseValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlBase_onValueChanged);
                    // обработчик события при изменении значений в дополнительных(добавленных программистом в наследуемых классах) элементах управления
                    __panelManagement.EventIndexControlCustomValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlCustom_onValueChanged);

                    __panelManagement.ItemCheck += new PanelManagementTaskCalculate.ItemCheckedParametersEventHandler(panelManagement_onItemCheck);
                } else
                    throw new Exception(string.Format(@"HPanelTepCommon._panelManagement::set () - повторное создание панели управления..."));
            }
        }
        /// <summary>
        /// Признак необходимости загрузки из БД входных значений
        /// </summary>
        public bool IsInParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES);
            }
        }
        /// <summary>
        /// Признак необходимости загрузки из БД выходных значений
        /// </summary>
        public bool IsOutParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME);
            }
        }
        /// <summary>
        /// Класс для описания аргумента события на панели управления, связанное с основными элементами управления
        ///  : период расчета, часовой пояс, диапазон для расчета
        /// </summary>
        public class EventIndexControlBaseValueChangedArgs : EventArgs
        {
        }
        /// <summary>
        /// Событие для добавления основного параметра для панели управления
        /// </summary>
        protected event Action<NALG_PARAMETER> eventAddNAlgParameter;
        /// <summary>
        /// Событие для добавления детализированного (компонент) параметра для панели управления
        /// </summary>
        protected event Action<PUT_PARAMETER> eventAddPutParameter;
        /// <summary>
        /// Событие при добавлении компонента(оборудования) станции
        /// </summary>
        protected event Action<TECComponent> eventAddComponent;
        /// <summary>
        /// Событие для оповещения панелей о завершении загрузки значений из БД
        /// </summary>
        protected event Action eventSetValuesCompleted;
        /// <summary>
        /// Метод для создания объекта панели управления
        /// </summary>
        /// <returns></returns>
        protected abstract PanelManagementTaskCalculate createPanelManagement();
        /// <summary>
        /// Удалить сессию (+ очистить реквизиты сессии)
        /// </summary>
        protected virtual void deleteSession()
        {
            int err = -1;

            m_dictValues.Clear();

            Session.Clear();

            (__handlerDb as HandlerDbTaskCalculate).DeleteSession(out err);
        }
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return (__handlerDb as HandlerDbTaskCalculate)._Session; } }
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(bool bClose = false)
        {
            if (bClose == true)
                _panelManagement.Clear();
            else
                ;

            deleteSession();            

            base.clear();
        }
        /// <summary>
        /// Обработчик события при изменении значений в основных элементах управления на панели упарвления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected virtual void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            if ((obj == null)
                || ((!(obj == null))
                    && ((ID_DBTABLE)obj == ID_DBTABLE.UNKNOWN))) {
            // изменен DateTimeRange
                //??? перед очисткой или после (не требуются ли предыдущий диапазон даты/времени)
                Session.SetDatetimeRange(_panelManagement.DatetimeRange);

                if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok)
                    panelManagement_DatetimeRange_onChanged();
                else
                    ;
            } else {
            // изменены PERIOD или TIMEZONE
                switch ((ID_DBTABLE)obj) {
                    case ID_DBTABLE.TIME:
                        Session.CurrentIdPeriod = _panelManagement.IdPeriod;

                        if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok) {
                            //clear();

                            panelManagement_Period_onChanged();
                        } else
                            ;
                        break;
                    case ID_DBTABLE.TIMEZONE:
                        Session.CurrentIdTimezone = _panelManagement.IdTimezone;
                            //, (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)_panelManagement.IdTimezone)[0][@"OFFSET_UTC"]);

                        Session.SetDatetimeRange(_panelManagement.DatetimeRange);

                        if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok) {
                            //clear();

                            panelManagement_TimezoneChanged();
                        } else
                            ;
                        break;
                    default:
                        throw new Exception(string.Format(@"HPanelTepCommon::panelManagement_EventIndexControlBase_onValueChanged () - {} неизвестный тип события...", obj));
                        //break;
                }
            }

            //// очистить содержание представления
            //clear();
            ////// при наличии признака - загрузить/отобразить значения из БД
            ////if (s_bAutoUpdateValues == true)
            ////    updateDataValues();
            ////else ;
        }
        /// <summary>
        /// Обработчик события при изменении значений в дополнительных элементах управления на панели упарвления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected virtual void panelManagement_EventIndexControlCustom_onValueChanged(object obj) { }        
        /// <summary>
        /// Обработчик события на панели управления - изменение признака выбора снятия/постановки на отображение элемента
        ///  , включения/выключения из расчета элемента
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        protected abstract void panelManagement_onItemCheck(PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev);
        /// <summary>
        /// Добавить на панель все элементы: параметры в алгоритме расчета, компоненты станции
        /// </summary>
        private void add_all()
        {
            addComponents();

            if (IsInParameters == true)
                addAlgParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES);
            else
                ;

            if (IsOutParameters == true)
                addAlgParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменение диапазона времени расчета
        /// </summary>
        protected virtual void panelManagement_DatetimeRange_onChanged()
        {
        }
        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void panelManagement_Period_onChanged()
        {
            add_all();
        }
        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        protected virtual void panelManagement_TimezoneChanged()
        {
            add_all();
        }
        /// <summary>
        /// Метод по завершению загрузки из БД одного параметра в алгоритме расчета
        /// </summary>
        /// <param name="obj">Параметр в алгоритме расчета</param>
        protected abstract void onAddNAlgParameter(NALG_PARAMETER obj);
        /// <summary>
        /// Метод по завершению загрузки из БД одного параметра, связанного с компонентом станции, в алгоритме расчета
        /// </summary>
        /// <param name="obj">Параметр в алгоритме расчета, связанный с компонентом станции</param>
        protected abstract void onAddPutParameter(PUT_PARAMETER obj);
        /// <summary>
        /// Метод по завершению загрузки информации по компоненту станции
        /// </summary>
        /// <param name="comp">Объект, описывающий компонент станции</param>
        protected abstract void onAddComponent(TECComponent comp);
        /// <summary>
        /// Обраюотчик события - завершение загрузки значений из БД
        /// </summary>
        protected abstract void onSetValuesCompleted();

        #region Добавление компонентов, параметров в алгоритме расчета
        private bool getIdComponentOwner(int id_comp, out int id_comp_owner)
        {
            bool bRes = false;

            DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.COMP_VALUES].Select(string.Format(@"ID={0} AND ID_PAR={1}", id_comp, (int)COMP_PARAMETER.OWNER));

            if (rows.Length == 1)
                bRes = int.TryParse((string)rows[0][@"VALUE"], out id_comp_owner);
            else {
                id_comp_owner = -1;

                bRes = false;
            }

            return bRes;
        }
        /// <summary>
        /// Добавить компоненты станции для панели
        /// </summary>
        private void addComponents()
        {
            int err = -1
                , id_comp = -1, id_comp_owner = -1
                , enabled = -1, visibled = -1;
            bool bEnabled = false
                , bVisibled = false;

            foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows) {
                id_comp = r[@"ID"] is DBNull ? -1 : (short)r[@"ID"];

                if (id_comp > 0) {
                    if (getIdComponentOwner(id_comp, out id_comp_owner) == true) {
                        if (int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.ENABLED_ITEM), out enabled) == false)
                            enabled = -1;
                        else
                            ;
                        bEnabled = !(enabled < 0) ? enabled == 0 ? false : enabled == 1 ? true : true : true;

                        if (int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.VISIBLED_ITEM), out visibled) == false)
                            visibled = -1;
                        else
                            ;
                        bVisibled = !(visibled < 0) ? visibled == 0 ? false : visibled == 1 ? true : true : true;

                        eventAddComponent(new TECComponent(id_comp
                            , id_comp_owner
                            , r[@"DESCRIPTION"] is DBNull ? string.Empty : ((string)r[@"DESCRIPTION"]).Trim()
                            , bEnabled
                            , bVisibled
                        ));
                    } else
                        Logging.Logg().Error(string.Format(@"HPanelTepCommon::panelManagement_PeriodChanged () - не определенный идентификатор родительского компонента для {0}...", id_comp), Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    Logging.Logg().Error(string.Format(@"HPanelTepCommon::panelManagement_PeriodChanged () - не определенный идентификатор компонента..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// Добавить параметры из алгоритма расчета
        /// </summary>
        /// <param name="type">Тип расчета</param>
        private void addAlgParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            int err = -1
                , id_alg = -1
                , id_comp = -1, id_comp_owner = -1
                , prjRatio = -1, vsRatio = -1, vsRound = -1
                , enabled = -1, visibled = -1;
            bool bEnabled = false
                , bVisibled = false;
            string n_alg = string.Empty
                , comp_shr_name = string.Empty;
            List<int> listIdNAlg = new List<int>();
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            HandlerDbTaskCalculate.TECComponent component;

            dictVisualSettings = _handlerDb.GetParameterVisualSettings(new int[] { m_Id, (int)Session.CurrentIdPeriod }, out err);

            //Список параметров для отображения
            IEnumerable<DataRow> listParameter =
                // в каждой строке значения полей, относящихся к параметру алгоритма расчета одинаковые, т.к. 'ListParameter' объединение 2-х таблиц
                //ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First()) // исключить дублирование по полю [ID_ALG]
                type == HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? _handlerDb.ListInParameter.Select(x => x)
                    : type == HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES ? _handlerDb.ListOutParameter.Select(x => x)
                        : new List<DataRow>()
                            ;

            //Заполнить элементы управления с компонентами станции 
            foreach (DataRow r in listParameter) {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();

                if (int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, n_alg, HTepUsers.ID_ALLOWED.ENABLED_ITEM), out enabled) == false)
                    enabled = -1;
                else
                    ;
                bEnabled = !(enabled < 0) ? enabled == 0 ? false : enabled == 1 ? true : true : true;

                if (int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, n_alg, HTepUsers.ID_ALLOWED.VISIBLED_ITEM), out visibled) == false)
                    visibled = -1;
                else
                    ;
                bVisibled = !(visibled < 0) ? visibled == 0 ? false : visibled == 1 ? true : true : true;

                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (listIdNAlg.IndexOf(id_alg) < 0) {
                    // добавить в список идентификатор параметра алгоритма расчета
                    listIdNAlg.Add(id_alg);                    

                    //strItem = string.Format(@"{0} ({1})", n_alg, ((string)r[@"NAME_SHR"]).Trim());                    
                    // получить значения для настройки визуального отображения
                    if (dictVisualSettings.ContainsKey(n_alg) == true) {
                        // установленные в проекте
                        vsRatio = dictVisualSettings[n_alg].m_ratio;
                        vsRound = dictVisualSettings[n_alg].m_round;
                    } else {
                        // по умолчанию
                        vsRatio = HTepUsers.s_iRatioDefault;
                        vsRound = HTepUsers.s_iRoundDefault;
                    }

                    eventAddNAlgParameter(new NALG_PARAMETER(
                        type
                        , id_alg, n_alg
                        , r[@"NAME_SHR"] is DBNull ? string.Empty : ((string)r[@"NAME_SHR"]).Trim()
                        , r[@"DESCRIPTION"] is DBNull ? string.Empty : ((string)r[@"DESCRIPTION"]).Trim()
                        , (AGREGATE_ACTION)short.Parse(r[@"AVG"].ToString().Trim())
                        , ((int)r[@"ID_MEASURE"])
                        , r[@"NAME_SHR_MEASURE"] is DBNull ? string.Empty : ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        , r[@"SYMBOL"] is DBNull ? string.Empty : ((string)r[@"SYMBOL"]).Trim()
                        , bEnabled
                        , bVisibled
                        //, prjRatio
                        , vsRatio, vsRound
                    ));
                } else {
                    // параметр уже был добавлен                    
                }

                // всегда добавлять (каждый параметр)
                id_comp = (int)r[@"ID_COMP"];
                if ((id_comp > 0)
                    && (getIdComponentOwner(id_comp, out id_comp_owner) == true)) {
                    if (m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Select(string.Format(@"ID={0}", id_comp)).Length == 1) {
                        comp_shr_name = m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Select(string.Format(@"ID={0}", id_comp))[0][@"DESCRIPTION"].ToString().Trim();                    

                        component = new TECComponent(id_comp
                            , id_comp_owner
                            , comp_shr_name
                            , bEnabled
                            , bVisibled
                        );

                        prjRatio = (int)r[@"ID_RATIO"];

                        // только, если назначенн обработчик в 'PanelTaskTepOutVal'
                        eventAddPutParameter?.Invoke(new PUT_PARAMETER() {
                            /*Key = new PUT_PARAMETER.KEY() {*/ m_idNAlg = id_alg/*, m_idComp = id_comp }*/
                            , m_Id = (int)r[@"ID"]
                            , m_component = component
                            , m_prjRatio = prjRatio
                            , m_bEnabled = bEnabled
                            , m_bVisibled = bVisibled
                            ,
                        });
                    } else
                        Logging.Logg().Error(string.Format(@"::addAlgParameters () - для ID_ALG={0}, N_ALG={1}, ID_COMPONENT={2} компонент вне установленного фильтра компонентов задачи..."
                                , id_alg, n_alg, id_comp)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    Logging.Logg().Error(string.Format(@"::addAlgParameters () - для ID_ALG={0}, N_ALG={1} некорректный идентификатор (ID_COMPONENT не найден) параметра в алгоритме расчета..."
                            , id_alg, n_alg)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        #endregion
        /// <summary>
        /// Ссылка на объект для обращения к БД
        /// </summary>
        protected HandlerDbTaskCalculate _handlerDb { get { return __handlerDb as HandlerDbTaskCalculate; } }
        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        ///  в случае загрузки "сырых" значений = ID_PERIOD.HOUR
        ///  в случае загрузки "учтенных" значений -  в зависимости от установленного пользователем</param>
        /// </summary>
        /// </summary>
        protected virtual void updateDataValues()
        {
            int err = -1
                //, cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1
                , iRegDbConn = -1; // признак установленного соединения (ошибка, был создан ранее, новое соединение)
            string errMsg = string.Empty;

            __handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0)) {
                // установить значения в таблицах для расчета, создать новую сессию
                // предыдущая сессия удалена в 'clear'
                setValues(out err, out errMsg);

                if (err == 0) {
                    // создать копии для возможности сохранения изменений
                    cloneValues();
                    // отобразить значения
                    eventSetValuesCompleted?.Invoke();
                } else {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();

                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            } else
                ;

            if (!(iRegDbConn > 0))
                __handlerDb.UnRegisterDbConnection();
            else
                ;
        }

        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        protected virtual void setValues(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string strQuery = string.Empty;
            ID_DBTABLE idDbTable = ID_DBTABLE.UNKNOWN;
            Dictionary<KEY_VALUES, DataTable> dictTableValues;

            Session.NewId();

            //m_dictValues.Clear(); - очищена в 'deleteSession'
            dictTableValues = new Dictionary<KEY_VALUES, DataTable>();

            foreach (TaskCalculate.TYPE type in Enum.GetValues(typeof(TaskCalculate.TYPE)))
                if (((!((int)type == 0)) && ((int)type > 0) && (!(type == TaskCalculate.TYPE.UNKNOWN)))
                    && (TaskCalculateType & type) == type) {
                    //m_dictValues[new KEY_VALUES() { TypeState = HandlerDbValues.STATE_VALUE.ORIGINAL, TypeCalculate = type }] =
                    dictTableValues.Add(new KEY_VALUES() { TypeState = HandlerDbValues.STATE_VALUE.ORIGINAL, TypeCalculate = type }
                        , _handlerDb.GetTableValues(m_Id, type, out err, out strErr));
                } else
                    ;

            //Начать новую сессию расчета
            _handlerDb.CreateSession(m_Id
                //, Session.CountBasePeriod
                , dictTableValues[new KEY_VALUES() { TypeState = HandlerDbValues.STATE_VALUE.ORIGINAL, TypeCalculate = TaskCalculate.TYPE.IN_VALUES }]
                , new DataTable()
                , out err, out strErr);

            //foreach (KEY_VALUES keyValues in m_dictValues.Keys) {
            foreach (KEY_VALUES keyValues in dictTableValues.Keys) {
                idDbTable = keyValues.TypeCalculate == TaskCalculate.TYPE.IN_VALUES ? ID_DBTABLE.INVALUES :
                    keyValues.TypeCalculate == TaskCalculate.TYPE.OUT_VALUES ? ID_DBTABLE.OUTVALUES :
                        ID_DBTABLE.UNKNOWN; // не найдено наименование таблицы, ошибка в запросе

                if (!(idDbTable == ID_DBTABLE.UNKNOWN)) {
                // получить результирующаю таблицу
                // получить входные для расчета значения для возможности редактирования
                    strQuery = string.Format(@"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [EXTENDED_DEFINITION] as [DATE_TIME], [WR_DATETIME]" // [ID_PUT] as [ID] 
                        + @" FROM [{0}]"
                        + @" WHERE [ID_SESSION]={1}"
                            , HandlerDbValues.s_dictDbTables[idDbTable].m_name
                            , Session.m_Id);

                    m_dictValues.Add(keyValues
                        //, HandlerDbTaskCalculate.TableToListValues(dictTableValues[keyValues]) // простое копирование из таблицы
                        , HandlerDbTaskCalculate.TableToListValues(_handlerDb.Select(strQuery, out err)) // сложное обращение к БД, но происходит дополнительная проверка (создание новой сессии с корректными данными)
                        );
                } else
                    Logging.Logg().Error (string.Format(@"HPanelTepCommon::setValues () - не найден идентификатор таблицы БД..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        protected virtual void cloneValues()
        {
            List<KEY_VALUES> keys = new List<KEY_VALUES>();

            //for (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES indx = (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.UNKNOWN + 1);
            //    indx < TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT;
            //    indx++)
            //    if (!(m_arTableOrigin[(int)indx] == null))
            //        m_arTableEdit[(int)indx] =
            //            m_arTableOrigin[(int)indx].Copy();
            //    else
            //        ;

            keys = m_dictValues.Keys.ToList();

            foreach (KEY_VALUES key in keys)
            //??? создается ли новая копия
                m_dictValues.Add(new KEY_VALUES() { TypeCalculate = key.TypeCalculate, TypeState = HandlerDbValues.STATE_VALUE.EDIT }, new List<VALUES> (m_dictValues[key]));
        }

        public override void Stop()
        {
            clear(true);

            base.Stop();
        }

        /// <summary>
        /// ??? дублирование метода 'HMath::Parse' преобразование числа в нужный формат отображения
        /// </summary>
        /// <param name="value">число</param>
        /// <returns>преобразованное число</returns>
        public static float AsParseToF(string value)
        {
            int _indxChar = 0;
            string _sepReplace = string.Empty;
            bool bFlag = true;
            //char[] _separators = { ' ', ',', '.', ':', '\t'};
            //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();
            float fValue = 0;

            foreach (char item in value.ToCharArray()) {
                if (!char.IsDigit(item))
                    if (char.IsLetter(item))
                        value = value.Remove(_indxChar, 1);
                    else
                        _sepReplace = value.Substring(_indxChar, 1);
                else
                    _indxChar++;

                switch (_sepReplace) {
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
                try {
                    fValue = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                } catch (Exception e) {
                    if (value.ToString() == "")
                        fValue = 0;
                }


            return fValue;
        }
    }
}
