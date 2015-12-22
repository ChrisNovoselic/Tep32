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

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Роли(группы) пользователей";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelDictRolesUnit));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
