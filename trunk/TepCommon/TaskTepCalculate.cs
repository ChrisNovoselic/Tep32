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
            /// Класс для хранения всех значений, необъодимых для расчета
            /// </summary>
            protected class P_ALG : Dictionary<string, Dictionary<int, P_ALG.P_PUT>>
            {
                /// <summary>
                /// Идентификатор - строка - номер алгоритма расчета
                /// </summary>
                public string m_strId;
                /// <summary>
                /// Идентификатор - целочисленное значение, уникальное в границах БД
                /// </summary>
                public int m_iId;
                /// <summary>
                /// Признак запрета на расчет/обновление/использование значения
                /// </summary>
                public bool m_bDeny;
                /// <summary>
                /// Класс для хранения значений для одного из компонентов станции
                ///  в рамках параметра в алгоритме рачета
                /// </summary>
                public class P_PUT
                {
                    /// <summary>
                    /// Идентификатор - целочисленное значение, уникальное в границах БД
                    /// </summary>
                    public int m_iId;
                    /// <summary>
                    /// Идентификатор компонента ТЭЦ (ключ), уникальное в границах БД
                    /// </summary>
                    public int m_iIdComponent;
                    /// <summary>
                    /// Признак запрета на расчет/обновление/использование значения
                    /// </summary>
                    public bool m_bDeny;
                    /// <summary>
                    /// Значение параметра в алгоритме расчета для компонента станции
                    /// </summary>
                    public float m_fValue;
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
            /// <summary>
            /// Константы - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            const int BL1 = 1029
                    , BL2 = 1030
                    , BL3 = 1031
                    , BL4 = 1032
                    , BL5 = 1033
                    , BL6 = 1034
                    , ST = 5;
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
                P_ALG.P_PUT itemPut = null;
                int idPut = -1;
                string strNAlg = string.Empty;
                
                foreach (DataRow r in tablePar.Rows)
                {
                    idPut = (int)r[@"ID"];
                    strNAlg = ((string)r[@"N_ALG"]).Trim ();
                    rVal = tableVal.Select (@"ID_PUT=" + idPut);

                    if (rVal.Length == 1)
                    {
                        if (In.ContainsKey(strNAlg) == false)
                            In.Add(strNAlg, new Dictionary<int, P_ALG.P_PUT>());
                        else
                            ;

                        itemPut = new P_ALG.P_PUT()
                            {
                                m_iId = idPut
                                , m_iIdComponent = (int)r[@"ID_COMP"]
                                , m_bDeny = false
                                , m_fValue = (float)(double)rVal[0][@"VALUE"]
                            };
                        In[strNAlg].Add(idPut, itemPut);
                    }
                    else
                    {
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

                initValues(listDataTables);                

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
