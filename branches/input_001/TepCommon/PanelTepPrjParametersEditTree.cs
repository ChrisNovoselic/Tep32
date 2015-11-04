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
    public partial class PanelTepPrjParametersEditTree : HPanelEditTree
    {
        /// <summary>
        /// Перечисление для индексироания уровней "дерева" параметров алгоритма
        /// </summary>
        protected enum ID_LEVEL
        {
            TASK /*Задача*/, N_ALG /*Параметр алгоритма*/, TIME /*Интервал времени*/, COMP /*Компонент станции*/
            , PUT
        };
        /// <summary>
        /// Массив признаков отображения доп./информациии на панели
        ///  в зависимости от выбранного уровня в "дереве"
        ///  - индекс признака соответствует игдексу идентификатора уровня в 'ID_LEVEL'
        /// </summary>
        protected bool[] m_arIsShowDetailLevels;
        /// <summary>
        /// Список идентификаторов уровней для постронения "дерева" параметров алгоритма
        /// </summary>
        protected List<ID_LEVEL> m_listIDLevels;
        //private Dictionary <int, List<ID_LEVEL>> m_dictIDTaskLevels;

        /// <summary>
        /// Перечисление для индексирования элементов управления на панели
        /// </summary>
        private enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE //Кнопки
            , TREECTRL_PRJ_ALG //"дерево"
            , DGV_PRJ_PPOP //свойства элемента "дерева"
            , DGV_PRJ_DETAIL //детализация свойств элемкента "дерева"
            , LABEL_PARAM_DESC //Описание
            , INDEX_CONTROL_COUNT, //Общее кол-во элементов
        };
        /// <summary>
        /// Надписи для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        /// <summary>
        /// Текущий(выбранный) идентификатор задачи в "дереве" авпаметров алгоритма расчета
        /// , ??? (на будущее - для схем построения "дерева" для различных задач)
        /// </summary>
        private int _task;
        private int m_Task
        {
            get { return _task; }
        }

        /// <summary>
        /// Текущий(выбранный) уровень "дерева"
        /// </summary>
        private ID_LEVEL  _level;
        private ID_LEVEL m_Level
        {
            get { return _level; }

            set { if (!(_level == value)) levelChanged (value); else ; }
        }
        /// <summary>
        /// "Обработка" события - изменение значения уровня "дерева"
        /// </summary>
        /// <param name="newLevel"></param>
        private void levelChanged(ID_LEVEL newLevel)
        {
            DataGridView dgv;
            //Очистить список "детализации"
            clearPrjDetail();

            bool bIsShowDetail = m_arIsShowDetailLevels[(int)_level]
                , bNewIsShowDetail = m_arIsShowDetailLevels[(int)newLevel];

            int iShowDetail = 0;

            if (!(bIsShowDetail == bNewIsShowDetail))
                if (bNewIsShowDetail == true)
                    iShowDetail = 1;
                else
                    iShowDetail = -1;
            else
                ;

            //switch (_level)
            //{
            //    case INDEX_LEVEL.TASK:
            //        switch (newLevel)
            //        {
            //            //case INDEX_LEVEL.TASK:
            //            //    break;
            //            case INDEX_LEVEL.N_ALG: //Параметр алгоритма
            //            case INDEX_LEVEL.TIME:
            //                iShowDetail = 1;
            //                break;                        
            //            case INDEX_LEVEL.COMP:
            //                break;
            //            default:
            //                break;
            //        }
            //        break;
            //    case INDEX_LEVEL.N_ALG:
            //        switch (newLevel)
            //        {
            //            case INDEX_LEVEL.TASK:
            //            case INDEX_LEVEL.COMP:
            //                iShowDetail = -1;
            //                break;
            //            //case INDEX_LEVEL.N_ALG:
            //            //    break;
            //            case INDEX_LEVEL.TIME:
            //                break;
            //            default:
            //                break;
            //        }
            //        break;
            //    case INDEX_LEVEL.TIME:
            //        switch (newLevel)
            //        {
            //            case INDEX_LEVEL.TASK:
            //            case INDEX_LEVEL.COMP:
            //                iShowDetail = -1;
            //                break;
            //            case INDEX_LEVEL.N_ALG:
            //                break;
            //            //case INDEX_LEVEL.TIME:
            //            //    break;
            //            default:
            //                break;
            //        }
            //        break;
            //    case INDEX_LEVEL.COMP:
            //        switch (newLevel)
            //        {
            //            case INDEX_LEVEL.TASK:
            //                break;
            //            case INDEX_LEVEL.N_ALG:
            //            case INDEX_LEVEL.TIME:
            //                iShowDetail = 1;
            //                break;
            //            //case INDEX_LEVEL.COMP:
            //            //    break;
            //            default:
            //                break;
            //        }
            //        break;
            //    default:
            //        break;
            //}

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

            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP] as DataGridView;
            dgv.ReadOnly = !(newLevel == ID_LEVEL.N_ALG);
            if (dgv.ReadOnly == false)
                dgv.Columns[0].ReadOnly = true;
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

        public PanelTepPrjParametersEditTree(IPlugIn plugIn, string tableNames)
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
            m_ctrlTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);

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
            //Обработчик события "Выбор строки"
            dgv.SelectionChanged += new EventHandler(HPanelEdit_dgvPropSelectionChanged);
            //Обработчик события "Редактирование значения" (только для алгоритма)
            dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEditTree_dgvPropCellEndEdit);

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
            //Обработчик события "Выбор строки"
            dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjDetailSelectionChanged);
            //dgv.CellEndEdit += new DataGridViewCellEventHandler (HPanelEditTree_dgvPrjDetailCellEndEdit);
            dgv.CellValueChanged += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjDetailCellValueChanged);

            addLabelDesc((int)INDEX_CONTROL.LABEL_PARAM_DESC);

            this.ResumeLayout(false);

            //Обработчики нажатия кнопок
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_ADD]).Click += new System.EventHandler(HPanelEditTree_btnAdd_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_DELETE]).Click += new System.EventHandler(HPanelEditTree_btnDelete_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_SAVE]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_UPDATE]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        private void fillTableKeys(ref DbConnection dbConn, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string[] arNameTableKey = new string[(int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY] { @"time", @"comp_list", @"task" }
                    , arErrKey = new string[(int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY] { @"словарь 'интервалы времени'"
                                                                                        , @"словарь 'компоненты станции'"
                                                                                        , @"проект 'список задач ИРС'" };
            for (int i = 0; i < (int)INDEX_TABLE_KEY.COUNT_INDEX_TABLE_KEY; i++)
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

        private void fillTableEdits(ref DbConnection dbConn, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            for (int i = 0; i < (int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER; i ++)
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
        }

        private int reAddNodes(int indxLevel, TreeNode node_parent, string id_parent)
        {
            int iRes = 0;

            TreeNode node = null;
            TreeNodeCollection nodes;
            string strId = string.Empty
                , strKey = string.Empty;
            DataRow[] rows;
            int iAdd = 0;

            if (indxLevel < m_listLevelParameters.Count)
            {
                if (node_parent == null)
                    nodes = m_ctrlTreeView.Nodes;
                else
                    nodes = node_parent.Nodes;
                
                rows = m_listLevelParameters[indxLevel].Select(id_parent);

                foreach (DataRow r in rows)
                {
                    //Строка с идентификатором задачи
                    //strId = r[m_listLevelParameters[indxLevel].id].ToString().Trim();
                    strId = m_listLevelParameters[indxLevel].GetId(r);
                    if (strId.Equals(string.Empty) == false)
                        strKey = concatIdNode(node_parent, strId);
                    else
                        strKey = id_parent;

                    if (nodes.Find(strKey, false).Length == 0)
                    {
                        //Элемент дерева для очередной задачи
                        if (m_listLevelParameters[indxLevel].desc.Equals(string.Empty) == false)
                        {
                            node = nodes.Add(strKey, r[m_listLevelParameters[indxLevel].desc].ToString().Trim());
                            iRes++;
                        }
                        else
                        {
                            node = node_parent;
                        }

                        if ((indxLevel + 1) < m_listLevelParameters.Count)
                        {
                            iAdd = reAddNodes(indxLevel + 1, node, strKey);
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
                        ;
                }
            }
            else
                ;

            return iRes;
        }

        protected class LEVEL_PARAMETERS
        {
            public DataTable table;
            private string id;
                //, id_sel
            public string dep
                , desc;

            public LEVEL_PARAMETERS(DataTable table, string id, /*string id_sel,*/ string dep, string desc)
            {
                this.table = table;
                this.id = id;
                //this.id_sel = id_sel;
                this.dep = dep;
                this.desc = desc;
            }

            public DataRow[] Select(string id_parent)
            {
                string sel = string.Empty;
                string[] ids = id_parent.Split(new string[] { @"::" }, StringSplitOptions.RemoveEmptyEntries);

                if (dep.Equals(string.Empty) == false)
                {
                    if (!(this.dep.IndexOf(@"{ID_PARENT_0}") < 0))
                        sel = this.dep.Replace(@"{ID_PARENT_0}", ids[ids.Length - 1]);
                    else
                        sel = this.dep;

                    if (!(sel.IndexOf(@"{ID_PARENT_1}") < 0))
                        if (ids.Length > 1)
                            sel = sel.Replace(@"{ID_PARENT_1}", ids[ids.Length - 2]);
                        else
                            Logging.Logg().Error(@"LEVEL_PARAMETERS::Select (id_parent=" + id_parent + @") - отсутствует необходимый параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                    else
                        ;
                }
                else
                    ;

                return table.Select(sel);
            }

            public string GetId(DataRow r)
            {
                string strRes = string.Empty;
                string[] localIds = id.Split(new string [] { @"," }, StringSplitOptions.RemoveEmptyEntries);

                if (localIds.Length == 1)
                    strRes = r[id].ToString().Trim();
                else
                    if (localIds.Length > 1)
                    {
                        foreach (string localId in localIds)
                            strRes += r[localId].ToString().Trim() + @"::";

                        strRes = strRes.Substring(0, strRes.Length - @"::".Length);
                    }
                    else
                        ;

                return strRes;
            }
        }

        protected List<LEVEL_PARAMETERS> m_listLevelParameters;

        protected virtual void initTreeNodes()
        {
            if (!(m_listLevelParameters == null))
                reAddNodes(0, null, string.Empty);
            else
                Logging.Logg().Error(@"HPanelEditTree::initTreeNodes () - не инициализирован список 'm_listLevelParameters' ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void initialize(ref DbConnection dbConn, out int err, out string strErr)
        {
            int i = -1;

            err = 0;
            strErr = string.Empty;

            //Заполнить редактируемые "оригинальные" таблицы из БД...
            fillTableEdits(ref dbConn, out err, out strErr);

            if (err == 0)
                fillTableKeys(ref dbConn, out err, out strErr);
            else
                ; //Строка с описанием ошибки заполнена

            if (err == 0)
            {//Только если обе выборки  рез-м = 0 (УСПЕХ)
                //Копии оригинальных таблиц для редактирования и последующего сравнения с оригигальными...
                successRecUpdateInsertDelete();

                //Вариант №1
                initTreeNodes();

                ////Вариант №2
                //List<TreeNode> listNodes;
                //DataRow[] rowAlgs
                //    , rowPuts;
                //List<string> strIds;

                //listNodes = new List<TreeNode>();
                //strIds = new List<string>();
                //for (i = 0; i < m_listIDLevels.Count; i++)
                //{
                //    listNodes.Add(null);
                //    strIds.Add(string.Empty);
                //}
                ////Заполнить "дерево" элементами 1-го уровня (ALGORITM-задача)
                //foreach (DataRow r0 in m_arTableKey[(int)INDEX_TABLE_KEY.TASK].Rows)
                //{
                //    //Строка с идентификатором задачи
                //    strIds[0] = r0[@"ID"].ToString().Trim();
                //    //Элемент дерева для очередной задачи
                //    listNodes[0] = m_ctrlTreeView.Nodes.Add(strIds[0], r0[@"DESCRIPTION"].ToString().Trim());

                //    //Массив строк таблицы параметров алгоритма для задачи с очередным ID
                //    rowAlgs = m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID_TASK=" + r0[@"ID"]);

                //    //Проверить наличие строк для "задачи"
                //    if (rowAlgs.Length > 0)
                //    {
                //        //Заполнить "дерево" элементами 2-го уровня (ALGORITM-номалг)
                //        foreach (DataRow r1 in rowAlgs)
                //        {
                //            //Вариант №1
                //            //Строка с идентификатором параметра алгоритма
                //            strIds[1] = r1[@"ID"].ToString().Trim();
                //            //Элемент дерева для очередного параметра алгоритма
                //            listNodes[1] = listNodes[0].Nodes.Add(concatIdNode(listNodes[0], strIds[1]), r1[@"N_ALG"].ToString().Trim());
                //            //Заполнить "дерево" элементами 3-го уровня (Интервал)
                //            foreach (DataRow r2 in m_arTableKey[(int)INDEX_TABLE_KEY.TIME].Rows)
                //            {
                //                rowPuts = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + strIds[1] + @" AND ID_TIME=" + r2[@"ID"]);
                //                if (rowPuts.Length > 0)
                //                {
                //                    strIds[2] = r2[@"ID"].ToString().Trim();
                //                    listNodes[2] = listNodes[1].Nodes.Add(concatIdNode(listNodes[1], strIds[2]), r2[@"DESCRIPTION"].ToString().Trim());
                //                    //Заполнить "дерево" элементами 4-го уровня (Компонент)
                //                    foreach (DataRow r3 in rowPuts)
                //                    {
                //                        strIds[3] = r3[@"ID_COMP"].ToString().Trim();
                //                        strIds[4] = r3[@"ID"].ToString().Trim();
                //                        listNodes[3] = listNodes[2].Nodes.Add(
                //                            //concatIdNode(concatIdNode(listNodes[2], strIds[2]), strIds[4])
                //                            concatIdNode(listNodes[2], strIds[4])
                //                            , m_arTableKey[(int)INDEX_TABLE_KEY.COMP_LIST].Select(@"ID=" + r3[@"ID_COMP"])[0][@"DESCRIPTION"].ToString().Trim());
                //                    }
                //                }
                //                else
                //                    continue;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        addNodeNull(listNodes[0]);
                //    }
                //}

                DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_PPOP]);                
                //Только для чтения
                dgv.ReadOnly = true;

                dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL]);
                //Только для чтения
                dgv.Columns[0].ReadOnly = true;                

                m_Level = ID_LEVEL.TASK;
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
            if (nodeParent == null)
                return id;
            else
                return concatIdNode (nodeParent.Name, id);
        }

        private static string concatIdNode(string strIdParent, string id)
        {
            return strIdParent + @"::" + id;
        }
        /// <summary>
        /// Возвратить часть полного идентификатора элемента "дерева"
        /// </summary>
        /// <param name="id">Строка - полный идентификатор элемента "дерева"</param>
        /// <param name="level">Уровень "дерева" - идентификатор уровня</param>
        /// <returns>Строка - часть полного идентификатора</returns>
        private string getIdNodePart(string id, ID_LEVEL level)
        {
            return getIdNodePart(id, m_listIDLevels.IndexOf (level));
        }
        /// <summary>
        /// Возвратить часть полного идентификатора элемента "дерева"
        /// </summary>
        /// <param name="id">Строка - полный идентификатор элемента "дерева"</param>
        /// <param name="indxLev">Уровень элемента "дерева"</param>
        /// <returns>Строка - часть полного идентификатора</returns>
        private static string getIdNodePart(string id, int indxLev)
        {
            if (id.Equals(string.Empty) == false)
            {
                string []ids = id.Split(new string[] { @"::" }, StringSplitOptions.None);
                if (indxLev < ids.Length)
                    return ids[indxLev];
                else
                    return string.Empty;
            }
            else
                return string.Empty;
        }
        /// <summary>
        /// Добавить пустой элемент "дерева"
        /// </summary>
        /// <param name="nodeParent">Элемент "дерева" - родительский для пустого элемента</param>
        private void addNodeNull(TreeNode nodeParent)
        {
            addNodeNull(nodeParent.Nodes);
        }
        /// <summary>
        /// Добавить пустой элемент "дерева"
        /// </summary>
        /// <param name="nodes">Коллекция элементов</param>
        private void addNodeNull(TreeNodeCollection nodes)
        {
            nodes.Add(null, @"Параметры отсутствуют...");
        }
        /// <summary>
        /// Возвратить массив объектов - значений для полей новой (добавляемой строки)
        /// </summary>
        /// <param name="nodeSelName"></param>
        /// <returns></returns>
        protected virtual object[] getRowAdd(INDEX_PARAMETER indxPar, string nodeSelName)
        {
            object[] arObjRes = null;

            int id = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)indxPar]);
            if (id == 0) id += 10001; else ;

            arObjRes = new object[] {
                            id
                        , @"НаимКраткое"
                        //, @"НаимПолное"
                        , "НомАлгоритм"
                        , @"Описание_параметра..."
                        , 0
                        , Int32.Parse(nodeSelName)
                    };

            return arObjRes;
        }
        /// <summary>
        /// Обработчик события нажатия кнопки "Добавить"
        /// </summary>
        /// <param name="obj">Объект инициировавший событие - "кнопка"</param>
        /// <param name="ev">Аргумент события</param>
        private void HPanelEditTree_btnAdd_Click(object obj, EventArgs ev)
        {
            TreeNode nodeSel = m_ctrlTreeView.SelectedNode;

            if (!(nodeSel == null))
            {
                int level = nodeSel.Level;

                switch (level)
                {
                    case (int)ID_LEVEL.TASK:
                        DataRow rowAdd = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Add(getRowAdd(INDEX_PARAMETER.ALGORITM, nodeSel.Name));
                        //Удалить элемент 'Параметры отсутствуют...'
                        if ((nodeSel.Nodes.Count == 1)
                            && ((nodeSel.Nodes[0].Name == null) || (nodeSel.Nodes[0].Name.Equals (string.Empty) == true)))
                            nodeSel.Nodes.RemoveAt(0);
                        else
                            ;

                        TreeNode nodeAdd = nodeSel.Nodes.Add(concatIdNode(nodeSel
                            , rowAdd[@"ID"].ToString())
                            , rowAdd[@"N_ALG"].ToString().Trim());
                        //nodeAdd.La
                        break;
                    default:
                        // в других случаях кн. "Добавить" - вЫкл.
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
                ID_LEVEL level = (ID_LEVEL)nodeSel.Level;

                switch (level)
                {
                    case ID_LEVEL.TASK:                        
                        break;
                    case ID_LEVEL.N_ALG:                        
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

    partial class PanelTepPrjParametersEditTree
    {
        protected enum INDEX_PARAMETER {ALGORITM, PUT, COUNT_INDEX_PARAMETER};
        protected enum INDEX_TABLE_KEY { TIME, COMP_LIST, TASK, COUNT_INDEX_TABLE_KEY };
        string[] m_arNameTables;
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        protected DataTable [] m_arTableKey;

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
            m_Level = m_listIDLevels[ev.Node.Level];

            //Строка с идентификатором параметра алгоритма расчета ТЭП
            string strIdAlg = getIdNodePart(nodeSel.Name, ID_LEVEL.N_ALG);
            //Идентификатор текущего параметра алгоритма
            if ((strIdAlg == null) || (strIdAlg.Equals (string.Empty) == true))
                m_idAlg = -1; //Если выбран "верхний" уровень, или выбран "пустой" параметр
            else
                m_idAlg = Convert.ToInt32(getIdNodePart(nodeSel.Name, ID_LEVEL.N_ALG));

            switch (m_Level)
            {
                case ID_LEVEL.TASK: //Задача
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TASK], m_Level, false);
                    break;
                case ID_LEVEL.N_ALG: //Параметр алгоритма
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], m_Level, false);
                    if (iRes == 0)
                        nodeAfterSelectDetail(INDEX_TABLE_KEY.TIME, strIdAlg, @"ID_TIME=");
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
                case ID_LEVEL.TIME:
                    iRes = nodeAfterSelect(ev.Node, m_arTableKey[(int)INDEX_TABLE_KEY.TIME], m_Level, false);
                    if (iRes == 0)
                        nodeAfterSelectDetail(INDEX_TABLE_KEY.COMP_LIST, strIdAlg, @"ID_TIME=" + getIdNodePart (ev.Node.Name, m_Level) + @" AND ID_COMP=");
                    else
                        ;
                    break;
                case ID_LEVEL.COMP:
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.PUT], ID_LEVEL.PUT, false);
                    break;
                default:
                    break;
            }            
        }

        /// <summary>
        /// Заполнение 'DataGridView' со свойствами выбранного в "дереве" элемента
        /// </summary>
        /// <param name="node">выбранный элемент "дерева"</param>
        /// <param name="tblProp">целевая таблица свойств</param>
        /// <param name="level">уровень элемента "дерева"</param>
        /// <param name="bThrow">признак формирования исключения при ошибке</param>
        /// <returns>Признак выполнения функции</returns>
        private int nodeAfterSelect(TreeNode node, DataTable tblProp, ID_LEVEL level, bool bThrow)
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
        /// <summary>
        /// Заполнить список детализации
        /// </summary>
        /// <param name="key">Индекс таблицы</param>
        /// <param name="strIdAlg">Значение идентификатора - прямое условие отбора записей в таблице</param>
        /// <param name="where">Дополнительное условие отбора записей в таблице</param>        
        private void nodeAfterSelectDetail(INDEX_TABLE_KEY key, string strIdAlg, string where) {
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
            switch (m_listIDLevels[ev.Node.Level])
            {
                case ID_LEVEL.TASK: //Задача - не редактируется
                case ID_LEVEL.TIME: //Интервал времени - не редактируется
                case ID_LEVEL.COMP: //Компонент станции - не редактируется
                    ev.CancelEdit = true;
                    break;
                case ID_LEVEL.N_ALG: //Параметр алгоритма
                    DataRow[] rowsProp = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNodePart(ev.Node.Name, m_listIDLevels[ev.Node.Level]));
                    break;
                default:
                    break;
            }
        }

        private void HPanelEditTree_dgvPropCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            string strThrow = string.Empty;

            if ((m_Level == ID_LEVEL.N_ALG)
                && (ev.ColumnIndex == 1)            
                )
            {
                DataGridView dgv = obj as DataGridView;
                DataRow[] rowsAlg = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + m_idAlg);

                if (rowsAlg.Length == 1)
                {
                    string strNameField = dgv.Rows[ev.RowIndex].Cells[0].Value.ToString().Trim()
                        , strVal = dgv.Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value.ToString().Trim();
                    rowsAlg[0][strNameField] = strVal;

                    if (strNameField.Equals(@"N_ALG") == true)
                    {//Изменить подпись в "дереве"
                        (m_dictControls[(int)INDEX_CONTROL.TREECTRL_PRJ_ALG] as TreeView).SelectedNode.Text = strVal;
                    }
                    else
                        ;
                }
                else
                    strThrow = @"найдено " + rowsAlg.Length + @" записей";
            }
            else
                strThrow = @"редактирование запрещено";

            if (strThrow.Equals(string.Empty) == false)
                throw new Exception(@"HPanelEditTree_dgvPropCellEndEdit () - " + strThrow + @"...");
            else
                ;
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
                    case ID_LEVEL.N_ALG:
                        iKey = (int)INDEX_TABLE_KEY.TIME;
                        strErr = @"интервала времени";
                        break;
                    case ID_LEVEL.TIME:
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
                                    , Convert.ToInt32(getIdNodePart (strIdDetail, ID_LEVEL.N_ALG)) //ALG
                                    , Convert.ToInt32(getIdNodePart (strIdDetail, ID_LEVEL.TIME)) //TIME
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
                                    idPut = Convert.ToInt32(getIdNodePart(strIdDetail, ID_LEVEL.PUT));
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
                                strIdToDelete = getIdNodePart(node.Name, ID_LEVEL.PUT);
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
