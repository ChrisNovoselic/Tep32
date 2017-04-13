﻿using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskAutobook
{
    partial class PanelTaskAutobookMonthValues
    {
        /// <summary>
        /// Класс для грида
        /// </summary>
        private class DataGridViewAutobookMonthValues : DataGridViewValues
        {
            ///// <summary>
            ///// Перечисление для индексации столбцов со служебной информацией
            ///// </summary>
            //protected enum INDEX_SERVICE_COLUMN : uint { ALG, DATE, COUNT }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            /// <param name="nameDGV">Наименование элемента управления</param>
            public DataGridViewAutobookMonthValues(string name) : base(ModeData.DATETIME)
            {
                Name = name;

                InitializeComponents();

                m_dictTECComponent = new Dictionary<int, HandlerDbTaskCalculate.TECComponent>();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Dock = DockStyle.Fill;
                Name = INDEX_CONTROL.DGV_VALUES.ToString();                                

                //Ширина столбцов под видимую область
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                RowHeadersVisible = true;

                //AddColumn(-2, string.Empty, INDEX_SERVICE_COLUMN.ALG.ToString(), true, false);
                //AddColumn(-1, "Дата", INDEX_SERVICE_COLUMN.DATE.ToString(), true, true);
            }

            public override void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                List<HandlerDbTaskCalculate.TECComponent> listTECComponent;
                HandlerDbTaskCalculate.TECComponent comp_tec;

                listTECComponent = new List<HandlerDbTaskCalculate.TECComponent>();

                listPutParameter.ForEach(putPar => {
                    if (listTECComponent.IndexOf(putPar.m_component) < 0)
                        listTECComponent.Add(putPar.m_component);
                    else
                        ;
                });
                // сортировать ГТП: 1,2 ПЕРЕД 3-6
                listTECComponent.Sort(delegate (HandlerDbTaskCalculate.TECComponent comp1, HandlerDbTaskCalculate.TECComponent comp2) {
                    return comp1.m_Id < comp2.m_Id ? -1
                        : comp1.m_Id > comp2.m_Id ? 1
                            : 0;
                });
                //ГТП - Корректировка ПТО
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true)
                        Columns.Add(string.Format(@"COLUMN_CORRECT_{0}", comp.m_Id), string.Format(@"Корр-ка ПТО {0}", comp.m_nameShr));
                    else
                        ;
                });
                //ГТП - Значения
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true)
                        Columns.Add(string.Format(@"COLUMN_{0}", comp.m_Id), string.Format(@"{0}", comp.m_nameShr));
                    else
                        ;
                });
                //Станция - компонент
                comp_tec = listTECComponent.Find(comp => { return comp.IsTec; });
                //Станция - Значения - ежесуточные
                Columns.Add(string.Format(@"COLUMN_ST_DAY_{0}", comp_tec.m_Id), string.Format(@"{0} значения", comp_tec.m_nameShr));
                //Станция - Значения - нарастающие
                Columns.Add(string.Format(@"COLUMN_ST_SUM_{0}", comp_tec.m_Id), string.Format(@"{0} нараст.", comp_tec.m_nameShr));
                //Станция - План - ежесуточный
                Columns.Add(string.Format(@"COLUMN_PLAN_DAY_{0}", comp_tec.m_Id), string.Format(@"План нараст."));
                //Станция - План - отклонение
                Columns.Add(string.Format(@"COLUMN_PALN_DEV_{0}", comp_tec.m_Id), string.Format(@"План отклон."));

                //Columns.Add(string.Format(@""), string.Format(@""));
                //Columns.Add(string.Format(@""), string.Format(@""));
                //Columns.Add(string.Format(@""), string.Format(@""));
            }

            ///// <summary>
            ///// Добавить столбец
            ///// </summary>
            ///// <param name="text">Текст для заголовка столбца</param>
            ///// <param name="bRead">Флаг изменения пользователем ячейки</param>
            ///// <param name="nameCol">Наименование столбца</param>
            //public void AddColumn(string txtHeader, bool bRead, string nameCol)
            //{
            //    DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;
            //    DataGridViewAutoSizeColumnMode autoSzColMode = DataGridViewAutoSizeColumnMode.NotSet;
            //    //DataGridViewColumnHeadersHeightSizeMode HeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            //    try {
            //        HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false };
            //        alignText = DataGridViewContentAlignment.MiddleRight;
            //        autoSzColMode = DataGridViewAutoSizeColumnMode.Fill;
            //        //column.Frozen = true;
            //        column.ReadOnly = bRead;
            //        column.Name = nameCol;
            //        column.HeaderText = txtHeader;
            //        column.DefaultCellStyle.Alignment = alignText;
            //        column.AutoSizeMode = autoSzColMode;
            //        Columns.Add(column as DataGridViewTextBoxColumn);
            //    } catch (Exception e) {
            //        HClassLibrary.Logging.Logg().Exception(e, @"DGVAutoBook::AddColumn () - ...", HClassLibrary.Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //}

            ///// <summary>
            ///// Установка идПута для столбца
            ///// </summary>
            ///// <param name="idPut">номер пута</param>
            ///// <param name="nameCol">имя стобца</param>
            //public void AddIdComp(int idPut, string nameCol)
            //{
            //    foreach (HDataGridViewColumn col in Columns)
            //        if (col.Name == nameCol)
            //            col.m_iIdComp = idPut;
            //        else
            //            ;
            //}

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="nameCol">имя стобца</param>
            /// <param name="bReadOnly">true/false</param>            
            public void SetReadOnly(string nameCol, bool bReadOnly)
            {
                foreach (DataGridViewColumn col in Columns)
                    if (col.Name == nameCol)
                        col.ReadOnly = bReadOnly;
                    else
                        ;
            }

            private Dictionary<int, HandlerDbTaskCalculate.TECComponent> m_dictTECComponent;

            public override void AddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                base.AddPutParameter(putPar);

                if (m_dictTECComponent.ContainsKey(putPar.IdComponent) == false)
                    m_dictTECComponent.Add(putPar.IdComponent, putPar.m_component);
                else
                    ;
            }

            public override void Clear()
            {
                base.Clear();

                m_dictTECComponent.Clear();
            }
            /// <summary>
            /// Отобразить значения
            /// </summary>
            /// <param name="inValues">Входные значения</param>
            /// <param name="outValues">Выходные значения</param>
            public override void ShowValues(IEnumerable<HandlerDbTaskCalculate.VALUE> inValues, IEnumerable<HandlerDbTaskCalculate.VALUE> outValues, out int err)
            {
                throw new NotImplementedException();
            }
        }
    }
}
