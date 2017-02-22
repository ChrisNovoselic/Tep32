using HClassLibrary;
using InterfacePlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;

namespace PluginTaskVedomostBl
{
    public partial class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// ??? переменная с текущем отклоненеим от UTC
        /// </summary>
        static int s_currentOffSet;
        /// <summary>
        /// Для обозначения выбора 1 или 6 блоков
        /// </summary>
        static bool s_flagBl = true;
        ///// <summary>
        ///// ??? Делегат (возврат пикчи по Ид)
        ///// </summary>
        ///// <param name="id">ид грида</param>
        ///// <returns>picture</returns>
        //public delegate PictureBox PictureBoxDelegateIntFunc(int id);
        ///// <summary>
        ///// ??? Делегат 
        ///// </summary>
        ///// <returns>грид</returns>
        //public delegate DataGridView DataGridViewDelegateFunc();
        /// <summary>
        /// ??? экземпляр делегата(возврат пикчи по Ид)
        /// </summary>
        static public Func<int, PictureBox> s_getPicture;
        /// <summary>
        /// ??? экземпляр делегата(возврат отображения активного)
        /// </summary>
        static public Func<DataGridView> s_getDGV;
        /// <summary>
        /// ??? экземпляр делегата(возврат Ид)
        /// </summary>
        static public Func<int> s_getIdComp;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        private bool m_bflgClear = false;
        ///// <summary>
        ///// 
        ///// </summary>
        //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Массив словарей для составления хидеров каждого блока(ТГ)
        /// </summary>
        protected Dictionary<int, List<string[]>> m_dictHeaderBlock;
        /// <summary>
        /// Лист с группами хидеров отображения
        /// </summary>
        protected static List<List<string>> s_listGroupHeaders = new List<List<string>> {
            // группа №1
            new List<string> { "Острый пар", "Горячий промперегрев", "ХПП" }
            // группа №2
            , new List<string> { "Питательная вода", "Продувка", "Конденсатор", "Холодный воздух", "Горячий воздух", "Кислород", "VI отбор", "VII отбор" }
            // группа №3
            , new List<string> { "Уходящие газы", "", "", "", "РОУ", "Сетевая вода", "Выхлоп ЦНД" }
        };
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            /*, DGV_DATA_B1, DGV_DATA_B2, DGV_DATA_B3, DGV_DATA_B4, DGV_DATA_B5, DGV_DATA_B6
            , RADIOBTN_BLK1, RADIOBTN_BLK2, RADIOBTN_BLK3, RADIOBTN_BLK4, RADIOBTN_BLK5, RADIOBTN_BLK6*/
            , LABEL_DESC, TBLP_HGRID, PICTURE_BOXDGV, PANEL_PICTUREDGV
                , COUNT
        }
        /// <summary>
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            /*PERIOD, // идентификаторы периодов расчетов, использующихся на форме
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов*/
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //DENY_COMP_CALCULATED, 
            DENY_COMP_VISIBLED,
            BLOCK_VISIBLED, HGRID_VISIBLE,
            //DENY_PARAMETER_CALCULATED, // запрещенных для расчета
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Объект для обращения к БД
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskVedomostBlCalculate; } }
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementVedomostBl PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementVedomostBl;
            }
        }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementVedomostBl();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskVedomostBlCalculate();
        }
        /// <summary>
        /// экземпляр класса вьюхи
        /// </summary>
        protected DataGridViewVedomostBl m_dgvVedomst;
        /// <summary>
        /// 
        /// </summary>
        protected ReportExcel m_rptExcel;
        /// <summary>
        /// экземпляр класса пикчи
        /// </summary>
        protected PictureVedBl m_pictureVedBl;
        /// <summary>
        /// ??? почему статик Экземпляр класса обрабокти данных
        /// </summary>
        private static VedomostBlCalculate s_VedCalculate;
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        /// <summary>
        /// класс пикчи
        /// </summary>
        protected class PictureVedBl : PictureBox
        {
            /// <summary>
            /// ид Пикчи
            /// </summary>
            public int m_idCompPicture;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="viewActive">активный грид</param>
            public PictureVedBl(DataGridViewVedomostBl viewActive)
            {
                InitializeComponents(viewActive);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="viewActive">активный грид</param>
            private void InitializeComponents(DataGridViewVedomostBl viewActive)
            {
                int _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

                Size = new Size(viewActive.Width - 10, _drwH);
                m_idCompPicture = (int)viewActive.Tag;
                Controls.Add(viewActive);
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            /// <summary>
            /// экземпляр интерфейса приложения
            /// </summary>
            private Excel.Application m_excApp;
            /// <summary>
            /// экземпляр интерфейса книги
            /// </summary>
            private Excel.Workbook m_workBook;
            /// <summary>
            /// экземпляр интерфейса листа
            /// </summary>
            private Excel.Worksheet m_wrkSheet;
            //private object _missingObj = System.Reflection.Missing.Value;
            /// <summary>
            /// Массив данных
            /// </summary>
            protected object[,] arrayData;

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
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("VedomostBl");
                    int indxCol = 1;

                    try
                    {
                        paintTable(dgView);
                    }
                    catch (Exception e)
                    {
                        closeExcel();
                        MessageBox.Show("???" + "Ошибка прорисовки таблицы для экспорта! " + e.ToString());
                    }

                    try
                    {
                        fillToArray(dgView);

                        for (int i = 0; i < dgView.Columns.Count; i++)
                            if (i >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                            {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxCol];

                                if (dgView.Columns[i].HeaderText != "")
                                {
                                    foreach (Excel.Range cell in colRange.Cells)
                                        if (Convert.ToString(cell.Value) != "")
                                        {
                                            if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                                            {
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
                                //indxCol++;
                            }

                        setSignature(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        closeExcel();
                    }
                    catch (Exception e)
                    {
                        closeExcel();
                        MessageBox.Show("???" + "Ошибка экспорта данных!" + e.ToString());
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
                int indexArray = 0;

                for (int i = 0; i < dgvActive.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvActive.Columns.Count; j++)
                        if (j >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                        {
                            if (j > ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                                arrayData[i, indexArray] = s_VedCalculate.AsParseToF(dgvActive.Rows[i].Cells[j].Value.ToString());
                            else
                                arrayData[i, indexArray] = dgvActive.Rows[i].Cells[j].Value.ToString();

                            indexArray++;
                        }
                    indexArray = 0;
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
                    idDgv = (int)(dgvActive as DataGridViewVedomostBl).Tag;
                //m_excApp.Visible = true;
                //получаем диапазон
                Excel.Range colRange = (m_wrkSheet.Cells[2, colSheetBegin - 1] as Excel.Range);
                //записываем данные в ячейки
                colRange.Cells[rowSheet + 1, colSheetBegin - 1] = "Дата";
                //получаем диапазон с условием длины заголовка
                var cellsDate = m_wrkSheet.get_Range(getAdressRangeCol(rowSheet, (rowSheet + 1) + 1, colSheetBegin - 1));
                //объединяем ячейки
                mergeCells(cellsDate.Address);
                cellsDate.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                cellsDate.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                paintBorder(cellsDate, (int)Excel.XlLineStyle.xlContinuous);

                foreach (var list in s_listGroupHeaders)
                    foreach (var item in list)
                    {
                        //получаем диапазон
                        colRange = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = item;
                        colSheetEnd += (dgvActive as DataGridViewVedomostBl).m_arIntTopHeader[idDgv][indxCol];
                        //выделяем область(левый верхний угол и правый нижний)
                        var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
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
                var Commoncells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1; rowSheet = 3;

                foreach (var item in (dgvActive as DataGridViewVedomostBl).m_headerMiddle[idDgv])
                {
                    //получаем диапазон
                    colRange = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                    //записываем данные в ячейки
                    colRange.Value2 = item;
                    colSheetEnd += (dgvActive as DataGridViewVedomostBl).m_arMiddleCol[idDgv][(dgvActive as DataGridViewVedomostBl).m_headerMiddle[idDgv].ToList().IndexOf(item)];
                    // выделяем область(левый верхний угол и правый нижний)
                    var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
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
                Commoncells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1; rowSheet = 3;

                for (int i = 0; i < dgvActive.Columns.Count; i++)
                {
                    if (i > ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                    {
                        //получаем диапазон
                        colRange = (m_wrkSheet.Cells[rowSheet + 1, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = dgvActive.Columns[i].HeaderText;
                        // выделяем область(левый верхний угол и правый нижний)
                        var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet + 1, colSheetBegin, colSheetEnd));

                        cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        paintBorder(cells, (int)Excel.XlLineStyle.xlContinuous);
                        colSheetEnd++;
                        colSheetBegin = colSheetEnd + 1;
                    }
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

                switch ((Excel.XlLineStyle)typeBorder)
                {
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
                Excel.Range RowRangeBegin = (Excel.Range)m_wrkSheet.Cells[rowSheetBegin, colSheet],
                  RowRangeEnd = (Excel.Range)m_wrkSheet.Cells[rowSheetEnd, colSheet];
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
                Excel.Range colRangeBegin = (Excel.Range)m_wrkSheet.Cells[rowSheet, colSheetBegin],
                    colRangeEnd = (Excel.Range)m_wrkSheet.Cells[rowSheet, colSheetEnd];
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
                m_wrkSheet.get_Range(cells).Merge();
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
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
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
                exclRMonth.Value2 = "Ведомость блока №" + (dgv as DataGridViewVedomostBl).BlockCount + " за " + HDateTime.NameMonths[dtRange.Begin.Month - 1] + " месяц " + dtRange.Begin.Year + " года";
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
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        _indxrow = i;
                        break;
                    }
                //формировние начальной и конечной координаты диапазона
                addresBegin = (colRange.Cells[_indxrow] as Excel.Range).Address;
                _indxrow = _indxrow + dgv.Rows.Count;
                cellEnd = cellEnd + (dgv.Columns.Count - 1);
                addresEnd = (m_wrkSheet.Cells[_indxrow - 1, cellEnd] as Excel.Range).Address;
                //получение диапазона
                addressRange = addresBegin + ":" + addresEnd;
                Excel.Range rangeFill = m_wrkSheet.get_Range(addressRange);
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
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")

                        if (((Excel.Range)colRange.Cells[i - 1]).Value2 == null)
                        {
                            row = i;
                            break;
                        }

                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    //colRange.Cells.NumberFormat = "0";
                    if (indxColDgv >= ((int)DataGridViewVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                        colRange.Cells[row] = s_VedCalculate.AsParseToF(Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value));
                    else
                        colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);

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
                Excel.Range rangeCol = (Excel.Range)m_wrkSheet.Columns[1];

                while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "")
                {
                    if (Convert.ToString(((Excel.Range)rangeCol.Cells[row + 1]).Value) == "")
                        break;
                    else
                    {
                        Excel.Range rangeRow = (Excel.Range)m_wrkSheet.Rows[row];
                        rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                    }
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            private void closeExcel()
            {
                try
                {
                    //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                    //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                    m_excApp = null;
                    m_workBook = null;
                    m_wrkSheet = null;
                    GC.Collect();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        ///класс для обработки данных
        /// </summary>
        public class VedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public VedomostBlCalculate()
                : base()
            {

            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }
        
            /// <summary>
            /// преобразование числа в нужный формат отображения
            /// </summary>
            /// <param name="value">число</param>
            /// <returns>преобразованное число</returns>
            public float AsParseToF(string value)
            {
                float fRes = 0;

                int _indxChar = 0;
                string _sepReplace = string.Empty;
                bool bParsed = true;
                //char[] _separators = { ' ', ',', '.', ':', '\t'};
                //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();                

                foreach (char ch in value.ToCharArray())
                {
                    if (!char.IsDigit(ch))
                        if (char.IsLetter(ch))
                            value = value.Remove(_indxChar, 1);
                        else
                            _sepReplace = value.Substring(_indxChar, 1);
                    else
                        _indxChar++;

                    switch (_sepReplace) {
                        case ".":
                        case ",":
                        case " ":
                        case ":":
                            bParsed = float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fRes);
                            break;
                    }
                }

                if (bParsed == false)
                    try {
                        fRes = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    } catch (Exception) {
                        if (string.IsNullOrEmpty (value.ToString()) == true)
                            fRes = 0;
                        else
                            ;
                    }

                return fRes;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskVedomostBl(IPlugIn iFunc)
            : base(iFunc)
        {
            s_VedCalculate = new VedomostBlCalculate();

            HandlerDb.IdTask = ID_TASK.VEDOM_BL;
            //Session.SetDatetimeRange(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
            m_dictHeaderBlock = new Dictionary<int, List<string[]>> { };

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();

            s_getPicture = new Func <int, PictureBox> (GetPictureOfIdComp);
            s_getDGV = new Func<DataGridView>(GetDGVOfIdComp);
            s_getIdComp = new Func<int>(GetIdComp);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control();
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить" 

            SuspendLayout();

            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, 4); SetRowSpan(PanelManagement, 13);
            //контейнеры для DGV
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = INDEX_CONTROL.PICTURE_BOXDGV.ToString();
            pictureBox.TabStop = false;
            //
            Panel m_paneL = new Panel();
            m_paneL.Name = INDEX_CONTROL.PANEL_PICTUREDGV.ToString();
            m_paneL.Dock = DockStyle.Fill;
            (m_paneL as Panel).AutoScroll = true;
            Controls.Add(m_paneL, 5, posRow);
            SetColumnSpan(m_paneL, 9); SetRowSpan(m_paneL, 10);
            //
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += PanelTaskVedomostBl_expExcel_Click;
            PanelManagement.ItemCheck += new PanelManagementVedomostBl.ItemCheckedParametersEventHandler(panelManagement_ItemCheck);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).CheckedChanged += PanelManagementVedomost_CheckedChanged;
        }

        /// <summary>
        /// Обработчик события - Кнопка экспорта даных в Excel
        /// </summary>
        /// <param name="sender">объект, вызвавщий событие</param>
        /// <param name="e">Аргумент события, описывающий состояние элемента</param>
        private void PanelTaskVedomostBl_expExcel_Click(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();
            m_rptExcel.CreateExcel(getActiveView(), Session.m_rangeDatetime);
        }

        /// <summary>
        /// Обработчик события - изменение отображения кол-во групп заголовка
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_ItemCheck(PanelManagementVedomostBl.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //Изменить признак состояния компонента ТЭЦ/параметра алгоритма расчета
            if (ev.NewCheckState == CheckState.Unchecked)
                if (m_arListIds[(int)ev.m_indxId].IndexOf(idItem) < 0)
                    m_arListIds[(int)ev.m_indxId].Add(idItem);
                else; //throw new Exception (@"");
            else
                if (ev.NewCheckState == CheckState.Checked)
                if (!(m_arListIds[(int)ev.m_indxId].IndexOf(idItem) < 0))
                    m_arListIds[(int)ev.m_indxId].Remove(idItem);
                else; //throw new Exception (@"");
            else;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //Изменить структуру 'HDataGRidVIew's'          
            placementHGridViewOnTheForm(ev);
        }

        /// <summary>
        /// Обработчик события - Признак Корректировка_включена/корректировка_отключена 
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void PanelManagementVedomost_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Изменить структуру 'HDataGRidVIew's'
        /// </summary>
        /// <param name="item"></param>
        private void placementHGridViewOnTheForm(PanelManagementVedomostBl.ItemCheckedParametersEventArgs item)
        {
            bool bItemChecked = item.NewCheckState == CheckState.Checked ? true :
                  item.NewCheckState == CheckState.Unchecked ? false : false;
            DataGridViewVedomostBl cntrl = (getActiveView() as DataGridViewVedomostBl);
            //Поиск индекса элемента отображения
            switch ((INDEX_ID)item.m_indxId)
            {
                case INDEX_ID.HGRID_VISIBLE:
                    cntrl.HideColumns(cntrl as DataGridView, s_listGroupHeaders[item.m_idItem], bItemChecked);
                    ReSizeControls(cntrl as DataGridView);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Нахожджение активного DGV
        /// </summary>
        /// <returns>активная вьюха на панели</returns>
        private DataGridView getActiveView()
        {
            bool _flagb = false;
            Control cntrl = new Control();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                if (item.Visible == true)
                    foreach (DataGridView dgv in item.Controls)
                    {
                        cntrl = dgv;
                        _flagb = true;
                    }
                else if (_flagb)
                    break;

            return (cntrl as DataGridView);
        }

        /// <summary>
        /// Настройка размеров контролов отображения
        /// </summary>
        private void ReSizeControls(DataGridView dgv)
        {
            int cntVisibleColumns = 0;

            foreach (DataGridViewColumn col in dgv.Columns) {
                if (dgv.Columns.IndexOf(col) > 0)
                    col.Width = 65;
                else
                    ;

                if (col.Visible == true)
                    cntVisibleColumns++;
                else
                    ;
            }

            int _drwW = cntVisibleColumns * dgv.Columns[2].Width + 10
                , _drwH = (dgv.Rows.Count) * dgv.Rows[0].Height + 70;

            GetPictureOfIdComp((int)(dgv as DataGridViewVedomostBl).Tag).Size = new Size(_drwW + 2, _drwH);
            dgv.Size = new Size(_drwW + 2, _drwH);
        }

        /// <summary>
        /// Обработчик события - добавления строк в грид
        /// (для изменение размера контролов)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void DGVVedomostBl_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ReSizeControls(sender as DataGridView);
        }

        /// <summary>
        /// Возвращает пикчу по номеру
        /// </summary>
        /// <param name="idComp">ид номер грида</param>
        /// <returns>активная пикча на панели</returns>
        public PictureBox GetPictureOfIdComp(int idComp)
        {
            int cnt = 0,
                outCnt = 0;
            PictureBox cntrl = new PictureBox();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
            {
                if (idComp == item.m_idCompPicture)
                {
                    outCnt = cnt;
                    cntrl = (item as PictureBox);
                }
                else
                {
                    (item as PictureBox).Visible = false;
                    (item as PictureBox).Enabled = false;
                }
                cnt++;
            }

            if (outCnt == 0 || outCnt == 5)
                WhichBlIsSelected = true;
            else
                WhichBlIsSelected = false;

            return cntrl;
        }

        /// <summary>
        /// Возвращает по номеру грид
        /// </summary>
        /// <returns>активный грид на панели</returns>
        public DataGridView GetDGVOfIdComp()
        {
            DataGridView cntrl = new DataGridView();

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                foreach (DataGridViewVedomostBl item in picture.Controls)
                    if (item.Visible == true)
                        cntrl = (item as DataGridView);

            return cntrl;

        }

        /// <summary>
        /// Возвращает idComp
        /// </summary>
        /// <returns>индентификатор объекта</returns>
        public int GetIdComp()
        {
            int _idComp = 0;

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                if (picture.Visible == true)
                    foreach (DataGridViewVedomostBl item in picture.Controls)
                        if (item.Visible == true)
                            _idComp = (int)item.Tag;

            return _idComp;
        }

        /// <summary>
        /// Настройка размеров формы отображения данных
        /// </summary>
        /// <param name="dgv">активное окно отображения данных</param>
        public void ConfigureDataGridView(DataGridView dgv)
        {
            (dgv as DataGridViewVedomostBl).ConfigureColumns(dgv as DataGridView);
        }

        /// <summary>
        /// Инициализация радиобаттанов
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeRadioButtonBlock(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            
            try {
                //if (arId_comp[rbCnt] != 0)
                //добавление радиобатонов на форму
                PanelManagement.AddComponentRadioButton(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST]);

            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initializeRadioButtonBlock () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void initializeCheckBoxGroupHeaders(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            try {
                //??? добавить элементы управления (а подписи к группам)
                PanelManagement.AddComponentCheckBoxGroupHeaders(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST]);
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initializeRadioButtonBlock () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Инициализация сетки данных
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeDataGridView(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            int i = -1
                , idPar = -1
                , avg = -1
                , idComp = -1;
            DataGridViewVedomostBl dgv = null;
            DateTime dtRow = PanelManagement.DatetimeRange.Begin;
            //DataTable tableComponentId; // ид компонентов
            Dictionary<string, List<int>> dictVisualSett;

            //tableComponentId = HandlerDb.GetHeaderDGV(); // получение ид компонентов

            //создание грида со значениями
            for (i = 0; i < m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows.Count; i++)
            {
                dgv = new DataGridViewVedomostBl(int.Parse(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows[i]["ID"].ToString()));
                dgv.Name = string.Format(@"DGV_DATA_B{0}", i);
                dgv.BlockCount = i + 1;

                m_dictHeaderBlock.Add((int)dgv.Tag, GetListHeaders(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER], (int)dgv.Tag)); // cловарь заголовков

                dictVisualSett = visualSettingsCol((int)dgv.Tag);

                for (int k = 0; k < m_dictHeaderBlock[(int)dgv.Tag].Count; k++)
                {
                    idPar = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["ID_ALG"].ToString());
                    avg = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["AVG"].ToString());
                    idComp = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["ID"].ToString());

                    dgv.AddColumns(idPar
                        , new DataGridViewVedomostBl.COLUMN_PROPERTY {
                            topHeader = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.TOP].ToString(),
                            nameCol = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.MIDDLE].ToString(),
                            hdrText = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.LOW].ToString(),
                            m_idAlg = idPar,
                            m_IdComp = idComp,
                            m_vsRatio = dictVisualSett["ratio"][k],
                            m_vsRound = dictVisualSett["round"][k],
                            m_Avg = avg
                        }
                       , true);
                }

                for (i = 0; i < DaysInMonth + 1; i++)
                    if (dgv.Rows.Count != DaysInMonth)
                        dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY()
                        {
                            //m_idAlg = id_alg
                            //,
                            m_Value = dtRow.AddDays(i).ToShortDateString()
                        });
                    else {
                        dgv.RowsAdded += DGVVedomostBl_RowsAdded;

                        dgv.AddRow(
                            new DataGridViewVedomostBl.ROW_PROPERTY() {
                                //m_idAlg = id_alg
                                //,
                                m_Value = "ИТОГО"
                            }
                            , DaysInMonth
                        );
                    }

                ConfigureDataGridView(dgv);
                m_pictureVedBl = new PictureVedBl(dgv);
                (Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0] as Panel).Controls.Add(m_pictureVedBl);
                //возможность_редактирвоания_значений
                try {
                    if (m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()).Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true) {
                        if (int.Parse(m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()).Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                        else
                            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                    } else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                    if ((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                        for (int t = 0; t < dgv.RowCount; t++)
                            dgv.ReadOnlyColumns = false;
                    else
                        ;
                } catch (Exception exp) {
                    MessageBox.Show("???" + "Ошибки проверки возможности редактирования ячеек " + exp.ToString());
                }
            }
        }

        /// <summary>
        /// Инициализация групп отображения заголовков
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeGroupHeaders(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            int id_comp;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[]
            {
                INDEX_ID.HGRID_VISIBLE
            };

            bool[] arChecked = new bool[s_listGroupHeaders.Count];

            //getControl();

            foreach (var list in s_listGroupHeaders)
            {
                id_comp = s_listGroupHeaders.IndexOf(list);
                strItem = "Группа " + (id_comp + 1);
                // установить признак отображения группы столбцов
                //for (int i = 0; i < arChecked.Count(); i++)
                arChecked[id_comp] = true;
                PanelManagement.AddComponent(id_comp
                    , strItem
                    , list
                    , arIndxIdToAdd
                    , arChecked);
            }
        }

        /// <summary>
        /// Инициализация объектов формы
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_PERIOD idProfilePeriod = ID_PERIOD.UNKNOWN;
            string strItem = string.Empty;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            for (INDEX_ID id = INDEX_ID.ALL_COMPONENT; id < INDEX_ID.COUNT; id++)
                switch (id) {
                    /*case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;*/
                    case INDEX_ID.ALL_COMPONENT:
                        m_arListIds[(int)id] = new List<int> { };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }
            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMWZONE, COMP, PARAMETER, RATIO
            initialize
            //m_markTableDictPrj = new HMark
                (new ID_DBTABLE[] { /*ID_DBTABLE.PERIOD
                    , */ID_DBTABLE.TIME, ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , TaskCalculateType == HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );

            PanelManagement.Clear();
            //Dgv's
            initializeDataGridView(out err, out errMsg); //???
            //groupHeader                                        
            initializeGroupHeaders(out err, out errMsg);
            //радиобаттаны
            initializeRadioButtonBlock(out err, out errMsg);
            // активировать обработчик событий по индексу идентификатора
            PanelManagement.ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.HGRID_VISIBLE });
            //активность_кнопки_сохранения
            try
            {
                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                }
                else
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception exp) {
            // ???
                MessageBox.Show("???" + exp.ToString());
            }

            if (err == 0)
            {
                try
                {
                    if (m_bflgClear == false)
                        m_bflgClear = true;
                    else
                        m_bflgClear = false;

                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , new int[] { (int)ID_TIMEZONE.MSK }
                        , ID_TIMEZONE.MSK);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]);
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , new int[] { (int)ID_PERIOD.DAY }
                        , idProfilePeriod);
                    Session.SetCurrentPeriod(PanelManagement.IdPeriod);
                    PanelManagement.SetModeDatetimeRange();

                    (ctrl as ComboBox).Enabled = false;

                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), @"...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
        }

        /// <summary>
        /// Получение визуальных настроек 
        /// для отображения данных на форме
        /// </summary>
        /// <param name="idComp">идКомпонента</param>
        /// <returns>словарь настроечных данных</returns>
        private Dictionary<string, List<int>> visualSettingsCol(int idComp)
        {
            int err = -1
             , id_alg = -1;
            List<int> ratio = new List<int>()
            , round = new List<int>();
            string n_alg = string.Empty;

            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            Dictionary<string, List<int>> _dictSett = new Dictionary<string, List<int>>();

            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
               , new int[] {
                    m_id_panel
                    , idComp }
               , out err);

            IEnumerable<DataRow> listParameter = ListParameter.Select(x => x).Where(x => (int)x["ID_COMP"] == idComp);

            foreach (DataRow r in listParameter)
            {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();
                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                    // добавить в список идентификатор параметра алгоритма расчета
                    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg) == true)
                {// установленные в проекте
                    ratio.Add(dictVisualSettings[n_alg.Trim()].m_ratio);
                    round.Add(dictVisualSettings[n_alg.Trim()].m_round);
                }
                else
                {// по умолчанию
                    ratio.Add(HTepUsers.s_iRatioDefault);
                    round.Add(HTepUsers.s_iRoundDefault);
                }
            }
            _dictSett.Add("ratio", ratio);
            _dictSett.Add("round", round);

            return _dictSett;
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <returns>кол-во дней</returns>
        public int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            if (m_bflgClear)
            {
                //Установить новое значение для текущего периода
                setCurrentTimeZone(obj as ComboBox);
                // очистить содержание представления
                clear();
            }
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            ID_TIMEZONE idTimezone =
                //m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex]
                PanelManagement.IdTimezone
                ;

            Session.SetCurrentTimeZone(idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
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
            if (m_bflgClear)
                // очистить содержание представления
                clear();
        }

        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            int //err = -1,
              id_alg = -1;
            DataGridViewVedomostBl _dgv = (getActiveView() as DataGridViewVedomostBl);
            string n_alg = string.Empty;
            DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);

            //settingDateRange();
            Session.SetDatetimeRange(dtBegin, dtEnd);

            if (m_bflgClear)
            {
                clear();

                if (_dgv.Rows.Count != 0)
                    _dgv.ClearRows();

                for (int i = 0; i < DaysInMonth + 1; i++)
                {
                    if (_dgv.Rows.Count != DaysInMonth)
                        _dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = dt.AddDays(i).ToShortDateString()
                        });
                    else
                        _dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = "ИТОГО"
                        }
                        , DaysInMonth);
                }
            }

            _dgv.Rows[dtBegin.Day - 1].Selected = true;
            s_currentOffSet = Session.m_curOffsetUTC;
        }

        ///// <summary>
        ///// Установка длительности периода 
        ///// </summary>
        //private void settingDateRange()
        //{
        //    int cntDays,
        //        today = 0;

        //    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //      (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

        //    PanelManagementVedomostBl.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

        //}

        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select().ToList();

                return listRes;
            }
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = Session.CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet;

            if (!WhichBlIsSelected)
                dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
            else
                dtrGet = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

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
                        // отобразить значения
                        (getActiveView() as DataGridViewVedomostBl).ShowValues(m_arTableOrigin[(int)Session.m_ViewValues], m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER], Session.m_ViewValues);
                        //сохранить готовые значения в таблицу
                        m_arTableEdit[(int)Session.m_ViewValues] = valuesFence();
                    }
                    else
                        deleteSession();
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskVedomostBl::updatedataValues() - " + errMsg);
                }
            }
            else
                deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
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

            m_handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME, QUALITY"
                    , unCol
                    , origin
                    , edit
                    , out err);
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr">текст ошибки</param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            if (Session.m_ViewValues == HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE)
                //Запрос для получения архивных данных
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutvalArch(TaskCalculateType, HandlerDb.GetDateTimeRangeValuesVarArchive(), out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar(
                TaskCalculateType
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
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
                        , m_dictTableDictPrj[ID_DBTABLE.COMP_LIST]
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
            m_arTableEdit[(int)Session.m_ViewValues] =
             m_arTableOrigin[(int)Session.m_ViewValues].Clone();
        }

        /// <summary>
        /// формирование таблицы данных
        /// </summary>
        private DataTable valuesFence()
        { //сохранить вх. знач. в DataTable
            return (getActiveView() as DataGridViewVedomostBl).FillTableToSave(m_TableOrigin, (int)Session.m_Id, Session.m_ViewValues);
        }

        /// <summary>
        /// ??? проверка выборки блока(для 1 и 6)
        /// </summary>
        public bool WhichBlIsSelected
        {
            get { return s_flagBl; }

            set { s_flagBl = value; }
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
        //        , HandlerDb.GetQueryParameters(Type)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        //, HandlerDb.GetQueryModeDev()
        //        //// единицы измерения
        //        //, m_handlerDb.GetQueryMeasures()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio()
        //    };

        //    return arRes;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err">номер ошибки</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// удачное заврешение UpdateInsertDelete
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVarArchive();

            m_arTableOrigin[(int)Session.m_ViewValues] =
                HandlerDb.GetDataOutval(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, dtR, out err);
            //HandlerDb.GetInVal(Type
            //, dtR
            //, ActualIdPeriod
            //, out err);

            m_arTableEdit[(int)Session.m_ViewValues] =
            HandlerDb.SaveValues(m_arTableOrigin[(int)Session.m_ViewValues]
                , valuesFence()
                , (int)Session.m_currIdTimezone
                , out err);

            saveInvalValue(out err);
        }

        /// <summary>
        /// Сохранение входных знчений
        /// </summary>
        /// <param name="err">номер ошибки</param>
        private void saveInvalValue(out int err)
        {
            DateTimeRange[] dtrPer;

            if (!WhichBlIsSelected)
                dtrPer = HandlerDb.GetDateTimeRangeValuesVar();
            else
                dtrPer = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

            sortingDataToTable(m_arTableOrigin[(int)Session.m_ViewValues]
                , m_arTableEdit[(int)Session.m_ViewValues]
                , HandlerDb.GetNameTableOut(dtrPer[0].Begin)
                , @"ID"
                , out err
            );
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

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);

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

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);
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
            string newNameTable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + newNameTable;
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;

            onButtonLoadClick();
        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            updateDataValues();
        }
    }

    /// <summary>
    /// Класс для взамодействия с основным приложением (вызывающая программа)
    /// </summary>
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 21;
            register(21, typeof(PanelTaskVedomostBl), @"Задача", @"Ведомости эн./блоков");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}



