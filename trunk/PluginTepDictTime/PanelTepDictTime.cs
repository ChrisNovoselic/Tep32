using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PanelTepDictTime
{
    public class PanelTepDictTime : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictTime(IPlugIn iFunc)
            : base(@"time")
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }

        private void InitializeComponent()
        {
        }

        public void Initialize(object obj)
        {
            try
            {
                if (this.IsHandleCreated == true)
                    //if (this.InvokeRequired == true)
                    this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelTepDictPlugIns::Initialize () - BeginInvoke (initialize) - ...");
            }
        }
    }

    public class PlugIn : HFunc
    {
        public PlugIn()
            : base()
        {
            _Id = 3;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Интервалы времени";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTepDictTime));

            if (m_markDataHost.IsMarked((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB) == false)
                DataAskedHost((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);
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
                    ((PanelTepDictTime)_object).Initialize(obj);
                    break;
                default:
                    break;
            }
        }
    }
}
