using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

using ASUTP;

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

        private const string TEMPLATE_FOLDER = @"Template";

        private int _begin_data_row;

        private string _name_template_workbook;

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
        public ReportMSExcel(string nameTemplateWorkbook)
        {
            _name_template_workbook = nameTemplateWorkbook;

            m_excApp = new Excel.Application();
            m_excApp.Visible = false;
        }


        protected virtual void create(int headerColumn, int beginDataRow, Dictionary<int, List<string>> allValues, ASUTP.Core.DateTimeRange dtRange)
        {
            List<string> values;
            Excel.Range range;            

            //values = new List<string>();
            range = (Excel.Range)m_wrkSheet.Columns[headerColumn];
            setHeaderValues(range, dtRange, beginDataRow);

            foreach (int address in allValues.Keys) {
                range = (Excel.Range)m_wrkSheet.Columns[address];

                values = allValues[address];

                setColumnValues(range, values.Take(values.Count() - 1).ToList(), beginDataRow);
            }
        }

        /// <summary>
        /// Подключение шаблона листа экселя и его заполнение
        /// </summary>
        /// <param name="dgv">отрбражение данных</param>
        /// <param name="dtRange">дата</param>
        public void Create(string nameSheet, int headerColumn, int beginDataRow, Dictionary<int, List<string>> allValues, ASUTP.Core.DateTimeRange dtRange)
        {
            if (addWorkbook() == true) {
                m_workBook.AfterSave += workBook_AfterSave;
                m_workBook.BeforeClose += workBook_BeforeClose;
                m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item(nameSheet);

                try {
                    create(headerColumn, beginDataRow, allValues, dtRange);

                    m_excApp.Visible = true;
                    Marshal.ReleaseComObject(m_excApp);
                } catch (Exception e) {
                    close();

                    Logging.Logg().Exception(e, string.Format(@"TepCommon.ReportMSExcel::Create () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            } else
                Logging.Logg().Error(string.Format(@"TepCommon.ReportMSExcel::Create () - добавление книги..."), Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Подключение шаблона
        /// </summary>
        /// <returns>признак ошибки</returns>
        private bool addWorkbook()
        {
            bool bRes = false;

            string pathToTemplate = Path.GetFullPath(string.Format(@"{0}\{1}", TEMPLATE_FOLDER, _name_template_workbook));
            object pathToTemplateObj = pathToTemplate;

            try {
                m_workBook = m_excApp.Workbooks.Add(pathToTemplate);

                bRes = true;
            } catch (Exception e) {
                close();

                Logging.Logg().Exception(e, string.Format(@"TepCommon.ReportMSExcel::addWorkbook () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
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
        ///// Возвратить одну из частей строки, при наличии разделителя
        ///// </summary>
        ///// <param name="headerTxt">Исходная строка, в которой присутствует разделитель</param>
        ///// <returns>Часть строки</returns>
        //private string splitString(string headerTxt, char chSeparate = ',')
        //{
        //    string[] spltHeader = headerTxt.Split(chSeparate);

        //    if (spltHeader.Length > 1)
        //        // разделитель присутствует - вернуть крайнюю часть
        //        return spltHeader[1].TrimStart();
        //    else
        //        // разделитель отсутствует - вернуть исходную строку
        //        return spltHeader[0];
        //}

        ///// <summary>
        ///// Удаление пустой строки
        ///// </summary>
        ///// <param name="colRange">столбец в excel</param>
        ///// <param name="row">номер строки</param>
        //private void deleteNullRow(Excel.Range colRange, int row)
        //{
        //    Excel.Range rangeCol = (Excel.Range)m_wrkSheet.Columns[1];

        //    while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "") {
        //        Excel.Range rangeRow = (Excel.Range)m_wrkSheet.Rows[row];
        //        rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
        //    }
        //}

        protected void setHeaderValues(Excel.Range range, ASUTP.Core.DateTimeRange dates, int indxRowExcel)
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
        protected void setColumnValues(Excel.Range range
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
