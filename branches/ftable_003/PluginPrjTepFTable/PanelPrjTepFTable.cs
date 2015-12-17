using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...
using System.Drawing; //Graphics
using ZedGraph;
//using DataGridViewAutoFilter; //фильт значений "А3"

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjTepFTable
{
    /// <summary>
    /// Панель для "ведения" таблицы со значениями нормативных графиков
    /// </summary>
    public class PanelPrjTepFTable : HPanelTepCommon //HPanelEditListCommon
    {
        DataTable m_tblOrigin
            , m_tblEdit;
        ZedGraphFTable m_zGraph_fTABLE; // график фукнции

        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_DELETE,
            BUTTON_ADD, BUTTON_UPDATE,
            DGV_NALG, DGV_VALUES,
            LABEL_DESC, INDEX_CONTROL_COUNT,

            ZGRAPH_fTABLE, CHRTGRAPH_fTABLE,
            TEXTBOX_FIND, LABEL_FIND, PANEL_FIND,
            TABLELAYOUTPANEL_CALC, BUTTON_CALC,
            TEXTBOX_A1, TEXTBOX_A2, TEXTBOX_A3,
            TEXTBOX_F, TEXTBOX_REZULT, GRPBOX_CALC,
            COMBOBOX_PARAM
        };

        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить", @"Добавить", @"Удалить" };

        /// <summary>
        /// Установить признак активности текущему объекту
        /// </summary>
        /// <param name="activate">Признак направления активации</param>
        /// <returns>Результат актвации</returns>
        public override bool Activate(bool activate)
        {
            return base.Activate(activate);
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            DataGridView dgv = null;
            List<string> listNAlg;
            string strItem = string.Empty;
            m_tblOrigin = DbTSQLInterface.Select(ref dbConn, "SELECT * FROM [dbo].[ftable]", null, null, out err);

            if (err == 0)
            {
                m_tblEdit = m_tblOrigin.Copy();
                m_zGraph_fTABLE.Set(m_tblEdit);

                dgv = Controls.Find (INDEX_CONTROL.DGV_NALG.ToString (), true)[0] as DataGridView;
                listNAlg = new List<string>();

                //var distinctRows = (from DataRow r in m_tblOrigin.Rows select new { nalg = r["N_ALG"] }).Distinct();

                foreach (DataRow r in m_tblEdit.Rows)
                {
                    strItem = ((string)r[@"N_ALG"]).Trim();
                    if (listNAlg.Contains(strItem) == false)
                    {
                        listNAlg.Add(strItem);
                        dgv.Rows.Add(strItem, string.Empty, r[@"ID"]);
                    }
                    else
                        ;
                }
            }

            Logging.Logg().Debug(@"PluginTepPrjFTable::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        /// <summary>
        /// Функция динамического поиска
        /// </summary>
        /// <param name="text">искомый элемент</param>
        private void nALGVisibled(string text)
        {
            string where = string.Format("{0} like '{1}%'", new object[] { "N_ALG", text });
            bool bVisible = false;
            DataRow[] rowsEquale = m_tblEdit.Select(where);
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0]);

            foreach (DataGridViewRow rView in dgv.Rows)
            {
                bVisible = false;

                foreach (DataRow rEqu in rowsEquale)
                {
                    bVisible = rView.Cells[@"Функция"].Value.ToString().Equals(((string)rEqu[@"N_ALG"]).Trim());
                    if (bVisible == true)
                        break;
                    else
                        ;
                }

                rView.Visible = bVisible;
            }
        }

        /// <summary>
        /// Обработчик события - изменение выбранной строки
        ///  в отображении для таблицы с наименованями функций
        /// </summary>
        /// <param name="obj">Объект, инициировавший событий (отображение таблицы значений)</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvnALG_onSelectionChanged(object obj, EventArgs ev)
        {
            DataGridView dgv = obj as DataGridView;
            //TextBox tbValue = null;
            FTable.FRUNK runk = FTable.FRUNK.F1; // для блокировки/снятия с отображения столбцов
            DataRow[] rowsNAlg = null;
            int iSelIndex = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].Index : -1;

            for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 1); indx++)
                (Controls.Find(indx.ToString(), true)[0] as TextBox).TextChanged -= tbCalcValue_onTextChanged;

            dgv = Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0] as DataGridView;
            dgv.Rows.Clear();

            if (!(iSelIndex < 0))
            {
                runk = m_zGraph_fTABLE.GetRunk(NAlg);
                
                rowsNAlg = m_tblEdit.Select(@"N_ALG='" + NAlg + @"'");

                foreach (DataRow r in rowsNAlg)
                    dgv.Rows.Add(((float)r[@"A1"]).ToString(CultureInfo.InvariantCulture)
                        , ((float)r[@"A2"]).ToString(CultureInfo.InvariantCulture)
                        , ((float)r[@"A3"]).ToString(CultureInfo.InvariantCulture)
                        , ((float)r[@"F"]).ToString(CultureInfo.InvariantCulture)
                        , r[@"ID"]
                );

                switch (runk)
                {
                    case FTable.FRUNK.F1:// блокировать/снять с отображения 2-ой, 3-ий столбец
                        dgv.Columns[1].Visible =
                        dgv.Columns[2].Visible =
                            false;
                        break;
                    case FTable.FRUNK.F2:// блокировать/снять с отображения 3-ий столбец
                        dgv.Columns[1].Visible = true;
                        dgv.Columns[2].Visible = false;
                        break;
                    case FTable.FRUNK.F3:// ничего не блокируется
                        dgv.Columns[0].Visible =
                        dgv.Columns[1].Visible =
                        dgv.Columns[2].Visible =
                            true;
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 1); indx++)
                (Controls.Find(indx.ToString(), true)[0] as TextBox).TextChanged += new EventHandler (tbCalcValue_onTextChanged);
        }

        /// <summary>
        /// Обработчик события - изменение выбранной строки
        ///  в отображении для таблицы со значениями
        /// </summary>
        /// <param name="obj">Объект, инициировавший событий (отображение таблицы значений)</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvValues_onSelectionChanged(object obj, EventArgs ev)
        {
            DataGridView dgv = obj as DataGridView;
            FTable.FRUNK runk = FTable.FRUNK.F1; // для блокировки полей ввода
            TextBox tbValue = null; // элемент управления - поле для ввода текста
            int iSelIndex = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].Index : -1;

            //// отменить обработку событий "изменение текста", очистить поля ввода калькулятора
            //for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 1); indx++)
            //{
            //    tbValue = Controls.Find(indx.ToString(), true)[0] as TextBox;
            //    if (indx < INDEX_CONTROL.TEXTBOX_F)
            //        tbValue.TextChanged -= tbCalcValue_onTextChanged;
            //    else
            //        ;
            //    tbValue.Text = string.Empty;
            //}

            if (!(iSelIndex < 0))
            {
                runk = m_zGraph_fTABLE.GetRunk(NAlg);
                // установить новые значения в поля ввода для калькулятора
                for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F +1); indx++)
                {
                    tbValue = Controls.Find(indx.ToString(), true)[0] as TextBox;
                    tbValue.Text = dgv.Rows[iSelIndex].Cells[(int)(indx - INDEX_CONTROL.TEXTBOX_A1)].Value.ToString();
                }

                switch (runk)
                {
                    case FTable.FRUNK.F1:// блокировать 2-ое, 3-е поле ввода
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A2.ToString(), true)[0] as TextBox).Enabled =
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A3.ToString(), true)[0] as TextBox).Enabled =
                            false;
                        break;
                    case FTable.FRUNK.F2:// блокировать 3-е поле ввода
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A2.ToString(), true)[0] as TextBox).Enabled = true;
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A3.ToString(), true)[0] as TextBox).Enabled = false;
                        break;
                    case FTable.FRUNK.F3:// ничего не блокируется
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A1.ToString(), true)[0] as TextBox).Enabled =
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A2.ToString(), true)[0] as TextBox).Enabled =
                        (Controls.Find(INDEX_CONTROL.TEXTBOX_A3.ToString(), true)[0] as TextBox).Enabled =
                            true;
                        break;
                    default:
                        break;
                }
            }
            else
                ;
            //// восстановить обработчики событий
            //for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F); indx++)
            //    (Controls.Find(indx.ToString(), true)[0] as TextBox).TextChanged += new EventHandler(tbCalcValue_onTextChanged);
        }

        /// <summary>
        /// Обработка события при успешной синхронизации целевойй таблицы в БД
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrigin = m_tblEdit.Copy();
        }

        /// <summary>
        /// Метод синхронизации целевой таблицы в БД
        ///  (обновление, вставка, удаление записей)
        ///   в соответствии с изменениями
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="err">Признак ошибки при выполнении метода</param>
        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, @"ftable", @"ID", m_tblOrigin, m_tblEdit, out err);
        }

        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelPrjTepFTable(IPlugIn iFunc)
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
            dgv.Name = INDEX_CONTROL.DGV_NALG.ToString();
            i = INDEX_CONTROL.DGV_NALG;
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
            //Отменить возможность изменения высоты строк
            dgv.AllowUserToResizeRows = false;
            dgv.ColumnCount = 3;
            dgv.Columns[0].Name = "Функция";
            dgv.Columns[1].Name = "Описание";
            dgv.Columns[2].Name = "ID_REC";
            dgv.Columns[2].Visible = false;

            //Таблица с реперными точками 
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_VALUES.ToString();
            i = INDEX_CONTROL.DGV_VALUES;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 6);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 4);

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
            //Отменить возможность изменения высоты строк
            dgv.AllowUserToResizeRows = false;
            dgv.ColumnCount = 5;
            dgv.Columns[0].Name = "A1";
            dgv.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; 
            dgv.Columns[1].Name = "A2";
            dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns[2].Name = "A3";
            dgv.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns[3].Name = "F";
            dgv.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns[4].Name = "ID_REC";
            dgv.Columns[4].Visible = false;

            //Панель отображения графика
            this.m_zGraph_fTABLE = new ZedGraphFTable();
            this.m_zGraph_fTABLE.m_This.Name = INDEX_CONTROL.ZGRAPH_fTABLE.ToString();
            this.m_zGraph_fTABLE.m_This.Dock = DockStyle.Fill;
            this.Controls.Add(this.m_zGraph_fTABLE.m_This, 2, 0);
            this.SetColumnSpan(this.m_zGraph_fTABLE.m_This, 8);
            this.SetRowSpan(this.m_zGraph_fTABLE.m_This, 10);
            this.m_zGraph_fTABLE.m_This.AutoScaleMode = AutoScaleMode.Font;

            //
            System.Windows.Forms.ComboBox cmb_bxParam = new ComboBox();
            cmb_bxParam.Name = INDEX_CONTROL.COMBOBOX_PARAM.ToString();
            cmb_bxParam.Dock = DockStyle.Fill;

            //Панель группировки калькулятора
            TableLayoutPanel tabl = new TableLayoutPanel();
            tabl.Name = INDEX_CONTROL.TABLELAYOUTPANEL_CALC.ToString();
            tabl.Dock = DockStyle.Fill;
            tabl.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            tabl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            //Подписи для калькулятора
            System.Windows.Forms.Label lblValue = new System.Windows.Forms.Label();
            lblValue.Dock = DockStyle.Bottom;
            lblValue.Text = @"Значение A1";
            tabl.Controls.Add(lblValue, 0, 0);
            //
            lblValue = new System.Windows.Forms.Label();
            lblValue.Dock = DockStyle.Bottom;
            lblValue.Text = @"Значение A2";
            tabl.Controls.Add(lblValue, 1, 0);
            //
            lblValue = new System.Windows.Forms.Label();
            lblValue.Dock = DockStyle.Bottom;
            lblValue.Text = @"Значение A3";
            tabl.Controls.Add(lblValue, 2, 0);
            //
            lblValue = new System.Windows.Forms.Label();
            lblValue.Dock = DockStyle.Bottom;
            lblValue.Text = @"Результат";            
            tabl.Controls.Add(lblValue, 0, 2);
            //
            lblValue = new System.Windows.Forms.Label();
            lblValue.Dock = DockStyle.Bottom;
            (lblValue as System.Windows.Forms.Label).Text = @"Значение F";
            tabl.Controls.Add(lblValue, 3, 0);

            //Текстовые поля для данных калькулятора
            TextBox tbValue = new TextBox();
            tbValue.Name = INDEX_CONTROL.TEXTBOX_A1.ToString();
            tbValue.TextChanged += tbCalcValue_onTextChanged;
            tbValue.TextAlign = HorizontalAlignment.Right;
            tbValue.Dock = DockStyle.Fill;
            tabl.Controls.Add(tbValue, 0, 1);

            tbValue = new TextBox();
            tbValue.Name = INDEX_CONTROL.TEXTBOX_A2.ToString();            
            tbValue.TextChanged += tbCalcValue_onTextChanged;
            tbValue.TextAlign = HorizontalAlignment.Right;
            tbValue.Dock = DockStyle.Fill;
            tabl.Controls.Add(tbValue, 1, 1);

            tbValue = new TextBox();
            tbValue.Name = INDEX_CONTROL.TEXTBOX_A3.ToString();            
            tbValue.TextChanged += tbCalcValue_onTextChanged;
            tbValue.TextAlign = HorizontalAlignment.Right;
            tbValue.Dock = DockStyle.Fill;
            tabl.Controls.Add(tbValue, 2, 1);            

            tbValue = new TextBox();
            tbValue.Name = INDEX_CONTROL.TEXTBOX_F.ToString();
            tbValue.TextChanged += tbCalcValue_onTextChanged;
            tbValue.TextAlign = HorizontalAlignment.Right;
            tbValue.Dock = DockStyle.Fill;
            tbValue.ReadOnly = true;
            tabl.Controls.Add(tbValue, 3, 1);

            tbValue = new TextBox();
            tbValue.Name = INDEX_CONTROL.TEXTBOX_REZULT.ToString();
            tbValue.Dock = DockStyle.Fill;
            tbValue.ReadOnly = true;
            tabl.Controls.Add(tbValue, 0, 3);
            tabl.SetColumnSpan(tbValue, 2);

            Button btn_rez = new Button();
            btn_rez.Name = INDEX_CONTROL.BUTTON_CALC.ToString();
            btn_rez.Text = "REZ";
            btn_rez.Dock = DockStyle.Top;
            tabl.Controls.Add(btn_rez, 3, 3);            

            tabl.RowCount = 4;
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tabl.ColumnCount = 4;
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //
            GroupBox gpBoxCalc = new GroupBox();
            gpBoxCalc.Name = INDEX_CONTROL.GRPBOX_CALC.ToString();
            gpBoxCalc.Text = @"Калькулятор значений";
            gpBoxCalc.Dock = DockStyle.Fill;
            gpBoxCalc.Controls.Add(tabl);
            this.Controls.Add(gpBoxCalc, 0, 10);
            this.SetColumnSpan(gpBoxCalc, 5);
            this.SetRowSpan(gpBoxCalc, 3);
            //
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());

            ResumeLayout(false);
            PerformLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTable_btnAdd_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTAble_btnDelete_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_CALC.ToString(), true)[0]).Click += new EventHandler(PluginPrjTepFTable_ClickRez);

            //Обработчики событий
            // для отображения таблиц
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0]).SelectionChanged += new EventHandler (dgvnALG_onSelectionChanged);
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0]).SelectionChanged += new EventHandler(dgvValues_onSelectionChanged);
            // для поля ввода при поиске функции
            ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).TextChanged += new EventHandler(PluginPrjTepFTable_TextChanged);
        }

        /// <summary>
        /// Обрабоотка клика по кнопке результат
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PluginPrjTepFTable_ClickRez(object sender, EventArgs e)
        {
            string text = string.Empty;
            int m_indx = -1
                , countArg = -1;

            //string text = ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_A1.ToString(), true)[0]).Text;
            //int m_indx = nameALG.IndexOf(":");
            //int countArg = Convert.ToInt32(nameALG[m_indx + 1].ToString());

            //ArgApproxi = new string[countArg + 1];

            for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 1); indx++)
            {
                TextBox tbValue = Controls.Find(indx.ToString(), true)[0] as TextBox;
            }
        }

        /// <summary>
        /// Строка - наименование текущей (выбранной) функции
        /// </summary>
        private string NAlg
        {
            get
            {
                string strRes = string.Empty;
                DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0] as DataGridView;

                strRes = (string)dgv.Rows[dgv.SelectedRows[0].Index].Cells[@"Функция"].Value;
                
                return strRes;
            }
        }

        /// <summary>
        /// Событие изменения текстового поля
        /// (функция поиска)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginPrjTepFTable_TextChanged(object sender, EventArgs e)
        {
            nALGVisibled((sender as TextBox).Text);
        }

        /// <summary>
        /// Обработка клика по таблице со значениями.
        /// Изменение чекбокса, построение графика.
        /// </summary>
        /// <param name="sender">объект</param>
        /// <param name="e">событие</param>
        private void PluginPrjTepFTable_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv =
                //((DataGridView)Controls.Find(INDEX_CONTROL.DGV_PROP.ToString(), true)[0])
                sender as DataGridView
                ;

            if (e.RowIndex > -1)
            {
                if (dgv.Columns[e.ColumnIndex].Name == "check")
                {
                    DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (chk.Value == chk.FalseValue || chk.Value == null)
                    {
                        chk.TrueValue = true;
                        chk.Value = chk.TrueValue;
                        this.Update();
                    }
                    else
                    {
                        chk.Value = chk.FalseValue;
                        this.Update();

                    }
                    dgv.EndEdit();
                }
                else
                {
                    int m_indx = NAlg.IndexOf(":");
                    string m_nameColumn = "A" + NAlg[m_indx + 1].ToString();

                    m_zGraph_fTABLE.CreateParamMassive(NAlg, m_nameColumn, e.RowIndex);
                    //referencePoint = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    m_zGraph_fTABLE.CheckAmountArg(e.RowIndex);
                }
            }
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Добавить"
        ///  (в зависимости от текущего отображения для таблицы: функция, значения)
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void HPanelfTable_btnAdd_Click(object obj, EventArgs ev)
        {
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Удалить"
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void HPanelfTAble_btnDelete_Click(object obj, EventArgs ev)
        {
        }

        /// <summary>
        /// Удалить запись (значение) для функции
        /// </summary>
        /// <param name="indx">??? Номер записи</param>
        protected void delRecItem(int indx)
        {
            m_tblEdit.Rows[indx].Delete();
            m_tblEdit.AcceptChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void tbCalcValue_onTextChanged(object obj, EventArgs ev)
        {
            FTable.FRUNK runk = m_zGraph_fTABLE.GetRunk(NAlg);
            float[] pars = new float[(int)runk + 1];
            for (int indx = 0; indx < pars.Length; indx++)
                pars[indx] =
                    float.Parse((Controls.Find(((INDEX_CONTROL)(indx + (int)INDEX_CONTROL.TEXTBOX_A1)).ToString (), true)[0] as TextBox).Text, CultureInfo.InvariantCulture);

            (Controls.Find(INDEX_CONTROL.TEXTBOX_REZULT.ToString(), true)[0] as TextBox).Text =
               m_zGraph_fTABLE.Calculate(NAlg,pars).ToString (@"F2");
        }
    }

    /// <summary>
    /// Класс для взаимодействия с сервером (вызывающем приложением)
    /// </summary>
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
            createObject(typeof(PanelPrjTepFTable));

            base.OnClickMenuItem(obj, ev);
        }
    }
}