using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public abstract class HPanelEditTree : HPanelTepCommon
    {
        public HPanelEditTree(IPlugIn plugIn)
            : base(plugIn)
        {
        }
    }
}
