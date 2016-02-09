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
            private bool qSeason
            {
                get
                {
                    return ((!(DateTime.Now.Month < 5)) && (!(DateTime.Now.Month > 9)))
                        || ((DateTime.Now.Month == 5) && (DateTime.Now.Day > 15));
                }
            }
            
            /// <summary>
            /// Рассчитать значения для параметра в алгоритме расчета по идентификатору
            /// </summary>
            /// <param name="nAlg">Строковый идентификатор параметра</param>
            /// <returns>Значение для группы компонентов</returns>
            private float calculateNormative(string nAlg)
            {
                float fRes = 0F; // значение группы компонентов - результат выполнения

                int i = -1; // переменная цикла
                float fSum = 0F // промежуточная величина
                    , fTmp = -1F;

                switch (nAlg)
                {
                    #region 1, 2 - TAU раб, Э т
                    case @"1": //TAU раб
                    case @"2": //Э т
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][i].value = In[nAlg][i].value;
                            fRes += Norm[nAlg][i].value;
                        }
                        break;
                    #endregion

                    #region 3 - Q то
                    case @"3": //Q то
                        if (Type == TYPE.OUT_TEP_REALTIME)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (In[@"47"][i].value / In[@"1"][i].value < 0.7F)
                                    Norm[nAlg][i].value = 0F;
                                else
                                    Norm[nAlg][i].value = In[@"47"][i].value * In[@"48"][i].value - In[@"49"][i].value;

                                fRes += Norm[nAlg][i].value;
                            }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Norm[nAlg][i].value = In[@"47"][i].value * In[@"48"][i].value - In[@"49"][i].value;
                                fRes += Norm[nAlg][i].value;
                            }
                        break;
                    #endregion

                    #region 4 - Q пп
                    case @"4": //Q пп
                        if (Type == TYPE.OUT_TEP_REALTIME)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (Norm[@"3"][i].value == 0F)
                                    Norm[nAlg][i].value = 0F;
                                else
                                    Norm[nAlg][i].value = In[@"46"][i].value;

                                fRes += Norm[nAlg][i].value;
                            }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Norm[nAlg][i].value = In[@"46"][i].value;
                                fRes += Norm[nAlg][i].value;
                            }
                        break;
                    #endregion

                    #region 5 - Q отп ст
                    case @"5": //Q отп ст
                        fRes = In[@"81"][ST].value;
                        break;
                    #endregion

                    #region 6 - Q отп роу
                    case @"6": //Q отп роу
                        fRes = In[@"82"][ST].value;

                        if (Type == TYPE.OUT_TEP_REALTIME)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][i].value = Norm[@"4"][i].value / 2;
                        else
                        {
                            fSum = 0F;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                fSum += In[@"3"][i].value == 0 ? 0 : Norm[@"4"][i].value;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][i].value = Norm[@"3"][i].value == 0F ? 0F :
                                    Norm[@"4"][i].value == 0F ? 0F :
                                        fRes * Norm[@"4"][i].value / fSum;
                        }
                        break;
                    #endregion

                    #region 7 - Q отп тепл
                    case @"7":
                        fRes = Norm[@"5"][ST].value - Norm[@"6"][ST].value - In[@"85"][ST].value;

                        if (Type == TYPE.OUT_TEP_REALTIME)
                        {
                            fTmp = qSeason ? 0.97F : 0.95F;
                            //16 мая - 30 сентября, 1 октября - 15 мая
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][i].value = Norm[@"3"][i].value * fTmp;
                        }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][i].value = Norm[@"3"][ST].value == 0F ? 0F :
                                    fRes * Norm[@"3"][i].value / Norm[@"3"][ST].value;
                        break;
                    #endregion

                    #region 8 - Q отп
                    case @"8":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][i].value = Norm[@"6"][i].value + Norm[@"7"][i].value;
                            fRes += Norm[nAlg][i].value;
                        }
                        break;
                    #endregion

                    #region 9 - N т
                    case @"9":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][i].value = Norm[@"2"][i].value / Norm[@"1"][i].value;

                        fRes = Norm[@"2"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 10 - Q т ср
                    case @"10":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][i].value = Norm[@"3"][i].value / Norm[@"1"][i].value;

                        fRes = Norm[@"3"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 10.1 - P вто
                    case @"10.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][i].value = In[@"37"][i].value;
                        break;
                    #endregion

                    #region 11 - Q роу ср
                    case @"11":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][i].value = Norm[@"4"][i].value / Norm[@"1"][i].value;

                        fRes = Norm[@"4"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 12 - q т бр (исх)
                    case @"12":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch ((short)In[@"74"][i].value)
                            {
                                case 1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.40:1", Norm[@"9"][i].value);
                                    break;
                                case 2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    fTmp = fTable.F3(@"2.1:3", Norm[@"9"][i].value, Norm[@"10"][i].value, Norm[@"10.1"][i].value);
                                    break;
                                case 3: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.86:3", Norm[@"9"][i].value, Norm[@"10"][i].value, In[@"38"][i].value);
                                    break;
                                case 4: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F2(@"2.50:2", Norm[@"9"][i].value, Norm[@"10.1"][i].value);
                                    break;
                                default:
                                    Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный режим работы оборудования ...", Logging.INDEX_MESSAGE.NOT_SET);
                                    break;
                            }

                            Norm[nAlg][i].value = fTmp;
                        }
                        break;
                    #endregion

                    #region 13 - G о
                    case @"13":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch ((short)In[@"74"][i].value)
                            {
                                case 1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.55:1", Norm[@"9"][i].value);
                                    break;
                                case 2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    fTmp = fTable.F3(@"2.2:3", Norm[@"9"][i].value, Norm[@"10"][i].value, Norm[@"10.1"][i].value);
                                    break;
                                case 3: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.87:3", Norm[@"9"][i].value, Norm[@"10"][i].value, In[@"38"][i].value);
                                    break;
                                case 4: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F3(@"2.2:3", Norm[@"9"][i].value, Norm[@"10.1"][i].value);
                                    break;
                                default:
                                    Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный режим работы оборудования ...", Logging.INDEX_MESSAGE.NOT_SET);
                                    break;
                            }

                            Norm[nAlg][i].value = fTmp + In[@"46"][i].value / 0.7F / In[@"1"][i].value;
                        }
                        break;
                    #endregion

                    #region 14 - G 2
                    case @"14":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch ((short)In[@"74"][i].value)
                            {
                                case 1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.3:1", Norm[@"13"][i].value);
                                    break;
                                case 2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case 4: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F3(@"2.3а:3", Norm[@"13"][i].value, Norm[@"10"][i].value, Norm[@"10.1"][i].value);
                                    break;
                                case 3: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.3а:3", Norm[@"13"][i].value, Norm[@"10"][i].value, In[@"38"][i].value);
                                    break;
                                default:
                                    Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный режим работы оборудования ...", Logging.INDEX_MESSAGE.NOT_SET);
                                    break;
                            }

                            Norm[nAlg][i].value = fTmp - In[@"46"][i].value / 0.7F / In[@"1"][i].value;
                        }

                        fRes = -1F;
                        break;
                    #endregion

                    #region 15 -
                    #endregion

                    #region 16 -
                    #endregion

                    #region 17 -
                    #endregion

                    #region 18 -
                    #endregion

                    #region 19 -
                    #endregion

                    #region 20 -
                    #endregion

                    #region 21 -
                    #endregion

                    #region 22 -
                    #endregion

                    #region 23 -
                    #endregion

                    #region 24 -
                    #endregion

                    #region 25 -
                    #endregion

                    #region 26 -
                    #endregion

                    #region 27 -
                    #endregion

                    #region 28 -
                    #endregion

                    #region 29 -
                    #endregion

                    #region 30 -
                    #endregion

                    #region 31 -
                    #endregion

                    #region 32 -
                    #endregion

                    #region 33 -
                    #endregion

                    #region 34 -
                    #endregion

                    #region 35 -
                    #endregion

                    #region 36 -
                    #endregion

                    #region 37 -
                    #endregion

                    #region 38 -
                    #endregion

                    #region 39 -
                    #endregion

                    #region 40 -
                    #endregion

                    #region 41 -
                    #endregion

                    #region 42 -
                    #endregion

                    #region 43 -
                    #endregion

                    #region 44 -
                    #endregion

                    #region 45 -
                    #endregion

                    #region 46 -
                    #endregion

                    #region 47 -
                    #endregion

                    #region 48 -
                    #endregion

                    #region 49 -
                    #endregion

                    #region 50 -
                    #endregion

                    #region 51 -
                    #endregion

                    #region 52 -
                    #endregion

                    #region 53 -
                    #endregion

                    #region 54 -
                    #endregion

                    #region 55 -
                    #endregion

                    #region 56 -
                    #endregion

                    #region 57 -
                    #endregion

                    #region 58 -
                    #endregion

                    #region 59 -
                    #endregion

                    #region 60 -
                    #endregion

                    #region 61 -
                    #endregion

                    #region 62 -
                    #endregion

                    #region 63 -
                    #endregion

                    #region 64 -
                    #endregion

                    #region 65 -
                    #endregion

                    #region 66 -
                    #endregion

                    #region 67 -
                    #endregion

                    #region 68 -
                    #endregion

                    #region 69 -
                    #endregion

                    #region 70 -
                    #endregion

                    #region 71 -
                    #endregion

                    #region 72 -
                    #endregion

                    #region 73 -
                    #endregion

                    #region 74 -
                    #endregion

                    #region 75 -
                    #endregion

                    #region 76 -
                    #endregion

                    #region 77 -
                    #endregion

                    #region 78 -
                    #endregion

                    #region 79 -
                    #endregion

                    #region 80 -
                    #endregion

                    #region 81 -
                    #endregion

                    #region 82 -
                    #endregion

                    #region 83 -
                    #endregion

                    #region 84 -
                    #endregion

                    #region 85 -
                    #endregion

                    #region 86 -
                    #endregion

                    #region 87 -
                    #endregion

                    #region 88 -
                    #endregion

                    #region 89 -
                    #endregion

                    #region 90 -
                    #endregion

                    #region 91 -
                    #endregion

                    #region 92 -
                    #endregion

                    #region 93 -
                    #endregion

                    #region 94 -
                    #endregion

                    #region 95 -
                    #endregion

                    #region 96 -
                    #endregion

                    #region 97 -
                    #endregion

                    #region 98 -
                    #endregion

                    #region 99 -
                    #endregion

                    #region 100 -
                    #endregion

                    #region 101 -
                    #endregion

                    #region 102 -
                    #endregion

                    #region 103 -
                    #endregion

                    #region 104 -
                    #endregion

                    #region 105 -
                    #endregion

                    #region 106 -
                    #endregion

                    #region 107 -
                    #endregion

                    #region 108 -
                    #endregion

                    #region 109 -
                    #endregion

                    #region 110 -
                    #endregion

                    #region 111 -
                    #endregion

                    #region 112 -
                    #endregion

                    #region 113 -
                    #endregion

                    #region 114 -
                    #endregion

                    #region 115 -
                    #endregion

                    #region 116 -
                    #endregion

                    #region 117 -
                    #endregion

                    #region 118 -
                    #endregion

                    #region 119 -
                    #endregion

                    #region 120 -
                    #endregion

                    #region 121 -
                    #endregion

                    #region 122 -
                    #endregion

                    #region 123 -
                    #endregion

                    #region 124 -
                    #endregion

                    #region 125 -
                    #endregion

                    #region 126 -
                    #endregion

                    #region 127 -
                    #endregion

                    #region 128 -
                    #endregion

                    #region 129 -
                    #endregion

                    #region 130 -
                    #endregion

                    #region 131 -
                    #endregion

                    #region 132 -
                    #endregion

                    #region 133 -
                    #endregion

                    #region 134 -
                    #endregion

                    #region 135 -
                    #endregion

                    #region 136 -
                    #endregion

                    #region 137 -
                    #endregion

                    #region 138 -
                    #endregion

                    #region 139 -
                    #endregion

                    #region 140 -
                    #endregion

                    #region 141 -
                    #endregion

                    #region 142 -
                    #endregion

                    #region 143 -
                    #endregion

                    #region 144 -
                    #endregion

                    #region 145 -
                    #endregion

                    #region 146 -
                    #endregion

                    #region 147 -
                    #endregion

                    #region 148 -
                    #endregion

                    #region 149 -
                    #endregion

                    #region 150 -
                    #endregion

                    #region 151 -
                    #endregion

                    #region 152 -
                    #endregion

                    #region 153 -
                    #endregion

                    #region 154 -
                    #endregion

                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }

                return fRes;
            }
        }
    }
}
