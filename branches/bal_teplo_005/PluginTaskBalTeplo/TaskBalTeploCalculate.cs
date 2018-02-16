using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


using InterfacePlugIn;
using TepCommon;
using ASUTP;
using System.Windows.Forms;

namespace PluginTaskBalTeplo
{
    public class HandlerDbTaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public /*partial*/ class TaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            protected override int initValues(IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
            {
                int iRes = -1;

                #region инициализация входных параметров/значений
                iRes = initValues(In
                    , listNAlg
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = TYPE.IN_VALUES, TypeState = STATE_VALUE.EDIT }]);
                #endregion

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
            public TaskBalTeploCalculate(TYPE types
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
                : base(types, listNAlg, listPutPar, dictValues)
            {
            }

            private float calculateOut(string nAlg, DateTime stamp)
            {
                float fRes = 0F,
                     fTmp = -1F; //промежуточная величина
                float sum = 0,
                    sum1 = 0;
                P_ALG.KEY_P_VALUE keyStationPValue
                    , keyPValue;
                int i = -1;

                keyStationPValue = new P_ALG.KEY_P_VALUE() { Id = ST, Stamp = stamp };

                switch (nAlg) {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue = new P_ALG.KEY_P_VALUE() { Id = ID_COMP[i], Stamp = stamp };

                            double temp = 9.771 * Math.Pow(10, -4) + 1.774 * Math.Pow(10, -5) * In["1.2"][keyPValue].value / 100
                                + 2.52 * Math.Pow(10, -5) * Math.Pow((In["1.2"][keyPValue].value / 100), 2) + 2.96 * Math.Pow(10, -6) * Math.Pow((In["1.2"][keyPValue].value / 100 - 1.5), 3) * In["1.2"][keyPValue].value / 100
                                + (3.225 * Math.Pow(10, -6) + 1.3436 * Math.Pow(10, -6) * In["1.2"][keyPValue].value / 100 + 1.684 * Math.Pow(10, -8) * Math.Pow((In["1.2"][keyPValue].value / 100), 6)
                                + 1.432 * Math.Pow(10, -7) * Math.Pow((1 / (In["1.2"][keyPValue].value / 100 + 0.5)), 3)) * ((50 - In["1.4"][keyPValue].value * 0.0980665) / 10)
                                + (3.7 * Math.Pow(10, -8) + 3.588 * Math.Pow(10, -8) * Math.Pow((In["1.2"][keyPValue].value / 100), 3) - 4.05 * Math.Pow(10, -13) * Math.Pow((In["1.2"][keyPValue].value / 100), 9)) * Math.Pow(((50 - In["1.4"][keyPValue].value * 0.0980665) / 10), 2) +
                                +1.1766 * Math.Pow(10, -13) * Math.Pow((In["1.2"][keyPValue].value / 100), 12) * Math.Pow(((50 - In["1.4"][keyPValue].value * 0.0980665) / 10), 4);

                            Out[nAlg][keyPValue].value = (float)temp /* 10000*/;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        }
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue = new P_ALG.KEY_P_VALUE() { Id = ID_COMP[i], Stamp = stamp };

                            double temp = In["1.1"][keyPValue].value;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;

                        }
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue = new P_ALG.KEY_P_VALUE() { Id = ID_COMP[i], Stamp = stamp };

                            double p = In["1.4"][keyPValue].value;
                            double t = In["1.2"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["1.4"][keyPValue].value;//Индекс обратного давления
                            double t = In["1.2"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = 0/*(float)str*/;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);

                        }
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = (In["1.1"][keyPValue].value * (Out["1.3"][keyPValue].value - In["5.2"][keyStationPValue].value)) / 1000;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["2.5"][keyPValue].value;
                            double t = In["2.3"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["2.6"][keyPValue].value;
                            double t = In["2.4"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);

                        }
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = In["7.1"][keyPValue].value;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;

                        }
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = (In["2.1"][keyPValue].value * (Out["2.1"][keyPValue].value - In["5.2"][keyStationPValue].value)) / 1000;

                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;

                        }
                        break;
                    #endregion

                    #region 3.1
                    case @"3.1": //Тепло с подпиткой сумма суммы тепла по блокам и тепла с подпиткой теплосети
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double temp = 0;
                            if (Out["3.2"][keyPValue].value == 0) {
                                calculateOut("3.2", stamp);
                            }
                            Out[nAlg][keyPValue].value = (In["3.1"][keyPValue].value * (Out["3.2"][keyPValue].value - In["5.2"][keyStationPValue].value)) / 1000;

                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 3.2
                    case @"3.2": //Энтальпия тепла
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["2.6"][keyPValue].value;
                            double t = In["2.4"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 3.3
                    case @"3.3": //Тепло по блокам с подпиткой
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double temp = 0;
                            Out[nAlg][keyPValue].value = Out["1.5"][keyStationPValue].value + Out["3.1"][keyPValue].value;

                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 4.1
                    case @"4.1": //Q БД тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = Out["2.3"][keyStationPValue].value;
                            fRes += Out[nAlg][keyPValue].value;
                        }
                        nAlg = "4.3";
                        fRes = 0;
                        goto entalp;//????
                        break;
                    #endregion

                    #region 4.2
                    case @"4.2": //Q расч тс
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double temp = 0;
                            Out[nAlg][keyPValue].value = (In["4.1"][keyPValue].value * (Out["4.3"][keyPValue].value - In["5.2"][keyStationPValue].value)) / 1000;

                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 4.3
                    case @"4.3": //Энтальпия тс
                    entalp://????
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["4.4"][keyPValue].value;
                            double t = In["4.2"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 5.1
                    case @"5.1": //Тепло вывода F1              
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = Out["2.4"][keyStationPValue].value;
                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 5.2
                    case @"5.2": //Тепло вывода F2              
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            fRes += In["2.1"][keyPValue].value * Out["2.1"][keyPValue].value - In["2.2"][keyPValue].value * Out["2.2"][keyPValue].value;
                        }

                        Out[nAlg][keyStationPValue].value = fRes / 1000;
                        break;
                    #endregion

                    #region 5.3
                    case @"5.3": //Небаланс                     
                        for (i = (int)INDX_COMP.iST; i < (int)INDX_COMP.COUNT; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double temp = 0;
                            Out[nAlg][keyPValue].value = (In["2.1"][keyStationPValue].value - In["2.2"][keyStationPValue].value - In["4.1"][keyStationPValue].value) / In["2.1"][keyStationPValue].value * 100;
                            fRes += Out[nAlg][keyPValue].value;
                        }
                        break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Q бд
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double temp = 0;
                            Out[nAlg][keyPValue].value = In["7.1"][keyPValue].value;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;
                        }
                        nAlg = "6.3";
                        fRes = 0;
                        goto entpr;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Q расч
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            Out[nAlg][keyPValue].value = (In["6.1"][keyPValue].value * (Out["6.3"][keyPValue].value - In["5.2"][keyStationPValue].value)) / 1000;

                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes;
                        }
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Энтальпия пр
                    entpr:
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["6.5"][keyPValue].value;
                            double t = In["6.3"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP2);
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
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            double p = In["6.6"][keyPValue].value;//Индекс обратного давления
                            double t = In["6.4"][keyPValue].value;
                            double temp = (49.4 + 402.5 * t / 100 + 4.767 * Math.Pow((t / 100), 2) +
                                0.0333 * Math.Pow((t / 100), 6) +
                                (-9.25 + 1.67 * t / 100 + 7.36 * Math.Pow(10, -3) * Math.Pow((t / 100), 6) -
                                0.008 * Math.Pow((1 / (t / 100 + 0.5)), 5)) * ((50 - p * 0.0980665) / 10) +
                                (-0.073 + 0.079 * t / 100 + 6.8 * Math.Pow(10, -4) * Math.Pow((t / 100), 6)) * Math.Pow(((50 - p * 0.0980665) / 10), 2) +
                                3.39 * Math.Pow(10, -8) * Math.Pow((1 / 100), 12) * Math.Pow(((50 - p * 0.0980665) / 10), 4)) / 4.1868;

                            Out[nAlg][keyPValue].value = (float)temp;
                            fRes += Out[nAlg][keyPValue].value;
                            Out[nAlg][keyStationPValue].value = fRes / ((int)INDX_COMP.iST - (int)INDX_COMP.iPP2);
                        }
                        break;
                    #endregion

                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateMaket (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
                return fRes;
            }

            private float calculateIn(string nAlg, DateTime stamp)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная величина
                float sum = 0,
                    sum1 = 0;
                int i = -1;
                P_ALG.KEY_P_VALUE keyStationPValue
                    , keyPValue;
                double temp = 0;

                keyStationPValue = new P_ALG.KEY_P_VALUE() { Id = ST, Stamp = stamp };

                switch (nAlg) {
                    #region 1.1
                    case @"1.1": //Удельный объем
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["1.1"][keyPValue].value;
                        }

                        In[nAlg][keyStationPValue].value = (float)temp;
                        keyPValue.Id = ID_COMP[i - 1]; keyPValue.Stamp = stamp;
                        fRes += In["1.1"][keyPValue].value;
                        break;
                    #endregion

                    #region 1.2
                    case @"1.2": //Расход сетевой воды с поправкой
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["1.2"][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In["1.2"][keyStationPValue].value;
                        break;
                    #endregion

                    #region 1.3
                    case @"1.3": //Энтальпия пр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["1.3"][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In["1.3"][keyStationPValue].value;
                        break;
                    #endregion

                    #region 1.4
                    case @"1.4": //Энтальпия обр
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            In["1.4"][keyPValue].value = In["1.4"][keyPValue].value/* * (float)10.197*/;
                            temp = temp + In["1.4"][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In["1.4"][keyStationPValue].value;
                        break;
                    #endregion

                    #region 1.5
                    case @"1.5": //Тепло по блокам
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["1.5"][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iOP1 - (int)INDX_COMP.iBL1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In["1.5"][keyStationPValue].value;
                        break;
                    #endregion

                    #region 2.1
                    case @"2.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }

                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 2.2
                    case @"2.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }

                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                        break;
                    #endregion

                    #region 2.3
                    case @"2.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 2.4
                    case @"2.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 2.5
                    case @"2.5":
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            In[nAlg][keyPValue].value = In[nAlg][keyPValue].value * (float)10.197;
                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 2.6
                    case @"2.6":
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            In[nAlg][keyPValue].value = In[nAlg][keyPValue].value * (float)10.197;
                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 3.3
                    case @"3.3": //T циркулир. воды ТС по блокам
                        int col = 0;
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iOP1; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["1.3"][keyPValue].value;
                            col++;
                        }
                        In[nAlg][keyStationPValue].value = (float)(temp / col);
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 3.4
                    case @"3.4":

                        In[nAlg][keyStationPValue].value = In[nAlg][keyStationPValue].value /** (float)10.197*/;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 4.3
                    case @"4.3": //T обратной воды ТС по выводам
                        double temp_vzves = 0;
                        for (i = (int)INDX_COMP.iOP1; i < (int)INDX_COMP.iPP2; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In["2.2"][keyPValue].value;
                            temp_vzves = temp_vzves + (In["2.2"][keyPValue].value * In["2.4"][keyPValue].value);
                        }
                        In[nAlg][keyStationPValue].value = (float)(temp_vzves / temp);
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 4.4
                    case @"4.4":
                        In[nAlg][keyStationPValue].value = In[nAlg][keyStationPValue].value * (float)10.197;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 5.1
                    //case @"5.1":

                    //    In[nAlg][keyStationPValue].value = In[nAlg][keyStationPValue].value * (float)10.197;
                    //    fRes += In[nAlg][keyStationPValue].value;
                    //    break;
                    #endregion

                    #region 6.1
                    case @"6.1": //Энтальпия пр вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }

                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 6.2
                    case @"6.2": //Энтальпия обр вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }

                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                        break;
                    #endregion

                    #region 6.3
                    case @"6.3": //Q БД вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 6.4
                    case @"6.4": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 6.5
                    case @"6.5": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            In[nAlg][keyPValue].value = In[nAlg][keyPValue].value * (float)10.197;
                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    #region 6.6
                    case @"6.6": //Q расч вывод
                        for (i = (int)INDX_COMP.iPP2; i < (int)INDX_COMP.iST; i++) {
                            keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                            In[nAlg][keyPValue].value = In[nAlg][keyPValue].value * (float)10.197;
                            temp = temp + In[nAlg][keyPValue].value;
                        }
                        temp = temp / ((int)INDX_COMP.iPP2 - (int)INDX_COMP.iOP1);
                        In[nAlg][keyStationPValue].value = (float)temp;
                        fRes += In[nAlg][keyStationPValue].value;
                        break;
                    #endregion

                    default:
                        break;
                }
                return fRes;
            }

            public override void Execute(Action<TYPE, IEnumerable<VALUE>, RESULT> delegateResultListValue, Action<TYPE, int, RESULT> delegateResultNAlg)
            {
                var items = from pair in In
                            orderby pair.Key ascending
                            select pair;
                //??? зачем расчет входных
                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items) {
                    //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                    calculateIn(pAlg.Key, DateTime.MinValue); //??? дата/время все равно одинаковая для всех
                }
                // преобразование в список со значениями
                delegateResultListValue(TYPE.IN_VALUES, resultToListValue(_dictPAlg[TYPE.IN_VALUES]), RESULT.Ok);

                items = from pair in Out
                        orderby pair.Key ascending
                        select pair;
                // расчет
                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in items) {
                    //pAlg.Value[ID_COMP[ST]].value = calculateOut(pAlg.Key);
                    calculateOut(pAlg.Key, DateTime.MinValue); //??? дата/время все равно одинаковая для всех
                }
                // преобразование в список со значениями
                delegateResultListValue(TYPE.OUT_VALUES, resultToListValue(_dictPAlg[TYPE.OUT_VALUES]), RESULT.Ok);
            }
        }

        /// <summary>
        /// Таблицы со значениями для редактирования входные
        /// </summary>
        public DataTable[] m_arTableOrigin_in
            , m_arTableEdit_in;

        public DataTable m_dt_profile;

        /// <summary>
        /// Таблицы со значениями для редактирования выходные
        /// </summary>
        public DataTable[] m_arTableOrigin_out
            , m_arTableEdit_out;

        /// <summary>
        /// Получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <param name="dateBegin">Дата</param>
        /// <returns>таблица</returns>
        public DataTable getStructurOutval(out int err, DateTime dateBegin)
        {
            string strRes = string.Empty;
            DataTable res = new DataTable();

            strRes = "SELECT * FROM "
                + GetNameTableOut(dateBegin);

            res = Select(strRes, out err).Clone();
            res.Columns.Remove("ID");
            return res;
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">Дата</param>
        /// <returns>Имя таблицы</returns>
        public string GetNameTableOut(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;

            strRes = TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert"></param>
        /// <returns>Имя таблицы</returns>
        public string GetNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;

            strRes = TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.TYPE types
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
        {
            return new TaskBalTeploCalculate(types, listNAlg, listPutPar, dictValues);
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
                                , ASUTP.Helper.HUsers.Id.ToString()
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
                                , ASUTP.Helper.HUsers.Id.ToString()
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
        /// ??? Формирование таблицы вых. значений
        /// </summary>
        /// <param name="editTable">таблица</param>
        /// <param name="dgvView">отображение</param>
        /// <param name="dtOut">таблица с вых.зн.</param>
        public DataTable FillTableValueDay(DataTable editTable, DataGridView dgvView, DataTable dtOut)
        {
            //Array namePut = Enum.GetValues(typeof(INDEX_GTP));
            //string put;
            //double valueToRes;
            //editTable.Rows.Clear();

            //foreach (DataGridViewRow row in dgvView.Rows)
            //{
            //    if (Convert.ToDateTime(row.Cells["Date"].Value) < DateTime.Now.Date)
            //    {
            //        for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
            //        {
            //            put = dtOut.Rows[i]["ID"].ToString();
            //            valueToRes = Convert.ToDouble(row.Cells[namePut.GetValue(i).ToString()].Value) * Math.Pow(10, 6);

            //            editTable.Rows.Add(new object[] 
            //            {
            //                put
            //                , -1
            //                , 1.ToString()
            //                , valueToRes                
            //                , Convert.ToDateTime(row.Cells["Date"].Value.ToString()).ToString(CultureInfo.InvariantCulture)
            //                , i
            //            });
            //        }
            //    }
            //}
            return editTable;
        }

        ///// <summary>
        ///// Подготовить таблицы для проведения расчета
        ///// </summary>
        ///// <param name="err">Признак ошибки при выполнении функции</param>
        ///// <returns>Массив таблиц со значениями для расчета</returns>
        //protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        //{
        //    TaskCalculate.ListDATATABLE listRes = new TaskCalculate.ListDATATABLE();
        //    err = -1;

        //    //long idSession = -1;
        //    DataTable tableVal = null;

        //    if (isRegisterDbConnection == true)
        //        // проверить наличие сессии
        //        if (_Session.m_Id > 0) {
        //            // получить описание входных парметров в алгоритме расчета
        //            tableVal = Select(getQueryParameters(TaskCalculate.TYPE.IN_VALUES), out err);
        //            listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.IN_PARAMETER, m_table = tableVal.Copy() });
        //            // получить входные значения для сессии
        //            tableVal = getVariableTableValues(TaskCalculate.TYPE.IN_VALUES, out err);
        //            listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.IN_VALUES, m_table = tableVal.Copy() });

        //            if (type == TaskCalculate.TYPE.OUT_VALUES) {// дополнительно получить описание выходных-нормативных параметров в алгоритме расчета
        //                tableVal = Select(getQueryParameters(TaskCalculate.TYPE.OUT_VALUES), out err);
        //                listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.OUT_PARAMETER, m_table = tableVal.Copy() });
        //                // получить выходные значения для сессии
        //                tableVal = getVariableTableValues(TaskCalculate.TYPE.OUT_VALUES, out err);
        //                listRes.Add(new TaskCalculate.DATATABLE() { m_indx = TaskCalculate.INDEX_DATATABLE.OUT_VALUES, m_table = tableVal.Copy() });
        //            } else
        //                ;
        //        } else
        //            Logging.Logg().Error(@"HandlerDbTaskCalculate::prepareTepCalculateValues () - при получении идентифкатора сессии расчета...", Logging.INDEX_MESSAGE.NOT_SET);
        //    else
        //        ; // ошибка при регистрации соединения с БД

        //    return listRes;
        //}
    }
}
