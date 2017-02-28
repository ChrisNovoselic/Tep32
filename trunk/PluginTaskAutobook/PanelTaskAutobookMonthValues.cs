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
        /// <summary>
        /// ???
        /// </summary>
        public static int vsRatio;
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
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Перечисление - столбцы отображения
        /// </summary>
        public enum INDEX_GTP : int
        {
            UNKNOW = -1,
            GTP12, GTP36,
            TEC,
            CorGTP12, CorGTP36,
            COUNT
        }
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL { UNKNOWN = -1, LABEL_DESC = 1, DGV_DATA = 3 }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD, // идентификаторы периодов расчетов, использующихся на форме               
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов                   
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //    , DENY_COMP_CALCULATED,//DENY_PARAMETER_CALCULATED // запрещенных для расчета
            DENY_COMP_VISIBLED, //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Объект для взаимодействия с БД
        /// </summary>
        protected HandlerDbTaskAutobookMonthValuesCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskAutobookMonthValuesCalculate; } }
        /// <summary>
        /// Часовой пояс(часовой сдвиг)
        /// </summary>
        protected static int m_currentOffSet;
        ///// <summary>
        ///// 
        ///// </summary>
        //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        ///// <summary>
        ///// Таблицы со значениями словарных, проектных данных
        ///// </summary>
        //protected DataTable[] m_dictTableDictPrj;
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
        /// to Outlook
        /// </summary>
        protected ReportsToNSS m_rptsNSS;
        /// <summary>
        /// to Excel
        /// </summary>
        protected ReportExcel m_rptExcel;
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
        /// 
        /// </summary>
        /// <returns>экземпляр класса</returns>
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
                calcTable = new DataTable[(int)INDEX_GTP.COUNT];
                value = new List<string>((int)INDEX_GTP.COUNT);
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

                calcTable[(int)INDEX_GTP.GTP12] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
                calcTable[(int)INDEX_GTP.TEC] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
                calcTable[(int)INDEX_GTP.GTP36] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
                //
                var m_enumDT = (from r in dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].AsEnumerable()
                                orderby r.Field<DateTime>("WR_DATETIME")
                                select new
                                {
                                    DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                }).Distinct();

                for (int j = 0; j < m_enumDT.Count(); j++)
                {
                    i = 0;
                    calcTable[(int)INDEX_GTP.GTP12].Rows.Clear();
                    calcTable[(int)INDEX_GTP.GTP36].Rows.Clear();

                    DataRow[] drOrigin =
                        dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].
                        Select(string.Format(dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Locale
                        , "WR_DATETIME = '{0:o}'", m_enumDT.ElementAt(j).DATE_TIME));

                    foreach (DataRow row in drOrigin)
                    {
                        if (i < 2)
                        {
                            calcTable[(int)INDEX_GTP.GTP12].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        }
                        else
                            calcTable[(int)INDEX_GTP.GTP36].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        i++;
                    }

                    calculate(calcTable);

                    for (int t = 0; t < value.Count(); t++)
                    {
                        calcTable[(int)INDEX_GTP.TEC].Rows.Add(new object[]
                        {
                            dtOut.Rows[t]["ID"]
                            ,dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows[j]["ID_SESSION"]
                            ,dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows[j]["QUALITY"]
                            ,value[t]
                            ,Convert.ToDateTime(String.Format(dtOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Locale
                            ,m_enumDT.ElementAt(j).DATE_TIME.ToString()))
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

                for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_GTP.GTP12:
                            fTG12 = sumTG(tb_gtp[i]);
                            value.Add(fTG12.ToString());
                            break;
                        case (int)INDEX_GTP.GTP36:
                            fTG36 = sumTG(tb_gtp[i]);
                            value.Add(fTG36.ToString());
                            break;
                        case (int)INDEX_GTP.TEC:
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
        public class ReportsToNSS
        {
            /// <summary>
            /// Экземпляр класса
            /// </summary>
            CreateMessage m_crtMsg;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            public ReportsToNSS()
            {
                m_crtMsg = new CreateMessage();
            }

            /// <summary>
            /// Класс создания письма
            /// </summary>
            private class CreateMessage
            {
                /// <summary>
                /// 
                /// </summary>
                Outlook.Application m_oApp;

                /// <summary>
                /// конструктор(основной)
                /// </summary>
                public CreateMessage()
                {
                    m_oApp = new Outlook.Application();
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
                        Outlook.MailItem newMail = (Outlook.MailItem)m_oApp.CreateItem(Outlook.OlItemType.olMailItem);
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
                    m_oApp.Quit();
                    Marshal.ReleaseComObject(m_oApp);
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
                        + @"(01): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.TEC]["VALUE"].ToString())) + ":\r\n"
                        + @"(02): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.GTP12]["VALUE"].ToString())) + ":\r\n"
                        + @"(03): " + fewerValue(double.Parse(drReportDay[(int)INDEX_GTP.GTP36]["VALUE"].ToString())) + ":\r\n"
                        + @"END ";
                    /*bodyMsg = @"Дата " + reportDate.ToShortDateString() + ".\r\n"
                        + @"Станция, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ";\r\n"
                        + @"Блоки 1-2, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ";\r\n"
                        + @"Блоки 3-6, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]);*/

                    sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
                }
            }

            /// <summary>
            /// Редактирование значения
            /// </summary>
            /// <param name="val">значение</param>
            /// <returns>измененное знач.</returns>
            private string fewerValue(double val)
            {
                return Convert.ToString(val * Math.Pow(10F, 1 * vsRatio));
            }

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
        public class ReportExcel
        {
            /// <summary>
            /// Экземпляп приложения Excel
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
            /// 
            /// </summary>
            protected enum INDEX_DIVISION : int
            {
                UNKNOW = -1,
                SEPARATE_CELL,
                ADJACENT_CELL
            }

            /// <summary>
            /// конструктор(основной)
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
                if (addWorkBooks())
                {
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Autobook");
                    int indxRow = 1;

                    try
                    {
                        for (int i = 0; i < dgView.Columns.Count; i++)
                        {
                            if (dgView.Columns[i].HeaderText != "")
                            {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxRow];

                                foreach (Excel.Range cell in colRange.Cells)
                                    if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                                    {
                                        fillSheetExcel(colRange, dgView, i, cell.Row);
                                        break;
                                    }
                                indxRow++;
                            }
                        }
                        setPlanMonth(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        Marshal.ReleaseComObject(m_excApp);
                    }
                    catch (Exception e)
                    {
                        CloseExcel();
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
                    CloseExcel();
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
                CloseExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                CloseExcel();
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

                if (spltHeader.Length > (int)INDEX_DIVISION.ADJACENT_CELL)
                    return spltHeader[(int)INDEX_DIVISION.ADJACENT_CELL].TrimStart();
                else
                    return spltHeader[(int)INDEX_DIVISION.SEPARATE_CELL];
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
            public void CloseExcel()
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
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
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
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// преобразование числа в нужный формат отображения
        /// </summary>
        /// <param name="value">число</param>
        /// <returns>преобразованное число</returns>
        public static float AsParseToF(string value)
        {
            int _indxChar = 0;
            string _sepReplace = string.Empty;
            bool bFlag = true;
            //char[] _separators = { ' ', ',', '.', ':', '\t'};
            //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();
            float fValue = 0;

            foreach (char item in value.ToCharArray())
            {
                if (!char.IsDigit(item))
                    if (char.IsLetter(item))
                        value = value.Remove(_indxChar, 1);
                    else
                        _sepReplace = value.Substring(_indxChar, 1);
                else
                    _indxChar++;

                switch (_sepReplace)
                {
                    case ".":
                    case ",":
                    case " ":
                    case ":":
                        float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fValue);
                        bFlag = false;
                        break;
                }
            }

            if (bFlag)
                try
                {
                    fValue = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    if (value.ToString() == "")
                        fValue = 0;
                }


            return fValue;
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
            /// 
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
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;

                DateTimeRangeValue_Changed?.Invoke(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
            }
        }

        /// <summary>
        /// инициализация объектов
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
            m_dgvValues = new DataGridViewAutobookMonthValues(INDEX_CONTROL.DGV_DATA.ToString());
            m_dgvValues.Dock = DockStyle.Fill;
            m_dgvValues.Name = INDEX_CONTROL.DGV_DATA.ToString();
            m_dgvValues.AllowUserToResizeRows = false;
            m_dgvValues.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dgvValues.AddColumn("Корректировка ПТО,\r\nБлоки 1-2", true, INDEX_GTP.CorGTP12.ToString());
            m_dgvValues.AddColumn("Корректировка ПТО,\r\nБлоки 3-6", true, INDEX_GTP.CorGTP36.ToString());
            m_dgvValues.AddColumn("Блоки 1-2", true, INDEX_GTP.GTP12.ToString());
            m_dgvValues.AddColumn("Блоки 3-6", true, INDEX_GTP.GTP36.ToString());
            m_dgvValues.AddColumn("Станция,\r\nсутки", true, INDEX_GTP.TEC.ToString());
            m_dgvValues.AddColumn("Станция,\r\nнараст.", true, "StSwen");
            m_dgvValues.AddColumn("План нараст.", true, "PlanSwen");
            m_dgvValues.AddColumn("Отклонение от плана", true, "DevOfPlan");
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
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            ((ctrl as Button).ContextMenuStrip.Items.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            ((ctrl as Button).ContextMenuStrip.Items.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(btnHistory_OnClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(btnSend_OnClick);
            (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(btnExport_OnClick);
            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL.CALENDAR.ToString(), true)[0] as Button).

            m_dgvValues.CellParsing += dgvAB_CellParsing;
            //m_dgvAB.SelectionChanged += dgvAB_SelectionChanged;
        }

        ///// <summary>
        ///// обработка события - Выбор строки
        ///// </summary>
        ///// <param name="sender">Объект, инициировавший событие</param>
        ///// <param name="e">Аргумент события</param>
        //void dgvAB_SelectionChanged(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < (sender as DataGridView).SelectedRows.Count; i++)
        //        if ((sender as DataGridView).SelectedRows[i].Cells["Date"].Value != null)
        //        {
        //            DateTime dtRow = Convert.ToDateTime((sender as DataGridView).SelectedRows[i].Cells["Date"].Value);
        //            HDateTimePicker datetimePicker =
        //                (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
        //            m_bflgClear = false;
        //            datetimePicker.Value = dtRow;
        //        }
        //}

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
        }

        /// <summary>
        /// обработка ЭКСПОРТА(временно)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnExport_OnClick(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();
            m_rptExcel.CreateExcel(m_dgvValues, Session.m_rangeDatetime);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        private void btnSend_OnClick(object sender, EventArgs e)
        {
            m_rptsNSS = new ReportsToNSS();

            string toSend = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL.ToString(), true)[0] as TextBox).Text;
            DateTime dtSend = (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CALENDAR.ToString(), true)[0] as DateTimePicker).Value;
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = m_dgvValues.FillTableValueDay();
            //
            m_rptsNSS.SendMailToNSS(m_TableEdit, dtSend, toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">данные события</param>
        void dgvAB_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            int numMonth = PanelManagement.DatetimeRange.Begin.Month
                , day = m_dgvValues.Rows.Count;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            switch (m_dgvValues.Columns[e.ColumnIndex].Name)
            {
                case "CorGTP12":
                    //корректировка значений
                    m_dgvValues.editCells(e, INDEX_GTP.GTP12.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = m_dgvValues.FillTableCorValue(Session.m_curOffsetUTC, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = m_dgvValues.FillTableValueDay();
                    break;
                case "CorGTP36":
                    //корректировка значений
                    m_dgvValues.editCells(e, INDEX_GTP.GTP36.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = m_dgvValues.FillTableCorValue(Session.m_curOffsetUTC, e);
                    //сбор значений
                    m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = m_dgvValues.FillTableValueDay();
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
            deleteSession();

            base.Stop();
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr"></param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutval(arQueryRanges, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
                (
                TaskCalculateType
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
               , out err
                );
            //Получение значений корр. input
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                HandlerDb.GetCorInPut(TaskCalculateType
                , arQueryRanges
                , Session.ActualIdPeriod
                , out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , Session.CountBasePeriod
                        , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
                        , ref m_arTableOrigin
                        , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                        , out err, out strErr);
                else
                    strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
                        + @" по " + Session.m_rangeDatetime.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
                    + @" по " + Session.m_rangeDatetime.End.ToString();
        }

        /// <summary>
        /// copy
        /// </summary>
        private void setValues()
        {
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Clone();
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                = m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Clone();
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
              = m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE].Clone();
        }

        /// <summary>
        /// Проверка выбранного диапазона
        /// </summary>
        /// <param name="dtRange">диапазон дат</param>
        /// <returns></returns>
        private bool rangeCheking(DateTimeRange[] dtRange)
        {
            bool bflag = false;

            for (int i = 0; i < dtRange.Length; i++)
                if (dtRange[i].Begin.Month > DateTime.Now.Month)
                    if (dtRange[i].End.Year >= DateTime.Now.Year)
                        bflag = true;

            return bflag;
        }

        /// <summary>
        /// Загрузка сырых значений
        /// </summary>
        /// <param name="typeValues">тип загружаемых значений</param>
        private void updateDataValues(HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
        {
            int err = -1
                , cnt = Session.CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            //if (rangeCheking(dtrGet))
            //    MessageBox.Show("Выбранный диапазон месяцев неверен");
            //else
            //{
            clear();
            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (m_TableOrigin.Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();
                        //вычисление значений
                        m_AutoBookCalculate.getTable(m_arTableOrigin, HandlerDb.GetOutPut(out err));
                        //
                        m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                            m_AutoBookCalculate.calcTable[(int)INDEX_GTP.TEC].Copy();
                        //запись выходных значений во временную таблицу
                        HandlerDb.insertOutValues(out err, m_AutoBookCalculate.calcTable[(int)INDEX_GTP.TEC]);
                        // отобразить значения
                        m_dgvValues.ShowValues(m_arTableOrigin
                            , HandlerDb.GetPlanOnMonth(TaskCalculateType
                            , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
                            , Session.ActualIdPeriod
                            , out err)
                            , typeValues);
                        //формирование таблиц на основе грида
                        valuesFence();
                    }
                    else
                        deleteSession();
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                }
            }
            else
                deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
            //}
        }

        /// <summary>
        /// Загрузка архивных значений
        /// </summary>
        /// <param name="typeValues">тип загружаемых значений</param>
        private void loadArchValues(HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
        {
            int err = -1,
                cnt = Session.CountBasePeriod,
                iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            clear();
            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (true)
                    {
                        //запись выходных значений во временную таблицу
                        //HandlerDb.insertOutValues(out err, m_arTableOrigin[(int)typeValues]);
                        // отобразить значения
                        m_dgvValues.ShowValues(m_arTableOrigin
                            , HandlerDb.GetPlanOnMonth(TaskCalculateType
                            , HandlerDb.GetDateTimeRangeValuesVarPlanMonth()
                            , Session.ActualIdPeriod
                            , out err)
                            , typeValues);
                        //формирование таблиц на основе грида
                        valuesFence();
                    }
                    else;
                }
                else
                {
                    deleteSession();
                    throw new Exception(@"PanelTaskAutobookMonthValues::updatedataValues() - " + errMsg);
                }
            }
            else deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
        }

        /// <summary>
        /// формирование таблиц данных
        /// </summary>
        private void valuesFence()
        {
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
            //сохранить вых. знач. в DataTable
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                m_dgvValues.FillTableValueDay();
            //сохранить вх.корр. знач. в DataTable
            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                m_dgvValues.FillTableCorValue(Session.m_curOffsetUTC);
        }

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnHistory_OnClick(object obj, EventArgs ev)
        {
            try
            {
                Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;
                onButtonLoadClick();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            switch (Session.m_ViewValues)
            {
                case HandlerDbTaskCalculate.ID_VIEW_VALUES.UNKNOWN:
                    break;
                case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE:
                    updateDataValues(Session.m_ViewValues);
                    break;
                case HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE:
                    loadArchValues(Session.m_ViewValues);
                    break;
                case HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            try
            {
                Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;
                onButtonLoadClick();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
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
            int i = -1
                , id_comp = -1
                , tCount = 0;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            INDEX_ID[] arIndxIdToAdd =
                new INDEX_ID[]
                {
                        //INDEX_ID.DENY_COMP_CALCULATED,
                        INDEX_ID.DENY_COMP_VISIBLED
                };

            //m_dictTableDictPrj = new DataTable[(int)ID_DBTABLE.COUNT];
            int role = HTepUsers.Role;

            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id) {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { /*(int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY,*/ (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    case INDEX_ID.ALL_COMPONENT:
                        m_arListIds[(int)id] = new List<int> { };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMEZONE, COMP, MEASURE, RATIO
            initialize(new ID_DBTABLE[] { /*ID_DBTABLE.PERIOD, */ID_DBTABLE.TIMEZONE, ID_DBTABLE.COMP_LIST, ID_DBTABLE.MEASURE, ID_DBTABLE.RATIO }
                , out err, out errMsg);

            m_dictTableDictPrj.SetDbTableFilter(ID_DBTABLE.TIMEZONE, new int[] { (int)ID_TIMEZONE.MSK });
            m_dictTableDictPrj.SetDbTableFilter(ID_DBTABLE.TIME, new int[] { });

            bool[] arChecked = new bool[arIndxIdToAdd.Length];
            Array namePut = Enum.GetValues(typeof(INDEX_GTP));

            foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP].Rows) {
                id_comp = (int)r[@"ID"];
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(id_comp);

                m_dgvValues.AddIdComp(id_comp, namePut.GetValue(tCount).ToString());
                tCount++;
            }
            //возможность_редактирвоания_значений
            try {
                if (m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)INDEX_CONTROL.DGV_DATA).ToString()).Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                    if (int.Parse(m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)INDEX_CONTROL.DGV_DATA).ToString()).Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                    else
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                else
                    (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
            } catch (Exception e) {
            }
            //активность_кнопки_сохранения
            try {
                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                else
                    (Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception exp) {
                Logging.Logg().Exception(exp, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                MessageBox.Show(exp.ToString());
            }

            try {
                m_dgvValues.SetRatio(m_dictTableDictPrj[ID_DBTABLE.RATIO]);

                if (err == 0)
                {
                    //Заполнить элемент управления с часовыми поясами
                    idProfileTimezone = (ID_TIMEZONE)Enum.Parse(typeof(ID_TIMEZONE), m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]);
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , idProfileTimezone);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)Enum.Parse(typeof(ID_PERIOD), m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]);
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , idProfilePeriod);
                    Session.SetCurrentPeriod(PanelManagement.IdPeriod);
                    PanelManagement.SetModeDatetimeRange();

                    ctrl = Controls.Find(PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL.ToString(), true)[0];
                    //из profiles
                    key = ((int)PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL).ToString();
                    if (m_dictProfile.Keys.Contains(key) == true)
                        ctrl.Text = m_dictProfile.GetObjects(key).Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.MAIL).ToString()];
                    else
                        Logging.Logg().Warning(string.Format(@"PanelTaskAutoBook::initialize () - в словаре 'm_dictProfile' не определен ключ [{0:1}]..."
                            , key, PanelManagementAutobookMonthValues.INDEX_CONTROL.TXTBX_EMAIL)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskAutobookMonthVakues::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            setCurrentTimeZone(obj as ComboBox);
            // очистить содержание представления
            clear();
            m_currentOffSet = Session.m_curOffsetUTC;
        }

        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            int err = -1
             , id_alg = -1
             , ratio = -1
             , round = -1;
            string n_alg = string.Empty;
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);
            //
            settingDateRange();
            Session.SetDatetimeRange(dtBegin, dtEnd);
            // очистить содержание представления
            if (m_bflgClear)
            {
                clear();

                dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
                    , new int[] {
                        m_id_panel
                        , (int)Session.m_currIdPeriod }
                        , out err);

                IEnumerable<DataRow> listParameter = ListParameter.Select(x => x);

                foreach (DataRow r in listParameter)
                {
                    id_alg = (int)r[@"ID_ALG"];
                    n_alg = r[@"N_ALG"].ToString().Trim();
                    // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                    if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                        // добавить в список идентификатор параметра алгоритма расчета
                        m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);
                }

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg.Trim()) == true)
                {// установленные в проекте
                    ratio = dictVisualSettings[n_alg.Trim()].m_ratio;
                    round = dictVisualSettings[n_alg.Trim()].m_round;
                }
                else
                {// по умолчанию
                    ratio = HTepUsers.s_iRatioDefault;
                    round = HTepUsers.s_iRoundDefault;
                }

                m_dgvValues.ClearRows();
                //m_dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
                //заполнение представления
                for (int i = 0; i < DaysInMonth; i++)
                {
                    m_dgvValues.AddRow(new DataGridViewAutobookMonthValues.ROW_PROPERTY()
                    {
                        m_idAlg = id_alg
                                ,
                        //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                        //,
                        m_Value = dt.AddDays(i).ToShortDateString()
                                ,
                        m_vsRatio = ratio
                                ,
                        m_vsRound = round
                    });
                }
            }
            //m_dgvAB.SelectionChanged += dgvAB_SelectionChanged;
            //
            m_currentOffSet = Session.m_curOffsetUTC;
            m_bflgClear = true;
        }

        /// <summary>
        /// Установка длительности периода 
        /// </summary>
        private void settingDateRange()
        {
            int cntDays,
                today = 0;

            //PanelManagementAB.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

            //cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
            //  (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            //today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
            //    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

            //cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
            //    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
            //today = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

            //(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
            //    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

            //PanelManagementAB.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

        }

        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_dictTableDictPrj[ID_DBTABLE.COMP].Select().ToList();

                return listRes;
            }
        }

        /// <summary>
        /// очистка грида
        /// </summary>
        /// <param name="bClose"></param>
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true)
            {
                m_dgvValues.ClearRows();
                //dgvAB.ClearColumns();
            } else
                // очистить содержание представления
                m_dgvValues.ClearValues();

            base.clear(bClose);
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            int idTimezone = m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex];

            Session.SetCurrentTimeZone((ID_TIMEZONE)idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod(PanelManagement.IdPeriod);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            PanelManagement.SetModeDatetimeRange();
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);

            // очистить содержание представления
            clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementAutobookMonthValues.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }

        ///// <summary>
        ///// формирование запросов 
        ///// для справочных данных
        ///// </summary>
        ///// <returns>запрос</returns>
        //private string[] getQueryDictPrj()
        //{
        //    string[] arRes = null;

        //    arRes = new string[]
        //    {
        //        //PERIOD
        //        HandlerDb.GetQueryTimePeriods(m_strIdPeriods)
        //        //TIMEZONE
        //        , HandlerDb.GetQueryTimezones(m_strIdTimezones)
        //        // список компонентов
        //        , HandlerDb.GetQueryComp(Type)
        //        // параметры расчета
        //        //, HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        //, HandlerDb.GetQueryModeDev()
        //        //// единицы измерения
        //        , m_handlerDb.GetQueryMeasures()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio()
        //    };

        //    return arRes;
        //}

        /// <summary>
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;
            //сбор значений
            valuesFence();

            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = getStructurOutval(
                GetNameTableOut(PanelManagement.DatetimeRange.Begin), out err);

            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                HandlerDb.SaveResOut(m_TableOrigin, m_TableEdit, out err);

            if (m_TableEdit.Rows.Count > 0)
                //save вх. значений
                saveInvalValue(out err);
        }

        /// <summary>
        /// получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(string nameTable, out int err)
        {
            string strRes = string.Empty;

            strRes = "SELECT * FROM " + nameTable;

            return HandlerDb.Select(strRes, out err);
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableOut(DateTime dtInsert)
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
                , GetNameTableOut(PanelManagement.DatetimeRange.Begin)
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

            m_handlerDb.RecUpdateInsertDelete(nameTable
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

        /// <summary>
        /// Сохранение входных знчений(корр. величины)
        /// </summary>
        /// <param name="err">Признак выполнения операций внутри метода</param>
        private void saveInvalValue(out int err)
        {
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                HandlerDb.GetInPutID(TaskCalculateType, dtrPer, Session.ActualIdPeriod, out err);

            m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
            HandlerDb.SaveResInval(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , out err);

            sortingDataToTable(m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , getNameTableIn(dtrPer[0].Begin)
                , @"ID"
                , out err);
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        }
    }
}

