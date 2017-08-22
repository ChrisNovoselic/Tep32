using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;

namespace PluginTaskReaktivka
{
    partial class PanelTaskReaktivka
    {
        /// <summary>
        /// Представление для отображения значений (реактивная электроэнергия)
        /// </summary>
        protected class DataGridViewValuesReaktivka : DataGridViewValues
        {
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            //private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;

            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="nameDGV">Наименование представления (используется для поиска элемента)</param>
            public DataGridViewValuesReaktivka(string name, Func<int, int, float, int, float> fGetValueAsRatio)
                : base (ModeData.DATETIME, fGetValueAsRatio)
            {
                Name = name;

                InitializeComponents();
            }

            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                // Dock, MultiSelect, SelectionMode, ColumnHeadersVisible, ColumnHeadersHeightSizeMode, AllowUserToResizeColumns, AllowUserToResizeRows, AllowUserToAddRows, AllowUserToDeleteRows, AllowUserToOrderColumns
                // - устанавливаются в базовом классе
                //Не отображать заголовки строк
                RowHeadersVisible = true;

                //AddColumn(-2, string.Empty, INDEX_SERVICE_COLUMN.ALG.ToString(), true, false);
                //AddColumn(-1, "Дата", INDEX_SERVICE_COLUMN.DATE.ToString(), true, true);
            }


            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                throw new NotImplementedException();
            }          

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">номер компонента</param>
            /// <param name="txtHeader">заголовок столбца</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="bRead">"только чтение"</param>
            /// <param name="bVisibled">видимость столбца</param>
            public void AddColumn(HandlerDbTaskCalculate.IPUT_PARAMETERChange putPar)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (DataGridViewColumn col in Columns)
                        if ((((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).IdComponent > 0)
                            && (((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).m_component.IsTec == true)) {
                            indxCol = Columns.IndexOf(col);

                            break;
                        } else
                            ;

                    DataGridViewColumn column = new DataGridViewTextBoxColumn();
                    column.Tag = new COLUMN_TAG (putPar, ColumnCount + 2, false);
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для псевдо-столбцов
                        if (putPar.IdComponent < 0)
                        {// для служебных столбцов
                            if (putPar.IsVisibled == true) {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            } else
                                ;

                            column.Frozen = true;
                        }
                        else
                            ;
                    }

                    column.HeaderText = putPar.NameShrComponent;
                    column.ReadOnly = putPar.IsEnabled;
                    column.Name = @"???";
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = putPar.IsVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"DataGridViewTReaktivka::AddColumn (id_comp={0}) - ...", putPar.IdComponent), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            public void SetReadOnly (bool value)
            {
                foreach (DataGridViewColumn col in Columns)
                    if (((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).IdComponent > 0)
                        col.ReadOnly = value;
                    else
                        ;
            }

            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public void UpdateStructure(PanelManagementReaktivka.ItemCheckedParametersEventArgs item)
            {
                int indx = -1
                    , cIndx = -1
                    , rKey = -1;
                bool bItemChecked = item.NewCheckState == CheckState.Checked ? true :
                    item.NewCheckState == CheckState.Unchecked ? false :
                        false;

                //Поиск индекса элемента отображения
                if (item.m_type == PanelManagementTaskCalculate.ItemCheckedParametersEventArgs.TYPE.VISIBLE) {
                    if (item.IsComponent == true)
                    // найти индекс столбца (компонента) - по идентификатору
                        foreach (DataGridViewColumn col in Columns)
                            if (((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).IdComponent == item.m_idComp) {
                                indx = Columns.IndexOf(col);
                                break;
                            } else
                                ;
                    else
                    //??? рассмотреть другие случаи (NAlg, Put)
                        ;
                } else
                //??? рассмотреть другие случаи (ENABLE)
                    ;

                if (!(indx < 0)) {
                    if (item.m_type == PanelManagementTaskCalculate.ItemCheckedParametersEventArgs.TYPE.VISIBLE) { // VISIBLE
                        if (item.IsComponent == true) { // COMPONENT
                            cIndx = indx;
                            // для всех ячеек в столбце
                            Columns[cIndx].Visible = bItemChecked;
                        } else
                            //??? рассмотреть другие случаи (NAlg, Put)
                            ;
                    } else
                        //??? рассмотреть другие случаи (ENABLE)
                        ;
                } else
                // нет элемента для изменения стиля
                    ;
            }

            protected override bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value)
            {
                return (r.Tag is DateTime) ? value.stamp_value.Equals(((DateTime)(r.Tag))) == true : false;
            }
        }
    }
}
