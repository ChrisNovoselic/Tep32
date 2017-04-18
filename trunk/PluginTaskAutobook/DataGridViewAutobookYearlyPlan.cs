using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;
using System.Globalization;

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
                this.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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
                Columns[0].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

                Columns[0].Tag = putPar;
            }

            protected override bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value)
            {
                return (r.Tag is DateTime) ? value.stamp_value.Equals(((DateTime)(r.Tag))) == true : false;
            }
        }
    }
}
