using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Data.Common;
using GemBox.Spreadsheet;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Outlook = Microsoft.Office.Interop.Outlook;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskAutobook
{
    public class PanelTaskAutobookMonthValues : HPanelTepCommon
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="dgvView">отображение</param>
        delegate void delDeviation(int i, DataGridView dgvView);
        bool m_bflgClear = true;
        //public event DelegateBoolFunc EvtChangeRow;
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
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
        protected TaskAutobookCalculate AutoBookCalc;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
            , PERIOD, TIMEZONE, COMPONENT,
            PARAMETER //_IN, PARAMETER_OUT
                , MODE_DEV/*, MEASURE*/,
            RATIO
                , COUNT
        }
        /// <summary>
        /// 
        /// </summary>
        public enum INDEX_GTP : int
        {
            UNKNOW = -1,
            GTP12,
            GTP36,
            TEC,
            CorGTP12,
            CorGTP36,
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
            DGV_DATA,
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
        protected HandlerDbTaskAutobookMonthValuesCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskAutobookMonthValuesCalculate; } }
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// Часовой пояс(часовой сдвиг)
        /// </summary>
        protected static int m_currentOffSet;
        /// <summary>
        /// 
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// 
        /// </summary>
        public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        private PanelManagementAutobook createPanelManagement()
        {
            return new PanelManagementAutobook();
        }

        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        protected DGVAutoBook dgvAB;

        /// <summary>
        /// to Outlook
        /// </summary>
        protected ReportsToNSS rptsNSS;

        /// <summary>
        /// to Excel
        /// </summary>
        protected ReportExcel rptExcel;
        /// <summary>
        /// Панель на которой размещаются активные элементы управления
        /// </summary>
        private PanelManagementAutobook _panelManagement;
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementAutobook PanelManagement
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
            return new HandlerDbTaskAutobookMonthValuesCalculate();
        }

        /// <summary>
        /// Класс для грида
        /// </summary>
        protected class DGVAutoBook : DataGridView
        {
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
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            /// <param name="parametrs">параметры</param>
            public void ShowValues(DataTable[] tbOrigin, DataGridView dgvView, DataTable planOnMonth)
            {
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                ClearValues();
                bool bflg = false;
                double valueD;
                //заполнение плана
                if (planOnMonth.Rows.Count > 0)
                    planInMonth(planOnMonth.Rows[0]["VALUE"].ToString(),
                      Convert.ToDateTime(planOnMonth.Rows[0]["WR_DATETIME"].ToString()), dgvView);
                else ;


                for (int i = 0; i < dgvView.Rows.Count; i++)
                {
                    DataRow[] dr_CorValues = formingCorValue(tbOrigin, dgvView.Rows[i].Cells["DATE"].Value.ToString(), m_currentOffSet);
                    int count = 0;
                    //заполнение столбцов с корр. знач.
                    if (dr_CorValues[0] != null)
                    {
                        foreach (HDataGridViewColumn col in Columns)
                            for (int t = 0; t < dr_CorValues.Count(); t++)
                                if (col.m_iIdComp ==
                                    Convert.ToInt32(dr_CorValues[t]["ID_PUT"]))
                                {
                                    valueD = Convert.ToDouble(dr_CorValues[t]["VALUE"]) / Math.Pow(10, 6);
                                    dgvView.Rows[i].Cells[col.Index].Value = valueD;
                                }
                                else ;
                    }


                    for (int j = 0; j < tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count; j++)
                    {
                        //заполнение столбцов ГТП,ТЭЦ
                        if (dgvView.Rows[i].Cells["DATE"].Value.ToString() ==
                        Convert.ToDateTime(tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["WR_DATETIME"]).AddMinutes(m_currentOffSet).ToShortDateString())
                        {
                            dgvView.Rows[i].Cells[namePut.GetValue(count).ToString()].Value =
                                correctingValues(tbOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["VALUE"]
                                , namePut.GetValue(count).ToString(), ref bflg, i, dgvView);
                            count++;
                        }
                    }
                    fillCells(i, dgvView);
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
                view.Rows[row].Cells[INDEX_GTP.TEC.ToString()].Value = value + Convert.ToInt32(view.Rows[row].Cells[col].Value);

                for (int i = row; i < view.Rows.Count; i++)
                    if (view.Rows[i].Cells[INDEX_GTP.TEC.ToString()].Value != null)
                        fillCells(i, view);
                    else
                        break;
            }

            /// <summary>
            /// Вычисление параметров нараст.ст.
            /// и заполнение грида
            /// </summary>
            /// <param name="i">номер строки</param>
            /// <param name="dgvView">отображение</param>
            private void fillCells(int i, DataGridView dgvView)
            {
                int STswen = 0;

                if (dgvView.Rows[i].Cells[INDEX_GTP.TEC.ToString()].Value != null)
                {
                    if (i == 0)
                        dgvView.Rows[i].Cells["StSwen"].Value =
                           dgvView.Rows[i].Cells[INDEX_GTP.TEC.ToString()].Value;
                    else
                    {
                        STswen = Convert.ToInt32(dgvView.Rows[i].Cells[INDEX_GTP.TEC.ToString()].Value);
                        dgvView.Rows[i].Cells["StSwen"].Value = STswen + Convert.ToSingle(dgvView.Rows[i - 1].Cells["StSwen"].Value);
                    }
                    countDeviation(i, dgvView);
                }
                else ;
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
            private DataRow[] formingCorValue(DataTable[] dtOrigin, string date, int offSet)
            {
                DataRow[] dr_idCorPut = new DataRow[dtOrigin.Count()];
                DateTime dateOffSet;

                var m_enumResIDPUT = (from r in dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].AsEnumerable()
                                      orderby r.Field<DateTime>("WR_DATETIME")
                                      select new
                                      {
                                          DATE_TIME = r.Field<DateTime>("WR_DATETIME"),
                                      }).Distinct();

                for (int i = 0; i < m_enumResIDPUT.Count(); i++)
                {
                    dateOffSet = m_enumResIDPUT.ElementAt(i).DATE_TIME.AddMinutes(offSet);

                    if (date == dateOffSet.ToShortDateString())
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
                , int offset
                , object value
                , int column
                , int row)
            {
                double valueToRes;
                int idComp = 0;
                DateTime timeRes;
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

                            timeRes = Convert.ToDateTime(dgvView.Rows[i].Cells["Date"].Value.ToString());
                            //-1 не нужно записывать значение в таблицу
                            if (valueToRes > -1)
                                editTable.Rows.Add(new object[] 
                                {
                                    idComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes                
                                    , timeRes.AddMinutes(-offset).ToString(CultureInfo.InvariantCulture)
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
            public DataTable FillTableCorValue(DataTable editTable, DataGridView dgvView, int offset)
            {
                double valueToRes;
                DateTime dtRes;
                editTable.Rows.Clear();

                for (int i = 0; i < dgvView.Rows.Count; i++)
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_iIdComp > 0)
                        {
                            if (dgvView.Rows[i].Cells[col.Index].Value != null)
                                valueToRes = Convert.ToDouble(dgvView.Rows[i].Cells[col.Index].Value) * Math.Pow(10, 6);
                            else
                                valueToRes = -1;

                            dtRes = Convert.ToDateTime(dgvView.Rows[i].Cells["Date"].Value.ToString());

                            if (valueToRes > -1)
                                editTable.Rows.Add(new object[] 
                                {
                                    col.m_iIdComp
                                    , -1
                                    , 1.ToString()
                                    , valueToRes                
                                    , dtRes.AddMinutes(-offset).ToString(CultureInfo.InvariantCulture)
                                    , i
                                });
                        }
                    }
                }
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
                Array namePut = Enum.GetValues(typeof(INDEX_GTP));
                string put;
                double valueToRes;
                editTable.Rows.Clear();

                foreach (DataGridViewRow row in dgvView.Rows)
                {
                    if (Convert.ToDateTime(row.Cells["Date"].Value) <= DateTime.Now.Date)
                    {
                        if (row.Cells["TEC"].Value != null)
                            for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
                            {
                                put = dtOut.Rows[i]["ID"].ToString();
                                valueToRes = Convert.ToDouble(row.Cells[namePut.GetValue(i).ToString()].Value) * Math.Pow(10, 6);

                                editTable.Rows.Add(new object[] 
                            {
                                put
                                , -1
                                , 1.ToString()
                                , valueToRes                
                                , Convert.ToDateTime(row.Cells["Date"].Value.ToString()).ToString(CultureInfo.InvariantCulture)
                                , i
                            });
                            }
                    }
                }
                return editTable;
            }
        }

        /// <summary>
        /// калькулятор значений
        /// </summary>
        public class TaskAutobookCalculate : TepCommon.HandlerDbTaskCalculate.TaskCalculate
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
            public TaskAutobookCalculate()
            {
                calcTable = new DataTable[(int)INDEX_GTP.COUNT];
                value = new List<string>((int)INDEX_GTP.COUNT);
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

                calcTable[(int)INDEX_GTP.GTP12] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                calcTable[(int)INDEX_GTP.TEC] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
                calcTable[(int)INDEX_GTP.GTP36] = dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
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
                    calcTable[(int)INDEX_GTP.GTP12].Rows.Clear();
                    calcTable[(int)INDEX_GTP.GTP36].Rows.Clear();

                    DataRow[] drOrigin =
                        dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].
                        Select(String.Format(dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Locale
                        , "WR_DATETIME = '{0:o}'", m_enumDT.ElementAt(j).DATE_TIME));

                    foreach (DataRow row in drOrigin)
                    {
                        if (i < 2)
                        {
                            calcTable[(int)INDEX_GTP.GTP12].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        }
                        else
                            calcTable[(int)INDEX_GTP.GTP36].Rows.Add(new object[]
                            {
                                row["ID_PUT"]
                                ,row["ID_SESSION"]
                                ,row["QUALITY"]
                                ,row["VALUE"]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                            });
                        i++;
                    }

                    calculate(calcTable);

                    for (int t = 0; t < value.Count(); t++)
                    {
                        calcTable[(int)INDEX_GTP.TEC].Rows.Add(new object[]
                            {
                                dtOut.Rows[t]["ID"]
                                ,dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["ID_SESSION"]
                                ,dtOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows[j]["QUALITY"]
                                ,value[t]
                                ,m_enumDT.ElementAt(j).DATE_TIME
                                ,count
                            });
                        count++;
                    }
                }
            }

            /// <summary>
            /// Вычисление парамтеров ГТП и ТЭЦ
            /// </summary>
            /// <param name="tb_gtp">таблица с данными</param>
            private void calculate(DataTable[] tb_gtp)
            {
                float fTG12 = 0
                    , fTG36 = 0
                    , fTec = 0;

                if (value.Count() > 0)
                    value.Clear();

                for (int i = (int)INDEX_GTP.GTP12; i < (int)INDEX_GTP.CorGTP12; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_GTP.GTP12:
                            fTG12 = sumTG(tb_gtp[i]);
                            value.Add(fTG12.ToString("####"));
                            break;
                        case (int)INDEX_GTP.GTP36:
                            fTG36 = sumTG(tb_gtp[i]);
                            value.Add(fTG36.ToString("####"));
                            break;
                        case (int)INDEX_GTP.TEC:
                            fTec = fTG12 + fTG36;
                            value.Add(fTec.ToString("####"));
                            break;
                        default:
                            break;
                    }
                }
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
            CreateMessage m_crtMsg;

            public ReportsToNSS()
            {
                m_crtMsg = new CreateMessage();
            }

            /// <summary>
            /// Класс создания письма
            /// </summary>
            private class CreateMessage
            {
                /// <summary>
                /// 
                /// </summary>
                Outlook.Application oApp;

                /// <summary>
                /// конструктор(основной)
                /// </summary>
                public CreateMessage()
                {
                    oApp = new Outlook.Application();
                }

                /// <summary>
                /// Формирование письма
                /// </summary>
                /// <param name="subject">тема письма</param>
                /// <param name="body">тело сообщения</param>
                /// <param name="to">кому/куда</param>
                public void FormingMessage(string subject, string body, string to)
                {
                    try
                    {
                        Outlook.MailItem newMail = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                        newMail.To = to;
                        newMail.Subject = subject;
                        newMail.Body = body;
                        newMail.Importance = Outlook.OlImportance.olImportanceNormal;
                        newMail.Display();
                        sendMail(newMail);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="mail"></param>
                private void sendMail(Outlook.MailItem mail)
                {
                    //отправка
                    ((Outlook._MailItem)mail).Send();
                }

                /// <summary>
                /// Прикрепление файла к письму
                /// </summary>
                /// <param name="mail"></param>
                private void AddAttachment(Outlook.MailItem mail)
                {
                    OpenFileDialog attachment = new OpenFileDialog();

                    attachment.Title = "Select a file to send";
                    attachment.ShowDialog();

                    if (attachment.FileName.Length > 0)
                    {
                        mail.Attachments.Add(
                            attachment.FileName,
                            Outlook.OlAttachmentType.olByValue,
                            1,
                            attachment.FileName);
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                private void closeOutlook()
                {
                    ((Outlook.Application)oApp).Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oApp);
                    System.GC.Collect();
                }
            }

            /// <summary>
            /// Содание тела сообщения
            /// </summary>
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtRange">дата</param>
            private void createBodyToSend(ref string sbjct
                , ref string bodyMsg
                , DataTable sourceTable
                , DateTimeRange dtRange)
            {
                DataRow[] drReportDay;
                DateTime reportDate;

                reportDate = dtRange.Begin.AddHours(6).Date;
                drReportDay =
                    sourceTable.Select(String.Format(sourceTable.Locale, @"WR_DATETIME = '{0:o}'", reportDate));

                if ((double)drReportDay.Length != 0)
                {
                    bodyMsg = @"BEGIN " + "\r\n"
                        + @"(DATE):" + reportDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "\r\n"
                        + @"(01): " + fewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ":\r\n"
                        + @"(02): " + fewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ":\r\n"
                        + @"(03): " + fewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]) + ":\r\n"
                        + @"END ";
                    /*bodyMsg = @"Дата " + reportDate.ToShortDateString() + ".\r\n"
                        + @"Станция, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.TEC]["VALUE"]) + ";\r\n"
                        + @"Блоки 1-2, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP12]["VALUE"]) + ";\r\n"
                        + @"Блоки 3-6, сутки: " + FewerValue((double)drReportDay[(int)INDEX_GTP.GTP36]["VALUE"]);*/

                    sbjct = @"Отчет о выработке электроэнергии НТЭЦ-5 за " + reportDate.ToShortDateString();
                }
            }

            /// <summary>
            /// Редактирование знчения
            /// </summary>
            /// <param name="val">значение</param>
            /// <returns>измененное знач.</returns>
            private string fewerValue(double val)
            {
                return Convert.ToString(val / Math.Pow(10, 6)).ToString();
            }

            /// <summary>
            /// Создание. Подготовка. Отправка письма.
            /// </summary>
            /// <param name="sourceTable">таблица с данными</param>
            /// <param name="dtRange">выбранный промежуток</param>
            /// <param name="to">получатель</param>
            public void SendMailToNSS(DataTable sourceTable, DateTimeRange dtRange, string to)
            {
                string bodyMsg = string.Empty
                 , sbjct = string.Empty;

                createBodyToSend(ref sbjct, ref bodyMsg, sourceTable, dtRange);

                if (sbjct != "")
                    m_crtMsg.FormingMessage(sbjct, bodyMsg, to);
                else ;
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            private Excel.Application m_excApp;
            private Excel.Workbook m_workBook;
            private Excel.Worksheet m_wrkSheet;
            private object _missingObj = System.Reflection.Missing.Value;

            /// <summary>
            /// Экземпляр класса
            /// </summary>
            //ExcelFile efNSS;
            /// <summary>
            /// 
            /// </summary>
            protected enum INDEX_DIVISION : int
            {
                UNKNOW = -1,
                SEPARATE_CELL,
                ADJACENT_CELL
            }

            /// <summary>
            /// конструктор(основной)
            /// </summary>
            public ReportExcel()
            {
                m_excApp = new Excel.Application();
                m_excApp.Visible = false;
            }


            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgView"></param>
            /// <param name="dtRange"></param>
            public void CreateExcel(DataGridView dgView, DateTimeRange dtRange)
            {
                string pathToTemplate = @"D:\MyProjects\C.Net\TEP32\Tep\bin\Debug\Template\TemplateAutobook.xlsx";
                object pathToTemplateObj = pathToTemplate;
                m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                //
                m_workBook.AfterSave += workBook_AfterSave;
                m_workBook.BeforeClose += workBook_BeforeClose;
                m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("Autobook");

                try
                {
                    for (int i = 0; i < dgView.Columns.Count; i++)
                    {
                        int indxRow = 0;
                        Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[i + 1];

                        foreach (Excel.Range cell in colRange.Cells)
                            if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                            {
                                fillSheetExcel(colRange, dgView, i, cell.Row);
                                break;
                            }
                        indxRow++;
                    }
                    //
                    setPlanMonth(m_wrkSheet, dgView, dtRange);
                    m_excApp.Visible = true;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);
                }
                catch (Exception e)
                {
                    CloseExcel();
                }
            }

            /// <summary>
            /// Обработка события - закрытие экселя
            /// </summary>
            /// <param name="Cancel"></param>
            void workBook_BeforeClose(ref bool Cancel)
            {
                CloseExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                CloseExcel();
            }

            /// <summary>
            /// Добавление плана и месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setPlanMonth(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                Excel.Range exclRPL = exclWrksht.get_Range("C5");
                Excel.Range exclRMonth = exclWrksht.get_Range("A4");
                exclRPL.Value2 = dgv.Rows[dgv.Rows.Count - 1].Cells[@"PlanSwen"].Value;
                exclRMonth.Value2 = HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
            }

            /// <summary>
            /// Деление 
            /// </summary>
            /// <param name="headerTxt">строка</param>
            /// <returns>часть строки</returns>
            private string splitString(string headerTxt)
            {
                string[] spltHeader = headerTxt.Split(',');

                if (spltHeader.Length > (int)INDEX_DIVISION.ADJACENT_CELL)
                    return spltHeader[(int)INDEX_DIVISION.ADJACENT_CELL].TrimStart();
                else
                    return spltHeader[(int)INDEX_DIVISION.SEPARATE_CELL];
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне
            /// </summary>
            /// <param name="cellRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range cellRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < cellRange.Rows.Count; i++)
                    if (((Excel.Range)cellRange.Cells[i]).Value == null &&
                        ((Excel.Range)cellRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        row = i;
                        break;
                    }

                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    cellRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);
                    row++;
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            public void CloseExcel()
            {
                //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                m_excApp = null;
                m_workBook = null;
                m_wrkSheet = null;
                System.GC.Collect();
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobookMonthValues(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;
            AutoBookCalc = new TaskAutobookCalculate();

            m_arTableOrigin = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
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
        protected class PanelManagementAutobook : HPanelCommon
        {
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                    , BUTTON_SEND, BUTTON_SAVE,
                BUTTON_LOAD,
                BUTTON_EXPORT
                    ,
                TXTBX_EMAIL
                    , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN,
                HDTP_END
                                , MENUITEM_UPDATE,
                MENUITEM_HISTORY
                    , COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            public PanelManagementAutobook()
                : base(6, 5)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
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
                , hdtpEndtimePer = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
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

            dgvAB = new DGVAutoBook(INDEX_CONTROL.DGV_DATA.ToString());
            dgvAB.Dock = DockStyle.Fill;
            dgvAB.Name = INDEX_CONTROL.DGV_DATA.ToString();
            dgvAB.AllowUserToResizeRows = false;
            dgvAB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAB.AddColumn("Дата", true, "Date");
            dgvAB.AddColumn("Корректировка ПТО, Блоки 1-2", false, "CorGTP12", 23226);
            dgvAB.AddColumn("Корректировка ПТО, Блоки 3-6", false, "CorGTP36", 23227);
            dgvAB.AddColumn("Блоки 1-2", true, INDEX_GTP.GTP12.ToString());
            dgvAB.AddColumn("Блоки 3-6", true, INDEX_GTP.GTP36.ToString());
            dgvAB.AddColumn("Станция,сутки", true, INDEX_GTP.TEC.ToString());
            dgvAB.AddColumn("Станция,нараст.", true, "StSwen");
            dgvAB.AddColumn("План нараст.", true, "PlanSwen");
            dgvAB.AddColumn("Отклонение от плана", true, "DevOfPlan");
            this.Controls.Add(dgvAB, 4, posRow);
            this.SetColumnSpan(dgvAB, 9); this.SetRowSpan(dgvAB, 10);
            //
            this.Controls.Add(PanelManagement, 0, posRow);
            this.SetColumnSpan(PanelManagement, posColdgvTEPValues);
            this.SetRowSpan(PanelManagement, posRow = posRow + 6);//this.RowCount);            

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelAutobook_btnHistory_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SAVE.ToString(), true)[0] as Button).Click +=
                new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_SEND.ToString(), true)[0] as Button).Click +=
                new EventHandler(PanelTaskAutobookMonthValue_btnsend_Click);
            (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString(), true)[0] as Button).Click +=
                 new EventHandler(PanelTaskAutobookMonthValues_btnexport_Click);

            dgvAB.CellParsing += dgvAB_CellParsing;
            dgvAB.SelectionChanged += dgvAB_SelectionChanged;
        }

        /// <summary>
        /// обработка события - Выбор строки
        /// </summary>
        /// <param name="sender">грид</param>
        /// <param name="e"></param>
        void dgvAB_SelectionChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < (sender as DataGridView).SelectedRows.Count; i++)
            {
                DateTime dtRow = Convert.ToDateTime((sender as DataGridView).SelectedRows[i].Cells["Date"].Value);
                HDateTimePicker datetimePicker =
                    (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                m_bflgClear = false;
                //EvtChangeRow += PanelTaskAutobookMonthValues_EvtChangeRow;
                //EvtChangeRow(true);
                datetimePicker.Value = dtRow;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="param"></param>
        //void PanelTaskAutobookMonthValues_EvtChangeRow(bool param)
        //{

        //}

        /// <summary>
        /// обработка ЭКСПОРТА(временно)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValues_btnexport_Click(object sender, EventArgs e)
        {
            rptExcel = new ReportExcel();//
            rptExcel.CreateExcel(dgvAB, Session.m_rangeDatetime);
        }

        /// <summary>
        /// Оброботчик события клика кнопки отправить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PanelTaskAutobookMonthValue_btnsend_Click(object sender, EventArgs e)
        {
            rptsNSS = new ReportsToNSS();
            int err = -1;
            string toSend = (Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0] as TextBox).Text;

            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            dgvAB.FillTableValueDay(HandlerDb.OutValues(out err), dgvAB, HandlerDb.GetOutPut(out err));
            rptsNSS.SendMailToNSS(m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
            , Session.m_rangeDatetime, toSend);
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
            int numMonth = (Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month
                , day = dgvAB.Rows.Count;

            if (e.Value.ToString() == string.Empty)
                value = 0;
            else
                value = Convert.ToDouble(e.Value);// *Math.Pow(10, 6);

            valueCor = Convert.ToDouble(dgvAB.Rows[e.RowIndex].Cells[dgvAB.Columns[e.ColumnIndex].Name].Value);// *Math.Pow(10, 6);

            switch (dgvAB.Columns[e.ColumnIndex].Name)
            {
                case "CorGTP12":
                    if (value == 0)
                        dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value =
                            Convert.ToDouble(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value) - valueCor;
                    else
                        dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value =
                            (value - valueCor) + Convert.ToDouble(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value);
                    //корректировка значений
                    dgvAB.editCells(e.RowIndex, Convert.ToInt32(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP12.ToString()].Value)
                            , dgvAB, INDEX_GTP.GTP36.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                        dgvAB.FillTableCorValue(HandlerDb.OutValues(out err), dgvAB, Session.m_curOffsetUTC, value, e.ColumnIndex, e.RowIndex);
                    //сбор значений
                    m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                        dgvAB.FillTableValueDay(HandlerDb.OutValues(out err), dgvAB, HandlerDb.GetOutPut(out err));
                    break;
                case "CorGTP36":
                    if (value == 0)
                        dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value =
                            Convert.ToDouble(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value) - valueCor;
                    else
                        dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value =
                             (value - valueCor) + Convert.ToDouble(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value);
                    //корректировка значений
                    dgvAB.editCells(e.RowIndex, Convert.ToInt32(dgvAB.Rows[e.RowIndex].Cells[INDEX_GTP.GTP36.ToString()].Value)
                            , dgvAB, INDEX_GTP.GTP12.ToString());
                    //сбор корр.значений
                    m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                        dgvAB.FillTableCorValue(HandlerDb.OutValues(out err), dgvAB, Session.m_curOffsetUTC, value, e.ColumnIndex, e.RowIndex);
                    //сбор значений
                    m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                        dgvAB.FillTableValueDay(HandlerDb.OutValues(out err), dgvAB, HandlerDb.GetOutPut(out err));
                    break;
                default:
                    break;
            }
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
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );
            //Получение значений корр. input
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                HandlerDb.GetCorInPut(Type
                , arQueryRanges
                , ActualIdPeriod
                , out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(
                        CountBasePeriod
                        , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
                        , ref m_arTableOrigin
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
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                = m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Clone();
        }

        /// <summary>
        /// Проверка выбранного диапазона
        /// </summary>
        /// <param name="dtRange">диапазон дат</param>
        /// <returns></returns>
        private bool rangeCheking(DateTimeRange[] dtRange)
        {
            bool bflag = false;

            for (int i = 0; i < dtRange.Length; i++)
                if (dtRange[i].End.Month > DateTime.Now.Month)
                    if (dtRange[i].End.Year >= DateTime.Now.Year)
                        bflag = true;

            return bflag;
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet = HandlerDb.GetDateTimeRangeValuesVar();

            if (rangeCheking(dtrGet))
                MessageBox.Show("Выбранный диапазон месяцев неверен");
            else
            {
                clear();
                m_handlerDb.RegisterDbConnection(out iRegDbConn);

                if (!(iRegDbConn < 0))
                {
                    // установить значения в таблицах для расчета, создать новую сессию
                    setValues(dtrGet, out err, out errMsg);

                    if (err == 0)
                    {
                        if (m_TableOrigin.Rows.Count > 0)
                        {
                            // создать копии для возможности сохранения изменений
                            setValues();
                            //вычисление значений
                            AutoBookCalc.getTable(m_arTableOrigin, HandlerDb.GetOutPut(out err));
                            //
                            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                                AutoBookCalc.calcTable[(int)INDEX_GTP.TEC].Copy();
                            //запись выходных значений во временную таблицу
                            HandlerDb.insertOutValues(out err, AutoBookCalc.calcTable[(int)INDEX_GTP.TEC]);
                            // отобразить значения
                            dgvAB.ShowValues(m_arTableOrigin
                                , dgvAB
                                , HandlerDb.GetPlanOnMonth(Type
                                , dtrGet
                                , ActualIdPeriod
                                , out err));
                            //формирование таблиц на основе грида
                            valuesFence(out err);
                        }
                        else
                            deleteSession();
                    }
                    else
                    {
                        // в случае ошибки "обнулить" идентификатор сессии
                        deleteSession();
                        throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                    }
                }
                else
                    deleteSession();

                if (!(iRegDbConn > 0))
                    m_handlerDb.UnRegisterDbConnection();
                else
                    ;
            }
        }

        /// <summary>
        /// формирование таблиц данных
        /// </summary>
        /// <param name="err"></param>
        private void valuesFence(out int err)
        {
            //сохранить вых. знач. в DataTable
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
                dgvAB.FillTableValueDay(HandlerDb.OutValues(out err)
                   , dgvAB
                   , HandlerDb.GetOutPut(out err));
            //сохранить вх.корр. знач. в DataTable
            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                dgvAB.FillTableCorValue(HandlerDb.OutValues(out err), dgvAB, Session.m_curOffsetUTC);
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
            get { return m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }
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

            m_arTableDictPrjs = new DataTable[(int)INDEX_TABLE_DICTPRJ.COUNT];
            HTepUsers.ID_ROLES role = (HTepUsers.ID_ROLES)HTepUsers.Role;

            Control ctrl = null;
            string strItem = string.Empty;
            int i = -1;
            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj = getQueryDictPrj();
            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
            {
                m_arTableDictPrjs[i] = m_handlerDb.Select(arQueryDictPrj[i], out err);

                if (!(err == 0))
                    break;
                else
                    ;
            }
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
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 1; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][1]);//??
                    (PanelManagement as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
                    (ctrl as ComboBox).Enabled = false;

                    ctrl = Controls.Find(INDEX_CONTEXT.ID_CON.ToString(), true)[0];
                    DataTable tb = HandlerDb.GetProfilesContext(findMyID());
                    //из profiles
                    for (int j = 0; j < tb.Rows.Count; j++)
                        if (Convert.ToInt32(tb.Rows[j]["ID_CONTEXT"]) == (int)INDEX_CONTEXT.ID_CON)
                            ctrl.Text = tb.Rows[j]["VALUE"].ToString().TrimEnd();
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
            m_currentOffSet = Session.m_curOffsetUTC;
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
            Session.SetRangeDatetime(dtBegin, dtEnd);
            // очистить содержание представления
            if (m_bflgClear)
            {
                clear();
                //заполнение представления
                fillDaysGrid(dtBegin, dtBegin.Month);
            }
            //
            m_currentOffSet = Session.m_curOffsetUTC;
            m_bflgClear = true;
        }

        /// <summary>
        /// заполнение грида датами
        /// </summary>
        /// <param name="date">тек.дата</param>
        /// <param name="numMonth">номер месяца</param>
        private void fillDaysGrid(DateTime date, int numMonth)
        {
            DateTime dt = new DateTime(date.Year, date.Month, 1);
            dgvAB.SelectionChanged -= dgvAB_SelectionChanged;
            dgvAB.ClearRows();

            for (int i = 0; i < DayIsMonth; i++)
            {
                dgvAB.AddRow();
                dgvAB.Rows[i].Cells[0].Value = dt.AddDays(i).ToShortDateString();
            }
            dgvAB.Rows[date.Day - 1].Selected = true;
            dgvAB.SelectionChanged += dgvAB_SelectionChanged;
        }

        /// <summary>
        /// очистка грида
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
                if (!(m_arTableDictPrjs == null))
                    for (int i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT; i++)
                    {
                        if (!(m_arTableDictPrjs[i] == null))
                        {
                            m_arTableDictPrjs[i].Clear();
                            m_arTableDictPrjs[i] = null;
                        }
                        else
                            ;
                    }
                else
                    ;

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxTimezone_SelectedIndexChanged;
                cbx.Items.Clear();

                dgvAB.ClearRows();
                //dgvAB.ClearColumns();
            }
            else
                // очистить содержание представления
                dgvAB.ClearValues()
                ;
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
                , (int)m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            (PanelManagement as PanelManagementAutobook).SetPeriod(Session.m_currIdPeriod);
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);

            // очистить содержание представления
            clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementAutobook.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
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
                , HandlerDb.GetQueryParameters(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                //, HandlerDb.GetQueryModeDev()
                //// единицы измерения
                , m_handlerDb.GetQueryMeasures()
                // коэффициенты для единиц измерения
                , HandlerDb.GetQueryRatio()
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
            //сбор значений
            valuesFence(out err);

            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = getStructurOutval(
                GetNameTableOut((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value), out err);

            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
            HandlerDb.SaveResOut(m_TableOrigin
            , m_TableEdit, out err);

            if (m_TableEdit.Rows.Count > 0)
            {
                base.HPanelTepCommon_btnSave_Click(obj, ev);
                //save вх. значений
                saveInvalValue(out err);
            }

        }

        /// <summary>
        /// получает структуру таблицы 
        /// OUTVAL_XXXXXX
        /// </summary>
        /// <param name="err"></param>
        /// <returns>таблица</returns>
        private DataTable getStructurOutval(string nameTable, out int err)
        {
            string strRes = string.Empty;

            strRes = "SELECT * FROM " + nameTable;

            return HandlerDb.Select(strRes, out err);
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
            //
            sortingDataToTable(HandlerDb.GetDataOutval(HandlerDb.getDateTimeRangeExtremeVal(), out err)
                , m_TableEdit
                , GetNameTableOut((Controls.Find(PanelManagementAutobook.INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value)
                , @""
                , out err);
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="m_origin">оригинальная таблица</param>
        /// <param name="m_edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable m_origin, DataTable m_edit, string unCol, out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME"
                    , unCol
                    , m_origin
                    , m_edit
                    , out err);
        }

        /// <summary>
        /// разбор данных по разным табилца(взависимости от месяца)
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="m_origin">оригинальная таблица</param>
        /// <param name="m_edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsertUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void sortingDataToTable(DataTable origin
            , DataTable edit
            , string nameTable
            , string unCol
            , out int err)
        {
            string nameTableExtrmRow = string.Empty
                          , nameTableNew = string.Empty;
            DataTable editTemporary = new DataTable()
                , originTemporary = new DataTable();
            err = -1;
            editTemporary = edit.Clone();
            originTemporary = origin.Clone();
            nameTableNew = nameTable;

            foreach (DataRow row in edit.Rows)
            {
                nameTableExtrmRow = extremeRow(row["DATE_TIME"].ToString(), nameTableNew);

                if (nameTableExtrmRow != nameTableNew)
                {
                    foreach (DataRow rowOrigin in origin.Rows)
                        if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month != Convert.ToDateTime(row["DATE_TIME"]).Month)
                            originTemporary.Rows.Add(rowOrigin.ItemArray);

                    //if (m_edit.Rows.Count < 1)
                    //    m_edit.Rows.Add(row.ItemArray);

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);

                    nameTableNew = nameTableExtrmRow;
                    editTemporary.Rows.Clear();
                    originTemporary.Rows.Clear();
                    editTemporary.Rows.Add(row.ItemArray);
                }
                else
                    editTemporary.Rows.Add(row.ItemArray);
            }

            if (editTemporary.Rows.Count > 0)
            {
                foreach (DataRow rowOrigin in origin.Rows)
                    if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month == Convert.ToDateTime(editTemporary.Rows[0]["DATE_TIME"]).Month)
                        originTemporary.Rows.Add(rowOrigin.ItemArray);

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);
            }
        }

        /// <summary>
        /// Нахождение имени таблицы для крайних строк
        /// </summary>
        /// <param name="strDate">дата</param>
        /// <param name="nameTable">изначальное имя таблицы</param>
        /// <returns>имя таблицы</returns>
        private static string extremeRow(string strDate, string nameTable)
        {
            DateTime dtStr = Convert.ToDateTime(strDate);
            string m_nametable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + m_nametable;
        }

        /// <summary>
        /// Сохранение входных знчений(корр. величины)
        /// </summary>
        /// <param name="err"></param>
        private void saveInvalValue(out int err)
        {
            err = -1;
            DateTimeRange[] dtrPer = HandlerDb.GetDateTimeRangeValuesVar();

            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                HandlerDb.GetInPutID(Type, dtrPer, ActualIdPeriod, out err);

            m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
            HandlerDb.SaveResInval(m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
            , m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT], out err);

            sortingDataToTable(m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , GetNameTableIn(dtrPer[0].Begin)
                , @"ID"
                , out err);
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] =
               m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Copy();
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
}

