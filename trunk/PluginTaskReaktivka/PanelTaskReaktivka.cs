using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskReaktivka
{
    public class PanelTaskReaktivka : HPanelTepCommon
    {
        public PanelTaskReaktivka(IPlugIn iFunc)
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
            _Id = 24;

            _nameOwnerMenuItem = @"Задача";
            _nameMenuItem = @"Учет реактивной э/э";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTaskReaktivka));

            base.OnClickMenuItem(obj, ev);
        }
    }
}

