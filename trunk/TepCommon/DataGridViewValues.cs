using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TepCommon
{
    partial class HPanelTepCommon
    {
        protected class DataGridViewValues : DataGridView
        {
            public enum ModeData { NALG, DATETIME }

            public ModeData _modeData;

            public DataGridViewValues(ModeData modeData) : base() { _modeData = modeData; }

            protected class DictNAlgProperty : Dictionary <int, NALG_PROPERTY>
            {
            }

            /// <summary>
            /// Структура для описания добавляемых строк
            /// </summary>
            public class NALG_PROPERTY : NALG_PARAMETER
            {
                public NALG_PROPERTY(NALG_PARAMETER nAlgPar)
                    : base(nAlgPar.m_idNAlg
                          , -1 //???
                          , -1 //???
                          , nAlgPar.m_strNameShr
                          , nAlgPar.m_strDescription
                          , nAlgPar.m_strMeausure
                          , nAlgPar.m_strSymbol
                          , nAlgPar.m_bEnabled, nAlgPar.m_bVisibled
                          , nAlgPar.m_iRatio, nAlgPar.m_iRound
                    )
                {
                }
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { CALC_DENY, IS_NAN }
                    /// <summary>
                    /// Признак запрета расчета
                    /// </summary>
                    public bool m_bCalcDeny;
                    /// <summary>
                    /// Идентификатор элемента расчета, со сложным ключом (сопоставляемый с): идентификатор параметра в алгоритме расчета, идентификатор орбрудования - признак отсутствия значения
                    /// </summary>
                    public int m_IdPut;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idPut, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality, bool bCalcDeny)
                    {
                        m_IdPut = idPut;
                        m_iQuality = iQuality;
                        m_bCalcDeny = bCalcDeny;
                    }

                    public bool IsNaN { get { return m_IdPut < 0; } }
                }

                ///// <summary>
                ///// Признак отображения строки
                ///// </summary>
                //public bool m_bVisibled;

                public HDataGridViewCell[] m_arPropertiesCells;

                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];

                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT, false);
                }

                public string FormatRound { get { return string.Format(@"F{0}", m_iRound); } }
            }
            /// <summary>
            /// Структура для хранения значений одной из записей в таблице со описанием коэфициентов  при масштабировании физических величин
            /// </summary>
            protected struct RATIO
            {
                public int m_id;

                public int m_value;

                public string m_nameRU
                    , m_nameEN
                    , m_strDesc;
            }

            protected Dictionary<int, NALG_PROPERTY> m_dictNAlgProperties;
            /// <summary>
            /// Словарь со значенями коэффициентов при масштабировании физических величин (микро, милли, кило, Мега)
            /// </summary>
            protected Dictionary<int, RATIO> m_dictRatio;

            /// <summary>
            /// Установить значения для яччеек представления со значениями коэффициентов при масштабировании физических величин
            /// </summary>
            /// <param name="tblRatio">Таблица БД со описанием коэфициентов</param>
            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO() {
                        m_id = (int)r[@"ID"]
                        , m_value = (int)r[@"VALUE"]
                        , m_nameRU = (string)r[@"NAME_RU"]
                        , m_nameEN = (string)r[@"NAME_RU"]
                        , m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }

            private void addNAlg(NALG_PROPERTY nAlg)
            {
                if (m_dictNAlgProperties == null)
                    m_dictNAlgProperties = new Dictionary<int, NALG_PROPERTY>();
                else
                    ;

                if (m_dictNAlgProperties.ContainsKey(nAlg.m_idNAlg) == false)
                    m_dictNAlgProperties.Add(nAlg.m_idNAlg, nAlg);
                else
                    ;
            }

            public void AddNAlg(NALG_PROPERTY nAlg)
            {
                addNAlg(nAlg);
            }

            /// <summary>
            /// Добавить строку в таблицу (режим NALG)
            /// </summary>
            public virtual int AddRow(NALG_PROPERTY nAlg)
            {
                int iRes = -1;

                addNAlg(nAlg);

                // создать строку, добавить строку
                iRes = Rows.Add(new DataGridViewRow());
                //// установить значения в ячейках для служебной информации
                //Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                //Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
                Rows[iRes].Tag = nAlg.m_idNAlg;
                // инициализировать значения в служебных ячейках
                m_dictNAlgProperties[nAlg.m_idNAlg].InitCells(Columns.Count);

                return iRes;
            }
            /// <summary>
            /// Добавить строку в таблицу (режим DATETIME)
            /// </summary>
            public virtual int AddRow(DateTime dtRow)
            {
                int iRes = -1;

                // добавить строку
                iRes = Rows.Add(new DataGridViewRow());

                Rows[iRes].Tag = dtRow;

                return iRes;
            }

            /// <summary>
            /// Удалить все строки представления
            /// </summary>
            public virtual void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
                else
                    ;
            }

            /// <summary>
            /// Очитсить значения в ячейках представления
            /// </summary>
            public virtual void ClearValues()
            {
                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        c.Value = string.Empty;
            }
        }
    }
}
