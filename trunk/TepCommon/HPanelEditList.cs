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
    public abstract class HPanelEditListCommon : HPanelTepCommon
    {
        protected DataTable m_tblEdit
            , m_tblOrigin;
        protected string m_nameTable;
        protected string m_strKeyFields;

        public HPanelEditListCommon(IPlugIn plugIn, string nameTable, string keyFields)
            : base(plugIn)
        {
            m_nameTable = nameTable;
            m_strKeyFields = keyFields;
        }

        protected override void clear()
        {
            m_tblEdit.Clear();
            m_tblOrigin.Clear();

            base.clear();
        }

        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, m_nameTable, m_strKeyFields, m_tblOrigin, m_tblEdit, out err);
        }

        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrigin = m_tblEdit.Copy();
        }
    }

    partial class HPanelEditList
    {
        protected enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE
            , DGV_DICT_ITEM, DGV_DICT_PROP
            , LABEL_PROP_DESC
            , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton((int)i, m_arButtonText[(int)i]);

            DataGridView dgv = null;
            //Добавить "список" словарных величин
            i = INDEX_CONTROL.DGV_DICT_ITEM;
            m_dictControls.Add((int)i, new DataGridView());
            dgv = ((DataGridView)m_dictControls[(int)i]);
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
            //Ширина столбца по ширине род./элемента управления
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //Обработчик события "Выбор строки"
            dgv.SelectionChanged += new EventHandler(HPanelEdit_dgvDictEditSelectionChanged);
            ////Обработчик события "Редактирование строки"
            //dgv.CellStateChanged += new DataGridViewCellEventHandler(HPanelEdit_dgvDictEditCellStateChanged);
            //Обработчик события "Редактирование строки"
            dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEdit_dgvDictEditCellEndEdit);
            //Запретить удаление строк
            dgv.AllowUserToDeleteRows = false;

            //Добавить "список" свойств словарной величины
            i = INDEX_CONTROL.DGV_DICT_PROP;
            m_dictControls.Add((int)i, new DataGridView());
            dgv = ((DataGridView)m_dictControls[(int)i]);
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 5, 0);
            this.SetColumnSpan(dgv, 8); this.SetRowSpan(m_dictControls[(int)i], 10);
            //Добавить столбцы
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewTextBoxColumn ()
                });
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
            ////Обработчик события "Редактирование свойства"
            //dgv.CellStateChanged += new DataGridViewCellStateChangedEventHandler(HPanelEdit_dgvDictPropStateChanged);
            //Обработчик события "Редактирование свойства"
            dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEdit_dgvDictPropCellEndEdit);
            //Запретить удаление строк
            dgv.AllowUserToDeleteRows = false;

            addLabelDesc((int)INDEX_CONTROL.LABEL_PROP_DESC);

            this.ResumeLayout();

            //Обработчика нажатия кнопок
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_ADD]).Click += new System.EventHandler(HPanelEditList_btnAdd_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_DELETE]).Click += new System.EventHandler(HPanelEditList_btnDelete_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_SAVE]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)m_dictControls[(int)INDEX_CONTROL.BUTTON_UPDATE]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        #endregion
    }

    public partial class HPanelEditList : HPanelEditListCommon
    {
        protected string m_nameDescField;

        public HPanelEditList(IPlugIn plugIn, string nameTable, string keyFields, string nameDescField)
            : base(plugIn, nameTable, keyFields)
        {
            InitializeComponent();

            m_nameDescField = nameDescField;
        }

        public HPanelEditList(IContainer container, IPlugIn plugIn, string nameTable, string keyFields, string nameDescField)
            : this(plugIn, nameTable, keyFields, nameDescField)
        {
            container.Add(this);
        }

        protected override void Activate(bool activate)
        {
        }

        protected override void clear()
        {
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows.Clear();
            ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]).Rows.Clear();

            base.clear();
        }

        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            int i = -1;

            err = -1;
            errMsg = string.Empty;

            m_tblEdit = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + m_nameTable, null, null, out err);
            m_tblOrigin = m_tblEdit.Copy();

            if (err == 0)
            {
                Logging.Logg().Debug(@"HPanelEdit::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

                DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
                //Заполнение содержимым...
                for (i = 0; i < m_tblEdit.Columns.Count; i++)
                    dgv.Rows.Add(new object[] { m_tblEdit.Columns[i].ColumnName, string.Empty } );
                //Только "для чтения", если строк нет
                dgv.ReadOnly = !(m_tblEdit.Rows.Count > 0);

                dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]);                
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

        private void setCellsReadOnly()
        {
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
            //Строка с полем в 'DictEdit' "для чтения"
            dgv.Rows[m_tblEdit.Columns.IndexOf(m_nameDescField)].ReadOnly =
                //Крайняя строка "для чтения"
            dgv.Rows[m_tblEdit.Columns.Count].ReadOnly =
                //1-ый столбец "для чтения"
            dgv.Columns[0].ReadOnly =
                true;
        }

        //В том числе и для отображения актуальной "подсказки" для свойства
        private void HPanelEdit_dgvDictEditSelectionChanged(object obj, EventArgs ev)
        {
            int indx = -1;

            if (((DataGridView)obj).SelectedRows.Count == 1)
            {
                indx = ((DataGridView)obj).SelectedRows[0].Index;

                ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).ReadOnly = !((!(indx < 0)) && (indx < m_tblEdit.Rows.Count));

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
                Logging.Logg().Error(@"HPanelEdit::HPanelEdit_SelectionChanged () - выделена НЕ 1 строка", Logging.INDEX_MESSAGE.NOT_SET);
        }

        //Для редактирования свойства
        private void HPanelEdit_dgvDictPropCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            if (!(((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value == null))
                if (m_tblEdit.Rows[((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]).SelectedRows[0].Index][m_tblEdit.Columns[ev.RowIndex].ColumnName].ToString().Equals(
                    ((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string) == false)
                    m_tblEdit.Rows[((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]).SelectedRows[0].Index][m_tblEdit.Columns[ev.RowIndex].ColumnName] =
                        ((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string;
                else
                    ; //Отмена редактирования
            else
                ; //Отмена редактирования
        }

        private void HPanelEdit_dgvDictEditCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            int indx = ((DataGridView)obj).SelectedRows[0].Index;
            string valEdit = ((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string;

            if (!(valEdit == null))
            {
                if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
                {//Редактирование существующей записи
                    if (m_tblEdit.Rows[indx][m_nameDescField].ToString().Equals(valEdit) == false)
                        m_tblEdit.Rows[indx][m_nameDescField] = valEdit;
                    else
                        ; //Отмена редактирования
                }
                else
                {//Добавили новую
                    if (valEdit.Equals(string.Empty) == false)
                    {
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

                        m_tblEdit.Rows.Add(values);

                        dgv.ReadOnly = false;
                        setCellsReadOnly();
                    }
                    else
                        ; //Отмена редактирования
                }
            }
            else
                ; //Отмена редактирования
        }

        private void HPanelEditList_btnAdd_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]);
            dgv.Rows[dgv.NewRowIndex].Cells[0].Selected = true;
            dgv.BeginEdit(false);
        }

        private void HPanelEditList_btnDelete_Click(object obj, EventArgs ev)
        {
            int indx = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]).SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
            {//Удаление существующей записи
                m_tblEdit.Rows[indx].Delete();
                m_tblEdit.AcceptChanges();

                ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]).Rows.RemoveAt(indx);                
            }
            else
                ;
        }        
    }
}
