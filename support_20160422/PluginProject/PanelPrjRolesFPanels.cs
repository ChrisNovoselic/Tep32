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

namespace PluginProject
{
    public class PanelPrjRolesFPanels : HPanelEditTree
    {
        enum INDEX_PARSE_UNIT {USER=0, CONTEXT=3, PANEL=5 };
        TreeView_Users tvUsers = new TreeView_Users(false);
        DataGridView_Prop_Text_Check dgvProp = new DataGridView_Prop_Text_Check();
        DataGridView_Prop_Text_Check dgvProp_Context = new DataGridView_Prop_Text_Check();
        DataGridView_Prop_Text_Check dgvProp_Panel = new DataGridView_Prop_Text_Check();
        PanelProfiles panelProfiles = new PanelProfiles();
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
        public enum ID_Table : int { Unknown = -1, Role, User, Profiles, Count }

        /// <summary>
        /// Возвратить наименование компонента 
        /// </summary>
        /// <param name="indx">Индекс </param>
        /// <returns>Строка - наименование</returns>
        protected static string getNameMode(ID_Table id)
        {
            string[] nameModes = { "roles","users","profiles" };

            return nameModes[(int)id];
        }

        #endregion


        public PanelPrjRolesFPanels(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();

            //m_handlerDb = createHandlerDb();
            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];

            m_context_Unit = new DataTable();
            m_AllUnits = HUsers.GetTableProfileUnits.Copy(); ;
            m_context_Unit = m_AllUnits.Clone();
            m_panel_Unit = m_AllUnits.Clone();
            foreach (DataRow r in m_AllUnits.Select("ID>"+(int)INDEX_PARSE_UNIT.CONTEXT))
            {
                m_context_Unit.Rows.Add(r.ItemArray);
                m_AllUnits.Rows.Remove(r);
            }
            foreach (DataRow r in m_context_Unit.Select("ID>" + (int)INDEX_PARSE_UNIT.PANEL))
            {
                m_panel_Unit.Rows.Add(r.ItemArray);
                m_context_Unit.Rows.Remove(r);
            }
            dgvProp.create_dgv(m_AllUnits);
            dgvProp_Context.create_dgv(m_context_Unit);
            dgvProp_Panel.create_dgv(m_panel_Unit);

