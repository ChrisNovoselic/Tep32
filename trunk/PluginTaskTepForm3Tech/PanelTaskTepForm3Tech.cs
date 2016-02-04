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
    public class PanelTaskTepForm3Tech : HPanelTepCommon
    {
        public PanelTaskTepForm3Tech(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
        }

        protected override void recUpdateInsertDelete(out int err)
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
            createObject(typeof(PanelTaskTepForm3Tech));

            base.OnClickMenuItem(obj, ev);
        }
    }
}

