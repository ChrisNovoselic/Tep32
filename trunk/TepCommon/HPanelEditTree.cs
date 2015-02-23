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
    public partial class HPanelEditTree : HPanelTepCommon
    {
        public HPanelEditTree(IPlugIn plugIn, string nameTable, string keyFields)
            : base(plugIn, nameTable, keyFields)
        {
        }
        
        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            throw new NotImplementedException();
        }

        protected override void Activate(bool activate)
        {
            throw new NotImplementedException();
        }
    }

    partial class HPanelEditTree
    {        
    }
}
