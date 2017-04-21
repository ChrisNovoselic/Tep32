﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskTepMain
{
    public abstract partial class PanelTaskTepValues : PanelTaskTepCalculate
    {
        /// <summary>
        /// Класс для отображения значений входных/выходных для расчета ТЭП  параметров
        /// </summary>
        protected class DataGridViewTaskTepValues : DataGridViewTaskTepCalculate
        {
            public override void AddColumns(List<TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewTaskTepValues()
                : base()
            {
                //Разместить ячейки, установить свойства объекта
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                //AddColumn(-2, string.Empty, false);
                addColumn(
                    new TepCommon.HandlerDbTaskCalculate.TECComponent(-1, -1
                        , @"Размерность"
                        , false, false)
                    , ModeAddColumn.Service | ModeAddColumn.Visibled);

                RowHeadersVisible = true;
            }

            public override void ClearColumns()
            {
                List<DataGridViewColumn> listIndxToRemove;

                if (Columns.Count > 0) {
                    listIndxToRemove = new List<DataGridViewColumn>();

                    foreach (DataGridViewColumn col in Columns)
                        if (!(((TepCommon.HandlerDbTaskCalculate.TECComponent)col.Tag).m_Id < 0))
                            listIndxToRemove.Add(col);
                        else
                            ;

                    while (listIndxToRemove.Count > 0) {
                        Columns.Remove(listIndxToRemove[0]);
                        listIndxToRemove.RemoveAt(0);
                    }
                } else
                    ;
            }

            public void AddColumn(TepCommon.HandlerDbTaskCalculate.TECComponent comp)
            {
                addColumn(comp, ModeAddColumn.Insert | ModeAddColumn.Visibled);
            }
            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">Идентификатор компонента ТЭЦ</param>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bVisibled">Признак участия в расчете/отображения</param>
            protected override void addColumn(TepCommon.HandlerDbTaskCalculate.TECComponent comp, ModeAddColumn mode)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (DataGridViewColumn col in Columns)
                        if ((((TepCommon.HandlerDbTaskCalculate.TECComponent)col.Tag).m_Id > 0)
                            && (((TepCommon.HandlerDbTaskCalculate.TECComponent)col.Tag).m_Id < (int)TepCommon.HandlerDbTaskCalculate.TECComponent.TYPE.TG)) {
                            indxCol = Columns.IndexOf(col);

                            break;
                        } else
                            ;

                    DataGridViewColumn column = new DataGridViewTextBoxColumn();
                    column.Tag = comp;
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else {// для добавлямых столбцов
                        if ((mode & ModeAddColumn.Service) == ModeAddColumn.Service) {// для служебных столбцов
                            if ((mode & ModeAddColumn.Visibled) == ModeAddColumn.Visibled) {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            } else
                                ;

                            column.Frozen = true;
                            column.ReadOnly = true;
                        } else
                            ;
                    }

                    column.HeaderText = comp.m_nameShr;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = (mode & ModeAddColumn.Visibled) == ModeAddColumn.Visibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                } catch (Exception e) {
                    Logging.Logg().Exception(e
                        , string.Format(@"DataGridViewTEPValues::AddColumn (id_comp={0}) - ...", comp.m_Id)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="obj">Объект с параметром алгоритма расчета</param>
            public int AddRow(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER obj)
            {
                int iRes = -1;

                //!!! Объект уже добавлен в словарь
                //!!! столбец с 'SYMBOL' уже добавлен

                activateCellValue_onChanged(false);

                iRes = Rows.Add(new DataGridViewRow());
                Rows[iRes].Tag = obj.m_Id;

                // установить значение для заголовка
                Rows[iRes].HeaderCell.Value = obj.m_nAlg;
                // установить значение для всплывающей подсказки
                Rows[iRes].HeaderCell.ToolTipText = obj.m_strDescription;
                // установить значение для обозначения параметра и его ед./измерения
                Rows[iRes].Cells[0].Value = string.Format(@"{0},[{1}]", obj.m_strSymbol, obj.m_strMeausure);

                activateCellValue_onChanged(true);

                return iRes;
            }

            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="bNewEnabled">Новое (устанавливаемое) значение признака участия в расчете для параметра</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности изменения цвета ячейки</returns>
            private bool getColorCellToColumn(int iCol, int iRow, bool bNewEnabled, out Color clrRes)
            {
                bool bRes = false;

                int id_alg = -1
                    , id_comp = -1;
                TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality;

                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                id_alg = -1;
                id_comp = -1;

                if ((!(Rows[iRow].Cells[iCol].Tag == null))
                    && (Rows[iRow].Cells[iCol].Tag is CELL_PROPERTY)) {
                    iQuality = ((CELL_PROPERTY)Rows[iRow].Cells[iCol].Tag).m_iQuality;

                    bRes = ((m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].m_bEnabled == false)
                        && ((m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].IsNaN == false)));
                    if (bRes == true)
                        if (bNewEnabled == true)
                            switch (iQuality) {//??? LIMIT
                                case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER:
                                    clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                    break;
                                case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.SOURCE:
                                    clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                    break;
                                case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL:
                                    clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                    break;
                                case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC:
                                    clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                    break;
                                case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT:
                                    clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                                    break;
                                default:
                                    ; //??? throw
                                    break;
                            } else
                            clrRes = s_arCellColors[(int)INDEX_COLOR.DISABLED];
                    else
                        ;
                } else
                    //??? значению в ячейке не присвоена квалификация - значение не присваивалось
                    ;

                return bRes;
            }

            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="iCol">Индекс столбца ячейки</param>
            /// <param name="iRow">Индекс строки ячейки</param>
            /// <param name="bNewCalcDeny">Новое (устанавливаемое) значение признака участия в расчете для параметра</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности изменения цвета ячейки</returns>
            private bool getColorCellToRow(int iCol, int iRow, bool bNewEnabled, out Color clrRes)
            {
                bool bRes = false;

                int id_alg = -1
                    , id_comp = -1;
                TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality;
                bool bPrevEnabled = false;

                bRes = m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].IsNaN == false;
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                id_alg = -1;
                id_comp = -1;
                iQuality = ((CELL_PROPERTY)Rows[iRow].Cells[iCol].Tag).m_iQuality;

                //??? определить предыдущее состояние
                bPrevEnabled = ((TepCommon.HandlerDbTaskCalculate.TECComponent)Columns.Cast<DataGridViewColumn>().First(col => { return ((TepCommon.HandlerDbTaskCalculate.TECComponent)col.Tag).m_Id == id_comp; }).Tag).m_bEnabled;

                if (bRes == true)
                    if ((bNewEnabled == true)
                        && (bPrevEnabled == false))
                        switch (iQuality) {//??? LIMIT
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.USER];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.SOURCE:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                                break;
                            case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT:
                                clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                                break;
                            default:
                                ; //??? throw
                                break;
                        } else
                        clrRes = s_arCellColors[(int)INDEX_COLOR.DISABLED];
                else
                    ;

                return bRes;
            }            

            /// <summary>
            /// Обновить структуру таблицы (доступность(цвет)/видимость столбцов/строк)
            /// </summary>
            /// <param name="item">Аргумент события для обновления структуры представления</param>
            public /*override*/ void UpdateStructure(PanelManagementTaskTepValues.ItemCheckedParametersEventArgs item)
            {
                Color clrCell = Color.Empty; //Цвет фона для ячеек, не участвующих в расчете
                int indx = -1;
                bool bItemChecked = item.NewCheckState == CheckState.Checked ? true :
                    item.NewCheckState == CheckState.Unchecked ? false :
                        false;

                //Поиск индекса элемента отображения
                if (item.IsComponent == true) {
                    // найти индекс столбца (компонента) - по идентификатору
                    foreach (DataGridViewColumn c in Columns)
                        if (((TepCommon.HandlerDbTaskCalculate.TECComponent)c.Tag).m_Id == item.m_idComp) {
                            indx = Columns.IndexOf(c);
                            break;
                        } else
                            ;
                } else if (item.IsNAlg == true) {
                    // найти индекс строки (параметра) - по идентификатору
                    // вариант №1
                    indx = Rows.Cast<DataGridViewRow>().First(r => { return (int)r.Tag == item.m_idAlg; }).Index;
                    //// // вариант №2
                    //indx = (
                    //        from r in Rows.Cast<DataGridViewRow>()
                    //        where (int)r.Tag == item.m_idAlg
                    //        select new { r.Index }
                    //    ).Cast<int>().ElementAt<int>(0);
                    //// // вариант №3
                    //foreach (DataGridViewRow r in Rows)
                    //    if ((int)r.Tag == item.m_idAlg) {
                    //        indx = Rows.IndexOf(r);
                    //        break;
                    //    } else
                    //        ;
                } else
                    ;

                if (!(indx < 0))
                    if (item.m_type == PanelManagementTaskTepValues.ItemCheckedParametersEventArgs.TYPE.ENABLE) {
                        if (item.IsComponent == true) { // COMPONENT ENABLE
                            // для всех ячеек в столбце
                            foreach (DataGridViewRow r in Rows) {
                                if (getColorCellToColumn(indx, r.Index, bItemChecked, out clrCell) == true)
                                    r.Cells[indx].Style.BackColor = clrCell;
                                else
                                    ;
                            }
                            ((TepCommon.HandlerDbTaskCalculate.TECComponent)Columns[indx].Tag).SetEnabled(bItemChecked);
                        } else if (item.IsNAlg == true) { // NALG ENABLE
                            // для всех ячеек в строке
                            foreach (DataGridViewCell c in Rows[indx].Cells) {
                                if (getColorCellToRow(c.ColumnIndex, indx, bItemChecked, out clrCell) == true)
                                    c.Style.BackColor = clrCell;
                                else
                                    ;

                                m_dictNAlgProperties.SetEnabled((int)Rows[indx].Tag, ((TepCommon.HandlerDbTaskCalculate.TECComponent)Columns[c.ColumnIndex].Tag).m_Id, bItemChecked);
                            }
                        } else
                            ;
                    } else if (item.m_type == PanelManagementTaskTepValues.ItemCheckedParametersEventArgs.TYPE.VISIBLE) {
                        if (item.IsComponent == true) { // COMPONENT VISIBLE
                            // для всех ячеек в столбце
                            Columns[indx].Visible = bItemChecked;
                        } else if (item.IsNAlg == true) {  // NALG VISIBLE
                            // для всех ячеек в строке
                            Rows[indx].Visible = bItemChecked;
                        } else
                            ;
                    } else
                        ;
                else
                    // нет элемента для изменения стиля
                    ;
            }

            protected override bool isRowToShowValues(DataGridViewRow r, TepCommon.HandlerDbTaskCalculate.VALUE value)
            {
                return m_dictNAlgProperties[(int)r.Tag].m_dictPutParameters.ContainsKey(value.m_IdPut) == true;
            }

            /// <summary>
            /// Очистить содержание представления (например, перед )
            /// </summary>
            public override void ClearValues()
            {
                base.ClearValues();
            }

            ///// <summary>
            ///// обработчик события - изменение значения в ячейке
            ///// </summary>
            ///// <param name="obj">Обхект, иницировавший событие</param>
            ///// <param name="ev">Аргумент события</param>
            //private void onCellValueChanged(object obj, DataGridViewCellEventArgs ev)
            //{
            //    string strValue = string.Empty;
            //    double dblValue = double.NaN;
            //    int id_alg = -1
            //        , id_comp = -1;

            //    try {
            //        if ((!(ev.ColumnIndex < 0))
            //            && (!(ev.RowIndex < 0))) {
            //            id_alg = (int)Rows[ev.RowIndex].Tag;
            //            id_comp = ((TepCommon.HandlerDbTaskCalculate.TECComponent)Columns[ev.ColumnIndex].Tag).m_Id; //Идентификатор компонента

            //            if ((id_comp > 0) // только для реальных компонентов
            //                && (!(ev.RowIndex < 0))) {
            //                strValue = (string)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value;

            //                if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dblValue) == true) {
            //                    ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).SetValue(dblValue);

            //                    EventCellValueChanged(this, new DataGridViewTaskTepValues.DataGridViewTEPValuesCellValueChangedEventArgs(
            //                        id_alg //Идентификатор параметра [alg]
            //                        , id_comp
            //                        , m_dictNAlgProperties[id_alg].m_dictPutParameters[id_comp].m_Id //Идентификатор параметра с учетом периода расчета [put]
            //                        , ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).m_iQuality
            //                        , ((CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag).m_Value));
            //                } else
            //                    ; //??? невозможно преобразовать значение - отобразить сообщение для пользователя
            //            } else
            //                ; // в 0-ом столбце идентификатор параметра расчета
            //        } else
            //            ; // невозможно адресовать ячейку
            //    } catch (Exception e) {
            //        Logging.Logg().Exception(e, @"DataGridViewTEPValues::onCellValueChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //}
        }
    }
}
