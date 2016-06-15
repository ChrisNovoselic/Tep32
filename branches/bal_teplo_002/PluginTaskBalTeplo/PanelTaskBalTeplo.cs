using System;
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
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected INDEX_VIEW_VALUES m_ViewValues;
        /// <summary>
        /// 
        /// </summary>
        protected TaskBTCalculate BTCalc;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            ,PERIOD
            ,TIMEZONE
            ,COMPONENT
            ,PARAMETER //_IN
            ,PARAMETER_OUT
            ,MODE_DEV/*, MEASURE*/
            ,RATIO
            ,N_ALG
            ,N_ALG_OUT
            ,COUNT
        }
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
        /// Перечисление - признак типа загруженных из БД значений
        ///  "сырые" - от источников информации, "архивные" - сохраненные в БД
        /// </summary>
        protected enum INDEX_VIEW_VALUES : short
        {
            UNKNOWN = -1, SOURCE,
            ARCHIVE, COUNT
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
        /// Значения параметров сессии
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected TaskBalTeploCalculate HandlerDb { get { return m_handlerDb as TaskBalTeploCalculate; } }
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных входные
        /// </summary>
        protected DataTable[] m_arTableDictPrjs_in;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs_out;
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        private PanelManagementBalTeplo createPanelManagement()
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
        /// 
        /// </summary>
        protected ReportsToNSS rptsNSS = new ReportsToNSS();
        /// <summary>
        /// 
        /// </summary>
        protected ReportExcel rptExcel = new ReportExcel();

        private PanelManagementBalTeplo _panelManagement;
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

                return _panelManagement;
            }
        }

        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Отправить", @"Сохранить", @"Загрузить" };

        protected override HandlerDbValues createHandlerDb()
        {
            return new TaskBalTeploCalculate();
        }

        /// <summary>
        /// Класс для грида
        /// </summary>
        protected class DGVAutoBook : DataGridView
        {
            private int m_id_dgv;
            public enum INDEX_TYPE_DGV {Block=2001, Output=2002, TeploBL=2003, TeploOP=2004, Param=2005, PromPlozsh=2006};

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
                InitializeComponents(nameDGV);
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
            public void AddColumn(string txtHeader, bool bRead, string nameCol, int idPut)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_iIdComp = idPut };
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
            public void ShowValues(DataTable[] tbOrigin_in, DataTable[] tbOrigin_out, DataTable[] arr_tb_param_in)
            {
                //Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                //ClearValues();
                //bool bflg = false;
                //double valueD;
                ////заполнение плана
                //if (planOnMonth.Rows.Count > 0)
                //    planInMonth(planOnMonth.Rows[0]["VALUE"].ToString(),
                //      Convert.ToDateTime(planOnMonth.Rows[0]["WR_DATETIME"].ToString()), dgvView);
                //else ;


                //for (int i = 0; i < dgvView.Rows.Count; i++)
                //{
                //    DataRow[] dr_CorValues = formingCorrValue(tbOrigin, dgvView.Rows[i].Cells["DATE"].Value.ToString());
                //    int count = 0;
                //    //заполнение столбцов с корр. знач.
                //    if (dr_CorValues[0] != null)
                //    {
                //        foreach (HDataGridViewColumn col in Columns)
                //        {
                //            for (int t = 0; t < dr_CorValues.Count(); t++)
                //            {
                //                if (col.m_iIdComp ==
                //                    Convert.ToInt32(dr_CorValues[t]["ID_PUT"]))
                //                {
                //                    valueD = Convert.ToDouble(dr_CorValues[t]["VALUE"]) / Math.Pow(10, 6);
                //                    dgvView.Rows[i].Cells[col.Index].Value = valueD;
                //                }
                //                else ;
                //            }
                //        }
                //    }

                //    for (int j = 0; j < tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count; j++)
                //    {
                //        //заполнение столбцов ГПТ,ТЭЦ
                //        if (dgvView.Rows[i].Cells["DATE"].Value.ToString() ==
                //        Convert.ToDateTime(tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["WR_DATETIME"]).ToShortDateString())
                //        {
                //            dgvView.Rows[i].Cells[namePut.GetValue(count).ToString()].Value =
                //                correctingValues(tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["VALUE"]
                //                , namePut.GetValue(count).ToString(), ref bflg, i, dgvView);
                //            count++;

                //        }
                //    }
                //    fillCells(i, this);
                //}
                foreach (HDataGridViewColumn col in Columns)
                {
                    if(col.Index!=0)
                        foreach(DataGridViewRow row in Rows)
                        {
                            DataRow[] row_comp = arr_tb_param_in[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select("ID_ALG=" 
                                + col.m_iIdComp.ToString() 
                                + " and ID_COMP=" + row.HeaderCell.Value.ToString());
                            if(row_comp.Length>0)
                            {
                            DataRow[] row_val = (tbOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Select("ID_PUT="
                                + row_comp[0]["ID"].ToString()));
                            if(row_val.Length>0)
                                row.Cells[col.Index].Value = row_val[0]["VALUE"].ToString().Trim();
                            row.Cells[col.Index].ReadOnly = false;
                            }

                            row_comp = arr_tb_param_in[(int)INDEX_TABLE_DICTPRJ.PARAMETER_OUT].Select("ID_ALG="
                                + col.m_iIdComp.ToString()
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
                int idComp = 0;
                editTable.Rows.Clear();
                HDataGridViewColumn cols = (HDataGridViewColumn)dgvView.Columns[column];

                for (int i = 0; i < dgvView.Rows.Count; i++)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_iIdComp > 0)
                        {
                            if (cols.m_iIdComp == col.m_iIdComp &&
                                dgvView.Rows[i].Cells["Date"].Value == dgvView.Rows[row].Cells["Date"].Value)
                            {
                                valueToRes = Convert.ToDouble(value) * Math.Pow(10, 6);
                                idComp = cols.m_iIdComp;
                            }
                            else
                                if (dgvView.Rows[i].Cells[col.Index].Value != null)
                                {
                                    valueToRes = Convert.ToDouble(dgvView.Rows[i].Cells[col.Index].Value) * Math.Pow(10, 6);
                                    idComp = col.m_iIdComp;
                                }
                                else
                                    valueToRes = -1;

                            //-1 не нужно записывать значение в таблицу
                            if (valueToRes > -1)
                                editTable.Rows.Add(new object[] 
                                {
                                    idComp
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

            public void InitializeStruct(DataTable nAlgTable, DataTable nAlgOutTable, DataTable compTable, Dictionary<int,object[]> dict_profile)
            {
                this.Rows.Clear();
                this.Columns.Clear();
                DataRow[] colums_in;
                DataRow[] colums_out;
                DataRow[] rows;
                List<DataRow> col_in = new List<DataRow>();
                List<DataRow> col_out = new List<DataRow>();
                switch(m_type_dgv)
                {
                    case INDEX_TYPE_DGV.Block:
                        
                        rows = compTable.Select("ID_COMP=1000");
                        break;
                    case INDEX_TYPE_DGV.Output:
                        //colums_in = nAlgTable.Select("N_ALG='2'");
                        //colums_out = nAlgOutTable.Select("N_ALG='2'");
                        rows = compTable.Select("ID_COMP=2000");
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
                        rows = compTable.Select("ID_COMP=3000");
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
                        foreach (Double id in (double[])list[0])
                        {
                            col_in.Add(nAlgTable.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                        }
                    }
                    if (list[1].ToString() == "out")
                    {
                        foreach (Double id in (double[])list[0])
                        {
                            col_out.Add(nAlgOutTable.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                        }
                    }

                }
                colums_in = col_in.ToArray();
                colums_out = col_out.ToArray();

                this.AddColumn("Компонент", true,"Comp");
                foreach (DataRow c in colums_in)
                {
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), Convert.ToInt32(c["ID"]));
                }

                foreach (DataRow c in colums_out)
                {
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), Convert.ToInt32(c["ID"]));
                }

                foreach (DataRow r in rows)
                {
                    this.Rows.Add(new object[this.ColumnCount]);
                    this.Rows[Rows.Count - 1].Cells[0].Value = r["DESCRIPTION"].ToString().Trim();
                    this.Rows[Rows.Count - 1].HeaderCell.Value = r["ID"];
                }
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

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
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
        protected class PanelManagementBalTeplo : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                    , BUTTON_SEND, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT,
                TXTBX_EMAIL,
                CBX_PERIOD,
                CBX_TIMEZONE,
                HDTP_BEGIN,
                HDTP_END,
                MENUITEM_UPDATE,
                MENUITEM_HISTORY,
                RADIO_BLOCK,
                RADIO_TEPLO,
                RADIO_PROM_PLOZSH,
                COUNT
            }

            public enum TypeRadioBtn { Block, Teplo, PromPlozsh };

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            public PanelManagementBalTeplo()
                : base(6, 8)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
                (Controls.Find(INDEX_CONTROL_BASE.RADIO_BLOCK.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);
                (Controls.Find(INDEX_CONTROL_BASE.RADIO_TEPLO.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);
                (Controls.Find(INDEX_CONTROL_BASE.RADIO_PROM_PLOZSH.ToString(), true)[0] as RadioButton_BalTask).CheckedChanged += new EventHandler(CheckedChangedRadioBtn);

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
                //Период расчета - подпись
                Label lblCalcPer = new Label();
                lblCalcPer.Text = "Период расчета";
                //Период расчета - значение
                ComboBox cbxCalcPer = new ComboBox();
                cbxCalcPer.Name = INDEX_CONTROL_BASE.CBX_PERIOD.ToString();
                cbxCalcPer.DropDownStyle = ComboBoxStyle.DropDownList;
                //Часовой пояс расчета - подпись
                Label lblCalcTime = new Label();
                lblCalcTime.Text = "Часовой пояс расчета";
                //Часовой пояс расчета - значение
                ComboBox cbxCalcTime = new ComboBox();
                cbxCalcTime.Name = INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString();
                cbxCalcTime.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxCalcTime.Enabled = false;
                //
                TableLayoutPanel tlp = new TableLayoutPanel();
                tlp.AutoSize = true;
                tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlp.Controls.Add(lblCalcPer, 0, 0);
                tlp.Controls.Add(cbxCalcPer, 0, 1);
                tlp.Controls.Add(lblCalcTime, 1, 0);
                tlp.Controls.Add(cbxCalcTime, 1, 1);
                this.Controls.Add(tlp, 0, posRow);
                this.SetColumnSpan(tlp, 4); this.SetRowSpan(tlp, 1);
                //
                TableLayoutPanel tlpValue = new TableLayoutPanel();
                //tlpValue.ColumnStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
                tlpValue.Dock = DockStyle.Fill;
                tlpValue.AutoSize = true;
                tlpValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                ////Дата/время начала периода расчета - подпись
                Label lBeginCalcPer = new Label();
                lBeginCalcPer.Dock = DockStyle.Bottom;
                lBeginCalcPer.Text = @"Дата/время начала периода расчета:";
                ////Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker(s_dtDefaultAU, null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                tlpValue.Controls.Add(lBeginCalcPer, 0, 0);
                tlpValue.Controls.Add(ctrl, 0, 1);
                //Дата/время  окончания периода расчета - подпись
                Label lEndPer = new Label();
                lEndPer.Dock = DockStyle.Top;
                lEndPer.Text = @"Дата/время  окончания периода расчета:";
                //Дата/время  окончания периода расчета - значение
                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(1)
                    , tlpValue.Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);
                this.Controls.Add(tlpValue, 0, posRow = posRow + 1);
                this.SetColumnSpan(tlpValue, 4); this.SetRowSpan(tlpValue, 1);
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL_BASE.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка - импортировать
                Button ctrlBSend = new Button();
                ctrlBSend.Name = INDEX_CONTROL_BASE.BUTTON_SEND.ToString();
                ctrlBSend.Text = @"Отправить";
                ctrlBSend.Dock = DockStyle.Top;
                //ctrlBSend.Enabled = false;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL_BASE.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;
                //Поле с почтой
                TextBox ctrlTxt = new TextBox();
                ctrlTxt.Name = INDEX_CONTEXT.ID_CON.ToString();
                //ctrlTxt.Text = @"Pasternak_AS@sibeco.su";
                ctrlTxt.Dock = DockStyle.Top;

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
                ctrlRadioBlock.Name = INDEX_CONTROL_BASE.RADIO_BLOCK.ToString();
                ctrlRadioBlock.Text = @"По блокам";
                ctrlRadioBlock.Type = TypeRadioBtn.Block.ToString();
                ctrlRadioBlock.Dock = DockStyle.Top;
                ctrlRadioBlock.Checked = true;
                //
                RadioButton_BalTask ctrlRadioTeplo = new RadioButton_BalTask();
                ctrlRadioTeplo.Name = INDEX_CONTROL_BASE.RADIO_TEPLO.ToString();
                ctrlRadioTeplo.Text = @"По выводам";
                ctrlRadioTeplo.Type = TypeRadioBtn.Teplo.ToString();
                ctrlRadioTeplo.Dock = DockStyle.Top;
                //
                RadioButton_BalTask ctrlRadioProm = new RadioButton_BalTask();
                ctrlRadioProm.Name = INDEX_CONTROL_BASE.RADIO_PROM_PLOZSH.ToString();
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

            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name="idPeriod"></param>
            public void SetPeriod(ID_PERIOD idPeriod)
            {
                HDateTimePicker hdtpBtimePer = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                , hdtpEndtimePer = Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                //Выполнить запрос на получение значений для заполнения 'DataGridView'
                switch (idPeriod)
                {
                    case ID_PERIOD.HOUR:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , DateTime.Now.Hour
                            , 0
                            , 0).AddHours(-1);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddHours(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.HOUR;
                        break;
                    //case ID_PERIOD.SHIFTS:
                    //    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    //    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    //    break;
                    case ID_PERIOD.DAY:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , 0
                            , 0
                            , 0);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddDays(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.DAY;
                        break;
                    case ID_PERIOD.MONTH:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , 1
                            , 0
                            , 0
                            , 0);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddMonths(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.MONTH;
                        break;
                    case ID_PERIOD.YEAR:
                        hdtpBtimePer.Value = new DateTime(DateTime.Now.Year
                            , 1
                            , 1
                            , 0
                            , 0
                            , 0).AddYears(-1);
                        hdtpEndtimePer.Value = hdtpBtimePer.Value.AddYears(1);
                        hdtpBtimePer.Mode =
                        hdtpEndtimePer.Mode =
                            HDateTimePicker.MODE.YEAR;
                        break;
                    default:
                        break;
                }
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
            this.Controls.Add(dgvTeploBL, 4, posRow+5);
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

            Button btn = (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskAutobookMonthValue_btnsend_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskAutobookMonthValues_btnexport_Click);


            dgvBlock.CellParsing += dgvAB_CellParsing;
            dgvBlock.CellEndEdit += dgvAB_CellEndEdit;
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
        void dgvAB_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            double value,
                valueCor;
            int err = -1;
            int numMonth = (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month
                , day = dgvBlock.Rows.Count;

            if (e.Value.ToString() == string.Empty)
                value = 0;
            else
                value = Convert.ToDouble(e.Value);// *Math.Pow(10, 6);

            valueCor = Convert.ToDouble(dgvBlock.Rows[e.RowIndex].Cells[dgvBlock.Columns[e.ColumnIndex].Name].Value);// *Math.Pow(10, 6);

            //switch (dgvBlock.Columns[e.ColumnIndex].Name)
            //{
            //    case "CorGTP12":
            //        if (value == 0)
            //            dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP12.ToString()].Value =
            //                Convert.ToDouble(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP12.ToString()].Value) - valueCor;
            //        else
            //            dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP12.ToString()].Value =
            //                (value - valueCor) + Convert.ToDouble(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP12.ToString()].Value);
            //        //корректировка значений
            //        dgvBlock.editCells(e.RowIndex, Convert.ToInt32(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP12.ToString()].Value)
            //                , dgvBlock, INDEX_CALC.GTP36.ToString());
            //        //сбор корр.значений
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            //            dgvBlock.FillTableCorValue(HandlerDb.OutValues(out err), dgvBlock, value, e.ColumnIndex, e.RowIndex);
            //        //сбор значений
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            //            dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err), dgvBlock, HandlerDb.getOutPut(out err));
            //        break;
            //    case "CorGTP36":
            //        if (value == 0)
            //            dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP36.ToString()].Value =
            //                Convert.ToDouble(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP36.ToString()].Value) - valueCor;
            //        else
            //            dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP36.ToString()].Value =
            //                 (value - valueCor) + Convert.ToDouble(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP36.ToString()].Value);
            //        //корректировка значений
            //        dgvBlock.editCells(e.RowIndex, Convert.ToInt32(dgvBlock.Rows[e.RowIndex].Cells[INDEX_CALC.GTP36.ToString()].Value)
            //                , dgvBlock, INDEX_CALC.GTP12.ToString());
            //        //сбор корр.значений
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            //            dgvBlock.FillTableCorValue(HandlerDb.OutValues(out err), dgvBlock, value, e.ColumnIndex, e.RowIndex);
            //        //сбор значений
            //        m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            //            dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err), dgvBlock, HandlerDb.getOutPut(out err));
            //        break;
            //    default:
            //        break;
            //}
        }

        /// <summary>
        /// окнчание редактирваония
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dgvAB_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

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
            //изменение начальной даты
            if (arQueryRanges.Count() > 1)
                arQueryRanges[1] = new DateTimeRange(arQueryRanges[1].Begin.AddDays(-(arQueryRanges[1].Begin.Day - 1))
                    , arQueryRanges[1].End.AddDays(-(arQueryRanges[1].End.Day - 2)));
            else
                arQueryRanges[0] = new DateTimeRange(arQueryRanges[0].Begin.AddDays(-(arQueryRanges[0].Begin.Day - 1))
                    , arQueryRanges[0].End.AddDays(DayIsMonth - arQueryRanges[0].End.Day));
            //Запрос для получения архивных данных
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            //Получение значений корр. input
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY,INDEX_DBTABLE_NAME.INVALUES, out err);

            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, INDEX_DBTABLE_NAME.OUTVALUES, out err);

            /*HandlerDb.getCorInPut(Type
            , arQueryRanges
            , ActualIdPeriod
            , out err);*/
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(
                        CountBasePeriod
                        , m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
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
                        setValues();

                        dgvBlock.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
                        dgvOutput.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
                        dgvTeploBL.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
                        dgvTeploOP.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
                        dgvParam.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
                        dgvPromPlozsh.ShowValues(m_arTableOrigin_in, m_arTableOrigin_out
                            , m_arTableDictPrjs_in);
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
                    throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
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
                ID_PERIOD idPeriod = ActualIdPeriod;

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
        private void HPanelAutobook_btnHistory_Click(object obj, EventArgs ev)
        {
            m_ViewValues = INDEX_VIEW_VALUES.ARCHIVE;

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
            m_ViewValues = INDEX_VIEW_VALUES.SOURCE;

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
        private void get_m_arrDictPrj()
        {
            int err = 0;
            int i = -1;
            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj_in = getQueryDictPrj();
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrjs_in[i] = m_handlerDb.Select(arQueryDictPrj_in[i], out err);
                if (!(err == 0))
                    break;
                else
                    ;
            }
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

            m_arTableDictPrjs_in = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            int i = -1;
            string strItem = string.Empty;
            get_m_arrDictPrj();

            m_dt_profile = HandlerDb.GetProfilesContext(m_id_panel);
            dgvBlock.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvBlock.Type_DGV));
            dgvOutput.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvOutput.Type_DGV));
            dgvTeploBL.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvTeploBL.Type_DGV));
            dgvTeploOP.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvTeploOP.Type_DGV));
            dgvPromPlozsh.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvPromPlozsh.Type_DGV));
            dgvParam.InitializeStruct(m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.N_ALG_OUT], m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.COMPONENT], GetProfileDGV((int)dgvParam.Type_DGV));

            ////Назначить обработчик события - изменение дата/время начала периода
            //hdtpBegin.ValueChanged += new EventHandler(hdtpBegin_onValueChanged);
            //Назначить обработчик события - изменение дата/время окончания периода
            // при этом отменить обработку события - изменение дата/время начала периода
            // т.к. при изменении дата/время начала периода изменяется и дата/время окончания периода
            // (Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);

            if (err == 0)
            {
                try
                {
                    //initialize();
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 1; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][1]);//??
                    (PanelManagement as PanelManagementBalTeplo).SetPeriod(Session.m_currIdPeriod);
                    (ctrl as ComboBox).Enabled = false;

                    ctrl = Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0];
                    //из profiles
                    for (int j = 0; j < m_dt_profile.Rows.Count; j++)
                        if (Convert.ToInt32(m_dt_profile.Rows[j]["ID_CONTEXT"]) == (int)INDEX_CONTEXT.ID_CON)
                            ctrl.Text = m_dt_profile.Rows[j]["VALUE"].ToString().TrimEnd();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskAutoBook::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                switch ((INDEX_TABLE_DICTPRJ)i)
                {
                    case INDEX_TABLE_DICTPRJ.PERIOD:
                        errMsg = @"Получение интервалов времени для периода расчета";
                        break;
                    case INDEX_TABLE_DICTPRJ.TIMEZONE:
                        errMsg = @"Получение списка часовых поясов";
                        break;
                    case INDEX_TABLE_DICTPRJ.COMPONENT:
                        errMsg = @"Получение списка компонентов станции";
                        break;
                    case INDEX_TABLE_DICTPRJ.PARAMETER:
                        errMsg = @"Получение строковых идентификаторов параметров в алгоритме расчета";
                        break;
                    //case INDEX_TABLE_DICTPRJ.MODE_DEV:
                    //    errMsg = @"Получение идентификаторов режимов работы оборудования";
                    //    break;
                    //case INDEX_TABLE_DICTPRJ.MEASURE:
                    //    errMsg = @"Получение информации по единицам измерения";
                    //    break;
                    default:
                        errMsg = @"Неизвестная ошибка";
                        break;
                }
        }

        private Dictionary<int, object[]> GetProfileDGV(int id_dgv)
        {
            Dictionary<int, object[]> dict_profile = new Dictionary<int, object[]>();
            string[] id;
            List<double> ids = new List<double>();
            DataRow[] rows = m_dt_profile.Select("ID_UNIT= 7 and ID_ITEM='" + id_dgv + "'");
            string type = string.Empty;
            if (rows.Length == 2)
            {
                List<object> obj = new List<object>();
                foreach (DataRow r in rows)
                {
                    id = r["VALUE"].ToString().Trim().Split(';');
                    ids.Clear();
                    if (id.Length > 0)
                    {
                        foreach (string str in id)
                        {
                            ids.Add(Convert.ToDouble(str.Replace('.', ',')));
                        }
                    }
                    if (Convert.ToInt32(r["ID_CONTEXT"].ToString().Trim()) == 33)
                    {
                        type = "in";
                    }
                    if (Convert.ToInt32(r["ID_CONTEXT"].ToString().Trim()) == 34)
                    {
                        type = "out";
                    }
                    obj.Add(new object[] { ids.ToArray(), type });
                }
                dict_profile.Add(id_dgv, obj.ToArray());
                
            }

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
            Session.SetRangeDatetime(dtBegin, dtEnd);
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
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected void clear(int iCtrl = (int)INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            INDEX_CONTROL indxCtrl = (INDEX_CONTROL)iCtrl;

            deleteSession();
            //??? повторная проверка
            if (bClose == true)
            {
                if (!(m_arTableDictPrjs_in == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                    {
                        if (!(m_arTableDictPrjs_in[i] == null))
                        {
                            m_arTableDictPrjs_in[i].Clear();
                            m_arTableDictPrjs_in[i] = null;
                        }
                        else
                            ;
                    }
                else
                    ;

                cbx = Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                dgvBlock.ClearRows();
                dgvOutput.ClearRows();
                dgvTeploBL.ClearRows();
                dgvTeploOP.ClearRows();
                dgvParam.ClearRows();
                dgvPromPlozsh.ClearRows();
                //dgvAB.ClearColumns();
            }
            else
            {
                // очистить содержание представления
                dgvBlock.ClearValues();
                dgvOutput.ClearValues();
                dgvTeploBL.ClearValues();
                dgvTeploOP.ClearValues();
                dgvParam.ClearValues();
                dgvPromPlozsh.ClearValues();
            }
        }

        /// <summary>
        /// удаление сессии и очистка таблиц 
        /// с временными данными
        /// </summary>
        protected void deleteSession()
        {
            int err = -1;

            HandlerDb.DeleteSession(out err);
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            int idTimezone = m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex];

            Session.SetCurrentTimeZone((ID_TIMEZONE)idTimezone
                , (int)m_arTableDictPrjs_in[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagement as PanelManagementBalTeplo).SetPeriod(Session.m_currIdPeriod);
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
        /// формирование запросов 
        /// для справочных данных
        /// </summary>
        /// <returns>запрос</returns>
        private string[] getQueryDictPrj()
        {
            string[] arRes = null;

            arRes = new string[]
            {
                //PERIOD
                HandlerDb.GetQueryTimePeriods(m_strIdPeriods)
                //TIMEZONE
                , HandlerDb.GetQueryTimezones(m_strIdTimezones)
                // список компонентов
                , HandlerDb.GetQueryCompList()
                // параметры расчета
                , HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
                , HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                //, HandlerDb.GetQueryModeDev()
                //// единицы измерения
                , m_handlerDb.GetQueryMeasures()
                // коэффициенты для единиц измерения
                , HandlerDb.GetQueryRatio()
                //входные параметры
                ,HandlerDb.GetQueryNAlgList()
                ,HandlerDb.GetQueryNAlgOutList()
            };

            return arRes;
        }

        /// <summary>
        /// Строка для запроса информации по периодам расчетов
        /// </summary>        
        protected string m_strIdPeriods
        {
            get
            {
                string strRes = string.Empty;

                for (int i = 0; i < m_arListIds[(int)INDEX_ID.PERIOD].Count; i++)
                    strRes += m_arListIds[(int)INDEX_ID.PERIOD][i] + @",";
                strRes = strRes.Substring(0, strRes.Length - 1);

                return strRes;
            }
        }

        /// <summary>
        /// Строка для запроса информации по часовым поясам
        /// </summary>        
        protected string m_strIdTimezones
        {
            get
            {
                string strRes = string.Empty;

                for (int i = 0; i < m_arListIds[(int)INDEX_ID.TIMEZONE].Count; i++)
                    strRes += m_arListIds[(int)INDEX_ID.TIMEZONE][i] + @",";
                strRes = strRes.Substring(0, strRes.Length - 1);

                return strRes;
            }
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

            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = getStructurOutval(out err);
            
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.saveResInval(m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = getStructurOutval(out err);
            
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.saveResOut(m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);

            base.HPanelTepCommon_btnSave_Click(obj, ev);

            //saveInvalValue(out err);
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
                + GetNameTableOut((Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value);

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

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

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

            strRes = TepCommon.HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"_" + dtInsert.Year.ToString() + dtInsert.Month.ToString(@"00");

            return strRes;
        }

        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(GetNameTableIn((Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString()
           , true)[0] as HDateTimePicker).Value)
           , @"ID_PUT, DATE_TIME"
           , @""
           , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
           , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
           , out err);

            m_handlerDb.RecUpdateInsertDelete(GetNameTableOut((Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString()
           , true)[0] as HDateTimePicker).Value)
           , @"ID_PUT, DATE_TIME"
           , @""
           , m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
           , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
           , out err);
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
                HandlerDb.getInPut(Type, dtrPer, ActualIdPeriod, out err);

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            HandlerDb.saveResInval(m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

            m_handlerDb.RecUpdateInsertDelete(GetNameTableIn((Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString()
                     , true)[0] as HDateTimePicker).Value)
                     , @"ID_PUT, DATE_TIME"
                     , @"ID"
                     , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                     , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                     , out err);
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

