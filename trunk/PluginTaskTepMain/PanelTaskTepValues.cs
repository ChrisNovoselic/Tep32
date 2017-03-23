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
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }

            //set { m_arTableOrigin[(int)ID_VIEW_VALUES.SOURCE] = value.Copy(); }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }

            //set { m_arTableEdit[(int)ID_VIEW_VALUES.SOURCE] = value.Copy(); }
        }
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepValues(IPlugIn iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc, type)
        {
            m_arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponents();
            // назначить обработчики для кнопок 'Результат'
            (Controls.Find(INDEX_CONTROL.BUTTON_RUN_RES.ToString(), true)[0] as Button).Click += new EventHandler(btnRunRes_onClick);
            (m_dgvValues as DataGridViewTEPValues).EventCellValueChanged += new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventHandler(onEventCellValueChanged);
            (m_dgvValues as DataGridViewTEPValues).SelectionChanged += new EventHandler(dgvValues_onSelectionChanged);
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
            m_dgvValues = new DataGridViewTEPValues();
            int posColdgvValues = 4
                , heightRowdgvValues = 10;

            SuspendLayout();

            //initializeLayoutStyle ();

            Controls.Add(PanelManagement, 0, 0);
            SetColumnSpan(PanelManagement, posColdgvValues); SetRowSpan(PanelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvValues); SetRowSpan(m_dgvValues, heightRowdgvValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvValues, heightRowdgvValues);

            ResumeLayout(false);
            PerformLayout();

            //(Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            Button btn = (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelManagement_btnHistory_onClick);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);

            (Controls.Find(INDEX_CONTROL.BUTTON_IMPORT.ToString(), true)[0] as Button).Click += new EventHandler(panelManagement_btnImport_onClick);
            (Controls.Find(INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += new EventHandler(panelManagement_btnExport_onClick);

            (PanelManagement as PanelManagementTaskTepValues).ItemCheck += new PanelManagementTaskTepValues.ItemCheckedParametersEventHandler(panelManagement_onItemCheck);
        }

        //protected override void initialize()
        //{
        //    string strItem = string.Empty;
        //    int i = -1
        //        , id_comp = -1
        //        , iChecked = -1;
        //    INDEX_ID[] arIndxIdToAdd = new INDEX_ID[] {
        //        INDEX_ID.DENY_COMP_CALCULATED
        //        , INDEX_ID.DENY_COMP_VISIBLED
        //    };
        //    bool[] arChecked = new bool[arIndxIdToAdd.Length];

        //    //Заполнить элементы управления с компонентами станции
        //    foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows) {
        //        id_comp = (Int16)r[@"ID"];
        //        //m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);
        //        strItem = ((string)r[@"DESCRIPTION"]).Trim();
        //        // установить признак участия в расчете компонента станции
        //        arChecked[0] = int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.ENABLED_ITEM), out iChecked) == true ?
        //            iChecked == 1
        //                : true;
        //        // установить признак отображения компонента станции
        //        iChecked = -1;
        //        arChecked[0] = int.TryParse(m_dictProfile.GetAttribute(Session.CurrentIdPeriod, id_comp, HTepUsers.ID_ALLOWED.VISIBLED_ITEM), out iChecked) == true ?
        //            iChecked == 1
        //                : true;
        //        (PanelManagement as PanelManagementTaskTepValues).AddComponent(id_comp
        //            , strItem
        //            , arIndxIdToAdd
        //            , arChecked);

        //        m_dgvValues.AddColumn(id_comp, strItem, arChecked[1]);
        //    }

        //    m_dgvValues.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);
        //}

        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(bool bClose = false)
        {
            base.clear(bClose);

            //!!! PanelManagement.Clear - вызывается в базовом классе
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
            clear(true);

            base.Stop();
        }

        private void panelManagement_btnImport_onClick(object sender, EventArgs e)
        {
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_IMPORT;

            buttonLoad_onClick();            
        }

        private void panelManagement_btnExport_onClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        protected void setValues()
        {
            for (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES indx = (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.UNKNOWN + 1);
                indx < TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT;
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
                //, cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
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
                    m_dgvValues.ShowValues(m_TableEdit/*, m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]*/);
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
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;

            buttonLoad_onClick();
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Загрузить ранее сохраненные значения")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void panelManagement_btnHistory_onClick(object obj, EventArgs ev)
        {
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;

            buttonLoad_onClick();
        }

        protected virtual void buttonLoad_onClick()
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
        protected override List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select(/*@"ID_TIME=" + CurrIdPeriod*/).ToList<DataRow>();
                listRes.Sort(compareNAlg);

                return listRes;
            }
        }

        #region Обработка измнения значений основных элементов управления на панели управления 'PanelManagement'
        /// <summary>
        /// Обработчик события при изменении значения
        ///  одного из основных элементов управления на панели управления 'PanelManagement'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            if (obj is Enum)
                switch ((ID_DBTABLE)obj) {
                    case ID_DBTABLE.TIME:
                        break;
                    case ID_DBTABLE.TIMEZONE:
                        break;
                    case ID_DBTABLE.UNKNOWN:
                    default:
                        break;
                } else
                ;

            base.panelManagement_EventIndexControlBase_onValueChanged(obj);

            if (obj is Enum)
                switch ((ID_DBTABLE)obj) {
                    case ID_DBTABLE.TIME:
                        break;
                    case ID_DBTABLE.TIMEZONE:
                        break;
                    case ID_DBTABLE.UNKNOWN:
                    default:
                        break;
                }
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
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение периода расчета)
        /// </summary>
        protected override void panelManagement_PeriodChanged()
        {
            //??? проверить сохранены ли значения
            m_dgvValues.ClearRows();
            //??? зачем и столбцы тоже - вероятно, предполагаем, что в другом периоде другие компоненты?
            m_dgvValues.ClearColumns();

            //Очистить списки - элементы интерфейса
            (PanelManagement as PanelManagementTaskTepValues).Clear();

            base.panelManagement_PeriodChanged();
            // здесь заканчились все параметры расчета, компоненты станции - можно начать формировать структуру представления
            //m_dgvValues.BuildStructure();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void onAddNAlgParameter(NALG_PARAMETER obj)
        {
            base.onAddNAlgParameter(obj);

            (PanelManagement as PanelManagementTaskTepValues).AddNAlgParameter(obj);

            // добавить свойства для строки таблицы со значениями
            m_dgvValues.AddNAlgParameter(obj);
            // в процессе создаем структуру, т.к. она простая
            // , иначе требовалось бы подаждать добавления всех параметров 'NAlg'
            (m_dgvValues as DataGridViewTEPValues).AddRow(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void onAddPutParameter(PUT_PARAMETER obj)
        {
            base.onAddPutParameter(obj);

            // добавить свойства для строки таблицы со значениями
            m_dgvValues.AddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void onAddComponent(TECComponent obj)
        {
            base.onAddComponent(obj);

            (PanelManagement as PanelManagementTaskTepValues).AddComponent(obj);

            (m_dgvValues as DataGridViewTEPValues).AddComponent(obj);
            (m_dgvValues as DataGridViewTEPValues).AddColumn(obj);
        }
        #endregion

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_onItemCheck(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //??? где сохраняются изменения. только на элементе управления?
            ;
            //??? Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
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

        protected void dgvValues_onSelectionChanged(object sender, EventArgs ev)
        {
            DataTable inalg = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER];
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
            /// <param name="obj">Объект, инициировавший событие (DataGridViewTepValues)</param>
            /// <param name="ev">Аргумент события</param>
            public delegate void DataGridViewTEPValuesCellValueChangedEventHandler(object obj, DataGridViewTEPValuesCellValueChangedEventArgs ev);
            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public event DataGridViewTEPValuesCellValueChangedEventHandler EventCellValueChanged;

            public override void BuildStructure()
            {
                throw new NotImplementedException();
            }
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
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                //AddColumn(-2, string.Empty, false);
                addColumn(
                    new TECComponent (-1, -1
                        , @"Размерность"
                        , false, false)
                    , ModeAddColumn.Service | ModeAddColumn.Visibled);
            }

            public override void ClearColumns()
            {
                List<DataGridViewColumn> listIndxToRemove;

                if (Columns.Count > 0)
                {
                    listIndxToRemove = new List<DataGridViewColumn>();

                    foreach (DataGridViewColumn col in Columns)
                        if (!(((TECComponent)col.Tag).m_Id < 0))
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

            public void AddComponent(TECComponent comp)
            {
                ;
            }

            public void AddColumn(TECComponent comp)
            {
                addColumn(comp, ModeAddColumn.Insert | ModeAddColumn.Visibled);
            }
            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">Идентификатор компонента ТЭЦ</param>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bVisibled">Признак участия в расчете/отображения</param>
            protected override void addColumn(TECComponent comp, ModeAddColumn mode)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (DataGridViewColumn col in Columns)
                        if ((((TECComponent)col.Tag).m_Id > 0)
                            && (((TECComponent)col.Tag).m_Id < (int)TECComponent.TYPE.TG)) {
                            indxCol = Columns.IndexOf(col);

                            break;
                        } else
                            ;

                    DataGridViewColumn column = new DataGridViewTextBoxColumn();
                    column.Tag = comp;
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if ((mode & ModeAddColumn.Service) == ModeAddColumn.Service)
                        {// для служебных столбцов
                            if ((mode & ModeAddColumn.Visibled) == ModeAddColumn.Visibled)
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

                    column.HeaderText = comp.m_nameShr;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = (mode & ModeAddColumn.Visibled) == ModeAddColumn.Visibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e
                        , string.Format(@"DataGridViewTEPValues::AddColumn (id_comp={0}) - ...", comp.m_Id)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="obj">Объект с параметром алгоритма расчета</param>
            public int AddRow(NALG_PARAMETER obj)
            {
                int iRes = -1;

                //!!! Объект уже добавлен в словарь
                //!!! столбец с 'SYMBOL' уже добавлен

                iRes = Rows.Add(new DataGridViewRow());
                Rows[iRes].Tag = obj.m_Id;

                // установить значение для заголовка
                Rows[iRes].HeaderCell.Value = obj.m_nAlg;
                // установить значение для всплывающей подсказки
                Rows[iRes].HeaderCell.ToolTipText = obj.m_strDescription;
                // установить значение для обозначения параметра и его ед./измерения
                Rows[iRes].Cells[0].Value = string.Format(@"{0},[{1}]", obj.m_strSymbol, obj.m_strMeausure);

                return iRes;
            }

            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="bNewEnabled">Новое (устанавливаемое) значение признака участия в расчете для параметра</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности изменения цвета ячейки</returns>
            private bool getColorCellToColumn(int iCol, int iRow, bool bNewEnabled, out Color clrRes)
            {
                bool bRes = false;

                int id_alg = -1
                    , id_comp = -1;
                TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality;

                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                id_alg = -1;
                id_comp = -1;

                if ((!(Rows[iRow].Cells[iCol].Tag == null))
                    && (Rows[iRow].Cells[iCol].Tag is CELL_PROPERTY)) {
                    iQuality = ((CELL_PROPERTY)Rows[iRow].Cells[iCol].Tag).m_iQuality;

                    bRes = ((m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].m_bEnabled == false)
                        && ((m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].IsNaN == false)));
                    if (bRes == true)
                        if (bNewEnabled == true)
                            switch (iQuality) {//??? LIMIT
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
                            } else
                            clrRes = s_arCellColors[(int)INDEX_COLOR.DISABLED];
                    else
                        ;
                } else
                //??? значению в ячейке не присвоена квалификация - значение не присваивалось
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
            private bool getColorCellToRow(int iCol, int iRow, bool bNewEnabled, out Color clrRes)
            {
                bool bRes = false;

                int id_alg = -1
                    , id_comp = -1;
                TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality;
                bool bPrevEnabled = false;

                bRes = m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].IsNaN == false;
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                id_alg = -1;
                id_comp = -1;
                iQuality = ((CELL_PROPERTY)Rows[iRow].Cells[iCol].Tag).m_iQuality;

                //??? определить предыдущее состояние
                bPrevEnabled = ((TECComponent)Columns.Cast<DataGridViewColumn>().First(col => { return ((TECComponent)col.Tag).m_Id == id_comp; }).Tag).m_bEnabled;

                if (bRes == true)
                    if ((bNewEnabled == true)
                        && (bPrevEnabled == false))
                        switch (iQuality) {//??? LIMIT
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
                        clrRes = s_arCellColors[(int)INDEX_COLOR.DISABLED];
                else
                    ;

                return bRes;
            }

            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="id_alg">Идентификатор...</param>
            /// <param name="id_comp">Идентификатор...</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности размещения значения в ячейке</returns>
            private bool getColorCellToValue(int id_alg, int id_comp, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality, out Color clrRes)
            {
                bool bRes = false;

                bRes = !m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].IsNaN;
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];

                if (bRes == true)
                    switch (iQuality) { //??? USER, LIMIT
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
            /// Обновить структуру таблицы (доступность(цвет)/видимость столбцов/строк)
            /// </summary>
            /// <param name="item">Аргумент события для обновления структуры представления</param>
            public /*override*/ void UpdateStructure(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs item)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                int indx = -1;
                bool bItemChecked = item.NewCheckState == CheckState.Checked ? true :
                    item.NewCheckState == CheckState.Unchecked ? false :
                        false;

                //Поиск индекса элемента отображения
                if (item.IsComponent == true) {
                    // найти индекс столбца (компонента) - по идентификатору
                    foreach (DataGridViewColumn c in Columns)
                        if (((TECComponent)c.Tag).m_Id == item.m_idComp) {
                            indx = Columns.IndexOf(c);
                            break;
                        } else
                            ;
                } else if (item.IsNAlg == true) {
                    // найти индекс строки (параметра) - по идентификатору
                    // вариант №1
                    indx = Rows.Cast<DataGridViewRow>().First(r => { return (int)r.Tag == item.m_idAlg; }).Index;
                    //// // вариант №2
                    //indx = (
                    //        from r in Rows.Cast<DataGridViewRow>()
                    //        where (int)r.Tag == item.m_idAlg
                    //        select new { r.Index }
                    //    ).Cast<int>().ElementAt<int>(0);
                    //// // вариант №3
                    //foreach (DataGridViewRow r in Rows)
                    //    if ((int)r.Tag == item.m_idAlg) {
                    //        indx = Rows.IndexOf(r);
                    //        break;
                    //    } else
                    //        ;
                } else
                    ;

                if (!(indx < 0))
                    if (item.m_type == PanelManagementTaskTepValues.ItemCheckedParametersEventArgs.TYPE.ENABLE) {
                        if (item.IsComponent == true) { // COMPONENT ENABLE
                            // для всех ячеек в столбце
                            foreach (DataGridViewRow r in Rows) {
                                if (getColorCellToColumn(indx, r.Index, bItemChecked, out clrCell) == true)
                                    r.Cells[indx].Style.BackColor = clrCell;
                                else
                                    ;
                            }
                            ((TECComponent)Columns[indx].Tag).SetEnabled(bItemChecked);
                        } else if (item.IsNAlg == true) { // NALG ENABLE
                            // для всех ячеек в строке
                            foreach (DataGridViewCell c in Rows[indx].Cells) {
                                if (getColorCellToRow(c.ColumnIndex, indx, bItemChecked, out clrCell) == true)
                                    c.Style.BackColor = clrCell;
                                else
                                    ;

                                m_dictNAlgProperties.SetEnabled((int)Rows[indx].Tag, ((TECComponent)Columns[c.ColumnIndex].Tag).m_Id, bItemChecked);
                            }
                        } else
                            ;
                    } else if (item.m_type == PanelManagementTaskTepValues.ItemCheckedParametersEventArgs.TYPE.VISIBLE) {
                        if (item.IsComponent == true) { // COMPONENT VISIBLE
                            // для всех ячеек в столбце
                            Columns[indx].Visible = bItemChecked;
                        } else if (item.IsNAlg == true) {  // NALG VISIBLE
                            // для всех ячеек в строке
                            Rows[indx].Visible = bItemChecked;
                        } else
                            ;
                    } else
                        ;
                else
                // нет элемента для изменения стиля
                    ;
            }

            /// <summary>
            /// Отобразить значения
            /// </summary>
            /// <param name="values">Значения для отображения</param>
            public override void ShowValues(DataTable values/*, DataTable parameter, bool bUseRatio = true*/)
            {
                int idAlg = -1
                    , idPut = -1
                    , idComp = -1
                    , iQuality = -1
                    , indxCol = 0, indxRow = 0
                    , prjRatioValue = -1, vsRatioValue = -1;
                double dblVal = -1F;
                DataRow[] cellRows = null
                    //, parameterRows = null
                    ;
                Color clrCell = Color.Empty;

                CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewColumn col in Columns) {
                    idComp = ((TECComponent)col.Tag).m_Id;

                    if (idComp > 0)
                        foreach (DataGridViewRow row in Rows) {
                            dblVal = double.NaN;
                            iQuality = -1;
                            idAlg = (int)row.Cells[0].Value;
                            idPut = m_dictNAlgProperties[idAlg].m_dictPutParameters[idComp].m_Id;

                            cellRows = values.Select(@"ID_PUT=" + idPut);

                            if (cellRows.Length == 1) {
                                //idPut = (int)cellRows[0][@"ID_PUT"];
                                dblVal = ((double)cellRows[0][@"VALUE"]);
                                iQuality = (int)cellRows[0][@"QUALITY"];
                            } else
                            //??? continue
                                ;

                            indxCol = Columns.IndexOf(col);
                            //iRow = Rows.IndexOf(row);

                            row.Cells[indxCol].Tag = new CELL_PROPERTY() { m_Value = dblVal, m_iQuality = (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality };
                            row.Cells[indxCol].ReadOnly = double.IsNaN(dblVal);

                            if (getColorCellToValue(idAlg, idComp, (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality, out clrCell) == true)
                            {
                                //// символ (??? один для строки, но назначается много раз по числу столбцов)
                                //row.Cells[(int)INDEX_SERVICE_COLUMN.SYMBOL].Value = m_dictNAlgProperties[idAlg].m_strSymbol
                                //    + @",[" + m_dictRatio[m_dictNAlgProperties[idAlg].m_iRatio].m_nameRU + m_dictNAlgProperties[idAlg].m_strMeausure + @"]";

                                //if (bUseRatio == true) {
                                    // Множитель для значения - для отображения
                                    vsRatioValue =                                        
                                        m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value
                                        ;
                                    // Множитель для значения - исходный в БД
                                    prjRatioValue =
                                        //m_dictRatio[m_dictPropertiesRows[idAlg].m_ratio].m_value
                                        m_dictRatio[m_dictNAlgProperties[idAlg].m_prjRatio].m_value
                                        ;
                                    // проверить требуется ли преобразование
                                    if (!(prjRatioValue == vsRatioValue))
                                        // домножить значение на коэффициент
                                        dblVal *= Math.Pow(10F, prjRatioValue - vsRatioValue);
                                    else
                                        ;
                                //} else
                                //    ; //отображать без изменений

                                // отобразить с количеством знаков в соответствии с настройками
                                row.Cells[indxCol].Value = dblVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                                ;

                            row.Cells[indxCol].Style.BackColor = clrCell;
                        }
                    else
                        ;
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
                        if (((TECComponent)Columns[r.Cells.IndexOf(c)].Tag).m_Id > 0) {
                        // только для реальных компонетов - нельзя удалять идентификатор параметра
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
                int id_alg = -1
                    , id_comp = -1;

                try {
                    if ((!(ev.ColumnIndex < 0))
                        && (!(ev.RowIndex < 0))) {
                        id_alg = (int)Rows[ev.RowIndex].Tag;
                        id_comp = ((TECComponent)Columns[ev.ColumnIndex].Tag).m_Id; //Идентификатор компонента

                        if ((id_comp > 0) // только для реальных компонентов
                            && (!(ev.RowIndex < 0))) {
                            strValue = (string)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value;

                            if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dblValue) == true) {
                                ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).SetValue(dblValue);

                                EventCellValueChanged(this, new DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs(
                                    id_alg //Идентификатор параметра [alg]
                                    , id_comp
                                    , m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].m_Id //Идентификатор параметра с учетом периода расчета [put]
                                    , ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).m_iQuality
                                    , ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).m_Value));
                            } else
                                ; //??? невозможно преобразовать значение - отобразить сообщение для пользователя
                        } else
                            ; // в 0-ом столбце идентификатор параметра расчета
                    } else
                        ; // невозможно адресовать ячейку
                } catch (Exception e) {
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
        protected abstract class PanelManagementTaskTepValues : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementTaskTepValues()
                : base(ModeTimeControlPlacement.Queue)
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = null;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню лдя кнопки "Обновить-Загрузить"                

                SuspendLayout();

                //Расчет - выполнить - макет
                //Расчет - выполнить - норматив
                addButtonRun(0);

                posRow = 5;
                //Признаки включения/исключения из расчета
                //Признаки включения/исключения из расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить из расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //Признак для включения/исключения из расчета компонента
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения из расчета параметра
                ctrl = createControlNAlgParameterCalculated();
                ctrl.Name = INDEX_CONTROL.MIX_PARAMETER_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 3);

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
                ctrl.Enabled = true;
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
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения для отображения параметра
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                ResumeLayout(false);
                PerformLayout();
            }
            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
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
            protected abstract int addButtonRun(int posRow);
            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                base.Clear();

                //ActivateCheckedHandler(arIndxIdToClear, false);

                //??? почему только компоненты               
                clearComponents();
                //??? тоже очищаются
                clearParameters();
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

            protected INDEX_CONTROL getIndexControl(Control ctrl)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (strId.Equals(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString()) == true)
                    indxRes = INDEX_CONTROL.CLBX_COMP_CALCULATED;
                else
                    if (strId.Equals(INDEX_CONTROL.MIX_PARAMETER_CALCULATED.ToString()) == true)
                        indxRes = INDEX_CONTROL.MIX_PARAMETER_CALCULATED;
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

            private void clearParameters()
            {
                INDEX_CONTROL[] arIndxToClear = new INDEX_CONTROL[] {
                    INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                    , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                };

                for (int i = 0; i < arIndxToClear.Length; i++)
                    clear(arIndxToClear[i]);
            }

            private void clearComponents()
            {
                INDEX_CONTROL[] arIndxToClear = new INDEX_CONTROL[] {
                    INDEX_CONTROL.CLBX_COMP_CALCULATED
                    , INDEX_CONTROL.CLBX_COMP_VISIBLED
                };

                for (int i = 0; i < arIndxToClear.Length; i++)
                    clear(arIndxToClear[i]);
            }

            private void clear(INDEX_CONTROL indxCtrl)
            {
                (find(indxCtrl) as IControl).ClearItems();
            }
            /// <summary>
            /// (Де)Активировать обработчик события
            /// </summary>
            /// <param name="bActive">Признак (де)активации</param>
            protected override void activateControlChecked_onChanged(bool bActive)
            {
                activateControlChecked_onChanged(new INDEX_CONTROL[] {
                        INDEX_CONTROL.CLBX_COMP_CALCULATED
                        , INDEX_CONTROL.CLBX_COMP_VISIBLED
                        , INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                        , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                    }, bActive);
            }

            protected virtual void activateControlChecked_onChanged(INDEX_CONTROL[] arIndxControlToActivate, bool bActive)
            {
                //Из 'OutVal' вернется укороченный, т.к. в 'OutVal' есть 'TreeView' и его обработчик будет (де)активирован в наследуемом методе
                // в базовый метод должны быть переданы только идентификаторы-наименования 'CheckListBox'
                activateControlChecked_onChanged(arIndxControlToActivate.ToList().ConvertAll<string>(indx => { return indx.ToString(); }).ToArray(), bActive);
            }
            /// <summary>
            /// Добавить элемент компонент станции в списки
            ///  , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(TECComponent comp)
            {
                Control ctrl = null;
                bool bChecked = false;

                // в этих элементах управления размещаются элементы проекта - компоненты станции(оборудование)
                INDEX_CONTROL[] arIndexControl = new INDEX_CONTROL[] {
                    INDEX_CONTROL.CLBX_COMP_CALCULATED
                    , INDEX_CONTROL.CLBX_COMP_VISIBLED
                };

                foreach (INDEX_CONTROL indxCtrl in arIndexControl)
                {
                    ctrl = find(indxCtrl);

                    if (indxCtrl == INDEX_CONTROL.CLBX_COMP_CALCULATED)
                        bChecked = comp.m_bEnabled;
                    else if (indxCtrl == INDEX_CONTROL.CLBX_COMP_VISIBLED)
                        bChecked = comp.m_bVisibled;
                    else
                        bChecked = false;

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskCalculate).AddItem(comp.m_Id, comp.m_nameShr, bChecked);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddComponent () - не найден элемент для INDEX_ID=" + indxCtrl.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public void AddNAlgParameter(NALG_PARAMETER nAlgPar)
            {
                CheckedListBoxTaskCalculate ctrl;
                bool bChecked = false;

                // в этих элементах управления размещаются элементы проекта - параметры алгоритма расчета
                INDEX_CONTROL[] arIndexControl = new INDEX_CONTROL[] {
                    INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                    , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                };

                foreach (INDEX_CONTROL indxCtrl in arIndexControl) {
                    ctrl = find(indxCtrl) as CheckedListBoxTaskCalculate;

                    if (indxCtrl == INDEX_CONTROL.MIX_PARAMETER_CALCULATED)
                        bChecked = nAlgPar.m_bEnabled;
                    else if (indxCtrl == INDEX_CONTROL.CLBX_PARAMETER_VISIBLED)
                        bChecked = nAlgPar.m_bVisibled;
                    else
                        bChecked = false;

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskCalculate).AddItem(nAlgPar.m_Id, nAlgPar.m_strNameShr, bChecked);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddNAlgParameter () - не найден элемент =" + indxCtrl.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            protected virtual Control createControlNAlgParameterCalculated()
            {
                return new CheckedListBoxTaskCalculate();
            }

            //private void onSelectedIndexChanged(object obj, EventArgs ev)
            //{                
            //}

            //protected virtual void addItem(INDEX_ID indxId, Control ctrl, int id, string text, bool bChecked)
            //{
            //    (ctrl as CheckedListBoxTaskTepValues).AddItem(id, text, bChecked);
            //}

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                ItemCheckedParametersEventArgs.TYPE type;
                INDEX_CONTROL indxCtrl = INDEX_CONTROL.UNKNOWN;

                if (Enum.IsDefined(typeof(INDEX_CONTROL), (obj as Control).Name) == true) {
                    indxCtrl = (INDEX_CONTROL)Enum.Parse(typeof(INDEX_CONTROL), (obj as Control).Name);

                    switch (indxCtrl) {                        
                        case INDEX_CONTROL.CLBX_COMP_CALCULATED:
                        case INDEX_CONTROL.MIX_PARAMETER_CALCULATED:
                            type = ItemCheckedParametersEventArgs.TYPE.ENABLE;
                            break;
                        case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                        case INDEX_CONTROL.CLBX_PARAMETER_VISIBLED:
                        default:
                            type = ItemCheckedParametersEventArgs.TYPE.VISIBLE;
                            break;
                    }

                    itemCheck((obj as IControl).SelectedId, type, (ev as ItemCheckEventArgs).NewValue);
                } else
                    Logging.Logg().Error(string.Format(@""), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }

    public partial class PanelTaskTepValues
    {
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , BUTTON_RUN_PREV, BUTTON_RUN_RES
            , CLBX_COMP_CALCULATED, MIX_PARAMETER_CALCULATED
            , BUTTON_LOAD, MENUITEM_UPDATE, MENUITEM_HISTORY
                , BUTTON_SAVE, BUTTON_IMPORT, BUTTON_EXPORT
            , CLBX_COMP_VISIBLED, CLBX_PARAMETER_VISIBLED
            , DGV_DATA
            , LABEL_DESC
            ,
        }
    }
}
