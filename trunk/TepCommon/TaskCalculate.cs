using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using HClassLibrary;

namespace TepCommon
{
    public partial class HandlerDbTaskCalculate
    {
        public abstract class TaskCalculate : Object, IDisposable
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
                ,
            }

            protected TYPE _types;
            /// <summary>
            /// Признак - единственный ли флаг установлен в наборе флагов _types
            /// </summary>
            /// <param name="flags">Набор типов расчета</param>
            /// <returns>Признак наличия единственногофлага из набора</returns>            
            public static bool GetIsSingleTaskCalculateType(TYPE flags)
            {
                bool bRes = false;

                int counter = 0;

                foreach (TYPE type in Enum.GetValues(typeof(TYPE)))
                    if (!(type == TYPE.UNKNOWN))
                        counter += ((type & flags) == type) ? 1 : 0;
                    else
                        ;

                return bRes = (counter == 1);
            }

            public bool IsSingleTaskCalculateType
            {
                get {
                    return GetIsSingleTaskCalculateType(_types);
                }
            }
            /// <summary>
            /// Класс для хранения всех значений, необходимых для расчета
            /// </summary>
            protected class P_ALG : Dictionary<string, P_ALG.P_PUT>
            {
                public struct KEY_P_VALUE
                {
                    public int Id;

                    public DateTime Stamp;
                }

                /// <summary>
                /// Класс для хранения всех значений для одного из параметров, необходимых для расчета
                /// </summary>
                public class P_PUT : Dictionary<KEY_P_VALUE, P_PUT.P_VALUE>
                {
                    private P_PUT()
                        : base ()
                    {
                    }

