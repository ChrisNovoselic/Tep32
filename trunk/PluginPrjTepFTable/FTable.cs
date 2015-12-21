using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using System.Drawing;

using HClassLibrary;

namespace PluginPrjTepFTable
{
    public class FTable
    {
        /// <summary>
        /// Перечисление для обозночения уровня точки, функции
        /// </summary>
        public enum FRUNK { F1, F2, F3 }

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
        /// Словарь со значениями для всех функций
        ///  (ключ - наименование функции)
        /// </summary>
        protected Dictionary<string, ListPOINT> m_dictValues;
        /// <summary>
        /// 
        /// </summary>
        float[][] metka;
        /// <summary>
        /// Массив значений минимумов калькулятора
        /// </summary>
        float[][] ArgMin;
        /// <summary>
        /// Массив аргументов функции
        /// </summary>
        float[][] ValuesFunc;
        /// <summary>
        /// 
        /// </summary>
        double[][] FinalRezult;
        /// <summary>
        /// флаг для экстраполяции
        /// </summary>
        bool bflag1;
        bool bflag2;
        /// <summary>
        /// Признак выполнения условия
        ///  , которое задается для вычисления минимумов и для перестройки массивов
        /// </summary>
        protected bool condition;
        /// <summary>
        /// Составное наименование функции
        ///  имя_функции + наименование столбца
        /// </summary>
        protected string m_nameAlg;
        /// <summary>
        /// Значение-строка
        /// </summary>
        protected double referencePoint;

        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FTable()
        {
            m_dictValues = new Dictionary<string, ListPOINT>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        public FTable(DataTable src)
            : this()
        {
            Set(src);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nAlg"></param>
        /// <returns></returns>
        public FRUNK GetRunk(string nAlg) { return m_dictValues[nAlg].Runk; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
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
        /// Создание массива с данными всех значений одной функции
        /// Заполняет значениями функции для работы с ними.
        /// </summary>
        /// <param name="nameALG"></param>
        /// <param name="nColumn"></param>
        /// <param name="row"></param>
        public void CreateParamMassive(string nameALG, string nColumn, int row)
        {
            //ValuesFunc = ...;
        }

        /// <summary>
        /// Проверка на вложеность
        /// числа в диапазон первых минимумов
        /// </summary>
        private void rangeOfValues(int numArray, int numMin, int elemArray)
        {
            bflag1 = true;
            bflag2 = true;

            if ((referencePoint < ArgMin[numMin].ElementAt(elemArray + 1)) && (referencePoint > ArgMin[numMin].ElementAt(elemArray)))
            {
                for (int i = 0; i < ValuesFunc[numArray].Length; i++)
                {
                    calcCondition(i, numArray, numMin, numMin + 1, elemArray);

                    if (condition == true)
                        interpolation(numArray, numMin, elemArray, i);
                    else
                        ;
                }
            }
            else
            {
                for (int i = 0; i < ValuesFunc[numArray].Length; i++)
                {
                    calcCondition(i, numArray, numMin, numMin + 1, elemArray);

                    if (condition == true)
                        extrapolation(numArray, numMin, elemArray, i);
                    else
                        ;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <param name="array"></param>
        /// <param name="min"></param>
        /// <param name="nextarg"></param>
        /// <param name="elemMin"></param>
        private void calcCondition(int i, int array, int min, int nextarg, int elemArray)
        {
            int countArray = 0;
            if (elemArray > 1)
                countArray = 0;
            else if (elemArray > 0)
                countArray = 1;

            switch (nextarg)
            {
                case 1:
                    condition = true;
                    break;
                case 2:
                    if (ValuesFunc[array + 1].ElementAt(i) == ArgMin[0].ElementAt(elemArray)) // другой счетчик элемента массива минимума
                    {
                        condition = true;
                    }
                    else condition = false;
                    break;
                case 3:
                    if (ValuesFunc[2].ElementAt(i) == ArgMin[0].ElementAt(countArray) &&
                        ValuesFunc[1].ElementAt(i) == ArgMin[1].ElementAt(elemArray))
                    {
                        condition = true;
                    }
                    else condition = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Интерполяция значений функции
        /// </summary>
        private void interpolation(int numArray, int numMin, int elemArray, int i)
        {
            if (testConditionMIN1(i, numArray, numMin, elemArray) == true)
            {
                ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray);
                metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray);
            }
            else
                ;

            if (testConditionMIN2(i, numArray, numMin, elemArray) == true)
            {
                ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray + 1);
                metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray + 1);
            }
            else
                ;
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения более ближайщего значения MIN1
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <returns>Признак выполнения условия</returns>
        private bool testConditionMIN1(int i, int nArray, int NumMin, int elemArray)
        {
            bool bRes = false;

            double m_selectedCell = referencePoint;
            double m_onePeremen = m_selectedCell - ValuesFunc[nArray].ElementAt(i);
            double m_twoPeremen = m_selectedCell - ArgMin[NumMin].ElementAt(elemArray);//min1

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) &&
                (ValuesFunc[nArray].ElementAt(i) != ArgMin[NumMin].ElementAt(elemArray + 1))) //min2
                bRes = true;
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения более ближайщего значения MIN2
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <returns>Признак выполнения условия</returns>
        private bool testConditionMIN2(int i, int nArray, int NumMin, int elemArray)
        {
            bool bRes = false;

            double m_selectedCell = referencePoint;
            double m_onePeremen = ValuesFunc[nArray].ElementAt(i) - m_selectedCell;
            double m_twoPeremen = ArgMin[NumMin].ElementAt(elemArray + 1) - m_selectedCell; //min2

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) &&
                (ValuesFunc[nArray].ElementAt(i) != ArgMin[NumMin].ElementAt(elemArray)))
                bRes = true;
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Экстраполяция значений функций
        /// </summary>
        /// <param name="numArray"></param>
        private void extrapolation(int numArray, int numMin, int elemArray, int i)
        {
            if (bflag1 == true)
            {
                ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray);
                metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray);
                bflag1 = false;
            }
            else ;

            if (bflag2 == true && ArgMin[numMin].ElementAt(elemArray) != ValuesFunc[numArray].ElementAt(i)) //min1
            {
                ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray + 1);
                metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray + 1);
                bflag2 = false;
            }
            else ;

