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
        private enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE
            , TREECTRL_PRJ_ALG
            , DGV_PRJ_PUT
            , LABEL_PARAM_DESC
            , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        protected TreeView m_ctrlTreeView
        {
            get { return m_dictControls[(int)INDEX_CONTROL.TREECTRL_PRJ_ALG] as TreeView; }
        }

        public HPanelEditTree(IPlugIn plugIn)
            : base(plugIn)
        {
            m_arTableOrigin = new DataTable[(int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER];
            m_arTableEdit = new DataTable[(int)INDEX_PARAMETER.COUNT_INDEX_PARAMETER];
            
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
            m_ctrlTreeView.AfterSelect += new TreeViewEventHandler(TreeView_PrjAlgAfterSelect);

            //Добавить "список" свойств словарной величины
            i = INDEX_CONTROL.DGV_PRJ_PUT;
            m_dictControls.Add((int)i, new DataGridView());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(m_dictControls[(int)i], 5, 0);
            this.SetColumnSpan(m_dictControls[(int)i], 8); this.SetRowSpan(m_dictControls[(int)i], 10);
            //Добавить столбцы
            ((DataGridView)m_dictControls[(int)i]).Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewTextBoxColumn ()
                });
            //1-ый столбец
            ((DataGridView)m_dictControls[(int)i]).Columns[0].HeaderText = @"Свойство"; ((DataGridView)m_dictControls[(int)i]).Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            ((DataGridView)m_dictControls[(int)i]).Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //2-ой столбец
            ((DataGridView)m_dictControls[(int)i]).Columns[1].HeaderText = @"Значение";
            //Установить режим выделения - "полная" строка
            ((DataGridView)m_dictControls[(int)i]).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            ((DataGridView)m_dictControls[(int)i]).Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

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
            err = 0;
            strErr = string.Empty;

            //Заполнить оригинальные таблицы из БД...
            m_arTableOrigin [(int)INDEX_PARAMETER.ALGORITM] = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM inalg", null, null, out err);
            if (err == 0)
            {
                m_arTableOrigin[(int)INDEX_PARAMETER.PUT] = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM input", null, null, out err);

                if (err == 0)
                {
                    m_tblTime = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM time", null, null, out err);

                    if (err == 0)
                        m_tblCompList = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM comp_list", null, null, out err);
                    else
                        strErr = @"HPanelEditTree::initialize () - заполнение таблицы со значенями из словаря 'интервалы времени' ...";
                }
                else
                    strErr = @"HPanelEditTree::initialize () - заполнение таблицы с параметрами ...";
            }
            else
                strErr = @"HPanelEditTree::initialize () - заполнение таблицы с параметрами АЛГоритма ...";

            if (err == 0)
            {//Только если обе выборки  рез-м = 0 (УСПЕХ)
                //Копии оригинальных таблиц для редактирования и последующего сравнения с оригигальными...
                m_arTableEdit[(int)INDEX_PARAMETER.ALGORITM] = m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Copy();
                m_arTableEdit[(int)INDEX_PARAMETER.PUT] = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Copy();

                if (m_tblTime.Rows.Count > 0)
                    //Заполнить "дерево" элементами 1-го уровня (ALGORITM)
                    if (m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Rows.Count > 0)
                    {
                        TreeNode nodeAlg
                            , nodeTime
                            , nodePut;
                        DataRow[] rowPuts;
                        foreach (DataRow r in m_arTableOrigin[(int)INDEX_PARAMETER.ALGORITM].Rows)
                        {
                            string strIdAlg = r[@"ID"].ToString().Trim();
                            nodeAlg = m_ctrlTreeView.Nodes.Add(strIdAlg, r[@"N_ALG"].ToString().Trim());

                            foreach (DataRow rr in m_tblTime.Rows)
                            {
                                rowPuts = m_arTableOrigin[(int)INDEX_PARAMETER.PUT].Select(@"ID_ALG=" + strIdAlg + @" AND ID_TIME=" + rr[@"ID"]);
                                if (rowPuts.Length > 0)
                                {
                                    nodeTime = nodeAlg.Nodes.Add(strIdAlg + @"_" + rr[@"ID"]);
                                    foreach (DataRow rrr in rowPuts)
                                    {
                                        nodePut = nodeTime.Nodes.Add(rrr[@"ID"].ToString ().Trim (), m_tblCompList.Select (@"ID_COMP=" + rrr[@"ID_COMP"])[0][@"NAME_SHR"].ToString ().Trim());
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                    }
                    else
                    {
                        m_ctrlTreeView.Nodes.Add(null, @"Параметры отсутствуют...");
                    }
                else
                    ;

                m_ctrlTreeView.SelectedNode = m_ctrlTreeView.Nodes[0];
            }
            else
                strErr = @"HPanelEditTree::initialize () - заполнение таблицы со значенями из словаря 'компоненты станции' ...";
        }

        protected override void Activate(bool activate)
        {
        }

        protected override void clear()
        {
            base.clear();
        }

        private void HPanelEditTree_btnAdd_Click(object obj, EventArgs ev)
        {
            TreeNode nodeSel = m_ctrlTreeView.SelectedNode;

            if (!(nodeSel == null))
            {
                int level = nodeSel.Level;

                switch (level)
                {
                    case 0:
                        nodeSel.Nodes.Add((nodeSel.Nodes.Count + 1) + @"-ый параметр");
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
            throw new NotImplementedException();
        }
    }

    partial class HPanelEditTree
    {
        private enum INDEX_PARAMETER {ALGORITM, PUT, COUNT_INDEX_PARAMETER};
        private DataTable [] m_arTableOrigin
            , m_arTableEdit;
        private DataTable m_tblTime
            , m_tblCompList;

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        private void TreeView_PrjAlgAfterSelect(object obj, TreeViewEventArgs ev)
        {
            TreeViewAction act = ev.Action;
            TreeNode nodeAlg = ev.Node;
            algAfterSelect();
        }

        private void algAfterSelect()
        {
        }
    }
}
