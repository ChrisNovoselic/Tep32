using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictProfilesUnit
{
    public class PluginTepDictProfilesUnit : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PluginTepDictProfilesUnit(IPlugIn iFunc)
            : base(@"profiles_unit", @"DESCRIPTION")
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
            _Id = 6;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Элементы интерфейса";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepDictProfilesUnit));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
