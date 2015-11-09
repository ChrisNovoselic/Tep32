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

        private void saveAdding(int iListenerId)
        {
        }
    }

    public class PlugIn : HFuncDbEdit
    {      
        public PlugIn () : base () {            
            _Id = 2;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Состав плюгин'ов";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelDictPlugIns));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
