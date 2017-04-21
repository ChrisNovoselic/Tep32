using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskBalTeplo
{
    public class HandlerDbTaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public /*partial*/ class TaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            protected override int initValues(TepCommon.HandlerDbTaskCalculate.TaskCalculate.ListDATATABLE listDataTables)
            {
                initValues(Out, listDataTables.FindDataTable(INDEX_DATATABLE.OUT_PARAMETER), listDataTables.FindDataTable(INDEX_DATATABLE.OUT_VALUES));
                initValues(In, listDataTables.FindDataTable(INDEX_DATATABLE.IN_PARAMETER), listDataTables.FindDataTable(INDEX_DATATABLE.IN_VALUES));
                return 0;
            }

            /// <summary>
            /// Перечисления индексы для массива идентификаторов компонентов оборудования ТЭЦ
            /// </summary>
            private enum INDX_COMP : short
            {
                UNKNOWN = -1
                    , iBL1, iBL2, iBL3, iBL4, iBL5, iBL6,
                iOP1, iOP2, iOP3, iOP4, iOP5, iOP6,
                iPP2, iPP3, iPP4, iPP5, iPP6, iPP7, iPP8,
                iST
                    , COUNT
            };
            /// <summary>
            /// Константы - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private const int BL1 = 1029
                , BL2 = 1030
                , BL3 = 1031
                , BL4 = 1032
                , BL5 = 1033
                , BL6 = 1034
                , OP1 = 2001
                , OP2 = 2002
                , OP3 = 2003
                , OP4 = 2004
                , OP5 = 2005
                , OP6 = 2006
                , PP2 = 3002
                , PP3 = 3003
                , PP4 = 3004
                , PP5 = 3005
                , PP6 = 3006
                , PP7 = 3007
                , PP8 = 3008
                    , ST = 5;
            /// <summary>
            /// Массив - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private readonly int[] ID_COMP =
            {
                BL1, BL2, BL3, BL4, BL5, BL6
                ,OP1,OP2,OP3,OP4,OP5,OP6
                    ,PP2,PP3,PP4,PP5,PP6,PP7,PP8
                    , ST
            };
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskBalTeploCalculate(ListDATATABLE listDataTable)
                : base(listDataTable)
            {
                In = new P_ALG();
                Out = new P_ALG();
            }

            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable[] CalculateOut(ListDATATABLE listDataTables)
            {
                int iInitValuesRes = -1;

                DataTable tableRes = null;
                DataTable tableResIn = null;

                iInitValuesRes = initValues(listDataTables);

                if (iInitValuesRes == 0) {
                    var items = from pair in In
                                orderby pair.Key ascending
                                select pair;
                    // расчет
                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items) {
                        //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                        calculateIn(pAlg.Key);
                    }
                    // преобразование в таблицу
                    tableResIn = resultToTable(In);

                    items = from pair in Out
                            orderby pair.Key ascending
                            select pair;

                    // расчет
                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items) {
                        //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                        calculateOut(pAlg.Key);
                    }
                    // преобразование в таблицу
                    tableRes = resultToTable(Out);
                } else
                    ; // ошибка при инициализации параметров, значений
                return new DataTable[] { tableResIn, tableRes };
            }

            private float calculateOut(string nAlg)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная велечина
                float sum = 0,
                    sum1 = 0;
                int i = -1;
                switch (nAlg) {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            double str = 9.771 * Math.Pow(10, -4) + 1.774 * Math.Pow(10, -5) * In["1.2"][ID_COMP[i]].value / 100
                                + 2.52 * Math.Pow(10, -5) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 2) + 2.96 * Math.Pow(10, -6) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100 - 1.5), 3) * In["1.2"][ID_COMP[i]].value / 100
                                + (3.225 * Math.Pow(10, -6) + 1.3436 * Math.Pow(10, -6) * In["1.2"][ID_COMP[i]].value / 100 + 1.684 * Math.Pow(10, -8) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 6)
                                + 1.432 * Math.Pow(10, -7) * Math.Pow((1 / (In["1.2"][ID_COMP[i]].value / 100 + 0.5)), 3)) * ((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10)
                                + (3.7 * Math.Pow(10, -8) + 3.588 * Math.Pow(10, -8) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 3) - 4.05 * Math.Pow(10, -13) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 9)) * Math.Pow(((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10), 2) +
                                +1.1766 * Math.Pow(10, -13) * Math.Pow((In["1.2"][ID_COMP[i]].value / 100), 12) * Math.Pow(((50 - In["1.4"][ID_COMP[i]].value * 0.0980665) / 10), 4);

                            Out[nAlg][ID_COMP[i]].value = (float)str /* 10000*/;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        }
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            double str = In["1.1"][ID_COMP[i]].value;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            double p = In["1.4"][ID_COMP[i]].value;
                            double t = In["1.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            double p = In["1.4"][ID_COMP[i]].value;//Индекс обратного давления
                            double t = In["1.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = 0/*(float)str*/;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            Out[nAlg][ID_COMP[i]].value = (In["1.1"][ID_COMP[i]].value * (Out["1.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            double p = In["2.5"][ID_COMP[i]].value;
                            double t = In["2.3"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            double p = In["2.6"][ID_COMP[i]].value;
                            double t = In["2.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            Out[nAlg][ID_COMP[i]].value = In["7.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            Out[nAlg][ID_COMP[i]].value = (In["2.1"][ID_COMP[i]].value * (Out["2.1"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 3.1
                    case @"3.1": //Тепло с подпиткой сумма суммы тепла по блокам и тепла с подпиткой теплосети
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double str = 0;
                            if (Out["3.2"][ID_COMP[i]].value == 0) {
                                calculateOut("3.2");
                            }
                            Out[nAlg][ID_COMP[i]].value = (In["3.1"][ID_COMP[i]].value * (Out["3.2"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 3.2
                    case @"3.2": //Энтальпия тепла
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double p = In["2.6"][ID_COMP[i]].value;
                            double t = In["2.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 3.3
                    case @"3.3": //Тепло по блокам с подпиткой
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double str = 0;
                            Out[nAlg][ID_COMP[i]].value = Out["1.5"][ID_COMP[(int)INDX_COMP.iST]].value + Out["3.1"][ID_COMP[i]].value;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 4.1
                    case @"4.1": //Q БД тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            Out[nAlg][ID_COMP[i]].value = Out["2.3"][ID_COMP[(int)INDX_COMP.iST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        nAlg = "4.3";
                        fRes = 0;
                        goto entalp;//????
                        break;
                    #endregion

                    #region 4.2
                    case @"4.2": //Q расч тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = (In["4.1"][ID_COMP[i]].value * (Out["4.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 4.3
                    case @"4.3": //Энтальпия тс
                    entalp://????
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double p = In["4.4"][ID_COMP[i]].value;
                            double t = In["4.2"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 5.1
                    case @"5.1": //Тепло вывода F1              
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            Out[nAlg][ID_COMP[i]].value = Out["2.4"][ID_COMP[(int)INDX_COMP.iST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 5.2
                    case @"5.2": //Тепло вывода F2              
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            fRes += In["2.1"][ID_COMP[i]].value * Out["2.1"][ID_COMP[i]].value - In["2.2"][ID_COMP[i]].value * Out["2.2"][ID_COMP[i]].value;
                        }
                        Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / 1000;
                        break;
                    #endregion

                    #region 5.3
                    case @"5.3": //Небаланс                     
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = (In["2.1"][ID_COMP[(int)INDX_COMP.iST]].value - In["2.2"][ID_COMP[(int)INDX_COMP.iST]].value - In["4.1"][ID_COMP[(int)INDX_COMP.iST]].value) / In["2.1"][ID_COMP[(int)INDX_COMP.iST]].value * 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Q бд
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            double str = 0;

                            Out[nAlg][ID_COMP[i]].value = In["7.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;
                        }
                        nAlg = "6.3";
                        fRes = 0;
                        goto entpr;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Q расч
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            Out[nAlg][ID_COMP[i]].value = (In["6.1"][ID_COMP[i]].value * (Out["6.3"][ID_COMP[i]].value - In["5.2"][ID_COMP[(int)INDX_COMP.iST]].value)) / 1000;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes;

                        }
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Энтальпия пр
                    entpr:
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            double p = In["6.5"][ID_COMP[i]].value;
                            double t = In["6.3"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP2);

                        }
                        nAlg = "6.4";
                        fRes = 0;
                        goto entob;
                        break;
                    #endregion

                    #region 6.4
                    case @"6.4": //Энтальпия обр
                    entob:
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            double p = In["6.6"][ID_COMP[i]].value;//Индекс обратного давления
                            double t = In["6.4"][ID_COMP[i]].value;
                            double str = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][ID_COMP[i]].value = (float)str;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                            Out[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP2);

                        }
                        break;
                    #endregion

                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateMaket (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
                return fRes;
            }

            private float calculateIn(string nAlg)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная велечина
                float sum = 0,
                    sum1 = 0;
                int i = -1;
                double str = 0;
                switch (nAlg) {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            str = str + In["1.1"][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.1"][ID_COMP[i - 1]].value;
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            str = str + In["1.2"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.2"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            str = str + In["1.3"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.3"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            In["1.4"][ID_COMP[i]].value = In["1.4"][ID_COMP[i]].value/* * (float)10.197*/;
                            str = str + In["1.4"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.4"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            str = str + In["1.5"][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In["1.5"][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.5
                    case @"2.5":
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            In[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value * (float)10.197;
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 2.6
                    case @"2.6":
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            In[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value * (float)10.197;
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 3.3
                    case @"3.3": //T циркулир. воды ТС по блокам
                        int col = 0;
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            str = str + In["1.3"][ID_COMP[i]].value;
                            col++;
                        }
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)(str / col);
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 3.4
                    case @"3.4":

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value /** (float)10.197*/;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 4.3
                    case @"4.3": //T обратной воды ТС по выводам
                        double temp_vzves = 0;
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            str = str + In["2.2"][ID_COMP[i]].value;
                            temp_vzves = temp_vzves + (In["2.2"][ID_COMP[i]].value * In["2.4"][ID_COMP[i]].value);
                        }
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)(temp_vzves / str);
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 4.4
                    case @"4.4":

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value * (float)10.197;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 5.1
                    //case @"5.1":

                    //    In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value * (float)10.197;
                    //    fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                    //    break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }

                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.4
                    case @"6.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.5
                    case @"6.5": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            In[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value * (float)10.197;
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    #region 6.6
                    case @"6.6": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            In[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value * (float)10.197;
                            str = str + In[nAlg][ID_COMP[i]].value;
                        }
                        str = str / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value = (float)str;
                        fRes += In[nAlg][ID_COMP[(int)INDX_COMP.iST]].value;
                        break;
                    #endregion

                    default:
                        break;
                }
                return fRes;
            }

            private DataTable resultToTable(P_ALG pAlg)
            {
                DataTable tableRes = new DataTable();

                tableRes.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"ID", typeof(int))
                    , new DataColumn (@"QUALITY", typeof(short))
                    , new DataColumn (@"VALUE", typeof(float))
                });

                foreach (P_ALG.P_PUT pPut in pAlg.Values)
                    foreach (P_ALG.P_PUT.P_VAL val in pPut.Values)
                        tableRes.Rows.Add(new object[]
                            {
                                val.m_iId //ID_PUT
                                , val.m_sQuality //QUALITY
                                , val.value //VALUE
                            });

                return tableRes;
            }

            public override DataTable Calculate(TYPE type)
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.ListDATATABLE listDataTable)
        {
            return new TaskBalTeploCalculate(listDataTable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public DataTable OutValues(out int err)
        {
            string strQuery;
            strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME], [EXTENDED_DEFINITION]" // as [ID]
                + @" FROM [" + s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"]"
                + @" WHERE [ID_SESSION]=" + _Session.m_Id;

            return Select(strQuery, out err);
        }        

        /// <summary>
        /// Формирование таблицы для сохранения значений OUT
        /// </summary>
        /// <param name="tableOrigin">первичная таблица</param>
        /// <param name="tableRes">таблица с параметрами</param>
        /// <param name="err"></param>
        /// <returns>таблицу значений</returns>
        public DataTable saveResOut(DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = 0;
            DataTable tableEdit = new DataTable();
            string rowSel = null;
            tableEdit = tableOrigin.Clone();//копия структуры

            if (tableRes != null)
            {
                //foreach (DataGridViewRow r in dgvRes.Rows)
                //{
                for (int i = 0; i < tableRes.Rows.Count; i++)
                {
                    //if (r.Cells[namePut.GetValue(i).ToString()].Value != null)
                    //{
                    rowSel = tableRes.Rows[i]["ID_PUT"].ToString();

                    tableEdit.Rows.Add(new object[]
                            {
                                rowSel
                                , HUsers.Id.ToString()
                                , 0.ToString()
                                , (_Session.m_DatetimeRange.Begin - getOffsetMoscowToUTC).ToString(CultureInfo.InvariantCulture)
                                , ID_PERIOD.DAY
                                , ID_TIMEZONE.MSK
                                , 1.ToString()
                                , tableRes.Rows[i]["VALUE"]
                                , DateTime.Now
                            });
                    //}
                }
                //}
            }
            else;

            return tableEdit;
        }

        /// <summary>
        /// Смещение по времени Москвы до UTC
        /// </summary>
        private TimeSpan getOffsetMoscowToUTC
        {
            get
            {
                TimeSpan offset = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").BaseUtcOffset;
                
                return offset;
            }
            set
            {
            }
        }

        /// <summary>
        /// Формирование таблицы для сохранения значений IN
        /// </summary>
        /// <param name="tableOrigin">первичная таблица</param>
        /// <param name="tableRes">таблица с параметрами</param>
        /// <param name="err"></param>
        /// <returns>таблицу значений</returns>
        public DataTable saveResInval(DataTable tableOrigin, DataTable tableRes, out int err)
        {
            err = 0;
            DataTable tableEdit = new DataTable();
            string rowSel = null;
            tableEdit = tableOrigin.Clone();//копия структуры
            if (tableRes != null)
            {
                //foreach (DataGridViewRow r in dgvRes.Rows)
                //{
                for (int i = 0; i < tableRes.Rows.Count; i++)
                {
                    rowSel = tableRes.Rows[i]["ID_PUT"].ToString();
                    
                    tableEdit.Rows.Add(new object[]
                            {
                                rowSel
                                , HUsers.Id.ToString()
                                , 0.ToString()
                                , (_Session.m_DatetimeRange.Begin - getOffsetMoscowToUTC).ToString(CultureInfo.InvariantCulture)
                                , ID_PERIOD.DAY
                                , ID_TIMEZONE.MSK
                                , 1.ToString()
                                , tableRes.Rows[i]["VALUE"]
                                , DateTime.Now
                            });
                }
                //}
            }
            return tableEdit;
        }

        public override DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Подготовить таблицы для проведения расчета
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении функции</param>
        /// <returns>Массив таблиц со значенями для расчета</returns>
        protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        {
            TaskCalculate.ListDATATABLE listRes = new TaskCalculate.ListDATATABLE();
            err = -1;

            //long idSession = -1;
            DataTable tableVal = null;

            if (isRegisterDbConnection == true)
                // проверить наличие сессии
                if (_Session.m_Id > 0) {
                    // получить описание входных парметров в алгоритме расчета
                    tableVal = Select(getQueryParameters(TaskCalculate.TYPE.IN_VALUES), out err);
                    listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.IN_PARAMETER, m_table = tableVal.Copy() });
                    // получить входные значения для сессии
                    tableVal = getVariableTableValues(TaskCalculate.TYPE.IN_VALUES, out err);
                    listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.IN_VALUES, m_table = tableVal.Copy() });

                    if (type == TaskCalculate.TYPE.OUT_VALUES) {// дополнительно получить описание выходных-нормативных параметров в алгоритме расчета
                        tableVal = Select(getQueryParameters(TaskCalculate.TYPE.OUT_VALUES), out err);
                        listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.OUT_PARAMETER, m_table = tableVal.Copy() });
                        // получить выходные значения для сессии
                        tableVal = getVariableTableValues(TaskCalculate.TYPE.OUT_VALUES, out err);
                        listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.OUT_VALUES, m_table = tableVal.Copy() });
                    } else
                        ;
                } else
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::prepareTepCalculateValues () - при получении идентифкатора сессии расчета...", Logging.INDEX_MESSAGE.NOT_SET);
            else
                ; // ошибка при регистрации соединения с БД

            return listRes;
        }
    }
}
