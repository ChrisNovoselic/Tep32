using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using ZedGraph;

namespace PluginPrjTepFTable
{
    /// <summary>
    /// Класс - общий для графического представления значений
    /// </summary>
    public class ZedGraphFTable : FTable
    {
        /// <summary>
        /// Набор цветов для гарфиков
        /// </summary>
        protected static Color[] m_colorLine = { Color.Red, Color.Green, Color.Blue, Color.Navy, Color.Teal,
                                                 Color.Black,  Color.PeachPuff, Color.MediumVioletRed,
                                                 Color.SandyBrown, Color.ForestGreen, Color.DarkGreen,
                                                 Color.BlueViolet, Color.Plum, Color.YellowGreen,
                                                 Color.Moccasin, Color.DarkTurquoise,Color.Maroon};
        public ZedGraphControl m_This;

        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public ZedGraphFTable()
            : base()
        {
            m_This = new ZedGraphControl();
            
            initializeComponent();
        }

        PointPairList []pointList;

        /// <summary>
        /// Инициализация собственных компонентов элемента управления
        /// </summary>
        private void initializeComponent()
        {
            m_This.ScrollGrace = 0;
            m_This.ScrollMaxX = 0;
            m_This.ScrollMaxY = 0;
            m_This.ScrollMaxY2 = 0;
            m_This.ScrollMinX = 0;
            m_This.ScrollMinY = 0;
            m_This.ScrollMinY2 = 0;
            m_This.TabIndex = 0;
            m_This.IsEnableHEdit = false;
            m_This.IsEnableHPan = false;
            m_This.IsEnableHZoom = false;
            m_This.IsEnableSelection = false;
            m_This.IsEnableVEdit = false;
            m_This.IsEnableVPan = false;
            m_This.IsEnableVZoom = false;
            m_This.IsShowPointValues = true;

            m_This.ScrollGrace = 0;
            m_This.ScrollMaxX = 0;
            m_This.ScrollMaxY = 0;
            m_This.ScrollMaxY2 = 0;
            m_This.ScrollMinX = 0;
            m_This.ScrollMinY = 0;
            m_This.ScrollMinY2 = 0;
            m_This.TabIndex = 0;
            m_This.IsEnableHEdit = false;
            m_This.IsEnableHPan = false;
            m_This.IsEnableHZoom = true;
            m_This.IsEnableSelection = false;
            m_This.IsEnableVEdit = false;
            m_This.IsEnableVPan = false;
            m_This.IsEnableVZoom = false;
            m_This.IsShowPointValues = true;
        }

        /// <summary>
        /// Отображение графика функции
        /// по аргументу
        /// </summary>
        /// <param name="listP">набор координат</param>
        public void Draw(PointPairList listP, string nameAlg, double referencePoint)
        {
            GraphPane pane = new GraphPane();
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            LineItem m_LineItemCure = pane.AddCurve("NAME FUNC" + referencePoint + "", listP, Color.Black, SymbolType.VDash);
            m_LineItemCure.Label.Text = "" + nameAlg + "";

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
            m_This.AxisChange();

            // !!! Установим значение параметра IsBoundedRanges как true.
            // !!! Это означает, что при автоматическом подборе масштаба 
            // !!! нужно учитывать только видимый интервал графика
            pane.IsBoundedRanges = true;
            // Обновляем график
            m_This.Invalidate();
            m_This.Refresh();
        }

        /// <summary>
        /// Формированре масива 
        /// значений для графика функции
        /// по статичной переменной
        /// </summary>
        /// <param name="nameCol">кол-во переменных</param>
        /// <param name="row">номер строки</param>
        private void formingArrayValues(string strQuery)
        {
            //DataRow[] ftTableF = m_tblEdit.Select(strQuery.Replace(",", "."));

            //PointPairList points = new PointPairList();

            //for (int i = 0; i < ftTableF.Length; i++)
            //{
            //    points.Add(Convert.ToDouble(ftTableF[i]["A1"]), Convert.ToDouble(ftTableF[i]["F"]));
            //}

            //drawGraph(points);
        }

