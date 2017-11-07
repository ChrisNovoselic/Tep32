using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictFPanels : HPanelEditList
    {
        public PanelDictFPanels(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"fpanels", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();

            //Дополнительные действия при сохранении значений
            delegateSaveAdding = saveAdding;
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent () { }

        private void saveAdding() { /*дополнительные действия не требуются*/ }
    }
}
