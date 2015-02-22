using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictTime
{
    public class PanelTepDictTime : HPanelEdit
    {
        public PanelTepDictTime(IPlugIn iFunc)
            : base(iFunc, @"time", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }

    public class PlugIn : HFuncDictEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 3;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Интервалы времени";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictTime));
            //createObject(this.GetType());

            base.OnClickMenuItem (obj, ev);
        }
    }
}
