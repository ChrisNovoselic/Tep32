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

namespace PluginTepPrjOutParameters
{
    public class PluginTepPrjOutParameters : HPanelEditTree
    {
        public PluginTepPrjOutParameters(IPlugIn iFunc)
            : base(iFunc, @"outalg, output")
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
            _Id = 12;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Выходные параметры";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepPrjOutParameters));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
