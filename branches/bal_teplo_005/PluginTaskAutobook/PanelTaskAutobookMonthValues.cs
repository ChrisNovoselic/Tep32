﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TepCommon;
using Outlook = Microsoft.Office.Interop.Outlook;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using ASUTP;
using ASUTP.Core;

namespace PluginTaskAutobook
{
    public partial class PanelTaskAutobookMonthValues : HPanelTepCommon
    {
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL { UNKNOWN = -1, LABEL_DESC = 1, DGV_VALUES = 3 }
        /// <summary>
        /// Объект для взаимодействия с БД
        /// </summary>
        protected HandlerDbTaskAutobookMonthValuesCalculate HandlerDb { get { return __handlerDb as HandlerDbTaskAutobookMonthValuesCalculate; } }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementAutobookMonthValues();
        }
        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        private DataGridViewAutobookMonthValues m_dgvValues;
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementAutobookMonthValues PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementAutobookMonthValues;
            }
        }
        /// <summary>
        /// Создать объект класса для обращений (чтение/запись) к БД
        /// </summary>
        /// <returns>Экземпляр класса для обращений (чтение/запись) к БД</returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskAutobookMonthValuesCalculate();
        }
        /// <summary>
        /// Класс по работе с формированием 
        /// и отправкой отчета NSS
        /// </summary>
        private class ReportEMailNSS
        {
            ///// <summary>
            ///// Экземпляр класса
            ///// </summary>
            //OutlookMessage m_crtMsg;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            public ReportEMailNSS()
            {
            }
            /// <summary>
            /// Класс создания письма
            /// </summary>
            private class OutlookMessage : IDisposable
            {
                /// <summary>
                /// Объект обработки отправлений по электронной почты
                /// </summary>
                Outlook.Application m_outlookApp;
                /// <summary>
                /// Объект с данными для отправления в одном сообщении
                /// </summary>
                Outlook.MailItem _newMail;

                private bool _bIsOutlookRunning;

                /// <summary>
                /// Конструктор(основной)
                /// </summary>
                public OutlookMessage()
                {
                    m_outlookApp = null;
                    _bIsOutlookRunning = false;

                    try {
                        m_outlookApp = (Microsoft.Office.Interop.Outlook.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Outlook.Application");

                        _bIsOutlookRunning = true;
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, string.Format(@"ReportEMailNSS.OutlookMessage::ctor() - проверка использования выполняющегося экземпляра..."), Logging.INDEX_MESSAGE.NOT_SET);
                    } finally {
                        if (object.Equals(m_outlookApp, null) == true)
                            // если не выполняется - создать объект MS Outlook
                            try {
                                m_outlookApp = new Outlook.Application();
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, string.Format(@"ReportEMailNSS.OutlookMessage::ctor() - создание ..."), Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        else
                            ;
                    }
                }

                public void Dispose()
                {
                    closeOutlook();
                }

                /// <summary>
                /// Формирование письма
                /// </summary>
                /// <param name="subject">тема письма</param>
                /// <param name="body">тело сообщения</param>
                /// <param name="to">кому/куда</param>
                public bool Format(string subject, string body, string to)
                {
                    bool bRes = false;

                    string msgAbort = string.Empty;

                    try {
                        msgAbort = string.Format(@"PanelTaskAutobookMonthValues.ReportEMailNSS.OutlookMessage::Format (to={0}) - не создан объект MS Outlook...", to);

                        if (Equals(m_outlookApp, null) == false) {
                            _newMail = (Outlook.MailItem)m_outlookApp?.CreateItem(Outlook.OlItemType.olMailItem);
                            _newMail.To = to;
                            _newMail.Subject = subject;
                            _newMail.Body = body;
                            _newMail.Importance = Outlook.OlImportance.olImportanceNormal;
                            _newMail.Display();

                            bRes = true;
                        } else
                            Logging.Logg().Error(msgAbort, Logging.INDEX_MESSAGE.NOT_SET);
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, msgAbort, Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return bRes;
                }

                public void Send()
                {
                    if (Equals(_newMail, null) == false)
                        _newMail.Send();
                    else
                        Logging.Logg().Error(string.Format(@"PanelTaskAutobookMonthValues.ReportEMailNSS.OutlookMessage::Send () - сообщение (не создано)не отправлено..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
                /// <summary>
                /// Прикрепление файла к письму
                /// </summary>
                /// <param name="mail">Сообщение для отправления</param>
                private void AddAttachment(Outlook.MailItem mail)
                {
                    OpenFileDialog attachment = new OpenFileDialog();

                    attachment.Title = "Select a file to send";
                    attachment.ShowDialog();

                    if (attachment.FileName.Length > 0) {
                        mail.Attachments.Add(
                            attachment.FileName,
                            Outlook.OlAttachmentType.olByValue,
                            1,
                            attachment.FileName);
                    } else
                        ;
                }
                /// <summary>
                /// Завершить работу клиента обработки сообщений электронной почты
                /// </summary>
                private void closeOutlook()
                {
                    if (Equals(m_outlookApp, null) == false) {
                        // проверить: не выполнялся ли ранее клиент
                        if (_bIsOutlookRunning == false)
                            m_outlookApp.Quit();
                        else
                            ;

                        Marshal.ReleaseComObject(m_outlookApp);
                    } else
                        ;

                    GC.Collect();
                }
            }
            /// <summary>
            /// Создать содержание сообщения для отправления по эл./почте
            /// </summary>
            /// <param name="parameters">Список параметров для отправления</param>
            /// <param name="values">Список значений для отправления</param>
            /// <param name="fGetValueAsRatio">Делегат для преобразования значений</param>
            /// <param name="reportDate">Дата за которую отправляются значения</param>
            /// <returns>Строка(с переводами строк) - тело сообщения</returns>
            private string createBodyToSend(IEnumerable<TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> parameters
                , IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> values
                , Func<int, int, float, int, float> fGetValueAsRatio
                , DateTime reportDate)
            {
                string strRes = string.Empty;

                int[] arIdComponent = {
                    HandlerDbTaskAutobookMonthValuesCalculate.TaskAutobookMonthValuesCalculate.ST
                    , HandlerDbTaskAutobookMonthValuesCalculate.TaskAutobookMonthValuesCalculate.GTP12
                    , HandlerDbTaskAutobookMonthValuesCalculate.TaskAutobookMonthValuesCalculate.GTP36
                };
                HandlerDbTaskCalculate.PUT_PARAMETER putPar;

                if (parameters.Count() == values.Count()) {
                    strRes = string.Format(@"BEGIN {0}(DATE):{1}{0}"
                        , Environment.NewLine
                        , reportDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

                    for (int i = 0; i < arIdComponent.Length; i ++) {
                        putPar = parameters.FirstOrDefault(par => { return par.IdComponent == arIdComponent[i]; });

                        strRes += string.Format(@"({1:00}): {2}:{0}"
                            , Environment.NewLine
                            , (i + 1)
                            , fGetValueAsRatio(putPar.m_idNAlg, putPar.m_Id, values.FirstOrDefault(value => { return value.m_IdPut == putPar.m_Id; }).value, -1));
                    }

                    strRes += @"END ";
                } else
                    Logging.Logg().Error(string.Format(@"ReportEMailNSS::createBodyToSend (date={0}) - в аргументе нет набора за требуюмую дату...", reportDate), Logging.INDEX_MESSAGE.NOT_SET);

                return strRes;
            }

            ///// <summary>
            ///// Редактирование значения
            ///// </summary>
            ///// <param name="val">значение</param>
            ///// <returns>измененное знач.</returns>
            //private string fewerValue(double val)
            //{
            //    return Convert.ToString(val * Math.Pow(10F, 1 * vsRatio));
            //}

            /// <summary>
            /// Создание. Подготовка. Отправка письма.
            /// </summary>
            /// <param name="tableSource">таблица с данными</param>
            /// <param name="dtSend">выбранный промежуток</param>
            /// <param name="to">получатель</param>
            public void SendMailToNSS(IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> parameters
                , IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> values
                , Func<int, int, float, int, float> fGetValueAsRatio
                , DateTime dtSend
                , string to)
            {
                string bodyMsg = string.Empty
                    , sbjct = string.Empty;
                DateTime reportDate = DateTime.MinValue;

                reportDate = dtSend.AddHours(6).Date;//??
                sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
                bodyMsg = createBodyToSend(parameters, values, fGetValueAsRatio, reportDate);

                if (sbjct.Equals(string.Empty) == false)
                    using (OutlookMessage outlookMessage = new OutlookMessage()) {
                        if (outlookMessage.Format(sbjct, bodyMsg, to) == true)
                            outlookMessage.Send();
                        else
                            ;
                    }
                else
                    ;
            }
        }

        private class ReportMSExcel : TepCommon.ReportMSExcel
        {
            public ReportMSExcel(string nameTemplateWorkbook) : base(nameTemplateWorkbook)
            {
            }

            protected override void create(int headerColumn, int beginDataRow, Dictionary<int, List<string>> allValues, DateTimeRange dtRange)
            {
                int cntDays = -1;

                base.create(headerColumn, beginDataRow, allValues, dtRange);

                cntDays = (int)(dtRange.End.Date - dtRange.Begin.Date).TotalDays;
                // TODO: получить значение плановой выработки э/э
                m_wrkSheet.get_Range("C5").Value2 = m_wrkSheet.get_Range(string.Format("H{0}", (beginDataRow + cntDays - 1))).Value2;
                m_wrkSheet.get_Range("A4").Value2 = string.Format(@"{0} {1}", HDateTime.NameMonths[dtRange.Begin.Month - 1], dtRange.Begin.Year);
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookMonthValues(ASUTP.PlugIn.IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES | TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDatetime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended;

            InitializeComponent();

            m_dgvValues.EventCellValueChanged += new Action<HandlerDbTaskCalculate.KEY_VALUES, HandlerDbTaskCalculate.VALUE> (HandlerDb.SetValue);
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementAutobookMonthValues : PanelManagementTaskCalculate //HPanelCommon
        {
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1,
                BUTTON_SEND_EMAIL, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL, CALENDAR_EMAIL,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CHKBX_EDIT,
                COUNT
            }
            /// <summary>
            /// Признак разрешения сохранения значений (доступность кнопки сохранить)
            ///  , должен коррелировать с доступностью столбцов для редактирования
            /// </summary>
            public bool AllowButtonSaveEnabled { get { return _allowButtonSaveEnabled; } set { _allowButtonSaveEnabled = value; } }
            private bool _allowButtonSaveEnabled;
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
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementAutobookMonthValues()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels) //4, 3
            {
                Control ctrl;

                InitializeComponents();

                // обеспечить вкл./выкл. крнопки "Экспорт"
                ctrl = findControl(INDEX_CONTROL.TXTBX_EMAIL.ToString());
                (ctrl as TextBox).TextChanged += tbxEMail_OnTextChanged;
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"

                SuspendLayout();

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new ASUTP.Control.DropDownButton ();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Календарь
                ctrl = new DateTimePicker();
                ctrl.Name = INDEX_CONTROL.CALENDAR_EMAIL.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as DateTimePicker).Format = DateTimePickerFormat.Custom;
                (ctrl as DateTimePicker).CustomFormat = "dd MMM, yyyy";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl,1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Поле с почтой
                ctrl = new TextBox();
                ctrl.Name = INDEX_CONTROL.TXTBX_EMAIL.ToString();
                //ctrlTxt.Text = @"Pasternak_AS@sibeco.su";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - Экспорт
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - Отправить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SEND_EMAIL.ToString();
                ctrl.Text = @"Отправить";
                ctrl.Enabled = false;
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);                                
                //Признак Корректировка_включена/корректировка_отключена 
                ctrl = new CheckBox();
                ctrl.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
                ctrl.Text = @"Корректировка значений разрешена";
                ctrl.Dock = DockStyle.Top;
                ctrl.Enabled = false;
                (ctrl as CheckBox).Checked = true;
                Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                // не используется
                //throw new NotImplementedException();
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                // не используется
                //throw new NotImplementedException();
            }

            private void tbxEMail_OnTextChanged(object obj, EventArgs ev)
            {
                bool bEMailSendEnabled = false;
                Button btnEMailSend;

                try {
                    new System.Net.Mail.MailAddress((obj as TextBox).Text);

                    bEMailSendEnabled = true;
                } catch {                    
                }

                btnEMailSend = findControl(INDEX_CONTROL.BUTTON_SEND_EMAIL.ToString()) as Button;
                if (!(btnEMailSend.Enabled == bEMailSendEnabled))
                    btnEMailSend.Enabled = bEMailSendEnabled;
                else
                    ;
            }

            #region Установить вкл./выкл. состояние для кнопки
            private void setButtonEnabled(INDEX_CONTROL indx, bool value)
            {
                Control ctrl = null;
                ctrl = findControl(indx.ToString());
                if (!(ctrl.Enabled == value))
                    ctrl.Enabled = value;
                else
                    ;
            }

            public bool ButtonSaveEnabled
            {
                set { setButtonEnabled(INDEX_CONTROL.BUTTON_SAVE, value & AllowButtonSaveEnabled); }
            }

            public bool ButtonExportEnabled
            {
                set { setButtonEnabled(INDEX_CONTROL.BUTTON_EXPORT, value); }
            }
            #endregion
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvValues = 4;

            SuspendLayout();

            posRow = 0;
            m_dgvValues = new DataGridViewAutobookMonthValues(INDEX_CONTROL.DGV_VALUES.ToString(), HandlerDb.GetValueAsRatio);            
            Controls.Add(m_dgvValues, posColdgvValues, posRow);
            SetColumnSpan(m_dgvValues, ColumnCount - posColdgvValues); SetRowSpan(m_dgvValues, 10);
            //
            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, posColdgvValues);
            SetRowSpan(PanelManagement, RowCount);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvValues);

            ResumeLayout(false);
            PerformLayout();

            ctrl = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            (ctrl as Button).Click += // действие по умолчанию
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            ((ctrl as Button).ContextMenuStrip.Items.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            ((ctrl as Button).ContextMenuStrip.Items.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(btnHistory_OnClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(panelTepCommon_btnSave_onClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SEND_EMAIL.ToString(), true)[0] as Button).Click +=
                new EventHandler(btnSend_onClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(btnExport_onClick);
            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CALENDAR.ToString(), true)[0] as Button).
        }

        /// <summary>
        /// обработка ЭКСПОРТА(временно)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnExport_onClick(object sender, EventArgs e)
        {
            new ReportMSExcel(@"TemplateAutobook.xlsx")
                .Create(@"Autobook", 1, 9, m_dgvValues.GetValuesToReportMSExcel(), Session.m_DatetimeRange);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnSend_onClick(object sender, EventArgs e)
        {
            ReportEMailNSS rep = new ReportEMailNSS();
            TepCommon.HandlerDbTaskCalculate.KEY_VALUES key;            
            DateTime dtValues = DateTime.MinValue;
            IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> putParameters;
            string e_mail = string.Empty;

            key = new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() {
                TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , TypeState = HandlerDbValues.STATE_VALUE.EDIT
            };
            dtValues = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CALENDAR_EMAIL.ToString(), true)[0] as DateTimePicker).Value.Date;
            putParameters = HandlerDb.GetPutParameters("191");
            e_mail = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL.ToString(), true)[0] as TextBox).Text;

            if (HandlerDb.Values.ContainsKey(key) == true)
                rep.SendMailToNSS(putParameters
                    , (from value in HandlerDb.Values[key]
                        where (value.stamp_value == ((HandlerDb.ModeDataDatetime == HandlerDbTaskCalculate.MODE_DATA_DATETIME.Begined)
                                ? dtValues.AddDays(1).Add(-Session.m_curOffsetUTC) // в случае, если данные сохраняются с меткой "начало интервала"
                                    : ((HandlerDb.ModeDataDatetime == HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended)
                                        ? dtValues.Add(-Session.m_curOffsetUTC) // в случае, если данные сохраняются с меткой "окончание интервала"
                                            : DateTime.MinValue)))
                            && (putParameters.Select(putPar => putPar.m_Id).Contains(value.m_IdPut))
                        select value)
                    , HandlerDb.GetValueAsRatio
                    , dtValues
                    , e_mail);
            else
                Logging.Logg().Error(string.Format(@"PanelTaskAutobookMonthValues::btnSend_onClick () - нет данных для отправления по e-mail..."), Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            HandlerDb.Stop();

            base.Stop();
        }

        protected override void handlerDbTaskCalculate_onEventCompleted(HandlerDbTaskCalculate.EVENT evt, TepCommon.HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            string mesToStatusStrip = string.Empty;

            switch (evt) {
                case HandlerDbTaskCalculate.EVENT.SET_VALUES: // вычисление значений, сохранение во временной таблице
                    mesToStatusStrip = string.Format(@"Получение значений из БД");
                    break;
                case HandlerDbTaskCalculate.EVENT.CALCULATE:
                    break;
                case HandlerDbTaskCalculate.EVENT.EDIT_VALUE: // отобразить значения
                    mesToStatusStrip = string.Format(@"Редактирование значения на форме");
                    break;
                case HandlerDbTaskCalculate.EVENT.SAVE_CHANGES:
                    mesToStatusStrip = string.Format(@"Сохранение значений в БД");
                    break;
                default:
                    break;
            }

            dataAskedHostMessageToStatusStrip(res, mesToStatusStrip);

            if ((res == TepCommon.HandlerDbTaskCalculate.RESULT.Ok)
                || (res == TepCommon.HandlerDbTaskCalculate.RESULT.Warning))
                switch (evt) {
                    case HandlerDbTaskCalculate.EVENT.SET_VALUES: // вычисление значений, сохранение во временной таблице
                        HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
                        break;
                    case HandlerDbTaskCalculate.EVENT.CALCULATE: // отобразить значения
                        m_dgvValues.ShowValues(HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() {
                                TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES
                                , TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() {
                                TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                                , TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , out err);
                        break;
                    case HandlerDbTaskCalculate.EVENT.EDIT_VALUE: // отобразить значения + разблокировать кнопку "Сохранить"
                        m_dgvValues.ShowValues(HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() {
                                TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES
                                , TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , HandlerDb.Values[new TepCommon.HandlerDbTaskCalculate.KEY_VALUES() {
                                TypeCalculate = TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                                , TypeState = HandlerDbValues.STATE_VALUE.EDIT }]
                            , out err);

                        PanelManagement.ButtonSaveEnabled = true;
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
            Logging.Logg().Debug(string.Format(@"PanelTaskAutobookYearlyPlan::handlerDbTaskCalculate_onCalculateProcess () - выполнен расчет <{0}>, тип={1}, рез-т={2}...", ev.m_nAlg.m_strNameShr, ev.m_type, ev.m_result), Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnHistory_OnClick(object obj, EventArgs ev)
        {
            try {
                // ... - загрузить/отобразить значения из БД
                HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"PanelTaskAutobookMonthValues::btnHistory_OnClick () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            try
            {
                // m_dgvValues.ClearValues();
                clear(false); //!!! нельзя вызывать, т.к. будет выполнен базовый метод
                // ... - загрузить/отобразить значения из БД
                HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"PanelTaskAutobookMonthValues::panelTepCommon_btnUpdate_onClick () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

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
                if (activate == true)
                    ;
                else
                    ;
            else
                ;

            return bRes;
        }

        /// <summary>
        /// инициализация элементов на форме
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_PERIOD idProfilePeriod;
            ID_TIMEZONE idProfileTimezone;
            string strItem = string.Empty
                , key = string.Empty;
            Control ctrl = null;

            // ВАЖНО! Обязательно до инициализации таблиц проекта (сортировка призойдет при вызове этой функции).
            HandlerDb.ModeNAlgSorting = HandlerDbTaskCalculate.MODE_NALG_SORTING.NotSortable;

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMEZONE, COMP, MEASURE, RATIO
            initialize(new ID_DBTABLE[] {
                    ID_DBTABLE.TIME
                    , ID_DBTABLE.TIMEZONE
                    , IsInParameters == true ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , IsOutParameters == true ? ID_DBTABLE.OUT_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.COMP_LIST
                    , ID_DBTABLE.MEASURE
                    , ID_DBTABLE.RATIO
                }
                , out err, out errMsg);

            HandlerDb.FilterDbTableTimezone = TepCommon.HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = TepCommon.HandlerDbTaskCalculate.DbTableTime.Month;
            HandlerDb.FilterDbTableCompList = TepCommon.HandlerDbTaskCalculate.DbTableCompList.Tec
                | TepCommon.HandlerDbTaskCalculate.DbTableCompList.Gtp
                | TepCommon.HandlerDbTaskCalculate.DbTableCompList.Tg;

            // возможность_редактирвоания_значений
            try {
                (findControl(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString()) as CheckBox).Checked =
                    m_dictProfile.GetBooleanAttribute(ID_PERIOD.MONTH, INDEX_CONTROL.DGV_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM);
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - установка признака Редактирование столбца...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            // активность_кнопки_сохранения
            try {
                PanelManagement.AllowButtonSaveEnabled =
                    m_dictProfile.GetBooleanAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL);
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - установка признака доступности кнопки Сохранить...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            try {
                if (err == 0) {
                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = (ID_TIMEZONE)Enum.Parse(typeof(ID_TIMEZONE), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.TIMEZONE));
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , idProfileTimezone);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)Enum.Parse(typeof(ID_PERIOD), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.PERIOD));
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , idProfilePeriod);

                    ctrl = Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL.ToString(), true)[0];
                    // из profiles
                    key = ((int)PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL).ToString();
                    if (m_dictProfile.Keys.Contains(key) == true)
                        ctrl.Text = m_dictProfile.GetAttribute(key, HTepUsers.ID_ALLOWED.ADDRESS_MAIL_AUTOBOOK);
                    else
                        Logging.Logg().Warning(string.Format(@"PanelTaskAutoBook::initialize () - в словаре 'm_dictProfile' не определен ключ [{0:1}]..."
                            , key, PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL)
                                , Logging.INDEX_MESSAGE.NOT_SET);

                    PanelManagement.AllowUserPeriodChanged = false;
                    PanelManagement.AllowUserTimezoneChanged = false;
                }
                else
                    Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutobookMonthValues::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void addValueRows()
        {
            m_dgvValues.AddRows (new DataGridViewValues.DateTimeStamp() {
                Start = PanelManagement.DatetimeRange.Begin + HandlerDb.OffsetUTC
                , Finish = PanelManagement.DatetimeRange.End + HandlerDb.OffsetUTC
                , Increment = TimeSpan.FromDays(1)
                , ModeDataDatetime = HandlerDb.ModeDataDatetime
            });
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

            m_dgvValues.AddColumns(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter);

            addValueRows();
        }        
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_Period_onChanged()
        {
            base.panelManagement_Period_onChanged();

            m_dgvValues.AddColumns(HandlerDb.ListNAlgParameter, HandlerDb.ListPutParameter);

            addValueRows();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);

            m_dgvValues.AddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);

            m_dgvValues.AddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить компонент станции (оборудовнаие)
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(TepCommon.HandlerDbTaskCalculate.TECComponent obj)
        {
            base.handlerDbTaskCalculate_onAddComponent(obj);
        }
        #endregion

        /// <summary>
        /// Очистить значения, при необходимости, удалить строки в представлении для отображения значений
        /// </summary>
        /// <param name="bClose">Признак неодбходимости удаления строк
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true)
            {
                m_dgvValues.Clear();
            } else
                // очистить содержание представления
                m_dgvValues.ClearValues();

            base.clear(bClose);
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

            PanelManagement.ButtonSaveEnabled = false;

            new Thread(new ParameterizedThreadStart(HandlerDb.SaveChanges)) { IsBackground = true }.Start(null);
        }
    }
}

