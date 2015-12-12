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

namespace PluginPrjRolesPlugIns
{
    public class PanelPrjRolesPlugIns : PanelPrjRolesAccess
    {
        public PanelPrjRolesPlugIns(IPlugIn iFunc)
            : base(iFunc, @"roles", @"ID_EXT,ID_PLUGIN", @"plugins", @"IsUse")
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
            _Id = 7;

            _nameOwnerMenuItem = @"Проект\Права доступа";
            _nameMenuItem = @"Роли (группы)";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelPrjRolesPlugIns));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
