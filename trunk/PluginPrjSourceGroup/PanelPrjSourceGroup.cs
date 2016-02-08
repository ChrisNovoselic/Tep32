using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjSourceGroup
{
    public class PanelPrjSourceGroup : HPanelEditList
    {
        public PanelPrjSourceGroup(IPlugIn iFunc)
            : base(iFunc, @"source_group", @"ID", @"DESCRIPTION")
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
            _Id = 14;
            register(14, typeof(PanelPrjSourceGroup), @"Проект", @"Группы источников данных");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
