using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginProject
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
}
