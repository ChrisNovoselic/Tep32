using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PanelPrjSourceGroup : HPanelEditList
    {
        public PanelPrjSourceGroup(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"source_group", @"ID", @"DESCRIPTION")
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
