using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictTime : HPanelEditList
    {
        public PanelDictTime(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"time", @"ID", @"DESCRIPTION")
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
