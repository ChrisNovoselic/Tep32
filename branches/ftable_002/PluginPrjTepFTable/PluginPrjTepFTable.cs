using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Windows.Forms; //DataGridView...
using System.Drawing; //Graphics
using ZedGraph;
using DataGridViewAutoFilter; //фильт значений "А3"

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjTepFTable
{
    public delegate void CheckBoxClickedHandler(bool state);
    /// <summary>
    /// Класс для отработки события чека чекбокса
    /// </summary>
    public class DataGridViewCheckBoxHeaderCellEventArgs : EventArgs
    {
        bool _bChecked;
        public DataGridViewCheckBoxHeaderCellEventArgs(bool bChecked)
        {
            _bChecked = bChecked;
        }
        public bool Checked
        {
            get { return _bChecked; }
        }
    }

    /// <summary>
    /// Класс для создания чекбокса в заголовке грида
    /// </summary>
    class DatagridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        Point checkBoxLocation; // расположение чекбокса
        Size checkBoxSize; //размер чекбокса
        bool _checked = false; //
        static bool bFlag;
        Point _cellLocation = new Point();
        System.Windows.Forms.VisualStyles.CheckBoxState _cbState =
            System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal; //визуаллизация состояния чекбокса
        public event CheckBoxClickedHandler OnCheckBoxClicked;

        public DatagridViewCheckBoxHeaderCell()
        {
        }

        /// <summary>
        /// Отрисовка чекбокса и флажка
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds">границы чека</param>
        /// <param name="rowIndex">индекс строки отрисовки чека</param>
        /// <param name="dataGridViewElementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle">стиль ячейки грида</param>
        /// <param name="advancedBorderStyle">стиль границ между ячейками</param>
        /// <param name="paintParts"></param>
        protected override void Paint(System.Drawing.Graphics graphics,
            System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value,
                formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            Point p = new Point();
            Size s = CheckBoxRenderer.GetGlyphSize(graphics, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);

            p.X = cellBounds.Location.X + (cellBounds.Width / 2) - (s.Width / 2);
            p.Y = cellBounds.Location.Y + (cellBounds.Height / 2) - (s.Height / 2);
            _cellLocation = cellBounds.Location;
            checkBoxLocation = p;
            checkBoxSize = s;

            //проверка состояния чека, отрисовка флажка
            if (bFlag)
                _cbState = System.Windows.Forms.VisualStyles.
                    CheckBoxState.CheckedNormal;
            else
                _cbState = System.Windows.Forms.VisualStyles.
                    CheckBoxState.UncheckedNormal;
            CheckBoxRenderer.DrawCheckBox
            (graphics, checkBoxLocation, _cbState);
        }

        /// <summary>
        /// обработка чека
        /// </summary>
        /// <param name="flag"></param>
        public static void chekedState(bool flag)
        {
            bFlag = flag;
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);

            if (p.X >= checkBoxLocation.X && p.X <=
                checkBoxLocation.X + checkBoxSize.Width
            && p.Y >= checkBoxLocation.Y && p.Y <=
                checkBoxLocation.Y + checkBoxSize.Height)
            {
                _checked = !_checked;
                if (OnCheckBoxClicked != null)
                {
                    OnCheckBoxClicked(_checked);
                    this.DataGridView.InvalidateCell(this);
                }
            }
            base.OnMouseClick(e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PluginPrjTepFTable : HPanelTepCommon
    {
<<<<<<< .mine
        DataTable m_tblOrign, m_tableEdit;
        ZedGraph.ZedGraphControl m_zGraph_fTABLE; // график фукнции
        double min1 = Math.Exp(15); //граница диапазона первая
        double min2 = -1 * Math.Exp(15); //граница диапазона вторая
        double metka1; //значения от мин1
        double metka2; //значения от мин2
        GraphPane pane; //область отображения графиков
        LineItem[] myCurve; //массив графиков
        PointPairList[] pointList; //
        string nameALG; //имя функции
        string referencePoint; //выбранная точка фукнции
        string[] ArgApproxi; //массив значений калькулятора
        DataRow[] ValuesFunc; // массив аргументов функции
        bool condition; // условие, которое задается для вычисления минимумов и для перестройки массивов
=======
        DataTable m_tblOrign, m_tableEdit;
        ZedGraph.ZedGraphControl m_zGraph_fTABLE;
        //System.Windows.Forms.DataVisualization.Charting.Chart m_chartGraph_fTABLE;
        double min1 = Math.Exp(15); //граница диапазона первая
        double min2 = -1 * Math.Exp(15); //граница диапазона вторая
        double metka1;
        double metka2;
        string nameALG; //имя функции
        List<double> minPoint = new List<double>();
        string referencePoint; //выбранная точка фукнции
        DataRow[] ValuesFunc; // массив аргументов функции
        bool condition; // условие, которое задается для вычисления минимумов и для перестройки массивов
>>>>>>> .r97


        protected static Color[] m_colorLine = { Color.Red, Color.Green, Color.Blue, Color.Black, Color.PeachPuff, Color.Khaki, Color.PaleGoldenrod };
        /// <summary>
        /// Набор цветов для гарфиков
        /// </summary>
        protected static Color[] m_colorLine = { Color.Red, Color.Green, Color.Blue, Color.Navy, Color.Teal,
                                                 Color.Black,  Color.PeachPuff, Color.MediumVioletRed,
                                                 Color.SandyBrown, Color.ForestGreen, Color.DarkGreen,
                                                 Color.BlueViolet, Color.Plum, Color.YellowGreen,
                                                 Color.Moccasin, Color.DarkTurquoise,Color.Maroon};
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_DELETE,
            BUTTON_ADD, BUTTON_UPDATE,
            DGV_fTABLE, DGV_algTABLE,
            LABEL_DESC, INDEX_CONTROL_COUNT,
<<<<<<< .mine
            ZGRAPH_fTABLE, CHRTGRAPH_fTABLE,
            TEXTBOX_FIND, LABEL_FIND, PANEL_FIND,
            TABLELAYOUTPANEL_CALC, BUTTON_CALC,
            TEXTBOX_A1, TEXTBOX_A2, TEXTBOX_A3,
            TEXTBOX_F, TEXTBOX_REZULT, GRPBOX_CALC,
            COMBOBOX_PARAM
=======
            ZGRAPH_fTABLE, CHRTGRAPH_fTABLE,
            TEXTBOX_FIND, LABEL_FIND, PANEL_FIND
>>>>>>> .r97
        };

        /// <summary>
        /// Набор кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Сохранить", @"Обновить", @"Добавить", @"Удалить" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public override bool Activate(bool activate)
        {
            return base.Activate(activate);
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(ref DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            //int i = -1;
            string strConn = "SELECT * FROM [TEP_NTEC_5].[dbo].[ftable]";

            if (err == 0)
            {
                fillALGTable(ref dbConn, out err, out errMsg);
                m_tblOrign = DbTSQLInterface.Select(ref dbConn, strConn, null, null, out err);
            }

            Logging.Logg().Debug(@"PluginTepPrjFTable::initialize () - усПех ...", Logging.INDEX_MESSAGE.NOT_SET);

        }

        /// <summary>
        /// Класс - общий для графического представления значений
        /// </summary>
        //private class HZedGraph : ZedGraphControl
        // {
        //     /// <summary>
        //     /// Конструктор - основной (без параметров)
        //     /// </summary>
        //     public HZedGraph()
        //         : base()
        //     {
        //         initializeComponent();
        //     }

        //     /// <summary>
        //     /// Конструктор - вспомогательный (с параметрами)
        //     /// </summary>
        //     /// <param name="container">Владелец объекта</param>
        //     public HZedGraph(IContainer container)
        //         : this()
        //     {
        //         container.Add(this);
        //     }

        //     /// <summary>
        //     /// Инициализация собственных компонентов элемента управления
        //     /// </summary>
        //     private void initializeComponent()
        //     {
        //         this.ScrollGrace = 0;
        //         this.ScrollMaxX = 0;
        //         this.ScrollMaxY = 0;
        //         this.ScrollMaxY2 = 0;
        //         this.ScrollMinX = 0;
        //         this.ScrollMinY = 0;
        //         this.ScrollMinY2 = 0;
        //         this.TabIndex = 0;
        //         this.IsEnableHEdit = false;
        //         this.IsEnableHPan = false;
        //         this.IsEnableHZoom = false;
        //         this.IsEnableSelection = false;
        //         this.IsEnableVEdit = false;
        //         this.IsEnableVPan = false;
        //         this.IsEnableVZoom = false;
        //         this.IsShowPointValues = true;
        //     }          
        // }

        /// <summary>
        ///Запрос к БД, для заполнения таблцы функциями
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        /// <param name="strErr"></param>
        /// <param name="query">строка запроса</param>
        /// <returns></returns>
        private DataTable SqlConn(ref DbConnection dbConn, out int err, out string strErr, string m_query)
        {
            err = 0;
            strErr = string.Empty;
            DataTable m_tblwork = new DataTable();

            return m_tblwork = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);
        }

        /// <summary>
        /// Заполнение таблицы с функциями
        /// </summary>
        /// <param name="dbConn">подключение</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr">ошибка</param>
        private void fillALGTable(ref DbConnection dbConn, out int err, out string strErr)
        {
            string m_query = "SELECT DISTINCT N_ALG, DESCRIPTION FROM [TEP_NTEC_5].[dbo].[ftable] ORDER BY N_ALG ";
            m_tableEdit = SqlConn(ref dbConn, out err, out strErr, m_query);

            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);

            addNumberRows(m_tableEdit.Rows.Count, dgv);

            for (int i = 0; i < m_tableEdit.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_tableEdit.Rows[i]["N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_tableEdit.Rows[i]["DESCRIPTION"];
            }
        }

<<<<<<< .mine
        /// <summary>
        /// Создание массива с данными всех значений одной функции
        /// Заполняет значениями функции для работы с ними.
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void createParamMassive(string nameALG, string nColumn, int row)
=======
        /// <summary>
        /// Заполняет значениями функции
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void createParamMassive(string nameALG)
>>>>>>> .r97
        {
<<<<<<< .mine
            string m_fName = "N_ALG = '" + nameALG + "'";
            string m_searchVal2 = string.Empty;
            string m_searchValn = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Rows[row].Cells[nColumn].Value.ToString();

            if (nColumn == "A3")
                m_searchVal2 = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Rows[row].Cells["A2"].Value.ToString();

            if (!(nColumn == "A1"))
            {
                if (nColumn == "A2")
                {
                    m_fName += " and " + nColumn + " = '" + m_searchValn.Replace(",", ".") + "'";
                }
                else
                {
                    m_fName += " and " + nColumn + " = " + m_searchValn.Replace(",", ".") + " and A2 = '" + m_searchVal2.Replace(",", ".") + "'";
                }
            }

            ValuesFunc = m_tblOrign.Select(m_fName);
=======
            string m_fName = "N_ALG = '" + nameALG + "'";
            ValuesFunc = m_tblOrign.Select(m_fName);
>>>>>>> .r97
        }

        /// <summary>
<<<<<<< .mine
        /// Проверка на вложеность
        /// числа в диапазон первых минимумов
=======
        /// Проверка на вложеность
        /// числа в диапазон
        /// </summary>
        /// <param name="columnname"></param>
        private void rangeOfValues(string columnName)
        {
            if (((Convert.ToDouble(referencePoint)) < min2) && ((Convert.ToDouble(referencePoint)) > min1))
            {

                for (int i = 0; i < ValuesFunc.Length; i++)
                {
                    if (condition == true)
                    {
                        interpolation(columnName);
                    }
                }
            }
            else
            {
                if (condition == true)
                {
                    extrapolation(columnName);
                }
            }
        }

        /// <summary>
        /// Интерполяция значений функции
        /// </summary>
        /// <param name="columnName"></param>
        private void interpolation(string columnName)
        {
            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (testConditionMIN1(i, columnName) == true)
                {
                    min1 = Convert.ToDouble(ValuesFunc[i][columnName].ToString());
                    metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }

                if (testConditionMIN2(i, columnName) == true)
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][columnName].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }
            }
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN1
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца</param>
        /// <returns></returns>
        private bool testConditionMIN1(int i, string column)
        {
            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = m_selectedCell - Convert.ToDouble(ValuesFunc[i][column]);
            double m_twoPeremen = m_selectedCell - min1;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min2)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN2
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца массива</param>
        /// <returns></returns>
        private bool testConditionMIN2(int i, string column)
        {
            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = Convert.ToDouble(ValuesFunc[i][column]) - m_selectedCell;
            double m_twoPeremen = min2 - m_selectedCell;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min1)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Экстраполяция значений функций
        /// </summary>
        /// <param name="column"></param>
        private void extrapolation(string column)
        {
            bool m_bflag1 = true;
            bool m_bflag2 = true;

            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (m_bflag1 == true)
                {
                    min1 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    m_bflag1 = false;
                }

                if (m_bflag2 == true && min1 == Convert.ToDouble(ValuesFunc[i][column]))
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    m_bflag2 = false;
                }

                if (m_bflag1 == false && m_bflag2 == false)
                {
                    ABSfunc(i, column);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n_column"></param>
        private void ABSfunc(int i, string n_column)
        {
            double m_oneParam = Convert.ToDouble(referencePoint);
            double m_peremen1 = Math.Abs(m_oneParam - Convert.ToDouble(ValuesFunc[i][n_column].ToString()));
            double m_ABSmin1 = Math.Abs(m_oneParam - min1);
            double m_ABSmin2 = Math.Abs(m_oneParam - min2);

            if (m_peremen1 < m_ABSmin1 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min2))
            {
                if (m_ABSmin1 < m_ABSmin2)
                {
                    min2 = min1;
                    metka2 = metka1;
                }
                min1 = Convert.ToDouble(ValuesFunc[i][n_column].ToString());
                metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
            }
            else
            {
                if (m_peremen1 < m_ABSmin2 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min1))
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][n_column].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }
            }
        }

        /// <summary>
        /// Определение границ диапазона
        /// аргумента от наименьшего до наибольшего
        /// </summary>
        /// <param name="nameColumn">имя колонки</param>
        private void searchMainMIN(string nameColumn)
        {
            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (condition == true)
                {
                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) < min1)
                    {
                        min1 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka1 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }

                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) > min2)
                    {
                        min2 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka2 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //private void receivingAdditionalMIN()
        //{
        //    for (int i = 0; i < ValuesFunc.Length; i++)
        //    {
        //        if (condition == true)
        //        {
        //            if (Convert.ToDouble(ValuesFunc[i][nameColumn]) < min1)
        //            {
        //                min1 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
        //                metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
        //            }

        //            if (Convert.ToDouble(ValuesFunc[i][nameColumn]) > min2)
        //            {
        //                min2 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
        //                metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        private void Chart(object X, object Y)
        {
            //m_chartGraph_fTABLE = new System.Windows.Forms.DataVisualization.Charting.Chart();
            //m_chartGraph_fTABLE.ChartAreas["Area1"] = new ChartArea("Area1");
            //m_chartGraph_fTABLE.ChartAreas["Area1"].AxisX.Minimum = min1;
            //m_chartGraph_fTABLE.ChartAreas["Area1"].AxisX.Maximum = min2;
            ////m_chartGraph_fTABLE.ChartAreas["Area1"].AxisX.Maximum = min2;
            //m_chartGraph_fTABLE.Series["S"] = new Series("S");

            // m_chartGraph_fTABLE.Series["S"].Points.AddXY(X,Y);

            //m_chartGraph_fTABLE.Series["S"].YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Primary;

            //MasterPane mP = new MasterPane();
            //GraphPane gP = new GraphPane();

            //m_chartGraph_fTABLE.Invalidate();
        }

        /// <summary>
        /// Проверка на кол-во аргументов функции
        /// </summary>
        /// <param name="nameF">имя функции</param>
        private void checkAmountArg(string nameF, int row)
        {
            int m_indx = nameF.IndexOf(":");
            string m_amountARG = nameF[m_indx + 1].ToString();

            switch (m_amountARG)
            {
                case "1":
                    condition = true;
                    funcWithOneArgs(m_amountARG, row);

                    break;
                case "2":
                    condition = true;
                    funcWithTwoArgs(m_amountARG, row);

                    break;
                case "3":
                    funcWithThreeArgs(m_amountARG, row);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Формированре масива 
        /// значений для графика функции
        /// </summary>
        /// <param name="nameCol">кол-во переменных</param>
        /// <param name="row">номер ячейки</param>
        private void formingArrayValues(int row, string nameCol)
        {
            double[] m_dmA1;
            double[] m_dmF;
            m_dmA1 = new double[ValuesFunc.Length];
            m_dmF = new double[ValuesFunc.Length];

            var m_enmfTAble = (from r in ValuesFunc.AsEnumerable()
                               select new
                               {
                                   nameColumn = r.Field<float>(nameCol),
                               }).Distinct();

            for (int i = 0; i < m_enmfTAble.Count(); i++)
            {
                for (int j = 0; j < ValuesFunc.Length; j++)
                {
                    if (m_enmfTAble.ElementAt(i) == ValuesFunc[j][nameCol])
                    {
                        m_dmA1[i] = new double();
                        m_dmF[i] = new double();

                        m_dmA1.SetValue(ValuesFunc[i]["A1"].ToString(), i);
                        m_dmF.SetValue(ValuesFunc[i]["F"].ToString(), i);
                    }
                }
            }

            DrawGraph(m_dmA1, m_dmF, m_enmfTAble.Count());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colList">кол-во графиков</param>
        private void DrawGraph(double[] x, double[] y, int colList)
        {
            //m_zGraph_fTABLE = new ZedGraphControl();

            GraphPane pane = m_zGraph_fTABLE.GraphPane;

            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            // Создадим списоки точек
            PointPairList[] pointList;
            pointList = new PointPairList[colList];

            for (int i = 0; i < colList; i++)
            {
                pointList[i] = new PointPairList();
            }

            // Заполняем список точек
            for (int i = 0; i < colList; i++)
            {
                pointList[i].Add(x, y);
            }

            for (int i = 0; i < colList; i++)
            {
                LineItem myCurve = pane.AddCurve("NAME FUNC", pointList[i], m_colorLine.ElementAt(i), SymbolType.None);
            }

            // Устанавливаем интересующий нас интервал по оси X
            /* pane.XAxis.Scale.Min = min1;
             pane.XAxis.Scale.Max = min2;

             // !!!
             // Устанавливаем интересующий нас интервал по оси Y
             pane.YAxis.Scale.Min = metka1;
             pane.YAxis.Scale.Max = metka2;*/
            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            m_zGraph_fTABLE.AxisChange();

            // Обновляем график
            m_zGraph_fTABLE.Invalidate();

        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с одним параметром
        /// </summary>
        /// <param name="colCount">кол-во аргументов</param>
        private void funcWithOneArgs(string nameCol, int row)
        {
            string nameColumn = "A" + nameCol;

            searchMainMIN(nameColumn);
            formingArrayValues(row, nameColumn);
            //receiptCharts(nameCol);
            //rangeOfValues(nameColumn);
            //obtaingPointMain();
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с двумя параметрами
        /// </summary>
        /// <param name="nameCol">кол-во аргументов</param>
        private void funcWithTwoArgs(string nameCol, int row)
        {
            string nameColumn = "A" + nameCol;

            formingArrayValues(row, nameColumn);
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с тремя парметрами
        /// </summary>
        /// <param name="nameCol">кол-во аргументов</param>
        private void funcWithThreeArgs(string nameCol, int numCol)
        {
            string nameColumn = "A" + nameCol;

            formingArrayValues(numCol, nameColumn);

            if (true)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colCount"></param>
        private void filterArray(int colCount, string nameCol)
        {
            //        int row = 0;
            //        bool m_bFlag = Convert.ToDouble(ValuesFunc[][nameCol].ToString())==min1;

            //        if ()
            //{

            //}
            //        for (int i = 0; i < ValuesFunc.Length; i++)
            //        {
            //            if (m_bFlag == true)
            //            {
            //                row++;

            //                for (int j = 0; j < colCount; j++)
            //                {
            //                    string column = "A"+colCount;
            //                    ValuesFunc[row][column] = ValuesFunc[i][column];
            //                }
            //            }
            //        }
            //        //???
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double obtaingPointMain()
        {
            return ((Convert.ToDouble(referencePoint) - min1) * (metka2 - metka1)) / (min2 - min1) + metka1;
        }

        //private double 
        /// <summary>
        /// Добавление строк
>>>>>>> .r97
        /// </summary>
        /// <param name="columnname">имя столбца("А1,А2,А3")</param>
        private void rangeOfValues(string columnName)
        {
            if (((Convert.ToDouble(referencePoint)) < min2) && ((Convert.ToDouble(referencePoint)) > min1))
            {

                for (int i = 0; i < ValuesFunc.Length; i++)
                {
                    if (condition == true)
                    {
                        interpolation(columnName);
                    }
                }
            }
            else
            {
                if (condition == true)
                {
                    extrapolation(columnName);
                }
            }
        }

        /// <summary>
        /// Интерполяция значений функции
        /// </summary>
        /// <param name="columnName">имя столбца</param>
        private void interpolation(string columnName)
        {
            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (testConditionMIN1(i, columnName) == true)
                {
                    min1 = Convert.ToDouble(ValuesFunc[i][columnName].ToString());
                    metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }

                if (testConditionMIN2(i, columnName) == true)
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][columnName].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }
            }
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN1
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца</param>
        /// <returns></returns>
        private bool testConditionMIN1(int i, string column)
        {
            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = m_selectedCell - Convert.ToDouble(ValuesFunc[i][column]);
            double m_twoPeremen = m_selectedCell - min1;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min2)))
                return true;

            else return false;
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN2
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца массива</param>
        /// <returns></returns>
        private bool testConditionMIN2(int i, string column)
        {
            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = Convert.ToDouble(ValuesFunc[i][column]) - m_selectedCell;
            double m_twoPeremen = min2 - m_selectedCell;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min1)))
                return true;

           else return false;
        }

        /// <summary>
        /// Экстраполяция значений функций
        /// </summary>
        /// <param name="column"></param>
        private void extrapolation(string column)
        {
            bool m_bflag1 = true;
            bool m_bflag2 = true;

            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (m_bflag1 == true)
                {
                    min1 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    m_bflag1 = false;
                }

                if (m_bflag2 == true && min1 == Convert.ToDouble(ValuesFunc[i][column]))
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    m_bflag2 = false;
                }

                if (m_bflag1 == false && m_bflag2 == false)
                {
                    ABSfunc(i, column);
                }
            }
        }

        /// <summary>
        /// Проверка аргумента на близость к заданому числу, 
        /// если он еще ближе чем минимумы, то запоминается
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n_column"></param>
        private void ABSfunc(int i, string n_column)
        {
            double m_oneParam = Convert.ToDouble(referencePoint);
            double m_peremen1 = Math.Abs(m_oneParam - Convert.ToDouble(ValuesFunc[i][n_column].ToString()));
            double m_ABSmin1 = Math.Abs(m_oneParam - min1);
            double m_ABSmin2 = Math.Abs(m_oneParam - min2);

            if (m_peremen1 < m_ABSmin1 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min2))
            {
                if (m_ABSmin1 < m_ABSmin2)
                {
                    min2 = min1;
                    metka2 = metka1;
                }
                min1 = Convert.ToDouble(ValuesFunc[i][n_column].ToString());
                metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
            }
            else
            {
                if (m_peremen1 < m_ABSmin2 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min1))
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][n_column].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }
            }
        }

        /// <summary>
        /// Определение границ диапазона
        /// аргумента от наименьшего до наибольшего
        /// </summary>
        /// <param name="nameColumn">имя колонки</param>
        private void searchMainMIN(string nameColumn)
        {
            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (condition == true)
                {
                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) < min1)
                    {
                        min1 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka1 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }

                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) > min2)
                    {
                        min2 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka2 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //private void receivingAdditionalMIN()
        //{
        //    for (int i = 0; i < ValuesFunc.Length; i++)
        //    {
        //        if (condition == true)
        //        {
        //            if (Convert.ToDouble(ValuesFunc[i][nameColumn]) < min1)
        //            {
        //                min1 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
        //                metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
        //            }

        //            if (Convert.ToDouble(ValuesFunc[i][nameColumn]) > min2)
        //            {
        //                min2 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
        //                metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Проверка на кол-во аргументов функции(отображение графиков)
        /// </summary>
        /// <param name="nameF">имя функции</param>
        private void checkAmountArg(int row)
        {
            int m_indx = nameALG.IndexOf(":");
            string m_nameColumn = "A" + nameALG[m_indx + 1].ToString();
            string m_filter = "N_ALG = '" + nameALG + "'";

            switch (m_nameColumn)
            {
                case "A1":
                    condition = true;
                    funcWithOneArgs(m_nameColumn, m_filter);
                    break;
                case "A2":
                    condition = true;
                    funcWithTwoArgs(m_nameColumn, createqueryString(row));
                    selectGroupCheckBox(row, m_nameColumn);
                    break;
                case "A3":
                    condition = true;
                    funcWithThreeArgs(createqueryString(row));
                    selectGroupCheckBox(row, m_nameColumn);
                    break;
                default:
                    MessageBox.Show("MZF");
                    break;
            }
        }

        /// <summary>
        /// Изменение грида(скрыть/показать столбцы, добавить столбцы с чекбоксом и фильтром)
        /// </summary>
        /// <param name="countArg">кол-во аргументов</param>
        private void addColumnsInGrid(string countArg)
        {
            createChkBoxHeaderCell(((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]));
            addCheckedListHeader(((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]));

            if (countArg == "A1")
            {
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["check"].Visible = false;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3filter"].Visible = false;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3"].Visible = true;
            }
            else
            {
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["check"].Visible = true;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3"].Visible = false;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3filter"].Visible = true;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3filter"].DisplayIndex = 2;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["F"].DisplayIndex = 3;
            }

            if (countArg == "A2")
            {
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3filter"].Visible = false;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3"].Visible = true;
                ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Columns["A3"].DisplayIndex = 2;
            }

        }

        /// <summary>
        /// Создание чекбокосов 
        /// и добавление колонки
        /// </summary>
        /// <param name="dgv">датагрид</param>
        private void createChkBoxHeaderCell(DataGridView dgv)
        {
            if (dgv.Columns["check"] == null)
            {
                DataGridViewCheckBoxColumn columnsChckdBox = new DataGridViewCheckBoxColumn();
                DatagridViewCheckBoxHeaderCell columnsChckdBoxHeader = new DatagridViewCheckBoxHeaderCell();
                columnsChckdBox.HeaderCell = columnsChckdBoxHeader;
                columnsChckdBoxHeader.Value = true;
                dgv.Columns.Add(columnsChckdBox);
                columnsChckdBox.ThreeState = false;
                columnsChckdBox.Name = "check";
                columnsChckdBoxHeader.OnCheckBoxClicked += new CheckBoxClickedHandler(cbHeader_OnCheckBoxClicked);
            }
        }

        /// <summary>
        /// Создание фильтрующего списка
        /// </summary>
        /// <param name="dgv"></param>
        private void addCheckedListHeader(DataGridView dgv)
        {
            if (dgv.Columns["A3filter"] == null)
            {
                DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn paramFilter = new DataGridViewAutoFilterTextBoxColumn();
                dgv.Columns.Add(paramFilter);
                paramFilter.DataPropertyName = "A3";
                paramFilter.HeaderText = "A3";
                paramFilter.Name = "A3filter";
                paramFilter.Resizable = System.Windows.Forms.DataGridViewTriState.True;
                dgv.KeyDown += new System.Windows.Forms.KeyEventHandler(dataGridView_KeyDown);
                dgv.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(dataGridView_DataBindingComplete);
            }
        }

        /// <summary>
        /// Displays the drop-down list when the user presses 
        /// ALT+DOWN ARROW or ALT+UP ARROW.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
            {
                DataGridViewAutoFilterColumnHeaderCell filterCell =
                      ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).CurrentCell.OwningColumn.HeaderCell as
                    DataGridViewAutoFilterColumnHeaderCell;
                if (filterCell != null)
                {
                    filterCell.ShowDropDownList();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// обновляет кол-во элеменьов в выборке(не работает/не нужен)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //String filterStatus = DataGridViewAutoFilterColumnHeaderCell.GetFilterStatus(dgv);

            //if (String.IsNullOrEmpty(filterStatus))
            //{
            //    showAllLabel.Visible = false;
            //    filterStatusLabel.Visible = false;
            //}
            //else
            //{
            //    showAllLabel.Visible = true;
            //    filterStatusLabel.Visible = true;
            //    filterStatusLabel.Text = filterStatus;
            //}
        }

        /// <summary>
        /// Функция обработки клика по чекбоксу в хидере
        /// </summary>
        /// <param name="check"></param>
        private void cbHeader_OnCheckBoxClicked(bool check)
        {
            DataGridView dgv = (DataGridView)(Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);

            if (!(DataGridViewAutoFilterColumnHeaderCell.FilterValue() == ""))
            {
                DataGridViewCheckBoxCell chk;

                int m_indx = nameALG.IndexOf(":");
                string m_nameColumn = "A" + nameALG[m_indx + 1].ToString();
                pane.CurveList.Clear();

                if (check == true)
                {
                    sampleValues(m_nameColumn);
                }


                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (check == true)
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = true;
                        this.Update();
                        DatagridViewCheckBoxHeaderCell.chekedState(true);
                    }
                    else
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = false;
                        pane.CurveList.Clear();
                        DatagridViewCheckBoxHeaderCell.chekedState(false);
                        this.Update();
                    }
                }

                pane.AxisChange();
                m_zGraph_fTABLE.AxisChange();
                m_zGraph_fTABLE.Invalidate();
            }
            else
            {
                DatagridViewCheckBoxHeaderCell.chekedState(false);
            }
        }

        /// <summary>
        /// Формированре масива 
        /// значений для графика функции
        /// по статичной переменной
        /// </summary>
        /// <param name="nameCol">кол-во переменных</param>
        /// <param name="row">номер строки</param>
        private void formingArrayValues(string m_strquery)
        {
            DataRow[] m_ftTableF = m_tblOrign.Select(m_strquery.Replace(",", "."));

            PointPairList points = new PointPairList();

            for (int i = 0; i < m_ftTableF.Length; i++)
            {
                points.Add(Convert.ToDouble(m_ftTableF[i]["A1"]), Convert.ToDouble(m_ftTableF[i]["F"]));
            }

            drawGraph(points);
        }

        /// <summary>
        /// Формированре масива 
        /// значений для графика функции
        /// для всех значений
        /// </summary>
        /// <param name="nameCol">имя столбца</param>
        private void sampleValues(string nameCol)
        {
            string m_strquery = string.Empty;
            int m_countList;
            DataRow[] m_ftTableF;
            string m_elem;
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);

            var m_enValues = (from r in m_tblOrign.AsEnumerable()
                              where r.Field<string>("N_ALG") == nameALG
                              select new
                              {
                                  nameCol = r.Field<float>(nameCol),

                              }).Distinct();

            if (!(nameCol == "A3"))
            {
                m_countList = m_enValues.Count();
                pointList = new PointPairList[m_countList];

                for (int i = 0; i < m_enValues.Count(); i++)
                {

                    m_elem = m_enValues.ElementAt(i).nameCol.ToString().Replace(",", ".");
                    m_strquery = " N_ALG = " + "'" + nameALG + "'and " + nameCol + " = " + m_elem + "";

                    m_ftTableF = m_tblOrign.Select(m_strquery);

                    createPointLists(i, m_ftTableF);
                }
            }

            else
            {
                var m_enValuesA2 = (from r in m_tblOrign.AsEnumerable()
                                    where r.Field<string>("N_ALG") == nameALG
                                    select new
                                    {
                                        nameCol = r.Field<float>("A2"),

                                    }).Distinct();

                m_countList = m_enValues.Count() * m_enValuesA2.Count();
                pointList = new PointPairList[m_countList];
                int num = 0;
                string m_ftValue = DataGridViewAutoFilterColumnHeaderCell.FilterValue();

                m_elem = m_ftValue.ToString().Replace(",", ".");

                for (int i = 0; i < m_enValuesA2.Count(); i++)
                {
                    m_strquery = " N_ALG = " + "'" + nameALG + "' and " + nameCol + " = " + m_elem + " and A2= " + m_enValuesA2.ElementAt(i).nameCol.ToString().Replace(",", ".") + "";
                    m_ftTableF = m_tblOrign.Select(m_strquery);

                    if (!(m_ftTableF.Count() == 0))
                    {
                        createPointLists(num, m_ftTableF);
                        num++;
                    }
                }
            }
            createGraphs(pointList.Count());
        }

        /// <summary>
        /// Создание наборов точек
        /// </summary>
        /// <param name="i">номер листа</param>
        /// <param name="array">массив с точками</param>
        private void createPointLists(int i, DataRow[] array)
        {
            if (pointList[i] == null)
                pointList[i] = new PointPairList();

            for (int j = 0; j < array.Count(); j++)
            {
                //// Заполняем список точками          
                pointList[i].Add(Convert.ToDouble(array[j]["A1"]), Convert.ToDouble(array[j]["F"]));
            }
        }

        /// <summary>
        /// Формирование списка точек
        /// </summary>
        /// <param name="querry">запрос на выборку данных</param>
        /// <returns></returns>
        private PointPairList FillPointList(string querry)
        {
            DataRow[] m_ftTableF = m_tblOrign.Select(querry);

            PointPairList pointlist = new PointPairList();

            for (int i = 0; i < m_ftTableF.Length; i++)
            {
                pointlist.Add(Convert.ToDouble(m_ftTableF[i]["A1"]), Convert.ToDouble(m_ftTableF[i]["F"]));
            }

            return pointlist;
        }

        /// <summary>
        /// Создание графика на основе всех точек
        /// </summary>
        /// <param name="countListPoint">кол-во графиков</param>
        private void createGraphs(int countGraph)
        {
            myCurve = new LineItem[countGraph];

            for (int i = 0; i < countGraph; i++)
            {
                if (!(pointList[i] == null))
                {
                    myCurve[i] = new LineItem("" + i + "");
                    myCurve[i] = pane.AddCurve("NAME FUNC", pointList[i], m_colorLine.ElementAt(i), SymbolType.VDash);
                    myCurve[i].Label.Text = "" + nameALG + "";
                }
            }

            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;

            // По оси Y установим автоматический подбор масштаба
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;

            pane.YAxis.MajorGrid.IsZeroLine = false;
            // !!!
            // Устанавливаем интересующий нас интервал по оси Y
            //pane.YAxis.Scale.Min = metka1;
            // pane.YAxis.Scale.Max = metka2;

            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            pane.AxisChange();
            m_zGraph_fTABLE.AxisChange();

            // !!! Установим значение параметра IsBoundedRanges как true.
            // !!! Это означает, что при автоматическом подборе масштаба 
            // !!! нужно учитывать только видимый интервал графика
            pane.IsBoundedRanges = true;
            // Обновляем график
            m_zGraph_fTABLE.Invalidate();
            m_zGraph_fTABLE.Refresh();
        }

        /// <summary>
        /// Формирование строки запроса данных
        /// для построения гарфика
        /// </summary>
        /// <param name="row">номер строки</param>
        /// <returns>строка запроса к таблице</returns>
        private string createqueryString(int row)
        {
            int m_indx = nameALG.IndexOf(":");
            string m_nameColumn = "A" + nameALG[m_indx + 1].ToString();
            string m_filter = "N_ALG = '" + nameALG + "'";
            string m_searchValn = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Rows[row].Cells[m_nameColumn].Value.ToString();
            string m_searchVal2 = string.Empty;

            if (m_nameColumn == "A3")
                m_searchVal2 = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).Rows[row].Cells["A2"].Value.ToString();

            if (m_nameColumn == "A2")
            {
                m_filter += " and " + m_nameColumn + " = " + m_searchValn.Replace(",", ".") + "";
            }
            else
            {
                m_filter += " and " + m_nameColumn + " = " + m_searchValn.Replace(",", ".") + " and A2 =" + m_searchVal2.Replace(",", ".") + "";
            }

            return m_filter;
        }

        /// <summary>
        /// изменение чекбоксов в гриде
        /// </summary>
        /// <param name="rowIndex">номер строки для параметра</param>
        /// <param name="nameCol">имя колонки</param>
        private void selectGroupCheckBox(int rowIndex, string nameCol)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            string m_cellsRef = dgv.Rows[rowIndex].Cells[nameCol].Value.ToString();
            string m_A2Value;
            //bool m_bflag = m_cellsRef == dgv.Rows[i].Cells[nameCol].Value.ToString();
            DataGridViewCheckBoxCell chk;

            if (nameCol == "A3")
            {
                m_A2Value = dgv.Rows[rowIndex].Cells["A2"].Value.ToString();

                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (m_cellsRef == dgv.Rows[i].Cells[nameCol].Value.ToString() && m_A2Value == dgv.Rows[i].Cells["A2"].Value.ToString())
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = true;
                    }
                    else
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = false;
                    }
                }
            }

            else
            {
                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (m_cellsRef == dgv.Rows[i].Cells[nameCol].Value.ToString())
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = true;
                    }
                    else
                    {
                        chk = (DataGridViewCheckBoxCell)dgv.Rows[i].Cells["check"];
                        chk.Value = false;
                    }
                }
            }
        }

        /// <summary>
        /// Отображение графика функции
        /// по аргументу
        /// </summary>
        /// <param name="listP">набор координат</param>
        private void drawGraph(PointPairList listP)
        {
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            LineItem m_LineItemCure = pane.AddCurve("NAME FUNC" + referencePoint + "", listP, Color.Black, SymbolType.VDash);
            m_LineItemCure.Label.Text = "" + nameALG + "";

            // Устанавливаем интересующий нас интервал по оси X
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;

            // По оси Y установим автоматический подбор масштаба
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;
            // !!!
            // Устанавливаем интересующий нас интервал по оси Y
            //pane.YAxis.Scale.Min = metka1;
            // pane.YAxis.Scale.Max = metka2;
            pane.YAxis.MajorGrid.IsZeroLine = false;
            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            pane.AxisChange();
            m_zGraph_fTABLE.AxisChange();

            // !!! Установим значение параметра IsBoundedRanges как true.
            // !!! Это означает, что при автоматическом подборе масштаба 
            // !!! нужно учитывать только видимый интервал графика
            pane.IsBoundedRanges = true;
            // Обновляем график
            m_zGraph_fTABLE.Invalidate();
            m_zGraph_fTABLE.Refresh();
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с одним параметром
        /// </summary>
        /// <param name="colCount">кол-во аргументов</param>
        private void funcWithOneArgs(string nameCol, string filter)
        {
            searchMainMIN(nameCol);
            formingArrayValues(filter);
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с двумя параметрами
        /// </summary>
        /// <param name="nameCol"></param>
        /// <param name="filter"></param>
        private void funcWithTwoArgs(string nameCol, string filter)
        {
            //searchMainMIN(nameCol);
            formingArrayValues(filter);
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с тремя парметрами
        /// </summary>
        /// <param name="filter"></param>
        private void funcWithThreeArgs(string filter)
        {
            //searchMainMIN(nameCol);
            formingArrayValues(filter);
        }

        /// <summary>
        /// Фильтрация массива-функции
        /// сравнение значений массива с найденными минимумами
        /// </summary>
        /// <param name="colCount">кол-во столбцов</param>
        /// <param name="nameCol">имя столбца</param>
        /// <param name="arg">набор минимумов</param>
        private void filterArray(int colCount, string nameCol, string[] arg)
        {
            int m_row = 0;
            int m_colParam = 2;
            bool m_bFlag = false;

            if (arg.Count() > 3)
                m_colParam = 4;

            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                for (int t = 0; t < m_colParam; t++)
                {
                    if (Convert.ToDouble(ValuesFunc[i][nameCol]) == Convert.ToDouble(arg.ElementAt(t).ToString()))
                    {
                        m_bFlag = true;
                    }
                }

                if (m_bFlag)
                {
                    m_row++;

                    for (int j = 1; j < colCount; j++)
                    {
                        string column = "A" + j;
                        ValuesFunc[m_row][column] = ValuesFunc[i][column];
                    }
                }
            }
            //???
        }

        /// <summary>
        /// вычисление конечного (return)
        /// результата
        /// </summary>
        /// <returns></returns>
        private double obtaingPointMain(double metka1, double metka2, double min1, double min2, double refPoint)
        {
            return ((Convert.ToDouble(refPoint) - min1) * (metka2 - metka1)) / (min2 - min1) + metka1;
        }

        /// <summary>
        /// Добавление строк в датагрид
        /// </summary>
        /// <param name="count">кол-во строк/param>
        /// <param name="dgv">датагрид</param>
        private void addNumberRows(int count, DataGridView dgv)
        {
            if (dgv.RowCount > 0)
            {
                dgv.Rows.Clear();

                for (int i = 0; i < count; i++)
                {
                    dgv.Rows.Add();
                }
            }

            else
            {
                for (int i = 0; i < count; i++)
                {
                    dgv.Rows.Add();
                }
            }
        }

        /// <summary>
        /// Функция динамического поиска
        /// </summary>
        /// <param name="text">искомый элемент</param>
        private void m_findALG(string text)
        {
            string m_fltr = string.Format("{0} like '{1}%'", new object[] { "N_ALG", text });
            DataRow[] m_drSearch = m_tableEdit.Select(m_fltr);
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);

            addNumberRows(m_drSearch.Count(), dgv);

            for (int i = 0; i < m_drSearch.Count(); i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_drSearch[i][@"N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_drSearch[i][@"DESCRIPTION"];
            }
        }

        /// <summary>
        /// Создание таблицы для датагрида
        /// для заполнения источника данных
        /// </summary>
        /// <param name="dr">строки</param>
        /// <returns></returns>
        private BindingSource createSource(DataRow[] dr)
        {
            DataTable dt = new DataTable();
            DataColumn DC1 = new DataColumn("A1");
            DC1.DataType = System.Type.GetType("System.Decimal");
            dt.Columns.Add(DC1);

            DataColumn DC2 = new DataColumn("A2");
            DC2.DataType = System.Type.GetType("System.Decimal");
            dt.Columns.Add(DC2);

            DataColumn DC3 = new DataColumn("A3");
            DC3.DataType = System.Type.GetType("System.Decimal");
            dt.Columns.Add(DC3);

            DataColumn DCf = new DataColumn("F");
            DCf.DataType = System.Type.GetType("System.Decimal");
            dt.Columns.Add(DCf);

            DataRow myNewRow;

            for (int i = 0; i < dr.Count(); i++)
            {
                myNewRow = dt.NewRow();

                myNewRow["A1"] = dr[i][2];
                myNewRow["A2"] = dr[i][3];
                myNewRow["A3"] = dr[i][4];
                myNewRow["F"] = dr[i][5];
                dt.Rows.Add(myNewRow);
            }

            BindingSource bs = new BindingSource();
            bs.DataSource = dt;

            return bs;
        }

        /// <summary>
        /// Заполнение грида реперными точками функции
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void fillfTable(string nameALG)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            string m_fName = "N_ALG = '" + nameALG + "'";
            DataRow[] m_drValues = m_tblOrign.Select(m_fName);

            dgv.DataSource = createSource(m_drValues);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_tblOrign = m_tableEdit.Copy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="err"></param>
        protected override void recUpdateInsertDelete(ref DbConnection dbConn, out int err)
        {
            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, @"ftable", @"ID", m_tblOrign, m_tableEdit, out err);
        }

        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        /// <param name="iFunc"></param>
        public PluginPrjTepFTable(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        private void InitializeComponent()
        {
            DataGridView dgv = null;
            //Control ctrl = null;

            this.SuspendLayout();

            //Добавить кнопки
            INDEX_CONTROL i = INDEX_CONTROL.BUTTON_SAVE;

            for (i = INDEX_CONTROL.BUTTON_SAVE; i < (INDEX_CONTROL.BUTTON_UPDATE + 1); i++)
                addButton(i.ToString(), (int)i, m_arButtonText[(int)i]);

            //Поиск функции
            TextBox txtbx_find = new TextBox();
            txtbx_find.Name = INDEX_CONTROL.TEXTBOX_FIND.ToString();
            txtbx_find.Dock = DockStyle.Fill;

            //Подпись поиска
            System.Windows.Forms.Label lbl_find = new System.Windows.Forms.Label();
            lbl_find.Name = INDEX_CONTROL.LABEL_FIND.ToString();
            lbl_find.Dock = DockStyle.Bottom;
            (lbl_find as System.Windows.Forms.Label).Text = @"Поиск";

            //Группировка поиска 
            //и его подписи
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Name = INDEX_CONTROL.PANEL_FIND.ToString();
            tlp.Dock = DockStyle.Fill;
            tlp.AutoSize = true;
            tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlp.Controls.Add(lbl_find);
            tlp.Controls.Add(txtbx_find);
            this.Controls.Add(tlp, 1, 0);
            this.SetColumnSpan(tlp, 4);
            this.SetRowSpan(tlp, 1);

            //Таблица с функциями
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_algTABLE.ToString();
            i = INDEX_CONTROL.DGV_algTABLE;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 1);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 5);

            dgv.ReadOnly = true;
            //Запретить выделение "много" строк
            dgv.MultiSelect = false;
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            dgv.ColumnHeadersVisible = true;
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //Ширина столбцов под видимую область
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnCount = 2;
            dgv.Columns[0].Name = "Функция";
            dgv.Columns[1].Name = "Описание";

            //Таблица с реперными точками 
            dgv = new DataGridView();
            dgv.Name = INDEX_CONTROL.DGV_fTABLE.ToString();
            i = INDEX_CONTROL.DGV_fTABLE;
            dgv.Dock = DockStyle.Fill;
            //Разместить эл-т упр-я
            this.Controls.Add(dgv, 1, 6);
            this.SetColumnSpan(dgv, 4);
            this.SetRowSpan(dgv, 4);

            dgv.ReadOnly = true;
            //Запретить выделение "много" строк
            dgv.MultiSelect = false;
            //Установить режим выделения - "полная" строка
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //Установить режим "невидимые" заголовки столбцов
            dgv.ColumnHeadersVisible = true;
            //Отменить возможность добавления строк
            dgv.AllowUserToAddRows = false;
            //Отменить возможность удаления строк
            dgv.AllowUserToDeleteRows = false;
            //Отменить возможность изменения порядка следования столбцов строк
            dgv.AllowUserToOrderColumns = false;
            //Не отображать заголовки строк
            dgv.RowHeadersVisible = false;
            //Ширина столбцов под видимую область
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dgv.ColumnCount = 4;
            //dgv.Columns[0].Name = "A1";
            //dgv.Columns[1].Name = "A2";
            //dgv.Columns[2].Name = "A3";
            //dgv.Columns[3].Name = "F";
            //dgv.Columns[4].Visible = false;

<<<<<<< .mine
            //Панель отображения графика
            this.m_zGraph_fTABLE = new ZedGraph.ZedGraphControl();
=======
            //
            this.m_zGraph_fTABLE = new ZedGraph.ZedGraphControl();
>>>>>>> .r97
            m_zGraph_fTABLE.Name = INDEX_CONTROL.ZGRAPH_fTABLE.ToString();
            m_zGraph_fTABLE.Dock = DockStyle.Fill;
            this.m_zGraph_fTABLE.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(m_zGraph_fTABLE, 2, 0);
            this.SetColumnSpan(m_zGraph_fTABLE, 8);
            this.SetRowSpan(m_zGraph_fTABLE, 10);
            this.m_zGraph_fTABLE.ScrollGrace = 0;
            this.m_zGraph_fTABLE.ScrollMaxX = 0;
            this.m_zGraph_fTABLE.ScrollMaxY = 0;
            this.m_zGraph_fTABLE.ScrollMaxY2 = 0;
            this.m_zGraph_fTABLE.ScrollMinX = 0;
            this.m_zGraph_fTABLE.ScrollMinY = 0;
            this.m_zGraph_fTABLE.ScrollMinY2 = 0;
            this.m_zGraph_fTABLE.TabIndex = 0;
            this.m_zGraph_fTABLE.IsEnableHEdit = false;
            this.m_zGraph_fTABLE.IsEnableHPan = false;
            this.m_zGraph_fTABLE.IsEnableHZoom = false;
            this.m_zGraph_fTABLE.IsEnableSelection = false;
            this.m_zGraph_fTABLE.IsEnableVEdit = false;
            this.m_zGraph_fTABLE.IsEnableVPan = false;
            this.m_zGraph_fTABLE.IsEnableVZoom = false;
            this.m_zGraph_fTABLE.IsShowPointValues = true;

<<<<<<< .mine
            this.m_zGraph_fTABLE.ScrollGrace = 0;
            this.m_zGraph_fTABLE.ScrollMaxX = 0;
            this.m_zGraph_fTABLE.ScrollMaxY = 0;
            this.m_zGraph_fTABLE.ScrollMaxY2 = 0;
            this.m_zGraph_fTABLE.ScrollMinX = 0;
            this.m_zGraph_fTABLE.ScrollMinY = 0;
            this.m_zGraph_fTABLE.ScrollMinY2 = 0;
            this.m_zGraph_fTABLE.TabIndex = 0;
            this.m_zGraph_fTABLE.IsEnableHEdit = false;
            this.m_zGraph_fTABLE.IsEnableHPan = false;
            this.m_zGraph_fTABLE.IsEnableHZoom = true;
            this.m_zGraph_fTABLE.IsEnableSelection = false;
            this.m_zGraph_fTABLE.IsEnableVEdit = false;
            this.m_zGraph_fTABLE.IsEnableVPan = false;
            this.m_zGraph_fTABLE.IsEnableVZoom = false;
            this.m_zGraph_fTABLE.IsShowPointValues = true;

            //
            System.Windows.Forms.ComboBox cmb_bxParam = new ComboBox();
            cmb_bxParam.Name = INDEX_CONTROL.COMBOBOX_PARAM.ToString();
            cmb_bxParam.Dock = DockStyle.Fill;

            pane = m_zGraph_fTABLE.GraphPane;
            //Подписи для калькулятора
            System.Windows.Forms.Label lbl_A1 = new System.Windows.Forms.Label();
            lbl_A1.Dock = DockStyle.Bottom;
            (lbl_A1 as System.Windows.Forms.Label).Text = @"Значение A1";
            //
            System.Windows.Forms.Label lbl_A2 = new System.Windows.Forms.Label();
            lbl_A2.Dock = DockStyle.Bottom;
            (lbl_A2 as System.Windows.Forms.Label).Text = @"Значение A2";
            //
            System.Windows.Forms.Label lbl_A3 = new System.Windows.Forms.Label();
            lbl_A3.Dock = DockStyle.Bottom;
            (lbl_A3 as System.Windows.Forms.Label).Text = @"Значение A3";
            //
            System.Windows.Forms.Label lbl_rez = new System.Windows.Forms.Label();
            lbl_rez.Dock = DockStyle.Bottom;
            (lbl_rez as System.Windows.Forms.Label).Text = @"Результат";
            //
            System.Windows.Forms.Label lbl_F = new System.Windows.Forms.Label();
            lbl_F.Dock = DockStyle.Bottom;
            (lbl_F as System.Windows.Forms.Label).Text = @"Значение F";

            //Текстовые поля для данных калькулятора
            TextBox txtbx_A1 = new TextBox();
            txtbx_A1.Name = INDEX_CONTROL.TEXTBOX_A1.ToString();
            txtbx_A1.Dock = DockStyle.Fill;

            TextBox txtbx_A3 = new TextBox();
            txtbx_A3.Name = INDEX_CONTROL.TEXTBOX_A3.ToString();
            txtbx_A3.Dock = DockStyle.Fill;

            TextBox txtbx_A2 = new TextBox();
            txtbx_A2.Name = INDEX_CONTROL.TEXTBOX_A2.ToString();
            txtbx_A2.Dock = DockStyle.Fill;

            TextBox txtbx_F = new TextBox();
            txtbx_F.Name = INDEX_CONTROL.TEXTBOX_F.ToString();
            txtbx_F.Dock = DockStyle.Fill;

            TextBox txtbx_REZ = new TextBox();
            txtbx_REZ.Name = INDEX_CONTROL.TEXTBOX_REZULT.ToString();
            txtbx_REZ.Dock = DockStyle.Fill;
            txtbx_REZ.ReadOnly = true;

            Button btn_rez = new Button();
            btn_rez.Name = INDEX_CONTROL.BUTTON_CALC.ToString();
            btn_rez.Text = "REZ";
            btn_rez.Dock = DockStyle.Top;

            //Панель группировки калькулятора
            TableLayoutPanel tabl = new TableLayoutPanel();
            tabl.Name = INDEX_CONTROL.TABLELAYOUTPANEL_CALC.ToString();
            tabl.Dock = DockStyle.Fill;
            tabl.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            tabl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tabl.Controls.Add(lbl_A1, 0, 0);
            tabl.Controls.Add(lbl_A2, 1, 0);
            tabl.Controls.Add(lbl_A3, 2, 0);
            tabl.Controls.Add(lbl_F, 3, 0);
            tabl.Controls.Add(txtbx_A1, 0, 1);
            tabl.Controls.Add(txtbx_A2, 1, 1);
            tabl.Controls.Add(txtbx_A3, 2, 1);
            tabl.Controls.Add(txtbx_F, 3, 1);
            tabl.Controls.Add(lbl_rez, 0, 2);
            tabl.Controls.Add(txtbx_REZ, 0, 3);
            tabl.Controls.Add(btn_rez, 3, 3);
            tabl.SetColumnSpan(txtbx_REZ, 2);
            tabl.RowCount = 4;
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tabl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tabl.ColumnCount = 4;
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tabl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //
            GroupBox gpBoxCalc = new GroupBox();
            gpBoxCalc.Name = INDEX_CONTROL.GRPBOX_CALC.ToString();
            gpBoxCalc.Text = @"Калькулятор значений";
            gpBoxCalc.Dock = DockStyle.Fill;
            gpBoxCalc.Controls.Add(tabl);
            this.Controls.Add(gpBoxCalc, 0, 10);
            this.SetColumnSpan(gpBoxCalc, 5);
            this.SetRowSpan(gpBoxCalc, 3);
            //
=======
            //
            //m_chartGraph_fTABLE = new System.Windows.Forms.DataVisualization.Charting.Chart();
            //m_chartGraph_fTABLE.Name = INDEX_CONTROL.CHRTGRAPH_fTABLE.ToString();
            //m_chartGraph_fTABLE.Dock = DockStyle.Fill;
            //this.Controls.Add(m_chartGraph_fTABLE, 2, 0);
            //this.SetColumnSpan(m_chartGraph_fTABLE, 8);
            //this.SetRowSpan(m_chartGraph_fTABLE, 9);

>>>>>>> .r97
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());

            ResumeLayout(false);
            PerformLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTable_btnAdd_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTAble_btnDelete_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_CALC.ToString(), true)[0]).Click += new EventHandler(PluginPrjTepFTable_ClickRez);

            //Обработчики событий
