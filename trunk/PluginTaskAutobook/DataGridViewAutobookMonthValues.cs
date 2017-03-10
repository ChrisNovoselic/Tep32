using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskAutobook
{
    partial class PanelTaskAutobookMonthValues
    {
        /// <summary>
        /// Класс для грида
        /// </summary>
        private class DataGridViewAutobookMonthValues : DataGridViewValues
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }         
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            /// <param name="nameDGV">Наименование элемента управления</param>
            public DataGridViewAutobookMonthValues(string name)
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
                //Отменить возможность добавления строк
                AllowUserToAddRows = false;
                //Отменить возможность удаления строк
                AllowUserToDeleteRows = false;
                //Отменить возможность изменения порядка следования столбцов строк
                AllowUserToOrderColumns = false;
                //Не отображать заголовки строк
                RowHeadersVisible = false;
                //Ширина столбцов под видимую область
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                AddColumn(-2, string.Empty, "ALG", true, false);
                AddColumn(-1, "Дата", "DATE", true, true);
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
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            public void AddColumn(string txtHeader, bool bRead, string nameCol)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    //column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    HClassLibrary.Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", HClassLibrary.Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="idPut">индентификатор источника</param>
            public void AddColumn(int idPut, string txtHeader, string nameCol, bool bRead, bool bVisibled)
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

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = idPut };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (idPut < 0)
                            // для служебных столбцов
                            //column.Frozen = true
                            ;
                    }

                    column.ReadOnly = bRead;
                    column.HeaderText = txtHeader;
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
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + idPut + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Установка идПута для столбца
            /// </summary>
            /// <param name="idPut">номер пута</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddIdComp(int idPut, string nameCol)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.m_iIdComp = idPut;
                    else
                        ;
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bRead">true/false</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddBRead(bool bRead, string nameCol)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.ReadOnly = bRead;
                    else
                        ;
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
            /// 
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            public void ClearValues()
            {
                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="planOnMonth">план на месяц</param>
            /// <param name="typeValues">тип данных</param>
            public void ShowValues(DataTable[] tbOrigin
                , DataTable planOnMonth
                , HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int idAlg = -1
                  , vsRatioValue = -1
                  , corOffset = 0;
                DataRow[] dr_CorValues, dr_Values = null;
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                bool bflg = false;
                double dblVal = -1F;
                //
                ClearValues();
                //заполнение плана
                if (planOnMonth.Rows.Count > 0)
                    planInMonth(planOnMonth.Rows[0]["VALUE"].ToString(),
                      Convert.ToDateTime(planOnMonth.Rows[0]["WR_DATETIME"].ToString()));
                //заполнение столбцов с корр. знач.
                foreach (DataGridViewRow row in Rows) {
                    foreach (HDataGridViewColumn col in Columns) {
                        if (col.Index > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) {
                            dr_CorValues = formingValue(tbOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                               , row.Cells["DATE"].Value.ToString(), m_currentOffSet, col.m_iIdComp);
                            idAlg = (int)row.Cells["ALG"].Value;

                            if (dr_CorValues != null)
                                for (int t = 0; t < dr_CorValues.Count(); t++) {
                                    dblVal = Convert.ToDouble(dr_CorValues[t]["VALUE"]);
                                    vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                    dblVal *= Math.Pow(10F, -1 * vsRatioValue);

                                    row.Cells[col.Index].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, CultureInfo.InvariantCulture);
                                }
                            else
                                ;

                            if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE == (int)typeValues)
                                corOffset = 1;
                            else
                                ;

                            dr_Values = formingValue(tbOrigin[(int)typeValues],
                               row.Cells["DATE"].Value.ToString()
                               , (m_currentOffSet * corOffset)
                               , col.m_iIdComp);

                            if (dr_Values != null)
                                if (dr_Values.Count() > 0)
                                    //заполнение столбцов ГТП,ТЭЦ
                                    for (int p = 0; p < dr_Values.Count(); p++)
                                        if (row.Cells["DATE"].Value.ToString() ==
                                        Convert.ToDateTime(dr_Values[p]["WR_DATETIME"]).AddMinutes(m_currentOffSet).AddDays(-1).ToShortDateString())
                                        {
                                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                                            dblVal = correctingValues(Math.Pow(10F, -1 * vsRatioValue)
                                                , dr_Values[p]["VALUE"]
                                                , col.Name
                                                , ref bflg
                                                , row
                                                , typeValues);


                                            dblVal *= Math.Pow(10F, -1 * vsRatioValue);

                                            row.Cells[col.Index].Value = dblVal.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                                CultureInfo.InvariantCulture);
                                        }

                        }
                    }
                    editCells(row);
                }
            }

            /// <summary>
            ///Корректировка знач.
            /// </summary>
            /// <param name="pow"></param>
            /// <param name="rowValue">значение</param>
            /// <param name="namecol">имя столбца</param>
            /// <param name="bflg">признак корректировки</param>
            /// <param name="row">тек.строка</param>
            /// <param name="typeValues">тип загружаеммых данных(архивные/текущие)</param>
            /// <returns></returns>
            private double correctingValues(double pow
                , object rowValue
                , string namecol
                , ref bool bflg
                , DataGridViewRow row
                , HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                double valRes = 0
                    , signValues = 1;

                switch (typeValues)
                {
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE:
                        signValues = -1;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE:
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                switch (namecol)
                {
                    case "GTP12":
                        if (double.TryParse(row.Cells["CorGTP12"].Value.ToString(), out valRes))
                        {
                            valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "GTP36":
                        if (double.TryParse(row.Cells["CorGTP36"].Value.ToString(), out valRes))
                        {
                            valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "TEC":
                        if (bflg)
                        {
                            valRes = double.Parse(row.Cells["GTP12"].Value.ToString()) * pow
                                + double.Parse(row.Cells["GTP36"].Value.ToString()) * pow;
                            bflg = false;
                        }
                        else
                            valRes = (double)rowValue;
                        break;
                    default:
                        break;
                }
                return valRes;
            }

            /// <summary>
            /// Вычисление месячного плана
            /// </summary>
            /// <param name="value">значение</param>
            /// <param name="date">дата</param>
            private void planInMonth(string value, DateTime date)
            {
                int idAlg
                     , vsRatioValue = -1;
                double planDay
                   , dbValue
                    , increment = 0;

                idAlg = (int)Rows[0].Cells["ALG"].Value;
                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                planDay = (Convert.ToSingle(value)
                   / DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month));

                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    increment = increment + planDay;
                    dbValue = increment * Math.Pow(10F, 1 * vsRatioValue);
                    Rows[i].Cells["PlanSwen"].Value = dbValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound,
                                             CultureInfo.InvariantCulture);
                }

                dbValue = float.Parse(value) * Math.Pow(10F, -1 * vsRatioValue);

                Rows[DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month) - 1].Cells["PlanSwen"].Value =
                    dbValue.ToString(@"F" + m_dictPropertiesRows[idAlg].m_vsRound, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Редактирование значений ввиду новых корр. значений
            /// </summary>
            /// <param name="e"></param>
            /// <param name="colName">имя столбца, 
            /// который попадает под корректировку</param>
            public void editCells(DataGridViewCellParsingEventArgs e, string colName)
            {
                double valueNew,//новое знач.
                valueCor,//первичное знач.
                valueCell = 0,//знач. ячейки
                value = 0;

                if (e.Value.ToString() == string.Empty)
                    valueNew = 0;
                else
                    valueNew = AsParseToF(e.Value.ToString());

                valueCor = AsParseToF(Rows[e.RowIndex].Cells[Columns[e.ColumnIndex].Name].Value.ToString());

                switch ((INDEX_GTP)Enum.Parse(typeof(INDEX_GTP), Columns[e.ColumnIndex].Name))
                {
                    case INDEX_GTP.CorGTP12:
                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), out valueCell);

                        break;
                    case INDEX_GTP.CorGTP36:
                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), out valueCell);
                        break;
                }

                if (!(valueCell == 0)) {
                    Rows[e.RowIndex].Cells[INDEX_GTP.TEC.ToString()].Value = AsParseToF(Rows[e.RowIndex].Cells[colName].Value.ToString())
                               + valueCell;

                    for (int i = e.RowIndex; i < Rows.Count; i++)
                        if (double.TryParse(Rows[e.RowIndex].Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                            fillCells(Rows[i]);
                        else
                            break;
                } else
                //??? значение ячейки == 0, ну и что? считать не надо
                    ;
            }

            /// <summary>
            /// Редактирование значений ввиду новых корр. значений
            /// </summary>
            /// <param name="row">редактируемая строка</param>
            public void editCells(DataGridViewRow row)
            {
                double valueCor,//новое знач.
                valueCell = 0,//знач. ячейки
                value = 0;

                foreach (DataGridViewColumn col in row.DataGridView.Columns) {
                    switch ((INDEX_GTP)Enum.Parse(typeof(INDEX_GTP), col.Name)) {
                        case INDEX_GTP.CorGTP12:
                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCor + valueCell;
                            break;

                        case INDEX_GTP.CorGTP36:
                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCor + valueCell;
                            break;
                    }
                }

                Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value = double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString())
                           + double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString());

                for (int i = row.Index; i < Rows.Count; i++)
                    if (double.TryParse(Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                        fillCells(Rows[i]);
                    else
                        break;
            }

            ///// <summary>
            ///// Редактирование значений ввиду новых корр. значений
            ///// </summary>
            ///// <param name="row">редактируемая строка</param>
            //public void editCells(DataGridViewRow row)
            //{
            //    double valueCor,//новое знач.
            //    valueCell = 0,//знач. ячейки
            //    value = 0;

            //    foreach (DataGridViewColumn col in row.DataGridView.Columns)
            //    {
            //        if (col.Name == "CorGTP12")
            //        {
            //            if (row.Cells[col.Name].Value.ToString() == string.Empty)
            //                valueCor = 0;
            //            else
            //                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());

            //            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

            //            if (valueCor == 0)
            //                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCell - valueCor;
            //            else
            //                Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value = valueCor + valueCell;
            //        }
            //        else if (col.Name == "CorGTP36")
            //        {
            //            if (row.Cells[col.Name].Value.ToString() == string.Empty)
            //                valueCor = 0;
            //            else
            //                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());

            //            double.TryParse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

            //            if (valueCor == 0)
            //                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCell - valueCor;
            //            else
            //                Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value = valueCor + valueCell;
            //            break;
            //        }
            //    }

            //    Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value = double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP12.ToString()].Value.ToString())
            //               + double.Parse(Rows[row.Index].Cells[INDEX_GTP.GTP36.ToString()].Value.ToString());

            //    for (int i = row.Index; i < Rows.Count; i++)
            //        if (double.TryParse(Rows[row.Index].Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
            //            fillCells(Rows[i]);
            //        else
            //            break;
            //}

            /// <summary>
            /// Вычисление параметров нараст.ст.
            /// и заполнение грида
            /// </summary>
            /// <param name="row">строка</param>
            private void fillCells(DataGridViewRow row)
            {
                int value
                    , swenValue = 0;

                if (int.TryParse(row.Cells[INDEX_GTP.TEC.ToString()].Value.ToString(), out value))
                {
                    if (row.Index == 0)
                        row.Cells["StSwen"].Value = value;
                    else
                    {
                        int.TryParse(row.DataGridView.Rows[row.Index - 1].Cells["StSwen"].Value.ToString(), out swenValue);
                        row.Cells["StSwen"].Value = value + swenValue;
                    }
                    countDeviation(row);
                }
            }

            /// <summary>
            /// Вычисление отклонения от плана
            /// </summary>
            /// <param name="row">строка</param>
            public void countDeviation(DataGridViewRow row)
            {
                int _number = 0;

                if (row.Cells["StSwen"].Value == null)
                    row.Cells["DevOfPlan"].Value = "";
                else
                    if (int.TryParse(row.Cells["PlanSwen"].Value.ToString(), out _number))
                    row.Cells["DevOfPlan"].Value = Convert.ToSingle(row.Cells["StSwen"].Value) - _number;
                else
                    row.Cells["DevOfPlan"].Value = Convert.ToSingle(row.Cells["StSwen"].Value) - 0;
            }

            /// <summary>
            /// Отбор строк по дате и идПуту
            /// </summary>
            /// <param name="dtOrigin">таблица значений</param>
            /// <param name="date">дата</param>
            /// <param name="idPut">идЭлемента</param>
            /// <returns>набор строк</returns>
            private DataRow[] formingValue(DataTable dtOrigin, string date, int idPut)
            {
                DateTime dateOffSet;
                DataRow[] dr_idCorPut = null;

                var m_enumResIDPUT = (from r in dtOrigin.AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    dateOffSet = m_enumResIDPUT.ElementAt(i).DATE_TIME;

                    if (date == dateOffSet.ToShortDateString())
                    {
                        dr_idCorPut = dtOrigin.Select(
                        string.Format(dtOrigin.Locale
                        , "WR_DATETIME = '{0:o}' AND ID_PUT = {1}", m_enumResIDPUT.ElementAt(i).DATE_TIME, idPut));
                        break;
                    }
                }
                return dr_idCorPut;
            }

            /// <summary>
            /// Отбор строк по дате и идПуту
            /// </summary>
            /// <param name="dtOrigin">таблица значений</param>
            /// <param name="date">дата</param>
            /// <param name="offSet">часовая разница</param>
            /// <param name="idPut">идЭлемента</param>
            /// <returns>набор строк</returns>
            private DataRow[] formingValue(DataTable dtOrigin
                , string date
                , int offSet
                , int idPut)
            {
                DateTime dateOffSet;
                DataRow[] dr_idCorPut = null;

                var m_enumResIDPUT = (from r in dtOrigin.AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    dateOffSet = m_enumResIDPUT.ElementAt(i).DATE_TIME.AddMinutes(offSet).AddDays(-1);

                    if (date == dateOffSet.ToShortDateString())
                    {
                        dr_idCorPut = dtOrigin.Select(string.Format(dtOrigin.Locale, "WR_DATETIME = '{0:o}' AND ID_PUT = {1}", m_enumResIDPUT.ElementAt(i).DATE_TIME, idPut));
                        break;
                    }
                }
                return dr_idCorPut;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="e">переменная с данными события</param>
            /// <returns></returns>
            public DataTable FillTableCorValue(int offset, DataGridViewCellParsingEventArgs e)
            {
                double valueToRes;
                int idComp = 0
                     , idAlg
                     , vsRatioValue = -1;
                DateTime timeRes;
                HDataGridViewColumn cols = (HDataGridViewColumn)Columns[e.ColumnIndex];

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });


                for (int i = 0; i < Rows.Count; i++)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.Index > (int)INDEX_GTP.GTP36 & col.Index < (int)INDEX_GTP.CorGTP36)
                        {
                            idAlg = (int)Rows[0].Cells["ALG"].Value;
                            vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;

                            if (cols.m_iIdComp == col.m_iIdComp &&
                                Rows[i].Cells["Date"].Value == Rows[e.RowIndex].Cells["Date"].Value)
                            {
                                valueToRes = AsParseToF(e.Value.ToString()) * Math.Pow(10F, 1 * vsRatioValue);
                                idComp = cols.m_iIdComp;
                            }
                            else
                                if (double.TryParse(Rows[i].Cells[col.Index].Value.ToString(), out valueToRes))
                            {
                                valueToRes *= Math.Pow(10F, 1 * vsRatioValue);
                                idComp = col.m_iIdComp;
                            }
                            else
                                valueToRes = -1;

                            timeRes = Convert.ToDateTime(Rows[i].Cells["Date"].Value.ToString());
                            //при -1 не нужно записывать значение в таблицу
                            if (valueToRes > -1)
                                dtSourceEdit.Rows.Add(new object[]
                                {
                                    idComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes
                                    , timeRes.AddMinutes(-offset).ToString("F",dtSourceEdit.Locale)
                                    , i
                                });
                        }
                    }
                }
                return dtSourceEdit;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <returns>таблица значений</returns>
            public DataTable FillTableCorValue(int offset)
            {
                int idAlg
                    , vsRatioValue = -1;
                double valueToRes = -1;
                DateTime dtRes = new DateTime();

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                    });

                foreach (DataGridViewRow row in Rows)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.Index > (int)INDEX_GTP.GTP36 & col.Index < (int)INDEX_GTP.CorGTP36)
                        {
                            try
                            {
                                idAlg = (int)row.Cells["ALG"].Value;
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                dtRes = Convert.ToDateTime(row.Cells["Date"].Value.ToString());

                                if (row.Cells[col.Index].Value != null)
                                    if (row.Cells[col.Index].Value.ToString() != "")
                                    {
                                        valueToRes = AsParseToF(row.Cells[col.Index].Value.ToString());
                                        valueToRes *= Math.Pow(10F, 1 * vsRatioValue);
                                    }
                                    else
                                        valueToRes = -1;
                            }
                            catch (Exception)
                            {

                            }

                            if (valueToRes > -1)
                                dtSourceEdit.Rows.Add(new object[]
                                {
                                    col.m_iIdComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes
                                    , dtRes.AddMinutes(-offset).ToString("F",dtSourceEdit.Locale)
                                    , row.Cells["Date"].Value.ToString()
                                });
                        }
                    }
                }
                return dtSourceEdit;
            }

            /// <summary>
            /// Формирование таблицы вых. значений
            /// </summary>
            /// <returns></returns>
            public DataTable FillTableValueDay()
            {
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                int idAlg
                   , vsRatioValue = -1;
                double valueToRes = 0F;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                    });

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.Index > (int)INDEX_GTP.CorGTP12 & col.Index <= (int)INDEX_GTP.COUNT + 1)
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (Convert.ToDateTime(row.Cells["Date"].Value) <= DateTime.Now.Date)
                            {
                                idAlg = (int)row.Cells["ALG"].Value;
                                vsRatioValue = m_dictRatio[m_dictPropertiesRows[idAlg].m_vsRatio].m_value;
                                vsRatio = vsRatioValue;

                                if (row.Cells[col.Index].Value != null)
                                {
                                    AsParseToF(row.Cells[col.Index].Value.ToString());

                                    valueToRes *= Math.Pow(10F, 1 * vsRatioValue);

                                    dtSourceEdit.Rows.Add(new object[]
                                    {
                                        col.m_iIdComp
                                        , -1
                                        , 1.ToString()
                                        , valueToRes
                                        , Convert.ToDateTime(row.Cells["Date"].Value.ToString()).ToString("F",dtSourceEdit.Locale)
                                        , row.Cells["Date"].Value.ToString()
                                    });
                                }
                            }
                        }
                }
                return dtSourceEdit;
            }
        }
    }
}
