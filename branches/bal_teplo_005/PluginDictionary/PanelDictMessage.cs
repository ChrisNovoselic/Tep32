using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictMessage : HPanelEditList
    {
        public PanelDictMessage(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"messages", @"ID", @"DESCRIPTION")
        {
            InitializeComponent();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
        }
    }
}