<<<<<<< .mine
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]).CellContentClick += new DataGridViewCellEventHandler(PluginPrjTepAlgTable_CellContentClick);
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).CellMouseClick += new DataGridViewCellMouseEventHandler(PluginPrjTepFTable_CellContentClick);

=======
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]).CellContentClick += new DataGridViewCellEventHandler(PluginPrjTepAlgTable_CellContentClick);
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).CellMouseClick += new DataGridViewCellMouseEventHandler(PluginPrjTepFTable_CellContentClick);
>>>>>>> .r97
            ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).TextChanged += new EventHandler(PluginPrjTepFTable_TextChanged);
        }

        /// <summary>
<<<<<<< .mine
        /// Обрабоотка клика по кнопке результат
=======
        /// Событие изменения текстового поля
        /// (функция поиска)
>>>>>>> .r97
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PluginPrjTepFTable_ClickRez(object sender, EventArgs e)
        {
            string text = ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_A1.ToString(), true)[0]).Text;
            int m_indx = nameALG.IndexOf(":");
            int countArg = Convert.ToInt32(nameALG[m_indx + 1].ToString());

            ArgApproxi = new string[countArg + 1];

            //ArgApproxi.SetValue(((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_A1.ToString(), true)[0]).Text, 0);
            //ArgApproxi.SetValue(((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_A2.ToString(), true)[0]).Text, 1);
            //ArgApproxi.SetValue(((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_A3.ToString(), true)[0]).Text, 2);
            //ArgApproxi.SetValue(((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_F.ToString(), true)[0]).Text, 3);

            selectFuncArg(countArg);
        }

        /// <summary>
        /// Проверка кол-ва парметров (для калькулятора)
        /// </summary>
        /// <param name="countArg">кол-во парамтеров</param>
        private void selectFuncArg(int countArg)
        {
            string colArg = "A" + countArg;

            switch (colArg)
            {
                case "A1":

                    break;
                case "A2":

                    break;
                case "A3":

                    break;
                default:
                    MessageBox.Show("MZF");
                    break;
            }
        }

        /// <summary>
        /// Калькулятор значений(в разработке)
        /// </summary>
        /// <param name="countArg"></param>
        private void calc_proc(int countArg)
        {
            double m_y1;
            double m_y2;

            for (int i = 0; i < countArg; i++)
            {
                if (true)
                {

                }

                if (!(countArg > 1))
                {

                }
            }

            //for (int i = 0; i < length; i++)
            //{

            //}
        }

        /// <summary>
        /// Событие изменения текстового поля
        /// (функция поиска)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginPrjTepFTable_TextChanged(object sender, EventArgs e)
        {
            string text = ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).Text;

            m_findALG(text);
        }

        /// <summary>
        /// Щелчек по ячейки с функцией
        /// </summary>
        /// <param name="sender">объект</param>
        /// <param name="e">событие</param>
        private void PluginPrjTepAlgTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);
