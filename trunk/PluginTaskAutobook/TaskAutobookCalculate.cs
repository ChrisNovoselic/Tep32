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
    public class HandlerDbTaskAutobookMonthValuesCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();
        }
        /// <summary>
        /// Рассчитать выходные значения
        /// </summary>
        /// <param name="type">Тип расчета</param>
        /// <param name="tableOrigin">Оригинальная таблица</param>
        /// <param name="tableCalc">Выходная таблмца с рассчитанными значениями</param>
        /// <param name="err">Признак ошибки при выполнении метода</param>
        protected override void calculate(TaskCalculate.TYPE type, out DataTable tableOrigin, out DataTable tableCalc, out int err)
        {
            tableOrigin = new DataTable();
            tableCalc = new DataTable();
            err = 0;
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
    public class HandlerDbTaskAutobookYarlyPlanCalculate : HandlerDbTaskCalculate
    {
        /// <summary>
        /// Создать объект расчета для типа задачи
        /// </summary>
        /// <param name="type">Тип расчетной задачи</param>
        protected override void createTaskCalculate(/*ID_TASK idTask*/)
        {
            base.createTaskCalculate();
        }

        /// <summary>
        /// Рассчитать выходные значения
        /// </summary>
        /// <param name="type">Тип расчета</param>
        /// <param name="tableOrigin">Оригинальная таблица</param>
        /// <param name="tableCalc">Выходная таблмца с рассчитанными значениями</param>
        /// <param name="err">Признак ошибки при выполнении метода</param>
        protected override void calculate(TaskCalculate.TYPE type, out DataTable tableOrigin, out DataTable tableCalc, out int err)
        {
            err = 0;

            tableOrigin = new DataTable();
            tableCalc = new DataTable();            
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


