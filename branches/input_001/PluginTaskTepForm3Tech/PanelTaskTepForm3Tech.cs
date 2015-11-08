using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskTepForm3Tech
{
    public class PluginTaskTepForm3Tech : HPanelTepCommon
    {
        public PluginTaskTepForm3Tech(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initialize(ref System.Data.Common.DbConnection dbConn, out int err, out string errMsg)
        {
            throw new NotImplementedException();
        }

        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 22;

            _nameOwnerMenuItem = @"Задача\Расчет ТЭП";
            _nameMenuItem = @"Форма 3-тех";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTaskTepForm3Tech));

            base.OnClickMenuItem(obj, ev);
        }
    }
}

