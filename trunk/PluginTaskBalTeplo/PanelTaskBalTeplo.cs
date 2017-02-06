﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using Outlook = Microsoft.Office.Interop.Outlook;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;
using System.Drawing;
using System.Data;
using System.Reflection;

namespace PluginTaskBalTeplo
{
    public class PanelTaskBalTeplo : HPanelTepCommon
    {
        private string m_type_dgv;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="dgvView"></param>
        delegate void delDeviation(int i, DataGridView dgvView);
        /// <summary>
        /// Таблицы со значениями для редактирования входные
        /// </summary>
        protected DataTable[] m_arTableOrigin_in
            , m_arTableEdit_in;

        protected DataTable m_dt_profile;
        /// <summary>
        /// Таблицы со значениями для редактирования выходные
        /// </summary>
        protected DataTable[] m_arTableOrigin_out
            , m_arTableEdit_out;
        
        /// <summary>
        /// 
        /// </summary>
        protected TaskBTCalculate BTCalc;
        /// <summary>
        /// 
        /// </summary>
        public enum INDEX_CALC : int
        {
            UNKNOW = -1,
            CALC,
            CorCALC,
            COUNT
        }
        
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_Block,
            DGV_Output,
            DGV_TeploBL,
            DGV_TeploOP,
            DGV_PromPlozsh,
            DGV_Param,
            DGV_PLANEYAR
                , LABEL_DESC
        }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD // идентификаторы периодов расчетов, использующихся на форме
                ,
            TIMEZONE // идентификаторы (целочисленные, из БД системы) часовых поясов
                //    , ALL_COMPONENT,
                //ALL_NALG // все идентификаторы компонентов ТЭЦ/параметров
                //    , DENY_COMP_CALCULATED,
                //DENY_PARAMETER_CALCULATED // запрещенных для расчета
                //    , DENY_COMP_VISIBLED,
                //DENY_PARAMETER_VISIBLED // запрещенных для отображения
                , COUNT
        }
        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_CONTEXT
        {
            ID_CON = 10
        }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskBalTeploCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskBalTeploCalculate; } }
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        ///// <summary>
        ///// 
        ///// </summary>
        //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        ///// <summary>
        ///// Таблицы со значениями словарных, проектных данных входные
        ///// </summary>
        //protected DataTable[] m_dictTableDictPrj;
        ///// <summary>
        ///// Таблицы со значениями словарных, проектных данных
        ///// </summary>
        //protected DataTable[] m_arTableDictPrjs_out;
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementBalTeplo();
        }
        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        protected DGVAutoBook dgvBlock,
            dgvOutput,
            dgvTeploBL,
            dgvTeploOP,
            dgvPromPlozsh,
            dgvParam;
        /// <summary>
        /// ???
        /// </summary>
        protected ReportsToNSS rptsNSS = new ReportsToNSS();
        /// <summary>
        /// 
        /// </summary>
        protected ReportExcel rptExcel = new ReportExcel();

        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        protected PanelManagementBalTeplo PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();
                else
                    ;

                return _panelManagement as PanelManagementBalTeplo;
            }
        }

        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Отправить", @"Сохранить", @"Загрузить" };

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskBalTeploCalculate();
        }

        /// <summary>
        /// Класс для грида
        /// </summary>
        protected class DGVAutoBook : DataGridView
        {
            private int m_id_dgv;

            private Dictionary<string, HTepUsers.DictElement> m_dict_ProfileNALG_IN
                , m_dict_ProfileNALG_OUT;

            private DataTable m_dbRatio;

            public enum INDEX_TYPE_DGV { Block = 2001, Output = 2002, TeploBL = 2003, TeploOP = 2004, Param = 2005, PromPlozsh = 2006 };
            
            private INDEX_TYPE_DGV m_type_dgv;

            public INDEX_TYPE_DGV Type_DGV
            {
                get
                {
                    return m_type_dgv;
                }
                set
                {
                    m_type_dgv = value;
                }
            }

            public DGVAutoBook(string nameDGV)
            {
                m_dict_ProfileNALG_IN = new Dictionary<string, HTepUsers.DictElement>();
                m_dict_ProfileNALG_OUT = new Dictionary<string, HTepUsers.DictElement>();
                m_dbRatio = new DataTable();

                InitializeComponents(nameDGV);
                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
            }

            private void InitializeComponents(string nameDGV)
            {
                this.Name = nameDGV;
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
                if(((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value!=null)
                    if (double.IsNaN(double.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace('.',','))) == false)
                    {
                        if(((HDataGridViewColumn)Columns[e.ColumnIndex]).m_bInPut==true)
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.ROUND).ToString()]);
                        }
                        else
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.ROUND).ToString()]);
                        }

                        DataRow[] rows_Ratio = m_dbRatio.Select("ID=" + idRatio);

                        if (rows_Ratio.Length>0)
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
            /// 
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
            /// 
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

                double[] agr = new double[Columns.Count];

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.Index != 0)
                        foreach (DataGridViewRow row in Rows)
                        {
                            DataRow[] row_comp = dict_tb_param_in[ID_DBTABLE.IN_PARAMETER].Select("N_ALG="
                                + col.m_N_ALG
                                + " AND ID_COMP=" + row.HeaderCell.Value.ToString());

                            if (col.m_bInPut == true)
                            {
                                if (row_comp.Length > 0)
                                {
                                    DataRow[] row_val = (tbOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Select("ID_PUT="
                                        + row_comp[0]["ID"].ToString()));
                                    if (row_val.Length > 0)
                                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                                    row.Cells[col.Index].ReadOnly = false;
                                }
                            }
                            else
                            {
                                row_comp = dict_tb_param_in[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG="
                                    + col.m_N_ALG.ToString()
                                    + " and ID_COMP=" + row.HeaderCell.Value.ToString());
                                if (row_comp.Length > 0)
                                {
                                    DataRow[] row_val = (tbOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Select("ID_PUT="
                                        + row_comp[0]["ID"].ToString()));
                                    if (row_val.Length > 0)
                                        row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                                }
                            }
                        }
                    if (Rows.Count > 1)
                    {
                        if (Convert.ToInt32(Rows[Rows.Count - 1].HeaderCell.Value) == 5)
                        {
                            Rows[Rows.Count - 1].Cells[0].Value = "Итого";
                        }
                    }
                }
            }

            /// <summary>
            ///Корректировка знач.
            /// </summary>
            /// <param name="rowValue">значение</param>
            /// <param name="namecol">имя столбца</param>
            /// <param name="rowcount">номер строки</param>
            /// <param name="dgv">тек.представление</param>
            /// <returns></returns>
            private double correctingValues(object rowValue
                , string namecol
                , ref bool bflg
                , int rowcount
                , DataGridView dgv)
            {
                double valRes = 0;

                switch (namecol)
                {
                    case "GTP12":
                        valRes = Convert.ToDouble(dgv.Rows[rowcount].Cells["CorGTP12"].Value) + (double)rowValue;
                        bflg = true;
                        break;
                    case "GTP36":
                        valRes = Convert.ToDouble(dgv.Rows[rowcount].Cells["CorGTP36"].Value) + (double)rowValue;
                        bflg = true;
                        break;
                    case "TEC":
                        if (bflg)
                        {
                            valRes = Convert.ToDouble(dgv.Rows[rowcount].Cells["GTP12"].Value) +
                               Convert.ToDouble(dgv.Rows[rowcount].Cells["GTP36"].Value);
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
            /// <param name="month">номер месяца</param>
            private void planInMonth(string value, DateTime date, DataGridView dgvAB)
            {
                float planDay = (Convert.ToSingle(value)
                    / DateTime.DaysInMonth(date.Year, date.AddMonths(-1).Month)) / (float)Math.Pow(10, 6);
                int increment = 0;
                planDay = Convert.ToInt32(planDay.ToString("####"));

                for (int i = 0; i < dgvAB.Rows.Count - 1; i++)
                {
                    increment = increment + Convert.ToInt32(planDay);
                    dgvAB.Rows[i].Cells["PlanSwen"].Value = increment.ToString("####");
                }
                dgvAB.Rows[DateTime.DaysInMonth(date.Year, date.Month - 1) - 1].Cells["PlanSwen"].Value =
                    (Convert.ToSingle(value) / Math.Pow(10, 6)).ToString("####");
            }

            /// <summary>
            /// Редактирование занчений ввиду новых корр. значений
            /// </summary>
            /// <param name="row">номер строки</param>
            /// <param name="value">значение</param>
            /// <param name="view">грид</param>
            /// <param name="col">имя столбца</param>
            public void editCells(int row, int value, DataGridView view, string col)
            {

            }

            /// <summary>
            /// Вычисление параметров нараст.ст.
            /// и заполнение грида
            /// </summary>
            /// <param name="i">номер строки</param>
            /// <param name="dgvView">отображение</param>
            private void fillCells(int i, DataGridView dgvView)
            {

            }

            /// <summary>
            /// Вычисление отклонения от плана
            /// </summary>
            /// <param name="i">номер строки</param>
            /// <param name="dgvView">отображение</param>
            public void countDeviation(int i, DataGridView dgvView)
            {
                if (dgvView.Rows[i].Cells["StSwen"].Value == null)
                    dgvView.Rows[i].Cells["DevOfPlan"].Value = "";
                else
                    dgvView.Rows[i].Cells["DevOfPlan"].Value =
                        Convert.ToSingle(dgvView.Rows[i].Cells["StSwen"].Value) - Convert.ToInt32(dgvView.Rows[i].Cells["PlanSwen"].Value);
            }

            /// <summary>
            /// Корректировочные значения
            /// </summary>
            /// <param name="dtOrigin">таблица</param>
            /// <param name="date">дата</param>
            private DataRow[] formingCorrValue(DataTable[] dtOrigin, string date)
            {
                DataRow[] dr_idCorPut = new DataRow[dtOrigin.Count()];

                var m_enumResIDPUT = (from r in dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    if (date == m_enumResIDPUT.ElementAt(i).DATE_TIME.ToShortDateString())
                    {
                        dr_idCorPut = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Select(
                         String.Format(dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Locale
                         , "WR_DATETIME = '{0:o}'", m_enumResIDPUT.ElementAt(i).DATE_TIME));
                        break;
                    }
                    else ;

                }
                return dr_idCorPut;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="editTable">таблица значений</param>
            /// <param name="dgvView">отображение</param>
            /// <param name="value">значение</param>
            /// <param name="column">столбец</param>
            /// <param name="row">строка</param>
            public DataTable FillTableCorValue(DataTable editTable
                , DataGridView dgvView
                , object value
                , int column
                , int row)
            {
                double valueToRes;
                string N_ALG = "";
                editTable.Rows.Clear();
                HDataGridViewColumn cols = (HDataGridViewColumn)dgvView.Columns[column];

                for (int i = 0; i < dgvView.Rows.Count; i++)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (double.Parse(col.m_N_ALG) > 0)
                        {
                            if (cols.m_N_ALG == col.m_N_ALG &&
                                dgvView.Rows[i].Cells["Date"].Value == dgvView.Rows[row].Cells["Date"].Value)
                            {
                                valueToRes = Convert.ToDouble(value) * Math.Pow(10, 6);
                                N_ALG = cols.m_N_ALG;
                            }
                            else
                                if (dgvView.Rows[i].Cells[col.Index].Value != null)
                                {
                                    valueToRes = Convert.ToDouble(dgvView.Rows[i].Cells[col.Index].Value) * Math.Pow(10, 6);
                                    N_ALG = col.m_N_ALG;
                                }
                                else
                                    valueToRes = -1;

                            //-1 не нужно записывать значение в таблицу
                            if (valueToRes > -1)
                                editTable.Rows.Add(new object[] 
                                {
                                    N_ALG
                                    , -1
                                    , 1.ToString()
                                    , valueToRes                
                                    , Convert.ToDateTime(dgvView.Rows[i].Cells["Date"].Value.ToString()).ToString(CultureInfo.InvariantCulture)
                                    , i
                                });
                        }
                    }
                }
                return editTable;
            }

            /// <summary>
            /// Формирование таблицы корр. значений
            /// </summary>
            /// <param name="editTable">таблица</param>
            /// <param name="dgvView">отображение</param>
            /// <returns>таблица значений</returns>
            public DataTable FillTableCorValue(DataTable editTable, DataGridView dgvView)
            {
                //double valueToRes;
                //editTable.Rows.Clear();

                //for (int i = 0; i < dgvView.Rows.Count; i++)
                //{
                //    foreach (HDataGridViewColumn col in Columns)
                //    {
                //        if (col.m_iIdComp > 0)
                //        {
                //            if (dgvView.Rows[i].Cells[col.Index].Value != null)
                //                valueToRes = Convert.ToDouble(dgvView.Rows[i].Cells[col.Index].Value) * Math.Pow(10, 6);
                //            else
                //                valueToRes = -1;

                //            if (valueToRes > -1)
                //                editTable.Rows.Add(new object[] 
                //                {
                //                    col.m_iIdComp
                //                    , -1
                //                    , 1.ToString()
                //                    , valueToRes                
                //                    , Convert.ToDateTime(dgvView.Rows[i].Cells["Date"].Value.ToString()).ToString(CultureInfo.InvariantCulture)
                //                    , i
                //                });
                //        }
                //    }
                //}
                return editTable;
            }

            /// <summary>
            /// Формирование таблицы вых. значений
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

            public void InitializeStruct(DataTable nAlgTable, DataTable nAlgOutTable, DataTable compTable, Dictionary<int, object[]> dict_profile, DataTable db_ratio)
            {
                this.CellValueChanged -= new DataGridViewCellEventHandler(cellEndEdit);
                this.Rows.Clear();
                this.Columns.Clear();
                DataRow[] colums_in;
                DataRow[] colums_out;
                DataRow[] rows;
                List<DataRow> col_in = new List<DataRow>();
                List<DataRow> col_out = new List<DataRow>();
                m_dbRatio = db_ratio.Copy();
                switch (m_type_dgv)
                {
                    case INDEX_TYPE_DGV.Block:

                        rows = compTable.Select("ID_COMP=1000 or ID_COMP=1");
                        break;
                    case INDEX_TYPE_DGV.Output:
                        //colums_in = nAlgTable.Select("N_ALG='2'");
                        //colums_out = nAlgOutTable.Select("N_ALG='2'");
                        rows = compTable.Select("ID_COMP=2000 or ID_COMP=1");
                        break;
                    case INDEX_TYPE_DGV.TeploBL:
                        //colums_in = nAlgTable.Select("N_ALG='3'");
                        //colums_out = nAlgOutTable.Select("N_ALG='3'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_TYPE_DGV.TeploOP:
                        //colums_in = nAlgTable.Select("N_ALG='4'");
                        //colums_out = nAlgOutTable.Select("N_ALG='4'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_TYPE_DGV.Param:
                        //colums_in = nAlgTable.Select("N_ALG='5'");
                        //colums_out = nAlgOutTable.Select("N_ALG='5'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_TYPE_DGV.PromPlozsh:
                        //colums_in = nAlgTable.Select("N_ALG='6'");
                        //colums_out = nAlgOutTable.Select("N_ALG='6'");
                        rows = compTable.Select("ID_COMP=3000 or ID_COMP=1");
                        break;
                    default:
                        //colums_in = nAlgTable.Select();
                        //colums_out = nAlgOutTable.Select();
                        rows = compTable.Select();
                        break;
                }

                foreach (object[] list in dict_profile[(int)m_type_dgv])
                {
                    if (list[1].ToString() == "in")
                    {

                        m_dict_ProfileNALG_IN = (Dictionary<string,HTepUsers.DictElement>)list[2];

                        foreach (Double id in (double[])list[0])
                        {
                            col_in.Add(nAlgTable.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                        }
                    }
                    if (list[1].ToString() == "out")
                    {
                        m_dict_ProfileNALG_OUT = (Dictionary<string, HTepUsers.DictElement>)list[2];

                        foreach (Double id in (double[])list[0])
                        {
                            col_out.Add(nAlgOutTable.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                        }
                    }
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
                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
            }
        }

        /// <summary>
        /// калькулятор значений
        /// </summary>
        public class TaskBTCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            public DataTable[] calcTable;
            /// <summary>
            /// выходные значения
            /// </summary>
            public List<string> value;

            /// <summary>
            /// 
            /// </summary>
            public TaskBTCalculate()
            {
                calcTable = new DataTable[(int)INDEX_CALC.COUNT];
                value = new List<string>((int)INDEX_CALC.COUNT);
            }

            /// <summary>
            /// Суммирование значений ТГ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            /// <returns>отредактированое значение</returns>
            private float sumTG(DataTable tb_gtp)
            {
                float value = 0;
                int pow = 10;

                foreach (DataRow item in tb_gtp.Rows)
                    value = value + Convert.ToSingle(item[@"VALUE"].ToString());

                value = value / (float)Math.Pow(pow, 6);

                return value;
            }

            /// <summary>
            /// разбор данных по гтп
            /// </summary>
            /// <param name="dtOrigin">таблица с данными</param>
            /// /// <param name="dtOut">таблица с параметрами</param>
            public void getTable(DataTable[] dtOrigin, DataTable dtOut)
            {
                int i = 0
                , count = 0;

                calcTable[(int)INDEX_CALC.CALC] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                //
                var m_enumDT = (from r in dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].AsEnumerable()
                                orderby r.Field<DateTime>("WR_DATETIME")
                                select new
                                {
                                    DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                }).Distinct();

                for (int j = 0; j < m_enumDT.Count(); j++)
                {
                    i = 0;
                    calcTable[(int)INDEX_CALC.CALC].Rows.Clear();

                    DataRow[] drOrigin =
                        dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].
                        Select(String.Format(dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Locale
                        , "WR_DATETIME = '{0:o}'", m_enumDT.ElementAt(j).DATE_TIME));

                    foreach (DataRow row in drOrigin)
                    {
                        if (i < 2)
                        {
                            calcTable[(int)INDEX_CALC.CALC].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        }
                        i++;
                    }

                    calculate(calcTable);
                }
            }

            /// <summary>
            /// Вычисление парамтеров ГТП и ТЭЦ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            private void calculate(DataTable[] tb_gtp)
            {

            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Класс по работе с формированием 
        /// и отправкой отчета NSS
        /// </summary>
        public class ReportsToNSS
        {
            //CreateMessage m_crtMsg;

            //public ReportsToNSS()
            //{
            //    m_crtMsg = new CreateMessage();
            //}

            ///// <summary>
            ///// Класс создания письма
            ///// </summary>
            //private class CreateMessage
            //{
            //    /// <summary>
            //    /// 
            //    /// </summary>
            //    Outlook.Application oApp;
            //    /// <summary>
            //    /// конструктор(основной)
            //    /// </summary>
            //    public CreateMessage()
            //    {
            //        oApp = new Outlook.Application();
            //    }

            //    /// <summary>
            //    /// Формирование письма
            //    /// </summary>
            //    /// <param name="subject">тема письма</param>
            //    /// <param name="body">тело сообщения</param>
            //    /// <param name="to">кому/куда</param>
            //    public void FormingMessage(string subject, string body, string to)
            //    {
            //        try
            //        {
            //            Outlook.MailItem newMail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            //            newMail.To = to;
            //            newMail.Subject = subject;
            //            newMail.Body = body;
            //            newMail.Importance = Outlook.OlImportance.olImportanceNormal;
            //            newMail.Display(false);
            //            sendMail(newMail);
            //        }
            //        catch (Exception)
            //        {

            //        }

            //    }

            //    /// <summary>
            //    /// 
            //    /// </summary>
            //    /// <param name="mail"></param>
            //    private void sendMail(Outlook.MailItem mail)
            //    {
            //        //отправка
            //        ((Outlook._MailItem)mail).Send();
            //    }

            //    /// <summary>
            //    /// Прикрепление файла к письму
            //    /// </summary>
            //    /// <param name="mail"></param>
            //    private void AddAttachment(Outlook.MailItem mail)
            //    {
            //        OpenFileDialog attachment = new OpenFileDialog();

            //        attachment.Title = "Select a file to send";
            //        attachment.ShowDialog();

            //        if (attachment.FileName.Length > 0)
            //        {
            //            mail.Attachments.Add(
            //                attachment.FileName,
            //                Outlook.OlAttachmentType.olByValue,
            //                1,
            //                attachment.FileName);
            //        }
            //    }
            //}

            ///// <summary>
            ///// Содание тела сообщения
            ///// </summary>
            ///// <param name="sourceTable">таблица с данными</param>
            ///// <param name="dtRange">выбранный промежуток</param>
            //private void createBodyToSend(ref string sbjct
            //    , ref string bodyMsg
            //    , DataTable sourceTable
            //    , DateTimeRange[] dtRange)
            //{
            //    DataRow[] drReportDay;
            //    DateTime reportDate;

            //    for (int i = 0; i < dtRange.Length; i++)
            //    {
            //        reportDate = dtRange[i].Begin.AddHours(6).Date;
            //        drReportDay =
            //            sourceTable.Select(String.Format(sourceTable.Locale, @"WR_DATETIME = '{0:o}'", reportDate));

            //        if ((double)drReportDay.Length != 0)
            //        {
            //            bodyMsg = @"BEGIN " + "\r\n"
            //                + @"(DATE):" + reportDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "\r\n"
            //                + @"(01): " + fewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ":\r\n"
            //                + @"(02): " + fewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ":\r\n"
            //                + @"(03): " + fewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]) + ":\r\n"
            //                + @"END ";
            //            /*bodyMsg = @"Дата " + reportDate.ToShortDateString() + ".\r\n"
            //                + @"Станция, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ";\r\n"
            //                + @"Блоки 1-2, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ";\r\n"
            //                + @"Блоки 3-6, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]);*/

            //            sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
            //        }
            //    }
            //}

            ///// <summary>
            ///// Редактирование знчения
            ///// </summary>
            ///// <param name="val">значение</param>
            ///// <returns>измененное знач.</returns>
            //private string fewerValue(double val)
            //{
            //    return Convert.ToString(val / Math.Pow(10, 6)).ToString();
            //}

            ///// <summary>
            ///// Создание. Подготвока. Отправка письма.
            ///// </summary>
            ///// <param name="sourceTable">таблица с данными</param>
            ///// <param name="dtRange">выбранный промежуток</param>
            ///// /// <param name="to">получатель</param>
            //public void SendMailToNSS(DataTable sourceTable, DateTimeRange[] dtRange, string to)
            //{
            //    string bodyMsg = string.Empty
            //     , sbjct = string.Empty;

            //    createBodyToSend(ref sbjct, ref bodyMsg, sourceTable, dtRange);

            //    if (sbjct != "")
            //        m_crtMsg.FormingMessage(sbjct, bodyMsg, to);
            //    else ;
            //}
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            ///// <summary>
            ///// Экземпляр класса
            ///// </summary>
            //ExcelFile efNSS;
            ///// <summary>
            ///// 
            ///// </summary>
            //protected enum INDEX_DIVISION : int
            //{
            //    UNKNOW = -1,
            //    SEPARATE_CELL,
            //    ADJACENT_CELL
            //}
            ///// <summary>
            ///// конструктор(основной)
            ///// </summary>
            //public ReportExcel()
            //{
            //    efNSS = new ExcelFile();
            //}

            //public void CreateExcel(DataGridView dgView)
            //{
            //    efNSS.LoadXls(@"D:\MyProjects\C.Net\TEP32\Tep\bin\Debug\Template\TemplateAutobook.xls");
            //    ExcelWorksheet wrkSheets = efNSS.Worksheets["Autobook"];

            //    for (int i = 0; i < wrkSheets.Columns.Count; i++)
            //    {
            //        int indxRow = 0;
            //        bool bflag = false;

            //        foreach (ExcelCell cell in wrkSheets.Columns[i].Cells)
            //        {
            //            for (int j = 0; j < dgView.Columns.Count; j++)
            //            {
            //                if (Convert.ToString(cell.Value) == splitString(dgView.Columns[j].HeaderText))
            //                {
            //                    fillSheetExcel(wrkSheets, dgView, j, indxRow, i);
            //                    bflag = true;
            //                    break;
            //                }
            //                else
            //                    ;
            //            }
            //            indxRow++;
            //            if (bflag == true)
            //                break;
            //        }
            //    }
            //    //wrkSheets.
            //    //efNSS.SaveXls("");
            //    //efNSS.
            //    // Select active worksheet.
            //    //efNSS = wrkSheets.Worksheets.ActiveWorksheet;
            //    //efNSS.Worksheets.ActiveWorksheet;
            //}

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="headerTxt"></param>
            ///// <returns></returns>
            //private string splitString(string headerTxt)
            //{
            //    string[] spltHeader = headerTxt.Split(',');

            //    if (spltHeader.Length > (int)INDEX_DIVISION.ADJACENT_CELL)
            //        return spltHeader[(int)INDEX_DIVISION.ADJACENT_CELL];
            //    else
            //        return spltHeader[(int)INDEX_DIVISION.SEPARATE_CELL];
            //}

            //private void fillSheetExcel(ExcelWorksheet wrkSheet
            //    , DataGridView dgv
            //    , int indxColDgv
            //    , int indxRowExcel
            //    , int indxColExcel)
            //{
            //    CellRange cellRange = wrkSheet.Columns[indxColExcel].Cells;

            //    for (int i = 0; i < dgv.Rows.Count; i++)
            //    {
            //        for (int j = indxRowExcel + 2; j < wrkSheet.Rows.Count; j++)
            //        {
            //            cellRange[j].Value = dgv.Rows[i].Cells[indxColDgv].Value;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskBalTeplo(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.BAL_TEPLO;
            BTCalc = new TaskBTCalculate();
            m_dt_profile = new DataTable();

            m_arTableOrigin_in = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit_in = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            m_arTableOrigin_out = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit_out = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();

            Session.SetDatetimeRange(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
            PanelManagement.CheckedChangedRadioBtnEvent += new PanelManagementBalTeplo.CheckedChangedRadioBtnEventHandler(CheckedChangedRadioBtn);
        }

        private void CheckedChangedRadioBtn(object sender, PanelManagementBalTeplo.CheckedChangedRadioBtnEventArgs e)
        {
            m_type_dgv = ((PanelManagementBalTeplo.RadioButton_BalTask)(sender)).Type;
            if (m_type_dgv == PanelManagementBalTeplo.TypeRadioBtn.Block.ToString())
            {
                dgvOutput.Visible = false;
                dgvTeploOP.Visible = false;
                dgvParam.Visible = false;
                dgvPromPlozsh.Visible = false;
                dgvBlock.Visible = true;
                dgvTeploBL.Visible = true;
            }
            if (m_type_dgv == PanelManagementBalTeplo.TypeRadioBtn.Teplo.ToString())
            {
                dgvBlock.Visible = false;
                dgvTeploBL.Visible = false;
                dgvParam.Visible = false;
                dgvPromPlozsh.Visible = false;
                dgvTeploOP.Visible = true;
                dgvOutput.Visible = true;
            }
            if (m_type_dgv == PanelManagementBalTeplo.TypeRadioBtn.PromPlozsh.ToString())
            {
                dgvBlock.Visible = false;
                dgvOutput.Visible = false;
                dgvTeploBL.Visible = false;
                dgvTeploOP.Visible = false;
                dgvParam.Visible = true;
                dgvPromPlozsh.Visible = true;
            }
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <param name="numMonth">номер месяца</param>
        /// <returns>кол-во дней</returns>
        public int DayIsMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// Панель элементов
        /// </summary>
        protected class PanelManagementBalTeplo : PanelManagementTaskCalculate //HPanelCommon
        {
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1
                    , BUTTON_SEND, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT,
                TXTBX_EMAIL,
                MENUITEM_UPDATE,
                MENUITEM_HISTORY,
                RADIO_BLOCK,
                RADIO_TEPLO,
                RADIO_PROM_PLOZSH,
                COUNT
            }

            public enum TypeRadioBtn { Block, Teplo, PromPlozsh };

            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            public PanelManagementBalTeplo()
                : base() //6, 8
            {
                InitializeComponents();

                (Controls.Find(INDEX_CONTROL.RADIO_BLOCK.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);
                (Controls.Find(INDEX_CONTROL.RADIO_TEPLO.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);
                (Controls.Find(INDEX_CONTROL.RADIO_PROM_PLOZSH.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);

            }

            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                //int posColdgvTEPValues = 6;
                SuspendLayout();

                posRow = 0;
                //Период расчета
                //Период расчета - подпись, значение
                SetPositionPeriod(new Point(0, posRow), new Size(this.ColumnCount / 2, 1));

                //Период расчета - подпись, значение
                SetPositionTimezone(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount / 2, 1));

                //Дата/время начала периода расчета
                posRow = SetPositionDateTimePicker(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount, 4));

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка - импортировать
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                ctrlBSend.Visible = false;
                //ctrlBSend.Enabled = false;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;
                ctrlExp.Visible = false;
                //Поле с почтой
                TextBox ctrlTxt = new TextBox();
                ctrlTxt.Name = INDEX_CONTEXT.ID_CON.ToString();
                //ctrlTxt.Text = @"Pasternak_AS@sibeco.su";
                ctrlTxt.Dock = DockStyle.Top;
                ctrlTxt.Visible = false;

                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBSend, 1, 0);
                tlpButton.Controls.Add(ctrlBsave, 0, 1);
                tlpButton.Controls.Add(ctrlTxt, 1, 1);
                tlpButton.Controls.Add(ctrlExp, 0, 2);
                this.Controls.Add(tlpButton, 0, posRow = posRow + 2);
                this.SetColumnSpan(tlpButton, 4); this.SetRowSpan(tlpButton, 3);

                //
                RadioButton_BalTask ctrlRadioBlock = new RadioButton_BalTask();
                ctrlRadioBlock.Name = INDEX_CONTROL.RADIO_BLOCK.ToString();
                ctrlRadioBlock.Text = @"По блокам";
                ctrlRadioBlock.Type = TypeRadioBtn.Block.ToString();
                ctrlRadioBlock.Dock = DockStyle.Top;
                ctrlRadioBlock.Checked = true;
                //
                RadioButton_BalTask ctrlRadioTeplo = new RadioButton_BalTask();
                ctrlRadioTeplo.Name = INDEX_CONTROL.RADIO_TEPLO.ToString();
                ctrlRadioTeplo.Text = @"По выводам";
                ctrlRadioTeplo.Type = TypeRadioBtn.Teplo.ToString();
                ctrlRadioTeplo.Dock = DockStyle.Top;
                //
                RadioButton_BalTask ctrlRadioProm = new RadioButton_BalTask();
                ctrlRadioProm.Name = INDEX_CONTROL.RADIO_PROM_PLOZSH.ToString();
                ctrlRadioProm.Text = @"Пром. площадки";
                ctrlRadioProm.Type = TypeRadioBtn.PromPlozsh.ToString();
                ctrlRadioProm.Dock = DockStyle.Top;

                TableLayoutPanel tlpRadioBtn = new TableLayoutPanel();
                tlpRadioBtn.Dock = DockStyle.Fill;
                tlpRadioBtn.AutoSize = true;
                tlpRadioBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpRadioBtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpRadioBtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));

                tlpRadioBtn.Controls.Add(ctrlRadioBlock, 0, 0);
                tlpRadioBtn.Controls.Add(ctrlRadioTeplo, 0, 1);
                tlpRadioBtn.Controls.Add(ctrlRadioProm, 0, 2);

                this.Controls.Add(tlpRadioBtn, 0, posRow = posRow + 3);
                this.SetColumnSpan(tlpRadioBtn, 4); this.SetRowSpan(tlpRadioBtn, 3);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;

                if (!(DateTimeRangeValue_Changed == null))
                    DateTimeRangeValue_Changed(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
                else
                    ;
            }

            private void CheckedChangedRadioBtn(object obj, EventArgs e)
            {
                if ((obj as RadioButton_BalTask).Checked == true)
                    if (CheckedChangedRadioBtnEvent != null)
                    {
                        CheckedChangedRadioBtnEvent(obj, new CheckedChangedRadioBtnEventArgs());
                    }
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения значения ячейки
            /// </summary>
            public class CheckedChangedRadioBtnEventArgs : EventArgs
            {
                /// <summary>
                /// Компонента
                /// </summary>
                public object m_Comp;

                public CheckedChangedRadioBtnEventArgs()
                    : base()
                {
                    m_Comp = null;

                }

                public CheckedChangedRadioBtnEventArgs(int comp)
                    : this()
                {
                    m_Comp = comp;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события - изменение значения в ячейке
            /// </summary>
            public delegate void CheckedChangedRadioBtnEventHandler(object obj, CheckedChangedRadioBtnEventArgs e);

            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public CheckedChangedRadioBtnEventHandler CheckedChangedRadioBtnEvent;

            public class RadioButton_BalTask : RadioButton
            {
                protected string m_type;
                public string Type
                {
                    get
                    {
                        return m_type;
                    }
                    set
                    {
                        m_type = value;
                    }
                }
            }
        }

        /// <summary>
        /// инициализация объектов
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvTEPValues = 4;

            SuspendLayout();

            posRow = 0;

            #region DGV
            dgvBlock = new DGVAutoBook(INDEX_CONTROL.DGV_Block.ToString());
            dgvBlock.Dock = DockStyle.Fill;
            dgvBlock.Name = INDEX_CONTROL.DGV_Block.ToString();
            dgvBlock.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.Block;
            dgvBlock.AllowUserToResizeRows = false;
            dgvBlock.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBlock.Visible = true;
            this.Controls.Add(dgvBlock, 4, posRow);
            this.SetColumnSpan(dgvBlock, 9); this.SetRowSpan(dgvBlock, 5);
            //
            dgvOutput = new DGVAutoBook(INDEX_CONTROL.DGV_Output.ToString());
            dgvOutput.Dock = DockStyle.Fill;
            dgvOutput.Name = INDEX_CONTROL.DGV_Output.ToString();
            dgvOutput.AllowUserToResizeRows = false;
            dgvOutput.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.Output;
            dgvOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOutput.Visible = false;
            this.Controls.Add(dgvOutput, 4, posRow);
            this.SetColumnSpan(dgvOutput, 9); this.SetRowSpan(dgvOutput, 5);
            //
            dgvTeploBL = new DGVAutoBook(INDEX_CONTROL.DGV_TeploBL.ToString());
            dgvTeploBL.Dock = DockStyle.Fill;
            dgvTeploBL.Name = INDEX_CONTROL.DGV_TeploBL.ToString();
            dgvTeploBL.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.TeploBL;
            dgvTeploBL.AllowUserToResizeRows = false;
            dgvTeploBL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploBL.Visible = true;
            this.Controls.Add(dgvTeploBL, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploBL, 9); this.SetRowSpan(dgvTeploBL, 5);
            //
            dgvTeploOP = new DGVAutoBook(INDEX_CONTROL.DGV_TeploOP.ToString());
            dgvTeploOP.Dock = DockStyle.Fill;
            dgvTeploOP.Name = INDEX_CONTROL.DGV_TeploOP.ToString();
            dgvTeploOP.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.TeploOP;
            dgvTeploOP.AllowUserToResizeRows = false;
            dgvTeploOP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploOP.Visible = false;
            this.Controls.Add(dgvTeploOP, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploOP, 9); this.SetRowSpan(dgvTeploOP, 5);
            //
            dgvPromPlozsh = new DGVAutoBook(INDEX_CONTROL.DGV_PromPlozsh.ToString());
            dgvPromPlozsh.Dock = DockStyle.Fill;
            dgvPromPlozsh.Name = INDEX_CONTROL.DGV_PromPlozsh.ToString();
            dgvPromPlozsh.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.PromPlozsh;
            dgvPromPlozsh.AllowUserToResizeRows = false;
            dgvPromPlozsh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPromPlozsh.Visible = false;
            this.Controls.Add(dgvPromPlozsh, 4, posRow);
            this.SetColumnSpan(dgvPromPlozsh, 9); this.SetRowSpan(dgvPromPlozsh, 5);
            //
            dgvParam = new DGVAutoBook(INDEX_CONTROL.DGV_Param.ToString());
            dgvParam.Dock = DockStyle.Fill;
            dgvParam.Name = INDEX_CONTROL.DGV_Param.ToString();
            dgvParam.Type_DGV = DGVAutoBook.INDEX_TYPE_DGV.Param;
            dgvParam.AllowUserToResizeRows = false;
            dgvParam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParam.Visible = false;
            this.Controls.Add(dgvParam, 4, posRow + 5);
            this.SetColumnSpan(dgvParam, 9); this.SetRowSpan(dgvParam, 5);
            #endregion
            //
            this.Controls.Add(PanelManagement, 0, posRow);
            this.SetColumnSpan(PanelManagement, posColdgvTEPValues);
            this.SetRowSpan(PanelManagement, posRow = posRow + 9);//this.RowCount);     

            ////
            //TableLayoutPanel tlpYear = new TableLayoutPanel();
            //tlpYear.Dock = DockStyle.Fill;
            //tlpYear.AutoSize = true;
            //tlpYear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            //tlpYear.Controls.Add(lblyearDGV, 0, 0);
            //tlpYear.Controls.Add(lblTEC, 0, 1);
            ////tlpYear.Controls.Add(dgvYear, 0, 2);
            //this.Controls.Add(tlpYear, 0, posRow = posRow + 1);
            //this.SetColumnSpan(tlpYear, 4); this.SetRowSpan(tlpYear, 7);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(btnHistory_OnClick);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskAutobookMonthValue_btnsend_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskAutobookMonthValues_btnexport_Click);


            dgvBlock.CellParsing += dgvCellParsing;
            dgvOutput.CellParsing += dgvCellParsing;
            dgvParam.CellParsing += dgvCellParsing;
            dgvPromPlozsh.CellParsing += dgvCellParsing;
            dgvTeploBL.CellParsing += dgvCellParsing;
            dgvTeploOP.CellParsing += dgvCellParsing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValues_btnexport_Click(object sender, EventArgs e)
        {
            //rptExcel.CreateExcel(dgvAB);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValue_btnsend_Click(object sender, EventArgs e)
        {
            int err = -1;
            string toSend = (Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0] as TextBox).Text;

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err), dgvBlock, HandlerDb.getOutPut(out err));
            //rptsNSS.SendMailToNSS(m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            //, HandlerDb.GetDateTimeRangeValuesVar(), toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dgvCellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            int err = -1;
            int id_put = -1;
            string N_ALG = (((DGVAutoBook)sender).Columns[e.ColumnIndex] as DGVAutoBook.HDataGridViewColumn).m_N_ALG;
            int id_comp = Convert.ToInt32(((DGVAutoBook)sender).Rows[e.RowIndex].HeaderCell.Value);

            if ((((DGVAutoBook)sender).Columns[e.ColumnIndex] as DGVAutoBook.HDataGridViewColumn).m_bInPut == true)
            {
                DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
                if (rows.Length == 1)
                    id_put = Convert.ToInt32(rows[0]["ID"]);
                m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            }
            else
            {
                DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
                if (rows.Length == 1)
                    id_put = Convert.ToInt32(rows[0]["ID"]);
                m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            }
            HandlerDb.RegisterDbConnection(out err);
            HandlerDb.RecUpdateInsertDelete(
                TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name
                , "ID_PUT,ID_SESSION"
                , null
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                , out err
            );
            //HandlerDb.insertInValues(m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);
            HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar (
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , out err
            );
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar(
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES
                , out err
            );
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();

            HandlerDb.UnRegisterDbConnection();

            dgvBlock.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            dgvOutput.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            dgvTeploBL.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            dgvTeploOP.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            dgvParam.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            dgvPromPlozsh.ShowValues(m_arTableEdit_in, m_arTableEdit_out
                , m_dictTableDictPrj);
            ((DGVAutoBook)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = e.Value;
        }
        
        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            deleteSession();

            base.Stop();
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr"></param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            ////изменение начальной даты
            //if (arQueryRanges.Count() > 1)
            //    arQueryRanges[1] = new DateTimeRange(arQueryRanges[1].Begin.AddDays(-(arQueryRanges[1].Begin.Day - 1))
            //        , arQueryRanges[1].End.AddDays(-(arQueryRanges[1].End.Day - 2)));
            //else
            //    arQueryRanges[0] = new DateTimeRange(arQueryRanges[0].Begin.AddDays(-(arQueryRanges[0].Begin.Day - 1))
            //        , arQueryRanges[0].End.AddDays(DayIsMonth - arQueryRanges[0].End.Day));
            
            //Запрос для получения архивных данных
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.INVALUES, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , HandlerDb.ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Merge(HandlerDb.GetValuesDayVar
                (
                Type
                , HandlerDb.ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                ));

            //Получение значений по-умолчанию input
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.INVALUES, out err);

            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.OUTVALUES, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , HandlerDb.ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.OUTVALUES, out err);

            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , CountBasePeriod
                        , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
                        , ref m_arTableOrigin_in
                        , ref m_arTableOrigin_out
                        , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                        , out err, out strErr);
                else
                    strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
                        + @" по " + Session.m_rangeDatetime.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
                    + @" по " + Session.m_rangeDatetime.End.ToString();
        }

        /// <summary>
        /// copy
        /// </summary>
        private void setValues()
        {
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Copy();
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                = m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Copy();
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                = m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1
                , iRegDbConn = -1;
            string errMsg = string.Empty;

            m_handlerDb.RegisterDbConnection(out iRegDbConn);
            clear();

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg);

                if (err == 0)
                {
                    if (m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        //setValues();
                        //вычисление значений
                        HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
                        m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                            (
                            TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES,
                            out err
                            );
                        m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                            (
                            TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES,
                            out err
                            );
                        setValues();

                        dgvBlock.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        dgvOutput.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        dgvTeploBL.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        dgvTeploOP.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        dgvParam.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        dgvPromPlozsh.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_dictTableDictPrj);
                        ////сохранить вых. знач. в DataTable
                        //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                        //    dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err)
                        //       , dgvBlock
                        //       , HandlerDb.getOutPut(out err));
                        ////сохранить вых.корр. знач. в DataTable
                        //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                        //    dgvBlock.FillTableCorValue(HandlerDb.OutValues(out err), dgvBlock);
                    }
                    else ;
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskBalTeplo::updatedataValues() - " + errMsg);
                }
                //удалить сессию
                //deleteSession();
            }
            else
                ;

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
            else
                ;
        }

        /// <summary>
        /// Количество базовых периодов
        /// </summary>
        protected int CountBasePeriod
        {
            get
            {
                int iRes = -1;
                ID_PERIOD idPeriod = HandlerDb.ActualIdPeriod;

                iRes =
                    idPeriod == ID_PERIOD.HOUR ?
                        (int)(Session.m_rangeDatetime.End - Session.m_rangeDatetime.Begin).TotalHours - 0 :
                        idPeriod == ID_PERIOD.DAY ?
                            (int)(Session.m_rangeDatetime.End - Session.m_rangeDatetime.Begin).TotalDays - 0 :
                            24
                            ;

                return iRes;
            }
        }

        /// <summary>
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void btnHistory_OnClick(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskBalTeploCalculate.SESSION.INDEX_VIEW_VALUES.ARCHIVE;

            onButtonLoadClick();
        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            updateDataValues();
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Загрузить" (кнопка - аналог "Обновить")
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? кнопка или п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskBalTeploCalculate.SESSION.INDEX_VIEW_VALUES.SOURCE;

            onButtonLoadClick();

        }
        /// <summary>
        /// 
        /// </summary>
        protected System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activate"></param>
        /// <returns></returns>
        public override bool Activate(bool activate)
        {
            bool bRes = false;
            int err = -1;

            bRes = base.Activate(activate);

            if (bRes == true)
            {
                if (activate == true)
                    HandlerDb.InitSession(out err);
                else
                    ;
            }
            else
                ;

            return bRes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <param name="errMsg"></param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];
            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id)
                {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }

            m_dictTableDictPrj = new DataTable[(int)ID_DBTABLE.COUNT];
            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            int i = -1;
            string strItem = string.Empty;
            initialize(new ID_DBTABLE[] {
                ID_DBTABLE.PERIOD, ID_DBTABLE.TIMEZONE, ID_DBTABLE.COMP_LIST, ID_DBTABLE.MEASURE, ID_DBTABLE.RATIO
                , ID_DBTABLE.INALG, ID_DBTABLE.OUTALG, }
                , out err, out errMsg
            );

            if (err == 0) {
                try {
                    //??? m_dt_profile = HandlerDb.GetProfilesContext(m_id_panel);
                    dgvBlock.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvBlock.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvOutput.InitializeStruct(m_dictTableDictPrj[(ID_DBTABLE.INALG)], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvOutput.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvTeploBL.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvTeploBL.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvTeploOP.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvTeploOP.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvPromPlozsh.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvPromPlozsh.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvParam.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvParam.Type_DGV), m_dictTableDictPrj[ID_DBTABLE.RATIO]);

                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE], (ID_TIMEZONE)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.TIMEZONE).ToString()]));
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.PERIOD], (ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]))); //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]));
                    PanelManagement.SetModeDatetimeRange();

                    ctrl = Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0];
                    //из profiles
                    for (int j = 0; j < m_dt_profile.Rows.Count; j++)
                        if (Convert.ToInt32(m_dt_profile.Rows[j]["CONTEXT"]) == (int)INDEX_CONTEXT.ID_CON)
                            ctrl.Text = m_dt_profile.Rows[j]["VALUE"].ToString().TrimEnd();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), errMsg, Logging.INDEX_MESSAGE.NOT_SET);
        }
        
        private Dictionary<int, object[]> GetProfileDGV(int id_dgv)
        {
            Dictionary<int, object[]> dict_profile = new Dictionary<int, object[]>();
            string value = string.Empty;
            string[] contexts = {"33","34"};
            string[] id;
            List<double> ids = new List<double>();
            string type = string.Empty;
                        
            List<object> obj = new List<object>();

            foreach (string context in contexts)
            {
                Dictionary<string, HTepUsers.DictElement> dictParamNALG = new Dictionary<string, HTepUsers.DictElement>();
                value = m_dictProfile.Objects[id_dgv.ToString()].Objects[context].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.INPUT_PARAM).ToString()];

                id = value.Trim().Split(';');
                ids.Clear();
                if (id.Length > 0)
                {
                    foreach (string str in id)
                    {
                        ids.Add(Convert.ToDouble(str.Replace('.', ',')));
                    }
                }

                if (Convert.ToInt32(context) == 33)
                {
                    type = "in";
                }
                if (Convert.ToInt32(context) == 34)
                {
                    type = "out";
                }

                obj.Add(new object[] { ids.ToArray(), type, m_dictProfile.Objects[context].Objects });
            }
            dict_profile.Add(id_dgv, obj.ToArray());


            return dict_profile;
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            setCurrentTimeZone(obj as ComboBox);
            // очистить содержание представления
            clear();
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }

        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            // очистить содержание представления
            clear();
            Session.SetDatetimeRange(dtBegin, dtEnd);
            //заполнение представления
            //fillDaysGrid(dtBegin, dtBegin.Month);
        }

        /// <summary>
        /// заполнение грида датами
        /// </summary>
        /// <param name="date">тек.дата</param>
        /// <param name="numMonth">номер месяца</param>
        private void fillDaysGrid(DateTime date, int numMonth)
        {
            DateTime dt = new DateTime(date.Year, date.Month, 1);
            dgvBlock.ClearRows();

            for (int i = 0; i < DayIsMonth; i++)
            {
                dgvBlock.AddRow();
                dgvBlock.Rows[i].Cells[0].Value = dt.AddDays(i).ToShortDateString();
            }
            dgvBlock.Rows[date.Day - 1].Selected = true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bClose"></param>
        protected override void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true) {
                dgvBlock.ClearRows();
                dgvOutput.ClearRows();
                dgvTeploBL.ClearRows();
                dgvTeploOP.ClearRows();
                dgvParam.ClearRows();
                dgvPromPlozsh.ClearRows();
            } else {
                // очистить содержание представления
                dgvBlock.ClearValues();
                dgvOutput.ClearValues();
                dgvTeploBL.ClearValues();
                dgvTeploOP.ClearValues();
                dgvParam.ClearValues();
                dgvPromPlozsh.ClearValues();
            }

            base.clear(bClose);
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            int idTimezone = m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex];

            Session.SetCurrentTimeZone((ID_TIMEZONE)idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod(PanelManagement.IdPeriod);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            PanelManagement.SetModeDatetimeRange();
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);

            // очистить содержание представления
            clear();
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementBalTeplo.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                        PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                    else
                        ;
            else
                throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }

        /// <summary>
        /// Сохранение значений в БД
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.saveResInval(getStructurOutval(out err)
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.saveResOut(getStructurOutval(out err)
            , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

            base.HPanelTepCommon_btnSave_Click(obj, ev);
        }

        /// <summary>
        /// получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(out int err)
        {
            string strRes = string.Empty;
            DataTable res = new DataTable();

            strRes = "SELECT * FROM "
                + GetNameTableOut(PanelManagement.DatetimeRange.Begin);

            res = HandlerDb.Select(strRes, out err).Clone();
            res.Columns.Remove("ID");
            return res;
        }

        /// <summary>
        /// Получение имени таблицы вых.зн. в БД
        /// </summary>
        /// <param name="dtInsert">дата</param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableOut(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;

            strRes = TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.OUTVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Получение имени таблицы вх.зн. в БД
        /// </summary>
        /// <param name="dtInsert"></param>
        /// <returns>имя таблицы</returns>
        public string GetNameTableIn(DateTime dtInsert)
        {
            string strRes = string.Empty;

            if (dtInsert == null)
                throw new Exception(@"PanelTaskAutobook::GetNameTable () - невозможно определить наименование таблицы...");
            else
                ;

            strRes = TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(GetNameTableIn(
                PanelManagement.DatetimeRange.Begin)
                , @"ID_PUT, DATE_TIME, ID_USER, ID_SOURCE"
                , @""
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                , out err
            );

            m_handlerDb.RecUpdateInsertDelete(
                GetNameTableOut(PanelManagement.DatetimeRange.Begin)
                , @"ID_PUT, DATE_TIME, ID_USER, ID_SOURCE"
                , @""
                , m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]
                , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                , out err
            );
        }

        /// <summary>
        /// Сохранение выходных знчений(План ТЭЦ)
        /// </summary>
        /// <param name="err"></param>
        private void saveInvalValue(out int err)
        {
            err = -1;
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                HandlerDb.getInPut(Type, dtrPer, HandlerDb.ActualIdPeriod, out err);

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            HandlerDb.saveResInval(m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

            m_handlerDb.RecUpdateInsertDelete(
                GetNameTableIn(PanelManagement.DatetimeRange.Begin)
                , @"ID_PUT, DATE_TIME"
                , @"ID"
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , out err
            );
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
               m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
               m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>key</returns>
        private int findMyID()
        {
            int Res = 0;
            Dictionary<int, Type> dictRegId = (_iFuncPlugin as PlugInBase).GetRegisterTypes();

            foreach (var item in dictRegId)
                if (item.Value == this.GetType())
                    Res = item.Key;

            return Res;
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 19;
            register(19, typeof(PanelTaskBalTeplo), @"Задача", @"Баланс тепла");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

