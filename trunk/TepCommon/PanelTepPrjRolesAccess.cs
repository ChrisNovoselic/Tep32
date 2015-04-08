using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public class PanelTepPrjRolesAccess : HPanelEditListCommon
    {
        string m_query;
        string m_nameTableAccessUnit;
        DataTable m_tblItem, m_tblAccessUnit;

        private enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_UPDATE
            , DGV_PRJ_ITEM, DGV_PRJ_ACCESS
            , LABEL_ACCESS_DESC
            , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить" };

        public PanelTepPrjRolesAccess(IPlugIn iFunc, string nameTableTarget, string idFields, string nameTableAccessUnit)
            : base(iFunc, nameTableTarget, idFields)
        {
            m_nameTableAccessUnit = nameTableAccessUnit;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кропки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;
            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton((int)i, m_arButtonText[(int)i]);

            //Добавить "список" словарных величин
            m_dictControls.Add((int)INDEX_CONTROL.DGV_PRJ_ITEM, new DataGridView());
            DataGridView dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ITEM] as DataGridView;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 0);
            this.SetColumnSpan(dgv, 4); this.SetRowSpan(dgv, 13);
            //Добавить столбец
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn ()
                });            
            //Запретить выделение "много" строк
            dgv.MultiSelect = false;
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            dgv.ColumnHeadersVisible = false;            
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //Не отображать заголовки столбцов
            dgv.ColumnHeadersVisible = false;
            //1-ый столбец (только "для чтения")
            dgv.Columns[0].ReadOnly = true;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //Добавить "список" свойств словарной величины
            m_dictControls.Add((int)INDEX_CONTROL.DGV_PRJ_ACCESS, new DataGridView());
            dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ACCESS] as DataGridView;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 5, 0);
            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 10);
            //Добавить столбцы
            (dgv).Columns.AddRange(new DataGridViewColumn[] {
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
            dgv.Columns[1].HeaderText = @"Доступ";
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            addLabelDesc((int)INDEX_CONTROL.LABEL_ACCESS_DESC);

            this.ResumeLayout();

            //Обработчика нажатия кнопок
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_SAVE]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_UPDATE]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        protected override void initialize(ref DbConnection dbConn, out int err, out string strErr)
        {
            int i = -1;

            err = 0;
            strErr = string.Empty;
            m_query = @"SELECT * FROM " + m_nameTable;
            m_tblEdit = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);
            m_tblOrigin = m_tblEdit.Copy();
            m_tblItem = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + @"roles_unit", null, null, out err);
            m_tblAccessUnit = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + m_nameTableAccessUnit, null, null, out err);

            if (err == 0)
            {
                Logging.Logg().Debug(@"PanelTepPrjRolesAccess::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

                DataGridView dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ACCESS] as DataGridView;
                if (m_tblAccessUnit.Rows.Count > 0)
                {
                    for (i = 0; i < m_tblAccessUnit.Rows.Count; i++)
                    {
                        dgv.Rows.Add(new object[] { m_tblAccessUnit.Rows[i][@"DESCRIPTION"], false });
                    }

                    //Обработчик события "Выбор строки"
                    dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjAccessSelectionChanged);
                    //Обработчик события "Редактирование свойства"
                    dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjAccessCellEndEdit);
                    //dgv.CellValueChanged += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjAccessCellEndEdit);
                }
                else
                    //Только "для чтения", если строк нет
                    dgv.ReadOnly = true;
                
                dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ITEM] as DataGridView;
                if (m_tblItem.Rows.Count > 0)
                {
                    for (i = 0; i < m_tblItem.Rows.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblItem.Rows[i][@"DESCRIPTION"] });

                    //Обработчик события "Выбор строки"
                    dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjItemSelectionChanged);

                    setPrjAccessValues(0);
                }
                else
                    //Только "для чтения", если строк нет
                    dgv.ReadOnly = true;
            }

            else
                ;
        }

        protected override void Activate(bool activate)
        {
        }

        protected override void clear()
        {
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ITEM]).Rows.Clear();
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ACCESS]).Rows.Clear();

            base.clear();
        }

        private void HPanelEditTree_dgvPrjItemSelectionChanged(object obj, EventArgs ev)
        {
            if (((DataGridView)obj).SelectedRows.Count == 1)
                setPrjAccessValues(((DataGridView)obj).SelectedRows[0].Index);
            else
                ;
        }

        private void HPanelEditTree_dgvPrjAccessSelectionChanged(object obj, EventArgs ev)
        {
        }

        private void HPanelEditTree_dgvPrjAccessCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            DataGridView dgvItem = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ITEM] as DataGridView
                , dgvAccess = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ACCESS] as DataGridView;
            DataRow[] rowsAccess = m_tblEdit.Select(m_strKeyFields.Split(',')[0] + @"=" + m_tblItem.Rows[dgvItem.SelectedRows[0].Index][@"ID"]
                                    + @" AND "
                                    + m_strKeyFields.Split(',')[1] + @"=" + m_tblAccessUnit.Rows [dgvAccess.SelectedRows[0].Index][@"ID"]);

            if (rowsAccess.Length == 1)
            {
                rowsAccess[0][@"IsUse"] = (((DataGridViewCheckBoxCell)dgvAccess.Rows[ev.RowIndex].Cells[ev.ColumnIndex]).Value.ToString() == true.ToString ()) ? 1 : 0;
            }
            else
                ; //??? Ошибка...
        }

        private void setPrjAccessValues(int indx)
        {
            DataRow[] rowsAccessUnit = m_tblEdit.Select(m_strKeyFields.Split(',')[0] + @"=" + m_tblItem.Rows[indx][@"ID"]);

            DataGridView dgv = m_dictControls[(int)INDEX_CONTROL.DGV_PRJ_ACCESS] as DataGridView;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                ((DataGridViewCheckBoxCell)dgv.Rows[i].Cells[1]).Value = Int16.Parse(rowsAccessUnit[i][@"IsUse"].ToString()) == 1;
            }
        }
    }
}
