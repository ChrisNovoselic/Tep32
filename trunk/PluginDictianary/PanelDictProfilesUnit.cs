﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PanelDictProfilesUnit : HPanelEditList
    {
        public PanelDictProfilesUnit(IPlugIn iFunc)
            : base(iFunc, @"profiles_unit", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
}
