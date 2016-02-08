using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictMeasure
{
    public class PanelDictMeasure : HPanelEditList
    {
        public PanelDictMeasure(IPlugIn iFunc)
            : base(iFunc, @"measure", @"ID", @"DESCRIPTION")
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
            _Id = 10;
            register(10, typeof(PanelDictMeasure), @"Настройка", @"Единицы измерения");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
