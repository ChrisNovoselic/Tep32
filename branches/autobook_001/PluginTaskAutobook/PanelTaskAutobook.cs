using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Diagnostics;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskAutobook
{
    public class PanelTaskAutobook : HPanelTepCommon
    {
        /// <summary>
        /// 
        /// </summary>
        string[] GetMonth = 
        { 
            "Январь", "Февраль", "Март", "Апрель", 
            "Май", "Июнь", "Июль", "Август", "Сентябрь", 
            "Октябрь", "Ноябрь", "Декабрь","Январь сл. года"
        };
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Перечисление - индексы таблиц со словарными величинами и проектными данными
        /// </summary>
        protected enum INDEX_TABLE_DICTPRJ : int
        {
            UNKNOWN = -1
          , PERIOD, TIMEZONE, COMPONENT,
            PARAMETER //_IN, PARAMETER_OUT
                ,// MODE_DEV/*, MEASURE*/,
            RATIO
                , COUNT
        }
        /// <summary>
        /// Актуальный идентификатор периода расчета (с учетом режима отображаемых данных)
        /// </summary>
        protected ID_PERIOD ActualIdPeriod { get { return m_ViewValues == INDEX_VIEW_VALUES.SOURCE ? ID_PERIOD.DAY : Session.m_currIdPeriod; } }
        /// <summary>
        /// Признак отображаемых на текущий момент значений
        /// </summary>
        protected INDEX_VIEW_VALUES m_ViewValues;
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
            UNKNOWN = -1
                 ,
            HDTP_END,
            HDTP_BEGIN
                , BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD,
            DGV_DATA,
            DGV_PLANEYAR
                ,
            LABEL_DESC
              ,
            CBX_PERIOD,
            CBX_TIMEZONE
                ,
            MENUITEM_UPDATE, MENUITEM_HISTORY
        };
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1
            ,
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
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return HandlerDb._Session; } }
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalcAutoBook HandlerDb { get { return m_handlerDb as HandlerDbTaskCalcAutoBook; } }
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Отображение значений в табличном представлении(значения)
        /// </summary>
        protected DGVAutoBook dgvAB;
        /// <summary>
        /// Отображение значений в табличном представлении(план)
        /// </summary>
        protected DGVAutoBook dgvYear;
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        protected DataTable[] m_arTableDictPrjs;
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        // protected abstract PanelManagementTaskAutoBook createPanelManagement();

        //private PanelManagementTaskAutoBook _panelManagement;
        ///// <summary>
        ///// Панель на которой размещаются активные элементы управления
        ///// </summary>
        //protected PanelManagementTaskAutoBook PanelManagement
        //{
        //    get
        //    {
        //        if (_panelManagement == null)
        //            _panelManagement = createPanelManagement();
        //        else
        //            ;

        //        return _panelManagement;
        //    }
        //}

        //private PanelManagementTaskAutoBook createPanelManagement()
        //{
        //    return new PanelManagementTaskAutoBook();
        //}
        /// <summary>
        /// 
        /// </summary>
        private static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Набор текстов для подписей для кнопок
        /// </summary>
        protected static string[] m_arButtonText = { @"Отправить", @"Сохранить", @"Загрузить" };

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskCalcAutoBook();
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
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead"></param>
            public void AddColumn(string text, bool bRead)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    //column.Frozen = true;
                    column.ReadOnly = bRead;
                    column.HeaderText = text;
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
                //CellValueChanged -= onCellValueChanged;

                //foreach (DataGridViewRow r in Rows)
                //    foreach (DataGridViewCell c in r.Cells)
                //        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                //        {
                //            c.Value = string.Empty;
                //            c.Style.BackColor = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                //        }
                //        else
                //            ;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class HandlerDbTaskCalcAutoBook : HandlerDbTaskCalculate
        {
            /// <summary>
            /// Запрос к БД по получению редактируемых значений (автоматически собираемые значения)
            ///  , структура таблицы совместима с [inval], [outval]
            /// </summary>
            /// <param name="type"></param>
            /// <param name="idPeriod">период</param>
            /// <param name="cntBasePeriod"></param>
            /// <param name="arQueryRanges">диапазон времени запроса</param>
            /// <returns></returns>
            public override string getQueryValuesVar(TaskCalculate.TYPE type, ID_PERIOD idPeriod, int cntBasePeriod, DateTimeRange[] arQueryRanges)
            {
                string strRes = string.Empty
                , whereParameters = string.Empty;

                if (!(type == TaskCalculate.TYPE.UNKNOWN))
                {
                    // аналог в 'GetQueryParameters'
                    //whereParameters = getWhereRangeAlg(type);
                    //if (whereParameters.Equals(string.Empty) == false)
                    //    whereParameters = @" AND a." + whereParameters;
                    //else
                    //    ;

                    int i = -1;
                    bool bLastItem = false
                        , bEquDatetime = false;

                    for (i = 0; i < arQueryRanges.Length; i++)
                    {
                        bLastItem = !(i < (arQueryRanges.Length - 1));

                        strRes += @"SELECT v.ID_PUT, v.QUALITY, v.[VALUE]"
                                + @", " + _Session.m_Id + @" as [ID_SESSION]"
                                + @",[DATE_TIME]"
                                + @", m.[AVG]"
                            //+ @", GETDATE () as [WR_DATETIME]"
                            + @" FROM [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.VALUE) + @"_" + arQueryRanges[i].Begin.ToString(@"yyyyMM") + @"] v"
                                + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.PUT) + @"] p ON p.ID = v.ID_PUT"
                                + @" LEFT JOIN [dbo].[" + getNameDbTable(type, TABLE_CALCULATE_REQUIRED.ALG) + @"] a ON a.ID = p.ID_ALG AND a.ID_TASK = " + (int)IdTask + whereParameters
                                + @" LEFT JOIN [dbo].[measure] m ON a.ID_MEASURE = m.ID"
                            + @" WHERE v.[ID_TIME] = " + (int)idPeriod //???ID_PERIOD.HOUR //??? _currIdPeriod
                            ;
                        // при попадании даты/времени на границу перехода между отчетными периодами (месяц)
                        // 'Begin' == 'End'
                        if (bLastItem == true)
                            bEquDatetime = arQueryRanges[i].Begin.Equals(arQueryRanges[i].End);
                        else
                            ;

                        if (bEquDatetime == false)
                            strRes += @" AND [DATE_TIME] >= '" + arQueryRanges[i].Begin.ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                          + @" AND [DATE_TIME] < '" + arQueryRanges[i].End.ToString(@"yyyyMMdd HH:mm:ss") + @"'";

                        if (bLastItem == false)
                            strRes += @" UNION ALL";
                        else
                            ;
                    }

                    strRes = @"SELECT v.ID_PUT" // as [ID]"
                            + @", " + _Session.m_Id + @" as [ID_SESSION]"
                            + @", [QUALITY]"
                            + ",[VALUE]"
                             + ",[DATE_TIME] as [WR_DATETIME]"
                        + @" FROM (" + strRes + @") as v"
                        + @" ORDER BY  v.ID_PUT,v.DATE_TIME"
                        ;
                }
                else
                    Logging.Logg().Error(@"HandlerDbTaskCalculate::getQueryValuesVar () - неизветстный тип расчета...", Logging.INDEX_MESSAGE.NOT_SET);

                return strRes;
            }

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="cntBasePeriod"></param>
            ///// <param name="tablePars"></param>
            ///// <param name="arTableValues"></param>
            ///// <param name="dtRange"></param>
            ///// <param name="err"></param>
            ///// <param name="strErr"></param>
            //public override void CreateSession(int cntBasePeriod, DataTable tablePars, ref DataTable[] arTableValues, DateTimeRange dtRange, out int err, out string strErr)
            //{
            //    err = 0;
            //    strErr = string.Empty;

            //    int iAVG = -1;
            //    string strQuery = string.Empty;
            //    // строки для удаления из таблицы значений "по умолчанию"
            //    // при наличии дубликатов строк в таблице с загруженными из источников с данными
            //    DataRow[] rowsSel = null;

            //    // удалить строки из таблицы со значениями "по умолчанию"???
            //    //foreach (DataRow rValVar in arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows)
            //    //{
            //    //    rowsSel = arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_PUT=" + rValVar[@"ID_PUT"]);
            //    //    foreach (DataRow rToRemove in rowsSel)
            //    //        arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Rows.Remove(rToRemove);
            //    //}
            //    // вставить строки из таблицы со значениями 
            //    foreach (DataRow rValDef in arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows)
            //    {
            //        rowsSel = tablePars.Select(@"ID=" + rValDef[@"ID_PUT"]);
            //        if (rowsSel.Length == 1)
            //        {
            //            iAVG = (Int16)rowsSel[0][@"AVG"];

            //            arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Add(new object[]
            //            {
            //             rValDef[@"ID_PUT"]
            //                , _Session.m_Id //ID_SESSION
            //                , (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT //QUALITY
            //                , (iAVG == 0) ? cntBasePeriod * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
            //                , HDateTime.ToMoscowTimeZone() //??? GETADTE()
            //            }
            //            );
            //        }
            //        else
            //            ; // по идентификатору найден не единственный параметр расчета
            //    }


            //    if ((arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Columns.Count > 0)
            //        && (arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Count > 0))
            //    {
            //        //Вситвить строку с идентификатором новой сессии
            //        insertIdSession(cntBasePeriod, out err);
            //        //Вставить строки в таблицу БД со входными значениями для расчета
            //        insertInValues(arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION], out err);
            //        //Вставить строки в таблицу БД со выходными значениями для расчета
            //        insertOutValues(out err);

            //        // необходимость очистки/загрузки - приведение структуры таблицы к совместимому с [inval]
            //        arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION].Rows.Clear();
            //        // получить входные для расчета значения для возможности редактирования
            //        strQuery = @"SELECT [ID_PUT], [ID_SESSION], [QUALITY], [VALUE], [WR_DATETIME]" // as [ID]
            //            + @" FROM [" + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @"]"
            //            + @" WHERE [ID_SESSION]=" + _Session.m_Id;
            //        arTableValues[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = Select(strQuery, out err);
            //    }
            //    else
            //        Logging.Logg().Error(@"HandlerDbTaskCalculate::CreateSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);

            //}

            /// <summary>
            /// Возвратить наименование таблицы 
            /// </summary>
            /// <param name="type">Тип панели/расчета</param>
            /// <param name="req">Индекс таблицы, требуемой при расчете</param>
            /// <returns>Наименование таблицы</returns>
            private static string getNameDbTable(TaskCalculate.TYPE type, TABLE_CALCULATE_REQUIRED req)
            {
                INDEX_DBTABLE_NAME indx = INDEX_DBTABLE_NAME.UNKNOWN;

                indx = TaskCalculate.GetIndexNameDbTable(type, req);

                return s_NameDbTables[(int)indx];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TaskAutobookCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            public TaskAutobookCalculate()
            {

            }

            private void Calculate()
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
        /// 
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskAutobook(IPlugIn iFunc)
            : base(iFunc)
        {
            HandlerDb.IdTask = ID_TASK.AUTOBOOK;

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];

            InitializeComponent();

            Session.SetRangeDatetime(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <param name="numMonth">номер месяца</param>
        /// <returns>кол-во дней</returns>
        private double monthIsDays(int numMonth)
        {
            return DateTime.DaysInMonth(s_dtDefaultAU.Year, numMonth);
        }

        private void InitializeComponent()
        {
            Control ctrl = null;
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
            //int posColdgvTEPValues = 6;

            SuspendLayout();

            posRow = 0;
            //this.Controls.Add(PanelManagement, 0, posRow);
            //this.SetColumnSpan(PanelManagement, posColdgvTEPValues);
            //this.SetRowSpan(PanelManagement, this.RowCount);
            dgvYear = new DGVAutoBook(INDEX_CONTROL.DGV_PLANEYAR.ToString());
            dgvYear.AddColumn("Месяц", true);
            dgvYear.AddColumn("Выработка, тыс. кВтч", false);
            //dgv.RowCount = GetMonth.Length;
            for (int i = 0; i < GetMonth.Length; i++)
            {
                dgvYear.AddRow();
                dgvYear.Rows[i].Cells[0].Value = GetMonth[i];
            }
            //
            dgvAB = new DGVAutoBook(INDEX_CONTROL.DGV_DATA.ToString());
            dgvAB.AddColumn("Дата", true);
            dgvAB.AddColumn("Корректировка ПТО блоки 1-2", false);
            dgvAB.AddColumn("Корректировка ПТО блоки 3-6", false);
            dgvAB.AddColumn("Блоки 1-2", true);
            dgvAB.AddColumn("Блоки 3-6", true);
            dgvAB.AddColumn("Станция,сутки", true);
            dgvAB.AddColumn("Станция,нараст.", true);
            dgvAB.AddColumn("План нараст.", false);
            dgvAB.AddColumn("Отклонение от плана", true);
            this.Controls.Add(dgvAB, 4, posRow);
            this.SetColumnSpan(dgvAB, 9); this.SetRowSpan(dgvAB, 10);

            //Период расчета - подпись
            Label lblCalcPer = new Label();
            lblCalcPer.Text = "Период расчета";
            //Период расчета - значение
            ComboBox cbxCalcPer = new ComboBox();
            cbxCalcPer.Name = INDEX_CONTROL.CBX_PERIOD.ToString();
            cbxCalcPer.DropDownStyle = ComboBoxStyle.DropDownList;
            //Часовой пояс расчета - подпись
            Label lblCalcTime = new Label();
            lblCalcTime.Text = "Часовой пояс расчета";
            //Часовой пояс расчета - значение
            ComboBox cbxCalcTime = new ComboBox();
            cbxCalcTime.Name = INDEX_CONTROL.CBX_TIMEZONE.ToString();
            cbxCalcTime.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxCalcTime.Enabled = false;
            //
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.AutoSize = true;
            tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            //tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlp.Controls.Add(lblCalcPer, 0, 0);
            tlp.Controls.Add(cbxCalcPer, 0, 1);
            tlp.Controls.Add(lblCalcTime, 1, 0);
            tlp.Controls.Add(cbxCalcTime, 1, 1);
            this.Controls.Add(tlp, 0, posRow);
            this.SetColumnSpan(tlp, 4);
            this.SetRowSpan(tlp, 1);
            ////Дата/время начала периода расчета - подпись
            Label lBeginCalcPer = new System.Windows.Forms.Label();
            lBeginCalcPer.Dock = DockStyle.Top;
            lBeginCalcPer.Text = @"Дата/время начала периода расчета";
            ////Дата/время начала периода расчета - значения
            HDateTimePicker hdtpBtimePer = new HDateTimePicker(s_dtDefaultAU, null);
            hdtpBtimePer.Name = INDEX_CONTROL.HDTP_BEGIN.ToString();
            TableLayoutPanel tlpPeriod = new TableLayoutPanel();
            tlpPeriod.Dock = DockStyle.Top;
            tlpPeriod.AutoSize = true;
            tlpPeriod.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlpPeriod.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlpPeriod.Controls.Add(lBeginCalcPer, 0, 0);
            tlpPeriod.Controls.Add(hdtpBtimePer, 0, 1);
            this.Controls.Add(tlpPeriod, 0, posRow = posRow + 1);
            this.SetColumnSpan(tlpPeriod, 4);
            this.SetRowSpan(tlpPeriod, 1);
            //Дата/время  окончания периода расчета - подпись
            Label lEndPer = new System.Windows.Forms.Label();
            lEndPer.Dock = DockStyle.Top;
            lEndPer.Text = @"Дата/время  окончания периода расчета:";
            //Дата/время  окончания периода расчета - значения
            HDateTimePicker hdtpEndtimePer = new HDateTimePicker(s_dtDefaultAU.AddDays(monthIsDays(s_dtDefaultAU.Month)), hdtpBtimePer);
            hdtpEndtimePer.Name = INDEX_CONTROL.HDTP_END.ToString();
            //
            TableLayoutPanel tlpValue = new TableLayoutPanel();
            tlpValue.Dock = DockStyle.Fill;
            tlpValue.AutoSize = true;
            tlpValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlpValue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tlpValue.Controls.Add(lEndPer, 0, 0);
            tlpValue.Controls.Add(hdtpEndtimePer, 0, 1);
            this.Controls.Add(tlpValue, 0, posRow = posRow + 1);
            this.SetColumnSpan(tlpValue, 4); this.SetRowSpan(tlpValue, 1);
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
            ctrlBSend.Enabled = false;
            //Кнопка - сохранить
            Button ctrlBsave = new Button();
            ctrlBsave.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
            ctrlBsave.Text = @"Сохранить";
            ctrlBsave.Dock = DockStyle.Top;

            TableLayoutPanel tlpButton = new TableLayoutPanel();
            tlpButton.Dock = DockStyle.Fill;
            tlpButton.AutoSize = true;
            tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlpButton.Controls.Add(ctrl, 0, 0);
            tlpButton.Controls.Add(ctrlBSend, 1, 0);
            tlpButton.Controls.Add(ctrlBsave, 0, 1);
            this.Controls.Add(tlpButton, 0, posRow = posRow + 1);
            this.SetColumnSpan(tlpButton, 4); this.SetRowSpan(tlpButton, 2);
            //
            Label lblMonthPlan = new System.Windows.Forms.Label();
            lblMonthPlan.Text = @"Плановая выработка  тыс. кВтч: ";
            lblMonthPlan.Dock = DockStyle.Top;
            this.Controls.Add(lblMonthPlan, 0, posRow = posRow + 2);
            this.SetColumnSpan(lblMonthPlan, 4); this.SetRowSpan(lblMonthPlan, 1);
            //
            //
            Label lblyearDGV = new System.Windows.Forms.Label();
            lblyearDGV.Dock = DockStyle.Top;
            lblyearDGV.Text = @"Плановая выработка электроэнергии на "
                + DateTime.Now.Year + " год.";
            Label lblTEC = new System.Windows.Forms.Label();
            lblTEC.Dock = DockStyle.Top;
            lblTEC.Text = @"Новосибирская ТЭЦ-5";
            //
            TableLayoutPanel tlpYear = new TableLayoutPanel();
            tlpYear.Dock = DockStyle.Fill;
            tlpYear.AutoSize = true;
            tlpYear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            tlpYear.Controls.Add(lblyearDGV, 0, 0);
            tlpYear.Controls.Add(lblTEC, 0, 1);
            tlpYear.Controls.Add(dgvYear, 0, 2);
            this.Controls.Add(tlpYear, 0, posRow = posRow + 1);
            this.SetColumnSpan(tlpYear, 4); this.SetRowSpan(tlpYear, 7);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString());
            ////сборка всех элементов
            //TableLayoutPanel tlpAllComp = new TableLayoutPanel();
            //tlpAllComp.Dock = DockStyle.Fill;
            //tlpAllComp.AutoSize = true;
            //tlpAllComp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            //tlpAllComp.Controls.Add(tlp, 0, 0);
            //tlpAllComp.Controls.Add(tlpPeriod, 0, 1);
            //tlpAllComp.Controls.Add(tlpValue, 0, 2);
            //tlpAllComp.Controls.Add(tlpButton, 0, 3);
            //tlpAllComp.Controls.Add(tlpYear, 0, 4);
            //this.Controls.Add(tlpAllComp, 0, posRow);
            //this.SetColumnSpan(tlpAllComp, 4); this.SetRowSpan(tlpAllComp, 20);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelAutobook_btnHistory_Click);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err"></param>
        /// <param name="strErr"></param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr, int i)
        {
            err = 0;
            strErr = string.Empty;
            //
            Session.New();
            //изменение начальной даты
            arQueryRanges[0] = new DateTimeRange(arQueryRanges[0].Begin.AddDays(-(arQueryRanges[0].Begin.Day - 1 - i)), arQueryRanges[0].End.AddDays(-(arQueryRanges[0].End.Day - 2 - i)));
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar
                (
                Type
                , ActualIdPeriod
                , CountBasePeriod
                , arQueryRanges
               , out err
                );

            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDef(ActualIdPeriod, out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    // , получить входные для расчета значения для возможности редактирования
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

        private void setValues()
        {

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

                for (int i = 0; i < monthIsDays(Session.m_rangeDatetime.Begin.Month); i++)
                {

                    // установить значения в таблицах для расчета, создать новую сессию
                    // предыдущая сессия удалена в 'clear'
                    setValues(HandlerDb.GetDateTimeRangeValuesVar(), out err, out errMsg, i);

                    if (err == 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();
                        // отобразить значкения
                        //dgvAB.ShowValues(m_TableEdit, m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]);
                    }
                    else
                    {
                        // в случае ошибки "обнулить" идентификатор сессии
                        deleteSession();
                        throw new Exception(@"PanelTaskTepValues::updatedataValues() - " + errMsg);
                    }
                    //deleteSession();
                }
            }
            else
                ;

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
            else
                ;
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

        public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

        public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

        protected System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableOrigin[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }

        protected System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableEdit[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }

        /// <summary>
        /// Обработчик события - изменение дата/время окончания периода
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
        {
            HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;
            //m_dtRange.Set(hdtpEnd.LeadingValue, hdtpEnd.Value);

            if (!(DateTimeRangeValue_Changed == null))
                DateTimeRangeValue_Changed(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
            else
                ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idPeriod"></param>
        public void SetPeriod(ID_PERIOD idPeriod)
        {
            HDateTimePicker hdtpBtimePer = Controls.Find(INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
            , hdtpEndtimePer = Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker;
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
                    hdtpEndtimePer.Value = hdtpBtimePer.Value.AddDays(monthIsDays(hdtpBtimePer.Value.Month));
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
                {
                    HandlerDb.InitSession(out err);
                }
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
            (Controls.Find(INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);

            if (err == 0)
            {
                try
                {
                    //initialize();
                    //Заполнить элемент управления с часовыми поясами
                    ctrl = Controls.Find(PanelTaskAutobook.INDEX_CONTROL.CBX_TIMEZONE.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.TIMEZONE].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"NAME_SHR"]);
                    // порядок именно такой (установить 0, назначить обработчик)
                    //, чтобы исключить повторное обновление отображения
                    (ctrl as ComboBox).SelectedIndex = 2; //??? требуется прочитать из [profile]
                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_SelectedIndexChanged);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    ctrl = Controls.Find(PanelTaskAutobook.INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0];
                    foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                        (ctrl as ComboBox).Items.Add(r[@"DESCRIPTION"]);

                    (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                    (ctrl as ComboBox).SelectedIndex = 1; //??? требуется прочитать из [profile]
                    Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][1]);//??
                    SetPeriod(Session.m_currIdPeriod);
                    (ctrl as ComboBox).Enabled = false;

                    ////// отобразить значения
                    //updateDataValues();

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
            fillDaysGrid(dtBegin, dtBegin.Month);
            Session.SetRangeDatetime(dtBegin, dtEnd);
            //// при наличии признака - загрузить/отобразить значения из БД
            //if (s_bAutoUpdateValues == true)
            //    updateDataValues();
            //else ;
        }

        /// <summary>
        /// заполнение грида датами
        /// </summary>
        /// <param name="date">тек.дата</param>
        /// <param name="numMonth">номер месяца</param>
        private void fillDaysGrid(DateTime date, int numMonth)
        {
            DateTime dt = new DateTime(date.Year, date.Month, 1);
            dgvAB.ClearRows();

            for (int i = 0; i < monthIsDays(numMonth); i++)
            {
                dgvAB.AddRow();
                dgvAB.Rows[i].Cells[0].Value = dt.AddDays(i).ToShortDateString();
            }
            dgvAB.Rows[date.Day - 1].Selected = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCtrl"></param>
        /// <param name="bClose"></param>
        protected void clear(int iCtrl = (int)PanelTaskAutobook.INDEX_CONTROL.UNKNOWN, bool bClose = false)
        {
            ComboBox cbx = null;
            PanelTaskAutobook.INDEX_CONTROL indxCtrl = (PanelTaskAutobook.INDEX_CONTROL)iCtrl;

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

                cbx = Controls.Find(PanelTaskAutobook.INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0] as ComboBox;
                cbx.SelectedIndexChanged -= cbxPeriod_SelectedIndexChanged;
                cbx.Items.Clear();

                cbx = Controls.Find(PanelTaskAutobook.INDEX_CONTROL.CBX_TIMEZONE.ToString(), true)[0] as ComboBox;
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
            Session.SetCurrentPeriod((ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD][(Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0] as ComboBox).SelectedIndex]);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            SetPeriod(Session.m_currIdPeriod);
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
            //if (!(PanelManagement == null))
            if (active == true)
                DateTimeRangeValue_Changed += new DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
            else
                if (active == false)
                    DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    ;
            //else
            //    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
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
                HandlerDb.GetQueryTimePeriods (m_strIdPeriods)
                //TIMEZONE
                , HandlerDb.GetQueryTimezones (m_strIdTimezones)
                // список компонентов
                , HandlerDb.GetQueryCompList()
                // параметры расчета
                , HandlerDb.GetQueryParameters(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
                //// настройки визуального отображения значений
                //, @""
                // режимы работы
                //, HandlerDb.GetQueryModeDev()
                //// единицы измерения
                //, m_handlerDb.GetQueryMeasures ()
                // коэффициенты для единиц измерения
                , HandlerDb.GetQueryRatio ()
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
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.OUTVALUES]
                , @"ID_PUT, ID_TIME"
                , m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , out err);
        }

        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Copy();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 23;
            register(23, typeof(PanelTaskAutobook), @"Задача", @"Учет активной э/э");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

