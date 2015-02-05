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
    public partial class HPanelEdit : TableLayoutPanel, IObjectDictEdit
    {
        private DataTable m_tblEdit
            , m_tblOrigin;
        private ConnectionSettings m_connSett;
        protected string m_nameTable
            , m_nameDescField;
        private string m_query;

        //Дополнительные действия при сохранении значений
        protected DelegateIntFunc delegateSaveAdding;

        public HPanelEdit(string nameTable, string nameDescField)
        {
            InitializeComponent();

            m_nameTable = nameTable;
            m_nameDescField = nameDescField;
        }

        public HPanelEdit(IContainer container, string nameTable, string nameDescField)
            : this(nameTable, nameDescField)
        {
            container.Add(this);
        }

        protected void Activate (bool activate) {
        }

        private void clear()
        {
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows.Clear();
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_EDIT]).Rows.Clear();

            m_tblEdit.Clear();
            m_tblOrigin.Clear();
        }

        public void Initialize(object obj)
        {
            try
            {
                if (this.IsHandleCreated == true)
                    //if (this.InvokeRequired == true)
                    this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"PanelTepDictPlugIns::Initialize () - BeginInvoke (initialize) - ...");
            }
        }

        private void initialize(out int err, out string errMsg)
        {
            int i = -1
                , iListenerId = -1;

            err = -1;
            errMsg = string.Empty;

            iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB");
            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((!(dbConn == null)) && (err == 0))
            {
                m_query = @"SELECT * FROM " + m_nameTable;
                m_tblEdit = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);
                m_tblOrigin = m_tblEdit.Copy();

                if (err == 0)
                {
                    Logging.Logg().Debug(@"HPanelEdit::initialize () - усПех ...");

                    DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
                    //Обработчик события "Выбор строки"
                    dgv.SelectionChanged += new EventHandler(HPanelEdit_dgvDictPropSelectionChanged);
                    ////Обработчик события "Редактирование свойства"
                    //dgv.CellStateChanged += new DataGridViewCellStateChangedEventHandler(HPanelEdit_dgvDictPropStateChanged);
                    //Обработчик события "Редактирование свойства"
                    dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEdit_dgvDictPropCellEndEdit);
                    //Запретить удаление строк
                    dgv.AllowUserToDeleteRows = false;
                    //Заполнение содержимым...
                    for (i = 0; i < m_tblEdit.Columns.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblEdit.Columns[i].ColumnName, string.Empty });

                    dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_EDIT]);
                    //Обработчик события "Выбор строки"
                    dgv.SelectionChanged += new EventHandler(HPanelEdit_dgvDictEditSelectionChanged);
                    ////Обработчик события "Редактирование строки"
                    //dgv.CellStateChanged += new DataGridViewCellEventHandler(HPanelEdit_dgvDictEditCellStateChanged);
                    //Обработчик события "Редактирование строки"
                    dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEdit_dgvDictEditCellEndEdit);
                    //Запретить удаление строк
                    dgv.AllowUserToDeleteRows = false;
                    //Заполнение содержимым...
                    for (i = 0; i < m_tblEdit.Rows.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblEdit.Rows[i][m_nameDescField].ToString().Trim() });
                }
                else
                {
                    errMsg = @"не удалось получить значения из целевой таблицы [" + m_nameTable + @"]";
                    err = -1;
                }
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);
        }

        private void initialize(object obj)
        {
            int err = -1;
            string errMsg = string.Empty;

            if (((EventArgsDataHost)obj).par[0] is ConnectionSettings)
            {
                m_connSett = (ConnectionSettings)((EventArgsDataHost)obj).par[0];

                err = 0;
            }
            else
                errMsg = @"не корректен тип объекта с параметрами соедиения";

            if (err == 0)
            {
                initialize(out err, out errMsg);
            }
            else
                ;

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::initialize () - " + errMsg);
            }
            else
            {
            }
        }

        private void setCellsReadOnly()
        {
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
            //Строка с полем в 'DictEdit' "для чтения"
            dgv.Rows[m_tblEdit.Columns.IndexOf (m_nameDescField)].ReadOnly =
            //Крайняя строка "для чтения"
            dgv.Rows[m_tblEdit.Columns.Count].ReadOnly =
            //1-ый столбец "для чтения"
            dgv.Columns[0].ReadOnly =
                true;
        }

        //Для отображения актуальной "подсказки" для свойства
        private void HPanelEdit_dgvDictPropSelectionChanged(object obj, EventArgs ev)
        {
        }

        //В том числе и для отображения актуальной "подсказки" для свойства
        private void HPanelEdit_dgvDictEditSelectionChanged(object obj, EventArgs ev)
        {
            int indx = -1;

            if (((DataGridView)obj).SelectedRows.Count == 1)
            {
                indx = ((DataGridView)obj).SelectedRows[0].Index;

                ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).ReadOnly = ! ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count));

                if (((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).ReadOnly == false)
                {
                    for (int i = 0; i < ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).NewRowIndex; i++)
                        ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows[i].Cells[1].Value = m_tblEdit.Rows[indx][((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows[i].Cells[0].Value.ToString()].ToString().Trim();

                    setCellsReadOnly();
                }
                else
                    for (int i = 0; i < ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).NewRowIndex; i++)
                        ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows[i].Cells[1].Value = string.Empty;
            }
            else
                Logging.Logg().Error(@"HPanelEdit::HPanelEdit_SelectionChanged () - выделена НЕ 1 строка");
        }

        //Для редактирования свойства
        private void HPanelEdit_dgvDictPropCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            m_tblEdit.Rows[((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_EDIT]).SelectedRows[0].Index][m_tblEdit.Columns[ev.RowIndex].ColumnName] =
                ((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string;
        }

        private void HPanelEdit_dgvDictEditCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            int indx = ((DataGridView)obj).SelectedRows[0].Index;
            string valEdit = ((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string;

            if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
            {//Редактирование существующей записи
                m_tblEdit.Rows[indx][m_nameDescField] = valEdit;
            }
            else
            {//Добавили новую
                DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
                //Заполнение содержимым...
                object valProp;
                object[] values = new object[m_tblEdit.Columns.Count];
                for (int i = 0; i < m_tblEdit.Columns.Count; i++)
                    if (m_tblEdit.Columns[i].ColumnName.Equals(m_nameDescField) == false)
                    {
                        if (m_tblEdit.Columns[i].DataType.IsPrimitive == true)
                            valProp = m_tblEdit.Rows.Count + 1;
                        else
                            valProp = m_tblEdit.Columns[i].ColumnName;

                        dgv.Rows[i].Cells[1].Value =
                        values[i] =
                            valProp;
                    }
                    else
                    {
                        dgv.Rows[i].Cells[1].Value =
                        values[i] =
                            valEdit;
                    }

                m_tblEdit.Rows.Add (values);

                dgv.ReadOnly = false;
                setCellsReadOnly();
            }
        }

        private void HPanelEdit_btnAdd_Click(object obj, EventArgs ev)
        {
        }

        private void HPanelEdit_btnDelete_Click(object obj, EventArgs ev)
        {
            int indx = ((DataGridView)obj).SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
            {//Удаление существующей записи
                m_tblEdit.Rows[indx].Delete ();
            }
            else
                ;
        }

        private void HPanelEdit_btnSave_Click(object obj, EventArgs ev)
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1;
            string errMsg = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((!(dbConn == null)) && (err == 0))
            {
                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, m_nameTable, m_tblOrigin, m_tblEdit, out err);

                if (!(err == 0))
                {
                    errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
                }
                else
                {
                    m_tblOrigin = m_tblEdit.Copy();

                    if (!(delegateSaveAdding == null))
                        delegateSaveAdding(iListenerId);
                    else
                        ;
                }
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnSave_Click () - " + errMsg);
            }
            else
            {
            }
        }

        private void HPanelEdit_btnUpdate_Click(object obj, EventArgs ev)
        {
            clear();

            int err = -1;
            string errMsg = string.Empty;

            initialize (out err, out errMsg);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnUpdate_Click () - " + errMsg);
            }
            else
            {
            }
        }
    }
}
