using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using TepCommon;
using InterfacePlugIn;

namespace PluginTaskReaktivka
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 24;
            register(24, typeof(PanelTaskReaktivka), @"Задача", @"Учет реактивной э/э");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

