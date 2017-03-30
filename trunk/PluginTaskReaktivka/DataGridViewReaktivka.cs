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
                Dock = DockStyle.Fill;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
                //Запрет изменения размера строк
                AllowUserToResizeRows = false;
                //Отменить возможность добавления строк
                AllowUserToAddRows = false;
                //Отменить возможность удаления строк
                AllowUserToDeleteRows = false;
                //Отменить возможность изменения порядка следования столбцов строк
                AllowUserToOrderColumns = false;
                //Не отображать заголовки строк
                RowHeadersVisible = false;
                //Ширина столбцов под видимую область
                //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                //AddColumn(-2, string.Empty, INDEX_SERVICE_COLUMN.ALG.ToString(), true, false);
                //AddColumn(-1, "Дата", INDEX_SERVICE_COLUMN.DATE.ToString(), true, true);
            }


            public override void BuildStructure(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
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

                    DataGridViewColumn column = new DataGridViewColumn();
                    column.Tag = putPar;
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
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


            /// <summary>
            /// Отображение значений
            /// </summary>
            /// <param name="source">таблица с даными</param>
            public void ShowValues(DataTable source)
            {
                int idAlg = -1
                   , idParameter = -1
                   , iQuality = -1
                   , iCol = 0, iRow = 0
                   , vsRatioValue = -1;
                double dblVal = -1F,
                    dbSumVal = 0;
                DataRow[] parameterRows = null;

                //if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE == (int)typeValues)
                //    ;

                foreach (DataGridViewColumn col in Columns) {
                    try {
                        parameterRows = source.Select(string.Format(source.Locale, "ID_PUT = " + ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).IdComponent));
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, @"DataGridViewValuesReaktivka::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    foreach (DataGridViewRow row in Rows) {
                        dbSumVal = 0;

                        if (row.Index != RowCount - 1) {
                            try
                            {
                                idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_idNAlg;
                            } catch (Exception exp) {
                                Logging.Logg().Exception(exp, @"DataGridViewValuesReaktivka::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }

                            for (int i = 0; i < parameterRows.Count(); i++) {
                                //??? как можно сравнить дату/время в строках
                                //??? сравнивать дату, но при этом добавлять минуты
                                //??? зачем учитывать смещение
                                if (Convert.ToDateTime(parameterRows[i][@"WR_DATETIME"]).AddMinutes(180/*m_currentOffSet*/).AddDays(-1).ToShortDateString() ==
                                    ((DateTime)row.Tag).ToString()) {
                                    idParameter = (int)parameterRows[i][@"ID_PUT"];
                                    dblVal = ((double)parameterRows[i][@"VALUE"]);
                                    iQuality = (int)parameterRows[i][@"QUALITY"];

                                    if ((row.Cells[iCol].ReadOnly = double.IsNaN(dblVal)) == false) {
                                        dblVal = GetValueCellAsRatio(idAlg, dblVal);

                                        row.Cells[iCol].Value = dblVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                                        dbSumVal += dblVal;
                                    } else
                                        ;
                                } else
                                    ;
                            }
                        } else
                            row.Cells[iCol].Value = dbSumVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);

                        iRow++;
                    }

                    iCol++;                    
                }
            }

            /// <summary>
            /// Перерасчет суммы по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <param name="indxRow">индекс строки</param>
            /// <param name="newValue">новое значение</param>
            public void SumValue(int indxCol, int indxRow)
            {
                //int idAlg = -1;
                //double sumValue = 0F
                //    , value;

                //idAlg = (int)Rows[indxRow].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value;

                //foreach (DataGridViewRow row in Rows)
                //    if (row.Cells[indxCol].Value != null)
                //    {
                //        if (Rows.Count - 1 != row.Index)
                //        {
                //            value = AsParseToF(row.Cells[indxCol].Value.ToString());
                //            sumValue += value;
                //        }
                //        else
                //            row.Cells[indxCol].Value = sumValue.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                //        formatCell();
                //    }
                //    else
                //        ;
            }

            /// <summary>
            /// ??? Формирование таблицы данных с отображения
            /// </summary>
            /// <param name="tableSourceOrg">таблица с оригинальными данными</param>
            /// <param name="idSession">номер сессии пользователя</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>таблица с новыми данными с вьюхи</returns>
            public DataTable GetValue(DataTable tableSourceOrg, int idSession, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                //int i = 0
                //    , idAlg = -1
                //     , vsRatioValue = -1
                //     , quality = -1;
                //double valueToRes = 0;
                //DateTime dtVal;

                DataTable tableSourceEdit = new DataTable();
                //tableSourceEdit.Columns.AddRange(new DataColumn[] {
                //    new DataColumn (@"ID_PUT", typeof (int))
                //    , new DataColumn (@"ID_SESSION", typeof (long))
                //    , new DataColumn (@"QUALITY", typeof (int))
                //    , new DataColumn (@"VALUE", typeof (float))
                //    , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                //    , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                //});

                //foreach (DataGridViewColumn col in Columns)
                //{
                //    if (col.m_iIdComp > 0)
                //        foreach (DataGridViewRow row in Rows)
                //            if (row.Index < (row.DataGridView.RowCount - 1))
                //            // без крайней строки
                //                if ((!(row.Cells[col.Index].Value == null))
                //                    && (string.IsNullOrEmpty(row.Cells[col.Index].Value.ToString()) == false)) {
                //                    idAlg = (int)row.Cells[INDEX_SERVICE_COLUMN.ALG.ToString()].Value;
                //                    valueToRes = Convert.ToDouble(row.Cells[col.Index].Value.ToString().Replace('.', ','));
                //                    vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;

                //                    valueToRes *= Math.Pow(10F, 1 * vsRatioValue);
                //                    dtVal = (DateTime)row.Tag;

                //                    quality = diffRowsInTables(tableSourceOrg, valueToRes, i, idAlg, typeValues);

                //                    tableSourceEdit.Rows.Add(new object[]
                //                    {
                //                        col.m_iIdComp
                //                        , idSession
                //                        , quality
                //                        , valueToRes
                //                        //??? зачем учитывать смещение
                //                        , dtVal.AddMinutes(-180/*m_currentOffSet*/).ToString("F", tableSourceEdit.Locale)
                //                        , i
                //                    });
                //                    i++;
                //                } else
                //                // ячейка пустая
                //                    ;
                //            else
                //            // крайняя строка
                //                ;
                //}

                //try
                //{
                //    tableSourceEdit = sortingTable(tableSourceEdit, "WR_DATETIME, ID_PUT");
                //}
                //catch (Exception)
                //{
                //    throw;
                //}

                return tableSourceEdit;
            }

            /// <summary>
            /// ??? Форматирование значений
            /// </summary>
            private void formatCell()
            {
                //int idAlg = -1
                //     , vsRatioValue = -1,
                //     iCol = 0;
                ////double dblVal = 1F;

                //foreach (HDataGridViewColumn column in Columns)
                //{
                //    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                //        foreach (DataGridViewRow row in Rows)
                //        {
                //            if (row.Index != row.DataGridView.RowCount - 1)
                //                if (row.Cells[iCol].Value != null)
                //                    if (row.Cells[iCol].Value.ToString() != "")
                //                    {
                //                        idAlg = (int)row.Cells[INDEX_SERVICE_COLUMN.ALG.ToString()].Value;
                //                        vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;
                //                        row.Cells[iCol].Value =
                //                            //AsParseToF
                //                            HMath.doubleParse
                //                                (row.Cells[iCol].Value.ToString()).ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                //                    }
                //        }
                //    iCol++;
                //}
            }

            /// <summary>
            /// соритровка таблицы по столбцу
            /// </summary>
            /// <param name="table">таблица для сортировки</param>
            /// <param name="sortStr">имя столбца/ов для сортировки</param>
            /// <returns>отсортированная таблица</returns>
            private DataTable sortingTable(DataTable table, string colSort)
            {
                DataView dView = table.DefaultView;
                string sortExpression = string.Format(colSort);
                dView.Sort = sortExpression;
                table = dView.ToTable();

                return table;
            }

            /// <summary>
            /// Проверка на изменение значений в двух таблицах
            /// </summary>
            /// <param name="origin">оригинальная таблица</param>
            /// <param name="editValue">значение</param>
            /// <param name="i">номер строки</param>
            /// <param name="idAlg">номер алгоритма</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>показатель изменения</returns>
            private int diffRowsInTables(DataTable origin, double editValue, int i, int idAlg, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int quality = 1;
                double originValues;

                origin = sortingTable(origin, "ID_PUT, WR_DATETIME");

                if (origin.Rows.Count - 1 < i)
                    originValues = 0;
                else
                    originValues =
                        AsParseToF(origin.Rows[i]["VALUE"].ToString());

                switch (typeValues)
                {
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE:
                        if (originValues.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture).Equals(editValue.ToString()) == false)
                            quality = 2;
                        else
                        //???
                            ;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD:
                        quality = 1;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                return quality;
            }
        }
    }
}
