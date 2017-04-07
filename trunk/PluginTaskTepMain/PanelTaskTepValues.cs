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
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]; }

            //set { m_arTableOrigin[(int)ID_VIEW_VALUES.SOURCE] = value.Copy(); }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]; }

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
            (m_dgvValues as DataGridViewTaskTepValues).EventCellValueChanged += new DataGridViewTaskTepValues.DataGridViewTEPValuesCellValueChangedEventHandler(onEventCellValueChanged);
            (m_dgvValues as DataGridViewTaskTepValues).SelectionChanged += new EventHandler(dgvValues_onSelectionChanged);
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
            m_dgvValues = new DataGridViewTaskTepValues();
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
            (findControl(INDEX_CONTROL.BUTTON_SAVE.ToString()) as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);

            (findControl(INDEX_CONTROL.BUTTON_IMPORT.ToString()) as Button).Click += new EventHandler(panelManagement_btnImport_onClick);
            (findControl(INDEX_CONTROL.BUTTON_EXPORT.ToString()) as Button).Click += new EventHandler(panelManagement_btnExport_onClick);

            //(PanelManagement as PanelManagementTaskTepValues).ItemCheck += new PanelManagementTaskTepValues.ItemCheckedParametersEventHandler(panelManagement_onItemCheck);
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
            base.Stop();
        }

        private void panelManagement_btnImport_onClick(object sender, EventArgs e)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_IMPORT);
        }

        private void panelManagement_btnExport_onClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Выполнить запрос к БД, отобразить рез-т запроса
        /////  в случае загрузки "сырых" значений = ID_PERIOD.HOUR
        /////  в случае загрузки "учтенных" значений -  в зависимости от установленного пользователем</param>
        ///// </summary>
        ///// </summary>
        //private void updateDataValues()
        //{
        //    int err = -1
        //        //, cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
        //        , iAVG = -1
        //        , iRegDbConn = -1; // признак установленного соединения (ошибка, был создан ранее, новое соединение)
        //    string errMsg = string.Empty;

        //    m_handlerDb.RegisterDbConnection(out iRegDbConn);

        //    if (!(iRegDbConn < 0))
        //    {
        //        // установить значения в таблицах для расчета, создать новую сессию
        //        // предыдущая сессия удалена в 'clear'
        //        setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg);

        //        if (err == 0)
        //        {
        //            // создать копии для возможности сохранения изменений
        //            setValues();
        //            // отобразить значения
        //            m_dgvValues.ShowValues(m_TableEdit/*, m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]*/);
        //        }
        //        else
        //        {
        //            // в случае ошибки "обнулить" идентификатор сессии
        //            deleteSession();

        //            throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
        //        }
        //    }
        //    else
        //        ;

        //    if (!(iRegDbConn > 0))
        //        m_handlerDb.UnRegisterDbConnection();
        //    else
        //        ;
        //}

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(HandlerDbTaskTepCalculate.RESULT res)
        {
            // отобразить значения
            m_dgvValues.ShowValues(m_TableEdit/*, m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]*/);
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Загрузить ранее сохраненные значения")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void panelManagement_btnHistory_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
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
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение периода расчета)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            //??? проверить сохранены ли значения
            m_dgvValues.ClearRows();
            //??? зачем и столбцы тоже - вероятно, предполагаем, что в другом периоде другие компоненты?
            m_dgvValues.ClearColumns();

            //Очистить списки - элементы интерфейса
            (PanelManagement as PanelManagementTaskTepValues).Clear();

            base.panelManagement_Period_onChanged();
            // здесь заканчились все параметры расчета, компоненты станции - можно начать формировать структуру представления
            //m_dgvValues.BuildStructure();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);

            (PanelManagement as PanelManagementTaskTepValues).AddNAlgParameter(obj);

            // добавить свойства для строки таблицы со значениями
            m_dgvValues.AddNAlgParameter(obj);
            // в процессе создаем структуру, т.к. она простая
            // , иначе требовалось бы подаждать добавления всех параметров 'NAlg'
            (m_dgvValues as DataGridViewTaskTepValues).AddRow(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);

            // добавить свойства для строки таблицы со значениями
            m_dgvValues.AddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(TepCommon.HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);

            (PanelManagement as PanelManagementTaskTepValues).AddComponent(obj);

            (m_dgvValues as DataGridViewTaskTepValues).AddComponent(obj);
            (m_dgvValues as DataGridViewTaskTepValues).AddColumn(obj);
        }
        #endregion

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs ev)
        {
            //??? где сохраняются изменения. только на элементе управления?
            ;
            //??? Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            ;
            //Изменить структуру 'DataGridView'
            //m_dgvValues.UpdateStructure ();            
            (m_dgvValues as DataGridViewTaskTepValues).UpdateStructure(ev);
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected abstract void onEventCellValueChanged(object dgv, DataGridViewTaskTepValues.DataGridViewTEPValuesCellValueChangedEventArgs ev);

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
