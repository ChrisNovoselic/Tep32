using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
//using InterfacePlugIn;

namespace TepCommon
{
    public abstract partial class PanelTepTaskValues : HPanelTepCommon
    {
        public PanelTepTaskValues(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
        }

        protected class PanelManagement
        {
            public PanelManagement()
            {
            }
        }
    }

    public partial class PanelTepTaskValues
    {
        protected enum INDEX_CONTROL { CB_PERIOD }

        protected PanelManagement m_panelManagement;
    }
}
