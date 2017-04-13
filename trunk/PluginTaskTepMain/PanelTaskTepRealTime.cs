using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;
using System.Drawing;

namespace PluginTaskTepMain
{
    public partial class PanelTaskTepRealTime : PanelTaskTepCalculate
    {
        public PanelTaskTepRealTime(IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME)
        {
            InitializeComponent();

            (Controls.Find(INDEX_CONTROL.BUTTON_RUN.ToString(), true)[0] as Button).Click += new EventHandler(btnRunRes_onClick);
        }
        /// <summary>
        /// Создание, размещение элементов управления
        /// </summary>
        private void InitializeComponent()
        {
            m_dgvValues = new DataGridViewTEPRealTime();
            int posColdgvTEPValues = 4
                , heightRowdgvTEPValues = 10;

            SuspendLayout();

            Controls.Add(PanelManagement, 0, 0);
            SetColumnSpan(PanelManagement, posColdgvTEPValues); SetRowSpan(PanelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvTEPValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvTEPValues); SetRowSpan(m_dgvValues, heightRowdgvTEPValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvTEPValues, heightRowdgvTEPValues);

            ResumeLayout(false);
            PerformLayout();
        }
        /// <summary>
        /// Получить значения из БД
        /// </summary>
        /// <param name="dbConn">Объект с установленным соединением с БД</param>
        /// <param name="err">Признак ошибки при выполнении функции</param>
        /// <param name="errMsg">Детализация ошибки (при наличии)</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            base.initialize(out err, out errMsg);
        }
        ///// <summary>
        ///// Заполнение значениями элементов управления
        ///// </summary>
        //protected override void initialize()
        //{
        //}
        /// <summary>
        /// Обработчик события - нажатие кнопки "Результирующее действие - Расчет"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void btnRunRes_onClick(object obj, EventArgs ev)
        {
            int err = -1;
            string strErr = string.Empty;

            // удалить устаревшую сессию
            HandlerDb.Clear();
            //??? создать новую сессию
            // установить/отобразить значения
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
            // произвести расчет
            HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME);            
        }
        ///// <summary>
        ///// Инициировать подготовку к расчету
        /////  , выполнить расчет
        /////  , актуализировать таблицы с временными значениями
        ///// </summary>
        ///// <param name="type">Тип требуемого расчета</param>
        //protected override void btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        //{
        //    throw new NotImplementedException();
        //}
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTEPRealTime : DataGridViewTaskTepCalculate
        {
            public DataGridViewTEPRealTime() : base ()
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents ()
            {
            }            

            public override void ShowValues(IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> inValues, IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> outValues, out int err)
            {
                err = 0;
            }

            public override void ClearColumns()
            {
            }

            public override void ClearRows()
            {
            }

            public override void ClearValues()
            {
            }

            public override void AddColumns(List<TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                throw new NotImplementedException();
            }

            protected override void addColumn(TepCommon.HandlerDbTaskCalculate.TECComponent comp, ModeAddColumn mode)
            {
                throw new NotImplementedException();
            }
        }

        protected override PanelTaskTepCalculate.PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementTaskTepRealTime ();
        }

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(HPanelTepCommon.PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateCompleted(TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(object obj)
        {
            throw new NotImplementedException();
        }

        private class PanelManagementTaskTepRealTime : HPanelTepCommon.PanelManagementTaskCalculate
        {
            public PanelManagementTaskTepRealTime()
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

                posRow = 0;
                ////Период расчета - подпись, значение
                //SetPositionPeriod(new Point(0, posRow), new Size(this.ColumnCount / 2, 1));

                ////Период расчета - подпись, значение
                //SetPositionTimezone(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount / 2, 1));

                ////??? значение для 'posRow'
                ////Дата/время начала периода расчета
                //posRow = SetPositionDateTimePicker(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount, 4));

                //Расчет - выполнить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"Расчет";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 2);

                ResumeLayout(false);
                PerformLayout();
            }
            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                throw new NotImplementedException();
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                throw new NotImplementedException();
            }
        }
    }

    public partial class PanelTaskTepRealTime
    {
        private enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , BUTTON_RUN
            , LABEL_DESC
                , COUNT
        }
    }
}

