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
        /// Структура для хранения значений для одной из точек функции
        /// </summary>
        protected struct POINT
        {
            public int m_idRec;
            public double a1
                , a2
                , a3
                , f;
        }
        /// <summary>
        /// Класс для хранения списка всех значений одной функции
        /// </summary>
        protected class ListPOINT : List<POINT>
        {
        }
        /// <summary>
        /// Словарь со значениями для всех функций
        ///  (ключ - наименование функции)
        /// </summary>
        protected Dictionary<string, ListPOINT> m_dictValues;
        /// <summary>
        /// Граница диапазона (1-я, 2-я)
        /// </summary>
        double min1 = Math.Exp(15)
            , min2 = -1 * Math.Exp(15);
        /// <summary>
        /// Значения отклонений от границ (1-ой, 2-ой)
        /// </summary>
        double metka1
            , metka2;
        /// <summary>
        /// Массив значений калькулятора
        /// </summary>
        string[] ArgApproxi;
        /// <summary>
        /// Массив аргументов функции
        /// </summary>
        DataRow[] ValuesFunc;
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
        protected string referencePoint;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FTable()
        {
            m_dictValues = new Dictionary<string, ListPOINT>();
        }

        public FTable(DataTable src) : this ()
        {
            Set(src);
        }

        public void Set(DataTable src)
        {
            string nAlg = string.Empty;

            m_dictValues.Clear();

            foreach (DataRow r in src.Rows)
            {
                nAlg = (string)r[@"N_ALG"];

                if (m_dictValues.Keys.Contains(nAlg) == false)
                    m_dictValues.Add(nAlg, new ListPOINT());
                else
                    ;

                m_dictValues[nAlg].Add(new POINT() { 
                    m_idRec = (int)r[@"ID"]
                    , a1 = (double)r[@"A1"]
                    , a2 = (double)r[@"A2"]
                    , a3 = (double)r[@"A3"]
                    , f = (double)r[@"F"]
                });
            }
        }
        /// <summary>
        /// Создание массива с данными всех значений одной функции
        /// Заполняет значениями функции для работы с ними.
        /// </summary>
        /// <param name="nameALG">имя функции</param>
        public void CreateParamMassive(string nameALG, string nColumn, int row)
        {
            //ValuesFunc = ...;
        }
        /// <summary>
        /// Проверка на вложеность
        /// числа в диапазон первых минимумов
        /// </summary>
        /// <param name="columnname">имя столбца("А1,А2,А3")</param>
        private void rangeOfValues(string columnName)
        {
            if (((Convert.ToDouble(referencePoint)) < min2) && ((Convert.ToDouble(referencePoint)) > min1))
                for (int i = 0; i < ValuesFunc.Length; i++)
                    if (condition == true)
                        interpolation(columnName);
                    else
                        ;
            else
                if (condition == true)
                    extrapolation(columnName);
                else
                    ;
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
                else
                    ;

                if (testConditionMIN2(i, columnName) == true)
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][columnName].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                }
                else
                    ;
            }
        }
        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN1
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца</param>
        /// <returns>Признак выполнения условия</returns>
        private bool testConditionMIN1(int i, string column)
        {
            bool bRes = false;

            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = m_selectedCell - Convert.ToDouble(ValuesFunc[i][column]);
            double m_twoPeremen = m_selectedCell - min1;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min2)))
                bRes = true;
            else
                ;

            return bRes;
        }
        /// <summary>
        /// Проверка условия интерполяции 
        /// для нахождения MIN2
        /// </summary>
        /// <param name="i">номер строки</param>
        /// <param name="column">имя столбца массива</param>
        /// <returns>Признак выполнения условия</returns>
        private bool testConditionMIN2(int i, string column)
        {
            bool bRes = false;

            double m_selectedCell = Convert.ToDouble(referencePoint);
            double m_onePeremen = Convert.ToDouble(ValuesFunc[i][column]) - m_selectedCell;
            double m_twoPeremen = min2 - m_selectedCell;

            if ((m_onePeremen < m_twoPeremen) && (m_onePeremen >= 0) && (!(Convert.ToDouble(ValuesFunc[i][column]) == min1)))
                bRes = true;
            else
                ;

            return bRes;
        }
        /// <summary>
        /// Экстраполяция значений функций
        /// </summary>
        /// <param name="column"></param>
        private void extrapolation(string column)
        {
            bool bflag1 = true;
            bool bflag2 = true;

            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                if (bflag1 == true)
                {
                    min1 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    bflag1 = false;
                }

                if (bflag2 == true && min1 == Convert.ToDouble(ValuesFunc[i][column]))
                {
                    min2 = Convert.ToDouble(ValuesFunc[i][column].ToString());
                    metka2 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
                    bflag2 = false;
                }

                if (bflag1 == false && bflag2 == false)
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
            double oneParam = Convert.ToDouble(referencePoint);
            double peremen1 = Math.Abs(oneParam - Convert.ToDouble(ValuesFunc[i][n_column].ToString()));
            double ABSmin1 = Math.Abs(oneParam - min1);
            double ABSmin2 = Math.Abs(oneParam - min2);

            if (peremen1 < ABSmin1 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min2))
            {
                if (ABSmin1 < ABSmin2)
                {
                    min2 = min1;
                    metka2 = metka1;
                }
                min1 = Convert.ToDouble(ValuesFunc[i][n_column].ToString());
                metka1 = Convert.ToInt32(ValuesFunc[i]["F"].ToString());
            }
            else
            {
                if (peremen1 < ABSmin2 && !(Convert.ToDouble(ValuesFunc[i][n_column].ToString()) == min1))
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
                if (condition == true)
                {
                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) < min1)
                    {
                        min1 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka1 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }
                    else
                        ;

                    if (Convert.ToDouble(ValuesFunc[i][nameColumn]) > min2)
                    {
                        min2 = Convert.ToDouble(ValuesFunc[i][nameColumn]);
                        metka2 = Convert.ToDouble(ValuesFunc[i]["F"].ToString());
                    }
                    else
                        ;
                }
                else
                    ;
        }        
        /// <summary>
        /// Функция нахождения реперных точек
        /// с одним параметром
        /// </summary>
        /// <param name="colCount">кол-во аргументов</param>
        protected virtual void funcWithOneArgs(string nameCol, string filter)
        {
            searchMainMIN(nameCol);
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
            else
                ;

            for (int i = 0; i < ValuesFunc.Length; i++)
            {
                for (int t = 0; t < m_colParam; t++)
                    if (Convert.ToDouble(ValuesFunc[i][nameCol]) == Convert.ToDouble(arg.ElementAt(t).ToString()))
                        m_bFlag = true;
                    else
                        ;

                if (m_bFlag)
                {
                    m_row++;

                    for (int j = 1; j < colCount; j++)
                    {
                        string column = "A" + j;
                        ValuesFunc[m_row][column] = ValuesFunc[i][column];
                    }
                }
                else
                    ;
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
        /// Вычислить значения для функции
        ///  по заданным аргументам
        /// </summary>
        /// <param name="nAlg">Наменование функции</param>
        /// <param name="args">Аргументы для вычисления функции</param>
        /// <returns></returns>
        public double Calculate (string nAlg, params double []args)
        {
            double dblRes = -1F;

            return dblRes;
        }
    }
}
