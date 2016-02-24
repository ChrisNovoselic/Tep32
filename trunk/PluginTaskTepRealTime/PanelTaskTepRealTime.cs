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

namespace PluginTaskTepRealTime
{
    public partial class PanelTaskTepRealTime : PanelTaskTepCalculate
    {
        public PanelTaskTepRealTime(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME)
        {
            InitializeComponent();
            //Обязательно наличие объекта - панели управления
            activateDateTimeRangeValue_OnChanged(true);
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
        /// <summary>
        /// Заполнение значениями элементов управления
        /// </summary>
        protected override void initialize()
        {            
        }

        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTEPRealTime : DataGridViewTEPCalculate
        {
            public DataGridViewTEPRealTime() : base ()
            {
                InitializeComponents();
            }
            
            private void InitializeComponents ()
            {
            }
            
            public override void AddColumn(int id_comp, string text, bool bVisibled)
            {
            }

            public override void AddRow(ROW_PROPERTY rowProp)
            {
            }

            public override void ShowValues(DataTable values, DataTable parameter)
            {
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

            //public override void UpdateStructure(int id_comp, int id_par, PanelTaskTepValues.INDEX_ID indxDeny, bool bItemChecked)
            //{
            //}
        }

        protected override PanelTaskTepCalculate.PanelManagementTaskTepCalculate createPanelManagement()
        {
            return new PanelManagementTaskTepRealTime ();
        }

        private class PanelManagementTaskTepRealTime : PanelManagementTaskTepCalculate
        {
            public PanelManagementTaskTepRealTime()
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

                posRow = 0;
                //Период расчета - значение
                ctrl = Controls.Find(INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                this.Controls.Remove(ctrl);
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Период расчета - значение
                ctrl = Controls.Find(INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                this.Controls.Remove(ctrl);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Расчет - выполнить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"Расчет";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 2);

                //Дата/время начала периода расчета
                //Дата/время начала периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время начала периода расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 2);
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

                ResumeLayout(false);
                PerformLayout();
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

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 27;
            register(27, typeof(PanelTaskTepRealTime), @"Задача\Расчет ТЭП", @"Оперативно");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

