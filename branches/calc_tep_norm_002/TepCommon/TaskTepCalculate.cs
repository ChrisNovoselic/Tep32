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
        public abstract class TaskCalculate : Object
        {
            /// <summary>
            /// Перечисление - индексы типов вкладок (объектов наследуемых классов)
            /// </summary>
            public enum TYPE { UNKNOWN = -1, IN_VALUES, OUT_TEP_NORM_VALUES, OUT_VALUES, OUT_TEP_REALTIME, COUNT }
            private TYPE _type;
            /// <summary>
            /// Индекс типа вкладки для текущего объекта
            /// </summary>
            //public TYPE Type { get { return _type; } set { _type = value; } }

            protected virtual bool isRealTime { get { return _type == TYPE.OUT_TEP_REALTIME; } }
            /// <summary>
            /// Перечисление - индексы таблиц, передаваемых объекту в качестве элементов массива-аргумента
            /// </summary>
            public enum INDEX_DATATABLE : short
            {
                UNKNOWN = -1
                , FTABLE
                , IN_PARAMETER, IN_VALUES
                , OUT_NORM_PARAMETER, OUT_NORM_VALUES
                , OUT_PARAMETER, OUT_VALUES
                    , COUNT
            }
            /// <summary>
            /// Класс для хранения всех значений, необходимых для расчета
            /// </summary>
            protected class P_ALG : Dictionary<string, P_ALG.P_PUT>
            {
                /// <summary>
                /// Класс для хранения всех значений для одного из параметров, необходимых для расчета
                /// </summary>
                public class P_PUT : Dictionary<int, P_PUT.P_VAL>
                {
                    ///// <summary>
                    ///// Идентификатор - строка - номер алгоритма расчета
                    ///// </summary>
                    //public string m_strId;
                    /// <summary>
                    /// Идентификатор - целочисленное значение, уникальное в границах БД
                    /// </summary>
                    public int m_iId;
                    /// <summary>
                    /// Признак запрета на расчет/обновление/использование значения
                    /// </summary>
                    public bool m_bDeny;
                    /// <summary>
                    /// Признак усреднения величины
                    /// </summary>
                    public short m_sAVG;
                    /// <summary>
                    /// Класс для хранения значений для одного из компонентов станции
                    ///  в рамках параметра в алгоритме рачета
                    /// </summary>
                    public class P_VAL
                    {
                        /// <summary>
                        /// Идентификатор - целочисленное значение, уникальное в границах БД
                        /// </summary>
                        public int m_iId;
                        ///// <summary>
                        ///// Идентификатор компонента ТЭЦ (ключ), уникальное в границах БД
                        ///// </summary>
                        //public int m_iIdComponent;
                        /// <summary>
                        /// Признак запрета на расчет/обновление/использование значения
                        /// </summary>
                        public bool m_bDeny;
                        private float _value;
                        /// <summary>
                        /// Значение параметра в алгоритме расчета для компонента станции
                        ///  , при оформлении исключение из правила (для минимизации кодирования)
                        /// </summary>
                        public float value
                        {
                            get { return _value; }

                            set
                            {
                                if (m_bDeny == false)
                                {
                                    if (float.IsInfinity(value) == false)
                                    {
                                        _value = value;
                                        m_sQuality = ID_QUALITY_VALUE.CALCULATED;
                                    }
                                    else
                                        ;
                                }
                                else
                                    ;
                            }
                        }
                        /// <summary>
                        /// Признак качества значения параметра
                        /// </summary>
                        public ID_QUALITY_VALUE m_sQuality;
                        /// <summary>
                        /// Идентификатор 
                        /// </summary>
                        public int m_idRatio;
                        /// <summary>
                        /// Минимальное, максимальное значение
                        /// </summary>
                        public float m_fMinValue;
                        public float m_fMaxValue;
                    }
                }
            }
            /// <summary>
            /// Структура - элемент массива при передаче аргумента в функции расчета
            /// </summary>
            public struct DATATABLE
            {
                /// <summary>
                /// Индекс - указание на предназначение таблицы
                /// </summary>
                public INDEX_DATATABLE m_indx;
                /// <summary>
                /// Таблица со значениями для выполнения расчета
                /// </summary>
                public DataTable m_table;                
            }
            /// <summary>
            /// Класс для представления аргументов при инициализации расчета
            /// </summary>
            public class ListDATATABLE : List <DATATABLE>
            {
                public DataTable FindDataTable(INDEX_DATATABLE indxDataTable)
                {
                    DataTable tableRes = null;

                    foreach (DATATABLE dataTable in this)
                        if (dataTable.m_indx == indxDataTable)
                        {
                            tableRes = dataTable.m_table;

                            break;
                        }
                        else
                            ;

                    return tableRes;
                }
            }
            /// <summary>
            /// Словарь с ВХОДными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            protected P_ALG In;
            /// <summary>
            /// Словарь с расчетными ВЫХОДными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            protected P_ALG Out;
            /// <summary>
            /// Конструктор основной (с параметром)
            /// </summary>
            /// <param name="type">Тип расчета</param>
            public TaskCalculate(/*TYPE type*/)
            {
                //Type = type;
            }
            /// <summary>
            /// Возвратить индкус таблицы БД по указанным типам расчета и рассчитываемых значений
            /// </summary>
            /// <param name="type">Тип расчета</param>
            /// <param name="req">Тип рассчитываемых значений</param>
            /// <returns>Индекс таблицы БД в списке</returns>
            public static INDEX_DBTABLE_NAME GetIndexNameDbTable (TYPE type, TABLE_CALCULATE_REQUIRED req)
            {
                INDEX_DBTABLE_NAME indxRes = INDEX_DBTABLE_NAME.UNKNOWN;
                
                switch (type)
                {
                    case TaskCalculate.TYPE.IN_VALUES:
                        switch (req)
                        {
                            case TABLE_CALCULATE_REQUIRED.ALG:
                                indxRes = INDEX_DBTABLE_NAME.INALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                indxRes = INDEX_DBTABLE_NAME.INPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                indxRes = INDEX_DBTABLE_NAME.INVALUES;
                                break;
                            default:
                                break;
                        }
                        break;
                    case TaskCalculate.TYPE.OUT_TEP_NORM_VALUES:
                    case TaskCalculate.TYPE.OUT_VALUES:
                        switch (req)
                        {
                            case TABLE_CALCULATE_REQUIRED.ALG:
                                indxRes = INDEX_DBTABLE_NAME.OUTALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                indxRes = INDEX_DBTABLE_NAME.OUTPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                indxRes = INDEX_DBTABLE_NAME.OUTVALUES;
                                break;
                            default:
                                break;
                        }
                        break;
                    case TaskCalculate.TYPE.OUT_TEP_REALTIME:
                        switch (req)
                        {
                            case TABLE_CALCULATE_REQUIRED.ALG:
                                indxRes = INDEX_DBTABLE_NAME.INALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                indxRes = INDEX_DBTABLE_NAME.INPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                indxRes = INDEX_DBTABLE_NAME.INVALUES;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                return indxRes;
            }
            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected abstract int initValues(ListDATATABLE listDataTables);
            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="pAlg">Объект - словарь структур для расчета</param>
            /// <param name="tablePar">Таблица с параметрами</param>
            /// <param name="tableVal">Таблица со значениями</param>
            protected int initValues(P_ALG pAlg, DataTable tablePar, DataTable tableVal)
            {
                int iRes = 0; //Предположение, что ошибки нет
                
                DataRow[] rVal = null;
                int idPut = -1
                    , idComponent = -1;
                string strNAlg = string.Empty;

                pAlg.Clear();

                // цикл по всем параметрам расчета
                foreach (DataRow rPar in tablePar.Rows)
                {
                    // найти соответствие параметра в алгоритме расчета и значения для него
                    idPut = (int)rPar[@"ID"];
                    // идентификатор параметра в алгоритме расчета - ключ для словаря с его характеристиками
                    strNAlg = ((string)rPar[@"N_ALG"]).Trim();
                    rVal = tableVal.Select(@"ID_PUT=" + idPut);
                    // проверить успешность нахождения соответствия
                    if (rVal.Length == 1)
                    {
                        if (pAlg.ContainsKey(strNAlg) == false)
                        {// добавить параметр в алгоритме расчета
                            pAlg.Add(strNAlg, new P_ALG.P_PUT());

                            pAlg[strNAlg].m_sAVG = (Int16)rPar[@"AVG"];
                            pAlg[strNAlg].m_bDeny = false;
                        }
                        else
                            ;
                        // идентификатор компонента станции - ключ для словаря со значением и характеристиками для него
                        idComponent = (int)rPar[@"ID_COMP"];

                        if (pAlg[strNAlg].ContainsKey(idComponent) == false)
                            pAlg[strNAlg].Add(idComponent, new P_ALG.P_PUT.P_VAL()
                            // добавить параметр компонента в алгоритме расчета
                            {
                                m_iId = idPut
                                //, m_iIdComponent = idComponent
                                , m_bDeny = false
                                , value = (float)(double)rVal[0][@"VALUE"]
                                , m_sQuality = ID_QUALITY_VALUE.DEFAULT // не рассчитывался
                                , m_idRatio = (int)rPar[@"ID_RATIO"]
                                , m_fMinValue = (rPar[@"MINVALUE"] is DBNull) ? 0 : (float)rPar[@"MINVALUE"] //??? - ошибка д.б. float
                                , m_fMaxValue = (rPar[@"MAXVALUE"] is DBNull) ? 0 : (float)rPar[@"MAXVALUE"] //??? - ошибка д.б. float
                            });
                        else
                            ;
                    }
                    else
                    {// ошибка - не найдено соответствие параметр-значение
                        iRes = -1;

                        Logging.Logg().Error(@"TaskCalculate::initValues (ID_PUT=" + idPut + @") - не найдено соответствие параметра и значения...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }

                return iRes;
            }
        }        
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public partial class TaskTepCalculate : TaskCalculate
        {
            /// <summary>
            /// Признак расчета ТЭП-оперативно
            /// </summary>
            protected override bool isRealTime { get { return ! (m_indxCompRealTime == INDX_COMP.UNKNOWN); } }

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
            private enum INDX_COMP : short { UNKNOWN = -1
                , iBL1, iBL2, iBL3, iBL4, iBL5, iBL6, iST
                , COUNT};
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
            private readonly int [] ID_COMP =
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
            private enum MODE_DEV : short { UNKNOWN = -1, COND_1 = 1, ELEKTRO2_2, ELEKTRO1_2a, TEPLO_3
                , COUNT }
            /// <summary>
            /// Словарь - режимы работы для компонентов станции
            /// </summary>
            private Dictionary <int, MODE_DEV> _modeDev;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskTepCalculate() : base ()
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
                /*-------------56.1 - Pо-------------*/
                calculateNormative(@"57");
                /*-------------57 - i пе-------------*/
                /*-------------58 -------------*/
                /*-------------59 -------------*/

                /*-------------26 -------------*/
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
