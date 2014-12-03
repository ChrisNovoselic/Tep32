using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace TepCommon
{
    public partial class HPanelEdit : TableLayoutPanel
    {
        public HPanelEdit()
        {
            InitializeComponent();
        }

        public HPanelEdit(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