            if (bflag1 == false && bflag2 == false)
            {
                ABSfunc(i, numArray, numMin, elemArray);
            }
            else ;

        }

        /// <summary>
        /// Проверка аргумента на близость к заданому числу, 
        /// если он еще ближе чем минимумы, то запоминается
        /// </summary>
        /// <param name="i"></param>
        /// <param name="nArray"></param>
        private void ABSfunc(int i, int nArray, int numMin, int elemArray)
        {
            double peremen1 = Math.Abs(referencePoint - ValuesFunc[nArray].ElementAt(i));
            double ABSmin1 = Math.Abs(referencePoint - ArgMin[numMin].ElementAt(elemArray)); //min1
            double ABSmin2 = Math.Abs(referencePoint - ArgMin[numMin].ElementAt(elemArray + 1)); //min2

            if (peremen1 < ABSmin1 && ValuesFunc[nArray].ElementAt(i) != ArgMin[numMin].ElementAt(elemArray + 1)) //min2
            {
                if (ABSmin1 < ABSmin2)
                {
                    ArgMin[numMin].SetValue(ArgMin[numMin].ElementAt(0), elemArray + 1);
                    metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray + 1);
                }

                ArgMin[numMin].SetValue(ValuesFunc[nArray].ElementAt(i), elemArray);
                metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray);
            }
            else
            {
                if (peremen1 < ABSmin2 && ValuesFunc[nArray].ElementAt(i) != ArgMin[numMin].ElementAt(elemArray)) //min1
                {
                    ArgMin[numMin].SetValue(ValuesFunc[nArray].ElementAt(i), elemArray + 1);
                    metka[numMin].SetValue(ValuesFunc[nArray].ElementAt(i), elemArray + 1);
                }
            }
        }

        /// <summary>
        /// Определение границ диапазона
        /// аргумента от наименьшего до наибольшего
        /// </summary>
        /// <param name="bflag"></param>
        /// <param name="numMin"></param>
        /// <param name="numArray"></param>
        private void searchMainMIN(int numMin, int numArray, int elemArray)
        {
            /// <summary>
            /// Граница диапазона (1-я, 2-я)
            /// </summary>
            double min1 = Math.Exp(15)
                , min2 = -1 * Math.Exp(15);

            for (int i = 0; i < m_dictValues[m_nameAlg].Count(); i++)
            {
                calcCondition(i, numArray, numMin, numMin + 1, elemArray);

                if (condition == true)
                {
                    if (ValuesFunc[numArray].ElementAt(i) < min1)
                    {
                        ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray);
                        metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray);
                        min1 = ValuesFunc[numArray].ElementAt(i);
                    }
                    else
                        ;

                    if (ValuesFunc[numArray].ElementAt(i) > min2)
                    {
                        ArgMin[numMin].SetValue(ValuesFunc[numArray].ElementAt(i), elemArray + 1);
                        metka[numMin].SetValue(m_dictValues[m_nameAlg][i].f, elemArray + 1);
                        min2 = ValuesFunc[numArray].ElementAt(i);//убрать в будущем, как вариант
                    }
                    else
                        ;
                }
                else
                    ;
            }
        }

        /// <summary>
        /// Функция нахождения реперных точек
        /// с одним параметром
        /// </summary>
        /// <param name="colCount">кол-во аргументов</param>
        protected virtual void funcWithOneArgs(string filter)
        {
            //searchMainMIN();
        }

        /// <summary>
        /// Фильтрация массива-функции
        /// сравнение значений массива с найденными минимумами
        /// </summary>
        /// <param name="numArray">номер массива</param>
        /// <param name="argMin">номер минимума</param>
        /// <param name="elemArray">элемент массива</param>
        private void filterArray(int numArray, int numMin, int elemArray)
        {
            int count = 0;

            if (ArgMin[numMin].Count() > 3)
            {

            }

            for (int i = 0; i < ValuesFunc[numArray].Length; i++)
            {
                if (ValuesFunc[numArray].ElementAt(i) == ArgMin[numArray].ElementAt(elemArray))
                {
                    count++;

                    for (int j = 0; j < numArray; j++)
                    {
                        //ValuesFunc[].SetValue(ValuesFunc[j].ElementAt(i),count); //???
                    }
                }
            }
            //??? либо замена массива, либо создание нового.
        }

        /// <summary>
        /// вычисление конечного (return)
        /// результата
        /// </summary>
        /// <param name="numArray">номер массива</param>
        /// <param name="elemArray">элеент массива</param>
        /// <returns></returns>
        private double obtaingPointMain(int numArray, int elemArray, int t)
        {
            if (t < 1)
                return (referencePoint - ArgMin[numArray].ElementAt(elemArray)) * (metka[numArray].ElementAt(elemArray + 1) - metka[numArray].ElementAt(elemArray)) /
               (ArgMin[numArray].ElementAt(elemArray + 1) - ArgMin[numArray].ElementAt(elemArray)) + metka[numArray].ElementAt(elemArray);
            else
                return (referencePoint - ArgMin[numArray].ElementAt(elemArray)) * (FinalRezult[numArray].ElementAt(elemArray + 1) - FinalRezult[numArray].ElementAt(elemArray)) /
                    ((ArgMin[numArray].ElementAt(elemArray + 1) - ArgMin[numArray].ElementAt(elemArray)) + FinalRezult[numArray].ElementAt(elemArray));
            //(referencePoint - min1) * (metka2 - metka1) / (min2 - min1) + metka1;
        }

        /// <summary>
        /// Вычислить значения для функции
        ///  по заданным аргументам
        ///  для одного аргумента все считает
        ///  для двух - ошибка в вычислении
        ///  для трех - алгоритм недописан
        /// </summary>
        /// <param name="args">Аргументы для вычисления функции</param>
        /// <returns></returns>
        public double Calculate(string nameALG, params float[] args)
        {
            double dblRes = -1F;
            int boolExpress = 1; //
            int Param = args.Count(); //переменная номера массива
            m_nameAlg = nameALG; //имя фукнции
            int elemArray = 0; //номер элеента в массиве
            ArgMin = new float[args.Count()][]; //массив минимумов для всех трех столбцов
            metka = new float[args.Count()][]; //
            FinalRezult = new double[args.Count()][];
            ValuesFunc = new float[Param][]; // массивы значений трех столбцов

            for (int i = 0; i < args.Count(); i++)
            {
                referencePoint = args.ElementAt(Param - 1);
                selectArgs(Param);
                metka[i] = new float[Convert.ToInt32(Math.Pow(2, i + 1))];
                ArgMin[i] = new float[Convert.ToInt32(Math.Pow(2, i + 1))];

                for (int j = 0; j < boolExpress; j++)
                {
                    searchMainMIN(i, Param - 1, elemArray);
                    rangeOfValues(Param - 1, i, elemArray);
                   elemArray = elemArray+2;
                }

                //filterArray(Param-1,i);
                boolExpress = Convert.ToInt32(Math.Pow(2, i + 1));
                Param--;
                elemArray = 0;
            }

            elemArray = 0;
            Param = args.Count();

            for (int t = 0; t < args.Count(); t++)
            {
                boolExpress = Convert.ToInt32(Math.Pow(2, Param - 1));
                referencePoint = args.ElementAt(t);
                FinalRezult[t] = new double[boolExpress];

                for (int i = 0; i < boolExpress; i++)
                {
                    FinalRezult[t].SetValue(obtaingPointMain(Param - 1, elemArray, t), i);
                    elemArray = elemArray + 2;
                }

                elemArray = 0;
                Param--;
                dblRes = FinalRezult[t].ElementAt(0);
            }

            return dblRes;
        }

        /// <summary>
        /// Заполнение массива данными столбца
        /// </summary>
        /// <param name="num">номер массива</param>
        /// <returns></returns>
        private float[][] getValuesA1(int num)
        {
            ValuesFunc[num - 1] = new float[m_dictValues[m_nameAlg].Count()];

            for (int i = 0; i < m_dictValues[m_nameAlg].Count(); i++)
            {
                ValuesFunc[num - 1].SetValue(m_dictValues[m_nameAlg][i].a1, i);
            }

            return ValuesFunc;
        }

        /// <summary>
        /// Заполнение массива данными столбца
        /// </summary>
        /// <param name="num">номер массива</param>
        /// <returns></returns>
        private float[][] getValuesA2(int num)
        {
            ValuesFunc[num - 1] = new float[m_dictValues[m_nameAlg].Count()];

            for (int i = 0; i < m_dictValues[m_nameAlg].Count(); i++)
            {
                ValuesFunc[num - 1].SetValue(m_dictValues[m_nameAlg][i].a2, i);
            }

            return ValuesFunc;
        }

        /// <summary>
        /// Заполнение массива данными столбца
        /// </summary>
        /// <param name="num">номер массива</param>
        /// <returns></returns>
        private float[][] getValuesA3(int num)
        {
            ValuesFunc[num - 1] = new float[m_dictValues[m_nameAlg].Count()];

            for (int i = 0; i < m_dictValues[m_nameAlg].Count(); i++)
            {
                ValuesFunc[num - 1].SetValue(Convert.ToInt32(m_dictValues[m_nameAlg][i].a3), i);
            }

            return ValuesFunc;
        }

        /// <summary>
        /// Проверка кол-ва парметров (для калькулятора)
        /// </summary>
        /// <param name="runk">кол-во парамтеров</param>
        private void selectArgs(int runk)
        {
            switch (runk)
            {
                case 1:
                    getValuesA1(runk);
                    break;

                case 2:
                    getValuesA2(runk);
                    break;

                case 3:
                    getValuesA3(runk);
                    break;
            }
        }
    }
}
