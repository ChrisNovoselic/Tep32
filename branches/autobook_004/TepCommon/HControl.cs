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
            UNKNOWN = -1, DAY, MONTH, YEAR,
            HOUR
                , COUNT
        }
        /// <summary>
        /// Перечисление для индесов дочерних элементов управления
        /// </summary>
        private enum INDEX_CONTROL
        {
            UNKNOWN = -1, DAY, MONTH, YEAR,
            HOUR
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
        /// <summary>
        /// Смещение относительно ведущего (если он есть) элемента управления
        /// </summary>
        TimeSpan m_tsLeading;
        /// <summary>
        /// События изменения значения
        /// </summary>
        public event EventHandler ValueChanged;

        private enum INDEX_VALUE : uint { PREVIOUS, CURRENT }
        private DateTime[] _value;

        /// <summary>
        /// Значение дата/время объекта
        /// </summary>
        public DateTime Value
        {
            get
            {
                return _value[(int)INDEX_VALUE.CURRENT];
            }

            set
            {
                INDEX_CONTROL indx = INDEX_CONTROL.UNKNOWN;
                ComboBox cbx = null;

                _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];
                _value[(int)INDEX_VALUE.CURRENT] = value;

                for (indx = (INDEX_CONTROL.UNKNOWN + 1); indx < INDEX_CONTROL.COUNT; indx++)
                {
                    cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                    cbx.SelectedIndexChanged -= m_arSelectIndexChangedHandlers[(int)indx];
                }

                indx = INDEX_CONTROL.HOUR;
                cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Hour;
                indx = INDEX_CONTROL.DAY;
                cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Day - 1;
                indx = INDEX_CONTROL.MONTH;
                cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Month - 1;
                indx = INDEX_CONTROL.YEAR;
                cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Year - (_value[(int)INDEX_VALUE.CURRENT].Year - s_iBackwardYears);

                onSelectedIndexChanged();

                for (indx = (INDEX_CONTROL.UNKNOWN + 1); indx < INDEX_CONTROL.COUNT; indx++)
                {
                    cbx = Controls.Find(indx.ToString(), true)[0] as ComboBox;
                    cbx.SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)indx];
                }
            }
        }
        /// <summary>
        /// Значение ведущего элемента управления
        /// </summary>
        public DateTime LeadingValue
        {
            get
            {
                return _value[(int)INDEX_VALUE.CURRENT] - m_tsLeading;
            }
        }

        private EventHandler[] m_arSelectIndexChangedHandlers;
        /// <summary>
        /// Изменить доступность элементов управления
        /// </summary>
        private void enable()
        {
            Control ctrl = null;
            bool bEnabled = false;

            for (INDEX_CONTROL indx = (INDEX_CONTROL.UNKNOWN + 1); indx < INDEX_CONTROL.COUNT; indx++)
            {
                ctrl = Controls.Find(indx.ToString(), true)[0];
                if (Mode == MODE.UNKNOWN)
                    ctrl.Enabled = false;
                else
                {
                    bEnabled = _matrixEnabled[(int)_mode, (int)indx];
                    if ((!(m_objLeading == null))
                        && (bEnabled == true))
                        switch (Mode)
                        {
                            case MODE.DAY:
                                switch (indx)
                                {
                                    case INDEX_CONTROL.DAY:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.MONTH:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.YEAR:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.HOUR:
                                        bEnabled = false;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case MODE.MONTH:
                                switch (indx)
                                {
                                    case INDEX_CONTROL.DAY:
                                        bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.MONTH:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.YEAR:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.HOUR:
                                        bEnabled = false;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case MODE.YEAR:
                                switch (indx)
                                {
                                    case INDEX_CONTROL.DAY:
                                        bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.MONTH:
                                        bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.YEAR:
                                        //bEnabled = false;
                                        break;
                                    case INDEX_CONTROL.HOUR:
                                        bEnabled = false;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case MODE.HOUR:
                                // все включено
                                break;
                            default:
                                break;
                        }
                    else
                        ;
                    ctrl.Enabled = bEnabled;
                }
            }
        }

        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="year">Номер года</param>
        /// <param name="month">Номер месяца</param>
        /// <param name="day">Номер дня в месяце</param>
        /// <param name="hour">Номер часа в сутках</param>
        /// <param name="objLeading">Объект от значений которого зависит значения создаваемого объекта</param>
        public HDateTimePicker(DateTime dtValue, HDateTimePicker objLeading)
            : base(12, 1)
        {
            _value = new DateTime[] { DateTime.MinValue, dtValue };

            m_arSelectIndexChangedHandlers = new EventHandler[(int)INDEX_CONTROL.COUNT] { cbxDay_onSelectedIndexChanged
                , cbxMonth_onSelectedIndexChanged
                , cbxYear_onSelectedIndexChanged
                , cbxHour_onSelectedIndexChanged };

            InitializeComponents();

            Mode = MODE.UNKNOWN;

            m_objLeading = objLeading;

            if (!(m_objLeading == null))
            {
                m_objLeading.ValueChanged += new EventHandler(leading_ValueChanged);

                m_tsLeading = _value[(int)INDEX_VALUE.CURRENT] - m_objLeading.Value;
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
            if (true)
            {
                _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];
                _value[(int)INDEX_VALUE.CURRENT] = new DateTime(
                    _value[(int)INDEX_VALUE.CURRENT].Year
                    , _value[(int)INDEX_VALUE.CURRENT].Month
                    , (obj as ComboBox).SelectedIndex + 1
                    , _value[(int)INDEX_VALUE.CURRENT].Hour
                    , 0
                    , 0
                );//??

                onSelectedIndexChanged();
            }
            else
                MessageBox.Show("");

        }

        /// <summary>
        /// Обработчик события - изменения номера месяца
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxMonth_onSelectedIndexChanged(object obj, EventArgs ev)
        {
            _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];

            if (_value[(int)INDEX_VALUE.CURRENT].Day ==
            DateTime.DaysInMonth(_value[(int)INDEX_VALUE.CURRENT].Year, _value[(int)INDEX_VALUE.CURRENT].Month))
                _value[(int)INDEX_VALUE.CURRENT] = new DateTime(
                _value[(int)INDEX_VALUE.CURRENT].Year
                , (obj as ComboBox).SelectedIndex + 1
                , DateTime.DaysInMonth(_value[(int)INDEX_VALUE.PREVIOUS].Year, (obj as ComboBox).SelectedIndex + 1)
                , _value[(int)INDEX_VALUE.CURRENT].Hour
                , 0
                , 0
            );
            else
                _value[(int)INDEX_VALUE.CURRENT] = new DateTime(
             _value[(int)INDEX_VALUE.CURRENT].Year
             , (obj as ComboBox).SelectedIndex + 1
             , _value[(int)INDEX_VALUE.CURRENT].Day
             , _value[(int)INDEX_VALUE.CURRENT].Hour
             , 0
             , 0);

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменения номера года
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxYear_onSelectedIndexChanged(object obj, EventArgs ev)
        {
            _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];
            _value[(int)INDEX_VALUE.CURRENT] = new DateTime(
                (obj as ComboBox).SelectedIndex + (DateTime.Today.Year - s_iBackwardYears)
                , _value[(int)INDEX_VALUE.CURRENT].Month
                , _value[(int)INDEX_VALUE.CURRENT].Day
                , _value[(int)INDEX_VALUE.CURRENT].Hour
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
            _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];
            _value[(int)INDEX_VALUE.CURRENT] = new DateTime(
                _value[(int)INDEX_VALUE.CURRENT].Year
                , _value[(int)INDEX_VALUE.CURRENT].Month
                , _value[(int)INDEX_VALUE.CURRENT].Day
                , (obj as ComboBox).SelectedIndex + 0
                , 0
                , 0
            );

            onSelectedIndexChanged();
        }
        /// <summary>
        /// Обработчик события - изменение значения любого из компонентов даты/времени
        /// </summary>
        private void onSelectedIndexChanged()
        {
            //Проверить наличие ведущего объекта
            if (!(m_objLeading == null))
                // изменить разность между собственным значением и значения ведущего объекта
                m_tsLeading = _value[(int)INDEX_VALUE.CURRENT] - m_objLeading.Value;
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
        private void leading_ValueChanged(object obj, EventArgs ev)
        {
            HDateTimePicker objLeading = obj as HDateTimePicker;
            ComboBox cbxYear = null
                , cbxMonth = null
                , cbxDay = null
                , cbxHour = null;
            int iDiffYear = -1;

            //??? учитывать значение в "ведущем" календаре
            iDiffYear = objLeading.Value.Year - _value[(int)INDEX_VALUE.CURRENT].Year;
            _value[(int)INDEX_VALUE.PREVIOUS] = _value[(int)INDEX_VALUE.CURRENT];
            _value[(int)INDEX_VALUE.CURRENT] = objLeading.Value + m_tsLeading;

            cbxYear = Controls.Find(INDEX_CONTROL.YEAR.ToString(), true)[0] as ComboBox;
            cbxMonth = Controls.Find(INDEX_CONTROL.MONTH.ToString(), true)[0] as ComboBox;
            cbxDay = Controls.Find(INDEX_CONTROL.DAY.ToString(), true)[0] as ComboBox;
            cbxHour = Controls.Find(INDEX_CONTROL.HOUR.ToString(), true)[0] as ComboBox;

            cbxYear.SelectedIndexChanged -= m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.YEAR];
            cbxMonth.SelectedIndexChanged -= m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.MONTH];
            cbxDay.SelectedIndexChanged -= m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.DAY];
            cbxHour.SelectedIndexChanged -= m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.HOUR];

            cbxYear.SelectedIndex += iDiffYear;
            cbxMonth.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Month - 1;
            cbxDay.Items.Clear();
            for (int i = 0; i < DateTime.DaysInMonth(_value[(int)INDEX_VALUE.CURRENT].Year, _value[(int)INDEX_VALUE.CURRENT].Month); i++)
                cbxDay.Items.Add(i + 1);
            cbxDay.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Day - 1;
            cbxHour.SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Hour; // == 0 ? _value[(int)INDEX_VALUE.CURRENT].Hour : _value[(int)INDEX_VALUE.CURRENT].Hour - 1;

            cbxYear.SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.YEAR];
            cbxMonth.SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.MONTH];
            cbxDay.SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.DAY];
            cbxHour.SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)INDEX_CONTROL.HOUR];

            ValueChanged(this, EventArgs.Empty);
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
            for (i = 0; i < 31 ; i++)//DateTime.DaysInMonth(_value[(int)INDEX_VALUE.CURRENT].Year, _value[(int)INDEX_VALUE.CURRENT].Month)
                (ctrl as ComboBox).Items.Add(i + 1);
            (ctrl as ComboBox).SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Day - 1;
            (ctrl as ComboBox).SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)indx];

            //Дата - наименование месяца
            indx = INDEX_CONTROL.MONTH;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 2, 0);
            SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
            for (i = 0; i < 12; i++)
                (ctrl as ComboBox).Items.Add(HDateTime.NameMonths[i]);
            (ctrl as ComboBox).SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Month - 1;
            (ctrl as ComboBox).SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)indx];

            //Дата - год
            indx = INDEX_CONTROL.YEAR;
            ctrl = new ComboBox();
            ctrl.Name = indx.ToString();
            ctrl.Dock = DockStyle.Fill;
            (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(ctrl, 6, 0);
            SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
            for (i = (_value[(int)INDEX_VALUE.CURRENT].Year - s_iBackwardYears); i < (_value[(int)INDEX_VALUE.CURRENT].Year + s_iForwardYears); i++)
                (ctrl as ComboBox).Items.Add(i.ToString());
            (ctrl as ComboBox).SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Year - (_value[(int)INDEX_VALUE.CURRENT].Year - s_iBackwardYears);
            (ctrl as ComboBox).SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)indx];

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
            (ctrl as ComboBox).SelectedIndex = _value[(int)INDEX_VALUE.CURRENT].Hour - 0;
            (ctrl as ComboBox).SelectedIndexChanged += m_arSelectIndexChangedHandlers[(int)indx];

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
