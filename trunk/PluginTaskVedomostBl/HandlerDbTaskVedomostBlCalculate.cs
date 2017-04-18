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
        /// <summary>
        /// Создать объект для расчета выходных значений
        /// </summary>
        protected override void createTaskCalculate()
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
}