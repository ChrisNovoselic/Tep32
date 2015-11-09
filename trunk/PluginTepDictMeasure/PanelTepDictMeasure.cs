using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictMeasure
{
    public class PanelTepDictMeasure : HPanelEditList
    {
        public PanelTepDictMeasure(IPlugIn iFunc)
            : base(iFunc, @"measure", @"ID", @"DESCRIPTION")
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
            _Id = 10;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Единицы измерения";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictMeasure));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
