using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictPlugIns
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

    public class PlugIn : HFuncDbEdit
    {      
        public PlugIn () : base () {            
            _Id = 2;
            register(2, typeof(PanelDictPlugIns), @"Настройка", @"Состав плюгин'ов");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
