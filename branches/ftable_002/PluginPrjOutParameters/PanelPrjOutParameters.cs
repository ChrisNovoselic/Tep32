using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjOutParameters
{
    public class PanelPrjOutParameters : PanelTepPrjParametersEditTree
    {
        public PanelPrjOutParameters(IPlugIn iFunc)
            : base(iFunc, @"outalg, output")
        {
            //Вариант №1-1
            m_listIDLevels = new List<ID_LEVEL> { ID_LEVEL.TASK, ID_LEVEL.N_ALG, ID_LEVEL.TIME, ID_LEVEL.COMP, ID_LEVEL.PUT };
            m_arIsShowDetailLevels = new bool[] { false, true, true, false };

            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initTreeNodes()
        {
            //Вариант №1-1
            m_listLevelParameters = new List<LEVEL_PARAMETERS>();
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.TASK], @"ID", string.Empty, @"DESCRIPTION"));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM], @"ID", @"ID_TASK={ID_PARENT_0}", @"N_ALG"));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.TIME], @"ID", string.Empty, @"DESCRIPTION"));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.PUT], @"ID_COMP,ID", @"ID_ALG={ID_PARENT_1} AND ID_TIME={ID_PARENT_0}", string.Empty));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.COMP_LIST], string.Empty, @"ID={ID_PARENT_1}", @"DESCRIPTION"));

            ////Вариант №1-2
            //m_listLevelParameters = new List<LEVEL_PARAMETERS>();
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.TASK], @"ID", string.Empty, @"DESCRIPTION"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.TIME], @"ID", string.Empty, @"DESCRIPTION"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM], @"ID", @"ID_TASK={ID_PARENT_1}", @"N_ALG"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.PUT], @"ID_COMP,ID", @"ID_ALG={ID_PARENT_0} AND ID_TIME={ID_PARENT_1}", string.Empty));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableKey[(int)INDEX_TABLE_KEY.COMP_LIST], string.Empty, @"ID={ID_PARENT_1}", @"DESCRIPTION"));

            base.initTreeNodes();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 12;

            _nameOwnerMenuItem = @"Проект\Параметры";
            _nameMenuItem = @"Выходные";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelPrjOutParameters));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
