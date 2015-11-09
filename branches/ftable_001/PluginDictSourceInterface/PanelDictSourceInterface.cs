using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictSourceInterface
{
    public class PanelDictSourceInterface : HPanelEditList
    {
        public PanelDictSourceInterface(IPlugIn iFunc)
            : base(iFunc, @"source_interface", @"ID", @"DESCRIPTION")
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
            _Id = 13;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Интерфейсы источников данных";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelDictSourceInterface));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
