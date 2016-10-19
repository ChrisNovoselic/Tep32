using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using System.ComponentModel;
using System.Collections;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;
using System.Xml;
using System.Drawing;


namespace PluginProject
{
    public class PanelPrjRolesFPanels : HPanelEditTree
    {
        enum INDEX_PARSE_UNIT { USER = 0, PANEL = 100, CONTEXT = 200 };
        TreeView_Users tvUsers = new TreeView_Users(false);
        DataGridView_Prop_Text_Check dgvProp = new DataGridView_Prop_Text_Check();
        DataGridView_Prop_Text_Check dgvProp_Context = new DataGridView_Prop_Text_Check();
        DataGridView_Prop_Text_Check dgvProp_Panel = new DataGridView_Prop_Text_Check();
        TreeViewProfile treeProfiles = new TreeViewProfile();
        TableLayoutPanel panel_Prop = new TableLayoutPanel();
        

        #region Переменные

        //DelegateStringFunc delegateErrorReport;
        //DelegateStringFunc delegateWarningReport;
        //DelegateStringFunc delegateActionReport;
        //DelegateBoolFunc delegateReportClear;

        /// <summary>
        /// Перечисление для индексироания уровней "дерева" параметров алгоритма
        /// </summary>
        protected enum ID_LEVEL
        {
            UNKNOWN = -1
                , ROLE /*Роль*/, USER /*Пользователь*/
        };

        /// <summary>
        /// Текущий(выбранный) уровень "дерева"
        /// </summary>
        private ID_LEVEL _Level;
        protected ID_LEVEL m_Level
        {
            get { return _Level; }

            set { _Level = value; }
        }
        bool bIsRole = false;
        /// <summary>
        /// Идентификаторы объектов формы
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE,
            BUTTON_BREAK
                , TREE_DICT_ITEM,
            DGV_DICT_PROP
                ,
            PUNEL_PROP_DESC
                , INDEX_CONTROL_COUNT,
        };

        /// <summary>
        /// Имена кнопок на панели
        /// </summary>
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить" };

        DataTable m_AllUnits, m_context_Unit, m_panel_Unit;

        /// <summary>
        /// Текущий выбранный компонент
        /// </summary>
        int m_sel_comp;

        TreeView_Users.ID_Comp m_list_id;
        DataTable m_table_TEC = new DataTable();
        DataTable[] m_ar_panel_table = new DataTable[3];
        /// <summary>
        /// Массив оригинальных таблиц
        /// </summary>
        DataTable[] m_arr_origTable;

        /// <summary>
        /// Массив редактируемых таблиц
        /// </summary>
        DataTable[] m_arr_editTable;

        /// <summary>
        /// Тип выбраной ноды
        /// </summary>
        TreeView_Users.Type_Comp m_type_sel_node;

        /// <summary>
        /// Идентификаторы типов таблиц
        /// </summary>
        public enum ID_Table : int { Unknown = -1, Role, User, Count }

        /// <summary>
        /// Возвратить наименование компонента 
        /// </summary>
        /// <param name="indx">Индекс </param>
        /// <returns>Строка - наименование</returns>
        protected static string getNameMode(ID_Table id)
        {
            string[] nameModes = { "roles", "users" };

            return nameModes[(int)id];
        }

        Dictionary<string, XmlDocument>[] arrDictXml_Edit, arrDictXml_Orig;
        Dictionary<string, HTepUsers.DictElement>[] arrDictProfiles;

        enum TypeName : int {Panel, Item, Context };

        XmlDocument selectedXml;
        HTepUsers.HTepProfilesXml.ParamComponent selectedComp;
        enum NewNameElement : int {NewPanel=999, NewItem=1999, NewContext=2999 };
        #endregion


        public PanelPrjRolesFPanels(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
            arrDictXml_Edit = new Dictionary<string, XmlDocument>[2];
            arrDictXml_Orig = new Dictionary<string, XmlDocument>[2];
            arrDictProfiles = new Dictionary<string, HTepUsers.DictElement>[2];
            selectedXml = new XmlDocument();
            selectedComp = new HTepUsers.HTepProfilesXml.ParamComponent();
            //m_handlerDb = createHandlerDb();
            //arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.User] = HTepUsers.GetDicpXmlUsers;
            //arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.Role] = HTepUsers.GetDicpXmlRoles;
            //arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.User] = HTepUsers.GetDicpUsersProfile;
            //arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.Role] = HTepUsers.GetDicpRolesProfile;

            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];

            m_context_Unit = new DataTable();
            m_AllUnits = HTepUsers.GetTableProfileUnits.Copy(); ;
            m_context_Unit = m_AllUnits.Clone();
            m_panel_Unit = m_AllUnits.Clone();
            foreach (DataRow r in m_AllUnits.Select("ID>" + (int)INDEX_PARSE_UNIT.PANEL + " AND ID<" + (int)INDEX_PARSE_UNIT.CONTEXT))
            {
                m_panel_Unit.Rows.Add(r.ItemArray);
                m_AllUnits.Rows.Remove(r);
            }
            foreach (DataRow r in m_AllUnits.Select("ID>" + (int)INDEX_PARSE_UNIT.CONTEXT))
            {
                m_context_Unit.Rows.Add(r.ItemArray);
                m_AllUnits.Rows.Remove(r);
            }
            dgvProp.create_dgv(m_AllUnits);
            dgvProp_Context.create_dgv(m_context_Unit);
            dgvProp_Panel.create_dgv(m_panel_Unit);

            tvUsers.EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            treeProfiles.AfterSelect += new TreeViewEventHandler(this.treeProfiles_SelectedNode);

            ((Button)(Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Click += new EventHandler(btnSave_Click);
            ((Button)(Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Click += new EventHandler(btnBreak_Click);
            dgvProp.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_CellEndEdit);
            dgvProp_Context.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_Context_CellEndEdit);
            dgvProp_Panel.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_Panel_CellEndEdit);
            treeProfiles.AfterLabelEdit += new NodeLabelEditEventHandler(treeView_NodeEdit);
            treeProfiles.ClickItem += new TreeViewProfile.ClickItemEventHandler(clickItemContext);
        }

