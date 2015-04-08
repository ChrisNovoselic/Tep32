using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepPrjRolesProfiles
{
    public class PanelTepPrjRolesProfiles : PanelTepPrjRolesAccess
    {
        public PanelTepPrjRolesProfiles(IPlugIn iFunc)
            : base(iFunc, @"profiles", @"ID_EXT,ID_UNIT", @"profiles_unit")
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
            _Id = 11;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Права доступа элементов интерфейса";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepPrjRolesProfiles));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