                    public P_PUT(int id, bool deny, AGREGATE_ACTION avg)
                        : this()
                    {
                        m_iId = id;

                        m_bDeny = deny;

                        m_avg = avg;
                    }
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
                    public AGREGATE_ACTION m_avg;
                    /// <summary>
                    /// Класс для хранения значений для одного из компонентов станции
                    ///  в рамках параметра в алгоритме рачета
                    /// </summary>
                    public class P_VALUE
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
                        /// Значение параметра в алгоритме расчета
                        /// </summary>
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
                        /// Минимальное значение
                        /// </summary>
                        public float m_fMinValue;
                        /// <summary>
                        /// Максимальное значение
                        /// </summary>
                        public float m_fMaxValue;
                    }
                }
            }
            /// <summary>
            /// Словарь с ВХОДными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            protected P_ALG In;
            /// <summary>
            /// Словарь с расчетными НОРМативными параметрами - ключ - идентификатор в алгоритме расчета
            ///  (инициализируется в ~ от переданного параметра - требуемые типы расчета)
            /// </summary>
            protected P_ALG Norm;
            /// <summary>
            /// Словарь с расчетными ВЫХОДными параметрами - ключ - идентификатор в алгоритме расчета
            /// </summary>
            protected P_ALG Out;

            protected Dictionary<TaskCalculate.TYPE, P_ALG> _dictPAlg;
            /// <summary>
            /// Конструктор основной (с параметром)
            /// </summary>
            /// <param name="type">Тип расчета</param>
            public TaskCalculate(TYPE types
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
            //public TaskCalculate(TYPE types, ListDATATABLE listDataTables)
            {
                _types = types;

                In = new P_ALG();
                Out = new P_ALG();

                _dictPAlg = new Dictionary<TYPE, P_ALG>() {
                     { TYPE.IN_VALUES, In }
                    , { TYPE.OUT_VALUES, Out }
                };

                if ((types & TYPE.OUT_TEP_NORM_VALUES) == TYPE.OUT_TEP_NORM_VALUES) {
                    Norm = new P_ALG();
                    _dictPAlg.Add(TYPE.OUT_TEP_NORM_VALUES, Norm);
                } else
                    ;

                if (initValues(listNAlg, listPutPar, dictValues) < 0)
                    Logging.Logg().Error(string.Format(@"TaskCalculate::ctor () - вызов 'initValues ()' ...")
                        , Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;
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

            protected abstract int initValues(IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues);

            protected int initValues(P_ALG pAlg
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , IEnumerable<VALUE> values)
            {
                int iRes = 0;

                NALG_PARAMETER nAlg;
                IEnumerable<VALUE> putValues;
                //VALUE value;
                P_ALG.KEY_P_VALUE keyPValue;

                foreach (PUT_PARAMETER putPar in listPutPar) {
                    if (putPar.IsNaN == false) {
                        nAlg = listNAlg.FirstOrDefault(item => { return item.m_Id == putPar.m_idNAlg; });
                        putValues = values.Where(item => { return item.m_IdPut == putPar.m_Id; });

                        if (!(nAlg == null))
                            if (!(nAlg.m_Id < 0))
                                foreach (VALUE value in putValues)
                                    if ((value.m_IdPut > 0)
                                        /*&& (((value.stamp_value.Equals(DateTime.MinValue) == false)))*/) {
                                        if (pAlg.ContainsKey(nAlg.m_nAlg) == false)
                                            pAlg.Add(nAlg.m_nAlg, new P_ALG.P_PUT(nAlg.m_Id, !nAlg.m_bEnabled, nAlg.m_sAverage));
                                        else
                                            ;

                                        keyPValue = new P_ALG.KEY_P_VALUE() { Id = putPar.IdComponent, Stamp = value.stamp_value };

                                        if (pAlg[nAlg.m_nAlg].ContainsKey(keyPValue) == false)
                                            pAlg[nAlg.m_nAlg].Add(keyPValue
                                                , new P_ALG.P_PUT.P_VALUE() {
                                                    m_iId = putPar.m_Id
                                                    , m_bDeny = !putPar.IsEnabled
                                                    , m_idRatio = putPar.m_prjRatio
                                                    , m_sQuality = (value.stamp_value.Equals(DateTime.MinValue) == false) ? value.m_iQuality : ID_QUALITY_VALUE.NOT_REC
                                                    , value = (value.stamp_value.Equals(DateTime.MinValue) == false) ? value.value : 0F
                                                    , m_fMinValue = putPar.m_fltMinValue
                                                    , m_fMaxValue = putPar.m_fltMaxValue
                                                });
                                        else
                                        // для параметра 1-го порядка уже содержится значение параметра 2-го порядка
                                            ;
                                    } else
                                    // некорректный параметр расчета 2-го порядка
                                        ;
                            else
                            // не найден либо параметр 1-го порядка, либо значение для параметра 2-го порядка
                                ;
                        else
                        // не найден параметр 1-го порядка (не ошибка - возможно putPar для другого типа расчета)
                            ;
                    } else
                    // параметр 2-го порядка не достоверен
                        ;
                }

                return iRes;
            }
            ///// <summary>
            ///// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            ///// </summary>
            ///// <param name="pAlg">Объект - словарь структур для расчета</param>
            ///// <param name="tablePar">Таблица с параметрами</param>
            ///// <param name="tableVal">Таблица со значениями</param>
            //protected int initValues(P_ALG pAlg, DataTable tablePar, DataTable tableVal)
            //{
            //    int iRes = 0; //Предположение, что ошибки нет

            //    DataRow[] rVal = null;
            //    int idPut = -1
            //        , idComponent = -1;
            //    string strNAlg = string.Empty;

            //    pAlg.Clear();

            //    // цикл по всем параметрам расчета
            //    foreach (DataRow rPar in tablePar.Rows)
            //    {
            //        // найти соответствие параметра в алгоритме расчета и значения для него
            //        idPut = (int)rPar[@"ID"];
            //        // идентификатор параметра в алгоритме расчета - ключ для словаря с его характеристиками
            //        strNAlg = ((string)rPar[@"N_ALG"]).Trim();
            //        rVal = tableVal.Select(@"ID_PUT=" + idPut);
            //        // проверить успешность нахождения соответствия
            //        if (rVal.Length == 1)
            //        {
            //            if (pAlg.ContainsKey(strNAlg) == false)
            //            {// добавить параметр в алгоритме расчета
            //                pAlg.Add(strNAlg, new P_ALG.P_PUT());

            //                pAlg[strNAlg].m_avg = (AGREGATE_ACTION)(Int16)rPar[@"AVG"];
            //                pAlg[strNAlg].m_bDeny = false;
            //            }
            //            else
            //                ;
            //            // идентификатор компонента станции - ключ для словаря со значением и характеристиками для него
            //            idComponent = (int)rPar[@"ID_COMP"];

            //            if (pAlg[strNAlg].ContainsKey(idComponent) == false)
            //                pAlg[strNAlg].Add(idComponent, new P_ALG.P_PUT.P_VAL()
            //            // добавить параметр компонента в алгоритме расчета
            //                {
            //                    m_iId = idPut
            //                    //, m_iIdComponent = idComponent
            //                    , m_bDeny = false
            //                    , value = (float)(double)rVal[0][@"VALUE"]
            //                    , m_sQuality = ID_QUALITY_VALUE.DEFAULT // не рассчитывался
            //                    , m_idRatio = (int)rPar[@"ID_RATIO"]
            //                    , m_fMinValue = (rPar[@"MINVALUE"] is DBNull) ? 0 : (float)rPar[@"MINVALUE"] //??? - ошибка д.б. float
            //                    , m_fMaxValue = (rPar[@"MAXVALUE"] is DBNull) ? 0 : (float)rPar[@"MAXVALUE"] //??? - ошибка д.б. float
            //                });
            //            else
            //                ;
            //        } else {// ошибка - не найдено соответствие параметр-значение
            //            iRes = -1;

            //            Logging.Logg().Error(@"TaskCalculate::initValues (ID_PUT=" + idPut + @") - не найдено соответствие параметра и значения...", Logging.INDEX_MESSAGE.NOT_SET);
            //        }
            //    }

            //    return iRes;
            //}

            protected void validateKeyPValue(P_ALG.P_PUT pPut, P_ALG.KEY_P_VALUE keyGroupPValue)
            {
                P_ALG.P_PUT.P_VALUE emptyPValue
                    , templatePValue;
                P_ALG.KEY_P_VALUE keyEmptyPValue;

                keyEmptyPValue.Stamp = DateTime.MinValue;

                // сформировать ключ с датой "по умолчанию"
                keyEmptyPValue.Id = keyGroupPValue.Id;
                // попробовать найти элемент с датой "по умолчанию"
                if (pPut.ContainsKey(keyEmptyPValue) == true) {
                    // сохранить значение перед удалением элемента с датой "по умолчанию"
                    emptyPValue = pPut[keyEmptyPValue];
                    // удалить элемент с датой "по умолчанию"
                    pPut.Remove(keyEmptyPValue);
                    // добавить элемент с новым ключом и старым значением
                    pPut.Add(keyGroupPValue, emptyPValue);
                } else if (pPut.ContainsKey(keyGroupPValue) == false) {
                    templatePValue = pPut.FirstOrDefault(put => { return put.Key.Id == keyGroupPValue.Id; }).Value;

                    if (templatePValue.m_iId > 0)
                        pPut.Add(
                            keyGroupPValue
                            , new P_ALG.P_PUT.P_VALUE() {
                                m_iId = templatePValue.m_iId
                                , m_bDeny = templatePValue.m_bDeny
                                , m_idRatio = templatePValue.m_idRatio
                                , m_sQuality = ID_QUALITY_VALUE.NOT_REC
                                , value = 0F
                                , m_fMinValue = templatePValue.m_fMinValue
                                , m_fMaxValue = templatePValue.m_fMaxValue
                            }
                        );
                    else
                        ;
                } else
                    ;
            }
            /// <summary>
            /// Выполнить расчет
            /// </summary>
            /// <param name="delegateResultListValue">Метод обратного вызова при завершении расчета</param>
            /// <param name="delegateResultNAlg">Метод обратного вызова при завершении расчета одного из параметров алгоритма рачетов</param>
            public abstract void Execute(Action <TYPE, IEnumerable<VALUE>, RESULT> delegateResultListValue, Action<TYPE, int, RESULT> delegateResultNAlg);
            /// <summary>
            /// Преоразовать результаты расчетов в список со значениями для дальнейшей обработки(отображения)
            /// </summary>
            /// <param name="pAlg">Словарь с данными для расчетов или результатов расчетов</param>
            /// <returns>Список со значениями для отображения</returns>
            protected static IEnumerable<VALUE> resultToListValue(P_ALG pAlg)
            {
                List<VALUE> listRes = new List<VALUE>();

                foreach (P_ALG.P_PUT pPut in pAlg.Values)
                    foreach (KeyValuePair<P_ALG.KEY_P_VALUE, P_ALG.P_PUT.P_VALUE> pair in pPut)
                        if (listRes.FindIndex(item => { return (item.m_IdPut == pair.Value.m_iId) && (item.stamp_value == pair.Key.Stamp); }) < 0)
                            listRes.Add(
                                new VALUE() {
                                    m_IdPut = pair.Value.m_iId
                                    , m_iQuality = pair.Value.m_sQuality
                                    , value = pair.Value.value
                                    , stamp_value = pair.Key.Stamp
                                    , stamp_write = DateTime.MinValue
                                }
                            );
                        else
                            Logging.Logg().Error(string.Format(@"TaskCalculate::resultToListValue () - дублирование при преобразовании резудьтатов расчетов...")
                                , Logging.INDEX_MESSAGE.NOT_SET);

                return listRes;
            }
            
            /// <summary>
            /// Преобразование словаря с результатом в таблицу для БД
            /// </summary>
            /// <param name="pAlg">Словарь с результатами расчета</param>
            /// <returns>Таблица для БД с результатами расчета</returns>
            protected static DataTable resultToTable(P_ALG pAlg)
            {
                DataTable tableRes = new DataTable();

                tableRes.Columns.AddRange(new DataColumn[] {
                    new DataColumn (@"ID", typeof(int))
                    , new DataColumn (@"QUALITY", typeof(short))
                    , new DataColumn (@"VALUE", typeof(float))
                });

                foreach (P_ALG.P_PUT pPut in pAlg.Values)
                    foreach (P_ALG.P_PUT.P_VALUE val in pPut.Values)
                        tableRes.Rows.Add(new object[]
                            {
                                val.m_iId //ID_PUT
                                , val.m_sQuality //QUALITY
                                //VALUE
                                , ((double.IsNaN (val.value) == false) && (double.IsInfinity (val.value) == false)) ? val.value : -1F
                            });

                return tableRes;
            }

            #region IDisposable Support
            private bool disposedValue = false; // Для определения избыточных вызовов

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue) {
                    if (disposing) {
                        // TODO: освободить управляемое состояние (управляемые объекты).
                    }

                    // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                    // TODO: задать большим полям значение NULL.

                    disposedValue = true;
                }
            }

            // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
            // ~TaskCalculate() {
            //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            //   Dispose(false);
            // }

            // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
            void IDisposable.Dispose()
            {
                // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
                Dispose(true);
                // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
