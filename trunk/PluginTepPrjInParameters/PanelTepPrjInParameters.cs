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

namespace PluginTepPrjInParameters
{
    public class PluginTepPrjInParameters : HPanelEditTree
    {
        public PluginTepPrjInParameters(IPlugIn iFunc)
            : base(iFunc)
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
            _Id = 8;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Входные параметры";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepPrjInParameters));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
