using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; //ToolStripMenuItem

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictPlugIns
{
    public class PanelTepDictPlugIns : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictPlugIns(IPlugIn iFunc) : base (@"plugins")
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }

        private void InitializeComponent () {
        }
    }

    public class PlugIn : HFuncDictEdit
    {      
        public PlugIn () : base () {            
            _Id = 2;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Состав плюгин'ов";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictPlugIns));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
