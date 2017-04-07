using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public abstract partial class HandlerDbTaskCalculate : HandlerDbValues
    {
        /// <summary>
        /// Описание компонента станции (ТГ, ГТП, ВЫВОД, ПромПлощадка)
        /// </summary>
        public struct TECComponent
        {
            //private TECComponent() { _id = -1; _iType = getType(_id); _idOwner = -1; _iTypeOwner = getType(_idOwner); m_nameShr = string.Empty; _bEnabled = true; _bVisibled = true; }

            public TECComponent(int id, int idOwner, string nameShr, bool bEnabled, bool bVisibled)
            {
                _id = id;

                _idOwner = idOwner;

                _iType = getType(_id);

                _iTypeOwner = getType(_idOwner);

                m_nameShr = nameShr;

                _bEnabled = bEnabled;

                _bVisibled = bVisibled;
            }

            public enum TYPE { UNKNOWN = -1, TEC = ID_COMP.TEC, GTP = ID_COMP.GTP, TG = ID_COMP.TG, UNREACHABLE = (int)ID_START_RECORD.ALG }

            private int _id;

            public int m_Id { get { return _id; } set { if (!(_id == value)) { _id = value; _iType = getType(value); } else; } }

            private int _idOwner;

            public int m_idOwner { get { return _idOwner; } set { if (!(_idOwner == value)) { _idOwner = value; _iTypeOwner = getType(value); } else; } }

            private TYPE _iType;

            public TYPE m_iType { get { return _iType; } }

            private TYPE _iTypeOwner;

            public TYPE m_iTypeOwner { get { return _iTypeOwner; } }

            public string m_nameShr;
            /// <summary>
            /// Признак включения (используется в расчете)
            /// </summary>
            private bool _bEnabled;

            public bool m_bEnabled { get { return _bEnabled; } }

            public void SetEnabled(bool value) { _bEnabled = value; }

            private bool _bVisibled;
            /// <summary>
            /// Признак отображения (отображать на вкладке/элементе интерфейса)
            /// </summary>
            public bool m_bVisibled { get { return _bVisibled; } }

            public void SetVisibled(bool value) { _bVisibled = value; }

            private static TYPE getType(int id)
            {
                TYPE typeRes = TYPE.UNKNOWN;

                if ((id > (int)TYPE.TG)
                    && (id < (int)TYPE.UNREACHABLE))
                    typeRes = TYPE.TG;
                else if ((id > (int)TYPE.GTP)
                    && (id < (int)TYPE.TG))
                    typeRes = TYPE.GTP;
                else if ((!(id < (int)TYPE.TEC))
                    && (id < (int)TYPE.GTP))
                    typeRes = TYPE.TEC;
                else
                    ;

                return typeRes;
            }

            public bool IsTg { get { return m_iType == TYPE.TG; } }

            public bool IsGtp { get { return m_iType == TYPE.GTP; } }

            public bool IsTec { get { return m_iType == TYPE.TEC; } }
        }
        /// <summary>
        /// Свойства параметра в алгоритме расчета 2-го уровня (связан с компонентом)
        /// </summary>
        public struct PUT_PARAMETER
        {
            //public struct KEY
            //{
            /// <summary>
            /// Идентификатор в БД элемента в алгоритме расчета
            /// </summary>
            public int m_idNAlg;
            //    /// <summary>
            //    /// Идентификатор в БД компонента(оборудования)
            //    /// </summary>
            //    public int m_idComp;
            //}

            //public KEY Key;
            /// <summary>
            /// Идентификатор в БД (в соответствии с компонентом-оборудованием)
            /// </summary>
            public int m_Id;

            public TECComponent m_component;

            public int IdComponent { get { return m_component.m_Id; } }

            public string NameShrComponent { get { return m_component.m_nameShr; } }

            public bool IsEnabled { get { return m_bEnabled; } }

            public bool IsVisibled { get { return m_bVisibled; } }

            public int m_prjRatio;
            /// <summary>
            /// Признак доступности (участия в расчете, если 'NALG' выключен, то и 'PUT' тоже выключен)
            /// </summary>
            public bool m_bEnabled;
            /// <summary>
            /// Признак отображения
            /// </summary>
            public bool m_bVisibled;
            /// <summary>
            /// Конструктор объекта - 
            /// </summary>
            /// <param name="id_alg">Идентификатор в БД элемента в алгоритме расчета</param>
            /// <param name="id_comp">Идентификатор в БД компонента(оборудования)</param>
            /// <param name="id_put">Идентификатор в БД (в соответствии с компонентом-оборудованием)</param>
            /// <param name="comp">Объект компонента</param>
            /// <param name="enabled">Признак доступности (участия в расчете, если 'NALG' выключен, то и 'PUT' тоже выключен)</param>
            //public PUT_PARAMETER(int id_alg, int id_put, TECComponent comp, bool enabled, bool visibled)
            //    : this(/*new TepCommon.HPanelTepCommon.PUT_PARAMETER.KEY() { m_idNAlg =*/ id_alg/*, m_idComp = id_comp }*/
            //        , id_put
            //        , comp
            //        , enabled
            //        , visibled)
            //{
            //}

            public PUT_PARAMETER(int id_alg/*KEY key*/, int id_put, TECComponent comp, int prjRatio, bool enabled, bool visibled)
            {
                m_idNAlg = id_alg //Key = key
                    ;
                m_Id = id_put;
                m_component = comp;
                m_prjRatio = prjRatio;
                m_bEnabled = enabled;
                m_bVisibled = visibled;
            }

            public void SetEnabled(bool value)
            {
                m_bEnabled = value;
            }

            public void SetVisible(bool value)
            {
                m_bVisibled = value;
            }

            public bool IsNaN { get { return m_Id < 0; } }
        }
        /// <summary>
        /// Свойства параметра в алгоритме расчета 1-го уровня (не связан с компонентом)
        ///  класс, т.к. от структуры наследование невозможно
        /// </summary>
        public class NALG_PARAMETER
        {
            //private TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE _type;

            public TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE m_type; // { get { return _type; } }

            private int _id;
            /// <summary>
            /// Идентификатор в БД элемента в алгоритме расчета
            /// </summary>
            public int m_Id
            {
                get {
                    return _id;
                }

                set {
                    _id = value;

                    //if ((!(_id < (int)ID_START_RECORD.))
                    //    && (true))
                }
            }

            public string m_nAlg;
            /// <summary>
            /// Наименовнаие (краткое) параметра
            /// </summary>
            public string m_strNameShr;
            /// <summary>
            /// Наименовнаие (полное) параметра
            /// </summary>
            public string m_strDescription;

            public AGREGATE_ACTION m_sAverage;

            public int m_iIdMeasure;
            /// <summary>
            /// Наименовнаие единиц измерения
            /// </summary>
            public string m_strMeausure;
            /// <summary>
            /// Формула (или символ для краткого обозначения)
            /// </summary>
            public string m_strSymbol;
            /// <summary>
            /// Признак доступности (участия в расчете, если 'NALG' выключен, то и 'PUT' тоже выключен)
            /// </summary>
            public bool m_bEnabled;
            /// <summary>
            /// Признак отображения (не имеет смысла для 'PUT', т.к. полностью зависит от 'NALG')
            /// </summary>
            public bool m_bVisibled;

            //public int m_prjRatio;
            /// <summary>
            /// Показатель степени 10 при преобразовании (для отображения) 
            /// </summary>
            public int m_vsRatio;
            /// <summary>
            /// Количество знаков после запятой при округлении (для отображения)
            /// </summary>
            public int m_vsRound;
            /// <summary>
            /// Конструктор объекта - 
            /// </summary>
            /// <param name="id_alg">Идентификатор в БД элемента в алгоритме расчета</param>
            /// <param name="nameShr">Наименовнаие параметра</param>
            /// <param name="enabled">Признак доступности (участия в расчете, если 'NALG' выключен, то и 'PUT' тоже выключен)</param>
            /// <param name="visibled">Признак отображения (не имеет смысла для 'PUT', т.к. полностью зависит от 'NALG')</param>
            /// <param name="prjRatio">Показатель степени 10 при преобразовании (загружаемое значение) </param>
            /// <param name="vsRatio">Показатель степени 10 при преобразовании (для отображения) </param>
            /// <param name="vsRound">Количество знаков после запятой при округлении (для отображения)</param>
            public NALG_PARAMETER(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type
                , int id_alg, string n_alg
                , string nameShr, string desc
                , AGREGATE_ACTION sAverage
                , int idMeasure, string nameShrMeasure, string symbol
                , bool enabled, bool visibled
                /*, int prjRatio*/
                , int vsRatio, int vsRound)
            {
                m_type = type;
                m_Id = id_alg;
                m_nAlg = n_alg;
                m_strNameShr = nameShr;
                m_strDescription = desc;
                m_sAverage = sAverage;
                m_iIdMeasure = idMeasure;
                m_strMeausure = nameShrMeasure;
                m_strSymbol = symbol;
                m_bEnabled = enabled;
                m_bVisibled = visibled;
                //m_prjRatio = prjRatio;
                m_vsRatio = vsRatio;
                m_vsRound = vsRound;
            }

            public NALG_PARAMETER(NALG_PARAMETER clone)
                : this(clone.m_type
                        , clone.m_Id, clone.m_nAlg
                        , clone.m_strNameShr
                        , clone.m_strDescription
                        , clone.m_sAverage
                        , clone.m_iIdMeasure
                        , clone.m_strMeausure
                        , clone.m_strSymbol
                        , clone.m_bEnabled, clone.m_bVisibled
                        /*, clone.m_prjRatio*/
                        , clone.m_vsRatio, clone.m_vsRound)
            {
            }
        }
        /// <summary>
        /// Ключ для идентификации значения
        ///  , иначе указание к какому параметру в алгоритме расчета принадлежит значение
        /// </summary>
        public struct KEY_VALUES
        {
            public TaskCalculate.TYPE TypeCalculate;

            public STATE_VALUE TypeState;
        }
        /// <summary>
        /// Значение для параметра в алгоритме расчета и его свойства
        /// </summary>
        public struct VALUES
        {
            public int m_IdPut;

            public int m_iQuality;

            public float value;

            public DateTime stamp_value;

            public DateTime stamp_write;
        }
        /// <summary>
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        public enum ID_VIEW_VALUES : short
        {
            SOURCE_IMPORT = -11
            , UNKNOWN = -1, SOURCE_LOAD, ARCHIVE, DEFAULT
                , COUNT
        }
        /// <summary>
        /// Перечисление - индексы (идентификаторы) типов значений, требующихся для расчета
        ///  в той или иной задаче
        /// </summary>
        public enum TABLE_CALCULATE_REQUIRED : short {
            UNKNOWN = -1
            , ALG, PUT
            , VALUE
                , COUNT
        }
        /// <summary>
        /// Перечисление - идентификаторы состояния полученных из БД значений
        /// </summary>
        public enum ID_QUALITY_VALUE { NOT_REC = -3, PARTIAL, DEFAULT, SOURCE, USER, CALCULATED }
        /// <summary>
        /// Мвксимальное кол-во строк для однвременной (в одном пакете запросов) вставки
        ///  в таблицу с временными значениями [INVAL]
        /// </summary>
        private const int MAX_ROWCOUNT_TO_INSERT = 666;

        private ID_TASK _iIdTask;
        /// <summary>
        /// Идентификатор задачи
        /// </summary>
        public ID_TASK IdTask { get { return _iIdTask; } set { if (!(_iIdTask == value)) { _iIdTask = value; createTaskCalculate(); } else; } }
        /// <summary>
        /// Объект для произведения расчетов
        /// </summary>
        protected TaskCalculate m_taskCalculate;

        public enum MODE_DATA_DATETIME { Begined, Ended }

        private MODE_DATA_DATETIME _modeDataDateTime;

        public MODE_DATA_DATETIME ModeDataDateTime { set { _modeDataDateTime = value; } }
        /// <summary>
        /// Параметры сессии
        /// </summary>
        public class SESSION
        {
            private int _idFPanel;

            public int m_IdFpanel { get { return _idFPanel; } set { _idFPanel = value; } }
            ///// <summary>
            ///// Перечисление - типы сессии
            ///// локальный: только для вкладок норматив, макет. В этом режиме расчет не возможен (только просмотр)
            ///// общий: для всех вкладок
            ///// Коррелирует с 'INDEX_VIEW_VALUES'
            ///// </summary>
            //protected enum TYPE : short
            //{
            //    Unknown = -1
            //    , Locale, Common
            //    , Count
            //}

            //protected TYPE m_type
            //{
            //    get
            //    {
            //        TYPE typeRes = TYPE.Unknown;

            //        //if (Type == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
            //        //    typeRes = TYPE.Common;
            //        //else
            //            if (m_LoadValues == INDEX_LOAD_VALUES.SOURCE)
            //                typeRes = TYPE.Common;
            //            else
            //                if (m_LoadValues == INDEX_LOAD_VALUES.ARCHIVE)
            //                    typeRes = TYPE.Locale;
            //                else
            //                    ;

            //        return typeRes;
            //    }
            //}            
            /// <summary>
            /// Признак отображаемых на текущий момент значений
            /// </summary>
            public ID_VIEW_VALUES m_ViewValues;

            private long _id;
            /// <summary>
            /// Идентификатор сессии - уникальный идентификатор
            ///  для наборов входных, расчетных (нормативных, макетных) значений
            /// </summary>
            public long m_Id { get { return _id; } set { _id = value; } }
            /// <summary>
            /// Текущий выбранный идентификатор периода расчета
            /// </summary>
            private ID_PERIOD _currentIdPeriod;

            public ID_PERIOD CurrentIdPeriod
            {
                get { return _currentIdPeriod; }

                set { if (!(_currentIdPeriod == value)) { _currentIdPeriod = value; } else; }
            }
            /// <summary>
            /// ??? Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
            /// </summary>
            public ID_PERIOD ActualIdPeriod { get { return m_ViewValues == ID_VIEW_VALUES.SOURCE_LOAD ? ID_PERIOD.DAY : _currentIdPeriod; } }
            /// <summary>
            /// Идентификатор текущий выбранного часового пояса
            /// </summary>
            private ID_TIMEZONE _currentIdTimezone;

            public ID_TIMEZONE CurrentIdTimezone
            {
                get { return _currentIdTimezone; }

                set {
                    if (!(_currentIdTimezone == value)) {
                        _currentIdTimezone = value;

                        m_curOffsetUTC = value == ID_TIMEZONE.UNKNOWN ?
                            TimeSpan.MinValue
                                : getOffsetUTC(_currentIdTimezone);
                    } else
                        ;
                }
            }

            public TimeSpan m_curOffsetUTC;

            public DateTimeRange m_DatetimeRange;
            /// <summary>
            /// Количество базовых периодов
            /// </summary>
            public int CountBasePeriod
            {
                get {
                    int iRes = -1;
                    ID_PERIOD idPeriod = ActualIdPeriod;

                    switch (idPeriod) {
                        case ID_PERIOD.HOUR:
                            iRes = (int)(m_DatetimeRange.End - m_DatetimeRange.Begin).TotalHours - 0;
                            break;
                        case ID_PERIOD.DAY:
                            iRes = (int)(m_DatetimeRange.End - m_DatetimeRange.Begin).TotalDays - 0;
                            break;
                        case ID_PERIOD.MONTH: //???
                        case ID_PERIOD.YEAR: //???
                        default:
                            iRes = 1;
                            break;
                    }

                    return iRes;
                }
            }

            public void Initialize(DataRow r
                //, TimeSpanDelegateIdTimezoneFunc getOffsetUTC
                )
            {
                Initialize((long)r[@"ID_CALCULATE"]
                    , (ID_PERIOD)r[@"ID_TIME"]
                    , (ID_TIMEZONE)r[@"ID_TIMEZONE"]
                    //, (int)r[@"OFFSET_UTC"]
                    , new DateTimeRange((DateTime)r[@"DATETIME_BEGIN"], (DateTime)r[@"DATETIME_END"])
                //, getOffsetUTC
                );
            }

            private TimeSpanDelegateIdTimezoneFunc getOffsetUTC;

            public void Initialize(long id
                , ID_PERIOD idPeriod
                , ID_TIMEZONE idTimezone
                , DateTimeRange rangeDatetime
                //, TimeSpanDelegateIdTimezoneFunc getOffsetUTC
                )
            {
                m_Id = id;
                CurrentIdPeriod = idPeriod;
                _currentIdTimezone = idTimezone;
                m_curOffsetUTC = getOffsetUTC(_currentIdTimezone);
                m_DatetimeRange = rangeDatetime;
            }

            public void Clear()
            {
                _currentIdPeriod = ID_PERIOD.UNKNOWN;
                _currentIdTimezone = ID_TIMEZONE.UNKNOWN;
            }

            public void NewId()
            {
                m_Id = HMath.GetRandomNumber();
            }

            public void SetDatetimeRange(DateTimeRange dtRange)
            {
                SetDatetimeRange(dtRange.Begin, dtRange.End);
            }

            public void SetDatetimeRange(DateTime dtBegin, DateTime dtEnd)
            {
                m_DatetimeRange.Set(dtBegin, dtEnd);
            }
            ///// <summary>
            ///// Установить новое значение для текущего периода
            ///// </summary>
            ///// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны двты/времени</param>
            //public void SetCurrentTimeZone(ID_TIMEZONE idTimazone, int offsetUTC)
            //{
            //    m_currIdTimezone = idTimazone;
            //    m_curOffsetUTC = offsetUTC;
            //}

            //public void SetCurrentPeriod(ID_PERIOD idPeriod)
            //{
            //    m_currIdPeriod = idPeriod;
            //}

            //public void SetIdFPanel(int idFPanel)
            //{
            //    _idFPanel = idFPanel;
            //}

            public SESSION(TimeSpanDelegateIdTimezoneFunc getOffsetUTC)
                : base()
            {
                m_Id = -1;
                m_IdFpanel = -1;
                m_ViewValues = ID_VIEW_VALUES.UNKNOWN;
                CurrentIdPeriod = ID_PERIOD.UNKNOWN;
                CurrentIdTimezone = ID_TIMEZONE.UNKNOWN;
                //, m_curOffsetUTC = TimeSpan.MinValue
                m_DatetimeRange = new DateTimeRange();

                this.getOffsetUTC = getOffsetUTC;
            }
        }

        public SESSION _Session;

        #region DbTableCompList
        [Flags]
        public enum DbTableCompList
        {
            NotSet = 0x0
            , PromPlozh = 1 //??? 3000
            , Vyvod = 2 //??? 2000
            , Tg = 4 //1000
            , Gtp = 8 //500
            , Tec = 16 //1
        }

        DbTableCompList _filterDbTableCompList;

        public DbTableCompList FilterDbTableCompList
        {
            get { return _filterDbTableCompList; }

            set {
                _filterDbTableCompList = value;
                //??? не учитывается
                DictionaryTableDictProject.Error iRes = DictionaryTableDictProject.Error.Any; // ошибка

                int[] limits = null;
                ID_DBTABLE idDbTable = ID_DBTABLE.COMP_LIST; // new ID_DBTABLE[] { ID_DBTABLE.COMP_LIST, ID_DBTABLE.IN_PARAMETER };
                DictionaryTableDictProject.ListTSQLWhere listTSQLWhere = new DictionaryTableDictProject.ListTSQLWhere(idDbTable); // null;

                try {
                    foreach (DbTableCompList item in Enum.GetValues(typeof(DbTableCompList))) {
                        if ((!(item == DbTableCompList.NotSet))
                            && (_filterDbTableCompList & item) == item) {
                            switch (item) {
                                case DbTableCompList.Tg:
                                    limits = new int[] { (int)ID_COMP.TG, (int)ID_COMP.VYVOD };
                                    break;
                                case DbTableCompList.Gtp:
                                    limits = new int[] { (int)ID_COMP.GTP, (int)ID_COMP.TG };
                                    break;
                                case DbTableCompList.Tec:
                                    limits = new int[] { (int)ID_COMP.TEC, (int)ID_COMP.GTP };
                                    break;
                                default:
                                    break;
                            }

                            if (!(limits == null))
                                listTSQLWhere.Add(new DictionaryTableDictProject.TSQLWhereItem(@"ID", DictionaryTableDictProject.TSQLWhereItem.RULE.Between, limits));
                            else
                                ;
                        } else
                            ;
                    }

                    //foreach (ID_DBTABLE idDbTable in idDbTables) {
                    //    listTSQLWhere = new DictionaryTableDictProject.ListTSQLWhere(idDbTable);

                        iRes = m_dictTableDictPrj.SetDbTableFilter(listTSQLWhere);
                    //}
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                        , _filterDbTableCompList, idDbTable)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = DictionaryTableDictProject.Error.ExFilterBuilder; // исключение при поиске строк для удаления
                }
            }
        }
        #endregion

        #region DbTableTimezone
        [Flags]
        public enum DbTableTimezone
        {
            NotSet = 0x0
            , Utc = 0x1
            , Msk = 0x2
            , Nsk = 0x4
        }

        DbTableTimezone _filterDbTableTimezone;

        public DbTableTimezone FilterDbTableTimezone
        {
            get { return _filterDbTableTimezone; }

            set {
                _filterDbTableTimezone = value;
                //??? не учитывается
                DictionaryTableDictProject.Error iRes = DictionaryTableDictProject.Error.Any; // ошибка

                ID_DBTABLE idDbTable = ID_DBTABLE.TIMEZONE;
                DictionaryTableDictProject.ListTSQLWhere listTSQLWhere = new DictionaryTableDictProject.ListTSQLWhere(idDbTable);
                ID_TIMEZONE idTimezone = ID_TIMEZONE.UNKNOWN;

                try {
                    foreach (DbTableTimezone item in Enum.GetValues(typeof(DbTableTimezone))) {
                        idTimezone = ID_TIMEZONE.UNKNOWN;

                        if ((_filterDbTableTimezone & item) == item)
                            switch (item) {
                                case DbTableTimezone.Utc:
                                    idTimezone = ID_TIMEZONE.UTC;
                                    break;
                                case DbTableTimezone.Msk:
                                    idTimezone = ID_TIMEZONE.MSK;
                                    break;
                                case DbTableTimezone.Nsk:
                                    idTimezone = ID_TIMEZONE.MSK;
                                    break;
                                default:
                                    break;
                            } else
                            ;

                        if (!(idTimezone == ID_TIMEZONE.UNKNOWN))
                            listTSQLWhere.Add(new DictionaryTableDictProject.TSQLWhereItem(@"ID", DictionaryTableDictProject.TSQLWhereItem.RULE.Equale, (int)idTimezone));
                        else
                            ;
                    }

                    iRes = m_dictTableDictPrj.SetDbTableFilter(listTSQLWhere);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                        , _filterDbTableCompList, idDbTable)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = DictionaryTableDictProject.Error.ExFilterBuilder; // исключение при поиске строк для удаления
                }
            }
        }
        #endregion

        #region DbTableTime
        [Flags]
        public enum DbTableTime
        {
            NotSet = 0x0
            , Hour = 0x1
            , Day = 0x2
            , Month = 0x4
        }

        DbTableTime _filterDbTableTime;

        public DbTableTime FilterDbTableTime
        {
            get { return _filterDbTableTime; }

            set {
                _filterDbTableTime = value;
                //??? не учитывается
                DictionaryTableDictProject.Error iRes = DictionaryTableDictProject.Error.Any; // ошибка

                ID_DBTABLE idDbTable = ID_DBTABLE.TIME;
                DictionaryTableDictProject.ListTSQLWhere listTSQLWhere = new DictionaryTableDictProject.ListTSQLWhere(idDbTable);
                ID_PERIOD idPeriod = ID_PERIOD.UNKNOWN;

                try {
                    foreach (DbTableTime item in Enum.GetValues(typeof(DbTableTime))) {
                        idPeriod = ID_PERIOD.UNKNOWN;

                        if ((_filterDbTableTime & item) == item)
                            switch (item) {
                                case DbTableTime.Hour:
                                    idPeriod = ID_PERIOD.HOUR;
                                    break;
                                case DbTableTime.Day:
                                    idPeriod = ID_PERIOD.DAY;
                                    break;
                                case DbTableTime.Month:
                                    idPeriod = ID_PERIOD.MONTH;
                                    break;
                                default:
                                    break;
                            } else
                            ;

                        if (!(idPeriod == ID_PERIOD.UNKNOWN))
                            listTSQLWhere.Add(new DictionaryTableDictProject.TSQLWhereItem(@"ID", DictionaryTableDictProject.TSQLWhereItem.RULE.Equale, (int)idPeriod));
                        else
                            ;
                    }

                    iRes = m_dictTableDictPrj.SetDbTableFilter(listTSQLWhere);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                        , _filterDbTableCompList, idDbTable)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = DictionaryTableDictProject.Error.ExFilterBuilder; // исключение при поиске строк для удаления
                }
            }
        }

        //private void setDbTableFilter(DbTableTime filterDbTableTime)
        //{
        //}
        #endregion

        public HandlerDbTaskCalculate(ID_TASK idTask = ID_TASK.UNKNOWN, MODE_DATA_DATETIME modeDataDateTime = MODE_DATA_DATETIME.Ended)
            : base()
        {
            _listNAlgParameter = new List<HandlerDbTaskCalculate.NALG_PARAMETER>();
            _listTECComponent = new List<HandlerDbTaskCalculate.TECComponent>();
            _listPutParameter = new List<HandlerDbTaskCalculate.PUT_PARAMETER>();

            _dictValues = new Dictionary<KEY_VALUES, List<VALUES>>();

            _filterDbTableCompList = DbTableCompList.NotSet;
            _filterDbTableTime = DbTableTime.NotSet;
            _filterDbTableTimezone = DbTableTimezone.NotSet;

            _Session = new SESSION(getOffsetUTC);

            IdTask = idTask;
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected virtual void createTaskCalculate(/*ID_TASK idTask*/)
        {
            if (!(m_taskCalculate == null))
                m_taskCalculate = null;
            else
                ;
        }

        public enum RESULT { Error = -1, Ok, Warning }
        /// <summary>
        /// Событие для оповещения панелей о завершении загрузки значений из БД
        /// </summary>
        public event Action<RESULT> EventSetValuesCompleted;

        public override void Clear()
        {
            _dictValues.Clear();

            _listNAlgParameter.Clear();
            _listPutParameter.Clear();
            _listTECComponent.Clear();

            deleteSession();

            base.Clear();
        }

        public virtual void UpdateDataValues(int idFPanel, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE taskCalculateType, HandlerDbTaskCalculate.ID_VIEW_VALUES viewValues)
        {
            _Session.m_ViewValues = viewValues;

            // ... - загрузить/отобразить значения из БД
            UpdateDataValues(idFPanel, taskCalculateType);
        }
        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        ///  в случае загрузки "сырых" значений = ID_PERIOD.HOUR
        ///  в случае загрузки "учтенных" значений -  в зависимости от установленного пользователем</param>
        /// </summary>
        /// </summary>
        public virtual void UpdateDataValues(int idFPanel, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE taskCalculateType)
        {
            int err = -1
                //, cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1
                , iRegDbConn = -1; // признак установленного соединения (ошибка, был создан ранее, новое соединение)
            string errMsg = string.Empty;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0)) {
                // установить значения в таблицах для расчета, создать новую сессию
                // предыдущая сессия удалена в 'clear'
                setValues(idFPanel, taskCalculateType, out err, out errMsg);

                if (err == 0) {
                    // создать копии для возможности сохранения изменений
                    cloneValues();
                    // отобразить значения
                    EventSetValuesCompleted?.Invoke(RESULT.Ok);
                } else {
                    // в случае ошибки "обнулить" идентификатор сессии
                    Clear();

                    EventSetValuesCompleted?.Invoke(RESULT.Error);

                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            } else
                ;

            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }
        /// <summary>
        /// Удалить сессию (+ очистить реквизиты сессии)
        /// </summary>
        protected virtual void deleteSession()
        {
            int err = -1;

            _Session.Clear();

            (this as HandlerDbTaskCalculate).DeleteSession(out err);
        }
        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        protected virtual void setValues(int idFPanel, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE taskCalculateType, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string strQuery = string.Empty;
            ID_DBTABLE idDbTable = ID_DBTABLE.UNKNOWN;
            Dictionary<KEY_VALUES, DataTable> dictTableValues;

            _Session.NewId();

            //m_dictValues.Clear(); - очищена в 'deleteSession'
            dictTableValues = new Dictionary<KEY_VALUES, DataTable>();

            foreach (TaskCalculate.TYPE type in Enum.GetValues(typeof(TaskCalculate.TYPE)))
                if (((!((int)type == 0)) && ((int)type > 0) && (!(type == TaskCalculate.TYPE.UNKNOWN)))
                    && (taskCalculateType & type) == type) {
                    //m_dictValues[new KEY_VALUES() { TypeState = HandlerDbValues.STATE_VALUE.ORIGINAL, TypeCalculate = type }] =
                    dictTableValues.Add(new KEY_VALUES() { TypeState = HandlerDbValues.STATE_VALUE.ORIGINAL, TypeCalculate = type }
                        , GetTableValues(idFPanel, type, out err, out strErr));
                } else
                    ;

            //Начать новую сессию расчета
            CreateSession(idFPanel
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
                            , _Session.m_Id);

                    _dictValues.Add(keyValues
                        //, HandlerDbTaskCalculate.TableToListValues(dictTableValues[keyValues]) // простое копирование из таблицы
                        , HandlerDbTaskCalculate.TableToListValues(Select(strQuery, out err)) // сложное обращение к БД, но происходит дополнительная проверка (создание новой сессии с корректными данными)
                        );
                } else
                    Logging.Logg().Error(string.Format(@"HPanelTepCommon::setValues () - не найден идентификатор таблицы БД..."), Logging.INDEX_MESSAGE.NOT_SET);
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

            keys = _dictValues.Keys.ToList();

            foreach (KEY_VALUES key in keys)
                //??? создается ли новая копия
                _dictValues.Add(new KEY_VALUES() { TypeCalculate = key.TypeCalculate, TypeState = HandlerDbValues.STATE_VALUE.EDIT }, new List<VALUES>(_dictValues[key]));
        }

        private DataTable mergeTableValues(DataTable tablePars, DataTable[] arTableValues, int cntBasePeriod)
        {
            DataTable tableRes = new DataTable();

            int iAVG = -1;
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsSel = null;
            List<object[]> listValuesToAdd = new List<object[]>();

            if (arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Columns.Count > 0) {
                tableRes = arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Clone();

                foreach (DataRow rValPar in tablePars.Rows) {
                    listValuesToAdd.Clear();
                    rowsSel = arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Select(@"ID_PUT=" + rValPar[@"ID"]);

                    if (rowsSel.Length == 0) {
                        // добавить из таблицы "по умолчанию"
                        rowsSel = arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Select(@"ID_PUT=" + rValPar[@"ID"]);

                        if (rowsSel.Length == 0) {
                            // добавить "0"
                            listValuesToAdd.Add(new object[] {
                                (int)rValPar[@"ID"]
                                //, HUsers.Id //ID_USER
                                //, -1 //ID_SOURCE
                                , _Session.m_Id //ID_SESSION
                                , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC //QUALITY
                                , 0F //VALUE
                                , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                , string.Format(@"{0}", DateTime.MinValue) //EXTENSION_DEFAULT
                            });
                        } else if (rowsSel.Length == 1)
                            listValuesToAdd.Add(new object[] {
                                rowsSel[0][@"ID_PUT"]
                                //, HUsers.Id //ID_USER
                                //, -1 //ID_SOURCE
                                , _Session.m_Id //ID_SESSION
                                , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT //QUALITY
                                , (iAVG == 0) ? cntBasePeriod * (double)rowsSel[0][@"VALUE"] : (double)rowsSel[0][@"VALUE"] //VALUE
                                , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                , string.Format(@"{0}", DateTime.MinValue) //EXTENSION_DEFAULT
                            });
                        else
                            // по идентификатору найдено не единственное значение для параметра расчета
                            Logging.Logg().Error(string.Format(@"HandlerDbTaskCalculate::mergeTableValues () - для ID_PERIOD={0} ID_PUT={1} найдено больше, чем одно значение ..."
                                , _Session.CurrentIdPeriod.ToString()
                                , rValPar[@"ID_PUT"])
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    } else {
                        if (rowsSel.Length > 0)
                            // добавить из источника
                            foreach (DataRow rValSrc in rowsSel)
                                listValuesToAdd.Add(new object[] {
                                    rValSrc[@"ID_PUT"]
                                    //, HUsers.Id //ID_USER
                                    //, -1 //ID_SOURCE
                                    , _Session.m_Id //ID_SESSION
                                    , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.SOURCE //QUALITY
                                    , (iAVG == 0) ? cntBasePeriod * (double)rValSrc[@"VALUE"] : (double)rValSrc[@"VALUE"] //VALUE
                                    , rValSrc[@"WR_DATETIME"]
                                    , rValSrc[@"EXTENDED_DEFINITION"]
                                });
                        else
                            // по идентификатору найдено не единственное значение для параметра расчета
                            Logging.Logg().Warning(string.Format(@"HandlerDbTaskCalculate::mergeTableValues () - для ID_PERIOD={0} ID_PUT={1} найдено больше, чем одно значение ..."
                                , _Session.CurrentIdPeriod.ToString()
                                , rValPar[@"ID_PUT"])
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    listValuesToAdd.ForEach(values => { tableRes.Rows.Add(values); });
                }
            } else
                ;

            //// удалить строки из таблицы со значениями "по умолчанию"
            //foreach (DataRow rValVar in arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Rows) {
            //    rowsSel = arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Select(@"ID_PUT=" + rValVar[@"ID_PUT"]);
            //    foreach (DataRow rToRemove in rowsSel)
            //        arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Rows.Remove(rToRemove);
            //}
            //// вставить строки из таблицы со значениями "по умолчанию"
            //foreach (DataRow rValDef in arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Rows) {
            //    rowsSel = tablePars.Select(@"ID=" + rValDef[@"ID_PUT"]);

            //    if (rowsSel.Length == 1) {
            //        iAVG = (Int16)rowsSel[0][@"AVG"];

            //        tableRes.Rows.Add(new object[]
            //            {
            //                rValDef[@"ID_PUT"]
            //                //, HUsers.Id //ID_USER
            //                //, -1 //ID_SOURCE
            //                , _Session.m_Id //ID_SESSION
            //                , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT //QUALITY
            //                , (iAVG == 0) ? cntBasePeriod * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
            //                , HDateTime.ToMoscowTimeZone() //??? GETADTE()
            //                , 0 //EXTENSION_DEFAULT
            //            }
            //        );
            //    } else
            //        ; // по идентификатору найден не единственный параметр расчета
            //}

            return tableRes;
        }
        /// <summary>
        /// Создать новую сессию для расчета
        ///  - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="idFPanel">Идентификатор панели на замену [ID_TASK]</param>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="tablePars">Таблица характеристик входных параметров</param>
        /// <param name="tableSessionValues">Таблица значений входных параметров</param>
        /// <param name="tableDefValues">Таблица значений по умолчанию входных параметров</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        public virtual void CreateSession(int idFPanel
            //, int cntBasePeriod
            //, DataTable tablePars
            , DataTable tableInValues
            , DataTable tableOutValues
            //, DateTimeRange dtRange
            , out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string strQuery = string.Empty;

            //correctValues(ref arTableValues[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]
            //    , ref tablePars);

            if ((tableInValues.Columns.Count > 0)
                && (tableInValues.Rows.Count > 0)) {
                //Вставить строку с идентификатором новой сессии
                insertIdSession(idFPanel/*, cntBasePeriod*/, out err);
                //Вставить строки в таблицу БД со входными значениями для расчета
                insertInValues(tableInValues, out err);
                //Вставить строки в таблицу БД с выходными значениями для расчета
                insertOutNullValues(out err);
            } else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Вставить в таблицу БД идентификатор новой сессии
        /// </summary>
        /// <param name="id">Идентификатор сессии</param>
        /// <param name="idPeriod">Идентификатор периода расчета</param>
        /// <param name="cntBasePeriod">Количество базовых периодов расчета в интервале расчета</param>
        /// <param name="idTimezone">Идентификатор часового пояса</param>
        /// <param name="dtRange">Диапазон даты/времени для интервала расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        protected void insertIdSession(int idFPanel
            //, int cntBasePeriod
            //, DateTimeRange dtRange
            , out int err)
        {
            err = -1;

            string strQuery = string.Empty;

            // подготовить содержание запроса при вставке значений, идентифицирующих новую сессию
            strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.SESSION].m_name + @" ("
                + @"[ID_CALCULATE]"
                + @", [ID_FPANEL]"
                + @", [ID_USER]"
                + @", [ID_TIME]"
                + @", [ID_TIMEZONE]"
                + @", [DATETIME_BEGIN]"
                + @", [DATETIME_END]) VALUES ("
                ;

            strQuery += _Session.m_Id;
            strQuery += @"," +
                //(Int32)IdTask
                idFPanel
                ;
            strQuery += @"," + HTepUsers.Id;
            strQuery += @"," + (int)_Session.CurrentIdPeriod;
            strQuery += @"," + (int)_Session.CurrentIdTimezone;
            strQuery += @",'" + _Session.m_DatetimeRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture)  // @"yyyyMMdd HH:mm:ss"
            strQuery += @",'" + _Session.m_DatetimeRange.End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";//(System.Globalization.CultureInfo.InvariantCulture) ; // @"yyyyMMdd HH:mm:ss"

            strQuery += @")";

            //Вставить в таблицу БД строку с идентификтором новой сессии
            DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
        }
        /// <summary>
        /// Вставить значения в таблицу для временных входных значений
        /// </summary>
        /// <param name="tableInValues">Таблица со значениями для вставки</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        private void insertInValues(DataTable tableInValues, out int err)
        {
            err = -1;

            string strQuery = string.Empty, strBaseQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;
            int iCntToInsert = -1; // кол-во строк обработанных для вставки

            // подготовить содержание запроса при вставке значений во временную таблицу для расчета
            strBaseQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name + @" (";

            arTypeColumns = new Type[tableInValues.Columns.Count];
            arNameColumns = new string[tableInValues.Columns.Count];
            foreach (DataColumn c in tableInValues.Columns) {
                arTypeColumns[c.Ordinal] = c.DataType;
                if (c.ColumnName.Equals(@"ID") == true)
                    strNameColumn = @"ID_PUT";
                else
                    strNameColumn = c.ColumnName;
                arNameColumns[c.Ordinal] = strNameColumn;
                strBaseQuery += strNameColumn + @",";
            }
            // исключить лишнюю запятую
            strBaseQuery = strBaseQuery.Substring(0, strBaseQuery.Length - 1);

            strBaseQuery += @") VALUES ";

            iCntToInsert = 0;
            strQuery = strBaseQuery;
            foreach (DataRow r in tableInValues.Rows) {
                strQuery += @"(";
                // вставить значения в запрос
                foreach (DataColumn c in tableInValues.Columns)
                    strQuery += DbTSQLInterface.ValueToQuery(r[c.Ordinal], arTypeColumns[c.Ordinal]) + @",";

                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);

                strQuery += @"),";

                iCntToInsert++;

                if (iCntToInsert > MAX_ROWCOUNT_TO_INSERT) {
                    // исключить лишнюю запятую
                    strQuery = strQuery.Substring(0, strQuery.Length - 1);
                    //Вставить во временную таблицу в БД входные для расчета значения
                    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
                    // обнулить счетчик строк, основной запрос
                    iCntToInsert = 0;
                    strQuery = strBaseQuery;
                } else
                    ;
            }
            // исключить лишнюю запятую
            strQuery = strQuery.Substring(0, strQuery.Length - 1);
            //Вставить во временную таблицу в БД входные для расчета значения
            DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
        }
        /// <summary>
        /// Вставить значения в таблицу для временных выходных значений сессии расчета
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        private void insertOutNullValues(out int err)
        {
            err = -1;

            TaskCalculate.TYPE type = TaskCalculate.TYPE.UNKNOWN;

            if ((IdTask == ID_TASK.TEP)
                || (IdTask == ID_TASK.AUTOBOOK))
                type = TaskCalculate.TYPE.OUT_TEP_NORM_VALUES;
            else if (IdTask == ID_TASK.AUTOBOOK)
                type = TaskCalculate.TYPE.OUT_VALUES;
            else
                ;

            if (type.Equals(TaskCalculate.TYPE.UNKNOWN) == false)
                insertOutNullValues(_Session.m_Id, type, out err);
            else
                ;
        }
        /// <summary>
        /// Вставить значения в таблицу для временных выходных значений сессии расчета
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="typeCalc">Тип расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        private void insertOutNullValues(long idSession, TaskCalculate.TYPE typeCalc, out int err)
        {
            err = -1;

            DataTable tableParameters = null;
            string strBaseQuery = string.Empty
                , strQuery = string.Empty;
            int iRowCounterToInsert = -1;

            strQuery = getQueryParameters(typeCalc);
            tableParameters = Select(strQuery, out err);

            strBaseQuery =
            strQuery =
                @"INSERT INTO " + s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @" VALUES ";

            iRowCounterToInsert = 0;
            foreach (DataRow rPar in tableParameters.Rows) {
                if (iRowCounterToInsert > MAX_ROWCOUNT_TO_INSERT) {
                    // исключить лишнюю запятую
                    strQuery = strQuery.Substring(0, strQuery.Length - 1);
                    // вставить строки в таблицу
                    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);

                    if (!(err == 0))
                        // при ошибке - не продолжать
                        break;
                    else
                        ;

                    strQuery = strBaseQuery;
                    iRowCounterToInsert = 0;
                } else
                    ;

                strQuery += @"(";

                strQuery += idSession + @"," //ID_SEESION
                    + rPar[@"ID"] + @"," //ID_PUT
                    + (-3).ToString() + @"," //QUALITY
                    + 0F.ToString() + @"," //VALUE
                    + string.Format(@"CONVERT(varchar, CONVERT(datetime2, '{0}', 102), 127)", DateTime.MinValue.ToString(@"yyyyMMdd HH:mm:ss")) + @","
                    + string.Format(@"'{0}'", DateTime.MinValue.ToString(@"yyyyMMdd HH:mm:ss")) //EXTENSION_DEFENITION
                    ;

                strQuery += @"),";

                iRowCounterToInsert++;
            }

            if (err == 0) {
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);
                // вставить строки в таблицу
                DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
            } else
                ; // при ошибке - не продолжать
        }

        protected virtual bool IsDeleteSession { get { return _Session.m_Id > 0; } }
        /// <summary>
        /// Удалить запись о параметрах сессии расчета (по триггеру - все входные и выходные значения)
        /// </summary>
        /// <param name="idSession">Идентификатор сессии расчета</param>
        /// <param name="err">Идентификатор ошибки при выполнении функции</param>
        public void DeleteSession(out int err)
        {
            err = 0; // предполагаем, ошибки нет

            int iRegDbConn = -1; // признак регистрации соединения с БД
            string strQuery = string.Empty;

            if (IsDeleteSession == true) {
                RegisterDbConnection(out iRegDbConn);

                if (!(iRegDbConn < 0)) {
                    strQuery = @"DELETE FROM [dbo].[" + HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.SESSION].m_name + @"]"
                        + @" WHERE [ID_CALCULATE]=" + _Session.m_Id;

                    DbTSQLInterface.ExecNonQuery(ref _dbConnection, strQuery, null, null, out err);
                } else
                    ;

                if (!(iRegDbConn > 0)) {
                    UnRegisterDbConnection();
                } else
                    ;
            } else
                ;
            // очистить сессию
            if (err == 0)
                _Session.m_Id = -1;
            else
                ;
        }
        /// <summary>
        /// Обновить значения во временой таблице
        /// </summary>
        /// <param name="indxDbTable">Индекс таблицы в списке таблиц БД</param>
        /// <param name="tableOriginValues">Таблица с исходными значениями</param>
        /// <param name="tableEditValues">Таблица с измененными значениями</param>
        /// <param name="err">Идентификатор ошибки при выполнении операции</param>
        public void UpdateSession(ID_DBTABLE idDbTable
            , DataTable tableOriginValues
            , DataTable tableEditValues
            , out int err)
        {
            err = -1;

            RecUpdateInsertDelete(s_dictDbTables[idDbTable].m_name, @"ID_PUT, ID_SESSION", string.Empty, tableOriginValues, tableEditValues, out err);
        }
        /// <summary>
        /// Условие выбора строки с парметрами сессии (панель, пользователь, идентификатор интервала, идентификатор часового пояса)
        /// </summary>
        protected virtual string whereQuerySession
        {
            get {
                return @"s.[ID_CALCULATE]=" + _Session.m_Id;
            }
        }
        /// <summary>
        /// Возвратить строку запроса для получения текущего идентификатора сессии расчета
        /// </summary>
        private string querySession
        {
            get {
                return string.Format(@"SELECT s.*, tz.[OFFSET_UTC] FROM [{0}] as s"
                    + @" JOIN [{1}] tz ON s.ID_TIMEZONE = tz.ID"
                    +
                        //@" WHERE [ID_USER]=" + HTepUsers.Id
                        @" WHERE {2}"
                    , s_dictDbTables[ID_DBTABLE.SESSION].m_name
                    , s_dictDbTables[ID_DBTABLE.TIMEZONE].m_name
                    , whereQuerySession);
            }
        }
        /// <summary>
        /// Возвратить строку запроса для получения
        /// </summary>
        /// <param name="idSession">Идентификатор сессии</param>
        /// <param name="type">Тип значений (входные, выходные-нормативы - только для ТЭП, выходные)</param>
        /// <returns>Строка - содержание запроса</returns>
        protected virtual string getQueryVariableValues(TaskCalculate.TYPE type)
        {
            string strRes = string.Empty
                , strJoinValues = string.Empty;

            if (!(type == TaskCalculate.TYPE.UNKNOWN)) {
                strJoinValues = getRangeAlg(type);
                if (strJoinValues.Equals(string.Empty) == false)
                    strJoinValues = @" JOIN [" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT AND p.ID_ALG" + strJoinValues;
                else
                    ;

                strRes = @"SELECT v.* FROM " + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @" as v"
                    + strJoinValues
                    + @" WHERE [ID_SESSION]=" + _Session.m_Id;
            } else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Возвратить диапазон идентификаторов (для WHERE на T-SQL) параметров в алгоритме расчета
        /// </summary>
        /// <param name="type">Тип расчета</param>
        /// <returns>Строка для WHERE с диапазоном идентификаторов</returns>
        private string getRangeAlg(TaskCalculate.TYPE type)
        {
            string strRes = string.Empty;

            ID_START_RECORD idRecStart = ID_START_RECORD.ALG
                , idRecEnd = ID_START_RECORD.PUT;

            switch (type) {
                case TaskCalculate.TYPE.IN_VALUES:
                    break;
                case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                case TaskCalculate.TYPE.OUT_VALUES:
                    idRecStart = type == TaskCalculate.TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                        type == TaskCalculate.TYPE.OUT_VALUES ? ID_START_RECORD.ALG :
                            ID_START_RECORD.ALG;
                    idRecEnd = type == TaskCalculate.TYPE.OUT_TEP_NORM_VALUES ? ID_START_RECORD.PUT :
                        type == TaskCalculate.TYPE.OUT_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                            ID_START_RECORD.PUT;

                    strRes = @" BETWEEN " + (int)(idRecStart - 1) + @" AND " + (int)(idRecEnd - 1);
                    break;
                default:
                    break;
            }

            return strRes;
        }
        /// <summary>
        /// Строка - условие для TSQL-запроса для указания диапазона идентификаторов
        ///  выходных параметров алгоритма расчета
        /// </summary>
        private string getWhereRangeAlg(TaskCalculate.TYPE type, string strNameFieldId = @"ID")
        {
            string strRes = string.Empty;

            switch (type) {
                case TaskCalculate.TYPE.IN_VALUES:
                    break;
                case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                case TaskCalculate.TYPE.OUT_VALUES:
                    strRes = @"[" + strNameFieldId + @"]" + getRangeAlg(type);
                    break;
                default:
                    break;
            }

            return strRes;
        }
        /// <summary>
        /// Возвратить строку запроса к БД для получения параметров по алгоритму расчета
        /// </summary>
        /// <param name="type">Тип расчета</param>
        /// <returns>Строка запроса</returns>
        protected virtual string getQueryParameters(TaskCalculate.TYPE type/* = TaskCalculate.TYPE.UNKNOWN*/)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty;

            //if (type == TaskCalculate.TYPE.UNKNOWN)
            //    type = m_taskCalculate.Type;
            //else
            //    ;

            if (!(type == TaskCalculate.TYPE.UNKNOWN)) {
                // аналог в 'getQueryValuesVar'
                whereParameters = getWhereRangeAlg(type);
                if (whereParameters.Equals(string.Empty) == false)
                    whereParameters = @" AND a." + whereParameters;
                else
                    ;

                strRes = @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                        + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                        + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                    + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] as p"
                        + @" JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] as a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask
                        + whereParameters
                        + @" JOIN [dbo].[" + s_dictDbTables[ID_DBTABLE.MEASURE].m_name + @"] as m ON a.ID_MEASURE = m.ID ORDER BY ID";
            } else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::GetQueryParameters () - неизвестный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Возвратить наименование таблицы 
        /// </summary>
        /// <param name="type">Тип панели/расчета</param>
        /// <param name="req">Индекс таблицы, требуемой при расчете</param>
        /// <returns>Наименование таблицы</returns>
        protected static string getNameDbTable(TaskCalculate.TYPE type, TABLE_CALCULATE_REQUIRED req)
        {
            ID_DBTABLE id = ID_DBTABLE.UNKNOWN;

            id = TaskCalculate.GetIdDbTable(type, req);

            return s_dictDbTables[id].m_name;
        }
        /// <summary>
        /// Список значений, загруженных из БД
        /// </summary>
        protected Dictionary<KEY_VALUES, List<VALUES>> _dictValues;
        /// <summary>
        /// Список значений, загруженных из БД
        /// </summary>
        public Dictionary<KEY_VALUES, List<VALUES>> Values { get { return _dictValues; } }
        #region Добавление компонентов, параметров в алгоритме расчета
        /// <summary>
        /// Список параметров алгоритма расчета, не связанных с компонентом станции (верхний/1-ый уровень)
        /// </summary>
        private List<HandlerDbTaskCalculate.NALG_PARAMETER> _listNAlgParameter;
        /// <summary>
        /// Список параметров алгоритма расчета, не связанных с компонентом станции (верхний/1-ый уровень)
        /// </summary>
        public List<HandlerDbTaskCalculate.NALG_PARAMETER> ListNAlgParameter { get { return _listNAlgParameter; } }
        /// <summary>
        /// Список компонентов станции
        /// </summary>
        private List<HandlerDbTaskCalculate.TECComponent> _listTECComponent;
        /// <summary>
        /// Список компонентов станции
        /// </summary>
        public List<HandlerDbTaskCalculate.TECComponent> ListTECComponent { get { return _listTECComponent; } }
        /// <summary>
        /// Список параметров алгоритма расчета, связанных с компонентом станции (нижний/2-ой уровень)
        /// </summary>
        private List<HandlerDbTaskCalculate.PUT_PARAMETER> _listPutParameter;
        /// <summary>
        /// Список параметров алгоритма расчета, связанных с компонентом станции (нижний/2-ой уровень)
        /// </summary>
        public List<HandlerDbTaskCalculate.PUT_PARAMETER> ListPutParameter { get { return _listPutParameter; } }
        /// <summary>
        /// Событие для добавления основного параметра для панели управления
        /// </summary>
        public event Action<NALG_PARAMETER> EventAddNAlgParameter;
        /// <summary>
        /// Событие для добавления детализированного (компонент) параметра для панели управления
        /// </summary>
        public event Action<PUT_PARAMETER> EventAddPutParameter;
        /// <summary>
        /// Событие при добавлении компонента(оборудования) станции
        /// </summary>
        public event Action<TECComponent> EventAddComponent;
        /// <summary>
        /// Получить идентификатор родительского объекта для компонента (оборудования)
        /// </summary>
        /// <param name="id_comp">Идентификатор компонента</param>
        /// <param name="id_comp_owner">Результат - идентификатор родительского объекта</param>
        /// <returns>Признак успеха/ошибка порлучения результата</returns>
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
        public void AddComponents(HTepUsers.DictionaryProfileItem profile)
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
                        if (int.TryParse(profile.GetAttribute(_Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.ENABLED_ITEM), out enabled) == false)
                            enabled = -1;
                        else
                            ;
                        bEnabled = !(enabled < 0) ? enabled == 0 ? false : enabled == 1 ? true : true : true;

                        if (int.TryParse(profile.GetAttribute(_Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.VISIBLED_ITEM), out visibled) == false)
                            visibled = -1;
                        else
                            ;
                        bVisibled = !(visibled < 0) ? visibled == 0 ? false : visibled == 1 ? true : true : true;

                        EventAddComponent(new TECComponent(id_comp
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
        public void AddAlgParameters(int idFPanel, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, HTepUsers.DictionaryProfileItem profile)
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

            dictVisualSettings = GetParameterVisualSettings(new int[] { idFPanel, (int)_Session.CurrentIdPeriod }, out err);

            //Список параметров для отображения
            IEnumerable<DataRow> listParameter =
                // в каждой строке значения полей, относящихся к параметру алгоритма расчета одинаковые, т.к. 'ListParameter' объединение 2-х таблиц
                //ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First()) // исключить дублирование по полю [ID_ALG]
                type == HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select().Select(x => x)
                    : type == HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES ? m_dictTableDictPrj[ID_DBTABLE.OUT_PARAMETER].Select().Select(x => x)
                        : new List<DataRow>()
                            ;

            //Заполнить элементы управления с компонентами станции 
            foreach (DataRow r in listParameter) {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();

                if (int.TryParse(profile.GetAttribute(_Session.CurrentIdPeriod, n_alg, HTepUsers.ID_ALLOWED.ENABLED_ITEM), out enabled) == false)
                    enabled = -1;
                else
                    ;
                bEnabled = !(enabled < 0) ? enabled == 0 ? false : enabled == 1 ? true : true : true;

                if (int.TryParse(profile.GetAttribute(_Session.CurrentIdPeriod, n_alg, HTepUsers.ID_ALLOWED.VISIBLED_ITEM), out visibled) == false)
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

                    EventAddNAlgParameter(new NALG_PARAMETER(
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
                        EventAddPutParameter?.Invoke(new PUT_PARAMETER() {
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
        /// Добавить таблицу в словарь со словарно-проектными значениями
        /// </summary>
        /// <param name="id">Идентификатор таблицы БД</param>
        /// <param name="err">Признак ошибки при чтении данных таблицы из БД</param>
        sealed public override void AddTableDictPrj(ID_DBTABLE id, out int err)
        {
            switch (id) {
                case ID_DBTABLE.IN_PARAMETER:
                    addTableDictPrj(id
                        , Select(getQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES), out err));
                    break;
                case ID_DBTABLE.OUT_PARAMETER:
                    addTableDictPrj(id
                        , Select(getQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES), out err));
                    break;
                default:
                    base.AddTableDictPrj(id, out err);
                    break;
            }
        }
        ///// <summary>
        ///// Возвратить наименование таблицы 
        ///// </summary>
        ///// <param name="req">Индекс таблицы, требуемой при расчете</param>
        ///// <returns>Наименование таблицы</returns>
        //private string getNameDbTable(TABLE_CALCULATE_REQUIRED req)
        //{
        //    return getNameDbTable(m_taskCalculate.Type, req);
        //}
        /// <summary>
        /// Возвратить массив диапазонов даты/времени для запроса значений
        /// </summary>
        /// <returns>Массив диапазонов даты/времени</returns>
        protected virtual DateTimeRange[] getDateTimeRangeVariableValues()
        {
            DateTimeRange[] arRangesRes = null;

            int i = -1;
            bool bEndMonthBoudary = false;
            // привести дату/время к UTC
            DateTime dtBegin = _Session.m_DatetimeRange.Begin.AddMinutes(-1 * _Session.m_curOffsetUTC.TotalMinutes)
                , dtEnd = _Session.m_DatetimeRange.End.AddMinutes(-1 * _Session.m_curOffsetUTC.TotalMinutes);

            if (_modeDataDateTime == MODE_DATA_DATETIME.Begined) {
                dtBegin -= TimeSpan.FromDays(1);
                dtEnd -= TimeSpan.FromDays(1);
            } else
                ;

            arRangesRes = new DateTimeRange[(dtEnd.Month - dtBegin.Month) + 12 * (dtEnd.Year - dtBegin.Year) + 1];
            bEndMonthBoudary = HDateTime.IsMonthBoundary(dtEnd);
            if (bEndMonthBoudary == false)
                if (arRangesRes.Length == 1)
                    // самый простой вариант - один элемент в массиве - одна таблица
                    arRangesRes[0] = new DateTimeRange(dtBegin, dtEnd);
                else
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц
                    for (i = 0; i < arRangesRes.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                        else
                            if (i == arRangesRes.Length - 1)
                            // крайний элемент массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                        else
                            // для элементов в "середине" массива
                            arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
            else
                if (bEndMonthBoudary == true)
                // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц ('diffMonth' всегда > 0)
                // + использование следующей за 'dtEnd' таблицы
                for (i = 0; i < arRangesRes.Length; i++)
                    if (i == 0)
                        // предыдущих значений нет
                        arRangesRes[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                    else
                        if (i == arRangesRes.Length - 1)
                        // крайний элемент массива
                        arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, dtEnd);
                    else
                        // для элементов в "середине" массива
                        arRangesRes[i] = new DateTimeRange(arRangesRes[i - 1].End, HDateTime.ToNextMonthBoundary(arRangesRes[i - 1].End));
            else
                ;

            return arRangesRes;
        }
        /// <summary>
        /// Запрос для получения значений "по умолчанию"
        /// </summary>
        private string getQueryDefaultValues(ID_PERIOD idPeriod)
        {
            string strRes = string.Empty;

            strRes = @"SELECT"
                + @" *"
                + @" FROM [dbo].[" + HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVAL_DEF].m_name + @"] v"
                + @" WHERE [ID_TIME] = " + (int)idPeriod //(int)_currIdPeriod
                    ;

            return strRes;
        }

        public enum MODE_AGREGATE_GETVALUES { OFF, ON }

        public MODE_AGREGATE_GETVALUES ModeAgregateGetValues;
        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        ///  , структура таблицы совместима с [inval], [outval]
        /// </summary>
        protected virtual string getQueryVariableValues(TaskCalculate.TYPE type
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges)
        {
            string strRes = string.Empty
                , whereParameters = string.Empty
                , subQuery = string.Empty
                , partDateadd = string.Empty;

            switch (idPeriod) {
                case ID_PERIOD.YEAR:
                    partDateadd = @"YEAR";
                    break;
                case ID_PERIOD.MONTH:
                    partDateadd = @"MONTH";
                    break;
                case ID_PERIOD.DAY:
                    partDateadd = @"DAY";
                    break;
                case ID_PERIOD.HOUR: //???
                    partDateadd = @"HOUR";
                    break;
                default:
                    break;
            }

            if (!(type == TaskCalculate.TYPE.UNKNOWN)) {
                // аналог в 'GetQueryParameters'
                whereParameters = getWhereRangeAlg(type);
                if (whereParameters.Equals(string.Empty) == false)
                    whereParameters = @" AND a." + whereParameters;
                else
                    ;

                int i = -1;
                bool bLastItem = false
                    , bEquDatetime = false;

                for (i = 0; i < arQueryRanges.Length; i++) {
                    bLastItem = !(i < (arQueryRanges.Length - 1));

                    subQuery += string.Format(@"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
                            + @"{0}"
                            + @", v.[WR_DATETIME]"
                            + @", CONVERT(varchar, {1}, 127)" + @" as [EXTENDED_DEFINITION]"
                        + @" FROM [dbo].[{2}] v"
                            + @" LEFT JOIN [dbo].[{3}] p ON p.ID = v.ID_PUT"
                            + @" RIGHT JOIN [dbo].[{4}] a ON a.ID = p.ID_ALG AND a.ID_TASK = {5}"
                            + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                        + @" WHERE v.[ID_TIME] = {6}" //???ID_PERIOD.HOUR //??? _currIdPeriod
                            , (ModeAgregateGetValues == MODE_AGREGATE_GETVALUES.ON ? @", m.[AVG]" : ModeAgregateGetValues == MODE_AGREGATE_GETVALUES.OFF ? string.Empty : string.Empty)
                            , (_modeDataDateTime == MODE_DATA_DATETIME.Begined) ? string.Format(@"DATEADD({0}, 1, v.[DATE_TIME])", partDateadd)
                                    : (_modeDataDateTime == MODE_DATA_DATETIME.Ended) ? @"v.[DATE_TIME]"
                                        : string.Empty
                            , string.Format(@"{0}_{1}", getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE), arQueryRanges[i].Begin.ToString(@"yyyyMM"))
                            , getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT)
                            , getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG)
                            , (int)_iIdTask + whereParameters
                            , (int)idPeriod
                        );
                    // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                    // 'Begin' == 'End'
                    if (bLastItem == true)
                        bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);
                    else
                        ;

                    if (bEquDatetime == false)
                        subQuery += string.Format(@" AND [DATE_TIME] > '{0:yyyyMMdd HH:mm:ss}' AND [DATE_TIME] <= '{1:yyyyMMdd HH:mm:ss}'"
                            , arQueryRanges[i].Begin, arQueryRanges[i].End);
                    else
                        subQuery += string.Format(@" AND [DATE_TIME] = '{0:yyyyMMdd HH:mm:ss}'", arQueryRanges[i].Begin);

                    if (bLastItem == false)
                        subQuery += @" UNION ALL ";
                    else
                        ;
                }

                strRes = @"SELECT v.ID_PUT" // as [ID]"
                        + @", " + _Session.m_Id + @" as [ID_SESSION]";
                if (ModeAgregateGetValues == MODE_AGREGATE_GETVALUES.ON)
                    strRes += @", CASE"
                            + @" WHEN COUNT (*) = " + cntBasePeriod + @" THEN MIN(v.[QUALITY])"
                            + @" WHEN COUNT (*) = 0 THEN " + (int)ID_QUALITY_VALUE.NOT_REC
                                + @" ELSE " + (int)ID_QUALITY_VALUE.PARTIAL
                            + @" END as [QUALITY]"
                        + @", CASE"
                            + @" WHEN v.[AVG] = 0 THEN SUM (v.[VALUE])"
                            + @" WHEN v.[AVG] = 1 THEN AVG (v.[VALUE])"
                                + @" ELSE MIN (v.[VALUE])"
                            + @" END as [VALUE]";
                else
                    strRes += @", v.[QUALITY] as [QUALITY]"
                        + @", v.[VALUE] as [VALUE]";

                strRes += @", v.[WR_DATETIME] as [WR_DATETIME]"
                           + @", [EXTENDED_DEFINITION]"
                    + @" FROM (" + subQuery + @") as v";

                if (ModeAgregateGetValues == MODE_AGREGATE_GETVALUES.ON)
                    strRes += @" GROUP BY v.ID_PUT"
                        + @", v.[AVG], v.[EXTENDED_DEFINITION]";
                else
                    ;
            } else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значенями по умолчанию
        /// </summary>
        /// <param name="idPeriod">Идентификатор </param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Объект-таблица со значенями по умолчанию</returns>
        protected virtual DataTable getDefaultValues(ID_PERIOD idPeriod, out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection, getQueryDefaultValues(idPeriod), null, null, out err);

            return tableRes;
        }

        /// <summary>
        /// Возвратить объект-таблицу со значениями из таблицы с временными для расчета
        /// </summary>
        /// <param name="type">Тип значений (входные, выходные)</param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Объект-таблица</returns>
        protected DataTable getVariableTableValues(TaskCalculate.TYPE type
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryVariableValues(type)
                , null, null
                , out err);

            return tableRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значениями из таблицы с сохраняемыми значениями из источников информации
        /// </summary>
        /// <param name="idPeriod">Идентификатор расчета (HOUR, DAY, MONTH, YEAR)</param>
        /// <param name="cntBasePeriod">Количество периодов расчета в интервале запрашиваемых данных</param>
        /// <param name="arQueryRanges">Массив диапазонов даты/времени - интервал(ы) заправшиваемых данных</param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Таблица со значенями</returns>
        protected virtual DataTable getArchiveTableValues(TaskCalculate.TYPE type
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            err = 0;

            DataTable tableRes = new DataTable();

            return tableRes;
        }
        /// <summary>
        /// Возвратить объект-таблицу со значениями из таблицы с сохраняемыми значениями из источников информации
        /// </summary>
        /// <param name="idPeriod">Идентификатор расчета (HOUR, DAY, MONTH, YEAR)</param>
        /// <param name="cntBasePeriod">Количество периодов расчета в интервале запрашиваемых данных</param>
        /// <param name="arQueryRanges">Массив диапазонов даты/времени - интервал(ы) заправшиваемых данных</param>
        /// <param name="err">Признак выполнения функции</param>
        /// <returns>Таблица со значенями</returns>
        protected virtual DataTable getVariableTableValues(TaskCalculate.TYPE type
            , ID_PERIOD idPeriod
            , int cntBasePeriod
            , DateTimeRange[] arQueryRanges
            , out int err)
        {
            DataTable tableRes = new DataTable();

            err = -1;

            tableRes = DbTSQLInterface.Select(ref _dbConnection
                , getQueryVariableValues(type
                    , idPeriod
                    , cntBasePeriod
                    , arQueryRanges)
                , null, null
                , out err);

            return tableRes;
        }

        public abstract DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err);
        /// <summary>
        /// Возвратить объект сессии расчета
        /// </summary>
        /// <param name="err">Признак выполнении функции</param>
        /// <returns>Сессия расчета</returns>
        public void InitSession(out int err)
        {
            err = -1;

            DataTable tableSession = null;
            DataRow rowSession = null;
            int iRegDbConn = -1
                , iCntSession = -1;

            _Session.m_Id = -1;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0)) {
                // прочитать параметры сессии для текущего пользователя
                tableSession = DbTSQLInterface.Select(ref _dbConnection, querySession, null, null, out err);//??ID_PANEL
                // получить количество зарегистрированных сессий для пользователя
                iCntSession = tableSession.Rows.Count;

                if ((err == 0)
                    && (iCntSession == 1)) {
                    rowSession = tableSession.Rows[0];
                    _Session.Initialize(rowSession);
                } else
                    if (err == 0)
                    switch (iCntSession) {
                        case 0:
                            err = -101;
                            break;
                        default:
                            err = -102;
                            break;
                    } else
                    ; // ошибка получения параметров сессии
            } else
                ;

            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }
        /// <summary>
        /// Тип делегата для передачи аргумента в конструктор сессии
        /// , сессии делегат требуется для определения смещения меток времени от UTC при изменении пользователем час./пояса
        /// </summary>
        /// <param name="id">Идентификатор часового пояса</param>
        /// <returns>Смещение относительно UTC</returns>
        public delegate TimeSpan TimeSpanDelegateIdTimezoneFunc(ID_TIMEZONE id);

        public static List<VALUES> TableToListValues(DataTable table)
        {
            List<VALUES> listRes = new List<VALUES>();

            DateTime dtValue;

            //listRes = (from r in arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Rows.Cast<DataRow>()
            //    select new VALUES() {
            //        m_IdPut = int.Parse(r[@"ID_PUT"].ToString())
            //        , m_iQuality = int.Parse(r[@"QUALITY"].ToString())
            //        , value = float.Parse(r[@"VALUE"].ToString())
            //        , stamp = (DateTime)r[@"WR_DATETIME"]
            //    }).ToList();
            foreach (DataRow r in table.Rows) {
                if ((!(r[@"DATE_TIME"] is DBNull))
                    && (DateTime.TryParse((string)r[@"DATE_TIME"], out dtValue) == true))
                    ;
                else
                    dtValue = DateTime.MinValue;

                listRes.Add(new VALUES() { m_IdPut = int.Parse(r[@"ID_PUT"].ToString())
                    , m_iQuality = int.Parse(r[@"QUALITY"].ToString())
                    , value = float.Parse(r[@"VALUE"].ToString())
                    , stamp_value = dtValue
                    , stamp_write = !(r[@"WR_DATETIME"] is DBNull) ? (DateTime)r[@"WR_DATETIME"] : DateTime.MinValue
                });
            }

            return listRes;
        }

        public virtual DataTable GetTableValues(int idFPanel, TaskCalculate.TYPE type, out int err, out string strErr)
        {
            err = -1;
            strErr = string.Empty;

            //List<VALUES> listRes = new List<VALUES>();
            DataTable tableRes = null;

            DateTimeRange[] arQueryRanges = getDateTimeRangeVariableValues();
            DataTable tablePars = null;
            DataTable[] arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            //List<VALUES>[] arValuesOrigin = new List<VALUES>[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            tablePars = (type == TaskCalculate.TYPE.IN_VALUES) ? m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER] :
                ((type == TaskCalculate.TYPE.OUT_VALUES) || (type == TaskCalculate.TYPE.OUT_TEP_NORM_VALUES)) ? m_dictTableDictPrj[ID_DBTABLE.OUT_PARAMETER] :
                    new DataTable(); // пустая таблица с параметрами

            //Запрос для получения архивных данных
            arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = getArchiveTableValues(type, _Session.ActualIdPeriod, _Session.CountBasePeriod, arQueryRanges, out err);
            //arValuesOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = getValues(arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]);
            //Запрос для получения автоматически собираемых данных
            arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = _Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD ?
                getVariableTableValues(type, _Session.ActualIdPeriod, _Session.CountBasePeriod, arQueryRanges, out err) :
                    _Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_IMPORT ? GetImportTableValues(type
                        , _Session.m_Id
                        , tablePars
                        , m_dictTableDictPrj[ID_DBTABLE.RATIO]
                        , out err) :
                            new DataTable();
            //arValuesOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = getValues(arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]);
            //Проверить признак выполнения запроса
            if (err == 0) {
                //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = getDefaultValues(_Session.ActualIdPeriod, out err);
                //arValuesOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = getValues(arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]);
                //Проверить признак выполнения запроса
                if (err == 0) {
                    // получить результирующаю таблицу
                    tableRes = mergeTableValues(tablePars, arTableOrigin, _Session.CountBasePeriod);

                } else
                    strErr = @"ошибка получения данных по умолчанию с " + _Session.m_DatetimeRange.Begin.ToString()
                        + @" по " + _Session.m_DatetimeRange.End.ToString();
            } else
                strErr = @"ошибка получения автоматически собираемых данных с " + _Session.m_DatetimeRange.Begin.ToString()
                    + @" по " + _Session.m_DatetimeRange.End.ToString();

            if (tableRes == null)
                tableRes = new DataTable();
            else
                ;

            return
                //listRes
                tableRes
                ;
        }

        private TimeSpan getOffsetUTC(ID_TIMEZONE id)
        {
            int err = -1;

            TimeSpan tsRes = TimeSpan.Zero;

            int iRegDbConn = -1;

            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0)) {
                tsRes = TimeSpan.FromMinutes(
                    (int)DbTSQLInterface.Select(ref _dbConnection
                        , string.Format(@"SELECT [OFFSET_UTC] FROM [{0}] WHERE [ID]={1}", s_dictDbTables[ID_DBTABLE.TIMEZONE].m_name, (int)id)
                        , null, null, out err)
                        .Rows[0][0]
                );
            } else
                ;

            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;

            return tsRes;
        }
        /// <summary>
        /// Подготовить таблицы для проведения расчета
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении функции</param>
        /// <returns>Массив таблиц со значенями для расчета</returns>
        protected abstract TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err);

        //protected virtual void correctValues(ref DataTable tableValues, ref DataTable tablePars) { }

        protected abstract void calculate(TaskCalculate.TYPE type, out DataTable tableOrigin, out DataTable tableCalc, out int err);
        /// <summary>
        /// Расчитать выходные-нормативные значения для задачи (например, "Расчет ТЭП")
        ///  , сохранить значения во временной таблице для возможности предварительного просмотра результата
        /// </summary>
        public void Calculate(TaskCalculate.TYPE type)
        {
            int err = -1
                , iRegDbConn = -1;

            DataTable tableOrigin
                , tableCalcRes;

            // регистрация соединения с БД
            RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0)) {
                switch (IdTask) {
                    case ID_TASK.TEP:
                    //case ID_TASK.REAKTIVKA:// для этой задачи нет вычислений
                    case ID_TASK.AUTOBOOK:
                    case ID_TASK.BAL_TEPLO: //Для работы с балансом тепла 6,06,2016 Апельганс
                        calculate(type, out tableOrigin, out tableCalcRes, out err);
                        if (!(err == 0))
                            Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - ошибка при выполнеии расчета задачи ID=" + IdTask.ToString() + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
                        else
                        // сохранить результаты вычисления
                            saveResult(tableOrigin, tableCalcRes, out err);
                        break;
                    default:
                        Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - неизвестный тип задачи расчета...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
            } else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при регистрации соединения...", Logging.INDEX_MESSAGE.NOT_SET);

            // отмена регистрации БД - только, если регистрация произведена в текущем контексте
            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;
        }
        /// <summary>
        /// Сохранить результаты вычислений в таблице для временных значений
        /// </summary>
        /// <param name="tableOrigin">??? Таблица с оригинальными значениями</param>
        /// <param name="tableRes">??? Таблица с оригинальными значениями</param>
        /// <param name="err">Признак выполнения операции сохранения</param>
        protected void saveResult(DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = -1;

            DataTable tableEdit = new DataTable();
            DataRow[] rowSel = null;

            tableEdit = tableOrigin.Clone();

            foreach (DataRow r in tableOrigin.Rows) {
                rowSel = tableRes.Select(@"ID=" + r[@"ID_PUT"]);

                if (rowSel.Length == 1) {
                    tableEdit.Rows.Add(new object[] {
                        //r[@"ID"],
                        r[@"ID_SESSION"]
                        , r[@"ID_PUT"]
                        , rowSel[0][@"QUALITY"]
                        , rowSel[0][@"VALUE"]
                        , HDateTime.ToMoscowTimeZone ().ToString (CultureInfo.InvariantCulture)
                    });
                } else
                    ; //??? ошибка
            }

            RecUpdateInsertDelete(s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name, @"ID_PUT", string.Empty, tableOrigin, tableEdit, out err);
        }

        public Dictionary<string, HTepUsers.VISUAL_SETTING> GetParameterVisualSettings(int[] fields, out int err)
        {
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictRes;

            int iRegDbConn = -1;

            RegisterDbConnection(out iRegDbConn);

            dictRes = HTepUsers.GetParameterVisualSettings(_dbConnection, fields, out err);

            if (!(iRegDbConn > 0))
                UnRegisterDbConnection();
            else
                ;

            return dictRes;
        }
    }
}
