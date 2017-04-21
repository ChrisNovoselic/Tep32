using HClassLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    public class HandlerDbTaskVedomostBlCalculate : HandlerDbTaskCalculate
    {
        private partial class TaskVedomostBlCalculate : TaskCalculate
        {
            public TaskVedomostBlCalculate(ListDATATABLE listDataTables) : base(listDataTables)
            {
            }

            protected override int initValues(ListDATATABLE listDataTables)
            {
                int iRes = -1;

                return iRes;
            }

            public override DataTable Calculate(TYPE type)
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Создать объект для расчета выходных значений
        /// </summary>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.ListDATATABLE listDataTable)
        {
            return new TaskVedomostBlCalculate(listDataTable);
        }

        public override DataTable GetImportTableValues(TaskCalculate.TYPE type, long idSession, DataTable tableInParameter, DataTable tableRatio, out int err)
        {
            throw new NotImplementedException();
        }

        protected override TaskCalculate.ListDATATABLE prepareCalculateValues(TaskCalculate.TYPE type, out int err)
        {
            throw new NotImplementedException();
        }
    }
}