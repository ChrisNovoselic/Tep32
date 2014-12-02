using System.Windows.Forms;

namespace Tep32
{
    partial class FormParameters
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.m_dgvData = new DataGridView ();
            this.m_dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {new DataGridViewTextBoxColumn (),
                                                                                            new DataGridViewTextBoxColumn (),
                                                                                            new DataGridViewTextBoxColumn ()});
            this.m_dgvData.Columns [0].HeaderText = @"Параметр";
            this.m_dgvData.Columns [0].Width = 90;
            this.m_dgvData.Columns [0].ReadOnly = true;
            this.m_dgvData.Columns[0].Resizable = DataGridViewTriState.False;
            this.m_dgvData.Columns[1].HeaderText = @"Значение";
            this.m_dgvData.Columns[1].Width = 60;
            this.m_dgvData.Columns[1].ReadOnly = false;
            this.m_dgvData.Columns[1].Resizable = DataGridViewTriState.False;
            this.m_dgvData.Columns[2].HeaderText = @"Ед./изм.";
            this.m_dgvData.Columns[2].Width = 40;
            this.m_dgvData.Columns[2].ReadOnly = true;
            this.m_dgvData.Columns[2].Resizable = DataGridViewTriState.False;

            this.m_dgvData.Location = new System.Drawing.Point (6, 6);
            this.m_dgvData.Size = new System.Drawing.Size (238, 78);

            this.m_dgvData.RowHeadersWidth = 40;
            
            // 
            // FormParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 125);
            this.Controls.Add(this.m_dgvData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormParameters";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Параметры приложения";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Parameters_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected DataGridView m_dgvData;
    }
}