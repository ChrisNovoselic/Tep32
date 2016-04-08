using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
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
}