<<<<<<< .mine
            nameALG = dgv.Rows[e.RowIndex].Cells["Функция"].Value.ToString();
            int m_indx = nameALG.IndexOf(":");
            string m_nameColumn = "A" + nameALG[m_indx + 1].ToString();
=======
            nameALG = dgv.Rows[e.RowIndex].Cells["Функция"].Value.ToString();
            createParamMassive(nameALG);
            showprmFunc(nameALG);
        }
>>>>>>> .r97

<<<<<<< .mine
            fillfTable(nameALG);
            addColumnsInGrid(m_nameColumn);
=======
        /// <summary>
        /// Обработка выбора аргумента функции
        /// </summary>
        /// <param name="sender">объект</param>
        /// <param name="e">событие</param>
        private void PluginPrjTepFTable_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            string m_valueCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            referencePoint = m_valueCell;

            checkAmountArg(nameALG, e.RowIndex);
>>>>>>> .r97
        }

        /// <summary>
        /// Обработка клика по таблице со значениями.
        /// Изменение чекбокса, построение графика.
        /// </summary>
        /// <param name="sender">объект</param>
        /// <param name="e">событие</param>
        private void PluginPrjTepFTable_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);

            if (e.RowIndex > -1)
            {
                if (dgv.Columns[e.ColumnIndex].Name == "check")
                {
                    DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (chk.Value == chk.FalseValue || chk.Value == null)
                    {
                        chk.TrueValue = true;
                        chk.Value = chk.TrueValue;
                        this.Update();
                    }
                    else
                    {
                        chk.Value = chk.FalseValue;
                        this.Update();

                    }
                    dgv.EndEdit();
                }
                else
                {
                    int m_indx = nameALG.IndexOf(":");
                    string m_nameColumn = "A" + nameALG[m_indx + 1].ToString();

                    createParamMassive(nameALG, m_nameColumn, e.RowIndex);
                    //referencePoint = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    checkAmountArg(e.RowIndex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTable_btnAdd_Click(object obj, EventArgs ev)
        {
            //DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            //dgv.Rows[dgv.NewRowIndex].Cells[0].Selected = true;
            //dgv.BeginEdit(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTAble_btnDelete_Click(object obj, EventArgs ev)
        {
            //DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0] as DataGridView;

            //int indx = dgv.SelectedRows[0].Index;

            //if ((!(indx < 0)) && (indx < m_tableEdit.Rows.Count))
            //{//Удаление существующей записи
            //    delRecItem(indx);

<<<<<<< .mine
            //    dgv.Rows.RemoveAt(indx);
            //}
=======
                dgv.Rows.RemoveAt(indx);
            }
>>>>>>> .r97
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indx"></param>
        protected void delRecItem(int indx)
        {
            m_tableEdit.Rows[indx].Delete();
            m_tableEdit.AcceptChanges();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 16;

            _nameOwnerMenuItem = @"Проект";
            _nameMenuItem = @"Нормативные графики";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PluginPrjTepFTable));

            base.OnClickMenuItem(obj, ev);
        }
    }
}