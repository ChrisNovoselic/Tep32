
using ASUTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskBalTeplo
{
    partial class PanelTaskBalTeplo
    {
        /// <summary>
        /// Класс для грида
        /// </summary>
        protected class DataGridViewBalTeploValues : DataGridView
        {
            private Dictionary<string, HTepUsers.DictionaryProfileItem> m_dict_ProfileNALG_IN
                , m_dict_ProfileNALG_OUT;

            private DataTable m_dbRatio;

            #region Таги datagridvalue (???)

            public enum TYPE_COLUMN_TAG : short { UNKNOWN = short.MinValue, COMPONENT, PUT_PARAMETER, FORMULA_HELPER, GROUPING_PARAMETR }

            public struct COLUMN_TAG
            {
                /// <summary>
                /// Идентификатор столбца
                /// </summary>
                public object value;
                /// <summary>
                /// Тип объекта, являющегося идентификатором столбца
                /// </summary>
                public TYPE_COLUMN_TAG Type;
                /// <summary>
                /// Признак отмены агрегационной функции
                ///  (при наличии таковой, по, например, указанной в проекте единице измерения)
                /// </summary>
                public bool ActionAgregateCancel;
                /// <summary>
                /// Индекс(номер, адрес) столбца в книге MS Excel при экспорте значений столбца
                ///  , отсутствие значения - признак отсутствия необходимости экпорта значений столбца
                /// </summary>
                public int TemplateReportAddress;
            }

            #endregion


            public enum INDEX_VIEW_VALUES { Block = 2001, Output = 2002, TeploBL = 2003, TeploOP = 2004, Param = 2005, PromPlozsh = 2006 };

            public INDEX_VIEW_VALUES m_ViewValues;

            public DataGridViewBalTeploValues(string name)
            {
                m_dict_ProfileNALG_IN = new Dictionary<string, HTepUsers.DictionaryProfileItem>();
                m_dict_ProfileNALG_OUT = new Dictionary<string, HTepUsers.DictionaryProfileItem>();
                m_dbRatio = new DataTable();

                this.Name = name;
                InitializeComponents();

                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
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
                //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            }

            private void cellEndEdit(object sender, DataGridViewCellEventArgs e)
            {
                int iRatio = HTepUsers.s_iRatioDefault,
                    iRound = HTepUsers.s_iRoundDefault,
                    idRatio = 0;


                this.CellValueChanged -= new DataGridViewCellEventHandler(cellEndEdit);
                if (((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                    if (double.IsNaN(double.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace('.', ','))) == false)
                    {
                        if (((HDataGridViewColumn)Columns[e.ColumnIndex]).m_bInPut == true)
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND).ToString()]);
                        }
                        else
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND).ToString()]);
                        }

                        DataRow[] rows_Ratio = m_dbRatio.Select("ID=" + idRatio);

                        if (rows_Ratio.Length > 0)
                            iRatio = int.Parse(rows_Ratio[0]["VALUE"].ToString());

                        double value = double.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace('.', ','));
                        value *= Math.Pow(10F, -1 * iRatio);
                        ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString(@"F" + iRound,
                                                        CultureInfo.InvariantCulture);
                    }

                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
            }

            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            public class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public string m_N_ALG;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
                public bool m_bInPut;
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

                try {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleLeft;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    //column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead">флаг изменения пользователем ячейки</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="idPut">индентификатор источника</param>
            public void AddColumn(string txtHeader, bool bRead, string nameCol, string N_ALG, bool bInPut)
            {
                HDataGridViewColumn column;
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try {
                    column = new HDataGridViewColumn() { m_bCalcDeny = false, m_N_ALG = N_ALG, m_bInPut = bInPut };
                    alignText = DataGridViewContentAlignment.MiddleLeft;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    //column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            public void AddRow()
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                i = Rows.Add(row);
            }

            /// <summary>
            /// Удалить строки
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                {
                    Rows.Clear();
                }
                else
                    ;
            }

            /// <summary>
            /// Очистить значения
            /// </summary>
            public void ClearValues()
            {
                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > 0) // нельзя удалять идентификатор параметра
                        {
                            c.Value = null;
                        }
                        else
                            ;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);

            }

            /// <summary>
            /// Признак, указывающий принажлежит ли значение строке
            ///  иными словами: отображать ли значение в этой строке
            /// </summary>
            /// <param name="r">Строка (проверяемая) для отображения значения</param>
            /// <param name="value">Значение для отображения в строке</param>
            /// <returns>Признак - результат проверки условия (Истина - отображать/принадлежит)</returns>
            protected bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value)
            {
                return true; // необходимость проверки? способ?
            }

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin_in">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            /// <param name="parametrs">параметры</param>
            public void ShowValues(IEnumerable<HandlerDbTaskCalculate.VALUE> inValues
                , IEnumerable<HandlerDbTaskCalculate.VALUE> outValues, Dictionary<ID_DBTABLE, DataTable> dict_tb_param_in)
            {
                int idAlg = -1
                   , idPut = -1
                   , iCol = 0;
                float fltVal = -1F,
                        fltColumnAgregateValue = 0;
                AGREGATE_ACTION columnAction = AGREGATE_ACTION.UNKNOWN;
                HandlerDbTaskCalculate.IPUT_PARAMETERChange putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();
                IEnumerable<HandlerDbTaskCalculate.VALUE> columnValues = null;

                #region делегат для поиска значений во входных аргументах (для столбца; для ячейки)
                Func<HandlerDbTaskCalculate.VALUE, int, bool> get_values = (HandlerDbTaskCalculate.VALUE value, int id_put) => {
                    return (value.m_IdPut == id_put)
                        && (((value.stamp_value - DateTime.MinValue).TotalDays > 0)
                            || ((!((value.stamp_value - DateTime.MinValue).TotalDays > 0)))
                        );
                };
                #endregion

                #region делегат для отображения значений с попутной установкой значений свойств ячейки, локальных переменных
                Action<DataGridViewCell, HandlerDbTaskCalculate.VALUE, AGREGATE_ACTION> show_value = (DataGridViewCell cell, HandlerDbTaskCalculate.VALUE value, AGREGATE_ACTION c_action) =>
                {
                    fltVal = value.value;
                    //iQuality = value.m_iQuality;

                    if (!(c_action == AGREGATE_ACTION.UNKNOWN))
                        fltColumnAgregateValue += fltVal;
                    else
                        ;

                    //cell.Tag = new CELL_PROPERTY() { m_Value = fltVal, m_iQuality = value.m_iQuality };
                    cell.ReadOnly = Columns[iCol].ReadOnly
                        || double.IsNaN(fltVal);

                        // отобразить с количеством знаков в соответствии с настройками
                    cell.Value = fltVal;
                    cell.ToolTipText = fltVal.ToString();

                    //cell.Style.BackColor = clrCell;
                };
                #endregion



                #region Отображение значений в столбце для обычного параметра
                //try
                //{
                //    putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value;
                //    idPut = putPar.m_Id;

                //    columnValues = inValues.Where(value => get_values(value, idPut));
                //    columnValues = columnValues.Union(outValues.Where(value => get_values(value, idPut)));

                //    idAlg = putPar.m_idNAlg;
                //}
                //catch (Exception e)
                //{
                //    Logging.Logg().Exception(e, @"DataGridViewValues::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                //}

                //if ((putPar.IdComponent > 0)
                //    && (!(columnValues == null)))
                //{
                //    columnAction = ((COLUMN_TAG)col.Tag).ActionAgregateCancel == true ? AGREGATE_ACTION.UNKNOWN : getColumnAction(idAlg);

                //    foreach (DataGridViewRow r in Rows)
                //    {
                //        if (columnValues.Count() > 0)
                //            // есть значение хотя бы для одной строки
                //            foreach (HandlerDbTaskCalculate.VALUE value in columnValues)
                //            {
                //                if (isRowToShowValues(r, value) == true)
                //                {
                //                    show_value(r.Cells[iCol]
                //                        , value
                //                        , columnAction);
                //                }
                //                else
                //                    r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                //            }
                //        else
                //        {
                //            // нет значений ни для одной строки
                //            r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                //        }
                //    } // цикл по строкам

                //    if (!(columnAction == AGREGATE_ACTION.UNKNOWN))
                //    {
                //        fltColumnAgregateValue = GetValueCellAsRatio(idAlg, idPut, fltColumnAgregateValue);

                //        Rows[Rows.Count - 1].Cells[iCol].Value =
                //            fltColumnAgregateValue;
                //    }
                //    else
                //        ;
                //}
                //else
                //    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - не найдено ни одного значения для [ID_PUT={0}] в наборе данных [COUNT={1}] для отображения..."
                //            , ((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).m_Id, inValues.Count())
                //        , Logging.INDEX_MESSAGE.NOT_SET);
                #endregion

                #region comment

                //component = (HandlerDbTaskCalculate.TECComponent)((COLUMN_TAG)col.Tag).value;

                //if (!(component.m_iType == HandlerDbTaskCalculate.TECComponent.TYPE.UNKNOWN))
                //{
                //    //columnAction = ((COLUMN_TAG)col.Tag).ActionAgregateCancel == true ? AGREGATE_ACTION.UNKNOWN : getColumnAction(idAlg);

                //    foreach (DataGridViewRow r in Rows)
                //    {
                //        idAlg = (int)r.Tag;

                //        putPar = m_dictNAlgProperties.FirstPutParameter(idAlg, component.m_Id);

                //        if (!(putPar == null))
                //        {
                //            idPut = putPar.m_Id;

                //            columnValues = inValues.Where(value => get_values(value, idPut));
                //            columnValues = columnValues.Union(outValues.Where(value => get_values(value, idPut)));
                //        }
                //        else
                //            ;

                //        if (columnValues.Count() > 0)
                //            // есть значение хотя бы для одной строки
                //            foreach (HandlerDbTaskCalculate.VALUE value in columnValues)
                //            {
                //                show_value(r.Cells[iCol]
                //                    , value
                //                    , AGREGATE_ACTION.UNKNOWN);

                //                //r.Cells[iCol].Style.Format = m_dictNAlgProperties[idAlg].FormatRound;
                //            }
                //        else
                //            ;
                //    }
                //}
                //else
                //    ;

                #endregion



                // почему "1"? т.к. предполагается, что в наличии минимальный набор: "строка с данными" + "итоговая строка"
                if (RowCount > 1)
                {
                    // отменить обработку события - изменение значения в ячейке представления
                    //activateCellValue_onChanged(false);

                    foreach (DataGridViewColumn col in Columns)
                    {
                        iCol = col.Index;
                        fltColumnAgregateValue = 0F;

                        try
                        {
                            putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value;
                            idPut = putPar.m_Id;

                            columnValues = inValues.Where(value => get_values(value, idPut));
                            //columnValues = columnValues.Union(outValues.Where(value => get_values(value, idPut)));

                            idAlg = putPar.m_idNAlg;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"DataGridViewValues::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }

                        foreach (DataGridViewRow r in Rows)
                        {
                            if (columnValues.Count() > 0)
                                // есть значение хотя бы для одной строки
                                foreach (HandlerDbTaskCalculate.VALUE value in columnValues)
                                {
                                    if (isRowToShowValues(r, value) == true)
                                    {
                                        show_value(r.Cells[iCol]
                                            , value
                                            , columnAction);
                                    }
                                    else
                                    {
                                        //r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                    }
                                }
                            else
                            {
                                // нет значений ни для одной строки
                                //r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                            }
                        } // цикл по строкам

                        if (!(col.Tag == null))
                            switch (false) //(((COLUMN_TAG)col.Tag).Type)
                            {
                                //case TYPE_COLUMN_TAG.COMPONENT:
                                //    break;
                                //case TYPE_COLUMN_TAG.PUT_PARAMETER:
                                //case TYPE_COLUMN_TAG.GROUPING_PARAMETR:
                                //    break;
                                //case TYPE_COLUMN_TAG.FORMULA_HELPER:
                                //    break;
                                //default: // для столбца указан не известный тип идентификатора ('tag')
                                //    Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::ShowValues () - {0}-неизвестный тип идентификатора столбца ...", col.Tag.GetType().FullName), Logging.INDEX_MESSAGE.NOT_SET);
                                //    break;
                            }
                        else
                            // для столбца не указан идентификатор ('tag')
                            Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::ShowValues () - не укахан идентификатор столбца ..."), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    // восстановить обработку события - изменение значение в ячейке
                    //activateCellValue_onChanged(true);
                }
                else
                    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - нет строк для отображения..."), Logging.INDEX_MESSAGE.NOT_SET);






                //DataRow[] row_comp;
                //DataRow[] row_val;
                //double[] agr = new double[Columns.Count];

                //foreach (HDataGridViewColumn col in Columns)
                //{
                //    if (col.Index > 0)
                //        foreach (DataGridViewRow row in Rows)
                //        {
                //            row_comp = dict_tb_param_in[ID_DBTABLE.IN_PARAMETER].Select("N_ALG="
                //                + col.m_N_ALG
                //                + " AND ID_COMP=" + row.HeaderCell.Value.ToString());

                //            if (col.m_bInPut == true)
                //            {
                //                if (row_comp.Length > 0)
                //                {
                //                    //row_val = (tbOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Select("ID_PUT="
                //                    //    + row_comp[0]["ID"].ToString()));
                //                    row_val = (tbOrigin_in.Select("ID_PUT="
                //                        + row_comp[0]["ID"].ToString()));

                //                    if (row_val.Length > 0)
                //                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                //                    else
                //                        ;

                //                    row.Cells[col.Index].ReadOnly = false;
                //                }
                //                else
                //                    ;
                //            }
                //            else
                //            {
                //                row_comp = dict_tb_param_in[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG="
                //                    + col.m_N_ALG.ToString()
                //                    + " and ID_COMP=" + row.HeaderCell.Value.ToString());

                //                if (row_comp.Length > 0)
                //                {
                //                    row_val = (tbOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD].Select("ID_PUT="
                //                        + row_comp[0]["ID"].ToString()));

                //                    if (row_val.Length > 0)
                //                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                //                    else
                //                        ;
                //                }
                //                else
                //                    ;
                //            }
                //        }
                //    else
                //        // col.Index == 0
                //        ;

                //    if (Rows.Count > 1)
                //        //??? почему "5"
                //        if (Convert.ToInt32(Rows[Rows.Count - 1].HeaderCell.Value) == 5)
                //            Rows[Rows.Count - 1].Cells[0].Value = "Итого";
                //        else
                //            ;
                //    else
                //        ;
                //}
            }

            public void InitializeStruct(DataTable tableInNAlg, DataTable tableOutNAlg, DataTable tableComp, Dictionary<int, object[]> dict_profile, DataTable tableRatio)
            {
                this.CellValueChanged -= new DataGridViewCellEventHandler(cellEndEdit);
                this.Rows.Clear();
                this.Columns.Clear();
                DataRow[] colums_in;
                DataRow[] colums_out;
                DataRow[] rows;
                List<DataRow> col_in = new List<DataRow>();
                List<DataRow> col_out = new List<DataRow>();
                m_dbRatio = tableRatio.Copy();

                switch (m_ViewValues)
                {
                    case INDEX_VIEW_VALUES.Block:

                        rows = tableComp.Select("ID_COMP=1000 or ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.Output:
                        //colums_in = nAlgTable.Select("N_ALG='2'");
                        //colums_out = nAlgOutTable.Select("N_ALG='2'");
                        rows = tableComp.Select("ID_COMP=2000 or ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.TeploBL:
                        //colums_in = nAlgTable.Select("N_ALG='3'");
                        //colums_out = nAlgOutTable.Select("N_ALG='3'");
                        rows = tableComp.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.TeploOP:
                        //colums_in = nAlgTable.Select("N_ALG='4'");
                        //colums_out = nAlgOutTable.Select("N_ALG='4'");
                        rows = tableComp.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.Param:
                        //colums_in = nAlgTable.Select("N_ALG='5'");
                        //colums_out = nAlgOutTable.Select("N_ALG='5'");
                        rows = tableComp.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.PromPlozsh:
                        //colums_in = nAlgTable.Select("N_ALG='6'");
                        //colums_out = nAlgOutTable.Select("N_ALG='6'");
                        rows = tableComp.Select("ID_COMP=3000 or ID_COMP=1");
                        break;
                    default:
                        //colums_in = nAlgTable.Select();
                        //colums_out = nAlgOutTable.Select();
                        rows = tableComp.Select();
                        break;
                }

                foreach (object[] list in dict_profile[(int)m_ViewValues])
                {
                    if ((TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE)list[1] == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
                    {

                        m_dict_ProfileNALG_IN = (Dictionary<string, HTepUsers.DictionaryProfileItem>)list[2];

                        foreach (Double id in (double[])list[0])
                            col_in.Add(tableInNAlg.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                    }
                    else
                        ;

                    if ((TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE)list[1] == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                    {
                        m_dict_ProfileNALG_OUT = (Dictionary<string, HTepUsers.DictionaryProfileItem>)list[2];

                        foreach (Double id in (double[])list[0])
                            col_out.Add(tableOutNAlg.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                    }
                    else
                        ;
                }
                colums_in = col_in.ToArray();
                colums_out = col_out.ToArray();

                this.AddColumn("Компонент", true, "Comp");
                foreach (DataRow c in colums_in)
                {
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), (c["N_ALG"]).ToString(), true);
                }

                foreach (DataRow c in colums_out)
                {
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), (c["N_ALG"]).ToString(), false);
                }

                foreach (DataRow r in rows)
                {
                    this.Rows.Add(new object[this.ColumnCount]);
                    this.Rows[Rows.Count - 1].Cells[0].Value = r["DESCRIPTION"].ToString().Trim();
                    this.Rows[Rows.Count - 1].HeaderCell.Value = r["ID"];
                }

                if (Rows.Count > 1)
                {
                    Rows.RemoveAt(0);
                    this.Rows.Add();
                    this.Rows[Rows.Count - 1].Cells[0].Value = "Итого";
                    this.Rows[Rows.Count - 1].HeaderCell.Value = rows[0]["ID"].ToString().Trim();
                }
                else
                    ;

                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
            }

            /// <summary>
            /// ??? Формирование таблицы вых. значений
            /// </summary>
            /// <param name="editTable">таблица</param>
            /// <param name="dgvView">отображение</param>
            /// <param name="dtOut">таблица с вых.зн.</param>
            public DataTable FillTableValueDay(DataTable editTable, DataGridView dgvView, DataTable dtOut)
            {
                //Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                //string put;
                //double valueToRes;
                //editTable.Rows.Clear();

                //foreach (DataGridViewRow row in dgvView.Rows)
                //{
                //    if (Convert.ToDateTime(row.Cells["Date"].Value) < DateTime.Now.Date)
                //    {
                //        for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
                //        {
                //            put = dtOut.Rows[i]["ID"].ToString();
                //            valueToRes = Convert.ToDouble(row.Cells[namePut.GetValue(i).ToString()].Value) * Math.Pow(10, 6);

                //            editTable.Rows.Add(new object[] 
                //            {
                //                put
                //                , -1
                //                , 1.ToString()
                //                , valueToRes                
                //                , Convert.ToDateTime(row.Cells["Date"].Value.ToString()).ToString(CultureInfo.InvariantCulture)
                //                , i
                //            });
                //        }
                //    }
                //}
                return editTable;
            }
        }
    }
}
