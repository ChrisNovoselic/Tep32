using System;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using TepCommon;
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
        protected static Color[] s_colorLineItem = { Color.Red, Color.Green, Color.Blue, Color.Navy, Color.Teal,
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

            GraphPane pane = m_This.GraphPane;
            //По оси X установим автоматический подбор масштаба
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;
            //По оси Y установим автоматический подбор масштаба
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;
            // !!!
            pane.YAxis.MajorGrid.IsZeroLine = true;

            // Включаем отображение сетки напротив крупных рисок по оси X
            pane.XAxis.MajorGrid.IsVisible = true;
            // Задаем вид пунктирной линии для крупных рисок по оси X:
            // Длина штрихов равна 10 пикселям, ... 
            pane.XAxis.MajorGrid.DashOn = 10;
            // затем 5 пикселей - пропуск
            pane.XAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.XAxis.MajorGrid.PenWidth = 0.1F;
            pane.XAxis.MajorGrid.Color = Color.LightGray;
            // Включаем отображение сетки напротив мелких рисок по оси X
            pane.XAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.XAxis.MinorGrid.DashOn = 1;
            pane.XAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.XAxis.MinorGrid.PenWidth = 0.1F;
            pane.XAxis.MinorGrid.Color = Color.LightGray;

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = Color.LightGray;
            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = Color.LightGray;
        }
        /// <summary>
        /// Класс для отображения одной линии
        /// </summary>
        private class PointPairList : ZedGraph.PointPairList
        {
            /// <summary>
            /// Ключ для линии
            /// </summary>
            private POINT key;
            /// <summary>
            /// Ранг (порядок) линии
            /// </summary>
            private FRUNK fRunk;
            /// <summary>
            /// Конструктор - основной
            /// </summary>
            /// <param name="key">Ключ для линии</param>
            /// <param name="fRunk">Ранг (порядок) линии</param>
            public PointPairList(POINT key, FRUNK fRunk)
            {
                this.key = key;
                this.fRunk = fRunk;
            }
            /// <summary>
            /// Возвратить признак принадлежности точки к линии по ключу
            /// </summary>
            /// <param name="pt">Точка для проверки принадлежности к линии</param>
            /// <returns>Признак принадлежности</returns>
            public bool ContainsKey(POINT pt)
            {
                bool bRes = false;
                //Сравнить ключ и значения аргументов точки
                switch (fRunk)
                {
                    case FRUNK.F1:
                        bRes = true;
                        break;
                    case FRUNK.F2:
                        bRes = pt.a2 == key.a2;
                        break;
                    case FRUNK.F3:
                        bRes = (pt.a2 == key.a2) && (pt.a3 == key.a3);
                        break;
                    default:
                        break;
                }

                return bRes;
            }
            /// <summary>
            /// Возвратить подпись к линии
            /// </summary>
            /// <returns>Подпись для линии</returns>
            public string GetLabel()
            {
                string strRes = string.Empty;

                switch (fRunk)
                {
                    case FRUNK.F1:
                        break;
                    case FRUNK.F2:
                        strRes = @"(" + key.a2.ToString (@"F1") + @")";
                        break;
                    case FRUNK.F3:
                        strRes = @"(" + key.a2.ToString(@"F1", CultureInfo.InvariantCulture) + @";"
                            + key.a3.ToString(@"F1", CultureInfo.InvariantCulture) + @")";
                        break;
                    default:
                        break;
                }

                return strRes;
            }
        }
        /// <summary>
        /// Возвратить все аргументы функции для указанного ранга
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <returns>Массив аргументов функции</returns>
        private List<PointPairList> getPointPairListValues(string nameAlg)
        {
            FRUNK fRunk = m_dictValues[nameAlg].Runk;
            List<PointPairList> listRes = new List<PointPairList> ();
            PointPairList item = null;
            bool bNewItem = false;

            foreach (POINT pt in m_dictValues[nameAlg])
            {
                bNewItem = true;

                if (listRes.Count == 0)
                    ;
                else
                    foreach (PointPairList ppl in listRes)
                        if (ppl.ContainsKey(pt) == true)
                        {
                            item = ppl;
                            bNewItem = false;
                        }
                        else
                            ;

                if (bNewItem == true)
                {
                    item = new PointPairList(pt, fRunk);
                    listRes.Add(item);
                }
                else
                    ;

                if (!(item == null))
                    item.Add(new PointPair(pt.X(FRUNK.F1), pt.f));
                else
                    ;
            }

            return listRes;
        }
        /// <summary>
        /// Добавить все линии
        /// </summary>
        /// <param name="pane">Панль для отображения</param>
        /// <param name="nameAlg">Наименование функции</param>
        private void addCurves(GraphPane pane, string nameAlg)
        {
            List<PointPairList> listPointPairList = getPointPairListValues (nameAlg);
            LineItem lineItemCurve;
            Color clrLineItemCurve;
            string labelItemCurve = string.Empty;

            foreach (PointPairList item in listPointPairList)
            {
                labelItemCurve = item.GetLabel ();
                if (labelItemCurve.Equals(string.Empty) == true)
                    labelItemCurve = nameAlg;
                else
                    ;

                clrLineItemCurve = s_colorLineItem[listPointPairList.IndexOf(item) % (s_colorLineItem.Length)];
                lineItemCurve = pane.AddCurve(labelItemCurve, item, clrLineItemCurve, SymbolType.VDash);
            }
        }
        /// <summary>
        /// Отображение графика функции
        ///  по аргументу
        /// </summary>
        /// <param name="listP">набор координат</param>
        public void Draw(string nameAlg)
        {
            GraphPane pane = m_This.GraphPane;
            //Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            addCurves(pane, nameAlg);

            pane.Title.Text = nameAlg;

            //Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // в противном случае на рисунке будет показана только часть графика
            // , которая умещается в интервалы по осям, установленные по умолчанию
            pane.AxisChange();
            m_This.AxisChange();

            //!!! Установим значение параметра IsBoundedRanges как true.
            //!!!  это означает, что при автоматическом подборе масштаба 
            //!!!  нужно учитывать только видимый интервал графика
            pane.IsBoundedRanges = true;
            //Обновляем график
            m_This.Invalidate();
            m_This.Refresh();
        }
    }
}
