using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using InterfacePlugIn;
using System.Globalization;

namespace TepCommon
{
    public class HPanelDesc : TableLayoutPanel
    {
        enum ID_LBL { lblGroup, lblTab, lblDGV1, lblDGV2, lblDGV3, selRow };

        string[] m_desc_lbl = new string[] { "lblGroupDesc", "lblTabDesc", "lblDGV1Desc", "lblDGV2Desc", "lblDGV3Desc", "selRowDesc" };
        string[] m_name_lbl = new string[] { "lblGroupName", "lblTabName", "lblDGV1Name", "lblDGV2Name", "lblDGV3Name", "selRowName" };
        string[] m_name_lbl_text = new string[] { "Группа вкладок: ", "Вкладка: ", "Таблица: ", "Таблица: ", "Таблица: ", "Выбранная строка: " };

        private void Initialize()
        {
            this.SuspendLayout();
            this.ColumnCount = 7;
            this.RowCount = 14;
            int i = 0;
            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.ColumnCount));
            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.RowCount));
            int rows = 0;
            int col = 0;

            this.Dock = DockStyle.Fill;

            Control ctrl = new Control();

            ctrl = new Label();
            ctrl.Name = "obj";
            ctrl.Text = "Объект";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 2);

            col = 2;

            ctrl = new Label();
            ctrl.Name = "desc";
            ctrl.Text = "Описание";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 4);

            col = 0;
            rows = 2;

            for (i = 0; i < m_desc_lbl.Length; i++)
            {
                ctrl = new Label();
                ctrl.Name = m_name_lbl[i];
                ctrl.Text = m_name_lbl_text[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 2);

                ctrl = new Label();
                ctrl.Name = m_desc_lbl[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                col = 2;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 4);
                rows += 2;
                col = 0;
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public HPanelDesc()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Поле описания группы вкладок
        /// </summary>
        public string[] SetLblGroup
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = "Группа вкладок: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания вкладки
        /// </summary>
        public string[] SetLblTab
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = "Вкладка: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 1
        /// </summary>
        public string[] SetLblDGV1Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public string[] SetLblDGV2Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public string[] SetLblDGV3Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания выбранной строки
        /// </summary>
        public string[] SetLblRowDesc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[1] != string.Empty)
                {
                    ctrl.Text = value[1];
                    ctrl.Visible = true;
                }
                else
                    ctrl.Visible = false;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[0] != string.Empty)
                {
                    value[0].Replace('_', ' ');
                    ctrl.Text = "Cтрока: " + value[0];
                    ctrl.Visible = true;
                }

            }
        }

        /// <summary>
        /// Поле отображения описания группы вкладок
        /// </summary>
        public bool SetLblGroup_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания вкладки
        /// </summary>
        public bool SetLblTab_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания таблицы 1
        /// </summary>
        public bool SetLblDGV1Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public bool SetLblDGV2Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public bool SetLblDGV3Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания выбранной строки
        /// </summary>
        public bool SetLblRowDesc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

            }
        }

    }
}
