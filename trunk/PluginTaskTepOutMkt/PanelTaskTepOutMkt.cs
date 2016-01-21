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

namespace PluginTaskTepOutMkt
{
    public class PanelTaskTepOutMkt : PanelTaskTepOutVal
    {
        /// <summary>
        /// Перечисление - индексы таблиц для значений
        ///  , собранных в автоматическом режиме
        ///  , "по умолчанию"
        /// </summary>
        private enum INDEX_TABLE_VALUES : int { REGISTRED, COUNT }
        public PanelTaskTepOutMkt(IPlugIn iFunc)
            : base(iFunc, ID_START_RECORD.ALG)
        {
            m_arTableOrigin = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override System.Data.DataTable m_TableOrigin
        {
            get { throw new NotImplementedException(); }
        }

        protected override System.Data.DataTable m_TableEdit
        {
            get { throw new NotImplementedException(); }
        }

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        protected override void setValues(ref DbConnection dbConn, out int err, out string strErr)
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : PlugInTepTaskCalculate
    {
        public PlugIn()
            : base()
        {
            _Id = 28;

            _nameOwnerMenuItem = @"Задача\Расчет ТЭП";
            _nameMenuItem = @"Выход-макет";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTaskTepOutMkt));

            base.OnClickMenuItem(obj, ev);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