            tvUsers.EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            panelProfiles.GetTableContext += new PanelProfiles.GetTableContextEventHandler(panelProfiles_GetTableContext);
            panelProfiles.GetItem += new PanelProfiles.GetItemEventHandler(panelProfiles_GetItem);
            panelProfiles.GetContext += new PanelProfiles.GetContextEventHandler(panelProfiles_GetContext);
            panelProfiles.GetPanel += new PanelProfiles.GetPanelEventHandler(panelProfiles_GetPanel);
            panelProfiles.GetDelContext += new PanelProfiles.GetDelContextEventHandler(panelProfiles_GetDelContext);


            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Click += new EventHandler(btnSave_Click);
            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Click += new EventHandler(btnBreak_Click);
            dgvProp.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_CellEndEdit);
            dgvProp_Context.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_Context_CellEndEdit);
            dgvProp_Panel.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(dgvProp_Panel_CellEndEdit);

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
            panelProfiles.Dock = DockStyle.Fill;
            panel_Prop.Dock = DockStyle.Fill;

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

            this.Controls.Add(panelProfiles, 5, 0);
            this.SetColumnSpan(panelProfiles, 4); this.SetRowSpan(panelProfiles, 10);

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

            m_handlerDb.RegisterDbConnection(out err);

            DbConnection connConfigDB = m_handlerDb.DbConnection;

            if (m_table_TEC.Columns.Count == 0)
            {
                DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
                m_table_TEC.Columns.AddRange(columns);
            }

            //m_list_TEC = new InitTEC_200(idListener, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec;
            m_table_TEC.Rows.Clear();

            //foreach (TEC t in m_list_TEC)
            //{
            //    object[] row = { t.m_id.ToString(), t.name_shr.ToString() };

            //    table_TEC.Rows.Add(row);
            //}

            User.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";

            User.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";

            m_arr_origTable[(int)ID_Table.Profiles] = User.GetTableAllProfile(connConfigDB).Copy();

            
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
            DataTable[] massTable = new DataTable[1];
            DataTable[] tables = new DataTable[3];
            bIsRole = false;
            m_sel_comp = IdComp;
            m_list_id = list_id;
            massTable[0] = getProfileTable(m_arr_editTable[(int)ID_Table.Profiles], list_id.id_role, list_id.id_user, bIsRole);
            dgvProp.Update_dgv(IdComp, massTable);
            
            if (list_id.id_user.Equals(-1) == false)
            {
                bIsRole = false;
                m_type_sel_node = TreeView_Users.Type_Comp.User;
                panelProfiles.FillControls(m_ar_panel_table, m_arr_editTable[(int)ID_Table.Profiles], IdComp, false);

            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                bIsRole = true;
                m_type_sel_node = TreeView_Users.Type_Comp.Role;
                panelProfiles.FillControls(m_ar_panel_table, m_arr_editTable[(int)ID_Table.Profiles], IdComp, true);
            }
        }

        /// <summary>
        /// Получение таблицы профайла
        /// </summary>
        /// <param name="tableAllProfiles">Таблица со всеми профайлами</param>
        /// <param name="id_role">ИД роли</param>
        /// <param name="id_user">ИД пользователя</param>
        /// <param name="bIsRole">Это роль</param>
        /// <returns>Возвращает таблицу</returns>
        private DataTable getProfileTable(DataTable tableAllProfiles, int id_role, int id_user, bool bIsRole)
        {
            DataTable profileTable = new DataTable();
            profileTable.Columns.Add("ID");
            profileTable.Columns.Add("VALUE");
            profileTable.Columns.Add("ID_UNIT");
            DbConnection connConfigDB;
            int  err;
            Dictionary<int, User.UNIT_VALUE> profile = null;
            m_handlerDb.RegisterDbConnection(out err);
            connConfigDB = m_handlerDb.DbConnection;

            profile = User.GetDictProfileItem(connConfigDB, id_role, id_user, bIsRole, tableAllProfiles);

            m_handlerDb.UnRegisterDbConnection();
            for (int i = 0; i < profile.Count; i++)
            {
                object[] obj = new object[3];
                obj[0] = i + 1;
                obj[1] = profile[i + 1].m_value;
                obj[2] = profile[i + 1].m_idType;
                profileTable.Rows.Add(obj);
            }

            return profileTable;
        }

        protected void panelProfiles_GetTableContext(object sender, PanelProfiles.GetTableContextEventArgs e)
        {
            DataTable profile_context = ((DataTable)e.table).Clone();
            DataRow context_row = ((DataTable)e.table).Rows[0];
            foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT="
                + context_row["ID_EXT"]
                + " and IS_ROLE="
                + context_row["IS_ROLE"]
                + " AND ID_TAB="
                + context_row["ID_TAB"]
                + " AND ID_ITEM="
                + context_row["ID_ITEM"]
                + " AND ID_CONTEXT="
                + context_row["ID_CONTEXT"]))
            {
                profile_context.Rows.Add(r.ItemArray);
            }
            if (((DataTable)e.table).Rows.Count > 0)
            {
                dgvProp_Context.Update_dgv(0, new DataTable[] { profile_context });
            }
        }

        protected void panelProfiles_GetItem(object sender, PanelProfiles.GetItemEventArgs e)
        {
            m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(e.rowItem.ItemArray);
            TreeNode sel_node = tvUsers.SelectedNode;
            tvUsers.SelectedNode = sel_node;
            activate_btn(true);
        }

        protected void panelProfiles_GetContext(object sender, PanelProfiles.GetContextEventArgs e)
        {
            DataRow new_context = e.rowContext;
            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" 
                + new_context["ID_EXT"] 
                + " and IS_ROLE=" 
                + new_context["IS_ROLE"]
                + " AND ID_TAB="
                + new_context["ID_TAB"] 
                + " AND ID_ITEM="
                + new_context["ID_ITEM"] 
                + " AND ID_CONTEXT=-1");
            if (rows.Length == 0)
            {
                foreach (DataRow r in m_context_Unit.Rows)
                {
                    new_context["ID_UNIT"] = r["ID"];
                    new_context["VALUE"] = "-1";
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new_context.ItemArray);
                }
                
            }
            else
            {
                if (rows.Length == 1)
                {
                    rows[0]["ID_CONTEXT"] = new_context["ID_CONTEXT"];
                    rows[0]["ID_UNIT"] = m_context_Unit.Rows[0]["ID"]; ;
                    rows[0]["VALUE"] = "-1";
                    for (int i = 1; i < m_context_Unit.Rows.Count;i++ )
                    {
                        new_context["ID_UNIT"] = m_context_Unit.Rows[i]["ID"];
                        new_context["VALUE"] = "-1";
                        m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new_context.ItemArray);
                    }
                }
            }
            activate_btn(true);
            TreeNode sel_node = tvUsers.SelectedNode;
            tvUsers.SelectedNode = sel_node;
        }

        protected void panelProfiles_GetPanel(object sender, PanelProfiles.GetPanelEventArgs e)
        {
            DataTable profile_panel = m_arr_editTable[(int)ID_Table.Profiles].Clone();
            DataRow context_row = e.rowPanel;
            foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT="
                + context_row["ID_EXT"]
                + " and IS_ROLE="
                + context_row["IS_ROLE"]
                + " AND ID_TAB="
                + context_row["ID_TAB"]
                + " AND ID_ITEM="
                + context_row["ID_ITEM"]
                + " AND ID_CONTEXT="
                + context_row["ID_CONTEXT"]))
            {
                if(r["VALUE"].ToString().Trim()!="-1")
                    profile_panel.Rows.Add(r.ItemArray);
            }
            if (profile_panel.Rows.Count == 0)
            {
                foreach (DataRow r in m_panel_Unit.Rows)
                {
                    context_row["ID_UNIT"] = r["ID"];
                    context_row["VALUE"] = "0";
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(context_row.ItemArray);
                    profile_panel.Rows.Add(context_row.ItemArray);
                }
                activate_btn(true);
            }

            if (profile_panel.Rows.Count == m_panel_Unit.Rows.Count)
            {
                dgvProp_Panel.Update_dgv(0, new DataTable[] { profile_panel });
            }
        }

        protected void panelProfiles_GetDelContext(object sender, PanelProfiles.GetDelContextEventArgs e)
        {
            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + e.rowDelContext[0] + " and IS_ROLE=" + e.rowDelContext[1] + " and ID_TAB=" + e.rowDelContext[2] + " and ID_ITEM=" + e.rowDelContext[3] + " and ID_CONTEXT=" + e.rowDelContext[4]);
            if (rows.Length == 2)
            {
                foreach (DataRow r in rows)
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Remove(r);
                }
                activate_btn(true);
            }
        }

        protected void activate_btn(bool active)
        {
            this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(),true)[0].Enabled = active;
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

            if (validate_saving(m_arr_editTable[(int)ID_Table.Profiles], out warning) == false)
            {
                m_handlerDb.RegisterDbConnection(out err);
                m_handlerDb.RecUpdateInsertDelete(getNameMode(ID_Table.Profiles), "ID_EXT,IS_ROLE,ID_TAB,ID_ITEM,ID_CONTEXT,ID_UNIT", string.Empty, m_arr_origTable[(int)ID_Table.Profiles], m_arr_editTable[(int)ID_Table.Profiles], out err);
                m_handlerDb.UnRegisterDbConnection();
                fillDataTable();
                resetDataTable();

                //((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

                activate_btn(false);
            }
            else
            {
                //delegateWarningReport(warning[(int)ID_Table.Role] + warning[(int)ID_Table.User]);
                //MessageBox.Show(warning[0] + warning[1] + warning[2] + warning[3], "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected void dgvProp_CellEndEdit(object sender,DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            string id = m_AllUnits.Select(@"DESCRIPTION='" + e.m_Header_name + @"'")[0]["ID"].ToString();
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
            
            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + m_sel_comp + " and IS_ROLE=" + obj[0] + " and ID_UNIT=" + id + " and ID_TAB=0");
            if (rows.Length == 0)
                m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new object[] { m_sel_comp, obj[0], id ,obj[1], 0, 0, 0 });
            else
                rows[0]["VALUE"] = obj[1];
            activate_btn(true);

        }

        protected void dgvProp_Context_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            string id = m_context_Unit.Select(@"DESCRIPTION='" + e.m_Header_name + @"'")[0]["ID"].ToString();
            object[] obj = new object[2];
            string[] query = panelProfiles.GetSelectQuery;
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

            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + query[0] + " and IS_ROLE=" + query[1] + " and ID_UNIT=" + id + " AND ID_TAB=" + query[2] + " and ID_ITEM=" + query[3] + " AND ID_CONTEXT=" + query[4]);
            if (rows.Length == 0)
                m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new object[] { m_sel_comp, obj[0], id, obj[1], query[2], query[3], query[4] });
            else
                rows[0]["VALUE"] = obj[1];

            activate_btn(true);
        }

        protected void dgvProp_Panel_CellEndEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            string id = m_panel_Unit.Select(@"DESCRIPTION='" + e.m_Header_name + @"'")[0]["ID"].ToString();
            object[] obj = new object[2];
            string[] query = panelProfiles.GetSelectQuery;
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

            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + query[0] + " and IS_ROLE=" + query[1] + " and ID_UNIT=" + id + " AND ID_TAB=" + query[2] + " and ID_ITEM=0 AND ID_CONTEXT=0");
            if (rows.Length == 0)
                m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new object[] { m_sel_comp, obj[0], id, obj[1], query[2], 0, 0 });
            else
                rows[0]["VALUE"] = obj[1];

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
                        if(i!=3)
                            if (Convert.ToString(row[i]) == "-1")
                            {
                                have = true;
                                warning += "Для пользователя " + row["ID_EXT"] + " параметр " + table_profiles.Columns[i].ColumnName + " равен '-1'." + '\n';
                            }
                    }
                }
            return have;
        }

    }

    public class PanelProfiles : TableLayoutPanel
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов


        private System.Windows.Forms.ComboBox cbTasks;
        private System.Windows.Forms.ComboBox cbPlugins;
        private System.Windows.Forms.ComboBox cbPanels;
        private System.Windows.Forms.ComboBox cbItems;
        private System.Windows.Forms.DataGridView dgvContext;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBreak;
        private System.Windows.Forms.TextBox tbAddItem;

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbTasks = new System.Windows.Forms.ComboBox();
            this.cbPlugins = new System.Windows.Forms.ComboBox();
            this.cbPanels = new System.Windows.Forms.ComboBox();
            this.cbItems = new System.Windows.Forms.ComboBox();
            this.dgvContext = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBreak = new System.Windows.Forms.Button();
            this.tbAddItem = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cbTasks
            // 
            this.SetColumnSpan(this.cbTasks, 5);
            this.cbTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTasks.FormattingEnabled = true;
            this.cbTasks.Name = "cbTasks";
            this.SetRowSpan(this.cbTasks, 1);
            this.cbTasks.TabIndex = 0;
            this.cbTasks.SelectedIndexChanged += new System.EventHandler(this.cbTasks_SelectedIndexChanged);
            // 
            // cbPlugins
            // 
            this.SetColumnSpan(this.cbPlugins, 5);
            this.cbPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbPlugins.FormattingEnabled = true;
            this.cbPlugins.Name = "cbPlugins";
            this.SetRowSpan(this.cbPlugins, 1);
            this.cbPlugins.TabIndex = 0;
            this.cbPlugins.SelectedIndexChanged += new System.EventHandler(this.cbPlugins_SelectedIndexChanged);
            // 
            // cbPanels
            // 
            this.SetColumnSpan(this.cbPanels, 5);
            this.cbPanels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbPanels.FormattingEnabled = true;
            this.cbPanels.Name = "cbPanels";
            this.SetRowSpan(this.cbPanels, 1);
            this.cbPanels.TabIndex = 0;
            this.cbPanels.SelectedIndexChanged += new System.EventHandler(this.cbPanels_SelectedIndexChanged);
            // 
            // cbItems
            // 
            this.SetColumnSpan(this.cbItems, 3);
            this.cbItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbItems.FormattingEnabled = true;
            this.cbItems.Name = "cbItems";
            this.SetRowSpan(this.cbItems, 1);
            this.cbItems.TabIndex = 0;
            this.cbItems.SelectedIndexChanged += new System.EventHandler(this.cbItems_SelectedIndexChanged);
            // 
            //dgvContext
            // 
            this.SetColumnSpan(this.dgvContext, 5);
            this.SetRowSpan(this.dgvContext, 6);
            this.dgvContext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvContext.Name = "dgvContext";
            this.dgvContext.TabIndex = 0;
            this.dgvContext.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvContext.MultiSelect = false;
            this.dgvContext.Columns.Add("Context", "Context");
            this.dgvContext.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvContext.SelectionChanged += new System.EventHandler(this.dgvContext_SelectionChanged);
            this.dgvContext.RowsAdded += new DataGridViewRowsAddedEventHandler(this.dgvContext_RowsAdded);
            this.dgvContext.CellEndEdit += new DataGridViewCellEventHandler(this.dgvContext_EndCellEdit);
            this.dgvContext.UserDeletedRow += new DataGridViewRowEventHandler(this.dgvContext_DelRow);
            // 
            // tbAddItem
            // 
            this.SetColumnSpan(this.tbAddItem, 3);
            this.SetRowSpan(this.tbAddItem, 1);
            this.tbAddItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAddItem.Name = "tbAddItem";
            this.tbAddItem.TabIndex = 0;
            this.tbAddItem.Enabled = false;
            // 
            // btnAdd
            // 
            this.SetColumnSpan(this.btnAdd, 2);
            this.SetRowSpan(this.btnAdd, 1);
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Text = "Добавить";
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSave
            // 
            this.SetColumnSpan(this.btnSave, 1);
            this.SetRowSpan(this.btnSave, 1);
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.Name = "btnSave";
            this.btnSave.Text = "Сохранить";
            this.btnSave.TabIndex = 0;
            this.btnSave.Enabled = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBreak
            // 
            this.SetColumnSpan(this.btnBreak, 1);
            this.SetRowSpan(this.btnBreak, 1);
            this.btnBreak.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBreak.Name = "btnBreak";
            this.btnBreak.Text = "Отмена";
            this.btnBreak.TabIndex = 0;
            this.btnBreak.Enabled = false;
            this.btnBreak.Click += new System.EventHandler(this.btnBreak_Click);
            // 
            // panelProfiles
            // 
            this.ColumnCount = 5;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.Controls.Add(this.cbTasks, 0, 0);
            this.Controls.Add(this.cbPlugins, 0, 1);
            this.Controls.Add(this.cbPanels, 0, 2);
            this.Controls.Add(this.cbItems, 0, 3);
            this.Controls.Add(this.btnAdd, 3, 3);
            this.Controls.Add(this.tbAddItem, 0, 4);
            this.Controls.Add(this.btnSave, 3, 4);
            this.Controls.Add(this.btnBreak, 5, 4);
            this.Controls.Add(this.dgvContext, 0, 5);
            this.RowCount = 10;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.Dock = DockStyle.Fill;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        public enum INDEX_COMBOBOX { TASKS = 0, PLUGINS, PANELS, PROFILES, ITEMS, COUNT };
        DataTable[] arr_Tables_orig;
        DataTable[] arr_Tables_edit;
        int m_id_obj;
        bool m_b_role;

        public PanelProfiles()
        {
            InitializeComponent();

            arr_Tables_orig = new DataTable[(int)INDEX_COMBOBOX.COUNT];
            arr_Tables_edit = new DataTable[(int)INDEX_COMBOBOX.COUNT];
        }

        public string[] GetSelectQuery
        {
            get
            {
                object[] query_selected = new object[3];
                if (dgvContext.SelectedRows.Count == 0)
                {
                    query_selected[0] = 0;
                }
                else
                {
                    query_selected[0] = dgvContext.SelectedRows[0].Cells[0].Value;
                }
                if (cbItems.Text == string.Empty || cbItems.Text == "")
                {
                    query_selected[1] = 0;
                }
                else
                {
                    query_selected[1] = cbItems.SelectedValue.ToString();
                }
                if (cbPanels.Text == string.Empty || cbPanels.Text == "")
                {
                    query_selected[2] = 0;
                }
                else
                {
                    query_selected[2] = cbPanels.SelectedValue.ToString();
                }
                string[] query = { m_id_obj.ToString(), m_b_role.ToString(), query_selected[2].ToString(), query_selected[1].ToString(), query_selected[0].ToString() };

                return query;
            }
        }

        public void FillControls(DataTable[] ar_cbTable, DataTable profiles, int id_role_user, bool role)
        {
            dgvContext.RowsAdded -= dgvContext_RowsAdded;
            cbTasks.SelectedIndexChanged -= cbTasks_SelectedIndexChanged;
            m_id_obj = id_role_user;
            m_b_role = role;
            ar_cbTable.CopyTo(arr_Tables_orig,0);
            arr_Tables_orig[3] = profiles.Copy();

            arr_Tables_edit[0] = arr_Tables_orig[0].Clone();
            arr_Tables_edit[1] = arr_Tables_orig[1].Clone();
            arr_Tables_edit[2] = arr_Tables_orig[2].Clone();
            arr_Tables_edit[4] = profiles.Clone();
            arr_Tables_edit[3] = profiles.Copy();


            cbTasks.DataSource = arr_Tables_orig[(int)INDEX_COMBOBOX.TASKS];
            cbTasks.ValueMember = "ID";
            cbTasks.DisplayMember = "DESCRIPTION";
            cbTasks.Text = string.Empty;
            cbPlugins.Text = string.Empty;
            cbPanels.Text = string.Empty;
            cbItems.Text = string.Empty;
            dgvContext.Rows.Clear();
            
            cbTasks.SelectedIndexChanged += cbTasks_SelectedIndexChanged;
            cbTasks.SelectedIndex = -1;
            cbTasks.SelectedIndex = 0;
            dgvContext.RowsAdded += dgvContext_RowsAdded;
        }

        private void cbTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTasks.SelectedValue != null)
            {
                dgvContext.RowsAdded -= dgvContext_RowsAdded;
                cbPlugins.SelectedIndexChanged -= cbPlugins_SelectedIndexChanged;
                arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS].Rows.Clear();
                foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PLUGINS].Select("ID_TASK=" + cbTasks.SelectedValue.ToString()))
                    arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS].Rows.Add(row.ItemArray);

                cbPlugins.DataSource = arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS];
                cbPlugins.ValueMember = "ID";
                cbPlugins.DisplayMember = "DESCRIPTION";

                cbPlugins.Text = string.Empty;
                cbPanels.Text = string.Empty;
                cbItems.Text = string.Empty;

                dgvContext.Rows.Clear();
                cbPlugins.SelectedIndexChanged += cbPlugins_SelectedIndexChanged;
                cbPlugins.SelectedIndex = -1;
                cbPlugins.SelectedIndex = 0;
                dgvContext.RowsAdded += dgvContext_RowsAdded;
            }
        }

        private void cbPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPlugins.SelectedValue != null)
            {
                dgvContext.RowsAdded -= dgvContext_RowsAdded;
                cbPanels.SelectedIndexChanged -= cbPanels_SelectedIndexChanged;
                arr_Tables_edit[(int)INDEX_COMBOBOX.PANELS].Rows.Clear();
                foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PANELS].Select("ID_PLUGIN=" + cbPlugins.SelectedValue.ToString()))
                    arr_Tables_edit[(int)INDEX_COMBOBOX.PANELS].Rows.Add(row.ItemArray);

                cbPanels.DataSource = arr_Tables_edit[(int)INDEX_COMBOBOX.PANELS];
                cbPanels.ValueMember = "ID";
                cbPanels.DisplayMember = "DESCRIPTION";
                //cbPanels.SelectedIndex = 1;
                //cbPanels.SelectedIndex = 0;
                cbPanels.Text = string.Empty;
                cbItems.Text = string.Empty;
                dgvContext.Rows.Clear();
                cbPanels.SelectedIndexChanged += cbPanels_SelectedIndexChanged;
                cbPanels.SelectedIndex = -1;
                cbPanels.SelectedIndex = 0;
                dgvContext.RowsAdded += dgvContext_RowsAdded;
            }
        }

        private void cbPanels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPanels.SelectedValue != null)
            {
                DataTable items = new DataTable();
                DataTable newItemTable = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Clone();


                dgvContext.RowsAdded -= dgvContext_RowsAdded;
                cbItems.SelectedIndexChanged -= cbItems_SelectedIndexChanged;
                arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Clear();
                foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT<>'0' and ID_TAB=" + cbPanels.SelectedValue.ToString() + " and ID_EXT =" + m_id_obj + " and IS_ROLE=" + m_b_role.ToString()))
                {
                    arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Add(row.ItemArray);
                }

                if (cbPanels.SelectedValue != null)
                {
                    newItemTable.Rows.Add(new object[] { m_id_obj, Convert.ToInt32(m_b_role), -1, -1, cbPanels.SelectedValue.ToString(), 0, 0 });
                }

                if (GetPanel != null)
                {
                    GetPanel(this, new GetPanelEventArgs(newItemTable.Rows[0]));
                }

                items.Columns.Add("ID");
                foreach (DataRow row in arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows)
                {
                    if (items.Select("ID=" + row["ID_ITEM"].ToString().Trim()).Length == 0)
                    {
                        items.Rows.Add(new object[] { row["ID_ITEM"].ToString().Trim() });
                    }
                }
                cbItems.DataSource = items;
                cbItems.DisplayMember = "ID";
                cbItems.ValueMember = "ID";
                //listBoxItems.SelectedIndex = 1;
                //listBoxItems.SelectedIndex = 0;
                cbItems.Text = string.Empty;
                dgvContext.Rows.Clear();
                cbItems.SelectedIndexChanged += cbItems_SelectedIndexChanged;
                if (cbItems.Items.Count > 0)
                {
                    cbItems.SelectedIndex = -1;
                    cbItems.SelectedIndex = 0;
                }
                dgvContext.RowsAdded += dgvContext_RowsAdded;
            }
        }

        private void cbItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbItems.SelectedValue != null)
            {
                dgvContext.RowsAdded -= dgvContext_RowsAdded;
                dgvContext.SelectionChanged -= dgvContext_SelectionChanged;

                DataTable items = new DataTable();
                items.Columns.Add("ID");
                items.Rows.Clear();
                foreach (DataRow row in arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Select("ID_ITEM=" + cbItems.SelectedValue.ToString()))
                {
                    if (items.Select("ID=" + row["ID_CONTEXT"].ToString().Trim()).Length == 0)
                    {
                        if (Convert.ToInt32(row["ID_CONTEXT"]) != -1)
                            items.Rows.Add(new object[] { row["ID_CONTEXT"].ToString().Trim() });
                    }
                }
                items.DefaultView.Sort = "ID";

                dgvContext.Rows.Clear();
                foreach (DataRow row in items.Rows)
                {
                    dgvContext.Rows.Add(row["ID"]);
                    dgvContext.Rows[dgvContext.Rows.Count - 2].ReadOnly = true;
                }
                dgvContext.Sort(dgvContext.Columns["Context"], ListSortDirection.Ascending);
                //cbPanels.SelectedIndex = 1;
                //cbPanels.SelectedIndex = 0;
                dgvContext.SelectionChanged += dgvContext_SelectionChanged;
                dgvContext.Rows[1].Selected = true;
                dgvContext.Rows[0].Selected = true;
                dgvContext.RowsAdded += dgvContext_RowsAdded;
            }

        }

        private void dgvContext_SelectionChanged(object sender, EventArgs e)
        {
            dgvContext.RowsAdded -= dgvContext_RowsAdded;
            DataTable items = new DataTable();
            items = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Clone();
            items.Rows.Clear();
            if(dgvContext.SelectedRows.Count!=0)
                if (dgvContext.SelectedRows[0].Cells[0].Value != null)
                {
                    foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT=" + dgvContext.SelectedRows[0].Cells[0].Value.ToString() + " and ID_ITEM=" + cbItems.SelectedValue.ToString() + " and ID_TAB=" + cbPanels.SelectedValue.ToString() + " and ID_EXT =" + m_id_obj + " and IS_ROLE=" + m_b_role.ToString()))
                    {
                        items.Rows.Add(row.ItemArray);
                    }

                    if (GetTableContext != null)
                    {
                        GetTableContext(this, new GetTableContextEventArgs((object)items));
                    }
                }
            
            dgvContext.RowsAdded += dgvContext_RowsAdded;
        }

        private void dgvContext_DelRow(object sender, DataGridViewRowEventArgs e)
        {
            object[] query_selected = new object[3];
            query_selected[0] = 0;

            if (cbItems.Text == string.Empty || cbItems.Text == "")
            {
                query_selected[1] = 0;
            }
            else
            {
                query_selected[1] = cbItems.SelectedValue.ToString();
            }
            if (cbPanels.Text == string.Empty || cbPanels.Text == "")
            {
                query_selected[2] = 0;
            }
            else
            {
                query_selected[2] = cbPanels.SelectedValue.ToString();
            }
            string[] deleted = { m_id_obj.ToString(), m_b_role.ToString(), query_selected[2].ToString(), query_selected[1].ToString(), query_selected[0].ToString() };

            deleted[4] = e.Row.Cells[0].Value.ToString();
            if (GetDelContext != null)
                GetDelContext(this, new GetDelContextEventArgs(deleted));
        }

        bool new_row_need = false;
        private void dgvContext_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            new_row_need = true;
        }
        private void dgvContext_EndCellEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (new_row_need == true)
            {
                try
                {
                    DataTable newItemTable = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Clone();
                    if (cbItems.SelectedValue != null)
                    {
                        DataRow[] rows = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT=-1 and ID_ITEM=" + cbItems.SelectedValue.ToString() + " and ID_TAB=" + cbPanels.SelectedValue.ToString() + " and ID_EXT =" + m_id_obj + " and IS_ROLE=" + m_b_role.ToString());
                        if (rows.Length == 1)
                        {
                            arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT=-1 and ID_ITEM="
                                + cbItems.SelectedValue.ToString() + " and ID_TAB="
                                + cbPanels.SelectedValue.ToString() + " and ID_EXT ="
                                + m_id_obj + " and IS_ROLE="
                                + m_b_role.ToString())[0]["ID_CONTEXT"] = dgvContext.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                            arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT=-1 and ID_ITEM="
                                + cbItems.SelectedValue.ToString() + " and ID_TAB="
                                + cbPanels.SelectedValue.ToString() + " and ID_EXT ="
                                + m_id_obj + " and IS_ROLE="
                                + m_b_role.ToString())[0]["ID_CONTEXT"] = dgvContext.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                            arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Select("ID_CONTEXT=-1 and ID_ITEM="
                                + cbItems.SelectedValue.ToString() + " and ID_TAB="
                                + cbPanels.SelectedValue.ToString() + " and ID_EXT ="
                                + m_id_obj + " and IS_ROLE="
                                + m_b_role.ToString())[0]["ID_CONTEXT"] = dgvContext.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                            
                            if (GetContext != null)
                                GetContext(this, new GetContextEventArgs(rows[0]));

                            //dgvContext.Rows.RemoveAt(e.RowIndex);
                        }
                        else
                        {
                            newItemTable.Rows.Add(new object[] { m_id_obj, Convert.ToInt32(m_b_role), -1, -1, cbPanels.SelectedValue.ToString(), cbItems.Text, dgvContext.Rows[e.RowIndex].Cells[0].Value });
                            arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Rows.Add(newItemTable.Rows[0].ItemArray);
                            arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Rows.Add(newItemTable.Rows[0].ItemArray);
                            arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Add(newItemTable.Rows[0].ItemArray);
                            if (GetContext != null)
                                GetContext(this, new GetContextEventArgs(newItemTable.Rows[0]));
                        }
                    }
                }
                catch (Exception ec)
                {
                    //dgvContext.Rows.RemoveAt(e.RowIndex);

                    MessageBox.Show(ec.Message + '\n'+"Введите другое значение!");
                }
            }

            new_row_need = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            cbItems.Enabled = false;
            btnSave.Enabled = true;
            btnBreak.Enabled = true;
            tbAddItem.Enabled = true;
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable newItemTable = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Clone();
            if (tbAddItem.Text != "" & tbAddItem.Text != string.Empty)
            {
                newItemTable.Rows.Add(new object[] { m_id_obj, Convert.ToInt32(m_b_role), -1, -1, cbPanels.SelectedValue.ToString(), tbAddItem.Text, -1 });

                if (GetItem != null)
                {
                    arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Rows.Add(newItemTable.Rows[0].ItemArray);
                    arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Rows.Add(newItemTable.Rows[0].ItemArray);
                    arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Add(newItemTable.Rows[0].ItemArray);

                    ((DataTable)cbItems.DataSource).Rows.Add(tbAddItem.Text);

                    GetItem(this, new GetItemEventArgs(newItemTable.Rows[0]));
                }
            }

            btnBreak.PerformClick();
        }
        private void btnBreak_Click(object sender, EventArgs e)
        {
            cbItems.Enabled = true;
            btnSave.Enabled = false;
            btnBreak.Enabled = false;
            tbAddItem.Enabled = false;
            tbAddItem.Text = string.Empty;
        }



        /// <summary>
        /// Класс для описания аргумента события - получение таблицы с профайлами элементов
        /// </summary>
        public class GetTableContextEventArgs : EventArgs
        {
            /// <summary>
            /// таблица с профайлами элементов
            /// </summary>
            public object table;
            
            public GetTableContextEventArgs(object Table)
            {
                table = Table;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение таблицы с профайлами элементов
        /// </summary>
        public delegate void GetTableContextEventHandler(object obj, GetTableContextEventArgs e);

        /// <summary>
        /// Событие - получение таблицы с профайлами элементов
        /// </summary>
        public GetTableContextEventHandler GetTableContext;



        /// <summary>
        /// Класс для описания аргумента события - получение строки нового Item
        /// </summary>
        public class GetItemEventArgs : EventArgs
        {
            /// <summary>
            /// таблица с профайлами элементов
            /// </summary>
            public DataRow rowItem;

            public GetItemEventArgs(DataRow RowItem)
            {
                rowItem = RowItem;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение строки нового Item
        /// </summary>
        public delegate void GetItemEventHandler(object obj, GetItemEventArgs e);

        /// <summary>
        /// Событие - получение строки нового Item
        /// </summary>
        public GetItemEventHandler GetItem;


        /// <summary>
        /// Класс для описания аргумента события - получение строки нового Item
        /// </summary>
        public class GetPanelEventArgs : EventArgs
        {
            /// <summary>
            /// таблица с профайлами элементов
            /// </summary>
            public DataRow rowPanel;

            public GetPanelEventArgs(DataRow RowPanel)
            {
                rowPanel = RowPanel;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение строки нового Item
        /// </summary>
        public delegate void GetPanelEventHandler(object obj, GetPanelEventArgs e);

        /// <summary>
        /// Событие - получение строки нового Item
        /// </summary>
        public GetPanelEventHandler GetPanel;


        /// <summary>
        /// Класс для описания аргумента события - получение строки нового Item
        /// </summary>
        public class GetContextEventArgs : EventArgs
        {
            /// <summary>
            /// таблица с профайлами элементов
            /// </summary>
            public DataRow rowContext;

            public GetContextEventArgs(DataRow RowContext)
            {
                rowContext = RowContext;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение строки нового Item
        /// </summary>
        public delegate void GetContextEventHandler(object obj, GetContextEventArgs e);

        /// <summary>
        /// Событие - получение строки нового Item
        /// </summary>
        public GetContextEventHandler GetContext;

        /// <summary>
        /// Класс для описания аргумента события - получение строки нового Item
        /// </summary>
        public class GetDelContextEventArgs : EventArgs
        {
            /// <summary>
            /// таблица с профайлами элементов
            /// </summary>
            public string[] rowDelContext;

            public GetDelContextEventArgs(string[] RowDelContext)
            {
                rowDelContext = RowDelContext;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение строки нового Item
        /// </summary>
        public delegate void GetDelContextEventHandler(object obj, GetDelContextEventArgs e);

        /// <summary>
        /// Событие - получение строки нового Item
        /// </summary>
        public GetDelContextEventHandler GetDelContext;

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

                    foreach (DataRow r in HUsers.GetTableProfileUnits.Rows)
                    {
                        id_unit = (int)r[@"ID"];

                        if (id_unit <4)
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

                query = @"SELECT * FROM " + m_nameTableProfilesData +" ORDER BY ID_UNIT";
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
