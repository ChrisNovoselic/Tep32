using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepTaskOutval
{
    public class PluginTepTaskOutval : PanelTepTaskValues
    {
        public PluginTepTaskOutval(IPlugIn iFunc)
            : base(iFunc, @"outalg", @"output")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }

    public class PlugIn : PlugInTepTaskValues
    {
        public PlugIn()
            : base()
        {
            _Id = 18;

            _nameOwnerMenuItem = @"Задача\Расчет ТЭП";
            _nameMenuItem = @"Выходные данные";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepTaskOutval));

            base.OnClickMenuItem(obj, ev);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
