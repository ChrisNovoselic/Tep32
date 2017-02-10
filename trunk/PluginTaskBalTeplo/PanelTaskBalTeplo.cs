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
using System.Reflection;

namespace PluginTaskBalTeplo
{
    public class PanelTaskBalTeplo : HPanelTepCommon
    {
        public enum INDEX_VIEW_VALUES { Block, Vyvod, PromPlozsh };

        private INDEX_VIEW_VALUES m_ViewValues;
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
        /// ???
        /// </summary>
        protected TaskBalTeploCalculate m_calculate;

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
            DGV_Param
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
        protected DataGridViewBalTeploValues dgvBlock,
            dgvOutput,
            dgvTeploBL,
            dgvTeploOP,
            dgvPromPlozsh,
            dgvParam;

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

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskBalTeploCalculate();
        }

        /// <summary>
        /// Класс для грида
        /// </summary>
        protected class DataGridViewBalTeploValues : DataGridView
        {
            private Dictionary<string, HTepUsers.DictElement> m_dict_ProfileNALG_IN
                , m_dict_ProfileNALG_OUT;

            private DataTable m_dbRatio;

            public enum INDEX_VIEW_VALUES { Block = 2001, Output = 2002, TeploBL = 2003, TeploOP = 2004, Param = 2005, PromPlozsh = 2006 };
            
            public INDEX_VIEW_VALUES m_ViewValues;

