using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginProject
{
    public class PanelPrjRolesAccess : HPanelEditTree
    {
        DataTable m_table_TEC = new DataTable();

        /// <summary>
        /// Текущий выбранный компонент
        /// </summary>
        int m_sel_comp;

        TreeView_Users.ID_Comp m_list_id;

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

        /// <summary>
        /// Тип выбраной ноды
        /// </summary>
        TreeView_Users.Type_Comp m_type_sel_node;

        /// <summary>
        /// Идентификаторы объектов формы
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE,
            BUTTON_BREAK
                , TREE_DICT_ITEM,
            DGV_DICT_PROP
                , INDEX_CONTROL_COUNT,
        };

        /// <summary>
        /// Идентификаторы типов таблиц
        /// </summary>
        public enum ID_Table : int { Unknown = -1, Role, User, Panels, Count }

        /// <summary>
        /// Возвратить наименование компонента 
        /// </summary>
        /// <param name="indx">Индекс </param>
        /// <returns>Строка - наименование</returns>
        protected static string getNameMode(ID_Table id)
        {
            string[] nameModes = { "roles", "users", "profiles" };

            return nameModes[(int)id];
        }


        protected DataTable m_tblAccessUnit;
        DataTable m_tblEdit, 
            m_tblOrigin, 
            m_tblItem;

        /// <summary>
        /// Массив оригинальных таблиц
        /// </summary>
        DataTable[] m_arr_UserRolesTable;

        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить" };

        public PanelPrjRolesAccess(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
            m_handlerDb = createHandlerDb();
            m_arr_UserRolesTable = new DataTable[3];
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        private void InitializeComponent()
        {
            Control ctrl = null;

            this.SuspendLayout();

            //Добавить кропки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;
            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_BREAK + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);
            //TreeView
            ctrl = new TreeView_Users(false);
            ctrl.Name = INDEX_CONTROL.TREE_DICT_ITEM.ToString();
            ctrl.Dock = DockStyle.Fill;
            this.Controls.Add(ctrl, 1, 0);
            this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 13);

            //DGV
            ctrl = new DataGridView_Prop_Text_Check();
            ctrl.Name = INDEX_CONTROL.DGV_DICT_PROP.ToString();
            ctrl.Dock = DockStyle.Fill;
            this.Controls.Add(ctrl, 7, 0);
            this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 10);
            addLabelDesc("PANEL_DESC");
            this.ResumeLayout();

            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Enabled = false;

            //Обработчика нажатия кнопок
            ((Button)Controls.Find (INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(this.btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0]).Click += new System.EventHandler(this.btnBreak_Click);

            ((DataGridView_Prop_Text_Check)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).EventCellValueChanged += new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_CellEndEdit);
        }

        protected override void initialize(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            fillDataTable();

        }

        protected void fillDataTable()
        {
            int err = -1;
            TreeNode node = null;
            TreeNode par_node = null;
            TreeView_Users tree = ((TreeView_Users)Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]);
            if (tree.SelectedNode != null)
            {
                node = tree.SelectedNode;
                par_node = tree.SelectedNode.Parent;
            }
            m_handlerDb.RegisterDbConnection(out err);
            m_tblOrigin = null;
            m_tblOrigin = User.GetProfiles(m_handlerDb.DbConnection, out err);
            m_arr_UserRolesTable[(int)ID_Table.Role] = m_handlerDb.GetDataTable(@"roles_unit", out err);
            m_arr_UserRolesTable[(int)ID_Table.User] = m_handlerDb.GetDataTable(@"users", out err);
            m_arr_UserRolesTable[(int)ID_Table.Panels] = User.GetRolesPanels(m_handlerDb.DbConnection, out err);

            if (m_table_TEC.Columns.Count == 0)
            {
                DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
                m_table_TEC.Columns.AddRange(columns);
            }

            //m_list_TEC = new InitTEC_200(idListener, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec;
            m_table_TEC.Rows.Clear();
            m_table_TEC.Rows.Add(new object[] { "5", "ТЭЦ-5" });

            Control ctrl = this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0];
            ((TreeView_Users)ctrl).Update_tree(m_arr_UserRolesTable[(int)ID_Table.User], m_arr_UserRolesTable[(int)ID_Table.Role]);
            ((TreeView_Users)ctrl).GetID += new TreeView_Users.intGetID(this.GetNextID);
            ((TreeView_Users)ctrl).EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            ((TreeView_Users)ctrl).Report += new TreeView_Users.ReportEventHandler(this.tree_report);

            ((DataGridView_Prop_Text_Check)this.Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).Create_DGV(m_arr_UserRolesTable[(int)ID_Table.Panels]);
            
            m_handlerDb.UnRegisterDbConnection();
            resetDataTable();


            if (node == null)
            {
                tree.SelectedNode = tree.Nodes[1];
                tree.SelectedNode = tree.Nodes[0];
            }
            else
            {
                tree.SelectedNode = tree.Nodes[0];
                tree.SelectedNode = tree.Nodes[1];
                if (par_node != null)
                {
                    tree.SelectedNode = tree.Nodes[par_node.Index].Nodes[node.Index];
                }
                else
                {
                    tree.SelectedNode = tree.Nodes[node.Index];
                }
            }
        }

        protected void resetDataTable()
        {
            m_tblEdit = null;
            m_tblEdit = m_tblOrigin.Copy();
        }

        protected override void addLabelDesc(string id, int posCol = 7, int posRow = 10)
        {
            base.addLabelDesc(id, posCol, posRow);
        }

        /// <summary>
        /// Обработчик для получения следующего идентификатора
        /// </summary>
        /// <returns>Возвращает идентификатор</returns>
        private int GetNextID(object sender, TreeView_Users.GetIDEventArgs e)
        {
            int ID = 0;
            int err = 0;

            if (e.IdComp == (int)ID_Table.Role)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_UserRolesTable[(int)ID_Table.Role], out err);
            }
            if (e.IdComp == (int)ID_Table.User)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_UserRolesTable[(int)ID_Table.User], out err);
            }

            return ID;
        }

        /// <summary>
        /// Обработчик события получения сообщения от TreeView
        /// </summary>
        private void tree_report(object sender, TreeView_Users.ReportEventArgs e)
        {
            //if (e.Action != string.Empty)
            //    delegateActionReport(e.Action);
            //if (e.Warning != string.Empty)
            //    delegateWarningReport(e.Warning);
            //if (e.Error != string.Empty)
            //    delegateErrorReport(e.Error);
            //if (e.Clear != false)
            //    delegateReportClear(e.Clear);
        }

        /// <summary>
        /// Внесени изменений в измененную таблицу со списком компонентов
        /// </summary>
        /// <param name="id_comp">ID компонента</param>
        /// <param name="header">Заголовок изменяемой ячейки</param>
        /// <param name="value">Новое значение изменяемой ячейки</param>
        /// <param name="table_edit">Таблицу в которую поместить изменения</param>
        private void edit_table(int id_comp, string header, string value, DataTable table_edit, TreeView_Users.ID_Comp list_id)
        {
            for (int i = 0; i < table_edit.Rows.Count; i++)
            {
                if (Convert.ToInt32(table_edit.Rows[i]["ID"]) == id_comp)
                {
                    for (int b = 0; b < table_edit.Columns.Count; b++)
                    {
                        if (table_edit.Columns[b].ColumnName.ToString() == header)
                        {
                            if (table_edit.Rows[i][b].ToString() != value)
                            {
                                table_edit.Rows[i][b] = value;

                                if (header == "DESCRIPTION")
                                {
                                    ((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Rename_Node(list_id, value);
                                }
                                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = true;
                                //btnOK.Enabled = true;
                                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = true;
                                //btnBreak.Enabled = true;
                            }
                        }
                    }
                }
            }
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
            DataTable[] massTable = new DataTable[1];
            
            DataRow[] profiles;
            m_sel_comp = IdComp;
            m_list_id = list_id;
            m_type_sel_node = list_id.type;
            if (m_list_id.id_user != -1)
            {
                profiles = m_tblEdit.Select("ID_EXT=" + m_list_id.id_user + " and IS_ROLE=0");
            }
            else
                profiles = m_tblEdit.Select("ID_EXT=" + m_list_id.id_role + " and IS_ROLE=1");

            massTable[0] = m_tblEdit.Clone();
            massTable[0].DefaultView.Sort = "ID_FPANEL";
            
            foreach (DataRow r in profiles)
            {
                massTable[0].Rows.Add(r.ItemArray);
            }

            foreach (DataRow r in m_arr_UserRolesTable[(int)ID_Table.Panels].Rows)
            {
                DataRow[] row_sel = massTable[0].Select("ID_FPANEL=" + r["ID"]);
                if (row_sel.Length == 0)
                {
                    massTable[0].Rows.Add(new object[] {null,null, r["ID"] ,0});
                }
            }
            massTable[0]=massTable[0].DefaultView.ToTable();
            
            ((DataGridView_Prop_Text_Check)this.Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(),true)[0]).Update_DGV(IdComp, massTable);
            
        }

        protected void dgvProp_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            string id = e.m_Header_name.Trim();//m_arr_UserRolesTable[(int)ID_Table.Panels].Select(@"DESCRIPTION='" + e.m_Header_name + @"'")[0]["ID"].ToString();
            object[] obj = new object[2];
            if (m_type_sel_node == TreeView_Users.Type_Comp.Role)
            {
                obj[0] = 1;
            }
            if (m_type_sel_node == TreeView_Users.Type_Comp.User)
            {
                obj[0] = 0;
            }

            if (e.m_Value == "True")
                obj[1] = 1;
            else
                if (e.m_Value == "False")
                    obj[1] = 0;
                else
                    obj[1] = e.m_Value;

            DataRow[] rows = m_tblEdit.Select("ID_EXT=" + m_sel_comp + " and IS_ROLE=" + obj[0] + " and ID_FPANEL=" + id);
            if (rows.Length == 0)
                m_tblEdit.Rows.Add(new object[] { m_sel_comp, obj[0], id, obj[1]});
            else
                rows[0]["VALUE"] = obj[1];
            activate_btn(true);
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
            string keys = string.Empty;
            m_tblOrigin.Columns["VALUE"].ColumnName = m_tblEdit.Columns["VALUE"].ColumnName = "IsUse";
            m_handlerDb.RegisterDbConnection(out err);
            m_handlerDb.RecUpdateInsertDelete(getNameMode(ID_Table.Role), "ID_EXT,IS_ROLE,ID_FPANEL", string.Empty, m_tblOrigin, m_tblEdit, out err);
            m_handlerDb.UnRegisterDbConnection();
            m_tblOrigin.Columns["IsUse"].ColumnName = m_tblEdit.Columns["IsUse"].ColumnName = "VALUE";
            fillDataTable();
            resetDataTable();

            //((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

            activate_btn(false);
        }
    }
}
