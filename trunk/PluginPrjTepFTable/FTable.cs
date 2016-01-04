using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using System.Drawing;

using HClassLibrary;

namespace PluginPrjTepFTable
{
    // http://pmpu.ru/vf4/interpolation
    // https://ru.wikibooks.org/wiki/Реализации_алгоритмов/Интерполяция/Многочлен_Лагранжа
    public class FTable
    {        
        private enum INDEX_NEAR { UNKNOWN = -1
            , LEFT, RIGHT
            , COUNT }
        /// <summary>
        /// Перечисление для обозночения уровня точки, функции
        /// </summary>
        public enum FRUNK { UNKNOWN = -1
            , F1, F2, F3
            , COUNT }
        /// <summary>
        /// Структура для хранения значений для одной из точек функции
        /// </summary>
        protected struct POINT
        {
            /// <summary>
            /// Уровень для точки
            /// </summary>
            public FRUNK Runk;
            /// <summary>
            /// Идентификатор строки в таблице БД
            /// </summary>
            public int m_idRec;
            /// <summary>
            /// Значение для точки
            /// </summary>
            public float a1
                , a2
                , a3
                , f;
            /// <summary>
            /// Конструктор основной (с параметрами)
            /// </summary>
            /// <param name="id">Идентификатор записи в таблице БД</param>
            /// <param name="a1">1-ый аргумент</param>
            /// <param name="a2">2-ой аргумент</param>
            /// <param name="a3">3-ий аргумент</param>
            /// <param name="f">Значение функции для аргументов</param>
            public POINT(int id, float a1, float a2, float a3, float f)
            {
                m_idRec = id;
                this.a1 = a1;
                this.a2 = a2;
                this.a3 = a3;
                this.f = f;

                Runk = ((!(a2 == 0F)) || (!(a3 == 0F))) ? (!(a3 == 0F)) ? FRUNK.F3 : (!(a2 == 0F)) ? FRUNK.F2 : FRUNK.F1 : FRUNK.F1;
            }

