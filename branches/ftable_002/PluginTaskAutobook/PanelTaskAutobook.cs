﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskAutobook
{
    public class PluginTaskAutobook : HPanelTepCommon
    {
        public PluginTaskAutobook(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initialize(ref System.Data.Common.DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
        }

        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 23;

            _nameOwnerMenuItem = @"Задача";
            _nameMenuItem = @"Учет активной э/э";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginTaskAutobook));

            base.OnClickMenuItem(obj, ev);
        }
    }
}

