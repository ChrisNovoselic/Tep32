using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
//using InterfacePlugIn;

namespace TepCommon
{
    public abstract class PanelTepTaskValues : HPanelTepCommon
    {
        public PanelTepTaskValues(IPlugIn iFunc)
            : base(iFunc)
        {
        }
    }
}
