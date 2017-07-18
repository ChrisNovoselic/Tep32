using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace TepCommon
{
    /// <summary>
    /// Класс формирования отчета Excel 
    /// </summary>
    public abstract class ReportMSExcel
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
        protected Excel.Worksheet m_wrkSheet;
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

        private const int BEGIN_DATA_ROW = 9;

        protected abstract void create();

        /// <summary>
        /// Подключение шаблона листа экселя и его заполнение
        /// </summary>
        /// <param name="dgv">отрбражение данных</param>
        /// <param name="dtRange">дата</param>
        public void Create(string nameSheet, DataGridView dgv, DateTimeRange dtRange)
        {
            if (addWorkbook() == true) {
                m_workBook.AfterSave += workBook_AfterSave;
                m_workBook.BeforeClose += workBook_BeforeClose;
                m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item(nameSheet);

                try {
                    create();

                    m_excApp.Visible = true;
                    Marshal.ReleaseComObject(m_excApp);
                } catch (Exception e) {
                    close();

                    Logging.Logg().Exception(e, string.Format(@"PanelTaskAutobookMonthValues.ReportMSExcel::Create () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        /// <summary>
        /// Подключение шаблона
        /// </summary>
        /// <returns>признак ошибки</returns>
        private bool addWorkbook()
        {
            bool bRes = false;

            //string pathToTemplate = @"D:\MyProjects\C.Net\TEP32\Tep\bin\Debug\Template\TemplateAutobook.xlsx";
            string pathToTemplate = Path.GetFullPath(@"Template\TemplateAutobook.xlsx");
            object pathToTemplateObj = pathToTemplate;

            try {
                m_workBook = m_excApp.Workbooks.Add(pathToTemplate);

                bRes = true;
            } catch (Exception e) {
                close();

                Logging.Logg().Exception(e, string.Format(@"PanelTaskAutobookMonthValues.ReportMSExcel::addWorkbook () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return bRes;
        }

        /// <summary>
        /// Обработка события - закрытие экселя
        /// </summary>
        /// <param name="Cancel"></param>
        void workBook_BeforeClose(ref bool Cancel)
        {
            //Close();
        }

        /// <summary>
        /// обработка события сохранения книги
        /// </summary>
        /// <param name="Success"></param>
        void workBook_AfterSave(bool Success)
        {
            close();
        }

        ///// <summary>
        ///// Деление 
        ///// </summary>
        ///// <param name="headerTxt">строка</param>
        ///// <returns>часть строки</returns>
        //private string splitString(string headerTxt)
        //{
        //    string[] spltHeader = headerTxt.Split(',');

        //    if (spltHeader.Length > (int)MODE_CELL_BORDER.ADJACENT)
        //        return spltHeader[(int)MODE_CELL_BORDER.ADJACENT].TrimStart();
        //    else
        //        return spltHeader[(int)MODE_CELL_BORDER.SEPARATE];
        //}

        protected void setHeaderValues(Excel.Range range, HClassLibrary.DateTimeRange dates, int indxRowExcel)
        {
            int row = -1
                , cntDay = -1;
            DateTime curDate;

            row = indxRowExcel;
            cntDay = (dates.End - dates.Begin).Days;

            for (int j = 0; j < cntDay; j++)
                range.Cells[row++] = Convert.ToString(curDate = dates.Begin.AddDays(j));
        }

        /// <summary>
        /// Заполнение выбранного стоблца в шаблоне
        /// </summary>
        /// <param name="range">столбец в excel</param>
        /// <param name="dgv">отображение</param>
        /// <param name="indxColDgv">индекс столбца</param>
        /// <param name="indxRowExcel">индекс строки в excel</param>
        private void setColumnValues(Excel.Range range
            , List<string> values
            , int indxRowExcel)
        {
            int row = 0;

            for (int i = indxRowExcel; i < range.Rows.Count; i++)
                if ((((Excel.Range)range.Cells[i]).Value == null)
                    && (((Excel.Range)range.Cells[i]).MergeCells.ToString().Equals(true.ToString()) == false)) {
                    row = i;

                    break;
                } else
                    ;

            for (int j = 0; j < values.Count; j++)
                range.Cells[row++] = Convert.ToString(values[j]);
        }

        /// <summary>
        /// вызов закрытия Excel
        /// </summary>
        private void close()
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
}
