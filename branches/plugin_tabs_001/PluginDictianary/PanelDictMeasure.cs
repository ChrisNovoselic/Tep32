using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PanelDictMeasure : HPanelEditList
    {
        public PanelDictMeasure(IPlugIn iFunc)
            : base(iFunc, @"measure", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
}
