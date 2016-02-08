using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictProfilesUnit
{
    public class PanelDictProfilesUnit : HPanelEditList
    {
        public PanelDictProfilesUnit(IPlugIn iFunc)
            : base(iFunc, @"profiles_unit", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 6;
            register(6, typeof(PanelDictProfilesUnit), @"Настройка", @"Элементы интерфейса");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