            public DataGridViewBalTeploValues(string nameDGV)
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
                                    DataRow[] row_val = (tbOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT="
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
                                    DataRow[] row_val = (tbOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT="
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
                switch (m_ViewValues)
                {
                    case INDEX_VIEW_VALUES.Block:

                        rows = compTable.Select("ID_COMP=1000 or ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.Output:
                        //colums_in = nAlgTable.Select("N_ALG='2'");
                        //colums_out = nAlgOutTable.Select("N_ALG='2'");
                        rows = compTable.Select("ID_COMP=2000 or ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.TeploBL:
                        //colums_in = nAlgTable.Select("N_ALG='3'");
                        //colums_out = nAlgOutTable.Select("N_ALG='3'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.TeploOP:
                        //colums_in = nAlgTable.Select("N_ALG='4'");
                        //colums_out = nAlgOutTable.Select("N_ALG='4'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.Param:
                        //colums_in = nAlgTable.Select("N_ALG='5'");
                        //colums_out = nAlgOutTable.Select("N_ALG='5'");
                        rows = compTable.Select("ID_COMP=1");
                        break;
                    case INDEX_VIEW_VALUES.PromPlozsh:
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

                foreach (object[] list in dict_profile[(int)m_ViewValues])
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

        /// <summary>
        /// калькулятор значений
        /// </summary>
        public class TaskBalTeploCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор основной (без аргументов)
            /// </summary>
            public TaskBalTeploCalculate()
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
        /// Конструктор - основной
        /// </summary>
        /// <param name="iFunc">Объект для взаимодействия с вызывающим приложением</param>
        public PanelTaskBalTeplo(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.BAL_TEPLO;
            m_calculate = new TaskBalTeploCalculate();
            m_dt_profile = new DataTable();

            m_arTableOrigin_in = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit_in = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            m_arTableOrigin_out = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit_out = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponents();

            Session.SetDatetimeRange(PanelManagement.DatetimeRange);
            PanelManagement.EventCheckedChangedIndexViewValues += new EventHandler(onCheckedChangedIndexViewValues);
        }

        private void onCheckedChangedIndexViewValues(object sender, EventArgs e)
        {
            PanelManagementBalTeplo.CheckedChangedIndexViewValuesEventArgs ev = e as PanelManagementBalTeplo.CheckedChangedIndexViewValuesEventArgs;

            m_ViewValues = (INDEX_VIEW_VALUES)((Control)sender).Tag;

            switch (m_ViewValues) {
                case INDEX_VIEW_VALUES.Block:
                    dgvOutput.Visible = false;
                    dgvTeploOP.Visible = false;
                    dgvParam.Visible = false;
                    dgvPromPlozsh.Visible = false;
                    dgvBlock.Visible = true;
                    dgvTeploBL.Visible = true;
                    break;
                case INDEX_VIEW_VALUES.Vyvod:
                    dgvBlock.Visible = false;
                    dgvTeploBL.Visible = false;
                    dgvParam.Visible = false;
                    dgvPromPlozsh.Visible = false;
                    dgvTeploOP.Visible = true;
                    dgvOutput.Visible = true;
                    break;
                case INDEX_VIEW_VALUES.PromPlozsh:
                    dgvBlock.Visible = false;
                    dgvOutput.Visible = false;
                    dgvTeploBL.Visible = false;
                    dgvTeploOP.Visible = false;
                    dgvParam.Visible = true;
                    dgvPromPlozsh.Visible = true;
                    break;
                default:
                    break;
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
                    , BUTTON_IMPORT, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT,
                MENUITEM_UPDATE,
                MENUITEM_HISTORY,
                RADIO_BLOCK,
                RADIO_VYVOD,
                RADIO_PROM_PLOZSH,
                COUNT
            }

            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

            public PanelManagementBalTeplo()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels) //6, 8
            {
                InitializeComponents();

                for (INDEX_CONTROL indx = INDEX_CONTROL.RADIO_BLOCK; !(indx > INDEX_CONTROL.RADIO_PROM_PLOZSH); indx ++)
                    (Controls.Find(indx.ToString(), true)[0] as RadioButton).CheckedChanged += new EventHandler(onCheckedChangedIndexViewValues);
            }

            private void InitializeComponents()
            {
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"

                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                
                SuspendLayout();

                posRow = 6;
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
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - импортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_IMPORT.ToString();
                ctrl.Text = @"Импорт";
                ctrl.Dock = DockStyle.Top;
                ctrl.Visible = true;
                ctrl.Enabled = false;
                //ctrlBSend.Enabled = false;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";                
                ctrl.Visible = true;
                ctrl.Enabled = false;
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_BLOCK.ToString();
                ctrl.Text = @"По блокам";
                ctrl.Tag = INDEX_VIEW_VALUES.Block;                
                (ctrl as RadioButton).Checked = true;
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_VYVOD.ToString();
                ctrl.Text = @"По выводам";
                ctrl.Tag = INDEX_VIEW_VALUES.Vyvod;
                //ctrlRadioTeplo.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //
                ctrl = new RadioButton();
                ctrl.Name = INDEX_CONTROL.RADIO_PROM_PLOZSH.ToString();
                ctrl.Text = @"Пром. площадки";
                ctrl.Tag = INDEX_VIEW_VALUES.PromPlozsh;
                //ctrlRadioProm.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);

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

            private void onCheckedChangedIndexViewValues(object obj, EventArgs e)
            {
                if ((obj as RadioButton).Checked == true)
                    EventCheckedChangedIndexViewValues?.Invoke(obj, new CheckedChangedIndexViewValuesEventArgs());
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения значения ячейки
            /// </summary>
            public class CheckedChangedIndexViewValuesEventArgs : EventArgs
            {
                /// <summary>
                /// Компонента
                /// </summary>
                public object m_Comp;

                public CheckedChangedIndexViewValuesEventArgs()
                    : base()
                {
                    m_Comp = null;

                }

                public CheckedChangedIndexViewValuesEventArgs(int comp)
                    : this()
                {
                    m_Comp = comp;
                }
            }

            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public EventHandler EventCheckedChangedIndexViewValues;            
        }

        /// <summary>
        /// инициализация объектов
        /// </summary>
        private void InitializeComponents()
        {
            Control ctrl = new Control(); ;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            int posColdgvValues = 4;

            SuspendLayout();

            posRow = 0;

            #region DGV
            dgvBlock = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Block.ToString());
            dgvBlock.Dock = DockStyle.Fill;
            dgvBlock.Name = INDEX_CONTROL.DGV_Block.ToString();
            dgvBlock.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Block;
            dgvBlock.AllowUserToResizeRows = false;
            dgvBlock.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBlock.Visible = true;
            this.Controls.Add(dgvBlock, 4, posRow);
            this.SetColumnSpan(dgvBlock, 9); this.SetRowSpan(dgvBlock, 5);
            //
            dgvOutput = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Output.ToString());
            dgvOutput.Dock = DockStyle.Fill;
            dgvOutput.Name = INDEX_CONTROL.DGV_Output.ToString();
            dgvOutput.AllowUserToResizeRows = false;
            dgvOutput.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Output;
            dgvOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOutput.Visible = false;
            this.Controls.Add(dgvOutput, 4, posRow);
            this.SetColumnSpan(dgvOutput, 9); this.SetRowSpan(dgvOutput, 5);
            //
            dgvTeploBL = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_TeploBL.ToString());
            dgvTeploBL.Dock = DockStyle.Fill;
            dgvTeploBL.Name = INDEX_CONTROL.DGV_TeploBL.ToString();
            dgvTeploBL.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.TeploBL;
            dgvTeploBL.AllowUserToResizeRows = false;
            dgvTeploBL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploBL.Visible = true;
            this.Controls.Add(dgvTeploBL, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploBL, 9); this.SetRowSpan(dgvTeploBL, 5);
            //
            dgvTeploOP = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_TeploOP.ToString());
            dgvTeploOP.Dock = DockStyle.Fill;
            dgvTeploOP.Name = INDEX_CONTROL.DGV_TeploOP.ToString();
            dgvTeploOP.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.TeploOP;
            dgvTeploOP.AllowUserToResizeRows = false;
            dgvTeploOP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeploOP.Visible = false;
            this.Controls.Add(dgvTeploOP, 4, posRow + 5);
            this.SetColumnSpan(dgvTeploOP, 9); this.SetRowSpan(dgvTeploOP, 5);
            //
            dgvPromPlozsh = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_PromPlozsh.ToString());
            dgvPromPlozsh.Dock = DockStyle.Fill;
            dgvPromPlozsh.Name = INDEX_CONTROL.DGV_PromPlozsh.ToString();
            dgvPromPlozsh.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.PromPlozsh;
            dgvPromPlozsh.AllowUserToResizeRows = false;
            dgvPromPlozsh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPromPlozsh.Visible = false;
            this.Controls.Add(dgvPromPlozsh, 4, posRow);
            this.SetColumnSpan(dgvPromPlozsh, 9); this.SetRowSpan(dgvPromPlozsh, 5);
            //
            dgvParam = new DataGridViewBalTeploValues(INDEX_CONTROL.DGV_Param.ToString());
            dgvParam.Dock = DockStyle.Fill;
            dgvParam.Name = INDEX_CONTROL.DGV_Param.ToString();
            dgvParam.m_ViewValues = DataGridViewBalTeploValues.INDEX_VIEW_VALUES.Param;
            dgvParam.AllowUserToResizeRows = false;
            dgvParam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParam.Visible = false;
            this.Controls.Add(dgvParam, 4, posRow + 5);
            this.SetColumnSpan(dgvParam, 9); this.SetRowSpan(dgvParam, 5);
            #endregion
            //
            this.Controls.Add(PanelManagement, 0, posRow);
            this.SetColumnSpan(PanelManagement, posColdgvValues);
            this.SetRowSpan(PanelManagement, RowCount);//this.RowCount);     

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
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_IMPORT.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskBalTeplo_btnimport_Click);
            (Controls.Find(PanelManagementBalTeplo.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskbalTeplo_btnexport_Click);

            dgvBlock.CellParsing += dgvCellParsing;
            dgvOutput.CellParsing += dgvCellParsing;
            dgvParam.CellParsing += dgvCellParsing;
            dgvPromPlozsh.CellParsing += dgvCellParsing;
            dgvTeploBL.CellParsing += dgvCellParsing;
            dgvTeploOP.CellParsing += dgvCellParsing;
        }

        /// <summary>
        /// Обработчик события - нажатие на кнопку "Экспорт"
        /// </summary>
        /// <param name="sender">Объект - инициатор события (кнопка)</param>
        /// <param name="e">Аргумент события</param>
        void PanelTaskbalTeplo_btnexport_Click(object sender, EventArgs e)
        {
            //rptExcel.CreateExcel(dgvAB);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender">Объект - инициатор события (кнопка "Отправить")</param>
        /// <param name="e">Аргумент события</param>
        void PanelTaskBalTeplo_btnimport_Click(object sender, EventArgs e)
        {
            int err = -1;
            string toSend = (Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0] as TextBox).Text;

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err), dgvBlock, HandlerDb.getOutPut(out err));
            //rptsNSS.SendMailToNSS(m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
            //, HandlerDb.GetDateTimeRangeValuesVar(), toSend);
        }

