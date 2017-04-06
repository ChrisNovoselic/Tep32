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
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            /// <param name="nameDGV">Наименование элемента управления</param>
            public DataGridViewAutobookMonthValues(string name) : base(ModeData.DATETIME)
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
                Name = INDEX_CONTROL.DGV_VALUES.ToString();                                

                //Ширина столбцов под видимую область
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                RowHeadersVisible = true;

                //AddColumn(-2, string.Empty, INDEX_SERVICE_COLUMN.ALG.ToString(), true, false);
                //AddColumn(-1, "Дата", INDEX_SERVICE_COLUMN.DATE.ToString(), true, true);
            }

            public override void BuildStructure(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                List<HandlerDbTaskCalculate.TECComponent> listTECComponent;
                HandlerDbTaskCalculate.TECComponent comp_tec;

                listTECComponent = new List<HandlerDbTaskCalculate.TECComponent>();

                listPutParameter.ForEach(putPar => {
                    if (listTECComponent.IndexOf(putPar.m_component) < 0)
                        listTECComponent.Add(putPar.m_component);
                    else
                        ;
                });
                // сортировать ГТП: 1,2 ПЕРЕД 3-6
                listTECComponent.Sort(delegate (HandlerDbTaskCalculate.TECComponent comp1, HandlerDbTaskCalculate.TECComponent comp2) {
                    return comp1.m_Id < comp2.m_Id ? -1
                        : comp1.m_Id > comp2.m_Id ? 1
                            : 0;
                });
                //ГТП - Корректировка ПТО
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true)
                        Columns.Add(string.Format(@"COLUMN_CORRECT_{0}", comp.m_Id), string.Format(@"Корр-ка ПТО {0}", comp.m_nameShr));
                    else
                        ;
                });
                //ГТП - Значения
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true)
                        Columns.Add(string.Format(@"COLUMN_{0}", comp.m_Id), string.Format(@"{0}", comp.m_nameShr));
                    else
                        ;
                });
                //Станция - компонент
                comp_tec = listTECComponent.Find(comp => { return comp.IsTec; });
                //Станция - Значения - ежесуточные
                Columns.Add(string.Format(@"COLUMN_ST_DAY_{0}", comp_tec.m_Id), string.Format(@"{0} значения", comp_tec.m_nameShr));
                //Станция - Значения - нарастающие
                Columns.Add(string.Format(@"COLUMN_ST_SUM_{0}", comp_tec.m_Id), string.Format(@"{0} нараст.", comp_tec.m_nameShr));
                //Станция - План - ежесуточный
                Columns.Add(string.Format(@"COLUMN_PLAN_DAY_{0}", comp_tec.m_Id), string.Format(@"План нараст."));
                //Станция - План - отклонение
                Columns.Add(string.Format(@"COLUMN_PALN_DEV_{0}", comp_tec.m_Id), string.Format(@"План отклон."));

                //Columns.Add(string.Format(@""), string.Format(@""));
                //Columns.Add(string.Format(@""), string.Format(@""));
                //Columns.Add(string.Format(@""), string.Format(@""));
            }

            ///// <summary>
            ///// Добавить столбец
            ///// </summary>
            ///// <param name="text">Текст для заголовка столбца</param>
            ///// <param name="bRead">Флаг изменения пользователем ячейки</param>
            ///// <param name="nameCol">Наименование столбца</param>
            //public void AddColumn(string txtHeader, bool bRead, string nameCol)
            //{
            //    DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
            //    DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
            //    //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            //    try {
            //        HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
            //        alignText = DataGridViewContentAlignment.MiddleRight;
            //        autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
            //        //column.Frozen = true;
            //        column.ReadOnly = bRead;
            //        column.Name = nameCol;
            //        column.HeaderText = txtHeader;
            //        column.DefaultCellStyle.Alignment = alignText;
            //        column.AutoSizeMode = autoSzColMode;
            //        Columns.Add(column as DataGridViewTextBoxColumn);
            //    } catch (Exception e) {
            //        HClassLibrary.Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", HClassLibrary.Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //}

            ///// <summary>
            ///// Установка идПута для столбца
            ///// </summary>
            ///// <param name="idPut">номер пута</param>
            ///// <param name="nameCol">имя стобца</param>
            //public void AddIdComp(int idPut, string nameCol)
            //{
            //    foreach (HDataGridViewColumn col in Columns)
            //        if (col.Name == nameCol)
            //            col.m_iIdComp = idPut;
            //        else
            //            ;
            //}

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="nameCol">имя стобца</param>
            /// <param name="bReadOnly">true/false</param>            
            public void SetReadOnly(string nameCol, bool bReadOnly)
            {
                foreach (DataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.ReadOnly = bReadOnly;
                    else
                        ;
            }

            private Dictionary<int, HandlerDbTaskCalculate.TECComponent> m_dictTECComponent;

            public override void AddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                base.AddPutParameter(putPar);

                if (m_dictTECComponent.ContainsKey(putPar.IdComponent) == false)
                    m_dictTECComponent.Add(putPar.IdComponent, putPar.m_component);
                else
                    ;
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tablesOrigin">таблица значений</param>
            /// <param name="tablePlanMonth">план на месяц</param>
            /// <param name="viewValues">тип данных</param>
            public void ShowValues(DataTable[] tablesOrigin
                , DataTable tablePlanMonth
                , HandlerDbTaskCalculate.ID_VIEW_VALUES viewValues)
            {
                int idAlg = -1
                  , idComp = -1;
                DataRow[] arRowValues = null;
                Array namePut = Enum.GetValues(typeof(INDEX_COLUMN));
                bool bflg = false;
                double dblVal = -1F;
                //
                ClearValues();
                //заполнение ячеек с плановыми значениями
                if (tablePlanMonth.Rows.Count > 0)
                    //showPlanValues(tablePlanMonth.Rows[0]["VALUE"].ToString()
                    //    , Convert.ToDateTime(tablePlanMonth.Rows[0]["WR_DATETIME"].ToString()))
                        ;
                else
                    ;
                //заполнение столбцов с корр. знач.
                foreach (DataGridViewRow row in Rows) {
                    foreach (DataGridViewColumn col in Columns) {
                        idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_idNAlg;
                        idComp = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).IdComponent;

                        // сформировать скорректированные значения для отображения
                        arRowValues = formingValue(tablesOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                            , (DateTime)row.Tag
                            ////??? зачем это смещение
                            //, 180 /*m_currentOffSet*/
                            , idComp);
                            
                        if (!(arRowValues == null))
                        // заполнить ячейки с корректированными значениями
                            for (int t = 0; t < arRowValues.Count(); t++) {
                                dblVal = GetValueCellAsRatio(idAlg, -1, Convert.ToDouble(arRowValues[t]["VALUE"]));

                                row.Cells[col.Index].Value = dblVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                            }
                        else
                            ;

                        //if (HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD == viewValues)
                        //    corOffset = 1;
                        //else
                        //    ;

                        arRowValues = formingValue(tablesOrigin[(int)viewValues]
                            , (DateTime)row.Tag
                            ////??? зачем это смещение
                            //, (180 /*m_currentOffSet*/ * corOffset)                               
                            , idComp);

                        if ((!(arRowValues == null))
                            && (arRowValues.Count() > 0))
                        // заполнение столбцов ГТП, ТЭЦ
                            for (int p = 0; p < arRowValues.Count(); p++)
                                if (((DateTime)row.Tag).Equals(Convert.ToDateTime(arRowValues[p]["WR_DATETIME"])) == true) {
                                    //vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;

                                    dblVal = correctingValues(/*Math.Pow(10F, -1 * vsRatioValue)
                                        , */arRowValues[p]["VALUE"]
                                        , col.Name
                                        , ref bflg
                                        , row
                                        , viewValues);                                        

                                    //dblVal *= Math.Pow(10F, -1 * vsRatioValue);
                                    dblVal = GetValueCellAsRatio(idAlg, -1, dblVal);

                                    row.Cells[col.Index].Value = dblVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                                } else
                                    ;
                        else
                            ;
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
            private double correctingValues(/*double pow
                , */object rowValue
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
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD:
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
                            //valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "GTP36":
                        if (double.TryParse(row.Cells["CorGTP36"].Value.ToString(), out valRes))
                        {
                            //valRes *= pow;
                            valRes += (double)rowValue * signValues;
                            bflg = true;
                        }
                        else
                            valRes = (double)rowValue;

                        break;
                    case "TEC":
                        if (bflg)
                        {
                            valRes = double.Parse(row.Cells["GTP12"].Value.ToString()) /** pow*/
                                + double.Parse(row.Cells["GTP36"].Value.ToString()) /** pow*/;
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

            ///// <summary>
            ///// Вычисление месячного плана
            ///// </summary>
            ///// <param name="value">значение</param>
            ///// <param name="date">дата</param>
            //private void showPlanValues(string value, DateTime date)
            //{
            //    int idAlg
            //         //, vsRatioValue = -1
            //         , indxLastRow = -1;
            //    double planDay
            //       , dblValue
            //        , increment = 0;

            //    idAlg = (int)Rows[0].Cells[INDEX_SERVICE_COLUMN.ALG.ToString()].Value;
            //    //vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;

            //    planDay = (Convert.ToSingle(value)
            //       / DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month));

            //    for (int i = 0; i < Rows.Count - 1; i++)
            //    {
            //        increment = increment + planDay;
            //        dblValue = increment * GetValueCellAsRatio(idAlg, -1, increment);

            //        Rows[i].Cells["PlanSwen"].Value =
            //            dblValue.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
            //    }
            //    //Значение для крайней строки - индекс строки
            //    indxLastRow = DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month) - 1;
            //    //Значение для крайней строки - значение
            //    dblValue = GetValueCellAsRatio(idAlg, -1, float.Parse(value));
            //    //Значение для крайней строки - форматирование значения
            //    Rows[indxLastRow].Cells["PlanSwen"].Value =
            //        dblValue.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
            //}

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

                switch ((INDEX_COLUMN)Enum.Parse(typeof(INDEX_COLUMN), Columns[e.ColumnIndex].Name))
                {
                    case INDEX_COLUMN.CorGTP12:
                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_COLUMN.GTP12.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_COLUMN.GTP36.ToString()].Value.ToString(), out valueCell);

                        break;
                    case INDEX_COLUMN.CorGTP36:
                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_COLUMN.GTP36.ToString()].Value.ToString(), out valueCell);

                        if (valueCell != 0)
                            if (valueNew == 0)
                                Rows[e.RowIndex].Cells[colName].Value = valueCell - valueCor;
                            else
                                Rows[e.RowIndex].Cells[colName].Value = (valueNew - valueCor) + valueCell;

                        double.TryParse(Rows[e.RowIndex].Cells[INDEX_COLUMN.GTP12.ToString()].Value.ToString(), out valueCell);
                        break;
                }

                if (!(valueCell == 0)) {
                    Rows[e.RowIndex].Cells[INDEX_COLUMN.TEC.ToString()].Value = AsParseToF(Rows[e.RowIndex].Cells[colName].Value.ToString())
                               + valueCell;

                    for (int i = e.RowIndex; i < Rows.Count; i++)
                        if (double.TryParse(Rows[e.RowIndex].Cells[INDEX_COLUMN.TEC.ToString()].Value.ToString(), out value))
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
                    switch ((INDEX_COLUMN)Enum.Parse(typeof(INDEX_COLUMN), col.Name)) {
                        case INDEX_COLUMN.CorGTP12:
                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_COLUMN.GTP12.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_COLUMN.GTP12.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_COLUMN.GTP12.ToString()].Value = valueCor + valueCell;
                            break;

                        case INDEX_COLUMN.CorGTP36:
                            if (row.Cells[col.Name].Value.ToString() == string.Empty)
                                valueCor = 0;
                            else
                                valueCor = AsParseToF(row.Cells[col.Name].Value.ToString());
                            //double.TryParse(row.Cells[col.Name].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCor);

                            double.TryParse(Rows[row.Index].Cells[INDEX_COLUMN.GTP36.ToString()].Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out valueCell);

                            if (valueCor == 0)
                                Rows[row.Index].Cells[INDEX_COLUMN.GTP36.ToString()].Value = valueCell - valueCor;
                            else
                                Rows[row.Index].Cells[INDEX_COLUMN.GTP36.ToString()].Value = valueCor + valueCell;
                            break;
                    }
                }

                Rows[row.Index].Cells[INDEX_COLUMN.TEC.ToString()].Value = double.Parse(Rows[row.Index].Cells[INDEX_COLUMN.GTP12.ToString()].Value.ToString())
                           + double.Parse(Rows[row.Index].Cells[INDEX_COLUMN.GTP36.ToString()].Value.ToString());

                for (int i = row.Index; i < Rows.Count; i++)
                    if (double.TryParse(Rows[row.Index].Cells[INDEX_COLUMN.TEC.ToString()].Value.ToString(), out value))
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

                if (int.TryParse(row.Cells[INDEX_COLUMN.TEC.ToString()].Value.ToString(), out value))
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
            /// <param name="tableOrigin">таблица значений</param>
            /// <param name="date">дата</param>
            /// <param name="idPut">идЭлемента</param>
            /// <returns>набор строк</returns>
            private DataRow[] formingValue(DataTable tableOrigin, DateTime date, int idPut)
            {
                DateTime dateOffSet;
                DataRow[] arRowRes = null;

                var enumResIDPUT = (from r in tableOrigin.AsEnumerable()
                    orderby r.Field<DateTime>("WR_DATETIME")
                    select new
                    {
                        WR_DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                    }).Distinct();

                for (int i = 0; i < enumResIDPUT.Count(); i++) {
                    dateOffSet = enumResIDPUT.ElementAt(i).WR_DATE_TIME;

                    if (date == dateOffSet.Date) {
                        arRowRes = tableOrigin.Select(
                            string.Format(tableOrigin.Locale
                                , "WR_DATETIME = '{0:o}' AND ID_PUT = {1}"
                                , enumResIDPUT.ElementAt(i).WR_DATE_TIME, idPut));

                        break;
                    } else
                        ;
                }

                return arRowRes;
            }

            ///// <summary>
            ///// Отбор строк по дате и идПуту
            ///// </summary>
            ///// <param name="dtOrigin">таблица значений</param>
            ///// <param name="date">дата</param>
            ///// <param name="offSet">часовая разница</param>
            ///// <param name="idPut">идЭлемента</param>
            ///// <returns>набор строк</returns>
            //private DataRow[] formingValue(DataTable tableOrigin
            //    , DateTime date
            //    , int offSet
            //    , int idPut)
            //{
            //    DateTime dateOffSet;
            //    DataRow[] arRowRes = null;

            //    var m_enumResIDPUT = (from r in tableOrigin.AsEnumerable()
            //                          orderby r.Field<DateTime>("WR_DATETIME")
            //                          select new
            //                          {
            //                              WR_DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
            //                          }).Distinct();

            //    for (int i = 0; i < m_enumResIDPUT.Count(); i++)
            //    {
            //        dateOffSet = m_enumResIDPUT.ElementAt(i).WR_DATE_TIME.AddMinutes(offSet).AddDays(-1);

            //        if (date == dateOffSet.Date)
            //        {
            //            arRowRes = tableOrigin.Select(string.Format(tableOrigin.Locale, "WR_DATETIME = '{0:o}' AND ID_PUT = {1}", m_enumResIDPUT.ElementAt(i).WR_DATE_TIME, idPut));
            //            break;
            //        }
            //    }
            //    return arRowRes;
            //}

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="e">переменная с данными события</param>
            /// <returns></returns>
            public DataTable FillTableCorValue(int offset, DataGridViewCellParsingEventArgs e)
            {
                DataTable tableRes = null;

                //double valueToRes;
                //int idComp = -1
                //     , idAlg = -1;
                //DateTime timeRes;
                //DataGridViewColumn cols = (DataGridViewColumn)Columns[e.ColumnIndex];

                //tableSourceEdit = new DataTable();
                //tableSourceEdit.Columns.AddRange(new DataColumn[] {
                //        new DataColumn (@"ID_PUT", typeof (int))
                //        , new DataColumn (@"ID_SESSION", typeof (long))
                //        , new DataColumn (@"QUALITY", typeof (int))
                //        , new DataColumn (@"VALUE", typeof (float))
                //        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                //        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                //    });

                //for (int i = 0; i < Rows.Count; i++) {
                //    foreach (DataGridViewColumn col in Columns) {
                //        if (col.Index > (int)INDEX_COLUMN.GTP36 & col.Index < (int)INDEX_COLUMN.CorGTP36) {
                //            idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_idNAlg;

                //            if (cols.m_iIdComp == col.m_iIdComp &&
                //                Rows[i].Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value == Rows[e.RowIndex].Cells["Date"].Value) {
                //                valueToRes = GetValueCellAsRatio(idAlg, -1, AsParseToF(e.Value.ToString()));
                //                idComp = cols.m_iIdComp;
                //            } else
                //                if (double.TryParse(Rows[i].Cells[col.Index].Value.ToString(), out valueToRes)) {
                //                valueToRes = GetValueCellAsRatio(idAlg, -1, valueToRes);
                //                idComp = col.m_iIdComp;
                //            } else
                //                valueToRes = -1;

                //            timeRes = Convert.ToDateTime(Rows[i].Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString());
                //            //при -1 не нужно записывать значение в таблицу
                //            if (valueToRes > -1)
                //                tableSourceEdit.Rows.Add(new object[]
                //                {
                //                    idComp
                //                    , -1
                //                    , 1.ToString()
                //                    , valueToRes
                //                    , timeRes.AddMinutes(-offset).ToString("F",tableSourceEdit.Locale)
                //                    , i
                //                });
                //        }
                //    }
                //}

                return tableRes;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="offset"></param>
            /// <returns>таблица значений</returns>
            public DataTable FillTableCorValue(int offset)
            {
                DataTable tableRes = null;

                //int idAlg
                //    //, vsRatioValue = -1
                //    ;
                //double valueToRes = -1;
                //DateTime dtRes = new DateTime();

                //dtSourceEdit = new DataTable();
                //dtSourceEdit.Columns.AddRange(new DataColumn[] {
                //        new DataColumn (@"ID_PUT", typeof (int))
                //        , new DataColumn (@"ID_SESSION", typeof (long))
                //        , new DataColumn (@"QUALITY", typeof (int))
                //        , new DataColumn (@"VALUE", typeof (float))
                //        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                //        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                //    });

                //foreach (DataGridViewRow row in Rows)
                //    foreach (HDataGridViewColumn col in Columns)
                //        if (col.Index > (int)INDEX_COLUMN.GTP36 & col.Index < (int)INDEX_COLUMN.CorGTP36) {
                //            try {
                //                idAlg = (int)row.Cells[INDEX_SERVICE_COLUMN.ALG.ToString()].Value;
                //                //vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;
                //                dtRes = Convert.ToDateTime(row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString());

                //                if (row.Cells[col.Index].Value != null)
                //                    if (string.IsNullOrEmpty(row.Cells[col.Index].Value.ToString()) == false) {
                //                        valueToRes = AsParseToF(row.Cells[col.Index].Value.ToString());
                //                        //valueToRes *= Math.Pow(10F, 1 * vsRatioValue);
                //                        valueToRes = GetValueCellAsRatio(idAlg, valueToRes);
                //                    } else
                //                        valueToRes = -1;
                //            } catch (Exception) {

                //            }

                //            if (valueToRes > -1)
                //                dtSourceEdit.Rows.Add(new object[] {
                //                    col.m_iIdComp
                //                    , -1
                //                    , 1.ToString()
                //                    , valueToRes
                //                    , dtRes.AddMinutes(-offset).ToString("F",dtSourceEdit.Locale)
                //                    , row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString()
                //                });
                //        } else
                //            ;
                //// цикл по столбцам - окончание
                //// цикл по строкам - окончание

                return tableRes;
            }

            /// <summary>
            /// Формирование таблицы вых. значений
            /// </summary>
            /// <returns></returns>
            public DataTable FillTableValueDay()
            {
                DataTable tableRes = null;

                //Array namePut = Enum.GetValues(typeof(INDEX_COLUMN));
                //int idAlg
                //   //, vsRatioValue = -1
                //   ;
                //double valueToRes = 0F;

                //tableRes = new DataTable();
                //tableRes.Columns.AddRange(new DataColumn[] {
                //        new DataColumn (@"ID_PUT", typeof (int))
                //        , new DataColumn (@"ID_SESSION", typeof (long))
                //        , new DataColumn (@"QUALITY", typeof (int))
                //        , new DataColumn (@"VALUE", typeof (float))
                //        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                //        , new DataColumn (@"EXTENDED_DEFINITION", typeof (string))
                //    });

                //foreach (DataGridViewColumn col in Columns) {
                //    if (col.Index > (int)INDEX_COLUMN.CorGTP12 & col.Index <= (int)INDEX_COLUMN.COUNT + 1)
                //        foreach (DataGridViewRow row in Rows) {
                //            if (Convert.ToDateTime(row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value) <= DateTime.Now.Date) {
                //                idAlg = (int)row.Cells[INDEX_SERVICE_COLUMN.ALG.ToString()].Value;
                //                //vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;
                //                //vsRatio = vsRatioValue;

                //                if (row.Cells[col.Index].Value != null) {
                //                    AsParseToF(row.Cells[col.Index].Value.ToString());

                //                    valueToRes = GetValueCellAsRatio(idAlg, -1, valueToRes);

                //                    tableRes.Rows.Add(new object[]
                //                    {
                //                        col.m_iIdComp
                //                        , -1
                //                        , 1.ToString()
                //                        , valueToRes
                //                        , Convert.ToDateTime(row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString()).ToString("F", tableRes.Locale)
                //                        , row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString()
                //                    });
                //                }
                //            }
                //        }
                //}

                return tableRes;
            }

            public override void Clear()
            {
                base.Clear();
            }
        }
    }
}
