using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictRolesUnit
{
    public class PanelDictRolesUnit : HPanelEditList
    {
        public PanelDictRolesUnit(IPlugIn iFunc)
            : base(iFunc, @"roles_unit", @"ID", @"DESCRIPTION")
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
            _Id = 5;
            register(5, typeof(PanelDictRolesUnit), @"Проект", @"Роли(группы) пользователей");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
