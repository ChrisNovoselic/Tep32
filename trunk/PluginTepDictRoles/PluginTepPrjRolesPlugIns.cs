using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictProfiles
{
    public class PluginTepDictProfiles : HPanelEdit
    {
        public PluginTepDictProfiles(IPlugIn iFunc)
            : base(iFunc, @"roles", @"DESCRIPTION")
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
            _Id = 7;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Права доступа ролей(групп)";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepDictProfiles));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
