﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; //ToolStripMenuItem

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictPlugIns
{
    public class PanelTepDictPlugIns : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictPlugIns(IPlugIn iFunc) : base (@"plugins")
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }

        private void InitializeComponent () {
        }

        public void Initialize(object obj)
        {
            try {
                if (this.IsHandleCreated == true)
                //if (this.InvokeRequired == true)
                    this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
                else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTepDictPlugIns::Initialize () - BeginInvoke (initialize) - ...");
            }
        }
    }

    public class PlugIn : HFunc
    {      
        public PlugIn () : base () {            
            _Id = 2;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Состав плюгин'ов";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject (typeof(PanelTepDictPlugIns));

            if (m_markDataHost.IsMarked((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB) == false)
                DataAskedHost ((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);
            else
                ;

            //Передать главной форме параметр
            DataAskedHost(obj);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            //throw new NotImplementedException();

            base.OnEvtDataRecievedHost(obj);

            switch (((EventArgsDataHost)obj).id)
            {
                case (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB:
                    ((PanelTepDictPlugIns)_object).Initialize(obj);
                    break;
                default:
                    break;
            }
        }
    }
}
