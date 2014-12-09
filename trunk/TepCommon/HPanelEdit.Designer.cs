using System.Windows.Forms;

namespace TepCommon
{
    partial class HPanelEdit
    {
        protected enum INDEX_CONTROL { BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE
                                        , DGV_DICT_EDIT, DGV_DICT_PROP
                                        , LABEL_PROP_DESC
                                        , INDEX_CONTROL_COUNT, };
        
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

        #region Код, автоматически созданный конструктором компонентов

        protected Button [] m_arBtnDatabase;
        protected DataGridView m_dgvDictEdit;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.Dock = DockStyle.Fill;

            m_arBtnDatabase = new Button[] { new Button(), new Button(), new Button(), new Button() };

            // --BUTTON_ADD--
            m_arBtnDatabase [(int)INDEX_CONTROL.BUTTON_ADD].Location = new System.Drawing.Point (1, 1);
            m_arBtnDatabase [(int)INDEX_CONTROL.BUTTON_ADD].Size = new System.Drawing.Size (79, 23);
            m_arBtnDatabase [(int)INDEX_CONTROL.BUTTON_ADD].Text = @"";
            this.Controls.Add(m_arBtnDatabase[(int)INDEX_CONTROL.BUTTON_ADD], );                

            m_dgvDictEdit = new DataGridView ();
            m_dgvDictEdit.Dock = DockStyle.Fill;

            this.RowCount = 13;
            this.ColumnCount = 13;

            this.SuspendLayout ();

            this.ResumeLayout ();
        }

        #endregion
    }
}
