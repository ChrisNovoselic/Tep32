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
            /// Зафиксировать в журнале сообщение об ошибке - неизвестный тип режима работы оборудования
            /// </summary>
            private void logErrorUnknownModeDev(string nAlg, int indxComp)
            {
                Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @", ID_COMP=" + ID_COMP[indxComp] + @") - неизвестный режим работы оборудования ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Рассчитать значения для параметра в алгоритме расчета по идентификатору
            /// </summary>
            /// <param name="nAlg">Строковый идентификатор параметра</param>
            /// <returns>Значение для группы компонентов</returns>
            private float calculateNormative(string nAlg)
            {
                float fRes = 0F; // значение группы компонентов - результат выполнения

                int i = -1 // переменная цикла
                    , id_comp = -1; // идентификатор компонента
                float fSum = 0F // промежуточная величина
                    , fTmp = -1F
                    , fSum1 = 0F
                ,fSum2 = 0F;
                float[] fRunkValues = new float[(int)FTable.FRUNK.COUNT];
                // только для вычисления пар.20 - 4-х мерная функция
                string nameFTable = string.Empty
                    , postfixFTable = string.Empty;
                float[,] fRunk4 = null;

                switch (nAlg)
                {
                    #region 1, 2 - TAU раб, Э т
                    case @"1": //TAU раб
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value;
                        }
                        //??? для станции и для компонентов д.б. одинаковое значение
                        fRes = Norm[nAlg][ID_COMP[0]].value;
                        break;
                    case @"2": //Э т
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][ID_COMP[i]].value = In[nAlg][ID_COMP[i]].value;
                            fRes += Norm[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 3 - Q то
                    case @"3": //Q то
                        if (isRealTime == true)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (In[@"47"][ID_COMP[i]].value / In[@"1"][ID_COMP[i]].value < 0.7F)
                                    Norm[nAlg][ID_COMP[i]].value = 0F;
                                else
                                    Norm[nAlg][ID_COMP[i]].value = In[@"47"][ID_COMP[i]].value * In[@"48"][ID_COMP[i]].value - In[@"49"][ID_COMP[i]].value;

                                fRes += Norm[nAlg][ID_COMP[i]].value;
                            }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Norm[nAlg][ID_COMP[i]].value = In[@"47"][ID_COMP[i]].value * In[@"48"][ID_COMP[i]].value - In[@"49"][ID_COMP[i]].value;
                                fRes += Norm[nAlg][ID_COMP[i]].value;
                            }
                        break;
                    #endregion

                    #region 4 - Q пп
                    case @"4": //Q пп
                        if (isRealTime == true)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (Norm[@"3"][ID_COMP[i]].value == 0F)
                                    Norm[nAlg][ID_COMP[i]].value = 0F;
                                else
                                    Norm[nAlg][ID_COMP[i]].value = In[@"46"][ID_COMP[i]].value;

                                fRes += Norm[nAlg][ID_COMP[i]].value;
                            }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                Norm[nAlg][ID_COMP[i]].value = In[@"46"][ID_COMP[i]].value;
                                fRes += Norm[nAlg][ID_COMP[i]].value;
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

                        if (isRealTime == true)
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][ID_COMP[i]].value = Norm[@"4"][ID_COMP[i]].value / 2;
                        else
                        {
                            fSum = 0F;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                fSum += In[@"3"][ID_COMP[i]].value == 0 ? 0 : Norm[@"4"][ID_COMP[i]].value;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][ID_COMP[i]].value = Norm[@"3"][ID_COMP[i]].value == 0F ? 0F :
                                    Norm[@"4"][ID_COMP[i]].value == 0F ? 0F :
                                        fRes * Norm[@"4"][ID_COMP[i]].value / fSum;
                        }
                        break;
                    #endregion

                    #region 7 - Q отп тепл
                    case @"7":
                        fRes = Norm[@"5"][ST].value - Norm[@"6"][ST].value - In[@"85"][ST].value;

                        if (isRealTime == true)
                        {
                            fTmp = qSeason ? 0.97F : 0.95F;
                            //16 мая - 30 сентября, 1 октября - 15 мая
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][ID_COMP[i]].value = Norm[@"3"][ID_COMP[i]].value * fTmp;
                        }
                        else
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                Norm[nAlg][ID_COMP[i]].value = Norm[@"3"][ST].value == 0F ? 0F :
                                    fRes * Norm[@"3"][ID_COMP[i]].value / Norm[@"3"][ST].value;
                        break;
                    #endregion

                    #region 8 - Q отп
                    case @"8":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][ID_COMP[i]].value = Norm[@"6"][ID_COMP[i]].value + Norm[@"7"][ID_COMP[i]].value;
                            fRes += Norm[nAlg][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 9 - N т
                    case @"9":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            if (!(Norm[@"1"][ID_COMP[i]].value == 0F))
                                Norm[nAlg][ID_COMP[i]].value = Norm[@"2"][ID_COMP[i]].value / Norm[@"1"][ID_COMP[i]].value;
                            else
                                ;

                        fRes = Norm[@"2"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 10 - Q т ср
                    case @"10":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][ID_COMP[i]].value = Norm[@"3"][ID_COMP[i]].value / Norm[@"1"][ID_COMP[i]].value;

                        fRes = Norm[@"3"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 10.1 - P вто
                    case @"10.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][ID_COMP[i]].value = In[@"37"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 11 - Q роу ср
                    case @"11":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][ID_COMP[i]].value = Norm[@"4"][ID_COMP[i]].value / Norm[@"1"][ID_COMP[i]].value;

                        fRes = Norm[@"4"][ST].value / Norm[@"1"][ST].value;
                        break;
                    #endregion

                    #region 12 - q т бр (исх)
                    case @"12":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.40:1", Norm[@"9"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    fTmp = fTable.F3(@"2.1:3", Norm[@"9"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.86:3", Norm[@"9"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, In[@"38"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F2(@"2.50:2", Norm[@"9"][ID_COMP[i]].value, Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }

                            Norm[nAlg][ID_COMP[i]].value = fTmp;
                        }
                        break;
                    #endregion

                    #region 13 - G о
                    case @"13":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.55:1", Norm[@"9"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    fTmp = fTable.F3(@"2.2:3", Norm[@"9"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.87:3", Norm[@"9"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, In[@"38"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F3(@"2.2:3", Norm[@"9"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }

                            Norm[nAlg][ID_COMP[i]].value = fTmp + In[@"46"][ID_COMP[i]].value / 0.7F / In[@"1"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 14 - G 2
                    case @"14":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.3:1", Norm[@"13"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F3(@"2.3б:3", Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F3(@"2.3а:3", Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, In[@"38"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }

                            Norm[nAlg][ID_COMP[i]].value = fTmp - In[@"46"][ID_COMP[i]].value / 0.7F / In[@"1"][ID_COMP[i]].value;
                            fSum += Norm[nAlg][ID_COMP[i]].value;
                        }

                        if (isRealTimeBL1456 == true)
                            //??? почему умножаем на кол-во блоков
                            //??? как определяется кол-во блоков
                            Norm[nAlg][ST].value = fSum * n_blokov1;
                        else
                            Norm[nAlg][ST].value = fSum;
                        break;
                    #endregion

                    #region 14.1 - G цв
                    case @"14.1":
                        if (In[@"70"][ST].value == 0F)
                            fRes = 0F;
                        else
                        {
                            fTmp = (isRealTimeBL1456 == true) ? n_blokov1 : In[@"89"][ST].value;
                            fRunkValues[(int)FTable.FRUNK.F1] = fTmp;
                            fRunkValues[(int)FTable.FRUNK.F2] = (float)Math.Round((decimal)(In[@"6"][ST].value / In[@"70"][ST].value / 1.9F), 1);

                            fRes = fTable.F2(@"2.4а:2", fRunkValues[(int)FTable.FRUNK.F1], fRunkValues[(int)FTable.FRUNK.F2]);
                        }
                        break;
                    #endregion

                    #region 15 - P 2 (н)
                    case @"15":
                        fRunkValues[(int)FTable.FRUNK.F3] = -1F;

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fRunkValues[(int)FTable.FRUNK.F1] = Norm[@"14"][ID_COMP[i]].value;
                            fRunkValues[(int)FTable.FRUNK.F2] = In[@"28"][ID_COMP[i]].value;

                            Norm[nAlg][ID_COMP[i]].value = fTable.F3(@"2.4:3", fRunkValues);
                        }
                        break;
                    #endregion

                    #region 15.1 - dQ э (P2)
                    case @"15.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fRunkValues[(int)FTable.FRUNK.F1] = Norm[@"15"][ID_COMP[i]].value;
                            fRunkValues[(int)FTable.FRUNK.F2] = Norm[@"14"][ID_COMP[i]].value;

                            Norm[nAlg][ID_COMP[i]].value = fTable.F2(@"2.84:2", fRunkValues);
                        }
                        break;
                    #endregion

                    #region 16 - dQ бр (P2)
                    case @"16":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (!(Norm[@"9"][ID_COMP[i]].value == 0F))
                                Norm[nAlg][ID_COMP[i]].value = 1000 * Norm[@"15.1"][ID_COMP[i]].value / Norm[@"9"][ID_COMP[i]].value;
                            else
                                ;
                        }
                        break;
                    #endregion

                    #region 17 - dqт бр (P2)
                    case @"17":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = fTable.F1(@"2.5а:1", Norm[@"13"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F2(@"2.5:2", Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }

                            Norm[nAlg][ID_COMP[i]].value = fTmp * Norm[@"11"][ID_COMP[i]].value;
                        }
                        break;
                    #endregion

                    #region 18 - t 2(н)
                    case @"18":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = 0F;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    fTmp = -1F;
                                    Logging.Logg().Warning(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - не расчитывается при режиме '1 - Конденсационный'...", Logging.INDEX_MESSAGE.NOT_SET);
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    fTmp = fTable.F1(@"2.6:1", Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fTmp = fTable.F1(@"2.89:1", Norm[@"38"][ID_COMP[i]].value);
                                    break;
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    fTmp = fTable.F1(@"2.6:1", Norm[@"10.1"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }

                            Norm[nAlg][ID_COMP[i]].value = fTmp;
                        }
                        break;
                    #endregion

                    #region 19 - dt 2
                    case @"19":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            Norm[nAlg][ID_COMP[i]].value = In[@"49"][ID_COMP[i]].value - Norm[@"18"][ID_COMP[i]].value;
                        break;
                    #endregion

                    #region 20 - dqт бр (t 2)
                    case @"20":
                        fRunk4 = new float[2, (int)(INDX_COMP.COUNT - 1)];
                        // для левой границы [0, i] 4-х мерной функции
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            nameFTable = @"2.7";
                            fTmp = Norm[@"10.1"][ID_COMP[i]].value;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fRunk4[0, i] = 0F;
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    if (fTmp < 0.8F)
                                        ;
                                    else
                                        if ((!(fTmp < 0.8F)) && (!(fTmp > 0.99F)))
                                            ;
                                        else
                                            if ((!(fTmp < 1.0F)) && (!(fTmp > 1.19F)))
                                                nameFTable += @"а";
                                            else
                                                if ((!(fTmp < 1.2F)) && (!(fTmp > 1.39F)))
                                                    nameFTable += @"б";
                                                else
                                                    if ((!(fTmp < 1.4F)) && (!(fTmp > 1.59F)))
                                                        nameFTable += @"в";
                                                    else
                                                        if ((!(fTmp < 1.6F)) && (!(fTmp > 1.79F)))
                                                            nameFTable += @"г";
                                                        else
                                                            if (!(fTmp < 1.8F))
                                                                nameFTable += @"д";
                                                            else
                                                                ;

                                    if (!(Norm[@"19"][ID_COMP[i]].value < 0))
                                        postfixFTable = @"+";
                                    else
                                        postfixFTable = @"-";

                                    nameFTable += postfixFTable;
                                    nameFTable += @":3";

                                    fRunk4[0, i] = fTable.F3(nameFTable, Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"19"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }
                        }

                        // для правой границы [1, i] 4-х мерной функции
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            nameFTable = @"2.7";
                            fTmp = Norm[@"10.1"][ID_COMP[i]].value;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    fRunk4[1, i] = 0F;
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    if (fTmp < 0.8F)
                                        ;
                                    else
                                        if ((!(fTmp < 0.8F)) && (!(fTmp > 0.99F)))
                                            nameFTable += @"а";
                                        else
                                            if ((!(fTmp < 1.0F)) && (!(fTmp > 1.19F)))
                                                nameFTable += @"б";
                                            else
                                                if ((!(fTmp < 1.2F)) && (!(fTmp > 1.39F)))
                                                    nameFTable += @"в";
                                                else
                                                    if ((!(fTmp < 1.4F)) && (!(fTmp > 1.59F)))
                                                        nameFTable += @"г";
                                                    else
                                                        if ((!(fTmp < 1.6F)) && (!(fTmp > 1.79F)))
                                                            nameFTable += @"д";
                                                        else
                                                            if (!(fTmp < 1.8F))
                                                                nameFTable += @"е";
                                                            else
                                                                ;

                                    if (!(Norm[@"19"][ID_COMP[i]].value < 0))
                                        postfixFTable = @"+";
                                    else
                                        postfixFTable = @"-";

                                    nameFTable += postfixFTable;
                                    nameFTable += @":3";

                                    fRunk4[1, i] = fTable.F3(nameFTable, Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value, Norm[@"19"][ID_COMP[i]].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }
                        }

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = Norm[@"10.1"][ID_COMP[i]].value;

                            switch (_modeDev[i])
                            {
                                case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    break;
                                case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                    if (fTmp < 0.8F)
                                        Norm[nAlg][ID_COMP[i]].value = fRunk4[0, i];
                                    else
                                        if ((!(fTmp < 0.8F)) && (!(fTmp > 0.99F)))
                                            Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (0.99F - fTmp) + fRunk4[1, i] * (fTmp - 0.8F)) / 0.19F;
                                        else
                                            if ((!(fTmp < 1.0F)) && (!(fTmp > 1.19F)))
                                                Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (1.19F - fTmp) + fRunk4[1, i] * (fTmp - 1.0F)) / 0.19F;
                                            else
                                                if ((!(fTmp < 1.2F)) && (!(fTmp > 1.39F)))
                                                    Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (1.39F - fTmp) + fRunk4[1, i] * (fTmp - 1.2F)) / 0.19F;
                                                else
                                                    if ((!(fTmp < 1.4F)) && (!(fTmp > 1.59F)))
                                                        Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (1.59F - fTmp) + fRunk4[1, i] * (fTmp - 1.4F)) / 0.19F;
                                                    else
                                                        if ((!(fTmp < 1.6F)) && (!(fTmp > 1.79F)))
                                                            Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (1.79F - fTmp) + fRunk4[1, i] * (fTmp - 1.6F)) / 0.19F;
                                                        else
                                                            if (!(fTmp < 1.8F))
                                                                Norm[nAlg][ID_COMP[i]].value = (fRunk4[0, i] * (1.99F - fTmp) + fRunk4[1, i] * (fTmp - 1.8F)) / 0.19F;
                                                            else
                                                                Norm[nAlg][ID_COMP[i]].value = fRunk4[1, i];
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }
                        }

                        //??? - зачем предыдущие вычисления, если есть прямая ~ от пар.19 ???
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            if (Norm[@"19"][ID_COMP[i]].value == 0F)
                                Norm[nAlg][ID_COMP[i]].value = 0F;
                            else
                                ;
                        break;
                    #endregion

                    #region 21 - dqт бр(Gпв)
                    case @"21":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            fTmp = In[@"25"][ID_COMP[i]].value / Norm[@"1"][ID_COMP[i]].value;

                            if (fTmp > Norm[@"13"][ID_COMP[i]].value)
                                postfixFTable = @"-";
                            else
                                postfixFTable = @"+";

                            if (fTmp == Norm[@"13"][ID_COMP[i]].value)
                                Norm[nAlg][ID_COMP[i]].value = 0F;
                            else
                            {
                                switch (_modeDev[ID_COMP[i]])
                                {
                                    case MODE_DEV.COND_1:
                                        nameFTable = @"2.8";

                                        nameFTable += postfixFTable;
                                        nameFTable += @":1";

                                        Norm[nAlg][ID_COMP[i]].value = fTable.F1(nameFTable, Norm[@"13"][ID_COMP[i]].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.ELEKTRO1_2a:
                                    case MODE_DEV.TEPLO_3:
                                        nameFTable = @"2.8а";

                                        if (fTmp > Norm[@"13"][ID_COMP[i]].value)
                                            nameFTable += @"-";
                                        else
                                            nameFTable += @"+";

                                        nameFTable += @":2";

                                        Norm[nAlg][ID_COMP[i]].value = fTable.F2(nameFTable, Norm[@"13"][ID_COMP[i]].value, Norm[@"10"][ID_COMP[i]].value);
                                        break;
                                    default:
                                        //??? ошибка
                                        break;
                                }
                            }
                        }
                        break;
                    #endregion

                    #region 22 - dqт бр(рес)
                    case @"22":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            if (In[@"72"][id_comp].value > 35000F)
                                Norm[nAlg][id_comp].value = (Norm[@"12"][id_comp].value * 0.0085F * In[@"72"][id_comp].value - 35000) / 10000;
                            else
                                Norm[nAlg][id_comp].value = 0F;
                        }
                        break;
                    #endregion

                    #region 23 - dqт бр(пуск)
                    case @"23":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            if (!(Norm[@"2"][id_comp].value == 0))
                                Norm[nAlg][id_comp].value = 182.3F * In[@"69"][id_comp].value * 1000 / Norm[@"2"][id_comp].value;
                            else
                                ;
                        }
                        break;
                    #endregion

                    #region 24 - - dqт бр(ном)
                    case @"24":
                        fTmp = 0F;

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = Norm[@"12"][id_comp].value
                                + Norm[@"16"][id_comp].value
                                + Norm[@"17"][id_comp].value
                                + Norm[@"20"][id_comp].value
                                + Norm[@"21"][id_comp].value
                                + Norm[@"22"][id_comp].value
                                + Norm[@"23"][id_comp].value;

                            fTmp += Norm[nAlg][id_comp].value * Norm[@"2"][id_comp].value;
                        }

                        fRes = fTmp / Norm[@"2"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 25 - W т/тф(ном)
                    case @"25":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            switch (_modeDev[ID_COMP[i]])
                            {
                                case MODE_DEV.COND_1:
                                    Norm[nAlg][id_comp].value = 0F;
                                    break;
                                case MODE_DEV.ELEKTRO2_2:
                                    Norm[nAlg][id_comp].value = fTable.F2(@"2.9:2", Norm[@"13"][id_comp].value, Norm[@"10.1"][id_comp].value);
                                    break;
                                case MODE_DEV.ELEKTRO1_2a:
                                    Norm[nAlg][id_comp].value = fTable.F2(@"2.9а:2", Norm[@"13"][id_comp].value, Norm[@"38"][id_comp].value);
                                    break;
                                case MODE_DEV.TEPLO_3:
                                    Norm[nAlg][id_comp].value = fTable.F2(@"2.9б:2", Norm[@"13"][id_comp].value, Norm[@"38"][id_comp].value);
                                    break;
                                default:
                                    logErrorUnknownModeDev(nAlg, i);
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 26 -
                    case "26":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][id_comp].value = Out[@"4"][ID_COMP[i]].value
                                * (Out[@"57.1"][ID_COMP[i]].value - Out[@"60"][ID_COMP[i]].value) / .7F / 860;
                        }
                        break;
                    #endregion

                    #region 27 -
                    case "27":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (Out[@"4"][ID_COMP[i]].value == 0)
                                Norm[nAlg][id_comp].value = 0;
                            else
                                Norm[nAlg][id_comp].value = fTable.F1("2.9в:1", Out[@"13"][ID_COMP[i]].value);
                        }
                        break;
                    #endregion

                    #region 28 -
                    case "28":
                        //???
                        break;
                    #endregion

                    #region 29 -
                    case "29":
                        //???
                        break;
                    #endregion

                    #region 30 -
                    case "30":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][id_comp].value = fTable.F1("2.95:1", Out[@"13"][ID_COMP[i]].value);
                        }
                        break;
                    #endregion

                    #region 31 -
                    case "31":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][id_comp].value = 0.36F;
                        }
                        break;
                    #endregion

                    #region 32 -
                    case "32":
                        //n_blokov??
                        if (isRealTime)
                            fRes = fTable.F1("2.11:1", Out[@"14"][ID_COMP[ST]].value * n_blokov);
                        else
                            fRes = fTable.F1("2.11:1", Out[@"14"][ID_COMP[ST]].value);
                        break;
                    #endregion

                    #region 33 -
                    case "33":

                        break;
                    #endregion

                    #region 34 -
                    #endregion

                    #region 35 -
                    case "35":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            Norm[nAlg][id_comp].value = 5.19F * In["69"][ID_COMP[i]].value;
                            fRes += Norm[nAlg][id_comp].value;
                        }
                        break;
                    #endregion

                    #region 36 -
                    case "36":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (isRealTime)
                            {

                                Norm[nAlg][id_comp].value = (1.03F * (Out["30"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value
                                    + Out["31"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value + (Out["29"][ID_COMP[ST]].value
                                    + Out["32"][ID_COMP[ST]].value) * In["70"][ID_COMP[ST]].value / n_blokov1)
                                + Out["35"][ID_COMP[i]].value) / Out["2"][ID_COMP[i]].value * 100;

                            }
                            else

                                Norm[nAlg][id_comp].value = (1.03F * (Out["30"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value
                                    + Out["31"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value + (Out["29"][ID_COMP[ST]].value
                                    + Out["32"][ID_COMP[ST]].value) * In["70"][ID_COMP[ST]].value * Out[@"2"][ID_COMP[i]].value
                                    / Out[@"2"][ID_COMP[ST]].value)
                                + Out["35"][ID_COMP[i]].value) / Out["2"][ID_COMP[i]].value * 100;

                            fSum += Out["30"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value;
                            fSum1 += Out["31"][ID_COMP[i]].value * Out["1"][ID_COMP[i]].value;
                            fSum2=+ Norm[nAlg][id_comp].value;

                        }
                        fRes = (1.03F * (fSum + fSum1 + (Out["29"][ID_COMP[ST]].value + Out["32"][ID_COMP[ST]].value)
                            * In["70"][ID_COMP[ST]].value) + fSum2) / Out["2"][ID_COMP[ST]].value * 100;
                        break;
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

                    #region 49 - D пе
                    case @"49":
                        fRes = 0F;

                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = In[@"13"][id_comp].value + In[@"14"][id_comp].value;

                            fRes += Norm[nAlg][id_comp].value;
                        }
                        break;
                    #endregion

                    #region 50 - D пе
                    case @"50":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = In[@"49"][id_comp].value / In[@"1"][id_comp].value;
                        }

                        fRes = Norm[@"49"][ID_COMP[ST]].value / Norm[@"1"][ID_COMP[ST]].value;
                        break;
                    #endregion

                    #region 51 - t пе
                    case @"51":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            if (!(i == (int)INDX_COMP.iBL5))
                            {
                                id_comp = ID_COMP[i];

                                Norm[nAlg][id_comp].value = In[@"17"][id_comp].value + In[@"17.1"][id_comp].value / 2;
                            }
                            else
                                ;
                        }

                        Norm[nAlg][ID_COMP[BL5]].value = In[@"18"][id_comp].value
                            + In[@"18.1"][id_comp].value / 2
                            + fTable.F1(@"2.22:1", Out[@"50"][ID_COMP[BL5]].value);
                        break;
                    #endregion

                    #region 51.1 - t пе
                    case @"51.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            switch ((INDX_COMP)i)
                            {
                                case INDX_COMP.iBL2:
                                case INDX_COMP.iBL3:
                                    Norm[nAlg][ID_COMP[BL5]].value = In[@"17"][id_comp].value
                                        + In[@"17.1"][id_comp].value / 2
                                        - fTable.F1(@"2.22:1", Out[@"50"][ID_COMP[BL5]].value);
                                    break;
                                case INDX_COMP.iBL1:
                                case INDX_COMP.iBL4:
                                case INDX_COMP.iBL5:
                                case INDX_COMP.iBL6:
                                    Norm[nAlg][id_comp].value = In[@"18"][id_comp].value
                                        + In[@"18.1"][id_comp].value / 2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 52 - t гпп
                    case @"52":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            switch ((INDX_COMP)i)
                            {
                                default:
                                    Norm[nAlg][id_comp].value = In[@"21"][id_comp].value + In[@"21.1"][id_comp].value / 2;
                                    break;
                                case INDX_COMP.iBL5:
                                    Norm[nAlg][id_comp].value = In[@"22"][id_comp].value
                                        + In[@"22.1"][id_comp].value / 2
                                        + fTable.F1("2.22б:1", Norm[@"55"][id_comp].value / Norm[@"1"][id_comp].value);
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 52.1 - t гпп
                    case @"52.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            switch ((INDX_COMP)i)
                            {
                                case INDX_COMP.iBL2:
                                case INDX_COMP.iBL3:
                                    Norm[nAlg][id_comp].value = In[@"22"][id_comp].value
                                        + In[@"22.1"][id_comp].value / 2
                                        + fTable.F1("2.22б:1", Norm[@"55"][id_comp].value / Norm[@"1"][id_comp].value);
                                    break;
                                case INDX_COMP.iBL1:
                                case INDX_COMP.iBL4:
                                case INDX_COMP.iBL5:
                                case INDX_COMP.iBL6:
                                    Norm[nAlg][id_comp].value = In[@"22"][id_comp].value + In[@"22.1"][id_comp].value / 2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 53 - P гпп
                    case @"53":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = In[@"19"][id_comp].value;
                        }
                        break;
                    #endregion

                    #region 54 - D хпп
                    case @"54":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            switch (_modeDev[id_comp])
                            {
                                case MODE_DEV.COND_1:
                                    nameFTable = @"2.24а:1";
                                    break;
                                case MODE_DEV.ELEKTRO2_2:
                                case MODE_DEV.TEPLO_3:
                                    nameFTable = @"2.24:1";
                                    break;
                                case MODE_DEV.ELEKTRO1_2a:
                                    nameFTable = @"2.24б:1";
                                    break;
                                default:
                                    break;
                            }

                            Norm[nAlg][id_comp].value = fTable.F1(nameFTable, Norm[@"50"][id_comp].value);
                        }
                        break;
                    #endregion

                    #region 55 - D гпп
                    case @"55":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = Norm[@"54"][id_comp].value * Norm[@"1"][id_comp].value - In[@"46"][id_comp].value / 0.7F;
                        }
                        break;
                    #endregion

                    #region 56 - P пе
                    case @"56":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = In[@"15"][id_comp].value
                                + In[@"15.1"][id_comp].value / 2;

                            switch ((INDX_COMP)i)
                            {
                                case INDX_COMP.iBL1:
                                case INDX_COMP.iBL2:
                                case INDX_COMP.iBL3:
                                case INDX_COMP.iBL5:
                                    Norm[nAlg][id_comp].value += fTable.F1(@"2.41а:1", Norm[@"50"][id_comp].value);
                                    break;
                                case INDX_COMP.iBL4:
                                case INDX_COMP.iBL6:
                                default:
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 56.1 - Pо
                    case @"56.1":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            Norm[nAlg][id_comp].value = In[@"15.2"][id_comp].value;
                        }
                        break;
                    #endregion

                    #region 57 - i пе
                    case @"57":
                        for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                        {
                            id_comp = ID_COMP[i];

                            fTmp = Norm[@"51"][id_comp].value;

                            Norm[nAlg][id_comp].value = (float)(503.43F
                                + 11.02849F * (float)Math.Log((fTmp + 273.15F) / 647.27F)
                                + 229.2569F * (fTmp + 273.15) / 647.27F
                                + 37.93129F * (float)Math.Pow(((fTmp + 273.15F) / 647.27F), 2)
                                + 0.758195F - 7.97826F / (float)Math.Pow(((fTmp + 273.15F) / 1000), 2)
                                - (3.078455F * (fTmp + 273.15F) / 1000 - 0.21549F) / (float)Math.Pow(((fTmp + 273.15F) / 1000 - 0.21F), 3)
                                    * Norm[@"56"][id_comp].value / 100)
                                + (float)(0.0644126F - 0.268671F / (float)Math.Pow(((fTmp + 273.15F) / 1000), 8)
                                - 0.216661F / 100 / (float)Math.Pow(((fTmp + 273.15F) / 1000), 14))
                                    * (float)Math.Pow((Norm[@"56"][id_comp].value / 100), 2);
                        }
                        break;
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

                    case @"":
                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }

                return fRes;
            }
        }
    }
}
