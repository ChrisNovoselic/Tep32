using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictSourceInterface
{
    public class PanelTepDictSourceInterface : HPanelEditList
    {
        public PanelTepDictSourceInterface(IPlugIn iFunc)
            : base(iFunc, @"source_interface", @"ID", @"DESCRIPTION")
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
            _Id = 13;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Интерфейсы источников данных";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictSourceInterface));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
