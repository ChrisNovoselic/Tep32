﻿using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskAutobook
{
    partial class PanelTaskAutobookYearlyPlan
    {
        /// <summary>
        /// Панель отображения значений 
        /// и их обработки
        /// </summary>
        protected class DataGridViewAutobookYearlyPlan : DataGridViewValues
        {
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            private enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, MONTH_NAME, COUNT }

            /// <summary>
            /// основной конструктор
            /// </summary>
            /// <param name="nameDGV"></param>
            public DataGridViewAutobookYearlyPlan(string name) : base(ModeData.DATETIME)
            {
                Name = name;

                InitializeComponents();
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
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                AddColumn(-3, string.Empty, "ALG", true, false);
                AddColumn(-2, "Дата", "DATE", true, false);
                AddColumn(-1, "Месяц", "Month", true, true);
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


            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="text">Текст для заголовка столбца</param>
            /// <param name="bRead"></param>
            public void AddColumn(string txtHeader, bool bRead, string nameCol)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
                //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

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
                    Logging.Logg().Exception(e, @"DataGridViewAutoBookYearlyPlan::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавить столбец
            /// </summary>
            /// <param name="id_comp">номер компонента</param>
            /// <param name="txtHeader">заголовок столбца</param>
            /// <param name="nameCol">имя столбца</param>
            /// <param name="bRead">"только чтение"</param>
            /// <param name="bVisibled">видимость столбца</param>
            public void AddColumn(int id_comp, string txtHeader, string nameCol, bool bRead, bool bVisibled)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
                DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;

                try
                {
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    foreach (HDataGridViewColumn col in Columns)
                        if ((col.m_iIdComp > 0)
                            && (col.m_iIdComp < 1000))
                        {
                            indxCol = Columns.IndexOf(col);
                            break;
                        }

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_iIdComp = id_comp, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        if (id_comp < 0)
                        {// для служебных столбцов
                            if (bVisibled == true)
                            {// только для столбца с [SYMBOL]
                                alignText = DataGridViewContentAlignment.MiddleLeft;
                                autoSzColMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }
                            column.Frozen = true;
                            column.ReadOnly = true;
                        }
                    }

                    column.HeaderText = txtHeader;
                    column.Name = nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.AutoSizeMode = autoSzColMode;
                    column.Visible = bVisibled;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewTEPValues::AddColumn (id_comp=" + id_comp + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public void AddBRead(bool bRead)
            {
                foreach (HDataGridViewColumn col in Columns)
                    col.ReadOnly = bRead;
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
            ///// <summary>
            ///// Добавить строку в таблицу
            ///// </summary>
            //public void AddRow(ROW_PROPERTY rowProp)
            //{
            //    int i = -1;
            //    // создать строку
            //    DataGridViewRow row = new DataGridViewRow();
            //    if (m_dictPropertiesRows == null)
            //        m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

            //    if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
            //        m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

            //    // добавить строку
            //    i = Rows.Add(row);
            //    // установить значения в ячейках для служебной информации
            //    Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.MONTH_NAME].Value = rowProp.m_Value;
            //    Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
            //    // инициализировать значения в служебных ячейках
            //    m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
            //}

            /// <summary>
            /// заполнение датагрида
            /// </summary>
            /// <param name="tbOrigin">таблица значений</param>
            /// <param name="dgvView">контрол</param>
            public void ShowValues(DataTable tbOrigin)
            {
                double dblVal = -1F;
                int idAlg = -1;

                ClearValues();

                foreach (HDataGridViewColumn col in Columns)
                    if (col.Index > ((int)INDEX_SERVICE_COLUMN.COUNT - 1))
                        foreach (DataGridViewRow row in Rows)
                            for (int j = 0; j < tbOrigin.Rows.Count; j++)
                            {
                                idAlg = (int)row.Cells["ALG"].Value;

                                if (row.Cells["Month"].Value.ToString() ==
                                    GetMonth.ElementAt(Convert.ToDateTime(tbOrigin.Rows[j]["WR_DATETIME"]).AddMonths(-1).Month - 1))
                                {
                                    if (double.TryParse(tbOrigin.Rows[j]["VALUE"].ToString(), out dblVal) == true)
                                        dblVal = GetValueCellAsRatio(idAlg, -1, dblVal);
                                    else
                                    //???
                                        dblVal = 0F;

                                    row.Cells[col.Index].Value =
                                        dblVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
            }

            /// <summary>
            /// Установка идПута для столбца
            /// </summary>
            /// <param name="idPut">номер пута</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddIdComp(int idPut, string nameCol)
            {
                foreach (HDataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.m_iIdComp = idPut;
            }

            /// <summary>
            /// ??? Формирвоание значений
            /// </summary>
            /// <param name="idSession">номер сессии</param>
            public DataTable FillTableEdit(int idSession)
            {
                //int i = 0
                //    , idAlg = -1
                //    , vsRatioValue = -1;
                //double valueToRes;

                DataTable editTable = new DataTable();
                //editTable.Columns.AddRange(new DataColumn[] {
                //        new DataColumn (@"ID_PUT", typeof (int))
                //        , new DataColumn (@"ID_SESSION", typeof (long))
                //        , new DataColumn (@"QUALITY", typeof (int))
                //        , new DataColumn (@"VALUE", typeof (float))
                //        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                //        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                //    });

                //foreach (HDataGridViewColumn col in Columns)
                //    if (col.m_iIdComp > 0)
                //        foreach (DataGridViewRow row in Rows) {
                //            idAlg = (int)row.Cells["ALG"].Value;
                //            vsRatioValue = m_dictRatio[m_dictNAlgProperties[idAlg].m_vsRatio].m_value;

                //            if (row.Cells[col.Index].Value != null)
                //                if (double.TryParse(row.Cells[col.Index].Value.ToString(), out valueToRes))
                //                    editTable.Rows.Add(new object[] {
                //                        col.m_iIdComp
                //                        , idSession
                //                        , 1.ToString()
                //                        , valueToRes *= Math.Pow(10F, 1 * vsRatioValue)
                //                        , Convert.ToDateTime(row.Cells[INDEX_SERVICE_COLUMN.DATE.ToString()].Value.ToString()).ToString("F", editTable.Locale)
                //                        , i
                //                    });
                //                else
                //                    ;
                //            else
                //                ;

                //            i++;
                //        }
                //else
                //    ;

                return editTable;
            }
        }
    }
}
