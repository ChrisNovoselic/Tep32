using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PanelDictMessage : HPanelEditList
    {
        public PanelDictMessage(IPlugIn iFunc)
            : base(iFunc, @"messages", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
}
