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
            //(m_taskCalculate as HandlerDbTaskCalculate.TaskTepCalculate).CorrectValues(ref tableSesion, ref tableProject);
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
            /// Признак расчета ТЭП-оперативно (для любого из эн./блоков)
            /// </summary>
            protected /*override */bool isRealTime { get { return !(m_indxCompRealTime == INDX_COMP.UNKNOWN); } }
            //protected virtual bool isRealTime { get { return _type == TYPE.OUT_TEP_REALTIME; } }
            /// <summary>
            /// Признак расчета ТЭП-оперативно (только для одного из эн./блоков №№1, 4, 5, 6)
            /// </summary>
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
            /// ???Количество блоков ВСЕГО
            /// </summary>
            private int n_blokov;
            /// <summary>
            /// ???Количество работающих блоков
            /// </summary>
            private int n_blokov1;
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

            private struct NodeCalc { public string nAlg; public bool IsGroup; }

            private int calculateNormative()
            {
                int iRes = 0;

                NodeCalc[] arCalculate = new NodeCalc[] {
                    /*-------------1 - TAU раб-------------*/
                    new NodeCalc() { nAlg = @"1", IsGroup = true }
                    /*-------------2 - Э т-------------*/
                    , new NodeCalc() { nAlg = @"2", IsGroup = true }
                    /*-------------3 - Q то-------------*/
                    , new NodeCalc() { nAlg = @"3", IsGroup = true }
                    /*-------------4 - Q пп-------------*/
                    , new NodeCalc() { nAlg = @"4", IsGroup = true }
                    /*-------------5 - Q отп ст-------------*/
                    , new NodeCalc() { nAlg = @"5", IsGroup = true }
                    /*-------------6 - Q отп роу-------------*/
                    , new NodeCalc() { nAlg = @"6", IsGroup = true }
                    /*-------------7 - Q отп тепл-------------*/
                    , new NodeCalc() { nAlg = @"7", IsGroup = true }
                    /*-------------8 - Q отп-------------*/
                    , new NodeCalc() { nAlg = @"8", IsGroup = true }
                    /*-------------9 - N т-------------*/
                    , new NodeCalc() { nAlg = @"9", IsGroup = true }
                    /*-------------10 - Q т ср-------------*/
                    , new NodeCalc() { nAlg = @"10", IsGroup = true }
                    /*-------------10.1 P вто-------------*/
                    , new NodeCalc() { nAlg = @"10.1", IsGroup = false }
                    /*-------------11 - Q роу ср-------------*/
                    , new NodeCalc() { nAlg = @"11", IsGroup = true }
                    /*-------------12 - q т бр (исх)-------------*/
                    , new NodeCalc() { nAlg = @"12", IsGroup = false }
                    /*-------------13 - G о-------------*/
                    , new NodeCalc() { nAlg = @"13", IsGroup = false }
                    /*-------------14 - G 2-------------*/
                    , new NodeCalc() { nAlg = @"14", IsGroup = true }
                    /*-------------14.1 - G цв-------------*/
                    , new NodeCalc() { nAlg = @"14.1", IsGroup = true }
                    /*-------------15 - P 2 (н)-------------*/
                    , new NodeCalc() { nAlg = @"15", IsGroup = false }
                    /*-------------15.1 - dQ э (P2)-------------*/
                    , new NodeCalc() { nAlg = @"15.1", IsGroup = false }
                    /*-------------16 - dQ бр (P2)-------------*/
                    , new NodeCalc() { nAlg = @"16", IsGroup = false }
                    /*-------------17 - dqт бр (P2)-------------*/
                    , new NodeCalc() { nAlg = @"17", IsGroup = false }
                    /*-------------18 - t 2(н)-------------*/
                    , new NodeCalc() { nAlg = @"18", IsGroup = false }
                    /*-------------19 - dt 2-------------*/
                    , new NodeCalc() { nAlg = @"19", IsGroup = false }
                    /*-------------20 - dqт бр (t 2)-------------*/
                    , new NodeCalc() { nAlg = @"20", IsGroup = false }
                    /*-------------21 - dqт бр(Gпв)-------------*/
                    , new NodeCalc() { nAlg = @"21", IsGroup = false }
                    /*-------------22 - dqт бр(рес)-------------*/
                    , new NodeCalc() { nAlg = @"22", IsGroup = false }
                    /*-------------23 - dqт бр(пуск)-------------*/
                    , new NodeCalc() { nAlg = @"23", IsGroup = false }
                    /*-------------24 - dqт бр(ном) -------------*/
                    , new NodeCalc() { nAlg = @"24", IsGroup = true }
                    /*-------------25 - W т/тф(ном)-------------*/
                    , new NodeCalc() { nAlg = @"25", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------49 - D пе -------------*/
                    , new NodeCalc() { nAlg = @"49", IsGroup = true }
                    /*-------------50 - D пе -------------*/
                    , new NodeCalc() { nAlg = @"50", IsGroup = true }
                    /*-------------51 - t пе -------------*/
                    , new NodeCalc() { nAlg = @"51", IsGroup = false }
                    /*-------------51.1 - t оп -------------*/
                    , new NodeCalc() { nAlg = @"51.1", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------54 - D хпп-------------*/
                    , new NodeCalc() { nAlg = @"54", IsGroup = false }
                    /*-------------55 - D гпп-------------*/
                    , new NodeCalc() { nAlg = @"55", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------52 - t гпп-------------*/
                    , new NodeCalc() { nAlg = @"52", IsGroup = false }
                    /*-------------52.1 - t гпп-------------*/
                    , new NodeCalc() { nAlg = @"52.1", IsGroup = false }
                    /*-------------53 - P гпп-------------*/
                    , new NodeCalc() { nAlg = @"53", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------56 - P пе-------------*/
                    , new NodeCalc() { nAlg = @"56", IsGroup = false }
                    /*-------------57 - i пе-------------*/
                    , new NodeCalc() { nAlg = @"57", IsGroup = false }
                    /*-------------57.1 - i оп-------------*/
                    , new NodeCalc() { nAlg = @"57.1", IsGroup = false }
                    /*-------------58 i пв-------------*/
                    , new NodeCalc() { nAlg = @"58", IsGroup = false }
                    /*-------------59 i гпп к-------------*/
                    , new NodeCalc() { nAlg = @"59", IsGroup = false }
                    /*-------------59.1 i гпп т-------------*/
                    , new NodeCalc() { nAlg = @"59.1", IsGroup = false }
                    /*-------------60 i хпп-------------*/
                    , new NodeCalc() { nAlg = @"60", IsGroup = false }
                    /*-------------61 i пр-------------*/
                    , new NodeCalc() { nAlg = @"61", IsGroup = false }
                    /*-------------62 i т-------------*/
                    , new NodeCalc() { nAlg = @"62", IsGroup = false }
                    /*-------------63 i 2-------------*/
                    , new NodeCalc() { nAlg = @"63", IsGroup = false }
                    /*-------------64 Q к бр-------------*/
                    , new NodeCalc() { nAlg = @"64", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------59 - доля газа только для бл.1,2 в опер.расч.-------------*/
                    , new NodeCalc() { nAlg = @"59", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------65 Q к бр-------------*/
                    , new NodeCalc() { nAlg = @"65", IsGroup = false }

                    //Изменение прямого порядка вычисления
                    /*-------------26 Э тф п-------------*/
                    , new NodeCalc() { nAlg = @"26", IsGroup = false }
                    /*-------------27 W п/тф-------------*/
                    , new NodeCalc() { nAlg = @"27", IsGroup = false }
                    /*-------------28 N кн (ном)-------------*/
                    , new NodeCalc() { nAlg = @"28", IsGroup = true }
                    /*-------------29 N цн (н) гр-------------*/
                    , new NodeCalc() { nAlg = @"29", IsGroup = true }
                    /*-------------30 N кэн (н)-------------*/
                    , new NodeCalc() { nAlg = @"30", IsGroup = false }
                    /*-------------31 N бл(н)т-------------*/
                    , new NodeCalc() { nAlg = @"31", IsGroup = false }
                    /*-------------32 N ст(н)т гр-------------*/
                    , new NodeCalc() { nAlg = @"32", IsGroup = true }
                    /*-------------33 -------------*/
                    /*-------------34 -------------*/
                    /*-------------35 Э т сн(пуск)-------------*/
                    , new NodeCalc() { nAlg = @"35", IsGroup = true }
                    /*-------------36 Э т сн/(ном)-------------*/
                    , new NodeCalc() { nAlg = @"36", IsGroup = true }
                    /*-------------37 Э цн (ном) гр-------------*/
                    , new NodeCalc() { nAlg = @"37", IsGroup = true }
                    /*-------------38 Q т.о(отопл)-------------*/
                    , new NodeCalc() { nAlg = @"38", IsGroup = true }
                    /*-------------39 Q т.о(вент)-------------*/
                    , new NodeCalc() { nAlg = @"39", IsGroup = true }
                    /*-------------40 Q т сн(пуск)-------------*/
                    , new NodeCalc() { nAlg = @"40", IsGroup = true }
                    /*-------------41 q т сн(ном)-------------*/
                    , new NodeCalc() { nAlg = @"41", IsGroup = true }
                    /*-------------42 q т н (ном)-------------*/
                    , new NodeCalc() { nAlg = @"42", IsGroup = true }
                    /*-------------43 k по-------------*/
                    , new NodeCalc() { nAlg = @"43", IsGroup = true }
                    /*-------------44 k то-------------*/
                    , new NodeCalc() { nAlg = @"44", IsGroup = true }
                    /*-------------45 dQ э по-------------*/
                    , new NodeCalc() { nAlg = @"45", IsGroup = true }
                    /*-------------46 dQ э то-------------*/
                    , new NodeCalc() { nAlg = @"46", IsGroup = true }
                    /*-------------47 dQ э-------------*/
                    , new NodeCalc() { nAlg = @"47", IsGroup = true }
                    /*-------------48 k отр (т)-------------*/
                    , new NodeCalc() { nAlg = @"48", IsGroup = true }

                    //Изменение прямого порядка вычисления
                    /*-------------66 D пв-------------*/
                    , new NodeCalc() { nAlg = @"66", IsGroup = true }
                    /*-------------66.1 D пв ср-------------*/
                    , new NodeCalc() { nAlg = @"66.1", IsGroup = true }
                    /*-------------67 alfa вэк (н)-------------*/
                    , new NodeCalc() { nAlg = @"67", IsGroup = false }
                    /*-------------68 dalfa ух (н)-------------*/
                    , new NodeCalc() { nAlg = @"68", IsGroup = false }
                    /*-------------69 alfa yx (н)-------------*/
                    , new NodeCalc() { nAlg = @"69", IsGroup = false }
                    /*-------------70 q 4 исх-------------*/
                    , new NodeCalc() { nAlg = @"70", IsGroup = false }
                    /*-------------71 dq 4 (Ap)-------------*/
                    , new NodeCalc() { nAlg = @"71", IsGroup = false }
                    /*-------------72 dq 4 (Wp)-------------*/
                    , new NodeCalc() { nAlg = @"72", IsGroup = false }
                    /*-------------73 q 4 (н)-------------*/
                    , new NodeCalc() { nAlg = @"73", IsGroup = true }
                    /*-------------74 t пв (н)-------------*/
                    , new NodeCalc() { nAlg = @"74", IsGroup = true }
                    /*-------------75 t yx исх-------------*/
                    , new NodeCalc() { nAlg = @"75", IsGroup = false }
                    /*-------------76 dt yx (t пв)-------------*/
                    , new NodeCalc() { nAlg = @"76", IsGroup = false }
                    /*-------------77 dt yx (t вп)-------------*/
                    , new NodeCalc() { nAlg = @"77", IsGroup = false }
                    /*-------------78 dt yx (t рец)-------------*/
                    , new NodeCalc() { nAlg = @"78", IsGroup = false }
                    /*-------------79 t yx (н)-------------*/
                    , new NodeCalc() { nAlg = @"79", IsGroup = true }
                    /*-------------80 k-------------*/
                    , new NodeCalc() { nAlg = @"80", IsGroup = false }
                    /*-------------81 c-------------*/
                    , new NodeCalc() { nAlg = @"81", IsGroup = false }
                    /*-------------82 b-------------*/
                    , new NodeCalc() { nAlg = @"82", IsGroup = false }
                    /*-------------83 q 2 (н)-------------*/
                    , new NodeCalc() { nAlg = @"83", IsGroup = true }
                    /*-------------84 q 5 (н)-------------*/
                    , new NodeCalc() { nAlg = @"84", IsGroup = false }
                    /*-------------85 q 6 (н)-------------*/
                    , new NodeCalc() { nAlg = @"85", IsGroup = false }
                    /*-------------86 q pec (н)-------------*/
                    , new NodeCalc() { nAlg = @"86", IsGroup = false }
                    /*-------------87 q пуск (н)-------------*/
                    , new NodeCalc() { nAlg = @"87", IsGroup = false }
                    /*-------------88 КПД к бр (ном)-------------*/
                    , new NodeCalc() { nAlg = @"88", IsGroup = true }
                    /*-------------89 alfa уг-------------*/
                    , new NodeCalc() { nAlg = @"89", IsGroup = false }
                    /*-------------90 В нат (ном)-------------*/
                    , new NodeCalc() { nAlg = @"90", IsGroup = true }
                    /*-------------91 Э тд(н) исх-------------*/
                    , new NodeCalc() { nAlg = @"91", IsGroup = false }
                    /*-------------92 dЭ тд(Wp)-------------*/
                    , new NodeCalc() { nAlg = @"92", IsGroup = false }
                    /*-------------93 dЭ тд (t вп)-------------*/
                    , new NodeCalc() { nAlg = @"93", IsGroup = false }
                    /*-------------94 1/0???-------------*/
                    /*-------------95 Э тд (ном)-------------*/
                    , new NodeCalc() { nAlg = @"95", IsGroup = true }
                    /*-------------96 Э пп (исх)-------------*/
                    , new NodeCalc() { nAlg = @"96", IsGroup = false }
                    /*-------------97-------------*/
                    , new NodeCalc() { nAlg = @"", IsGroup = false }
                    /*-------------98-------------*/
                    /*-------------99-------------*/
                    /*-------------100-------------*/
                    /*-------------101-------------*/
                    /*-------------102-------------*/
                    /*-------------103-------------*/
                    /*-------------104-------------*/
                    /*-------------105-------------*/
                    /*-------------106-------------*/
                    /*-------------107-------------*/
                    /*-------------108-------------*/
                    /*-------------109-------------*/
                    /*-------------110-------------*/
                    /*-------------111-------------*/
                    /*-------------112-------------*/
                    /*-------------113-------------*/
                    /*-------------114-------------*/
                    /*-------------115-------------*/
                    /*-------------116-------------*/
                    /*-------------117-------------*/
                    /*-------------118-------------*/
                    /*-------------119-------------*/
                    /*-------------120-------------*/
                    /*-------------121-------------*/
                    /*-------------122-------------*/
                    /*-------------123-------------*/
                    /*-------------124-------------*/
                    /*-------------125-------------*/
                    /*-------------126-------------*/
                    /*-------------127-------------*/
                    /*-------------128-------------*/
                    /*-------------129-------------*/
                    /*-------------130-------------*/
                    /*-------------131-------------*/
                    /*-------------132-------------*/
                    /*-------------133-------------*/
                    /*-------------134-------------*/
                    /*-------------135-------------*/
                    /*-------------136-------------*/
                    /*-------------137-------------*/
                    /*-------------138-------------*/
                    /*-------------139-------------*/
                    /*-------------140-------------*/
                    /*-------------141-------------*/
                    /*-------------142-------------*/
                    /*-------------143-------------*/
                    /*-------------144-------------*/
                    /*-------------145-------------*/
                    /*-------------146-------------*/
                    /*-------------147-------------*/
                    /*-------------148-------------*/
                    /*-------------149-------------*/
                    /*-------------150-------------*/
                    /*-------------151-------------*/
                    /*-------------152-------------*/
                    /*-------------153-------------*/
                    /*-------------154-------------*/
                    /*-------------155-------------*/
                };

                for (int i = 0; i < arCalculate.Length; i++)
                    if (arCalculate[i].IsGroup == true)
                        Norm[arCalculate[i].nAlg][ST].value = calculateNormative(arCalculate[i].nAlg);
                    else
                        calculateNormative(arCalculate[i].nAlg);                

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

                NodeCalc[] arCalculate = new NodeCalc[] {
                };

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
                                //VALUE
                                , ((double.IsNaN (val.value) == false) && (double.IsInfinity (val.value) == false)) ? val.value : -1F
                            });

                return tableRes;
            }
        }
    }
}
