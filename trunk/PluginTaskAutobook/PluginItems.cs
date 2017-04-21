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

            register((int)ID_FPANEL.AUTOBOOK_MONTHVALUES, typeof(PanelTaskAutobookMonthValues), @"Задача\Учет активной э/э", @"Значения по-суточно");
            register((int)ID_FPANEL.AUTOBOOK_YEARLYPLAN, typeof(PanelTaskAutobookYearlyPlan), @"Задача\Учет активной э/э", @"План по-месячно");            
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
