﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PanelPrjRolesProfiles : PanelPrjRolesAccess
    {
        public PanelPrjRolesProfiles(IPlugIn iFunc)
            : base(iFunc, @"profiles", @"ID_EXT,ID_UNIT", @"profiles_unit", @"VALUE")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {            
        }

        protected override void selectAccessUnit(out int err)
        {
            base.selectAccessUnit(out err);

            List<int> listIdNotAccessUnit = new List<int> ();
            DataRow[] rowsNotAccessUnit = m_tblAccessUnit.Select(@"NOT ID_UNIT=" + 8);
            foreach (DataRow r in rowsNotAccessUnit)
            {
                listIdNotAccessUnit.Add(Int32.Parse (r[@"ID"].ToString().Trim()));
                m_tblAccessUnit.Rows.Remove(r);
            }

            foreach (int id in listIdNotAccessUnit)
            {
                rowsNotAccessUnit = m_tblEdit.Select(m_strKeyFields.Split(',')[1] + @"=" + id);

                foreach (DataRow r in rowsNotAccessUnit)
                    m_tblEdit.Rows.Remove(r);
            }
        }
    }
}
