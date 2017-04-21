using System;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

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
        public partial class TaskAutobookMonthValuesCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            public TaskAutobookMonthValuesCalculate(ListDATATABLE listDataTable) : base(listDataTable) { }

            public override DataTable Calculate(TYPE type)
            {
                throw new NotImplementedException();
            }

            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.ListDATATABLE listDataTable)
        {
            return new TaskAutobookMonthValuesCalculate(listDataTable);
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
    /// <summary>
    /// PlanAutoBook
    /// </summary>
    public class HandlerDbTaskAutobookYarlyPlanCalculate : TepCommon.HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override TaskCalculate createTaskCalculate(TaskCalculate.ListDATATABLE listDataTable)
        {
            throw new NotImplementedException();
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


