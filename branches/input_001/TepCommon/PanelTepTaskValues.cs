using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;

namespace TepCommon
{
    public abstract partial class PanelTepTaskValues : HPanelTepCommon
    {
        public PanelTepTaskValues(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            m_panelManagement = new PanelManagement ();
            m_dgvValues = new DataGridViewTEPValues ();

            SuspendLayout ();

            initializeLayoutStyle ();

            Controls.Add (m_panelManagement, 0, 0);
            SetColumnSpan(m_panelManagement, 4); SetRowSpan(m_panelManagement, 13);

            Controls.Add(m_dgvValues, 4, 0);
            SetColumnSpan(m_dgvValues, 9); SetRowSpan(m_dgvValues, 10);

            addLabelDesc (17);

            ResumeLayout (false);
            PerformLayout ();
        }

        protected class DataGridViewTEPValues : DataGridView
        {
            public DataGridViewTEPValues ()
            {
                InitializeComponents ();
            }

            private void InitializeComponents()
            {
                this.Dock = DockStyle.Fill;
            }
        }

        protected class PanelManagement : HPanelCommon
        {
            private class HDateTimePicker : HPanelCommon
            {
                public HDateTimePicker () : base (12, 1)
                {
                    InitializeComponents ();
                }

                private void InitializeComponents()
                {
                    Control ctrl;

                    SuspendLayout();

                    initializeLayoutStyle ();

                    //Дата - номер дня
                    ctrl = new NumericUpDown ();
                    ctrl.Dock = DockStyle.Fill;
                    Controls.Add (ctrl, 0, 0);
                    SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);

                    //Дата - наименование месяца
                    ctrl = new ComboBox ();
                    ctrl.Dock = DockStyle.Fill;
                    Controls.Add(ctrl, 2, 0);
                    SetColumnSpan(ctrl, 5); SetRowSpan(ctrl, 1);

                    //Дата - год
                    ctrl = new NumericUpDown();
                    ctrl.Dock = DockStyle.Fill;
                    Controls.Add(ctrl, 7, 0);
                    SetColumnSpan(ctrl, 3); SetRowSpan(ctrl, 1);

                    //Время - час
                    ctrl = new NumericUpDown();
                    ctrl.Dock = DockStyle.Fill;
                    Controls.Add(ctrl, 10, 0);
                    SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);

                    ResumeLayout(false);
                    PerformLayout();
                }

                protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
                {
                    initializeLayoutStyleEvenly();
                }
            }
            
            public PanelManagement() : base (8, 18)
            {
                InitializeComponents ();
            }

            private void InitializeComponents ()
            {
                Control ctrl = null;

                SuspendLayout();

                initializeLayoutStyle();

                //Период расчета
                //Период расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as System.Windows.Forms.Label).Text = @"Период:";
                this.Controls.Add(ctrl, 0, 0);
                SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                //Период расчета - значение
                ctrl = new ComboBox ();
                ctrl.Name = INDEX_CONTROL.CB_PERIOD.ToString ();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                this.Controls.Add (ctrl, 2, 0);
                SetColumnSpan(ctrl, 6); SetRowSpan(ctrl, 1);

                //Дата/время начала периода расчета
                //Дата/время начала периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время начала периода расчета";
                this.Controls.Add(ctrl, 0, 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker();
                ctrl.Name = INDEX_CONTROL.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, 2);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета
                //Дата/время  окончания периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время  окончания периода расчета:";
                this.Controls.Add(ctrl, 0, 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета - значения
                ctrl = new HDateTimePicker();
                ctrl.Name = INDEX_CONTROL.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, 4);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly ();
            }
        }
    }

    public partial class PanelTepTaskValues
    {
        protected enum INDEX_CONTROL { CB_PERIOD, HDTP_BEGIN, HDTP_END }

        protected PanelManagement m_panelManagement;
        protected DataGridViewTEPValues m_dgvValues;
    }
}
