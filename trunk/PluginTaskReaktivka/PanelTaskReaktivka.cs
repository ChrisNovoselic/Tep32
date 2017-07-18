﻿using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;

namespace PluginTaskReaktivka
{
    public partial class PanelTaskReaktivka : HPanelTepCommon
    {
        /// <summary>
        /// Перечисление - идентификаторы элементов управления на панели
        /// </summary>
        protected enum INDEX_CONTROL
        {
            /// <summary>
            /// Неизвестный элемент
            /// </summary>
            UNKNOWN = -1,
            /// <summary>
            /// Представление для отображения значений
            /// </summary>
            DATAGRIDVIEW_VALUES,
            /// <summary>
            /// Панель оперативной справочной информпции
            /// </summary>
            LABEL_DESC
        }
        /// <summary>
        /// Объект для обращения к БД (чтение/сохранение значений)
        /// </summary>
        protected HandlerDbTaskReaktivkaCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskReaktivkaCalculate; } }
        /// <summary>
        /// Объект для создания отчета в MS Excel
        /// </summary>
        protected ReportExcel m_reportExcel;
        /// <summary>
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        protected enum INDEX_VIEW_VALUES : short
        {
            UNKNOWN = -1
                , SOURCE, ARCHIVE
                , COUNT
        }
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementReaktivka PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementReaktivka;
            }
        }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementReaktivka();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskReaktivkaCalculate();
        }
        /// <summary>
        /// Экземпляр класса отображения данных
        /// </summary>
        DataGridViewValuesReaktivka m_dgvValues;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc">Объект для взаимодействия с вызывающей программой</param>
        public PanelTaskReaktivka(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.REAKTIVKA;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDatetime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Begined;

            InitializeComponents();

            m_dgvValues.EventCellValueChanged += new Action<HandlerDbTaskCalculate.KEY_VALUES, HandlerDbTaskCalculate.VALUE>(HandlerDb.SetValue);
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
            m_dgvValues = new DataGridViewValuesReaktivka(INDEX_CONTROL.DATAGRIDVIEW_VALUES.ToString(), HandlerDb.GetValueAsRatio);

            foreach (DataGridViewColumn column in m_dgvValues.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"
            int posColdgvValues = 4
                , heightRowdgvValues = 10;

            SuspendLayout();

            Controls.Add(PanelManagement, 0, posRow = posRow + 1);
            SetColumnSpan(PanelManagement, posColdgvValues); SetRowSpan(PanelManagement, RowCount);

            Controls.Add(m_dgvValues, posColdgvValues, posRow);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvValues); SetRowSpan(m_dgvValues, heightRowdgvValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementReaktivka.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementReaktivka.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTaskReaktivka_btnHistory_onClick);
            (findControl(PanelManagementReaktivka.INDEX_CONTROL.BUTTON_SAVE.ToString())as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);
            (findControl(PanelManagementReaktivka.INDEX_CONTROL.BUTTON_EXPORT.ToString()) as Button).Click += panelTaskReaktivka_btnExport_onClick;
        }

        /// <summary>
        /// Обработчик события - нажатие клавиши ЭКСПОРТ
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void panelTaskReaktivka_btnExport_onClick(object sender, EventArgs e)
        {
            m_reportExcel = new ReportExcel();//
            m_reportExcel.CreateExcel(m_dgvValues, Session.m_DatetimeRange);
        }

        /// <summary>
        /// инициализация параметров вкладки
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_PERIOD idProfilePeriod;
            ID_TIMEZONE idProfileTimezone;
            string strItem = string.Empty;
            object[] keys;

            int role = (int)HTepUsers.Role;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, COMP, TIMEZONE, RATIO
            initialize(
                new ID_DBTABLE[] {
                    ID_DBTABLE.TIME
                    , ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , ID_DBTABLE.RATIO
                    , ID_DBTABLE.IN_PARAMETER
                    //, ID_DBTABLE.OUT_PARAMETER
                    ,
                }
                , out err, out errMsg);

            HandlerDb.FilterDbTableTimezone = HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = HandlerDbTaskCalculate.DbTableTime.Month;
            HandlerDb.FilterDbTableCompList = HandlerDbTaskCalculate.DbTableCompList.Tg;

            if (err == 0) {
                try {
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = ID_PERIOD.MONTH;
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , idProfilePeriod); //??? активный период требуется прочитать из [profile]
                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = ID_TIMEZONE.MSK;
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , idProfileTimezone); //??? активный пояс требуется прочитать из [profile]

                    PanelManagement.AllowUserPeriodChanged = false;
                    PanelManagement.AllowUserTimezoneChanged = false;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskReaktivka::initialize () - инициализация стандартных элементов управления...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                // возможность_редактирвоания_значений
                try {
                    keys = new object[] { ID_PERIOD.MONTH, INDEX_CONTROL.DATAGRIDVIEW_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM };
                    if (string.IsNullOrEmpty(m_dictProfile.GetAttribute(keys)) == false)
                        (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.CHKBX_ENABLED_DATAGRIDVIEW_VALUES.ToString(), true)[0] as CheckBox).Checked =
                            int.Parse(m_dictProfile.GetAttribute(keys)) == 1;
                    else
                        (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.CHKBX_ENABLED_DATAGRIDVIEW_VALUES.ToString(), true)[0] as CheckBox).Checked = false;

                    m_dgvValues.SetReadOnly(!(Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.CHKBX_ENABLED_DATAGRIDVIEW_VALUES.ToString(), true)[0] as CheckBox).Checked);
                    //??? это не одно и то же, что и редактирование представления? требуется объединить признаки, т.е. выставлять значение по одному и тому же признаку
                    if (string.IsNullOrEmpty(m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == false)
                        (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled =
                            int.Parse(m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == 1;
                    else
                        (Controls.Find(PanelManagementReaktivka.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskReaktivka::initialize () - установка свойств элементов в соответствии с профилем...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            } else
                errMsg = @"Неизвестная ошибка";
        }

        private void addValueRows()
        {
            TimeSpan tsOffsetUTC = TimeSpan.FromDays(1) - Session.m_curOffsetUTC;

            m_dgvValues.AddRows(new DataGridViewValues.DateTimeStamp() {
                Start = PanelManagement.DatetimeRange.Begin + tsOffsetUTC
                , Increment = TimeSpan.FromDays(1)
                , ModeDataDatetime = HandlerDb.ModeDataDatetime
            });
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
            base.panelManagement_DatetimeRange_onChanged();

            addValueRows();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_TimezoneChanged()
        {
            base.panelManagement_TimezoneChanged();

            addValueRows();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            base.panelManagement_Period_onChanged();

            addValueRows();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);

            m_dgvValues.AddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);

            m_dgvValues.AddPutParameter(obj);

            m_dgvValues.AddColumn(obj);
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);

            PanelManagement.AddComponent(obj);
        }
        #endregion

        /// <summary>
        /// Очистить представление (полнота ~ признака)
        /// </summary>
        /// <param name="bClose">Признак полноты очистки представления</param>
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true) {
                m_dgvValues.Clear();
            }
            else
                // очистить содержание представления
                m_dgvValues.ClearValues();

            base.clear(bClose);
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            int err = -1;

            new Thread(new ParameterizedThreadStart(HandlerDb.SaveChanges)) { IsBackground = true }.Start(null);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            ////???
            //clear();
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelTaskReaktivka_btnHistory_onClick(object obj, EventArgs ev)
        {
            ////???
            //clear();
            // ... - загрузить/отобразить значения из БД
            HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
        }

        /// <summary>
        /// Класс формирования отчета MS Excel 
        /// </summary>
        public class ReportExcel
        {
            private Excel.Application m_excApp;
            private Excel.Workbook m_workBook;
            private Excel.Worksheet m_wrkSheet;
            private object _missingObj = System.Reflection.Missing.Value;

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public ReportExcel()
            {
                m_excApp = new Excel.Application();
                m_excApp.Visible = false;
            }

            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgView">отрбражение данных</param>
            /// <param name="dtRange">дата</param>
            public void CreateExcel(DataGridView dgView, DateTimeRange dtRange)
            {
                string cellValue = string.Empty;

                if (addWorkBooks() == true) {
                    // успешное добавление листа в книгу
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Reaktivka");
                    int indxCol = 1;

                    try {
                        for (int i = 0; i < dgView.Columns.Count; i++) {
                            if (dgView.Columns[i].HeaderText.Equals(string.Empty) == false) {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxCol];

                                foreach (Excel.Range cell in colRange.Cells) {
                                    cellValue = Convert.ToString(cell.Value);

                                    if (cellValue.Equals(string.Empty) == false)
                                        if (cellValue == splitString(dgView.Columns[i].HeaderText)) {
                                            fillSheetExcel(colRange, dgView, i, cell.Row);

                                            break;
                                        } else
                                        // продолжать поиск
                                            ;
                                    else
                                    // продолжать поиск
                                        ;
                                }

                                indxCol++;
                            }
                        }
                        //
                        setSignature(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        closeExcel();
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);
                    } catch (Exception e) {
                        closeExcel();

                        Logging.Logg().Exception(e, string.Format("Panelreaktivka.ReportExcel::CreateExcel () - ошибка экспорта данных..."), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                } else
                    throw new Exception(string.Format(@"Panelreaktivka.ReportExcel::CreateExcel () - ошибка добавления листа в книгу MS Excel..."));
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateReaktivka.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
                    closeExcel();
                    bflag = false;
                    MessageBox.Show("Отсутствует шаблон для отчета Excel");
                }
                return bflag;
            }

            /// <summary>
            /// Обработка события - закрытие экселя
            /// </summary>
            /// <param name="Cancel"></param>
            void workBook_BeforeClose(ref bool Cancel)
            {
                closeExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                closeExcel();
            }

            /// <summary>
            /// Добавление подписи месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setSignature(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                Excel.Range exclTEC = exclWrksht.get_Range("B2");
                Excel.Range exclRMonth = exclWrksht.get_Range("A2");
                exclRMonth.Value2 = HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
            }

            /// <summary>
            /// Возвратить одну из частей строки, при наличии разделителя
            /// </summary>
            /// <param name="headerTxt">Исходная строка, в которой присутствует разделитель</param>
            /// <returns>Часть строки</returns>
            private string splitString(string headerTxt, char chSeparate = ',')
            {
                string[] spltHeader = headerTxt.Split(chSeparate);

                if (spltHeader.Length > 1)
                // разделитель присутствует - вернуть крайнюю часть
                    return spltHeader[1].TrimStart();
                else
                // разделитель отсутствует - вернуть исходную строку
                    return spltHeader[0];
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        row = i;
                        break;
                    }

                for (int j = 0; j < dgv.Rows.Count; j++)
                    if (dgv.Rows.Count - 1 != j)
                    {
                        colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                        row++;
                    }
                    else
                    {
                        deleteNullRow(colRange, row);

                        if (Convert.ToString(((Excel.Range)colRange.Cells[row]).Value) == "")
                            colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                    }

            }

            /// <summary>
            /// Удаление пустой строки
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="row">номер строки</param>
            private void deleteNullRow(Excel.Range colRange, int row)
            {
                Excel.Range rangeCol = (Excel.Range)m_wrkSheet.Columns[1];

                while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "")
                {
                    Excel.Range rangeRow = (Excel.Range)m_wrkSheet.Rows[row];
                    rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            private void closeExcel()
            {
                //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                m_excApp = null;
                m_workBook = null;
                m_wrkSheet = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(PanelManagementReaktivka.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //??? Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //m_dictProfile.SetAttribute();
            //Изменить структуру 'DataGridView'          
            (m_dgvValues as DataGridViewValuesReaktivka).UpdateStructure(ev);
        }

        protected override void handlerDbTaskCalculate_onEventCompleted(HandlerDbTaskCalculate.EVENT evt, TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            string msgToStatusStrip = string.Empty;

            HandlerDbTaskCalculate.KEY_VALUES key;
            IEnumerable<HandlerDbTaskCalculate.VALUE> inValues
                , outValues;

            switch (evt) {
                case HandlerDbTaskCalculate.EVENT.SET_VALUES:
                    msgToStatusStrip = string.Format(@"Получение значений из БД");
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

                        m_dgvValues.ShowValues(inValues, outValues, out err);
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
        /// Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        private string getNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = HandlerDbValues.s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable origin, DataTable edit, string unCol, out int err)
        {
            err = -1;

            __handlerDb.RecUpdateInsertDelete(nameTable
                , @"ID_PUT, DATE_TIME"
                , unCol
                , origin
                , edit
                , out err);
        }

        /// <summary>
        /// Нахождение имени таблицы для крайних строк
        /// </summary>
        /// <param name="strDate">дата</param>
        /// <param name="nameTable">изначальное имя таблицы</param>
        /// <returns>имя таблицы</returns>
        private static string extremeRow(string strDate, string nameTable)
        {
            DateTime dtStr = Convert.ToDateTime(strDate);
            string newNameTable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + newNameTable;
        }

        ///// <summary>
        ///// разбор данных по разным табилца(взависимости от месяца)
        ///// </summary>
        ///// <param name="origin">оригинальная таблица</param>
        ///// <param name="edit">таблица с данными</param>
        ///// <param name="nameTable">имя таблицы</param>
        ///// <param name="unCol">столбец, неучаствующий в InsertUpdate</param>
        ///// <param name="err">номер ошибки</param>
        //private void sortingDataToTable(DataTable origin
        //    , DataTable edit
        //    , string nameTable
        //    , string unCol
        //    , out int err)
        //{
        //    string nameTableExtrmRow = string.Empty
        //                  , nameTableNew = string.Empty;
        //    DataTable editTemporary = new DataTable()
        //        , originTemporary = new DataTable();

        //    err = -1;
        //    editTemporary = edit.Clone();
        //    originTemporary = origin.Clone();
        //    nameTableNew = nameTable;

        //    foreach (DataRow row in edit.Rows)
        //    {
        //        nameTableExtrmRow = extremeRow(row["DATE_TIME"].ToString(), nameTableNew);

        //        if (nameTableExtrmRow != nameTableNew)
        //        {
        //            foreach (DataRow rowOrigin in origin.Rows)
        //                if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month != Convert.ToDateTime(row["DATE_TIME"]).Month)
        //                    originTemporary.Rows.Add(rowOrigin.ItemArray);

        //            updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);

        //            nameTableNew = nameTableExtrmRow;
        //            editTemporary.Rows.Clear();
        //            originTemporary.Rows.Clear();
        //            editTemporary.Rows.Add(row.ItemArray);
        //        }
        //        else
        //            editTemporary.Rows.Add(row.ItemArray);
        //    }

        //    if (editTemporary.Rows.Count > 0)
        //    {
        //        foreach (DataRow rowOrigin in origin.Rows)
        //            if (extremeRow(Convert.ToDateTime(rowOrigin["DATE_TIME"]).ToString(), nameTableNew) == nameTableNew)
        //                originTemporary.Rows.Add(rowOrigin.ItemArray);

        //        updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);
        //    }
        //}

        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            HandlerDb.Stop();

            base.Stop();
        }
    }
}
