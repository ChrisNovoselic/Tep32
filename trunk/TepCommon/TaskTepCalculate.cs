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
            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected abstract void initValues(ListDATATABLE listDataTables);
        }
        /// <summary>
        /// Перечисление - индексы типов вкладок (объектов наследуемых классов)
        /// </summary>
        public enum TYPE { UNKNOWN = -1, IN_VALUES, OUT_TEP_NORM_VALUES, OUT_VALUES, OUT_TEP_REALTIME, COUNT }
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public class TaskTepCalculate : TaskCalculate
        {
            int n_blokov;
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
            public TaskTepCalculate()
            {
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
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица нормативных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateNormative(ListDATATABLE listDataTables)
            {
                DataTable tableRes = new DataTable();

                int i = -1;
                float fSum = -1F;

                initValues(listDataTables);

                /*-------------Пар. 1 TAU раб-------------*/
                fSum = 0F;
                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                {
                    fSum += In[@"1"][i].value;
                    Norm[@"1"][i].value = In[@"1"][i].value;
                }
                Norm[@"1"][ST].value = fSum;

                /*-------------Пар. 2 Э т-------------*/
                fSum = 0F;
                for (i = (int)INDX_COMP.iBL1; i < (int)INDX_COMP.iST; i++)
                {
                    fSum += In[@"2"][i].value;
                    Norm[@"2"][i].value = In[@"2"][i].value;
                }
                Norm[@"2"][ST].value = fSum;

                /*-------------Пар. 3 Q то-------------*/

                /*-------------Пар. 4 Q пп-------------*/

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

                initValues(listDataTables);

                return tableRes;
            }
        }
    }
}
