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

namespace TepCommon
{
    public abstract partial class PanelTaskTepValues : HPanelTepCommon
    {
        /// <summary>
        /// Перечисление - индексы таблиц для значений
        ///  , собранных в автоматическом режиме
        ///  , "по умолчанию"
        /// </summary>
        private enum INDEX_TABLE_VALUES : int { VARIABLE, DEFAULT, COUNT }
        private enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1, PERIOD, TIMEZONE, COMPONENT, PARAMETER/*, VISUAL_SETTINGS*/, MODE_DEV/*, MEASURE*/, RATIO
            , COUNT_TABLE_DICTPRJ }
        /// <summary>
        /// Наименования таблиц с парметрами для расчета
        /// </summary>
        private string m_strNameTableAlg
            , m_strNameTablePut
            , m_strNameTableValues;
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
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        private DataTable []m_arTableDictPrjs;
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID { UNKNOWN = -1
            , PERIOD // идентификаторы периодов расчетов, использующихся на форме
            , TIMEZONE //Идентификаторы (целочисленные, из БД системы) часовых поясов
            , ALL_COMPONENT, ALL_PARAMETER // все идентификаторы компонентов ТЭЦ/параметров
            , DENY_COMP_CALCULATED, DENY_PARAMETER_CALCULATED //запрещенных для расчета
            , DENY_COMP_VISIBLED, DENY_PARAMETER_VISIBLED // запрещенных для отображения
            , COUNT_INDEX_ID }
        /// <summary>
        /// Массив списков идентификаторов компонентов ТЭЦ/параметров
        /// </summary>
        private List<int> [] m_arListIds;
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Объект для обмена данными с БД
        /// </summary>
        protected HandlerDbTepTaskValues m_handlerDb;
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        public PanelTaskTepValues(IPlugIn iFunc, string strNameTableAlg, string strNameTablePut, string strNameTableValues)
            : base(iFunc)
        {
            //int iRes = compareNAlg (@"4.1", @"10");
            //iRes = compareNAlg (@"10", @"4.1");
            //iRes = compareNAlg (@"10.1", @"7.1");
            //iRes = compareNAlg(@"4", @"10.1");

            m_arTableOrigin = new DataTable [(int)INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];

            m_strNameTableAlg = strNameTableAlg;
            m_strNameTablePut = strNameTablePut;
            m_strNameTableValues = strNameTableValues;

            m_handlerDb = new HandlerDbTepTaskValues();

            InitializeComponents();

            m_panelManagement.DateTimeRangeValue_Changed += new EventHandler(datetimeRangeValue_onChanged);
            m_dgvValues.EventCellValueChanged += new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventHandler(onEventCellValueChanged);
        }
        /// <summary>
        /// Инициализация элементов управления объекта
        /// </summary>
        private void InitializeComponents()
        {
            #region Код, не относящийся к инициализации элементов управления
            m_arListIds = new List<int>[(int)INDEX_ID.COUNT_INDEX_ID];
            for (INDEX_ID i = INDEX_ID.PERIOD; i < INDEX_ID.COUNT_INDEX_ID; i++)
                switch (i)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)i] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.SHIFTS, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)i] = new List<int> { (int)ID_TIMEZONE.UTC };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)i] = new List<int>();
                        break;
                }
            #endregion

            m_arTableDictPrjs = new DataTable [(int)INDEX_TABLE_DICTPRJ.COUNT_TABLE_DICTPRJ];

            m_panelManagement = new PanelManagement ();
            m_dgvValues = new DataGridViewTEPValues ();
            int posColdgvTEPValues = 4
                , hightRowdgvTEPValues = 10;

            SuspendLayout ();

            initializeLayoutStyle ();

            Controls.Add (m_panelManagement, 0, 0);
            SetColumnSpan(m_panelManagement, posColdgvTEPValues); SetRowSpan(m_panelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvTEPValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvTEPValues); SetRowSpan(m_dgvValues, hightRowdgvTEPValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvTEPValues, hightRowdgvTEPValues);

            ResumeLayout (false);
            PerformLayout ();

            (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
        }
        /// <summary>
        /// Инициализация данных объекта
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="err">Признак результата выполнения инициализации</param>
        /// <param name="errMsg">Строка - пояснение к ошибке при выполнении</param>
        protected override void initialize(ref System.Data.Common.DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;            

            Control ctrl = null;
            CheckedListBox clbxCompCalculated
                , clbxCompVisibled;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            bool bChecked = false;
            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj = queryDictPrj;
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT_TABLE_DICTPRJ; i++)
            {
                m_arTableDictPrjs[i] = DbTSQLInterface.Select(ref dbConn, arQueryDictPrj[i], null, null, out err);

                if (!(err == 0))
                    break;
                else
                    ;
            }

            if (err == 0)
            {
                try
                {
                    base.Start();

                    m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Clear();
                    //Заполнить элементы управления с компонентами станции
                    clbxCompCalculated = Controls.Find(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString(), true)[0] as CheckedListBox;
                    clbxCompVisibled = Controls.Find(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString(), true)[0] as CheckedListBox;
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
                    {
                        id_comp = (Int16)r[@"ID"];
                        m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
                        strItem = ((string)r[@"DESCRIPTION"]).Trim();
                        // установить признак участия в расчете компонента станции
                        bChecked = m_arListIds[(int)INDEX_ID.DENY_COMP_CALCULATED].IndexOf(id_comp) < 0;
                        clbxCompCalculated.Items.Add(strItem, bChecked);
                        // установить признак отображения компонента станции
                        bChecked = m_arListIds[(int)INDEX_ID.DENY_COMP_VISIBLED].IndexOf(id_comp) < 0;
                        clbxCompVisibled.Items.Add(strItem, bChecked);
                        m_dgvValues.AddColumn(id_comp, strItem, bChecked);
                    }
                    // установить единый обработчик события - изменение состояния признака участие_в_расчете/видимость
                    // компонента станции для элементов управления
                    clbxCompCalculated.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);
                    clbxCompVisibled.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);

                    m_dgvValues.SetRatio(m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.RATIO]);

                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 0;

                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(INDEX_CONTROL.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 0;

                    // отобразить значения
                    updateDataValues();
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
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        private void clear(bool bClose = false)
        {
            int i = -1;
            CheckedListBox clbx = null;
            ComboBox cbx = null;
            HDateTimePicker hdtp = null;

            if (bClose == true)
            {
                for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT_TABLE_DICTPRJ; i++)
                {
                    m_arTableDictPrjs[i].Clear();
                    m_arTableDictPrjs[i] = null;
                }

                clbx = Controls.Find(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString(), true)[0] as CheckedListBox;
                clbx.ItemCheck -= clbx_ItemCheck;
                clbx.Items.Clear();
                clbx = Controls.Find(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString(), true)[0] as CheckedListBox;
                clbx.ItemCheck -= clbx_ItemCheck;
                clbx.Items.Clear();
                clbx = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString(), true)[0] as CheckedListBox;
                clbx.Items.Clear();
                clbx = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString(), true)[0] as CheckedListBox;
                clbx.Items.Clear();

                cbx = Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(INDEX_CONTROL.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                //hdtp = Controls.Find(INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker;
                //hdtp.set

                m_dgvValues.Columns.Clear();
            }
            else
                ;

            // очистить содержание представления
            m_dgvValues.ClearValues();
        }
        /// <summary>
        /// Остановить сопутствующие объекты
        /// </summary>
        public override void Stop()
        {
            clear(true);
            
            base.Stop();
        }
        /// <summary>
        /// Установить признак активности панель при выборе ее пользователем
        /// </summary>
        /// <param name="activate">Признак активности</param>
        /// <returns>Результат выполнения - был ли установлен признак</returns>
        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            return bRes;
        }
        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            err = -1;

            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn
                , @"inval_def"
                , @"ID_INPUT, ID_TIME"
                , m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT]
                , out err);
        }
        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();
        }
        /// <summary>
        /// Массив запросов к БД по получению словарных и проектных значений
        /// </summary>
        private string[] queryDictPrj
        {
            get
            {
                return new string[]
                {
                    //PERIOD
                    @"SELECT * FROM [time] WHERE [ID] IN (" + m_strIdPeriods + @")"
                    //TIMEZONE
                    , @"SELECT * FROM [timezones] WHERE [ID] IN (" + m_strIdTimezones + @")"
                    // список компонентов
                    , @"SELECT * FROM [comp_list] "
                        + @"WHERE ([ID] = 5 AND [ID_COMP] = 1)"
                            + @" OR ([ID_COMP] = 1000)"
                    // параметры расчета
                    , @"SELECT p.ID, p.ID_ALG, p.ID_COMP, p.ID_RATIO, p.MINVALUE, p.MAXVALUE"
                        + @", a.NAME_SHR, a.N_ALG, a.DESCRIPTION, a.ID_MEASURE, a.SYMBOL"
                        + @", m.NAME_RU as NAME_SHR_MEASURE, m.[AVG]"
                    + @" FROM [dbo].[" + m_strNameTablePut + @"] as p"
                        + @" JOIN [dbo].[" + m_strNameTableAlg + @"] as a ON a.ID_TASK = 1 AND a.ID = p.ID_ALG"
                        + @" JOIN [dbo].[measure] as m ON a.ID_MEASURE = m.ID"
                    //// настройки визуального отображения значений
                    //, @""
                    // режимы работы
                    , @"SELECT * FROM [dbo].[mode_dev]"
                    //// единицы измерения
                    //, @"SELECT * FROM [dbo].[measure]"
                    // коэффициенты для единиц измерения
                    , @"SELECT * FROM [dbo].[ratio]"
                };
            }
        }
        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        /// </summary>
        private string queryValuesVar
        {
            get
            {
                return @"SELECT"
	                + @" p.ID"
	                + @", " + HUsers.Id //ID_USER
	                + @", v.ID_SOURCE"
	                + @", 0" //ID_SESSION
                    //+ @", CAST ('" + m_panelManagement.m_dtRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    //+ @", CAST ('" + m_panelManagement.m_dtRange.End.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    //+ @", v.ID_TIME"
                    //+ @", v.ID_TIMEZONE"
	                + @", v.QUALITY"
	                + @", CASE WHEN m.[AVG] = 0 THEN SUM (v.[VALUE])"
		                + @" WHEN m.[AVG] = 1 THEN AVG (v.[VALUE])"
		                + @" ELSE MIN (v.[VALUE]) END as VALUE"
	                + @", GETDATE ()"
                    + @" FROM [dbo].[" + m_strNameTableValues + @"_201512] v"
	                    + @" LEFT JOIN [dbo].[" + m_strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
                        + @" LEFT JOIN [dbo].[" + m_strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
	                    + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                    + @" WHERE [ID_TIME] = " + (int)_currIdPeriod
                        + @" AND [DATE_TIME] > CAST ('" + m_panelManagement.m_dtRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                        + @" AND [DATE_TIME] <= CAST ('" + m_panelManagement.m_dtRange.End.ToString (@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    + @" GROUP BY v.ID_INPUT, v.ID_SOURCE, v.ID_TIME, v.ID_TIMEZONE, v.QUALITY"
	                    + @", a.ID_MEASURE, a.N_ALG"
                        + @", p.ID, p.ID_ALG, p.ID_COMP, p.MAXVALUE, p.MINVALUE"
	                    + @", m.[AVG]"
                        ;
            }
        }
        /// <summary>
        /// Запрос для получения значений "по умолчанию"
        /// </summary>
        private string queryValuesDef
        {
            get
            {
                return @"SELECT"
                    + @" *"
                    + @" FROM [dbo].[" + m_strNameTableValues + @"_def] v"
                    + @" WHERE [ID_TIME] = " + (int)_currIdPeriod
                        ;
            }
        }
        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        /// </summary>
        private void updateDataValues()
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1
                , cnt = (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1;
            string errMsg = string.Empty
                , query = string.Empty;
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsSel = null;
            // представление очичается в 'clear ()' - при автоматическом вызове, при нажатии на кнопку "Загрузить" (аналог "Обновить")
            DbConnection dbConn = null;            
            // получить объект для соединения с БД
            dbConn = DbSources.Sources().GetConnection(iListenerId, out err);
            // проверить успешность получения 
            if ((!(dbConn == null)) && (err == 0))
            {
                //Запрос для получения автоматически собираемых данных
                query = queryValuesVar;
                //Заполнить таблицу автоматически собираемыми данными
                m_arTableOrigin [(int)INDEX_TABLE_VALUES.VARIABLE] = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                {
                    //Запрос для получения данных вводимых вручную
                    query = queryValuesDef;
                    //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                    m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT] = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
                    //Проверить признак выполнения запроса
                    if (err == 0)
                    {
                        // удалить строки из таблицы со значениями "по умолчанию"
                        foreach (DataRow rValVar in m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows)
                        {
                            rowsSel = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_INPUT=" + rValVar[@"ID"]);
                            foreach (DataRow rToRemove in rowsSel)
                                m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Rows.Remove(rToRemove);
                        }
                        // вставить строки из таблицы со значениями "по умолчанию"
                        foreach (DataRow rValDef in m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Rows)
                        {
                            rowsSel = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select(@"ID=" + rValDef[@"ID_INPUT"]);
                            if (rowsSel.Length == 1)
                            {
                                iAVG = (Int16)rowsSel[0][@"AVG"];

                                m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows.Add(new object[]
                                    {
                                        rValDef[@"ID_INPUT"]
                                        , HUsers.Id //ID_USER
                                        , -1 //ID_SOURCE
                                        , 0 //ID_SESSION
                                        , -1 //QUALITY
                                        , (iAVG == 0) ? cnt * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
                                        , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                    }
                                );
                            }
                            else
                                ; // по иднгтификатору найден не единственный парпметр расчета
                        }
                        // создать копии для возможности сохранения изменений
                        m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Copy();
                        m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();

                        m_dgvValues.ShowValues(m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE], m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]);
                    }
                    else
                        errMsg = @"ошибка получения данных по умолчанию с " + m_panelManagement.m_dtRange.Begin.ToString()
                            + @" по " + m_panelManagement.m_dtRange.End.ToString();
                }
                else
                    errMsg = @"ошибка получения автоматически собираемых данных с " + m_panelManagement.m_dtRange.Begin.ToString()
                        + @" по " + m_panelManagement.m_dtRange.End.ToString();
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelTepCommon_btnUpdate_Click () - " + errMsg);
            }
            else
                ;
        }        
        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            base.HPanelTepCommon_btnUpdate_Click(obj, ev);

            updateDataValues ();
        }
        /// <summary>
        /// Сравнить строки с параметрами алгоритма расчета по строковому номеру в алгоритме
        ///  для сортировки при отображении списка параметров расчета
        /// </summary>
        /// <param name="r1">1-я строка для сравнения</param>
        /// <param name="r2">2-я строка для сравнения</param>
        /// <returns>Результат сравнения (-1 - 1-я МЕНЬШЕ 2-ой, 1 - 1-я БОЛЬШЕ 2-ой)</returns>
        private int compareNAlg (DataRow r1, DataRow r2)
        {
            int iRes = 0
                , iLength = -1
                , indx = -1;
            char[] delimeter = new char[] { '.' };
            string nAlg1 = ((string)r1[@"N_ALG"]).Trim()
                , nAlg2 = ((string)r2[@"N_ALG"]).Trim();

            string[] arParts1 = nAlg1.Split(delimeter, StringSplitOptions.RemoveEmptyEntries)
                , arParts2 = nAlg2.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);

             if ((!(arParts1.Length < 1)) && (!(arParts2.Length < 1)))
             {
                 indx = 0;
                 iRes = int.Parse(arParts1[indx]) > int.Parse(arParts2[indx]) ? 1
                     : int.Parse(arParts1[indx]) < int.Parse(arParts2[indx]) ? -1 : 0;

                 if (iRes == 0)
                 {
                     iLength = arParts1.Length > arParts2.Length ? 1 :
                         arParts1.Length < arParts2.Length ? -1 : 0;

                     if (iLength == 0)
                     {
                         if ((!(arParts1.Length < 2)) && (!(arParts2.Length < 2)))
                         {
                             indx = 1;
                             iRes = int.Parse(arParts1[indx]) > int.Parse(arParts2[indx]) ? 1
                                 : int.Parse(arParts1[indx]) < int.Parse(arParts2[indx]) ? -1 : 0;
                         }
                         else
                             ;
                     }
                     else
                         iRes = iLength;
                 }
                 else
                     ;
             }
             else
                 throw new Exception(@":PanelTaskTepValues:compareNAlg () - номер алгоритма некорректен (не найдены цифры)...");
             return iRes;
        }
        /// <summary>
        /// Текущий выбранный идентификатор периода расчета
        /// </summary>
        private ID_PERIOD _currIdPeriod;
        ///// <summary>
        ///// Текущий выбранный идентификатор периода расчета
        ///// </summary>
        //private ID_TIME CurrIdPeriod
        //{
        //    get
        //    {
        //        return
        //            //m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]
        //            _currIdPeriod
        //            ;
        //    }
        //}

        private ID_TIMEZONE _currIdTimezone;
        /// <summary>
        /// Количество базовых периодов
        /// </summary>
        private int CountBasePeriod
        {
            get
            {
                return _currIdPeriod == ID_PERIOD.HOUR ?
                    (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0 :
                    _currIdPeriod == ID_PERIOD.DAY ?
                        (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalDays - 0 :
                        24;
            }
        }
        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List <DataRow> ListParameter
        {
            get
            {
                List <DataRow> listRes;
                
                listRes = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select(/*@"ID_TIME=" + CurrIdPeriod*/).ToList<DataRow>();
                listRes.Sort(compareNAlg);

                return listRes;
            }
        }
        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            ComboBox cbx = obj as ComboBox;
            int err = -1
                , id_alg = -1
                , ratio = -1
                , round = -1;
            string strItem = string.Empty;
            bool bVisibled = false;
            CheckedListBox clbxParsCalculated = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString(), true)[0] as CheckedListBox
                , clbxParsVisibled = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString(), true)[0] as CheckedListBox;
            Dictionary<int, HTepUsers.VISUAL_SETTING> dictVisualsettings = new Dictionary<int, HTepUsers.VISUAL_SETTING>();
            //Установить новое значение для текущего периода
            _currIdPeriod = (ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][cbx.SelectedIndex];
            //Отменить обработку событий
            clbxParsCalculated.ItemCheck -= clbx_ItemCheck;
            clbxParsVisibled.ItemCheck -= clbx_ItemCheck;
            //Очистиить списки
            clbxParsCalculated.Items.Clear();
            clbxParsVisibled.Items.Clear();
            m_arListIds[(int)INDEX_ID.ALL_PARAMETER].Clear();
            //??? проверить сохранены ли значения
            m_dgvValues.ClearRows();
            //Список параметров для отображения            
            IEnumerable<DataRow> listParameter =
                //ListParameter.Select(par => (string)par[@"ID_ALG"]).Distinct() as IEnumerable<DataRow>
                ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First())
                ;
            //Установки для отображения значений
            dictVisualsettings = HTepUsers.GetParameterVisualSettings(m_connSett
                , new int[] {
                    1
                    , (_iFuncPlugin as PlugInBase)._Id
                    , (_iFuncPlugin as PlugInBase)._Id
                    , (int)_currIdPeriod }
                , out err);
            //Заполнить элементы управления с компонентами станции 
            foreach (DataRow r in listParameter)
            {
                id_alg = (int)r[@"ID_ALG"];

                if (m_arListIds[(int)INDEX_ID.ALL_PARAMETER].IndexOf(id_alg) < 0)
                {
                    m_arListIds[(int)INDEX_ID.ALL_PARAMETER].Add(id_alg);

                    strItem = ((string)r[@"N_ALG"]).Trim () + @" (" + ((string)r[@"NAME_SHR"]).Trim() + @")";
                    clbxParsCalculated.Items.Add(strItem, m_arListIds[(int)INDEX_ID.DENY_PARAMETER_CALCULATED].IndexOf(id_alg) < 0);
                    bVisibled = m_arListIds[(int)INDEX_ID.DENY_PARAMETER_VISIBLED].IndexOf(id_alg) < 0;
                    clbxParsVisibled.Items.Add(strItem, bVisibled);

                    if (dictVisualsettings.ContainsKey(id_alg) == true)
                    {
                        ratio = dictVisualsettings[id_alg].m_ratio;
                        round = dictVisualsettings[id_alg].m_round;
                    }
                    else
                    {
                        ratio = HTepUsers.s_iRatioDefault;
                        round = HTepUsers.s_iRoundDefault;
                    }

                    m_dgvValues.AddRow(new DataGridViewTEPValues.ROW_PROPERTY()
                    {
                        m_idAlg = id_alg
                        , m_strHeaderText = ((string)r[@"N_ALG"]).Trim()
                        , m_strToolTipText = ((string)r[@"NAME_SHR"]).Trim()
                        , m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        , m_strSymbol = !(r[@"SYMBOL"] is DBNull) ? ((string)r[@"SYMBOL"]).Trim() : string.Empty
                        //, m_bVisibled = bVisibled
                        , m_vsRatio = ratio
                        , m_vsRound = round
                        , m_ratio = (int)r[@"ID_RATIO"]
                    });
                }
                else
                    ;
            }
            //Возобновить обработку событий
            clbxParsCalculated.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);
            clbxParsVisibled.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);
            //Установить новые режимы для "календарей"
            HDateTimePicker hdtpBegin = Controls.Find(INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                , hdtpEnd = Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker;
            //Выполнить запрос на получение значений для заполнения 'DataGridView'
            switch (_currIdPeriod)
            {
                case ID_PERIOD.HOUR:
                    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    break;
                case ID_PERIOD.SHIFTS:
                    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    break;
                case ID_PERIOD.DAY:
                    hdtpBegin.Mode = HDateTimePicker.MODE.DAY;
                    hdtpEnd.Mode = HDateTimePicker.MODE.DAY;
                    break;
                case ID_PERIOD.MONTH:
                    hdtpBegin.Mode = HDateTimePicker.MODE.MONTH;
                    hdtpEnd.Mode = HDateTimePicker.MODE.MONTH;
                    break;
                case ID_PERIOD.YEAR:
                    hdtpBegin.Mode = HDateTimePicker.MODE.YEAR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.YEAR;
                    break;
                default:
                    break;
            }
            ////Загрузить значения для отображения
            //m_handlerDb.Load(_currIdPeriod);
        }

        private void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            ComboBox cbx = obj as ComboBox;
            //Установить новое значение для текущего периода
            _currIdTimezone = (ID_TIMEZONE)m_arListIds[(int)INDEX_ID.TIMEZONE][cbx.SelectedIndex];
        }
        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Арнумент события, описывающий состояние элемента</param>
        private void clbx_ItemCheck(object obj, ItemCheckEventArgs ev)
        {
            INDEX_CONTROL id = INDEX_CONTROL.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
            INDEX_ID indxIdDeny = INDEX_ID.UNKNOWN;
            int id_item = -1 //Идентификатор элемента списка (компонент ТЭЦ/параметр алгоритма)
                //, iCol = -2 // при передаче в функцию в качестве аргумента +1 (из-за ТЭЦ в 0-м столбце)
                //, iRow = -1 // '-1' - признак применения/НЕприменения действий к типу элементов таблицы 
                ;
            string strId = (obj as Control).Name;
            //Определить идентификатор
            if (strId.Equals(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString()) == true)
                id = INDEX_CONTROL.CLBX_COMP_CALCULATED;
            else
                if (strId.Equals(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString()) == true)
                    id = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED;
                else
                    if (strId.Equals(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString()) == true)
                        id = INDEX_CONTROL.CLBX_COMP_VISIBLED;
                    else
                        if (strId.Equals(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString()) == true)
                            id = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED;
                        else
                            throw new Exception(@"PanelTaskTepValues::clbx_ItemCheck () - не найден объект 'CheckedListBox'...");
            //Найти идентификатор компонента ТЭЦ/параметра алгоритма расчета
            // , соответствующий изменившему состояние элементу 'CheckedListBox'
            switch (id)
            {
                case INDEX_CONTROL.CLBX_COMP_CALCULATED:
                case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                    id_item = m_arListIds[(int)INDEX_ID.ALL_COMPONENT][ev.Index];
                    indxIdDeny = id == INDEX_CONTROL.CLBX_COMP_CALCULATED ? INDEX_ID.DENY_COMP_CALCULATED :
                        id == INDEX_CONTROL.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                    //iCol = ev.Index;
                    break;
                case INDEX_CONTROL.CLBX_PARAMETER_CALCULATED:
                case INDEX_CONTROL.CLBX_PARAMETER_VISIBLED:
                    id_item = m_arListIds[(int)INDEX_ID.ALL_PARAMETER][ev.Index];
                    indxIdDeny = id == INDEX_CONTROL.CLBX_PARAMETER_CALCULATED ? INDEX_ID.DENY_PARAMETER_CALCULATED :
                        id == INDEX_CONTROL.CLBX_PARAMETER_VISIBLED ? INDEX_ID.DENY_PARAMETER_VISIBLED : INDEX_ID.UNKNOWN;
                    //iRow = ev.Index;
                    break;
                default:
                    break;
            }            
            //Изменить признак состояния компонента ТЭЦ/параметра алгоритма расчета
            if (ev.NewValue == CheckState.Unchecked)
                if (m_arListIds[(int)indxIdDeny].IndexOf(id_item) < 0)
                    m_arListIds[(int)indxIdDeny].Add (id_item);
                else
                    ; //throw new Exception (@"");
            else
                if (ev.NewValue == CheckState.Checked)
                    if (! (m_arListIds[(int)indxIdDeny].IndexOf(id_item) < 0))
                        m_arListIds[(int)indxIdDeny].Remove (id_item);
                    else
                        ; //throw new Exception (@"");
                else
                    ;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            ;
            //Изменить структуру 'DataGridView'
            //m_dgvValues.UpdateStructure ();            
            m_dgvValues.UpdateStructure(indxIdDeny
                //, iCol + 1, iRow
                , id_item
                , ev.NewValue == CheckState.Checked ? true : ev.NewValue == CheckState.Unchecked ? false : false);
        }
        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(object obj, EventArgs ev)
        {
            // очистить содержание представления
            clear();
            //if ((! (m_tblOrigin == null))
            //    && (m_tblOrigin.Rows.Count > 0))
                updateDataValues();
            //else ;
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        private void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            //int id_par = (int)(pars as object [])[0]
            //    , id_comp = (int)(pars as object[])[1]
            //    , idParameter = (int)(pars as object[])[2];
            //double val = (double)(pars as object[])[3];

            DataRow[] rowsParameter = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_INPUT=" + ev.m_IdParameter);

            if (rowsParameter.Length == 1)
            {
                rowsParameter[0][@"VALUE"] = ev.m_Value;
            }
            else
                ;
        }
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTEPValues : DataGridView
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            private enum INDEX_SERVICE_COLUMN : uint { ID_ALG, SYMBOL, COUNT }
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
                    public int m_iQuality;

                    public HDataGridViewCell(int idParameter, int iQuality, bool bCalcDeny)
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

                public int m_vsRatio;

                public int m_ratio;

                public int m_vsRound;

                public HDataGridViewCell[] m_arPropertiesCells;

                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];
                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, -1, false);                    
                }
            }
            /// <summary>
            /// Перечисления для индексирования массива со значениями цветов для фона ячеек
            /// </summary>
            private enum INDEX_COLOR : uint { EMPTY, VARIABLE, DEFAULT, CALC_DENY, NAN, COUNT }
            /// <summary>
            /// Массив со значениями цветов для фона ячеек
            /// </summary>
            private Color[] m_arCellColors = new Color[(int)INDEX_COLOR.COUNT] { Color.Gray, Color.White, Color.Yellow, Color.LightGray, Color.White };
            /// <summary>
            /// Класс для описания аргумента события - изменения значения ячейки
            /// </summary>
            public class DataGridViewTEPValuesCellValueChangedEventArgs : EventArgs
            {
                public int m_IdComp
                    , m_IdAlg
                    , m_IdParameter;
                public double m_Value;

                public DataGridViewTEPValuesCellValueChangedEventArgs()
                    : base()
                {
                    m_IdAlg =
                    m_IdComp =
                    m_IdParameter =
                        -1;
                    m_Value = -1F;
                }

                public DataGridViewTEPValuesCellValueChangedEventArgs(int id_alg, int id_comp, int id_par, double val)
                    : this()
                {
                    m_IdAlg = id_alg;
                    m_IdComp = id_comp;
                    m_IdParameter = id_par;
                    m_Value = val;
                }
            }
            /// <summary>
            /// Тип делегата для обработки события - изменение значения в ячейке
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ev"></param>
            public delegate void DataGridViewTEPValuesCellValueChangedEventHandler (object obj, DataGridViewTEPValuesCellValueChangedEventArgs ev);
            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public DataGridViewTEPValuesCellValueChangedEventHandler EventCellValueChanged;            
            ///// <summary>
            ///// Список свойств ячеек в строке
            ///// </summary>
            //private List<ROW_PROPERTY> m_listPropertiesRows;
            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_iIdComp;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
            }

            //private class HDataGridViewRow : DataGridViewRow
            //{
            //    public HDataGridViewRow(ROW_PROPERTY rowProp)
            //    {
            //        m_Properties = rowProp;
            //        // установить значение для заголовка
            //        HeaderCell.Value = rowProp.m_strHeaderText
            //            //+ @"[" + rowProp.m_strMeasure + @"]"
            //            ;
            //        // установить значение для всплывающей подсказки
            //        HeaderCell.ToolTipText = rowProp.m_strToolTipText;
            //    }
                
            //    public ROW_PROPERTY m_Properties;
            //}

            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewTEPValues ()
            {
                //Разместить ячейки, установить свойства объекта
                InitializeComponents ();
                //Назначить (внутренний) обработчик события - изменение значения ячейки
                // для дальнейшей ретрансляции родительскому объекту
                CellValueChanged += new DataGridViewCellEventHandler (onCellValueChanged);
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

                AddColumn (-2, string.Empty, false);
                AddColumn(-1, @"Размерность", true);
            }
            /// <summary>
            /// Удалить строки
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                {
                    Rows.Clear();

                    m_dictPropertiesRows.Clear();
                }
                else
                    ;
            }
            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">Идентификатор компонента ТЭЦ</param>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bVisibled">Признак участия в расчете/отображения</param>
            public void AddColumn (int id_comp, string text, bool bVisibled)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (HDataGridViewColumn col in Columns)
                        if ((col.m_iIdComp > 0)
                            && (col.m_iIdComp < 1000))
                        {
                            indxCol = Columns.IndexOf(col);

                            break;
                        }
                        else
                            ;

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = id_comp, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для компонентов ТЭЦ
                        ; // оставить значения по умолчанию
                    else
                    {// для служебных столбцов                        
                        if (id_comp < 0)
                        {// только для столбца с [SYMBOL]
                            if (bVisibled == true)
                            {
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;                                
                            }
                            else
                                ;

                            column.Frozen = true;
                        }
                        else
                            ;
                    }

                    column.HeaderText = text;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;                    
                    column.Visible = bVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + id_comp + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="id_par">Идентификатор параметра алгоритма</param>
            /// <param name="headerText">Текст заголовка строки</param>
            /// <param name="toolTipText">Текст подсказки для заголовка строки</param>
            /// <param name="bVisibled">Признак отображения строки</param>
            public void AddRow(ROW_PROPERTY rowProp)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();
                else
                    ;
                m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);
                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value = rowProp.m_idAlg;
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.SYMBOL].Value = rowProp.m_strSymbol
                    + @",[" + rowProp.m_strMeasure + @"]"
                    ;
                // инициализировать значения в служебных ячейках
                m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
                // установить значение для заголовка
                Rows[i].HeaderCell.Value = rowProp.m_strHeaderText
                    //+ @"[" + rowProp.m_strMeasure + @"]"
                    ;
                // установить значение для всплывающей подсказки
                Rows[i].HeaderCell.ToolTipText = rowProp.m_strToolTipText;
            }
            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="bNewCalcDeny">Новое (устанавливаемое) значение признака участия в расчете для параметра</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности изменения цвета ячейки</returns>
            private bool getClrCellToComp(int iCol, int iRow, bool bNewCalcDeny, out Color clrRes)
            {
                bool bRes = false;
                clrRes = m_arCellColors[(int)INDEX_COLOR.EMPTY];
                int id_alg = (int)Rows[iRow].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;

                bRes = ((m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_bCalcDeny == false)
                    && ((m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].IsNaN == false)));
                if (bRes == true)
                    clrRes = ((bNewCalcDeny == true) && (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_bCalcDeny == false)) ?
                        !(m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality < 0) ? m_arCellColors[(int)INDEX_COLOR.VARIABLE] : m_arCellColors[(int)INDEX_COLOR.DEFAULT] :
                        m_arCellColors[(int)INDEX_COLOR.CALC_DENY];
                else
                    ;

                return bRes;
            }
            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="bNewCalcDeny">Новое (устанавливаемое) значение признака участия в расчете для параметра</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности изменения цвета ячейки</returns>
            private bool getClrCellToParameter(int iCol, int iRow, bool bNewCalcDeny, out Color clrRes)
            {
                bool bRes = false;
                clrRes = m_arCellColors[(int)INDEX_COLOR.EMPTY];
                int id_alg = (int)Rows[iRow].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;

                bRes = m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].IsNaN == false;
                if (bRes == true)
                {
                    clrRes = ((bNewCalcDeny == true) && ((Columns[iCol] as HDataGridViewColumn).m_bCalcDeny == false)) ?
                        !(m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality < 0) ? m_arCellColors[(int)INDEX_COLOR.VARIABLE] : m_arCellColors[(int)INDEX_COLOR.DEFAULT] :
                        m_arCellColors[(int)INDEX_COLOR.CALC_DENY];
                }
                else
                    ;

                return bRes;
            }
            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности размещения значения в ячейке</returns>
            private bool getClrCellToValue(int iCol, int iRow, out Color clrRes)
            {
                int id_alg = (int)Rows[iRow].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;
                bool bRes = !m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].IsNaN;
                clrRes = m_arCellColors[(int)INDEX_COLOR.EMPTY];

                if (bRes == true)
                    if (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality < 0)
                        clrRes = m_arCellColors[(int)INDEX_COLOR.DEFAULT];
                    else
                        clrRes = m_arCellColors[(int)INDEX_COLOR.VARIABLE];
                else
                    clrRes = m_arCellColors[(int)INDEX_COLOR.NAN];

                return bRes;
            }
            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public void UpdateStructure(PanelTaskTepValues.INDEX_ID indxDeny, int id, bool bItemChecked)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                int indx = -1
                    , cIndx = -1
                    , rKey = -1;
                //Поиск индекса элемента отображения
                switch (indxDeny)
                {
                    case INDEX_ID.DENY_COMP_CALCULATED:
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        // найти индекс столбца (компонента) - по идентификатору
                        foreach (HDataGridViewColumn c in Columns)
                            if (c.m_iIdComp == id)
                            {
                                indx = Columns.IndexOf(c);
                                break;
                            }
                            else
                                ;
                        break;
                    case INDEX_ID.DENY_PARAMETER_CALCULATED:
                    case INDEX_ID.DENY_PARAMETER_VISIBLED:
                        // найти индекс строки (параметра) - по идентификатору
                        foreach (DataGridViewRow r in Rows)
                            if ((int)r.Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value == id)
                            {
                                indx = Rows.IndexOf(r);
                                break;
                            }
                            else
                                ;
                        break;
                    default:
                        break;
                }

                if (!(indx < 0))
                {
                    switch (indxDeny)
                    {
                        case INDEX_ID.DENY_COMP_CALCULATED:
                            cIndx = indx;
                            // для всех ячеек в столбце
                            foreach (DataGridViewRow r in Rows)
                            {
                                indx = Rows.IndexOf(r);
                                if (getClrCellToComp(cIndx, indx, bItemChecked, out clrCell) == true)
                                    r.Cells[cIndx].Style.BackColor = clrCell;
                                else
                                    ;
                            }
                            (Columns[cIndx] as HDataGridViewColumn).m_bCalcDeny = !bItemChecked;
                            break;
                        case INDEX_ID.DENY_PARAMETER_CALCULATED:
                            rKey = (int)Rows[indx].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;
                            // для всех ячеек в строке
                            foreach (DataGridViewCell c in Rows[indx].Cells)
                            {
                                cIndx = Rows[indx].Cells.IndexOf(c);
                                if (getClrCellToParameter(cIndx, indx, bItemChecked, out clrCell) == true)
                                    c.Style.BackColor = clrCell;
                                else
                                    ;

                                m_dictPropertiesRows[rKey].m_arPropertiesCells[cIndx].m_bCalcDeny = !bItemChecked;
                            }
                            break;
                        case INDEX_ID.DENY_COMP_VISIBLED:
                            cIndx = indx;
                            // для всех ячеек в столбце
                            Columns[cIndx].Visible = bItemChecked;
                            break;
                        case INDEX_ID.DENY_PARAMETER_VISIBLED:
                            // для всех ячеек в строке
                            Rows[indx].Visible = bItemChecked;
                            break;
                        default:
                            break;
                    }
                }
                else
                    ; // нет элемента для изменения стиля
            }
            /// <summary>
            /// Отобразить значения
            /// </summary>
            /// <param name="values">Значения для отображения</param>
            public void ShowValues(DataTable values, DataTable parameter)
            {
                int idAlg = -1
                    , idParameter = -1
                    , iQuality = -1
                    , iCol = 0, iRow = 0
                    , ratioValue = -1, vsRatioValue = -1;
                double dblVal = -1F;
                DataRow[] cellRows = null
                    , parameterRows = null;
                Color clrCell = Color.Empty;

                CellValueChanged -= onCellValueChanged;

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                        {
                            dblVal = double.NaN;
                            idParameter = -1;
                            iQuality = -1;
                            idAlg = (int)row.Cells[0].Value;
                            parameterRows = parameter.Select(@"ID_COMP=" + col.m_iIdComp + @" AND " + @"ID_ALG=" + idAlg);
                            if (parameterRows.Length == 1)
                            {
                                cellRows = values.Select(@"ID=" + parameterRows[0][@"ID"]);

                                if (cellRows.Length == 1)
                                {
                                    idParameter = (int)cellRows[0][@"ID"];
                                    dblVal = ((double)cellRows[0][@"VALUE"]);
                                    iQuality = (int)cellRows[0][@"QUALITY"];
                                }
                                else
                                    ; // continue
                            }
                            else
                                ; // параметр расчета для компонента станции не найден

                            iRow = Rows.IndexOf(row);
                            m_dictPropertiesRows[idAlg].m_arPropertiesCells[iCol].m_IdParameter = idParameter;
                            m_dictPropertiesRows[idAlg].m_arPropertiesCells[iCol].m_iQuality = iQuality;
                            row.Cells[iCol].ReadOnly = double.IsNaN(dblVal);

                            if (getClrCellToValue(iCol, iRow, out clrCell) == true)
                            {
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                ratioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_ratio].m_value;
                                dblVal /= Math.Pow(10F, vsRatioValue);
                                row.Cells[iCol].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                                ;

                            row.Cells[iCol].Style.BackColor = clrCell;
                        }
                    else
                        ;

                    iCol++;
                }

                CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
            }
            /// <summary>
            /// Очистить содержание представления (например, перед )
            /// </summary>
            public void ClearValues()
            {
                CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;
                        else
                            ;

                CellValueChanged += new DataGridViewCellEventHandler (onCellValueChanged);
            }

            private void onCellValueChanged(object obj, DataGridViewCellEventArgs ev)
            {
                string strValue = string.Empty;
                double dblValue = double.NaN;
                int id_alg = (int)Rows[ev.RowIndex].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;

                try
                {
                    //0 - идентификатор компонета (служебный)
                    //1 - размерность (служебный)
                    if ((ev.ColumnIndex > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        && (! (ev.RowIndex < 0)))
                    {
                        strValue = (string)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value;

                        if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dblValue) == true)
                            EventCellValueChanged(this, new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs((int)Rows[ev.RowIndex].Cells[0].Value //Идентификатор параметра [alg]
                                , (Columns[ev.ColumnIndex] as HDataGridViewColumn).m_iIdComp //Идентификатор компонента
                                , m_dictPropertiesRows[id_alg].m_arPropertiesCells[ev.ColumnIndex].m_IdParameter //Идентификатор параметра с учетом периода расчета [put]
                                , dblValue));
                        else
                            ; //??? невозможно преобразовать значение - отобразить сообщение для пользователя
                    }
                    else
                        ; // в 0-ом столбце идентификатор параметра расчета
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::onCellValueChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            private struct RATIO
            {
                public int m_id;

                public int m_value;

                public string m_nameRU
                    , m_nameEN
                    , m_strDesc;
            }

            private Dictionary<int, RATIO> m_dictRatio;

            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO() { m_id = (int)r[@"ID"]
                        , m_value = (int)r[@"VALUE"]
                        , m_nameRU = (string)r[@"NAME_RU"]
                        , m_nameEN = (string)r[@"NAME_RU"]
                        , m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagement : HPanelCommon
        {
            public EventHandler DateTimeRangeValue_Changed;
            public DateTimeRange m_dtRange;

            public PanelManagement() : base (8, 21)
            {
                InitializeComponents ();

                HDateTimePicker hdtpBegin = Controls.Find(INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                    , hdtpEnd = Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                m_dtRange = new DateTimeRange(hdtpBegin.Value, hdtpEnd.Value);
                ////Назначить обработчик события - изменение дата/время начала периода
                //hdtpBegin.ValueChanged += new EventHandler(hdtpBegin_onValueChanged);
                //Назначить обработчик события - изменение дата/время окончания периода
                // при этом отменить обработку события - изменение дата/время начала периода
                // т.к. при изменении дата/время начала периода изменяется и дата/время окончания периода
                hdtpEnd.ValueChanged += new EventHandler (hdtpEnd_onValueChanged);
            }

            private void InitializeComponents ()
            {
                Control ctrl = null;
                int posRow = -1;
                DateTime today = DateTime.Today;

                SuspendLayout();

                initializeLayoutStyle();

                posRow = 0;
                //Период расчета
                ////Период расчета - подпись
                //ctrl = new System.Windows.Forms.Label();
                //ctrl.Dock = DockStyle.Bottom;
                //(ctrl as System.Windows.Forms.Label).Text = @"Период:";
                //this.Controls.Add(ctrl, 0, posRow);
                //SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                //Период расчета - значение
                ctrl = new ComboBox ();
                ctrl.Name = INDEX_CONTROL.CBX_PERIOD.ToString ();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);                

                //Часовой пояс расчета
                ////Часовой пояс - подпись
                //ctrl = new System.Windows.Forms.Label();
                //ctrl.Dock = DockStyle.Bottom;
                //(ctrl as System.Windows.Forms.Label).Text = @"Часовой пояс:";
                //this.Controls.Add(ctrl, 0, posRow);
                //SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                //Период расчета - значение
                ctrl = new ComboBox();
                ctrl.Name = INDEX_CONTROL.CBX_TIMEZONE.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Расчет - выполнить - норматив
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"К нормативу";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Расчет - выполнить - макет
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"К макету";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Дата/время начала периода расчета
                //Дата/время начала периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время начала периода расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker(today.Year, today.Month, today.Day, 0, null);
                ctrl.Name = INDEX_CONTROL.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета
                //Дата/время  окончания периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время  окончания периода расчета:";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета - значения
                ctrl = new HDateTimePicker(today.Year, today.Month, today.Day, 1, Controls.Find(INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);

                //Признаки включения/исключения из расчета
                //Признаки включения/исключения из расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить из расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Признак для включения/исключения из расчета компонента
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения из расчета параметра
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem (@"Входные значения"));
                ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Учтенные значения"));
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Кнопка - импортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_IMPORT.ToString();
                ctrl.Text = @"Импорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                ctrl.Enabled = false;
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);                
                //Кнопка - экспортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                ctrl.Enabled = false;

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения для отображения параметра
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                ResumeLayout(false);
                PerformLayout();
            }
            
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly ();
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
            private void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEnd = obj as HDateTimePicker;
                m_dtRange.Set(hdtpEnd.LeadingValue, hdtpEnd.Value);

                DateTimeRangeValue_Changed(this, EventArgs.Empty);
            }
        }
    }

    public partial class PanelTaskTepValues
    {
        protected enum INDEX_CONTROL { UNKNOWN = -1
            , BUTTON_RUN
            , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END
            , CLBX_COMP_CALCULATED, CLBX_PARAMETER_CALCULATED
            , BUTTON_LOAD, BUTTON_SAVE, BUTTON_IMPORT, BUTTON_EXPORT
            , CLBX_COMP_VISIBLED, CLBX_PARAMETER_VISIBLED
            , DGV_DATA
            , LABEL_DESC }

        protected PanelManagement m_panelManagement;
        protected DataGridViewTEPValues m_dgvValues;
    }

    public class PlugInTepTaskValues : HFuncDbEdit
    {
        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
