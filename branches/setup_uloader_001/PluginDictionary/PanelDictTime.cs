﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginDictionary
{
    public class PanelDictTime : HPanelEditList
    {
        public PanelDictTime(IPlugIn iFunc)
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
