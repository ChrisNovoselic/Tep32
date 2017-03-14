using HClassLibrary;
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
                            idRatio = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.INDEX_PROFILE.RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.INDEX_PROFILE.ROUND).ToString()]);
                        }
                        else
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.INDEX_PROFILE.RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.INDEX_PROFILE.ROUND).ToString()]);
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
                DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
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
                }
                catch (Exception e)
                {
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
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_N_ALG = N_ALG, m_bInPut = bInPut };
                    alignText = DataGridViewContentAlignment.MiddleLeft;
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
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin_in">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            /// <param name="parametrs">параметры</param>
            public void ShowValues(DataTable[] tbOrigin_in, DataTable[] tbOrigin_out, Dictionary<ID_DBTABLE, DataTable> dict_tb_param_in)
            {
                DataRow[] row_comp;
                DataRow[] row_val;
                double[] agr = new double[Columns.Count];

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.Index > 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            row_comp = dict_tb_param_in[ID_DBTABLE.IN_PARAMETER].Select("N_ALG="
                                + col.m_N_ALG
                                + " AND ID_COMP=" + row.HeaderCell.Value.ToString());

                            if (col.m_bInPut == true)
                            {
                                if (row_comp.Length > 0)
                                {
                                    row_val = (tbOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT="
                                        + row_comp[0]["ID"].ToString()));

                                    if (row_val.Length > 0)
                                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                                    else
                                        ;

                                    row.Cells[col.Index].ReadOnly = false;
                                }
                                else
                                    ;
                            }
                            else
                            {
                                row_comp = dict_tb_param_in[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG="
                                    + col.m_N_ALG.ToString()
                                    + " and ID_COMP=" + row.HeaderCell.Value.ToString());

                                if (row_comp.Length > 0)
                                {
                                    row_val = (tbOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT="
                                        + row_comp[0]["ID"].ToString()));

                                    if (row_val.Length > 0)
                                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                                    else
                                        ;
                                }
                                else
                                    ;
                            }
                        }
                    else
                        // col.Index == 0
                        ;

                    if (Rows.Count > 1)
                        //??? почему "5"
                        if (Convert.ToInt32(Rows[Rows.Count - 1].HeaderCell.Value) == 5)
                            Rows[Rows.Count - 1].Cells[0].Value = "Итого";
                        else
                            ;
                    else
                        ;
                }
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
