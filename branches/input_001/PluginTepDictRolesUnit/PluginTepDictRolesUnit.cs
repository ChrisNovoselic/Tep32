using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictRolesUnit
{
    public class PanelTepDictRolesUnit : HPanelEditList
    {
        public PanelTepDictRolesUnit(IPlugIn iFunc)
            : base(iFunc, @"roles_unit", @"ID", @"DESCRIPTION")
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
            _Id = 5;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Роли(группы) пользователей";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictRolesUnit));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
