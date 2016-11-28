using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskTepMain
{
    public abstract partial class PanelTaskTepValues : PanelTaskTepCalculate
    {
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;

        ///// <summary>
        ///// Оригигальная таблица со значениями для отображения
        ///// </summary>
        //protected abstract DataTable m_TableOrigin { get; }
        ///// <summary>
        ///// Редактируемая (с внесенными изменениями)
        /////  таблица со значениями для отображения
        ///// </summary>
        //protected abstract DataTable m_TableEdit { get; }

        protected System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableOrigin[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableEdit[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }
        /// <summary>
        /// Событие для добавления основного параметра для панели управления
        /// </summary>
        protected event DelegateObjectFunc eventAddNAlgParameter;
        /// <summary>
        /// Событие для добавления детализированного (компонент) параметра для панели управления
        /// </summary>
        protected event DelegateObjectFunc eventAddCompParameter;
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepValues(IPlugIn iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc, type)
        {
            m_arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponents();
            // назначить обработчики для кнопок 'Результат'
            (Controls.Find(INDEX_CONTROL.BUTTON_RUN_RES.ToString(), true)[0] as Button).Click += new EventHandler(btnRunRes_onClick);
            //Обязательно наличие объекта - панели управления
            activateDateTimeRangeValue_OnChanged(true);
            (m_dgvValues as DataGridViewTEPValues).EventCellValueChanged += new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventHandler(onEventCellValueChanged);
            (m_dgvValues as DataGridViewTEPValues).SelectionChanged += new EventHandler(m_dgvValues_SelectionChanged);
        }
        /// <summary>
        /// Инициализация элементов управления объекта
        /// </summary>
        private void InitializeComponents()
        {
            m_dgvValues = new DataGridViewTEPValues();
            int posColdgvTEPValues = 4
                , heightRowdgvTEPValues = 10;

            SuspendLayout();

            //initializeLayoutStyle ();

            Controls.Add(PanelManagement, 0, 0);
            SetColumnSpan(PanelManagement, posColdgvTEPValues); SetRowSpan(PanelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvTEPValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvTEPValues); SetRowSpan(m_dgvValues, heightRowdgvTEPValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvTEPValues, heightRowdgvTEPValues);

            ResumeLayout(false);
            PerformLayout();

            //(Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            Button btn = (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);

            (PanelManagement as PanelManagementTaskTepValues).ItemCheck += new PanelManagementTaskTepValues.ItemCheckedParametersEventHandler(panelManagement_ItemCheck);
        }

        protected override void initialize()
        {
            string strItem = string.Empty;
            int i = -1
                , id_comp = -1;
            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[] {
                        INDEX_ID.DENY_COMP_CALCULATED
                        , INDEX_ID.DENY_COMP_VISIBLED
                    };
            bool[] arChecked = new bool[arIndxIdToAdd.Length];

            //Заполнить элементы управления с компонентами станции
            foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
            {
                id_comp = (Int16)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
                strItem = ((string)r[@"DESCRIPTION"]).Trim();
                // установить признак участия в расчете компонента станции
                arChecked[0] = m_arListIds[(int)INDEX_ID.DENY_COMP_CALCULATED].IndexOf(id_comp) < 0;
                // установить признак отображения компонента станции
                arChecked[1] = m_arListIds[(int)INDEX_ID.DENY_COMP_VISIBLED].IndexOf(id_comp) < 0;
                (PanelManagement as PanelManagementTaskTepValues).AddComponent(id_comp
                    , strItem
                    , arIndxIdToAdd
                    , arChecked);

                m_dgvValues.AddColumn(id_comp, strItem, arChecked[1]);
            }
            //Установить обработчик события - добавить параметр
            eventAddNAlgParameter += new DelegateObjectFunc((PanelManagement as PanelManagementTaskTepValues).OnAddParameter);
            // установить единый обработчик события - изменение состояния признака участие_в_расчете/видимость
            // компонента станции для элементов управления
            (PanelManagement as PanelManagementTaskTepValues).ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.DENY_COMP_CALCULATED, INDEX_ID.DENY_COMP_VISIBLED });

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
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;
            // в базовом классе 'indxCtrl' все равно не известен
            base.clear(iCtrl, bClose);

            if (bClose == true)
            {
                (PanelManagement as PanelManagementTaskTepValues).Clear();
            }
            else
                ;
        }

        public override bool Activate(bool activate)
        {
            bool bRes = false;
            int err = -1;

            bRes = base.Activate(activate);

            //if (bRes == true) {
            //    if (activate == true) {                    
            //    } else
            //        ;
            //} else
            //    ;

            return bRes;
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
        /// Установить значения таблиц для редактирования
        /// </summary>
        protected void setValues()
        {
            for (TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES indx = (TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.UNKNOWN + 1);
                indx < TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT;
                indx++)
                if (!(m_arTableOrigin[(int)indx] == null))
                    m_arTableEdit[(int)indx] =
                        m_arTableOrigin[(int)indx].Copy();
                else
                    ;
        }

        /// <summary>
        /// Выполнить запрос к БД, отобразить рез-т запроса
        ///  в случае загрузки "сырых" значений = ID_PERIOD.HOUR
        ///  в случае загрузки "учтенных" значений -  в зависимости от установленного пользователем</param>
        /// </summary>
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1
                , iRegDbConn = -1; // признак установленного соединения (ошибка, был создан ранее, новое соединение)
            string errMsg = string.Empty;

            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                // предыдущая сессия удалена в 'clear'
                setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg);

                if (err == 0)
                {
                    // создать копии для возможности сохранения изменений
                    setValues();
                    // отобразить значения
                    m_dgvValues.ShowValues(m_TableEdit, m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]);
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();

                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            }
            else
                ;

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
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
            Session.m_LoadValues = TepCommon.HandlerDbTaskCalculate.SESSION.INDEX_LOAD_VALUES.SOURCE;

            onButtonLoadClick();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
        {
            Session.m_LoadValues = TepCommon.HandlerDbTaskCalculate.SESSION.INDEX_LOAD_VALUES.ARCHIVE;

            onButtonLoadClick();
        }

        protected virtual void onButtonLoadClick()
        {
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
        private int compareNAlg(DataRow r1, DataRow r2)
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
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

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
            string n_alg = string.Empty;
            INDEX_ID[] arIndexIdToAdd = new INDEX_ID[] { INDEX_ID.DENY_PARAMETER_CALCULATED, INDEX_ID.DENY_PARAMETER_VISIBLED };
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][cbx.SelectedIndex]);
            //Отменить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
            (PanelManagement as PanelManagementTaskTepValues).ActivateCheckedHandler(false, arIndexIdToAdd);
            //Очистиить списки - элементы интерфейса
            (PanelManagement as PanelManagementTaskTepValues).Clear(arIndexIdToAdd);
            //Очистить список с параметрами, т.к. он м.б. индивидуален для каждого из периодов расчета
            m_arListIds[(int)INDEX_ID.ALL_NALG].Clear();
            //??? проверить сохранены ли значения
            m_dgvValues.ClearRows();
            //Список параметров для отображения            
            IEnumerable<DataRow> listParameter =
                // в каждой строке значения полей, относящихся к параметру алгоритма расчета одинаковые, т.к. 'ListParameter' объединение 2-х таблиц
                //ListParameter.GroupBy(x => x[@"ID_ALG"]).Select(y => y.First()) // исключить дублирование по полю [ID_ALG]
                ListParameter.Select(x => x);
            //Установки для отображения значений
            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
                , new int[] {
                    //1
                    //, (_iFuncPlugin as PlugInBase)._Id
                    m_id_panel
                    , (int)Session.m_currIdPeriod }
                , out err);
            //Заполнить элементы управления с компонентами станции 
            foreach (DataRow r in listParameter)
            {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();
                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                {
                    // добавить в список идентификатор параметра алгоритма расчета
                    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);

                    strItem = ((string)r[@"N_ALG"]).Trim() + @" (" + ((string)r[@"NAME_SHR"]).Trim() + @")";
                    eventAddNAlgParameter(new PanelManagementTaskTepValues.ADDING_PARAMETER()
                    {
                        m_idNAlg = id_alg
                        ,
                        m_idComp = (int)r[@"ID_COMP"]
                        ,
                        m_idPut = (int)r[@"ID"]
                        ,
                        m_strText = strItem
                        ,
                        m_arIndexIdToAdd = arIndexIdToAdd
                        ,
                        m_arChecked = new bool[] {
                                m_arListIds[(int)INDEX_ID.DENY_PARAMETER_CALCULATED].IndexOf(id_alg) < 0
                                , m_arListIds[(int)INDEX_ID.DENY_PARAMETER_VISIBLED].IndexOf(id_alg) < 0
                            }
                    });
                    // получить значения для настройки визуального отображения
                    if (dictVisualSettings.ContainsKey(n_alg) == true)
                    {// установленные в проекте
                        ratio = dictVisualSettings[n_alg].m_ratio;
                        round = dictVisualSettings[n_alg].m_round;
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
                        ,
                        m_strHeaderText = ((string)r[@"N_ALG"]).Trim()
                        ,
                        m_strToolTipText = ((string)r[@"NAME_SHR"]).Trim()
                        ,
                        m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        ,
                        m_strSymbol = !(r[@"SYMBOL"] is DBNull) ? ((string)r[@"SYMBOL"]).Trim() : string.Empty
                            //, m_bVisibled = bVisibled
                        ,
                        m_vsRatio = ratio
                        ,
                        m_vsRound = round
                        //, m_ratio = (int)r[@"ID_RATIO"]
                    });
                }
                else
                    // параметр уже был добавлен
                    if (!(eventAddCompParameter == null))
                        // только, если назначенн обработчик в 'PanelTaskTepOutVal'
                        eventAddCompParameter(new PanelManagementTaskTepValues.ADDING_PARAMETER()
                        {
                            m_idNAlg = id_alg
                            ,
                            m_idComp = (int)r[@"ID_COMP"]
                            ,
                            m_idPut = (int)r[@"ID"]
                            ,
                            m_strText = strItem
                            ,
                            m_arIndexIdToAdd = new INDEX_ID[] { INDEX_ID.DENY_PARAMETER_CALCULATED }
                            ,
                            m_arChecked = new bool[] {
                                    m_arListIds[(int)INDEX_ID.DENY_PARAMETER_CALCULATED].IndexOf(id_alg) < 0
                            }
                        });
                    else
                        ;
            }
            //Возобновить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
            (PanelManagement as PanelManagementTaskTepValues).ActivateCheckedHandler(true, arIndexIdToAdd);

            base.cbxPeriod_SelectedIndexChanged(obj, ev);
        }
        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_ItemCheck(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //if ((ev.m_address.m_idComp > 0)
            //    && ((ev.m_address.m_idAlg > 0)))
            //{
            //}
            //else
            //    if (ev.m_address.m_idComp > 0)
            //    {
            //    }
            //    else
            //        if (ev.m_address.m_idAlg > 0)
            //        {
            //        }
            //        else
            //            throw new Exception(@"PanelTaskTepValues::panelManagement_ItemCheck () - ...");

            //Изменить признак состояния компонента ТЭЦ/параметра алгоритма расчета
            if (ev.m_newCheckState == CheckState.Unchecked)
                if (m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0)
                    m_arListIds[(int)ev.m_indxIdDeny].Add(idItem);
                else
                    ; //throw new Exception (@"");
            else
                if (ev.m_newCheckState == CheckState.Checked)
                    if (!(m_arListIds[(int)ev.m_indxIdDeny].IndexOf(idItem) < 0))
                        m_arListIds[(int)ev.m_indxIdDeny].Remove(idItem);
                    else
                        ; //throw new Exception (@"");
                else
                    ;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            ;
            //Изменить структуру 'DataGridView'
            //m_dgvValues.UpdateStructure ();            
            (m_dgvValues as DataGridViewTEPValues).UpdateStructure(ev);
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected abstract void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev);

        protected void m_dgvValues_SelectionChanged(object sender, EventArgs ev)
        {
            DataTable inalg = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER];
            if (inalg != null)
                foreach (DataRow r in inalg.Rows)
                    if ((m_dgvValues.SelectedCells.Count == 1)
                        && (m_dgvValues.SelectedCells[0].RowIndex < m_dgvValues.Rows.Count))
                        if (m_dgvValues.Rows[m_dgvValues.SelectedCells[0].RowIndex].HeaderCell.Value != null)
                            if (r["n_ALG"].ToString().Trim() == m_dgvValues.Rows[m_dgvValues.SelectedCells[0].RowIndex].HeaderCell.Value.ToString())
                            {
                                SetDescSelRow(r["DESCRIPTION"].ToString(), r["NAME_SHR"].ToString());
                            }
                            else
                                ; // идентификатор параметра в алгоритме расчета НЕ срвпадает со значением заголовка строки
                        else
                            ; // значение заголовка строки НЕ= 0
                    else
                        ; // кол-во выделенных ячеек НЕ= 1 ИЛИ индекс выделенной ячейки-строки БОЛЬШЕ= кол-ву строк в 'm_dgvValues'
            else
                ; // табл. с параметрами не инициализирована
        }

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
                public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;
                public double m_Value;

                public DataGridViewTEPValuesCellValueChangedEventArgs()
                    : base()
                {
                    m_IdAlg =
                    m_IdComp =
                    m_IdParameter =
                        -1;
                    m_iQuality = TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT;
                    m_Value = -1F;
                }

                public DataGridViewTEPValuesCellValueChangedEventArgs(int id_alg
                    , int id_comp
                    , int id_par
                    , TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE quality
                    , double val)
                        : this()
                {
                    m_IdAlg = id_alg;
                    m_IdComp = id_comp;
                    m_IdParameter = id_par;
                    m_iQuality = quality;
                    m_Value = val;
                }
            }
            /// <summary>
            /// Тип делегата для обработки события - изменение значения в ячейке
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ev"></param>
            public delegate void DataGridViewTEPValuesCellValueChangedEventHandler(object obj, DataGridViewTEPValuesCellValueChangedEventArgs ev);
            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public event DataGridViewTEPValuesCellValueChangedEventHandler EventCellValueChanged;
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
            public DataGridViewTEPValues()
                : base()
            {
                //Разместить ячейки, установить свойства объекта
                InitializeComponents();
                //Назначить (внутренний) обработчик события - изменение значения ячейки
                // для дальнейшей ретрансляции родительскому объекту
                CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
                //RowsRemoved += new DataGridViewRowsRemovedEventHandler (onRowsRemoved);
            }

            private void InitializeComponents()
            {
                AddColumn(-2, string.Empty, false);
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
            public override void AddColumn(int id_comp, string text, bool bVisibled)
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
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.SOURCE:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT:
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
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.SOURCE:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT:
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
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT: // только для входной таблицы - значение по умолчанию [inval_def]
                            clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                            break;
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL: // см. 'getQueryValuesVar' - неполные данные
                            clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                            break;
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC: // см. 'getQueryValuesVar' - нет ни одной записи
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
            public /*override*/ void UpdateStructure(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs item)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                int indx = -1
                    , cIndx = -1
                    , rKey = -1;
                bool bItemChecked = item.m_newCheckState == CheckState.Checked ? true :
                    item.m_newCheckState == CheckState.Unchecked ? false :
                        false;

                //Поиск индекса элемента отображения
                switch (item.m_indxIdDeny)
                {
                    case INDEX_ID.DENY_COMP_CALCULATED:
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        // найти индекс столбца (компонента) - по идентификатору
                        foreach (HDataGridViewColumn c in Columns)
                            if (c.m_iIdComp == item.m_idItem)
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
                            if ((int)r.Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value == item.m_idItem)
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
                    switch (item.m_indxIdDeny)
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
                                cellRows = values.Select(@"ID_PUT=" + parameterRows[0][@"ID"]);

                                if (cellRows.Length == 1)
                                {
                                    idParameter = (int)cellRows[0][@"ID_PUT"];
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
                            m_dictPropertiesRows[idAlg].m_arPropertiesCells[iCol].m_iQuality = (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality;
                            row.Cells[iCol].ReadOnly = double.IsNaN(dblVal);

                            if (getClrCellToValue(iCol, iRow, out clrCell) == true)
                            {
                                // символ (??? один для строки, но назначается много раз по числу столбцов)
                                row.Cells[(int)INDEX_SERVICE_COLUMN.SYMBOL].Value = m_dictPropertiesRows[idAlg].m_strSymbol
                                    + @",[" + m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_nameRU + m_dictPropertiesRows[idAlg].m_strMeasure + @"]";
                                // Множитель для значения - для отображения
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                // Множитель для значения - исходный в БД
                                ratioValue =
                                    //m_dictRatio[m_dictPropertiesRows[idAlg].m_ratio].m_value
                                    m_dictRatio[(int)parameterRows[0][@"ID_RATIO"]].m_value
                                    ;
                                // проверить требуется ли преобразование
                                if (!(ratioValue == vsRatioValue))
                                    // домножить значение на коэффициент
                                    dblVal *= Math.Pow(10F, ratioValue - vsRatioValue);
                                else
                                    ;
                                // отобразить с количеством знаков в соответствии с настройками
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

                CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
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
                    //0 - идентификатор компонента (служебный)
                    //1 - размерность (служебный)
                    if ((ev.ColumnIndex > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        && (!(ev.RowIndex < 0)))
                    {
                        strValue = (string)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value;

                        if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dblValue) == true)
                            EventCellValueChanged(this, new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs(
                                (int)Rows[ev.RowIndex].Cells[0].Value //Идентификатор параметра [alg]
                                , (Columns[ev.ColumnIndex] as HDataGridViewColumn).m_iIdComp //Идентификатор компонента
                                , m_dictPropertiesRows[id_alg].m_arPropertiesCells[ev.ColumnIndex].m_IdParameter //Идентификатор параметра с учетом периода расчета [put]
                                , m_dictPropertiesRows[id_alg].m_arPropertiesCells[ev.ColumnIndex].m_iQuality
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
        protected class PanelManagementTaskTepValues : PanelManagementTaskTepCalculate
        {
            ///// <summary>
            ///// Структура для хранения координат элемента расчета
            ///// </summary>
            //public struct ADDRESS_CALC
            //{
            //    public ADDRESS_CALC(ADDRESS_CALC src)
            //    {
            //        m_idItem = src.m_idItem;
            //        m_indxIdDeny = src.m_indxIdDeny;
            //    }
            //    ///// <summary>
            //    ///// Идентификатор компонента ТЭЦ
            //    ///// </summary>
            //    //public int m_idComp;
            //    /// <summary>
            //    /// Идентификатор в алгоритме расчета
            //    /// </summary>
            //    public int m_idItem;
            //    ///// <summary>
            //    ///// Идентификатор в алгоритме расчета для компонента ТЭЦ
            //    ///// </summary>
            //    //public int m_idPut;
            //    /// <summary>
            //    /// Индекс типа идентификаторов
            //    /// </summary>
            //    public INDEX_ID m_indxIdDeny;
            //}
            /// <summary>
            /// Класс аргумента для события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public class ItemCheckedParametersEventArgs : EventArgs
            {
                /// <summary>
                /// Индекс в списке идентификаторов
                ///  для получения ключа в словаре со значениями
                /// </summary>
                public INDEX_ID m_indxIdDeny;
                /// <summary>
                /// Идентификатор в алгоритме расчета
                /// </summary>
                public int m_idItem;
                /// <summary>
                /// Состояние элемента, связанного с компонентом/параметром_расчета
                /// </summary>
                public CheckState m_newCheckState;

                public ItemCheckedParametersEventArgs(int idItem, INDEX_ID indxIdDeny, CheckState newCheckState)
                    : base()
                {
                    m_idItem = idItem;
                    m_indxIdDeny = indxIdDeny;
                    m_newCheckState = newCheckState;
                }
            }

            /// <summary>
            /// Тип обработчика события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            /// <param name="ev">Аргумент события</param>
            public delegate void ItemCheckedParametersEventHandler(ItemCheckedParametersEventArgs ev);
            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;

            //protected ADDRESS_CALC m_address;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementTaskTepValues()
                : base()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
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
                //Часовой пояс расчета
                ctrl = Controls.Find(INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                this.Controls.Remove(ctrl);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Расчет - выполнить - макет
                //Расчет - выполнить - норматив
                posRow = addButtonRun(posRow);

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
                ctrl = new CheckedListBoxTaskTepValues();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения из расчета параметра
                ctrl = createControlParameterCalculated();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
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
                ctrl = new CheckedListBoxTaskTepValues();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения для отображения параметра
                ctrl = new CheckedListBoxTaskTepValues();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

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
            ///// <summary>
            ///// Обработчик события - изменение дата/время окончания периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //private void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            //{
            //    HDateTimePicker hdtpEnd = obj as HDateTimePicker;
            //    m_dtRange.Set(hdtpEnd.LeadingValue, hdtpEnd.Value);

            //    if (! (DateTimeRangeValue_Changed == null))
            //        DateTimeRangeValue_Changed(this, EventArgs.Empty);
            //    else
            //        ;
            //}

            /// <summary>
            /// Добавить кнопки, инициирующие процесс расчета
            /// </summary>
            /// <param name="posRow">Позиция по горизонтали для размещения 1-ой (вверху) кнопки</param>
            /// <returns>Позиция по горизонтали для размещения следующего элемента</returns>
            protected virtual int addButtonRun(int posRow)
            {
                int iRes = posRow;

                return iRes;
            }
            /// <summary>
            /// Очистить
            /// </summary>
            public void Clear()
            {
                INDEX_ID[] arIndxIdToClear = new INDEX_ID[] { INDEX_ID.DENY_COMP_CALCULATED, INDEX_ID.DENY_COMP_VISIBLED };

                ActivateCheckedHandler(false, arIndxIdToClear);

                Clear(arIndxIdToClear);
            }

            protected INDEX_ID getIndexIdOfControl(Control ctrl)
            {
                INDEX_CONTROL id = INDEX_CONTROL.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
                INDEX_ID indxRes = INDEX_ID.UNKNOWN;

                try
                {
                    //Определить идентификатор
                    id = getIndexControl(ctrl);
                    // , соответствующий изменившему состояние элементу 'CheckedListBox'
                    switch (id)
                    {
                        case INDEX_CONTROL.CLBX_COMP_CALCULATED:
                        case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_COMP_CALCULATED ? INDEX_ID.DENY_COMP_CALCULATED :
                                id == INDEX_CONTROL.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        case INDEX_CONTROL.CLBX_PARAMETER_CALCULATED:
                        case INDEX_CONTROL.CLBX_PARAMETER_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_PARAMETER_CALCULATED ? INDEX_ID.DENY_PARAMETER_CALCULATED :
                                id == INDEX_CONTROL.CLBX_PARAMETER_VISIBLED ? INDEX_ID.DENY_PARAMETER_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelManagementTaskTepValues::onItemCheck () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return indxRes;
            }
            /// <summary>
            /// Найти элемент управления на панели по индексу идентификатора
            /// </summary>
            /// <param name="id">Индекс идентификатора, используемого для заполнения элемента управления</param>
            /// <returns>Дочерний элемент управления</returns>
            protected Control find(INDEX_ID id)
            {
                Control ctrlRes = null;

                ctrlRes = find(getIndexControlOfIndexID(id));

                return ctrlRes;
            }

            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns></returns>
            protected Control find(INDEX_CONTROL indxCtrl)
            {
                Control ctrlRes = null;

                ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];

                return ctrlRes;
            }

            /// <summary>
            /// Возвратить идентификатор элемента управления по идентификатору
            ///  , используемого для его заполнения
            /// </summary>
            /// <param name="indxId"></param>
            /// <returns></returns>
            protected INDEX_CONTROL getIndexControlOfIndexID(INDEX_ID indxId)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                switch (indxId)
                {
                    case INDEX_ID.DENY_COMP_CALCULATED:
                        indxRes = INDEX_CONTROL.CLBX_COMP_CALCULATED;
                        break;
                    case INDEX_ID.DENY_PARAMETER_CALCULATED:
                        indxRes = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED;
                        break;
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        indxRes = INDEX_CONTROL.CLBX_COMP_VISIBLED;
                        break;
                    case INDEX_ID.DENY_PARAMETER_VISIBLED:
                        indxRes = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED;
                        break;
                    default:
                        break;
                }

                return indxRes;
            }

            protected INDEX_CONTROL getIndexControl(Control ctrl)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (strId.Equals(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString()) == true)
                    indxRes = INDEX_CONTROL.CLBX_COMP_CALCULATED;
                else
                    if (strId.Equals(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString()) == true)
                        indxRes = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED;
                    else
                        if (strId.Equals(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString()) == true)
                            indxRes = INDEX_CONTROL.CLBX_COMP_VISIBLED;
                        else
                            if (strId.Equals(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString()) == true)
                                indxRes = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED;
                            else
                                throw new Exception(@"PanelTaskTepValues::getIndexControl () - не найден объект 'CheckedListBox'...");

                return indxRes;
            }

            public void Clear(INDEX_ID[] arIdToClear)
            {
                for (int i = 0; i < arIdToClear.Length; i++)
                    clear(arIdToClear[i]);
            }

            private void clear(INDEX_ID idToClear)
            {
                (find(idToClear) as IControl).ClearItems();
            }

            public void ActivateCheckedHandler(bool bActive, INDEX_ID[] arIdToActivate)
            {
                for (int i = 0; i < arIdToActivate.Length; i++)
                    activateCheckedHandler(bActive, arIdToActivate[i]);
            }

            protected virtual void activateCheckedHandler(bool bActive, INDEX_ID idToActivate)
            {
                INDEX_CONTROL indxCtrl = INDEX_CONTROL.UNKNOWN;
                CheckedListBox clbx = null;

                indxCtrl = getIndexControlOfIndexID(idToActivate);

                if (!(indxCtrl == INDEX_CONTROL.UNKNOWN))
                {
                    clbx = (Controls.Find(indxCtrl.ToString(), true)[0] as CheckedListBox);

                    if (bActive == true)
                    {
                        //clbx.SelectedIndexChanged += new EventHandler (onSelectedIndexChanged);
                        clbx.ItemCheck += new ItemCheckEventHandler(onItemCheck);
                    }
                    else
                    {
                        //clbx.SelectedIndexChanged -= onSelectedIndexChanged;
                        clbx.ItemCheck -= onItemCheck;
                    }
                }
                else
                    ;
            }

            /// <summary>
            /// Добавить элемент компонент станции в списки
            ///  , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(int id_comp, string text, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            {
                Control ctrl = null;

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskTepValues).AddItem(id_comp, text, arChecked[i]);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public string GetNameComponent(int id_comp)
            {
                string strRes = string.Empty;

                CheckedListBoxTaskTepValues ctrl = null;

                ctrl = find(INDEX_ID.DENY_COMP_CALCULATED) as CheckedListBoxTaskTepValues;
                strRes = ctrl.GetNameItem(id_comp);

                return strRes;
            }

            public struct ADDING_PARAMETER
            {
                public int m_idNAlg;
                public int m_idComp;
                public int m_idPut;
                public string m_strText;
                public INDEX_ID[] m_arIndexIdToAdd;
                public bool[] m_arChecked;

                public ADDING_PARAMETER(int id_alg, int id_comp, int id_put, string text, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
                {
                    m_idNAlg = id_alg;
                    m_idComp = id_comp;
                    m_idPut = id_put;
                    m_strText = text;
                    m_arIndexIdToAdd = new INDEX_ID[arIndexIdToAdd.Length]; arIndexIdToAdd.CopyTo(m_arIndexIdToAdd, 0);
                    m_arChecked = new bool[arChecked.Length]; arChecked.CopyTo(m_arChecked, 0);
                }
            }

            //public void AddParameter(int id_alg, int id_comp, int id_put, string text, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            public void OnAddParameter(object obj)
            {
                ADDING_PARAMETER addPar = (ADDING_PARAMETER)obj;
                Control ctrl = null;

                for (int i = 0; i < addPar.m_arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(addPar.m_arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        addParameter(ctrl, addPar.m_idNAlg, addPar.m_idComp, addPar.m_idPut, addPar.m_strText, addPar.m_arChecked[i]);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddParameter () - не найден элемент для INDEX_ID=" + addPar.m_arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            protected virtual Control createControlParameterCalculated()
            {
                return new CheckedListBoxTaskTepValues();
            }

            protected virtual void addParameter(Control ctrl, int id_alg, int id_comp, int id_put, string text, bool bChecked)
            {
                if (ctrl is CheckedListBoxTaskTepValues)
                    (ctrl as CheckedListBoxTaskTepValues).AddItem(id_alg, text, bChecked);
                else
                    ;
            }

            //private void onSelectedIndexChanged(object obj, EventArgs ev)
            //{                
            //}

            //protected virtual void addItem(INDEX_ID indxId, Control ctrl, int id, string text, bool bChecked)
            //{
            //    (ctrl as CheckedListBoxTaskTepValues).AddItem(id, text, bChecked);
            //}

            /// <summary>
            /// Обработчик события - изменение состояния элемента списка
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected void onItemCheck(object obj, ItemCheckEventArgs ev)
            {
                itemCheck((obj as IControl).SelectedId, getIndexIdOfControl(obj as Control), ev.NewValue);
            }

            /// <summary>
            /// Инициировать событие - изменение признака элемента
            /// </summary>
            /// <param name="address">Адрес элемента</param>
            /// <param name="checkState">Значение признака элемента</param>
            protected void itemCheck(int idItem, INDEX_ID indxIdDeny, CheckState checkState)
            {
                ItemCheck(new ItemCheckedParametersEventArgs(idItem, indxIdDeny, checkState));
            }

            /// <summary>
            /// Интерфейс для всех элементов управления с компонентами станции, параметрами расчета
            /// </summary>
            protected interface IControl
            {
                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                int SelectedId { get; }
                ///// <summary>
                ///// Добавить элемент в список
                ///// </summary>
                ///// <param name="text">Текст подписи элемента</param>
                ///// <param name="id">Идентификатор элемента</param>
                ///// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                //void AddItem(int id, string text, bool bChecked);
                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                void ClearItems();
            }

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskTepValues : CheckedListBox, IControl
            {
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                public CheckedListBoxTaskTepValues()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId { get { return m_listId[SelectedIndex]; } }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                    Items.Add(text, bChecked);
                    m_listId.Add(id);
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Items.Clear();
                    m_listId.Clear();
                }

                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }
            }
        }
    }

    public partial class PanelTaskTepValues
    {
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , BUTTON_RUN_PREV,
            BUTTON_RUN_RES
                , CLBX_COMP_CALCULATED,
            CLBX_PARAMETER_CALCULATED
                , BUTTON_LOAD, MENUITEM_UPDATE, MENUITEM_HISTORY, BUTTON_SAVE, BUTTON_IMPORT,
            BUTTON_EXPORT
                , CLBX_COMP_VISIBLED,
            CLBX_PARAMETER_VISIBLED
                ,
            DGV_DATA
                ,
            LABEL_DESC
              ,
        }
    }
}
