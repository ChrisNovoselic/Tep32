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
        /// <summary>
        /// Возвратить список с заголовками представления для отображения значений
        /// </summary>
        /// <param name="arlistStr">лист парамтеров</param>
        /// <param name="rowPars">таблица с данными</param>
        /// <returns>Список массивов строк-заговков</returns>
        public List<string[]> GetListHeaders(DataTable tableSource, int id)
        {
            List<List<string>> arlistStr;
            DataRow[] rowPars;
            int cntHeader = 0;
            string[] arStrHeader;
            List<string[]> listHeader = new List<string[]> { };

            using (ListStringHeaderParseer parser = new ListStringHeaderParseer(tableSource, id)) { arlistStr = parser.m_ListParam; }

            rowPars = tableSource.Select("ID_COMP = " + id);

            var enumHeader = (from r in rowPars.AsEnumerable()
                              orderby r.Field<int>("ID")
                              select new
                              {
                                  NAME_SHR = r.Field<string>("NAME_SHR"),
                              }).Distinct();

            listHeader.Clear();

            for (int j = 0; j < arlistStr.Count; j++) {
                if (arlistStr[j].Count < 3)
                    arStrHeader = new string[arlistStr[j].Count + 1];
                else
                    arStrHeader = new string[arlistStr[j].Count];

                cntHeader = 0;

                for (int level = arlistStr[j].Count - 1; level > -1; level--) {
                    switch (level) {
                        case (int)LEVEL_HEADER.TOP:
                            for (int t = 0; t < s_listGroupHeaders.Count; t++) {
                                for (int n = 0; n < s_listGroupHeaders[t].Count; n++) {
                                    cntHeader++;
                                    if (int.Parse(arlistStr[j].ElementAt((int)LEVEL_HEADER.TOP)) == cntHeader) {
                                        arStrHeader[level] = s_listGroupHeaders[t][n];
                                        listHeader.Add(arStrHeader);

                                        t = s_listGroupHeaders.Count; // прервать внешний цикл
                                        break;
                                    } else
                                        ;
                                }
                            }
                            break;
                        case (int)LEVEL_HEADER.MIDDLE:
                            // ??? почему < 3
                            if (arlistStr[j].Count < 3)
                                arStrHeader[level + 1] = "";
                            else
                                ;

                            arStrHeader[(int)LEVEL_HEADER.MIDDLE] = rowPars[j]["NAME_SHR"].ToString().Trim();
                            break;
                        case (int)LEVEL_HEADER.LOW:
                            arStrHeader[level] = rowPars[j]["DESCRIPTION"].ToString().Trim();
                            break;
                        default:
                            break;
                    }
                } // for - level
            }

            return listHeader;
        }

        /// <summary>
        /// класс для формирования листа с параметрами 
        /// для формирования заголовков
        /// </summary>
        private class ListStringHeaderParseer : IDisposable
        {
            /// <summary>
            /// набор листов с параметрами группировки
            /// </summary>
            public List<List<string>> m_ListParam;

            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="table">таблица с данными</param>
            /// <param name="id_comp">параметр для выборки</param>
            public ListStringHeaderParseer(DataTable table, int id_comp)
            {
                parse(table.Select("ID_COMP = " + id_comp));
            }

            /// <summary>
            /// формирование листа параметров вида x.y.z,
            /// где x - TopHeader, y - MiddleHeader, y - LowHeader
            /// </summary>
            /// <param name="tablePars">таблица с данными</param>
            private void parse(DataRow[] tablePars)
            {
                m_ListParam = new List<List<string>>(tablePars.Count());

                List<string> list;

                foreach (DataRow row in tablePars)
                {
                    list = new List<string>();

                    list = row["N_ALG"].ToString().Split('.', ',').ToList();
                    m_ListParam.Add(list);
                }
            }

            public void Dispose()
            {
            }
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
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            public enum INDEX_SERVICE_COLUMN : uint { ALG = 0, DATE, COUNT }
            ///// <summary>
            ///// Словарь настроечных данных
            ///// </summary>
            //private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;
            private Dictionary<int, COLUMN_PROPERTY> m_dictPropertyColumns;

            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="nameDGV">Идентификатор оборудования - блока, данные которого отображаются в текущем представлении</param>
            public DataGridViewVedomostBl(int tag) : base (ModeData.DATETIME)
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

                AddColumns(-2, "ALG", string.Empty, false);
                AddColumns(-1, "Date", "Дата", true);
            }


            public override void BuildStructure()
            {
            }

            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_IdAlg;
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_IdComp;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
                /// <summary>
                /// признак общей группы
                /// </summary>
                public string m_topHeader;
            }

            /// <summary>
            /// Структура для описания добавляемых столбцов
            /// </summary>
            public class COLUMN_PROPERTY
            {
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { IS_NAN }
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idParameter, HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }

                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// признак агрегации
                /// </summary>
                public int m_Avg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRound;
                /// <summary>
                /// Имя колонки
                /// </summary>
                public string nameCol;
                /// <summary>
                /// Текст в колонке
                /// </summary>
                public string hdrText;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public string topHeader;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public int m_IdComp;
            }

            public void AddRow(DateTime dtRow, bool bEnded)
            {
                //AddRow(dtRow);
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="nameCol">имя колонки</param>
            /// <param name="headerText">текст заголовка</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, string nameCol, string headerText, bool bVisible)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_IdAlg = idHeader, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    column.Frozen = true;
                    column.Visible = bVisible;
                    column.ReadOnly = false;
                    column.Name = nameCol;
                    column.HeaderText = headerText;
                    column.DefaultCellStyle.Alignment = alignText;
                    //column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DGVVedBl::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="col_prop">Структура для описания добавляемых столбцов</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, COLUMN_PROPERTY col_prop, bool bVisible)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    if (m_dictPropertyColumns == null)
                        m_dictPropertyColumns = new Dictionary<int, COLUMN_PROPERTY>();

                    if (!m_dictPropertyColumns.ContainsKey(col_prop.m_idAlg))
                        m_dictPropertyColumns.Add(col_prop.m_idAlg, col_prop);
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    //foreach (HDataGridViewColumn col in Columns)
                    //    if ((col.m_iIdComp > 0)
                    //        && (col.m_iIdComp < 1000))
                    //    {
                    //        indxCol = Columns.IndexOf(col);
                    //        break;
                    //    }

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_topHeader = col_prop.topHeader, m_IdAlg = idHeader, m_IdComp = col_prop.m_IdComp };
                    alignText = DataGridViewContentAlignment.MiddleRight;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        //if (idHeader < 0)
                        //{// для служебных столбцов
                        if (bVisible == true)
                        {// только для столбца с [SYMBOL]
                            alignText = DataGridViewContentAlignment.MiddleLeft;
                        }
                        column.Frozen = true;
                        column.ReadOnly = true;
                        //}
                    }

                    column.HeaderText = col_prop.hdrText;
                    column.Name = col_prop.nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.Visible = bVisible;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewVedBl::AddColumn (idHeader=" + idHeader + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bReadOnly">true/false</param>
            public bool ReadOnlyColumns
            {
                set {
                    foreach (HDataGridViewColumn col in Columns)
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

                s_iCountColumn = cntCol * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width +
                    Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width / s_listGroupHeaders.Count;

                Paint += new PaintEventHandler(dataGridView1_Paint);
            }

            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            /// <param name="idTG">номер идТГ</param>
            private void formingTitleLists(int idTG)
            {
                string _oldItem = string.Empty;
                List<string> _listTop = new List<string>(),
                    _listMiddle = new List<string>();

                if (m_headerTop.ContainsKey(idTG))
                    m_headerTop.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_IdAlg >= 0)
                        if (col.Visible == true)
                            if (col.m_topHeader != "")
                                if (col.m_topHeader != _oldItem)
                                {
                                    _oldItem = col.m_topHeader;
                                    _listTop.Add(col.m_topHeader);
                                }
                                else;
                            else
                                _listTop.Add(col.m_topHeader);
                        else;
                    else;

                m_headerTop.Add(idTG, _listTop);

                if (m_headerMiddle.ContainsKey(idTG))
                    m_headerMiddle.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_IdAlg >= 0)
                        if (col.Visible == true)
                            if (col.Name != _oldItem)
                            {
                                _oldItem = col.Name;
                                _listMiddle.Add(col.Name);
                            }

                m_headerMiddle.Add(idTG, _listMiddle);
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            /// <param name="idDgv">номер окна отображения</param>
            private void formRelationsHeading(int idDgv)
            {
                string _oldItem = string.Empty;
                int _indx = 0,
                    _untdColM = 0;
                int[] _arrIntTop = new int[m_headerTop[idDgv].Count()],
                    _arrIntMiddle = new int[m_headerMiddle[idDgv].Count()];

                if (m_arIntTopHeader.ContainsKey(idDgv))
                    m_arIntTopHeader.Remove(idDgv);
                else
                    ;

                foreach (var item in m_headerTop[idDgv])
                {
                    int untdCol = 0;
                    foreach (HDataGridViewColumn col in Columns)
                        if (col.Visible == true)
                            if (col.m_topHeader == item)
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
                    _arrIntTop[_indx] = untdCol;
                    _indx++;
                }

                m_arIntTopHeader.Add(idDgv, _arrIntTop);
                _indx = 0;

                if (m_arMiddleCol.ContainsKey(idDgv))
                    m_arMiddleCol.Remove(idDgv);

                foreach (var item in m_headerMiddle[idDgv])
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_IdAlg > -1)
                            if (item == col.Name)
                                _untdColM++;
                            else
                                if (_untdColM > 0)
                                break;
                    }
                    _arrIntMiddle[_indx] = _untdColM;
                    _indx++;
                    _untdColM = 0;
                }
                m_arMiddleCol.Add(idDgv, _arrIntMiddle);
            }

            /// <summary>
            /// ??? зачем в 1-ом аргументе указывать объект Скрыть/показать столбцы из списка групп
            /// </summary>
            /// <param name="listHeaderTop">лист с именами заголовков</param>
            /// <param name="isCheck">проверка чека</param>
            public void SetHeaderVisibled(List<string> listHeaderTop, bool isCheck)
            {
                try {
                    foreach (var item in listHeaderTop)
                        foreach (HDataGridViewColumn col in Columns)
                            if (col.m_topHeader == item)
                                col.Visible = isCheck;
                            else
                                ;
                } catch (Exception) { }

                ConfigureColumns();
            }

            /// <summary>
            /// обработчик события перерисовки грида(построение шапки заголовка)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            void dataGridView1_Paint(object sender, PaintEventArgs e)
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
                    r1.Width = m_arMiddleCol[idComp][m_headerMiddle[idComp].ToList().IndexOf(item)]
                        * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    r1.Height = height + 3;//??? 

                    if (m_headerMiddle[idComp].ToList().IndexOf(item) - 1 > -1)
                        r1.X = r1.X + m_arMiddleCol[idComp][m_headerMiddle[idComp].ToList().IndexOf(item) - 1]
                            * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        r1.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
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
                    r2.Width = m_arIntTopHeader[idComp][indxCol] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    r2.Height = height + 2;//??? 

                    if (indxCol - 1 > -1)
                        r2.X = r2.X + m_arIntTopHeader[idComp][indxCol - 1] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        r2.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
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

            /// <summary>
            /// обработчик события - перерисовки ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            static void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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
            public void ShowValues(DataTable tableOrigin, DataTable tableInParameter, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                DataTable _dtOriginVal = new DataTable(),
                    _dtEditVal = new DataTable();
                int idAlg = -1
                   , idParameter = -1
                   , _hoursOffSet
                   , iCol = 0
                   , _vsRatioValue = -1;
                double dblVal = -1F;

                DataRow[] parameterRows = null,
                    editRow = null;

                _dtOriginVal = tableOrigin.Copy();
                ClearValues();

                if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE == (int)typeValues)
                    if (s_flagBl)
                        _hoursOffSet = -1 * (-(TimeZoneInfo.Local.BaseUtcOffset.Hours + 1) + 24);
                    else
                        _hoursOffSet = (s_currentOffSet / 60);
                else
                    _hoursOffSet = s_currentOffSet / 60;

                if (_dtOriginVal.Rows.Count > 0)
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        {
                            try
                            {
                                parameterRows = tableInParameter.Select(string.Format(tableInParameter.Locale
                                    , "ID_ALG = " + col.m_IdAlg + " AND ID_COMP = " + (int)Tag));
                                editRow = _dtOriginVal.Select(string.Format(_dtOriginVal.Locale, "ID_PUT = " + (int)parameterRows[0]["ID"]));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("???" + "Ошибка выборки данных!");
                            }

                            for (int i = 0; i < editRow.Count(); i++)
                            {
                                _vsRatioValue = m_dictPropertyColumns[col.m_IdAlg].m_vsRatio;

                                if (Convert.ToDateTime(editRow[i][@"WR_DATETIME"]).AddHours(_hoursOffSet).ToShortDateString() ==
                                    Rows[i].Cells["Date"].Value.ToString())
                                {
                                    Rows[i].Cells[iCol].Value =
                                        (((double)editRow[i][@"VALUE"]).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound,
                                            CultureInfo.InvariantCulture));
                                }
                                else
                                    ;
                            }

                            try
                            {
                                if (m_dictPropertyColumns[col.m_IdAlg].m_Avg == 0)
                                    Rows[RowCount - 1].Cells[iCol].Value =
                                        sumVal(_dtEditVal, col.Index).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound, CultureInfo.InvariantCulture);
                                else
                                    Rows[RowCount - 1].Cells[iCol].Value =
                                        avgVal(_dtEditVal, col.Index).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound, CultureInfo.InvariantCulture);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show("???" + "Ошибка усредненния данных по столбцу " + col.m_topHeader + "! " + exp.ToString());
                            }
                        }
                        else
                            ;

                        iCol++;
                    }
            }

            /// <summary>
            /// Получение суммы по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>сумма по столбцу</returns>
            private double sumVal(DataTable table, int indxCol)
            {
                double _sumValue = 0F;

                try {
                    foreach (DataGridViewRow row in Rows)
                        if (row.Index < Rows.Count - 1)
                        // все кроме крайней строки
                            if ((!(row.Cells[indxCol].Value == null))
                                && (string.IsNullOrEmpty(row.Cells[indxCol].Value.ToString()) == false))
                            // только, если есть значение для разбора
                                _sumValue += HMath.doubleParse(row.Cells[indxCol].Value.ToString());
                            else
                                ;
                        else
                            ;
                } catch (Exception e) {
                    MessageBox.Show("???" + "Ошибка суммирования столбца!");
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::sumVal () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return _sumValue;
            }

            /// <summary>
            /// Получение среднего по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>среднее по столбцу</returns>
            private double avgVal(DataTable table, int indxCol)
            {
                int cntNum = 0;
                double avgValue = 0F
                   , sumValue = 0F;

                try {
                    foreach (DataGridViewRow row in Rows)
                        if ((!(row.Cells[indxCol].Value == null))
                            && (string.IsNullOrEmpty(row.Cells[indxCol].Value.ToString()) == false)) {
                            sumValue += HMath.doubleParse(row.Cells[indxCol].Value.ToString());
                            cntNum++;
                        } else
                            ;
                } catch (Exception exp) {
                    MessageBox.Show("???" + "Ошибка усреднения столбца!");
                    Logging.Logg().Exception(exp, @"PanelTaskVedomostBl::avgVal () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return avgValue = sumValue / cntNum;
            }

            /// <summary>
            /// Формирование таблицы данных с отображения
            /// </summary>
            /// <param name="dtSourceOrg">таблица с оригинальными данными</param>
            /// <param name="idSession">номер сессии пользователя</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>таблица с новыми данными с вьюхи</returns>
            public DataTable FillTableToSave(DataTable dtSourceOrg, int idSession, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int i = 0,
                    idAlg = -1
                    , hoursOffSet
                    , vsRatioValue = -1
                    , quality = 0,
                    indexPut = 0;
                double valueToRes = 0;
                DateTime dtVal;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(
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

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.m_IdAlg > 0)
                    {
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (row.Cells[col.Index].Value != null)
                                    if (row.Cells[col.Index].Value.ToString() != "")
                                    {
                                        idAlg = col.m_IdAlg;
                                        valueToRes = HPanelTepCommon.AsParseToF(row.Cells[col.Index].Value.ToString());
                                        vsRatioValue = m_dictPropertyColumns[idAlg].m_vsRatio;
                                        valueToRes *= Math.Pow(10F, vsRatioValue);
                                        dtVal = Convert.ToDateTime(row.Cells["Date"].Value.ToString());
                                        quality = diffRowsInTables(dtSourceOrg, valueToRes, i, idAlg, typeValues);

                                        dtSourceEdit.Rows.Add(new object[]
                                        {
                                            col.m_IdComp
                                            , idSession
                                            , quality
                                            , valueToRes
                                            , dtVal.AddMinutes(-s_currentOffSet).ToString("F",dtSourceEdit.Locale)
                                            , i
                                        });
                                        i++;
                                    }
                        }
                        indexPut++;
                    }
                }
                dtSourceEdit = sortDataTable(dtSourceEdit, "WR_DATETIME");
                return dtSourceEdit;
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
            private int diffRowsInTables(DataTable origin, double editValue, int i, int idAlg, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int quality = 1;
                double originValues;

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
                        if (originValues.ToString(@"F" + m_dictPropertyColumns[idAlg].m_vsRound, CultureInfo.InvariantCulture) != editValue.ToString())
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