        public override bool Activate(bool active)
        {

            fillDataTable();

            return base.Activate(active);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;
            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_BREAK + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = false;
            //btnOK.Enabled = true;
            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = true;
            //btnBreak.Enabled = true;

            tvUsers.Dock = DockStyle.Fill;
            dgvProp.Dock = DockStyle.Fill;
            dgvProp_Context.Dock = DockStyle.Fill;
            dgvProp_Panel.Dock = DockStyle.Fill;
            //panelProfiles.Dock = DockStyle.Fill;
            treeProfiles.Dock = DockStyle.Fill;
            panel_Prop.Dock = DockStyle.Fill;
            treeProfiles.HideSelection = false;
            treeProfiles.LabelEdit = true;

            this.Controls.Add(tvUsers, 1, 0);
            this.SetColumnSpan(tvUsers, 4); this.SetRowSpan(tvUsers, 13);

            this.panel_Prop.ColumnCount = 1;
            this.panel_Prop.RowCount = 3;

            this.panel_Prop.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.panel_Prop.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.panel_Prop.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));


            this.panel_Prop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));

            this.panel_Prop.Controls.Add(dgvProp, 0, 0);
            this.SetColumnSpan(dgvProp, 1); this.SetRowSpan(dgvProp, 1);
            this.panel_Prop.Controls.Add(dgvProp_Context, 0, 2);
            this.SetColumnSpan(dgvProp_Context, 1); this.SetRowSpan(dgvProp_Context, 1);
            this.panel_Prop.Controls.Add(dgvProp_Panel, 0, 1);
            this.SetColumnSpan(dgvProp_Context, 1); this.SetRowSpan(dgvProp_Context, 1);

            this.Controls.Add(panel_Prop, 9, 0);
            this.SetColumnSpan(panel_Prop, 4); this.SetRowSpan(panel_Prop, 10);

            //this.Controls.Add(panelProfiles, 5, 0);
            //this.SetColumnSpan(panelProfiles, 4); this.SetRowSpan(panelProfiles, 10);

            this.Controls.Add(treeProfiles, 5, 0);
            this.SetColumnSpan(treeProfiles, 4); this.SetRowSpan(treeProfiles, 10);

            addLabelDesc(INDEX_CONTROL.PUNEL_PROP_DESC.ToString());

            this.ResumeLayout();
        }

        #region Наследуемые

        protected override void recUpdateInsertDelete(out int err)
        {
            err = 0;
        }

        protected override void successRecUpdateInsertDelete()
        {

        }

        protected override void initialize(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
        }

        #endregion

        /// <summary>
        /// Получение таблиц
        /// </summary>
        private void fillDataTable()
        {
            int err = -1;

            HTepUsers.HTepProfilesXml.UpdateProfile(m_handlerDb.ConnectionSettings);
            arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.User] = copyXml(HTepUsers.GetDicpXmlUsers);
            arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.Role] = copyXml(HTepUsers.GetDicpXmlRoles);

            arrDictXml_Orig[(int)HTepUsers.HTepProfilesXml.Type.User] = copyXml(HTepUsers.GetDicpXmlUsers);
            arrDictXml_Orig[(int)HTepUsers.HTepProfilesXml.Type.Role] = copyXml(HTepUsers.GetDicpXmlRoles);

            arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.User] = HTepUsers.GetDicpUsersProfile;
            arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.Role] = HTepUsers.GetDicpRolesProfile;
            
            m_handlerDb.RegisterDbConnection(out err);

            DbConnection connConfigDB = m_handlerDb.DbConnection;

            if (m_table_TEC.Columns.Count == 0)
            {
                DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
                m_table_TEC.Columns.AddRange(columns);
            }

            m_table_TEC.Rows.Clear();

            HTepUsers.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";

            HTepUsers.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";



            string query = "Select * from dbo.task";

            m_ar_panel_table[0] = m_handlerDb.Select(query, out err);

            query = "Select * from dbo.plugins";
            m_ar_panel_table[1] = m_handlerDb.Select(query, out err);

            query = "Select * from dbo.fpanels";
            m_ar_panel_table[2] = m_handlerDb.Select(query, out err);

            m_handlerDb.UnRegisterDbConnection();

            resetDataTable();

            tvUsers.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
        }

        private Dictionary<string, XmlDocument> copyXml(Dictionary<string, XmlDocument> dictXml)
        {
            Dictionary<string, XmlDocument> newXml = new Dictionary<string, XmlDocument>();

            foreach(string key in dictXml.Keys)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(dictXml[key].InnerXml);
                newXml.Add(key, doc);
            }

            return newXml;
        }

        /// <summary>
        /// Сброс таблиц
        /// </summary>
        private void resetDataTable()
        {
            for (ID_Table i = ID_Table.Unknown + 1; i < ID_Table.Count; i++)
                m_arr_editTable[(int)i] = m_arr_origTable[(int)i].Copy();
        }

        /// <summary>
        /// Обработчик получения данных от TreeView
        /// </summary>
        private void get_operation_tree(object sender, TreeView_Users.EditNodeEventArgs e)
        {
            if (e.Operation == TreeView_Users.ID_Operation.Select)
            {
                select(e.PathComp, e.IdComp);
            }
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void select(TreeView_Users.ID_Comp list_id, int IdComp)
        {
            dgvProp.ClearCells();
            dgvProp_Context.ClearCells();
            dgvProp_Panel.ClearCells();
            DataTable[] massTable = new DataTable[1];
            DataTable[] tables = new DataTable[3];
            bIsRole = false;
            m_sel_comp = IdComp;
            m_list_id = list_id;
            //massTable[0] = getProfileTable(m_arr_editTable[(int)ID_Table.Profiles], list_id.id_role, list_id.id_user, bIsRole);
            //dgvProp.Update_dgv(IdComp, massTable);

            dgvProp_Context.ClearCells();
            dgvProp_Panel.ClearCells();
            dgvProp_Context.ClearSelection();
            dgvProp_Panel.ClearSelection();
            dgvProp_Context.Enabled = false;
            dgvProp_Panel.Enabled = false;

            if (list_id.id_user.Equals(-1) == false)
            {
                treeProfiles.Nodes.Clear();
                bIsRole = false;
                m_type_sel_node = TreeView_Users.Type_Comp.User;
                selectedXml = arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.User][list_id.id_user.ToString()];
                fillTreeProfiles(selectedXml);
                //panelProfiles.FillControls(m_ar_panel_table, m_arr_editTable[(int)ID_Table.Profiles], IdComp, false);
                dgvProp.Update_dgv(arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.User][list_id.id_user.ToString()].Attributes);
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                treeProfiles.Nodes.Clear();
                bIsRole = true;
                m_type_sel_node = TreeView_Users.Type_Comp.Role;
                selectedXml = arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.Role][list_id.id_role.ToString()];
                fillTreeProfiles(selectedXml);
                //panelProfiles.FillControls(m_ar_panel_table, m_arr_editTable[(int)ID_Table.Profiles], IdComp, true);
                dgvProp.Update_dgv(arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.Role][list_id.id_role.ToString()].Attributes);

            }
        }

        private void treeProfiles_SelectedNode(object sender, TreeViewEventArgs e)
        {
            selectedComp = new HTepUsers.HTepProfilesXml.ParamComponent();
            string tag = e.Node.Tag.ToString();
            string[] tags = tag.Split(',');
            Dictionary<string, HTepUsers.DictElement> dict = new Dictionary<string, HTepUsers.DictElement>();
            if (tags.Length > 1)
            {
                if (m_list_id.id_user.Equals(-1) == false)//User
                {
                    dict = arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.User][m_list_id.id_user.ToString()].Objects;
                }

                if (m_list_id.id_user.Equals(-1) == true & m_list_id.id_role.Equals(-1) == false)//Role
                {
                    dict = arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.Role][m_list_id.id_role.ToString()].Objects;
                }
                switch ((TypeName)Int16.Parse(tags[0]))
                {
                    case TypeName.Context:
                        dgvProp_Context.ClearCells();
                        dgvProp_Panel.ClearCells();
                        dgvProp_Context.ClearSelection();
                        dgvProp_Context.Enabled = true;
                        dgvProp_Panel.Enabled = true;

                        selectedComp.ID_Panel = Int32.Parse(e.Node.Parent.Parent.Tag.ToString().Split(',')[1]);
                        selectedComp.ID_Item = Int32.Parse(e.Node.Parent.Tag.ToString().Split(',')[1]);
                        selectedComp.Context = tags[1];

                        if (dict.ContainsKey(selectedComp.ID_Panel.ToString()) == true)
                        {
                            dgvProp_Panel.Update_dgv(dict[selectedComp.ID_Panel.ToString()].Attributes);

                            if (dict[selectedComp.ID_Panel.ToString()].Objects.ContainsKey(selectedComp.ID_Item.ToString()) == true)
                            {
                                if (dict[selectedComp.ID_Panel.ToString()].Objects[selectedComp.ID_Item.ToString()].Objects.ContainsKey(selectedComp.Context.ToString()) == true)
                                {
                                    dgvProp_Context.Update_dgv(dict[selectedComp.ID_Panel.ToString()].Objects[selectedComp.ID_Item.ToString()].Objects[selectedComp.Context.ToString()].Attributes);
                                }
                            }
                        }
                        break;

                    case TypeName.Item:
                        dgvProp_Context.ClearCells();
                        dgvProp_Panel.ClearCells();
                        dgvProp_Context.ClearSelection();
                        dgvProp_Context.Enabled = false;
                        dgvProp_Panel.Enabled = true;

                        selectedComp.ID_Panel = Int32.Parse(e.Node.Parent.Tag.ToString().Split(',')[1]);

                        if (dict.ContainsKey(selectedComp.ID_Panel.ToString()) == true)
                        {
                            dgvProp_Panel.Update_dgv(dict[selectedComp.ID_Panel.ToString()].Attributes);
                        }
                            break;
                        
                    case TypeName.Panel:
                        dgvProp_Context.ClearCells();
                        dgvProp_Panel.ClearCells();
                        dgvProp_Context.ClearSelection();
                        dgvProp_Panel.ClearSelection();
                        dgvProp_Context.Enabled = false;
                        dgvProp_Panel.Enabled = true;

                        selectedComp.ID_Panel = Int32.Parse(tags[1]);
                        if(dict.ContainsKey(selectedComp.ID_Panel.ToString()) ==true)
                            dgvProp_Panel.Update_dgv(dict[selectedComp.ID_Panel.ToString()].Attributes);
                        break;
                }
            }
        }

        private void fillTreeProfiles(XmlDocument xmlProfile)
        {
            XmlNode node = xmlProfile.ChildNodes[1].ChildNodes[0];
            int i = 0;
            int level = 0;
            foreach(XmlNode nodeChild in node.ChildNodes)
            {
                treeProfiles.Nodes.Add(new TreeNode(((TypeName)level).ToString()+' '+nodeChild.Name.Remove(0, 1)));
                TreeNode tNode;
                tNode = treeProfiles.Nodes[i];
                tNode.Tag = ((int)((TypeName)level)).ToString() + ',' + nodeChild.Name.Remove(0, 1);
                AddNodeToTree(nodeChild, tNode, level);
                i++;
            }


        }

        private void AddNodeToTree(XmlNode inXmlNode, TreeNode inTreeNode, int level)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            level++;
            int i = 0;
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    inTreeNode.Nodes.Add(new TreeNode(((TypeName)level).ToString() + ' ' + xNode.Name.Remove(0,1)));
                    tNode = inTreeNode.Nodes[i];
                    tNode.Tag = ((int)((TypeName)level)).ToString() + ',' + xNode.Name.Remove(0, 1);
                    AddNodeToTree(xNode, tNode, level);
                }
            }
            else
            {
                inTreeNode.Text = ((TypeName)(level-1)).ToString() + ' ' + inXmlNode.LocalName.ToString().Remove(0, 1);
            }
        }

        private void treeView_NodeEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                HTepUsers.HTepProfilesXml.ParamComponent comp = new HTepUsers.HTepProfilesXml.ParamComponent();
                Dictionary<string, HTepUsers.DictElement> dict = new Dictionary<string, HTepUsers.DictElement>();
                //HTepUsers.DictElement element = new HTepUsers.DictElement();
                XmlDocument xml = new XmlDocument();
                int id = -1;
                HTepUsers.HTepProfilesXml.Type type = new HTepUsers.HTepProfilesXml.Type();
                string tag = e.Node.Tag.ToString();
                string[] tags = tag.Split(',');

                if (m_list_id.id_user.Equals(-1) == false)//User
                {
                    type = HTepUsers.HTepProfilesXml.Type.User;
                    dict = arrDictProfiles[(int)type][m_list_id.id_user.ToString()].Objects;
                    xml = arrDictXml_Edit[(int)type][m_list_id.id_user.ToString()];
                    id = m_list_id.id_user;
                    
                }

                if (m_list_id.id_user.Equals(-1) == true & m_list_id.id_role.Equals(-1) == false)//Role
                {
                    type = HTepUsers.HTepProfilesXml.Type.Role;
                    dict = arrDictProfiles[(int)type][m_list_id.id_role.ToString()].Objects;
                    xml = arrDictXml_Edit[(int)type][m_list_id.id_role.ToString()];
                    id = m_list_id.id_role;
                }

                if (tags.Length > 1)
                {
                    switch ((TypeName)short.Parse(tags[0]))
                    {
                        case TypeName.Context:
                            comp.ID_Item = int.Parse(e.Node.Parent.Text.Split(' ')[1]);
                            comp.ID_Panel = int.Parse(e.Node.Parent.Parent.Text.Split(' ')[1]);
                            if (e.Node.Nodes.Find("Context " + e.Label.Split(' ')[1], false).Length == 0)
                            {
                                #region XML
                                XmlNode newNode = xml.CreateElement("_" + e.Label.Split(' ')[1]);

                                foreach (XmlAttribute attr in xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + comp.ID_Item.ToString()]["_" + e.Node.Text.Split(' ')[1]].Attributes)
                                {
                                    newNode.Attributes.Append(attr);
                                }

                                foreach (XmlNode node in xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + comp.ID_Item.ToString()]["_" + e.Node.Text.Split(' ')[1]])
                                {
                                    newNode.AppendChild(node.Clone());
                                }

                                xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + comp.ID_Item.ToString()].ReplaceChild(newNode, xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + comp.ID_Item.ToString()]["_" + e.Node.Text.Split(' ')[1]]);
                                #endregion

                                #region DICT

                                dict[comp.ID_Panel.ToString()].Objects[comp.ID_Item.ToString()].Objects.Add(e.Label.Split(' ')[1], dict[comp.ID_Panel.ToString()].Objects[comp.ID_Item.ToString()].Objects[e.Node.Text.Split(' ')[1]]);
                                dict[comp.ID_Panel.ToString()].Objects[comp.ID_Item.ToString()].Objects.Remove(e.Node.Text.Split(' ')[1]);

                                #endregion

                                e.Node.Tag = (tag.Split(',')[0] + ',' + e.Label.Split(' ')[1]);

                                activate_btn(true);
                            }
                            else
                            {
                                Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                            break;

                        case TypeName.Item:
                            comp.ID_Panel = int.Parse(e.Node.Parent.Text.Split(' ')[1]);
                            if (e.Node.Nodes.Find("Item " + e.Label.Split(' ')[1], false).Length == 0)
                            {
                                #region XML
                                XmlNode newNode = xml.CreateElement("_" + e.Label.Split(' ')[1]);

                                foreach (XmlAttribute attr in xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + e.Node.Text.Split(' ')[1]].Attributes)
                                {
                                    newNode.Attributes.Append(attr);
                                }

                                foreach (XmlNode node in xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + e.Node.Text.Split(' ')[1]])
                                {
                                    newNode.AppendChild(node.Clone());
                                }

                                xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()].ReplaceChild(newNode, xml.ChildNodes[1].ChildNodes[0]["_" + comp.ID_Panel.ToString()]["_" + e.Node.Text.Split(' ')[1]]);
                                #endregion

                                #region DICT

                                dict[comp.ID_Panel.ToString()].Objects.Add(e.Label.Split(' ')[1], dict[comp.ID_Panel.ToString()].Objects[e.Node.Text.Split(' ')[1]]);
                                dict[comp.ID_Panel.ToString()].Objects.Remove(e.Node.Text.Split(' ')[1]);

                                #endregion

                                e.Node.Tag = (tag.Split(',')[0] + ',' + e.Label.Split(' ')[1]);

                                activate_btn(true);
                            }
                            else
                            {
                                Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                            break;

                        case TypeName.Panel:
                            if (e.Node.Nodes.Find("Panel " + e.Label.Split(' ')[1], false).Length == 0)
                            {
                                #region XML
                                XmlNode newNode = xml.CreateElement("_"+ e.Label.Split(' ')[1]);

                                foreach (XmlAttribute attr in xml.ChildNodes[1].ChildNodes[0]["_" + e.Node.Text.Split(' ')[1]].Attributes)
                                {
                                    newNode.Attributes.Append(attr);
                                }

                                foreach (XmlNode node in xml.ChildNodes[1].ChildNodes[0]["_" + e.Node.Text.Split(' ')[1]])
                                {
                                    newNode.AppendChild(node.Clone());
                                }

                                xml.ChildNodes[1].ChildNodes[0].ReplaceChild(newNode, xml.ChildNodes[1].ChildNodes[0]["_" + e.Node.Text.Split(' ')[1]]);
                                #endregion

                                #region DICT

                                dict.Add(e.Label.Split(' ')[1],dict[e.Node.Text.Split(' ')[1]]);
                                dict.Remove(e.Node.Text.Split(' ')[1]);

                                #endregion

                                e.Node.Tag = (tag.Split(',')[0] + ',' + e.Label.Split(' ')[1]);

                                activate_btn(true);
                            }
                            else
                            {
                                Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                            break;
                    }
                }
            }
        }

        private void clickItemContext(object sender, TreeViewProfile.ClickItemEventArgs e)
        {
            HTepUsers.HTepProfilesXml.ParamComponent comp = new HTepUsers.HTepProfilesXml.ParamComponent();
            string tag = string.Empty;// e.Node.Tag.ToString();
            string[] tags = tag.Split(',');
            XmlDocument xmlDoc = new XmlDocument();
            int id = -1;
            HTepUsers.HTepProfilesXml.Type type = new HTepUsers.HTepProfilesXml.Type();
            Dictionary<string, HTepUsers.DictElement> dict = new Dictionary<string, HTepUsers.DictElement>();
            
            if (m_list_id.id_user.Equals(-1) == false)//User
            {
                dict = arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.User][m_list_id.id_user.ToString()].Objects;
                xmlDoc = arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.User][m_list_id.id_user.ToString()];
                id = m_list_id.id_user;
                type = HTepUsers.HTepProfilesXml.Type.User;
            }
            else
            {
                if (m_list_id.id_user.Equals(-1) == true & m_list_id.id_role.Equals(-1) == false)//Role
                {
                    dict = arrDictProfiles[(int)HTepUsers.HTepProfilesXml.Type.Role][m_list_id.id_role.ToString()].Objects;
                    xmlDoc = arrDictXml_Edit[(int)HTepUsers.HTepProfilesXml.Type.Role][m_list_id.id_role.ToString()];
                    id = m_list_id.id_role;
                    type = HTepUsers.HTepProfilesXml.Type.Role;
                }
            }

            if (e.Node != null)
            {
                tag = e.Node.Tag.ToString();
                tags = tag.Split(',');

                switch (e.TypeButt)
                {
                    #region Add in element
                    case TreeViewProfile.TypeButton.Add:

                        switch ((TypeName)short.Parse(tags[0]))
                        {
                            case TypeName.Item:
                                comp.ID_Panel = int.Parse(e.Node.Parent.Tag.ToString().Split(',')[1]);
                                comp.ID_Item = int.Parse(e.Node.Tag.ToString().Split(',')[1]);
                                comp.Context = ((int)NewNameElement.NewContext).ToString();
                                if (e.Node.Nodes.Find("Context " + ((int)NewNameElement.NewContext).ToString(), false).Length == 0)
                                {
                                    xmlDoc = HTepUsers.HTepProfilesXml.AddElement(xmlDoc, id, type, HTepUsers.HTepProfilesXml.Component.Context, comp);
                                    e.Node.Nodes.Add("Context " + ((int)NewNameElement.NewContext).ToString(), "Context " + ((int)NewNameElement.NewContext).ToString());
                                    e.Node.Nodes["Context " + ((int)NewNameElement.NewContext).ToString()].Tag = "2," + ((int)NewNameElement.NewContext).ToString();

                                    HTepUsers.DictElement dictEl = new HTepUsers.DictElement();
                                    dictEl.Attributes = new Dictionary<string, string>();
                                    dictEl.Objects = new Dictionary<string, HTepUsers.DictElement>();
                                    dict[comp.ID_Panel.ToString()].Objects[comp.ID_Item.ToString()].Objects.Add(((int)NewNameElement.NewContext).ToString(), dictEl);
                                    activate_btn(true);

                                    activate_btn(true);
                                }
                                else
                                {
                                    Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                                }
                                break;

                            case TypeName.Panel:
                                comp.ID_Panel = int.Parse(tags[1]);
                                comp.ID_Item = (int)NewNameElement.NewItem;
                                if (e.Node.Nodes.Find("Item " + ((int)NewNameElement.NewItem).ToString(), false).Length == 0)
                                {
                                    xmlDoc = HTepUsers.HTepProfilesXml.AddElement(xmlDoc, id, type, HTepUsers.HTepProfilesXml.Component.Item, comp);
                                    e.Node.Nodes.Add("Item " + ((int)NewNameElement.NewItem).ToString(), "Item " + ((int)NewNameElement.NewItem).ToString());
                                    e.Node.Nodes["Item " + ((int)NewNameElement.NewItem).ToString()].Tag = "1," + ((int)NewNameElement.NewItem).ToString();

                                    HTepUsers.DictElement dictEl = new HTepUsers.DictElement();
                                    dictEl.Attributes = new Dictionary<string, string>();
                                    dictEl.Objects = new Dictionary<string, HTepUsers.DictElement>();
                                    dict[comp.ID_Panel.ToString()].Objects.Add(((int)NewNameElement.NewItem).ToString(), dictEl);
                                    activate_btn(true);
                                }
                                else
                                {
                                    Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                                }
                                break;
                        }
                        break;
                    #endregion
                    case TreeViewProfile.TypeButton.Delete:

                        switch ((TypeName)short.Parse(tags[0]))
                        {
                            case TypeName.Context:
                                comp.ID_Panel = int.Parse(e.Node.Parent.Parent.Tag.ToString().Split(',')[1]);
                                comp.ID_Item = int.Parse(e.Node.Parent.Tag.ToString().Split(',')[1]);
                                comp.Context = tags[1];
                                xmlDoc = HTepUsers.HTepProfilesXml.DelElement(xmlDoc,id, type, HTepUsers.HTepProfilesXml.Component.Context, comp);
                                dict[comp.ID_Panel.ToString()].Objects[comp.ID_Item.ToString()].Objects.Remove(comp.Context.ToString());
                                e.Node.Remove();

                                activate_btn(true);
                                break;

                            case TypeName.Item:
                                comp.ID_Panel = int.Parse(e.Node.Parent.Tag.ToString().Split(',')[1]);
                                comp.ID_Item = int.Parse(e.Node.Tag.ToString().Split(',')[1]);
                                xmlDoc = HTepUsers.HTepProfilesXml.DelElement(xmlDoc, id, type, HTepUsers.HTepProfilesXml.Component.Item, comp);
                                dict[comp.ID_Panel.ToString()].Objects.Remove(comp.ID_Item.ToString());
                                e.Node.Remove();

                                activate_btn(true);
                                break;

                            case TypeName.Panel:
                                comp.ID_Panel = int.Parse(tags[1]);
                                xmlDoc = HTepUsers.HTepProfilesXml.DelElement(xmlDoc, id, type, HTepUsers.HTepProfilesXml.Component.Panel, comp);
                                dict.Remove(comp.ID_Panel.ToString());
                                e.Node.Remove();

                                activate_btn(true);
                                break;
                        }
                        break;
                }
            }
            else
            {
                #region Add panel
                switch (e.TypeButt)
                {
                    case TreeViewProfile.TypeButton.Add:
                        comp.ID_Panel = (int)NewNameElement.NewPanel;
                        if (treeProfiles.Nodes.Find("Panel " + ((int)NewNameElement.NewPanel).ToString(), false).Length == 0)
                        {
                            xmlDoc =HTepUsers.HTepProfilesXml.AddElement(xmlDoc, id, type, HTepUsers.HTepProfilesXml.Component.Panel, comp);
                            treeProfiles.Nodes.Add("Panel " + ((int)NewNameElement.NewPanel).ToString(), "Panel " + ((int)NewNameElement.NewPanel).ToString());
                            treeProfiles.Nodes["Panel " + ((int)NewNameElement.NewPanel).ToString()].Tag = "0," + ((int)NewNameElement.NewPanel).ToString();
                            HTepUsers.DictElement dictEl = new HTepUsers.DictElement();
                            dictEl.Attributes = new Dictionary<string, string>();
                            dictEl.Objects = new Dictionary<string, HTepUsers.DictElement>();
                            dict.Add(((int)NewNameElement.NewPanel).ToString(), dictEl);
                            activate_btn(true);
                        }
                        else
                        {
                            Logging.Logg().Action("PanelPrjRolesFPanels:clickItemContext - Элемент с таким именем уже существует", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        break;
                }
                #endregion
            }
        }

        protected void activate_btn(bool active)
        {
            this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0].Enabled = active;
            //this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0].Enabled = active;
        }

        protected void btnBreak_Click(object sender, EventArgs e)
        {
            fillDataTable();
            activate_btn(false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int err = -1;
            string warning;
            string keys = string.Empty;

            HTepUsers.HTepProfilesXml.SaveXml(m_handlerDb.ConnectionSettings,arrDictXml_Orig,arrDictXml_Edit);

            activate_btn(false);
            fillDataTable();
            resetDataTable();

            //if (validate_saving(m_arr_editTable[(int)ID_Table.Profiles], out warning) == false)
            //{
            //    m_handlerDb.RegisterDbConnection(out err);
            //    m_handlerDb.RecUpdateInsertDelete(getNameMode(ID_Table.Profiles), "ID_EXT,IS_ROLE,ID_TAB,ID_ITEM,CONTEXT,ID_UNIT", string.Empty, m_arr_origTable[(int)ID_Table.Profiles], m_arr_editTable[(int)ID_Table.Profiles], out err);
            //    m_handlerDb.UnRegisterDbConnection();
            //    fillDataTable();
            //    resetDataTable();

            //    //((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

            //    activate_btn(false);
            //}
            //else
            //{
            //    //delegateWarningReport(warning[(int)ID_Table.Role] + warning[(int)ID_Table.User]);
            //    //MessageBox.Show(warning[0] + warning[1] + warning[2] + warning[3], "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }



        protected void dgvProp_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            HTepUsers.HTepProfilesXml.ParamComponent parComp = new HTepUsers.HTepProfilesXml.ParamComponent();
            string value = string.Empty;
            int id = -1;
            HTepUsers.HTepProfilesXml.Type type = HTepUsers.HTepProfilesXml.Type.Role;

            if (e.m_Value == "True")
                value = 1.ToString();
            else
                if (e.m_Value == "False")
                    value = 0.ToString();
                else
                    value = e.m_Value;

            switch (m_type_sel_node)
            {
                case TreeView_Users.Type_Comp.Role:
                    id = m_list_id.id_role;
                    type = HTepUsers.HTepProfilesXml.Type.Role;
                    break;
                case TreeView_Users.Type_Comp.User:
                    id = m_list_id.id_user;
                    type = HTepUsers.HTepProfilesXml.Type.User;
                    break;
            }

            parComp.Value = value;
            parComp.ID_Unit = Int32.Parse(e.m_Header_name);

            selectedXml = HTepUsers.HTepProfilesXml.EditAttr(selectedXml, id, type, HTepUsers.HTepProfilesXml.Component.None, parComp);

            
            activate_btn(true);

        }

        protected void dgvProp_Context_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            HTepUsers.HTepProfilesXml.ParamComponent parComp = new HTepUsers.HTepProfilesXml.ParamComponent();
            string value = string.Empty;
            int id = -1;
            HTepUsers.HTepProfilesXml.Type type = HTepUsers.HTepProfilesXml.Type.Role;

            if (e.m_Value == "True")
                value = 1.ToString();
            else
                if (e.m_Value == "False")
                value = 0.ToString();
            else
                value = e.m_Value;

            switch (m_type_sel_node)
            {
                case TreeView_Users.Type_Comp.Role:
                    id = m_list_id.id_role;
                    type = HTepUsers.HTepProfilesXml.Type.Role;
                    break;
                case TreeView_Users.Type_Comp.User:
                    id = m_list_id.id_user;
                    type = HTepUsers.HTepProfilesXml.Type.User;
                    break;
            }

            parComp.ID_Panel = selectedComp.ID_Panel;
            parComp.ID_Item = selectedComp.ID_Item;
            parComp.Context= selectedComp.Context;
            parComp.Value = value;
            parComp.ID_Unit = Int32.Parse(e.m_Header_name);

            selectedXml = HTepUsers.HTepProfilesXml.EditAttr(selectedXml, id, type, HTepUsers.HTepProfilesXml.Component.Context, parComp);


            activate_btn(true);

        }

        protected void dgvProp_Panel_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            HTepUsers.HTepProfilesXml.ParamComponent parComp = new HTepUsers.HTepProfilesXml.ParamComponent();
            string value = string.Empty;
            int id = -1;
            HTepUsers.HTepProfilesXml.Type type = HTepUsers.HTepProfilesXml.Type.Role;

            if (e.m_Value == "True")
                value = 1.ToString();
            else
                if (e.m_Value == "False")
                value = 0.ToString();
            else
                value = e.m_Value;

            switch (m_type_sel_node)
            {
                case TreeView_Users.Type_Comp.Role:
                    id = m_list_id.id_role;
                    type = HTepUsers.HTepProfilesXml.Type.Role;
                    break;
                case TreeView_Users.Type_Comp.User:
                    id = m_list_id.id_user;
                    type = HTepUsers.HTepProfilesXml.Type.User;
                    break;
            }

            parComp.ID_Panel = selectedComp.ID_Panel;
            parComp.Value = value;
            parComp.ID_Unit = Int32.Parse(e.m_Header_name);

            selectedXml = HTepUsers.HTepProfilesXml.EditAttr(selectedXml, id, type, HTepUsers.HTepProfilesXml.Component.Panel, parComp);


            activate_btn(true);
        }


        /// <summary>
        /// Проверка критичных параметров перед сохранением
        /// </summary>
        /// <param name="mass_table">Таблица для проверки</param>
        /// <param name="warning">Строка с описанием ошибки</param>
        /// <returns>Возвращает переменную показывающую наличие не введенных параметров</returns>
        private bool validate_saving(DataTable table_profiles, out string warning)
        {
            bool have = false;
            warning = string.Empty;
            foreach (DataRow row in table_profiles.Rows)
            {
                for (int i = 0; i < table_profiles.Columns.Count; i++)
                {
                    if (i != 3)
                        if (Convert.ToString(row[i]) == "-1")
                        {
                            have = true;
                            warning += "Для пользователя " + row["ID_EXT"] + " параметр " + table_profiles.Columns[i].ColumnName + " равен '-1'." + '\n';
                        }
                }
            }
            return have;
        }


        public class TreeViewProfile : TreeView
        {
            public ContextMenuStrip m_contextMenuNode, m_contextMenuTree, m_contextMenuContext;
            public enum TypeButton : int { Add, Delete };
            string[] m_arrNameButton = { "Добавить элемент", "Удалить элемент" };

            public TreeViewProfile() : base()
            {
                m_contextMenuNode = new ContextMenuStrip();
                m_contextMenuTree = new ContextMenuStrip();
                m_contextMenuContext = new ContextMenuStrip();

                m_contextMenuNode.Items.Add(m_arrNameButton[(int)TypeButton.Add]);
                m_contextMenuNode.Items.Add(m_arrNameButton[(int)TypeButton.Delete]);
                m_contextMenuTree.Items.Add(m_arrNameButton[(int)TypeButton.Add]);
                m_contextMenuContext.Items.Add(m_arrNameButton[(int)TypeButton.Delete]);

                this.AfterSelect += new TreeViewEventHandler(selectNode);
                this.NodeMouseClick += new TreeNodeMouseClickEventHandler(nodeClick);
                this.ContextMenuStrip = m_contextMenuTree;

                m_contextMenuNode.ItemClicked += new ToolStripItemClickedEventHandler(contextItemClick);
                m_contextMenuTree.ItemClicked += new ToolStripItemClickedEventHandler(contextItemClick);
                m_contextMenuContext.ItemClicked += new ToolStripItemClickedEventHandler(contextItemClick);
            }

            private void selectNode(object sender, TreeViewEventArgs e)
            {
                if (e.Node.ContextMenuStrip == null)
                    if (e.Node.Tag.ToString().Split(',')[0] != ((int)TypeName.Context).ToString() )
                        e.Node.ContextMenuStrip = m_contextMenuNode;
                    else
                        e.Node.ContextMenuStrip = m_contextMenuContext;
            }

            /// <summary>
            /// Обработчик события нажатия на элемент в TreeView
            /// </summary>
            private void nodeClick(object sender, TreeNodeMouseClickEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши
                }
            }

            private void contextItemClick(object sender, ToolStripItemClickedEventArgs e)
            {
                if (((ContextMenuStrip)sender).Items.Count == 1)
                {
                    if (e.ClickedItem.Text == m_arrNameButton[(int)TypeButton.Add])
                    {
                        ClickItem?.Invoke(this, new ClickItemEventArgs(TypeButton.Add, null));
                    }
                    else
                    {
                        ClickItem?.Invoke(this, new ClickItemEventArgs(TypeButton.Delete, this.SelectedNode));
                    }
                }
                else
                {
                    if (e.ClickedItem.Text == m_arrNameButton[(int)TypeButton.Add])
                    {
                        ClickItem?.Invoke(this, new ClickItemEventArgs(TypeButton.Add, this.SelectedNode));
                    }
                    else
                    {
                        if (e.ClickedItem.Text == m_arrNameButton[(int)TypeButton.Delete])
                        {
                            ClickItem?.Invoke(this, new ClickItemEventArgs(TypeButton.Delete, this.SelectedNode));
                        }
                    }
                }
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения компонента
            /// </summary>
            public class ClickItemEventArgs : EventArgs
            {
                /// <summary>
                /// Тип действия
                /// </summary>
                public TypeButton TypeButt;

                /// <summary>
                /// Выбранная нода
                /// </summary>
                public TreeNode Node;

                public ClickItemEventArgs(TypeButton typeButt, TreeNode node)
                {
                    TypeButt = typeButt;
                    Node = node;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события - изменение компонента
            /// </summary>
            public delegate void ClickItemEventHandler(object obj, ClickItemEventArgs e);

            /// <summary>
            /// Событие - редактирование компонента
            /// </summary>
            public event ClickItemEventHandler ClickItem;
        }

    }

    public class DataGridView_Prop_Text_Check : DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }

        public DataGridView_Prop_Text_Check()
            : base()
        {
            this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            //this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
            //this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>
        /// Запрос на получение таблицы со свойствами и ComboBox
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public void create_dgv(DataTable tables)
        {
            this.CellValueChanged -= cell_EndEdit;
            this.Rows.Clear();

            foreach (DataRow r in tables.Rows)
            {
                DataGridViewRow row = new DataGridViewRow();

                if (r["ID_UNIT"].ToString().Trim() == "8")
                {
                    DataGridViewCheckBoxCell check = new DataGridViewCheckBoxCell();
                    row.Cells.Add(check);
                    check.Value = false;
                    this.Rows.Add(row);
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = r["DESCRIPTION"].ToString().Trim();
                    
                }
                else
                {
                    this.Rows.Add();
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = r["DESCRIPTION"].ToString().Trim();
                    this.Rows[this.Rows.Count - 1].Cells[0].Value = "";
                }
                this.Rows[this.Rows.Count - 1].Tag = r["ID"].ToString().Trim();
            }
            this.CellValueChanged += cell_EndEdit;
        }

        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            this.CellValueChanged -= cell_EndEdit;
            DataTable inputTable = tables[0];

            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Cells[0] is DataGridViewCheckBoxCell)
                {
                    if (Convert.ToInt32(inputTable.Rows[i]["VALUE"]) == 0)
                        this.Rows[i].Cells[0].Value = false;
                    else
                        this.Rows[i].Cells[0].Value = true;
                }
                else
                {
                    if (inputTable.Rows.Count <= i)
                    {
                        this.Rows[i].Cells[0].Value = string.Empty;
                    }
                    else
                    {
                        this.Rows[i].Cells[0].Value = inputTable.Rows[i]["VALUE"];
                    }
                }
            }
            this.CellValueChanged += cell_EndEdit;
        }

        public void Update_dgv(Dictionary<string, string> dict)
        {
            this.CellValueChanged -= cell_EndEdit;
            
            foreach(DataGridViewRow row in this.Rows)
            {
                if (row.Cells[0] is DataGridViewCheckBoxCell)
                {
                    if (dict.ContainsKey(row.Tag.ToString()) == true)
                    {
                        if (Convert.ToInt32(dict[row.Tag.ToString()]) == 0)
                            row.Cells[0].Value = false;
                        else
                            row.Cells[0].Value = true;
                    }
                    else
                        row.Cells[0].Value = false;
                }
                else
                {
                    if (dict.ContainsKey(row.Tag.ToString()) == true)
                    {
                        row.Cells[0].Value = dict[row.Tag.ToString()];
                    }
                }
            }

            this.CellValueChanged += cell_EndEdit;
        }

        public void ClearCells()
        {
            this.CellValueChanged -= cell_EndEdit;

            foreach (DataGridViewRow row in this.Rows)
            {
                if (row.Cells[0] is DataGridViewCheckBoxCell)

                    row.Cells[0].Value = false;
                else
                    row.Cells[0].Value = string.Empty;
            }

            this.CellValueChanged += cell_EndEdit;
        }

        protected override void cell_EndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int n_row = -1;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].HeaderCell.Value.ToString() == "ID")
                {
                    n_row = Convert.ToInt32(this.Rows[i].Cells[0].Value);
                }
            }
            if (Rows[e.RowIndex].Cells[0].Value != null)
            {
                if (EventCellValueChanged != null)
                    EventCellValueChanged(this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs(n_row//Идентификатор компонента
                                        , Rows[e.RowIndex].Tag.ToString() //Идентификатор компонента
                                        , Rows[e.RowIndex].Cells[0].Value.ToString() //Идентификатор параметра с учетом периода расчета
                                        ));
            }
            else
            {
                if (EventCellValueChanged != null)
                    EventCellValueChanged(this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs(n_row//Идентификатор компонента
                                        , Rows[e.RowIndex].Tag.ToString() //Идентификатор компонента
                                        , null //Идентификатор параметра с учетом периода расчета
                                        ));
            }
        }
    }

    public class User
    {
        public static string m_nameTableProfilesData = @"profiles";

        /// <summary>
        /// Таблица со значениями параметров для пользователя и роли
        /// </summary>
        static DataTable m_tblValues = new DataTable();
        static DataTable m_allProfile;

        /// <summary>
        /// Структура со значением и типом значения
        /// </summary>
        public struct UNIT_VALUE
        {
            public object m_value;
            public int m_idType;
        }

        protected class Profile
        {
            DbConnection m_dbConn;
            bool m_bIsRole;
            int m_id_role
                , m_id_user;

            public Profile(DbConnection dbConn, int id_role, int id_user, bool bIsRole, DataTable allProfiles)
            {
                m_dbConn = dbConn;
                m_bIsRole = bIsRole;
                m_id_role = id_role;
                m_id_user = id_user;
                Update(true, allProfiles);
            }

            /// <summary>
            /// Метод для получения словаря с параметрами Profil'а для пользователя
            /// </summary>
            /// <param name="id_ext">ИД пользователя</param>
            /// <param name="bIsRole">Флаг для определения роли</param>
            /// <returns>Словарь с параметрами</returns>
            public Dictionary<int, UNIT_VALUE> GetProfileItem
            {
                get
                {
                    int id_unit = -1;
                    DataRow[] unitRows = new DataRow[1]; ;

                    Dictionary<int, UNIT_VALUE> dictRes = new Dictionary<int, UNIT_VALUE>();

                    foreach (DataRow r in HTepUsers.GetTableProfileUnits.Rows)
                    {
                        id_unit = (int)r[@"ID"];

                        if (id_unit < 4)
                        {
                            unitRows[0] = GetRowAllowed(id_unit);

                            if (unitRows.Length == 1)
                            {
                                dictRes.Add(id_unit, new UNIT_VALUE() { m_value = unitRows[0][@"VALUE"].ToString().Trim(), m_idType = Convert.ToInt32(unitRows[0][@"ID_UNIT"]) });
                            }
                            else
                                Logging.Logg().Warning(@"", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }

                    return dictRes;
                }
            }

            /// <summary>
            /// Обновление таблиц
            /// </summary>
            /// <param name="id_role">ИД роли</param>
            /// <param name="id_user">ИД пользователя</param>
            private void Update(bool bThrow, DataTable allProfiles)
            {
                string query = string.Empty
                    , errMsg = string.Empty;
                DataRow[] rows;
                DataTable table = new DataTable();

                foreach (DataColumn r in allProfiles.Columns)
                {
                    table.Columns.Add(r.ColumnName);
                }

                if (m_bIsRole == true)
                    rows = allProfiles.Select("ID_EXT=" + m_id_role + @" AND IS_ROLE=1");
                else
                    rows = allProfiles.Select("(ID_EXT=" + m_id_role + @" AND IS_ROLE=1)" + @" OR (ID_EXT=" + m_id_user + @" AND IS_ROLE=0)");

                foreach (DataRow r in rows)
                {
                    table.Rows.Add(r.ItemArray);
                    //foreach (DataColumn c in allProfiles.Columns)
                    //{
                    //    m_tblValues.Rows[m_tblValues.Rows.Count - 1][c.ColumnName] = r[c.ColumnName];
                    //}
                }
                m_tblValues = table.Copy();
            }

            /// <summary>
            /// Метод получения строки со значениями прав доступа
            /// </summary>
            /// <param name="id">ИД типа</param>
            /// <param name="bIsRole"></param>
            /// <returns></returns>
            private DataRow GetRowAllowed(int id)
            {
                DataRow objRes = null;

                DataRow[] rowsAllowed = m_tblValues.Select("ID_UNIT='" + id + "' and ID_TAB=0");

                switch (rowsAllowed.Length)
                {
                    case 1:
                        objRes = rowsAllowed[0];
                        break;
                    case 2:
                        //В табл. с настройками возможность 'id' определена как для "роли", так и для "пользователя"
                        // требуется выбрать строку с 'IS_ROLE' == 0 (пользователя)
                        // ...
                        foreach (DataRow r in rowsAllowed)
                            if (Int16.Parse(r[@"IS_ROLE"].ToString()) == Convert.ToInt32(m_bIsRole))
                            {
                                objRes = r;
                                break;
                            }
                            else
                                ;
                        break;
                    default: //Ошибка - исключение
                        throw new Exception(@"HUsers.HProfiles::GetAllowed (id=" + id + @") - не найдено ни одной записи...");
                }

                return objRes;
            }

            public static DataTable GetAllProfile(DbConnection dbConn)
            {
                int err = -1;
                string query = string.Empty
                    , errMsg = string.Empty;

                query = @"SELECT * FROM " + m_nameTableProfilesData + " ORDER BY ID_UNIT";
                m_allProfile = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
                return m_allProfile;
            }
        }

        /// <summary>
        /// Метод для получения таблицы со всеми профайлами
        /// </summary>
        /// <param name="dbConn"></param>
        /// <returns></returns>
        public static DataTable GetTableAllProfile(DbConnection dbConn)
        {
            return Profile.GetAllProfile(dbConn);
        }

        /// <summary>
        /// Метод для получения словаря со значениями прав доступа
        /// </summary>
        /// <param name="iListenerId">Идентификатор для подключения к БД</param>
        /// <param name="id_role">ИД роли</param>
        /// <param name="id_user">ИД пользователя</param>
        /// <param name="bIsRole">Пользователь или роль</param>
        /// <returns>Словарь со значениями</returns>
        public static Dictionary<int, UNIT_VALUE> GetDictProfileItem(DbConnection dbConn, int id_role, int id_user, bool bIsRole, DataTable allProfiles)
        {
            Dictionary<int, UNIT_VALUE> dictPrifileItem = null;
            Profile profile = new Profile(dbConn, id_role, id_user, bIsRole, allProfiles);

            dictPrifileItem = profile.GetProfileItem;

            return dictPrifileItem;
        }


        /// <summary>
        /// Функция получения строки запроса пользователя
        ///  /// <returns>Строка строку запроса</returns>
        /// </summary>
        private static string getUsersRequest(string where, string orderby)
        {
            string strQuery = string.Empty;
            //strQuer//strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='" + Environment.UserDomainName + "\\" + Environment.UserName + "'";
            //strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='NE\\ChrjapinAN'";
            strQuery = "SELECT * FROM users";
            if ((!(where == null)) && (where.Length > 0))
                strQuery += " WHERE " + where;
            else
                ;

            if ((!(orderby == null)) && (orderby.Length > 0))
                strQuery += " ORDER BY " + orderby;
            else
                ;

            return strQuery;
        }

        /// <summary>
        /// Функция запроса для поиска пользователя
        /// </summary>
        public static void GetUsers(ref DbConnection conn, string where, string orderby, out DataTable users, out int err)
        {
            err = 0;
            users = null;

            if (!(conn == null))
            {
                users = new DataTable();
                Logging.Logg().Debug(@"HUsers::GetUsers () - запрос для поиска пользователей = [" + getUsersRequest(where, orderby) + @"]", Logging.INDEX_MESSAGE.NOT_SET);
                users = DbTSQLInterface.Select(ref conn, getUsersRequest(where, orderby), null, null, out err);
            }
            else
            {
                err = -1;
            }
        }

        /// <summary>
        /// Функция взятия ролей из БД
        /// </summary>
        public static void GetRoles(ref DbConnection conn, string where, string orderby, out DataTable roles, out int err)
        {
            err = 0;
            roles = null;
            string query = string.Empty;

            if (!(conn == null))
            {
                roles = new DataTable();
                query = @"SELECT * FROM ROLES_UNIT";

                if ((where.Equals(null) == true) || (where.Equals(string.Empty) == true))
                    query += @" WHERE ID < 500";
                else
                    query += @" WHERE " + where;

                roles = DbTSQLInterface.Select(ref conn, query, null, null, out err);
            }
            else
            {
                err = -1;
            }
        }

        public static DataTable GetRolesPanels(DbConnection conn, out int err)
        {
            err = 0;
            DataTable roles = null;
            string query = string.Empty;

            if (!(conn == null))
            {
                roles = new DataTable();
                query = @"SELECT ID, DESCRIPTION, ID_UNIT=8 FROM dbo.fpanels ORDER BY ID";

                roles = DbTSQLInterface.Select(ref conn, query, null, null, out err);
            }
            else
            {
                err = -1;
            }
            return roles;
        }

        public static DataTable GetProfiles(DbConnection conn, out int err)
        {
            err = 0;
            DataTable profiles = null;
            string query = string.Empty;

            if (!(conn == null))
            {
                profiles = new DataTable();
                query = "SELECT dbo.roles.ID_EXT, dbo.roles.IS_ROLE, dbo.roles.ID_FPANEL, dbo.roles.IsUse AS VALUE FROM dbo.roles ORDER BY dbo.roles.ID_FPANEL";
                profiles = DbTSQLInterface.Select(ref conn, query, null, null, out err);
            }
            else
            {
                err = -1;
            }
            return profiles;
        }


    }
}
