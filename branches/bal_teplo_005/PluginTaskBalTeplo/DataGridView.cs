using ASUTP;
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
        protected class DataGridViewBalTeploValues : DataGridViewValues
        {
            private Dictionary<string, HTepUsers.DictionaryProfileItem> m_dict_ProfileNALG_IN
                , m_dict_ProfileNALG_OUT;
            private DataTable m_dbRatio;

            List<int> m_put_params_in, m_put_params_out;

            public enum INDEX_VIEW_VALUES { Block = 2001, Output = 2002, TeploBL = 2003, TeploOP = 2004, Param = 2005, PromPlozsh = 2006 };

            //public enum PUT_PARAMS_IN { 23573 };

            //public enum PUT_PARAMS_OUT { };

            public INDEX_VIEW_VALUES m_ViewValues;

            public DataGridViewBalTeploValues(string name, Func<int, int, float, int, float> fGetValueAsRatio)
                : base(ModeData.NALG, fGetValueAsRatio)
            {
                m_dict_ProfileNALG_IN = new Dictionary<string, HTepUsers.DictionaryProfileItem>();
                m_dict_ProfileNALG_OUT = new Dictionary<string, HTepUsers.DictionaryProfileItem>();
                m_dbRatio = new DataTable();

                m_put_params_in = new List<int> { 23573, 23580, 23587, 23594, 23601, 23573 }; // id?
                m_put_params_out = new List<int> { 23573 };

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

            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">номер компонента</param>
            /// <param name="txtHeader">заголовок столбца</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="bRead">"только чтение"</param>
            /// <param name="bVisibled">видимость столбца</param>
            public void AddColumn(HandlerDbTaskCalculate.IPUT_PARAMETERChange putPar)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (DataGridViewColumn col in Columns)
                        if ((((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).IdComponent > 0)
                            && (((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).m_component.IsTec == true))
                        {
                            indxCol = Columns.IndexOf(col);

                            break;
                        }
                        else
                            ;

                    DataGridViewColumn column = new DataGridViewTextBoxColumn();
                    column.Tag = new COLUMN_TAG(putPar, ColumnCount + 2, false);
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для псевдо-столбцов
                        if (putPar.IdComponent < 0)
                        {// для служебных столбцов
                            if (putPar.IsVisibled == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }
                            else
                            { }

                            column.Frozen = true;
                        }
                        else
                        { }
                    }

                    column.HeaderText = putPar.NameShrComponent;
                    column.ReadOnly = putPar.IsEnabled;
                    column.Name = @"???";
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = putPar.IsVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, string.Format(@"DataGridViewTReaktivka::AddColumn (id_comp={0}) - ...", putPar.IdComponent), Logging.INDEX_MESSAGE.NOT_SET);
                }
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
                            idRatio = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_IN[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND).ToString()]);
                        }
                        else
                        {
                            idRatio = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO).ToString()]);
                            iRound = int.Parse(m_dict_ProfileNALG_OUT[((HDataGridViewColumn)Columns[e.ColumnIndex]).m_N_ALG.ToString().Trim()].Attributes[((int)HTepUsers.ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND).ToString()]);
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

                try {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleLeft;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Tag = -1; // заголовок столбца -1
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
            public void AddColumn(string txtHeader, bool bRead, string nameCol, string N_ALG, bool bInPut, int Tag, TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                HDataGridViewColumn column;
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try {
                    column = new HDataGridViewColumn() { m_bCalcDeny = false, m_N_ALG = N_ALG, m_bInPut = bInPut };
                    alignText = DataGridViewContentAlignment.MiddleLeft;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
                    column.ReadOnly = bRead;
                    column.Name = nameCol;
                    column.HeaderText = txtHeader;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Tag = new COLUMN_TAG(putPar, ColumnCount + 2, false);
                    Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
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
                            c.Value = null; // нужен ли вызов onCellValueChanged??
                        }
                        else
                            ;
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            public void SetReadOnly(bool value)
            {
                foreach (DataGridViewColumn col in Columns)
                    if (((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).IdComponent > 0)
                        col.ReadOnly = value;
                    else
                        ;
            }

            /// <summary>
            /// Признак, указывающий принажлежит ли значение строке
            /// иными словами: отображать ли значение в этой строке
            /// </summary>
            /// <param name="r">Строка (проверяемая) для отображения значения</param>
            /// <param name="value">Значение для отображения в строке</param>
            /// <returns>Признак - результат проверки условия (Истина - отображать/принадлежит)</returns>
            protected override bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value)
            {
                return (r.Tag is DateTime) ? value.stamp_value.Equals(((DateTime)(r.Tag))) == true : false;
            }

            /// <summary>
            /// Заполнение датагрида
            /// </summary>
            /// <param name="inValues">Список входных значений</param>
            /// <param name="outValues">Список выходных значений</param>
            public void ShowValues(IEnumerable<HandlerDbTaskCalculate.VALUE> inValues
                , IEnumerable<HandlerDbTaskCalculate.VALUE> outValues)
            {
                int idAlg = -1
                   , idPut = -1
                   , idPutCell = -1
                   , iCol = 0;
                float fltVal = -1F,
                        fltColumnAgregateValue = 0;
                AGREGATE_ACTION columnAction = AGREGATE_ACTION.UNKNOWN;
                HandlerDbTaskCalculate.IPUT_PARAMETERChange putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();
                IEnumerable<HandlerDbTaskCalculate.VALUE> columnValues = null;
                IEnumerable<HandlerDbTaskCalculate.VALUE> cellValues = null;

                DataGridViewRow row;

                #region делегат для поиска значений во входных аргументах (для столбца; для ячейки)
                Func<HandlerDbTaskCalculate.VALUE, int, bool> get_values = (HandlerDbTaskCalculate.VALUE value, int id_put) => {
                    return (value.m_IdPut == id_put)
                        && (((value.stamp_value - DateTime.MinValue).TotalDays > 0)
                            || ((!((value.stamp_value - DateTime.MinValue).TotalDays > 0)))
                        );
                };
                #endregion

                #region делегат для отображения значений с попутной установкой значений свойств ячейки, локальных переменных
                Action<DataGridViewCell, HandlerDbTaskCalculate.VALUE, AGREGATE_ACTION> show_value = (DataGridViewCell cell, HandlerDbTaskCalculate.VALUE value, AGREGATE_ACTION c_action) =>
                {
                    fltVal = value.value;

                    if ((c_action == AGREGATE_ACTION.UNKNOWN))
                        fltColumnAgregateValue += fltVal;
                    else
                        ;

                    //cell.Tag = new CELL_PROPERTY() { m_Value = fltVal, m_iQuality = value.m_iQuality };
                    cell.ReadOnly = Columns[iCol].ReadOnly
                        || double.IsNaN(fltVal);

                        // отобразить с количеством знаков в соответствии с настройками
                    cell.Value = fltVal;
                    cell.ToolTipText = fltVal.ToString();
                };
                #endregion

                // Т.к. предполагается, что в наличии минимальный набор: "строка с данными" + "итоговая строка"
                if (RowCount > 1)
                {
                    // отменить обработку события - изменение значения в ячейке представления
                    //activateCellValue_onChanged(false);

                    // возможно, цикл по строкам? 
                    foreach (DataGridViewColumn col in Columns)
                    {
                        iCol = col.Index;
                        fltColumnAgregateValue = 0F;
                        try
                        {
                            // получаем id колонки, по логике = id станции
                            putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value;
                            idPut = putPar.m_Id;

                            // все значения, относящиеся к выбранному id (общестанц.) ??? перенести
                            columnValues = inValues.Where(value => get_values(value, idPut));
                            columnValues = columnValues.Union(outValues.Where(value => get_values(value, idPut)));

                            idAlg = putPar.m_idNAlg;

                            // получить тип действия над переменной (суммирование/усреднение)
                            columnAction = ((COLUMN_TAG)col.Tag).ActionAgregateCancel == true ? AGREGATE_ACTION.UNKNOWN : getColumnAction(idAlg);

                            for (int ind = 0; ind < Rows.Count - 1; ind++) // исключая строку "Итого"
                            {
                                row = Rows[ind];

                                // id параметра для конкретной ТГ
                                idPutCell = (Int32)row.Cells[iCol].Tag;

                                // все значения, относящиеся к выбранному id (конкр. оборуд.)
                                cellValues = inValues.Where(value => get_values(value, idPutCell));
                                cellValues = columnValues.Union(outValues.Where(value => get_values(value, idPutCell)));

                                if (columnValues.Count() > 0)
                                    // есть значение хотя бы для одной строки
                                    foreach (HandlerDbTaskCalculate.VALUE value in cellValues)
                                    {
                                        show_value(row.Cells[iCol], value , columnAction);
                                    }
                                else
                                {
                                    // нет значений ни для одной строки
                                    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - нет строк для отображения..."), Logging.INDEX_MESSAGE.NOT_SET);
                                }
                            }

                            // вывод суммарных значений (строка "Итого") ??? необходим отдельный метод для усреднения, зачем???
                            if ((fltColumnAgregateValue > float.MinValue)
                                                        && (!(columnAction == AGREGATE_ACTION.UNKNOWN)))
                                Rows[Rows.Count - 1].Cells[iCol].Value = columnAction == AGREGATE_ACTION.SUMMA
                                    ? fltColumnAgregateValue
                                        : fltColumnAgregateValue / (Rows.Count - 1);
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"DataGridViewValues::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }  
                    }
                    // восстановить обработку события - изменение значение в ячейке
                    //activateCellValue_onChanged(true);
                }
                else
                    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - нет строк для отображения..."), Logging.INDEX_MESSAGE.NOT_SET);
            
            }

            /// <summary>
            /// Возвратить тип агрегационной функции над множеством значений в столбце
            /// </summary>
            /// <param name="id_alg">Идентификатор параметра в аогоритме расчета 1-го порядка</param>
            /// <returns>Тип агрегационной функции над множеством значений в столбце</returns>
            private AGREGATE_ACTION getColumnAction(int id_alg)
            {
                //return Rows[RowCount - 1].Tag.GetType().IsPrimitive == true // есть ли итоговая строка?
                //    ? m_dictNAlgProperties[id_alg].m_sAverage // итоговая строка - есть (операция по агрегации известна)
                //        : AGREGATE_ACTION.UNKNOWN; // итоговой строки - нет (операция по агрегации неизвестна и не выполняется)

                return AGREGATE_ACTION.SUMMA;
            }

            DataRow[] colums_in;
            DataRow[] colums_out;
            DataRow[] rows;

            /// <summary>
            /// Сформировать списки строк, содержащих параметры для указанной таблицы (входной и выходной)
            /// </summary>
            /// <param name=""></param>
            public void getTableNalg(DataTable tableInNAlg, DataTable tableOutNAlg, DataTable tableComp, Dictionary<int, object[]> dict_profile)
            {
                List<DataRow> col_in = new List<DataRow>();
                List<DataRow> col_out = new List<DataRow>();

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
                    { }

                    if ((TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE)list[1] == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                    {
                        m_dict_ProfileNALG_OUT = (Dictionary<string, HTepUsers.DictionaryProfileItem>)list[2];

                        foreach (Double id in (double[])list[0])
                            col_out.Add(tableOutNAlg.Select("N_ALG='" + id.ToString().Trim().Replace(',', '.') + "'")[0]);
                    }
                    else
                    { }
                }
                colums_in = col_in.ToArray();
                colums_out = col_out.ToArray();
            }

            /// <summary>
            /// Инициализация dataGridView 
            /// </summary>
            /// <param name="tableInNAlg"></param>
            /// <param name="tableOutNAlg"></param>
            /// <param name="tableComp"></param>
            /// <returns></returns>
            public void InitializeStruct(DataTable tableInNAlg, DataTable tableOutNAlg, DataTable tableComp, Dictionary<int, object[]> dict_profile, DataTable tableRatio
                , List<Int32> listIDCells)
            {
                this.CellValueChanged -= new DataGridViewCellEventHandler(cellEndEdit);
                this.Rows.Clear();
                this.Columns.Clear();
                DataTable tableNAlg = new DataTable();

                List<DataRow> col_in = new List<DataRow>();
                List<DataRow> col_out = new List<DataRow>();
                m_dbRatio = tableRatio.Copy();
                int indx;

                TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();

                getTableNalg(tableInNAlg, tableOutNAlg, tableComp, dict_profile);

                // получить список ID из таблицы inval, для размещения в TAG столбцов и ячеек ???

                indx = 0;
                this.AddColumn("Компонент", true, "Comp");
                foreach (DataRow c in colums_in)
                {
                    putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();
                    //putPar.m_Id = 23573;
                    putPar.m_Id = m_put_params_in[indx];
                    putPar.m_idNAlg = (Int32)(c[0]);
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), (c["N_ALG"]).ToString(), true, (Int32)c[0], putPar);
                    indx++;
                }
                indx = 0;
                foreach (DataRow c in colums_out)
                {
                    putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();
                    //putPar.m_Id = 23573;
                    putPar.m_Id = m_put_params_out[indx];
                    putPar.m_idNAlg = (Int32)(c[0]);
                    this.AddColumn(c["NAME_SHR"].ToString().Trim(), true, c["NAME_SHR"].ToString().Trim(), (c["N_ALG"]).ToString(), false, (Int32)c[0], putPar);
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
                { }

                this.CellValueChanged += new DataGridViewCellEventHandler(cellEndEdit);
            }
        }
    }
}
