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
using System.Windows.Forms.DataVisualization.Charting;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginPrjTepFTable
{
    public class PluginPrjTepFTable : HPanelTepCommon
    {
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


        protected static Color[] m_colorLine = { Color.Red, Color.Green, Color.Blue, Color.Black, Color.PeachPuff, Color.Khaki, Color.PaleGoldenrod };
        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_CONTROL
        {
            BUTTON_SAVE, BUTTON_DELETE,
            BUTTON_ADD, BUTTON_UPDATE,
            DGV_fTABLE, DGV_algTABLE,
            LABEL_DESC, INDEX_CONTROL_COUNT,
            ZGRAPH_fTABLE, CHRTGRAPH_fTABLE,
            TEXTBOX_FIND, LABEL_FIND, PANEL_FIND
        };

        /// <summary>
        /// 
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
        /// 
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

            checkNumberRows(m_tableEdit.Rows.Count, dgv);

            for (int i = 0; i < m_tableEdit.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_tableEdit.Rows[i]["N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_tableEdit.Rows[i]["DESCRIPTION"];
            }
        }

        /// <summary>
        /// Заполняет значениями функции
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void createParamMassive(string nameALG)
        {
            string m_fName = "N_ALG = '" + nameALG + "'";
            ValuesFunc = m_tblOrign.Select(m_fName);
        }

        /// <summary>
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
        /// </summary>
        /// <param name="count">кол-во строк/param>
        /// <param name="dgv">датагрид</param>
        private void checkNumberRows(int count, DataGridView dgv)
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
        /// Функция поиска
        /// </summary>
        /// <param name="text">искомый элемент</param>
        private void m_findALG(string text)
        {
            string m_fltr = string.Format("{0} like '{1}%'", new object[] { "N_ALG", text });
            DataRow[] m_drSearch = m_tableEdit.Select(m_fltr);
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]);

            checkNumberRows(m_drSearch.Count(), dgv);

            for (int i = 0; i < m_drSearch.Count(); i++)
            {
                dgv.Rows[i].Cells["Функция"].Value = m_drSearch[i][@"N_ALG"];
                dgv.Rows[i].Cells["Описание"].Value = m_drSearch[i][@"DESCRIPTION"];
            }
        }

        /// <summary>
        /// Отображение реперных точек функции
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        private void showprmFunc(string nameALG)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            string m_fName = "N_ALG = '" + nameALG + "'";
            DataRow[] m_drValues = m_tblOrign.Select(m_fName);

            checkNumberRows(m_drValues.Count(), dgv);

            for (int i = 0; i < m_drValues.Count(); i++)
            {
                dgv.Rows[i].Cells["A1"].Value = m_drValues[i][2];
                dgv.Rows[i].Cells["A2"].Value = m_drValues[i][3];
                dgv.Rows[i].Cells["A3"].Value = m_drValues[i][4];
                dgv.Rows[i].Cells["F"].Value = m_drValues[i][5];
            }
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
            dgv.ColumnCount = 4;
            dgv.Columns[0].Name = "A1";
            dgv.Columns[1].Name = "A2";
            dgv.Columns[2].Name = "A3";
            dgv.Columns[3].Name = "F";

            //
            this.m_zGraph_fTABLE = new ZedGraph.ZedGraphControl();
            m_zGraph_fTABLE.Name = INDEX_CONTROL.ZGRAPH_fTABLE.ToString();
            m_zGraph_fTABLE.Dock = DockStyle.Fill;
            this.m_zGraph_fTABLE.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(m_zGraph_fTABLE, 2, 0);
            this.SetColumnSpan(m_zGraph_fTABLE, 8);
            this.SetRowSpan(m_zGraph_fTABLE, 9);
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

            //
            //m_chartGraph_fTABLE = new System.Windows.Forms.DataVisualization.Charting.Chart();
            //m_chartGraph_fTABLE.Name = INDEX_CONTROL.CHRTGRAPH_fTABLE.ToString();
            //m_chartGraph_fTABLE.Dock = DockStyle.Fill;
            //this.Controls.Add(m_chartGraph_fTABLE, 2, 0);
            //this.SetColumnSpan(m_chartGraph_fTABLE, 8);
            //this.SetRowSpan(m_chartGraph_fTABLE, 9);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());

            ResumeLayout(false);
            PerformLayout();

            //Обработчика нажатия кнопок
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_ADD.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTable_btnAdd_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_DELETE.ToString(), true)[0]).Click += new System.EventHandler(HPanelfTAble_btnDelete_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnSave_Click);
            ((Button)Controls.Find(INDEX_CONTROL.BUTTON_UPDATE.ToString(), true)[0]).Click += new System.EventHandler(HPanelTepCommon_btnUpdate_Click);
            //Обработчики событий
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_algTABLE.ToString(), true)[0]).CellContentClick += new DataGridViewCellEventHandler(PluginPrjTepAlgTable_CellContentClick);
            ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]).CellMouseClick += new DataGridViewCellMouseEventHandler(PluginPrjTepFTable_CellContentClick);
            ((TextBox)Controls.Find(INDEX_CONTROL.TEXTBOX_FIND.ToString(), true)[0]).TextChanged += new EventHandler(PluginPrjTepFTable_TextChanged);
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
            nameALG = dgv.Rows[e.RowIndex].Cells["Функция"].Value.ToString();
            createParamMassive(nameALG);
            showprmFunc(nameALG);
        }

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTable_btnAdd_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0]);
            dgv.Rows[dgv.NewRowIndex].Cells[0].Selected = true;
            dgv.BeginEdit(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void HPanelfTAble_btnDelete_Click(object obj, EventArgs ev)
        {
            DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_fTABLE.ToString(), true)[0] as DataGridView;

            int indx = dgv.SelectedRows[0].Index;

            if ((!(indx < 0)) && (indx < m_tableEdit.Rows.Count))
            {//Удаление существующей записи
                delRecItem(indx);

                dgv.Rows.RemoveAt(indx);
            }
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
