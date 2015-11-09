using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...

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
        //DataGridView dgv;

        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE,
            BUTTON_DELETE,
            BUTTON_ADD,
            BUTTON_UPDATE,
            DGV_fTABLE,
            LABEL_DESC
                , INDEX_CONTROL_COUNT
        };

        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить", @"Добавить", @"Удалить" };

        protected override void Activate(bool activate)
        {
            throw new NotImplementedException();
        }

        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            err = -1;
            errMsg = string.Empty;
            int i = -1;

            m_query = @"SELECT * FROM ftable";
            m_tblOrign = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);
            m_tableEdit = m_tblOrign.Copy();

            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_fTABLE]);
            dgv.DataSource = m_tableEdit;

            dgv.Columns[0].Width = 30;
            dgv.Columns[1].Width = 70;
            dgv.Columns[2].Width = 45;
            dgv.Columns[3].Width = 45;
            dgv.Columns[4].Width = 45;
            dgv.Columns[5].Width = 50;


            /* for (int j = 0; j < m_tableEdit.Columns.Count; j++)
                 dgv.Rows.Add(new object[] { m_tableEdit.Columns[j].ColumnName, string.Empty });
             //Только "для чтения", если строк нет
             dgv.ReadOnly = !(m_tableEdit.Rows.Count > 0);

             for (i = 0; i < m_tblOrign.Rows.Count; i++)
                dgv.Rows.Add(new object[] { m_tblOrign.Rows[i].ToString().Trim() });*/

            Logging.Logg().Debug(@"PluginTepPrjFTable::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrign = m_tableEdit.Copy();
        }

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, @"ftable", @"ID", m_tblOrign, m_tableEdit, out err);
        }

        public PluginPrjTepFTable(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;
            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton((int)i, m_arButtonText[(int)i]);

            //Добавить "список" словарных величин
            m_dictControls.Add((int)INDEX_CONTROL.DGV_fTABLE, new DataGridView());
            DataGridView dgv = m_dictControls[(int)INDEX_CONTROL.DGV_fTABLE] as DataGridView;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 0);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 13);

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

            addLabelDesc((int)INDEX_CONTROL.LABEL_DESC);

            this.ResumeLayout();


            //Обработчика нажатия кнопок
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_ADD]).Click += new System.EventHandler(HPanelfTable_btnAdd_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_DELETE]).Click += new System.EventHandler(HPanelfTAble_btnDelete_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_SAVE]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_UPDATE]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTable_btnAdd_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_fTABLE]);
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
            int indx = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_fTABLE]).SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tableEdit.Rows.Count))
            {//Удаление существующей записи
                delRecItem(indx);

                ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_fTABLE]).Rows.RemoveAt(indx);
            }
            else
                ;
        }

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
