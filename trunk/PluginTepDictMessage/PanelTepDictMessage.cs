using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictMessage
{
    public class PanelTepDictMessage : HPanelEditList
    {
        public PanelTepDictMessage(IPlugIn iFunc)
            : base(iFunc, @"messages", @"ID", @"DESCRIPTION")
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
            _Id = 4;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Типы сообщений журнала";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictMessage));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
