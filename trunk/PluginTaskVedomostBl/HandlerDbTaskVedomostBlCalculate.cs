using HClassLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using TepCommon;
using System.Collections.Generic;

namespace PluginTaskVedomostBl
{
    public class HandlerDbTaskVedomostBlCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        ///класс для обработки данных
        /// </summary>
        public class TaskVedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TaskVedomostBlCalculate(TYPE types
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

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
                , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
                , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Создать объект для расчета выходных значений
        /// </summary>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.TYPE types
            , IEnumerable<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlg
            , IEnumerable<HandlerDbTaskCalculate.PUT_PARAMETER> listPutPar
            , Dictionary<KEY_VALUES, List<VALUE>> dictValues)
        {
            return new TaskVedomostBlCalculate(types, listNAlg, listPutPar, dictValues);
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