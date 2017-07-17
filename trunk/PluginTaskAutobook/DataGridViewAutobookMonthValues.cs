using HClassLibrary;
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
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            /// <param name="nameDGV">Наименование элемента управления</param>
            public DataGridViewAutobookMonthValues(string name, Func<int, int, float, int, float> fGetValueAsRatio) : base(ModeData.DATETIME, fGetValueAsRatio)
            {
                Name = name;

                InitializeComponents();

                m_dictTECComponent = new Dictionary<int, TepCommon.HandlerDbTaskCalculate.TECComponent>();
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
            }

            public override void AddColumns(List<TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter
                , List<TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> listPutParameter)
            {
                DataGridViewColumn column;
                List<TepCommon.HandlerDbTaskCalculate.TECComponent> listTECComponent;
                TepCommon.HandlerDbTaskCalculate.TECComponent comp_tec;
                string strFormula = string.Empty;

                // Функция поиска объекта 'PUT_PARAMETER' для его назначения в свойство 'Tag' для добавляемого столбца
                Func<HandlerDbTaskCalculate.TaskCalculate.TYPE, int, TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> findPutParameterGTP = (HandlerDbTaskCalculate.TaskCalculate.TYPE type, int id) => {
                    TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putRes = new TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER();

                    IEnumerable<TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER> puts;
                    TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER nAlgRes = null;

                    puts = listPutParameter.Where(putPar => { return putPar.IdComponent == id; });
                    if (puts.Count() > 0) {
                        foreach (TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putPar in puts) {
                            nAlgRes = listNAlgParameter.FirstOrDefault(nAlg => { return (nAlg.m_Id == putPar.m_idNAlg) && (nAlg.m_type == type); });

                            if (!(nAlgRes == null)) {
                                putRes = putPar;

                                break;
                            } else
                                ;
                        }
                    } else
                    // ошибка на 1-ом этапе - возвращается объект по умолчанию (IsNaN == true)
                        ;

                    return putRes;
                };

                listTECComponent = new List<TepCommon.HandlerDbTaskCalculate.TECComponent>();

                listPutParameter.ForEach(putPar => {
                    if (listTECComponent.IndexOf(putPar.m_component) < 0)
                        listTECComponent.Add(putPar.m_component);
                    else
                        ;
                });
                // сортировать ГТП: сначала 1,2 затем 3-6
                listTECComponent.Sort(delegate (TepCommon.HandlerDbTaskCalculate.TECComponent comp1, TepCommon.HandlerDbTaskCalculate.TECComponent comp2) {
                    return comp1.m_Id < comp2.m_Id ? -1
                        : comp1.m_Id > comp2.m_Id ? 1
                            : 0;
                });
                //ГТП - Корректировка ПТО
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true) {
                        column = new DataGridViewTextBoxColumn();
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.Name = string.Format(@"COLUMN_CORRECT_{0}", comp.m_Id); column.HeaderText = string.Format(@"Корр-ка ПТО {0}", comp.m_nameShr);
                        column.Tag = new COLUMN_TAG(findPutParameterGTP(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, comp.m_Id), false);
                        column.ReadOnly = false;
                        Columns.Add(column);
                    } else
                        ;
                });
                //ГТП - Значения (без корректировки)
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true) {
                        column = new DataGridViewTextBoxColumn();
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.Name = string.Format(@"COLUMN_VALUE_{0}", comp.m_Id);
                        column.HeaderText = string.Format(@"По прибору {0}", comp.m_nameShr);
                        column.Tag = new COLUMN_TAG(findPutParameterGTP(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, comp.m_Id), false);
                        column.Visible = false;
                        column.ReadOnly = true;
                        Columns.Add(column);
                    } else
                        ;
                });
                //ГТП - Значения по прибору учета с учетом корректрировки
                listTECComponent.ForEach(comp => {
                    if (comp.IsGtp == true) {
                        column = new DataGridViewTextBoxColumn();
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.Name = string.Format(@"CORRECT_VALUE_{0}", comp.m_Id); column.HeaderText = string.Format(@"+ коррект. {0}", comp.m_nameShr);
                        column.Tag = new COLUMN_TAG (new FormulaHelper(string.Format("COLUMN_CORRECT_{0}+COLUMN_VALUE_{0}", comp.m_Id)), false);
                        column.ReadOnly = true;
                        Columns.Add(column);                        
                    } else
                        ;
                });
                //Станция - компонент
                comp_tec = listTECComponent.Find(comp => { return comp.IsTec; });
                //Станция - Значения - ежесуточные
                column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.Name = string.Format(@"COLUMN_ST_DAY_{0}", comp_tec.m_Id);
                column.HeaderText = string.Format(@"{0} значения", comp_tec.m_nameShr);
                listTECComponent.ForEach(comp => { if (comp.IsGtp == true) { strFormula += string.Format(@"{0}CORRECT_VALUE_{1}", string.IsNullOrEmpty(strFormula) == true ? string.Empty : @"+", comp.m_Id); } else { } });
                column.Tag =
                    //findPutParameterGTP(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, comp_tec.m_Id)                    
                    new COLUMN_TAG (new FormulaHelper(strFormula), false);
                    ;
                Columns.Add(column);                
                //Станция - Значения - нарастающие
                column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.Name = string.Format(@"COLUMN_ST_SUM_{0}", comp_tec.m_Id);
                column.HeaderText = string.Format(@"{0} нараст.", comp_tec.m_nameShr);
                column.Tag =
                    //ToolsHelper.Compiler.Compile(ToolsHelper.Parser.Parse(string.Format("INC(COLUMN_ST_DAY_{0})", comp_tec.m_Id)))
                    new COLUMN_TAG (new FormulaHelper(string.Format("SUMM({0})", string.Format(@"COLUMN_ST_DAY_{0}", comp_tec.m_Id))), true)
                    ;
                column.ReadOnly = true;
                Columns.Add(column);
                //Станция - План - ежесуточный
                column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.Name = string.Format(@"COLUMN_PLAN_DAY_{0}", comp_tec.m_Id);
                column.HeaderText = string.Format(@"План сутки");
                column.Tag = new COLUMN_TAG (findPutParameterGTP(HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, comp_tec.m_Id), false);
                column.ReadOnly = true;
                column.Visible = false;
                Columns.Add(column);
                //Станция - План - ежесуточный - накапливаемый
                column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.Name = string.Format(@"COLUMN_PLAN_SUM_{0}", comp_tec.m_Id);
                column.HeaderText = string.Format(@"План нараст.");
                column.Tag = new COLUMN_TAG (new FormulaHelper(string.Format("SUMM({0})", string.Format(@"COLUMN_PLAN_DAY_{0}", comp_tec.m_Id))), false);
                column.ReadOnly = true;
                Columns.Add(column);
                //Станция - План - отклонение
                column = new DataGridViewTextBoxColumn();
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.Name = string.Format(@"COLUMN_PLAN_DEV_{0}", comp_tec.m_Id);
                column.HeaderText = string.Format(@"План отклон.");
                column.Tag = new COLUMN_TAG (new FormulaHelper(string.Format("COLUMN_PLAN_SUM_{0}-COLUMN_ST_SUM_{0}", comp_tec.m_Id)), true);
                column.ReadOnly = true;
                Columns.Add(column);                
            }
            /// <summary>
            /// Словарь с перечнем компонентов станции
            ///  , тлько те компоненты, с кот. связаны параметры алгоритма расчета 2-го порядка
            /// </summary>
            private Dictionary<int, TepCommon.HandlerDbTaskCalculate.TECComponent> m_dictTECComponent;
            /// <summary>
            /// Добавить параметр алгоритма расчета 2-го порядка
            /// </summary>
            /// <param name="putPar">Параметр алгоритма расчета 2-го порядка (связаннй с компонентом станции)</param>
            public override void AddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putPar)
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
            /// Признак, указывающий принажлежит ли значение строке
            ///  иными словами: отображать ли значение в этой строке
            /// </summary>
            /// <param name="r">Строка (проверяемая) для отображения значения</param>
            /// <param name="value">Значение для отображения в строке</param>
            /// <returns>Признак - результат проверки условия (Истина - отображать/принадлежит)</returns>
            protected override bool isRowToShowValues(DataGridViewRow r, TepCommon.HandlerDbTaskCalculate.VALUE value)
            {
                return (r.Tag is DateTime) ? value.stamp_value.Equals(((DateTime)(r.Tag))) == true : false;
            }
        }
    }
}
