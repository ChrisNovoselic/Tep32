using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...
using System.Drawing; //Graphics
using ZedGraph;
using System.Windows.Forms.DataVisualization.Charting;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjTepFTable
{
    public class PluginPrjTepFTable : HPanelTepCommon
    {
        string m_query;
        DataTable m_tblOrign,
            m_tableEdit;
        ZedGraph.ZedGraphControl m_zGraph_fTABLE;
        System.Windows.Forms.DataVisualization.Charting.Chart m_chartGraph_fTABLE;

        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_DELETE,
            BUTTON_ADD, BUTTON_UPDATE,
            DGV_fTABLE, DGV_algTABLE,
            LABEL_DESC, INDEX_CONTROL_COUNT,
            ZGRAPH_fTABLE,
            TEXTBOX_FIND, LABEL_FIND, PANEL_FIND
        };

        /// <summary>
        /// 
        /// </summary>
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить", @"Добавить", @"Удалить" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public override bool Activate(bool activate)
        {
            return base.Activate(activate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            int i = -1;
            string strConn = "SELECT * FROM [TEP_NTEC_5].[dbo].[ftable]";

            if (err == 0)
            {
                fillALGTable(ref dbConn, out err, out errMsg);
                m_tblOrign = DbTSQLInterface.Select(ref dbConn, strConn, null, null, out err);
            }
            else ;

            Logging.Logg().Debug(@"PluginTepPrjFTable::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        /// <summary>
        /// Класс - общий для графического представления значений
        /// </summary>
        private class HZedGraph : ZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZedGraph()
                : base()
            {
                initializeComponent();
            }

            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZedGraph(IContainer container)
                : this()
            {
                container.Add(this);
            }

            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
            {
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;
            }

            private void Graph()
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        /// <param name="strErr"></param>
        /// <param name="query">строка запроса</param>
        /// <returns></returns>
        private DataTable SqlConn(ref DbConnection dbConn, out int err, out string strErr, string m_query)
        {
            err = 0;
            strErr = string.Empty;
            DataTable m_tblwork = new DataTable();

            return m_tblwork = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);
        }

        /// <summary>
        /// Заполнение таблицы с функциями
        /// </summary>
        /// <param name="dbConn">подключение</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr">ошибка</param>
        private void fillALGTable(ref DbConnection dbConn, out int err, out string strErr)
        {
            string m_query = "SELECT DISTINCT N_ALG, DESCRIPTION FROM [TEP_NTEC_5].[dbo].[ftable] ORDER BY N_ALG ";

            m_tableEdit = SqlConn(ref dbConn, out err, out strErr, m_query);

            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);

            checkNumberRows(m_tableEdit.Rows.Count, dgv);

            for (int i = 0; i < m_tableEdit.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_tableEdit.Rows[i]["N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_tableEdit.Rows[i]["DESCRIPTION"];
            }
        }

        private void Chart()
        {
                       //m_chartGraph_fTABLE = new System.Windows.Forms.DataVisualization.Charting.Chart();
            //m_chartGraph_fTABLE.Series[0].
        }

        /// <summary>
        /// Добавление строк
        /// </summary>
        /// <param name="count">кол-во строк/param>
        /// <param name="dgv">датагрид</param>
        private void checkNumberRows(int count, DataGridView dgv)
        {
            if (dgv.RowCount > 0)
            {
                dgv.Rows.Clear();

                for (int i = 0; i < count; i++)
                {
                    dgv.Rows.Add();
                }
            }

            else
            {
                for (int i = 0; i < count; i++)
                {
                    dgv.Rows.Add();
                }
            }


        }

        /// <summary>
        /// Функция поиска
        /// </summary>
        /// <param name="text">искомый элемент</param>
        private void m_findALG(string text)
        {
            string m_fltr = string.Format("{0} like '{1}%'", new object[] { "N_ALG", text });
            DataRow[] m_drSearch = m_tableEdit.Select(m_fltr);
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);

            checkNumberRows(m_drSearch.Count(), dgv);

            for (int i = 0; i < m_drSearch.Count(); i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_drSearch[i][@"N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_drSearch[i][@"DESCRIPTION"];
            }
        }

        /// <summary>
        /// Отображение реперных точек функции
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void showprmFunc(string nameALG)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            string m_fName = "N_ALG = '" + nameALG + "'";
            DataRow[] m_drValues = m_tblOrign.Select(m_fName);

            checkNumberRows(m_drValues.Count(), dgv);

            for (int i = 0; i < m_drValues.Count(); i++)
            {
                dgv.Rows[i].Cells["A1"].Value = m_drValues[i][2];
                dgv.Rows[i].Cells["A2"].Value = m_drValues[i][3];
                dgv.Rows[i].Cells["A3"].Value = m_drValues[i][4];
                dgv.Rows[i].Cells["F"].Value = m_drValues[i][5];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrign = m_tableEdit.Copy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, @"ftable", @"ID", m_tblOrign, m_tableEdit, out err);
        }

        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        /// <param name="iFunc"></param>
        public PluginPrjTepFTable(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        private void InitializeComponent()
        {
            DataGridView dgv = null;
            //Control ctrl = null;

            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;

            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            //Поиск функции
            TextBox txtbx_find = new TextBox();
            txtbx_find.Name = INDEX_CONTROL.TEXTBOX_FIND.ToString();
            txtbx_find.Dock = DockStyle.Fill;

            //Подпись поиска
            System.Windows.Forms.Label lbl_find = new System.Windows.Forms.Label();
            lbl_find.Name = INDEX_CONTROL.LABEL_FIND.ToString();
            lbl_find.Dock = DockStyle.Bottom;
            (lbl_find as System.Windows.Forms.Label).Text = @"Поиск";

            //Группировка поиска 
            //и его подписи
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Name = INDEX_CONTROL.PANEL_FIND.ToString();
            tlp.Dock = DockStyle.Fill;
            tlp.AutoSize = true;
            tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlp.Controls.Add(lbl_find);
            tlp.Controls.Add(txtbx_find);
            this.Controls.Add(tlp, 1, 0);
            this.SetColumnSpan(tlp, 4);
            this.SetRowSpan(tlp, 1);

            //Таблица с функциями
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_algTABLE.ToString();
            i = INDEX_CONTROL.DGV_algTABLE;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 1);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 5);

            dgv.ReadOnly = true;
            //Запретить выделение "много" строк
            dgv.MultiSelect = false;
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            dgv.ColumnHeadersVisible = true;
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //Ширина столбцов под видимую область
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnCount = 2;
            dgv.Columns[0].Name = "Функция";
            dgv.Columns[1].Name = "Описание";

            //Таблица с реперными точками 
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_fTABLE.ToString();
            i = INDEX_CONTROL.DGV_fTABLE;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 6);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 5);

            dgv.ReadOnly = true;
            //Запретить выделение "много" строк
            dgv.MultiSelect = false;
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            dgv.ColumnHeadersVisible = true;
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //Ширина столбцов под видимую область
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnCount = 4;
            dgv.Columns[0].Name = "A1";
            dgv.Columns[1].Name = "A2";
            dgv.Columns[2].Name = "A3";
            dgv.Columns[3].Name = "F";

            //
            m_zGraph_fTABLE = new HZedGraph();
            m_zGraph_fTABLE.Name = INDEX_CONTROL.ZGRAPH_fTABLE.ToString();
            m_zGraph_fTABLE.Dock = DockStyle.Fill;
            this.Controls.Add(m_zGraph_fTABLE, 2, 0);
            this.SetColumnSpan(m_zGraph_fTABLE, 8);
            this.SetRowSpan(m_zGraph_fTABLE, 9);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());

            ResumeLayout(false);
            PerformLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTable_btnAdd_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTAble_btnDelete_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
            //Обработчики событий
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]).CellContentClick += new DataGridViewCellEventHandler(PluginPrjTepFTable_CellContentClick);
            ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).TextChanged += new EventHandler(PluginPrjTepFTable_TextChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginPrjTepFTable_TextChanged(object sender, EventArgs e)
        {
            string text = ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).Text;

            m_findALG(text);
        }

        /// <summary>
        /// Щелчек по ячейки с функцией
        /// </summary>
        /// <param name="sender">объект</param>
        /// <param name="e">событие</param>
        private void PluginPrjTepFTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);
            string m_nameALG = dgv.Rows[e.RowIndex].Cells["Функция"].Value.ToString();

            showprmFunc(m_nameALG);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTable_btnAdd_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            dgv.Rows[dgv.NewRowIndex].Cells[0].Selected = true;
            dgv.BeginEdit(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTAble_btnDelete_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0] as DataGridView;

            int indx = dgv.SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tableEdit.Rows.Count))
            {//Удаление существующей записи
                delRecItem(indx);

                dgv.Rows.RemoveAt(indx);
            }
            else
                ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indx"></param>
        protected void delRecItem(int indx)
        {
            m_tableEdit.Rows[indx].Delete();
            m_tableEdit.AcceptChanges();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 16;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Нормативные графики";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginPrjTepFTable));

            base.OnClickMenuItem(obj, ev);
        }
    }
}
