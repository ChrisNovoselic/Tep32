using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public partial class HandlerDbTaskCalculate : HandlerDbValues
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public partial class TaskTepCalculate : TaskCalculate
        {
            private float calculateMaket(string nAlg)
            {
                float fRes = 0F,
                     fTmp = -1F;//промежуточная велечина
                float sum = 0,
                    sum1 = 0;
                int i = -1;

                switch (nAlg)
                {
                    #region 1 - Ny cp
                    case @"1": //
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 200;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 2 - Nm
                    case @"2": //
                        fRes = In[@"76"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 3 - Qy cp
                    case @"3": //  
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 240;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 4 - Эвыр
                    case @"4": //
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"2"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"2"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 5 - TAU э и
                    case @"5": //
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"4"][ID_COMP[i]].value / Out[@"1"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"4"][ID_COMP[ST]].value / Out[@"1"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 6 - Qпо
                    case @"6":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"4"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"4"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 7 - Qто
                    case @"7":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"3"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"3"][ID_COMP[ST]].value;
                        break;
                    #endregion
                    case @"8":

                        break;

                    #region 9 - Q
                    case @"9":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"6"][ID_COMP[i]].value + Out[@"7"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 10 - TAU т и
                    case @"10":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"9"][ID_COMP[i]].value / Out[@"3"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"9"][ID_COMP[ST]].value / Out[@"3"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 11 - Qотп
                    case @"11":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"8"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"8"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 12 - Qот гв
                    case @"12":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"11"][ID_COMP[i]].value - In[@"83"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 13 - Qот отр
                    case @"13":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"8"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"8"][ID_COMP[ST]].value;
                        break;
                    #endregion
                    case @"14":

                        break;

                    #region 15 - Qэ
                    case @"15":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = ((Norm[@"49"][ID_COMP[i]].value * Norm[@"57.1"][ID_COMP[i]].value
                                + (Norm[@"55"][ID_COMP[i]].value) * (Norm[@"59.1"][ID_COMP[i]].value) - Norm[@"60"][ID_COMP[i]].value
                                - Norm[@"66"][ID_COMP[i]].value * Norm[@"58"][ID_COMP[i]].value) / 1000 - (Norm[@"3"][ID_COMP[i]].value
                                + Norm[@"4"][ID_COMP[i]].value));
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 16 - Qт сн(н)
                    case @"16":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"41"][ID_COMP[i]].value
                                * Out[@"15"][ID_COMP[i]].value / 100 + (float)15.4 * (In[@"68"][ID_COMP[i]].value
                                - In[@"69"][ID_COMP[i]].value);
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 16.1 - Qт сн
                    case @"16.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (Out[@"16"][ID_COMP[i]].value + Out[@"18"][ID_COMP[i]].value == 0)
                                Out[nAlg][ID_COMP[i]].value = 0;
                            else
                                Out[nAlg][ID_COMP[i]].value = (Out[@"9"][ID_COMP[i]].value - Out[@"11"][ID_COMP[i]].value) *
                                    Out[@"16"][ID_COMP[i]].value / (Out[@"16"][ID_COMP[i]].value + Out[@"18"][ID_COMP[i]].value);

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 17 - Qк бр
                    case @"17":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"64"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 18 - Qк сн(н)
                    case @"18":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"121"][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value / 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 19 - Qк сн
                    case @"19":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (Out[@"9"][ID_COMP[i]].value - Out[@"11"][ID_COMP[i]].value) - Out[@"16.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 20 - TAU т раб
                    case @"20":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"1"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"1"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 21 - TAU т рез
                    case @"21":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"73"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 22 -
                    case @"22":

                        break;
                    #endregion

                    #region 23 -
                    case @"23":

                        break;
                    #endregion

                    #region 24 - n т
                    case @"24":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"68"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 25 - n т(н)
                    case @"25":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"69"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 25.1 - dB (n k)
                    case @"25.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (float)64.2 * (Out[@"24"][ID_COMP[i]].value - Out[@"25"][ID_COMP[i]].value);
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 26 -
                    case @"26":

                        break;
                    #endregion

                    #region 27 - Этепл
                    case @"27":
                        if (isRealTime == true)
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (Norm[@"8"][ID_COMP[i]].value == 0)
                                    Out[nAlg][ID_COMP[i]].value = 0;
                                else
                                    Out[nAlg][ID_COMP[i]].value = In[@"10"][ID_COMP[i]].value + (In[@"10.4"][ID_COMP[ST]].value +
                                        In[@"11.1"][ID_COMP[ST]].value + In[@"11.2"][ID_COMP[ST]].value + In[@"12"][ID_COMP[ST]].value) / n_blokov1;

                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        else
                        {
                            //float sum = 0;
                            //float sum1 = 0;
                            for (int j = 0; j < n_blokov; j++)
                            {
                                sum += Norm[@"8"][ID_COMP[j]].value;
                                sum1 += In[@"10"][ID_COMP[j]].value;
                            }

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (sum == 0)
                                    Out[nAlg][ID_COMP[i]].value = 0;
                                else
                                    Out[nAlg][ID_COMP[i]].value = In[@"10"][ID_COMP[i]].value + (In[@"10"][ID_COMP[ST]].value
                                        - sum1) * In[@"47"][ID_COMP[i]].value / sum; ;

                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }

                        break;
                    #endregion

                    #region 28 - Эт сн
                    case @"28":
                        if (isRealTime == true)
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"4"][ID_COMP[i]].value + In[@"4"][ID_COMP[ST]].value;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < n_blokov; j++)
                                sum += In[@"4"][ID_COMP[j]].value;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"4"][ID_COMP[i]].value + (In[@"4"][ID_COMP[ST]].value - sum) *
                                    Out[@"4"][ID_COMP[i]].value / Out[@"4"][ID_COMP[ST]].value;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        break;
                    #endregion

                    #region 29 - Эк сн
                    case @"29":
                        if (isRealTime == true)
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"7"][ID_COMP[i]].value + In[@"7"][ID_COMP[ST]].value;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < n_blokov; j++)
                                sum += In[@"7"][ID_COMP[j]].value;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"7"][ID_COMP[i]].value + (In[@"7"][ID_COMP[ST]].value - sum) *
                                    Out[@"4"][ID_COMP[i]].value / Out[@"4"][ID_COMP[ST]].value;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        break;
                    #endregion

                    #region 30 - Эсн
                    case @"30":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"29"][ID_COMP[i]].value + Out[@"28"][ID_COMP[i]].value + Out[@"27"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 31 - Эот
                    case @"31":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"4"][ID_COMP[i]].value - Out[@"30"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 32 - Qнас
                    case @"32":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (float)0.001 * 860 * (In[@"10.1"][ID_COMP[i]].value + In[@"10.2"][ID_COMP[i]].value) *
                                85 / 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 33 - Kэ
                    case @"33":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In[@"47"][ID_COMP[i]].value == 0) ? 1 : (Out[@"15"][ID_COMP[i]].value + Out[@"16.1"][ID_COMP[i]].value
                                + Norm[@"47"][ID_COMP[i]].value) / (Out[@"15"][ID_COMP[i]].value + Out[@"16.1"][ID_COMP[i]].value + Norm[@"47"][ID_COMP[i]].value
                                + (Out[@"11"][ID_COMP[i]].value - Out[@"32"][ID_COMP[i]].value)
                                * (100 + Norm[@"125"][ID_COMP[i]].value) / 100);
                            //??? double cikl1
                            //SUM???
                            if (In[@"47"][ID_COMP[i]].value == 0)
                                fRes += 1;
                            else
                                fRes += (Out[@"15"][ID_COMP[ST]].value + Out[@"16.1"][ID_COMP[ST]].value + Norm[@"47"][ID_COMP[ST]].value
                                    + Out[@"11"][ID_COMP[ST]].value - Out[@"32"][ID_COMP[ST]].value) * (100 + Norm[@"125"][ID_COMP[ST]].value) / 100;
                        }
                        break;
                    #endregion

                    #region 34 - Ээ сн
                    case @"34":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"29"][ID_COMP[i]].value * Out[@"33"][ID_COMP[i]].value + Out[@"28"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 35 - Этэ сн
                    case @"35":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"30"][ID_COMP[i]].value - Out[@"34"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 36 - Эт сн(н)
                    case @"36":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"36"][ID_COMP[i]].value * Out[@"4"][ID_COMP[i]].value / 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 37 - Эк сн(н)
                    case @"37":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"108"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 38 - Этепл сн(н)
                    case @"38":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"144"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 39 - Ээ сн(н)
                    case @"39":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"128"][ID_COMP[i]].value * Out[@"4"][ID_COMP[i]].value / 100;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 40 - Этэ сн(н)
                    case @"40":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"36"][ID_COMP[i]].value + Out[@"37"][ID_COMP[i]].value
                                + Out[@"38"][ID_COMP[i]].value - Out[@"39"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 41 - Эцн
                    case @"41":
                        if (isRealTime == true)
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"6"][ID_COMP[ST]].value / n_blokov1;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        else
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"6"][ID_COMP[ST]].value * Out[@"4"][ID_COMP[i]].value / Out[@"4"][ID_COMP[ST]].value;
                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        break;
                    #endregion

                    #region 42 - Эцн(н)
                    case @"42":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"29"][ID_COMP[ST]].value * In[@"70"][ID_COMP[ST]].value
                                * Out[@"4"][ID_COMP[i]].value + Out[@"4"][ID_COMP[ST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 43 - Эпэн
                    case @"43":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"8.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 44 - Gпв
                    case @"44":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"66"][ID_COMP[i]].value / 1000;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 45 - Эпэн (н)
                    case @"45":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"99"][ID_COMP[i]].value * Out[@"44"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 46 - Дт
                    case @"46":
                        //??? 
                        float param = 0;
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"47"][ID_COMP[i]].value.ToString() == "2a")
                                //LOG???
                                param = In[@"38"][ID_COMP[i]].value;
                            else
                                //LOG???
                                param = In[@"37"][ID_COMP[i]].value;

                            //???pow==^
                            param =
                                (float)Math.Pow((float)2.6864264 - 0.20096551 * param - 2.16688 / 1000 * Math.Pow(param, 2)
                                - 9.480808 / 1E5 * Math.Pow(param, 3) + 6.135062 / 1E6 * Math.Pow(param, 4) + 3.6917245 / 1E6
                                * Math.Pow(param, 5), -1);
                            //??? 
                            param = (float)(-753.317 + 6959.4093 * param - 29257.981 * Math.Pow(param, 2)
                                + 71285.169 * Math.Pow(param, 3) - 86752.84 * Math.Pow(param, 4)
                                + 42641.056 * Math.Pow(param, 5));

                            Out[nAlg][ID_COMP[i]].value = Out[@"7"][ID_COMP[i]].value * 1000
                                / (Norm[@"62"][ID_COMP[i]].value - param);

                            //fRes += Out[nAlg][ID_COMP[i]].value;
                        }

                        break;
                    #endregion

                    #region 47 - Дрег
                    case @"47":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"46"][ID_COMP[i]].value * (Norm[@"58"][ID_COMP[i]].value - 107)
                                / (728 - Norm[@"58"][ID_COMP[i]].value);
                        }
                        break;
                    #endregion

                    #region 48 - Этф п
                    case @"48":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"26"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 49 - Этф т
                    case @"49":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"74"][ID_COMP[i]].value.ToString() == "1")
                                Out[nAlg][ID_COMP[i]].value = 0;
                            else
                                Out[nAlg][ID_COMP[i]].value = (Out[@"47"][ID_COMP[i]].value * (Norm[@"57.1"][ID_COMP[i]].value
                                    + (Norm[@"59.1"][ID_COMP[i]].value - Norm[@"60"][ID_COMP[i]].value) - Norm[@"62"][ID_COMP[i]].value)
                                    + Out[@"47"][ID_COMP[i]].value * (Norm[@"57.1"][ID_COMP[i]].value + (Norm[@"59.1"][ID_COMP[i]].value
                                    - Norm[@"60"][ID_COMP[i]].value) - 752)) / 860;

                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 50 - Этф
                    case @"50":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"48"][ID_COMP[i]].value + Out[@"49"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 51 - Этф
                    case @"51":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"50"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"50"][ID_COMP[ST]].value / Out[@"4"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 52 - Wтф т
                    case @"52":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"49"][ID_COMP[i]].value / Out[@"7"][ID_COMP[i]].value * 1000;
                        }
                        fRes = Out[@"49"][ID_COMP[ST]].value / Out[@"7"][ID_COMP[ST]].value * 1000;
                        break;
                    #endregion

                    #region 53 - Wтф п
                    case @"53":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"27"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region
                    case @"54":

                        break;
                    #endregion

                    #region 55 - qт бр
                    case @"55":
                        fRes = Out[@"15"][ID_COMP[ST]].value / Out[@"4"][ID_COMP[ST]].value * 1000;
                        break;
                    #endregion

                    #region 56 - qт бр(н)
                    case @"56":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"24"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"24"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 57 - (q3+q4)/н
                    case @"57":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (.05F * In[@"56"][ID_COMP[i]].value / (100 - In[@"56"][ID_COMP[i]].value)
                                + .95F * In[@"57"][ID_COMP[i]].value / (100 - In[@"57"][ID_COMP[i]].value))
                                * 7800 * In[@"55"][ID_COMP[ST]].value / 1E2F / In[@"53"][ID_COMP[ST]].value;

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 58 - q3(н)+q4(н)
                    case @"58":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"73"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"73"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 59 - alfa p
                    case @"59":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (21 - (float)(.02F * Norm[@"89"][ID_COMP[i]].value / 1E2F + .1F
                                * (Norm[@"59"][ID_COMP[i]].value / 1E2) * (In[@"35"][ID_COMP[i]].value + In[@"36"][ID_COMP[i]].value)
                                / 2) / (21 - (In[@"35"][ID_COMP[i]].value + In[@""][ID_COMP[i]].value) / 2));

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 60 - alfa p(н)
                    case @"60":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"67"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 61 - dalfa pyx
                    case @"61":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"44"][ID_COMP[i]].value * (float)Math.Sqrt(472.2F / Norm[@"65"][ID_COMP[i]].value);
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 62 - dalfa pyx(н)
                    case @"62":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"68"][ID_COMP[i]].value * 1E2F;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 63 - dalfa yx-д
                    case @"63":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"44.1"][ID_COMP[i]].value * (float)Math.Sqrt(446.1 / Norm[@"65"][ID_COMP[i]].value);
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 64 - t хв
                    case @"64":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In[@"31"][ID_COMP[i]].value * In[@"31.1"][ID_COMP[i]].value) / 2;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 65 - t 'вп
                    case @"65":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In[@"32"][ID_COMP[i]].value * In[@"32.1"][ID_COMP[i]].value) / 2;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 66 - t yx
                    case @"66":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In[@"39"][ID_COMP[i]].value * In[@"40"][ID_COMP[i]].value) / 2;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 67 - t yx(y)
                    case @"67":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"79"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"79"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 67.1 - dq2(t yx)
                    case @"67.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (Out[@"69"][ID_COMP[i]].value * (Out[@"66"][ID_COMP[i]].value - Out[@"67"][ID_COMP[i]].value))
                                / (Out[@"67"][ID_COMP[i]].value - ((Out[@"60"][ID_COMP[i]].value + Out[@"62"][ID_COMP[i]].value / 1E2F) * Out[@"64"][ID_COMP[i]].value
                                / (Out[@"60"][ID_COMP[i]].value + Out[@"62"][ID_COMP[i]].value / 1E2F) + Norm[@"82"][ID_COMP[i]].value));
                        }
                        break;
                    #endregion

                    #region 67.2 - dB(t yx)
                    case @"67.2":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value * Out[@"67.1"][ID_COMP[i]].value / Out[@"74"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 68 - q2
                    case @"68":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (Norm[@"80"][ID_COMP[i]].value * (Out[@"59"][ID_COMP[i]].value
                                + Out[@"61"][ID_COMP[i]].value / 1E2F) + Norm[@"81"][ID_COMP[i]].value) * (Out[@"66"][ID_COMP[i]].value
                                - (Out[@"59"][ID_COMP[i]].value + Out[@"61"][ID_COMP[i]].value / 1E2F) / ((Out[@"59"][ID_COMP[i]].value
                                    + Out[@"61"][ID_COMP[i]].value / 1E2F) + Norm[@"82"][ID_COMP[i]].value)
                                    * Out[@"64"][ID_COMP[i]].value) * (.9805F + .00013F * Out[@"66"][ID_COMP[i]].value)
                                        * (1 - .01F * Out[@"57"][ID_COMP[i]].value) / 1E2F + (.2F - .95F * In[@"55"][ID_COMP[ST]].value
                                        * Out[@"89"][ID_COMP[i]].value / 1E2F * Out[@"66"][ID_COMP[ST]].value) / In[@"53"][ID_COMP[ST]].value;

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 69 - q2(н)
                    case @"69":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"83"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"83"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 70 - D0
                    case @"70":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"49"][ID_COMP[i]].value / 1E3F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 71 - q пуск
                    case @"71":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"68"][ID_COMP[i]].value * 64.2F * 7 * 1E2F / (Out[@"17"][ID_COMP[i]].value * 1E2F
                                / (100 - Out[@"68"][ID_COMP[i]].value - Out[@"57"][ID_COMP[i]].value - Norm[@"84"][ID_COMP[i]].value
                                - Norm[@"85"][ID_COMP[i]].value) + 85.0F * 7);
                        }
                        break;
                    #endregion

                    #region 72 - q пуск(н)
                    case @"72":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"87"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 73 - КПДк бр
                    case @"73":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 100 - Out[@"68"][ID_COMP[i]].value - Out[@"57"][ID_COMP[i]].value
                                - Norm[@"84"][ID_COMP[i]].value - Norm[@"85"][ID_COMP[i]].value - Out[@"71"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 74 - КПДк бр(н)
                    case @"74":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"88"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"88"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 75 - B
                    case @"75":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"17"][ID_COMP[i]].value * 1E2f / 7 / Out[@"73"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 76 - В г
                    case @"76":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value * In[@"59"][ID_COMP[i]].value / 1E2F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 77 - В м
                    case @"77":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value * In[@"60"][ID_COMP[i]].value / 1E2F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 78 - В тв
                    case @"78":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value - Out[@"76"][ID_COMP[i]].value - Out[@"77"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 79 - В э
                    case @"79":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value * Out[@"33"][ID_COMP[i]].value * Out[@"31"][ID_COMP[i]].value
                                / (Out[@"4"][ID_COMP[i]].value - Out[@"34"][ID_COMP[i]].value);
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 80 - В тэ
                    case @"80":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"75"][ID_COMP[i]].value - Out[@"79"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 81 - b э
                    case @"81":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"79"][ID_COMP[i]].value * 1E3F / Out[@"31"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"79"][ID_COMP[ST]].value * 1E3F / Out[@"31"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 82 - b э н
                    case @"82":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"136"][ID_COMP[i]].value;
                        }
                        //??realtime
                        switch (m_indxCompRealTime)
                        {
                            case INDX_COMP.iBL1:
                                fRes = Out[nAlg][ID_COMP[BL1]].value;
                                break;
                            case INDX_COMP.iBL2:
                                fRes = Out[nAlg][ID_COMP[BL2]].value;
                                break;
                            case INDX_COMP.iBL3:
                                fRes = Out[nAlg][ID_COMP[BL3]].value;
                                break;
                            case INDX_COMP.iBL4:
                                fRes = Out[nAlg][ID_COMP[BL4]].value;
                                break;
                            case INDX_COMP.iBL5:
                                fRes = Out[nAlg][ID_COMP[BL5]].value;
                                break;
                            case INDX_COMP.iBL6:
                                fRes = Out[nAlg][ID_COMP[BL6]].value;
                                break;
                            default:
                                fRes = Out[nAlg][ID_COMP[ST]].value;
                                break;
                        }
                        break;
                    #endregion

                    #region 83 - b э нр
                    case @"83":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"137"][ID_COMP[i]].value;
                        }
                        //??realtime
                        switch (m_indxCompRealTime)
                        {
                            case INDX_COMP.iBL1:
                                fRes = Out[nAlg][ID_COMP[BL1]].value;
                                break;
                            case INDX_COMP.iBL2:
                                fRes = Out[nAlg][ID_COMP[BL2]].value;
                                break;
                            case INDX_COMP.iBL3:
                                fRes = Out[nAlg][ID_COMP[BL3]].value;
                                break;
                            case INDX_COMP.iBL4:
                                fRes = Out[nAlg][ID_COMP[BL4]].value;
                                break;
                            case INDX_COMP.iBL5:
                                fRes = Out[nAlg][ID_COMP[BL5]].value;
                                break;
                            case INDX_COMP.iBL6:
                                fRes = Out[nAlg][ID_COMP[BL6]].value;
                                break;
                            default:
                                fRes = Out[nAlg][ID_COMP[ST]].value;
                                break;
                        }
                        break;
                    #endregion

                    #region 84 - b тэ
                    case @"84":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"80"][ID_COMP[i]].value * 1E3F / Out[@"11"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"80"][ID_COMP[ST]].value * 1E3F / Out[@"11"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 85 - b тэ (н)
                    case @"85":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"148"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"148"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 86 - b тэ нр
                    case @"86":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"150"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"148"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 87 -
                    case @"87":

                        break;
                    #endregion

                    #region 88 -
                    case @"88":

                        break;
                    #endregion

                    #region 89 - dQэ по(отр)
                    case @"89":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"45"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"45"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 90 - dQэ то(отр)
                    case @"90":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"46"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"46"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 91 - Котр(к) э
                    case @"91":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"135"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"79"][ID_COMP[i]].value;
                            sum1 += Out[@"135"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 92 - Котр(к) тэ
                    case @"92":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"135"][ID_COMP[i]].value;

                            sum += (Out[nAlg][ID_COMP[i]].value - Out[@"79"][ID_COMP[i]].value - Out[@"81"][ID_COMP[i]].value
                                * Out[@"27"][ID_COMP[i]].value / 1E3F) * Out[@"92"][ID_COMP[i]].value;
                            sum1 += (Out[nAlg][ID_COMP[i]].value - Out[@"79"][ID_COMP[i]].value - Out[@"81"][ID_COMP[i]].value
                                * Out[@"27"][ID_COMP[i]].value / 1E3F);
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 93 - Po
                    case @"93":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"56.1"][ID_COMP[i]].value;

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"70"][ID_COMP[i]].value;
                            sum1 += Out[@"70"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 94 - Po(н)
                    case @"94":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = fTable.F1(@"2.65a:1", Norm[@"50"][ID_COMP[i]].value);

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"70"][ID_COMP[i]].value;
                            sum1 += Out[@"70"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 95 - alfa qт(Po)
                    case @"95":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"74"][ID_COMP[i]].value.ToString() == "1")
                                Out[nAlg][ID_COMP[i]].value = fTable.F2(@"2.66:2", Norm[@"50"][ID_COMP[i]].value, Out[@"93"][ID_COMP[i]].value);
                            else
                            {
                                if (Norm[@"10"][ID_COMP[i]].value <= 60 && Norm[@"50"][ID_COMP[i]].value <= 510 && Out[@"93"][ID_COMP[i]].value <= 130)
                                    Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                else
                                {
                                    if (Norm[@"10"][ID_COMP[i]].value <= 60)
                                    {
                                        if (Norm[@"50"][ID_COMP[i]].value <= 510)
                                        {
                                            if (Out[@"93"][ID_COMP[i]].value <= 130)
                                                Out[nAlg][ID_COMP[i]].value = -0.32F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                            else
                                                Out[nAlg][ID_COMP[i]].value = 0.32F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                        }
                                        else
                                        {
                                            if (Out[@"93"][ID_COMP[i]].value <= 130)
                                                Out[nAlg][ID_COMP[i]].value = 0.7F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                            else
                                                Out[nAlg][ID_COMP[i]].value = -0.7F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                        }
                                    }
                                    else
                                    {
                                        if (Norm[@"50"][ID_COMP[i]].value <= 510)
                                        {
                                            if (Out[@"93"][ID_COMP[i]].value <= 130)
                                                Out[nAlg][ID_COMP[i]].value = -0.37F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                            else
                                                Out[nAlg][ID_COMP[i]].value = 0.37F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                        }
                                        else
                                        {
                                            if (Out[@"93"][ID_COMP[i]].value <= 125)
                                                Out[nAlg][ID_COMP[i]].value = 0.76F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                            else
                                            {
                                                if (Out[@"93"][ID_COMP[i]].value <= 130)
                                                    Out[nAlg][ID_COMP[i]].value = 1.04F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                                else
                                                    Out[nAlg][ID_COMP[i]].value = -0.8F * Math.Abs(Out[@"93"][ID_COMP[i]].value - Out[@"94"][ID_COMP[i]].value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //                   if(inm(getIndexOfIInM("74"),i)=="1",F2(iom(getIndexOfIIoM("50"),i),oum(getIndexOfIOutM("93"),i),"2.66:2"),
                        //                    IIF(inm(getIndexOfIInM("74"),i)=="2" OR inm(getIndexOfIInM("74"),i)=="2à" OR inm(getIndexOfIInM("74"),i)=="3",
                        //   IIF(iom(getIndexOfIIoM("10"),i)<=60,
                        //       IIF(iom(getIndexOfIIoM("50"),i)<=510,IIF(oum(getIndexOfIOutM("93"),i)<=130,-0.32,0.32)
                        //                  * ABS(oum(getIndexOfIOutM("93"),i)-oum(getIndexOfIOutM("94"),i)),
                        //                  IIF(oum(getIndexOfIOutM("93"),i)<=130,0.7,-0.7)
                        //                                              *ABS(oum(getIndexOfIOutM("93"),i)-oum(getIndexOfIOutM("94"),i))),
                        //                                  IIF(iom(getIndexOfIIoM("50"),i)<=510,IIF(oum(getIndexOfIOutM("93"),i)<=130,-0.37,0.37)
                        //*ABS(oum(getIndexOfIOutM("93"),i)-oum(getIndexOfIOutM("94"),i)),
                        //IIF(oum(getIndexOfIOutM("93"),i)<=125,0.76,
                        //IIF(oum(getIndexOfIOutM("93"),i)<=130,1.04,-0.8))
                        //                                                  *ABS(oum(getIndexOfIOutM("93"),i)-oum(getIndexOfIOutM("94"),i)))),1/0))

                        break;
                    #endregion

                    #region 96 - Pп
                    case @"96":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"23"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"6"][ID_COMP[i]].value;
                            sum1 += Out[@"6"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 97 - Pт
                    case @"97":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            switch (In[@"74"][ID_COMP[i]].value.ToString())
                            {
                                case "2a":
                                    Out[nAlg][ID_COMP[i]].value = In[@"38"][ID_COMP[i]].value;
                                    break;
                                default:
                                    Out[nAlg][ID_COMP[i]].value = In[@"37"][ID_COMP[i]].value;
                                    break;
                            }

                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"7"][ID_COMP[i]].value;
                            sum1 += Out[@"7"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 98 - t o
                    case @"98":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"51.1"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"4"][ID_COMP[i]].value;
                            sum1 += Out[@"4"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 99 - t o (н)
                    case @"99":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 540;
                        }
                        fRes = 540;
                        break;
                    #endregion

                    #region 100 - alfa qт(t o)
                    case @"100":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"74"][ID_COMP[i]].value.ToString() == "1")
                                Out[nAlg][ID_COMP[i]].value = fTable.F1(@"2.70:1", Out[@"98"][ID_COMP[i]].value);
                            else
                                if (In[@"74"][ID_COMP[i]].value.ToString() == "2" || In[@"74"][ID_COMP[i]].value.ToString() == "2a"
                                    || In[@"74"][ID_COMP[i]].value.ToString() == "3")
                                    Out[nAlg][ID_COMP[i]].value = fTable.F3(@"2.71:3", Norm[@"50"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Out[@"98"][ID_COMP[i]].value);
                                else
                                {
                                    //??error
                                    //1 / 0;
                                }
                        }
                        break;
                    #endregion

                    #region 101 - t цдс
                    case @"101":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"52.1"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"4"][ID_COMP[i]].value;
                            sum1 += Out[@"4"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 102 - t цдс(н)
                    case @"102":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 540;
                        }
                        fRes = 540;
                        break;
                    #endregion

                    #region 103 - alfa qт(цдс)
                    case @"103":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"74"][ID_COMP[i]].value.ToString() == "1")
                                Out[nAlg][ID_COMP[i]].value = fTable.F1(@"2.72:1", Out[@"101"][ID_COMP[i]].value);
                            else
                                if (In[@"74"][ID_COMP[i]].value.ToString() == "2" || In[@"74"][ID_COMP[i]].value.ToString() == "2a"
                                    || In[@"74"][ID_COMP[i]].value.ToString() == "3")
                                    Out[nAlg][ID_COMP[i]].value = fTable.F3(@"2.73:3", Norm[@"50"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Out[@"101"][ID_COMP[i]].value);
                                else
                                {
                                    //??error
                                    //1 / 0;
                                }
                        }
                        break;
                    #endregion

                    #region 104 - Эконд
                    case @"104":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"4"][ID_COMP[i]].value - Out[@"50"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"4"][ID_COMP[ST]].value - Out[@"50"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 105 - P2
                    case @"105":
                        //???ALTERC
                        Out[nAlg][ID_COMP[0]].value = In[@"30"][ID_COMP[i]].value / 98.067F;
                        Out[nAlg][ID_COMP[1]].value = In[@"30"][ID_COMP[i]].value / 98.067F;
                        Out[nAlg][ID_COMP[2]].value = In[@"30"][ID_COMP[i]].value / 98.067F;
                        Out[nAlg][ID_COMP[3]].value = In[@"30"][ID_COMP[i]].value / 98.067F;
                        Out[nAlg][ID_COMP[4]].value = In[@"30"][ID_COMP[i]].value / 98.067F;
                        Out[nAlg][ID_COMP[5]].value = In[@"30"][ID_COMP[i]].value;

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 106 - P2(н)
                    case @"106":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"15"][ID_COMP[i]].value / 98.067F;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 107 - dN(P2)
                    case @"107":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (Norm[@"30"][ID_COMP[i]].value <= 100)
                                Out[nAlg][ID_COMP[i]].value = fTable.F1("2.45:1", Norm[@"14"][ID_COMP[i]].value) * (Out[@"105"][ID_COMP[i]].value
                                    - Out[@"106"][ID_COMP[i]].value) / .01F;
                            else
                                if (Norm[@"14"][ID_COMP[i]].value > 100)
                                    Out[nAlg][ID_COMP[i]].value = 1.06F * Out[@"105"][ID_COMP[i]].value - Out[@"106"][ID_COMP[i]].value / .01F;
                                else
                                    ;
                        }
                        break;
                    #endregion

                    #region 108 - dQэ(P2)
                    case @"108":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 1.929F * Out[@"107"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 109 - t 1
                    case @"109":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"28"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 110 - t 2
                    case @"110":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"29"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 111 - t к
                    case @"111":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = fTable.F1("2.75:1", Out[@"105"][ID_COMP[i]].value);
                        }
                        break;
                    #endregion

                    #region 112 - dt
                    case @"112":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"28"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 113 - dt ''(н)
                    case @"113":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = fTable.F3("2.64;3", Norm[@"14"][ID_COMP[i]].value, Out[@"109"][ID_COMP[i]].value, Norm[@"14.1"][ID_COMP[ST]].value);
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"104"][ID_COMP[i]].value;
                            sum1 += Out[@"104"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 114 - dt ''
                    case @"114":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"112"][ID_COMP[i]].value - Out[@"113"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 115 - t к ''
                    case @"115":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"111"][ID_COMP[i]].value - Out[@"114"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 116 - P2
                    case @"116":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = fTable.F1("2.76;1", Out[@"115"][ID_COMP[i]].value);
                        }
                        break;
                    #endregion

                    #region 117 - dN(P2 '')
                    case @"117":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (Norm[@"14"][ID_COMP[i]].value <= 100)
                            {
                                Out[nAlg][ID_COMP[i]].value = fTable.F1("2.45;1", Norm[@"14"][ID_COMP[i]].value) * (Out[@"105"][ID_COMP[i]].value - Out[@"116"][ID_COMP[i]].value) / .01F;
                            }
                            else
                                if (Norm[@"14"][ID_COMP[i]].value > 100)
                                {
                                    Out[nAlg][ID_COMP[i]].value = 1.06F * (Out[@"105"][ID_COMP[i]].value - Out[@"116"][ID_COMP[i]].value) / .01F;
                                }
                                else ;
                        }
                        break;
                    #endregion

                    #region 118 - dQ (P2 '')
                    case @"118":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 1.929F * Out[@"117"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 119 - t пв
                    case @"119":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"115"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"44"][ID_COMP[i]].value;
                            sum1 += Out[@"44"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 120 - t пв(н)
                    case @"120":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"74"][ID_COMP[i]].value;

                        }
                        fRes = Norm[@"74"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 121 - alfa qт(tпв)
                    case @"121":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"120"][ID_COMP[i]].value - Out[@"119"][ID_COMP[i]].value;

                            if (In[@"74"][ID_COMP[i]].value.ToString() == "1")
                                Out[nAlg][ID_COMP[i]].value = fTable.F1("2.77:1", Out[@"121"][ID_COMP[i]].value);
                            else
                                if (In[@"74"][ID_COMP[i]].value.ToString() == "2" || In[@"74"][ID_COMP[i]].value.ToString() == "2a"
                                    || In[@"74"][ID_COMP[i]].value.ToString() == "3")
                                {
                                    Out[nAlg][ID_COMP[i]].value = fTable.F2("2.87:2", Norm[@"50"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value)
                                        * Out[@"121"][ID_COMP[i]].value / 2;
                                }
                                else ;
                        }
                        break;
                    #endregion

                    #region 122 - 
                    case @"122":

                        break;
                    #endregion

                    #region 123 - Э тд
                    case @"123":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (In[@"59"][ID_COMP[ST]].value == 100)
                            {
                                Out[nAlg][ID_COMP[i]].value = In[@"8.2"][ID_COMP[ST]].value + In["8.4"][ID_COMP[i]].value;
                            }
                            else
                                Out[nAlg][ID_COMP[i]].value = In[@"8.2"][ID_COMP[ST]].value + In["8.4"][ID_COMP[i]].value
                                    / In[@"1"][ID_COMP[ST]].value * (In["1"][ID_COMP[i]].value - In[@"70.1"][ID_COMP[ST]].value)
                                    * .35F + In["8.4"][ID_COMP[i]].value / In["1"][ID_COMP[i]].value * In["70.1"][ID_COMP[i]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 124 - Э тд(н)
                    case @"124":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"95"][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value / 1E3F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 125 - Э пп
                    case @"125":

                        if ((m_indxCompRealTime == INDX_COMP.iBL1))
                        {
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (!(In[@"59"][ID_COMP[i]].value > 99.99F) && !(In[@"59"][ID_COMP[i]].value < 90))
                                    Out[nAlg][ID_COMP[i]].value = Out[@"127"][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value / 1E3F;
                                else
                                    if (In[@"59"][ID_COMP[i]].value == 100)
                                        Out[nAlg][ID_COMP[i]].value = 0;
                                    else
                                        Out[nAlg][ID_COMP[i]].value = In[@"8.3"][ID_COMP[i]].value + In[@"8.4"][ID_COMP[i]].value * 0.65F;

                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        }
                        else
                            if ((m_indxCompRealTime == INDX_COMP.iBL2))
                            {
                                if (In[@"59"][ID_COMP[i]].value == 100)
                                    Out[nAlg][ID_COMP[i]].value = 0;
                                else
                                    Out[nAlg][ID_COMP[i]].value = In[@"8.3"][ID_COMP[i]].value + In[@"8.4"][ID_COMP[i]].value
                                        / In[@"1"][ID_COMP[i]].value * (In[@"1"][ID_COMP[i]].value - In[@"70.1"][ID_COMP[i]].value) * .65F;

                                fRes += Out[nAlg][ID_COMP[i]].value;
                            }
                        break;
                    #endregion

                    #region 126 - В уг
                    case @"126":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"78"][ID_COMP[i]].value * 7000 / In[@"53"][ID_COMP[ST]].value;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 127 - Э пп(н)
                    case @"127":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"98"][ID_COMP[i]].value * Out[@"126"][ID_COMP[i]].value / 1E3F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 128 - 
                    case @"128":

                        break;
                    #endregion

                    #region 129 - Qт ср
                    case @"129":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"9"][ID_COMP[i]].value;
                        }
                        fRes = Out[nAlg][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 130 - Qт ср
                    case @"130":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"10"][ID_COMP[i]].value;
                        }
                        fRes = Out[nAlg][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 131 - Q пр ср
                    case @"131":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"4"][ID_COMP[i]].value / Norm[@"1"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"4"][ID_COMP[ST]].value / Norm[@"1"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 132 - Q сум ср
                    case @"132":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"130"][ID_COMP[i]].value + Out[@"131"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"130"][ID_COMP[ST]].value + Out[@"131"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 133 - alfa отр
                    case @"133":
                        //???n_blokov+3
                        fRes = Out[@"13"][ID_COMP[ST]].value / Out[@"11"][ID_COMP[n_blokov + 3]].value * 100;
                        break;
                    #endregion

                    #region 134 - alfa г.в.
                    case @"134":
                        //???n_blokov+3
                        fRes = Out[@"12"][ID_COMP[n_blokov + 3]].value / Out[@"11"][ID_COMP[n_blokov + 3]].value * 100;
                        break;
                    #endregion

                    #region 135 - Эсн
                    case @"135":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"30"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"30"][ID_COMP[ST]].value / Out[@"4"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 136 - Эсн(н)
                    case @"136":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (Out[@"36"][ID_COMP[i]].value + Out[@"37"][ID_COMP[i]].value
                                + Out[@"38"][ID_COMP[i]].value) / Out[@"4"][ID_COMP[i]].value * 100;
                        }
                        fRes = (Out[@"36"][ID_COMP[ST]].value + Out[@"37"][ID_COMP[ST]].value
                                + Out[@"38"][ID_COMP[ST]].value) / Out[@"4"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 137 -  ???Ээ сн
                    case @"137":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"34"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"34"][ID_COMP[ST]].value + Out[@"4"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 138 - Ээ сн(н)
                    case @"138":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"39"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"34"][ID_COMP[ST]].value / Out[@"39"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 139 - Этэ сн
                    case @"139":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"35"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"35"][ID_COMP[ST]].value / Out[@"11"][ID_COMP[ST]].value * 1E3F;
                        break;
                    #endregion

                    #region 140 - Этэ сн(н)
                    case @"140":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"40"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"40"][ID_COMP[ST]].value / Out[@"11"][ID_COMP[ST]].value * 1E3F;
                        break;
                    #endregion

                    #region 141 - Ки э
                    case @"141":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"4"][ID_COMP[i]].value / 200 / In[@"70"][ID_COMP[i]].value * 100;
                        }

                        fRes = Out[@"4"][ID_COMP[ST]].value / 1200 / In[@"70"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 142 - Ки тэ
                    case @"142":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"9"][ID_COMP[i]].value / 240 / In[@"70"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"40"][ID_COMP[ST]].value / 1440 / Out[@"11"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 143 - V (н)
                    case @"143":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (1 - Out[@"106"][ID_COMP[i]].value) * 100;
                        }
                        fRes = (1 - Out[@"40"][ID_COMP[ST]].value) * 100;
                        break;
                    #endregion

                    #region 144 - V
                    case @"144":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (1 - Out[@"105"][ID_COMP[i]].value) * 100;
                        }
                        fRes = (1 - Out[@"40"][ID_COMP[ST]].value) * 100;
                        break;
                    #endregion

                    #region 145 - qт сн(н)
                    case @"145":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"16"][ID_COMP[i]].value / Out[@"15"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"16"][ID_COMP[i]].value / Out[@"15"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 146 - qт сн
                    case @"146":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"16.1"][ID_COMP[i]].value / Out[@"15"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"16.1"][ID_COMP[i]].value / Out[@"15"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 147 - Эт сн(н)
                    case @"147":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"36"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"36"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 148 - Эт сн
                    case @"148":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"28"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"28"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 149 - Эцн(н)
                    case @"149":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"42"][ID_COMP[i]].value / Out[@"104"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"42"][ID_COMP[i]].value / Out[@"104"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 150 - Эцн
                    case @"150":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"41"][ID_COMP[i]].value / Out[@"104"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"41"][ID_COMP[i]].value / Out[@"104"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 151 - qт н(н)
                    case @"151":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"56"][ID_COMP[i]].value * (100 + Out[@"145"][ID_COMP[i]].value)
                            / (100 - Out[@"147"][ID_COMP[i]].value);
                        }
                        fRes = Out[@"56"][ID_COMP[ST]].value * (100 + Out[@"145"][ID_COMP[ST]].value)
                            / (100 - Out[@"147"][ID_COMP[ST]].value);
                        break;
                    #endregion

                    #region 152 - qт н
                    case @"152":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"55"][ID_COMP[i]].value * (100 + Out[@"146"][ID_COMP[i]].value)
                            / (100 - Out[@"148"][ID_COMP[i]].value);
                        }
                        fRes = Out[@"55"][ID_COMP[ST]].value * (100 + Out[@"146"][ID_COMP[ST]].value)
                            / (100 - Out[@"148"][ID_COMP[ST]].value);
                        break;
                    #endregion

                    #region 153 - Этепл(н)
                    case @"153":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"38"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"38"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 154 - Этепл
                    case @"154":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"27"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"27"][ID_COMP[i]].value / Out[@"11"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 155 - Эк сн(н)
                    case @"155":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"37"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"37"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 156 - Эк сн
                    case @"156":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"29"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        }
                        fRes = Out[@"29"][ID_COMP[i]].value / Out[@"4"][ID_COMP[i]].value * 1E2F;
                        break;
                    #endregion

                    #region 157 - Эпп(н)
                    case @"157":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"127"][ID_COMP[i]].value / Out[@"126"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"127"][ID_COMP[i]].value / Out[@"126"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 158 - Эпп
                    case @"158":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"125"][ID_COMP[i]].value / Out[@"126"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"125"][ID_COMP[i]].value / Out[@"126"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 159 - Эпэн(н)
                    case @"159":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"45"][ID_COMP[i]].value / Out[@"44"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"45"][ID_COMP[i]].value / Out[@"44"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 160 - Эпэн
                    case @"160":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"43"][ID_COMP[i]].value / Out[@"44"][ID_COMP[i]].value;
                        }
                        fRes = Out[@"43"][ID_COMP[i]].value / Out[@"44"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 161 - Этд(н)
                    case @"161":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"124"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"124"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 162 - Этд
                    case @"162":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"123"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 1E3F;
                        }
                        fRes = Out[@"123"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 1E3F;
                        break;
                    #endregion

                    #region 163 - Dпе
                    case @"163":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"50"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 163.1 - D пв ср
                    case @"163.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"66.1"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 164 - Qк бр
                    case @"164":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"65"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"65"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 165 - Pк
                    case @"165":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"56"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 166 - t к
                    case @"166":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"51"][ID_COMP[i]].value;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 167 - t гв
                    case @"167":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = (In[@"33"][ID_COMP[i]].value + In[@"34"][ID_COMP[i]].value) / 2;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 168 - Гун(н)
                    case @"168":
                        Out[nAlg][ID_COMP[0]].value = fTable.F2("2.41:2", Out[@"164"][ID_COMP[i]].value, In[@"59"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[1]].value = fTable.F2("2.41:2", Out[@"164"][ID_COMP[i]].value, In[@"59"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[2]].value = 3.4F;
                        Out[nAlg][ID_COMP[3]].value = 3.4F;
                        Out[nAlg][ID_COMP[4]].value = 4.1F;
                        Out[nAlg][ID_COMP[5]].value = fTable.F1("2.41:1", Out[@"164"][ID_COMP[i]].value);

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"78"][ID_COMP[i]].value;
                            sum1 += Out[@"78"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 169 - Гун
                    case @"169":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"57"][ID_COMP[i]].value * Norm[@"89"][ID_COMP[i]].value / 1E2F;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"78"][ID_COMP[i]].value;
                            sum1 += Out[@"78"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 170 - Гшл(н)
                    case @"170":
                        Out[nAlg][ID_COMP[0]].value = fTable.F2("2.43:2", Out[@"164"][ID_COMP[i]].value, In[@"59"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[1]].value = fTable.F2("2.43:2", Out[@"164"][ID_COMP[i]].value, In[@"59"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[2]].value = fTable.F1("2.43:1", Out[@"164"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[3]].value = fTable.F1("2.43:1", Out[@"164"][ID_COMP[i]].value);
                        Out[nAlg][ID_COMP[4]].value = 4;
                        Out[nAlg][ID_COMP[i]].value = fTable.F1("2.43:1(б)", Out[@"164"][ID_COMP[i]].value);
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"78"][ID_COMP[i]].value;
                            sum1 += Out[@"78"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 171 - Гшл
                    case @"171":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[0]].value = In[@"56"][ID_COMP[i]].value - Norm[@"89"][ID_COMP[i]].value / 1E2F;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"78"][ID_COMP[i]].value;
                            sum1 += Out[@"78"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 172 - q прочие
                    case @"172":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = 100 - Out[@"68"][ID_COMP[i]].value - Out[@"57"][ID_COMP[i]].value
                                - Out[@"73"][ID_COMP[i]].value;
                        }
                        fRes = 100 - Out[@"68"][ID_COMP[ST]].value - Out[@"57"][ID_COMP[ST]].value
                                - Out[@"73"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 173 - dalfa (н)
                    case @"173":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"62"][ID_COMP[i]].value + 10 * (472.2F / Norm[@"65"][ID_COMP[i]].value);
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 174 - dalfa
                    case @"174":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"61"][ID_COMP[i]].value + Out[@"63"][ID_COMP[i]].value / 100;
                            sum += Out[nAlg][ID_COMP[i]].value * Out[@"17"][ID_COMP[i]].value;
                            sum1 += Out[@"17"][ID_COMP[i]].value;
                        }
                        fRes = sum / sum1;
                        break;
                    #endregion

                    #region 175 - q к сн(н)
                    case @"175":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"18"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"18"][ID_COMP[ST]].value / Out[@"17"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 175.1 - q к сн
                    case @"175.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"19"][ID_COMP[i]].value / Out[@"17"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"19"][ID_COMP[ST]].value / Out[@"17"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 176 - Кпдк"нетто"   
                    case @"176":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"73"][ID_COMP[i]].value * (100 - Out[@"175.1"][ID_COMP[i]].value) / 100
                                * (100 - Out[@"137"][ID_COMP[i]].value) / (100 - Out[@"148"][ID_COMP[i]].value);
                        }
                        fRes = Out[@"73"][ID_COMP[ST]].value * (100 - Out[@"175.1"][ID_COMP[ST]].value) / 100
                                * (100 - Out[@"137"][ID_COMP[ST]].value) / (100 - Out[@"148"][ID_COMP[ST]].value);
                        break;
                    #endregion

                    #region 177 - Кпдк"нетто"(н)
                    case @"177":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"74"][ID_COMP[i]].value * (100 - Out[@"175"][ID_COMP[i]].value) / 100
                                * (100 - Out[@"138"][ID_COMP[i]].value) / (100 - Out[@"147"][ID_COMP[i]].value);
                        }
                        fRes = Out[@"74"][ID_COMP[ST]].value * (100 - Out[@"175"][ID_COMP[ST]].value) / 100
                                * (100 - Out[@"138"][ID_COMP[ST]].value) / (100 - Out[@"147"][ID_COMP[ST]].value);
                        break;
                    #endregion

                    #region 178 - Gхов
                    case @"178":
                        fRes = (In[@"78"][ID_COMP[ST]].value + In[@"79"][ID_COMP[ST]].value) / 1E3F;
                        break;
                    #endregion

                    #region 179 - Gпот(н)
                    case @"179":
                        fRes = In[@"80"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 179.1 - Gпот(н)
                    case @"179.1":
                        fRes = (Out[@"179"][ID_COMP[ST]].value * Out[@"44"][ID_COMP[ST]].value) / 100;
                        break;
                    #endregion

                    #region 180 - Gпот
                    case @"180":
                        fRes = (Out[@"178"][ID_COMP[ST]].value * Out[@"44"][ID_COMP[ST]].value) * 1E2F;
                        break;
                    #endregion

                    #region 181 - Gпрод
                    case @"181":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = In[@"27"][ID_COMP[i]].value / 1E3F;
                            fRes += Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 182 - Gпрод
                    case @"182":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"181"][ID_COMP[i]].value / Out[@"44"][ID_COMP[i]].value * 100;
                        }
                        fRes = Out[@"181"][ID_COMP[ST]].value / Out[@"44"][ID_COMP[ST]].value * 100;
                        break;
                    #endregion

                    #region 183 - Ктп
                    case @"183":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Norm[@"130"][ID_COMP[i]].value;
                        }
                        fRes = Norm[@"130"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 184 - n вн
                    case @"184":
                               for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Out[nAlg][ID_COMP[i]].value = Out[@"24"][ID_COMP[i]].value - Out[@"25"][ID_COMP[i]].value;
                            fRes = Out[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateMaket (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
                return fRes;
            }
        }
    }
}
