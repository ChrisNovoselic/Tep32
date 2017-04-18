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
            public DataGridViewValuesReaktivka(string name)
                : base (ModeData.DATETIME)
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
            public void AddColumn(HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (DataGridViewColumn col in Columns)
                        if ((((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).IdComponent > 0)
                            && (((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_component.IsTec == true)) {
                            indxCol = Columns.IndexOf(col);

                            break;
                        } else
                            ;

                    DataGridViewColumn column = new DataGridViewTextBoxColumn();
                    column.Tag = putPar;
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для псевдо-столбцов
                        if (putPar.IdComponent < 0)
                        {// для служебных столбцов
                            if (putPar.m_bVisibled == true) {// только для столбца с [SYMBOL]
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
                    column.ReadOnly = putPar.m_bEnabled;
                    column.Name = @"???";
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = putPar.m_bVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"DataGridViewTReaktivka::AddColumn (id_comp={0}) - ...", putPar.IdComponent), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            ///// <summary>
            ///// Добавить столбец
            ///// </summary>
            ///// <param name="text">Текст для заголовка столбца</param>
            ///// <param name="bRead">флаг изменения пользователем ячейки</param>
            ///// <param name="nameCol">имя столбца</param>
            ///// <param name="idPut">индентификатор источника</param>
            //public void AddColumn(string txtHeader, string nameCol, bool bRead, bool bVisibled)
            //{
            //    DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
            //    DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
            //    //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            //    try
            //    {
            //        HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
            //        alignText = DataGridViewContentAlignment.MiddleRight;
            //        autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
            //        column.Frozen = true;
            //        column.ReadOnly = bRead;
            //        column.Name = nameCol;
            //        column.HeaderText = txtHeader;
            //        column.DefaultCellStyle.Alignment = alignText;
            //        column.AutoSizeMode = autoSzColMode;
            //        Columns.Add(column as DataGridViewTextBoxColumn);
            //    }
            //    catch (Exception e)
            //    {
            //        Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //}

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            public void SetReadOnly (bool value)
            {
                foreach (DataGridViewColumn col in Columns)
                    if (((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).IdComponent > 0)
                        col.ReadOnly = value;
                    else
                        ;
            }

            ///// <summary>
            ///// Очищение отображения от значений
            ///// </summary>
            //public void ClearValues()
            //{
            //    //CellValueChanged -= onCellValueChanged;

            //    foreach (DataGridViewRow r in Rows)
            //        foreach (DataGridViewCell c in r.Cells)
            //            if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
            //                c.Value = string.Empty;

            //    //??? если установить 'true' - редактирование невозможно
            //    //ReadOnly = false;

            //    //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
            //}

            ///// <summary>
            ///// Добавить строку в таблицу
            ///// </summary>
            //public void AddRow(ROW_PROPERTY rowProp)
            //{
            //    int i = -1;
            //    // создать строку
            //    DataGridViewRow row = new DataGridViewRow();
            //    if (m_dictPropertiesRows == null)
            //        m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

            //    if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
            //        m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

            //    // добавить строку
            //    i = Rows.Add(row);
            //    // установить значения в ячейках для служебной информации
            //    Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
            //    Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
            //    // инициализировать значения в служебных ячейках
            //    m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
            //}

            ///// <summary>
            ///// Добавить строку в таблицу
            ///// </summary>
            //public void AddRow(ROW_PROPERTY rowProp, int DaysInMonth)
            //{
            //    int i = -1;
            //    // создать строку
            //    DataGridViewRow row = new DataGridViewRow();
            //    if (m_dictPropertiesRows == null)
            //        m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

            //    if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
            //        m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

            //    // добавить строку
            //    i = Rows.Add(row);
            //    // установить значения в ячейках для служебной информации
            //    Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
            //    // инициализировать значения в служебных ячейках
            //    //m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);

            //    if (i == DaysInMonth)
            //        foreach (HDataGridViewColumn col in Columns)
            //            Rows[i].Cells[col.Index].ReadOnly = true;//блокировка строк
            //}

            /// <summary>
            /// Обновить структуру таблицы
            /// </summary>
            /// <param name="indxDeny">Индекс элемента в массиве списков с отмененными для расчета/отображения компонентами ТЭЦ/параметрами алгоритма расчета</param>
            /// <param name="id">Идентификатор элемента (компонента/параметра)</param>
            /// <param name="bCheckedItem">Признак участия в расчете/отображения</param>
            public void UpdateStructure(PanelManagementReaktivka.ItemCheckedParametersEventArgs item)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
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
                        foreach (DataGridViewColumn c in Columns)
                            if (((HandlerDbTaskCalculate.PUT_PARAMETER)c.Tag).IdComponent == item.m_idComp) {
                                indx = Columns.IndexOf(c);
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