        /// <summary>
        /// Формированре масива 
        /// значений для графика функции
        /// для всех значений
        /// </summary>
        /// <param name="nameCol">имя столбца</param>
        private void sampleValues(string nameCol)
        {
            string strQuery = string.Empty;
            int countList = -1;
            string elem;
            ////??? без таблицы, использовать m_dictValues
            //var enValues = (from r in m_tblEdit.AsEnumerable()
            //                where r.Field<string>("N_ALG") == m_nameAlg
            //                select new
            //                {
            //                    nameCol = r.Field<float>(nameCol),
            //                }).Distinct();

            if (!(nameCol == "A3"))
            {                
                //countList = enValues.Count();
                //pointList = new PointPairList[countList];                

                //for (int i = 0; i < enValues.Count(); i++)
                //{

                //    elem = enValues.ElementAt(i).nameCol.ToString().Replace(",", ".");
                //    strQuery = " N_ALG = " + "'" + m_nameAlg + "'and " + nameCol + " = " + elem + "";

                //    ftTableF = m_tblEdit.Select(strQuery);

                //    createPointLists(i, ftTableF);
                //}
            }
            else
            {
                //var enValuesA2 = (from r in m_tblEdit.AsEnumerable()
                //                  where r.Field<string>("N_ALG") == m_nameAlg
                //                  select new
                //                  {
                //                      nameCol = r.Field<float>("A2"),

                //                  }).Distinct();

                //countList = enValues.Count() * enValuesA2.Count();

                pointList = new PointPairList[countList];
                int num = 0;
                string m_ftValue = DataGridViewAutoFilterColumnHeaderCell.FilterValue();

                elem = m_ftValue.ToString().Replace(",", ".");

                //for (int i = 0; i < enValuesA2.Count(); i++)
                //{
                //    strQuery = " N_ALG = " + "'" + m_nameAlg + "' and " + nameCol + " = " + elem + " and A2= " + enValuesA2.ElementAt(i).nameCol.ToString().Replace(",", ".") + "";
                //    //??? без таблицы, использовать m_dictValues
                //    //ftTableF = m_tblEdit.Select(strQuery);

                //    //if (!(ftTableF.Count() == 0))
                //    //{
                //    //    createPointLists(num, ftTableF);
                //    //    num++;
                //    //}
                //    //else
                //    //    ;
                //}
            }

            createGraphs();
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с одним параметром
        /// </summary>
        /// <param name="colCount">кол-во аргументов</param>
        protected override void funcWithOneArgs(string nameCol)
        {
            //formingArrayValues(filter);
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с двумя параметрами
        /// </summary>
        /// <param name="nameCol"></param>
        /// <param name="filter"></param>
        protected void funcWithTwoArgs(string filter)
        {
            formingArrayValues(filter);
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с тремя парметрами
        /// </summary>
        /// <param name="filter"></param>
        protected void funcWithThreeArgs(string filter)
        {
            formingArrayValues(filter);
        }

        /// <summary>
        /// Создание наборов точек
        /// </summary>
        /// <param name="i">номер листа</param>
        /// <param name="array">массив с точками</param>
        private void createPointLists(int i, ListPOINT array)
        {
            if (pointList[i] == null)
                pointList[i] = new PointPairList();
            else
                ;

            for (int j = 0; j < array.Count(); j++)
            {
                //Заполняем список точками          
                pointList[i].Add(array[i].a1, array[i].f);
            }
        }

        /// <summary>
        /// Проверка на кол-во аргументов функции(отображение графиков)
        /// </summary>
        /// <param name="iRow">номер строки - имя функции</param>
        public void CheckAmountArg(int iRow)
        {
            int indx = m_nameAlg.IndexOf(":");
            string nameColumn = "A" + m_nameAlg[indx + 1].ToString();
            string filter = "N_ALG = '" + m_nameAlg + "'";

            switch (nameColumn)
            {
                case "A1":
                    condition = true;
                    //funcWithOneArgs(nameColumn);
                    break;
                case "A2":
                    condition = true;
                    funcWithTwoArgs(getQueryToGraphic(iRow));
                    break;
                case "A3":
                    condition = true;
                    funcWithThreeArgs(getQueryToGraphic(iRow));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Формирование строки запроса данных
        /// для построения графика
        /// </summary>
        /// <param name="row">номер строки</param>
        /// <returns>строка запроса к таблице</returns>
        private string getQueryToGraphic(int row)
        {
            int indx = m_nameAlg.IndexOf(":");
            string nameColumn = "A" + m_nameAlg[indx + 1].ToString();
            string filter = "N_ALG = '" + m_nameAlg + "'";
            string searchValn = 
                //((DataGridView)Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0]).Rows[row].Cells[nameColumn].Value.ToString()
                string.Empty;
            string searchVal2 = string.Empty;

            if (nameColumn == "A3")
                searchVal2 =
                    //((DataGridView)Controls.Find(INDEX_CONTROL.DGV_VALUES.ToString(), true)[0]).Rows[row].Cells["A2"].Value.ToString()
                    string.Empty
                    ;
            else
                ;

            if (nameColumn == "A2")
            {
                filter += " and " + nameColumn + " = " + searchValn.Replace(",", ".") + "";
            }
            else
            {
                filter += " and " + nameColumn + " = " + searchValn.Replace(",", ".") + " and A2 =" + searchVal2.Replace(",", ".") + "";
            }

            return filter;
        }

        /// <summary>
        /// Создание графика на основе всех точек
        /// </summary>
        /// <param name="countListPoint">кол-во графиков</param>
        private void createGraphs()
        {
            LineItem[] myCurves = new LineItem[pointList.Count()];

            for (int i = 0; i < pointList.Count(); i++)
            {
                if (!(pointList[i] == null))
                {
                    myCurves[i] = new LineItem("" + i + "");
                    myCurves[i] = m_This.GraphPane.AddCurve("NAME FUNC", pointList[i], m_colorLine.ElementAt(i), SymbolType.VDash);
                    myCurves[i].Label.Text = "" + m_nameAlg + "";
                }
            }

            m_This.GraphPane.XAxis.Scale.MinAuto = true;
            m_This.GraphPane.XAxis.Scale.MaxAuto = true;

            // По оси Y установим автоматический подбор масштаба
            m_This.GraphPane.YAxis.Scale.MinAuto = true;
            m_This.GraphPane.YAxis.Scale.MaxAuto = true;

            m_This.GraphPane.YAxis.MajorGrid.IsZeroLine = false;
            // !!!
            // Устанавливаем интересующий нас интервал по оси Y
            //pane.YAxis.Scale.Min = metka1;
            // pane.YAxis.Scale.Max = metka2;

            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            m_This.GraphPane.AxisChange();
            m_This.AxisChange();

            // !!! Установим значение параметра IsBoundedRanges как true.
            // !!! Это означает, что при автоматическом подборе масштаба 
            // !!! нужно учитывать только видимый интервал графика
            m_This.GraphPane.IsBoundedRanges = true;
            // Обновляем график
            m_This.Invalidate();
            m_This.Refresh();
        }

        /// <summary>
        /// Формирование списка точек
        /// </summary>
        /// <param name="querry">запрос на выборку данных</param>
        /// <returns></returns>
        private PointPairList FillPointList(string where)
        {
            //DataRow[] ftTableF = m_tblEdit.Select(where);

            PointPairList pointlist = new PointPairList();

            //for (int i = 0; i < ftTableF.Length; i++)
            //    pointlist.Add(Convert.ToDouble(ftTableF[i]["A1"]), Convert.ToDouble(ftTableF[i]["F"]));

            return pointlist;
        }
    }
}
