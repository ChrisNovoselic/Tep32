using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictProfilesUnit : HPanelEditList
    {
        public PanelDictProfilesUnit(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"profiles_unit", @"ID", @"DESCRIPTION")
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
