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
    public abstract partial class PanelPrjParametersEditTree : HPanelEditTree
    {
        /// <summary>
        /// Перечисление - индексы для столбцов в таблице с параметрами алгоритма расчета
        /// </summary>
        private enum INDEX_COLUMN_TABLEALGORITM : uint { ID, NAME_SHR, N_ALG, DESCRIPTION, ID_MEASURE, ID_TASK, SYMBOL
            , COUNT}
        /// <summary>
        /// Массив значений по умолчанию для добавлямой строки в таблицу с параметрами алгоритма расчета
        /// </summary>
        private static object[] s_arTableAlgoritmDefaultValues = new object[(int)INDEX_COLUMN_TABLEALGORITM.COUNT] {
            -1
            , @"НаимКраткое"
            , @"НомАлгоритм"
            , @"Описание_параметра..."
            , 0
            , -1
            , @""
        };
        /// <summary>
        /// Перечисление для индексироания уровней "дерева" параметров алгоритма
        /// </summary>
        protected enum ID_LEVEL
        {
            UNKNOWN = -2
            , TEP_OUT = -1
            , TASK /*Задача*/, N_ALG /*Параметр алгоритма*/, /*TIME Интервал времени,*/ COMP /*Компонент станции*/
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

        private DataGridView m_dgvPrjProp
            , m_dgvPrjDetail;
        /// <summary>
        /// Перечисление для индексирования элементов управления на панели
        /// </summary>
        protected enum INDEX_CONTROL
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
        /// Текущий(выбранный) идентификатор задачи в "дереве" параметров алгоритма расчета
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
        protected ID_LEVEL m_Level
        {
            get { return _level; }

            set { if (!(_level == value)) onLevelChanged (value); else ; }
        }
        /// <summary>
        /// "Обработка" события - изменение значения уровня "дерева"
        /// </summary>
        /// <param name="newLevel">Новое значение для переменной (выбранный пользователем уровень древовидной структуры)</param>
        private void onLevelChanged(ID_LEVEL newLevel)
        {
            //Очистить список "детализации"
            clearPrjDetail();

            dgvVisible(newLevel);

            _level = newLevel;
        }

        private bool isDataGridViewPropReadOnly (ID_LEVEL level = ID_LEVEL.UNKNOWN)
        {
            bool bRes = false;
            ID_LEVEL curLevel = level;

            if (curLevel == ID_LEVEL.UNKNOWN)
                curLevel = m_Level;
            else
                ;

            bRes = (!(curLevel == ID_LEVEL.N_ALG)) && (!(curLevel == ID_LEVEL.PUT));
            
            return bRes;
        }
        /// <summary>
        /// Отобразить/снять с отображения
        /// </summary>
        /// <param name="newLevel">Новое значение для переменной (выбранный пользователем уровень древовидной структуры)</param>
        private void dgvVisible(ID_LEVEL newLevel)
        {
            bool bPrevIsShowProp = false, bNewIsShowProp = false
                , bPrevIsShowDetail = false, bNewIsShowDetail = false;

            int iShowAction = 0;

            if (newLevel > ID_LEVEL.TEP_OUT)
            {
                if (_level > ID_LEVEL.TEP_OUT)
                {
                    bPrevIsShowProp = true;
                    bPrevIsShowDetail = m_arIsShowDetailLevels[(int)_level];
                }
                else
                    ; // ранее m_dgvPrjProp, m_dgvPrjDetail доп./свойств не отображались
                // определить новые признаки отображения
                bNewIsShowProp = true;
                bNewIsShowDetail = m_arIsShowDetailLevels[(int)newLevel];
                // проверить на выполнение взаимоисключающих условий
                // m_dgvPrjDetail не м.б. отображен без m_dgvPrjProp
                if (((bPrevIsShowProp == false) && (bPrevIsShowDetail == true))
                    || ((bNewIsShowProp == false) && (bNewIsShowDetail == true)))
                    throw new Exception(@"PanelPrjParameterEditTree::onLevelChanged (_level=" + _level.ToString()
                        + @", newLevel=" + newLevel + @") - ...");
                else
                    ;

                if (bPrevIsShowProp == false)
                    // предыдущее состояние - ни один из объектов не отображался
                    if ((bNewIsShowProp == true) && (bNewIsShowDetail == true))
                        // отобразить оба объекта безусловно
                        iShowAction = 3;
                    else
                        if (bNewIsShowProp == true)
                            // отобразить только m_dgvPrjProp
                            iShowAction = 2;
                        else
                            ; // по прежнему ничего не отобраажать
                else
                    if (bPrevIsShowDetail == false)
                        // предыдущее состояние - отображался только m_dgvPrjProp
                        if (bNewIsShowProp == true)
                            if (bNewIsShowDetail == true)
                                // добавить m_dgvPrjDetail                                
                                iShowAction = 1;
                            else
                                ; // ничего не делать - отображение актуальное
                        else
                            //Удалить с панели 'DataGridView' c ID = DGV_PRJ_PROP
                            iShowAction = -1;
                    else
                        // предыдущее состояние - оба объекта отображались
                        if (bNewIsShowProp == false)
                            iShowAction = -3;
                        else
                            if (bNewIsShowDetail == false)
                                iShowAction = -2;
                            else
                                ; // текущее состояние актуальное - отображаются оба объекта

                m_dgvPrjProp.ReadOnly = isDataGridViewPropReadOnly (newLevel);
                if (m_dgvPrjProp.ReadOnly == false)
                    m_dgvPrjProp.Columns[0].ReadOnly = true;
                else
                    ;
            }
            else
                iShowAction = -3;

            switch (iShowAction)
            {
                case 3:
                    this.Controls.Add(m_dgvPrjProp, 5, 0);
                    this.SetColumnSpan(m_dgvPrjProp, 8); this.SetRowSpan(m_dgvPrjProp, 5);
                    this.Controls.Add(m_dgvPrjDetail, 5, 5);
                    this.SetColumnSpan(m_dgvPrjDetail, 8); this.SetRowSpan(m_dgvPrjDetail, 5);
                    break;
                case 2:
                    this.Controls.Add(m_dgvPrjProp, 5, 0);
                    this.SetColumnSpan(m_dgvPrjProp, 8); this.SetRowSpan(m_dgvPrjProp, 10);
                    break;
                case 1:
                    //Уменьшить кол-во строк для 'DataGridView' c ID = DGV_PRJ_PPOP
                    this.SetColumnSpan(m_dgvPrjProp, 8); this.SetRowSpan(m_dgvPrjProp, 5);
                    //Размесить 'DataGridView' c ID = DGV_PRJ_DETAIL
                    this.Controls.Add(m_dgvPrjDetail, 5, 5);
                    this.SetColumnSpan(m_dgvPrjDetail, 8); this.SetRowSpan(m_dgvPrjDetail, 5);
                    break;
                case -1:
                    //Удалить с панели 'DataGridView' c ID = DGV_PRJ_PROP
                    this.Controls.Remove(m_dgvPrjProp);
                    break;
                case -2:
                    //Удалить с панели 'DataGridView' c ID = DGV_PRJ_DETAIL
                    this.Controls.Remove(m_dgvPrjDetail);
                    //Увеличить кол-во строк для 'DataGridView' c ID = DGV_PRJ_PPOP
                    this.SetColumnSpan(m_dgvPrjProp, 8); this.SetRowSpan(m_dgvPrjProp, 10);
                    break;
                case -3:
                    //Удалить с панели 'DataGridView' c ID = DGV_PRJ_PROP
                    this.Controls.Remove(m_dgvPrjProp);
                    //Удалить с панели 'DataGridView' c ID = DGV_PRJ_DETAIL
                    this.Controls.Remove(m_dgvPrjDetail);
                    break;
                default:
                    break;
            }
        }

        protected virtual void btnEnable (string strIdTask)
        {
            bool bNewIsButtonAddEnabled = false, bNewIsButtonDeleteEnabled = false;

            switch (m_Level)
            {
                case ID_LEVEL.TASK:
                    //??? требуется знать идентификатор задачи
                    bNewIsButtonAddEnabled = true;
                    bNewIsButtonDeleteEnabled = false;
                    break;
                case ID_LEVEL.N_ALG:
                    bNewIsButtonAddEnabled = false;
                    bNewIsButtonDeleteEnabled = m_idAlg < 0 ? false : true;
                    break;
                //case ID_LEVEL.COMP:
                //    bPrevIsButtonAddEnabled = false;
                //    bPrevIsButtonDeleteEnabled = true;
                //    break;
                case ID_LEVEL.PUT:
                    bNewIsButtonAddEnabled = false;
                    bNewIsButtonDeleteEnabled = true;
                    break;
                case ID_LEVEL.TEP_OUT:
                    bNewIsButtonAddEnabled = true;
                    bNewIsButtonDeleteEnabled = false;
                    break;
                default:
                    break;
            }

            Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0].Enabled = bNewIsButtonAddEnabled;
            Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0].Enabled = bNewIsButtonDeleteEnabled;
        }

        private int _idAlg;
        /// <summary>
        /// Идентификатор текущего(выбранного) параметра алгоритма
        /// </summary>
        protected int m_idAlg {
            get { return _idAlg; }

            set { if (!(_idAlg == value)) onIdAlgChanged(value); else ; }
        }
        /// <summary>
        /// Обработчик события - изменение текущего параметра алгоритма расчета 
        /// </summary>
        /// <param name="newIdAlg"></param>
        private void onIdAlgChanged(int newIdAlg)
        {
            if (_idAlg > 0)
            {
                //DataRow []rowsAlg = m_arTableEdit [(int)INDEX_PARAMETER.ALGORITM].Select (@"ID=" + _idAlg);                

                //if (rowsAlg.Length == 1)
                //{
                //    //Список ключей для удаления (при отсутствии дочерних элементов)
                //    List<string> toDeleteKeys = new List<string> ();
                //    try
                //    {
                //        //Получить элемент, который теряет фокус выбора
                //        TreeNode prevNode = m_ctrlTreeView.Nodes.Find(rowsAlg[0][@"ID_TASK"].ToString().Trim() + @"::" + _idAlg, true)[0]
                //            // получить 1-ый дочерний
                //            , node = prevNode.FirstNode;
                //        // заполнить список ключей элементов для удаления
                //        while (!(node == null))
                //        {
                //            if (node.GetNodeCount(false) == 0)
                //            {
                //                toDeleteKeys.Add (node.Name);
                //            }
                //            else
                //                ;

                //            node = node.NextNode;
                //        }
                //        // удвлить элементы
                //        while (toDeleteKeys.Count > 0)
                //        {
                //            prevNode.Nodes.RemoveByKey(toDeleteKeys[0]);
                //            toDeleteKeys.RemoveAt(0);
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Logging.Logg().Exception(e, @"HPanelEditTree::idAlgChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                //    }
                //}
                //else
                //    throw new Exception(@"HPanelEditTree::idAlgChanged () - отсутствие(дублирование) параметра... [ID=" + _idAlg + @"]");
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
            m_dgvPrjDetail.Rows.Clear();
        }
        
        protected TreeView m_ctrlTreeView
        {
            get { return Controls.Find(INDEX_CONTROL.TREECTRL_PRJ_ALG.ToString(), true)[0] as TreeView; }
        }

        public PanelPrjParametersEditTree(IPlugIn plugIn, string tableNames)
            : base(plugIn)
        {
            _level = ID_LEVEL.UNKNOWN;
            _idAlg = -1;

            m_arNameTables = tableNames.Split (',');

            m_arTableOrigin = new DataTable[(int)INDEX_PARAMETER.COUNT];
            m_arTableEdit = new DataTable[(int)INDEX_PARAMETER.COUNT];
            m_arTableDictPrj = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Control ctrl = null;

            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            //Добавить "список" словарных величин
            ctrl = new TreeView();
            ctrl.Name = INDEX_CONTROL.TREECTRL_PRJ_ALG.ToString();
            i = INDEX_CONTROL.TREECTRL_PRJ_ALG;
            ctrl.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(ctrl, 1, 0);
            this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 13);
            m_ctrlTreeView.HideSelection = false;
            //m_ctrlTreeView.BeforeSelect += new TreeViewCancelEventHandler(TreeView_BeforeSelect);
            m_ctrlTreeView.AfterSelect += new TreeViewEventHandler(TreeView_AfterSelect);
            m_ctrlTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);

            //Добавить "список" свойств словарной величины
            m_dgvPrjProp = new DataGridView();
            m_dgvPrjProp.Name = INDEX_CONTROL.DGV_PRJ_PPOP.ToString ();
            i = INDEX_CONTROL.DGV_PRJ_PPOP;
            m_dgvPrjProp.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(m_dgvPrjProp, 5, 0);
            this.SetColumnSpan(m_dgvPrjProp, 8); this.SetRowSpan(m_dgvPrjProp, 10);
            //Добавить столбцы
            m_dgvPrjProp.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewTextBoxColumn ()
                });
            //Отменить возможность добавления строк
            m_dgvPrjProp.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            m_dgvPrjProp.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            m_dgvPrjProp.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            m_dgvPrjProp.RowHeadersVisible = false;
            //Отменить возможность изменения высоты строк
            m_dgvPrjProp.AllowUserToResizeRows = false;
            //1-ый столбец
            m_dgvPrjProp.Columns[0].HeaderText = @"Свойство"; m_dgvPrjProp.Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            m_dgvPrjProp.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //2-ой столбец
            m_dgvPrjProp.Columns[1].HeaderText = @"Значение";
            //Установить режим выделения - "полная" строка
            m_dgvPrjProp.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            m_dgvPrjProp.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //Обработчик события "Выбор строки"
            m_dgvPrjProp.SelectionChanged += new EventHandler(HPanelEdit_dgvPropSelectionChanged);
            //Обработчик события "Редактирование значения" (только для алгоритма)
            m_dgvPrjProp.CellEndEdit += new DataGridViewCellEventHandler(dgvProp_onCellEndEdit);

            //Создать "список" дополн./парамеиров (TIME, COMP)
            m_dgvPrjDetail = new DataGridView();
            m_dgvPrjDetail.Name = INDEX_CONTROL.DGV_PRJ_DETAIL.ToString ();
            i = INDEX_CONTROL.DGV_PRJ_DETAIL;
            m_dgvPrjDetail.Dock = DockStyle.Fill;

            //Добавить столбцы
            m_dgvPrjDetail.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewCheckBoxColumn ()
                });
            //Отменить возможность добавления строк
            m_dgvPrjDetail.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            m_dgvPrjDetail.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            m_dgvPrjDetail.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            m_dgvPrjDetail.RowHeadersVisible = false;
            //Отменить возможность изменения высоты строк
            m_dgvPrjDetail.AllowUserToResizeRows = false;
            //1-ый столбец (только "для чтения")
            m_dgvPrjDetail.Columns[0].HeaderText = @"Свойство"; m_dgvPrjDetail.Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            m_dgvPrjDetail.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //2-ой столбец
            m_dgvPrjDetail.Columns[1].HeaderText = @"Наличие";
            //Установить режим выделения - "полная" строка
            m_dgvPrjDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            m_dgvPrjDetail.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //Обработчик события "Выбор строки"
            m_dgvPrjDetail.SelectionChanged += new EventHandler(dgvPrjDetail_onSelectionChanged);
            //m_dgvPrjDetail.CellEndEdit += new DataGridViewCellEventHandler (HPanelEditTree_dgvPrjDetailCellEndEdit);
            m_dgvPrjDetail.CellValueChanged += new DataGridViewCellEventHandler(dgvPrjDetail_onCellValueChanged);

            addLabelDesc(INDEX_CONTROL.LABEL_PARAM_DESC.ToString ());

            this.ResumeLayout(false);

            //Обработчики нажатия кнопок
            ((Button)Controls.Find (INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(btnAdd_onClick);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(btnDelete_onClick);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        private void fillTableDictPrj(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string[] arNameTableKey = new string[(int)INDEX_TABLE_DICTPRJ.COUNT] { /*@"time"
                                                                                        ,*/ @"comp_list"
                                                                                        , @"task" }
                    , arErrKey = new string[(int)INDEX_TABLE_DICTPRJ.COUNT] { /*@"словарь 'интервалы времени'"
                                                                                        ,*/ @"словарь 'компоненты станции'"
                                                                                        , @"проект 'список задач ИРС'" };
            for (int i = 0; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrj[(int)i] = m_handlerDb.GetDataTable (arNameTableKey[(int)i], out err);

                if (!(m_arTableDictPrj[(int)i].Rows.Count > 0))
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

        private void fillTableEdits(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string query = string.Empty;

            for (int i = 0; i < (int)INDEX_PARAMETER.COUNT; i ++)
            {
                query = @"SELECT * FROM " + m_arNameTables[i];
                if (i == (int)INDEX_PARAMETER.ALGORITM)
                    query += @" ORDER BY [N_ALG]";

                m_arTableEdit[i] = m_handlerDb.Select(query, out err);

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
        /// <summary>
        /// Добавить элементы к древовидной структуре (рекурсивная функция)
        /// </summary>
        /// <param name="indxLevel">??? Индекс текущего уровня в структуре</param>
        /// <param name="node_parent">Объект - элемент структуры, родительский по отношению к добавляемым</param>
        /// <param name="id_parent">??? Строковый идентификатор родительского элемента структуры</param>
        /// <returns>Количество добавленных элементов</returns>
        protected virtual int reAddNodes(int indxLevel, TreeNode node_parent, string strId_parent)
        //protected virtual int reAddNodes(TreeNode node_parent)
        {
            int iRes = 0;

            TreeNode node = null;
            TreeNodeCollection nodes;
            string //strId_parent = node_parent == null ? string.Empty : node_parent.Name,
                strId = string.Empty
                , strKey = string.Empty
                , strItem = string.Empty;
            DataRow[] rows;
            int //indxLevel = node_parent == null ? 0 : node_parent.Level + 1,
                iAdd = 0;

            if (indxLevel < m_listLevelParameters.Count)
            {
                if (node_parent == null)
                    nodes = m_ctrlTreeView.Nodes;
                else
                    nodes = node_parent.Nodes;

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

                    if (nodes.Find(strKey, false).Length == 0)
                    {
                        //Элемент дерева для очередной задачи
                        if (m_listLevelParameters[indxLevel].desc.Equals(string.Empty) == false)
                        {
                            strItem = r[m_listLevelParameters[indxLevel].desc].ToString().Trim();
                            if (m_listLevelParameters[indxLevel].desc_detail.Equals (string.Empty) == false)
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
                                //reAddNodes (node)
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
        /// <summary>
        /// Класс для описания уровня древовидной структуры
        /// </summary>
        protected class LEVEL_PARAMETERS
        {
            public DataTable table;
            /// <summary>
            /// Строка - наименование поля в таблице с идентификатором
            /// </summary>
            private string id;
            /// <summary>
            /// Строка - условие для формирования зависимости от записи с более высоким уровнем в дереве
            /// </summary>
            public string dep;
            /// <summary>
            /// Строка - наименование поля для формирования содержания элемента дерева
            /// </summary>
            public string desc
                , desc_detail;

            public LEVEL_PARAMETERS(DataTable table, string id, /*string id_sel,*/ string dep, string desc, string desc_detail)
            {
                this.table = table;
                this.id = id;
                //this.id_sel = id_sel;
                this.dep = dep;
                this.desc = desc;
                this.desc_detail = desc_detail;
            }
            /// <summary>
            /// Получить масив строк для формирования подчиненной древовидной структуры
            ///  на основании строкового идентификатора верхнего уровня
            ///  и установленных правил для подчиненных уровней структуры
            /// </summary>
            /// <param name="id_parent">Строковый идентификатор элемента
            ///  с более высоким уровнем в древовидной структуре</param>
            /// <returns></returns>
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
            /// <summary>
            /// Возвратить строковый идентификатор
            /// </summary>
            /// <param name="r"></param>
            /// <returns>Строковый идентификатор</returns>
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
        /// <summary>
        /// Инициализация объекта элементамии жревовидной структуры
        /// </summary>
        protected virtual void initTreeNodes()
        {
            if (!(m_listLevelParameters == null))
                reAddNodes(0, null, string.Empty)
                //reAddNodes(null)
                    ;
            else
                Logging.Logg().Error(@"HPanelEditTree::initTreeNodes () - не инициализирован список 'm_listLevelParameters' ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void initialize(out int err, out string strErr)
        {
            int i = -1;

            err = 0;
            strErr = string.Empty;

            //Заполнить редактируемые "оригинальные" таблицы из БД...
            fillTableEdits(out err, out strErr);

            if (err == 0)
                fillTableDictPrj(out err, out strErr);
            else
                ; //Строка с описанием ошибки заполнена

            if (err == 0)
            {//Только если обе выборки  рез-м = 0 (УСПЕХ)
                //Копии оригинальных таблиц для редактирования и последующего сравнения с оригигальными...
                successRecUpdateInsertDelete();
                // установить правила для формирования элементов дерева 'm_listLevelParameters'
                initTreeNodes();

                //Только для чтения
                m_dgvPrjProp.ReadOnly = true;
                //Только для чтения
                m_dgvPrjDetail.Columns[0].ReadOnly = true;                

                m_Level = ID_LEVEL.TASK;
                m_ctrlTreeView.SelectedNode = m_ctrlTreeView.Nodes[0];
            }
            else
                ; //Строка с описанием ошибки заполнена
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            return bRes;
        }

        protected override void reinit()
        {
            m_ctrlTreeView.Nodes.Clear();
            
            base.reinit();
        }
        /// <summary>
        /// Получить строковый идентификатор элемента
        /// </summary>
        /// <param name="nodeParent">Элемент (родительский) структуры</param>
        /// <param name="id">Часть (собственная) строкового идентификатора элемента</param>
        /// <returns>Строковый идентификатор элемента древовидной структуры</returns>
        protected static string concatIdNode (TreeNode nodeParent, string id)
        {
            if (nodeParent == null)
                return id;
            else
                return concatIdNode (nodeParent.Name, id);
        }
        /// <summary>
        /// Получить строковый идентификатор элемента
        /// </summary>
        /// <param name="strIdParent">Строковый идентификатор</param>
        /// <param name="id">Часть (собственная) строкового идентификатора элемента</param>
        /// <returns>Строковый идентификатор элемента древовидной структуры</returns>
        protected static string concatIdNode(string strIdParent, string id)
        {
            return strIdParent + @"::" + id;
        }
        /// <summary>
        /// Возвратить часть полного идентификатора элемента "дерева"
        /// </summary>
        /// <param name="id">Строка - полный идентификатор элемента "дерева"</param>
        /// <param name="level">Уровень "дерева" - идентификатор уровня</param>
        /// <returns>Строка - часть полного идентификатора</returns>
        protected string getIdNodePart(string id, ID_LEVEL level)
        {
            int cntLevel = -1
                , offsetLevel = -1;

            if (level < ID_LEVEL.N_ALG)
                offsetLevel = 0;
            else
                offsetLevel = getLevelOffset(id, out cntLevel);

            return getIdNodePart(id, m_listIDLevels.IndexOf (level) + offsetLevel);
        }
        /// <summary>
        /// Возвратить часть полного идентификатора элемента "дерева"
        /// </summary>
        /// <param name="id">Строка - полный идентификатор элемента "дерева"</param>
        /// <param name="indxLev">Уровень элемента "дерева"</param>
        /// <returns>Строка - часть полного идентификатора</returns>
        protected static string getIdNodePart(string id, int indxLev)
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
        protected void addNodeNull(TreeNode nodeParent)
        {
            addNodeNull(nodeParent.Nodes);
        }
        /// <summary>
        /// Добавить пустой элемент "дерева"
        /// </summary>
        /// <param name="nodes">Коллекция элементов</param>
        protected void addNodeNull(TreeNodeCollection nodes)
        {
            nodes.Add(null, @"Параметры отсутствуют...");
        }
        /// <summary>
        /// Возвратить следующий целочисленный идентификатор для добавляемой строки
        ///  в таблице с параметрами в алгоритме расчета
        /// </summary>
        /// <returns>Целочисленный идентификатор</returns>
        protected virtual int getIdNextAlgoritm()
        {
            int iRes = -1
                , err = -1;

            iRes = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], out err);
            if (iRes == 0) iRes += (int)ID_START_RECORD.ALG; else ;

            return iRes;
        }
        /// <summary>
        /// Вставить строку в таблицу с параметрами алгоритма расчета
        /// </summary>
        /// <param name="nodeSelName">Строковый идентификатор параметра</param>
        /// <returns>Идентификатор добавленной строки</returns>
        protected int addRowToTableAlgoritm()
        {
            int iRes = getIdNextAlgoritm();

            if (iRes > 0)
            {
                m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Rows.Add(new object[] {
                            iRes //ID
                            , s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.NAME_SHR]
                            //, s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.]
                            , s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.N_ALG]
                            , s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.DESCRIPTION]
                            , s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.ID_MEASURE]
                            , Int32.Parse(getIdNodePart(m_ctrlTreeView.SelectedNode.Name, ID_LEVEL.TASK)) //ID_TASK
                            //??? SYMBOL
                        });
            }
            else
                ;

            return iRes;
        }

        protected abstract void addRowToTablePut(int idPut, int idComp);
        /// <summary>
        /// Обработчик события нажатия кнопки "Добавить"
        /// </summary>
        /// <param name="obj">Объект инициировавший событие - "кнопка"</param>
        /// <param name="ev">Аргумент события</param>
        private void btnAdd_onClick(object obj, EventArgs ev)
        {
            TreeNode node_parent = m_ctrlTreeView.SelectedNode;
            int idAlg = -1;

            if (!(node_parent == null))
                switch (m_Level)
                {
                    case ID_LEVEL.TASK:
                    case ID_LEVEL.TEP_OUT:
                        idAlg = addRowToTableAlgoritm();
                        //Удалить элемент 'Параметры отсутствуют...'
                        if ((node_parent.Nodes.Count == 1)
                            && ((node_parent.Nodes[0].Name == null) || (node_parent.Nodes[0].Name.Equals (string.Empty) == true)))
                            node_parent.Nodes.RemoveAt(0);
                        else
                            ;

                        TreeNode nodeAdd =
                        m_ctrlTreeView.SelectedNode =
                        node_parent.Nodes.Add(
                            concatIdNode(node_parent, idAlg.ToString())
                            , (string)s_arTableAlgoritmDefaultValues[(int)INDEX_COLUMN_TABLEALGORITM.N_ALG]);
                        break;
                    default:
                        // в других случаях кн. "Добавить" - вЫкл.
                        break;
                }
            else
                ;
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки "Удалить"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnDelete_onClick(object obj, EventArgs ev)
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

    partial class PanelPrjParametersEditTree
    {
        protected enum INDEX_PARAMETER {UNKNOWN = -1, ALGORITM, PUT, COUNT};
        protected enum INDEX_TABLE_DICTPRJ { /*TIME,*/ COMP_LIST, TASK, COUNT };
        string[] m_arNameTables;
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        protected DataTable [] m_arTableDictPrj;

        protected override void recUpdateInsertDelete(out int err)
        {
            err = 0;

            int iRegDbConn = -1;
            DbConnection dbConn = null;

            m_handlerDb.RegisterDbConnection(out iRegDbConn);
            dbConn = m_handlerDb.DbConnection;

            for (INDEX_PARAMETER i = INDEX_PARAMETER.ALGORITM; i < INDEX_PARAMETER.COUNT; i++)
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

            m_handlerDb.UnRegisterDbConnection();
        }

        protected override void successRecUpdateInsertDelete()
        {
            for (INDEX_PARAMETER i = INDEX_PARAMETER.ALGORITM; i < INDEX_PARAMETER.COUNT; i++)
            {
                if (!(m_arTableOrigin[(int)i] == null)) m_arTableOrigin[(int)i].Rows.Clear(); else ;
                m_arTableOrigin[(int)i] = m_arTableEdit[(int)i].Copy();
            }
        }

        private int getLevelOffset(string strId, out int counter)
        {
            int iRes = 0
                , iId = -1;
            counter = 0;
            string[] strIds = null;

            // получить все части идентификатора
            strIds = strId.Split(new string[] { @"::" }, StringSplitOptions.RemoveEmptyEntries);
            counter = strIds.Length;
            foreach (string partId in strIds)
                // проверить является ли часть числом
                if (Int32.TryParse(partId, out iId) == false)
                    iRes++;
                else
                    ;

            return iRes;
        }
        /// <summary>
        /// Получить новый идентификатор уровня в структуре элемента
        ///  по его идентификатору
        /// </summary>
        /// <param name="strId">Строковый идентификатор элемента</param>
        /// <returns>Уровень элемента</returns>
        private ID_LEVEL getLevelOfId(string strId)
        {
            int cnt = -1
                , levelOffset = -1;

            levelOffset = getLevelOffset(strId, out cnt);
            // вернуть идентификатор уровня, при этом учесть, что
            //  часть ИД не являющаяся числом, представляет собой виртуальный уровень
            return m_listIDLevels[cnt - 1 - levelOffset];
        }
        /// <summary>
        /// обработчик события - изменение выбранного элемента в древовидной структуре
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (TreeView)</param>
        /// <param name="ev">Аргумент события</param>
        private void TreeView_AfterSelect(object obj, TreeViewEventArgs ev)
        {
            int iRes = -1
                , idAlg = -1
                , cntLevel = -1, offsetLevel = -1;

            offsetLevel = getLevelOffset(ev.Node.Name, out cntLevel);
            //Строка с идентификатором параметра алгоритма расчета ТЭП
            string strIdAlg = getIdNodePart(ev.Node.Name, ID_LEVEL.N_ALG);
            //Проверить условие возможности определения идентификатора текущего параметра алгоритма
            if (strIdAlg.Equals(string.Empty) == true)
                if ((cntLevel - 1) > (int)ID_LEVEL.TASK)
                    m_Level = ID_LEVEL.TEP_OUT;
                else
                    m_Level = ID_LEVEL.TASK;
            else
            {
                if (Int32.TryParse(strIdAlg, out idAlg) == true)
                    //Индекс текущего уровня в "дереве"
                    m_Level = getLevelOfId(ev.Node.Name);
                else
                    ; //??? throw new Exception ();
            }
            //Идентификатор текущего параметра алгоритма
            m_idAlg = ! (idAlg < (int)ID_START_RECORD.ALG) ? idAlg : -1;
            //Установить доступность для кнопок "Добавить", "Удалить"
            btnEnable(getIdNodePart(ev.Node.Name, ID_LEVEL.TASK));

            switch (m_Level)
            {
                case ID_LEVEL.TASK: //Задача
                    iRes = nodeAfterSelect(ev.Node, m_arTableDictPrj[(int)INDEX_TABLE_DICTPRJ.TASK], m_Level, false);
                    break;
                case ID_LEVEL.N_ALG: //Параметр алгоритма
                    iRes = nodeAfterSelect(ev.Node, m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM], m_Level, false);
                    if ((iRes == 0)
                        && (m_idAlg > 0))
                        //nodeAfterSelectDetail(INDEX_TABLE_KEY.TIME, strIdAlg, @"ID_TIME=")
                        nodeAfterSelectDetail(INDEX_TABLE_DICTPRJ.COMP_LIST, strIdAlg, @"ID_COMP=")
                        ;
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
                //case ID_LEVEL.TIME:
                //    iRes = nodeAfterSelect(ev.Node, m_arTableDictPrj[(int)INDEX_TABLE_KEY.TIME], m_Level, false);
                //    if (iRes == 0)
                //        nodeAfterSelectDetail(INDEX_TABLE_KEY.COMP_LIST, strIdAlg, @"ID_TIME=" + getIdNodePart (ev.Node.Name, m_Level) + @" AND ID_COMP=");
                //    else
                //        ;
                //    break;
                //case ID_LEVEL.COMP: //???
                //    break;
                case ID_LEVEL.PUT:
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
            m_dgvPrjProp.Rows.Clear();
            int idNode = -1;
            if (Int32.TryParse (getIdNodePart(node.Name, level), out idNode) == true)
            //if (m_idAlg > 0)
            {
                DataRow[] rowsProp = tblProp.Select(@"ID=" +                    
                    idNode
                    //m_idAlg
                );
                if (rowsProp.Length == 1)
                {
                    //Заполнение содержимым...
                    foreach (DataColumn col in tblProp.Columns)
                        m_dgvPrjProp.Rows.Add(new object[] { col.ColumnName, rowsProp[0][col.ColumnName].ToString().Trim() });
                }
                else
                    iErr = -1;
            }
            else
                //iErr = -2
                ;

            if (bThrow == true)
                switch (iErr)
                {
                    case -1:
                        throw new Exception(@"HPanelEditTree::nodeAfterSelect () - отсутствие (или дублирование) записи...");
                    //case -2:
                    //    throw new Exception(@"HPanelEditTree::nodeAfterSelect () - отсутствует идентификатор...");
                    default:
                        break;
                }
            else
                ;


            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblDGV3Desc_View = false;

            return iErr;
        }
        /// <summary>
        /// Заполнить список детализации
        /// </summary>
        /// <param name="key">Индекс таблицы</param>
        /// <param name="strIdAlg">Значение идентификатора - прямое условие отбора записей в таблице</param>
        /// <param name="where">Дополнительное условие отбора записей в таблице</param>        
        private void nodeAfterSelectDetail(INDEX_TABLE_DICTPRJ key, string strIdAlg, string where)
        {
            DataRow[] rowsPut;
            bool bUpdate = m_dgvPrjDetail.Rows.Count == m_arTableDictPrj[(int)key].Rows.Count;
            int indxRow = -1;

            //Отменить обработку события
            //if (bUpdate == true)
            m_dgvPrjDetail.CellValueChanged -= dgvPrjDetail_onCellValueChanged;
            //else ;

            foreach (DataRow rDetail in m_arTableDictPrj[(int)key].Rows)
            {
                //rowsPut = getDetailRows(Int32.Parse(strIdAlg), where, Convert.ToInt32(rDetail[@"ID"]));
                rowsPut = m_arTableEdit[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + Int32.Parse(strIdAlg)
                                + @" AND " + where + rDetail[@"ID"]);

                //if (!(m_Level == prevLevel))
                //if (indxRow < dgv.Rows.Count)
                if (bUpdate == false)
                    //Заполнить "список" детализации
                    m_dgvPrjDetail.Rows.Add(new object[] { rDetail[@"DESCRIPTION"], rowsPut.Length > 0 });
                else
                    //Обновить "список" детализации
                    //???снова используется индекс...
                    m_dgvPrjDetail.Rows[++indxRow].Cells[1].Value = rowsPut.Length > 0;
            }

            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblDGV3Desc_View = true;

            //Добавить обработку события
            //if (bUpdate == true)
                m_dgvPrjDetail.CellValueChanged += dgvPrjDetail_onCellValueChanged;
            //else ;
        }
        /// <summary>
        /// Обработчик события - завершение редактирования подписи элемента древовидной структуры
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (дерево)</param>
        /// <param name="ev">Аргумент события</param>
        private void TreeView_AfterLabelEdit(object obj, NodeLabelEditEventArgs ev)
        {
            switch (m_Level)
            {
                case ID_LEVEL.TASK: //Задача - не редактируется
                //case ID_LEVEL.TIME: //Интервал времени - не редактируется
                //case ID_LEVEL.COMP: //???Компонент станции - не редактируется
                case ID_LEVEL.PUT: //???Компонент станции - не редактируется
                    ev.CancelEdit = true;
                    break;
                case ID_LEVEL.N_ALG: //Параметр алгоритма
                    DataRow[] rowsProp = m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM].Select(@"ID=" + getIdNodePart(ev.Node.Name, m_listIDLevels[ev.Node.Level]));
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Обработчик события - завершение редактирования значения ячейки объекта 'm_dgvPrjProp'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (отображение 'm_dgvPrjProp')</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvProp_onCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            string strThrow = string.Empty
                , strId = string.Empty;
            //Только для уровня "Номер в алгоритме"
            if ((isDataGridViewPropReadOnly () == false)
                && (ev.ColumnIndex == 1)            
                )
            {
                TreeNode nodeSel = m_ctrlTreeView.SelectedNode
                    , nodeParent = nodeSel.Parent;
                DataGridView dgv = obj as DataGridView;
                INDEX_PARAMETER indxParameter = m_Level == ID_LEVEL.N_ALG ? INDEX_PARAMETER.ALGORITM :
                    m_Level == ID_LEVEL.PUT ? INDEX_PARAMETER.PUT : INDEX_PARAMETER.UNKNOWN;
                DataRow[] rowsAlg = null;

                if (!(indxParameter == INDEX_PARAMETER.UNKNOWN))
                {
                    strId = getIdNodePart(nodeSel.Name, m_Level);
                    rowsAlg = m_arTableEdit[(int)indxParameter].Select(@"ID=" +
                        //m_idAlg
                        strId
                        );

                    if (rowsAlg.Length == 1)
                    {
                        string strNameField = dgv.Rows[ev.RowIndex].Cells[0].Value.ToString().Trim()
                            , strVal = dgv.Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value.ToString().Trim();
                        //Сохранить новое значение
                        rowsAlg[0][strNameField] = strVal;

                        if ((m_Level == ID_LEVEL.N_ALG) &&
                            ((strNameField.Equals(@"N_ALG") == true)
                                || (strNameField.Equals(@"NAME_SHR") == true)))
                            //Изменить подпись в "дереве"
                            (Controls.Find(INDEX_CONTROL.TREECTRL_PRJ_ALG.ToString(), true)[0] as TreeView).SelectedNode.Text =
                                rowsAlg[0][@"N_ALG"].ToString().Trim() + @" (" + rowsAlg[0][@"NAME_SHR"].ToString().Trim() + @")";
                        else
                            ;
                    }
                    else
                        strThrow = @"для ID=" + strId + @" найдено " + rowsAlg.Length + @" записей";
                }
                else
                    strThrow = @"неизвестный тип таблицы (LEVEL=" + m_Level.ToString () + @")...";
            }
            else
                strThrow = @"для LEVEL=" + m_Level.ToString () + @" редактирование запрещено";

            if (strThrow.Equals(string.Empty) == false)
                throw new Exception(@"HPanelEditTree_dgvPropCellEndEdit () - " + strThrow + @"...");
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменение текущей строки в 'm_dgvPrjProp'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (отображение 'm_dgvPrjProp')</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvPrjDetail_onSelectionChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события - изменение значения ячейки объекта 'm_dgvPrjDetail'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (отображение 'm_dgvPrjDetail')</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvPrjDetail_onCellValueChanged(object obj, DataGridViewCellEventArgs ev)
        {
            if (ev.ColumnIndex == 1)
            {
                DataGridView dgv;
                DataRow[] rowsDetail;
                string strDetail = string.Empty
                    , strIdDetail = string.Empty
                    , strErr = @"НЕТ ДАННЫХ";
                int iKey = -1
                    , id = -1
                    , idPut = -1
                    , err = -1;

                //dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_DETAIL] as DataGridView;
                dgv = obj as DataGridView;

                switch (m_Level)
                {
                    case ID_LEVEL.N_ALG:
                        iKey = (int)INDEX_TABLE_DICTPRJ.COMP_LIST;
                        strErr = @"компонента станции";
                        idPut = 0;
                        break;
                    //case ID_LEVEL.TIME:
                    //    iKey = (int)INDEX_TABLE_KEY.COMP_LIST;
                    //    strErr = @"компонента станции";
                    //    idPut = 0;
                    //    break;
                    default:
                        break;
                }

                strDetail = dgv.Rows[ev.RowIndex].Cells[0].Value.ToString().Trim();
                //???используется поиск по описанию
                rowsDetail = m_arTableDictPrj[iKey].Select(@"DESCRIPTION='" + strDetail + @"'");
                //Проверить кол-во строк (д.б. ОДНа и ТОЬКО ОДНа)
                if (rowsDetail.Length == 1)
                {
                    id = Convert.ToInt32(rowsDetail[0][@"ID"]);

                    if (bool.Parse(dgv.Rows[ev.RowIndex].Cells[1/*ev.ColumnIndex*/].Value.ToString()) == true)
                    {//Добавить элемент...
                        //Оппределить строку с идентификаторами
                        if (idPut == 0)
                        {//Только для реального параметра (для компонента станции)
                            idPut = DbTSQLInterface.GetIdNext(m_arTableEdit[(int)INDEX_PARAMETER.PUT], out err);
                            if (idPut == 0) idPut += (int)ID_START_RECORD.PUT; else ;

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
                                addRowToTablePut(idPut, id);
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

        protected override void HPanelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
            string desc = string.Empty;
            string name = string.Empty;
            try
            {
                if (((DataGridView)obj).SelectedRows.Count > 0)
                    if (!(Descriptions[(int)ID_DT_DESC.PROP].Columns.IndexOf(@"ID_TABLE=") < 0))
                    {
                        name = ((DataGridView)obj).SelectedRows[0].Cells[0].Value.ToString();
                        foreach (DataRow r in Descriptions[(int)ID_DT_DESC.PROP].Select("ID_TABLE=" + (int)m_Level))
                        {
                            if (name == r["PARAM_NAME"].ToString())
                            {
                                desc = r["DESCRIPTION"].ToString();
                            }
                        }
                    }
                    else
                        Logging.Logg().Error(@"PanelPrjParametersEditTree::HPanelEdit_dgvPropSelectionChanged () - не найдено поле [ID_TABLE] в [" + Descriptions[(int)ID_DT_DESC.PROP].TableName + @"] ..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelPrjParametersEditTree::HPanelEdit_dgvPropSelectionChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            SetDescSelRow(desc, name);
        }
    }
}
