﻿using System;
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

namespace TepCommon
{
    public abstract partial class HPanelCommon : HClassLibrary.HPanelCommon, IObjectDbEdit
    {
        /// <summary>
        /// Дополнительные действия при сохранении значений
        /// </summary>
        protected DelegateFunc delegateSaveAdding;
        /// <summary>
        /// Объект для реализации взаимодействия с главной программой
        /// </summary>
        protected IPlugIn _iFuncPlugin;
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE TaskCalculateType;
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;

        protected class DictionaryTableDictProject : Dictionary<ID_DBTABLE, DataTable>
        {
            public enum Error {
                ExFilterBuilder = -12
                , ExRowRemove
                , IdDbTableUnknown = -2
                , Any = -1
                , Not = 0
                , WFilterNoApplied }

            public class TSQLWhereItem
            {
                [Flags]
                public enum RULE {
                    NotSet
                    , Equale
                    , NotEquale
                    , Above
                    , Below
                }

                private string _nameField;

                private object _limit;

                public string Value { get; }

                public RULE Rules;

                public int Ready { get; }

                public TSQLWhereItem(string nameField, object limit, RULE rules)
                {
                    _nameField = nameField;
                    _limit = limit;
                    Rules = rules;

                    Ready = 0;

                    foreach (RULE rule in Enum.GetValues(typeof(RULE)))
                        if ((rule & Rules) == rule)
                            switch (rule) {
                                case RULE.Equale:
                                    Value = string.Format(@"{0}<{1} OR {0}>{1}", _nameField, _limit);
                                    break;
                                default:
                                    break;
                            }
                        else
                            ;
                }
            }            

            public class ListTSQLWhere : List<TSQLWhereItem>//, IDisposable
            {
                public ID_DBTABLE IdDbTable;

                public ListTSQLWhere(ID_DBTABLE idDbTable)
                {
                    IdDbTable = idDbTable;
                }

                public string Value
                {
                    get {
                        string strRes = string.Empty;

                        foreach (TSQLWhereItem item in this) {
                            if (item.Ready == 0) {
                                if (this.IndexOf(item) == 0)
                                    if (Count == 1)
                                        strRes = item.Value;
                                    else
                                        strRes = string.Format(@"({0})", item.Value);
                                else
                                    strRes += string.Format(@" AND ({0})", item.Value);
                            } else
                                ;
                        }

                        return strRes;
                    }
                }

                //public void Dispose()
                //{
                //}
            }

            public DictionaryTableDictProject ()
            {
                _filterDbTableCompList = DbTableCompList.NotSet;
                _filterDbTableTime = DbTableTime.NotSet;
                _filterDbTableTimezone = DbTableTimezone.NotSet;
            }

            #region DbTableCompList
            [Flags]
            public enum DbTableCompList {
                NotSet = 0x0
                , Tg = 0x1
                , Tec = 0x2
            }

            DbTableCompList _filterDbTableCompList;

            public DbTableCompList FilterDbTableCompList
            {
                get { return _filterDbTableCompList; }

                set {
                    _filterDbTableCompList = value;
                    //??? не учитывается
                    Error iRes = Error.Any; // ошибка

                    ID_DBTABLE idDbTable = ID_DBTABLE.COMP_LIST;
                    ListTSQLWhere listTSQLWhere = new ListTSQLWhere(idDbTable);

                    try {
                        foreach (DbTableCompList item in Enum.GetValues(typeof(DbTableCompList))) {
                            if ((_filterDbTableCompList & item) == item)
                                switch (item) {
                                    case DbTableCompList.Tg:
                                        listTSQLWhere.Add(new TSQLWhereItem(@"ID_COMP", (int)ID_COMP.TG, TSQLWhereItem.RULE.Equale));
                                        break;
                                    case DbTableCompList.Tec:
                                        listTSQLWhere.Add(new TSQLWhereItem(@"ID_COMP", (int)ID_COMP.TEC, TSQLWhereItem.RULE.Equale));
                                        break;
                                    default:
                                        break;
                                }
                            else
                                ;
                        }

                        iRes = setDbTableFilter(listTSQLWhere);
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                            , _filterDbTableCompList, idDbTable)
                                , Logging.INDEX_MESSAGE.NOT_SET);

                        iRes = Error.ExFilterBuilder; // исключение при поиске строк для удаления
                    }
                }
            }
            #endregion

