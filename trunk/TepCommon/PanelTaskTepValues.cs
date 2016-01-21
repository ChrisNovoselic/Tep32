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
    public abstract partial class PanelTaskTepValues : PanelTaskTepCalculate
    {
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Оригигальная таблица со значениями для отображения
        /// </summary>
        protected abstract DataTable m_TableOrigin { get; }
        /// <summary>
        /// Редактируемая (с внесенными изменениями)
        ///  таблица со значениями для отображения
        /// </summary>
        protected abstract DataTable m_TableEdit { get; }
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepValues(IPlugIn iFunc, string strNameTableAlg, string strNameTablePut, string strNameTableValues)
            : base(iFunc, strNameTableAlg, strNameTablePut, strNameTableValues)
        {
            InitializeComponents();
            //Обязательно наличие объекта - панели управления
            activateDateTimeRangeValue_OnChanged(true);
            (m_dgvValues as DataGridViewTEPValues).EventCellValueChanged += new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventHandler(onEventCellValueChanged);
        }
        /// <summary>
        /// Инициализация элементов управления объекта
        /// </summary>
        private void InitializeComponents()
        {
            m_panelManagement = new PanelManagement ();
            m_dgvValues = new DataGridViewTEPValues ();
            int posColdgvTEPValues = 4
                , heightRowdgvTEPValues = 10;

            SuspendLayout ();

            //initializeLayoutStyle ();

            Controls.Add (m_panelManagement, 0, 0);
            SetColumnSpan(m_panelManagement, posColdgvTEPValues); SetRowSpan(m_panelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvTEPValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvTEPValues); SetRowSpan(m_dgvValues, heightRowdgvTEPValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvTEPValues, heightRowdgvTEPValues);

            ResumeLayout (false);
            PerformLayout ();

            //(Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            Button btn = (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
        }

        protected override void initialize()
        {
            CheckedListBox clbxCompCalculated
                , clbxCompVisibled;
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            bool bChecked = false;

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
        }
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(int iCtrl = (int)INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            CheckedListBox clbx = null;
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;
            // в базовом классе 'indxCtrl' все равно не известен
            base.clear(iCtrl, bClose);

            if (bClose == true)
            {
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
            }
            else
                ;
        }
        /// <summary>
        /// Остановить сопутствующие объекты
        /// </summary>
        public override void Stop()
        {
            clear((int)INDEX_CONTROL.UNKNOWN, true);
            
            base.Stop();
        }
        /// <summary>
        /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
        /// </summary>
        protected string getQueryValuesVar ()
        {
            string strRes = string.Empty;

            int i = -1;
            bool bEndMonthBoudary = false
                , bLastItem = false
                , bEquDatetime = false;
            DateTimeRange[] arQueryRanges = null;
            // привести дату/время к UTC
            DateTime dtBegin = m_panelManagement.m_dtRange.Begin.AddMinutes (-1 * _curOffsetUTC)
                , dtEnd = m_panelManagement.m_dtRange.End.AddMinutes (-1 * _curOffsetUTC);
            arQueryRanges = new DateTimeRange[(dtEnd.Month - dtBegin.Month) + 12 * (dtEnd.Year - dtBegin.Year) + 1];
            bEndMonthBoudary = HDateTime.IsMonthBoundary(dtEnd);
            if (bEndMonthBoudary == false)
                if (arQueryRanges.Length == 1)
                    // самый простой вариант - один элемент в массиве - одна таблица
                    arQueryRanges[0] = new DateTimeRange(dtBegin, dtEnd);
                else
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц
                    for (i = 0; i < arQueryRanges.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arQueryRanges[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary (dtBegin));
                        else
                            if (i == arQueryRanges.Length - 1)
                                // крайний элемент массива
                                arQueryRanges[i] = new DateTimeRange(arQueryRanges[i - 1].End, dtEnd);
                            else
                                // для элементов в "середине" массива
                                arQueryRanges[i] = new DateTimeRange(arQueryRanges[i - 1].End, HDateTime.ToNextMonthBoundary (arQueryRanges[i - 1].End));
            else
                if (bEndMonthBoudary == true)
                    // два ИЛИ более элементов в массиве - две ИЛИ болле таблиц ('diffMonth' всегда > 0)
                    // + использование следующей за 'dtEnd' таблицы
                    for (i = 0; i < arQueryRanges.Length; i++)
                        if (i == 0)
                            // предыдущих значений нет
                            arQueryRanges[i] = new DateTimeRange(dtBegin, HDateTime.ToNextMonthBoundary(dtBegin));
                        else
                            if (i == arQueryRanges.Length - 1)
                                // крайний элемент массива
                                arQueryRanges[i] = new DateTimeRange(arQueryRanges[i - 1].End, dtEnd);
                            else
                                // для элементов в "середине" массива
                                arQueryRanges[i] = new DateTimeRange(arQueryRanges[i - 1].End, HDateTime.ToNextMonthBoundary(arQueryRanges[i - 1].End));
                else
                    ;

            for (i = 0; i < arQueryRanges.Length; i ++)
            {
                bLastItem = ! (i < (arQueryRanges.Length - 1));

                strRes += @"SELECT v.ID_INPUT, v.ID_TIME, v.ID_TIMEZONE"
                        + @", v.ID_SOURCE"
                        + @", " + _IdSession + @" as [ID_SESSION], v.QUALITY"
                        + @", [VALUE]"
                        //+ @", GETDATE () as [WR_DATETIME]"
                    + @" FROM [dbo].[" + m_strNameTableValues + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                    + @" WHERE v.[ID_TIME] = " + (int)ActualIdPeriod //???ID_PERIOD.HOUR //??? _currIdPeriod
                    ;
                // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                // 'Begin' == 'End'
                if (bLastItem == true)
                    bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);
                else
                    ;

                if (bEquDatetime == false)
                    strRes += @" AND [DATE_TIME] > '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                        + @" AND [DATE_TIME] <= '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";
                else
                    strRes += @" AND [DATE_TIME] = '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                if (bLastItem == false)
                    strRes += @" UNION ";
                else
                    ;
            }

            strRes = @"SELECT p.ID, " + HTepUsers.Id + @" as [ID_USER]"
                    + @", v.ID_SOURCE"
                    + @", " + _IdSession + @" as [ID_SESSION]"
                    + @", CASE"
                        + @" WHEN COUNT (*) = " + CountBasePeriod + @" THEN MIN(v.[QUALITY])"
                        + @" WHEN COUNT (*) = 0 THEN " + (int)ID_QUALITY_VALUE.NOT_REC
			                + @" ELSE " + (int)ID_QUALITY_VALUE.PARTIAL
		                + @" END as [QUALITY]"
                    + @", CASE"
                        + @" WHEN m.[AVG] = 0 THEN SUM (v.[VALUE])"
                        + @" WHEN m.[AVG] = 1 THEN AVG (v.[VALUE])"
                            + @" ELSE MIN (v.[VALUE])"
                        + @" END as [VALUE]"
                    + @", GETDATE () as [WR_DATETIME]"
                + @" FROM (" + strRes + @") as v"
                    + @" LEFT JOIN [dbo].[" + m_strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
                    + @" LEFT JOIN [dbo].[" + m_strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
                    + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                + @" GROUP BY v.ID_INPUT, v.ID_SOURCE, v.ID_TIME, v.ID_TIMEZONE, v.QUALITY"
                    + @", a.ID_MEASURE, a.N_ALG"
                    + @", p.ID, p.ID_ALG, p.ID_COMP, p.MAXVALUE, p.MINVALUE"
                    + @", m.[AVG]"
                ;

            //strRes = @"SELECT"
            //    + @" p.ID"
            //    + @", " + HUsers.Id //ID_USER
            //    + @", v.ID_SOURCE"
            //    + @", 0" //ID_SESSION
            //    //+ @", CAST ('" + m_panelManagement.m_dtRange.Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
            //    //+ @", CAST ('" + m_panelManagement.m_dtRange.End.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
            //    //+ @", v.ID_TIME"
            //    //+ @", v.ID_TIMEZONE"
            //    + @", v.QUALITY"
            //    + @", CASE WHEN m.[AVG] = 0 THEN SUM (v.[VALUE])"
            //        + @" WHEN m.[AVG] = 1 THEN AVG (v.[VALUE])"
            //        + @" ELSE MIN (v.[VALUE]) END as VALUE"
            //    + @", GETDATE ()"
            //    + @" FROM [dbo].[" + m_strNameTableValues + @"_" + getNameTableValuesPostfixDatetime(dtBegin, dtEnd) + @"] v"
            //        + @" LEFT JOIN [dbo].[" + m_strNameTablePut + @"] p ON p.ID = v.ID_INPUT"
            //        + @" LEFT JOIN [dbo].[" + m_strNameTableAlg + @"] a ON p.ID_ALG = a.ID"
            //        + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
            //    + @" WHERE [ID_TIME] = " + (int)ID_PERIOD.HOUR //(int)_currIdPeriod
            //        + @" AND [DATE_TIME] > CAST ('" + dtBegin.ToString(@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
            //        + @" AND [DATE_TIME] <= CAST ('" + dtEnd.ToString (@"yyyyMMdd HH:mm:ss") + @"' as datetime2)"
            //    + @" GROUP BY v.ID_INPUT, v.ID_SOURCE, v.ID_TIME, v.ID_TIMEZONE, v.QUALITY"
            //        + @", a.ID_MEASURE, a.N_ALG"
            //        + @", p.ID, p.ID_ALG, p.ID_COMP, p.MAXVALUE, p.MINVALUE"
            //        + @", m.[AVG]"
            //        ;

            return strRes;
        }
        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="dbConn">Ссылка на объектт соединения с БД</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при галичии ошибки</param>
        protected abstract void setValues(ref DbConnection dbConn, out int err, out string strErr);
        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        /// <param "idPeriod">Идентификатор периода расчета
        ///  в случае загрузки "сырых" значений = ID_PERIOD.HOUR
        ///  в случае загрузки "учтенных" значений -  в зависимости от установленного пользователем</param>
        /// </summary>
        private void updateDataValues()
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1
                , cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1;
            string errMsg = string.Empty;
            // представление очичается в 'clear ()' - при автоматическом вызове, при нажатии на кнопку "Загрузить" (аналог "Обновить")
            DbConnection dbConn = null;
            // получить объект для соединения с БД
            dbConn = DbSources.Sources().GetConnection(iListenerId, out err);
            // проверить успешность получения 
            if ((!(dbConn == null)) && (err == 0))
            {
                _IdSession = HMath.GetRandomNumber();

                setValues(ref dbConn, out err, out errMsg);

                if (err == 0)
                {
                    m_dgvValues.ShowValues(m_TableEdit, m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]);
                }
                else
                    // в случае ошибки "обнулить" идентификатор сессии
                    _IdSession = -1;
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);

            if (!(err == 0))
            {
                throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
            }
            else
                ;
        }        
        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.HISTORY;

            onButtonLoadClick();
        }

        private void onButtonLoadClick()
        {
            // вызов 'reinit()'
            //base.HPanelTepCommon_btnUpdate_Click(obj, ev);
            // для этой вкладки - требуется просто 'clear'
            // очистить содержание представления
            clear();
            // ... - загрузить/отобразить значения из БД
            updateDataValues();
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
                , i1 = -1, i2 = -1
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
                 if ((int.TryParse(arParts1[indx], out i1) == true)
                     && (int.TryParse(arParts2[indx], out i2) == true))
                     iRes = i1 > i2 ? 1
                          : i1 < i2 ? -1 : 0;
                 else
                     iRes = arParts1[indx].CompareTo(arParts2[indx]);

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
        protected override void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
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
            Dictionary<int, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<int, HTepUsers.VISUAL_SETTING>();
            //Установить новое значение для текущего периода
            _currIdPeriod = (ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][cbx.SelectedIndex];
            //Отменить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
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
                // в каждой строке значения полей, относящихся к параметру алгоритма расчета одинаковые, т.к. 'ListParameter' объединение 2-х таблиц
                ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First()) // исключить дублирование по полю [ID_ALG]                
                ;
            //Установки для отображения значений
            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_connSett
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
                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (m_arListIds[(int)INDEX_ID.ALL_PARAMETER].IndexOf(id_alg) < 0)
                {
                    // добавить в список идентификатор параметра алгоритма расчета
                    m_arListIds[(int)INDEX_ID.ALL_PARAMETER].Add(id_alg);

                    strItem = ((string)r[@"N_ALG"]).Trim () + @" (" + ((string)r[@"NAME_SHR"]).Trim() + @")";
                    clbxParsCalculated.Items.Add(strItem, m_arListIds[(int)INDEX_ID.DENY_PARAMETER_CALCULATED].IndexOf(id_alg) < 0);
                    bVisibled = m_arListIds[(int)INDEX_ID.DENY_PARAMETER_VISIBLED].IndexOf(id_alg) < 0;
                    clbxParsVisibled.Items.Add(strItem, bVisibled);
                    // получить значения для настройки визуального отображения
                    if (dictVisualSettings.ContainsKey(id_alg) == true)
                    {// установленные в проекте
                        ratio = dictVisualSettings[id_alg].m_ratio;
                        round = dictVisualSettings[id_alg].m_round;
                    }
                    else
                    {// по умолчанию
                        ratio = HTepUsers.s_iRatioDefault;
                        round = HTepUsers.s_iRoundDefault;
                    }
                    // добавить свойства для строки таблицы со значениями
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
                        //, m_ratio = (int)r[@"ID_RATIO"]
                    });
                }
                else
                    ;
            }
            //Возобновить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
            clbxParsCalculated.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);
            clbxParsVisibled.ItemCheck += new ItemCheckEventHandler(clbx_ItemCheck);

            base.cbxPeriod_SelectedIndexChanged(obj, ev);
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
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected abstract void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev);
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTEPValues : DataGridViewTEPCalculate
        {
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
            public DataGridViewTEPValues () : base ()
            {
                //Разместить ячейки, установить свойства объекта
                InitializeComponents ();
                //Назначить (внутренний) обработчик события - изменение значения ячейки
                // для дальнейшей ретрансляции родительскому объекту
                CellValueChanged += new DataGridViewCellEventHandler (onCellValueChanged);
                //RowsRemoved += new DataGridViewRowsRemovedEventHandler (onRowsRemoved);
            }

            private void InitializeComponents()
            {
                AddColumn (-2, string.Empty, false);
                AddColumn(-1, @"Размерность", true);
            }

            public override void ClearColumns()
            {
                List<HDataGridViewColumn> listIndxToRemove;

                if (Columns.Count > 0)
                {
                    listIndxToRemove = new List<HDataGridViewColumn>();

                    foreach (HDataGridViewColumn col in Columns)
                        if (!(col.m_iIdComp < 0))
                            listIndxToRemove.Add(col);
                        else
                            ;

                    while (listIndxToRemove.Count > 0)
                    {
                        Columns.Remove(listIndxToRemove[0]);
                        listIndxToRemove.RemoveAt(0);
                    }
                }
                else
                    ;
            }
            /// <summary>
            /// Удалить строки
            /// </summary>
            public override void ClearRows()
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
            public override void AddColumn (int id_comp, string text, bool bVisibled)
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

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (id_comp < 0)
                        {// для служебных столбцов
                            if (bVisibled == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;                                
                            }
                            else
                                ;

                            column.Frozen = true;
                            column.ReadOnly = true;
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
            public override void AddRow(ROW_PROPERTY rowProp)
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
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                int id_alg = (int)Rows[iRow].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;

                bRes = ((m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_bCalcDeny == false)
                    && ((m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].IsNaN == false)));
                if (bRes == true)
                    if ((bNewCalcDeny == true)
                        && (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_bCalcDeny == false))
                        switch (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality)
                        {//??? LIMIT
                            case ID_QUALITY_VALUE.USER:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                break;
                            case ID_QUALITY_VALUE.SOURCE:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                break;
                            case ID_QUALITY_VALUE.PARTIAL:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                break;
                            case ID_QUALITY_VALUE.NOT_REC:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                break;
                            case ID_QUALITY_VALUE.DEFAULT:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                                break;
                            default:
                                ; //??? throw
                                break;
                        }
                    else
                        clrRes = s_arCellColors[(int)INDEX_COLOR.CALC_DENY];
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
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                int id_alg = (int)Rows[iRow].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;

                bRes = m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].IsNaN == false;
                if (bRes == true)
                    if ((bNewCalcDeny == true)
                        && ((Columns[iCol] as HDataGridViewColumn).m_bCalcDeny == false))
                        switch (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality)
                        {//??? LIMIT
                            case ID_QUALITY_VALUE.USER:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                break;
                            case ID_QUALITY_VALUE.SOURCE:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                break;
                            case ID_QUALITY_VALUE.PARTIAL:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                break;
                            case ID_QUALITY_VALUE.NOT_REC:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                break;
                            case ID_QUALITY_VALUE.DEFAULT:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                                break;
                            default:
                                ; //??? throw
                                break;
                        }
                    else
                        clrRes = s_arCellColors[(int)INDEX_COLOR.CALC_DENY];
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
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];

                if (bRes == true)
                    switch (m_dictPropertiesRows[id_alg].m_arPropertiesCells[iCol].m_iQuality)
                    {//??? USER, LIMIT
                        case ID_QUALITY_VALUE.DEFAULT: // только для входной таблицы - значение по умолчанию [inval_def]
                            clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                            break;
                        case ID_QUALITY_VALUE.PARTIAL: // см. 'getQueryValuesVar' - неполные данные
                            clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                            break;
                        case ID_QUALITY_VALUE.NOT_REC: // см. 'getQueryValuesVar' - нет ни одной записи
                            clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                            break;
                        default:
                            clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                            break;
                    }
                else
                    clrRes = s_arCellColors[(int)INDEX_COLOR.NAN];

                return bRes;
            }
            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public override void UpdateStructure(PanelTaskTepValues.INDEX_ID indxDeny, int id, bool bItemChecked)
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
            public override void ShowValues(DataTable values, DataTable parameter)
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
                            m_dictPropertiesRows[idAlg].m_arPropertiesCells[iCol].m_iQuality = (ID_QUALITY_VALUE)iQuality;
                            row.Cells[iCol].ReadOnly = double.IsNaN(dblVal);

                            if (getClrCellToValue(iCol, iRow, out clrCell) == true)
                            {
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                ratioValue =
                                    //m_dictRatio[m_dictPropertiesRows[idAlg].m_ratio].m_value
                                    m_dictRatio[(int)parameterRows[0][@"ID_RATIO"]].m_value
                                    ;
                                if (!(ratioValue == vsRatioValue))
                                {
                                    row.Cells[(int)INDEX_SERVICE_COLUMN.SYMBOL].Value = m_dictPropertiesRows[idAlg].m_strSymbol
                                        + @",[" + m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_nameRU + m_dictPropertiesRows[idAlg].m_strMeasure + @"]";
                                    dblVal *= Math.Pow(10F, ratioValue > vsRatioValue ? vsRatioValue : -1 * vsRatioValue);
                                }
                                else
                                    ;
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
            public override void ClearValues()
            {
                CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                        {
                            c.Value = string.Empty;
                            c.Style.BackColor = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                        }
                        else
                            ;
                //??? если установить 'true' - редактирование невозможно
                ReadOnly = false;

                CellValueChanged += new DataGridViewCellEventHandler (onCellValueChanged);
            }
            /// <summary>
            /// обработчик события - изменение значения в ячейке
            /// </summary>
            /// <param name="obj">Обхект, иницировавший событие</param>
            /// <param name="ev">Аргумент события</param>
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

            private void onRowsRemoved(object obj, DataGridViewRowsRemovedEventArgs ev)
            {
            }
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagement : PanelManagementCalculate
        {
            public PanelManagement() : base ()
            {
                InitializeComponents ();
            }

            private void InitializeComponents ()
            {
                Control ctrl = null;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню лдя кнопки "Обновить-Загрузить"                

                SuspendLayout();

                //initializeLayoutStyle();

                posRow = 0;
                //Период расчета
                ////Период расчета - подпись
                //ctrl = new System.Windows.Forms.Label();
                //ctrl.Dock = DockStyle.Bottom;
                //(ctrl as System.Windows.Forms.Label).Text = @"Период:";
                //this.Controls.Add(ctrl, 0, posRow);
                //SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                //Период расчета - значение
                ctrl = Controls.Find(INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                this.Controls.Remove(ctrl);
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
                ctrl = Controls.Find(INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                this.Controls.Remove(ctrl);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Расчет - выполнить - норматив
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_NORM.ToString();
                ctrl.Text = @"К нормативу";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Расчет - выполнить - макет
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_MKT.ToString();
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
                ctrl = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker;
                this.Controls.Remove(ctrl);
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
                ctrl = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                this.Controls.Remove(ctrl);
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
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem (@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Учтенные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
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

                if (! (DateTimeRangeValue_Changed == null))
                    DateTimeRangeValue_Changed(this, EventArgs.Empty);
                else
                    ;
            }
        }
    }

    public partial class PanelTaskTepValues
    {
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , BUTTON_RUN_NORM, BUTTON_RUN_MKT, BUTTON_INVAL
            , CLBX_COMP_CALCULATED, CLBX_PARAMETER_CALCULATED
            , BUTTON_LOAD, MENUITEM_UPDATE, MENUITEM_HISTORY, BUTTON_SAVE, BUTTON_IMPORT, BUTTON_EXPORT
            , CLBX_COMP_VISIBLED, CLBX_PARAMETER_VISIBLED
            , DGV_DATA
            , LABEL_DESC
                ,
        }
    }
}
