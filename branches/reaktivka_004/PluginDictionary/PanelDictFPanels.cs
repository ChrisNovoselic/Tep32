﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictFPanels : HPanelEditList
    {
        public PanelDictFPanels(IPlugIn iFunc)
            : base(iFunc, @"fpanels", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();

            //Дополнительные действия при сохранении значений
            delegateSaveAdding = saveAdding;
        }

        private void InitializeComponent () {
        }

        private void saveAdding() { /*дополнительные действия не требуются*/ }
    }
}
