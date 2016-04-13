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
        TreeView_Users tvUsers = new TreeView_Users(false);
        DataGridView_Prop_Text_Check dgvProp = new DataGridView_Prop_Text_Check();
        DataGridView_Prop_Text_Check dgvProp_Context = new DataGridView_Prop_Text_Check();
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
        protected static string[] m_arButtonText = { @"Сохранить", @"Отмена" };

        DataTable m_AllUnits;

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
            string[] nameModes = { "roles_unit", "users" };

            return nameModes[(int)id];
        }

        #endregion

        public PanelPrjRolesFPanels(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];

            DataTable context_Unit = new DataTable();
            m_AllUnits = HUsers.GetTableProfileUnits;
            context_Unit = m_AllUnits.Clone();
            foreach (DataRow r in m_AllUnits.Select("ID>3"))
            {
                context_Unit.Rows.Add(r.ItemArray);
                m_AllUnits.Rows.Remove(r);
            }
            dgvProp.create_dgv(m_AllUnits);
            dgvProp_Context.create_dgv(context_Unit);

            fillDataTable();
            resetDataTable();

            tvUsers.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
            tvUsers.EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            panelProfiles.GetTableContext += new PanelProfiles.GetTableContextEventHandler(panelProfiles_GetTableContext);
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
            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = false;
            //btnBreak.Enabled = true;
            
            tvUsers.Dock = DockStyle.Fill;
            dgvProp.Dock = DockStyle.Fill;
            dgvProp_Context.Dock = DockStyle.Fill;
            panelProfiles.Dock = DockStyle.Fill;
            panel_Prop.Dock = DockStyle.Fill;

            this.Controls.Add(tvUsers, 1, 0);
            this.SetColumnSpan(tvUsers, 4); this.SetRowSpan(tvUsers, 13);

            this.panel_Prop.ColumnCount = 1;
            this.panel_Prop.RowCount = 2;

            this.panel_Prop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel_Prop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));

            this.panel_Prop.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));

            this.panel_Prop.Controls.Add(dgvProp, 0, 0);
            this.SetColumnSpan(dgvProp, 1); this.SetRowSpan(dgvProp, 1);
            this.panel_Prop.Controls.Add(dgvProp_Context, 0, 1);
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
            int idListener;
            DbConnection connConfigDB;

            int err = -1;

            idListener = register_idListenerConfDB(out err);
            connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
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

            HTepUsers.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";

            HTepUsers.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";

            m_arr_origTable[(int)ID_Table.Profiles] = User.GetTableAllProfile(connConfigDB);

            
            string query = "Select * from dbo.task";
            m_ar_panel_table[0]=HClassLibrary.DbTSQLInterface.Select(ref connConfigDB,query,null,null,out err);
            
            query = "Select * from dbo.plugins";
            m_ar_panel_table[1]=HClassLibrary.DbTSQLInterface.Select(ref connConfigDB,query,null,null,out err);
            
            query = "Select * from dbo.fpanels";
            m_ar_panel_table[2]=HClassLibrary.DbTSQLInterface.Select(ref connConfigDB,query,null,null,out err);

            unregister_idListenerConfDB(idListener);
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
            bool bIsRole = false;
            m_sel_comp = IdComp;
            m_list_id = list_id;
            massTable[0] = getProfileTable(m_arr_editTable[(int)ID_Table.Profiles], list_id.id_role, list_id.id_user, bIsRole);
            dgvProp.Update_dgv(IdComp, massTable);
            if (list_id.id_user != -1)
                panelProfiles.FillControls(m_ar_panel_table, m_arr_origTable[(int)ID_Table.Profiles], IdComp, false);
            else
                panelProfiles.FillControls(m_ar_panel_table, m_arr_origTable[(int)ID_Table.Profiles], IdComp, true);
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
            int idListener
                , err;
            Dictionary<int, User.UNIT_VALUE> profile = null;

            idListener = register_idListenerConfDB(out err);
            connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            profile = User.GetDictProfileItem(connConfigDB, id_role, id_user, bIsRole, tableAllProfiles);

            unregister_idListenerConfDB(idListener);

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

        /// <summary>
        /// Регистрация ID
        /// </summary>
        /// <param name="err">Ошибка в процессе регистрации</param>
        /// <returns>Возвращает ID</returns>
        protected int register_idListenerConfDB(out int err)
        {
            err = -1;
            int idListener = -1;

            ConnectionSettings connSett = FormMainBaseWithStatusStrip.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett();
            idListener = DbSources.Sources().Register(connSett, false, CONN_SETT_TYPE.MAIN_DB.ToString());

            return idListener;
        }

        /// <summary>
        /// Отмена регистрации ID
        /// </summary>
        /// <param name="idListener">ID</param>
        protected void unregister_idListenerConfDB(int idListener)
        {
            DbSources.Sources().UnRegister(idListener);
        }

        protected void panelProfiles_GetTableContext(object sender, PanelProfiles.GetTableContextEventArgs e)
        {
            dgvProp_Context.Update_dgv(0, new DataTable[] { (DataTable)e.table });
        }

    }

    public partial class PanelProfiles : TableLayoutPanel
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
        private System.Windows.Forms.ListBox listContext;

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
            this.listContext = new System.Windows.Forms.ListBox();
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
            // listContext
            // 
            this.SetColumnSpan(this.listContext, 5);
            this.SetRowSpan(this.listContext, 6);
            this.listContext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listContext.Name = "tbContext";
            this.listContext.TabIndex = 0;
            this.listContext.SelectedIndexChanged += new System.EventHandler(this.listContext_SelectedIndexChanged);
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
            this.Controls.Add(this.listContext, 0, 4);
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

        public void FillControls(DataTable[] ar_cbTable, DataTable profiles, int id_role_user, bool role)
        {
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
            listContext.DataSource = new DataTable();
            //cbTasks.SelectedIndex = 1;
            //cbTasks.SelectedIndex = 0;
            cbTasks.SelectedIndexChanged += cbTasks_SelectedIndexChanged;
        }

        private void cbTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbPlugins.SelectedIndexChanged -= cbPlugins_SelectedIndexChanged;
            arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS].Rows.Clear();
            foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PLUGINS].Select("ID_TASK=" + cbTasks.SelectedValue.ToString()))
                arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS].Rows.Add(row.ItemArray);

            cbPlugins.DataSource = arr_Tables_edit[(int)INDEX_COMBOBOX.PLUGINS];
            cbPlugins.ValueMember = "ID";
            cbPlugins.DisplayMember = "DESCRIPTION";
            //cbPlugins.SelectedIndex = 1;
           //cbPlugins.SelectedIndex = 0;
            cbPlugins.Text = string.Empty;
            cbPanels.Text = string.Empty;
            cbItems.Text = string.Empty;
            listContext.DataSource = new DataTable();
            cbPlugins.SelectedIndexChanged += cbPlugins_SelectedIndexChanged;
        }

        private void cbPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            listContext.DataSource = new DataTable();
            cbPanels.SelectedIndexChanged += cbPanels_SelectedIndexChanged;
        }

        private void cbPanels_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbItems.SelectedIndexChanged -= cbItems_SelectedIndexChanged;
            arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Clear();
            foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Select("ID_TAB=" + cbPanels.SelectedValue.ToString() + " and ID_EXT =" + m_id_obj + " and IS_ROLE=" + m_b_role.ToString()))
            {
                arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Rows.Add(row.ItemArray);
            }

            DataTable items = new DataTable();
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
            listContext.DataSource = new DataTable();
            cbItems.SelectedIndexChanged += cbItems_SelectedIndexChanged;
        }

        private void cbItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            listContext.SelectedIndexChanged -= listContext_SelectedIndexChanged;

            DataTable items = new DataTable();
            items.Columns.Add("ID");
            items.Rows.Clear();
            foreach (DataRow row in arr_Tables_edit[(int)INDEX_COMBOBOX.ITEMS].Select("ID_ITEM=" + cbItems.SelectedValue.ToString()))
            {
                if (items.Select("ID=" + row["ID_CONTEXT"].ToString().Trim()).Length == 0)
                {
                    items.Rows.Add(new object[] { row["ID_CONTEXT"].ToString().Trim() });
                }
            }
            items.DefaultView.Sort="ID";
            listContext.DataSource = items;
            listContext.ValueMember = "ID";
            listContext.DisplayMember = "ID";
            //cbPanels.SelectedIndex = 1;
            //cbPanels.SelectedIndex = 0;
            listContext.SelectedIndexChanged += listContext_SelectedIndexChanged;
        }

        private void listContext_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable items = new DataTable();
            items = arr_Tables_edit[(int)INDEX_COMBOBOX.PROFILES].Clone();
            items.Rows.Clear();
            foreach (DataRow row in arr_Tables_orig[(int)INDEX_COMBOBOX.PROFILES].Select("ID_CONTEXT=" + listContext.SelectedValue.ToString() + " and ID_ITEM=" + cbItems.SelectedValue.ToString() + " and ID_TAB=" + cbPanels.SelectedValue.ToString() + " and ID_EXT =" + m_id_obj + " and IS_ROLE=" + m_b_role.ToString()))
            {
                items.Rows.Add(row.ItemArray);
            }

            if (GetTableContext != null)
            {
                GetTableContext(this, new GetTableContextEventArgs((object)items));
            }
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

    }

    public class DataGridView_Prop_Text_Check : DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }

        public DataGridView_Prop_Text_Check()
            : base()
        {
            this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
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
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Cells[0] is DataGridViewCheckBoxCell)
                {
                    if (Convert.ToInt32(tables[0].Rows[i]["VALUE"]) == 0)
                        this.Rows[i].Cells[0].Value = false;
                    else
                        this.Rows[i].Cells[0].Value = true;
                }
                else
                {
                    if (tables[0].Rows.Count <= i)
                    {
                        this.Rows[i].Cells[0].Value = string.Empty;
                    }
                    else
                    {
                        this.Rows[i].Cells[0].Value = tables[0].Rows[i]["VALUE"];
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

                query = @"SELECT * FROM " + m_nameTableProfilesData;
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
    }
}
