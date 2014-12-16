using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public partial class HPanelEdit : HObjectDictEdit
    {
        public HPanelEdit(string nameTable) : base (nameTable)
        {
            InitializeComponent();
        }

        public HPanelEdit(IContainer container, string nameTable)
            : this(nameTable)
        {
            container.Add(this);
        }

        protected void Activate (bool activate) {
        }
    }
}
