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

            public short[] order;

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
        /// Класс представления для отображения значений
        /// </summary>
        protected class DataGridViewVedomostBl : DataGridViewValues
        {
            public interface IHeaderUpLevelChange
            {
                /// <summary>
                /// Порядок следования(индекс/ключ) ячейки родительского уровня
                /// </summary>
                int order_owner_level { get; set; }
                /// <summary>
                /// Содержание объединенной(возможно) ячейки
                /// </summary>
                string text { get; set; }
                /// <summary>
                /// Кол-во столбцов(заголовков) самого низкого уровня
                ///  , для вычисления ширины объединенной(возможно) ячейки
                /// </summary>
                int count_low_column { get; set; }

                void SetCountLowColumn(int count);

                void IncCountLowColumn();
            }
            /// <summary>
            /// Структура для хранения информации о подписи для объединенной(возможно) ячейки заголовка уровня, отличного от самого низкого
            ///  , т.е. TOP, MIDDLE
            /// </summary>
            public struct HEADER_UPLEVEL : IHeaderUpLevelChange
            {
                public int order_owner_level { get; set; }

                public string text { get; set; }

                public int count_low_column { get; set; }

                public void SetCountLowColumn(int count)
                {
                    count_low_column = count;
                }

                public void IncCountLowColumn()
                {
                    count_low_column++;
                }
            }          
            /// <summary>
            /// Список названий заголовков 
            /// верхнего и среднего уровней
            /// </summary>
            private List<IHeaderUpLevelChange> m_listHeaderTopLevel = new List<IHeaderUpLevelChange>()
                , m_listHeaderMiddleLevel = new List<IHeaderUpLevelChange>();
            public List<IHeaderUpLevelChange> ListHeaderTopLevel { get { return m_listHeaderTopLevel; } }
            public List<IHeaderUpLevelChange> ListHeaderMiddleLevel { get { return m_listHeaderMiddleLevel; } }
            ///// <summary>
            ///// словарь соотношения заголовков
            ///// </summary>
            //public int[] m_arCounterHeaderTop = new int[] { }
            //    , m_arCounterHeaderMiddle = new int[] { };            
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="nameDGV">Идентификатор оборудования - блока, данные которого отображаются в текущем представлении</param>
            public DataGridViewVedomostBl(HandlerDbTaskCalculate.TECComponent comp, Func<int, int, float, int, float> fGetValueAsRatio)
                : base (ModeData.DATETIME, fGetValueAsRatio)
            {
                Tag = comp;

                InitializeComponents();

                Paint += new PaintEventHandler(dataGridView_onPaint);
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
                ColumnHeadersHeight = ColumnHeadersHeight * (int)LEVEL_HEADER.COUNT;//высота от нижнего(headerText)

                ScrollBars = ScrollBars.None;
            }

            public int IdComponent { get { return ((HandlerDbTaskCalculate.TECComponent)Tag).m_Id; } }

            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                ////??? каждый раз получаем полный список и выбираем необходимый
                //dictVisualSett = getVisualSettingsOfIdComponent((int)dgv.Tag);

                AddColumns(listPutParameter
                    , getListHeaders(listNAlgParameter, listPutParameter));

                ////???
                //addRows();

                //ResizeControls();

                //ConfigureColumns();
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="tag">Дополнительные(прикрепляемые) сведения для столбца</param>
            /// <return>Индекс добавленного столбца</return>
            private int addColumn(COLUMN_TAG tag)
            {
                int iRes = -1; // индекс добавленного столбца

                string name = string.Empty //Наименование столбца
                    , text = string.Empty; //Текст заголовка столбца
                bool bVisibled = false; //Признак отображения

                DataGridViewTextBoxColumn column;
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                name = ((HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value).m_headers[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.MIDDLE];
                text = ((HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value).m_headers[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.LOW];
                bVisibled = ((HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value).IsVisibled;

                try
                {
                    column = new DataGridViewTextBoxColumn();

                    alignText = DataGridViewContentAlignment.MiddleRight;

                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    column.Frozen = true;

                    column.Visible = bVisibled; // col_prop.m_putParameter.IsVisibled;
                    column.ReadOnly = false;
                    column.Name = name; // col_prop.m_textMiddleHeader;
                    column.HeaderText = text; // col_prop.m_textLowHeader;
                    column.DefaultCellStyle.Alignment = alignText;

                    column.Tag = tag;

                    iRes = Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DataGridViewVedomostBl::addColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return iRes;
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
            public void ConfigureHeaders()
            {
                formingHeaderLists();

                formingRelationsHeading();
            }
            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            private void formingHeaderLists()
            {
                string prevTextHeaderTop = string.Empty
                    , prevTextHeaderMiddle = string.Empty;
                COLUMN_TAG tag;
                HandlerDbTaskCalculate.GROUPING_PARAMETER groupPutPar;
                int indxHeaderTop = -1;

                //TODO: оптимизировать (непонятно, что выполняется в цикле)                
                m_listHeaderTopLevel.Clear();
                m_listHeaderMiddleLevel.Clear();

                indxHeaderTop = (int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP;
                
                foreach (DataGridViewColumn col in Columns) {
                    tag = (COLUMN_TAG)col.Tag;

                    if (tag.Type == TYPE_COLUMN_TAG.GROUPING_PARAMETR) {
                        groupPutPar = (HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value;

                        if (!(groupPutPar.m_idNAlg < 0))
                            if (col.Visible == true) {
                                #region Формировать список заголовков высшего(TOP) уровня
                                if (string.IsNullOrEmpty(groupPutPar.m_headers[indxHeaderTop]) == false)
                                //??? заголовок верхнего уровня не пустой
                                    if (groupPutPar.m_headers[indxHeaderTop].Equals(prevTextHeaderTop) == false) {
                                    // предыдущее значение строки заголовка верхнего уровня НЕ совпадает с предыдущим значением
                                        // запомнить текущее значение
                                        prevTextHeaderTop = groupPutPar.m_headers[indxHeaderTop];
                                        // добавить в список с заголовками верхнего уровня
                                        m_listHeaderTopLevel.Add(new HEADER_UPLEVEL() {
                                            order_owner_level = -1 // порядка(индекса/ключа) ячейки верхнего уровня для ячейки верхнего уровня НЕТ
                                            , text = groupPutPar.m_headers[indxHeaderTop]
                                            , count_low_column = 0 });
                                    } else
                                    // предыдущее значение строки заголовка верхнего уровня совпадает с предыдущим значением
                                        ;
                                else
                                //??? заголовок верхнего уровня - пустой (зачем добавлять пустую строку)
                                    m_listHeaderTopLevel.Add(new HEADER_UPLEVEL() { order_owner_level = -1, text = string.Empty, count_low_column = 0 });
                                #endregion

                                #region Формировать список заголовков среднего(MIDDLE) уровня
                                if (col.Name.Equals(prevTextHeaderMiddle) == false) {
                                    prevTextHeaderMiddle = col.Name;
                                    m_listHeaderMiddleLevel.Add(new HEADER_UPLEVEL() {
                                        order_owner_level = groupPutPar.m_orders[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP]
                                        , text = col.Name
                                        , count_low_column = 0 });
                                } else
                                    ;
                                #endregion
                            } else
                            // столбец не отображается
                                ;
                        else
                        // неизвестный номер алгоритма расчета 1-го порядка
                            ;
                    } else
                        ;
                }
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            private void formingRelationsHeading()
            {
                string oldItem = string.Empty;
                short order = -1;
                int cntSpanColumn = -1;

                COLUMN_TAG tag;
                HandlerDbTaskCalculate.GROUPING_PARAMETER groupPutPar;

                #region Найти кол-во ячеек нижнего уровня, присоединенных к ячейке верхнего уровня
                m_listHeaderTopLevel.ForEach(item => {
                    cntSpanColumn = 0;

                    foreach (DataGridViewColumn col in Columns) {
                        tag = (COLUMN_TAG)col.Tag;
                        groupPutPar = (HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value;

                        if (col.Visible == true)
                            if (item.text.Equals(groupPutPar.m_headers[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP]) == true)
                                if (string.IsNullOrEmpty(item.text) == false)
                                    cntSpanColumn++;
                                else {
                                    cntSpanColumn = 1;

                                    break;
                                }
                            else
                                ;
                        else
                            ;
                    }

                    item.count_low_column = cntSpanColumn;
                });
                #endregion

                #region Найти кол-во ячеек нижнего уровня, присоединенных к ячейке среднего уровня, и порядок(индекс/ключ) ячейки верхнего уровня
                m_listHeaderMiddleLevel.ForEach(item => {
                    cntSpanColumn = 0;

                    foreach (DataGridViewColumn col in Columns) {
                        tag = (COLUMN_TAG)col.Tag;
                        groupPutPar = (HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value;

                        if (groupPutPar.m_idNAlg > -1)
                            if ((item.text.Equals(col.Name) == true)
                                && (item.order_owner_level == groupPutPar.m_orders[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP]))
                                cntSpanColumn++;
                            else
                                if (cntSpanColumn > 0) {
                                    order = groupPutPar.m_orders[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP];

                                    break;
                                } else
                                    ;
                        else
                            ;
                    }

                    item.count_low_column = cntSpanColumn;
                });
                #endregion
            }

            /// <summary>
            /// ??? зачем в 1-ом аргументе указывать объект Скрыть/показать столбцы из списка групп
            /// </summary>
            /// <param name="listHeaderTop">лист с именами заголовков</param>
            /// <param name="isCheck">проверка чека</param>
            public void SetColumnVisibled(List<string> listHeaderTop, bool isCheck)
            {
                COLUMN_TAG tag;
                HandlerDbTaskCalculate.GROUPING_PARAMETER groupPutPar;

                try {
                    foreach (var item in listHeaderTop)
                        foreach (DataGridViewColumn col in Columns) {
                            tag = (COLUMN_TAG)col.Tag;
                            groupPutPar = (HandlerDbTaskCalculate.GROUPING_PARAMETER)tag.value;

                            if (groupPutPar.m_headers[(int)HandlerDbTaskCalculate.GROUPING_PARAMETER.INDEX_HEADER.TOP].Equals(item) == true)
                                col.Visible = isCheck;
                            else
                                ;
                        }
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"DataGridViewVedomostBl::SetHeaderVisibled () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public static int PADDING_COLUMN = 10;

            public static int HEIGHT_HEADER = 70;

            private static int WIDTH_COLUMN_DEFAULT = 65;

            private int WIDTH_COLUMN_DATE { get { return RowHeadersVisible == true ? 0 : WIDTH_COLUMN_DEFAULT; } }

            private int WIDTH_COLUMN { get { return Columns[RowHeadersVisible == true ? 0 : 1].Width; }  }

            /// <summary>
            /// Обработчик события перерисовки представления(построение строк заголовка)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            void dataGridView_onPaint(object sender, PaintEventArgs e)
            {
                int height = -1;
                
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
                        & (GetCellDisplayRectangle(i, -1, true).X > 0)) {
                        rectParentColumn = GetCellDisplayRectangle(i, -1, true);
                        r1 = rectParentColumn;
                        r2 = rectParentColumn;

                        break;
                    } else
                        ;

                height = r1.Height / (int)LEVEL_HEADER.COUNT;

                // отобразить заголовки среднего(MIDDLE) уровня
                m_listHeaderMiddleLevel.ForEach(item => {
                    // ширину изменяем ПОСЛЕ изменения координаты X, т.к. требуется сохранить ее предыдущее значение
                    // высоту изменяем ПЕРЕД 1-ой итерацией
                    r1.Height = height + 3;//??? 
                    // определить 1-ую итерацию
                    if ((m_listHeaderMiddleLevel.IndexOf(item) - 1) > -1)
                    // добавить ширину предыдущего элемента
                        r1.X += r1.Width;
                    else {
                    // только для 1-ой итерации
                        r1.X += WIDTH_COLUMN_DATE;
                        r1.Y += r1.Height;
                    }

                    r1.Width = item.count_low_column * WIDTH_COLUMN;                    

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), r1);
                    e.Graphics.DrawString(item.text, ColumnHeadersDefaultCellStyle.Font
                        , new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor)
                        , r1
                        , format);
                    e.Graphics.DrawRectangle(pen, r1);
                });
                // отобразить заголовки самого верхнего(TOP) уровня
                m_listHeaderTopLevel.ForEach(item => {
                    // ширину изменяем ПОСЛЕ изменения координаты X, т.к. требуется сохранить ее предыдущее значение
                    // высоту изменяем НЕВОЗБРАННО (можем в т.ч. вынести из тела анонимной функции)
                    r2.Height = height + 2;//??? 
                                           // определить 1-ую итерацию
                    if ((m_listHeaderTopLevel.IndexOf(item) - 1) > -1)
                        // добавить ширину предыдущего элемента
                        r2.X += r2.Width;
                    else {
                        // только для 1-ой итерации
                        r2.X += WIDTH_COLUMN_DATE;
                        r2.Y += r2.Y;
                    }

                    r2.Width = item.count_low_column * WIDTH_COLUMN;

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), r2);
                    e.Graphics.DrawString(item.text, ColumnHeadersDefaultCellStyle.Font
                        , new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor)
                        , r2
                        , format);
                    e.Graphics.DrawRectangle(pen, r2);
                });

                //(sender as DGVVedomostBl).Paint -= new PaintEventHandler(dataGridView1_Paint);
            }

            public void AddColumns(List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter, List<HEADER>listHeaders)
            {
                int i = -1;

                HandlerDbTaskCalculate.GROUPING_PARAMETER groupPutPar;

                if (RowHeadersVisible == false) {
                    groupPutPar = new HandlerDbTaskCalculate.GROUPING_PARAMETER(-1, -1, new HandlerDbTaskCalculate.TECComponent() {
                            m_Id = -1
                            , m_idOwner = -1
                            , m_nameShr = string.Empty
                        }
                        , -1
                        , false, true
                        , float.MinValue, float.MaxValue
                        , string.Empty, "DATE", "Дата"
                        , -1, -1, -1);
                    // обязательно присваивать 'tag' до вызова Columns.Add
                    i = addColumn(new COLUMN_TAG(groupPutPar, -1, true));
                } else
                    ;

                for (int col = 0; col < listHeaders.Count; col++) {
                    groupPutPar = new HandlerDbTaskCalculate.GROUPING_PARAMETER(listPutParameter[col]
                        , listHeaders[col].values
                        , listHeaders[col].order);
                    // обязательно присваивать 'tag' до вызова Columns.Add
                    i = addColumn(new COLUMN_TAG (groupPutPar) {
                        TemplateReportAddress = -1
                        , ActionAgregateCancel = false
                    });
                }
            }

            /// <summary>
            /// Настройка размеров столбцов и, в ~ от них собственного размера
            /// </summary>
            public void SetSize()
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
                    r2.Y += e.CellBounds.Height / (int)LEVEL_HEADER.COUNT;
                    r2.Height = e.CellBounds.Height / (int)LEVEL_HEADER.COUNT;
                    e.PaintContent(r2);
                    e.Handled = true;
                }
            }
            ///// <summary>
            ///// Класс для сравнения/сортировки 2-х объектов 'HEADER'
            ///// </summary>
            //private class HeaderComparer : IComparer<HEADER>
            //{
            //    public int Compare(HEADER x, HEADER y)
            //    {
            //        throw new NotImplementedException();
            //    }
            //}
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
                short[] arShortOrder;
                List<HEADER> listHeaderSrc;            

                listHeaderSrc = new List<HEADER>(listPutParameter.Count);

                nalg_prop = null;

                foreach (HandlerDbTaskCalculate.PUT_PARAMETER put in listPutParameter) {
                    if ((nalg_prop == null)
                        || ((!(nalg_prop == null))
                            && (!(nalg_prop.m_Id == put.m_Id))))
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
                        , order =
                            nalg_prop
                                .m_nAlg
                                .Split('.', ',')
                                .Select<string, short>((s) => { return short.Parse(s); })
                                .ToArray()
                    });
                }

                listHeaderRes.Clear();

                for (int j = 0; j < listHeaderSrc.Count; j++) {
                    //??? почему 3
                    //  , может у всех по 3(количество уровней) элемента
                    if (listHeaderSrc[j].values.Length < 3) {
                        arStrHeader = new string[listHeaderSrc[j].values.Length + 1];
                        arShortOrder = new short[listHeaderSrc[j].values.Length + 1];
                    } else {
                        arStrHeader = new string[listHeaderSrc[j].values.Length];
                        arShortOrder = new short[listHeaderSrc[j].values.Length];
                    }

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
                                                arShortOrder[level] = listHeaderSrc[j].order[(int)LEVEL_HEADER.TOP];

                                                listHeaderRes.Add(new HEADER() {
                                                    idNAlg = listHeaderSrc[j].idNAlg
                                                    , idComponent = listHeaderSrc[j].idComponent
                                                    , idPut = listHeaderSrc[j].idPut
                                                    , src = nalg_prop
                                                        //.m_strNameShr
                                                        .m_nAlg
                                                    , values = arStrHeader
                                                    , order = arShortOrder
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
                                if (listHeaderSrc[j].values.Length < 3) {
                                    arStrHeader[level + 1] = string.Empty;
                                    arShortOrder[level + 1] = -1;
                                } else
                                    ;

                                arStrHeader[(int)LEVEL_HEADER.MIDDLE] = nalg_prop.m_strNameShr; // listHeader[j].src
                                arShortOrder[(int)LEVEL_HEADER.MIDDLE] = listHeaderSrc[j].order[(int)LEVEL_HEADER.MIDDLE];
                                break;
                            case LEVEL_HEADER.LOW:
                                arStrHeader[level] = nalg_prop.m_strDescription;
                                arShortOrder[level] = listHeaderSrc[j].order[level];
                                break;
                            default:
                                break;
                        }
                    } // for - level
                }

                Comparison<HEADER> comparision = (h1, h2) => {
                    int iRes = 0;

                    int lOrder = Math.Min(h1.order.Length, h2.order.Length);

                    for (int i = 0; i < lOrder; i++) {
                        if (h1.order[i] > h2.order[i])
                            iRes = 1;
                        else if (h1.order[i] < h2.order[i])
                            iRes = -1;
                        else
                            ;

                        if (!(iRes == 0))
                            break;
                        else
                            ;
                    }

                    return iRes;
                };

                //HeaderComparer comparer = new HeaderComparer();
                //listHeaderRes.Sort(comparer);
                listHeaderRes.Sort(comparision);                
                return listHeaderRes;
            }

            protected override bool isRowToShowValues(DataGridViewRow r, TepCommon.HandlerDbTaskCalculate.VALUE value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
