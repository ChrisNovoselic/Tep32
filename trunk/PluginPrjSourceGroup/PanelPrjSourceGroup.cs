using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjSourceGroup
{
    public class PanelPrjSourceGroup : HPanelEditList
    {
        public PanelPrjSourceGroup(IPlugIn iFunc)
            : base(iFunc, @"source_group", @"ID", @"DESCRIPTION")
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
            _Id = 14;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Группы источников данных";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelPrjSourceGroup));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
