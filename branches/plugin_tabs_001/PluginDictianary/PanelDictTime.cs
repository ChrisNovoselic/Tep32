using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PanelDictTime : HPanelEditList
    {
        public PanelDictTime(IPlugIn iFunc)
            : base(iFunc, @"time", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
}
