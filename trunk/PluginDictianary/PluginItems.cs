using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 2;

            register(2, typeof(PanelDictPlugIns), @"Настройка", @"Состав плюгин'ов");
            register(3, typeof(PanelDictTime), @"Настройка", @"Интервалы времени");
            register(4, typeof(PanelDictMessage), @"Настройка", @"Типы сообщений журнала");
            register(6, typeof(PanelDictProfilesUnit), @"Настройка", @"Элементы интерфейса");
            register(10, typeof(PanelDictMeasure), @"Настройка", @"Единицы измерения");
            register(13, typeof(PanelDictSourceInterface), @"Настройка", @"Интерфейсы источников данных");            
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
