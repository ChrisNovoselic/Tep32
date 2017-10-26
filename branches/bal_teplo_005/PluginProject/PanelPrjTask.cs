﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PanelPrjTask : HPanelEditList
    {
        public PanelPrjTask(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, @"task", @"ID", @"DESCRIPTION")
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
