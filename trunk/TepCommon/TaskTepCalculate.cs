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
            protected abstract void initValues(DATATABLE[] arDataTables);
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
            protected override void initValues(DATATABLE []arDataTables)
            {
                foreach (DATATABLE dataTable in arDataTables)
                    switch (dataTable.m_indx)
                    {
                        case INDEX_DATATABLE.FTABLE:
                            fTable.Set (dataTable.m_table);
                            break;
                        case INDEX_DATATABLE.IN_VALUES:
                            initInValues(dataTable.m_table);
                            break;
                        default:
                            break;
                    }
            }

            private void initInValues(DataTable table)
            {
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
            public DataTable CalculateNormative(DATATABLE []arDataTables)
            {
                DataTable tableRes = new DataTable();

                initValues(arDataTables);

                if (Norm[@"1"][BL1].m_bDeny == false)
                    Norm[@"1"][BL1].m_fValue = In[@"1"][BL4].m_fValue + In[@"2"][BL6].m_fValue * fTable.F2(@"2.4", In[@"1"][BL1].m_fValue, Norm[@"1"][BL2].m_fValue);
                else
                    ;

                if (Out[@"1"][BL2].m_bDeny == false)
                    Out[@"1"][BL2].m_fValue = Norm[@"1"][ST].m_fValue + In[@"2"][BL2].m_fValue;
                else
                    ;

                return tableRes;
            }
            /// <summary>
            /// Расчитать выходные значения
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            /// <returns>Таблица выходных значений, совместимая со структурой выходныъ значений в БД</returns>
            public DataTable CalculateMaket(DATATABLE[] arDataTables)
            {
                DataTable tableRes = new DataTable();

                initValues(arDataTables);

                return tableRes;
            }
        }
    }
}
