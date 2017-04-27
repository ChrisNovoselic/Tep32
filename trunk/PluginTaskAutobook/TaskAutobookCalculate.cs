using System;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;
using System.Collections.Generic;

namespace PluginTaskAutobook
{
    /// <summary>
    /// DayAutoBook
    /// </summary>
    public class HandlerDbTaskAutobookMonthValuesCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Класс для расчета ...
        /// </summary>
        /// <summary>
        /// калькулятор значений
        /// </summary>
        private class TaskAutobookMonthValuesCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            public TaskAutobookMonthValuesCalculate(TaskCalculate.TYPE types
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
                : base(types, listNAlg, listPutPar, dictValues)
            {
            }
            /// <summary>
            /// Перечисления индексы для массива идентификаторов компонентов оборудования ТЭЦ
            /// </summary>
            private enum INDX_COMP : short
            {
                UNKNOWN = -1
                    , iBL1, iBL2, iBL3, iBL4, iBL5, iBL6
                    , iGTP12, iGTP36
                    , iST
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
                , GTP12 = 115
                , GTP36 = 116
                    , ST = 5;
            /// <summary>
            /// Массив - идентификаторы компонентов оборудования ТЭЦ
            /// </summary>
            private readonly int[] ID_COMP =
            {
                BL1, BL2, BL3, BL4, BL5, BL6
                , GTP12, GTP36
                    , ST
            };

            public override void Execute(Action<TYPE, IEnumerable<VALUE>, RESULT> delegateResultListValue, Action<TYPE, int, RESULT> delegateResultNAlg)
            {
                RESULT res = RESULT.Ok;

                res = calculate(delegateResultNAlg);
                // преобразование в таблицу, вернуть
                // здесь _types всегда одно значение из набора: TYPE.OUT_VALUES
                delegateResultListValue(_types, resultToListValue(_dictPAlg[_types]), res);                    
            }

            protected override int initValues(IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
            {
                int iRes = -1;

                TYPE type;

                #region инициализация входных параметров/значений
                type = TYPE.IN_VALUES;
                iRes = initValues(In
                    , listNAlg.Where(item => { return (item.m_type & type) == type; })
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = type, TypeState = STATE_VALUE.EDIT }]);
                #endregion

                #region инициализация выходных параметров/значений
                type = TYPE.OUT_VALUES;
                iRes = initValues(Out
                    , listNAlg.Where(item => { return (item.m_type & type) == type; })
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = type, TypeState = STATE_VALUE.EDIT }]);
                #endregion

                return iRes;
            }

            private RESULT calculate(Action<TYPE, int, RESULT> delegateResultNAlg)
            {
                RESULT res = RESULT.Ok;
                RESULT[] resNAlg = new RESULT[_dictPAlg[TYPE.OUT_VALUES].Count];

                P_ALG.KEY_P_VALUE keyGroupPValue;
                IEnumerable<string> nAlgs; // парметры алгоритма участвующие в расчете
                IEnumerable<P_ALG.KEY_P_VALUE> calculateKeys = new List<P_ALG.KEY_P_VALUE>();
                float fltRes = -1F;

                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in _dictPAlg[TYPE.OUT_VALUES]) {
                    switch (pAlg.Key) {
                        case "191":
                            // добавить в список все параметры алгоритма расчета
                            nAlgs = new List<string>() { @"1" };
                            foreach(string nAlg in nAlgs)
                                calculateKeys = calculateKeys.Union(In[nAlg].Keys);

                            var keyPValueGroupDates = calculateKeys.GroupBy(k => k.Stamp).ToDictionary(g => { return g.Key; });

                            foreach (DateTime date in keyPValueGroupDates.Keys) {
                                // зафиксировать дату в ключе группового элемента
                                keyGroupPValue.Stamp = date;

                                // ГТП (1,2; 3-6)
                                foreach (P_ALG.KEY_P_VALUE keyPValue in keyPValueGroupDates[date]) {
                                    switch (keyPValue.Id) {
                                        case BL1:
                                        case BL2:
                                            keyGroupPValue.Id = GTP12; // ГТП 1,2
                                            break;
                                        case BL3:
                                        case BL4:
                                        case BL5:
                                        case BL6:
                                            keyGroupPValue.Id = GTP36; // ГТП 3-6
                                            break;
                                        default:
                                            throw new Exception(string.Format(@"TaskAutobookMonthValuesCalculate::calculate () - неизвестный идентификатор [ID_COMP={0}] компонента...", keyPValue.Id));
                                            break;
                                    }

                                    validateKeyPValue(Out[pAlg.Key], keyGroupPValue);

                                    if (Out[pAlg.Key].ContainsKey(keyGroupPValue) == true)
                                        Out[pAlg.Key][keyGroupPValue].value += In["1"][keyPValue].value;
                                    else
                                        throw new Exception(string.Format(@"TaskAutobookMonthValuesCalculate::calculate () - отсутствует ключ [ID={0}, DATA_DATE={1}]..."
                                            , keyGroupPValue.Id, keyGroupPValue.Stamp));
                                }
                                // станция
                                fltRes = 0F;
                                IEnumerable<P_ALG.KEY_P_VALUE> groupPValueKeys = Out[pAlg.Key].Keys.Where(key => { return !(TECComponent.GetType(key.Id) == TECComponent.TYPE.TEC); });

                                foreach (P_ALG.KEY_P_VALUE keyPValue in groupPValueKeys) {
                                    switch (keyPValue.Id) {
                                        case 115:
                                        case 116:
                                            keyGroupPValue.Id = 5; // станция
                                            break;
                                        default:
                                            throw new Exception(string.Format(@"TaskAutobookMonthValuesCalculate::calculate () - неизвестный идентификатор [ID_COMP={0}] компонента...", keyPValue.Id));
                                            break;
                                    }

                                    fltRes += Out[pAlg.Key][keyPValue].value;
                                }

                                keyGroupPValue.Id = 5; // станция
                                validateKeyPValue(Out[pAlg.Key], keyGroupPValue);

                                Out[pAlg.Key][keyGroupPValue].value = fltRes;
                            }
                            break;
                        case "":
                        default:
                            throw new Exception(string.Format(@"TaskAutobookMonthValuesCalculate::calculate () - неизвестный параметр [NAlg={0}] расчета 1-го порядка...", pAlg.Key));
                            break;
                    }

                    delegateResultNAlg(_types, pAlg.Value.m_iId, RESULT.Ok);
                }

                return res;
            }
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.TYPE types
                , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
        {
            return new TaskAutobookMonthValuesCalculate(types, listNAlg, listPutPar, dictValues);
        }

        public override DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// PlanAutoBook
    /// </summary>
    public class HandlerDbTaskAutobookYarlyPlanCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.TYPE types
            , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
            , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
            , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err)
        {
            throw new NotImplementedException();
        }
    }
}


