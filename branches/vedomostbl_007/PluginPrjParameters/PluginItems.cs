using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjParameters
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 8;

            register(8, typeof(PanelPrjInParameters), @"Проект\Параметры", @"Входные");
            register(12, typeof(PanelPrjOutParameters), @"Проект\Параметры", @"Выходные");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
