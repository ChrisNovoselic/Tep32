using System.Windows.Forms;

namespace TepCommon
{
    partial class HPanelEdit
    {
        protected enum INDEX_BUTTON { ADD, DELETE, SAVE, UPDATE, INDEX_BUTTON_COUNT, };
        
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

            m_arBtnDatabase = new Button[(int)INDEX_BUTTON.INDEX_BUTTON_COUNT] { new Button(), new Button(), new Button(), new Button() };

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