            #region DbTableTimezone
            [Flags]
            public enum DbTableTimezone {
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
                    Error iRes = Error.Any; // ошибка

                    ID_DBTABLE idDbTable = ID_DBTABLE.TIMEZONE;
                    ListTSQLWhere listTSQLWhere = new ListTSQLWhere(idDbTable);
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
                                }
                            else
                                ;

                            if (!(idTimezone == ID_TIMEZONE.UNKNOWN))
                                listTSQLWhere.Add(new TSQLWhereItem(@"ID", (int)idTimezone, TSQLWhereItem.RULE.Equale));
                            else
                                ;
                        }

                        iRes = setDbTableFilter(listTSQLWhere);
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                            , _filterDbTableCompList, idDbTable)
                                , Logging.INDEX_MESSAGE.NOT_SET);

                        iRes = Error.ExFilterBuilder; // исключение при поиске строк для удаления
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
                    Error iRes = Error.Any; // ошибка

                    ID_DBTABLE idDbTable = ID_DBTABLE.TIME;
                    ListTSQLWhere listTSQLWhere = new ListTSQLWhere(idDbTable);
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
                                }
                            else
                                ;

                            if (!(idPeriod == ID_PERIOD.UNKNOWN))
                                listTSQLWhere.Add(new TSQLWhereItem(@"ID", (int)idPeriod, TSQLWhereItem.RULE.Equale));
                            else
                                ;
                        }

                        iRes = setDbTableFilter(listTSQLWhere);
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, string.Format(@"FilterDbTableCompList.set (DbFilter={0}) - ID_DBTABLE={1}..."
                            , _filterDbTableCompList, idDbTable)
                                , Logging.INDEX_MESSAGE.NOT_SET);

                        iRes = Error.ExFilterBuilder; // исключение при поиске строк для удаления
                    }
                }
            }

            private void setDbTableFilter(DbTableTime filterDbTableTime)
            {
            }
            #endregion

            protected virtual Error setDbTableFilter(ListTSQLWhere where)
            {
                Error iRes = Error.Not; // ошибка

                ID_DBTABLE idDbTable = where.IdDbTable;
                string filter = where.Value;
                List<DataRow> rowsToDelete = new List<DataRow>();

                try {
                    if ((!(idDbTable == ID_DBTABLE.UNKNOWN))
                        && (string.IsNullOrEmpty(filter) == false))
                        if (ContainsKey(idDbTable) == true) {
                            rowsToDelete = this[idDbTable].Select(filter).ToList();

                            iRes = Error.Not; // Ok
                        } else
                            iRes = Error.IdDbTableUnknown;
                    else
                        iRes = Error.WFilterNoApplied; // фильтр не применен - не найден обработчик
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"SetDbTableFilter (DbFilter={0}, idDbTable={1})..."
                        , where.ToString(), idDbTable)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }

                try {
                    if (iRes == Error.Not) {
                        foreach (DataRow row in rowsToDelete)
                            this[idDbTable].Rows.Remove(row);
                    } else
                        ;

                    this[idDbTable].AcceptChanges();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"SetDbTableFilter (DbFilter={0}, idDbTable={1}) - строк для удаления={2}..."
                        , where.ToString(), idDbTable, rowsToDelete.Count)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = Error.ExRowRemove; // исключение при удалении строк
                }

                return iRes;
            }
        }
        /// <summary>
        /// Словарь с таблицами словарно-проектных значений
        /// </summary>
        protected DictionaryTableDictProject m_dictTableDictPrj;

        protected virtual void initialize(ID_DBTABLE[] arIdTableDictPrj, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            if ((!(m_dictTableDictPrj == null))
                && (m_dictTableDictPrj.Count > 0)) {
                Logging.Logg().Warning(@"HPanelTepCommon::initialize () - словарно-проектные таблицы повторная инициализация...", Logging.INDEX_MESSAGE.NOT_SET);

                m_dictTableDictPrj.Clear();
            } else
                m_dictTableDictPrj = new DictionaryTableDictProject();

            foreach (ID_DBTABLE id in /*Enum.GetValues(typeof(ID_DBTABLE))*/arIdTableDictPrj) {
                switch (id) {
                    case ID_DBTABLE.IN_PARAMETER:
                        m_dictTableDictPrj.Add(id
                            , m_handlerDb.Select((m_handlerDb as HandlerDbTaskCalculate).GetQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES), out err));
                        break;
                    case ID_DBTABLE.OUT_PARAMETER:
                        m_dictTableDictPrj.Add(id
                            , m_handlerDb.Select((m_handlerDb as HandlerDbTaskCalculate).GetQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES), out err));
                        break;
                    default:
                        m_dictTableDictPrj.Add(id, m_handlerDb.GetDataTable(id, out err));
                        break;
                }

                if (err < 0) {
                    // ошибка
                    switch (err) {
                        case -3: // наименовавние таблицы
                            errMsg = @"неизвестное наименовнаие таблицы";
                            break;
                        case -2: // неизвестный тип
                            errMsg = @"неизвестный тип таблицы";
                            break;
                        case -1:
                        default:
                            errMsg = @"неопределенная ошибка";
                            break;
                    }

                    errMsg = string.Format(@"HPanelTepCommon::initialize (тип={0}) - {1}...", id, errMsg);

                    break;
                } else
                    if (err > 0)
                    // предупреждение
                        switch (err) {
                            case 1: // идентификатор указан прежде, чем его можно инициализировать объект для него
                                break;
                            default:
                                break;
                        }
                    else
                    // ошибок, предупреждений нет
                        ;
            }

            //??? обязательная таблица
            ID_DBTABLE idDbTable = ID_DBTABLE.COMP_VALUES;
            if (m_dictTableDictPrj.ContainsKey(idDbTable) == false) {
                m_dictTableDictPrj.Add(idDbTable, m_handlerDb.GetDataTable(idDbTable, out err));
            } else
                ;
        }
        /// <summary>
        /// Удалить сессию (+ очистить реквизиты сессии)
        /// </summary>
        protected virtual void deleteSession()
        {
            int err = -1;

            (m_handlerDb as HandlerDbTaskCalculate).DeleteSession(out err);
        }
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return (m_handlerDb as HandlerDbTaskCalculate)._Session; } }        
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected virtual void clear(bool bClose = false)
        {
            deleteSession();
            //??? повторная проверка
            if (bClose == true) {
                clearTableDictPrj();

                //_panelManagement.Clear(bClose);

                // ??? - д.б. общий метод полной очистки всех представлений
            }
            else
            //??? очистить содержание всех представлений - д.б. общий метод
                ;
        }

        #region Apelgans
        /// <summary>
        /// Идентификатор текущего объекта панели(класса) в соответствии с решистрацией
        /// </summary>
        protected int m_Id;

        public enum INDEX_DATATABLE_DESCRIPTION { TABLE, PROPERTIES };

        public DataTable[] Descriptions = new DataTable[] { new DataTable(), new DataTable() };

        protected HTepUsers.DictionaryProfileItem m_dictProfile;

        public enum ID_AREA
        {
            MAIN = 1 //Главная
            , PROP = 2 //Свойства
            , DESC = 3 //Описание
        };
        /// <summary>
        /// Список групп
        /// </summary>
        string[] m_arr_name_group_panel = { "Настройка", "Проект", "Задача" };
        /// <summary>
        /// Строки для описания групп вкладок
        /// </summary>
        string[] m_description_group = new string[] { "Группа для настроек", "Группа для проектов", "Группа для задач" };

        public string m_name_panel_desc = string.Empty;

        #endregion

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public HPanelCommon(IPlugIn plugIn)
            : base(13, 13)
        {
            this._iFuncPlugin = plugIn;

            ////Создать объект "словарь" дочерних элементов управления
            //m_dictControls = new Dictionary<int, Control>();

            InitializeComponent();

            m_Id = ID;

            m_handlerDb = createHandlerDb();

            m_dictProfile = new HTepUsers.DictionaryProfileItem();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            initializeLayoutStyle();
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
        /// Объект для обмена данными с БД
        /// </summary>
        protected HandlerDbValues m_handlerDb;        
        /// <summary>
        /// Найти идентификатор типа текущей панели
        ///  , зарегистрированного в библиотеке
        /// </summary>
        /// <returns>Идентификатор типа панели</returns>
        private int ID
        {
            get {
                int iRes = -1;
                Type thisType = Type.Missing as Type;

                thisType = this.GetType();

                //Вариант №1
                KeyValuePair<int, Type>? pairRes = null;

                pairRes = (_iFuncPlugin as PlugInBase).GetRegisterTypes().First(item => { return item.Value == thisType; });

                if (!(pairRes == null))
                    iRes = pairRes.GetValueOrDefault().Key;
                else
                    ;

                ////Вариант №2
                //Dictionary<int, Type> dictRegId = (_iFuncPlugin as PlugInBase).GetRegisterTypes();

                //foreach (var item in dictRegId)
                //{
                //    if (item.Value == myType)
                //    {
                //        iRes = item.Key;
                //    }
                //}

                return iRes;
            }
        }

        private void clearTableDictPrj()
        {
            foreach (ID_DBTABLE id in Enum.GetValues(typeof(ID_DBTABLE))) {

            }
        }

        protected void initializeDescPanel()
        {
            int err = -1;

            Control ctrl = null;
            string desc = string.Empty
                , name = string.Empty
                , query = string.Empty;
            string[] ar_name = null;
            DataTable table;
            DataRow[] rows = null;

            if (string.IsNullOrEmpty(m_name_panel_desc) == false) {
                try {
                    ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
                    name = ((PlugInMenuItem)_iFuncPlugin).GetNameOwnerMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id);
                    ar_name = name.Split('\\');
                    name = ar_name[0];

                    for (int i = 0; i < m_arr_name_group_panel.Length; i++)
                        if (m_arr_name_group_panel[i] == name)
                            ((HPanelDesc)ctrl).SetLblGroup = new string[] { name, m_description_group[i] };
                        else
                            ;

                    //Описание вкладки
                    query = "SELECT DESCRIPTION FROM [dbo].[fpanels] WHERE [ID]=" + m_Id;
                    table = m_handlerDb.Select(query, out err);
                    if (table.Rows.Count != 0) {
                        desc = table.Rows[0][0].ToString();
                        ((HPanelDesc)ctrl).SetLblTab = new string[] { /*((PlugInMenuItem)_iFuncPlugin).GetNameMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id)*/
                            this.Parent.Text, desc
                        };
                    } else
                        ;

                    //Описания таблиц
                    query = "SELECT * FROM [dbo].[table_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE] = m_handlerDb.Select(query, out err);

                    //Описания параметров
                    query = "SELECT * FROM [dbo].[param_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES] = m_handlerDb.Select(query, out err);

                    //Описания параметров
                    query = "SELECT * FROM [dbo].[param_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES] = m_handlerDb.Select(query, out err);

                    if (!(err == 0))
                        Logging.Logg().Error("TepCommon.HpanelTepCommon initializeDescPanel - Select выполнен с ошибкой: " + err, Logging.INDEX_MESSAGE.NOT_SET);
                    else
                        ;

                    if (!(Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Columns.IndexOf("ID_TABLE") < 0)) {
                        rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_AREA.MAIN);

                        if (rows.Length == 1)
                            Logging.Logg().Error("TepCommon.HpanelTepCommon initializeDescPanel - Select выполнен с ошибкой: " + err, Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;

                        //DataRow[] rows = null;
                        if (!(Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Columns.IndexOf("ID_TABLE=") < 0)) {
                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_AREA.MAIN);
                            if (rows.Length == 1)
                                ((HPanelDesc)ctrl).SetLblDGV1Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                            else
                                ;

                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_AREA.PROP);
                            if (rows.Length == 1)
                                ((HPanelDesc)ctrl).SetLblDGV2Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                            else
                                ;

                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_AREA.DESC);
                            if (rows.Length == 1) {
                                ((HPanelDesc)ctrl).SetLblDGV3Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                                ((HPanelDesc)ctrl).SetLblDGV3Desc_View = false;
                            } else
                                ;
                        } else
                            Logging.Logg().Error(@"HPanelTepCommon::initializeDescPanel () - в таблице [" + Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].TableName + @"] не найдено поле [ID_TABLE]"
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"HPanelTepCommon::initializeDescPanel () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        public void Start(object obj)
        {
            //try
            //{
            //    if (this.IsHandleCreated == true)
            //        //if (this.InvokeRequired == true)
            //        this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
            //    else
            //        ;
            //}
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, @"HPanelEdit::Initialize () - BeginInvoke (initialize) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //}

            Start();

            //m_handlerDb.InitConnectionSettings(((EventArgsDataHost)obj).par[0] as ConnectionSettings);
            m_handlerDb.InitConnectionSettings(obj as ConnectionSettings);
            
            //HTepUsers.HTepProfilesXml.UpdateProfile(m_handlerDb.ConnectionSettings);
            m_dictProfile = HTepUsers.HTepProfilesXml.GetProfileUserPanel(HTepUsers.Id, HTepUsers.Role, m_Id);

        }

        //public override void Stop()
        //{
        //    while (Controls.Count > 0)
        //        Controls.RemoveAt(0);

        //    base.Stop();
        //}

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);
            int err = -1;
            string strErrMsg = string.Empty;

            try {
                if ((bRes == true)
                    && (active == true)
                    && (IsFirstActivated == true))
                        initialize(out err, out strErrMsg);
                else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"HPanelTepCommon::Activate () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            initializeDescPanel();

            return bRes;
        }
        /// <summary>
        /// Повторная инициализация
        /// </summary>
        protected virtual void reinit()
        {
            int err = -1;
            string errMsg = string.Empty;

            initialize(out err, out errMsg);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelTepCommon::clear () - " + errMsg);
            }
            else
            {
            }
        }

        protected abstract void initialize(out int err, out string errMsg);        
        /// <summary>
        /// Создать объект для обмена данными с БД
        /// </summary>
        /// <returns>Объект для обмена данными с БД</returns>
        protected abstract HandlerDbValues createHandlerDb();
        //protected abstract void Activate(bool activate);
        /// <summary>
        /// Добавить область оперативного описания выбранного объекта на вкладке
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="posCol">Позиция-столбец для размещения области описания</param>
        /// <param name="posRow">Позиция-строка для размещения области описания</param>
        protected virtual void addLabelDesc(string id, int posCol = 5, int posRow = 10)
        {
            GroupBox gbDesc = new GroupBox();
            gbDesc.Text = @"Описание";
            gbDesc.Dock = DockStyle.Fill;
            this.Controls.Add(gbDesc, posCol, posRow);
            this.SetColumnSpan(gbDesc, this.ColumnCount - posCol);
            this.SetRowSpan(gbDesc, this.RowCount - posRow);

            HPanelDesc ctrl = new HPanelDesc();
            ctrl.Name = id;
            gbDesc.Controls.Add(ctrl);
            m_name_panel_desc = id;
        }
        /// <summary>
        /// Для отображения актуальной "подсказки" для свойства
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void panelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
            string desc = string.Empty;
            string name = string.Empty;
            try {
                if (((DataGridView)obj).SelectedRows.Count > 0) {
                    name = ((DataGridView)obj).SelectedRows[0].Cells[0].Value.ToString();

                    foreach (DataRow r in Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES].Rows)
                        if (name == r["PARAM_NAME"].ToString())
                            desc = r["DESCRIPTION"].ToString();
                        else
                            ;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"HPanelCommon::HPanelEdit_dgvPropSelectionChanged () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            SetDescSelRow(desc, name);
        }

        protected void SetDescSelRow(string desc, string name)
        {
            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblRowDesc = new string[] { name, desc };
        }

        protected void addButton(Button ctrl, string id, int posCol, string text)
        {
            ctrl.Name = id;

            ctrl.Location = new System.Drawing.Point(1, 1);
            ctrl.Dock = DockStyle.Fill;
            ctrl.Text = text;
            //??? Идентификатор является позицией-столбцом
            this.Controls.Add(ctrl, 0, posCol);
            this.SetColumnSpan(ctrl, 1);
        }
        /// <summary>
        /// Добавить кнопку
        /// </summary>
        /// <param name="id">Идентификатор кнопки (свойство 'Name')</param>
        /// <param name="posCol">Позиция по вертикали</param>
        /// <param name="text">Подпись на кнопке</param>
        protected void addButton(string id, int posCol, string text)
        {
            Button ctrl = new Button();

            addButton(ctrl, id, posCol, text);
        }

        protected virtual void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            recUpdateInsertDelete(out err);

            if (!(err == 0))
            {
                errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
            }
            else
            {
                successRecUpdateInsertDelete();

                if (!(delegateSaveAdding == null))
                    delegateSaveAdding();
                else
                    ;
            }

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnSave_Click () - " + errMsg);
            }
            else
                ;
        }

        protected abstract void recUpdateInsertDelete(out int err);

        protected abstract void successRecUpdateInsertDelete();

        protected virtual void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            reinit();
        }
    }

    public abstract partial class HPanelTepCommon : HPanelCommon
    {
        protected struct TECComponent
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
        }

        protected struct PUT_PARAMETER
        {
            public struct KEY
            {
                /// <summary>
                /// Идентификатор в БД элемента в алгоритме расчета
                /// </summary>
                public int m_idNAlg;
                /// <summary>
                /// Идентификатор в БД компонента(оборудования)
                /// </summary>
                public int m_idComp;
            }

            public KEY Key;
            /// <summary>
            /// Идентификатор в БД (в соответствии с компонентом-оборудованием)
            /// </summary>
            public int m_Id;
            ///// <summary>
            ///// Наименовнаие параметра
            ///// </summary>
            //public string m_strNameShr;

            //public TECComponent m_component;

            //public int IdComponent { get { return m_component.m_id; } }

            //public string NameShrComponent { get { return m_component.m_nameShr; } }

            public bool IsEnabled { get { return m_bEnabled; } }

            public bool IsVisibled { get { return m_bVisibled; } }

            //public string m_strText;
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
            /// <param name="enabled">Признак доступности (участия в расчете, если 'NALG' выключен, то и 'PUT' тоже выключен)</param>
            public PUT_PARAMETER(int id_alg, int id_comp, int id_put, bool enabled, bool visibled)
                : this (new TepCommon.HPanelTepCommon.PUT_PARAMETER.KEY() { m_idNAlg = id_alg, m_idComp = id_comp }
                    , id_put
                    , enabled
                    , visibled)
            {
            }

            public PUT_PARAMETER(KEY key, int id_put, bool enabled, bool visibled)
            {
                Key = key;
                m_Id = id_put;
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

        protected class NALG_PARAMETER
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

            public short m_sAverage;

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

            public int m_prjRatio;
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
                , short sAverage
                , int idMeasure, string nameShrMeasure, string symbol
                , bool enabled, bool visibled
                , int prjRatio, int vsRatio, int vsRound)
            {
                m_type = type;
                m_Id = id_alg; m_nAlg = n_alg;
                m_strNameShr = nameShr;  m_strDescription = desc;
                m_sAverage = sAverage;
                m_iIdMeasure = idMeasure; m_strMeausure = nameShrMeasure; m_strSymbol = symbol;
                m_bEnabled = enabled; m_bVisibled = visibled;
                m_prjRatio = prjRatio; m_vsRatio = vsRatio; m_vsRound = vsRound;
            }

            public NALG_PARAMETER(NALG_PARAMETER clone)
                : this (clone.m_type
                        , clone.m_Id, clone.m_nAlg
                        , clone.m_strNameShr
                        , clone.m_strDescription
                        , clone.m_sAverage
                        , clone.m_iIdMeasure
                        , clone.m_strMeausure
                        , clone.m_strSymbol
                        , clone.m_bEnabled, clone.m_bVisibled
                        , clone.m_prjRatio, clone.m_vsRatio, clone.m_vsRound)
            {
            }
        }

        public HPanelTepCommon(IPlugIn plugIn) : base (plugIn)
        {
            eventAddNAlgParameter += new Action<NALG_PARAMETER>(onAddNAlgParameter);

            eventAddPutParameter += new Action<PUT_PARAMETER>(onAddPutParameter);

            eventAddComponent += new Action<TECComponent>(onAddComponent);
        }

        private PanelManagementTaskCalculate __panelManagement;

        protected PanelManagementTaskCalculate _panelManagement
        {
            get { return __panelManagement; }

            set {
                __panelManagement = value;
                // обработчик события при изменении значений в основных элементах управления
                __panelManagement.EventIndexControlBaseValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlBase_onValueChanged);
                // обработчик события при изменении значений в дополнительных(добавленных программистом в наследуемых классах) элементах управления
                __panelManagement.EventIndexControlCustomValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlCustom_onValueChanged);
            }
        }

        public bool IsInParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES);
            }
        }

        public bool IsOutParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME);
            }
        }

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

        protected abstract PanelManagementTaskCalculate createPanelManagement();
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

                panelManagement_DatetimeRangeChanged();
            } else {
                // изменены PERIOD или TIMEZONE
                switch ((ID_DBTABLE)obj) {
                    case ID_DBTABLE.TIME:
                        Session.CurrentIdPeriod = _panelManagement.IdPeriod;

                        panelManagement_PeriodChanged();
                        break;
                    case ID_DBTABLE.TIMEZONE:
                        Session.CurrentIdTimezone = _panelManagement.IdTimezone;
                            //, (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)_panelManagement.IdTimezone)[0][@"OFFSET_UTC"]);

                        panelManagement_TimezoneChanged();
                        break;
                    default:
                        throw new Exception(string.Format(@"HPanelTepCommon::panelManagement_EventIndexControlBase_onValueChanged () - {} неизвестный тип события...", obj));
                        //break;
                }
            }

            // очистить содержание представления
            clear();
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }
        /// <summary>
        /// Обработчик события при изменении значений в дополнительных элементах управления на панели упарвления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected virtual void panelManagement_EventIndexControlCustom_onValueChanged(object obj) { }        
        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        protected virtual List<DataRow> ListParameter { get; }

        protected virtual void panelManagement_DatetimeRangeChanged()
        {
        }
        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void panelManagement_PeriodChanged()
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

        protected virtual void panelManagement_TimezoneChanged()
        {
        }

        protected abstract void onAddNAlgParameter(NALG_PARAMETER obj);

        protected abstract void onAddPutParameter(PUT_PARAMETER obj);

        protected abstract void onAddComponent(TECComponent comp);

        #region Добавление компонентов, параметров в алгоритме расчета
        private bool getIdComponentOwner(int id_comp, out int id_comp_owner)
        {
            return int.TryParse((string)m_dictTableDictPrj[ID_DBTABLE.COMP_VALUES].Select(string.Format(@"ID={0} AND ID_PAR={1}", id_comp, (int)COMP_PARAMETER.OWNER))[0][@"VALUE"], out id_comp_owner);
        }

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
                        Logging.Logg().Error(string.Format(@"HPanelTepCommon::panelManagement_PeriodChanged () - не определенный идентификатор родительского компонента для {}...", id_comp), Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    Logging.Logg().Error(string.Format(@"HPanelTepCommon::panelManagement_PeriodChanged () - не определенный идентификатор компонента..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void addAlgParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            int err = -1
                , id_alg = -1
                , id_comp = -1, id_comp_owner = -1
                , prjRatio = -1, vsRatio = -1, vsRound = -1
                , enabled = -1, visibled = -1;
            bool bEnabled = false
                , bVisibled = false;
            string n_alg = string.Empty;
            List<int> listIdNAlg = new List<int>();
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            TECComponent component;

            //Список параметров для отображения
            IEnumerable<DataRow> listParameter =
                // в каждой строке значения полей, относящихся к параметру алгоритма расчета одинаковые, т.к. 'ListParameter' объединение 2-х таблиц
                //ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First()) // исключить дублирование по полю [ID_ALG]
                ListParameter.Select(x => x);

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
                        , ((short)r[@"AVG"])
                        , ((int)r[@"ID_MEASURE"])
                        , r[@"NAME_SHR_MEASURE"] is DBNull ? string.Empty : ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        , r[@"SYMBOL"] is DBNull ? string.Empty : ((string)r[@"SYMBOL"]).Trim()
                        , bEnabled
                        , bVisibled
                        , prjRatio, vsRatio, vsRound
                    ));
                } else {
                    // параметр уже был добавлен                    
                }

                // всегда добавлять (каждый параметр)
                id_comp = (int)r[@"ID_COMP"];
                if ((id_comp > 0)
                    && (getIdComponentOwner(id_comp, out id_comp_owner) == true)) {
                    component = new TECComponent(id_comp
                        , id_comp_owner
                        , string.Empty //??? m_strNameShr = ((string)r[@"NAME_SHR"]).Trim()                        
                        , bEnabled
                        , bVisibled
                    );
                    // только, если назначенн обработчик в 'PanelTaskTepOutVal'
                    eventAddPutParameter?.Invoke(new PUT_PARAMETER() {
                        Key = new PUT_PARAMETER.KEY() { m_idNAlg = id_alg, m_idComp = id_comp }
                        , m_Id = (int)r[@"ID"]
                        , m_bEnabled = bEnabled
                        , m_bVisibled = bVisibled
                        ,
                    });
                } else
                    Logging.Logg().Error(string.Format(@"::addAlgParameters () - некорректный идентификатор параметра в алгоритме расчета..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        #endregion

        /// <summary>
        /// преобразование числа в нужный формат отображения
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

    public class HPanelDesc : TableLayoutPanel
    {
        enum ID_LBL { lblGroup, lblTab, lblDGV1, lblDGV2, lblDGV3, selRow };

        string[] m_desc_lbl = new string[] { "lblGroupDesc", "lblTabDesc", "lblDGV1Desc", "lblDGV2Desc", "lblDGV3Desc", "selRowDesc" };
        string[] m_name_lbl = new string[] { "lblGroupName", "lblTabName", "lblDGV1Name", "lblDGV2Name", "lblDGV3Name", "selRowName" };
        string[] m_name_lbl_text = new string[] { "Группа вкладок: ", "Вкладка: ", "Таблица: ", "Таблица: ", "Таблица: ", "Выбранная строка: " };

        private void Initialize()
        {
            this.SuspendLayout();
            this.ColumnCount = 7;
            this.RowCount = 14;
            int i = 0;
            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.ColumnCount));
            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.RowCount));
            int rows = 0;
            int col = 0;

            this.Dock = DockStyle.Fill;

            Control ctrl = new Control();

            ctrl = new Label();
            ctrl.Name = "obj";
            ctrl.Text = "Объект";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 2);

            col = 2;

            ctrl = new Label();
            ctrl.Name = "desc";
            ctrl.Text = "Описание";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 4);

            col = 0;
            rows = 2;

            for (i = 0; i < m_desc_lbl.Length; i++)
            {
                ctrl = new Label();
                ctrl.Name = m_name_lbl[i];
                ctrl.Text = m_name_lbl_text[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 2);

                ctrl = new Label();
                ctrl.Name = m_desc_lbl[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                col = 2;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 4);
                rows += 2;
                col = 0;
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public HPanelDesc()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Поле описания группы вкладок
        /// </summary>
        public string[] SetLblGroup
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = "Группа вкладок: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания вкладки
        /// </summary>
        public string[] SetLblTab
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = "Вкладка: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 1
        /// </summary>
        public string[] SetLblDGV1Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public string[] SetLblDGV2Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public string[] SetLblDGV3Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания выбранной строки
        /// </summary>
        public string[] SetLblRowDesc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[1] != string.Empty)
                {
                    ctrl.Text = value[1];
                    ctrl.Visible = true;
                }
                else
                    ctrl.Visible = false;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[0] != string.Empty)
                {
                    value[0].Replace('_', ' ');
                    ctrl.Text = "Cтрока: " + value[0];
                    ctrl.Visible = true;
                }

            }
        }

        /// <summary>
        /// Поле отображения описания группы вкладок
        /// </summary>
        public bool SetLblGroup_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания вкладки
        /// </summary>
        public bool SetLblTab_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания таблицы 1
        /// </summary>
        public bool SetLblDGV1Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public bool SetLblDGV2Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public bool SetLblDGV3Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания выбранной строки
        /// </summary>
        public bool SetLblRowDesc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

            }
        }

    }
}
