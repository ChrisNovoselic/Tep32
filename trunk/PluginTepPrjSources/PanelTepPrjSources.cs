using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepPrjSources
{
    public class PanelTepPrjSources : HPanelEditList
    {
        public PanelTepPrjSources(IPlugIn iFunc)
            : base(iFunc, @"SOURCE", @"ID", @"DESCRIPTION")
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
            _Id = 15;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Список источников данных";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepPrjSources));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