            public float X(FRUNK fRunk)
            {
                float fRes = float.NaN;

                switch (fRunk)
                {
                    case FRUNK.F1:
                        fRes = a1;
                        break;
                    case FRUNK.F2:
                        fRes = a2;
                        break;
                    case FRUNK.F3:
                        fRes = a3;
                        break;
                    default:
                        break;
                }

                return fRes;
            }
        }
        /// <summary>
        /// Структура для хранения параметров при проверке условия
        ///  использования точки функции при поиске границ диапазона в 'getNearby'
        /// </summary>
        private struct LIMIT
        {
            public FRUNK fRunk;
            public float x;
        }
        /// <summary>
        /// Класс для хранения списка всех значений одной функции
        /// </summary>
        protected class ListPOINT : List<POINT>
        {
            /// <summary>
            /// Уровень функции
            /// </summary>
            public FRUNK Runk
            {
                get
                {
                    FRUNK runkRes = FRUNK.F1;

                    foreach (POINT p in this)
                        if (runkRes < p.Runk)
                        {
                            runkRes = p.Runk;

                            if (runkRes == FRUNK.F3)
                                break;
                            else
                                ;
                        }
                        else
                            ;

                    return runkRes;
                }
            }
        }
        /// <summary>
        /// Класс для хранения значений диапазона ближайших точек
        /// </summary>
        protected class RangePOINT
        {
            /// <summary>
            /// Массив точек диапазона
            /// </summary>
            private POINT[] _array;
            /// <summary>
            /// Конструктор - дополнительный (без параметров)
            /// </summary>
            public RangePOINT()
            {
                _array = new POINT[(int)INDEX_NEAR.COUNT];
            }
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="array">Массив точек для инициализации диапазона</param>
            public RangePOINT(POINT[] array) : this ()
            {
                array.CopyTo(_array, 0);
            }
            /// <summary>
            /// Точка "слева"
            /// </summary>
            public POINT Left
            {
                get { return _array[(int)INDEX_NEAR.LEFT]; }

                set { _array[(int)INDEX_NEAR.LEFT] = value; }
            }
            /// <summary>
            /// Точка "справа"
            /// </summary>
            public POINT Right
            {
                get { return _array[(int)INDEX_NEAR.RIGHT]; }

                set { _array[(int)INDEX_NEAR.RIGHT] = value; }
            }
        }
        /// <summary>
        /// Класс для хранения списка объектов с параметрами при проверке условиия
        ///  использования точки функции при поиске границ диапазона в 'getNearby'
        /// </summary>
        private class ListLIMIT : List<LIMIT>
        {
            public bool ContansPoint(POINT pt)
            {
                bool bRes = false;

                foreach (LIMIT lim in this)
                    if (pt.X(lim.fRunk) == lim.x)
                    {
                        bRes = true;
                        break;
                    }
                    else
                        ;

                return bRes;
            }
        }
        /// <summary>
        /// Словарь со значениями для всех функций
        ///  (ключ - наименование функции)
        /// </summary>
        protected Dictionary<string, ListPOINT> m_dictValues;
        /// <summary>
        /// Составное наименование функции
        ///  имя_функции + наименование столбца
        /// </summary>
        protected string m_nameAlg;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FTable()
        {
            m_dictValues = new Dictionary<string, ListPOINT>();
        }
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="src">Таблица в БД - источник данных</param>
        public FTable(DataTable src)
            : this()
        {
            Set(src);
        }
        /// <summary>
        /// Ранг функции
        /// </summary>
        /// <param name="nAlg">Наименование функции</param>
        /// <returns>Ранг функции (кол-во аргументов)</returns>
        private FRUNK _fRunk { get { return m_dictValues.ContainsKey(m_nameAlg) == true ? m_dictValues[m_nameAlg].Runk : FRUNK.UNKNOWN; } }
        /// <summary>
        /// Возвратить ранг функции по имени
        /// </summary>
        /// <param name="nAlg">Наименование функции</param>
        /// <returns>Ранг функции (кол-во аргументов)</returns>
        public FRUNK GetRunk(string nAlg) { return m_dictValues[nAlg].Runk; }
        /// <summary>
        /// Установить значения всех функций
        /// </summary>
        /// <param name="src">Таблица в БД - источник данных</param>
        public void Set(DataTable src)
        {
            string nAlg = string.Empty;

            m_dictValues.Clear();

            foreach (DataRow r in src.Rows)
            {
                nAlg = ((string)r[@"N_ALG"]).Trim();

                if (m_dictValues.Keys.Contains(nAlg) == false)
                    m_dictValues.Add(nAlg, new ListPOINT());
                else
                    ;

                m_dictValues[nAlg].Add(new POINT((int)r[@"ID"]
                    , (float)r[@"A1"]
                    , (float)r[@"A2"]
                    , (float)r[@"A3"]
                    , (float)r[@"F"]));
            }
        }
        /// <summary>
        /// Найти ближайшие реперные (узловые) точки
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <param name="x">Аргумент функции, относительно которой производится поиск</param>
        /// <param name="fRunk">Ранг аргумента</param>
        /// <returns>Массив точек</returns>
        private RangePOINT getNearby(string nameAlg, float x, FRUNK fRunk, ListLIMIT listLimit)
        {
            RangePOINT rangeRes = null;
            INDEX_NEAR indxNearby = INDEX_NEAR.UNKNOWN;
            //Получить диапазон аргументов, значений
            rangeRes = getRange(nameAlg, fRunk, listLimit);
            //Определитть порядок назначения ближайших соседних реперных точек
            if ((!(x < rangeRes.Left.X(fRunk)))
                && (!(x > rangeRes.Right.X(fRunk))))
            // точка внутри диапазона - использовать интерполяцию
                indxNearby = INDEX_NEAR.COUNT;
            else
            // точка вне диапазона
                if (x < rangeRes.Left.X(fRunk))
                // точка "слева" от диапазона - требуется уточнение правой границы
                    indxNearby = INDEX_NEAR.LEFT; // левая - не изменяется
                else
                    if (x > rangeRes.Right.X(fRunk))
                    // точка "справа" от диапазона - требуется уточнение левой границы
                        indxNearby = INDEX_NEAR.RIGHT; // правая - не изменяется
                    else
                        ;
            //Назначить ближайшие соседние реперные точки
            if (indxNearby == INDEX_NEAR.COUNT)
            // внутри диапазона
                interpolation(nameAlg, x, fRunk, ref rangeRes, listLimit);
            else
            // вне диапазона
                extrapolation(nameAlg, x, fRunk, ref rangeRes, indxNearby, listLimit);

            return rangeRes;
        }
        /// <summary>
        /// Возвратить диапазон точек по указанному рангу аргумента (кол-во точек должно быть - не меньше 1-ой)
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <param name="fRunk">Ранг аргумента</param>
        /// <returns>Массив точек</returns>
        private RangePOINT getRange(string nameAlg, FRUNK fRunk, ListLIMIT listLimit)
        {
            RangePOINT rangeRes = new RangePOINT(); //Результат
            float x = -1F //Аргумент функции
                , min = float.MaxValue, max = float.MinValue;

            foreach (POINT pt in m_dictValues[nameAlg])
                if ((listLimit.Count == 0)
                    || ((listLimit.Count > 0)
                        && (listLimit.ContansPoint(pt) == true)))
                {
                    x = pt.X(fRunk);

                    if (x < min)
                    {
                        min = x;
                        rangeRes.Left = pt;
                    }
                    else
                        ;

                    if (x > max)
                    {
                        max = x;
                        rangeRes.Right = pt;
                    }
                    else
                        ;                    
                }
                else
                    ;

            return rangeRes;
        }
        /// <summary>
        /// Уточнить диапазон соседних реперных (узловых) точек к указанному значению аргумента
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <param name="xValue">Значение аргумента</param>
        /// <param name="fRunk">Ранг аргумента</param>
        /// <param name="arNearby">Массив с реперными точками, требующий уточнения (приближение к значению)</param>
        private void interpolation(string nameAlg, float xValue, FRUNK fRunk, ref RangePOINT rangeNearby, ListLIMIT listLimit)
        {
            float x = -1F
                , min = rangeNearby.Left.X(fRunk), max = rangeNearby.Right.X(fRunk);

            foreach (POINT pt in m_dictValues[nameAlg])
                if ((listLimit.Count == 0)
                    || ((listLimit.Count > 0)
                        && (listLimit.ContansPoint(pt) == true)))
                {
                    x = pt.X(fRunk);

                    if (((xValue - x) < (xValue - min))
                        && (!((xValue - x) < 0))
                        && (!(x == max)))
                    {
                        min = x;
                        rangeNearby.Left = pt;

                        continue;
                    }
                    else
                        ;

                    if (((x - xValue) < (max - xValue))
                        && (!((x - xValue) < 0))
                        && (!(x == min)))
                    {
                        max = x;
                        rangeNearby.Right = pt;
                    }
                    else
                        ;                    
                }
                else
                    ;
        }
        /// <summary>
        /// Уточнить диапазон соседних реперных (узловых) точек к указанному значению аргумента
        /// </summary>
        /// <param name="nameAlg">Наименование функции</param>
        /// <param name="xValue">Значение аргумента</param>
        /// <param name="fRunk">Ранг аргумента</param>
        /// <param name="arNearby">Массив с реперными точками, требующий уточнения (приближение к значению)</param>
        /// <param name="indxConstNearby">Граница диапазона, остающейся постоянной</param>
        private void extrapolation(string nameAlg, float xValue, FRUNK fRunk, ref RangePOINT rangeNearby, INDEX_NEAR indxConstNearby, ListLIMIT listLimit)
        {
            float min = -1F, max = -1F
                , x = -1F;

            min = float.MaxValue; //rangeNearby.Left.X(fRunk);
            max = float.MinValue; //rangeNearby.Right.X(fRunk);

            foreach (POINT pt in m_dictValues[nameAlg])
                if ((listLimit.Count == 0)
                    || ((listLimit.Count > 0)
                        && (listLimit.ContansPoint(pt) == true)))
                {
                    x = pt.X(fRunk);

                    if (min == float.MaxValue)
                    {
                        min = x;
                        rangeNearby.Left = pt;

                        continue;
                    }
                    else
                        ;

                    if ((max == float.MinValue)
                        && (!(min == x)))
                    {
                        max = x;
                        rangeNearby.Right = pt;

                        continue;
                    }
                    else
                        ;

                    if ((!(min == float.MaxValue))
                        && (!(max == float.MinValue)))
                        if ((Math.Abs(xValue - x) < Math.Abs(xValue - min))
                            && (!(x == max)))
                        {
                            if ((xValue - min) < (xValue - max))
                            {
                                max = min;
                                rangeNearby.Right = rangeNearby.Left;
                            }
                            else
                                ;

                            min = x;
                            rangeNearby.Left = pt;
                        }
                        else
                            ;
                    else
                        if ((Math.Abs(xValue - x) < Math.Abs(xValue - max))
                            && (!(x == min)))
                        {
                            max = x;
                            rangeNearby.Right = pt;
                        }
                        else
                            ;
                }
                else
                    ;
        }
        /// <summary>
        /// Вычислить результирующее значение функции между 2-мя заданными точками
        /// </summary>
        /// <param name="x">Значение аргумента функции</param>
        /// <param name="fRunk">Ранг (порядок) аргумента</param>
        /// <param name="rangePt">Диапазон известных ближайших точек</param>
        /// <returns>Значение функции в точке</returns>
        private float calc(float x, FRUNK fRunk, RangePOINT rangePt)
        {
            float fRes = -1F;

            fRes = calc(x, rangePt.Left.X(fRunk), rangePt.Left.f, rangePt.Right.X(fRunk), rangePt.Right.f);

            return fRes;
        }
        /// <summary>
        /// Вычислить результирующее значение функции между 2-мя заданными точками
        /// </summary>
        /// <param name="x">Значение аргумента функции</param>
        /// <param name="xLeft">Значение аргумента известной ближайшей точки "слева"</param>
        /// <param name="fLeft">Значение функции в известной ближайшей точке "слева"</param>
        /// <param name="xRight">Значение аргумента известной ближайшей точки "справа"</param>
        /// <param name="fRight">Значение функции в известной ближайшей точке "справа"</param>
        /// <returns>Значение функции в точке</returns>
        private float calc(float x, float xLeft, float fLeft, float xRight, float fRight)
        {
            float fRes = -1F;

            fRes = (x - xLeft) * (fRight - fLeft) / (xRight - xLeft) + fLeft;

            return fRes;
        }
        /// <summary>
        /// Вычислить значения для функции
        ///  по заданным аргументам
        ///  для одного аргумента все считает
        ///  для двух - ошибка в вычислении
        ///  для трех - алгоритм недописан
        /// </summary>
        /// <param name="args">Аргументы для вычисления функции</param>
        /// <returns>Значение функции по заданным аргументам</returns>
        public float Calculate(string nameALG, FRUNK fRunkVar, params float[] args)
        {
            m_nameAlg = nameALG;
            FRUNK fRunk = _fRunk;
            ////??? для универсализации расчета
            //int iRunk = -1
            //    , iPow = -1
            //    , iRow =-1, iCol = -1;
            List<RangePOINT[,]> listPointNearby = new List<RangePOINT[,]>((int)(fRunk + 1));
            List<float [,]>listRes = new List<float[,]> ();

            if ((fRunkVar > FRUNK.UNKNOWN) // ранг введенной переменной д.б. известен
                && (! ((int)fRunkVar > args.Length)) // ранг введенной переменной д.б. не больше кол-ва аргументов
                && (m_dictValues[nameALG].Count > 1)) // для вычислений требуется как минимум 2 точки
            {
                ////??? попытка универсализации расчета
                //for (iRunk = (int)FRUNK.F1; iRunk < ((int)fRunk + 1); iRunk++)
                //{
                //    iPow = (int)(fRunk - iRunk);
                //    iRow = (int)(Math.Pow((fRunk == FRUNK.F1 ? 1F : (float)fRunk), iPow) / (iPow == 0 ? 1 : iPow));
                //    iCol = iPow == 0 ? 1 : iPow;
                //    listPointNearby.Insert(iRunk, new RangePOINT[iRow, iCol]);
                //    listRes.Insert(iRunk, new float[iRow, iCol]);
                //}                
                //for (iRunk = (int)FRUNK.F1; iRunk < ((int)fRunk + 1); iRunk++)
                //    for (int i = 0; i < listPointNearby[(int)iRunk].GetLength(0); i++)
                //        for (int j = 0; j < listPointNearby[(int)iRunk].GetLength(1); j++)
                //            ;

                switch (fRunk)
                {
                    case FRUNK.F1:
                        //??? не универсальное добавление элементов
                        listPointNearby.Insert((int)FRUNK.F1, new RangePOINT[1, 1]);
                        listRes.Insert((int)FRUNK.F1, new float[1, 1]);
                        // получить ближайшие реперные (узловые) точки
                        listPointNearby[(int)FRUNK.F1][0, 0] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT());
                        // вычисление промежуточных значений ... - нет
                        // вычисление рез-та
                        listRes[(int)fRunk][0, 0] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][(int)FRUNK.F1, (int)FRUNK.F1])
                            //-1F по умолчанию
                            ;
                        break;
                    case FRUNK.F2:
                        //??? не универсальное добавление элементов
                        listPointNearby.Insert((int)FRUNK.F1, new RangePOINT[2, 1]);
                        listRes.Insert((int)FRUNK.F1, new float[2, 1]);
                        listPointNearby.Insert((int)FRUNK.F2, new RangePOINT[1, 1]);
                        listRes.Insert((int)FRUNK.F2, new float[1, 1]);
                        // получить ближайшие реперные (узловые) точки
                        listPointNearby[(int)FRUNK.F2][0, 0] = getNearby(nameALG, args[(int)FRUNK.F2]
                            , FRUNK.F2
                            , new ListLIMIT());
                        listPointNearby[(int)FRUNK.F1][0, 0] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT () { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][0, 0].Left.a2 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F1][1, 0] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][0, 0].Right.a2 }
                                }
                            );
                        // вычисление промежуточных значений
                        // 1-ый ранг
                        listRes[(int)FRUNK.F1][0, 0] = calc (args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][0, 0]);
                        listRes[(int)FRUNK.F1][1, 0] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][1, 0]);
                        // вычисление рез-та
                        listRes[(int)fRunk][0, 0] = calc(args[(int)FRUNK.F2]
                            , listPointNearby[(int)FRUNK.F2][0, 0].Left.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][0, 0]
                            , listPointNearby[(int)FRUNK.F2][0, 0].Right.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][1, 0])
                            //-1F по умолчанию
                            ;
                        break;
                    case FRUNK.F3:
                        //??? не универсальное добавление элементов
                        listPointNearby.Insert((int)FRUNK.F1, new RangePOINT[2, 2]);
                        listRes.Insert((int)FRUNK.F1, new float[2, 2]);
                        listPointNearby.Insert((int)FRUNK.F2, new RangePOINT[2, 1]);
                        listRes.Insert((int)FRUNK.F2, new float[2, 1]);
                        listPointNearby.Insert((int)FRUNK.F3, new RangePOINT[2, 1]);
                        listRes.Insert((int)FRUNK.F3, new float[1, 1]);
                        // получить ближайшие реперные (узловые) точки
                        listPointNearby[(int)FRUNK.F3][0, 0] = getNearby(nameALG, args[(int)FRUNK.F3]
                            , FRUNK.F3
                            , new ListLIMIT());
                        listPointNearby[(int)FRUNK.F2][0, 0] = getNearby(nameALG, args[(int)FRUNK.F2]
                            , FRUNK.F2
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Left.a3 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F2][1, 0] = getNearby(nameALG, args[(int)FRUNK.F2]
                            , FRUNK.F2
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Right.a3 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F1][0, 0] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Right.a3 }
                                    , new LIMIT() { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][0, 0].Left.a2 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F1][1, 0] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Right.a3 }
                                    , new LIMIT() { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][0, 0].Right.a2 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F1][0, 1] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Left.a3 }
                                    , new LIMIT() { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][1, 0].Left.a2 }
                                }
                            );
                        listPointNearby[(int)FRUNK.F1][1, 1] = getNearby(nameALG, args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , new ListLIMIT()
                                {
                                    new LIMIT() { fRunk = FRUNK.F3, x = listPointNearby[(int)FRUNK.F3][0, 0].Left.a3 }
                                    , new LIMIT() { fRunk = FRUNK.F2, x = listPointNearby[(int)FRUNK.F2][1, 0].Right.a2 }
                                }
                            );
                        // вычисление промежуточных значений
                        // 1-ый ранг
                        listRes[(int)FRUNK.F1][0, 0] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][0, 0]);
                        listRes[(int)FRUNK.F1][1, 0] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][1, 0]);
                        listRes[(int)FRUNK.F1][0, 1] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][0, 1]);
                        listRes[(int)FRUNK.F1][1, 1] = calc(args[(int)FRUNK.F1]
                            , FRUNK.F1
                            , listPointNearby[(int)FRUNK.F1][1, 1]);
                        // 2-ой ранг
                        listRes[(int)FRUNK.F2][0, 0] = calc(args[(int)FRUNK.F2]
                            , listPointNearby[(int)FRUNK.F2][0, 0].Left.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][0, 0]
                            , listPointNearby[(int)FRUNK.F2][0, 0].Right.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][1, 0]);
                        listRes[(int)FRUNK.F2][1, 0] = calc(args[(int)FRUNK.F2]
                            , listPointNearby[(int)FRUNK.F2][1, 0].Left.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][0, 1]
                            , listPointNearby[(int)FRUNK.F2][1, 0].Right.X(FRUNK.F2)
                            , listRes[(int)FRUNK.F1][1, 1]);
                        // вычисление рез-та
                        listRes[(int)fRunk][0, 0] = calc(args[(int)FRUNK.F3]
                            , listPointNearby[(int)FRUNK.F3][0, 0].Left.X(FRUNK.F3)
                            , listRes[(int)FRUNK.F2][0, 0]
                            , listPointNearby[(int)FRUNK.F3][0, 0].Right.X(FRUNK.F3)
                            , listRes[(int)FRUNK.F2][1, 0])
                            //-1F по умолчанию
                            ;
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            m_nameAlg = string.Empty;

            return listRes[(int)fRunk][0, 0];
        }        
    }
}