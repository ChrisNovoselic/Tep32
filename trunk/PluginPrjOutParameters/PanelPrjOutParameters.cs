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
    public class PanelPrjOutParameters : PanelPrjParametersEditTree
    {
        public PanelPrjOutParameters(IPlugIn iFunc)
            : base(iFunc, @"outalg, output")
        {
            //Вариант №1-1
            m_listIDLevels = new List<ID_LEVEL> { ID_LEVEL.TASK, ID_LEVEL.N_ALG, /*ID_LEVEL.TIME,*/ ID_LEVEL.COMP, ID_LEVEL.PUT };
            m_arIsShowDetailLevels = new bool[] { false, true, false, false };

            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initTreeNodes()
        {
            //Вариант №1-1
            m_listLevelParameters = new List<LEVEL_PARAMETERS>();
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_DICTPRJ.TASK], @"ID", string.Empty, @"DESCRIPTION", string.Empty));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM], @"ID", @"ID_TASK={ID_PARENT_0}", @"N_ALG", @"NAME_SHR"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_DICTPRJ.TIME], @"ID", string.Empty, @"DESCRIPTION", string.Empty));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.PUT], @"ID_COMP,ID", @"ID_ALG={ID_PARENT_1} AND ID_TIME={ID_PARENT_0}", string.Empty, string.Empty));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.PUT], @"ID_COMP,ID", @"ID_ALG={ID_PARENT_0}", string.Empty, string.Empty));
            m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_DICTPRJ.COMP_LIST], string.Empty, @"ID={ID_PARENT_1}", @"DESCRIPTION", string.Empty));

            ////Вариант №1-2
            //m_listLevelParameters = new List<LEVEL_PARAMETERS>();
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_KEY.TASK], @"ID", string.Empty, @"DESCRIPTION"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_KEY.TIME], @"ID", string.Empty, @"DESCRIPTION"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM], @"ID", @"ID_TASK={ID_PARENT_1}", @"N_ALG"));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableOrigin[(int)INDEX_PARAMETER.PUT], @"ID_COMP,ID", @"ID_ALG={ID_PARENT_0} AND ID_TIME={ID_PARENT_1}", string.Empty));
            //m_listLevelParameters.Add(new LEVEL_PARAMETERS(m_arTableDictPrj[(int)INDEX_TABLE_KEY.COMP_LIST], string.Empty, @"ID={ID_PARENT_1}", @"DESCRIPTION"));

            base.initTreeNodes();
        }

        protected override int reAddNodes(int indxLevel, TreeNode node_parent, string strId_parent)
        //protected override int reAddNodes(TreeNode node_parent)
        {
            int iRes = 0;

            TreeNode node = null
                , node_norm = null
                , node_mkt = null;
            TreeNodeCollection nodes = null;
            string //strId_parent = node_parent == null ? string.Empty : node_parent.Name,
                strId = string.Empty
                , strKey = string.Empty
                , strItem = string.Empty;
            DataRow[] rows;
            int //indxLevel = node_parent == null ? 0 : node_parent.Level + 1,
                iAdd = 0
                , iId = -1;

            if (indxLevel < m_listLevelParameters.Count)
            {
                if (node_parent == null)
                    nodes = m_ctrlTreeView.Nodes;
                else
                {
                    #region код для учета особенности структуры для задачи с ИД = 1 (Расчет ТЭП)
                    if ((indxLevel == 1)
                        && (getIdNodePart(node_parent.Name, ID_LEVEL.TASK).Equals(@"1") == true))
                    {
                        node_norm = node_parent.Nodes.Add(@"1::norm", @"Норматив");
                        node_mkt = node_parent.Nodes.Add(@"1::mkt", @"Макет");
                    }
                    else
                    #endregion
                        nodes = node_parent.Nodes;
                }

                rows = m_listLevelParameters[indxLevel].Select(strId_parent);

                foreach (DataRow r in rows)
                {
                    //Строка с идентификатором задачи
                    //strId = r[m_listLevelParameters[indxLevel].id].ToString().Trim();
                    strId = m_listLevelParameters[indxLevel].GetId(r);
                    if (strId.Equals(string.Empty) == false)
                        strKey = concatIdNode(node_parent, strId);
                    else
                        strKey = strId_parent;

                    #region код для учета особенности структуры для задачи с ИД = 1 (Расчет ТЭП)
                    if ((indxLevel == 1)
                        && (getIdNodePart(node_parent.Name, ID_LEVEL.TASK).Equals(@"1") == true))
                    {
                        iId = int.Parse(strId);
                        if ((iId > 10000)
                            && (iId < 15000))
                            nodes = node_norm.Nodes;
                        else
                            if (iId > 25000)
                                nodes = node_mkt.Nodes;
                            else
                                throw new Exception(@"PanelPrjOutParameters::reAddNodes (ID_NODE=" + iId + @") - неизвестный диапазон");
                    }
                    else
                        ;
                    #endregion

                    if (nodes.Find(strKey, false).Length == 0)
                    {
                        //Элемент дерева для очередной задачи
                        if (m_listLevelParameters[indxLevel].desc.Equals(string.Empty) == false)
                        {
                            strItem = r[m_listLevelParameters[indxLevel].desc].ToString().Trim();
                            if (m_listLevelParameters[indxLevel].desc_detail.Equals(string.Empty) == false)
                                strItem += @" (" + r[m_listLevelParameters[indxLevel].desc_detail].ToString().Trim() + @")";
                            else
                                ;
                            node = nodes.Add(strKey, strItem);
                            iRes++;
                        }
                        else
                        {
                            node = node_parent;
                        }

                        if ((indxLevel + 1) < m_listLevelParameters.Count)
                        {
                            iAdd =
                                reAddNodes(indxLevel + 1, node, strKey)
                                //reAddNodes(node)
                                    ;
                            if (iAdd == 0)
                            {
                                if (indxLevel > 0)
                                {
                                    nodes.Remove(node);
                                    iRes--;
                                }
                                else
                                    addNodeNull(node);
                            }
                            else
                                iRes += iAdd;
                        }
                        else
                            ;
                    }
                    else
                        ; // нельзя добавить элемент с имеющимся ключом
                }
            }
            else
                ;

            return iRes;
        }

        protected override void btnEnable(string strIdTask)
        {
            bool bNewIsButtonAddEnabled = false;
            int idTask = -1;

            base.btnEnable(strIdTask);

            if (Int32.TryParse(strIdTask, out idTask) == true)
                if ((m_Level == ID_LEVEL.TASK) && (idTask == 1))
                {
                    switch (m_Level)
                    {
                        case ID_LEVEL.TASK:
                            //??? требуется знать идентификатор задачи
                            bNewIsButtonAddEnabled = false;
                            break;
                        default:
                            break;
                    }

                    Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0].Enabled = bNewIsButtonAddEnabled;
                }
                else
                    ;
            else
                ;
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
