using System;
using System.Windows.Forms;

using HClassLibrary;

namespace PluginTaskTepMain
{
    public class PanelTaskTepOutNorm : PanelTaskTepOutVal
    {
        ///// <summary>
        ///// Перечисление - индексы таблиц для значений
        /////  , собранных в автоматическом режиме
        /////  , "по умолчанию"
        ///// </summary>
        //private enum INDEX_TABLE_VALUES : int { REGISTRED, COUNT }

        public PanelTaskTepOutNorm(IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES)
        {
            //m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            //m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
        }

        protected override PanelTaskTepCalculate.PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementTaskTepOutNorm();
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepOutNorm : PanelManagementTaskTepOutVal
        {
            protected override int addButtonRun(int posRow)
            {
                Button ctrl = null;
                int iRes = posRow;
                //Расчет - выполнить - норматив
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_PREV.ToString();
                ctrl.Text = @"К вх.данным";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, iRes = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Расчет - выполнить - макет
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_RES.ToString();
                ctrl.Text = @"К макету";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, iRes = iRes + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                return iRes;
            }
        }
    }
}
