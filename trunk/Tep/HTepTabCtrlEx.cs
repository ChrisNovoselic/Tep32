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

        public int GetTabPageId()
        {
            return GetTabPageId(SelectedIndex);
        }

        public int GetTabPageId (int indx)
        {
            int iRes = -1;

            if (indx == TabPages.Count)
                if (!(_propTabLastRemoved == null))
                    iRes = _propTabLastRemoved.Value.id;
                else
                    ;
            else
                if ((!(indx < 0))
                    && (TabPages.Count > 0))
                    if (indx < TabPages.Count)
                        iRes = m_listPropTabs[indx].id;
                    else
                        ;                        
                else
                    ;

            return iRes;
        }
    }
}
