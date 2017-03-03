using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms; //DataGridView
using System.Data.Common; //DbConnection
using System.Data; //DataTable

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public abstract class HPanelEditListCommon : HPanelCommon
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

        protected override void reinit()
        {
            // перед повтрной инициализацией - очистить
            m_tblEdit.Clear();
            m_tblOrigin.Clear();

            base.reinit();
        }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbValues();
        }

        protected override void recUpdateInsertDelete(out int err)
        {
            m_handlerDb.RecUpdateInsertDelete(m_nameTable, m_strKeyFields, string.Empty, m_tblOrigin, m_tblEdit, out err);
        }

        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrigin = m_tblEdit.Copy();
        }
    }

    partial class HPanelEditList
    {
        protected class DataGridViewEditable : DataGridView
        {
            public enum TYPE : short { UNKNOWN = -1
                , LIST, PROPERTY
                    , COUNT }

            private TYPE m_type;

            private int m_iId;

            public event EventHandler EventSelectionChanged;
            public event EventHandler EventValueChanged;

            public DataGridViewEditable(TYPE type)
            {
                this.m_type = type;

                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                switch (m_type)
                {
                    case TYPE.LIST:
                        //Добавить столбец
                        this.Columns.AddRange(new DataGridViewColumn[] {
                            new DataGridViewTextBoxColumn ()
                        });
                        //Ширина столбца по ширине род./элемента управления
                        this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;                        
                        //Установить режим "невидимые" заголовки столбцов
                        this.ColumnHeadersVisible = false;                        
                        break;
                    case TYPE.PROPERTY:
                        //Добавить столбцы
                        this.Columns.AddRange(new DataGridViewColumn[] {
                            new DataGridViewTextBoxColumn ()
                            , new DataGridViewTextBoxColumn ()
                        });
                        //1-ый столбец
                        this.Columns[0].HeaderText = @"Свойство"; this.Columns[0].ReadOnly = true;
                        //Ширина столбца по ширине род./элемента управления
                        this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        //2-ой столбец
                        this.Columns[1].HeaderText = @"Значение";
                        //Ширина столбца по ширине род./элемента управления
                        this.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;                        
                        //Запретить вставку строк
                        this.AllowUserToAddRows = false;
                        break;
                    default:
                        break;
                }

                //Запретить выделение "много" строк
                this.MultiSelect = false;
                //Установить режим выделения - "полная" строка
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Запретить удаление строк
                this.AllowUserToDeleteRows = false;

                //Обработчик события "Выбор строки"
                this.SelectionChanged += new EventHandler(onSelectionChanged);
                //Обработчик события "Редактирование свойства"
                this.CellEndEdit += new DataGridViewCellEventHandler(onCellEndEdit);
            }

            private void onSelectionChanged(object obj, EventArgs ev)
            {
                EventSelectionChanged(this, EventArgs.Empty);
            }

            private void onCellEndEdit(object obj, DataGridViewCellEventArgs ev)
            {
                EventValueChanged(this, EventArgs.Empty);
            }

            public void ShowValues(int id, DataRow row)
            {
                m_iId = id;
            }

            public void ShowValues(int id, DataTable table)
            {
                m_iId = id;
            }
        }

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
        ///  содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            DataGridView dgv = new DataGridView();
            //Добавить "список" словарных величин
            dgv.Name = INDEX_CONTROL.DGV_DICT_ITEM.ToString();
            i = INDEX_CONTROL.DGV_DICT_ITEM;
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
            dgv.SelectionChanged += new EventHandler(HPanelEditList_dgvDictEditSelectionChanged);
            ////Обработчик события "Редактирование строки"
            //dgv.CellStateChanged += new DataGridViewCellEventHandler(HPanelEdit_dgvDictEditCellStateChanged);
            //Обработчик события "Редактирование строки"
            dgv.CellEndEdit += new DataGridViewCellEventHandler(HPanelEditList_dgvDictEditCellEndEdit);
            //Запретить удаление строк
            dgv.AllowUserToDeleteRows = false;

            //Добавить "список" свойств словарной величины
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_DICT_PROP.ToString ();
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 5, 0);
            this.SetColumnSpan(dgv, 8); this.SetRowSpan(dgv, 10);
            //Добавить столбцы
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                    new DataGridViewTextBoxColumn ()
                    , new DataGridViewTextBoxColumn ()
                });
            //1-ый столбец
            dgv.Columns[0].HeaderText = @"Свойство"; dgv.Columns[0].ReadOnly = true;
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
            //Запретить вставку строк
            dgv.AllowUserToAddRows = false;
            dgv.MultiSelect = false;

            addLabelDesc(INDEX_CONTROL.LABEL_PROP_DESC.ToString ());


            this.ResumeLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find (INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(HPanelEditList_btnAdd_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(HPanelEditList_btnDelete_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
        }

        #endregion      
    }

    public abstract partial class HPanelEditList : HPanelEditListCommon
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

        //public override bool Activate(bool activate)
        //{
        //    bool bRes = base.Activate(activate);

        //    return bRes;
        //}

        protected override void reinit()
        {
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).Rows.Clear();
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).Rows.Clear();

            base.reinit();
        }

        protected virtual void initProp(out int err, out string errMsg)
        {
            err = -1;
            errMsg = string.Empty;

            m_tblEdit = m_handlerDb.GetDataTable (m_nameTable, out err);
            m_tblOrigin = m_tblEdit.Copy();

            if (err == 0)
            {
                DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]);
                //Заполнение содержимым...
                for (int i = 0; i < m_tblEdit.Columns.Count; i++)
                {
                    dgv.Rows.Add(new object[] { m_tblEdit.Columns[i].ColumnName, string.Empty });
                }
                //Только "для чтения", если строк нет
                dgv.ReadOnly = !(m_tblEdit.Rows.Count > 0);
            }
            else
            {
                errMsg = @"не удалось получить значения из целевой таблицы [" + m_nameTable + @"]";
                err = -1;
            }
        }

        protected override void initialize(out int err, out string errMsg)
        {
            int i = -1;
            if (((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).RowCount > 1 || ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).RowCount > 0)
            {
                err = 0;
                errMsg = string.Empty;
            }
            else
            {
                initProp(out err, out errMsg);

                if (err == 0)
                {
                    DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]);
                    //Заполнение содержимым...
                    for (i = 0; i < m_tblEdit.Rows.Count; i++)
                        dgv.Rows.Add(new object[] { m_tblEdit.Rows[i][m_nameDescField].ToString().Trim() });

                    Logging.Logg().Debug(@"HPanelEditList::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    ;
            }
        }

        private void setCellsReadOnly()
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]);
            //Строка с полем в 'DictEdit' "для чтения"
            dgv.Rows[m_tblEdit.Columns.IndexOf(m_nameDescField)].ReadOnly =
            ////Крайняя строка "для чтения" - только, если в грид можно добавлть строки (AllowRowsAddedUser = true, NewRowIndex > -1)
            //dgv.Rows[m_tblEdit.Columns.Count].ReadOnly =
            //1-ый столбец "для чтения"
            dgv.Columns[0].ReadOnly =
                true;
        }

        /// <summary>
        /// Получение значения для объекта 'DataGridView'
        /// </summary>
        /// <param name="dgvProp">объект 'DataGridView' для отображения свойств</param>
        /// <param name="indxItem">индекс записи в таблице со словарными велечинами</param>
        /// <param name="indxProp">индекс строки объекта 'DataGridView' для отображения свойств</param>
        /// <returns>значение поля (по 'indxProp') записи (по 'indxItem') редактируемой таблицы</returns>
        protected virtual string getTableEditValue(DataGridView dgvProp, int indxItem, int indxProp)
        {
            return m_tblEdit.Rows[indxItem][dgvProp.Rows[indxProp].Cells[0].Value.ToString()].ToString().Trim();
        }

        //В том числе и для отображения актуальной "подсказки" для свойства
        private void HPanelEditList_dgvDictEditSelectionChanged(object obj, EventArgs ev)
        {
            int indx = -1;

            if (((DataGridView)obj).SelectedRows.Count == 1)
            {
                indx = ((DataGridView)obj).SelectedRows[0].Index;

                DataGridView dgvProp = Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0] as DataGridView;
                bool bReadOnly = !((!(indx < 0)) && (indx < m_tblEdit.Rows.Count));

                if (bReadOnly == false)
                    for (int i = 0; i < dgvProp.RowCount; i++)
                        dgvProp.Rows[i].Cells[1].Value = getTableEditValue(dgvProp, indx, i);
                else
                    for (int i = 0; i < dgvProp.RowCount; i++)
                        dgvProp.Rows[i].Cells[1].Value = string.Empty;

                dgvProp.ReadOnly = bReadOnly;
                if (bReadOnly == false)
                    setCellsReadOnly();
                else
                    ;

                Console.WriteLine(@"HPanelEditList_dgvDictEditSelectionChanged () - dgvProp.ReadOnly = " + dgvProp.ReadOnly + @" ...");
            }
            else
                Logging.Logg().Error(@"HPanelEditList::HPanelEdit_SelectionChanged () - выделена НЕ 1 строка", Logging.INDEX_MESSAGE.NOT_SET);

        }

        //private void 

        protected virtual void setTableEditValue(DataGridView dgvProp, int indxRow, int indxCol)
        {
            m_tblEdit.Rows[((DataGridView)(Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0])).SelectedRows[0].Index][m_tblEdit.Columns[indxRow].ColumnName] =
                dgvProp.Rows[indxRow].Cells[indxCol].Value as string;
        }

        //Для редактирования свойства
        private void HPanelEdit_dgvDictPropCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            if (!(((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value == null))
            {
                string strItemProp = getTableEditValue(obj as DataGridView
                                                    , ((DataGridView)Controls.Find (INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).SelectedRows[0].Index
                                                    , ev.RowIndex);

                //Сравнить предыдущее и текущее свойство
                if (strItemProp.Equals(((DataGridView)obj).Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value as string) == false)
                    //Если разные, то присвоить новое значение
                    setTableEditValue(obj as DataGridView, ev.RowIndex, ev.ColumnIndex);
                else
                    ; //Отмена редактирования
            }
            else
                ; //Отмена редактирования
        }

        //Заполнение содержимым...
        protected virtual object[] getValues(string valCellEditing)
        {
            DataGridView dgv;
            //dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_ITEM]);
            //string valEdit = dgv.Rows[dgv.RowCount].Cells[0].Value as string; //??? 0 == ev.ColumnIndex, dgv.RowCount == ev.RowIndex
            dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]);
            
            object valProp;
            object[] arRes = new object[m_tblEdit.Columns.Count];
            for (int i = 0; i < m_tblEdit.Columns.Count; i++)
                if (m_tblEdit.Columns[i].ColumnName.Equals(m_nameDescField) == false)
                {
                    if (m_tblEdit.Columns[i].DataType.IsPrimitive == true)
                        valProp = m_tblEdit.Rows.Count + 1;
                    else
                        valProp = m_tblEdit.Columns[i].ColumnName;

                    dgv.Rows[i].Cells[1].Value =
                    arRes[i] =
                        valProp;
                }
                else
                {
                    dgv.Rows[i].Cells[1].Value =
                    arRes[i] =
                        valCellEditing;
                }

            return arRes;
        }

        protected virtual void addRecItem(object [] vals)
        {
            m_tblEdit.Rows.Add(vals);
        }

        private void HPanelEditList_dgvDictEditCellEndEdit(object obj, DataGridViewCellEventArgs ev)
        {
            int indx = ev.RowIndex;
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
                        //Заполнение содержимым...
                        addRecItem(getValues (valEdit));

                        ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]).ReadOnly = false;
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
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]);
            dgv.Rows[dgv.NewRowIndex].Cells[0].Selected = true;
            dgv.BeginEdit(false);
        }

        protected virtual void delRecItem(int indx)
        {
            m_tblEdit.Rows[indx].Delete();
            m_tblEdit.AcceptChanges();
        }

        private void HPanelEditList_btnDelete_Click(object obj, EventArgs ev)
        {
            int indx = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
            {//Удаление существующей записи
                delRecItem(indx);

                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).Rows.RemoveAt(indx);                
            }
            else
                ;
        }        
    }

}
