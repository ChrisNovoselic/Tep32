using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictTime
{
    public class PanelDictTime : HPanelEditList
    {
        public PanelDictTime(IPlugIn iFunc)
            : base(iFunc, @"time", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();

            Description[(int)ID_DESC.Tab] = "Вкладка для редактирования временных интервалов ПО";
            Description[(int)ID_DESC.Group] = "Группа для настроек";
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
            _Id = 3;
            register(3, typeof(PanelDictTime), @"Настройка", @"Интервалы времени");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem (obj, ev);
        }
    }
}
