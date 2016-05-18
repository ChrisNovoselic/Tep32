using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskTepMain
{
    public class HandlerDbTaskTepCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();

            m_taskCalculate = new HandlerDbTaskCalculate.TaskTepCalculate();
        }
        /// <summary>
        /// Корректировка входных (сырых) значений - аналог 'import.prg'
        /// </summary>
        protected override void correctValues(ref DataTable tableSesion, ref DataTable tableProject)
        {
            (m_taskCalculate as HandlerDbTaskCalculate.TaskTepCalculate).CorrectValues(ref tableSesion, ref tableProject);
        }

        protected override void calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type, out int err)
        {
            err = -1;

            DataTable tableOrigin = null
                , tableCalcRes = null;

            TepCommon.HandlerDbTaskCalculate.TaskCalculate.ListDATATABLE listDataTables = null;

            // подготовить таблицы для расчета
            listDataTables = prepareTepCalculateValues(type, out err);
            if (err == 0)
            {
                // произвести вычисления
                switch (type)
                {
                    case TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                        tableOrigin = listDataTables.FindDataTable(TepCommon.HandlerDbTaskCalculate.TaskCalculate.INDEX_DATATABLE.OUT_NORM_VALUES);
                        tableCalcRes = (m_taskCalculate as HandlerDbTaskCalculate.TaskTepCalculate).CalculateNormative(listDataTables);
                        break;
                    case TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES:
                        tableCalcRes = (m_taskCalculate as HandlerDbTaskCalculate.TaskTepCalculate).CalculateMaket(listDataTables);
                        break;
                    default:
                        break;
                }
                // сохранить результаты вычисления
                saveResult(tableOrigin, tableCalcRes, out err);
            }
            else
                Logging.Logg().Error(@"HandlerDbTaskCalculate::Calculate () - при подготовке данных для расчета...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
    
    public partial class HandlerDbTaskCalculate : TepCommon.HandlerDbValues
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public partial class TaskTepCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Признак расчета ТЭП-оперативно
            /// </summary>
            protected /*override */bool isRealTime { get { return !(m_indxCompRealTime == INDX_COMP.UNKNOWN); } }
            //protected virtual bool isRealTime { get { return _type == TYPE.OUT_TEP_REALTIME; } }

            private bool isRealTimeBL1456
            {
                get
                {
                    return (m_indxCompRealTime == INDX_COMP.iBL1)
                        || (m_indxCompRealTime == INDX_COMP.iBL4)
                        || (m_indxCompRealTime == INDX_COMP.iBL5)
                        || (m_indxCompRealTime == INDX_COMP.iBL6);
                }
            }
            /// <summary>
            /// ???
            /// </summary>
            int n_blokov
                , n_blokov1;
            /// <summary>
            /// Перечисления индексы для массива идентификаторов компонентов оборудования ТЭЦ
            /// </summary>
            private enum INDX_COMP : short
            {
                UNKNOWN = -1
                    , iBL1, iBL2, iBL3, iBL4, iBL5, iBL6,
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
                    , ST = 5;
            /// <summary>
            /// Массив - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private readonly int[] ID_COMP =
            {
                BL1, BL2, BL3, BL4, BL5, BL6
                    , ST
            };
            /// <summary>
            /// Индекс целевого компонента ТЭЦ при расчете ТЭП-оперативно
            /// </summary>
            private INDX_COMP m_indxCompRealTime;
            /// <summary>
            /// Объект, обеспечивающий вычисление нормативных значений при работе оборудования ТЭЦ
            /// </summary>
            private FTable fTable;
            /// <summary>
            /// Словарь с расчетными НОРМативными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            private P_ALG Norm;
            /// <summary>
            /// Перечисление - режимы работы оборудования
            /// </summary>
            private enum MODE_DEV : short
            {
                UNKNOWN = -1, COND_1 = 1, ELEKTRO2_2, ELEKTRO1_2a,
                TEPLO_3
                    , COUNT
            }
            /// <summary>
            /// Словарь - режимы работы для компонентов станции
            /// </summary>
            private Dictionary<int, MODE_DEV> _modeDev;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskTepCalculate()
                : base()
            {
                m_indxCompRealTime = INDX_COMP.UNKNOWN;

                In = new P_ALG();
                Norm = new P_ALG();
                Out = new P_ALG();

                fTable = new FTable();
            }

            protected override int initValues(ListDATATABLE listDataTables)
            {
                int iRes = -1;

                // инициализация нормативных значений для оборудования
                fTable.Set(listDataTables.FindDataTable(INDEX_DATATABLE.FTABLE));
                // инициализация входных значений
                iRes = initInValues(listDataTables.FindDataTable(INDEX_DATATABLE.IN_PARAMETER)
                    , listDataTables.FindDataTable(INDEX_DATATABLE.IN_VALUES));

                // инициализация выходных-нормативных значений
                iRes = initNormValues(listDataTables.FindDataTable(INDEX_DATATABLE.OUT_NORM_PARAMETER)
                    , listDataTables.FindDataTable(INDEX_DATATABLE.OUT_NORM_VALUES));

                return iRes;
            }

            private int initInValues(DataTable tablePar, DataTable tableVal)
            {
                int iRes = 0;

                MODE_DEV mDev = MODE_DEV.UNKNOWN;

                iRes = initValues(In, tablePar, tableVal);

                if (In.ContainsKey(@"74") == true)
                {
                    _modeDev = new Dictionary<int, MODE_DEV>();

                    for (int i = (int)INDX_COMP.iBL1; (i < (int)INDX_COMP.COUNT) && (iRes == 0); i++)
                    {
                        switch ((int)In[@"74"][ID_COMP[i]].value)
                        {
                            case 1: //[MODE_DEV].1 - Конденсационный
                                mDev = MODE_DEV.COND_1;
                                break;
                            case 2: //[MODE_DEV].2 - Электр.граф (2 ст.)
                                mDev = MODE_DEV.ELEKTRO2_2;
                                break;
                            case 3: //[MODE_DEV].2а - Электр.граф (1 ст.)
                                mDev = MODE_DEV.ELEKTRO1_2a;
                                break;
                            case 4: //[MODE_DEV].3 - По тепл. граф.
                                mDev = MODE_DEV.TEPLO_3;
                                break;
                            default:
                                iRes = -1;

                                logErrorUnknownModeDev(@"InitInValues", ID_COMP[i]);
                                break;
                        }

                        if ((_modeDev.ContainsKey(i) == false)
                            && (!(mDev == MODE_DEV.UNKNOWN)))
                            _modeDev.Add(i, mDev);
                        else
                            ;
                    }
                }
                else
                {
                    iRes = -1;

                    Logging.Logg().Error(@"TaskTepCalculate::initInValues () - во входной таблице не установлен режим оборудования...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return iRes;
            }

            private int initNormValues(DataTable tablePar, DataTable tableVal)
            {
                int iRes = -1;

                iRes = initValues(Norm, tablePar, tableVal);

                return iRes;
            }

            private int initMktValues(DataTable tablePar, DataTable tableVal)
            {
                int iRes = -1;

                iRes = initValues(Out, tablePar, tableVal);

                return iRes;
            }
            /// <summary>
            /// Корректировка входных (сырых) значений - аналог 'import.prg'
            /// </summary>
            public void CorrectValues(ref DataTable tableData, ref DataTable tablePrjParameter)
            {
                string nAlg = string.Empty;
                DataRow[] rowsPar = null;
                int id_put = -1
                    , id_comp = -1;
                double[] arValues = new double [ID_COMP.Length];
                double dblVal = -1F
                    , _2b_d_2st = -1F;

                #region Электро - оперативный расчет
                if (isRealTime == true)
                {// только при оперативном расчете                    
                    switch (m_indxCompRealTime)
                    {
                        case INDX_COMP.iBL1:
                            id_comp = BL1;
                            break;
                        case INDX_COMP.iBL2:
                            id_comp = BL2;
                            break;
                        case INDX_COMP.iBL3:
                            id_comp = BL3;
                            break;
                        case INDX_COMP.iBL4:
                            id_comp = BL4;
                            break;
                        case INDX_COMP.iBL5:
                            id_comp = BL5;
                            break;                        
                        case INDX_COMP.iBL6:
                            id_comp = BL6;
                            break;
                        default:
                            break;
                    }
                    // определить коэффициент по 2-му параметру
                    nAlg = @"'2'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + id_comp);
                    _2b_d_2st = (double)tableData.Select(@"ID_PUT=" + (int)rowsPar[0][@"ID"])[0][@"VALUE"]
                        / (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"];
                    // "взвешивание" 4-го параметра
                    nAlg = @"'4'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                    dblVal = 0F;
                    for (int i = 0; i < rowsPar.Length; i++)
                        switch ((int)rowsPar[i][@"ID_COMP"])
                        {
                            case BL1:
                            case BL2:
                            case BL3:
                            case BL4:
                            case BL5:
                            case BL6:
                                dblVal += (double)tableData.Select(@"ID_PUT=" + (int)rowsPar[i][@"ID"])[0][@"VALUE"];
                                break;
                            case ST:
                            default:
                                break;
                        }
                    tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = ((double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] - dblVal) * _2b_d_2st;
                    // "взвешивание" 7-го параметра
                    nAlg = @"'7'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                    dblVal = 0F;
                    for (int i = 0; i < rowsPar.Length; i++)
                        switch ((int)rowsPar[i][@"ID_COMP"])
                        {
                            case BL1:
                            case BL2:
                            case BL3:
                            case BL4:
                            case BL5:
                            case BL6:
                                dblVal += (double)tableData.Select(@"ID_PUT=" + (int)rowsPar[i][@"ID"])[0][@"VALUE"];
                                break;
                            case ST:
                            default:
                                break;
                        }
                    tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = ((double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] - dblVal) * _2b_d_2st;
                    // "взвешивание" 10-го параметра
                    nAlg = @"'10'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                    dblVal = 0F;
                    for (int i = 0; i < rowsPar.Length; i++)
                        switch ((int)rowsPar[i][@"ID_COMP"])
                        {
                            case BL1:
                            case BL2:
                            case BL3:
                            case BL4:
                            case BL5:
                            case BL6:
                                dblVal += (double)tableData.Select(@"ID_PUT=" + (int)rowsPar[i][@"ID"])[0][@"VALUE"];
                                break;
                            case ST:
                            default:
                                break;
                        }
                    tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = ((double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] - dblVal) * _2b_d_2st;
                    // замена 2-го парметра
                    nAlg = @"'2'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + id_comp);
                    tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = tableData.Select(@"ID_PUT=" + (int)rowsPar[0][@"ID"])[0][@"VALUE"];
                    // замена 3-го парметра
                    nAlg = @"'3'";
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)ST);
                    id_put = (int)rowsPar[0][@"ID_PUT"];
                    rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + id_comp);
                    tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = tableData.Select(@"ID_PUT=" + (int)rowsPar[0][@"ID"])[0][@"VALUE"];
                    // 31-ый параметр скопировать в 32-ой, 31.1 параметр скопировать в 32.1
                    for (INDX_COMP i = (INDX_COMP.UNKNOWN + 1); i < INDX_COMP.iBL6; i++)
                    {// все компоненты за исключением, BL6, ST
                        // 31-ый параметр скопировать в 32-ой - получить значение
                        nAlg = @"'31'";
                        rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)i);
                        id_put = (int)rowsPar[0][@"ID_PUT"];
                        dblVal = (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"];
                        // сохранить значение
                        nAlg = @"'32'";
                        rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)i);
                        id_put = (int)rowsPar[0][@"ID_PUT"];
                        tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = dblVal;
                        // 31.1 параметр скопировать в 32.1 - получить значения
                        nAlg = @"'31.1'";
                        rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)i);
                        id_put = (int)rowsPar[0][@"ID_PUT"];
                        dblVal = (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"];
                        // сохранить значение
                        nAlg = @"'32.1'";
                        rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg + @" AND ID_COMP=" + (int)i);
                        id_put = (int)rowsPar[0][@"ID_PUT"];
                        tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = dblVal;
                    }
                }
                else
                    ;
                #endregion

                #region Электро - станция - 10.3
                nAlg = @"'10.3'";
                rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                dblVal = 0F;
                //??? проверить на кол-во строк (строк д.б. не больше ID_COMP.Length)
                for (int i = 0; i < rowsPar.Length; i ++)
                    switch ((int)rowsPar[i][@"ID_COMP"])
                    {
                        case BL1:
                        case BL2:
                        case BL3:
                        case BL4:
                        case BL5:
                            dblVal += (double)tableData.Select(@"ID_PUT=" + (int)rowsPar[i][@"ID"])[0][@"VALUE"];
                            break;
                        case ST:
                            id_put = (int)rowsPar[i][@"ID"];
                            break;
                        case BL6:
                        default:
                            break;
                    }

                tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = dblVal;
                #endregion

                #region Электро - 12
                // если 0, то 0
                #endregion

                #region Тепло - 37
                nAlg = @"'37'";
                rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                //??? проверить на кол-во строк (строк д.б. не больше ID_COMP.Length)
                for (int i = 0; i < rowsPar.Length; i++)
                    switch ((int)rowsPar[i][@"ID_COMP"])
                    {
                        case BL1:
                        case BL2:
                        case BL3:
                        case BL4:
                        case BL5:
                        case BL6:
                            id_put = (int)rowsPar[i][@"ID"];
                            tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] + 1F;
                            break;                        
                        case ST:
                        default:
                            break;
                    }
                #endregion

                #region Тепло - 38
                nAlg = @"'38'";
                rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                //??? проверить на кол-во строк (строк д.б. не больше ID_COMP.Length)
                for (int i = 0; i < rowsPar.Length; i++)
                    switch ((int)rowsPar[i][@"ID_COMP"])
                    {
                        case BL1:
                        case BL2:
                        case BL3:
                        case BL4:
                        case BL5:
                        case BL6:
                            id_put = (int)rowsPar[i][@"ID"];
                            tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] + 1F;
                            break;                                                
                        case ST:
                        default:
                            break;
                    }
                #endregion

                #region Тепло - 46
                nAlg = @"'46'";
                rowsPar = tablePrjParameter.Select(@"N_ALG=" + nAlg);
                //??? проверить на кол-во строк (строк д.б. не больше ID_COMP.Length)
                for (int i = 0; i < rowsPar.Length; i++)
                    switch ((int)rowsPar[i][@"ID_COMP"])
                    {
                        case BL1:
                        case BL2:
                        case BL3:
                        case BL4:
                        case BL5:
                        case BL6:
                            id_put = (int)rowsPar[i][@"ID"];
                            tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] = (double)tableData.Select(@"ID_PUT=" + id_put)[0][@"VALUE"] * .7F;
                            break;
                        default:
                            break;
                    }            
                #endregion

                #region Тепло - станция 80
                nAlg = @"'81'";
                #endregion

                #region Тепло - станция 81
                nAlg = @"'82'";
                #endregion
            }
            /// <summary>
            /// Расчитать выходные-нормативные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица нормативных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateNormative(ListDATATABLE listDataTables)
            {
                int iInitValuesRes = -1;

                DataTable tableRes = null;

                iInitValuesRes = initValues(listDataTables);

                if (iInitValuesRes == 0)
                {
                    // расчет
                    calculateNormative();

                    // преобразование в таблицу
                    tableRes = resultToTable(Norm);
                }
                else
                    ; // ошибка при инициализации параметров, значений

                return tableRes;
            }

            private int calculateNormative()
            {
                int iRes = 0;

                /*-------------1 - TAU раб-------------*/
                Norm[@"1"][ST].value = calculateNormative(@"1");
                /*-------------2 - Э т-------------*/
                Norm[@"2"][ID_COMP[ST]].value = calculateNormative(@"2");
                /*-------------3 - Q то-------------*/
                Norm[@"3"][ID_COMP[ST]].value = calculateNormative(@"3");
                /*-------------4 - Q пп-------------*/
                Norm[@"4"][ID_COMP[ST]].value = calculateNormative(@"4");
                /*-------------5 - Q отп ст-------------*/
                Norm[@"5"][ID_COMP[ST]].value = calculateNormative(@"5");
                /*-------------6 - Q отп роу-------------*/
                Norm[@"6"][ID_COMP[ST]].value = calculateNormative(@"6");
                /*-------------7 - Q отп тепл-------------*/
                Norm[@"7"][ID_COMP[ST]].value = calculateNormative(@"7");
                /*-------------8 - Q отп-------------*/
                Norm[@"8"][ID_COMP[ST]].value = calculateNormative(@"8");
                /*-------------9 - N т-------------*/
                Norm[@"9"][ID_COMP[ST]].value = calculateNormative(@"9");
                /*-------------10 - Q т ср-------------*/
                Norm[@"10"][ID_COMP[ST]].value = calculateNormative(@"10");
                /*-------------10.1 P вто-------------*/
                calculateNormative(@"10.1");
                /*-------------11 - Q роу ср-------------*/
                Norm[@"11"][ID_COMP[ST]].value = calculateNormative(@"11");
                /*-------------12 - q т бр (исх)-------------*/
                calculateNormative(@"12");
                /*-------------13 - G о-------------*/
                calculateNormative(@"13");
                /*-------------14 - G 2-------------*/
                Norm[@"14"][ID_COMP[ST]].value = calculateNormative(@"14");
                /*-------------14.1 - G цв-------------*/
                Norm[@"14.1"][ID_COMP[ST]].value = calculateNormative(@"14.1");
                /*-------------15 - P 2 (н)-------------*/
                calculateNormative(@"15");
                /*-------------15.1 - dQ э (P2)-------------*/
                calculateNormative(@"15.1");
                /*-------------16 - dQ бр (P2)-------------*/
                calculateNormative(@"16");
                /*-------------17 - dqт бр (P2)-------------*/
                calculateNormative(@"17");
                /*-------------18 - t 2(н)-------------*/
                calculateNormative(@"18");
                /*-------------19 - dt 2-------------*/
                calculateNormative(@"19");
                /*-------------20 - dqт бр (t 2)-------------*/
                calculateNormative(@"20");
                /*-------------21 - dqт бр(Gпв)-------------*/
                calculateNormative(@"21");
                /*-------------22 - dqт бр(рес)-------------*/
                calculateNormative(@"22");
                /*-------------23 - dqт бр(пуск)-------------*/
                calculateNormative(@"23");
                /*-------------24 - dqт бр(ном) -------------*/
                Norm[@"24"][ID_COMP[ST]].value = calculateNormative(@"24");
                /*-------------25 - W т/тф(ном)-------------*/
                calculateNormative(@"25");

                //Изменение прямого порядка вычисления
                /*-------------49 - D пе -------------*/
                Norm[@"49"][ID_COMP[ST]].value = calculateNormative(@"49");
                /*-------------50 - D пе -------------*/
                Norm[@"50"][ID_COMP[ST]].value = calculateNormative(@"50");
                /*-------------51 - t пе -------------*/
                calculateNormative(@"51");
                /*-------------51.1 - t оп -------------*/
                calculateNormative(@"51.1");

                //Изменение прямого порядка вычисления
                /*-------------54 - D хпп-------------*/
                calculateNormative(@"54");
                /*-------------55 - D гпп-------------*/
                calculateNormative(@"55");

                //Изменение прямого порядка вычисления
                /*-------------52 - t гпп-------------*/
                calculateNormative(@"52");
                /*-------------52.1 - t гпп-------------*/
                calculateNormative(@"52.1");
                /*-------------53 - P гпп-------------*/
                calculateNormative(@"53");

                //Изменение прямого порядка вычисления
                /*-------------56 - P пе-------------*/
                calculateNormative(@"56");
                /*-------------57 - i пе-------------*/
                calculateNormative(@"57");
                /*-------------57.1 - i оп-------------*/
                calculateNormative(@"57.1");
                /*-------------58 i пв-------------*/
                calculateNormative(@"58");
                /*-------------59 i гпп к-------------*/
                calculateNormative(@"59");
                /*-------------59.1 i гпп т-------------*/
                calculateNormative(@"59.1");
                /*-------------60 i хпп-------------*/
                calculateNormative(@"60");
                /*-------------61 i пр-------------*/
                calculateNormative(@"61");
                /*-------------62 i т-------------*/
                calculateNormative(@"62");
                /*-------------63 i 2-------------*/
                calculateNormative(@"63");
                /*-------------64 Q к бр-------------*/
                calculateNormative(@"64");

                //Изменение прямого порядка вычисления
                /*-------------59 - доля газа только для бл.1,2 в опер.расч.-------------*/
                calculateNormative(@"59");

                //Изменение прямого порядка вычисления
                /*-------------65 Q к бр-------------*/
                calculateNormative(@"65");

                //Изменение прямого порядка вычисления
                /*-------------26 Э тф п-------------*/
                calculateNormative(@"26");
                /*-------------27 -------------*/
                /*-------------28 -------------*/
                /*-------------29 -------------*/
                /*-------------30 -------------*/
                /*-------------31 -------------*/
                /*-------------32 -------------*/
                /*-------------33 -------------*/
                /*-------------34 -------------*/
                /*-------------35 -------------*/
                /*-------------36 -------------*/
                /*-------------37 -------------*/
                /*-------------38 -------------*/
                /*-------------39 -------------*/
                /*-------------40 -------------*/
                /*-------------41 -------------*/
                /*-------------42 -------------*/
                /*-------------43 -------------*/
                /*-------------44 -------------*/
                /*-------------45 -------------*/
                /*-------------46 -------------*/
                /*-------------47 -------------*/
                /*-------------48 -------------*/

                return iRes;
            }
            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateMaket(ListDATATABLE listDataTables)
            {
                int iInitValuesRes = -1;

                DataTable tableRes = null;

                iInitValuesRes = initValues(listDataTables);

                if (iInitValuesRes == 0)
                {
                    // расчет
                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Norm)
                        pAlg.Value[ST].value = calculateNormative(pAlg.Key);

                    foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Out)
                        pAlg.Value[ST].value = calculateMaket(pAlg.Key);

                    // преобразование в таблицу
                    tableRes = resultToTable(Out);
                }
                else
                    ; // ошибка при инициализации параметров, значений

                return tableRes;
            }

            private int calculateMaket()
            {
                int iRes = 0;

                return iRes;
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
        }
    }
}
