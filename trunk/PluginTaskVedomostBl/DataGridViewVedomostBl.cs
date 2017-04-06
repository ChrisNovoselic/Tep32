using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    partial class PanelTaskVedomostBl
    {
        /// <summary>
        /// Индекс уровней заголовков представлений
        /// </summary>
        protected enum LEVEL_HEADER
        {
            UNKNOW = -1
            , TOP, MIDDLE, LOW
                , COUNT
        }

        protected struct HEADER
        {
            public string src;

            public string[] values;

            public int idNAlg;

            public int idComponent;

            public int idPut;            

            public override string ToString()
            {
                return string.Format(@"idNAlg={0}, idComponent={1}, idPut={2}, src={3}, values.Length={4}"
                    , idNAlg, idComponent, idPut, src, values.Length);
            }
        }
        /// <summary>
        /// класс вьюхи
        /// </summary>
        protected class DataGridViewVedomostBl : DataGridViewValues
        {
            /// <summary>
            /// Количество строк/столбцов(уровней) в заголовках столбцов
            /// </summary>
            static int s_iCountColumn,
                s_GroupHeaderCount = s_listGroupHeaders.Count;            
            /// <summary>
            /// словарь названий заголовков 
            /// верхнего и среднего уровней
            /// </summary>
            public List<string> m_listTextHeaderTop = new List<string>()
                , m_listTextHeaderMiddle = new List<string>();
            /// <summary>
            /// словарь соотношения заголовков
            /// </summary>
            public int[] m_arCounterHeaderTop = new int[] { }
                , m_arCounterHeaderMiddle = new int[] { };
            /// <summary>
            /// перечисление уровней заголовка грида
            /// </summary>
            public enum INDEX_HEADER
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //public enum INDEX_SERVICE_COLUMN : uint { ALG = 0, DATE, COUNT }

            private List<HEADER> m_listHeaders;
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="nameDGV">Идентификатор оборудования - блока, данные которого отображаются в текущем представлении</param>
            public DataGridViewVedomostBl(HandlerDbTaskCalculate.TECComponent comp)
                : base (ModeData.DATETIME)
            {
                Tag = comp;

                InitializeComponents();
            }

            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Name = string.Format(@"DGV_BLOCK_{0}", IdComponent);
                // Dock, MultiSelect, SelectionMode, ColumnHeadersVisible, ColumnHeadersHeightSizeMode, AllowUserToResizeColumns, AllowUserToResizeRows, AllowUserToAddRows, AllowUserToDeleteRows, AllowUserToOrderColumns
                // - устанавливаются в базовом классе
                //Не отображать заголовки строк
                RowHeadersVisible = true;
                
                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
                ColumnHeadersHeight = ColumnHeadersHeight * s_GroupHeaderCount;//высота от нижнего(headerText)

                ScrollBars = ScrollBars.None;
            }

            public int IdComponent { get { return ((HandlerDbTaskCalculate.TECComponent)Tag).m_Id; } }

            public override void BuildStructure(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                AddHeaderColumns(getListHeaders(listNAlgParameter, listPutParameter)); // cловарь заголовков
                ////??? каждый раз получаем полный список и выбираем необходимый
                //dictVisualSett = getVisualSettingsOfIdComponent((int)dgv.Tag);

                AddColumns(listPutParameter);

                AddRows(DatetimeStamp, TimeSpan.FromDays(1));

                ResizeControls();

                ConfigureColumns();
            }

            /// <summary>
            /// Структура для описания добавляемых столбцов
            /// </summary>
            public class COLUMN_PROPERTY
            {
                /// <summary>
                /// Параметр в алгоритме расчета, связанный с компонентом станции
                /// </summary>
                public HandlerDbTaskCalculate.PUT_PARAMETER m_putParameter;
                /// <summary>
                /// Имя колонки
                /// </summary>
                public string m_textMiddleHeader;
                /// <summary>
                /// Текст в колонке
                /// </summary>
                public string m_textLowHeader;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public string m_textTopHeader;
            }

            /// <summary>
            /// Добавление столбца
            /// </summary>
            /// <param name="col_prop">Свойство столбца (Tag)</param>
            private void addColumn(COLUMN_PROPERTY col_prop)
            {
                DataGridViewTextBoxColumn column;
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    column = new DataGridViewTextBoxColumn();
                    column.Tag = col_prop;
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    column.Frozen = true;
                    column.Visible = col_prop.m_putParameter.m_bVisibled;
                    column.ReadOnly = false;
                    column.Name = col_prop.m_textMiddleHeader;
                    column.HeaderText = col_prop.m_textLowHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    //column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewVedomostBl::addColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        
            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bReadOnly">true/false</param>
            public bool ReadOnlyColumns
            {
                set {
                    foreach (DataGridViewColumn col in Columns)
                        //if (col.Name == nameCol)
                        col.ReadOnly = value;
                }
            }

            /// <summary>
            /// ??? зачем в аргументе указывать объект Подготовка параметров к рисовке хидера
            /// </summary>
            /// <param name="dgv">активное окно отображения данных</param>
            public void ConfigureColumns()
            {
                int cntCol = 0;
                formingTitleLists();

                formingRelationsHeading();

                foreach (DataGridViewColumn col in Columns)
                    if (col.Visible == true)
                        cntCol++;

                s_iCountColumn = cntCol * WIDTH_COLUMN + WIDTH_COLUMN / s_listGroupHeaders.Count;

                Paint += new PaintEventHandler(dataGridView_onPaint);
            }

            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            private void formingTitleLists()
            {
                string oldItem = string.Empty;
                COLUMN_PROPERTY col_prop;
                //List<string> listTop = new List<string>()
                //    , listMiddle = new List<string>();

                //if (m_headerTop.ContainsKey(idTG))
                //    m_headerTop.Remove(idTG);

                m_listTextHeaderTop.Clear();

                foreach (DataGridViewColumn col in Columns) {
                    col_prop = (COLUMN_PROPERTY)col.Tag;

                    if (!(col_prop.m_putParameter.m_idNAlg < 0))
                        if (col.Visible == true)
                            if (col_prop.m_textTopHeader.Equals(string.Empty) == false)
                                if (col_prop.m_textTopHeader.Equals(oldItem) == false)
                                {
                                    oldItem = col_prop.m_textTopHeader;
                                    m_listTextHeaderTop.Add(col_prop.m_textTopHeader);
                                }
                                else;
                            else
                                m_listTextHeaderTop.Add(col_prop.m_textTopHeader);
                        else;
                    else;
                }

                //m_headerTop.Add(idTG, listTop);

                //if (m_headerMiddle.ContainsKey(idTG))
                //    m_headerMiddle.Remove(idTG);
                //else
                //    ;

                m_listTextHeaderMiddle.Clear();

                foreach (DataGridViewColumn col in Columns) {
                    col_prop = (COLUMN_PROPERTY)col.Tag;

                    if (!(col_prop.m_putParameter.m_idNAlg < 0))
                        if (col.Visible == true)
                            if (col.Name != oldItem) {
                                oldItem = col.Name;
                                m_listTextHeaderMiddle.Add(col.Name);
                            } else
                                ;
                        else
                            ;
                    else
                        ;
                }

                //m_headerMiddle.Add(idTG, listMiddle);
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            private void formingRelationsHeading()
            {
                string oldItem = string.Empty;
                int indx = 0
                    , untdCol = -1;

                COLUMN_PROPERTY col_prop;

                m_arCounterHeaderTop = new int[m_listTextHeaderTop/*[idDgv]*/.Count];
                m_arCounterHeaderMiddle = new int[m_listTextHeaderMiddle/*[idDgv]*/.Count];

                //if (m_arIntTopHeader.ContainsKey(idDgv))
                //    m_arIntTopHeader.Remove(idDgv);
                //else
                //    ;

                foreach (var item in m_listTextHeaderTop/*[idDgv]*/) {
                    untdCol = 0;

                    foreach (DataGridViewColumn col in Columns) {
                        col_prop = (COLUMN_PROPERTY)col.Tag;

                        if (col.Visible == true)
                            if (col_prop.m_textTopHeader.Equals(item) == true)
                                if (string.IsNullOrEmpty(item) == false)
                                    untdCol++;
                                else {
                                    untdCol = 1;

                                    break;
                                }
                            else
                                ;
                        else
                            ;
                    }

                    m_arCounterHeaderTop[indx] = untdCol;
                    indx++;
                }

                indx = 0;                

                foreach (var item in m_listTextHeaderMiddle/*[idDgv]*/) {
                    untdCol = 0;

                    foreach (DataGridViewColumn col in Columns) {
                        col_prop = (COLUMN_PROPERTY)col.Tag;

                        if (col_prop.m_putParameter.m_idNAlg > -1)
                            if (item == col.Name)
                                untdCol++;
                            else
                                if (untdCol > 0)
                                    break;
                                else
                                    ;
                        else
                            ;
                    }

                    m_arCounterHeaderMiddle[indx] = untdCol;
                    indx++;
                }
            }

            /// <summary>
            /// ??? зачем в 1-ом аргументе указывать объект Скрыть/показать столбцы из списка групп
            /// </summary>
            /// <param name="listHeaderTop">лист с именами заголовков</param>
            /// <param name="isCheck">проверка чека</param>
            public void SetHeaderVisibled(List<string> listHeaderTop, bool isCheck)
            {
                COLUMN_PROPERTY col_prop;

                try {
                    foreach (var item in listHeaderTop)
                        foreach (DataGridViewColumn col in Columns) {
                            col_prop = (COLUMN_PROPERTY)col.Tag;

                            if (col_prop.m_textTopHeader.Equals(item) == true)
                                col.Visible = isCheck;
                            else
                                ;
                        }
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DataGridViewVedomostBl::SetHeaderVisibled () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                ConfigureColumns();
            }

            public static int PADDING_COLUMN = 10;

            public static int HEIGHT_HEADER = 70;

            private static int WIDTH_COLUMN_DEFAULT = 65;

            private int WIDTH_COLUMN_DATE { get { return RowHeadersVisible == true ? 0 : WIDTH_COLUMN_DEFAULT; } }

            private int WIDTH_COLUMN { get { return Columns[RowHeadersVisible == true ? 0 : 1].Width; }  }

            /// <summary>
            /// обработчик события перерисовки грида(построение шапки заголовка)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            void dataGridView_onPaint(object sender, PaintEventArgs e)
            {
                int indxCol = 0
                    , height = -1;
                
                // Область, занятая родительским заголовком столбца
                Rectangle rectParentColumn
                    , r1 = new Rectangle()
                    , r2 = new Rectangle();
                Pen pen;
                StringFormat format;

                pen = new Pen(Color.Black);
                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                //
                for (int i = 0; i < Columns.Count; i++)
                    if ((GetCellDisplayRectangle(i, -1, true).Height > 0)
                        & (GetCellDisplayRectangle(i, -1, true).X > 0))
                    {
                        rectParentColumn = GetCellDisplayRectangle(i, -1, true);
                        r1 = rectParentColumn;
                        r2 = rectParentColumn;
                        break;
                    }

                height = r1.Height / s_GroupHeaderCount;

                foreach (var item in m_listTextHeaderMiddle/*[idComp]*/)
                {
                    //get the column header cell
                    r1.Width = m_arCounterHeaderMiddle[m_listTextHeaderMiddle/*[idComp]*/.ToList().IndexOf(item)] * WIDTH_COLUMN;
                    r1.Height = height + 3;//??? 

                    if ((m_listTextHeaderMiddle/*[idComp]*/.ToList().IndexOf(item) - 1) > -1)
                        r1.X = r1.X + m_arCounterHeaderMiddle[m_listTextHeaderMiddle/*[idComp]*/.ToList().IndexOf(item) - 1] * WIDTH_COLUMN;
                    else
                    {
                        r1.X += WIDTH_COLUMN_DATE;
                        r1.Y = r1.Y + r1.Height;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), r1);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      r1,
                      format);
                    e.Graphics.DrawRectangle(pen, r1);
                }

                foreach (var item in m_listTextHeaderTop/*[idComp]*/)
                {
                    //get the column header cell
                    r2.Width = m_arCounterHeaderTop[indxCol] * WIDTH_COLUMN;
                    r2.Height = height + 2;//??? 

                    if (indxCol - 1 > -1)
                        r2.X = r2.X + m_arCounterHeaderTop[indxCol - 1] * WIDTH_COLUMN;
                    else
                    {
                        r2.X += WIDTH_COLUMN_DATE;
                        r2.Y += r2.Y;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), r2);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      r2,
                      format);
                    e.Graphics.DrawRectangle(pen, r2);
                    indxCol++;
                }

                //(sender as DGVVedomostBl).Paint -= new PaintEventHandler(dataGridView1_Paint);
            }

            public void AddHeaderColumns(List<HEADER> listHeaders)
            {
                m_listHeaders = new List<HEADER>(listHeaders);
            }

            public void AddColumns(List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                int i = -1;

                if (RowHeadersVisible == false)
                    addColumn(new DataGridViewVedomostBl.COLUMN_PROPERTY {
                        m_textTopHeader = string.Empty
                        , m_textMiddleHeader = "DATE"
                        , m_textLowHeader = "Дата"
                        , m_putParameter = new HandlerDbTaskCalculate.PUT_PARAMETER() {
                            m_idNAlg = -1
                            , m_Id = -1
                            , m_component = new HandlerDbTaskCalculate.TECComponent() {
                                m_Id = -1
                                , m_idOwner = -1
                                , m_nameShr = string.Empty
                            }
                            , m_bEnabled = false
                            , m_bVisibled = true
                        }
                    });
                else
                    ;

                for (int col = 0; col < m_listHeaders.Count; col++) {
                    addColumn(new DataGridViewVedomostBl.COLUMN_PROPERTY {
                            m_textTopHeader = m_listHeaders[col].values[(int)DataGridViewVedomostBl.INDEX_HEADER.TOP].ToString()
                            , m_textMiddleHeader = m_listHeaders[col].values[(int)DataGridViewVedomostBl.INDEX_HEADER.MIDDLE].ToString()
                            , m_textLowHeader = m_listHeaders[col].values[(int)DataGridViewVedomostBl.INDEX_HEADER.LOW].ToString()
                            , m_putParameter = listPutParameter[col]
                        });
                }
            }

            /// <summary>
            /// Настройка размеров контролов отображения
            /// </summary>
            public void ResizeControls()
            {
                int cntVisibleColumns = 0
                    , width = -1
                    , height = -1;
                //??? каждый раз устанавливать размеры для столбцов
                // , однако для них установлено свойство 'NoResizeble'
                foreach (DataGridViewColumn col in Columns) {
                    if ((Columns.IndexOf(col) > 0)
                        || (RowHeadersVisible == true))
                    // для всех столбцов, кроме даты (1-ый столбец)
                    // , если отображается заголовок строки, то дата в хаголовке строки
                        col.Width = WIDTH_COLUMN_DEFAULT;
                    else
                    // столбец - дата
                    // , только если не отображается заголовок строки
                        col.Width = WIDTH_COLUMN_DATE;

                    if (col.Visible == true)
                        cntVisibleColumns++;
                    else
                        ;
                }

                width = cntVisibleColumns * WIDTH_COLUMN + PADDING_COLUMN + (RowHeadersVisible == true ? RowHeadersWidth : 0);
                height = (Rows.Count) * Rows[0].Height + HEIGHT_HEADER;

                this.Size = new Size(width + 2, height);
            }

            /// <summary>
            /// Обработчик события - перерисовка ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            static void dataGridView_onCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
            {
                if (e.RowIndex == -1 && e.ColumnIndex > -1)
                {
                    e.PaintBackground(e.CellBounds, false);

                    Rectangle r2 = e.CellBounds;
                    r2.Y += e.CellBounds.Height / s_GroupHeaderCount;
                    r2.Height = e.CellBounds.Height / s_GroupHeaderCount;
                    e.PaintContent(r2);
                    e.Handled = true;
                }
            }

            /// <summary>
            /// Отобразить данные в представлении
            /// </summary>
            /// <param name="tableOrigin">таблица с данными</param>
            /// <param name="typeValues">тип загружаемых данных</param>
            public void ShowValues(DataTable tableOrigin, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                DataTable tableOriginCopy = new DataTable();
                int cntDay = -1
                   , hoursOffSet
                   , iCol = 0;

                DataRow[] editRow = null;
                NALG_PROPERTY nalg_prop;
                COLUMN_PROPERTY col_prop;

                tableOriginCopy = tableOrigin.Copy();
                ClearValues();

                if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD == (int)typeValues)
                    if (s_flagBl)
                        hoursOffSet = -1 * (-(TimeZoneInfo.Local.BaseUtcOffset.Hours + 1) + 24);
                    else
                        hoursOffSet = (s_currentOffSet / 60);
                else
                    hoursOffSet = s_currentOffSet / 60;

                if (tableOriginCopy.Rows.Count > 0)
                    foreach (DataGridViewColumn col in Columns) {
                        col_prop = (COLUMN_PROPERTY)col.Tag;
                        nalg_prop = m_dictNAlgProperties[col_prop.m_putParameter.m_idNAlg];

                        try {
                            editRow = tableOriginCopy.Select(string.Format(tableOriginCopy.Locale, "ID_PUT = " + col_prop.m_putParameter.m_Id));
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, string.Format(@"DataGridViewVedomostBl::ShowValues () - ошибка выборки данных ID_PUT={0}...", col_prop.m_putParameter.m_Id), Logging.INDEX_MESSAGE.NOT_SET);
                        }

                        for (int i = 0; i < editRow.Count(); i++) {
                            //??? почему сравниваются строки, а не значения
                            if (Convert.ToDateTime(editRow[i][@"WR_DATETIME"]).AddHours(hoursOffSet).ToShortDateString() ==
                                Rows[i].Cells["Date"].Value.ToString()) {
                                Rows[i].Cells[iCol].Value =
                                    ((double)editRow[i][@"VALUE"]).ToString(nalg_prop.FormatRound, CultureInfo.InvariantCulture);
                            } else
                                ;
                        }

                        try {
                            if (nalg_prop.m_sAverage == 0)
                                Rows[RowCount - 1].Cells[iCol].Value =
                                    summaColumnValues(col.Index, out cntDay).ToString(nalg_prop.FormatRound, CultureInfo.InvariantCulture);
                            else
                                Rows[RowCount - 1].Cells[iCol].Value =
                                    averageColumnValues(col.Index, out cntDay).ToString(nalg_prop.FormatRound, CultureInfo.InvariantCulture);
                        } catch (Exception e) {
                            Logging.Logg().Exception(e
                                , string.Format("???DataGridViewVedomostBl::ShowValues () - усредненние данных по столбцу name={0}", col_prop.m_textTopHeader)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        
                        iCol++;
                    } // цикл по столбцам представления
                else
                    ;
            }

            /// <summary>
            /// Получение суммы по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>сумма по столбцу</returns>
            private double summaColumnValues(int indxCol, out int counter)
            {
                counter = 0;
                double dblRes = 0F;

                try {
                    foreach (DataGridViewRow row in Rows)
                        if (row.Index < Rows.Count - 1)
                        // все кроме крайней строки
                            if ((!(row.Cells[indxCol].Value == null))
                                && (string.IsNullOrEmpty(row.Cells[indxCol].Value.ToString()) == false)) {
                            // только, если есть значение для разбора
                                dblRes += HMath.doubleParse(row.Cells[indxCol].Value.ToString());

                                counter++;
                            } else
                                ;
                        else
                            ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"PanelTaskVedomostBl::summaColumnValues () - суммирования столбца №{0}...", indxCol), Logging.INDEX_MESSAGE.NOT_SET);
                }

                return dblRes;
            }

            /// <summary>
            /// Получение среднего по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>среднее по столбцу</returns>
            private double averageColumnValues(int indxCol, out int counter)
            {
                counter = 0;
                double summaValue = summaColumnValues(indxCol, out counter)
                    , dblRes = 0F;

                if (counter > 0)
                    dblRes = summaValue / counter;
                else
                    dblRes = double.NaN;

                return dblRes;
            }

            /// <summary>
            /// Формирование таблицы данных с отображения
            /// </summary>
            /// <param name="tableSourceOrigin">таблица с оригинальными данными</param>
            /// <param name="idSession">номер сессии пользователя</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>таблица с новыми данными с вьюхи</returns>
            public DataTable FillTableToSave(DataTable tableSourceOrigin, int idSession, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int i = 0,
                    idAlg = -1
                    , hoursOffSet
                    , vsRatioValue = -1
                    , quality = 0,
                    indexPut = 0;
                double valueToRes = 0;
                DateTime dtVal;
                NALG_PROPERTY nalg_prop;
                COLUMN_PROPERTY col_prop;

                DataTable tableSourceEdit = new DataTable();
                tableSourceEdit.Columns.AddRange(
                    new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    }
                );

                if (s_flagBl)
                    hoursOffSet = 1 * (-(TimeZoneInfo.Local.BaseUtcOffset.Hours + 1) + 24);
                else
                    hoursOffSet = (s_currentOffSet / 60);

                foreach (DataGridViewColumn col in Columns) {
                    col_prop = (COLUMN_PROPERTY)col.Tag;
                    nalg_prop = m_dictNAlgProperties[col_prop.m_putParameter.m_idNAlg];

                    if (col_prop.m_putParameter.m_idNAlg > 0) {
                        foreach (DataGridViewRow row in Rows) {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (string.IsNullOrEmpty(row.Cells[col.Index].Value.ToString()) == false) {
                                    idAlg = col_prop.m_putParameter.m_idNAlg;
                                    valueToRes = HPanelTepCommon.AsParseToF(row.Cells[col.Index].Value.ToString());
                                    vsRatioValue = nalg_prop.m_vsRatio;
                                    valueToRes *= Math.Pow(10F, vsRatioValue);
                                    dtVal = (DateTime)row.Tag;
                                    //??? в этом методе сортируется табл. по 2-ум полям
                                    //!!! срочно исключить, т.к. внутри 2-ух циклов
                                    quality = diffRowsInTables(tableSourceOrigin, valueToRes, i, nalg_prop.FormatRound, typeValues);

                                    tableSourceEdit.Rows.Add(new object[] {
                                        col_prop.m_putParameter.IdComponent
                                        , idSession
                                        , quality
                                        , valueToRes
                                        , dtVal.AddMinutes(-s_currentOffSet).ToString("F",tableSourceEdit.Locale)
                                        , i
                                    });

                                    i++;
                                } else
                                // в ячейке не валидное значение, не может быть определено
                                    ;
                            else
                            // крайняя строка (ИТОГО за период)
                                ;
                        } // цикл по строкам (датам) в представлении

                        indexPut++;
                    } else
                    // идентификатор параметра в алгоритме расчета не валидный
                        ;
                } // цикл по столбцам (параметрам в алгоритме расчета, связанным с компонентом) в представлении

                tableSourceEdit = sortDataTable(tableSourceEdit, "WR_DATETIME");

                return tableSourceEdit;
            }

            /// <summary>
            /// ??? Сортировка таблицы по столбцу
            /// </summary>
            /// <param name="table">таблица для сортировки</param>
            /// <param name="sortStr">имя столбца/ов для сортировки</param>
            /// <returns>отсортированная таблица</returns>
            private DataTable sortDataTable(DataTable table, string colSort)
            {
                DataView dView = null;
                string sortExpression = string.Empty;

                try {
                    dView = table.DefaultView;
                    sortExpression = string.Format(colSort);

                    dView.Sort = sortExpression;
                    table = dView.ToTable();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DataGridViewVedomostBl::sortDataTable () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

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
            private int diffRowsInTables(DataTable origin, double editValue, int i, string formatRound, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int quality = 1;
                double originValues;
                //??? зачем сортировка
                origin = sortDataTable(origin, "ID_PUT, WR_DATETIME");

                if (origin.Rows.Count - 1 < i)
                    originValues = 0;
                else
                    originValues =
                        //HPanelTepCommon.AsParseToF(
                        HMath.doubleParse(
                            origin.Rows[i]["VALUE"].ToString()
                        );

                switch (typeValues)
                {
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE:
                        //??? почему сравниваются строки
                        if (originValues.ToString(formatRound, CultureInfo.InvariantCulture).Equals(editValue.ToString().Trim()) == false)
                            quality = 2;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_LOAD:
                        quality = 1;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                return quality;
            }

            /// <summary>
            /// Возвратить список с заголовками представления для отображения значений
            /// </summary>
            /// <param name="arlistStr">лист парамтеров</param>
            /// <param name="rowPars">таблица с данными</param>
            /// <returns>Список массивов строк-заговков</returns>
            private List<HEADER> getListHeaders(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                //List<string[]> listRes = new List<string[]> { };
                List<HEADER> listHeaderRes = new List<HEADER> { };

                int cntHeader = 0;
                HandlerDbTaskCalculate.NALG_PARAMETER nalg_prop;
                string[] arStrHeader;
                List<HEADER> listHeaderSrc;            

                listHeaderSrc = new List<HEADER>(listPutParameter.Count);

                nalg_prop = null;

                foreach (HandlerDbTaskCalculate.PUT_PARAMETER put in listPutParameter) {
                    if ((nalg_prop == null)
                        || ((!(nalg_prop == null)) && (!(nalg_prop.m_Id == put.m_Id))))
                        nalg_prop = listNAlgParameter.Find(nAlg => { return nAlg.m_Id == put.m_idNAlg; });
                    else
                        ;

                    listHeaderSrc.Add(new HEADER() {
                        idNAlg = nalg_prop.m_Id
                        , idComponent = put.IdComponent
                        , idPut = put.m_Id
                        , src = nalg_prop
                            //.m_strNameShr
                            .m_nAlg
                        , values =
                            nalg_prop
                                //.m_strNameShr
                                .m_nAlg
                                .Split('.', ',')
                    });
                }

                listHeaderRes.Clear();

                for (int j = 0; j < listHeaderSrc.Count; j++) {
                    //??? почему 3
                    //  , может у всех по 3(количество уровней) элемента
                    if (listHeaderSrc[j].values.Length < 3)
                        arStrHeader = new string[listHeaderSrc[j].values.Length + 1];
                    else
                        arStrHeader = new string[listHeaderSrc[j].values.Length];

                    cntHeader = 0;

                    for (int level = listHeaderSrc[j].values.Length - 1; level > -1; level--) {
                        if ((!(nalg_prop == null))
                            && (!(nalg_prop.m_Id == listHeaderSrc[j].idNAlg)))
                            nalg_prop = listNAlgParameter.Find(nAlg => { return nAlg.m_Id == listHeaderSrc[j].idNAlg; });
                        else
                            ;

                        switch ((LEVEL_HEADER)level) {
                            case LEVEL_HEADER.TOP:
                                for (int t = 0; t < s_listGroupHeaders.Count; t++) {
                                    for (int n = 0; n < s_listGroupHeaders[t].Count; n++) {
                                        cntHeader++;

                                        try {
                                            if (int.Parse(listHeaderSrc[j].values.ElementAt((int)LEVEL_HEADER.TOP)) == cntHeader) {
                                                arStrHeader[level] = s_listGroupHeaders[t][n];

                                                listHeaderRes.Add(new HEADER() {
                                                    idNAlg = listHeaderSrc[j].idNAlg
                                                    , idComponent = listHeaderSrc[j].idComponent
                                                    , idPut = listHeaderSrc[j].idPut
                                                    , src = nalg_prop
                                                        //.m_strNameShr
                                                        .m_nAlg
                                                    , values = arStrHeader
                                                });

                                                t = s_listGroupHeaders.Count; // прервать внешний цикл
                                                break;
                                            } else
                                                ;
                                        } catch (Exception e) {
                                            Logging.Logg().Exception(e, string.Format(@"PanelTaskVedomostBl::getListHeaders () - разбор TOP HEADER=[{0}]...", listHeaderSrc[j].ToString()), Logging.INDEX_MESSAGE.NOT_SET);
                                        }
                                    }
                                }
                                break;
                            case LEVEL_HEADER.MIDDLE:
                                // ??? почему < 3
                                if (listHeaderSrc[j].values.Length < 3)
                                    arStrHeader[level + 1] = string.Empty;
                                else
                                    ;

                                arStrHeader[(int)LEVEL_HEADER.MIDDLE] = nalg_prop.m_strNameShr; // listHeader[j].src
                                break;
                            case LEVEL_HEADER.LOW:
                                arStrHeader[level] = nalg_prop.m_strDescription;
                                break;
                            default:
                                break;
                        }
                    } // for - level
                }

                //var linqRes = from header in listHeaderRes select new string[] { header.values };
                //listRes = linqRes as List<string[]>;

                return listHeaderRes;
            }
        }
    }
}
