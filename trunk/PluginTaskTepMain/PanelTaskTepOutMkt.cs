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

namespace PluginTaskTepMain
{
    public class PanelTaskTepOutMkt : PanelTaskTepOutVal
    {
        ///// <summary>
        ///// Перечисление - индексы таблиц для значений
        /////  , собранных в автоматическом режиме
        /////  , "по умолчанию"
        ///// </summary>
        //private enum INDEX_TABLE_VALUES : int { REGISTRED, COUNT }

        public PanelTaskTepOutMkt(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        {
            //m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            //m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        //protected override System.Data.DataTable m_TableOrigin
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //protected override System.Data.DataTable m_TableEdit
        //{
        //    get { throw new NotImplementedException(); }
        //}

        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        //protected override void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        //{
        //    throw new NotImplementedException();
        //}

        protected override PanelTaskTepCalculate.PanelManagementTaskTepCalculate createPanelManagement()
        {
            return new PanelManagementTaskTepOuMkt();
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepOuMkt : PanelManagementTaskTepOutVal
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
                ctrl.Text = @"К нормативу";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, iRes = iRes + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                return iRes;
            }
        }
    }
}
