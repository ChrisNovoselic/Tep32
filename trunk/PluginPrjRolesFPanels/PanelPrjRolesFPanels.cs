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

namespace PluginPrjRolesFPanels
{
    public class PanelPrjRolesFPanels : PanelPrjRolesAccess
    {
        public PanelPrjRolesFPanels(IPlugIn iFunc)
            : base(iFunc, @"roles", @"ID_EXT,ID_FPANEL", @"fpanels", @"IsUse")
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
            register(7, typeof(PanelPrjRolesFPanels), @"Проект\Права доступа", @"Роли (группы)");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
