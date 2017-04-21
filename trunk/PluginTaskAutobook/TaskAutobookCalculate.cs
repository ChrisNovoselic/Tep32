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

            public override void Execute(Action<TYPE, IEnumerable<VALUE>, RESULT> delegateResultDataTable, Action<TYPE, string, RESULT> delegateResultPAlg)
            {
                throw new NotImplementedException();
            }

            protected override int initValues(IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
            {
                int iRes = -1;

                #region инициализация входных параметров/значений
                iRes = initValues(In
                    , listNAlg
                    , listPutPar
                    , dictValues[new KEY_VALUES() { TypeCalculate = TYPE.IN_VALUES, TypeState = STATE_VALUE.EDIT }]);
                #endregion

                return iRes;
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

        //protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        //{
        //    err = 0;

        //    TaskCalculate.ListDATATABLE listDataTableRes;

        //    listDataTableRes = new TaskCalculate.ListDATATABLE() {
        //        new TaskCalculate.DATATABLE() {
        //            m_indx = TaskCalculate.INDEX_DATATABLE.IN_PARAMETER
        //            , m_table = Select(getQueryParameters(TaskCalculate.TYPE.IN_VALUES), out err).Copy()
        //        }
        //        , new TaskCalculate.DATATABLE() {
        //            m_indx = TaskCalculate.INDEX_DATATABLE.IN_VALUES
        //            , m_table = getVariableTableValues(TaskCalculate.TYPE.IN_VALUES, out err).Copy()
        //        }
        //        , new TaskCalculate.DATATABLE() {
        //            m_indx = TaskCalculate.INDEX_DATATABLE.OUT_PARAMETER
        //            , m_table = Select(getQueryParameters(TaskCalculate.TYPE.OUT_VALUES), out err).Copy()
        //        }
        //        , new TaskCalculate.DATATABLE() {
        //            m_indx = TaskCalculate.INDEX_DATATABLE.OUT_VALUES
        //            , m_table = Select(getQueryParameters(TaskCalculate.TYPE.OUT_VALUES), out err).Copy()
        //        }
        //    };

        //    return listDataTableRes;
        //}
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

        //protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        //{
        //    throw new NotImplementedException();
        //}
    }
}


