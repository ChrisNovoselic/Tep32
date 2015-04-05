using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public partial class HPanelEditTree : HPanelTepCommon
    {
        private enum INDEX_LEVEL { TASK, N_ALG, TIME, COMP };
        
        private enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE
            , TREECTRL_PRJ_ALG
            , DGV_PRJ_PPOP
            , DGV_PRJ_DETAIL
            , LABEL_PARAM_DESC
            , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        private INDEX_LEVEL _level;
        private INDEX_LEVEL m_Level
        {
            get { return _level; }

            set { if (!(_level == value)) levelChanged (value); else ; }
        }
        private void levelChanged(INDEX_LEVEL newLevel)
        {
            DataGridView dgv;
            //Очистить список "детализации"
            dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL]);
            dgv.Rows.Clear();

            switch (_level)
            {
                case INDEX_LEVEL.TASK:
                    switch (newLevel)
                    {
                        //case INDEX_LEVEL.TASK:
                        //    break;
                        case INDEX_LEVEL.N_ALG:
                            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP] as DataGridView;
                            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 5);
                            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                            this.Controls.Add(dgv, 5, 5);
                            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 5);
                            break;
                        case INDEX_LEVEL.TIME:
                            break;
                        case INDEX_LEVEL.COMP:
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_LEVEL.N_ALG:
                    switch (newLevel)
                    {
                        case INDEX_LEVEL.TASK:
                            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                            this.Controls.Remove(dgv);
                            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP] as DataGridView;
                            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 10);
                            break;
                        //case INDEX_LEVEL.N_ALG:
                        //    break;
                        case INDEX_LEVEL.TIME:
                            break;
                        case INDEX_LEVEL.COMP:
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_LEVEL.TIME:
                    switch (newLevel)
                    {
                        case INDEX_LEVEL.TASK:
                            break;
                        case INDEX_LEVEL.N_ALG:
                            break;
                        //case INDEX_LEVEL.TIME:
                        //    break;
                        case INDEX_LEVEL.COMP:
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_LEVEL.COMP:
                    switch (newLevel)
                    {
                        case INDEX_LEVEL.TASK:
                            break;
                        case INDEX_LEVEL.N_ALG:
                            break;
                        case INDEX_LEVEL.TIME:
                            break;
                        //case INDEX_LEVEL.COMP:
                        //    break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }            

            _level = newLevel;
        }
        
        protected TreeView m_ctrlTreeView
        {
            get { return m_dictControls[(int)INDEX_CONTROL.TREECTRL_PRJ_ALG] as TreeView; }
        }

        public HPanelEditTree(IPlugIn plugIn)
            : base(plugIn)
        {
            m_arNameTables = new string[] { @"inalg", @"input" };

            m_arTableOrigin = new DataTable[(int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER];
            m_arTableEdit = new DataTable[(int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER];
            m_arTableKey = new DataTable[(int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY];
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton((int)i, m_arButtonText[(int)i]);

            //Добавить "список" словарных величин
            i = INDEX_CONTROL.TREECTRL_PRJ_ALG;
            m_dictControls.Add((int)i, new TreeView());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(m_dictControls[(int)i], 1, 0);
            this.SetColumnSpan(m_dictControls[(int)i], 4); this.SetRowSpan(m_dictControls[(int)i], 13);            
            m_ctrlTreeView.HideSelection = false;
            //m_ctrlTreeView.FullRowSelect = true;
            m_ctrlTreeView.AfterSelect += new TreeViewEventHandler(TreeView_AfterSelect);

            //Добавить "список" свойств словарной величины
            DataGridView dgv;
            i = INDEX_CONTROL.DGV_PRJ_PPOP;
            m_dictControls.Add((int)i, new DataGridView());
            dgv = m_dictControls[(int)i] as DataGridView;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 5, 0);
            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 10);
            //Добавить столбцы
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewTextBoxColumn ()
                });
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //1-ый столбец
            dgv.Columns[0].HeaderText = @"Свойство"; ((DataGridView)m_dictControls[(int)i]).Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //2-ой столбец
            dgv.Columns[1].HeaderText = @"Значение";
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //Создать "список" дополн./парамеиров (TIME, COMP)
            i = INDEX_CONTROL.DGV_PRJ_DETAIL;
            m_dictControls.Add((int)i, new DataGridView());
            dgv = m_dictControls[(int)i] as DataGridView;
            dgv.Dock = DockStyle.Fill;
            //Добавить столбцы
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewCheckBoxColumn ()
                });
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //1-ый столбец (только "для чтения")
            dgv.Columns[0].HeaderText = @"Свойство"; dgv.Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //2-ой столбец
            dgv.Columns[1].HeaderText = @"Наличие";
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            addLabelDesc((int)INDEX_CONTROL.LABEL_PARAM_DESC);

            this.ResumeLayout(false);

            //Обработчика нажатия кнопок
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_ADD]).Click += new System.EventHandler(HPanelEditTree_btnAdd_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_DELETE]).Click += new System.EventHandler(HPanelEditTree_btnDelete_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_SAVE]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_UPDATE]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }        

        protected override void initialize(ref DbConnection dbConn, out int err, out string strErr)
        {
            int i = -1;
            
            err = 0;
            strErr = string.Empty;

            //Заполнить оригинальные таблицы из БД...
            for (i = 0; i < (int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER; i ++)
            {
                m_arTableOrigin[i] = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + m_arNameTables[i], null, null, out err);

                if (!(err == 0))
                {
                    strErr = @"HPanelEditTree::initialize () - заполнение таблицы с параметрами ";

                    if (i == (int)INDEX_PARAMETER.ALGORITM)
                        strErr += @"АЛГоритма";
                    else
                        ;

                    strErr += @"...";

                    break;
                }
                else
                    ;
            }

            if (err == 0)
            {
                string[] arNameTableKey = new string[(int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY] { @"time", @"comp_list", @"task" }
                    , arErrKey = new string[(int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY] { @"словарь 'интервалы времени'"
                                                                                        , @"словарь 'компоненты станции'"
                                                                                        , @"проект 'список задач ПК'" };
                for (i = 0; i < (int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY; i++)
                {
                    m_arTableKey[(int)i] = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + arNameTableKey[(int)i], null, null, out err);

                    if (!(m_arTableKey[(int)i].Rows.Count > 0))
                        err = -1;
                    else
                        ;

                    if (!(err == 0))
                    {
                        strErr = @"HPanelEditTree::initialize () - заполнение таблицы " + arErrKey[i] + @"...";

                        break;
                    }
                    else
                        ;
                }

            }
            else
                ; //Строка с описанием ошибки заполнена

            if (err == 0)
            {//Только если обе выборки  рез-м = 0 (УСПЕХ)
                //Копии оригинальных таблиц для редактирования и последующего сравнения с оригигальными...
                m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM] = m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Copy();
                m_arTableEdit[(int)INDEX_PARAMETER.PUT] = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Copy();

                TreeNode nodeTask
                    , nodeAlg
                    , nodeTime
                    , nodePut;
                DataRow[] rowAlgs
                    , rowPuts;
                string strIdTask = string.Empty
                    , strIdAlg = string.Empty
                    , strIdTime = string.Empty
                    , strIdPut = string.Empty;

                //Заполнить "дерево" элементами 1-го уровня (ALGORITM)
                foreach (DataRow rTask in m_arTableKey[(int)INDEX_TABLE_KEY.TASK].Rows)
                {
                    strIdTask = rTask[@"ID"].ToString().Trim();
                    nodeTask = m_ctrlTreeView.Nodes.Add(strIdTask, rTask[@"DESCRIPTION"].ToString().Trim());

                    rowAlgs = m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID_TASK=" + rTask[@"ID"]);

                    //Заполнить "дерево" элементами 1-го уровня (ALGORITM)
                    if (rowAlgs.Length > 0)
                    {
                        foreach (DataRow rAlg in rowAlgs)
                        {
                            strIdAlg = rAlg[@"ID"].ToString().Trim();
                            nodeAlg = nodeTask.Nodes.Add(getIdNode(nodeTask, strIdAlg), rAlg[@"N_ALG"].ToString().Trim());

                            foreach (DataRow rTime in m_arTableKey[(int)INDEX_TABLE_KEY.TIME].Rows)
                            {
                                rowPuts = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + strIdAlg + @" AND ID_TIME=" + rTime[@"ID"]);
                                if (rowPuts.Length > 0)
                                {
                                    strIdTime = rTime[@"ID"].ToString().Trim();
                                    nodeTime = nodeAlg.Nodes.Add(getIdNode(nodeAlg, strIdTime), rTime[@"DESCRIPTION"].ToString().Trim());
                                    foreach (DataRow rPut in rowPuts)
                                    {
                                        strIdPut = rPut[@"ID"].ToString().Trim();
                                        nodePut = nodeTime.Nodes.Add(getIdNode(nodeTime, strIdPut), m_arTableKey[(int)INDEX_TABLE_KEY.COMP_LIST].Select(@"ID_COMP=" + rPut[@"ID_COMP"])[0][@"NAME_SHR"].ToString().Trim());
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                    }
                    else
                    {
                        addNodeNull(nodeTask);                        
                    }
                }

                m_ctrlTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);
                
                DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP]);
                //Обработчик события "Выбор строки"
                dgv.SelectionChanged += new EventHandler(HPanelEdit_dgvPropSelectionChanged);
                //Только для чтения
                dgv.ReadOnly = true;

                dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL]);
                //Обработчик события "Выбор строки"
                dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjDetailSelectionChanged);
                dgv.Columns[0].ReadOnly = true;
                dgv.CellEndEdit += new DataGridViewCellEventHandler (HPanelEditTree_dgvPrjDetailCellEndEdit);

                m_Level = INDEX_LEVEL.TASK;
                m_ctrlTreeView.SelectedNode = m_ctrlTreeView.Nodes[0];
            }
            else
                ; //Строка с описанием ошибки заполнена
        }

        protected override void Activate(bool activate)
        {
        }

        protected override void clear()
        {
            m_ctrlTreeView.Nodes.Clear();
            
            base.clear();
        }

        private static string getIdNode (TreeNode nodeParent, string id)
        {
            return nodeParent.Name + @"::" + id;
        }

        private static string getIdNode(string id, INDEX_LEVEL lev)
        {
            if (id.Equals(string.Empty) == false)
                return id.Split(new string[] { @"::" }, StringSplitOptions.None)[(int)lev];
            else
                return string.Empty;
        }

        private void addNodeNull(TreeNode nodeParent)
        {
            nodeParent.Nodes.Add(null, @"Параметры отсутствуют...");
        }

        private void HPanelEditTree_btnAdd_Click(object obj, EventArgs ev)
        {
            TreeNode nodeSel = m_ctrlTreeView.SelectedNode;

            if (!(nodeSel == null))
            {
                int level = nodeSel.Level;

                switch (level)
                {
                    case (int)INDEX_LEVEL.TASK:
                        int id = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM]);
                        if (id == 0) id += 10001; else ;
                        m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Add(new object [] {
                             id
                            , @"НаимКраткое"
                            , @"НаимПолное"
                            , "НомАлгоритм"
                            , @"Описание_параметра..."
                            , 0
                            , Int32.Parse(nodeSel.Name)
                        });
                        //Удалить элемент 'Параметры отсутствуют...'
                        if ((nodeSel.Nodes.Count == 1)
                            && ((nodeSel.Nodes[0].Name == null) || (nodeSel.Nodes[0].Name.Equals (string.Empty) == true)))
                            nodeSel.Nodes.RemoveAt(0);
                        else
                            ;

                        TreeNode nodeAdd = nodeSel.Nodes.Add(getIdNode (nodeSel, id.ToString ())
                            , m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows[m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Count - 1][@"N_ALG"].ToString ().Trim ());
                        //nodeAdd.La
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        private void HPanelEditTree_btnDelete_Click(object obj, EventArgs ev)
        {
            TreeNode nodeSel = m_ctrlTreeView.SelectedNode
                , nodeParent = nodeSel.Parent;

            if (!(nodeSel == null))
            {
                INDEX_LEVEL level = (INDEX_LEVEL)nodeSel.Level;

                switch (level)
                {
                    case INDEX_LEVEL.TASK:                        
                        break;
                    case INDEX_LEVEL.N_ALG:                        
                        m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Remove(m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNode(nodeSel.Name, level))[0]);
                        nodeParent.Nodes.Remove(nodeSel);

                        if (nodeParent.Nodes.Count == 0)
                            addNodeNull(nodeParent);
                        else
                            ;
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }
    }

    partial class HPanelEditTree
    {
        private enum INDEX_PARAMETER {ALGORITM, PUT, COUNT_INDEX_PARAMETER};
        private enum INDEX_TABLE_KEY { TIME, COMP_LIST, TASK, COUNT_INDEX_TABLE_KEY };
        string[] m_arNameTables;
        private DataTable [] m_arTableOrigin
            , m_arTableEdit;
        private DataTable [] m_arTableKey;

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            err = 0;

            for (INDEX_PARAMETER i = INDEX_PARAMETER.ALGORITM; i < INDEX_PARAMETER.COUNT_INDEX_PARAMETER; i++)
            {
                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn
                                            , m_arNameTables[(int)i]
                                            , @"ID"
                                            , m_arTableOrigin[(int)i]
                                            , m_arTableEdit[(int)i]
                                            , out err);

                if (!(err == 0))
                    break;
                else
                    ;
            }
        }

        protected override void successRecUpdateInsertDelete()
        {
            //throw new NotImplementedException();
        }

        private void TreeView_AfterSelect(object obj, TreeViewEventArgs ev)
        {
            int iRes = -1;
            TreeViewAction act = ev.Action;
            TreeNode nodeSel = ev.Node;

            m_Level = (INDEX_LEVEL)ev.Node.Level;

            switch (ev.Node.Level)
            {
                case (int)INDEX_LEVEL.TASK: //Задача
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TASK], INDEX_LEVEL.TASK, true);
                    break;
                case (int)INDEX_LEVEL.N_ALG: //Параметр алгоритма
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], INDEX_LEVEL.N_ALG, false);
                    if (iRes == 0)
                    {
                        //Заполнить список детализации
                        DataRow[] rowsPut;
                        DataGridView dgv;
                        dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                        foreach (DataRow rTime in m_arTableKey[(int)INDEX_TABLE_KEY.TIME].Rows)
                        {
                            rowsPut = m_arTableEdit[(int)INDEX_PARAMETER.PUT].Select(@"ID_TIME=" + rTime[@"ID"]);

                            dgv.Rows.Add(new object[] { rTime[@"DESCRIPTION"], rowsPut.Length > 0 });
                        }
                    }
                    else
                        ; //Выбран "некорректный" элемент
                    break;
                case (int)INDEX_LEVEL.TIME:
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TIME], (INDEX_LEVEL)ev.Node.Level, false);
                    break;
                default:
                    break;
            }            
        }

        private int nodeAfterSelect(TreeNode node, DataTable tblProp, INDEX_LEVEL level, bool bThrow)
        {
            int iErr = 0;
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP]);
            dgv.Rows.Clear();
            string strIdNode = getIdNode(node.Name, level);
            if (strIdNode.Equals(string.Empty) == false)
            {
                DataRow[] rowsProp = tblProp.Select(@"ID=" + strIdNode);
                if (rowsProp.Length == 1)
                {
                    //Заполнение содержимым...
                    foreach (DataColumn col in tblProp.Columns)
                        dgv.Rows.Add(new object[] { col.ColumnName, rowsProp[0][col.ColumnName].ToString().Trim() });
                }
                else
                    iErr = -1;
            }
            else
                iErr = -2;

            if (bThrow == true)
                switch (iErr)
                {
                    case -1:
                        throw new Exception(@"HPanelEditTree::nodeAfterSelect () - отсутствие (дублирование) записи...");
                    case -2:
                        throw new Exception(@"HPanelEditTree::nodeAfterSelect () - отсутствкет идентификатор...");
                    default:
                        break;
                }
            else
                ;

            return iErr;
        }

        private void TreeView_AfterLabelEdit(object obj, NodeLabelEditEventArgs ev)
        {
            switch (ev.Node.Level)
            {
                case (int)INDEX_LEVEL.TASK: //Задача - не редактируется
                case (int)INDEX_LEVEL.TIME: //Интервал времени - не редактируется
                case (int)INDEX_LEVEL.COMP: //Компонент станции - не редактируется
                    ev.CancelEdit = true;
                    break;
                case (int)INDEX_LEVEL.N_ALG: //Параметр алгоритма
                    DataRow[] rowsProp = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNode (ev.Node.Name, (INDEX_LEVEL)ev.Node.Level));
                    break;
                default:
                    break;
            }
        }

        private void HPanelEditTree_dgvPrjDetailSelectionChanged(object obj, EventArgs ev)
        {
        }

        private void HPanelEditTree_dgvPrjDetailCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            DataGridView dgv;
            DataRow[] rowsTime;
            string strTime = string.Empty
                , strIdTime = string.Empty;
            //dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
            dgv = obj as DataGridView;
            strTime = dgv.Rows[ev.RowIndex].Cells[0].Value.ToString ().Trim();            
            //???
            rowsTime = m_arTableKey[(int)INDEX_TABLE_KEY.TIME].Select(@"DESCRIPTION='" + strTime + @"'");
            if (rowsTime.Length == 1)
            {
                strIdTime = getIdNode(m_ctrlTreeView.SelectedNode, rowsTime[0][@"ID"].ToString().Trim());

                if (bool.Parse(dgv.Rows[ev.RowIndex].Cells[1/*ev.ColumnIndex*/].Value.ToString()) == true)
                {
                    if (m_ctrlTreeView.SelectedNode.Nodes.IndexOfKey(strIdTime) < 0)
                        m_ctrlTreeView.SelectedNode.Nodes.Add(strIdTime, strTime);
                    else
                        ;

                    if (m_ctrlTreeView.SelectedNode.IsExpanded == false)
                        m_ctrlTreeView.SelectedNode.Expand();
                    else
                        ;
                }
                else
                    m_ctrlTreeView.SelectedNode.Nodes.RemoveByKey(strIdTime);
            }
            else
                throw new Exception(@"HPanelEditTree::HPanelEditTree_dgvPrjDetailCellEndEdit () - отсутствие(дублирование) интервала времени...");
        }
    }
}
