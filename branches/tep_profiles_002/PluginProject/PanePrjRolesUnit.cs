using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

using System.Windows.Forms;
using System.Collections;
using System.Data;
using System.Data.Common;


using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PanelPrjRolesUnit : HPanelEditTree
    {
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

        DataTable m_AllUnits;
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

        /// <summary>
        /// Текущий выбранный компонент
        /// </summary>
        int m_sel_comp;

        TreeView_Users.ID_Comp m_list_id;
        DataTable m_table_TEC = new DataTable();

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
            string[] nameModes = { "roles_unit", "users", "profiles" };

            return nameModes[(int)id];
        }

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc">Плагин</param>
        public PanelPrjRolesUnit(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();

            //m_handlerDb = createHandlerDb();

            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];

        }

        public override bool Activate(bool active)
        {
            fillDataTable();
            resetDataTable();

            Control ctrl = new Control();
            ctrl = this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0];
            ((TreeView_Users)ctrl).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

            ((TreeView_Users)ctrl).GetID += new TreeView_Users.intGetID(this.GetNextID);
            ((TreeView_Users)ctrl).EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            ((TreeView_Users)ctrl).Report += new TreeView_Users.ReportEventHandler(this.tree_report);

            return base.Activate(active);
        }

        /// <summary>
        /// Инициализация компонентов
        /// </summary>
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

            Control ctrl = new Control();

            //treeView
            ctrl = new TreeView_Users();

            ctrl.Name = INDEX_CONTROL.TREE_DICT_ITEM.ToString();
            this.Controls.Add(ctrl, 1, 0);
            this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 13);

            //dgv1
            ctrl = new DataGridView_Prop_ComboBoxCell();
            ctrl.Name = INDEX_CONTROL.DGV_DICT_PROP.ToString();
            ctrl.Dock = DockStyle.Fill;
            ((DataGridView_Prop_ComboBoxCell)ctrl).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.Controls.Add(ctrl, 5, 0);

            this.SetColumnSpan(ctrl, 8); this.SetRowSpan(ctrl, 10);

            addLabelDesc(INDEX_CONTROL.PUNEL_PROP_DESC.ToString());

            this.ResumeLayout();

            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(this.buttonSAVE_click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0]).Click += new System.EventHandler(this.buttonBreak_click);
            ((DataGridView_Prop_ComboBoxCell)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_EndCellEdit);
            ((DataGridView_Prop_ComboBoxCell)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).SelectionChanged += new EventHandler(this.HPanelEdit_dgvPropSelectionChanged);

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
            DbConnection connConfigDB;

            int err = -1;
            m_handlerDb.RegisterDbConnection(out err);
            connConfigDB = m_handlerDb.DbConnection;

            if (m_table_TEC.Columns.Count == 0)
            {
                DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
                m_table_TEC.Columns.AddRange(columns);
            }

            //m_list_TEC = new InitTEC_200(idListener, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec;
            m_table_TEC.Rows.Clear();
            m_table_TEC.Rows.Add(new object[] { "5", "ТЭЦ-5" });

            //foreach (TEC t in m_list_TEC)
            //{
            //    object[] row = { t.m_id.ToString(), t.name_shr.ToString() };

            //    table_TEC.Rows.Add(row);
            //}

            User.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";

            User.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";

            m_AllUnits = HUsers.GetTableProfileUnits.Copy();
            foreach (DataRow r in m_AllUnits.Select("ID>3"))
            {
                m_AllUnits.Rows.Remove(r);
            }

            m_arr_origTable[(int)ID_Table.Profiles] = User.GetTableAllProfile(connConfigDB).Copy();

            m_handlerDb.UnRegisterDbConnection();
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
        /// Обработчик для получения следующего идентификатора
        /// </summary>
        /// <returns>Возвращает идентификатор</returns>
        private int GetNextID(object sender, TreeView_Users.GetIDEventArgs e)
        {
            int ID = 0;
            int err = 0;

            if (e.IdComp == (int)ID_Table.Role)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)ID_Table.Role], out err);
            }
            if (e.IdComp == (int)ID_Table.User)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)ID_Table.User], out err);
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
        /// Обработчик события окончания изменения ячейки свойств
        /// </summary>
        private void dgvProp_EndCellEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            //delegateReportClear(true);

            if (m_type_sel_node == TreeView_Users.Type_Comp.Role)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.Role], m_list_id);
            }
            if (m_type_sel_node == TreeView_Users.Type_Comp.User)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.User], m_list_id);
            }
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
            if (e.Operation == TreeView_Users.ID_Operation.Delete)
            {
                delete(e.PathComp);
            }
            if (e.Operation == TreeView_Users.ID_Operation.Insert)
            {
                insert(e.PathComp);
            }
            if (e.Operation == TreeView_Users.ID_Operation.Update)
            {
                update(e.PathComp, e.Value);
            }
        }

        /// <summary>
        /// Метод удаления компонента из таблицы
        /// </summary>
        /// <param name="list_id">Список идентификаторов объекта</param>
        private void delete(TreeView_Users.ID_Comp list_id)
        {
            int iRes = 0;

            if (list_id.id_user.Equals(-1) == false)
            {
                m_arr_editTable[(int)ID_Table.User].Rows.Remove(m_arr_editTable[(int)ID_Table.User].Select("ID=" + list_id.id_user)[0]);
                foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + list_id.id_user + " and IS_ROLE=0 and ID_TAB=0 and ID_ITEM=0 and CONTEXT=0"))
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Remove(r);
                }
                iRes = 1;
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                m_arr_editTable[(int)ID_Table.Role].Rows.Remove(m_arr_editTable[(int)ID_Table.Role].Select("ID=" + list_id.id_role)[0]);
                foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + list_id.id_role + " and IS_ROLE=1 and ID_TAB=0 and ID_ITEM=0 and CONTEXT=0"))
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Remove(r);
                }
                iRes = 1;
            }

            if (iRes == 1)
            {
                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = true;
                //btnOK.Enabled = true;
                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = true;
                //btnBreak.Enabled = true;
            }
        }

        /// <summary>
        /// Метод обновления связей компонента
        /// </summary>
        /// <param name="list_id">Идентификаторы компонента</param>
        /// <param name="obj">Тип изменяемого объекта ИД=1</param>
        private void update(TreeView_Users.ID_Comp list_id, string type_op)
        {
            string type = type_op;
            int iRes = 0;
            if (list_id.id_user.Equals(-1) == false)
            {
                if (iRes == 1)
                {
                    ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = true;
                    //btnOK.Enabled = true;
                    ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = true;
                    //btnBreak.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Метод добавления нового компонента
        /// </summary>
        /// <param name="list_id">Идентификатор нового компонента</param>
        /// <param name="obj"></param>
        private void insert(TreeView_Users.ID_Comp list_id)
        {
            int iRes = 0;
            if (list_id.id_user.Equals(-1) == false)//Добавление нового пользователя
            {
                object[] obj = new object[m_arr_editTable[(int)ID_Table.User].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)ID_Table.User].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID")
                    {
                        obj[i] = list_id.id_user;
                    }
                    else
                        if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID_ROLE")
                        {
                            obj[i] = list_id.id_role;
                        }
                        else
                            if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "IP")
                            {
                                obj[i] = "255.255.255.255";
                            }
                            else
                                if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "DESCRIPTION")
                                {
                                    obj[i] = TreeView_Users.Mass_NewVal_Comp((int)ID_Table.User);
                                }
                                else
                                    if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID_TEC")
                                    {
                                        obj[i] = 0;
                                    }
                                    else
                                        obj[i] = -1;
                }

                m_arr_editTable[(int)ID_Table.User].Rows.Add(obj);
                foreach (DataRow r in m_AllUnits.Rows)
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new object[] { obj[0], 0, r["ID"], "0", "0", "0", "0" });
                }
                iRes = 1;
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)//Добавление новой роли
            {
                object[] obj_role = new object[m_arr_editTable[(int)ID_Table.Role].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)ID_Table.Role].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)ID_Table.Role].Columns[i].ColumnName == "ID")
                    {
                        obj_role[i] = list_id.id_role;
                    }
                    else
                        if (m_arr_editTable[(int)ID_Table.Role].Columns[i].ColumnName == "DESCRIPTION")
                        {
                            obj_role[i] = TreeView_Users.Mass_NewVal_Comp((int)ID_Table.Role);
                        }
                }

                m_arr_editTable[(int)ID_Table.Role].Rows.Add(obj_role);
                foreach (DataRow r in m_AllUnits.Rows)
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(new object[] { obj_role[0], 1, r["ID"], "0", "0", "0", "0" });
                }

                iRes = 1;
            }

            if (iRes == 1)
            {
                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = true;
                //btnOK.Enabled = true;
                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = true;
                //btnBreak.Enabled = true;
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
            Control ctrl = this.Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0];
            if (list_id.id_user.Equals(-1) == false)
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.User];
                tables[1] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                tables[2] = m_table_TEC;
                ((DataGridView_Prop_ComboBoxCell)ctrl).Update_dgv(list_id.id_user, tables);
                bIsRole = false;
                m_type_sel_node = TreeView_Users.Type_Comp.User;

            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                ((DataGridView_Prop_ComboBoxCell)ctrl).Update_dgv(list_id.id_role, tables);
                bIsRole = true;
                m_type_sel_node = TreeView_Users.Type_Comp.Role;
            }

            if (Convert.ToInt32(((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).SelectedNode.Level) == (int)ID_LEVEL.ROLE)
            {
                m_Level = ID_LEVEL.ROLE;
            }
            else
            {
                if (Convert.ToInt32(((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).SelectedNode.Level) == (int)ID_LEVEL.USER)
                {
                    m_Level = ID_LEVEL.USER;
                }
            }
        }

        /// <summary>
        /// Проверка критичных параметров перед сохранением
        /// </summary>
        /// <param name="mass_table">Таблица для проверки</param>
        /// <param name="warning">Строка с описанием ошибки</param>
        /// <returns>Возвращает переменную показывающую наличие не введенных параметров</returns>
        private bool validate_saving(DataTable[] mass_table, out string[] warning)
        {
            bool have = false;
            int indx = -1;
            warning = new String[mass_table.Length];

            foreach (DataTable table in mass_table)
            {
                indx++;
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (Convert.ToString(row[i]) == "-1")
                        {
                            have = true;
                            warning[indx] += "Для пользователя " + row["DESCRIPTION"] + " параметр " + table.Columns[i].ColumnName + " равен '-1'." + '\n';
                        }
                    }
                }
            }
            return have;
        }

        /// <summary>
        /// Обработчик события кнопки "Применить"
        /// </summary>
        private void buttonSAVE_click(object sender, EventArgs e)
        {
            //delegateReportClear(true);
            int err = -1;
            string[] warning;
            string keys = string.Empty;

            if (validate_saving(m_arr_editTable, out warning) == false)
            {
                for (ID_Table i = ID_Table.Unknown + 1; i < ID_Table.Count; i++)
                {
                    switch (i)
                    {
                        case ID_Table.Role:
                        case ID_Table.User:
                            keys = @"ID";
                            break;
                        case ID_Table.Profiles:
                            keys = @"ID_EXT,IS_ROLE,ID_TAB,ID_ITEM,CONTEXT,ID_UNIT";
                            break;
                        default:
                            break;
                    }

                    m_handlerDb.RecUpdateInsertDelete(getNameMode(i), keys, "",m_arr_origTable[(int)i], m_arr_editTable[(int)i], out err);
                }

                fillDataTable();
                resetDataTable();
                ((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = false;
                //btnOK.Enabled = false;
                ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = false;
                //btnBreak.Enabled = false;

            }
            else
            {
                //delegateWarningReport(warning[(int)ID_Table.Role] + warning[(int)ID_Table.User]);
                //MessageBox.Show(warning[0] + warning[1] + warning[2] + warning[3], "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //db_sostav.Write_Audit(m_table_audit);
        }

        /// <summary>
        /// Обработчик события кнопки "Отмена"
        /// </summary>
        private void buttonBreak_click(object sender, EventArgs e)
        {
            //delegateReportClear(true);
            resetDataTable();

            ((TreeView_Users)this.Controls.Find(INDEX_CONTROL.TREE_DICT_ITEM.ToString(), true)[0]).Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0])).Enabled = false;
            //btnOK.Enabled = false;
            ((Button)(this.Controls.Find(INDEX_CONTROL.BUTTON_BREAK.ToString(), true)[0])).Enabled = false;
            //btnBreak.Enabled = false;
        }

        /// <summary>
        /// Обрыботчик события выбора строки в таблице свойств
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="ev">Делегат</param>
        protected override void HPanelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
            string desc = string.Empty;
            string name = string.Empty;
            try
            {
                if (((DataGridView_Prop)obj).SelectedRows.Count > 0)
                {
                    if (((DataGridView_Prop)obj).SelectedRows[0].HeaderCell.Value != null)
                    {
                        name = ((DataGridView_Prop)obj).SelectedRows[0].HeaderCell.Value.ToString();
                        foreach (DataRow r in Descriptions[(int)ID_DT_DESC.PROP].Select("ID_TABLE=" + (int)m_Level))
                        {
                            if (name == r["PARAM_NAME"].ToString())
                            {
                                desc = r["DESCRIPTION"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            SetDescSelRow(desc, name);


        }
    }

    public class DataGridView_Prop : DataGridView
    {
        private void InitializeComponent()
        {
            this.Columns.Add("Значение", "Значение");
            this.ColumnHeadersVisible = true;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.RowHeadersVisible = true;
            this.AllowUserToAddRows = false;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            //this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MultiSelect = false;
            this.RowHeadersWidth = 250;
        }

        public DataGridView_Prop()
            : base()
        {
            InitializeComponent();//инициализация компонентов

            this.CellValueChanged += new DataGridViewCellEventHandler(this.cell_EndEdit);
        }

        /// <summary>
        /// Запрос на получение таблицы со свойствами
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public virtual void Update_dgv(int id_component, DataTable[] tables)
        {
            this.Rows.Clear();
            DataRow[] rowsSel = tables[0].Select(@"ID=" + id_component);

            if (rowsSel.Length == 1)
                foreach (DataColumn col in tables[0].Columns)
                {
                    this.Rows.Add(rowsSel[0][col.ColumnName]);
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString();
                }
            else
                Logging.Logg().Error(@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

            //cell_ID_Edit_ReadOnly();
        }

        /// <summary>
        /// Метод для присваивания ячейке свойства ReadOnly
        /// </summary>
        /// <param name="id_cell">id ячейки</param>
        private void cell_ID_Edit_ReadOnly()
        {
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].HeaderCell.Value.ToString()[0] == 'I' && this.Rows[i].HeaderCell.Value.ToString()[1] == 'D')
                {
                    this.Rows[i].Cells["Значение"].ReadOnly = true;
                    this.Rows[i].Cells["Значение"].ToolTipText = "Только для чтения";
                }
            }
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки
        /// </summary>
        protected virtual void cell_EndEdit(object sender, DataGridViewCellEventArgs e)
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
                                        , Rows[e.RowIndex].HeaderCell.Value.ToString() //Идентификатор компонента
                                        , Rows[e.RowIndex].Cells[0].Value.ToString() //Идентификатор параметра с учетом периода расчета
                                        ));
            }
            else
            {
                if (EventCellValueChanged != null)
                    EventCellValueChanged(this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs(n_row//Идентификатор компонента
                                        , Rows[e.RowIndex].HeaderCell.Value.ToString() //Идентификатор компонента
                                        , null //Идентификатор параметра с учетом периода расчета
                                        ));
            }
        }

        /// <summary>
        /// Класс для описания аргумента события - изменения значения ячейки
        /// </summary>
        public class DataGridView_Prop_ValuesCellValueChangedEventArgs : EventArgs
        {
            /// <summary>
            /// ID компонента
            /// </summary>
            public int m_IdComp;

            /// <summary>
            /// Имя изменяемого параметра
            /// </summary>
            public string m_Header_name;

            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string m_Value;

            public DataGridView_Prop_ValuesCellValueChangedEventArgs()
                : base()
            {
                m_IdComp = -1;
                m_Header_name = string.Empty;
                m_Value = string.Empty;

            }

            public DataGridView_Prop_ValuesCellValueChangedEventArgs(int id_comp, string header_name, string value)
                : this()
            {
                m_IdComp = id_comp;
                m_Header_name = header_name;
                m_Value = value;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - изменение значения в ячейке
        /// </summary>
        public delegate void DataGridView_Prop_ValuesCellValueChangedEventHandler(object obj, DataGridView_Prop_ValuesCellValueChangedEventArgs e);

        /// <summary>
        /// Событие - изменение значения ячейки
        /// </summary>
        public DataGridView_Prop_ValuesCellValueChangedEventHandler EventCellValueChanged;

    }

    public class DataGridView_Prop_ComboBoxCell : DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }
        /// <summary>
        /// Запрос на получение таблицы со свойствами и ComboBox
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            this.CellValueChanged -= cell_EndEdit;
            this.Rows.Clear();
            DataRow[] rowsSel = tables[(int)INDEX_TABLE.user].Select(@"ID=" + id_component);

            if (rowsSel.Length == 1)
                foreach (DataColumn col in tables[(int)INDEX_TABLE.user].Columns)
                {
                    if (col.ColumnName == "ID_ROLE")
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                        combo.AutoComplete = true;
                        row.Cells.Add(combo);
                        //combo.Items.Clear();
                        this.Rows.Add(row);
                        ArrayList roles = new ArrayList();
                        foreach (DataRow row_role in tables[(int)INDEX_TABLE.role].Rows)
                        {
                            roles.Add(new role(row_role["DESCRIPTION"].ToString(), row_role["ID"].ToString()));
                        }
                        combo.DataSource = roles;
                        combo.DisplayMember = "NameRole";
                        combo.ValueMember = "IdRole";
                        this.Rows[Rows.Count - 1].HeaderCell.Value = "ID_ROLE";
                        this.Rows[Rows.Count - 1].Cells[0].Value = rowsSel[0][col.ColumnName].ToString();
                    }
                    else
                        if (col.ColumnName == "ID_TEC")
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                            combo.AutoComplete = true;
                            row.Cells.Add(combo);
                            //combo.Items.Clear();
                            this.Rows.Add(row);
                            ArrayList TEC = new ArrayList();
                            foreach (DataRow row_tec in tables[(int)INDEX_TABLE.tec].Rows)
                            {
                                TEC.Add(new role(row_tec["DESCRIPTION"].ToString(), row_tec["ID"].ToString()));
                            }
                            TEC.Add(new role("Все ТЭЦ", "0"));
                            combo.DataSource = TEC;
                            combo.DisplayMember = "NameRole";
                            combo.ValueMember = "IdRole";
                            this.Rows[Rows.Count - 1].HeaderCell.Value = "ID_TEC";
                            this.Rows[Rows.Count - 1].Cells[0].Value = rowsSel[0][col.ColumnName].ToString();
                        }

                        else
                        {
                            this.Rows.Add(rowsSel[0][col.ColumnName]);
                            this.Rows[this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString();
                        }
                }
            else
                Logging.Logg().Error(@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

            this.CellValueChanged += cell_EndEdit;
            //cell_ID_Edit_ReadOnly();
        }

        public class role
        {
            private string Name;
            private string ID;

            public role(string name, string id)
            {

                this.Name = name;
                this.ID = id;
            }

            public string NameRole
            {
                get
                {
                    return Name;
                }
            }

            public string IdRole
            {

                get
                {
                    return ID;
                }
            }

        }

    }

    public class TreeView_Users : TreeView
    {
        #region Переменные

        /// <summary>
        /// Идентификаторы для типов объектов
        /// </summary>
        public enum ID_OBJ : int { Role = 0, User };
        private bool m_b_visible_context_menu;
        string m_warningReport;

        public struct ID_Comp
        {
            public int id_role;
            public int id_user;
            public Type_Comp type;
        }


        ID_Comp m_selNode_id;

        public enum Type_Comp : int { Role, User }

        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Operation : int { Insert = 0, Delete, Update, Select }

        /// <summary>
        /// Возвратить наименование операции
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        protected static string getNameOperation(Int16 indx)
        {
            string[] nameModes = { "Insert", "Delete", "Update", "Select" };

            return nameModes[indx];
        }

        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Menu : int { AddRole = 0, AddUser, Delete }


        List<string> m_open_node = new List<string>();
        string selected_user = string.Empty;

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenu_TreeView;

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            System.Windows.Forms.ToolStripMenuItem добавитьРольToolStripMenuItem;

            contextMenu_TreeView = new System.Windows.Forms.ContextMenuStrip();
            добавитьРольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuStrip = contextMenu_TreeView;

            #region Context add TEC
            // 
            // contextMenu_TreeView
            // 
            this.contextMenu_TreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            добавитьРольToolStripMenuItem});
            this.contextMenu_TreeView.Name = "contextMenu_TreeView";
            this.contextMenu_TreeView.Visible = m_b_visible_context_menu;
            // 
            // добавитьТЭЦToolStripMenuItem
            // 
            добавитьРольToolStripMenuItem.Name = "добавитьРольToolStripMenuItem";
            добавитьРольToolStripMenuItem.Text = "Добавить роль";
            #endregion

            this.HideSelection = false;
        }

        public TreeView_Users(bool context_visible = true)
            : base()
        {
            m_b_visible_context_menu = context_visible;

            InitializeComponent();

            this.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.tree_NodeClick);
            this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_Role);

            this.AfterSelect += new TreeViewEventHandler(this.tree_NodeSelect);
        }

        /// <summary>
        /// Возвратить наименование компонента контекстного меню
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        protected static string getNameMode(int indx)
        {
            string[] nameModes = { "Добавить роль", "Добавить пользователя", "Удалить" };

            return nameModes[indx];
        }

        /// <summary>
        /// Для возвращения имена по умолчанию для компонентов
        /// </summary>
        /// <param name="indx">Идентификатор типа компонента</param>
        /// <returns>Имя по умолчанию</returns>
        public static string Mass_NewVal_Comp(int indx)
        {
            String[] arPREFIX_COMPONENT = { "Новая роль", "Новый пользователь" };

            return arPREFIX_COMPONENT[indx];
        }

        /// <summary>
        /// Метод для сохранения открытых элементов дерева при обновлении
        /// </summary>
        /// <param name="node">Массив Node с которых нужно начать</param>
        /// <param name="i">Начальное значение счетчика</param>
        /// <param name="set_check">Флаг для установки значений</param>
        private void checked_node(TreeNodeCollection node, int i, bool set_check = false)
        {
            if (set_check == false)
            {
                if (this.SelectedNode != null)
                {
                    selected_user = this.SelectedNode.Text;
                }
                foreach (TreeNode n in node)
                {
                    if (n.IsExpanded == true)
                    {
                        m_open_node.Add(n.Name);
                        if (n.FirstNode != null)
                            checked_node(n.Nodes, i);
                    }
                }
            }
            if (set_check == true)
            {
                foreach (TreeNode n in node)
                {
                    if (m_open_node.Count > 0 & i < m_open_node.Count)

                        if (m_open_node[i] == n.Name)
                        {
                            n.Expand();
                            i++;
                            if (n.FirstNode != null)
                                checked_node(n.Nodes, i, true);
                        }

                    if (n.Text == selected_user)
                    {
                        this.SelectedNode = n;
                    }
                }
            }
        }

        /// <summary>
        /// Заполнение TreeView компонентами
        /// </summary>
        public void Update_tree(DataTable table_users, DataTable table_role)
        {
            checked_node(this.Nodes, 0, false);

            this.Nodes.Clear();
            int num_node = 0;
            foreach (DataRow r in table_role.Rows)
            {
                Nodes.Add(r["DESCRIPTION"].ToString());
                num_node = Nodes.Count - 1;
                Nodes[num_node].Name = r["ID"].ToString();
                DataRow[] rows = table_users.Select("ID_ROLE=" + r["ID"].ToString());
                foreach (DataRow r_u in rows)
                {
                    Nodes[num_node].Nodes.Add(r_u["DESCRIPTION"].ToString());
                    Nodes[num_node].Nodes[Nodes[num_node].Nodes.Count - 1].Name = Nodes[num_node].Name + ":" + r_u["ID"].ToString();
                }
            }
            this.SelectedNode = this.Nodes[1];
            this.SelectedNode = this.Nodes[0];
            checked_node(this.Nodes, 0, true);

            //foreach (TreeNode n in this.Nodes)
            //{
            //    if (n.IsExpanded == true)
            //    {
            //        this.SelectedNode = n;
            //    }
            //}
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void tree_NodeSelect(object sender, TreeViewEventArgs e)
        {
            int idComp = 0;
            m_selNode_id = get_m_id_list(e.Node.Name);

            if (m_selNode_id.type == Type_Comp.Role)
                idComp = m_selNode_id.id_role;
            if (m_selNode_id.type == Type_Comp.User)
                idComp = m_selNode_id.id_user;

            if (EditNode != null)
                EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Select, idComp));
        }

        /// <summary>
        /// Метод для переименования ноды
        /// </summary>
        /// <param name="id_comp"></param>
        /// <param name="name"></param>
        public void Rename_Node(ID_Comp id_comp, string name)
        {
            if (id_comp.id_user.Equals(-1) == true & id_comp.id_role.Equals(-1) == false)
            {
                foreach (TreeNode n in this.Nodes)
                {
                    if (get_m_id_list(n.Name).id_role == id_comp.id_role)
                    {
                        n.Text = name;
                    }
                }
            }
            if (id_comp.id_user.Equals(-1) == false)
            {
                foreach (TreeNode n in this.Nodes)
                {
                    if (get_m_id_list(n.Name).id_role == id_comp.id_role)
                    {
                        foreach (TreeNode u in n.Nodes)
                        {
                            if (get_m_id_list(u.Name).id_user == id_comp.id_user)
                            {
                                u.Text = name;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод для запроса ID компонента в TreeView
        /// </summary>
        /// <param name="id_string">Строка с идентификаторами</param>
        /// <returns>Список с ID</returns>
        private ID_Comp get_m_id_list(string id_string)
        {
            ID_Comp id_comp = new ID_Comp();
            id_comp.id_role = -1;
            id_comp.id_user = -1;

            if (id_string != "")
            {
                string[] path = id_string.Split(':');
                if (path.Length == 2)
                {
                    id_comp.id_user = Convert.ToInt32(path[1].Trim());
                    id_comp.type = Type_Comp.User;
                }
                else
                {
                    id_comp.id_user = -1;
                    id_comp.type = Type_Comp.Role;
                }

                id_comp.id_role = Convert.ToInt32(path[0].Trim());

            }
            return id_comp;
        }

        /// <summary>
        /// Обработчик добавления новой роли
        /// </summary>
        private void add_New_Role(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Report != null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            int id_newRole = 0;
            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.AddRole))
            {
                this.Nodes.Add(Mass_NewVal_Comp((int)ID_OBJ.Role));

                if (GetID != null)
                    id_newRole = GetID(this, new GetIDEventArgs(m_selNode_id, (int)ID_OBJ.Role));

                Nodes[Nodes.Count - 1].Name = Convert.ToString(id_newRole);

                ID_Comp id = new ID_Comp();

                id.id_role = -1;
                id.id_user = -1;

                id.id_role = id_newRole;

                if (EditNode != null)
                    EditNode(this, new EditNodeEventArgs(id, ID_Operation.Insert, id.id_role));
            }
        }

        /// <summary>
        /// Обработчик добавления нового пользователя
        /// </summary>
        private void add_New_User(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Report != null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.AddUser))//Добавление нового пользователя
            {
                int id_newUser = 0;

                ID_Comp id = new ID_Comp();

                id.id_role = -1;
                id.id_user = -1;

                if (GetID != null)
                    id_newUser = GetID(this, new GetIDEventArgs(m_selNode_id, (int)ID_OBJ.User));

                id.id_role = m_selNode_id.id_role;
                id.id_user = id_newUser;

                if (EditNode != null)
                    EditNode(this, new EditNodeEventArgs(id, ID_Operation.Insert, id.id_user));

                foreach (TreeNode role in Nodes)
                {
                    if (Convert.ToInt32(role.Name) == m_selNode_id.id_role)
                    {
                        role.Nodes.Add(Mass_NewVal_Comp((int)ID_OBJ.User));
                        role.Nodes[role.Nodes.Count - 1].Name = Convert.ToString(id.id_role) + ":" + Convert.ToString(id_newUser);
                    }
                }
            }
            else
            {
                if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.Delete))//Удаление роли
                {
                    bool del = false;


                    if (SelectedNode.FirstNode == null)
                    {
                        del = true;
                    }
                    if (del == true)
                    {
                        if (EditNode != null)
                            EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Delete, m_selNode_id.id_role));

                        SelectedNode.Remove();
                    }
                    else
                    {
                        m_warningReport = "У роли " + SelectedNode.Text + " имеются пользователи!";
                        if (Report != null)
                            Report(this, new ReportEventArgs(string.Empty, string.Empty, m_warningReport, false));
                        //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text,"Внимание!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик удаления пользователя
        /// </summary>
        private void del_user(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Report != null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.Delete))//Удаление роли
            {
                if (EditNode != null)
                    EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Delete, m_selNode_id.id_user));

                SelectedNode.Remove();
            }
        }

        /// <summary>
        /// Обработчик события нажатия на элемент в TreeView
        /// </summary>
        private void tree_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            System.Windows.Forms.ContextMenuStrip contextMenu_TreeNode = new System.Windows.Forms.ContextMenuStrip();

            System.Windows.Forms.ToolStripMenuItem УдалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem добавитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            #region Нажатие правой кнопкой мыши

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши

                #region Добавление компонентов

                if (m_selNode_id.id_user != -1)//выбран ли элемент пользователь
                {
                    #region Context delete TG
                    // 
                    // contextMenu_TreeNode
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                УдалитьToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // УдалитьToolStripMenuItem
                    //
                    УдалитьToolStripMenuItem.Name = "УдалитьToolStripMenuItem";
                    УдалитьToolStripMenuItem.Text = "Удалить";
                    #endregion

                    this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.del_user);
                }

                if (m_selNode_id.id_user == -1 & m_selNode_id.id_role != -1)//Выбрана ли роль
                {
                    #region Добавление в ТЭЦ компонентов

                    #region Context TEC
                    // 
                    // contextMenu_TreeView_TEC
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    добавитьПользователяToolStripMenuItem,
                    УдалитьToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // добавитьПользователяToolStripMenuItem
                    // 
                    добавитьПользователяToolStripMenuItem.Name = "добавитьПользователяToolStripMenuItem";
                    добавитьПользователяToolStripMenuItem.Text = "Добавить пользователя";
                    // 
                    // УдалитьToolStripMenuItem
                    // 
                    УдалитьToolStripMenuItem.Name = "УдалитьToolStripMenuItem";
                    УдалитьToolStripMenuItem.Text = "Удалить";
                    #endregion

                    this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                    this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_User);

                    #endregion
                }

                #endregion
            }

            #endregion
        }


        /// <summary>
        /// Класс для описания аргумента события - изменения компонента
        /// </summary>
        public class EditNodeEventArgs : EventArgs
        {
            /// <summary>
            /// Список ID компонента
            /// </summary>
            public ID_Comp PathComp;

            /// <summary>
            /// Тип производимой операции
            /// </summary>
            public ID_Operation Operation;

            public int IdComp;

            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string Value;

            public EditNodeEventArgs(ID_Comp pathComp, ID_Operation operation, int idComp, string value = null)
            {
                PathComp = pathComp;
                IdComp = idComp;
                Operation = operation;
                Value = value;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - изменение компонента
        /// </summary>
        public delegate void EditNodeEventHandler(object obj, EditNodeEventArgs e);

        /// <summary>
        /// Событие - редактирование компонента
        /// </summary>
        public event EditNodeEventHandler EditNode;


        /// <summary>
        /// Класс для описания аргумента события - получение ID компонента
        /// </summary>
        public class GetIDEventArgs : EventArgs
        {
            /// <summary>
            /// Список ID компонента
            /// </summary>
            public ID_Comp PathComp;

            /// <summary>
            /// ID компонента
            /// </summary>
            public int IdComp;

            public GetIDEventArgs(ID_Comp path, int id_comp)
            {
                PathComp = path;
                IdComp = id_comp;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение ID компонента
        /// </summary>
        public delegate int intGetID(object obj, GetIDEventArgs e);

        /// <summary>
        /// Событие - получение ID компонента
        /// </summary>
        public intGetID GetID;


        /// <summary>
        /// Класс для описания аргумента события - передачи сообщения для строки состояния
        /// </summary>
        public class ReportEventArgs : EventArgs
        {
            /// <summary>
            /// ID компонента
            /// </summary>
            public string Action;

            public string Error;

            public string Warning;

            public bool Clear;

            public ReportEventArgs(string action, string error, string warning, bool clear)
            {
                Action = action;
                Error = error;
                Warning = warning;
                Clear = clear;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение репорта
        /// </summary>
        public delegate void ReportEventHandler(object obj, ReportEventArgs e);

        /// <summary>
        /// Событие - получение репорта
        /// </summary>
        public ReportEventHandler Report;
    }

}
