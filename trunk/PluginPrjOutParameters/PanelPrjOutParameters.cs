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

        protected override int reAddNodes(int indxLevel, TreeNode nodeParent, string strId_parent)
        //protected override int reAddNodes(TreeNode node_parent)
        {
            int iRes = 0;

            TreeNode node = null
                , node_par = null
                , node_norm = null
                , node_mkt = null;
            TreeNodeCollection nodes = null;
            bool bTaskTep = false;
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
                #region код для учета особенности структуры для задачи с ИД = 1 (Расчет ТЭП)
                bTaskTep = ((indxLevel == ((int)ID_LEVEL.TASK + 1))
                    && (getIdNodePart(nodeParent.Name, ID_LEVEL.TASK).Equals(((int)ID_TASK.TEP).ToString()) == true));
                #endregion
                
                if (nodeParent == null)
                    nodes = m_ctrlTreeView.Nodes;
                else
                {
                    #region код для учета особенности структуры для задачи с ИД = 1 (Расчет ТЭП)
                    if (bTaskTep == true)
                    {
                        node_norm = nodeParent.Nodes.Add(concatIdNode(nodeParent.Name, @"norm"), @"Норматив");
                        node_mkt = nodeParent.Nodes.Add(concatIdNode(nodeParent.Name, @"mkt"), @"Макет");
                    }
                    else
                    #endregion
                    {
                        node_par = nodeParent;
                        nodes = node_par.Nodes;
                    }
                }

                rows = m_listLevelParameters[indxLevel].Select(strId_parent);

                foreach (DataRow r in rows)
                {
                    //Строка с идентификатором задачи
                    strId = m_listLevelParameters[indxLevel].GetId(r).Trim();

                    #region код для учета особенности структуры для задачи с ИД = 1 (Расчет ТЭП)
                    if (bTaskTep == true)
                    {
                        iId = int.Parse(strId);
                        if ((!(iId < (int)ID_START_RECORD.ALG))
                            && (iId < (int)ID_START_RECORD.ALG_NORMATIVE))
                            node_par = node_mkt;
                        else
                            if ((!(iId < (int)ID_START_RECORD.ALG_NORMATIVE))
                                && (iId < (int)ID_START_RECORD.PUT))
                                node_par = node_norm;
                            else
                                throw new Exception(@"PanelPrjOutParameters::reAddNodes (ID_NODE=" + iId + @") - неизвестный диапазон");

                        nodes = node_par.Nodes;
                    }
                    else
                        ;
                    #endregion

                    //
                    if (strId.Equals(string.Empty) == false)
                        strKey = concatIdNode(node_par, strId);
                    else
                        strKey = strId_parent;

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
                            node = node_par;
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
                if ((m_Level == ID_LEVEL.TASK) && (idTask == (int)ID_TASK.TEP))
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
        /// <summary>
        /// Возвратить следующий целочисленный идентификатор для добавляемой строки
        ///  в таблице с параметрами в алгоритме расчета
        /// </summary>
        /// <returns>Целочисленный идентификатор</returns>
        protected override int getIdNextAlgoritm()
        {
            int iRes = -1
                , err = -1
                , min = -1, max = -1;
            string strNodeParentName = string.Empty;

            if ((ID_TASK)Int32.Parse(getIdNodePart(m_ctrlTreeView.SelectedNode.Name, ID_LEVEL.TASK)) == ID_TASK.TEP)
            {
                strNodeParentName = getIdNodePart(m_ctrlTreeView.SelectedNode.Name, (int)ID_LEVEL.N_ALG);
                if (strNodeParentName.Equals(@"norm") == true)
                {
                    min = (int)ID_START_RECORD.ALG_NORMATIVE;
                    max = (int)ID_START_RECORD.PUT;
                }
                else
                    if (strNodeParentName.Equals(@"mkt") == true)
                    {
                        min = (int)ID_START_RECORD.ALG;
                        max = (int)ID_START_RECORD.ALG_NORMATIVE;
                    }
                    else
                        throw new Exception(@"PanelPrjOutParameters::getIdNextAlgoritm () - неизвестный тип ");

                iRes = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], out err, @"ID", min - 1, max - 1);
                if (iRes == 0) iRes += (int)ID_START_RECORD.ALG; else ;
            }
            else
                iRes = base.getIdNextAlgoritm ();

            return iRes;
        }

        protected override void addRowToTablePut(int idPut, int idComp)
        {
            m_arTableEdit[(int)INDEX_PARAMETER.PUT].Rows.Add(new object[] {
                idPut
                , m_idAlg //ALG
                //, Convert.ToInt32(getIdNodePart (strIdDetail, ID_LEVEL.TIME)) //TIME
                , idComp //COMP
                , 0 //ID_RATIO
                , @"" //FORMULA                
                , -65384 //MIN
                , 65385 //MAX
            });
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
