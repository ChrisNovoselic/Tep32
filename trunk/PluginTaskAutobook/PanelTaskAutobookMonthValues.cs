using HClassLibrary;
using System;
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
using Excel = Microsoft.Office.Interop.Excel;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace PluginTaskAutobook
{
    public partial class PanelTaskAutobookMonthValues : HPanelTepCommon
    {
        ///// <summary>
        ///// ???
        ///// </summary>
        //public static int vsRatio;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        bool m_bflgClear = true;
        //public event DelegateBoolFunc EvtChangeRow;
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;        
        /// <summary>
        /// Объект для производства расчетов
        /// </summary>
        protected TaskAutobookCalculate m_AutoBookCalculate;        
        /// <summary>
        /// Перечисление - столбцы отображения
        /// </summary>
        public enum INDEX_COLUMN : int
        {
            UNKNOW = -1
            , CorGTP12, CorGTP36
            , GTP12, GTP36
            , TEC            
                , COUNT
        }
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
        /// калькулятор значений
        /// </summary>
        public class TaskAutobookCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            public DataTable[] calcTable;
            /// <summary>
            /// выходные значения
            /// </summary>
            public List<string> value;

            /// <summary>
            /// Конструктор
            /// </summary>
            public TaskAutobookCalculate()
            {
                calcTable = new DataTable[(int)INDEX_COLUMN.COUNT];
                value = new List<string>((int)INDEX_COLUMN.COUNT);
            }

            /// <summary>
            /// Суммирование значений ТГ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            /// <returns>отредактированое значение</returns>
            private double sumTG(DataTable tb_gtp)
            {
                double value = 0;

                foreach (DataRow item in tb_gtp.Rows)
                    value += Convert.ToDouble(item[@"VALUE"].ToString());

                return value;
            }

            /// <summary>
            /// разбор данных по гтп
            /// </summary>
            /// <param name="dtOrigin">таблица с данными</param>
            /// /// <param name="dtOut">таблица с параметрами</param>
            public void getTable(DataTable[] dtOrigin, DataTable dtOut)
            {
                int i = 0
                , count = 0;

                calcTable[(int)INDEX_COLUMN.GTP12] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Clone();
                calcTable[(int)INDEX_COLUMN.TEC] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Clone();
                calcTable[(int)INDEX_COLUMN.GTP36] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Clone();
                //
                var m_enumDT = (from r in dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].AsEnumerable()
                    orderby r.Field<DateTime>("WR_DATETIME")
                    select new
                    {
                        WR_DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                    }).Distinct();

                for (int j = 0; j < m_enumDT.Count(); j++)
                {
                    i = 0;
                    calcTable[(int)INDEX_COLUMN.GTP12].Rows.Clear();
                    calcTable[(int)INDEX_COLUMN.GTP36].Rows.Clear();

                    DataRow[] drOrigin =
                        dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].
                        Select(string.Format(dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Locale
                        , "WR_DATETIME = '{0:o}'", m_enumDT.ElementAt(j).WR_DATE_TIME));

                    foreach (DataRow row in drOrigin)
                    {
                        if (i < 2)
                        {
                            calcTable[(int)INDEX_COLUMN.GTP12].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).WR_DATE_TIME
                            });
                        }
                        else
                            calcTable[(int)INDEX_COLUMN.GTP36].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).WR_DATE_TIME
                            });
                        i++;
                    }

                    calculate(calcTable);

                    for (int t = 0; t < value.Count(); t++)
                    {
                        calcTable[(int)INDEX_COLUMN.TEC].Rows.Add(new object[]
                        {
                            dtOut.Rows[t]["ID"]
                            ,dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Rows[j]["ID_SESSION"]
                            ,dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Rows[j]["QUALITY"]
                            ,value[t]
                            ,Convert.ToDateTime(String.Format(dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Locale
                            ,m_enumDT.ElementAt(j).WR_DATE_TIME.ToString()))
                            ,count
                        });
                        count++;
                    }
                }
            }

            /// <summary>
            /// Вычисление парамтеров ГТП и ТЭЦ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            private void calculate(DataTable[] tb_gtp)
            {
                double fTG12 = 0
                    , fTG36 = 0
                    , fTec = 0;

                if (value.Count() > 0)
                    value.Clear();

                for (int i = (int)INDEX_COLUMN.GTP12; i < (int)INDEX_COLUMN.CorGTP12; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_COLUMN.GTP12:
                            fTG12 = sumTG(tb_gtp[i]);
                            value.Add(fTG12.ToString());
                            break;
                        case (int)INDEX_COLUMN.GTP36:
                            fTG36 = sumTG(tb_gtp[i]);
                            value.Add(fTG36.ToString());
                            break;
                        case (int)INDEX_COLUMN.TEC:
                            fTec = fTG12 + fTG36;
                            value.Add(fTec.ToString());
                            break;
                        default:
                            break;
                    }
                }
            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Класс по работе с формированием 
        /// и отправкой отчета NSS
        /// </summary>
        public class ReportEMailNSS
        {
            /// <summary>
            /// Экземпляр класса
            /// </summary>
            OutlookMessage m_crtMsg;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            public ReportEMailNSS()
            {
                m_crtMsg = new OutlookMessage();
            }

            /// <summary>
            /// Класс создания письма
            /// </summary>
            private class OutlookMessage
            {
                /// <summary>
                /// 
                /// </summary>
                Outlook.Application m_outlookApp;

                /// <summary>
                /// конструктор(основной)
                /// </summary>
                public OutlookMessage()
                {
                    m_outlookApp = new Outlook.Application();
                }

                /// <summary>
                /// Формирование письма
                /// </summary>
                /// <param name="subject">тема письма</param>
                /// <param name="body">тело сообщения</param>
                /// <param name="to">кому/куда</param>
                public void FormingMessage(string subject, string body, string to)
                {
                    try
                    {
                        Outlook.MailItem newMail = (Outlook.MailItem)m_outlookApp.CreateItem(Outlook.OlItemType.olMailItem);
                        newMail.To = to;
                        newMail.Subject = subject;
                        newMail.Body = body;
                        newMail.Importance = Outlook.OlImportance.olImportanceNormal;
                        newMail.Display();
                        sendMail(newMail);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="mail"></param>
                private void sendMail(Outlook.MailItem mail)
                {
                    //отправка
                    mail.Send();
                }

                /// <summary>
                /// Прикрепление файла к письму
                /// </summary>
                /// <param name="mail"></param>
                private void AddAttachment(Outlook.MailItem mail)
                {
                    OpenFileDialog attachment = new OpenFileDialog();

                    attachment.Title = "Select a file to send";
                    attachment.ShowDialog();

                    if (attachment.FileName.Length > 0)
                    {
                        mail.Attachments.Add(
                            attachment.FileName,
                            Outlook.OlAttachmentType.olByValue,
                            1,
                            attachment.FileName);
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                private void closeOutlook()
                {
                    m_outlookApp.Quit();
                    Marshal.ReleaseComObject(m_outlookApp);
                    GC.Collect();
                }
            }

            /// <summary>
            /// Содание тела сообщения
            /// </summary>
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtSend">дата</param>
            private void createBodyToSend(ref string sbjct
                , ref string bodyMsg
                , DataTable sourceTable
                , DateTime dtSend)
            {
                DataRow[] drReportDay;
                DateTime reportDate;

                reportDate = dtSend.AddHours(6).Date;//??
                drReportDay =
                    sourceTable.Select(string.Format(sourceTable.Locale, @"WR_DATETIME = '{0:o}'", reportDate));

                if ((double)drReportDay.Length != 0)
                {
                    bodyMsg = @"BEGIN " + "\r\n"
                        + @"(DATE):" + reportDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "\r\n"
                        + @"(01): "
                            +
                                //Convert.ToString(getValueAsRatio(idAlg, double.Parse(drReportDay[(int)INDEX_COLUMN.TEC]["VALUE"].ToString())))
                                @"??? Не работает"
                            + ":\r\n"
                        + @"(02): "
                            +
                                //Convert.ToString(getValueAsRatio(idAlg, double.Parse(drReportDay[(int)INDEX_COLUMN.GTP12]["VALUE"].ToString())))
                                @"??? Не работает"
                            + ":\r\n"
                        + @"(03): "
                            +
                                //Convert.ToString(getValueAsRatio(idAlg, double.Parse(drReportDay[(int)INDEX_COLUMN.GTP36]["VALUE"].ToString())))
                                @"??? Не работает"
                            + ":\r\n"
                        + @"END ";
                    /*bodyMsg = @"Дата " + reportDate.ToShortDateString() + ".\r\n"
                        + @"Станция, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ";\r\n"
                        + @"Блоки 1-2, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ";\r\n"
                        + @"Блоки 3-6, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]);*/

                    sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
                }
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
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtSend">выбранный промежуток</param>
            /// <param name="to">получатель</param>
            public void SendMailToNSS(DataTable sourceTable, DateTime dtSend, string to)
            {
                string bodyMsg = string.Empty
                 , sbjct = string.Empty;

                createBodyToSend(ref sbjct, ref bodyMsg, sourceTable, dtSend);

                if (sbjct != "")
                    m_crtMsg.FormingMessage(sbjct, bodyMsg, to);
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportMSExcel
        {
            /// <summary>
            /// Экземпляр приложения Excel
            /// </summary>
            private Excel.Application m_excApp;
            /// <summary>
            /// Экземпляр книги Excel
            /// </summary>
            private Excel.Workbook m_workBook;
            /// <summary>
            /// Экземпляр листа Excel
            /// </summary>
            private Excel.Worksheet m_wrkSheet;
            private object _missingObj = Missing.Value;

            /// <summary>
            /// Перечисление - индексы для указания признаков ячейки
            /// </summary>
            protected enum MODE_CELL_BORDER : int
            {
                UNKNOW = -1,
                SEPARATE,
                ADJACENT
            }

            /// <summary>
            /// конструктор(основной)
            /// </summary>
            public ReportMSExcel()
            {
                m_excApp = new Excel.Application();
                m_excApp.Visible = false;
            }

            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgv">отрбражение данных</param>
            /// <param name="dtRange">дата</param>
            public void Create(DataGridView dgv, DateTimeRange dtRange)
            {
                if (addWorkBooks())
                {
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Autobook");
                    int indxRow = 1;

                    try {
                        for (int i = 0; i < dgv.Columns.Count; i++) {
                            if (dgv.Columns[i].HeaderText != "") {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxRow];

                                foreach (Excel.Range cell in colRange.Cells)
                                    if (Convert.ToString(cell.Value) == splitString(dgv.Columns[i].HeaderText)) {
                                        fillSheetExcel(colRange, dgv, i, cell.Row);

                                        break;
                                    } else
                                        ;
                                indxRow++;
                            } else
                                ;
                        }
                        setPlanMonth(m_wrkSheet, dgv, dtRange);
                        m_excApp.Visible = true;
                        Marshal.ReleaseComObject(m_excApp);
                    } catch (Exception e) {
                        Close();
                        MessageBox.Show("Ошибка экспорта данных!" + e);
                    }
                }
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                //string pathToTemplate = @"D:\MyProjects\C.Net\TEP32\Tep\bin\Debug\Template\TemplateAutobook.xlsx";
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateAutobook.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
                    Close();
                    bflag = false;
                    MessageBox.Show("Отсутствует шаблон для отчета Excel" + exp);
                }
                return bflag;
            }

            /// <summary>
            /// Обработка события - закрытие экселя
            /// </summary>
            /// <param name="Cancel"></param>
            void workBook_BeforeClose(ref bool Cancel)
            {
                Close();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                Close();
            }

            /// <summary>
            /// Добавление плана и месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setPlanMonth(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                Excel.Range exclRPL = exclWrksht.get_Range("C5");
                Excel.Range exclRMonth = exclWrksht.get_Range("A4");
                exclRPL.Value2 = dgv.Rows[dgv.Rows.Count - 1].Cells[@"PlanSwen"].Value;
                exclRMonth.Value2 = HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
            }

            /// <summary>
            /// Деление 
            /// </summary>
            /// <param name="headerTxt">строка</param>
            /// <returns>часть строки</returns>
            private string splitString(string headerTxt)
            {
                string[] spltHeader = headerTxt.Split(',');

                if (spltHeader.Length > (int)MODE_CELL_BORDER.ADJACENT)
                    return spltHeader[(int)MODE_CELL_BORDER.ADJACENT].TrimStart();
                else
                    return spltHeader[(int)MODE_CELL_BORDER.SEPARATE];
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне
            /// </summary>
            /// <param name="cellRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range cellRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < cellRange.Rows.Count; i++)
                    if (((Excel.Range)cellRange.Cells[i]).Value == null &&
                        ((Excel.Range)cellRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        row = i;
                        break;
                    }

                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    cellRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                    row++;
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            public void Close()
            {
                //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                Marshal.ReleaseComObject(m_excApp);

                m_excApp = null;
                m_workBook = null;
                m_wrkSheet = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookMonthValues(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES | HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            HandlerDb.ModeAgregateGetValues = TepCommon.HandlerDbTaskCalculate.MODE_AGREGATE_GETVALUES.OFF;
            HandlerDb.ModeDataDateTime = TepCommon.HandlerDbTaskCalculate.MODE_DATA_DATETIME.Begined;
            m_AutoBookCalculate = new TaskAutobookCalculate();

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();

            //Session.SetDatetimeRange(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <returns>кол-во дней</returns>
        protected int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_DatetimeRange.Begin.Year, Session.m_DatetimeRange.Begin.Month);
            }
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementAutobookMonthValues : PanelManagementTaskCalculate //HPanelCommon
        {
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL, CALENDAR,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CHKBX_EDIT,
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
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementAutobookMonthValues()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels) //4, 3
            {
                InitializeComponents();
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
                ctrl = new DropDownButton();
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
                ctrl.Name = INDEX_CONTROL.CALENDAR.ToString();
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
                ctrl.Name = INDEX_CONTROL.BUTTON_SEND.ToString();
                ctrl.Text = @"Отправить";
                //ctrlBSend.Enabled = false;
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
            m_dgvValues = new DataGridViewAutobookMonthValues(INDEX_CONTROL.DGV_VALUES.ToString());            
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
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(btnSend_onClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(btnExport_onClick);
            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CALENDAR.ToString(), true)[0] as Button).

            m_dgvValues.CellParsing += dgvValues_CellParsing;
            //m_dgvAB.SelectionChanged += dgvAB_SelectionChanged;
        }

        /// <summary>
        /// обработка ЭКСПОРТА(временно)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnExport_onClick(object sender, EventArgs e)
        {
            ReportMSExcel rep = new ReportMSExcel();
            rep.Create(m_dgvValues, Session.m_DatetimeRange);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnSend_onClick(object sender, EventArgs e)
        {
            ReportEMailNSS rep = new ReportEMailNSS();

            string toSend = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL.ToString(), true)[0] as TextBox).Text;
            DateTime dtSend = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CALENDAR.ToString(), true)[0] as DateTimePicker).Value;
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = m_dgvValues.FillTableValueDay();
            //
            rep.SendMailToNSS(m_TableEdit, dtSend, toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void dgvValues_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            int numMonth = PanelManagement.DatetimeRange.Begin.Month
                , day = m_dgvValues.Rows.Count;

            switch ((INDEX_COLUMN)Enum.Parse(typeof(INDEX_COLUMN), m_dgvValues.Columns[e.ColumnIndex].Name)) {
                case INDEX_COLUMN.CorGTP12:
                    //корректировка значений
                    m_dgvValues.editCells(e, INDEX_COLUMN.GTP12.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = m_dgvValues.FillTableCorValue((int)Session.m_curOffsetUTC.TotalMinutes, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = m_dgvValues.FillTableValueDay();
                    break;
                case INDEX_COLUMN.CorGTP36:
                    //корректировка значений
                    m_dgvValues.editCells(e, INDEX_COLUMN.GTP36.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = m_dgvValues.FillTableCorValue((int)Session.m_curOffsetUTC.TotalMinutes, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = m_dgvValues.FillTableValueDay();
                    break;
                default:
                    break;
            }
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
        //    //Запрос для получения архивных данных
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutval(arQueryRanges, out err);
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
        //        (
        //        TaskCalculateType
        //        , Session.ActualIdPeriod
        //        , Session.CountBasePeriod
        //        , arQueryRanges
        //       , out err
        //        );
        //    //Получение значений корр. input
        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //        HandlerDb.GetCorInPut(TaskCalculateType
        //        , arQueryRanges
        //        , Session.ActualIdPeriod
        //        , out err);
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
        //                , ref m_arTableOrigin
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
        //    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //        m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Clone();
        //    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
        //        = m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
        //    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
        //      = m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE].Clone();
        //}

        ///// <summary>
        ///// Проверка выбранного диапазона
        ///// </summary>
        ///// <param name="dtRange">диапазон дат</param>
        ///// <returns></returns>
        //private bool rangeCheking(DateTimeRange[] dtRange)
        //{
        //    bool bflag = false;

        //    for (int i = 0; i < dtRange.Length; i++)
        //        if (dtRange[i].Begin.Month > DateTime.Now.Month)
        //            if (dtRange[i].End.Year >= DateTime.Now.Year)
        //                bflag = true;

        //    return bflag;
        //}

        ///// <summary>
        ///// Загрузка сырых значений
        ///// </summary>
        ///// <param name="typeValues">тип загружаемых значений</param>
        //private void updateDataValues(HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
        //{
        //    int err = -1
        //        , cnt = Session.CountBasePeriod
        //        , iRegDbConn = -1;
        //    string errMsg = string.Empty;
        //    DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

        //    //if (rangeCheking(dtrGet))
        //    //    MessageBox.Show("Выбранный диапазон месяцев неверен");
        //    //else
        //    //{
        //    clear();
        //    m_handlerDb.RegisterDbConnection(out iRegDbConn);

        //    if (!(iRegDbConn < 0))
        //    {
        //        // установить значения в таблицах для расчета, создать новую сессию
        //        setValues(dtrGet, out err, out errMsg);

        //        if (err == 0)
        //        {
        //            if (m_TableOrigin.Rows.Count > 0)
        //            {
        //                // создать копии для возможности сохранения изменений
        //                setValues();
        //                //вычисление значений
        //                m_AutoBookCalculate.getTable(m_arTableOrigin, HandlerDb.GetOutPut(out err));
        //                //
        //                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
        //                    m_AutoBookCalculate.calcTable[(int)INDEX_COLUMN.TEC].Copy();
        //                //запись выходных значений во временную таблицу
        //                HandlerDb.insertOutValues(out err, m_AutoBookCalculate.calcTable[(int)INDEX_COLUMN.TEC]);
        //                // отобразить значения
        //                m_dgvValues.ShowValues(m_arTableOrigin
        //                    , HandlerDb.GetPlanOnMonth(TaskCalculateType
        //                    , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
        //                    , Session.ActualIdPeriod
        //                    , out err)
        //                    , typeValues);
        //                //формирование таблиц на основе грида
        //                valuesFence();
        //            }
        //            else
        //                deleteSession();
        //        }
        //        else
        //        {
        //            // в случае ошибки "обнулить" идентификатор сессии
        //            deleteSession();
        //            throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
        //        }
        //    }
        //    else
        //        deleteSession();

        //    if (!(iRegDbConn > 0))
        //        m_handlerDb.UnRegisterDbConnection();
        //    //}
        //}

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(HandlerDbTaskCalculate.RESULT res)
        {
            //вычисление значений, сохранение во временной таблице
            HandlerDb.Calculate(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
        }

        protected override void handlerDbTaskCalculate_onCalculateCompleted(HandlerDbTaskCalculate.RESULT res)
        {
            int err = -1;

            // отобразить значения
            m_dgvValues.ShowValues(m_arTableOrigin
                , HandlerDb.GetPlanOnMonth(TaskCalculateType
                    , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
                    , Session.ActualIdPeriod
                    , out err)
                , Session.m_ViewValues);
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(object obj)
        {
            throw new NotImplementedException();
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
                HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE);
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
                // ... - загрузить/отобразить значения из БД
                HandlerDb.UpdateDataValues(m_Id, TaskCalculateType, HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"PanelTaskAutobookMonthValues::panelTepCommon_btnUpdate_onClick () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]; }
        }
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD]; }
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
                    HandlerDb.InitSession(out err);

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

            HandlerDb.FilterDbTableTimezone = HandlerDbTaskCalculate.DbTableTimezone.Msk;
            HandlerDb.FilterDbTableTime = HandlerDbTaskCalculate.DbTableTime.Month;
            HandlerDb.FilterDbTableCompList = HandlerDbTaskCalculate.DbTableCompList.Tec
                | HandlerDbTaskCalculate.DbTableCompList.Gtp
                | HandlerDbTaskCalculate.DbTableCompList.Tg;

            // возможность_редактирвоания_значений
            try {
                if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.MONTH, INDEX_CONTROL.DGV_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == true)
                    if ((MODE_CORRECT)Enum.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(ID_PERIOD.MONTH, INDEX_CONTROL.DGV_VALUES, HTepUsers.ID_ALLOWED.ENABLED_ITEM)) == MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                    else
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                else
                    (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
            } catch (Exception e) {
            }
            // активность_кнопки_сохранения
            try {
                if (Enum.IsDefined(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == true)
                    (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled =
                        (MODE_CORRECT)Enum.Parse(typeof(MODE_CORRECT), m_dictProfile.GetAttribute(HTepUsers.ID_ALLOWED.ENABLED_CONTROL)) == MODE_CORRECT.ENABLE;
                else
                    (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception exp) {
                Logging.Logg().Exception(exp, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            try {
                m_dgvValues.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);

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
                Start = PanelManagement.DatetimeRange.Begin + (TimeSpan.FromDays(1) - Session.m_curOffsetUTC)
                , Increment = TimeSpan.FromDays(1)
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
        protected override void handlerDbTaskCalculate_onAddNAlgParameter(HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddNAlgParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void handlerDbTaskCalculate_onAddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.handlerDbTaskCalculate_onAddPutParameter(obj);
        }
        /// <summary>
        /// Обработчик события - добавить компонент станции (оборудовнаие)
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void handlerDbTaskCalculate_onAddComponent(HandlerDbTaskCalculate.TECComponent obj)
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
            ////???сбор значений
            //valuesFence();

            //m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] = getStructurOutval(
            //    getNameTableOut(PanelManagement.DatetimeRange.Begin), out err);

            //m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
            //    HandlerDb.SaveResOut(m_TableOrigin, m_TableEdit, out err);

            //if (m_TableEdit.Rows.Count > 0)
                ////???save вх. значений
                //saveInvalValue(out err);
            //else
            //    ;
        }

        /// <summary>
        /// ??? получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(string nameTable, out int err)
        {
            return HandlerDb.Select("SELECT * FROM " + nameTable, out err);
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        private string getNameTableOut(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = HandlerDbValues.s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// ??? Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        private string getNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");

            strRes = HandlerDbValues.s_dictDbTables[ID_DBTABLE.INVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;
            //
            sortingDataToTable(HandlerDb.GetDataOutval(HandlerDb.getDateTimeRangeExtremeVal(), out err)
                , m_TableEdit
                , getNameTableOut(PanelManagement.DatetimeRange.Begin)
                , @""
                , out err);
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="m_origin">оригинальная таблица</param>
        /// <param name="m_edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable m_origin, DataTable m_edit, string unCol, out int err)
        {
            err = -1;

            __handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME"
                    , unCol
                    , m_origin
                    , m_edit
                    , out err);
        }

        /// <summary>
        /// разбор данных по разным табилца(взависимости от месяца)
        /// </summary>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="unCol">столбец, неучаствующий в InsertUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void sortingDataToTable(DataTable origin
            , DataTable edit
            , string nameTable
            , string unCol
            , out int err)
        {
            string nameTableExtrmRow = string.Empty
                          , nameTableNew = string.Empty;
            DataTable editTemporary = new DataTable()
                , originTemporary = new DataTable();

            err = -1;
            editTemporary = edit.Clone();
            originTemporary = origin.Clone();
            nameTableNew = nameTable;

            foreach (DataRow row in edit.Rows)
            {
                nameTableExtrmRow = extremeRow(row["DATE_TIME"].ToString(), nameTableNew);

                if (nameTableExtrmRow != nameTableNew)
                {
                    foreach (DataRow rowOrigin in origin.Rows)
                        if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month != Convert.ToDateTime(row["DATE_TIME"]).Month)
                            originTemporary.Rows.Add(rowOrigin.ItemArray);

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);//сохранение данных

                    nameTableNew = nameTableExtrmRow;
                    editTemporary.Rows.Clear();
                    originTemporary.Rows.Clear();
                    editTemporary.Rows.Add(row.ItemArray);
                }
                else
                    editTemporary.Rows.Add(row.ItemArray);
            }

            if (editTemporary.Rows.Count > 0)
            {
                foreach (DataRow rowOrigin in origin.Rows)
                    if (extremeRow(Convert.ToDateTime(rowOrigin["DATE_TIME"]).ToString(), nameTableNew) == nameTableNew)
                        originTemporary.Rows.Add(rowOrigin.ItemArray);

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);//сохранение данных
            }
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
            string m_nametable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + m_nametable;
        }

        ///// <summary>
        ///// Сохранение входных знчений(корр. величины)
        ///// </summary>
        ///// <param name="err">Признак выполнения операций внутри метода</param>
        //private void saveInvalValue(out int err)
        //{
        //    DateTimeRange[] dtrPer = HandlerDb.getDateTimeRangeVariableValues();

        //    m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //        HandlerDb.GetInPutID(TaskCalculateType, dtrPer, Session.ActualIdPeriod, out err);

        //    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
        //    HandlerDb.SaveResInval(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
        //        , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
        //        , out err);

        //    sortingDataToTable(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
        //        , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
        //        , getNameTableIn(dtrPer[0].Begin)
        //        , @"ID"
        //        , out err);
        //}

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Copy();
        }
    }
}

