﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TepCommon;
using InterfacePlugIn;
using System.Drawing;
using System.Reflection;
using ASUTP;

namespace PluginTaskBalTeplo
{
    public partial class PanelTaskBalTeplo : HPanelTepCommon
    {
        public enum INDEX_VIEW_VALUES { Block, Vyvod, PromPlozsh };

        private INDEX_VIEW_VALUES m_ViewValues;

        /// <summary>
        /// Объект класса для расчета технико-экономических показателей
        /// </summary>
        protected HandlerDbTaskBalTeploCalculate m_calculate;

        /// <summary>
        /// 
        /// </summary>
        public enum INDEX_CALC : int
        {
            UNKNOWN = -1
            , CALC
            , CorCALC
            , COUNT
        }

        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_Block,
            DGV_Output,
            DGV_TeploBL,
            DGV_TeploOP,
            DGV_PromPlozsh,
            DGV_Param
                , LABEL_DESC
        }

        /// <summary>
        /// ???
        /// </summary>
        protected enum INDEX_CONTEXT
        {
            ID_CON = 10
        }

        /// <summary>
        /// Объект доступа к данным
        /// </summary>
        private HandlerDbTaskBalTeploCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskBalTeploCalculate; } }

        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementBalTeplo();
        }

        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        protected DataGridViewBalTeploValues dgvBlock,
            dgvOutput,
            dgvTeploBL,
            dgvTeploOP,
            dgvPromPlozsh,
            dgvParam;

        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementBalTeplo PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();
                else
                    ;

                return _panelManagement as PanelManagementBalTeplo;
            }
        }

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskBalTeploCalculate();
        }

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(HPanelTepCommon.PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Конструктор - основной
        /// </summary>
        /// <param name="iFunc">Объект для взаимодействия с вызывающим приложением</param>
        public PanelTaskBalTeplo(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES | TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.BAL_TEPLO;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDatetime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;

            m_calculate = new HandlerDbTaskBalTeploCalculate();

            InitializeComponents();

            PanelManagement.EventCheckedChangedIndexViewValues += new EventHandler(onCheckedChangedIndexViewValues);
        }

        private void onCheckedChangedIndexViewValues(object sender, EventArgs e)
        {
            PanelManagementBalTeplo.CheckedChangedIndexViewValuesEventArgs ev = e as PanelManagementBalTeplo.CheckedChangedIndexViewValuesEventArgs;

            m_ViewValues = (INDEX_VIEW_VALUES)((Control)sender).Tag;

            switch (m_ViewValues) {
                case INDEX_VIEW_VALUES.Block:
                    dgvOutput.Visible = false;
                    dgvTeploOP.Visible = false;
                    dgvParam.Visible = false;
                    dgvPromPlozsh.Visible = false;
                    dgvBlock.Visible = true;
                    dgvTeploBL.Visible = true;
                    break;
                case INDEX_VIEW_VALUES.Vyvod:
                    dgvBlock.Visible = false;
                    dgvTeploBL.Visible = false;
                    dgvParam.Visible = false;
                    dgvPromPlozsh.Visible = false;
                    dgvTeploOP.Visible = true;
                    dgvOutput.Visible = true;
                    break;
                case INDEX_VIEW_VALUES.PromPlozsh:
                    dgvBlock.Visible = false;
                    dgvOutput.Visible = false;
                    dgvTeploBL.Visible = false;
                    dgvTeploOP.Visible = false;
                    dgvParam.Visible = true;
                    dgvPromPlozsh.Visible = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Кол-во дней в текущем месяце
        /// </summary>
        /// <param name="numMonth">Номер месяца</param>
        /// <returns>Кол-во дней</returns>
        public int DayIsMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_DatetimeRange.Begin.Year, Session.m_DatetimeRange.Begin.Month);
            }
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementBalTeplo : PanelManagementTaskCalculate //HPanelCommon
        {
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1,
                BUTTON_IMPORT, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT,
                MENUITEM_UPDATE,
                MENUITEM_HISTORY,
                RADIO_BLOCK,
                RADIO_VYVOD,
                RADIO_PROM_PLOZSH,
                COUNT
            }

            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }
            /// <summary>
            /// Конмтруктор - основной (без параметров)
            /// </summary>
            public PanelManagementBalTeplo()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels) //6, 8
            {
                InitializeComponents();

                for (INDEX_CONTROL indx = INDEX_CONTROL.RADIO_BLOCK; !(indx > INDEX_CONTROL.RADIO_PROM_PLOZSH); indx++)
                    (Controls.Find(indx.ToString(), true)[0] as RadioButton).CheckedChanged += new EventHandler(onCheckedChangedIndexViewValues);
            }

            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"

                SuspendLayout();

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new ASUTP.Control.DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - импортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_IMPORT.ToString();
                ctrl.Text = @"Импорт";
                ctrl.Dock = DockStyle.Top;
                ctrl.Visible = true;
                ctrl.Enabled = false;
                //ctrlBSend.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                ctrl.Visible = true;
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_BLOCK.ToString();
                ctrl.Text = @"По блокам";
                ctrl.Tag = INDEX_VIEW_VALUES.Block;
                (ctrl as RadioButton).Checked = true;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_VYVOD.ToString();
                ctrl.Text = @"По выводам";
                ctrl.Tag = INDEX_VIEW_VALUES.Vyvod;
                //ctrlRadioTeplo.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_PROM_PLOZSH.ToString();
                ctrl.Text = @"Пром. площадки";
                ctrl.Tag = INDEX_VIEW_VALUES.PromPlozsh;
                //ctrlRadioProm.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            ///// <summary>
            ///// Обработчик события - изменение дата/время окончания периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            //{
            //    HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;
            //    DateTimeRangeValue_Changed?.Invoke(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
            //}

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                throw new NotImplementedException();
            }

            private void onCheckedChangedIndexViewValues(object obj, EventArgs e)
            {
                if ((obj as RadioButton).Checked == true)
                    EventCheckedChangedIndexViewValues?.Invoke(obj, new CheckedChangedIndexViewValuesEventArgs());
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                //throw new NotImplementedException();
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения значения ячейки
            /// </summary>
            public class CheckedChangedIndexViewValuesEventArgs : EventArgs
            {
                /// <summary>
                /// Компонента
                /// </summary>
                public object m_Comp;

                public CheckedChangedIndexViewValuesEventArgs()
                    : base()
                {
                    m_Comp = null;
                }

                public CheckedChangedIndexViewValuesEventArgs(int comp)
                    : this()
                {
                    m_Comp = comp;
                }
            }

            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public EventHandler EventCheckedChangedIndexViewValues;
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvValues = 4;

            SuspendLayout();

            posRow = 0;

            #region DGV
            dgvBlock = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Block.ToString(), HandlerDb.GetValueAsRatio);
            dgvBlock.Dock = DockStyle.Fill;
            dgvBlock.Name = INDEX_CONTROL.DGV_Block.ToString();
            dgvBlock.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Block;
            dgvBlock.AllowUserToResizeRows = false;
            dgvBlock.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBlock.Visible = true;
            this.Controls.Add(dgvBlock, 4, posRow);
            this.SetColumnSpan(dgvBlock, 9); this.SetRowSpan(dgvBlock, 5);
            //
            dgvOutput = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Output.ToString(), HandlerDb.GetValueAsRatio);
            dgvOutput.Dock = DockStyle.Fill;
            dgvOutput.Name = INDEX_CONTROL.DGV_Output.ToString();
            dgvOutput.AllowUserToResizeRows = false;
            dgvOutput.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Output;
            dgvOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOutput.Visible = false;
            this.Controls.Add(dgvOutput, 4, posRow);
            this.SetColumnSpan(dgvOutput, 9); this.SetRowSpan(dgvOutput, 5);
            //
            dgvTeploBL = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_TeploBL.ToString(), HandlerDb.GetValueAsRatio);
            dgvTeploBL.Dock = DockStyle.Fill;
            dgvTeploBL.Name = INDEX_CONTROL.DGV_TeploBL.ToString();
            dgvTeploBL.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.TeploBL;
            dgvTeploBL.AllowUserToResizeRows = false;
            dgvTeploBL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploBL.Visible = true;
            this.Controls.Add(dgvTeploBL, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploBL, 9); this.SetRowSpan(dgvTeploBL, 5);
            //
            dgvTeploOP = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_TeploOP.ToString(), HandlerDb.GetValueAsRatio);
            dgvTeploOP.Dock = DockStyle.Fill;
            dgvTeploOP.Name = INDEX_CONTROL.DGV_TeploOP.ToString();
            dgvTeploOP.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.TeploOP;
            dgvTeploOP.AllowUserToResizeRows = false;
            dgvTeploOP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploOP.Visible = false;
            this.Controls.Add(dgvTeploOP, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploOP, 9); this.SetRowSpan(dgvTeploOP, 5);
            //
            dgvPromPlozsh = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_PromPlozsh.ToString(), HandlerDb.GetValueAsRatio);
            dgvPromPlozsh.Dock = DockStyle.Fill;
            dgvPromPlozsh.Name = INDEX_CONTROL.DGV_PromPlozsh.ToString();
            dgvPromPlozsh.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.PromPlozsh;
            dgvPromPlozsh.AllowUserToResizeRows = false;
            dgvPromPlozsh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPromPlozsh.Visible = false;
            this.Controls.Add(dgvPromPlozsh, 4, posRow);
            this.SetColumnSpan(dgvPromPlozsh, 9); this.SetRowSpan(dgvPromPlozsh, 5);
            //
            dgvParam = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Param.ToString(), HandlerDb.GetValueAsRatio);
            dgvParam.Dock = DockStyle.Fill;
            dgvParam.Name = INDEX_CONTROL.DGV_Param.ToString();
            dgvParam.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Param;
            dgvParam.AllowUserToResizeRows = false;
            dgvParam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParam.Visible = false;
            this.Controls.Add(dgvParam, 4, posRow + 5);
            this.SetColumnSpan(dgvParam, 9); this.SetRowSpan(dgvParam, 5);
            #endregion
            //
            this.Controls.Add(PanelManagement, 0, posRow);
            this.SetColumnSpan(PanelManagement, posColdgvValues);
            this.SetRowSpan(PanelManagement, RowCount);//this.RowCount);     

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(btnHistory_OnClick);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(panelTepCommon_btnSave_onClick);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_IMPORT.ToString(), true)[0] as Button).Click +=
                new EventHandler(panelTaskBalTeplo_btnImport_onClick);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(panelTaskbalTeplo_btnExport_onClick);

            dgvBlock.CellParsing += dgvCellParsing;
            dgvOutput.CellParsing += dgvCellParsing;
            dgvParam.CellParsing += dgvCellParsing;
            dgvPromPlozsh.CellParsing += dgvCellParsing;
            dgvTeploBL.CellParsing += dgvCellParsing;
            dgvTeploOP.CellParsing += dgvCellParsing;
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Экспорт"
        /// </summary>
        /// <param name="sender">Объект - инициатор события (кнопка)</param>
        /// <param name="e">Аргумент события</param>
        private void panelTaskbalTeplo_btnExport_onClick(object sender, EventArgs e)
        {
            //rptExcel.CreateExcel(dgvAB);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект - инициатор события (кнопка "Отправить")</param>
        /// <param name="e">Аргумент события</param>
        private void panelTaskBalTeplo_btnImport_onClick(object sender, EventArgs e)
        {
            //int err = -1;
            string toSend = (Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0] as TextBox).Text;

            //m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
            //    dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err), dgvBlock, HandlerDb.getOutPut(out err));
            //rptsNSS.SendMailToNSS(m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
            //, HandlerDb.GetDateTimeRangeValuesVar(), toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender">Объект - инициатор события (представление)</param>
        /// <param name="e">Аргумент события</param>
        void dgvCellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            //    int err = -1;
            //    int id_put = -1;
            //    string N_ALG = (((DataGridViewBalTeploValues)sender).Columns[e.ColumnIndex] as DataGridViewBalTeploValues.HDataGridViewColumn).m_N_ALG;
            //    int id_comp = Convert.ToInt32(((DataGridViewBalTeploValues)sender).Rows[e.RowIndex].HeaderCell.Value);

            //    if ((((DataGridViewBalTeploValues)sender).Columns[e.ColumnIndex] as DataGridViewBalTeploValues.HDataGridViewColumn).m_bInPut == true)
            //    {
            //        DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
            //        if (rows.Length == 1)
            //            id_put = Convert.ToInt32(rows[0]["ID"]);
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            //    }
            //    else
            //    {
            //        DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
            //        if (rows.Length == 1)
            //            id_put = Convert.ToInt32(rows[0]["ID"]);
            //        m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            //    }
            //    HandlerDb.RegisterDbConnection(out err);
            //    HandlerDb.RecUpdateInsertDelete(
            //        TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name
            //        , "ID_PUT,ID_SESSION"
            //        , null
            //        , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
            //        , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
            //        , out err
            //    );
            //    //HandlerDb.insertInValues(m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE], out err);
            //    HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
            //    m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetVariableValues (
            //        TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
            //        , out err
            //    );
            //    m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetVariableValues(
            //        TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES
            //        , out err
            //    );
            //    m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
            //    m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
            //        m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();

            //    HandlerDb.UnRegisterDbConnection();

            //    dgvBlock.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    dgvOutput.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    dgvTeploBL.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    dgvTeploOP.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    dgvParam.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    dgvPromPlozsh.ShowValues(m_arTableEdit_in, m_arTableEdit_out
            //        , m_dictTableDictPrj);
            //    ((DataGridViewBalTeploValues)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = e.Value;
        }

        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            HandlerDb.Stop();

            base.Stop();
        }

        ///// <summary>
        ///// получение значений
        ///// создание сессии
        ///// </summary>
        ///// <param name="arQueryRanges"></param>
        ///// <param name="err">номер ошибки</param>
        ///// <param name="strErr"></param>
        //private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        //{
        //    err = 0;
        //    strErr = string.Empty;
        //    //Создание сессии
        //    Session.New();
        //    ////изменение начальной даты
        //    //if (arQueryRanges.Count() > 1)
        //    //    arQueryRanges[1] = new DateTimeRange(arQueryRanges[1].Begin.AddDays(-(arQueryRanges[1].Begin.Day - 1))
        //    //        , arQueryRanges[1].End.AddDays(-(arQueryRanges[1].End.Day - 2)));
        //    //else
        //    //    arQueryRanges[0] = new DateTimeRange(arQueryRanges[0].Begin.AddDays(-(arQueryRanges[0].Begin.Day - 1))
        //    //        , arQueryRanges[0].End.AddDays(DayIsMonth - arQueryRanges[0].End.Day));

        //    //Запрос для получения архивных данных
        //    m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.INVALUES, out err);
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //        (
        //        TaskCalculateType
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err
        //        );
        //    m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Merge(HandlerDb.GetValuesDayVar
        //        (
        //        TaskCalculateType
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err
        //        ));

        //    //Получение значений по-умолчанию input
        //    m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.INVALUES, out err);

        //    m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.OUTVALUES, out err);
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //        (
        //        TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err
        //        );
        //    m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.OUTVALUES, out err);

        //    //Проверить признак выполнения запроса
        //    if (err == 0)
        //    {
        //        //Проверить признак выполнения запроса
        //        if (err == 0)
        //            //Начать новую сессию расчета
        //            //, получить входные для расчета значения для возможности редактирования
        //            HandlerDb.CreateSession(m_Id
        //                , Session.CountBasePeriod
        //                , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
        //                , ref m_arTableOrigin_in
        //                , ref m_arTableOrigin_out
        //                , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
        //                , out err, out strErr);
        //        else
        //            strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
        //                + @" по " + Session.m_rangeDatetime.End.ToString();
        //    }
        //    else
        //        strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
        //            + @" по " + Session.m_rangeDatetime.End.ToString();
        //}

        ///// <summary>
        ///// copy
        ///// </summary>
        //private void setValues()
        //{
        //    m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //        m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Copy();
        //    m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
        //        = m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        //    m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //        m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Copy();
        //    m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
        //        = m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        //}

        ///// <summary>
        ///// загрузка/обновление данных
        ///// </summary>
        //private void updateDataValues()
        //{
        //    int err = -1
        //        , cnt = Session.CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
        //        , iAVG = -1
        //        , iRegDbConn = -1;
        //    string errMsg = string.Empty;

        //    m_handlerDb.RegisterDbConnection(out iRegDbConn);
        //    clear();

        //    if (!(iRegDbConn < 0))
        //    {
        //        // установить значения в таблицах для расчета, создать новую сессию
        //        setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg);

        //        if (err == 0)
        //        {
        //            if (m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows.Count > 0)
        //            {
        //                // создать копии для возможности сохранения изменений
        //                //setValues();
        //                //вычисление значений
        //                HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
        //                m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //                    (
        //                    TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES,
        //                    out err
        //                    );
        //                m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //                    (
        //                    TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES,
        //                    out err
        //                    );
        //                setValues();


        //            }
        //            else ;
        //        }
        //        else
        //        {
        //            // в случае ошибки "обнулить" идентификатор сессии
        //            deleteSession();
        //            throw new Exception(@"PanelTaskBalTeplo::updatedataValues() - " + errMsg);
        //        }
        //        //удалить сессию
        //        //deleteSession();
        //    }
        //    else
        //        ;

        //    if (!(iRegDbConn > 0))
        //        m_handlerDb.UnRegisterDbConnection();
        //    else
        //        ;
        //}

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void btnHistory_OnClick(object obj, EventArgs ev)
        {
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;

            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            // ... - загрузить/отобразить значения из БД

            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
        }

        /// <summary>
        /// Установить признак активности панель при выборе ее пользователем
        /// </summary>
        /// <param name="activate">Признак активности</param>
        /// <returns>Результат выполнения - был ли установлен признак</returns>
        public override bool Activate(bool activate)
        {
            bool bRes = false;
            int err = -1;

            bRes = base.Activate(activate);

            if (bRes == true)
            {
                if (activate == true)
                    HandlerDb.InitSession(out err);
                else
                { }
            }
            else
            { }

            return bRes;
        }

        /// <summary>
        /// Инициализация словарных_проектных таблиц, активных/пассивных элементов управления
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении метода</param>
        /// <param name="errMsg">Сообщение об ошибке, соответствующее признаку ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_PERIOD idProfilePeriod;
            ID_TIMEZONE idProfileTimezone;

            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            int i = -1;
            string strItem = string.Empty;

            // ВАЖНО! Обязательно до инициализации таблиц проекта (сортировка призойдет при вызове этой функции).
            HandlerDb.ModeNAlgSorting = HandlerDbTaskCalculate.MODE_NALG_SORTING.NotSortable;

            initialize(new ID_DBTABLE[] {
                ID_DBTABLE.TIMEZONE,
                ID_DBTABLE.COMP_LIST,
                ID_DBTABLE.MEASURE,
                ID_DBTABLE.RATIO,
                ID_DBTABLE.INALG,
                ID_DBTABLE.OUTALG,
                ID_DBTABLE.TIME,
                ID_DBTABLE.IN_PARAMETER,
                ID_DBTABLE.OUT_PARAMETER}
                , out err, out errMsg
            );

            HandlerDb.FilterDbTableTimezone = TepCommon.HandlerDbTaskCalculate.DbTableTimezone.Utc;
            HandlerDb.FilterDbTableTime = TepCommon.HandlerDbTaskCalculate.DbTableTime.Hour;

            if (err == 0) {
                try {

                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = (ID_TIMEZONE)int.Parse(m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.TIMEZONE));
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE], idProfileTimezone);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = ID_PERIOD.HOUR;
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME], idProfilePeriod);

                    /*m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG],
                        m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], m_dictTableDictPrj[ID_DBTABLE.RATIO], HandlerDb.ListPutParameter*/

                    dgvBlock.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));
                    dgvOutput.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));
                    dgvTeploBL.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));
                    dgvTeploOP.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));
                    dgvPromPlozsh.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));
                    dgvParam.InitializeStruct(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter, GetProfileDataGridView((int)dgvBlock.m_ViewValues));

                    ctrl = Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0];

                    //из profiles

                    for (int j = 0; j < HandlerDb.m_dt_profile.Rows.Count; j++)
                        if (Convert.ToInt32(HandlerDb.m_dt_profile.Rows[j]["CONTEXT"]) == (int)INDEX_CONTEXT.ID_CON)
                            ctrl.Text = HandlerDb.m_dt_profile.Rows[j]["VALUE"].ToString().TrimEnd();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Возвратить настроки профиля для указанного ДатаГридВью на текущей вкладке
        /// </summary>
        /// <param name="tag">Идентификатор ДатаГридВью</param>
        /// <returns>Словарь со значениями параметров профиля ДатаГридВью</returns>
        private Dictionary<int, object[]> GetProfileDataGridView(int tag)
        {
            Dictionary<int, object[]> dictProfileRes = new Dictionary<int, object[]>();
            string value = string.Empty;
            //??? это не контекст, почему не константы
            string[] contexts = { "33", "34" };
            List<double> ids = new List<double>();
            TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN;

            List<object> obj = new List<object>();

            foreach (string context in contexts)
            {
                value = m_dictProfile.GetAttribute(tag, context, HTepUsers.ID_ALLOWED.INPUT_PARAM);

                ids.Clear();
                value.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList().ForEach(val => { ids.Add(ASUTP.Core.HMath.doubleParse(val)); });

                type = context.Equals(contexts[0]) == true ? TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES : //??? 33
                    context.Equals(contexts[1]) == true ? TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES : //??? 34; как в 'INPUT_PARAM' оказались 'OUT_VALUES'
                        TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN;

                obj.Add(new object[] { ids.ToArray(), type, m_dictProfile.GetObjects(context) });
            }

            dictProfileRes.Add(tag, obj.ToArray());

            return dictProfileRes;
        }

        #region Обработка измнения значений основных элементов управления на панели управления 'PanelManagement'
        /// <summary>
        /// Обработчик события при изменении значения
        ///  одного из основных элементов управления на панели управления 'PanelManagement'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            base.panelManagement_EventIndexControlBase_onValueChanged(obj);

            if (obj is Enum)
                ; // switch ()
            else
                ;
        }

        //protected override void panelManagement_OnEventDetailChanged(object obj)
        //{
        //    base.panelManagement_OnEventDetailChanged(obj);
        //}
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение даты/времени, диапазона даты/времени)
        /// </summary>
        protected override void panelManagement_DatetimeRange_onChanged()
        {
            base.clear(); // базовый? или наследовать?

            base.panelManagement_DatetimeRange_onChanged();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_TimezoneChanged()
        {
            base.clear();

            base.panelManagement_TimezoneChanged();

        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            base.clear();

            base.panelManagement_Period_onChanged();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(TepCommon.HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);
        }
        #endregion

        /// <summary>
        /// Обработчик события - завершение обработки (длительной) операции
        /// </summary>
        /// <param name="evt">Идентификатор (признак) выполняемой операции</param>
        /// <param name="res">Признак результата выполнения операции</param>
        protected override void handlerDbTaskCalculate_onEventCompleted(HandlerDbTaskCalculate.EVENT evt, TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            HandlerDbTaskCalculate.KEY_VALUES key;
            IEnumerable<HandlerDbTaskCalculate.VALUE> inValues
                , outValues;

            string msgToStatusStrip = string.Empty;

            switch (evt) {
                case HandlerDbTaskCalculate.EVENT.SET_VALUES:
                    break;
                case HandlerDbTaskCalculate.EVENT.CALCULATE:
                    break;
                case HandlerDbTaskCalculate.EVENT.EDIT_VALUE:
                    break;
                case HandlerDbTaskCalculate.EVENT.SAVE_CHANGES:
                    break;
                default:
                    break;
            }

            dataAskedHostMessageToStatusStrip(res, msgToStatusStrip);

            if ((res == TepCommon.HandlerDbTaskCalculate.RESULT.Ok)
                || (res == TepCommon.HandlerDbTaskCalculate.RESULT.Warning))
                switch (evt) {
                    case HandlerDbTaskCalculate.EVENT.SET_VALUES: // отображать значения при отсутствии ошибок

                        key = new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT };
                        inValues = (HandlerDb.Values.ContainsKey(key) == true) ? HandlerDb.Values[key] : new List<HandlerDbTaskCalculate.VALUE>();
                        key = new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, TypeState = HandlerDbValues.STATE_VALUE.EDIT };
                        outValues = (HandlerDb.Values.ContainsKey(key) == true) ? HandlerDb.Values[key] : new List<HandlerDbTaskCalculate.VALUE>();

                        dgvBlock.ShowValues(inValues, outValues);
                        dgvOutput.ShowValues(inValues, outValues);
                        dgvTeploBL.ShowValues(inValues, outValues);
                        dgvTeploOP.ShowValues(inValues, outValues);
                        dgvParam.ShowValues(inValues, outValues);
                        dgvPromPlozsh.ShowValues(inValues, outValues);
                        break;
                    case HandlerDbTaskCalculate.EVENT.CALCULATE:
                        break;
                    case HandlerDbTaskCalculate.EVENT.EDIT_VALUE:
                        break;
                    case HandlerDbTaskCalculate.EVENT.SAVE_CHANGES:
                        break;
                    default:
                        break;
                }
            else
                ;
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(HandlerDbTaskCalculate.CalculateProccessEventArgs ev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Заполнение грида датами
        /// </summary>
        /// <param name="date">Тек.дата</param>
        /// <param name="numMonth">Номер месяца</param>
        private void fillDaysGrid(DateTime date, int numMonth)
        {
            DateTime dt = new DateTime(date.Year, date.Month, 1);
            dgvBlock.ClearRows();

            for (int i = 0; i < DayIsMonth; i++)
            {
                dgvBlock.AddRow();
                dgvBlock.Rows[i].Cells[0].Value = dt.AddDays(i).ToShortDateString();
            }
            dgvBlock.Rows[date.Day - 1].Selected = true;
        }

        /// <summary>
        /// Очистка гридов при смене даты/закрытии
        /// </summary>
        /// <param name="bClose">Параметр, указывающий, закрывается ли панель</param>
        protected override void clear(bool bClose = false)
        {
            // Вызов из базового класса??? Дублирование? 

            //if (bClose)
            //{
            //    dgvBlock.ClearRows();
            //    dgvOutput.ClearRows();
            //    dgvTeploBL.ClearRows();
            //    dgvTeploOP.ClearRows();
            //    dgvParam.ClearRows();
            //    dgvPromPlozsh.ClearRows();
            //}
            //else
            //{
            //    // очистить содержание представления
            //    dgvBlock.ClearValues();
            //    dgvOutput.ClearValues();
            //    dgvTeploBL.ClearValues();
            //    dgvTeploOP.ClearValues();
            //    dgvParam.ClearValues();
            //    dgvPromPlozsh.ClearValues();
            //}
            //base.clear(bClose);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки "Сохранить" - сохранение значений в БД
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие(кнопка)</param>
        /// <param name="ev">Аргумент события(пустой)</param>
        protected override void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            HandlerDb.m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
            HandlerDb.saveResInval(HandlerDb.getStructurOutval(out err, PanelManagement.DatetimeRange.Begin)
            , HandlerDb.m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD], out err);

            HandlerDb.m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
            HandlerDb.saveResOut(HandlerDb.getStructurOutval(out err, PanelManagement.DatetimeRange.Begin)
            , HandlerDb.m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD], out err);

            base.panelTepCommon_btnSave_onClick(obj, ev);
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 19;
            register(19, typeof(PanelTaskBalTeplo), @"Задача", @"Баланс тепла");
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