        /// <summary>
        /// обработчик события датагрида -
        /// редактирвание значений.
        /// сохранение изменений в DataTable
        /// </summary>
        /// <param name="sender">Объект - инициатор события (представление)</param>
        /// <param name="e">Аргумент события</param>
        void dgvCellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            int err = -1;
            int id_put = -1;
            string N_ALG = (((DataGridViewBalTeploValues)sender).Columns[e.ColumnIndex] as DataGridViewBalTeploValues.HDataGridViewColumn).m_N_ALG;
            int id_comp = Convert.ToInt32(((DataGridViewBalTeploValues)sender).Rows[e.RowIndex].HeaderCell.Value);

            if ((((DataGridViewBalTeploValues)sender).Columns[e.ColumnIndex] as DataGridViewBalTeploValues.HDataGridViewColumn).m_bInPut == true)
            {
                DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
                if (rows.Length == 1)
                    id_put = Convert.ToInt32(rows[0]["ID"]);
                m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            }
            else
            {
                DataRow[] rows = m_dictTableDictPrj[ID_DBTABLE.OUT_PARAMETER].Select("N_ALG=" + N_ALG + " and ID_COMP=" + id_comp);
                if (rows.Length == 1)
                    id_put = Convert.ToInt32(rows[0]["ID"]);
                m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Select("ID_PUT=" + id_put)[0]["VALUE"] = e.Value;
            }
            HandlerDb.RegisterDbConnection(out err);
            HandlerDb.RecUpdateInsertDelete(
                TepCommon.HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVALUES].m_name
                , "ID_PUT,ID_SESSION"
                , null
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                , out err
            );
            //HandlerDb.insertInValues(m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE], out err);
            HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar (
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , out err
            );
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar(
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES
                , out err
            );
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();

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
            ((DataGridViewBalTeploValues)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = e.Value;
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
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.INVALUES, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
                (
                Type
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
               , out err
                );
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Merge(HandlerDb.GetValuesDayVar
                (
                Type
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
               , out err
                ));

            //Получение значений по-умолчанию input
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.INVALUES, out err);

            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetValuesArch(ID_DBTABLE.OUTVALUES, out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
                (
                TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
               , out err
                );
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = HandlerDb.GetValuesDefAll(ID_PERIOD.DAY, ID_DBTABLE.OUTVALUES, out err);

            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , Session.CountBasePeriod
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
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Copy();
            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                = m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Copy();
            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                = m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = Session.CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
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
                    if (m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        //setValues();
                        //вычисление значений
                        HandlerDb.Calculate(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
                        m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
                            (
                            TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES,
                            out err
                            );
                        m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar
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
                        //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
                        //    dgvBlock.FillTableValueDay(HandlerDb.OutValues(out err)
                        //       , dgvBlock
                        //       , HandlerDb.getOutPut(out err));
                        ////сохранить вых.корр. знач. в DataTable
                        //m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
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
        /// обработчик кнопки-архивные значения
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void btnHistory_OnClick(object obj, EventArgs ev)
        {
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;

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
            Session.m_ViewValues = TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;

            onButtonLoadClick();

        }
        /// <summary>
        /// 
        /// </summary>
        protected System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
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

            //m_dictTableDictPrj = new DataTable[(int)ID_DBTABLE.COUNT];
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
                    dgvBlock.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvBlock.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvOutput.InitializeStruct(m_dictTableDictPrj[(ID_DBTABLE.INALG)], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvOutput.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvTeploBL.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvTeploBL.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvTeploOP.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvTeploOP.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvPromPlozsh.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvPromPlozsh.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);
                    dgvParam.InitializeStruct(m_dictTableDictPrj[ID_DBTABLE.INALG], m_dictTableDictPrj[ID_DBTABLE.OUTALG], m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], GetProfileDGV((int)dgvParam.m_ViewValues), m_dictTableDictPrj[ID_DBTABLE.RATIO]);

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

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
            HandlerDb.saveResInval(getStructurOutval(out err)
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE], out err);

            m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
            HandlerDb.saveResOut(getStructurOutval(out err)
            , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE], out err);

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
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
                , out err
            );

            m_handlerDb.RecUpdateInsertDelete(
                GetNameTableOut(PanelManagement.DatetimeRange.Begin)
                , @"ID_PUT, DATE_TIME, ID_USER, ID_SOURCE"
                , @""
                , m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
                , m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]
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

            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                HandlerDb.getInPut(Type, dtrPer, Session.ActualIdPeriod, out err);

            m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
            HandlerDb.saveResInval(m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
            , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT], out err);

            m_handlerDb.RecUpdateInsertDelete(
                GetNameTableIn(PanelManagement.DatetimeRange.Begin)
                , @"ID_PUT, DATE_TIME"
                , @"ID"
                , m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , out err
            );
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
               m_arTableEdit_in[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
            m_arTableOrigin_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
               m_arTableEdit_out[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
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

