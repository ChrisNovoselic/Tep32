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
            public TYPE Type { get { return _type; } set { _type = value; } }

            protected virtual bool isRealTime { get { return Type == TYPE.OUT_TEP_REALTIME; } }
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
                        /// <summary>
                        /// Значение параметра в алгоритме расчета для компонента станции
                        ///  , при оформлении исключение из правила (для минимизации кодирования)
                        /// </summary>
                        public float value;
                        /// <summary>
                        /// Признак качества значения параметра
                        /// </summary>
                        public short m_sQuality;
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

                    //public Dictionary<int, P_ALG.P_PUT> values;
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

            public TaskCalculate(TYPE type)
            {
                Type = type;
            }

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
            protected abstract void initValues(ListDATATABLE listDataTables);
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
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskTepCalculate(TYPE type) : base (type)
            {
                m_indxCompRealTime = INDX_COMP.UNKNOWN;

                In = new P_ALG();
                Norm = new P_ALG();
                Out = new P_ALG();

                fTable = new FTable();
            }
            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override void initValues(ListDATATABLE listDataTables)
            {
                fTable.Set(listDataTables.FindDataTable (INDEX_DATATABLE.FTABLE));

                initInValues(listDataTables.FindDataTable(INDEX_DATATABLE.IN_PARAMETER)
                    , listDataTables.FindDataTable(INDEX_DATATABLE.IN_VALUES));
            }

            private void initInValues(DataTable tablePar, DataTable tableVal)
            {
                DataRow[] rVal = null;
                int idPut = -1
                    , idComponent = -1;
                string strNAlg = string.Empty;
                // цикл по всем параметрам расчета
                foreach (DataRow rPar in tablePar.Rows)
                {
                    // найти соответствие параметра в алгоритме расчета и значения для него
                    idPut = (int)rPar[@"ID"];
                    // идентификатор параметра в алгоритме расчета - ключ для словаря с его характеристиками
                    strNAlg = ((string)rPar[@"N_ALG"]).Trim ();
                    rVal = tableVal.Select (@"ID_PUT=" + idPut);
                    // проверить успешность нахождения соответствия
                    if (rVal.Length == 1)
                    {
                        if (In.ContainsKey(strNAlg) == false)
                        {// добавить параметр в алгоритме расчета
                            In.Add(strNAlg, new P_ALG.P_PUT());

                            In[strNAlg].m_sAVG = (Int16)rPar[@"AVG"];
                            In[strNAlg].m_bDeny = false;
                        }
                        else
                            ;
                        // идентификатор компонента станции - ключ для словаря со значением и характеристиками для него
                        idComponent = (int)rPar[@"ID_COMP"];

                        if (In[strNAlg].ContainsKey(idComponent) == false)
                            In[strNAlg].Add(idComponent, new P_ALG.P_PUT.P_VAL()
                        // добавить параметр компонента в алгоритме расчета
                            {
                                m_iId = idPut
                                //, m_iIdComponent = idComponent
                                , m_bDeny = false
                                , value = (float)(double)rVal[0][@"VALUE"]
                                , m_sQuality = 0 // не рассчитывался
                                , m_idRatio = (int)rPar[@"ID_RATIO"]
                                , m_fMinValue = (float)rPar[@"MINVALUE"]
                                , m_fMaxValue = (float)rPar[@"MAXVALUE"]
                            });
                        else
                            ;
                    }
                    else
                    {// ошибка - не найдено соответствие параметр-значение
                        Logging.Logg().Error(@"TaskTepCalculate::initInValues (ID_PUT=" + idPut + @") - не найдено соответствие параметра и значения...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }

            private void initNormValues(DataTable table)
            {
            }

            private void initMktValues(DataTable table)
            {
            }
            /// <summary>
            /// Расчитать выходные-нормативные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица нормативных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateNormative(ListDATATABLE listDataTables)
            {
                DataTable tableRes = new DataTable();

                // инициализация входных значений
                initValues(listDataTables);
                // расчет
                /*-------------1 - TAU раб-------------*/
                Norm[@"1"][ST].value = calculateNormative(@"1");
                /*---------------------------------------*/

                /*-------------2 - Э т-------------*/
                Norm[@"2"][ST].value = calculateNormative(@"2");
                /*---------------------------------------*/

                /*-------------3 - Q то-------------*/
                Norm[@"3"][ST].value = calculateNormative(@"3");
                /*---------------------------------------*/

                /*-------------4 - Q пп-------------*/
                Norm[@"4"][ST].value = calculateNormative(@"4");
                /*---------------------------------------*/

                /*-------------5 - Q отп ст-------------*/
                Norm[@"5"][ST].value = calculateNormative(@"5");
                /*---------------------------------------*/

                /*-------------6 - Q отп роу-------------*/
                Norm[@"6"][ST].value = calculateNormative(@"6");
                /*---------------------------------------*/

                /*-------------7 - Q отп тепл-------------*/
                Norm[@"7"][ST].value = calculateNormative(@"7");
                /*---------------------------------------*/

                /*-------------8 - Q отп-------------*/
                Norm[@"8"][ST].value = calculateNormative(@"8");
                /*---------------------------------------*/

                /*-------------9 - N т-------------*/
                Norm[@"9"][ST].value = calculateNormative(@"9");
                /*---------------------------------------*/

                /*-------------10 - Q т ср-------------*/
                Norm[@"10"][ST].value = calculateNormative(@"10");
                /*---------------------------------------*/

                /*-------------10.1 P вто-------------*/
                calculateNormative(@"10.1");
                /*---------------------------------------*/

                /*-------------11 - Q роу ср-------------*/
                Norm[@"11"][ST].value = calculateNormative(@"11");
                /*---------------------------------------*/

                /*-------------12 - q т бр (исх)-------------*/
                calculateNormative(@"12");
                /*---------------------------------------*/

                /*-------------13 - G о-------------*/
                calculateNormative(@"13");
                /*---------------------------------------*/

                /*-------------14 - G 2-------------*/
                Norm[@"14"][ST].value = calculateNormative(@"14");
                /*---------------------------------------*/

                /*-------------14.1 - G цв-------------*/
                Norm[@"14.1"][ST].value = calculateNormative(@"14.1");
                /*---------------------------------------*/

                /*-------------15 - P 2 (н)-------------*/
                calculateNormative(@"15");
                /*---------------------------------------*/

                /*-------------16 -------------*/
                /*---------------------------------------*/

                /*-------------17 -------------*/
                /*---------------------------------------*/

                /*-------------18 -------------*/
                /*---------------------------------------*/

                /*-------------19 -------------*/
                /*---------------------------------------*/

                /*-------------20 -------------*/
                /*---------------------------------------*/

                /*-------------21 -------------*/
                /*---------------------------------------*/

                /*-------------22 -------------*/
                /*---------------------------------------*/

                /*-------------23 -------------*/
                /*---------------------------------------*/

                /*-------------24 -------------*/
                /*---------------------------------------*/

                /*-------------25 -------------*/
                /*---------------------------------------*/

                /*-------------26 -------------*/
                /*---------------------------------------*/

                /*-------------27 -------------*/
                /*---------------------------------------*/

                /*-------------28 -------------*/
                /*---------------------------------------*/

                /*-------------29 -------------*/
                /*---------------------------------------*/

                /*-------------30 -------------*/
                /*---------------------------------------*/

                // преобразование в таблицу

                return tableRes;
            }            
            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateMaket(ListDATATABLE listDataTables)
            {
                DataTable tableRes = new DataTable();

                // инициализация входных значений
                initValues(listDataTables);

                // расчет
                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Norm)
                    pAlg.Value[ST].value = calculateNormative(pAlg.Key);

                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in Out)
                    pAlg.Value[ST].value = calculateMaket(pAlg.Key);

                // преобразование в таблицу

                return tableRes;
            }
        }
    }
}
