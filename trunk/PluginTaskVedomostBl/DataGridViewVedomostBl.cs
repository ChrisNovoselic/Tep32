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

        private struct HEADER
        {
            public string src;

            public string[] values;

            public int idNAlg;

            public int idPut;
        }
        /// <summary>
        /// Возвратить список с заголовками представления для отображения значений
        /// </summary>
        /// <param name="arlistStr">лист парамтеров</param>
        /// <param name="rowPars">таблица с данными</param>
        /// <returns>Список массивов строк-заговков</returns>
        private List<string[]> getListHeaders(List<NALG_PARAMETER>listNAlgParameter, List<PUT_PARAMETER>listPutParameter)
        {
            List<HEADER> listRes = new List<HEADER> { };

            int cntHeader = 0;
            NALG_PARAMETER nalg_prop;
            string[] arStrHeader;
            List<HEADER> listHeader;            

            listHeader = new List<HEADER>(listPutParameter.Count);

            nalg_prop = null;

            foreach (PUT_PARAMETER put in listPutParameter) {
                if ((nalg_prop == null)
                    || ((!(nalg_prop == null)) && (!(nalg_prop.m_Id == put.m_Id))))
                    nalg_prop = listNAlgParameter.Find(nAlg => { return nAlg.m_Id == put.m_idNAlg; });
                else
                    ;

                listHeader.Add(new HEADER() {
                    idNAlg = nalg_prop.m_Id
                    , idPut = put.m_Id
                    , src = nalg_prop.m_strNameShr
                    , values = nalg_prop.m_strNameShr.ToString().Split('.', ',')
                });
            }

            listRes.Clear();

            for (int j = 0; j < listHeader.Count; j++) {
                //??? почему 3
                //  , может у всех по 3(количество уровней) элемента
                if (listHeader[j].values.Length < 3)
                    arStrHeader = new string[listHeader[j].values.Length + 1];
                else
                    arStrHeader = new string[listHeader[j].values.Length];

                cntHeader = 0;

                for (int level = listHeader[j].values.Length - 1; level > -1; level--) {
                    if ((!(nalg_prop == null))
                        && (!(nalg_prop.m_Id == listHeader[j].idNAlg)))
                        nalg_prop = listNAlgParameter.Find(nAlg => { return nAlg.m_Id == listHeader[j].idNAlg; });
                    else
                        ;

                    switch ((LEVEL_HEADER)level) {
                        case LEVEL_HEADER.TOP:
                            for (int t = 0; t < s_listGroupHeaders.Count; t++) {
                                for (int n = 0; n < s_listGroupHeaders[t].Count; n++) {
                                    cntHeader++;
                                    if (int.Parse(listHeader[j].values.ElementAt((int)LEVEL_HEADER.TOP)) == cntHeader) {
                                        arStrHeader[level] = s_listGroupHeaders[t][n];
                                        listRes.Add(new HEADER() {
                                            idNAlg = listHeader[j].idNAlg
                                            , idPut = listHeader[j].idPut
                                            , src = nalg_prop.m_strNameShr
                                            , values = arStrHeader
                                        });

                                        t = s_listGroupHeaders.Count; // прервать внешний цикл
                                        break;
                                    } else
                                        ;
                                }
                            }
                            break;
                        case LEVEL_HEADER.MIDDLE:
                            // ??? почему < 3
                            if (listHeader[j].values.Length < 3)
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

            return (from header in listRes select new { header.values }) as List<string[]>;
        }

        /// <summary>
        /// класс вьюхи
        /// </summary>
        protected class DataGridViewVedomostBl : DataGridViewValues
        {
            /// <summary>
            /// Количество строк/столбцов(уровней) с заголовках столбцов
            /// </summary>
            static int s_iCountColumn,
                s_GroupHeaderCount = s_listGroupHeaders.Count;
            /// <summary>
            /// Область, занятая родительским заголовком столбца
            /// </summary>
            private Rectangle rectParentColumn;
            /// <summary>
            /// словарь названий заголовков 
            /// верхнего и среднего уровней
            /// </summary>
            public Dictionary<int, List<string>> m_headerTop = new Dictionary<int, List<string>>()
                , m_headerMiddle = new Dictionary<int, List<string>>();
            /// <summary>
            /// словарь соотношения заголовков
            /// </summary>
            public Dictionary<int, int[]> m_arIntTopHeader = new Dictionary<int, int[]> { }
                , m_arMiddleCol = new Dictionary<int, int[]> { };
            /// <summary>
            /// перечисление уровней заголовка грида
            /// </summary>
            public enum INDEX_HEADER
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }
            /// <summary>
            /// ??? зачем Количество блоков
            /// </summary>
            public int BlockCount;
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //public enum INDEX_SERVICE_COLUMN : uint { ALG = 0, DATE, COUNT }

            private List<string[]> m_listHeaders;
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="nameDGV">Идентификатор оборудования - блока, данные которого отображаются в текущем представлении</param>
            public DataGridViewVedomostBl(int tag)
                : base (ModeData.DATETIME)
            {
                Tag = tag;

                InitializeComponents();
            }

            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Name = ((INDEX_CONTROL)Tag).ToString();
                Dock = DockStyle.None;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
                //Запрет изменения размера строк
                AllowUserToResizeRows = false;
                //Отменить возможность добавления строк
                AllowUserToAddRows = false;
                //Отменить возможность удаления строк
                AllowUserToDeleteRows = false;
                //Отменить возможность изменения порядка следования столбцов строк
                AllowUserToOrderColumns = false;
                //Не отображать заголовки строк
                RowHeadersVisible = false;
                //
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                AllowUserToResizeColumns = false;
                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
                ColumnHeadersHeight = ColumnHeadersHeight * s_GroupHeaderCount;//высота от нижнего(headerText)
                ScrollBars = ScrollBars.None;
            }

            public override void BuildStructure()
            {
            }

            /// <summary>
            /// Структура для описания добавляемых столбцов
            /// </summary>
            public class COLUMN_PROPERTY
            {
                /// <summary>
                /// Параметр в алгоритме расчета, связанный с компонентом станции
                /// </summary>
                public PUT_PARAMETER m_putParameter;
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

            public void AddRow(DateTime dtRow, bool bEnded)
            {
                //AddRow(dtRow);
            }

            /// <summary>
            /// Добавление столбца
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="textMiddle">имя колонки</param>
            /// <param name="headerText">текст заголовка</param>
            /// <param name="bVisible">видимость</param>
            private void addColumn(int idHeader, COLUMN_PROPERTY col_prop)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    DataGridViewColumn column = new DataGridViewColumn();
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
                    Logging.Logg().Exception(e, @"DGVVedBl::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            ///// <summary>
            ///// Добавление колонки
            ///// </summary>
            ///// <param name="idHeader">номер колонки</param>
            ///// <param name="col_prop">Структура для описания добавляемых столбцов</param>
            ///// <param name="bVisible">видимость</param>
            //public void AddColumn(int idHeader, COLUMN_PROPERTY col_prop, bool bVisible)
            //{
            //    int indxCol = -1; // индекс столбца при вставке
            //    DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

            //    try
            //    {
            //        if (m_dictPropertyColumns == null)
            //            m_dictPropertyColumns = new Dictionary<int, COLUMN_PROPERTY>();

            //        if (!m_dictPropertyColumns.ContainsKey(col_prop.m_idAlg))
            //            m_dictPropertyColumns.Add(col_prop.m_idAlg, col_prop);
            //        // найти индекс нового столбца
            //        // столбец для станции - всегда крайний
            //        //foreach (HDataGridViewColumn col in Columns)
            //        //    if ((col.m_iIdComp > 0)
            //        //        && (col.m_iIdComp < 1000))
            //        //    {
            //        //        indxCol = Columns.IndexOf(col);
            //        //        break;
            //        //    }

            //        HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_topHeader = col_prop.m_textTopHeader, m_IdAlg = idHeader, m_IdComp = col_prop.m_IdComp };
            //        alignText = DataGridViewContentAlignment.MiddleRight;

            //        if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
            //            ; // оставить значения по умолчанию
            //        else
            //        {// для добавлямых столбцов
            //            //if (idHeader < 0)
            //            //{// для служебных столбцов
            //            if (bVisible == true)
            //            {// только для столбца с [SYMBOL]
            //                alignText = DataGridViewContentAlignment.MiddleLeft;
            //            }
            //            column.Frozen = true;
            //            column.ReadOnly = true;
            //            //}
            //        }

            //        column.HeaderText = col_prop.hdrText;
            //        column.Name = col_prop.nameCol;
            //        column.DefaultCellStyle.Alignment = alignText;
            //        column.Visible = bVisible;

            //        if (!(indxCol < 0))
            //            Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
            //        else
            //            Columns.Add(column as DataGridViewTextBoxColumn);
            //    }
            //    catch (Exception e)
            //    {
            //        Logging.Logg().Exception(e, @"DataGridViewVedBl::AddColumn (idHeader=" + idHeader + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //}

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
                formingTitleLists((int)Tag);

                formRelationsHeading((int)Tag);

                foreach (DataGridViewColumn col in Columns)
                    if (col.Visible == true)
                        cntCol++;

                s_iCountColumn = cntCol * WIDTH_COL1 + WIDTH_COL1 / s_listGroupHeaders.Count;

                Paint += new PaintEventHandler(dataGridView_onPaint);
            }

            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            /// <param name="idTG">номер идТГ</param>
            private void formingTitleLists(int idTG)
            {
                string oldItem = string.Empty;
                COLUMN_PROPERTY col_prop;
                List<string> listTop = new List<string>()
                    , listMiddle = new List<string>();

                if (m_headerTop.ContainsKey(idTG))
                    m_headerTop.Remove(idTG);

                foreach (DataGridViewColumn col in Columns) {
                    col_prop = (COLUMN_PROPERTY)col.Tag;

                    if (!(col_prop.m_putParameter.m_idNAlg < 0))
                        if (col.Visible == true)
                            if (col_prop.m_textTopHeader.Equals(string.Empty) == false)
                                if (col_prop.m_textTopHeader.Equals(oldItem) == false)
                                {
                                    oldItem = col_prop.m_textTopHeader;
                                    listTop.Add(col_prop.m_textTopHeader);
                                }
                                else;
                            else
                                listTop.Add(col_prop.m_textTopHeader);
                        else;
                    else;
                }

                m_headerTop.Add(idTG, listTop);

                if (m_headerMiddle.ContainsKey(idTG))
                    m_headerMiddle.Remove(idTG);
                else
                    ;

                foreach (DataGridViewColumn col in Columns) {
                    col_prop = (COLUMN_PROPERTY)col.Tag;

                    if (!(col_prop.m_putParameter.m_idNAlg < 0))
                        if (col.Visible == true)
                            if (col.Name != oldItem) {
                                oldItem = col.Name;
                                listMiddle.Add(col.Name);
                            } else
                                ;
                        else
                            ;
                    else
                        ;
                }

                m_headerMiddle.Add(idTG, listMiddle);
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            /// <param name="idDgv">номер окна отображения</param>
            private void formRelationsHeading(int idDgv)
            {
                string oldItem = string.Empty;
                int indx = 0
                    , untdCol = 0
                    , untdColM = 0;

                COLUMN_PROPERTY col_prop;
                int[] arrIntTop = new int[m_headerTop[idDgv].Count()]
                    , arrIntMiddle = new int[m_headerMiddle[idDgv].Count()];

                if (m_arIntTopHeader.ContainsKey(idDgv))
                    m_arIntTopHeader.Remove(idDgv);
                else
                    ;

                foreach (var item in m_headerTop[idDgv]) {
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

                    arrIntTop[indx] = untdCol;
                    indx++;
                }

                m_arIntTopHeader.Add(idDgv, arrIntTop);
                indx = 0;

                if (m_arMiddleCol.ContainsKey(idDgv))
                    m_arMiddleCol.Remove(idDgv);
                else
                    ;

                foreach (var item in m_headerMiddle[idDgv]) {
                    foreach (DataGridViewColumn col in Columns) {
                        col_prop = (COLUMN_PROPERTY)col.Tag;

                        if (col_prop.m_putParameter.m_idNAlg > -1)
                            if (item == col.Name)
                                untdColM++;
                            else
                                if (untdColM > 0)
                                    break;
                                else
                                    ;
                        else
                            ;
                    }

                    arrIntMiddle[indx] = untdColM;
                    indx++;
                    untdColM = 0;
                }

                m_arMiddleCol.Add(idDgv, arrIntMiddle);
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

            private int WIDTH_COL1 { get { return Columns[2].Width; }  }

            /// <summary>
            /// обработчик события перерисовки грида(построение шапки заголовка)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            void dataGridView_onPaint(object sender, PaintEventArgs e)
            {
                int indxCol = 0
                    , idComp = -1
                    , height = -1;
                Rectangle r1 = new Rectangle()
                    , r2 = new Rectangle();
                Pen pen;
                StringFormat format;

                pen = new Pen(Color.Black);
                format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                //
                for (int i = 0; i < Columns.Count; i++)
                    if (GetCellDisplayRectangle(i, -1, true).Height > 0 & GetCellDisplayRectangle(i, -1, true).X > 0)
                    {
                        rectParentColumn = GetCellDisplayRectangle(i, -1, true);
                        r1 = rectParentColumn;
                        r2 = rectParentColumn;
                        break;
                    }

                height = r1.Height / s_GroupHeaderCount;

                idComp = (int)(sender as DataGridViewVedomostBl).Tag;
                foreach (var item in m_headerMiddle[idComp])
                {
                    //get the column header cell
                    r1.Width = m_arMiddleCol[idComp][m_headerMiddle[idComp].ToList().IndexOf(item)] * WIDTH_COL1;
                    r1.Height = height + 3;//??? 

                    if (m_headerMiddle[idComp].ToList().IndexOf(item) - 1 > -1)
                        r1.X = r1.X + m_arMiddleCol[idComp][m_headerMiddle[idComp].ToList().IndexOf(item) - 1] * WIDTH_COL1;
                    else
                    {
                        r1.X += WIDTH_COL1;
                        r1.Y = r1.Y + r1.Height;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), r1);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      r1,
                      format);
                    e.Graphics.DrawRectangle(pen, r1);
                }

                foreach (var item in m_headerTop[idComp])
                {
                    //get the column header cell
                    r2.Width = m_arIntTopHeader[idComp][indxCol] * WIDTH_COL1;
                    r2.Height = height + 2;//??? 

                    if (indxCol - 1 > -1)
                        r2.X = r2.X + m_arIntTopHeader[idComp][indxCol - 1] * WIDTH_COL1;
                    else
                    {
                        r2.X += WIDTH_COL1;
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

            public void AddHeaderColumns(List<string[]> listHeaders)
            {
                m_listHeaders = new List<string[]>(listHeaders);
            }

            public void AddColumns(List<PUT_PARAMETER>listPutParameter)
            {
                int i = -1;

                for (int col = 0; col < m_listHeaders.Count; col++) {
                    addColumn(listPutParameter[col].m_idNAlg
                        , new DataGridViewVedomostBl.COLUMN_PROPERTY {
                            m_textTopHeader = m_listHeaders[col][(int)DataGridViewVedomostBl.INDEX_HEADER.TOP].ToString()
                            , m_textMiddleHeader = m_listHeaders[col][(int)DataGridViewVedomostBl.INDEX_HEADER.MIDDLE].ToString()
                            , m_textLowHeader = m_listHeaders[col][(int)DataGridViewVedomostBl.INDEX_HEADER.LOW].ToString()
                            , m_putParameter = listPutParameter[col]
                        });
                }
            }

            public void AddRows(DateTime dtStart, int cnt)
            {
                for (int i = 0; i < cnt + 1; i++)
                    AddRow(dtStart.AddDays(i), i < cnt);
            }

            /// <summary>
            /// Настройка размеров контролов отображения
            /// </summary>
            public void ResizeControls()
            {
                int cntVisibleColumns = 0;

                foreach (DataGridViewColumn col in Columns) {
                    if (Columns.IndexOf(col) > 0)
                        col.Width = 65;
                    else
                        ;

                    if (col.Visible == true)
                        cntVisibleColumns++;
                    else
                        ;
                }

                int _drwW = cntVisibleColumns * Columns[2].Width + 10
                    , _drwH = (Rows.Count) * Rows[0].Height + 70;

                //GetPictureOfIdComp((int)(dgv as DataGridViewVedomostBl).Tag).Size = new Size(_drwW + 2, _drwH);
                Size = new Size(_drwW + 2, _drwH);
            }

            /// <summary>
            /// обработчик события - перерисовки ячейки
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
            /// Отображение данных на вьюхе
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

                if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE == (int)typeValues)
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
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE:
                        quality = 1;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                return quality;
            }
        }
    }
}
