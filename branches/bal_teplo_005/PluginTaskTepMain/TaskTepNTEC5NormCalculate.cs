﻿using ASUTP;
using System;

using TepCommon;

namespace PluginTaskTepMain
{
    public partial class HandlerDbTaskTepCalculate
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        partial class TaskTepCalculate
        {
            private enum ERROR { Unknown = -1 }

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
            private void logErrorUnknownModeDev(string nAlg, P_ALG.KEY_P_VALUE keyPValue)
            {
                ASUTP.Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @", ID_COMP=" + keyPValue.Id + @") - неизвестный режим работы оборудования ..."
                    , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Рассчитать значения для параметра в алгоритме расчета по идентификатору
            /// </summary>
            /// <param name="nAlg">Строковый идентификатор параметра</param>
            /// <returns>Значение для группы компонентов</returns>
            private float calculateNormative(string nAlg, DateTime stamp)
            {
                float fRes = 0F; // значение группы компонентов - результат выполнения

                int i = -1; // переменная цикла
                P_ALG.KEY_P_VALUE keyStationPValue
                    , keyPValue;
                float fSum = 0F, fSum1 = 0F, fSum2 = 0F // промежуточные для вычисления величины
                    , fTmp = -1F, fTmp1 = 0F, fTmp2 = 0F;
                int iDay = DateTime.Now.Day
                    , iMonth = DateTime.Now.Month;//пар.144
                float[] fRunkValues = new float[(int)FTable.FRUNK.COUNT]
                    , fServiceValues = null;
                // только для вычисления пар.20 - 4-х мерная функция
                string nameFTable = string.Empty
                    , postfixFTable = string.Empty;
                float[,] fRunk4 = null;

                keyStationPValue = new P_ALG.KEY_P_VALUE() { Id = ID_COMP[ST], Stamp = stamp };

                try
                {                
                    switch (nAlg)
                    {
                        #region 1, 2 - TAU раб, Э т
                        case @"1": //TAU раб
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[nAlg][keyPValue].value;
                            }
                            //??? для станции и для компонентов д.б. одинаковое значение
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            fRes = Norm[nAlg][keyPValue].value;
                            break;
                        case @"2": //Э т
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                
                                Norm[nAlg][keyPValue].value = In[nAlg][keyPValue].value;
                                //??? чтобы расчет привести к виду в образце
                                Norm[nAlg][keyPValue].value /= 1000000;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 3 - Q то
                        case @"3": //Q то                            
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (isRealTime == true)
                                    if (In[@"47"][keyPValue].value / In[@"1"][keyPValue].value < 0.7F)
                                        Norm[nAlg][keyPValue].value = 0F;
                                    else
                                        Norm[nAlg][keyPValue].value = In[@"47"][keyPValue].value * (In[@"48"][keyPValue].value - In[@"49"][keyPValue].value);
                                else
                                    Norm[nAlg][keyPValue].value = In[@"47"][keyPValue].value * (In[@"48"][keyPValue].value - In[@"49"][keyPValue].value);
                                //??? чтобы расчет привести к виду в образце
                                Norm[nAlg][keyPValue].value /= 1000;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 4 - Q пп
                        case @"4": //Q пп
                            if (isRealTime == true)
                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    if (Norm[@"3"][keyPValue].value == 0F)
                                        Norm[nAlg][keyPValue].value = 0F;
                                    else
                                        Norm[nAlg][keyPValue].value = In[@"46"][keyPValue].value;

                                    fRes += Norm[nAlg][keyPValue].value;
                                }
                            else
                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                                {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = In[@"46"][keyPValue].value;

                                    fRes += Norm[nAlg][keyPValue].value;
                                }
                            break;
                        #endregion

                        #region 5 - Q отп ст
                        case @"5": //Q отп ст
                            fRes = In[@"81"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 6 - Q отп роу
                        case @"6": //Q отп роу
                            fRes = In[@"82"][keyStationPValue].value;

                            if (isRealTime == true)
                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = Norm[@"4"][keyPValue].value / 2;
                                }
                            else
                            {
                                fSum = 0F;

                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    fSum += In[@"3"][keyPValue].value == 0 ? 0 : Norm[@"4"][keyPValue].value;
                                }

                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = Norm[@"3"][keyPValue].value == 0F ? 0F :
                                        Norm[@"4"][keyPValue].value == 0F ? 0F :
                                            fRes * Norm[@"4"][keyPValue].value / fSum;
                                }
                            }
                            break;
                        #endregion

                        #region 7 - Q отп тепл
                        case @"7":
                            fRes = Norm[@"5"][keyStationPValue].value - Norm[@"6"][keyStationPValue].value - In[@"85"][keyStationPValue].value;

                            if (isRealTime == true)
                            {
                                fTmp = qSeason ? 0.97F : 0.95F;
                                //16 мая - 30 сентября, 1 октября - 15 мая
                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = Norm[@"3"][keyPValue].value * fTmp;
                                }
                            }
                            else
                                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = Norm[@"3"][keyStationPValue].value == 0F ? 0F :
                                        fRes * Norm[@"3"][keyPValue].value / Norm[@"3"][keyStationPValue].value;
                                }
                            break;
                        #endregion

                        #region 8 - Q отп
                        case @"8":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"6"][keyPValue].value + Norm[@"7"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 9 - N т
                        case @"9":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (!(Norm[@"1"][keyPValue].value == 0F))
                                    Norm[nAlg][keyPValue].value = Norm[@"2"][keyPValue].value / Norm[@"1"][keyPValue].value;
                                else
                                    ;
                            }

                            fRes = Norm[@"2"][keyStationPValue].value / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 10 - Q т ср
                        case @"10":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"3"][keyPValue].value / Norm[@"1"][keyPValue].value;
                            }

                            fRes = Norm[@"3"][keyStationPValue].value / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 10.1 - P вто
                        case @"10.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"37"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 11 - Q роу ср
                        case @"11":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"4"][keyPValue].value / Norm[@"1"][keyPValue].value;
                            }

                            fRes = Norm[@"4"][keyStationPValue].value / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 12 - q т бр (исх)
                        case @"12":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                fTmp = 0F;

                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                        fTmp = fTable.F1(@"2.40:1", Norm[@"9"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                        fTmp = fTable.F3(@"2.1:3", Norm[@"9"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                        fTmp = fTable.F3(@"2.86:3", Norm[@"9"][keyPValue].value, Norm[@"10"][keyPValue].value, In[@"38"][keyPValue].value);
                                        break;
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        fTmp = fTable.F2(@"2.50:2", Norm[@"9"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }

                                Norm[nAlg][keyPValue].value = fTmp;
                            }
                            break;
                        #endregion

                        #region 13 - G о
                        case @"13":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = 0F;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                        fTmp = fTable.F1(@"2.55:1", Norm[@"9"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                        fTmp = fTable.F3(@"2.2:3", Norm[@"9"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                        fTmp = fTable.F3(@"2.87:3", Norm[@"9"][keyPValue].value, Norm[@"10"][keyPValue].value, In[@"38"][keyPValue].value);
                                        break;
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        fTmp = fTable.F3(@"2.2:3", Norm[@"9"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }

                                Norm[nAlg][keyPValue].value = fTmp + In[@"46"][keyPValue].value / 0.7F / In[@"1"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 14 - G 2
                        case @"14":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                fTmp = 0F;

                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                        fTmp = fTable.F1(@"2.3:1", Norm[@"13"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        fTmp = fTable.F3(@"2.3б:3", Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                        fTmp = fTable.F3(@"2.3а:3", Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value, In[@"38"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }

                                Norm[nAlg][keyPValue].value = fTmp - In[@"46"][keyPValue].value / 0.7F / In[@"1"][keyPValue].value;
                                fSum += Norm[nAlg][keyPValue].value;
                            }

                            if (isRealTimeBL1456 == true)
                                //??? почему умножаем на кол-во блоков
                                //??? как определяется кол-во блоков
                                Norm[nAlg][keyStationPValue].value = fSum * n_blokov1;
                            else
                                Norm[nAlg][keyStationPValue].value = fSum;
                            break;
                        #endregion

                        #region 14.1 - G цв
                        case @"14.1":
                            if (In[@"70"][keyStationPValue].value == 0F)
                                fRes = 0F;
                            else
                            {
                                fTmp = (isRealTimeBL1456 == true) ? n_blokov1 : In[@"89"][keyStationPValue].value;
                                fRunkValues[(int)FTable.FRUNK.F1] = fTmp;
                                fRunkValues[(int)FTable.FRUNK.F2] = (float)Math.Round((decimal)(In[@"6"][keyStationPValue].value / In[@"70"][keyStationPValue].value / 1.9F), 1);

                                fRes = fTable.F2(@"2.4а:2", fRunkValues[(int)FTable.FRUNK.F1], fRunkValues[(int)FTable.FRUNK.F2]);
                            }
                            break;
                        #endregion

                        #region 15 - P 2 (н)
                        case @"15":
                            fRunkValues[(int)FTable.FRUNK.F3] = -1F;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fRunkValues[(int)FTable.FRUNK.F1] = Norm[@"14"][keyPValue].value;
                                fRunkValues[(int)FTable.FRUNK.F2] = In[@"28"][keyPValue].value;

                                Norm[nAlg][keyPValue].value = fTable.F3(@"2.4:3", fRunkValues);
                            }
                            break;
                        #endregion

                        #region 15.1 - dQ э (P2)
                        case @"15.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fRunkValues[(int)FTable.FRUNK.F1] = Norm[@"15"][keyPValue].value;
                                fRunkValues[(int)FTable.FRUNK.F2] = Norm[@"14"][keyPValue].value;

                                Norm[nAlg][keyPValue].value = fTable.F2(@"2.84:2", fRunkValues);
                            }
                            break;
                        #endregion

                        #region 16 - dQ бр (P2)
                        case @"16":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (!(Norm[@"9"][keyPValue].value == 0F))
                                    Norm[nAlg][keyPValue].value = 1000 * Norm[@"15.1"][keyPValue].value / Norm[@"9"][keyPValue].value;
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

                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                        fTmp = fTable.F1(@"2.5а:1", Norm[@"13"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        fTmp = fTable.F2(@"2.5:2", Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }

                                Norm[nAlg][keyPValue].value = fTmp * Norm[@"11"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 18 - t 2(н)
                        case @"18":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                fTmp = 0F;

                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                        fTmp = -1F;
                                        ASUTP.Logging.Logg().Warning(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - не расчитывается при режиме '1 - Конденсационный'..."
                                            , Logging.INDEX_MESSAGE.NOT_SET);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                        fTmp = fTable.F1(@"2.6:1", Norm[@"10.1"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                        fTmp = fTable.F1(@"2.89:1", Norm[@"38"][keyPValue].value);
                                        break;
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        fTmp = fTable.F1(@"2.6:1", Norm[@"10.1"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }

                                Norm[nAlg][keyPValue].value = fTmp;
                            }
                            break;
                        #endregion

                        #region 19 - dt 2
                        case @"19":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"49"][keyPValue].value - Norm[@"18"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 20 - dqт бр (t 2)
                        case @"20":
                            fRunk4 = new float[2, (int)(INDX_COMP.COUNT - 1)];
                            // для левой границы [0, i] 4-х мерной функции
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                nameFTable = @"2.7";
                                fTmp = Norm[@"10.1"][keyPValue].value;

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

                                        if (!(Norm[@"19"][keyPValue].value < 0))
                                            postfixFTable = @"+";
                                        else
                                            postfixFTable = @"-";

                                        nameFTable += postfixFTable;
                                        nameFTable += @":3";

                                        fRunk4[0, i] = fTable.F3(nameFTable, Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"19"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }
                            }

                            // для правой границы [1, i] 4-х мерной функции
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                nameFTable = @"2.7";
                                fTmp = Norm[@"10.1"][keyPValue].value;

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

                                        if (!(Norm[@"19"][keyPValue].value < 0))
                                            postfixFTable = @"+";
                                        else
                                            postfixFTable = @"-";

                                        nameFTable += postfixFTable;
                                        nameFTable += @":3";

                                        fRunk4[1, i] = fTable.F3(nameFTable, Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value, Norm[@"19"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }
                            }

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = Norm[@"10.1"][keyPValue].value;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1: //[MODE_DEV].1 - Конденсационный
                                    case MODE_DEV.ELEKTRO1_2a: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                        break;
                                    case MODE_DEV.ELEKTRO2_2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                    case MODE_DEV.TEPLO_3: //[MODE_DEV].3 - По тепл. граф.
                                        if (fTmp < 0.8F)
                                            Norm[nAlg][keyPValue].value = fRunk4[0, i];
                                        else
                                            if ((!(fTmp < 0.8F)) && (!(fTmp > 0.99F)))
                                                Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (0.99F - fTmp) + fRunk4[1, i] * (fTmp - 0.8F)) / 0.19F;
                                            else
                                                if ((!(fTmp < 1.0F)) && (!(fTmp > 1.19F)))
                                                    Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (1.19F - fTmp) + fRunk4[1, i] * (fTmp - 1.0F)) / 0.19F;
                                                else
                                                    if ((!(fTmp < 1.2F)) && (!(fTmp > 1.39F)))
                                                        Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (1.39F - fTmp) + fRunk4[1, i] * (fTmp - 1.2F)) / 0.19F;
                                                    else
                                                        if ((!(fTmp < 1.4F)) && (!(fTmp > 1.59F)))
                                                            Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (1.59F - fTmp) + fRunk4[1, i] * (fTmp - 1.4F)) / 0.19F;
                                                        else
                                                            if ((!(fTmp < 1.6F)) && (!(fTmp > 1.79F)))
                                                                Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (1.79F - fTmp) + fRunk4[1, i] * (fTmp - 1.6F)) / 0.19F;
                                                            else
                                                                if (!(fTmp < 1.8F))
                                                                    Norm[nAlg][keyPValue].value = (fRunk4[0, i] * (1.99F - fTmp) + fRunk4[1, i] * (fTmp - 1.8F)) / 0.19F;
                                                                else
                                                                    Norm[nAlg][keyPValue].value = fRunk4[1, i];
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }
                            }

                            //??? - зачем предыдущие вычисления, если есть прямая ~ от пар.19 ???
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"19"][keyPValue].value == 0F)
                                    Norm[nAlg][keyPValue].value = 0F;
                                else
                                    ;
                            }
                            break;
                        #endregion

                        #region 21 - dqт бр(Gпв)
                        case @"21":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = In[@"25"][keyPValue].value / Norm[@"1"][keyPValue].value;

                                if (fTmp > Norm[@"13"][keyPValue].value)
                                    postfixFTable = @"-";
                                else
                                    postfixFTable = @"+";

                                if (fTmp == Norm[@"13"][keyPValue].value)
                                    Norm[nAlg][keyPValue].value = 0F;
                                else
                                {
                                    switch (_modeDev[i])
                                    {
                                        case MODE_DEV.COND_1:
                                            nameFTable = @"2.8";

                                            nameFTable += postfixFTable;
                                            nameFTable += @":1";

                                            Norm[nAlg][keyPValue].value = fTable.F1(nameFTable, Norm[@"13"][keyPValue].value);
                                            break;
                                        case MODE_DEV.ELEKTRO2_2:
                                        case MODE_DEV.ELEKTRO1_2a:
                                        case MODE_DEV.TEPLO_3:
                                            nameFTable = @"2.8а";

                                            if (fTmp > Norm[@"13"][keyPValue].value)
                                                nameFTable += @"-";
                                            else
                                                nameFTable += @"+";

                                            nameFTable += @":2";

                                            Norm[nAlg][keyPValue].value = fTable.F2(nameFTable, Norm[@"13"][keyPValue].value, Norm[@"10"][keyPValue].value);
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
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"72"][keyPValue].value > 35000F)
                                    Norm[nAlg][keyPValue].value = (Norm[@"12"][keyPValue].value * 0.0085F * In[@"72"][keyPValue].value - 35000) / 10000;
                                else
                                    Norm[nAlg][keyPValue].value = 0F;
                            }
                            break;
                        #endregion

                        #region 23 - dqт бр(пуск)
                        case @"23":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (!(Norm[@"2"][keyPValue].value == 0))
                                    Norm[nAlg][keyPValue].value = 182.3F * In[@"69"][keyPValue].value * 1000 / Norm[@"2"][keyPValue].value;
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
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"12"][keyPValue].value
                                    + Norm[@"16"][keyPValue].value
                                    + Norm[@"17"][keyPValue].value
                                    + Norm[@"20"][keyPValue].value
                                    + Norm[@"21"][keyPValue].value
                                    + Norm[@"22"][keyPValue].value
                                    + Norm[@"23"][keyPValue].value;

                                fTmp += Norm[nAlg][keyPValue].value * Norm[@"2"][keyPValue].value;
                            }

                            fRes = fTmp / Norm[@"2"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 25 - W т/тф(ном)
                        case @"25":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = 0F;
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                        Norm[nAlg][keyPValue].value = fTable.F2(@"2.9:2", Norm[@"13"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a:
                                        Norm[nAlg][keyPValue].value = fTable.F2(@"2.9а:2", Norm[@"13"][keyPValue].value, Norm[@"38"][keyPValue].value);
                                        break;
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = fTable.F2(@"2.9б:2", Norm[@"13"][keyPValue].value, Norm[@"38"][keyPValue].value);
                                        break;
                                    default:
                                        logErrorUnknownModeDev(nAlg, keyPValue);
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region 26 - Э тф п
                        case "26":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"4"][keyPValue].value
                                    * (Norm[@"57.1"][keyPValue].value - Norm[@"60"][keyPValue].value) / .7F / 860;
                            }
                            break;
                        #endregion

                        #region 27 - W п/тф
                        case "27":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"4"][keyPValue].value == 0)
                                    Norm[nAlg][keyPValue].value = 0;
                                else
                                    Norm[nAlg][keyPValue].value = fTable.F1("2.9в:1", Norm[@"13"][keyPValue].value);
                            }
                            break;
                        #endregion

                        #region 28 - N кн (ном)
                        case "28":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value =
                                    Norm[@"9"][keyPValue].value - Norm[@"25"][keyPValue].value * Norm[@"10"][keyPValue].value / 1000
                                    - Norm[@"27"][keyPValue].value * Norm[@"11"][keyPValue].value / 1000;
                            }

                            fSum = 0F;
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"1"][keyPValue].value;
                            }

                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iST]; keyPValue.Stamp = stamp;
                            fRes = fSum / Norm[@"1"][keyPValue].value;
                            break;
                        #endregion

                        #region 29 - N цн (н) гр
                        case "29":
                            fServiceValues = new float[(int)INDX_COMP.iST + 2];
                            fSum =
                            fSum1 =
                            fSum2 =
                                0F;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                fSum1 += Norm[@"14"][keyPValue].value;
                                fSum2 += In[@"28"][keyPValue].value;
                            }

                            if (isRealTime == true)
                                fSum1 *= n_blokov1;
                            else
                                ;

                            fSum2 /= n_blokov;

                            // 0-ой индекс оставить = "0"
                            i = 1;
                            fServiceValues[i] = 2.3F;

                            i = 2;
                            if (fSum1 < 240)
                                fServiceValues[i] = 2.3F;
                            else
                                if ((!(fSum1 < 240)) && (fSum1 < 470))
                                    if (!(fSum2 > fTable.F1(@"2.10:1", fSum1)))
                                        fServiceValues[i] = 2.3F;
                                    else
                                        fServiceValues[i] = 4.6F;
                                else
                                    fServiceValues[i] = 4.6F;

                            i = 3;
                            if (fSum1 < 390)
                                fServiceValues[i] = 4.6F;
                            else
                                if ((!(fSum1 < 390)) && (fSum1 < 870))
                                    if (!(fSum2 > fTable.F1(@"2.10б:1", fSum1)))
                                        fServiceValues[i] = 4.6F;
                                    else
                                        fServiceValues[i] = 6.9F;
                                else
                                    fServiceValues[i] = 6.9F;

                            i = 4;
                            if (fSum1 < 510)
                                fServiceValues[i] = 4.6F;
                            else
                                if ((!(fSum1 < 510)) && (fSum1 < 1200))
                                    if (!(fSum2 > fTable.F1(@"2.10в:1", fSum1)))
                                        fServiceValues[i] = 4.6F;
                                    else
                                        fServiceValues[i] = 6.9F;
                                else
                                    fServiceValues[i] = 6.9F;

                            i = 5;
                            if (fSum1 < 740)
                                if (!(fSum2 > fTable.F1(@"2.10д:1", fSum1)))
                                    fServiceValues[i] = 4.6F;
                                else
                                    fServiceValues[i] = 6.9F;
                            else
                                fServiceValues[i] = 6.9F;

                            // 6-ой копия 5-го
                            fServiceValues[6] = fServiceValues[5];
                            // 7-ой индекс оставить = "0"

                            fSum1 = (isRealTime == true) ? n_blokov1 : (float)Math.Floor(In[@"89"][keyStationPValue].value);
                            fSum2 = (isRealTime == true) ? n_blokov1 : (float)Math.Ceiling(In[@"89"][keyStationPValue].value);
                            fSum2 = fSum2 == fSum1 ? fSum2 + 1 : fSum2;

                            fTmp1 = -1F;
                            fTmp2 = -1F;
                            // пара значений 'fSum1' и 'fTmp1'
                            if ((!(fSum1 < 0))
                                && (fSum1 < fServiceValues.Length))
                                fTmp1 = fServiceValues[(int)fSum1];
                            else
                                ;
                            // пара значений 'fSum' и 'fTmp2'
                            if ((!(fSum2 < 0))
                                && (fSum2 < fServiceValues.Length))
                                fTmp2 = fServiceValues[(int)fSum2];
                            else
                                ;

                            fRes = fTmp1 * (fSum2 - ((isRealTime == true) ? n_blokov1 : In[@"89"][keyStationPValue].value))
                                + fTmp2 * (((isRealTime == true) ? n_blokov1 : In[@"89"][keyStationPValue].value) - fSum1);
                            break;
                        #endregion

                        #region 30 - N кэн (н)
                        case "30":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = fTable.F1("2.95:1", Norm[@"13"][keyPValue].value);
                            }
                            break;
                        #endregion

                        #region 31 - N бл(н)т
                        case "31":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 0.36F;
                            }
                            break;
                        #endregion

                        #region 32 - N ст(н)т гр
                        case "32":
                            //???n_blokov
                            if (isRealTime == true)
                                fRes = fTable.F1("2.11:1", Norm[@"14"][keyStationPValue].value * n_blokov);
                            else
                                fRes = fTable.F1("2.11:1", Norm[@"14"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 33 -
                        case "33":

                            break;
                        #endregion

                        #region 34 -
                        #endregion

                        #region 35 - Э т сн(пуск)
                        case "35":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 5.19F * In["69"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 36 - Э т сн/(ном)
                        case "36":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (isRealTime == true)
                                {
                                    Norm[nAlg][keyPValue].value = (1.03F * (Norm["30"][keyPValue].value * Norm["1"][keyPValue].value
                                        + Norm["31"][keyPValue].value * Norm["1"][keyPValue].value + (Norm["29"][keyStationPValue].value
                                        + Norm["32"][keyStationPValue].value) * In["70"][keyStationPValue].value / n_blokov1)
                                        + Norm["35"][keyPValue].value) / Norm["2"][keyPValue].value * 100;
                                }
                                else
                                    Norm[nAlg][keyPValue].value = (1.03F * (Norm["30"][keyPValue].value * Norm["1"][keyPValue].value
                                        + Norm["31"][keyPValue].value * Norm["1"][keyPValue].value + (Norm["29"][keyStationPValue].value
                                        + Norm["32"][keyStationPValue].value) * In["70"][keyStationPValue].value * Norm[@"2"][keyPValue].value
                                        / Norm[@"2"][keyStationPValue].value) + Norm["35"][keyPValue].value)
                                        / Norm["2"][keyPValue].value * 100;

                                fSum += Norm["30"][keyPValue].value * Norm["1"][keyPValue].value;
                                fSum1 += Norm["31"][keyPValue].value * Norm["1"][keyPValue].value;

                                fSum2 += Norm[nAlg][keyPValue].value;
                            }

                            fRes = (1.03F * (fSum + fSum1 + (Norm["29"][keyStationPValue].value + Norm["32"][keyStationPValue].value)
                                * In["70"][keyStationPValue].value) + fSum2) / Norm["2"][keyStationPValue].value * 100;
                            break;
                        #endregion

                        #region 37 - Э цн (ном) гр
                        case "37":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm["29"][keyStationPValue].value * In["70"][keyStationPValue].value * Norm["2"][keyPValue].value
                                    / Norm["2"][keyStationPValue].value / Norm["28"][keyPValue].value / Norm["1"][keyPValue].value * 100;
                            }

                            fRes = Norm["29"][keyStationPValue].value * In["70"][keyStationPValue].value
                                / Norm["28"][keyStationPValue].value / Norm["1"][keyStationPValue].value * 100;
                            break;
                        #endregion

                        #region 38 - Q т.о(отопл)
                        case "38":
                            if (In["43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F2("2.13:2", In["43"][keyStationPValue].value, (In["70"][keyStationPValue].value
                                    * 6 - Norm["1"][keyStationPValue].value) / (6 * In["70"][keyStationPValue].value));
                            break;
                        #endregion

                        #region 39 - Q т.о(вент)
                        case "39":
                            if (In["43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F2("2.13а:2", In["43"][keyStationPValue].value, Norm[@"65"][keyStationPValue].value / 447.5F);
                            break;
                        #endregion

                        #region 40 - Q т сн(пуск)
                        case "40":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 15.4F * In[@"69"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 41 - q т сн(ном)
                        case "41":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (isRealTime)
                                    Norm[nAlg][keyPValue].value = (Norm[@"38"][keyStationPValue].value + Norm[@"38"][keyStationPValue].value)
                                        * Norm[@"1"][keyPValue].value * 1E5F / n_blokov1 / (Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value);
                                else
                                    Norm[nAlg][keyPValue].value = ((Norm[@"38"][keyStationPValue].value + Norm[@"39"][keyStationPValue].value)
                                        * In[@"70"][keyStationPValue].value * Norm[@"1"][keyPValue].value / Norm[@"1"][keyStationPValue].value
                                        + Norm[@"40"][keyPValue].value) * 1E5F / (Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value);

                                fSum += Norm[@"40"][keyPValue].value;
                            }

                            fRes = ((Norm[@"48"][keyStationPValue].value + Norm[@"39"][keyStationPValue].value) * In[@"70"][keyStationPValue].value + fSum)
                                * 1E5F / (Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 42 - q т н (ном)
                        case "42":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"24"][keyPValue].value * (100 + Norm[@"41"][keyPValue].value)
                                    / (100 - Norm[@"36"][keyPValue].value);
                            }

                            fRes = Norm[@"24"][keyStationPValue].value * (100 + Norm[@"41"][keyStationPValue].value)
                                / (100 - Norm[@"36"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 43 - k по
                        case "43":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (Norm[@"60"][keyPValue].value + (Norm[@"59.1"][keyPValue].value
                                    - Norm[@"60"][keyPValue].value) - Norm[@"63"][keyPValue].value) / (Norm[@"57.1"][keyPValue].value
                                    + (Norm[@"59"][keyPValue].value - Norm[@"60"][keyPValue].value) - Norm[@"63"][keyPValue].value)
                                    * (1 + .4F * (Norm[@"57.1"][keyPValue].value - Norm[@"60"][keyPValue].value) / (Norm[@"57.1"][keyPValue].value
                                    + (Norm[@"59.1"][keyPValue].value - Norm[@"60"][keyPValue].value) - Norm[@"63"][keyPValue].value));
                            }
                            break;
                        #endregion

                        #region 44 - k то
                        case "44":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = Norm[@"59.1"][keyPValue].value - Norm[@"60"][keyPValue].value;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = 0;
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.ELEKTRO1_2a:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = ((Norm[@"62"][keyPValue].value
                                            - Norm[@"63"][keyPValue].value)
                                            / (Norm[@"57.1"][keyPValue].value + (fTmp)
                                           - Norm[@"63"][keyPValue].value))
                                           * (1 + .4F * (Norm[@"57.1"][keyPValue].value
                                               + (fTmp) - Norm[@"62"][keyPValue].value)
                                               / (Norm[@"57.1"][keyPValue].value + (fTmp)
                                              - Norm[@"63"][keyPValue].value));
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region 45 - dQ э по
                        case "45":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"3"][keyPValue].value + Norm[@"4"][keyPValue].value == 0)
                                {
                                    Norm[nAlg][keyPValue].value = 0;
                                }
                                else
                                    Norm[nAlg][keyPValue].value = (Norm[@"4"][keyPValue].value * (1 - Norm[@"43"][keyPValue].value)
                                        * Norm[@"8"][keyPValue].value) / (Norm[@"3"][keyPValue].value + Norm[@"4"][keyPValue].value);

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 46 - dQ э то
                        case "46":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"3"][keyPValue].value + Norm[@"4"][keyPValue].value == 0)
                                {
                                    Norm[nAlg][keyPValue].value = 0;
                                }
                                else
                                    Norm[nAlg][keyPValue].value = (Norm[@"4"][keyPValue].value * (1 - Norm[@"44"][keyPValue].value)
                                        * Norm[@"8"][keyPValue].value) / (Norm[@"3"][keyPValue].value + Norm[@"4"][keyPValue].value);

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 47 - dQ э
                        case "47":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"45"][keyPValue].value + Norm[@"46"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 48 - k отр (т)
                        case "48":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value
                                    * (100 + Norm[@"41"][keyPValue].value) / 1E5F + Norm[@"47"][keyPValue].value)
                                    / (Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value * (100 + Norm[@"41"][keyPValue].value) / 1E5F);

                                fSum += Norm[@"47"][keyPValue].value;
                            }

                            fRes += (Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value
                                * (100 + Norm[@"41"][keyStationPValue].value) / 1E5F + fSum)
                                / (Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value
                                * (100 + Norm[@"41"][keyStationPValue].value) / 1E5F);
                            break;
                        #endregion

                        #region 49 - D пе
                        case @"49":
                            fRes = 0F;

                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"13"][keyPValue].value + In[@"14"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 50 - D пе
                        case @"50":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"49"][keyPValue].value / In[@"1"][keyPValue].value;
                            }

                            fRes = Norm[@"49"][keyStationPValue].value / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 51 - t пе
                        case @"51":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                if (!(i == (int)INDX_COMP.iBL5))
                                {
                                    keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                    Norm[nAlg][keyPValue].value = In[@"17"][keyPValue].value + In[@"17.1"][keyPValue].value / 2;
                                }
                                else
                                    ;
                            }

                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL5]; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = In[@"18"][keyPValue].value
                                + In[@"18.1"][keyPValue].value / 2
                                + fTable.F1(@"2.22:1", Norm[@"50"][keyPValue].value);
                            break;
                        #endregion

                        #region 51.1 - t пе
                        case @"51.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                switch ((INDX_COMP)i)
                                {
                                    case INDX_COMP.iBL2:
                                    case INDX_COMP.iBL3:
                                        keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL3]; keyPValue.Stamp = stamp;
                                        Norm[nAlg][keyPValue].value = In[@"17"][keyPValue].value
                                            + In[@"17.1"][keyPValue].value / 2
                                            - fTable.F1(@"2.22:1", Norm[@"50"][keyPValue].value);
                                        break;
                                    case INDX_COMP.iBL1:
                                    case INDX_COMP.iBL4:
                                    case INDX_COMP.iBL5:
                                    case INDX_COMP.iBL6:
                                        keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                        Norm[nAlg][keyPValue].value = In[@"18"][keyPValue].value
                                            + In[@"18.1"][keyPValue].value / 2;
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
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch ((INDX_COMP)i)
                                {
                                    default:
                                        Norm[nAlg][keyPValue].value = In[@"21"][keyPValue].value + In[@"21.1"][keyPValue].value / 2;
                                        break;
                                    case INDX_COMP.iBL5:
                                        Norm[nAlg][keyPValue].value = In[@"22"][keyPValue].value
                                            + In[@"22.1"][keyPValue].value / 2
                                            + fTable.F1("2.22б:1", Norm[@"55"][keyPValue].value / Norm[@"1"][keyPValue].value);
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region 52.1 - t гпп
                        case @"52.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch ((INDX_COMP)i)
                                {
                                    case INDX_COMP.iBL2:
                                    case INDX_COMP.iBL3:
                                        Norm[nAlg][keyPValue].value = In[@"22"][keyPValue].value
                                            + In[@"22.1"][keyPValue].value / 2
                                            + fTable.F1("2.22б:1", Norm[@"55"][keyPValue].value / Norm[@"1"][keyPValue].value);
                                        break;
                                    case INDX_COMP.iBL1:
                                    case INDX_COMP.iBL4:
                                    case INDX_COMP.iBL5:
                                    case INDX_COMP.iBL6:
                                        Norm[nAlg][keyPValue].value = In[@"22"][keyPValue].value + In[@"22.1"][keyPValue].value / 2;
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
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"19"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 54 - D хпп
                        case @"54":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
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

                                Norm[nAlg][keyPValue].value = fTable.F1(nameFTable, Norm[@"50"][keyPValue].value);
                            }
                            break;
                        #endregion

                        #region 55 - D гпп
                        case @"55":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"54"][keyPValue].value * Norm[@"1"][keyPValue].value - In[@"46"][keyPValue].value / 0.7F;
                            }
                            break;
                        #endregion

                        #region 56 - P пе
                        case @"56":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"15"][keyPValue].value
                                    + In[@"15.1"][keyPValue].value / 2;

                                switch ((INDX_COMP)i)
                                {
                                    case INDX_COMP.iBL1:
                                    case INDX_COMP.iBL2:
                                    case INDX_COMP.iBL3:
                                    case INDX_COMP.iBL5:
                                        Norm[nAlg][keyPValue].value += fTable.F1(@"2.41а:1", Norm[@"50"][keyPValue].value);
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
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"15.2"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 57 - i пе
                        case @"57":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = Norm[@"51"][keyPValue].value + 273.15F;

                                Norm[nAlg][keyPValue].value = (float)(503.43F
                                    + 11.02849F * (float)Math.Log((fTmp) / 647.27F)
                                    + 229.2569F * (fTmp) / 647.27F
                                    + 37.93129F * (float)Math.Pow(((fTmp + 273.15F) / 647.27F), 2)
                                    + 0.758195F - 7.97826F / (float)Math.Pow(((fTmp) / 1000), 2)
                                    - (3.078455F * (fTmp) / 1000 - 0.21549F) / (float)Math.Pow(((fTmp) / 1000 - 0.21F), 3)
                                        * Norm[@"56"][keyPValue].value / 100)
                                    + (float)(0.0644126F - 0.268671F / (float)Math.Pow(((fTmp) / 1000), 8)
                                    - 0.216661F / 100 / (float)Math.Pow(((fTmp) / 1000), 14))
                                        * (float)Math.Pow((Norm[@"56"][keyPValue].value / 100), 2);
                            }
                            break;
                        #endregion

                        #region 57.1 - i оп
                        case @"57.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = Norm[@"51.1"][keyPValue].value + 273.15F;

                                Norm[nAlg][keyPValue].value = (float)(503.43F
                                   + 11.02849F * (float)Math.Log((fTmp) / 647.27F)
                                   + 229.2569F * (fTmp) / 647.27F
                                   + 37.93129F * (float)Math.Pow(((fTmp + 273.15F) / 647.27F), 2)
                                   + 0.758195F - 7.97826F / (float)Math.Pow(((fTmp) / 1000), 2)
                                   - (3.078455F * (fTmp) / 1000 - 0.21549F) / (float)Math.Pow(((fTmp) / 1000 - 0.21F), 3)
                                       * Norm[@"56"][keyPValue].value / 100) + (float)(0.0644126F - 0.268671F
                                       / (float)Math.Pow(((fTmp) / 1000), 8) - 0.216661F / 100 / (float)Math.Pow(((fTmp) / 1000), 14))
                                       * (float)Math.Pow((Norm[@"56"][keyPValue].value / 100), 2);
                            }
                            break;
                        #endregion

                        #region 58 - i пв
                        case "58":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = In[@"26"][keyPValue].value / 100;

                                Norm[nAlg][keyPValue].value = (float)(49.4F + 402.5F * fTmp + 4.767F * (float)Math.Pow((fTmp), 2)
                                    + .0333F * (float)Math.Pow((fTmp), 6)
                                    + (float)(-9.25F + 1.67F * fTmp + .00736F * (float)Math.Pow((fTmp), 6))
                                    - .008F * (float)Math.Pow((1 / (fTmp + .5F)), 5))
                                    * (float)(50 - In[@"16"][keyPValue].value * .0980665) / 10
                                    + (float)(-.073F + .079F * fTmp + .00068F * (float)Math.Pow((fTmp), 6))
                                    * (float)Math.Pow(((50 - In[@"16"][keyPValue].value * .0980665F) / 10F), 2)
                                    + 3.39F / 1E8F * (float)Math.Pow((fTmp), 12)
                                     + (float)Math.Pow(((50 - In[@"16"][keyPValue].value * .0980665F) / 10F), 4) / 4.1868F;
                            }
                            break;
                        #endregion

                        #region 59 - i гпп к
                        case "59":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fTmp = Norm[@"52"][keyPValue].value + 273.15F;

                                Norm[nAlg][keyPValue].value = (float)(503.43F + 11.02849 * (float)Math.Log(fTmp / 647.27F)
                                    + 229.2569F * fTmp / 647.27F + 37.93129F * (float)Math.Pow((fTmp / 647.27F), 2)
                                    + (float)Math.Pow((0.758195F - 7.97826F / (float)(fTmp) / 1000), 2)
                                    - (float)(3.078455F * fTmp / 1000 - .21549F) / (float)Math.Pow((fTmp / 1000 - .21), 3)
                                    * Norm[@"53"][keyPValue].value / 100 + (float)Math.Pow((.0644126F - .268671F / fTmp / 1000), 8)
                                    - .216661F / 100 / (float)Math.Pow((fTmp / 1000), 14)
                                    * (float)Math.Pow((Norm[@"53"][keyPValue].value / 100), 2));
                            }
                            break;
                        #endregion

                        #region 59.1 - i гпп т
                        case "59.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                fTmp = Norm[@"52.1"][keyPValue].value + 273.15F;

                                Norm[nAlg][keyPValue].value = (float)(503.43F + 11.02849 * (float)Math.Log(fTmp / 647.27F)
                                    + 229.2569F * fTmp / 647.27F + 37.93129F * (float)Math.Pow((fTmp / 647.27F), 2)
                                    + (float)Math.Pow((0.758195F - 7.97826F / (float)(fTmp) / 1000), 2)
                                    - (float)(3.078455F * fTmp / 1000 - .21549F) / (float)Math.Pow((fTmp / 1000 - .21), 3)
                                    * Norm[@"53"][keyPValue].value / 100 + (float)Math.Pow((.0644126F - .268671F / fTmp / 1000), 8)
                                    - .216661F / 100 / (float)Math.Pow((fTmp / 1000), 14)
                                    * (float)Math.Pow((Norm[@"53"][keyPValue].value / 100), 2));
                            }
                            break;
                        #endregion

                        #region 60 - i хпп
                        case "60":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                fTmp = In[@"24"][keyPValue].value + 273.15F;

                                Norm[nAlg][keyPValue].value = (float)(503.43F + 11.02849 * (float)Math.Log(fTmp / 647.27F)
                                    + 229.2569F * fTmp / 647.27F + 37.93129F * (float)Math.Pow((fTmp / 647.27F), 2)
                                    + (float)Math.Pow((0.758195F - 7.97826F / (float)(fTmp) / 1000), 2)
                                    - (float)(3.078455F * fTmp / 1000 - .21549F) / (float)Math.Pow((fTmp / 1000 - .21), 3)
                                    * Norm[@"53"][keyPValue].value / 100 + (float)Math.Pow((.0644126F - .268671F / fTmp / 1000), 8)
                                    - .216661F / 100 / (float)Math.Pow((fTmp / 1000), 14)
                                    * (float)Math.Pow((Norm[@"53"][keyPValue].value / 100), 2));
                            }
                            break;
                        #endregion

                        #region 61 - i пр
                        case "61":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;
                                fTmp = (float)Math.Log(In[@"41"][keyPValue].value);

                                fSum = 1 / (float)(2.6864264F - .20096551F * fTmp - 2.16688F / 1E3F
                                    * (float)Math.Pow(fTmp, 2) - 9.480808 / 1E5 * (float)Math.Pow(fTmp, 3)
                                    + 6.135062 / 1E6F * (float)Math.Pow(fTmp, 4) + 3.6917245 / 1E6F * (float)Math.Pow(fTmp, 5));
                                Norm[nAlg][keyPValue].value = -753.317F + 6959.4093F * fTmp - 29257.981F * (float)Math.Pow(fTmp, 2)
                                    + (float)Math.Pow(fTmp, 5);
                            }
                            break;
                        #endregion

                        #region 62 - i т
                        case "62":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        //???1 / 0;
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a:
                                        Norm[nAlg][keyPValue].value = fTable.F2("2.83а:2", Norm[@"50"][keyPValue].value, In[@"38"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = fTable.F2("2.83:2", Norm[@"50"][keyPValue].value, Norm[@"10.1"][keyPValue].value);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region 63 - i 2
                        case "63":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value / 98.067F);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value / 98.067F);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value / 98.067F);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value / 98.067F);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value / 98.067F);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.91:1", In[@"30"][keyPValue].value);
                            break;
                        #endregion

                        #region 64 - Q к бр
                        case "64":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (m_indxCompRealTime)
                                {
                                    case INDX_COMP.iBL1:
                                    case INDX_COMP.iBL2:
                                    case INDX_COMP.iBL3:
                                    case INDX_COMP.iBL4:
                                    case INDX_COMP.iBL6:
                                        Norm[nAlg][keyPValue].value = (Norm[@"49"][keyPValue].value * (Norm[@"57"][keyPValue].value
                                        - Norm[@"58"][keyPValue].value) + Norm[@"55"][keyPValue].value
                                        * (Norm[@"59"][keyPValue].value - Norm[@"60"][keyPValue].value) + In[@"27"][keyPValue].value
                                        * (Norm[@"61"][keyPValue].value - Norm[@"58"][keyPValue].value)) / 1000;
                                        break;
                                    case INDX_COMP.iBL5:
                                        Norm[nAlg][keyPValue].value = (Norm[@"49"][keyPValue].value * (Norm[@"57"][keyPValue].value
                                            - Norm[@"58"][keyPValue].value) + Norm[@"55"][keyPValue].value
                                            * (Norm[@"59"][keyPValue].value - Norm[@"60"][keyPValue].value) + In[@"25"][keyPValue].value
                                            * .004F * (Norm[@"61"][keyPValue].value - Norm[@"58"][keyPValue].value)) / 1000;
                                        break;
                                    default:
                                        break;
                                }

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 65 - Q к бр
                        case "65":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"64"][keyPValue].value / Norm[@"1"][keyPValue].value;
                            }

                            fRes = Norm[@"64"][keyStationPValue].value / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 66 - D пв
                        case "66":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (m_indxCompRealTime)
                                {
                                    case INDX_COMP.iBL1:
                                    case INDX_COMP.iBL2:
                                    case INDX_COMP.iBL3:
                                    case INDX_COMP.iBL4:
                                    case INDX_COMP.iBL6:
                                        Norm[nAlg][keyPValue].value = Norm[@"49"][keyPValue].value + In[@"27"][keyPValue].value;
                                        break;
                                    case INDX_COMP.iBL5:
                                        Norm[nAlg][keyPValue].value = Norm[@"49"][keyPValue].value + In[@"25"][keyPValue].value * .004F;
                                        break;
                                    default:
                                        break;
                                }

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 66.1 - D пв ср
                        case "66.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"66"][keyPValue].value / Norm[@"1"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 67 - alfa вэк (н)
                        case "67":
                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL1]; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.65:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL2];
                            Norm[nAlg][keyPValue].value = fTable.F2("2.65:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.65в:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.65в:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.63:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.63:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 68 - dalfa ух (н)
                        case "68":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.82:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.82:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.82:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.82:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.80:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.80:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 69 - alfa yx (н)
                        case "69":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"67"][keyPValue].value + Norm[@"68"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 70 - q 4 исх
                        case "70":
                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL1]; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.96:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.96:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.96:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.96:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = 0.947F;
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.96:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 71 - dq 4 (Ap)
                        case "71":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = .064F * (In[@"55"][keyStationPValue].value - 14.5F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = .064F * (In[@"55"][keyStationPValue].value - 14.5F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = .064F * (In[@"55"][keyStationPValue].value - 14.5F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = .064F * (In[@"55"][keyStationPValue].value - 14.5F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = .075F * (In[@"55"][keyStationPValue].value - 14.5F);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = .085F * (In[@"55"][keyStationPValue].value - 14.5F);
                            break;
                        #endregion

                        #region 72 - dq 4 (Wp)
                        case "72":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = .01F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = .01F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = .01F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = .01F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = .012F * (In[@"55"][keyStationPValue].value - 13.0F);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = .013F * (In[@"55"][keyStationPValue].value - 13.0F);
                            break;
                        #endregion

                        #region 73 - q 4 (н)
                        case "73":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"70"][keyPValue].value + Norm[@"71"][keyPValue].value
                                    + Norm[@"72"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 74 - t пв (н)
                        case "74":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.20:1", Norm[@"50"][keyPValue].value);
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.ELEKTRO1_2a:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.20а:1", Norm[@"50"][keyPValue].value);
                                        break;
                                    default:
                                        //??? 1/0;
                                        break;
                                }

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"66"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"66"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 75 - t yx исх
                        case "75":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.23:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.23:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.23а:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.23а:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.21:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.21:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 76 - dt yx (t пв)
                        case "76":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = .2F * (In[@"26"][keyPValue].value - Norm[@"74"][keyPValue].value);
                            }
                            break;
                        #endregion

                        #region 77 - dt yx (t вп)
                        case "77":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = .50F * ((In[@"32"][keyPValue].value + In[@"32.1"][keyPValue].value) / 2 - 30);
                            }
                            break;
                        #endregion

                        #region 78 - dt yx (t рец)
                        case "78":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = -.3F * ((In[@"32"][keyPValue].value + In[@"32.1"][keyPValue].value) / 2
                                    - (In[@"31"][keyPValue].value + In[@"31.1"][keyPValue].value) / 2);
                            }
                            break;
                        #endregion

                        #region 79 - t yx (н)
                        case "79":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"75"][keyPValue].value + Norm[@"76"][keyPValue].value
                                    + Norm[@"77"][keyPValue].value + Norm[@"78"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 80 - k
                        case "80":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (3.5F + .02F * 1E3F * In[@"54"][keyStationPValue].value / In[@"53"][keyStationPValue].value)
                                    * (100 - In[@"59"][keyPValue].value) / 1E2F + 3.53F * In[@"59"][keyPValue].value / 1E2F;
                            }
                            break;
                        #endregion

                        #region 81 - c
                        case "81":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (.4F + .04F * 1E3F * In[@"54"][keyStationPValue].value / In[@"53"][keyStationPValue].value)
                                    * (100 - In[@"59"][keyPValue].value) / 1E2F + .6F * In[@"59"][keyPValue].value / 1E2F;
                            }
                            break;
                        #endregion

                        #region 82 - b
                        case "82":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = .14F * (100F - In[@"59"][keyPValue].value)
                                    / 1E2F + .18F * In[@"59"][keyPValue].value / 1E2F;
                            }
                            break;
                        #endregion

                        #region 83 - q 2 (н)
                        case "83":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (Norm[@"80"][keyPValue].value * Norm[@"69"][keyPValue].value)
                                    * (Norm[@"79"][keyPValue].value - Norm[@"69"][keyPValue].value * (In[@"31"][keyPValue].value
                                    + In[@"31.1"][keyPValue].value) / 2 / (Norm[@"69"][keyPValue].value + Norm[@"82"][keyPValue].value))
                                    * (.9805F + .00013F * Norm[@"79"][keyPValue].value) * (1 - .01F * Norm[@"73"][keyPValue].value) / 1E2F
                                    + .2F * .95F * In[@"55"][keyStationPValue].value * (100 - In[@"59"][keyPValue].value) / 1E2F
                                    * Norm[@"79"][keyPValue].value / In[@"53"][keyStationPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 84 - q 5 (н)
                        case "84":
                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL1]; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.98:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.98:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.98:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.98:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.99:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.99:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 85 - q 6 (н)
                        case "85":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (100 - In[@"59"][keyPValue].value)
                                   * 0.02F / 100F;
                            }
                            break;
                        #endregion

                        #region 86 - q pec (н)
                        case "86":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"72"][keyPValue].value > 35000)
                                    Norm[nAlg][keyPValue].value = .0055F * (In[@"72"][keyPValue].value - 35000)
                                        / 1E3F;
                                else
                                    Norm[nAlg][keyPValue].value = 0;
                            }
                            break;
                        #endregion

                        #region 87 - q пуск (н)
                        case "87":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = In[@"69"][keyPValue].value * 64.2F * 7 * 1E2F / (Norm[@"64"][keyPValue].value * 1E2F
                                    / (100 - Norm[@"73"][keyPValue].value - Norm[@"83"][keyPValue].value - Norm[@"84"][keyPValue].value
                                    - Norm[@"85"][keyPValue].value - Norm[@"86"][keyPValue].value) + 64.2F * 7);
                            }
                            break;
                        #endregion

                        #region 88 - КПД к бр (ном)
                        case "88":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 100 - Norm[@"83"][keyPValue].value - Norm[@"73"][keyPValue].value
                                    - Norm[@"84"][keyPValue].value - Norm[@"85"][keyPValue].value - Norm[@"86"][keyPValue].value
                                    - Norm[@"87"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 89 - alfa уг
                        case "89":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 100 - In[@"59"][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 90 - В нат (ном)
                        case "90":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"64"][keyPValue].value * Norm[@"89"][keyPValue].value
                                    * 1E3F / Norm[@"88"][keyPValue].value / In[@"53"][keyStationPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 91 - Э тд(н) исх
                        case "91":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.27:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = fTable.F2("2.27:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.27в:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.27в:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.25:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.25:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 92 - dЭ тд(Wp)
                        case "92":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = .041F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = .041F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = .041F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = .041F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = .04F * (In["54"][keyStationPValue].value - 13.0F);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = .04F * (In["54"][keyStationPValue].value - 13.0F);
                            break;
                        #endregion

                        #region 93 - dЭ тд (t вп)
                        case "93":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = .004F * ((In[@"32"][keyPValue].value + In[@"32.1"][keyPValue].value) / 2 - 30)
                                    * (100 - In[@"59"][keyPValue].value) / 100;
                            }
                            break;
                        #endregion

                        #region 94 - dЭ тд (t рец)
                        case "94":
                            //??? 1/0
                            break;
                        #endregion

                        #region 95 - Э тд (ном)
                        case "95":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"91"][keyPValue].value + Norm["92"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 96 - Э пп (исх)
                        case "96":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            if (In[@"59"][keyPValue].value == 100)
                                Norm[nAlg][keyPValue].value = 0;
                            else
                                Norm[nAlg][keyPValue].value = fTable.F2("2.26:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL2;
                            if (In[@"59"][keyPValue].value == 100)
                                Norm[nAlg][keyPValue].value = 0;
                            else
                                Norm[nAlg][keyPValue].value = fTable.F2("2.26:2", Norm[@"65"][keyPValue].value, In[@"59"][keyPValue].value);
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.26:1(3)", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL1;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.26:1(3)", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL1;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.26а:1", Norm[@"65"][keyPValue].value);
                            keyPValue.Id = BL1;
                            Norm[nAlg][keyPValue].value = fTable.F1("2.26:1(6)", Norm[@"65"][keyPValue].value);
                            break;
                        #endregion

                        #region 97 - dЭ пп (исх)
                        case "97":
                            keyPValue.Id = BL1; keyPValue.Stamp = stamp;
                            Norm[nAlg][keyPValue].value = .297F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL2;
                            Norm[nAlg][keyPValue].value = .297F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL3;
                            Norm[nAlg][keyPValue].value = .297F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL4;
                            Norm[nAlg][keyPValue].value = .297F * (In[@"54"][keyStationPValue].value - 13.0F) * (100 - In[@"59"][keyPValue].value) / 100;
                            keyPValue.Id = BL5;
                            Norm[nAlg][keyPValue].value = .297F * (In["54"][keyStationPValue].value - 13.0F);
                            keyPValue.Id = BL6;
                            Norm[nAlg][keyPValue].value = .297F * (In["54"][keyStationPValue].value - 13.0F);
                            break;
                        #endregion

                        #region 98 - Э пп (н)
                        case "98":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"96"][keyPValue].value + Norm["97"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"90"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"90"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 99 - Э пэн (н)
                        case "99":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = fTable.F1("2.29:1", Norm[@"66.1"][keyPValue].value);

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"66"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"66"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 100 - Э тп (н)
                        case "100":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"59"][keyPValue].value == 100)
                                    Norm[nAlg][keyPValue].value = 0;
                                else
                                    if (In[@"43"][keyStationPValue].value >= 0)
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.31:1", In[@"43"][keyStationPValue].value);
                                    else
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.31а:1", In[@"43"][keyStationPValue].value);
                            }
                            break;
                        #endregion

                        #region 101 - Э разг (н)
                        case "101":
                            if (In[@"43"][keyStationPValue].value >= 0)
                                fRes = fTable.F1("2.32:1", In[@"43"][keyStationPValue].value);
                            else
                                fRes = fTable.F1("2.32а:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 102 - N зшу (н)
                        case "102":
                            //?? n_blokov1
                            if (isRealTime)
                                fRes = fTable.F1("2.33:1", n_blokov1);
                            else
                                fRes = fTable.F1("2.33:1", In[@"89"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 103 - N ов (н)
                        case "103":
                            fRes = fTable.F1("2.34:1", (In[@"78"][keyStationPValue].value + In[@"79"][keyStationPValue].value)
                                / In[@"70"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 104 - N маз (н)
                        case "104":
                            if (In[@"92"][keyStationPValue].value <= 0)
                                fRes = 208F;
                            else
                                fRes = fTable.F1("2.34а:1", In[@"92"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 105 - N доп.пр (н)
                        case "105":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fSum += Norm[@"64"][keyPValue].value;
                            }

                            if (isRealTime)
                                fRes = fTable.F1("2.51:1", fSum * n_blokov1);
                            else
                                fRes = fTable.F1("2.51:1", Norm[@"64"][keyStationPValue].value / In[@"70"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 105.1 - Э разм (н)
                        case "105a":
                            if (In[@"43"][keyStationPValue].value > 0)
                                fRes = 0;
                            else
                                fRes = fTable.F1("2.51а:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 106 - N пр (н)
                        case "106":
                            //n_blokov1
                            if (isRealTime)
                            {
                                fRes = (Norm[@"102"][keyStationPValue].value + Norm[@"103"][keyStationPValue].value + Norm[@"104"][keyStationPValue].value
                                    + Norm[@"105"][keyStationPValue].value) * In[@"70"][keyStationPValue].value / 1E3F / n_blokov1 + Norm[@"101"][keyStationPValue].value
                                    * In[@"88"][keyStationPValue].value / 1E3F + Norm[@"105а"][keyStationPValue].value * In[@"88"][keyStationPValue].value / 1E3F;
                            }
                            else
                                fRes = (Norm[@"102"][keyStationPValue].value + Norm[@"103"][keyStationPValue].value + Norm[@"104"][keyStationPValue].value
                                    + Norm[@"105"][keyStationPValue].value)
                                    * In[@"70"][keyStationPValue].value / 1E3F + Norm[@"101"][keyStationPValue].value * In[@"88"][keyStationPValue].value
                                    / 1E3F + Norm[@"105а"][keyStationPValue].value
                                    * In[@"88"][keyStationPValue].value / 1E3F;
                            break;
                        #endregion

                        #region 107 - Э пуск (н)
                        case "107":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 6.19F * In[@"69"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 108 - Э к сн (н)
                        case "108":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 1.03F * (Norm[@"95"][keyPValue].value * Norm[@"64"][keyPValue].value
                                    + Norm[@"98"][keyPValue].value * Norm[@"90"][keyPValue].value + Norm[@"99"][keyPValue].value
                                    * Norm[@"66"][keyPValue].value + Norm[@"100"][keyPValue].value * Norm[@"90"][keyPValue].value)
                                    / 1E3F + 1.03F * Norm[@"106"][keyStationPValue].value * Norm[@"64"][keyPValue].value / Norm[@"64"][keyStationPValue].value
                                    + Norm[@"107"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 109 - Q от к (н)
                        case "109":
                            if (In[@"43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F2("2.93:2", In[@"43"][keyStationPValue].value, (6 * In[@"70"][keyStationPValue].value
                                    - Norm[@"1"][keyStationPValue].value) / (6 * In[@"70"][keyStationPValue].value));
                            break;
                        #endregion

                        #region 110 - Q от к (н)
                        case "110":
                            if (In[@"43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F2("2.94:2", In[@"43"][keyStationPValue].value, Norm[@"65"][keyStationPValue].value / 447.5F);
                            break;
                        #endregion

                        #region 111 - Q от IIк (н)
                        case "111":
                            if (In[@"43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F1("2.12:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 112 - Q пвк (н)
                        case "112":
                            if (In[@"43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F1("2.14:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 113 - Q об.в (н)
                        case "113":
                            fRes = fTable.F2("2.15:2", (In[@"78"][keyStationPValue].value + In[@"79"][keyStationPValue].value)
                                / In[@"70"][keyStationPValue].value, In[@"91"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 114 - Q разм (н)
                        case "114":
                            if (In[@"43"][keyStationPValue].value > 10)
                                fRes = 0;
                            else
                                fRes = fTable.F1("2.16:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 115 - Q мх (н)
                        case "115":
                            fRes = fTable.F1("2.28:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 116 - Q маз сл (н)
                        case "116":
                            if (In[@"61"][keyStationPValue].value == 0)
                                fRes = 0;
                            else
                                fRes = fTable.F1("2.52:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 117 - Q пр.сл (н)
                        case "117":
                            //??? true
                            if (true)
                                fRes = 0;
                            else
                                fRes = fTable.F2("2.56:2", In[@"62"][keyStationPValue].value, In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 118 - Q пр (н)
                        case "118":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++) {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                fSum += Norm[@"65"][keyPValue].value;
                            }

                            fRes = fTable.F1("2.60:1", fSum);
                            break;
                        #endregion

                        #region 119 - Q пуск (н)
                        case "119":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 15.4F * In[@"69"][keyPValue].value;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 120 - Q к сн (н) гр
                        case "120":
                            fRes = (Norm[@"109"][keyStationPValue].value + Norm[@"110"][keyStationPValue].value + Norm[@"111"][keyStationPValue].value
                                + Norm[@"112"][keyStationPValue].value + Norm[@"113"][keyStationPValue].value + Norm[@"114"][keyStationPValue].value
                                + Norm[@"115"][keyStationPValue].value + Norm[@"118"][keyStationPValue].value) * In[@"70"][keyStationPValue].value
                                + Norm[@"11"][keyStationPValue].value * In[@"61"][keyStationPValue].value + Norm[@"117"][keyStationPValue].value
                                + Norm[@"119"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 121 - q к сн (н)
                        case "121":
                            //???n_blokov1
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (isRealTime)
                                    Norm[nAlg][keyPValue].value = Norm[@"120"][keyStationPValue].value / n_blokov1 * 1E2F / Norm[@"64"][keyPValue].value;
                                else
                                    switch (_modeDev[i])
                                    {
                                        case MODE_DEV.COND_1:
                                            Norm[nAlg][keyPValue].value = ((Norm[@"120"][keyStationPValue].value - Norm[@"119"][keyStationPValue].value)
                                                * Norm[@"64"][keyPValue].value / Norm[@"64"][keyStationPValue].value + Norm[@"119"][keyPValue].value)
                                                * 1E2F / Norm[@"64"][keyPValue].value;
                                            break;
                                        case MODE_DEV.ELEKTRO2_2:
                                        case MODE_DEV.ELEKTRO1_2a:
                                        case MODE_DEV.TEPLO_3:
                                            Norm[nAlg][keyPValue].value = ((Norm[@"120"][keyStationPValue].value - Norm[@"119"][keyStationPValue].value)
                                                * (Norm[@"3"][keyPValue].value + Norm["4"][keyPValue].value) / (Norm[@"3"][keyStationPValue].value
                                                + Norm[@"4"][keyStationPValue].value + Norm[@"119"][keyStationPValue].value) * 1E2F / Norm[@"64"][keyStationPValue].value);
                                            break;
                                        case MODE_DEV.COUNT:
                                            break;
                                        default:
                                            break;
                                    }
                            }

                            fRes = Norm[@"120"][keyStationPValue].value * 1E2F / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 122 - Q псг (н)
                        case "122":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = fTable.F1("2.17:1", In[@"43"][keyStationPValue].value);
                            }
                            break;
                        #endregion

                        #region 123 - Q труб (н)
                        case "123":
                            fRes = fTable.F1("2.18:1", In[@"43"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 124 - Q птс (н)
                        case "124":
                            fRes = fTable.F2("2.19:2", (In[@"50"][keyStationPValue].value + In[@"50"][keyStationPValue].value) * 1E3F
                                / In[@"70"][keyStationPValue].value, In[@"91"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 125 - alfa пот (н)
                        case "125":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"85"][keyStationPValue].value == 0)
                                    fTmp = fTable.F1("2.12б:1", In[@"43"][keyStationPValue].value) + Norm[@"112"][keyStationPValue].value;
                                else
                                    fTmp = 0;

                                if (isRealTime)
                                {
                                    if (Norm[@"8"][keyPValue].value == 0)
                                        Norm[nAlg][keyPValue].value = 0;
                                    else
                                        Norm[nAlg][keyPValue].value = ((Norm[@"123"][keyStationPValue].value * In[@"71"][keyStationPValue].value
                                           + Norm[@"124"][keyStationPValue].value * In[@"71"][keyStationPValue].value + 393 * (In[@"50"][keyStationPValue].value
                                           + In[@"51"][keyStationPValue].value) / 1E3F) / n_blokov1 + Norm[@"122"][keyPValue].value * In[@"71"][keyPValue].value)
                                           / Norm[@"8"][keyPValue].value * 100;
                                } else {
                                    if (Norm[@"8"][keyPValue].value == 0)
                                        Norm[nAlg][keyPValue].value = 0;
                                    else
                                        Norm[nAlg][keyPValue].value = ((Norm[@"123"][keyStationPValue].value * In[@"71"][keyStationPValue].value
                                            + (Norm[@"124"][keyStationPValue].value + fTmp) * In[@"70"][keyStationPValue].value + 393 * (In[@"50"][keyStationPValue].value
                                            + In[@"51"][keyStationPValue].value) / 1E3F) * Norm[@"8"][keyPValue].value / Norm[@"8"][keyStationPValue].value
                                            + Norm[@"122"][keyPValue].value * In[@"71"][keyPValue].value) / Norm[@"8"][keyPValue].value * 100;
                                }

                                fSum += Norm[@"122"][keyPValue].value * In[@"71"][keyPValue].value;
                            }

                            if (Norm[@"8"][keyStationPValue].value == 0)
                                fRes = 0;
                            else
                                fRes = (Norm[@"123"][keyStationPValue].value * In[@"71"][keyStationPValue].value + (Norm[@"124"][keyStationPValue].value
                                    + fTmp) * In[@"70"][keyStationPValue].value + 393 * (In[@"50"][keyStationPValue].value + In[@"51"][keyStationPValue].value)
                                    / 1E3F + fSum) / Norm[@"8"][keyStationPValue].value * 100;
                            break;
                        #endregion

                        #region 126 - Q нас (н)
                        case "126":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 1 / 1E3F * 860 * Norm[@"139"][keyPValue].value * In[@"71"][keyPValue].value * 85 / 1E2F;

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 126.1 - alfa нас (н)
                        case "126.1":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"8"][keyPValue].value == 0)
                                    Norm[nAlg][keyPValue].value = 0;
                                else
                                    Norm[nAlg][keyPValue].value = Norm[@"126"][keyPValue].value / Norm[@"8"][keyPValue].value * 1E2F;
                            }

                            if (Norm[@"8"][keyStationPValue].value == 0)
                                fRes = 0;
                            else
                                fRes = Norm[@"126"][keyStationPValue].value / Norm[@"8"][keyStationPValue].value * 1E2F;
                            break;
                        #endregion

                        #region 127 - К э
                        case "127":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = 1;
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.ELEKTRO1_2a:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = (Norm[@"24"][keyPValue].value * (100 + Norm[@"41"][keyPValue].value)
                                            * Norm[@"2"][keyPValue].value / 1E5F + Norm[@"47"][keyPValue].value) / (Norm[@"24"][keyPValue].value
                                            * (100 + Norm[@"41"][keyPValue].value) * Norm[@"2"][keyPValue].value / 1E5F + Norm[@"47"][keyPValue].value
                                            + (Norm[@"8"][keyPValue].value - Norm[@"126"][keyPValue].value) * (100 + Norm[@"125"][keyPValue].value) / 1E2F);
                                        break;
                                    default:
                                        break;
                                }

                                fSum += In[@"47"][keyPValue].value;
                            }

                            if (fSum == 0)
                                fRes = 0;
                            else
                                fRes = (Norm[@"24"][keyStationPValue].value * (100 + Norm[@"41"][keyStationPValue].value)
                                    * Norm[@"2"][keyStationPValue].value / 1E5F + Norm[@"47"][keyStationPValue].value) / (Norm[@"24"][keyStationPValue].value
                                    * (100 + Norm[@"41"][keyStationPValue].value) * Norm[@"2"][keyStationPValue].value / 1E5F + Norm[@"47"][keyStationPValue].value
                                    + (Norm[@"8"][keyStationPValue].value - Norm[@"126"][keyStationPValue].value) * (100 + Norm[@"125"][keyStationPValue].value) / 1E2F);
                            break;
                        #endregion

                        #region 128 - Э э сн (н)
                        case "128":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"36"][keyPValue].value + Norm[@"127"][keyPValue].value * Norm[@"108"][keyPValue].value
                                    / Norm[@"2"][keyPValue].value * 100;
                            }

                            fRes = Norm[@"36"][keyStationPValue].value + Norm[@"127"][keyStationPValue].value * Norm[@"108"][keyStationPValue].value
                                / Norm[@"2"][keyStationPValue].value * 100;
                            break;
                        #endregion

                        #region 129 - КПД к н (ном)
                        case "129":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"88"][keyPValue].value * (100 - Norm[@"121"][keyPValue].value)
                                    * (100 - Norm[@"126"][keyPValue].value) / 100 / (100 - Norm[@"36"][keyPValue].value);
                            }

                            fRes = Norm[@"88"][keyStationPValue].value * (100 - Norm[@"121"][keyStationPValue].value)
                                * (100 - Norm[@"126"][keyStationPValue].value) / 100 / (100 - Norm[@"36"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 130 - НЮ тп
                        case "130":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 100 - 1.0F * 472.2F / Norm[@"65"][keyPValue].value;
                            }

                            fRes = 100 - 1.0F * 472.2F / Norm[@"65"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 131 - К з
                        case "131":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"65"][keyPValue].value / 472.2F;
                            }

                            fRes = Norm[@"65"][keyStationPValue].value / 472.2F;
                            break;
                        #endregion

                        #region 132 - К ст
                        case "132":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (fTable.F1("2.35:1", Norm[@"131"][keyPValue].value) > 1)
                                    Norm[nAlg][keyPValue].value = 1;
                                else
                                    Norm[nAlg][keyPValue].value = fTable.F1("2.35:1", Norm[@"131"][keyPValue].value);

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"64"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 133 - K осв э
                        case "133":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"84"][keyPValue].value == 1)
                                    Norm[nAlg][keyPValue].value = 4;
                                else
                                    if (In[@"84"][keyPValue].value == 2)
                                        Norm[nAlg][keyPValue].value = 3;
                                    else
                                        if (In[@"84"][keyPValue].value > 2)
                                            Norm[nAlg][keyPValue].value = 0;
                                        else
                                            //??1 / 0
                                            ;
                                fSum += Norm[@"64"][keyPValue].value * Norm[nAlg][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 134 - K осв т
                        case "134":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"84"][keyPValue].value == 1)
                                    Norm[nAlg][keyPValue].value = 2;
                                else
                                    if (In[@"84"][keyPValue].value == 2)
                                        Norm[nAlg][keyPValue].value = 1.5F;
                                    else
                                        if (In[@"84"][keyPValue].value > 2)
                                            Norm[nAlg][keyPValue].value = 0;
                                        else
                                            //??1 / 0
                                            ;

                                fSum += Norm[@"64"][keyPValue].value * Norm[nAlg][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"64"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 135 - K отр (к)
                        case "135":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = 1 + Norm[@"47"][keyPValue].value / (Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value
                                    * (100 + Norm[@"41"][keyPValue].value) / 1E5F + Norm[@"8"][keyPValue].value * (100 - Norm[@"126.1"][keyPValue].value)
                                    * (100 + Norm[@"125"][keyPValue].value) / 1E4F);
                            }

                            if (In[@"81"][keyStationPValue].value == 0)
                                fRes = 1;
                            else
                                fRes = 1 + Norm[@"47"][keyStationPValue].value / (Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value
                                    * (100 + Norm[@"41"][keyStationPValue].value) / 1E5F + Norm[@"126"][keyStationPValue].value / Norm[@"8"][keyStationPValue].value
                                    * 100) * (100 + Norm[@"125"][keyStationPValue].value) / 1E4F;
                            break;
                        #endregion

                        #region 136 - b э (н)
                        case "136":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"42"][keyPValue].value
                                    * (100 + Norm[@"132"][keyPValue].value + Norm[@"133"][keyPValue].value)
                                    * Norm[@"48"][keyPValue].value * 100 / (Norm[@"129"][keyPValue].value
                                    * Norm[@"130"][keyPValue].value * 7 * Norm[@"135"][keyPValue].value);
                            }

                            fRes = Norm[@"42"][keyStationPValue].value
                                * (100 + Norm[@"132"][keyStationPValue].value + Norm[@"133"][keyStationPValue].value)
                                * Norm[@"48"][keyStationPValue].value * 100 / (Norm[@"129"][keyStationPValue].value
                                * Norm[@"130"][keyStationPValue].value * 7 * Norm[@"135"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 137 - b э (нор)
                        case "137":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"136"][keyPValue].value * (1 + In[@"64"][keyStationPValue].value
                                    * (1 - In[@"65"][keyStationPValue].value));
                            }

                            fRes = Norm[@"136"][keyStationPValue].value * (1 + In[@"64"][keyStationPValue].value
                                * (1 - In[@"65"][keyStationPValue].value));
                            break;
                        #endregion

                        #region 138 - G св
                        case "138":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (In[@"71"][keyPValue].value == 0)
                                    Norm[nAlg][keyPValue].value = 0;
                                else
                                    Norm[nAlg][keyPValue].value = In[@"47"][keyPValue].value / In[@"71"][keyPValue].value * 1E3F;

                                fSum += Norm[@"1"][keyPValue].value * Norm[nAlg][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"1"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 139 - N сет
                        case "139":
                            keyPValue.Id = ID_COMP[(int)INDX_COMP.iBL1]; keyPValue.Stamp = stamp;
                            if (Norm[@"138"][keyPValue].value <= 4500)
                                Norm[nAlg][keyPValue].value = (float)(fTable.F1("2.39б:1", Norm[@"138"][keyPValue].value)
                                    * (float)(1.4F - (float)Math.Min(1.4F, In[@"10.1"][keyPValue].value
                                    / In[@"1"][keyPValue].value)) + fTable.F1("2.39в:1", Norm[@"138"][keyPValue].value)
                                    * (float)Math.Min(1.4F, In[@"10.1"][keyPValue].value / In[@"1"][keyPValue].value)) / 1.4F;
                            else
                                Norm[nAlg][keyPValue].value = fTable.F1("2.39г:1", Norm[@"138"][keyPValue].value);

                            for (i = (int)INDX_COMP.iBL2; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"138"][keyPValue].value <= 4500)
                                {
                                    fTmp = (float)(fTable.F1("2.39б:1", Norm[@"138"][keyPValue].value)
                                        * (float)(1.4F - (float)Math.Min(1.4F, In[@"10.1"][keyPValue].value
                                        / In[@"1"][keyPValue].value)) + fTable.F1("2.39в:1", Norm[@"138"][keyPValue].value)
                                        * (float)Math.Min(1.4F, In[@"10.1"][keyPValue].value / In[@"1"][keyPValue].value)) / 1.4F;
                                }
                                else
                                    fTmp = fTable.F1("2.39е:1", Norm[@"138"][keyPValue].value);

                                Norm[nAlg][keyPValue].value = fTmp;
                            }
                            break;
                        #endregion

                        #region 140 - G птс
                        case "140":
                            fRes = (In[@"50"][keyStationPValue].value + In[@"51"][keyStationPValue].value) / In[@"71"][keyStationPValue].value * 1E3F;
                            break;
                        #endregion

                        #region 141 - N подп
                        case "141":
                            fRes = fTable.F1("2.36:1", Norm[@"140"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 142 - N хов
                        case "142":
                            if (In[@"43"][keyStationPValue].value < 8)
                                fRes = fTable.F1("2.37:1", Norm[@"140"][keyStationPValue].value);
                            else
                                fRes = fTable.F1("2.37а:1", Norm[@"140"][keyStationPValue].value);
                            break;
                        #endregion

                        #region 143 - N кнб
                        case "143":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = 0;
                                        break;
                                    case MODE_DEV.ELEKTRO1_2a:
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.38:1", Norm[@"10"][keyPValue].value) / 1E3F;
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = fTable.F1("2.38а:1", Norm[@"10"][keyPValue].value) / 1E3F;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region 144 - Э тепл(н)
                        case "144":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (isRealTime)
                                {
                                    if (((!(iMonth < 6) && !(iMonth > 9)) || iMonth == 5) && iDay > 15)
                                    {
                                        switch (_modeDev[i])
                                        {
                                            case MODE_DEV.UNKNOWN:
                                                break;
                                            case MODE_DEV.COND_1:
                                                Norm[nAlg][keyPValue].value = 0;
                                                break;
                                            case MODE_DEV.ELEKTRO2_2:
                                            case MODE_DEV.ELEKTRO1_2a:
                                            case MODE_DEV.TEPLO_3:
                                                Norm[nAlg][keyPValue].value = (Norm[@"139"][keyPValue].value + Norm[@"143"][keyPValue].value)
                                                    * In[@"71"][keyPValue].value + (Norm[@"141"][keyStationPValue].value + Norm[@"142"][keyStationPValue].value)
                                                    * In[@"71"][keyStationPValue].value * Norm[@"8"][keyPValue].value / Norm[@"8"][keyStationPValue].value;
                                                break;
                                            case MODE_DEV.COUNT:
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (_modeDev[i])
                                        {
                                            case MODE_DEV.UNKNOWN:
                                                break;
                                            case MODE_DEV.COND_1:
                                                Norm[nAlg][keyPValue].value = 0;
                                                break;
                                            case MODE_DEV.ELEKTRO2_2:
                                            case MODE_DEV.ELEKTRO1_2a:
                                            case MODE_DEV.TEPLO_3:
                                                Norm[nAlg][keyPValue].value = (Norm[@"139"][keyPValue].value + Norm[@"143"][keyPValue].value)
                                                   * In[@"71"][keyPValue].value + (Norm[@"141"][keyStationPValue].value + Norm[@"142"][keyStationPValue].value)
                                                   * In[@"71"][keyStationPValue].value * Norm[@"7"][keyPValue].value / Norm[@"7"][keyStationPValue].value;
                                                break;
                                            case MODE_DEV.COUNT:
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    switch (_modeDev[i])
                                    {
                                        case MODE_DEV.UNKNOWN:
                                            break;
                                        case MODE_DEV.COND_1:
                                            Norm[nAlg][keyPValue].value = 0;
                                            break;
                                        case MODE_DEV.ELEKTRO2_2:
                                        case MODE_DEV.ELEKTRO1_2a:
                                        case MODE_DEV.TEPLO_3:
                                            Norm[nAlg][keyPValue].value = (Norm[@"139"][keyPValue].value + Norm[@"143"][keyPValue].value)
                                                    * In[@"71"][keyPValue].value + (Norm[@"141"][keyStationPValue].value + Norm[@"142"][keyStationPValue].value)
                                                    * In[@"71"][keyStationPValue].value * Norm[@"8"][keyPValue].value / Norm[@"8"][keyStationPValue].value;
                                            break;
                                        case MODE_DEV.COUNT:
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                fRes += Norm[nAlg][keyPValue].value;
                            }
                            break;
                        #endregion

                        #region 145 - b тэ бл
                        case "145":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                switch (_modeDev[i])
                                {
                                    case MODE_DEV.UNKNOWN:
                                        break;
                                    case MODE_DEV.COND_1:
                                        Norm[nAlg][keyPValue].value = 0;
                                        break;
                                    case MODE_DEV.ELEKTRO2_2:
                                    case MODE_DEV.ELEKTRO1_2a:
                                    case MODE_DEV.TEPLO_3:
                                        Norm[nAlg][keyPValue].value = (100 + Norm[@"125"][keyPValue].value) * (100 + Norm[@"132"][keyPValue].value
                                            + Norm[@"134"][keyPValue].value) * 1E3F / (Norm[@"129"][keyPValue].value * Norm[@"130"][keyPValue].value
                                            * 7 * Norm[@"135"][keyPValue].value);
                                        break;
                                    case MODE_DEV.COUNT:
                                        break;
                                    default:
                                        break;
                                }

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"8"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"8"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 146 - b тэ пвк
                        case "146":
                            fRes = In[@"86"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 146.1 - alfa пвк
                        case "146.1":
                            fRes = In[@"85"][keyStationPValue].value / Norm[@"5"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 147 - db тэ
                        case "147":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                if (Norm[@"8"][keyPValue].value == 0)
                                    Norm[nAlg][keyPValue].value = 0;
                                else
                                    Norm[nAlg][keyPValue].value = Norm[@"144"][keyPValue].value
                                        * Norm[@"136"][keyPValue].value / Norm[@"8"][keyPValue].value;

                                fSum += Norm[nAlg][keyPValue].value * Norm[@"3"][keyPValue].value;
                            }

                            fRes = fSum / Norm[@"3"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 148 - b тэ (н)
                        case "148":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"145"][keyPValue].value * (100 - Norm["126.1"][keyPValue].value) / 1E2F
                                    + Norm[@"147"][keyPValue].value;
                            }

                            fRes = Norm[@"145"][keyStationPValue].value * (100 - Norm["126.1"][keyStationPValue].value) / 1E2F
                                + Norm[@"147"][keyStationPValue].value; ;
                            break;
                        #endregion

                        #region 149 - b тэ (н) ст
                        case "149":
                            fRes = (Norm[@"145"][keyStationPValue].value * (100 - Norm[@"146.1"][keyStationPValue].value - Norm[@"126.1"][keyStationPValue].value)
                                + Norm[@"146"][keyStationPValue].value * Norm[@"146.1"][keyStationPValue].value) / 1E2F + In[@"87"][keyStationPValue].value * 1E3F
                                / Norm[@"5"][keyStationPValue].value + Norm[@"147"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 150 - b тэ (нр)
                        case "150":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"145"][keyStationPValue].value * (100 - Norm["126.1"][keyPValue].value) / 1E2F
                                    * (1 + In[@"66"][keyStationPValue].value * (1 - In[@"67"][keyStationPValue].value)) + Norm[@"147"][keyPValue].value
                                    * Norm[@"137"][keyPValue].value / Norm["136"][keyPValue].value;
                            }

                            fRes = Norm[@"145"][keyStationPValue].value * (100 - Norm["146.1"][keyStationPValue].value - Norm["126.1"][keyStationPValue].value)
                                * (1 + In[@"66"][keyStationPValue].value * (1 - In[@"67"][keyStationPValue].value)) / 1E2F + Norm[@"146"][keyStationPValue].value
                                * Norm[@"146.1"][keyStationPValue].value / 1E2F + In[@"87"][keyStationPValue].value * 1E3F / Norm[@"5"][keyStationPValue].value
                                + Norm[@"147"][keyStationPValue].value * Norm[@"137"][keyStationPValue].value / Norm[@"136"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 151 - balance
                        case "151":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value / 1E3F
                                    + (Norm[@"8"][keyPValue].value - Norm[@"126"][keyPValue].value) * (100 + Norm[@"125"][keyPValue].value)
                                    / 1E2F + Norm[@"41"][keyPValue].value * Norm[@"24"][keyPValue].value * Norm[@"2"][keyPValue].value
                                    / 1E3F / 1E2F + Norm[@"121"][keyPValue].value * Norm[@"64"][keyPValue].value / 100 + 472.2F
                                    * Norm[@"1"][keyPValue].value / 1E2F;
                            }

                            fRes = Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value / 1E3F
                                    + (Norm[@"8"][keyStationPValue].value - Norm[@"126"][keyStationPValue].value) * (100 + Norm[@"125"][keyStationPValue].value)
                                    / 1E2F + Norm[@"41"][keyStationPValue].value * Norm[@"24"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value
                                    / 1E3F / 1E2F + Norm[@"121"][keyStationPValue].value * Norm[@"64"][keyStationPValue].value / 100 + 472.2F
                                    * Norm[@"1"][keyStationPValue].value / 1E2F;
                            break;
                        #endregion

                        #region 152 - balance
                        case "152":
                            for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                            {
                                keyPValue.Id = ID_COMP[i]; keyPValue.Stamp = stamp;

                                Norm[nAlg][keyPValue].value = (Norm[@"64"][keyPValue].value - Norm["151"][keyPValue].value)
                                    / Norm[@"64"][keyPValue].value * 1E2F;
                            }

                            fRes = (Norm[@"64"][keyStationPValue].value - Norm["151"][keyStationPValue].value)
                                    / Norm[@"64"][keyStationPValue].value * 1E2F;
                            break;
                        #endregion

                        #region 153 - S Э сн (н)
                        case "153":
                            fRes = Norm[@"36"][keyStationPValue].value * Norm[@"2"][keyStationPValue].value / 1E2F + Norm[@"108"][keyStationPValue].value
                                + Norm[@"144"][keyStationPValue].value;
                            break;
                        #endregion

                        #region 154 - S Э сн (ф)
                        case "154":
                            fRes = In[@"3"][keyStationPValue].value;
                            break;
                        #endregion

                        case @"":
                        default:
                            ASUTP.Logging.Logg().Error(@"TaskTepCalculate::calculateNormative (N_ALG=" + nAlg + @") - неизвестный параметр...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                    }
                }
                catch (Exception e)
                {
                    ASUTP.Logging.Logg().Exception(e, @"TaskTepCalculate::calculateNormative (nAlg=" + nAlg + @") - ...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }

                return fRes;
            }
        }
    }
}
