using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskTepMain
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 17;

            register(17, typeof(PanelTaskTepInval), @"Задача\Расчет ТЭП", @"Входные данные");
            register(18, typeof(PanelTaskTepOutNorm), @"Задача\Расчет ТЭП", @"Выход-норматив");
            register(27, typeof(PanelTaskTepRealTime), @"Задача\Расчет ТЭП", @"Оперативно");
            register(28, typeof(PanelTaskTepOutMkt), @"Задача\Расчет ТЭП", @"Выход-макет");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
