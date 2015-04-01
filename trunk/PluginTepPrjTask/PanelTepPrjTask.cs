using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepPrjTask
{
    public class PanelTepPrjTask : HPanelEditList
    {
        public PanelTepPrjTask(IPlugIn iFunc)
            : base(iFunc, @"task", @"ID", @"DESCRIPTION")
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
            _Id = 9;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Список задач ПК";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepPrjTask));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
