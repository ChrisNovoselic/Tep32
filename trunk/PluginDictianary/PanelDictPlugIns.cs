using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictianary
{
    public class PanelDictPlugIns : HPanelEditList
    {
        public PanelDictPlugIns(IPlugIn iFunc)
            : base(iFunc, @"plugins", @"ID", @"DESCRIPTION")
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
