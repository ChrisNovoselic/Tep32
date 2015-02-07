using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictRolelesUnit
{
    public class PanelTepDictRolesUnit : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictRolesUnit(IPlugIn iFunc)
            : base(@"roles_unit", @"DESCRIPTION")
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
