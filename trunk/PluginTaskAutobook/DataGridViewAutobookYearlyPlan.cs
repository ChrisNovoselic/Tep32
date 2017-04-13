using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskAutobook
{
    partial class PanelTaskAutobookYearlyPlan
    {
        /// <summary>
        /// Панель отображения значений 
        /// и их обработки
        /// </summary>
        protected class DataGridViewAutobookYearlyPlan : DataGridViewValues
        {
            /// <summary>
            /// основной конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DataGridViewAutobookYearlyPlan(string name) : base(ModeData.DATETIME)
            {
                Name = name;

                InitializeComponents();
            }

            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                this.RowHeadersVisible = true;
            }

            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                addColumn(listNAlgParameter[0], listPutParameter[0]);
            }

            private void addColumn(HandlerDbTaskCalculate.NALG_PARAMETER nAlgPar, HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                Columns.Add(@"VALUE", @"Значения");
                Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            public override void ShowValues(IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> inValues, IEnumerable<TepCommon.HandlerDbTaskCalculate.VALUE> outValues, out int err)
            {
                err = 0;

                double dblVal = -1F;
                int idAlg = -1;

            }
        }
    }
}
