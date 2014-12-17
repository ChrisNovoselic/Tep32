using System.Windows.Forms;
using System.Collections.Generic;

namespace TepCommon
{
    partial class HPanelEdit
    {
        protected enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE,
            BUTTON_UPDATE
                , DGV_DICT_EDIT,
            DGV_DICT_PROP
                ,
            LABEL_PROP_DESC
                , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        protected Dictionary<int, Control> m_dictControls;
        
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void addButton (int indx) {
            m_dictControls.Add (indx, new Button ());

            m_dictControls[indx].Location = new System.Drawing.Point(1, 1);
            //m_dictControls[indx].Size = new System.Drawing.Size(79, 23);
            m_dictControls[indx].Dock = DockStyle.Fill;
            m_dictControls[indx].Text = m_arButtonText [indx];
            this.Controls.Add(m_dictControls[indx], 0, indx);
            this.SetColumnSpan(m_dictControls[indx], 1);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.Dock = DockStyle.Fill;
            // для отладки - "видимые" границы ячеек
            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            //Создать объект "словарь" дочерних элементов управления
            m_dictControls = new Dictionary<int,Control> ();

            //Установить кол-во строк/столбцов
            this.RowCount = 13; this.ColumnCount = 13;

            //Добавить стили "ширина" столлбцов
            for (int s = 0; s < this.ColumnCount; s++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)100 / this.ColumnCount));

            //Добавить стили "высота" строк
            for (int s = 0; s < this.RowCount; s++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)100 / this.RowCount));

            this.SuspendLayout();

            //Добавить кропки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton((int)i);

            //Добавить "список" словарных величин
            i = INDEX_CONTROL.DGV_DICT_EDIT;
            m_dictControls.Add((int)i, new DataGridView());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(m_dictControls[(int)i], 1, 0);            
            this.SetColumnSpan(m_dictControls[(int)i], 4); this.SetRowSpan(m_dictControls[(int)i], 13);
            //Добавить столбец
            ((DataGridView)m_dictControls[(int)i]).Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn ()
                });
            //Запретить выделение "много" строк
            ((DataGridView)m_dictControls[(int)i]).MultiSelect = false;
            //Установить режим выделения - "полная" строка
            ((DataGridView)m_dictControls[(int)i]).SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            ((DataGridView)m_dictControls[(int)i]).ColumnHeadersVisible = false;
            //Ширина столбца по ширине род./элемента управления
            ((DataGridView)m_dictControls[(int)i]).Columns [0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //Добавить "список" свойств словарной величины
            i = INDEX_CONTROL.DGV_DICT_PROP;
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

            GroupBox gbDesc = new GroupBox ();
            gbDesc.Text = @"Описание";
            gbDesc.Dock = DockStyle.Fill;
            this.Controls.Add(gbDesc, 5, 10);
            this.SetColumnSpan(gbDesc, 8); this.SetRowSpan(gbDesc, 3);

            i = INDEX_CONTROL.LABEL_PROP_DESC;
            m_dictControls.Add((int)i, new Label());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            gbDesc.Controls.Add (m_dictControls[(int)i]);

            this.ResumeLayout ();
        }

        #endregion
    }
}
