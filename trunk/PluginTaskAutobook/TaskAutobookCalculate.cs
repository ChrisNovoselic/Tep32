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

                #region инициализация входных параметров/значений
                iRes = initValues(In
                    , listNAlg.Where(item => { return (item.m_type & TYPE.IN_VALUES) == TYPE.IN_VALUES; })
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = TYPE.IN_VALUES, TypeState = STATE_VALUE.EDIT }]);
                #endregion

                #region инициализация выходных параметров/значений
                iRes = initValues(Out
                    , listNAlg.Where(item => { return (item.m_type & TYPE.OUT_VALUES) == TYPE.OUT_VALUES; })
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = TYPE.IN_VALUES, TypeState = STATE_VALUE.EDIT }]);
                #endregion

                return iRes;
            }

            private RESULT calculate(Action<TYPE, int, RESULT> delegateResultNAlg)
            {
                RESULT res = RESULT.Ok;
                RESULT[] resNAlg = new RESULT[_dictPAlg[TYPE.OUT_VALUES].Count];

                foreach (KeyValuePair<string, P_ALG.P_PUT> pAlg in _dictPAlg[TYPE.OUT_VALUES]) {
                    switch (pAlg.Key) {
                        case "1":
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


