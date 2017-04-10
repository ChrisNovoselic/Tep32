using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HClassLibrary;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;

namespace PluginTaskVedomostBl
{
    partial class PanelTaskVedomostBl
    {
        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            /// <summary>
            /// экземпляр интерфейса приложения
            /// </summary>
            private Excel.Application m_execApp;
            /// <summary>
            /// экземпляр интерфейса книги
            /// </summary>
            private Excel.Workbook m_workBook;
            /// <summary>
            /// экземпляр интерфейса листа
            /// </summary>
            private Excel.Worksheet m_workSheet;
            //private object _missingObj = System.Reflection.Missing.Value;
            /// <summary>
            /// Массив данных
            /// </summary>
            protected object[,] arrayData;

            /// <summary>
            /// Перечисление - типы
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
                m_execApp = new Excel.Application();
                m_execApp.Visible = false;
            }

            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgView">отрбражение данных</param>
            /// <param name="dtRange">дата</param>
            public void CreateExcel(DataGridView dgView, DateTimeRange dtRange)
            {
                if (addWorkBooks()) {
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_workSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("VedomostBl");
                    int indxCol = 1;

                    try {
                        paintTable(dgView);
                    } catch (Exception e) {
                        closeExcel();

                        Logging.Logg().Exception(e, @"PanelTaskVedomostBl.ReportExcel::CreateExcel () - вызов 'paintTable () - '...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    try {
                        fillToArray(dgView);

                        for (int i = 0; i < dgView.Columns.Count; i++) {
                            //if (i >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1)) {
                                Excel.Range colRange = (Excel.Range)m_workSheet.Columns[indxCol];

                                if (dgView.Columns[i].HeaderText.Equals(string.Empty) == false) {
                                    foreach (Excel.Range cell in colRange.Cells)
                                        if (Convert.ToString(cell.Value).Equals(string.Empty) == false) {
                                            if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText)) {
                                                fillSheetExcel(colRange, dgView, i, cell.Row);
                                                break;
                                            }
                                        }
                                }
                                //else
                                //    foreach (Excel.Range cell in colRange.Cells)
                                //        if (Convert.ToString(cell.Value) == dgView.Columns[i].Name)
                                //        {
                                //            fillSheetExcelToNHeader(colRange, dgView, i, cell.Row + 1);
                                //            break;
                                //        }
                                break;

                            //    //indxCol++;
                            //} else
                            //    ;
                        }

                        setSignature(m_workSheet, dgView, dtRange);
                        m_execApp.Visible = true;

                        closeExcel();
                    } catch (Exception e) {
                        closeExcel();

                        Logging.Logg().Exception(e, @"PanelTaskVedomostBl.ReportExcel::CreateExcel () - общая ошибка...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }

            /// <summary>
            /// Заполнение массива данными
            /// </summary>
            /// <param name="dgvActive">активное отображение данных</param>
            private void fillToArray(DataGridView dgvActive)
            {
                arrayData = new object[dgvActive.RowCount, dgvActive.ColumnCount - 1];
                int indexArray = -1;

                for (int i = 0; i < dgvActive.Rows.Count; i++) {
                    indexArray = 0;

                    arrayData[i, indexArray++] = dgvActive.Rows[i].Tag.ToString();

                    for (int j = 0; j < dgvActive.Columns.Count; j++)
                        //if (j >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1)) {
                            //if (j > ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                                arrayData[i, indexArray] =
                                    //s_VedCalculate.AsParseToF
                                    HMath.doubleParse
                                        (dgvActive.Rows[i].Cells[j].Value.ToString());
                            //else
                            //??? получить дату
                            //    arrayData[i, indexArray] = dgvActive.Rows[i].Cells[j].Value.ToString();

                            indexArray++;
                        //} else
                        //    ;                    
                }
            }

            /// <summary>
            /// Составление таблицы
            /// </summary>
            /// <param name="dgvActive">активное окно данных</param>
            private void paintTable(DataGridView dgvActive)
            {
                int indxCol = 0,
                    colSheetBegin = 2, colSheetEnd = 1,
                    rowSheet = 2,
                    idComponent = (dgvActive as DataGridViewVedomostBl).IdComponent;
                //m_excApp.Visible = true;
                //получаем диапазон
                Excel.Range colRange = (m_workSheet.Cells[2, colSheetBegin - 1] as Excel.Range);
                //записываем данные в ячейки
                colRange.Cells[rowSheet + 1, colSheetBegin - 1] = "Дата";
                //получаем диапазон с условием длины заголовка
                var cellsDate = m_workSheet.get_Range(getAdressRangeCol(rowSheet, (rowSheet + 1) + 1, colSheetBegin - 1));
                //объединяем ячейки
                mergeCells(cellsDate.Address);
                cellsDate.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                cellsDate.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                paintBorder(cellsDate, (int)Excel.XlLineStyle.xlContinuous);

                foreach (var list in s_listGroupHeaders)
                    foreach (var item in list) {
                        //получаем диапазон
                        colRange = (m_workSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = item;
                        colSheetEnd += (dgvActive as DataGridViewVedomostBl).m_arCounterHeaderTop[indxCol];
                        //выделяем область(левый верхний угол и правый нижний)
                        var cells = m_workSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                        //объединяем ячейки
                        mergeCells(cells.Address);
                        //string w = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range).ColumnWidth.ToString();
                        //(cells as Excel.Range).Width.ToString();
                        //

                        //выравнивание текста в ячейке                  
                        cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                        colSheetBegin = colSheetEnd + 1;

                        indxCol++;
                    }
                colSheetBegin = 2;
                //выделяем область(левый верхний угол и правый нижний)
                var Commoncells = m_workSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1;
                rowSheet = 3;

                foreach (var item in (dgvActive as DataGridViewVedomostBl).m_listTextHeaderMiddle[idComponent]) {
                    //получаем диапазон
                    colRange = (m_workSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                    //записываем данные в ячейки
                    colRange.Value2 = item;
                    colSheetEnd += (dgvActive as DataGridViewVedomostBl).m_arCounterHeaderMiddle[(dgvActive as DataGridViewVedomostBl).m_listTextHeaderMiddle[idComponent].ToList().IndexOf(item)];
                    // выделяем область(левый верхний угол и правый нижний)
                    var cells = m_workSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                    //объединяем ячейки
                    mergeCells(cells.Address);

                    //
                    cells.WrapText = true;
                    cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                    colSheetBegin = colSheetEnd + 1;
                }
                colSheetBegin = 2;
                //       
                Commoncells = m_workSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1;
                rowSheet = 3;

                for (int i = 0; i < dgvActive.Columns.Count; i++) {
                    //if (i > ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1)) {
                        //получаем диапазон
                        colRange = (m_workSheet.Cells[rowSheet + 1, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = dgvActive.Columns[i].HeaderText;
                        // выделяем область(левый верхний угол и правый нижний)
                        var cells = m_workSheet.get_Range(getAdressRangeRow(rowSheet + 1, colSheetBegin, colSheetEnd));

                        cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        paintBorder(cells, (int)Excel.XlLineStyle.xlContinuous);
                        colSheetEnd++;
                        colSheetBegin = colSheetEnd + 1;
                    //} else
                    //    ;
                }
            }

            /// <summary>
            /// Нарисовать границы ячейки
            /// </summary>
            /// <param name="cells">выбранный диапазон ячеек</param>
            /// <param name="typeBorder">тип линий</param>
            private void paintBorder(Excel.Range cells, int typeBorder)
            {
                Excel.XlLineStyle styleBorder = Excel.XlLineStyle.xlContinuous;

                switch ((Excel.XlLineStyle)typeBorder) {
                    case Excel.XlLineStyle.xlContinuous:
                        styleBorder = Excel.XlLineStyle.xlContinuous;
                        break;
                    case Excel.XlLineStyle.xlDash:
                        styleBorder = Excel.XlLineStyle.xlDash;
                        break;
                    case Excel.XlLineStyle.xlDashDot:
                        styleBorder = Excel.XlLineStyle.xlDashDot;
                        break;
                    case Excel.XlLineStyle.xlDashDotDot:
                        styleBorder = Excel.XlLineStyle.xlDashDotDot;
                        break;
                    case Excel.XlLineStyle.xlDot:
                        break;
                    case Excel.XlLineStyle.xlDouble:
                        break;
                    case Excel.XlLineStyle.xlSlantDashDot:
                        break;
                    case Excel.XlLineStyle.xlLineStyleNone:
                        break;
                    default:
                        break;
                }
                // внутренние вертикальные
                cells.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = styleBorder;
                // внутренние горизонтальные
                cells.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = styleBorder;
                // верхняя внешняя          
                cells.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = styleBorder;
                // правая внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = styleBorder;
                // левая внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = styleBorder;
                // нижняя внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = styleBorder;
            }

            /// <summary>
            /// Получения адреса диапазона ячеек в столбце
            /// </summary>
            /// <param name="rowSheet">номер строки</param>
            /// <param name="colSheetBegin">номер столбца начала</param>
            /// <param name="colSheetEnd">номер столбца конца</param>
            /// <returns>адрес ячеек в формате "A1:A3"</returns>
            private string getAdressRangeCol(int rowSheetBegin, int rowSheetEnd, int colSheet)
            {
                Excel.Range RowRangeBegin = (Excel.Range)m_workSheet.Cells[rowSheetBegin, colSheet],
                  RowRangeEnd = (Excel.Range)m_workSheet.Cells[rowSheetEnd, colSheet];
                string adressCell = string.Empty;

                adressCell = RowRangeBegin.Address + ":" + RowRangeEnd.Address;

                return adressCell;
            }

            /// <summary>
            /// Получения адреса диапазона ячеек в строке
            /// </summary>
            /// <param name="rowSheet">номер строки</param>
            /// <param name="colSheetBegin">номер столбца начала</param>
            /// <param name="colSheetEnd">номер столбца конца</param>
            /// <returns>адрес диапазона ячеек в формате "A1:C1"</returns>
            private string getAdressRangeRow(int rowSheet, int colSheetBegin, int colSheetEnd)
            {
                Excel.Range colRangeBegin = (Excel.Range)m_workSheet.Cells[rowSheet, colSheetBegin],
                    colRangeEnd = (Excel.Range)m_workSheet.Cells[rowSheet, colSheetEnd];
                string adressCell = string.Empty;

                adressCell = colRangeBegin.Address + ":" + colRangeEnd.Address;

                return adressCell;
            }

            /// <summary>
            /// Объединение ячеек
            /// </summary>
            /// <param name="cells">диапазон объединения</param>
            private void mergeCells(string cells)
            {
                m_workSheet.get_Range(cells).Merge();
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateVedBl.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try {
                    m_workBook = m_execApp.Workbooks.Add(pathToTemplate);
                } catch (Exception exp) {
                    closeExcel();
                    bflag = false;
                    MessageBox.Show("???" + "Отсутствует шаблон для отчета Excel" + exp.ToString());
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
                //Excel.Range exclTEC = exclWrksht.get_Range("B2");
                Excel.Range exclRMonth = exclWrksht.get_Range("R1");
                exclRMonth.Value2 = string.Format("Ведомость {0} за {1} месяц {2} года"
                    , ((HandlerDbTaskCalculate.TECComponent)(dgv as DataGridViewVedomostBl).Tag).m_nameShr, HDateTime.NameMonths[dtRange.Begin.Month - 1], dtRange.Begin.Year);
                exclRMonth.Font.Bold = true;
                //HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
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
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int _indxrow = 0;

                string addressRange = string.Empty,
                 addresBegin, addresEnd;
                int cellBegin, cellEnd = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True") {
                        _indxrow = i;
                        break;
                    }
                //формировние начальной и конечной координаты диапазона
                addresBegin = (colRange.Cells[_indxrow] as Excel.Range).Address;
                _indxrow = _indxrow + dgv.Rows.Count;
                cellEnd = cellEnd + (dgv.Columns.Count - 1);
                addresEnd = (m_workSheet.Cells[_indxrow - 1, cellEnd] as Excel.Range).Address;
                //получение диапазона
                addressRange = addresBegin + ":" + addresEnd;
                Excel.Range rangeFill = m_workSheet.get_Range(addressRange);
                //заполнение
                var arrayVar = arrayData;
                rangeFill.Value2 = arrayVar;
                paintBorder(rangeFill, (int)Excel.XlLineStyle.xlContinuous);
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне 
            /// (при условии пустого заголовка)
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcelToNHeader(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if ((((Excel.Range)colRange.Cells[i]).Value == null)
                        && (bool.Parse(((Excel.Range)colRange.Cells[i]).MergeCells.ToString()) == false))
                        if (((Excel.Range)colRange.Cells[i - 1]).Value2 == null) {
                            row = i;
                            break;
                        } else
                            ;
                    else
                        ;

                for (int j = 0; j < dgv.Rows.Count; j++) {
                    ////colRange.Cells.NumberFormat = "0";
                    //if (indxColDgv >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                        colRange.Cells[row] =
                            //s_VedCalculate.AsParseToF
                            HPanelTepCommon.AsParseToF
                                (Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value));
                    //else
                    //    colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);

                    paintBorder((Excel.Range)colRange.Cells[row], (int)Excel.XlLineStyle.xlContinuous);
                    row++;
                }
            }

            /// <summary>
            /// Удаление пустой строки
            /// (при условии, что ниже пустой строки есть строка с данными)
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="row">номер строки</param>
            private void deleteNullRow(Excel.Range colRange, int row)
            {
                Excel.Range rangeCol = (Excel.Range)m_workSheet.Columns[1];

                while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "") {
                    if (Convert.ToString(((Excel.Range)rangeCol.Cells[row + 1]).Value) == "")
                        break;
                    else {
                        Excel.Range rangeRow = (Excel.Range)m_workSheet.Rows[row];
                        rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                    }
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            private void closeExcel()
            {
                try {
                    //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                    //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(m_execApp);

                    m_execApp = null;
                    m_workBook = null;
                    m_workSheet = null;
                    GC.Collect();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl.ReportExcel::closeExcel () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }
    }
}
