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
    public partial class HandlerDbTaskCalculate
    {
        public abstract class TaskCalculate : Object
        {
            /// <summary>
            /// Перечисление - индексы (идентифкаторы) загружаемых/отображаемых значений
            /// </summary>
            [Flags]
            public enum TYPE { UNKNOWN = 0x0
                , IN_VALUES = 1
                , OUT_TEP_NORM_VALUES = 2
                , OUT_VALUES = 4
                , OUT_TEP_REALTIME = 8
                    , }
            private TYPE _type;
            /// <summary>
            /// Индекс типа вкладки для текущего объекта
            /// </summary>
            //public TYPE Type { get { return _type; } set { _type = value; } }
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
            public class ListDATATABLE : List<DATATABLE>
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
            public static ID_DBTABLE GetIdDbTable(TYPE type, TABLE_CALCULATE_REQUIRED req)
            {
                ID_DBTABLE idRes = ID_DBTABLE.UNKNOWN;

                switch (type)
                {
                    case TaskCalculate.TYPE.IN_VALUES:
                        switch (req)
                        {
                            case TABLE_CALCULATE_REQUIRED.ALG:
                                idRes = ID_DBTABLE.INALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                idRes = ID_DBTABLE.INPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                idRes = ID_DBTABLE.INVALUES;
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
                                idRes = ID_DBTABLE.OUTALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                idRes = ID_DBTABLE.OUTPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                idRes = ID_DBTABLE.OUTVALUES;
                                break;
                            default:
                                break;
                        }
                        break;
                    case TaskCalculate.TYPE.OUT_TEP_REALTIME:
                        switch (req)
                        {
                            case TABLE_CALCULATE_REQUIRED.ALG:
                                idRes = ID_DBTABLE.INALG;
                                break;
                            case TABLE_CALCULATE_REQUIRED.PUT:
                                idRes = ID_DBTABLE.INPUT;
                                break;
                            case TABLE_CALCULATE_REQUIRED.VALUE:
                                idRes = ID_DBTABLE.INVALUES;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                return idRes;
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
                                ,
                                m_bDeny = false
                                ,
                                value = (float)(double)rVal[0][@"VALUE"]
                                ,
                                m_sQuality = ID_QUALITY_VALUE.DEFAULT // не рассчитывался
                                ,
                                m_idRatio = (int)rPar[@"ID_RATIO"]
                                ,
                                m_fMinValue = (rPar[@"MINVALUE"] is DBNull) ? 0 : (float)rPar[@"MINVALUE"] //??? - ошибка д.б. float
                                ,
                                m_fMaxValue = (rPar[@"MAXVALUE"] is DBNull) ? 0 : (float)rPar[@"MAXVALUE"] //??? - ошибка д.б. float
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
    }
}
