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
        /// 
        /// </summary>
        protected class DGVReaktivka : DataGridView
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DGVReaktivka(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nameDGV"></param>
            private void InitializeComponents(string nameDGV)
            {
                Name = nameDGV;
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

                AddColumn(-2, string.Empty, "ALG", true, false);
                AddColumn(-1, "Дата", "Date", true, true);
            }

            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_iIdComp;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
            }

            /// <summary>
            /// Структура для описания добавляемых строк
            /// </summary>
            public class ROW_PROPERTY
            {
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { IS_NAN }
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idParameter, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                    }

                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }

                /// <summary>
                /// Пояснения к параметру в алгоритме расчета
                /// </summary>
                public string m_strMeasure
                    , m_Value;
                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRound;

                public HDataGridViewCell[] m_arPropertiesCells;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="cntCols"></param>
                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];
                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">номер компонента</param>
            /// <param name="txtHeader">заголовок столбца</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="bRead">"только чтение"</param>
            /// <param name="bVisibled">видимость столбца</param>
            public void AddColumn(int id_comp, string txtHeader, string nameCol, bool bRead, bool bVisibled)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (HDataGridViewColumn col in Columns)
                        if ((col.m_iIdComp > 0)
                            && (col.m_iIdComp < 1000))
                        {
                            indxCol = Columns.IndexOf(col);

                            break;
                        }
                        else
                            ;

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = id_comp, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (id_comp < 0)
                        {// для служебных столбцов
                            if (bVisibled == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }
                            column.Frozen = true;

                        }
                    }

                    column.HeaderText = txtHeader;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = bVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + id_comp + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="idPut">индентификатор источника</param>
            public void AddColumn(string txtHeader, string nameCol, bool bRead, bool bVisibled)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Удаление набора строк
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bRead">true/false</param>
            public void AddBRead(bool bRead)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_iIdComp > 0)
                        col.ReadOnly = bRead;
            }

            /// <summary>
            /// Очищение отображения от значений
            /// </summary>
            public void ClearValues()
            {
                //CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;

                //??? если установить 'true' - редактирование невозможно
                //ReadOnly = false;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow(ROW_PROPERTY rowProp)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

                if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
                    m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
                // инициализировать значения в служебных ячейках
                m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow(ROW_PROPERTY rowProp, int DaysInMonth)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

                if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
                    m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                // инициализировать значения в служебных ячейках
                //m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);

                if (i == DaysInMonth)
                    foreach (HDataGridViewColumn col in Columns)
                        Rows[i].Cells[col.Index].ReadOnly = true;//блокировка строк
            }

            /// <summary>
            /// 
            /// </summary>
            protected struct RATIO
            {
                public int m_id;
                public int m_value;
                public string m_nameRU
                    , m_nameEN
                    , m_strDesc;
            }

            /// <summary>
            /// 
            /// </summary>
            protected Dictionary<int, RATIO> m_dictRatio;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tblRatio"></param>
            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO()
                    {
                        m_id = (int)r[@"ID"]
                        ,
                        m_value = (int)r[@"VALUE"]
                        ,
                        m_nameRU = (string)r[@"NAME_RU"]
                        ,
                        m_nameEN = (string)r[@"NAME_RU"]
                        ,
                        m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }

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
                switch (item.m_indxIdDeny)
                {
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        // найти индекс столбца (компонента) - по идентификатору
                        foreach (HDataGridViewColumn c in Columns)
                            if (c.m_iIdComp == item.m_idItem)
                            {
                                indx = Columns.IndexOf(c);
                                break;
                            }
                        break;
                    default:
                        break;
                }

                if (!(indx < 0))
                {
                    switch (item.m_indxIdDeny)
                    {
                        //case INDEX_ID.DENY_COMP_CALCULATED:
                        //    cIndx = indx;
                        //    // для всех ячеек в столбце
                        //    foreach (DataGridViewRow r in Rows)
                        //    {
                        //        indx = Rows.IndexOf(r);
                        //        if (getClrCellToComp(cIndx, indx, bItemChecked, out clrCell) == true)
                        //            r.Cells[cIndx].Style.BackColor = clrCell;
                        //        else
                        //            ;
                        //    }
                        //    (Columns[cIndx] as HDataGridViewColumn).m_bCalcDeny = !bItemChecked;
                        //    break;
                        //case INDEX_ID.DENY_PARAMETER_CALCULATED:
                        //    rKey = (int)Rows[indx].Cells[(int)INDEX_SERVICE_COLUMN.ID_ALG].Value;
                        //    // для всех ячеек в строке
                        //    foreach (DataGridViewCell c in Rows[indx].Cells)
                        //    {
                        //        cIndx = Rows[indx].Cells.IndexOf(c);
                        //        if (getClrCellToParameter(cIndx, indx, bItemChecked, out clrCell) == true)
                        //            c.Style.BackColor = clrCell;
                        //        else
                        //            ;

                        //        m_dictPropertiesRows[rKey].m_arPropertiesCells[cIndx].m_bCalcDeny = !bItemChecked;
                        //    }
                        //    break;
                        case INDEX_ID.DENY_COMP_VISIBLED:
                            cIndx = indx;
                            // для всех ячеек в столбце
                            Columns[cIndx].Visible = bItemChecked;
                            break;
                            //case INDEX_ID.DENY_PARAMETER_VISIBLED:
                            //    // для всех ячеек в строке
                            //    Rows[indx].Visible = bItemChecked;
                            //    break;
                            //default:
                            //    break;
                    }
                }
                else
                    ; // нет элемента для изменения стиля
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

                var enumTime = (from r in source.AsEnumerable()
                                orderby r.Field<DateTime>("WR_DATETIME")
                                select new
                                {
                                    WR_DATETIME = r.Field<DateTime>("WR_DATETIME"),
                                }).Distinct();

                //if ((int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION == (int)typeValues)
                //    ;

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                    {
                        try
                        {
                            parameterRows = source.Select(string.Format(source.Locale, "ID_PUT = " + col.m_iIdComp));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }

                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != RowCount - 1)
                            {
                                try
                                {
                                    idAlg = (int)row.Cells["ALG"].Value;
                                }
                                catch (Exception exp)
                                {
                                    MessageBox.Show(exp.ToString());
                                }

                                for (int i = 0; i < parameterRows.Count(); i++)
                                {
                                    if (Convert.ToDateTime(parameterRows[i][@"WR_DATETIME"]).AddMinutes(m_currentOffSet).AddDays(-1).ToShortDateString() ==
                                        row.Cells["Date"].Value.ToString())
                                    {
                                        idParameter = (int)parameterRows[i][@"ID_PUT"];
                                        dblVal = ((double)parameterRows[i][@"VALUE"]);
                                        iQuality = (int)parameterRows[i][@"QUALITY"];

                                        row.Cells[iCol].ReadOnly = double.IsNaN(dblVal);
                                        vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                                        dblVal *= Math.Pow(10F, 1 * vsRatioValue);

                                        row.Cells[iCol].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                            CultureInfo.InvariantCulture);
                                        dbSumVal += dblVal;
                                    }
                                }
                            }
                            else
                                row.Cells[iCol].Value = dbSumVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                    CultureInfo.InvariantCulture);

                            iRow++;
                        }
                    }
                    iCol++;
                    dbSumVal = 0;
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
                int idAlg = -1;
                double sumValue = 0F
                    , value;

                idAlg = (int)Rows[indxRow].Cells[0].Value;

                foreach (DataGridViewRow row in Rows)
                    if (row.Cells[indxCol].Value != null)
                    {
                        if (Rows.Count - 1 != row.Index)
                        {
                            value = AsParseToF(row.Cells[indxCol].Value.ToString());
                            sumValue += value;
                        }
                        else
                            row.Cells[indxCol].Value = sumValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                        CultureInfo.InvariantCulture);
                        formatCell();
                    }
            }

            /// <summary>
            /// Формирование таблицы данных с отображения
            /// </summary>
            /// <param name="dtSourceOrg">таблица с оригинальными данными</param>
            /// <param name="idSession">номер сессии пользователя</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>таблица с новыми данными с вьюхи</returns>
            public DataTable GetValue(DataTable dtSourceOrg, int idSession, HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
            {
                int i = 0,
                    idAlg = -1,
                     vsRatioValue = -1
                     , quality = -1;
                double valueToRes = 0;
                DateTime dtVal;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.m_iIdComp > 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (row.Cells[col.Index].Value != null)
                                    if (row.Cells[col.Index].Value.ToString() != "")
                                    {
                                        idAlg = (int)row.Cells["ALG"].Value;
                                        valueToRes = Convert.ToDouble(row.Cells[col.Index].Value.ToString().Replace('.', ','));
                                        vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                                        valueToRes *= Math.Pow(10F, 1 * vsRatioValue);
                                        dtVal = Convert.ToDateTime(row.Cells["Date"].Value.ToString());

                                        quality = diffRowsInTables(dtSourceOrg, valueToRes, i, idAlg, typeValues);

                                        dtSourceEdit.Rows.Add(new object[]
                                        {
                                            col.m_iIdComp
                                            , idSession
                                            , quality
                                            , valueToRes
                                            , dtVal.AddMinutes(-m_currentOffSet).ToString("F",dtSourceEdit.Locale)
                                            , i
                                        });
                                        i++;
                                    }
                        }
                }

                try
                {
                    dtSourceEdit = sortingTable(dtSourceEdit, "WR_DATETIME, ID_PUT");
                }
                catch (Exception)
                {
                    throw;
                }

                return dtSourceEdit;
            }

            /// <summary>
            /// Форматирование значений
            /// </summary>
            private void formatCell()
            {
                int idAlg = -1
                     , vsRatioValue = -1,
                     iCol = 0;
                //double dblVal = 1F;

                foreach (HDataGridViewColumn column in Columns)
                {
                    if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (row.Cells[iCol].Value != null)
                                    if (row.Cells[iCol].Value.ToString() != "")
                                    {
                                        idAlg = (int)row.Cells["ALG"].Value;
                                        vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                        row.Cells[iCol].Value = AsParseToF(row.Cells[iCol].Value.ToString()).ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                                    CultureInfo.InvariantCulture);
                                    }
                        }
                    iCol++;
                }
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
            private int diffRowsInTables(DataTable origin, double editValue, int i, int idAlg, HandlerDbTaskCalculate.INDEX_TABLE_VALUES typeValues)
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
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE:
                        if (originValues.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, CultureInfo.InvariantCulture) != editValue.ToString())
                            quality = 2;
                        break;
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION:
                        quality = 1;
                        break;
                    case HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                return quality;
            }
        }
    }
}
