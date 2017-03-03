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
    public class PanelPrjTepFTable : TepCommon.HPanelCommon //HPanelEditListCommon
    {
        DataTable m_tblOrigin
            , m_tblEdit;
        ZedGraphFTable m_zGraph_fTABLE; // график фукнции

        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            , BUTTON_ADD, BUTTON_DELETE , BUTTON_SAVE, BUTTON_UPDATE
            , MENUITEM_ADD_POINT, MENUITEM_ADD_FUNCTION, MENUITEM_DELETE_POINT, MENUITEM_DELETE_FUNCTION
            , DGV_NALG, DGV_VALUES
            , LABEL_DESC
                , INDEX_CONTROL_COUNT
            , ZGRAPH_fTABLE
                , TEXTBOX_FIND, LABEL_FIND, PANEL_FIND
            , TABLELAYOUTPANEL_CALC /*BUTTON_CALC,*/
            // обязательно должны следовать один за другим, т.к. используются в цикле
            , TEXTBOX_A1, TEXTBOX_A2, TEXTBOX_A3
                , TEXTBOX_F
            , /*TEXTBOX_REZULT,*/ GRPBOX_CALC
            , COMBOBOX_PARAM
        };

        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        /// <summary>
        /// Установить признак активности текущему объекту
        /// </summary>
        /// <param name="activate">Признак направления активации</param>
        /// <returns>Результат актвации</returns>
        public override bool Activate(bool activate)
        {
            bool activ = base.Activate(activate);

            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblDGV3Desc_View = true;

            return activ;
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении инициализации</param>
        /// <param name="errMsg">Строкаа с сообщением об ошибке (при наличии)</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            DataGridView dgv = null;
            List<string> listNAlg;
            string strItem = string.Empty;
            m_tblOrigin = m_handlerDb.GetDataTable(ID_DBTABLE.FTABLE, out err);

            if (err == 0)
            {
                m_tblEdit = m_tblOrigin.Copy();
                m_zGraph_fTABLE.Set(m_tblEdit);

                dgv = Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0] as DataGridView;
                listNAlg = new List<string>();

                if (dgv.RowCount > 0)
                    dgv.Rows.Clear();
                else ;
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
        /// Создать объект для взаимодействия с БД
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override HandlerDbValues createHandlerDb()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Установить признак отображения для строк
        ///  в соответствии с введенным в поле "Поиск"
        ///  части наименования (NAlg) функции
        /// </summary>
        /// <param name="text">Часть наименования функции</param>
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
        /// <param name="obj">Объект, инициировавший событий (отображение таблицы функций)</param>
        /// <param name="ev">Аргумент события</param>
        private void dgvnALG_onSelectionChanged(object obj, EventArgs ev)
        {
            DataGridView dgv = obj as DataGridView;
            //TextBox tbValue = null;
            FTable.FRUNK runk = FTable.FRUNK.F1; // для блокировки/снятия с отображения столбцов
            DataRow[] rowsNAlg = null;
            int iSelIndex = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].Index : -1;
            //Удалить все строки со значенями для прдыдущей функции
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

                switch (runk)
                {
                    case FTable.FRUNK.F1:
                    case FTable.FRUNK.F2:
                    case FTable.FRUNK.F3:
                        m_zGraph_fTABLE.Draw(NAlg);
                        break;
                    default:
                        break;
                }
            }
            else
                ;
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

            if (!(iSelIndex < 0))
            {
                // отменить обработку событий "изменение текста", очистить поля ввода калькулятора
                for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 0); indx++)
                {
                    tbValue = Controls.Find(indx.ToString(), true)[0] as TextBox;
                    if (indx < (INDEX_CONTROL.TEXTBOX_F + 0))
                        tbValue.TextChanged -= tbCalcValue_onTextChanged;
                    else
                        ;
                    tbValue.Text = string.Empty;
                }

                runk = m_zGraph_fTABLE.GetRunk(NAlg);
                // установить новые значения в поля ввода для калькулятора
                for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 1); indx++)
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

                // восстановить обработчики событий
                for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < (INDEX_CONTROL.TEXTBOX_F + 0); indx++)
                    (Controls.Find(indx.ToString(), true)[0] as TextBox).TextChanged += new EventHandler(tbCalcValue_onTextChanged);
            }
            else
                ; // нет ни одной выбранной строки
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
        /// <param name="err">Признак ошибки при выполнении метода</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            m_handlerDb.RecUpdateInsertDelete(@"ftable", @"ID", string.Empty, m_tblOrigin, m_tblEdit, out err);
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
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            DataGridView dgv = null;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            DropDownButton btnDropDown = null;
            int iButtonDropDownMenuItem = -1;
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            string[] arLabelButtonDropDownMenuItem = new string[] { @"точку", @"функцию" };
            INDEX_CONTROL indxControlButtonDropDownMenuItem = INDEX_CONTROL.UNKNOWN;
            ToolStripItem menuItem;

            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;

            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                switch (i)
                {
                    case INDEX_CONTROL.BUTTON_ADD:
                    case INDEX_CONTROL.BUTTON_DELETE:
                        btnDropDown = new DropDownButton();
                        addButton(btnDropDown, i.ToString(), (int)i, m_arButtonText[(int)i]);

                        btnDropDown.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                        if (i == INDEX_CONTROL.BUTTON_ADD)
                            strPartLabelButtonDropDownMenuItem = @"Добавить";
                        else
                            if (i == INDEX_CONTROL.BUTTON_DELETE)
                                strPartLabelButtonDropDownMenuItem = @"Удалить";
                            else
                                ;

                        // п.меню для операции с точкой
                        indxControlButtonDropDownMenuItem = i == INDEX_CONTROL.BUTTON_ADD ? INDEX_CONTROL.MENUITEM_ADD_POINT :
                            i == INDEX_CONTROL.BUTTON_DELETE ? INDEX_CONTROL.MENUITEM_DELETE_POINT : INDEX_CONTROL.UNKNOWN;
                        iButtonDropDownMenuItem = btnDropDown.ContextMenuStrip.Items.Add(new ToolStripMenuItem());
                        menuItem = btnDropDown.ContextMenuStrip.Items[iButtonDropDownMenuItem];
                        menuItem.Text = strPartLabelButtonDropDownMenuItem + @" " + arLabelButtonDropDownMenuItem[iButtonDropDownMenuItem];
                        menuItem.Name = indxControlButtonDropDownMenuItem.ToString();

                        // п.меню для операции с функцией
                        indxControlButtonDropDownMenuItem = i == INDEX_CONTROL.BUTTON_ADD ? INDEX_CONTROL.MENUITEM_ADD_FUNCTION :
                            i == INDEX_CONTROL.BUTTON_DELETE ? INDEX_CONTROL.MENUITEM_DELETE_FUNCTION : INDEX_CONTROL.UNKNOWN;
                        iButtonDropDownMenuItem = btnDropDown.ContextMenuStrip.Items.Add(new ToolStripMenuItem());
                        menuItem = btnDropDown.ContextMenuStrip.Items[iButtonDropDownMenuItem];
                        menuItem.Text = strPartLabelButtonDropDownMenuItem + @" " + arLabelButtonDropDownMenuItem[iButtonDropDownMenuItem];
                        menuItem.Name = indxControlButtonDropDownMenuItem.ToString();
                        break;
                    default:
                        addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);
                        break;
                }

            //заблокировать кнопку добавить
            //Button btn = ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]);
            //btn.Enabled = false;

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

            dgv.CellMouseDoubleClick +=dgv_CellMouseDoubleClickNALG;
            dgv.CellEndEdit += dgv_CellEndEditNALG;

            //Таблица с реперными точками 
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_VALUES.ToString();
            i = INDEX_CONTROL.DGV_VALUES;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 6);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 5);
            //Запретить редактирование
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
            //обработчик нажатия мышки
            dgv.CellMouseDoubleClick += dgv_CellMouseDoubleClickValue;
            //обработчик редактирования ячейки
            dgv.CellEndEdit += dgv_CellEndEditValue;

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
            ////
            //lblValue = new System.Windows.Forms.Label();
            //lblValue.Dock = DockStyle.Bottom;
            //lblValue.Text = @"Результат";            
            //tabl.Controls.Add(lblValue, 0, 2);
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
            //tbValue.TextChanged += tbCalcValue_onTextChanged;
            tbValue.TextAlign = HorizontalAlignment.Right;
            tbValue.Dock = DockStyle.Fill;
            tbValue.ReadOnly = true;
            tabl.Controls.Add(tbValue, 3, 1);

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
            this.Controls.Add(gpBoxCalc, 1, 11);
            this.SetColumnSpan(gpBoxCalc, 4);
            this.SetRowSpan(gpBoxCalc, 2);
            //
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());

            ResumeLayout(false);
            PerformLayout();

            //Обработчика нажатия кнопок
            btnDropDown = ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]) as DropDownButton;
            btnDropDown.Click += new System.EventHandler(btnAddToPoint_OnClick);
            menuItem = (btnDropDown.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_ADD_POINT.ToString(), true)[0]);
            menuItem.Click += new System.EventHandler(btnAddToPoint_OnClick);
            menuItem = (btnDropDown.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_ADD_FUNCTION.ToString(), true)[0]);
            menuItem.Click += new System.EventHandler(btnAddToFunction_OnClick);
            btnDropDown = ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]) as DropDownButton;
            btnDropDown.Click += new System.EventHandler(btnDeleteToPoint_OnClick);
            menuItem = (btnDropDown.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_DELETE_POINT.ToString(), true)[0]);
            menuItem.Click += new System.EventHandler(btnDeleteToPoint_OnClick);
            menuItem = (btnDropDown.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_DELETE_FUNCTION.ToString(), true)[0]);
            menuItem.Click += new System.EventHandler(btnDeleteToFunction_OnClick);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
            //((Button)Controls.Find(INDEX_CONTROL.BUTTON_CALC.ToString(), true)[0]).Click += new EventHandler(PluginPrjTepFTable_ClickRez);

            //Обработчики событий
            // для отображения таблиц
            dgv = Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0] as DataGridView;
            dgv.SelectionChanged += new EventHandler(dgvnALG_onSelectionChanged);
            //// для определения признака удаления (ФУНКЦИЮ или точку)
            //dgv.RowEnter += new DataGridViewCellEventHandler(dgvnALG_OnRowEnter);
            //dgv.Leave += new EventHandler (dgvnALG_OnLeave);            
            dgv = Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0] as DataGridView;
            dgv.SelectionChanged += new EventHandler(dgvValues_onSelectionChanged);
            //// для определения признака удаления (функцию или ТОЧКУ)
            //dgv.RowEnter += new DataGridViewCellEventHandler(dgvValues_OnRowEnter);
            //dgv.Leave += new EventHandler(dgvValues_OnLeave);
            // для поля ввода при поиске функции
            ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).TextChanged += new EventHandler(PluginPrjTepFTable_TextChanged);
        }

        /// <summary>
        ///  Обработчик события - окончание редактирования ячейки 
        ///  таблицы реперных точек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dgv_CellEndEditValue(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            string nameCol = dgv.Columns[e.ColumnIndex].Name;
            string editValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            if (editRecValue((int)dgv.Rows[dgv.SelectedRows[0].Index].Cells[dgv.ColumnCount - 1].Value, nameCol, editValue) == 1)
                m_tblEdit.AcceptChanges();
            else ;

            m_zGraph_fTABLE.Set(m_tblEdit);
            m_zGraph_fTABLE.Draw(NAlg);
        }

        /// <summary>
        ///  Обработчик события - окончание редактирования ячейки
        ///  таблицы функций
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dgv_CellEndEditNALG(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            string nameCol = dgv.Columns[e.ColumnIndex].Name;
            string editValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            if (editRecNAlg((int)dgv.Rows[e.RowIndex].Cells[dgv.ColumnCount - 1].Value, nameCol, editValue) == 1)
                m_tblEdit.AcceptChanges();
            else ;

            m_zGraph_fTABLE.Set(m_tblEdit);
            m_zGraph_fTABLE.Draw(NAlg);
        }

        /// <summary>
        /// Редактирование значений реперных точек
        /// </summary>
        /// <param name="id_rec">Иднтификатор записи</param>
        /// <param name="colName">имя столбца</param>
        /// <param name="editValue">новое значение</param>
        /// <returns></returns>
        private int editRecValue(int id_rec, string colName, string editValue)
        {
            int iRes = -1;

            DataRow[] rowsToEdit = m_tblEdit.Select(@"ID=" + id_rec);
            if (rowsToEdit.Length == 1)
            {
                foreach (DataRow r in rowsToEdit)
                {
                    int indx = m_tblEdit.Rows.IndexOf(r);
                    m_tblEdit.Rows[indx][colName] = editValue;
                    iRes++;
                }
            }
            else
            {
                iRes = -2;
                Logging.Logg().Error(@"PanelPrjTepFTable::delRecItem () - неоднозначность при редактировании точки с ID=" + id_rec + @"..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        /// <summary>
        /// Редактирование значений реперных точек
        /// </summary>
        /// <param name="id_rec">Иднтификатор записи</param>
        /// <param name="colName">имя столбца</param>
        /// <param name="editValue">новое значение</param>
        /// <returns></returns>
        private int editRecNAlg(int id_rec, string colName, string editValue)
        {
            int iRes = -1;

            DataRow[] rowsToEdit = m_tblEdit.Select(@"ID=" + id_rec);
            if (rowsToEdit.Length == 1)
            {
                foreach (DataRow r in rowsToEdit)
                {
                    int indx = m_tblEdit.Rows.IndexOf(r);
                    m_tblEdit.Rows[indx]["N_ALG"] = editValue;
                    iRes++;
                }
            }
            else
            {
                iRes = -2;
                Logging.Logg().Error(@"PanelPrjTepFTable::delRecItem () - неоднозначность при редактировании функции с ID=" + id_rec + @"..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        /// <summary>
        /// Обработчик события - двойной щечок по ячейки с реперными точками
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_CellMouseDoubleClickValue(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            dgv.ReadOnly = false;
        }

        /// <summary>
        /// Обработчик события - двойной щечок по ячейки с функциями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_CellMouseDoubleClickNALG(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            dgv.ReadOnly = false;
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

                if (dgv.SelectedRows.Count == 0)
                    strRes = (string)dgv.Rows[0].Cells[@"Функция"].Value;
                else
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
        ///  изменение чекбокса, построение графика.
        /// </summary>
        /// <param name="sender">Объект, иницировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void PluginPrjTepFTable_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv =
                //((DataGridView)Controls.Find(INDEX_CONTROL.DGV_PROP.ToString(), true)[0])
                sender as DataGridView
                ;

            if (e.RowIndex > -1)
            {
            }
            else
                ;
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Добавить" (точку)
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnAddToPoint_OnClick(object obj, EventArgs ev)
        {
            addRecNAlgPoint(NAlg);
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Удалить" (точку)
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnDeleteToPoint_OnClick(object obj, EventArgs ev)
        {
            DataGridView dgvValues = Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0] as DataGridView;
            // в крайнем столбце (снятым с отображения) - идентификатор записи
            if (delRecNAlg((int)dgvValues.Rows[dgvValues.SelectedRows[0].Index].Cells[dgvValues.ColumnCount - 1].Value) == 1)
            {
                m_tblEdit.AcceptChanges();
                updateDGVNalg(m_tblEdit);
            }
            else
                ;
            dgvValues.Refresh();
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Добавить" (функцию)
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnAddToFunction_OnClick(object obj, EventArgs ev)
        {
            addRecNAlg();
        }

        /// <summary>
        /// Добавить точку для функции
        /// </summary>
        /// <param name="nameAlg">имя функции</param>
        private void addRecNAlgPoint(string nameAlg)
        {
            int err = -1;
            int idNALG = DbTSQLInterface.GetIdNext(m_tblEdit, out err);

            m_tblEdit.Rows.Add(new object[]
            {
                idNALG,
               nameAlg,
                1,
                0,
                0,
                1,
                nameAlg
            });

            m_tblEdit.AcceptChanges();
            updateDGVNalg(m_tblEdit);
        }

        /// <summary>
        /// Добавить функцию
        /// </summary>
        private void addRecNAlg()
        {
            int err = -1;
            int idNALG = DbTSQLInterface.GetIdNext(m_tblEdit, out err);

            m_tblEdit.Rows.Add(new object[]
            {
                idNALG,
               "func:1",
                1,
                0,
                0,
                1,
                "func:1"
            });

            m_tblEdit.AcceptChanges();
            updateDGVNalg(m_tblEdit);
        }

        /// <summary>
        /// Обновление записей DataGridView
        /// </summary>
        /// <param name="tbl"></param>
        private void updateDGVNalg(DataTable tbl)
        {
            DataGridView dgv = null;
            List<string> listNAlg;
            string strItem = string.Empty;

            m_zGraph_fTABLE.Set(tbl);

            dgv = Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0] as DataGridView;
            listNAlg = new List<string>();

            foreach (DataRow r in tbl.Rows)
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

            dgv.CurrentCell.Selected = false;
            dgv.CurrentCell = dgv.Rows[(dgv.RowCount-1)].Cells[0];
            dgv.Rows[(dgv.RowCount - 1)].Cells[0].Selected = true;
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Удалить" (функцию)
        /// </summary>
        /// <param name="obj">Объект - инициатор события (кнопка)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnDeleteToFunction_OnClick(object obj, EventArgs ev)
        {
            DataGridView dgvNALG = Controls.Find(INDEX_CONTROL.DGV_NALG.ToString(), true)[0] as DataGridView;
            // в 1-ом столбце - наименование функции
            if (delRecNAlg(dgvNALG.Rows[dgvNALG.SelectedRows[0].Index].Cells[0].Value.ToString()) > 0)
            {
                m_tblEdit.AcceptChanges();
                updateDGVNalg(m_tblEdit);
            }
            else
                ;
        }

        /// <summary>
        /// Удалить все записи (точки) для функции
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <result>Количество удаленных строк или признак ошибки</result>
        protected int delRecNAlg(string nameAlg)
        {
            int iRes = -1;

            DataRow[] rowsToDel = m_tblEdit.Select(@"N_ALG='" + nameAlg + @"'");
            if (rowsToDel.Length > 0)
            {
                iRes = 0;

                foreach (DataRow r in rowsToDel)
                {
                    m_tblEdit.Rows.Remove(r);

                    iRes++;
                }
            }
            else
            {
                iRes = -2;
                Logging.Logg().Error(@"PanelPrjTepFTable::delRecItem () - неоднозначность при удалении точек функции с NALG=" + nameAlg + @"..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        /// <summary>
        /// Удалить запись (значение) для функции
        /// </summary>
        /// <param name="indx">Иднтификатор записи</param>
        /// <result>Количество удаленных строк или признак ошибки</result>
        protected int delRecNAlg(int id_rec)
        {
            int iRes = -1;

            DataRow[] rowsToDel = m_tblEdit.Select(@"ID=" + id_rec);
            if (rowsToDel.Length == 1)
            {// удалять только, если строка есть И она единственная
                m_tblEdit.Rows.Remove(rowsToDel[0]);
                iRes = 1;
            }
            else
            {
                iRes = -2;
                Logging.Logg().Error(@"PanelPrjTepFTable::delRecItem () - неоднозначность при удалении точки с ID=" + id_rec + @"..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        /// <summary>
        /// Возвратить ранг изменившегося значения
        /// </summary>
        /// <param name="tbxValue">Поле ввода, в котором изменилось значение</param>
        /// <returns>Ранг изменившегося значения</returns>
        private FTable.FRUNK getRunkVariable(TextBox tbxValue)
        {
            FTable.FRUNK fRunkRes = FTable.FRUNK.UNKNOWN;

            for (INDEX_CONTROL indx = INDEX_CONTROL.TEXTBOX_A1; indx < INDEX_CONTROL.TEXTBOX_F; indx++)
                if (tbxValue.Name.Equals(indx.ToString()) == true)
                {
                    fRunkRes = (FTable.FRUNK)(indx - INDEX_CONTROL.TEXTBOX_A1);
                    break;
                }
                else
                    ;

            return fRunkRes;
        }

        /// <summary>
        /// Обработчик события - изменение значения в поле ввода
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (поле ввода)</param>
        /// <param name="ev">Аргумент события</param>
        private void tbCalcValue_onTextChanged(object obj, EventArgs ev)
        {
            FTable.FRUNK runk = m_zGraph_fTABLE.GetRunk(NAlg);
            bool bCalculate = false;
            string strVal = string.Empty;
            float[] pars = new float[(int)runk + 1];

            for (int indx = 0; indx < pars.Length; indx++)
            {
                bCalculate = float.TryParse((Controls.Find(((INDEX_CONTROL)(indx + (int)INDEX_CONTROL.TEXTBOX_A1)).ToString(), true)[0] as TextBox).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out pars[indx]);
                if (bCalculate == false)
                    break;
                else
                    ;
            }

            if (bCalculate == true)
                strVal = m_zGraph_fTABLE.Calculate(NAlg, getRunkVariable(obj as TextBox), pars).ToString(@"F2");
            else
                strVal = float.NaN.ToString();

            (Controls.Find(INDEX_CONTROL.TEXTBOX_F.ToString(), true)[0] as TextBox).Text = strVal;
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
            register(16, typeof(PanelPrjTepFTable), @"Проект", @"Нормативные графики");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}