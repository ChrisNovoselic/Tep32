using System.Windows.Forms;
using System.Collections.Generic;

namespace TepCommon
{
    partial class HPanelEdit
    {
        protected enum INDEX_CONTROL { BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE, BUTTON_UPDATE
                                        , DGV_DICT_EDIT, DGV_DICT_PROP
                                        , LABEL_PROP_DESC
                                        , INDEX_CONTROL_COUNT, };
        private static string [] m_arButtonText  = {@"Добавить", @"", @"", @""};
        
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
            m_dictControls[indx].Size = new System.Drawing.Size(79, 23);
            m_dictControls[indx].Text = m_arButtonText [indx];
            this.Controls.Add(m_dictControls[indx], 0, indx);
            this.SetColumnSpan(m_dictControls[indx], 3);
        }

        #region Код, автоматически созданный конструктором компонентов

        protected Dictionary <int, Control> m_dictControls;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.Dock = DockStyle.Fill;

            m_dictControls = new Dictionary<int,Control> ();

            this.RowCount = 13;
            this.ColumnCount = 13;

            this.SuspendLayout();

            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_ADD;
            for (i = INDEX_CONTROL.BUTTON_ADD; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i ++)
                addButton ((int)i);

            i = INDEX_CONTROL.DGV_DICT_EDIT;
            m_dictControls.Add((int)i, new DataGridView());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            this.SetColumnSpan(m_dictControls[(int)i], 3); this.SetRowSpan(m_dictControls[(int)i], 13);

            i = INDEX_CONTROL.DGV_DICT_PROP;
            m_dictControls.Add((int)i, new DataGridView());
            m_dictControls[(int)i].Dock = DockStyle.Fill;
            this.SetColumnSpan(m_dictControls[(int)i], 7); this.SetRowSpan(m_dictControls[(int)i], 9);

            this.ResumeLayout ();
        }

        #endregion
    }
}
