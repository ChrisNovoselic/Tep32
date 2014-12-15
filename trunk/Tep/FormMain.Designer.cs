using HClassLibrary;

namespace Tep64
{
    partial class FormMain
    {
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.настройкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.бДКонфигурацииToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.m_TabCtrl = new HTepTabCtrlEx ();

            this.MainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлToolStripMenuItem
                , настройкаToolStripMenuItem 
            });
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(923, 24);
            this.MainMenuStrip.TabIndex = 0;
            this.MainMenuStrip.Text = "MainMenuStrip";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(104, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // настройкаToolStripMenuItem
            // 
            this.настройкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.бДКонфигурацииToolStripMenuItem,
            });
            this.настройкаToolStripMenuItem.Name = "настройкаToolStripMenuItem";
            this.настройкаToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.настройкаToolStripMenuItem.Text = "Настройка";
            // 
            // бДКонфигурацииToolStripMenuItem
            // 
            this.бДКонфигурацииToolStripMenuItem.Name = "бДКонфигурацииToolStripMenuItem";
            this.бДКонфигурацииToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.бДКонфигурацииToolStripMenuItem.Text = "БД конфигурации";
            this.бДКонфигурацииToolStripMenuItem.Click += new System.EventHandler(this.бДКонфигурацииToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 665);
            this.Controls.Add(this.MainMenuStrip);
            //this.MainMenuStrip = this.menuStrip1;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ПК ЭТАП";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            //
            // m_TabCtrl
            //
            this.m_TabCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            //this.m_TabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_TabCtrl.Location = new System.Drawing.Point(0, MainMenuStrip.ClientSize.Height);
            this.m_TabCtrl.Name = "tabCtrl";
            this.m_TabCtrl.SelectedIndex = 0;
            this.m_TabCtrl.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - MainMenuStrip.ClientSize.Height - m_statusStripMain.ClientSize.Height);
            this.m_TabCtrl.TabIndex = 3;
            this.m_TabCtrl.SelectedIndexChanged += new System.EventHandler(this.TabCtrl_OnSelectedIndexChanged);
            this.Controls.Add(this.m_TabCtrl);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        
        private System.Windows.Forms.ToolStripMenuItem настройкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem бДКонфигурацииToolStripMenuItem;

        private HTepTabCtrlEx m_TabCtrl;
    }
}

