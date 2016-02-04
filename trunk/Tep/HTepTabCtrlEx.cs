using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace Tep64
{
    public partial class HTepTabCtrlEx : HClassLibrary.HTabCtrlEx
    {
        public HTepTabCtrlEx()
        {
            InitializeComponent();
        }

        public HTepTabCtrlEx(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public int SelectedId
        {
            get
            {
                int iRes = -1
                    , indxSel = -1;

                indxSel = SelectedIndex;

                if (indxSel < 0)
                    if (TabPages.Count > 0)
                        indxSel = 0;
                    else
                        ;
                else
                    ;

                if (!(indxSel < 0))
                    iRes = m_listPropTabs[indxSel].id;
                else
                    ;

                return iRes;
            }
        }
    }
}
