using System;
using System.Globalization;
using System.Data;

using TepCommon;
using System.Collections.Generic;

namespace PluginTaskReaktivka
{
    /// <summary>
    /// Обращение у к данным, производство расчетов
    /// </summary>
    public class HandlerDbTaskReaktivkaCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект для расчета выходных значений
        /// </summary>
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
