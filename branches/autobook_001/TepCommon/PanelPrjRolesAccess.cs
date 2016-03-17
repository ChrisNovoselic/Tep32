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
    public abstract class PanelPrjRolesAccess : HPanelEditListCommon
    {
        string m_query;
        string m_nameTableAccessUnit;
        DataTable m_tblItem;
        protected DataTable m_tblAccessUnit;
        string m_strNameFieldValue;

        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_UPDATE
            , DGV_PRJ_ITEM, DGV_PRJ_ACCESS
            , LABEL_ACCESS_DESC
            , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить" };

        public PanelPrjRolesAccess(IPlugIn iFunc, string nameTableTarget, string idFields, string nameTableAccessUnit, string strNameFieldValue)
            : base(iFunc, nameTableTarget, idFields)
        {
            m_nameTableAccessUnit = nameTableAccessUnit;
            m_strNameFieldValue = strNameFieldValue;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Control ctrl = null;

            this.SuspendLayout();

            //Добавить кропки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;
            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            //Добавить "список" словарных величин
            ctrl = new DataGridView();
            ctrl.Name = INDEX_CONTROL.DGV_PRJ_ITEM.ToString();
            DataGridView dgv = ctrl as DataGridView;
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
            //Обработчик события "Выбор строки"
            dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjItemSelectionChanged);

            //Добавить "список" свойств словарной величины
            ctrl = new DataGridView();
            ctrl.Name = INDEX_CONTROL.DGV_PRJ_ACCESS.ToString ();
            dgv = ctrl as DataGridView;
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
            //Обработчик события "Выбор строки"
            dgv.SelectionChanged += new EventHandler(HPanelEditTree_dgvPrjAccessSelectionChanged);
            //Обработчик события "Редактирование свойства"
            //dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjAccessCellEndEdit);
            dgv.CellValueChanged += new DataGridViewCellEventHandler(HPanelEditTree_dgvPrjAccessCellValueChanged);

            addLabelDesc(INDEX_CONTROL.LABEL_ACCESS_DESC.ToString ());

            this.ResumeLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find (INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        protected override void initialize(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            
            int i = -1;

            m_tblEdit = m_handlerDb.GetDataTable(m_nameTable, out err);
            m_tblItem = m_handlerDb.GetDataTable(@"roles_unit", out err);

            selectAccessUnit(out err);
            m_tblOrigin = m_tblEdit.Copy();

            if (err == 0)
            {
                Logging.Logg().Debug(@"PanelPrjRolesAccess::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

                DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString (), true)[0] as DataGridView;
                if (m_tblAccessUnit.Rows.Count > 0)
                    for (i = 0; i < m_tblAccessUnit.Rows.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblAccessUnit.Rows[i][@"DESCRIPTION"], false });
                else
                    //Только "для чтения", если строк нет
                    dgv.ReadOnly = true;

                dgv = Controls.Find(INDEX_CONTROL.DGV_PRJ_ITEM.ToString(), true)[0] as DataGridView;
                if (m_tblItem.Rows.Count > 0)
                {
                    for (i = 0; i < m_tblItem.Rows.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblItem.Rows[i][@"DESCRIPTION"] });

                    setPrjAccessValues(0);
                }
                else
                    //Только "для чтения", если строк нет
                    dgv.ReadOnly = true;

                dgv = Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString(), true)[0] as DataGridView;
                if (m_tblAccessUnit.Rows.Count > 0)
                {                    
                }
                else
                    ;
            }

            else
                ;
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            return bRes;
        }

        protected override void reinit()
        {
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_PRJ_ITEM.ToString(), true)[0]).Rows.Clear();
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString(), true)[0]).Rows.Clear();

            base.reinit();
        }

        protected virtual void selectAccessUnit(out int err)
        {
            m_tblAccessUnit = m_handlerDb.GetDataTable (m_nameTableAccessUnit, out err);
        }

        private void HPanelEditTree_dgvPrjItemSelectionChanged(object obj, EventArgs ev)
        {
            DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString(), true)[0] as DataGridView;
            dgv.CellValueChanged -= HPanelEditTree_dgvPrjAccessCellValueChanged;
            
            if (((DataGridView)obj).SelectedRows.Count == 1)
                setPrjAccessValues(((DataGridView)obj).SelectedRows[0].Index);
            else
                ;

            dgv.CellValueChanged += HPanelEditTree_dgvPrjAccessCellValueChanged;
        }

        private void HPanelEditTree_dgvPrjAccessSelectionChanged(object obj, EventArgs ev)
        {
        }

        //private void HPanelEditTree_dgvPrjAccessCellEndEdit(object obj, DataGridViewCellEventArgs ev)

        private void HPanelEditTree_dgvPrjAccessCellValueChanged(object obj, DataGridViewCellEventArgs ev)
        {
            DataGridView dgvItem = Controls.Find(INDEX_CONTROL.DGV_PRJ_ITEM.ToString(), true)[0] as DataGridView
                , dgvAccess = Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString(), true)[0] as DataGridView;
            DataRow [] rowsUnit = m_tblAccessUnit.Select (@"DESCRIPTION='" + dgvAccess.SelectedRows[0].Cells[0].Value + @"'")
                , rowsAccess = m_tblEdit.Select(m_strKeyFields.Split(',')[0] + @"=" + m_tblItem.Rows[dgvItem.SelectedRows[0].Index][@"ID"]
                                    + @" AND IS_ROLE=1 AND "
                                    + m_strKeyFields.Split(',')[1] + @"="
                                        //+ m_tblAccessUnit.Rows [dgvAccess.SelectedRows[0].Index][@"ID"]);
                                        + rowsUnit[0][@"ID"]);

            if (rowsAccess.Length == 1)
            {
                rowsAccess[0][m_strNameFieldValue] = (((DataGridViewCheckBoxCell)dgvAccess.Rows[ev.RowIndex].Cells[ev.ColumnIndex]).Value.ToString() == true.ToString ()) ? 1 : 0;
            }
            else
                //??? Ошибка...
                throw new Exception(@"HPanelTepPrjRolesaccess::HPanelEditTree_dgvPrjAccessCellValueChanged () - дублирование(отсутствие) параметра...");
        }

        private void setPrjAccessValues(int indx)
        {
            DataRow[] rowsAccessUnit = m_tblEdit.Select(m_strKeyFields.Split(',')[0] + @"=" + m_tblItem.Rows[indx][@"ID"]
                + @" AND IS_ROLE=1");

            DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_PRJ_ACCESS.ToString(), true)[0] as DataGridView;

            if (rowsAccessUnit.Length == dgv.Rows.Count)
                for (int i = 0; i < dgv.Rows.Count; i++)
                    ((DataGridViewCheckBoxCell)dgv.Rows[i].Cells[1]).Value = Int16.Parse(rowsAccessUnit[i][m_strNameFieldValue].ToString()) == 1;
            else
                throw new Exception(@"PanelPrjRolesAccess::setPrjAccessValues () - кол-во строк в БД и элементе упр-я НЕ совпадает...");
        }
    }
}
