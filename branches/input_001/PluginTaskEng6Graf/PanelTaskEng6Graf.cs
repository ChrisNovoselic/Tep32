using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskEng6Graf
{
    public class PluginTaskEng6Graf : HPanelTepCommon
    {
        public PluginTaskEng6Graf(IPlugIn iFunc)
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
            _Id = 26;

            _nameOwnerMenuItem = @"Задача";
            _nameMenuItem = @"Графики 3-х мин";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTaskEng6Graf));

            base.OnClickMenuItem(obj, ev);
        }
    }
}

