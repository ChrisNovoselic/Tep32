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
                    , { false, true, true, false } //MODE.MONTH
                    , { false, false, true, false } //MODE.YEAR
                    , { true, true, true, true }}; //MODE.CUSTOMIZE
        ///// <summary>
        ///// Текущие значения (номер года/месяца/дня/часа) в элементах управления
        ///// </summary>
        //private int _iYear
        //    , _iMonth
        //    , _iDay
        //    , _iHour;

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

        TimeSpan m_tsLeading;
        /// <summary>
        /// События изменения значения
        /// </summary>
        public event EventHandler ValueChanged;
        /// <summary>
        /// Значение дата/время объекта
        /// </summary>
        public DateTime Value;

        public DateTime LeadingValue { get { return Value - m_tsLeading; } }
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
            Value = new DateTime(year, month, day, hour, 0 , 0);

            InitializeComponents();

            Mode = MODE.UNKNOWN;

            m_objLeading = objLeading;
            if (!(m_objLeading == null))
            {
                m_objLeading.ValueChanged += new EventHandler(leading_ValueChanged);

                m_tsLeading = Value - m_objLeading.Value;
            }
            else
                ;
        }

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
            Value = new DateTime(
                Value.Year
                , Value.Month
                , (obj as ComboBox).SelectedIndex + 1
                , Value.Hour
                , 0
                , 0
            );

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменения номера месяца
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxMonth_onSelectedIndexChanged(object obj, EventArgs ev)
        {
            Value = new DateTime(
                Value.Year
                , (obj as ComboBox).SelectedIndex + 1
                , Value.Day
                , Value.Hour
                , 0
                , 0
            );

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменения номера года
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxYear_onSelectedIndexChanged(object obj, EventArgs ev)
        {
            Value = new DateTime(
                (obj as ComboBox).SelectedIndex + (DateTime.Today.Year - s_iBackwardYears)
                , Value.Month
                , Value.Day
                , Value.Hour
                , 0
                , 0
            );

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменения номера часа
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxHour_onSelectedIndexChanged(object obj, EventArgs ev)
        {
            Value = new DateTime(
                Value.Year
                , Value.Month
                , Value.Day
                , (obj as ComboBox).SelectedIndex + 0
                , 0
                , 0
            );

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменение знасния любого из компонентов даты/времени
        /// </summary>
        private void onSelectedIndexChanged()
        {
            //Проверить наличие ведущего объекта
            if (!(m_objLeading == null))
                // изменить разность между собственным значением и значения ведущего объекта
                m_tsLeading = Value - m_objLeading.Value;
            else
                ;
            // вызвать обработчик события - изменение значения объекта
            // , но только один и 1-ый из них
            // два обработчика когда объект ведущий (2-ой обработчик ведомого объекта)
            ValueChanged.GetInvocationList()[0].DynamicInvoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Обработчик события - изменения значения в "ведущем" календаре
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void leading_ValueChanged (object obj, EventArgs ev)
        {
            HDateTimePicker objLeading = obj as HDateTimePicker;
            ComboBox cbxYear = null
                , cbxMonth = null
                , cbxDay = null
                , cbxHour = null;
            int iDiffYear = -1;
            
            //??? учитывать значение в "ведущем" календаре
            iDiffYear = objLeading.Value.Year - Value.Year;
            Value = objLeading.Value + m_tsLeading;

            cbxYear = Controls.Find (INDEX_CONTROL.YEAR.ToString (), true)[0] as ComboBox;
            cbxMonth = Controls.Find(INDEX_CONTROL.MONTH.ToString(), true)[0] as ComboBox;
            cbxDay = Controls.Find(INDEX_CONTROL.DAY.ToString(), true)[0] as ComboBox;
            cbxHour = Controls.Find(INDEX_CONTROL.HOUR.ToString(), true)[0] as ComboBox;

            cbxYear.SelectedIndexChanged -= cbxYear_onSelectedIndexChanged;
            cbxMonth.SelectedIndexChanged -= cbxMonth_onSelectedIndexChanged;
            cbxDay.SelectedIndexChanged -= cbxDay_onSelectedIndexChanged;
            cbxHour.SelectedIndexChanged -= cbxHour_onSelectedIndexChanged;

            cbxYear.SelectedIndex += iDiffYear;
            cbxMonth.SelectedIndex = Value.Month - 1;
            cbxDay.SelectedIndex = Value.Day - 1;
            cbxHour.SelectedIndex = Value.Hour; // == 0 ? Value.Hour : Value.Hour - 1;

            cbxYear.SelectedIndexChanged += new EventHandler(cbxYear_onSelectedIndexChanged);
            cbxMonth.SelectedIndexChanged += new EventHandler(cbxMonth_onSelectedIndexChanged);
            cbxDay.SelectedIndexChanged += new EventHandler(cbxDay_onSelectedIndexChanged);
            cbxHour.SelectedIndexChanged += new EventHandler(cbxHour_onSelectedIndexChanged);            

            ValueChanged (this, EventArgs.Empty);
        }
    }

    partial class HDateTimePicker
    {
        private int s_iBackwardYears = 5
            , s_iForwardYears = 6;
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
            (ctrl as ComboBox).SelectedIndex = Value.Day - 1;
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
                (ctrl as ComboBox).Items.Add(HDateTime.NameMonths[i]);
            (ctrl as ComboBox).SelectedIndex = Value.Month - 1;
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxMonth_onSelectedIndexChanged);

            //Дата - год
            indx = INDEX_CONTROL.YEAR;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 7, 0);
            SetColumnSpan(ctrl, 3); SetRowSpan(ctrl, 1);
            for (i = (Value.Year - s_iBackwardYears); i < (Value.Year + s_iForwardYears); i++)
                (ctrl as ComboBox).Items.Add(i.ToString());
            (ctrl as ComboBox).SelectedIndex = Value.Year - (Value.Year - s_iBackwardYears);
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
                (ctrl as ComboBox).Items.Add(i + 0);
            (ctrl as ComboBox).SelectedIndex = Value.Hour - 0;
            (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxHour_onSelectedIndexChanged);

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
