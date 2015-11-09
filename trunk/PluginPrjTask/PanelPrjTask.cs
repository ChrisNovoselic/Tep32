using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjTask
{
    public class PanelPrjTask : HPanelEditList
    {
        public PanelPrjTask(IPlugIn iFunc)
            : base(iFunc, @"task", @"ID", @"DESCRIPTION")
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
            _Id = 9;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Список задач ИРС";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelPrjTask));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
