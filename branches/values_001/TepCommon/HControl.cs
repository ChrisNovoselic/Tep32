using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace TepCommon
{
    public partial class HDateTimePicker : HPanelCommon
    {
        public enum MODE
        {
            UNKNOWN = -1, DAY, MONTH, YEAR,
            CUSTOMIZE
                , COUNT
        }

        private enum INDEX_CONTROL
        {
            UNKNOWN = -1, DAY, MONTH, YEAR,
            HOUR
                , COUNT
        }

        private static bool[,] _matrixEnabled = new bool[(int)MODE.COUNT, (int)INDEX_CONTROL.COUNT] {
                    { true, true, true, false }
                    , { false, true, true, false }
                    , { false, false, true, false }
                    , { true, true, true, true }};

        private int _iYear
            , _iMonth
            , _iDay
            , _iHour;

        private MODE _mode;
        public MODE Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (!(_mode == value))
                {
                    _mode = value;

                    enable();
                }
                else
                    ;
            }
        }

        public event EventHandler ValueChanged;

        private void enable()
        {
            Control ctrl = null;
            for (INDEX_CONTROL indx = INDEX_CONTROL.DAY; indx < INDEX_CONTROL.COUNT; indx++)
            {
                ctrl = Controls.Find(indx.ToString(), true)[0];
                if (Mode == MODE.UNKNOWN)
                    ctrl.Enabled = false;
                else
                    ctrl.Enabled = _matrixEnabled[(int)_mode, (int)indx];
            }
        }

        public HDateTimePicker(MODE mode, int year, int month, int day, int hour = 1)
            : base(12, 1)
        {
            _iYear = year;
            _iMonth = month;
            _iDay = day;
            _iHour = hour;

            InitializeComponents();

            Mode = mode;
        }

        private string[] months = { @"январь", @"февраль", @"март"
                    , @"апрель", @"май", @"июнь"
                    , @"июль", @"август", @"сентябрь"
                    , @"октябрь", @"ноябрь", @"декабрь" };

        //public HDateTimePicker () : base (12, 1)
        //{
        //    InitializeComponents ();
        //}        

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly();
        }

        private void cbxDay_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }

        private void cbxMonth_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }

        private void cbxYear_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }

        private void cbxHour_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }
    }

    partial class HDateTimePicker
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponents()
        {
            //??? зачем
            components = new System.ComponentModel.Container();

            Control ctrl;
            INDEX_CONTROL indx = INDEX_CONTROL.UNKNOWN;
            int i = -1;

            SuspendLayout();

            initializeLayoutStyle();

            //Дата - номер дня
            indx = INDEX_CONTROL.DAY;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 0, 0);
            SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
            for (i = 0; i < 31; i++)
                (ctrl as ComboBox).Items.Add(i + 1);
            (ctrl as ComboBox).SelectedIndex = _iDay - 1;
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxDay_onSelectedIndexChanged);

            //Дата - наименование месяца
            indx = INDEX_CONTROL.MONTH;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 2, 0);
            SetColumnSpan(ctrl, 5); SetRowSpan(ctrl, 1);
            for (i = 0; i < 12; i++)
                (ctrl as ComboBox).Items.Add(months[i]);
            (ctrl as ComboBox).SelectedIndex = _iMonth - 1;
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxMonth_onSelectedIndexChanged);

            //Дата - год
            indx = INDEX_CONTROL.YEAR;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 7, 0);
            SetColumnSpan(ctrl, 3); SetRowSpan(ctrl, 1);
            for (i = 10; i < 21; i++)
                (ctrl as ComboBox).Items.Add(@"20" + i.ToString());
            (ctrl as ComboBox).SelectedIndex = _iYear - (2000 + 10);
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxYear_onSelectedIndexChanged);

            //Время - час
            indx = INDEX_CONTROL.HOUR;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 10, 0);
            SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
            for (i = 0; i < 24; i++)
                (ctrl as ComboBox).Items.Add(i + 1);
            (ctrl as ComboBox).SelectedIndex = _iHour - 1;
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxHour_onSelectedIndexChanged);

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
