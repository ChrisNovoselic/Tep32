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

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Элементы интерфейса";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelDictProfilesUnit));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
