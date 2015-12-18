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
        private enum INDEX_TABLE_VALUES : int { VARIABLE, DEFAULT, COUNT }
        private enum INDEX_TABLE_DICTPRJ : int { UNKNOWN = -1, PERIOD, COMPONENT, PARAMETER
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
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        private DataTable []m_arTableDictPrjs;
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID { UNKNOWN = -1
            , PERIOD // идентификаторы периодов расчетов, использующихся на форме
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
            m_dgvValues.EventCellValueChanged += new DelegateObjectFunc(onEventCellValueChanged);
        }

        private void InitializeComponents()
        {
            #region Код, не относящийся к инициализации элементов управления
            m_arListIds = new List<int>[(int)INDEX_ID.COUNT_INDEX_ID];
            for (INDEX_ID i = INDEX_ID.PERIOD; i < INDEX_ID.COUNT_INDEX_ID; i++)
                if (i == INDEX_ID.PERIOD)
                    m_arListIds[(int)i] = new List<int> { (int)ID_TIME.HOUR, (int)ID_TIME.SHIFTS, (int)ID_TIME.DAY, (int)ID_TIME.MONTH };
                else
                    //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                    m_arListIds[(int)i] = new List<int>();
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

        protected override void initialize(ref System.Data.Common.DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            Control ctrl = null;
            CheckedListBox clbxCompCalculated
                , clbxCompVisibled;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            bool bVisibled = false;
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
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Clear();
                //Заполнить элементы управления с компонентами станции
                clbxCompCalculated = Controls.Find(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString(), true)[0] as CheckedListBox;
                clbxCompVisibled = Controls.Find(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString(), true)[0] as CheckedListBox;
                foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
                {
                    id_comp = (Int16)r[@"ID"];
                    m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
                    strItem = (string)r[@"DESCRIPTION"];
                    clbxCompCalculated.Items.Add(strItem, m_arListIds[(int)INDEX_ID.DENY_COMP_CALCULATED].IndexOf(id_comp) < 0);
                    bVisibled = m_arListIds[(int)INDEX_ID.DENY_COMP_VISIBLED].IndexOf(id_comp) < 0;
                    clbxCompVisibled.Items.Add(strItem, bVisibled);
                    m_dgvValues.AddColumn(id_comp, strItem, bVisibled);
                }

                clbxCompCalculated.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);
                clbxCompVisibled.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);

                //Заполнить элемент управления с периодами расчета
                ctrl = Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0];
                foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                    (ctrl as ComboBox).Items.Add (r[@"DESCRIPTION"]);

                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                (ctrl as ComboBox).SelectedIndex = 0;                
            }
            else
                switch ((INDEX_TABLE_DICTPRJ)i)
                {
                    case INDEX_TABLE_DICTPRJ.PERIOD:
                        errMsg = @"Получение интервалов времени для периода расчета";
                        break;
                    case INDEX_TABLE_DICTPRJ.COMPONENT:
                        errMsg = @"Получение списка компонентов станции";
                        break;
                    case INDEX_TABLE_DICTPRJ.PARAMETER:
                        errMsg = @"Получение строковых идентификаторов параметров в алгоритме расчета";
                        break;
                    default:
                        break;
                }
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            return bRes;
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        private string[] queryDictPrj
        {
            get
            {
                return new string[]
                {
                    //PERIOD
                    @"SELECT * FROM [time] WHERE [ID] IN (" + m_strIdPeriods + @")"
                    // список компонентов
                    , @"SELECT * FROM [comp_list] "
                        + @"WHERE ([ID] = 5 AND [ID_COMP] = 1)"
                            + @" OR ([ID_COMP] = 1000)"
                    // параметры расчета
                    , @"SELECT put.*, alg.* FROM [dbo].[" + m_strNameTablePut + @"] as put"
                        + @" JOIN [dbo].[" + m_strNameTableAlg + @"] as alg ON alg.ID_TASK = 1 AND alg.ID = put.ID_ALG"
                            //+ @" AND put.ID_TIME in (" + m_strIdPeriods + @")"
                };
            }
        }

        private string queryValuesVar
        {
            get
            {
                return @"SELECT"
	                + @" p.ID"
	                + @", 0" //ID_USER
	                + @", v.ID_SOURCE"
	                + @", 0" //ID_SESSION
                    + @", CAST ('" + m_panelManagement.m_dtRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    + @", CAST ('" + m_panelManagement.m_dtRange.End.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
	                + @", v.ID_TIME"
	                + @", v.ID_TIMEZONE"
	                + @", v.QUALITY"
	                + @", CASE WHEN m.[AVG] = 0 THEN SUM (v.[VALUE])"
		                + @" WHEN m.[AVG] = 1 THEN AVG (v.[VALUE])"
		                + @" ELSE MIN (v.[VALUE]) END as VALUE"
	                + @", GETDATE ()"
                    + @" FROM [dbo].[" + m_strNameTableValues + @"_201512] v"
	                    + @" LEFT JOIN [dbo].[" + m_strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
                        + @" LEFT JOIN [dbo].[" + m_strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
	                    + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                    + @" WHERE [ID_TIME] = " + CurrIdPeriod
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
                int cnt = (int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0;

                return @"SELECT"
                    + @" p.ID"
                    + @", -1" //ID_USER
                    + @", -1"
                    + @", 0" //ID_SESSION
                    + @", CAST ('" + m_panelManagement.m_dtRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    + @", CAST ('" + m_panelManagement.m_dtRange.End.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
                    + @", -1"
                    + @", -1"
                    + @", -1"
                    + @", CASE WHEN m.[AVG] = 0 THEN " + cnt + @" * v.[VALUE]"
                        + @" WHEN m.[AVG] = 1 THEN v.[VALUE]"
                        + @" ELSE v.[VALUE] END as VALUE"
                    + @", GETDATE ()"
                    + @" FROM [dbo].[" + m_strNameTableValues + @"_def] v"
                        + @" LEFT JOIN [dbo].[" + m_strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
                        + @" LEFT JOIN [dbo].[" + m_strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
                        + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                    + @" WHERE [ID_TIME] = " + CurrIdPeriod
                        ;
            }
        }
        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        /// </summary>
        private void updateDataValues()
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1;
            string errMsg = string.Empty
                , query = string.Empty;
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsToRemove = null;
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
                            rowsToRemove = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID=" + rValVar[@"ID"]);
                            foreach (DataRow rToRemove in rowsToRemove)
                                m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Rows.Remove(rToRemove);
                        }
                        // создать копии для возможности сохранения изменений
                        m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Copy();
                        m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();

                        m_dgvValues.ShowValues(m_arTableEdit, m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]);
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
        /// Очистить обновляемые значения
        ///  после нажатия кнопки "Обновить"
        ///  и  перед отображением новых значений
        /// </summary>
        protected override void clear()
        {
            //base.clear();

            // очистить содержание представления
            m_dgvValues.ClearValues();
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
        private int CurrIdPeriod
        {
            get
            {
                return m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex];
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
            int id_alg = -1;
            string strItem = string.Empty;
            bool bVisibled = false;
            CheckedListBox clbxParsCalculated = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString(), true)[0] as CheckedListBox
                , clbxParsVisibled = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString(), true)[0] as CheckedListBox;
            //Отменить обработку событий
            clbxParsCalculated.ItemCheck -= clbx_ItemCheck;
            clbxParsVisibled.ItemCheck -= clbx_ItemCheck;
            //Очистиить списки
            clbxParsCalculated.Items.Clear();
            clbxParsVisibled.Items.Clear();
            m_arListIds[(int)INDEX_ID.ALL_PARAMETER].Clear();
            //??? проверить сохранены ли значения
            m_dgvValues.ClearRows();
            ////Запросить значения у главной формы
            //((PlugInBase)_iFuncPlugin).DataAskedHost(new object[] { (int)HFunc.ID_DATAASKED_HOST.SELECT, @"SELECT..." });
            IEnumerable<DataRow> listParameter =
                //ListParameter.Select(par => (string)par[@"ID_ALG"]).Distinct() as IEnumerable<DataRow>
                ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First())
                ;
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
                    m_dgvValues.AddRow(id_alg, ((string)r[@"N_ALG"]).Trim(), ((string)r[@"NAME_SHR"]).Trim(), bVisibled);
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
            switch ((ID_TIME)CurrIdPeriod)
            {
                case ID_TIME.HOUR:
                    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    break;
                case ID_TIME.SHIFTS:
                    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    break;
                case ID_TIME.DAY:
                    hdtpBegin.Mode = HDateTimePicker.MODE.DAY;
                    hdtpEnd.Mode = HDateTimePicker.MODE.DAY;
                    break;
                case ID_TIME.MONTH:
                    hdtpBegin.Mode = HDateTimePicker.MODE.MONTH;
                    hdtpEnd.Mode = HDateTimePicker.MODE.MONTH;
                    break;
                case ID_TIME.YEAR:
                    hdtpBegin.Mode = HDateTimePicker.MODE.YEAR;
                    hdtpEnd.Mode = HDateTimePicker.MODE.YEAR;
                    break;
                default:
                    break;
            }
            m_handlerDb.Load((ID_TIME)CurrIdPeriod);
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
        private void onEventCellValueChanged(object pars)
        {
            int id_par = (int)(pars as object [])[0]
                , id_comp = (int)(pars as object[])[1];
            double val = (double)(pars as object[])[2];
        }
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTEPValues : DataGridView
        {
            public DelegateObjectFunc EventCellValueChanged;
            /// <summary>
            /// Структура с дополнительными свойствами ячейки отображения
            /// </summary>
            private struct HDataGridViewCell //: DataGridViewCell
            {
                public enum INDEX_CELL_PROPERTY : uint { CALC_DENY, IS_NAN }
                /// <summary>
                /// Признак запрета расчета
                /// </summary>
                public bool m_bCalcDeny;
                /// <summary>
                /// Признак отсутствия значения
                /// </summary>
                public bool m_bIsNaN;

                public HDataGridViewCell(bool bCalcDeny, bool bIsNaN)
                {
                    m_bCalcDeny = bCalcDeny;
                    m_bIsNaN = bIsNaN;
                }

                //public bool IsCanColorChange { get { return ((m_bCalcDeny == false) && (m_bIsNaN == false)); } }
            }

            private List<HDataGridViewCell []> m_listPropertiesRows;

            //private List<bool> m_listCalcDenyRows;
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                public int m_iIdComp;
                public bool m_bCalcDeny;
            }

            public DataGridViewTEPValues ()
            {
                m_listPropertiesRows = new List<HDataGridViewCell []>();

                InitializeComponents ();

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

                AddColumn (-1, string.Empty, false);
            }
            /// <summary>
            /// Удалить строки
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                {
                    Rows.Clear();

                    m_listPropertiesRows.Clear();
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
                int indxColTEC = -1;
                foreach (HDataGridViewColumn col in Columns)
                    if ((col.m_iIdComp > 0)
                        && (col.m_iIdComp < 1000))
                        indxColTEC = Columns.IndexOf(col);
                    else
                        ;

                DataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = id_comp, m_bCalcDeny = false };
                column.HeaderText = text;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                if (!(indxColTEC < 0))
                    Columns.Insert(indxColTEC, column);
                else
                    Columns.Add(column);

                column.Visible = bVisibled;
            }
            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="id_par">Идентификатор параметра алгоритма</param>
            /// <param name="headerText">Текст заголовка строки</param>
            /// <param name="toolTipText">Текст подсказки для заголовка строки</param>
            /// <param name="bVisibled">Признак отображения строки</param>
            public void AddRow(int id_par, string headerText, string toolTipText, bool bVisibled)
            {
                int i = -1
                    , c = -1;
                DataGridViewRow row = new DataGridViewRow ();
                row.HeaderCell.Value = headerText;
                row.HeaderCell.ToolTipText = toolTipText;
                i = Rows.Add(row);
                Rows[i].Cells[0].Value = id_par;
                m_listPropertiesRows.Add(new HDataGridViewCell[Columns.Count]);
                i = m_listPropertiesRows.Count - 1;
                for (c = 0; c < Columns.Count; c++)
                    m_listPropertiesRows[i][c] = new HDataGridViewCell(false, true);
            }
            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public void UpdateStructure(PanelTaskTepValues.INDEX_ID indxDeny, int id, bool bCheckedItem)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                bool bIsCanColorChange = false;
                int indx = -1
                    , cIndx = -1;
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
                            if ((int)r.Cells[0].Value == id)
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
                            // для всех ячеек в столбце
                            foreach (DataGridViewRow r in Rows)
                            {
                                bIsCanColorChange = ((m_listPropertiesRows[Rows.IndexOf(r)][indx].m_bCalcDeny == false)
                                    && (m_listPropertiesRows[Rows.IndexOf(r)][indx].m_bIsNaN == false));
                                if (bIsCanColorChange == true)
                                {
                                    clrCell = ((bCheckedItem == true) && (m_listPropertiesRows[Rows.IndexOf(r)][indx].m_bCalcDeny == false)) ? Color.White : Color.LightGray;
                                    r.Cells[indx].Style.BackColor = clrCell;
                                }
                                else
                                    ;
                            }
                            (Columns[indx] as HDataGridViewColumn).m_bCalcDeny = ! bCheckedItem;
                            break;
                        case INDEX_ID.DENY_PARAMETER_CALCULATED:
                            // для всех ячеек в строке
                            foreach (DataGridViewCell c in Rows[indx].Cells)
                            {
                                cIndx = Rows[indx].Cells.IndexOf(c);
                                bIsCanColorChange = m_listPropertiesRows[indx][cIndx].m_bIsNaN == false;
                                if (bIsCanColorChange == true)
                                {
                                    clrCell = ((bCheckedItem == true) && ((Columns[cIndx] as HDataGridViewColumn).m_bCalcDeny == false)) ? Color.White : Color.LightGray;
                                    c.Style.BackColor = clrCell;
                                }
                                else
                                    ;

                                m_listPropertiesRows[indx][cIndx].m_bCalcDeny = !bCheckedItem;
                            }
                            break;
                        case INDEX_ID.DENY_COMP_VISIBLED:
                            // для всех ячеек в столбце
                            Columns[indx].Visible = bCheckedItem;
                            break;
                        case INDEX_ID.DENY_PARAMETER_VISIBLED:
                            // для всех ячеек в строке
                            Rows[indx].Visible = bCheckedItem;
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
            public void ShowValues(DataTable [] values, DataTable parameter)
            {
                int idAlg = -1
                    , iCol = 0
                    , iRow = 0;
                double dblVal = -1F;
                DataRow[] cellVarRows = null
                    , cellDefRows = null
                    , parameterRows = null;
                Color clrCell = Color.Empty;

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (iCol > 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            dblVal = double.NaN;
                            idAlg = (int)row.Cells[0].Value;
                            parameterRows = parameter.Select(@"ID_COMP=" + col.m_iIdComp + @" AND " + @"ID_ALG=" + idAlg);
                            if (parameterRows.Length == 1)
                            {
                                cellVarRows = values[(int)INDEX_TABLE_VALUES.VARIABLE].Select(@"ID=" + parameterRows[0][@"ID"]);
                                cellDefRows = values[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID=" + parameterRows[0][@"ID"]);

                                if (cellVarRows.Length == 1)
                                    dblVal = ((double)cellVarRows[0][@"VALUE"]);
                                else
                                    if (cellDefRows.Length == 1)
                                        dblVal = ((double)cellDefRows[0][@"VALUE"]);
                                    else
                                        ; // continue
                            }
                            else
                                ; // параметр расчета для компонента станции не найден

                            m_listPropertiesRows[Rows.IndexOf(row)][iCol].m_bIsNaN =
                            row.Cells[iCol].ReadOnly =
                                double.IsNaN(dblVal);
                            clrCell = Color.White;

                            if (double.IsNaN(dblVal) == false)
                                row.Cells[iCol].Value = dblVal.ToString(@"F1", System.Globalization.CultureInfo.InvariantCulture);
                            else
                                clrCell = Color.Gray;

                            row.Cells[iCol].Style.BackColor = clrCell;

                            iRow++;
                        }
                    else
                        ;

                    iCol++;
                }
            }
            /// <summary>
            /// Очистить содержание представления (например, перед )
            /// </summary>
            public void ClearValues()
            {
                CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > 0) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;
                        else
                            ;

                CellValueChanged += new DataGridViewCellEventHandler (onCellValueChanged);
            }

            private void onCellValueChanged(object obj, DataGridViewCellEventArgs ev)
            {
                string strValue = string.Empty;
                double dblValue = double.NaN;

                if (ev.ColumnIndex > 0)
                {
                    strValue = (string)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value;

                    if (double.TryParse(strValue, out dblValue) == true)
                        EventCellValueChanged(new object[] { Rows[ev.RowIndex].Cells[0].Value //Идентификатор параметра
                            , (Columns[ev.ColumnIndex] as HDataGridViewColumn).m_iIdComp //Идентификатор компонента
                            , dblValue });
                    else
                        ; //??? невозможно преобразовать значение - отобразить сообщение для пользователя
                }
                else
                    ; // в 0-ом столбце идентификатор параметра расчета
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
                //Расчет - выполнить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"Выполнить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
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
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
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
            , CBX_PERIOD, HDTP_BEGIN, HDTP_END
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
