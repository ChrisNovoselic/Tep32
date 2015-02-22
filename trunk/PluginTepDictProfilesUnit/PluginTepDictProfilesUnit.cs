﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictProfilesUnit
{
    public class PluginTepDictProfilesUnit : HPanelEdit
    {
        public PluginTepDictProfilesUnit(IPlugIn iFunc)
            : base(iFunc, @"profiles_unit", @"DESCRIPTION")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }

    public class PlugIn : HFuncDictEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 6;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Элементы интерфейса";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTepDictProfilesUnit));
            //createObject(this.GetType());

            base.OnClickMenuItem(obj, ev);
        }
    }
}
