using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskAutobook
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 23;

            register(23, typeof(PanelTaskAutobookMonthValues), @"Задача\Учет активной э/э", @"Помесячно");
            register(29, typeof(PanelTaskAutobookYearlyPlan), @"Задача\Учет активной э/э", @"Годовой план");            
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
