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
        /// <summary>
        /// Перечисление для режимов группового элемента управления
        /// </summary>
        public enum MODE
        {
            UNKNOWN = -1, DAY, MONTH, YEAR, HOUR           
                , COUNT
        }
        /// <summary>
        /// Перечисление для индесов дочерних элементов управления
        /// </summary>
        private enum INDEX_CONTROL
        {
            UNKNOWN = -1, DAY, MONTH, YEAR, HOUR
                , COUNT
        }
        /// <summary>
        /// Матрица доступности элементов управления при различных режимах
        /// </summary>
        private static bool[,] _matrixEnabled = new bool[(int)MODE.COUNT, (int)INDEX_CONTROL.COUNT] {
                    { true, true, true, false } //MODE.DAY
                    , { false, true, true, false } //MODE>MONTH
                    , { false, false, true, false } //MODE.YEAR
                    , { true, true, true, true }}; //MODE.CUSTOMIZE
        /// <summary>
        /// Текущие значения (номер года/месяца/дня/часа) в элементах управления
        /// </summary>
        private int _iYear
            , _iMonth
            , _iDay
            , _iHour;

        private MODE _mode;
        /// <summary>
        /// Текущим режим для групповго элемента управления
        /// </summary>
        public MODE Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                //Проверить признак изменения режима
                if (!(_mode == value))
                {// при изменении  - изменить доступность дочерних элементов управления
                    _mode = value;
                    
                    enable();
                }
                else
                    ;
            }
        }
        /// <summary>
        /// Объект от значений которого зависят значения текущего объекта
        /// </summary>
        HDateTimePicker m_objLeading;
        /// <summary>
        /// События изменения значения
        /// </summary>
        public event EventHandler ValueChanged;
        /// <summary>
        /// Изменить доступность элементов управления
        /// </summary>
        private void enable()
        {
            Control ctrl = null;
            bool bEnabled = false;

            //for (INDEX_CONTROL indx = INDEX_CONTROL.DAY; indx < INDEX_CONTROL.COUNT; indx++)
            //{
            //    ctrl = Controls.Find(indx.ToString(), true)[0];
            //    if (Mode == MODE.UNKNOWN)
            //        ctrl.Enabled = false;
            //    else
            //    {
            //        bEnabled = _matrixEnabled[(int)_mode, (int)indx];
            //        if ((!(m_objLeading == null))
            //            && (bEnabled == true))
            //            switch (Mode)
            //            {
            //                case MODE.DAY:
            //                    switch (indx)
            //                    {                                    
            //                        case INDEX_CONTROL.DAY:
            //                            //bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.MONTH:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.YEAR:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.HOUR:
            //                            bEnabled = false;
            //                            break;
            //                        default:
            //                            break;
            //                    }
            //                    break;
            //                case MODE.MONTH:
            //                    switch (indx)
            //                    {
            //                        case INDEX_CONTROL.DAY:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.MONTH:
            //                            //bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.YEAR:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.HOUR:
            //                            bEnabled = false;
            //                            break;
            //                        default:
            //                            break;
            //                    }
            //                    break;
            //                case MODE.YEAR:
            //                    switch (indx)
            //                    {
            //                        case INDEX_CONTROL.DAY:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.MONTH:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.YEAR:
            //                            //bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.HOUR:
            //                            bEnabled = false;
            //                            break;
            //                        default:
            //                            break;
            //                    }
            //                    break;
            //                case MODE.HOUR:
            //                    switch (indx)
            //                    {
            //                        case INDEX_CONTROL.DAY:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.MONTH:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.YEAR:
            //                            bEnabled = false;
            //                            break;
            //                        case INDEX_CONTROL.HOUR:
            //                            //bEnabled = false;
            //                            break;
            //                        default:
            //                            break;
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }
            //        else
            //            ;
            //        ctrl.Enabled = bEnabled;
            //    }
            //}
        }
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="year">Номер года</param>
        /// <param name="month">Номер месяца</param>
        /// <param name="day">Номер дня в месяце</param>
        /// <param name="hour">Номер часа в сутках</param>
        /// <param name="objLeading">Объект от значений которого зависит значения создаваемого объекта</param>
        public HDateTimePicker(int year, int month, int day, int hour, HDateTimePicker objLeading)
            : base(12, 1)
        {
            _iYear = year;
            _iMonth = month;
            _iDay = day;
            _iHour = hour;

            InitializeComponents();

            Mode = MODE.UNKNOWN;

            m_objLeading = objLeading;
            if (! (m_objLeading == null))
                m_objLeading.ValueChanged += new EventHandler(leading_ValueChanged);
            else
                ;
        }

        private static string[] months = { @"январь", @"февраль", @"март"
            , @"апрель", @"май", @"июнь"
            , @"июль", @"август", @"сентябрь"
            , @"октябрь", @"ноябрь", @"декабрь" };

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly();
        }
        /// <summary>
        /// Обработчик события - изменения номера дня
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxDay_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события - изменения номера месяца
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxMonth_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события - изменения номера года
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxYear_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события - изменения номера часа
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxHour_onSelectedIndexChanged(object obj, EventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события - изменения значения в 
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void leading_ValueChanged (object obj, EventArgs ev)
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
