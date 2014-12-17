using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictPlugIns
{
    public class PanelTepDictMessage : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictMessage(IPlugIn iFunc)
            : base(@"messages")
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
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
            _Id = 4;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Типы сообщений журнала";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictMessage));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
