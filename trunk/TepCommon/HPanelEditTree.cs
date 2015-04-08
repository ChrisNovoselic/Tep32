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
        //Перечисление для индексироания уровней "дерева"
        private enum INDEX_LEVEL { TASK /*Задача*/, N_ALG /*Параметр алгоритма*/, TIME /*Интервал времени*/, COMP /*Компонент станции*/
            , PUT };

        //Перечисление для индексирования элементов управления на панели
        private enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE //Кнопки
            , TREECTRL_PRJ_ALG //"дерево"
            , DGV_PRJ_PPOP //свойства элемента "дерева"
            , DGV_PRJ_DETAIL //детализация свойств элемкента "дерева"
            , LABEL_PARAM_DESC //Описание
            , INDEX_CONTROL_COUNT, //Общее кол-во элементов
        };
        //Надписи для кнопок
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        //Текущий(выбранный) уровень "дерева"
        private INDEX_LEVEL _level;
        private INDEX_LEVEL m_Level
        {
            get { return _level; }

            set { if (!(_level == value)) levelChanged (value); else ; }
        }
        /// <summary>
        /// "Обработка" события - изменение значения уровня "дерева"
        /// </summary>
        /// <param name="newLevel"></param>
        private void levelChanged(INDEX_LEVEL newLevel)
        {
            DataGridView dgv;
            //Очистить список "детализации"
            clearPrjDetail();

            int iShowDetail = 0;

            switch (_level)
            {
                case INDEX_LEVEL.TASK:
                    switch (newLevel)
                    {
                        //case INDEX_LEVEL.TASK:
                        //    break;
                        case INDEX_LEVEL.N_ALG: //Параметр алгоритма
                        case INDEX_LEVEL.TIME:
                            iShowDetail = 1;
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
                        case INDEX_LEVEL.COMP:
                            iShowDetail = -1;
                            break;
                        //case INDEX_LEVEL.N_ALG:
                        //    break;
                        case INDEX_LEVEL.TIME:
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_LEVEL.TIME:
                    switch (newLevel)
                    {
                        case INDEX_LEVEL.TASK:
                        case INDEX_LEVEL.COMP:
                            iShowDetail = -1;
                            break;
                        case INDEX_LEVEL.N_ALG:
                            break;
                        //case INDEX_LEVEL.TIME:
                        //    break;
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
                        case INDEX_LEVEL.TIME:
                            iShowDetail = 1;
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

            if (iShowDetail == 1)
            {
                //Уменьшить кол-во строк для 'DataGridView' c ID = DGV_PRJ_PPOP
                dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP] as DataGridView;
                this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 5);
                //Размесить 'DataGridView' c ID = DGV_PRJ_DETAIL
                dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                this.Controls.Add(dgv, 5, 5);
                this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 5);
            }
            else
                if (iShowDetail == -1)
                {
                    //Удалить с панели 'DataGridView' c ID = DGV_PRJ_DETAIL
                    dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                    this.Controls.Remove(dgv);
                    //Увеличить кол-во строк для 'DataGridView' c ID = DGV_PRJ_PPOP
                    dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP] as DataGridView;
                    this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 10);
                }
                else
                    ;

            _level = newLevel;
        }

        //Идентификатор текущего(выбранного) параметра алгоритма
        private int _idAlg;
        private int m_idAlg {
            get { return _idAlg; }

            set { if (!(_idAlg == value)) idAlgChanged(value); else ; }
        }

        private void idAlgChanged(int newIdAlg)
        {
            if (_idAlg > 0)
            {
                DataRow []rowsAlg = m_arTableEdit [(int)INDEX_PARAMETER.ALGORITM].Select (@"ID=" + _idAlg);                

                if (rowsAlg.Length == 1)
                {
                    List<string> toDeleteKeys = new List<string> ();
                    try
                    {
                        TreeNode prevNode = m_ctrlTreeView.Nodes.Find(rowsAlg[0][@"ID_TASK"].ToString().Trim() + @"::" + _idAlg, true)[0]
                            , node = prevNode.FirstNode;
                        while (!(node == null))
                        {
                            if (node.GetNodeCount(false) == 0)
                            {
                                toDeleteKeys.Add (node.Name);
                            }
                            else
                                ;

                            node = node.NextNode;
                        }

                        while (toDeleteKeys.Count > 0)
                        {
                            prevNode.Nodes.RemoveByKey(toDeleteKeys[0]);
                            toDeleteKeys.RemoveAt(0);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HPanelEditTree::idAlgChanged () - ...");
                    }
                }
                else
                    throw new Exception(@"HPanelEditTree::idAlgChanged () - отсутствие(дублирование) параметра... [ID=" + _idAlg + @"]");
            }
            else
                ; //Предыдущий параметр НЕ "действительный"
            
            _idAlg = newIdAlg;
        }

        /// <summary>
        /// Очистить список "детализации"
        /// </summary>
        private void clearPrjDetail()
        {
            (m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView).Rows.Clear();
        }
        
        protected TreeView m_ctrlTreeView
        {
            get { return m_dictControls[(int)INDEX_CONTROL.TREECTRL_PRJ_ALG] as TreeView; }
        }

        public HPanelEditTree(IPlugIn plugIn, string tableNames)
            : base(plugIn)
        {
            _level = 0;
            _idAlg = -1;

            m_arNameTables = tableNames.Split (',');

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
            //m_ctrlTreeView.BeforeSelect += new TreeViewCancelEventHandler(TreeView_BeforeSelect);
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

            //Обработчики нажатия кнопок
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

            //Заполнить "оригинальные" таблицы из БД...
            for (i = 0; i < (int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER; i ++)
            {
                m_arTableEdit[i] = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + m_arNameTables[i], null, null, out err);

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
                successRecUpdateInsertDelete();

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
                    //Строка с идентификатором задачи
                    strIdTask = rTask[@"ID"].ToString().Trim();
                    //Элемент дерева для очередной задачи
                    nodeTask = m_ctrlTreeView.Nodes.Add(strIdTask, rTask[@"DESCRIPTION"].ToString().Trim());

                    //Массив строк таблицы параметров алгоритма для задачи с очередным ID
                    rowAlgs = m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID_TASK=" + rTask[@"ID"]);

                    //Заполнить "дерево" элементами 1-го уровня (ALGORITM)
                    if (rowAlgs.Length > 0)
                    {
                        foreach (DataRow rAlg in rowAlgs)
                        {
                            //Строка с идентификатором параметра алгоритма
                            strIdAlg = rAlg[@"ID"].ToString().Trim();
                            //Элемент дерева для очередного параметра алгоритма
                            nodeAlg = nodeTask.Nodes.Add(concatIdNode(nodeTask, strIdAlg), rAlg[@"N_ALG"].ToString().Trim());

                            foreach (DataRow rTime in m_arTableKey[(int)INDEX_TABLE_KEY.TIME].Rows)
                            {
                                rowPuts = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + strIdAlg + @" AND ID_TIME=" + rTime[@"ID"]);
                                if (rowPuts.Length > 0)
                                {
                                    strIdTime = rTime[@"ID"].ToString().Trim();
                                    nodeTime = nodeAlg.Nodes.Add(concatIdNode(nodeAlg, strIdTime), rTime[@"DESCRIPTION"].ToString().Trim());
                                    foreach (DataRow rPut in rowPuts)
                                    {
                                        strIdPut = rPut[@"ID"].ToString().Trim();
                                        nodePut = nodeTime.Nodes.Add(concatIdNode(concatIdNode(nodeTime, strIdTime), strIdPut), m_arTableKey[(int)INDEX_TABLE_KEY.COMP_LIST].Select(@"ID=" + rPut[@"ID_COMP"])[0][@"DESCRIPTION"].ToString().Trim());
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
                //dgv.CellEndEdit += new DataGridViewCellEventHandler (HPanelEditTree_dgvPrjDetailCellEndEdit);
                dgv.CellValueChanged += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjDetailCellValueChanged);

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

        private static string concatIdNode (TreeNode nodeParent, string id)
        {
            return concatIdNode (nodeParent.Name, id);
        }

        private static string concatIdNode(string strIdParent, string id)
        {
            return strIdParent + @"::" + id;
        }

        private static string getIdNodePart(string id, INDEX_LEVEL lev)
        {
            if (id.Equals(string.Empty) == false)
            {
                string []ids = id.Split(new string[] { @"::" }, StringSplitOptions.None);
                if ((int)lev < ids.Length)
                    return ids[(int)lev];
                else
                    return string.Empty;
            }
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
                            //, @"НаимПолное"
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

                        TreeNode nodeAdd = nodeSel.Nodes.Add(concatIdNode(nodeSel, id.ToString())
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
                        m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Remove(m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNodePart(nodeSel.Name, level))[0]);
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
            for (INDEX_PARAMETER i = INDEX_PARAMETER.ALGORITM; i < INDEX_PARAMETER.COUNT_INDEX_PARAMETER; i++)
            {
                if (!(m_arTableOrigin[(int)i] == null)) m_arTableOrigin[(int)i].Rows.Clear(); else ;
                m_arTableOrigin[(int)i] = m_arTableEdit[(int)i].Copy();
            }
        }

        private void TreeView_AfterSelect(object obj, TreeViewEventArgs ev)
        {
            int iRes = -1;
            TreeViewAction act = ev.Action;
            TreeNode nodeSel = ev.Node;

            //Индекс текущего уровня в "дереве"
            m_Level = (INDEX_LEVEL)ev.Node.Level;

            //Строка с идентификатором параметра алгоритма расчета ТЭП
            string strIdAlg = getIdNodePart(nodeSel.Name, INDEX_LEVEL.N_ALG);
            //Идентификатор текущего параметра алгоритма
            if ((strIdAlg == null) || (strIdAlg.Equals (string.Empty) == true))
                m_idAlg = -1; //Если выбран "верхний" уровень, или выбран "пкстой" параметр
            else
                m_idAlg = Convert.ToInt32(getIdNodePart(nodeSel.Name, INDEX_LEVEL.N_ALG));

            switch (ev.Node.Level)
            {
                case (int)INDEX_LEVEL.TASK: //Задача
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TASK], INDEX_LEVEL.TASK, true);
                    break;
                case (int)INDEX_LEVEL.N_ALG: //Параметр алгоритма
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], (INDEX_LEVEL)ev.Node.Level, false);
                    if (iRes == 0)
                        nodeAfterSelectDetail(INDEX_TABLE_KEY.TIME, @"ID_TIME=", strIdAlg);
                    else
                        switch (iRes)
                        {
                            case -1: //Отсутствие(дублирование) параметра N_ALG
                            //    break;
                            case -2: //Выбран "некорректный" элемент
                                clearPrjDetail();
                                break;                            
                            default:
                                break;
                        }
                    break;
                case (int)INDEX_LEVEL.TIME:
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TIME], (INDEX_LEVEL)ev.Node.Level, false);
                    if (iRes == 0)
                        nodeAfterSelectDetail(INDEX_TABLE_KEY.COMP_LIST, @"ID_TIME=" + getIdNodePart (ev.Node.Name, INDEX_LEVEL.TIME) + @" AND ID_COMP=", strIdAlg);
                    else
                        ;
                    break;
                case (int)INDEX_LEVEL.COMP:
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.PUT], INDEX_LEVEL.PUT, false);
                    break;
                default:
                    break;
            }            
        }

        /// <summary>
        /// Заполнение 'DataGridView' со свойствами выранного в "деоеве" элемента
        /// </summary>
        /// <param name="node">выбранный элемент "дерева"</param>
        /// <param name="tblProp">целевая таблица свойств</param>
        /// <param name="level">уровень элемента "дерева"</param>
        /// <param name="bThrow">признак формирования исключения при ошибке</param>
        /// <returns></returns>
        private int nodeAfterSelect(TreeNode node, DataTable tblProp, INDEX_LEVEL level, bool bThrow)
        {
            int iErr = 0;
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP]);
            dgv.Rows.Clear();
            string strIdNode = getIdNodePart(node.Name, level);
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

        private void nodeAfterSelectDetail(INDEX_TABLE_KEY key, string where, string strIdAlg)
        {
            DataRow[] rowsPut;
            DataGridView dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
            bool bUpdate = dgv.Rows.Count == m_arTableKey[(int)key].Rows.Count;
            int indxRow = -1;

            //Отменить обработку события
            //if (bUpdate == true)
                dgv.CellValueChanged -= HPanelEditTree_dgvPrjDetailCellValueChanged;
            //else ;

            foreach (DataRow rDetail in m_arTableKey[(int)key].Rows)
            {
                //rowsPut = getDetailRows(Int32.Parse(strIdAlg), where, Convert.ToInt32(rDetail[@"ID"]));
                rowsPut = m_arTableEdit[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + Int32.Parse(strIdAlg)
                                + @" AND " + where + rDetail[@"ID"]);

                //if (!(m_Level == prevLevel))
                //if (indxRow < dgv.Rows.Count)
                if (bUpdate == false)
                    //Заполнить "список" детализации
                    dgv.Rows.Add(new object[] { rDetail[@"DESCRIPTION"], rowsPut.Length > 0 });
                else
                    //Обновить "список" детализации
                    //???снова используется индекс...
                    dgv.Rows[++indxRow].Cells[1].Value = rowsPut.Length > 0;
            }

            //Добавить обработку события
            //if (bUpdate == true)
                dgv.CellValueChanged += HPanelEditTree_dgvPrjDetailCellValueChanged;
            //else ;
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
                    DataRow[] rowsProp = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNodePart (ev.Node.Name, (INDEX_LEVEL)ev.Node.Level));
                    break;
                default:
                    break;
            }
        }

        private void HPanelEditTree_dgvPrjDetailSelectionChanged(object obj, EventArgs ev)
        {
        }

        private void HPanelEditTree_dgvPrjDetailCellValueChanged(object obj, DataGridViewCellEventArgs ev)
        {
            if (ev.ColumnIndex == 1)
            {
                DataGridView dgv;
                DataRow[] rowsDetail;
                string strDetail = string.Empty
                    , strIdDetail = string.Empty
                    , strErr = @"НЕТ ДАННЫХ";
                int iKey = -1
                    , id = -1;
                int idPut = -1;

                //dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                dgv = obj as DataGridView;

                switch (m_Level)
                {
                    case INDEX_LEVEL.N_ALG:
                        iKey = (int)INDEX_TABLE_KEY.TIME;
                        strErr = @"интервала времени";
                        break;
                    case INDEX_LEVEL.TIME:
                        iKey = (int)INDEX_TABLE_KEY.COMP_LIST;
                        strErr = @"компонента станции";
                        idPut = 0;
                        break;
                    default:
                        break;
                }

                strDetail = dgv.Rows[ev.RowIndex].Cells[0].Value.ToString().Trim();
                //???используется поиск по описанию
                rowsDetail = m_arTableKey[iKey].Select(@"DESCRIPTION='" + strDetail + @"'");
                //Проверить кол-во строк (д.б. ОДНа и ТОЬКО ОДНа)
                if (rowsDetail.Length == 1)
                {
                    id = Convert.ToInt32(rowsDetail[0][@"ID"]);

                    if (bool.Parse(dgv.Rows[ev.RowIndex].Cells[1/*ev.ColumnIndex*/].Value.ToString()) == true)
                    {//Добавить элемент...
                        //Оппределить строку с идентификаторами
                        if (idPut == 0)
                        {//Только для реального параметра (для компонента станции)
                            idPut = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)INDEX_PARAMETER.PUT]);
                            if (idPut == 0) idPut += 20001; else ;

                            strIdDetail = concatIdNode(m_ctrlTreeView.SelectedNode, id.ToString());
                            strIdDetail = concatIdNode(strIdDetail, idPut.ToString());
                        }
                        else
                            //Для интервала времени
                            strIdDetail = concatIdNode(m_ctrlTreeView.SelectedNode, id.ToString());
                        //Проверить наличие этого элемента в "дереве"
                        if (m_ctrlTreeView.SelectedNode.Nodes.IndexOfKey(strIdDetail) < 0)
                        {//...если элемент отсутствует
                            if (idPut > 0)
                            {//Только для реального параметра (для компонента станции)
                                m_arTableEdit[(int)INDEX_PARAMETER.PUT].Rows.Add(new object[] {
                                     idPut
                                    , Convert.ToInt32(getIdNodePart (strIdDetail, INDEX_LEVEL.N_ALG)) //ALG
                                    , Convert.ToInt32(getIdNodePart (strIdDetail, INDEX_LEVEL.TIME)) //TIME
                                    , id //COMP
                                    , 0
                                });
                            }
                            else
                                ; //Для интервала времени

                            //Отобразить добавленный элемент
                            m_ctrlTreeView.SelectedNode.Nodes.Add(strIdDetail, strDetail);
                        }
                        else
                            ;

                        //"Развернуть" родительский, по отношению к добавленному, элемент
                        if (m_ctrlTreeView.SelectedNode.IsExpanded == false)
                            //...если до этого элемент не был "развернут"
                            m_ctrlTreeView.SelectedNode.Expand();
                        else
                            ;
                    }
                    else
                    {//Удалить элемент
                        //Массив строк для удаления
                        DataRow[] rowsToDelete;
                        //Проверить тип удалямого параметра (по тек./уровню "дерева")
                        if (idPut == 0)
                        {//Только для "реального" параметра (для компонента станции)
                            //Оганизовать цикл по "дочерним" элементам
                            TreeNode node = m_ctrlTreeView.SelectedNode.FirstNode;
                            while (!(node == null))
                            {
                                if (node.Text.Equals(strDetail) == true)
                                {
                                    strIdDetail = node.Name; 
                                    idPut = Convert.ToInt32(getIdNodePart(strIdDetail, INDEX_LEVEL.PUT));
                                    //Массив строк таблицы для удаления
                                    rowsToDelete = m_arTableEdit[(int)INDEX_PARAMETER.PUT].Select(@"ID=" + idPut);
                                    //Проверить кол-во строк (д.б. ОДНа и ТОЬКО ОДНа)
                                    if (rowsToDelete.Length == 1)
                                        m_arTableEdit[(int)INDEX_PARAMETER.PUT].Rows.Remove(rowsToDelete[0]);
                                    else
                                        throw new Exception(@"HPanelEditTree::HPanelEditTree_dgvPrjDetailCellEndEdit () - отсутствие(дублирование) " + strErr + @"... [ID=" + idPut + @"]");
                                }
                                else
                                    ;
                                //Очередной элемент
                                node = node.NextNode;
                            }
                        }
                        else
                        {//Для интервала времени
                            strIdDetail = concatIdNode(m_ctrlTreeView.SelectedNode, id.ToString());
                            string strIdToDelete = string.Empty;
                            //Оганизовать цикл по "дочерним" элементам
                            TreeNode node = m_ctrlTreeView.SelectedNode.Nodes.Find(strIdDetail, false)[0].FirstNode;
                            while (!(node == null))
                            {
                                strIdToDelete = getIdNodePart(node.Name, INDEX_LEVEL.PUT);
                                rowsToDelete = m_arTableEdit[(int)INDEX_PARAMETER.PUT].Select(@"ID=" + strIdToDelete);
                                //Проверить кол-во строк (д.б. ОДНа и ТОЬКО ОДНа)
                                if (rowsToDelete.Length == 1)
                                    m_arTableEdit[(int)INDEX_PARAMETER.PUT].Rows.Remove(rowsToDelete[0]);
                                else
                                    throw new Exception(@"HPanelEditTree::HPanelEditTree_dgvPrjDetailCellEndEdit () - отсутствие(дублирование) " + strErr + @"... [ID=" + strIdToDelete + @"]");
                                //Очередной элемент
                                node = node.NextNode;
                            }
                        }
                        //Проверить наличие этого элемента в "дереве"
                        if (!(m_ctrlTreeView.SelectedNode.Nodes.IndexOfKey(strIdDetail) < 0))
                        {//...если элемент в наличии
                            //Удалить элемент из "дерева"
                            m_ctrlTreeView.SelectedNode.Nodes.RemoveByKey(strIdDetail);
                        }
                        else
                            ; //Элемент не найден
                    }
                }
                else
                    throw new Exception(@"HPanelEditTree::HPanelEditTree_dgvPrjDetailCellEndEdit () - отсутствие(дублирование) " + strErr + @"... [ID=" + strDetail + @"]");
            }
            else
                ; //Не для значения
        }
    }
}
