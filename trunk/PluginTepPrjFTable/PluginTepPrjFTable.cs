using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepPrjFTable
{
    public class PluginTepPrjFTable : HPanelEditTree
    {
        protected override void Activate(bool activate)
        {
            throw new NotImplementedException();
        }

        /*protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            err = -1;
            errMsg = string.Empty;
        }

        
        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }*/

        public PluginTepPrjFTable(IPlugIn iFunc)
            : base(iFunc,@"ftable")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
 
        }
    }

    public class PlugIn : HFuncDictEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 16;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Нормативные графики";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepPrjFTable));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
