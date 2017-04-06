using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HClassLibrary;

namespace TepCommon
{
    partial class HPanelTepCommon
    {
        protected abstract class DataGridViewValues : DataGridView
        {
            public enum ModeData { NALG, DATETIME }

            public ModeData _modeData;

            public struct DateTimeStamp
            {
                public DateTime Start;

                public TimeSpan Increment;
            }

            public DataGridViewValues(ModeData modeData)
                : base()
            {
                _modeData = modeData;

                InitializeComponents();
            }

            private void InitializeComponents()
            {
                Dock = DockStyle.Fill;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
                // 
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
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
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders | DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                ////Ширина столбцов под видимую область
                //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            /// <summary>
            /// Построить структуру представления (столбцы, строки)
            /// </summary>
            public abstract void AddColumns(List<HandlerDbTaskCalculate.NALG_PARAMETER> listNAlgParameter, List<HandlerDbTaskCalculate.PUT_PARAMETER>listPutParameter);

            protected class DictNAlgProperty : Dictionary <int, NALG_PROPERTY>
            {
                public void SetEnabled(int id_alg, int id_comp, bool value)
                {
                    this[id_alg].m_dictPutParameters[id_comp].SetEnabled(value);
                }
            }

            //public class DictPutParameter : Dictionary<int, HandlerDbTaskCalculate.PUT_PARAMETER>
            //{
            //    public void SetEnabled(int id_comp, bool value)
            //    {
            //        this[id_comp].SetEnabled(value); 
            //    }
            //}
            /// <summary>
            /// Структура для описания добавляемых строк
            /// </summary>
            public class NALG_PROPERTY : HandlerDbTaskCalculate.NALG_PARAMETER
            {
                public NALG_PROPERTY(HandlerDbTaskCalculate.NALG_PARAMETER nAlgPar)
                    : base(nAlgPar)
                {
                }

                public
                    //DictPutParameter
                    Dictionary<int, HandlerDbTaskCalculate.PUT_PARAMETER>
                        m_dictPutParameters;

                public string FormatRound { get { return string.Format(@"F{0}", m_vsRound); } }
            }
            /// <summary>
            /// Структура с дополнительными свойствами ячейки отображения
            /// </summary>
            public struct CELL_PROPERTY //: DataGridViewCell
            {
                /// <summary>
                /// Количество свойств структуры
                /// </summary>
                private static int CNT_SET = 2;
                /// <summary>
                /// Счетчик кол-ва изменений свойств, для проверки признака установки всех значений
                /// </summary>
                private int _cntSet;
                /// <summary>
                /// Метод для увеличения счетчика кол-ва изменений
                /// </summary>
                private void counter() { if (_cntSet < CNT_SET) _cntSet++; else/*дальнейшее увеличение необязательно*/; }

                private double _value;
                /// <summary>
                /// Значение в ячейке
                /// </summary>
                public double m_Value { get { return _value; } set { _value = value; counter(); } }

                public void SetValue(double value) { m_Value = value; }

                private TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE _iQuality;
                /// <summary>
                /// Признак качества значения в ячейке
                /// </summary>
                public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality { get { return _iQuality; } set { _iQuality = value; counter(); } }

                public CELL_PROPERTY(float value, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                {
                    _cntSet = CNT_SET; //!!! по количеству свойств

                    _value = value;
                    _iQuality = iQuality;
                }

                public bool IsNaN { get { return _cntSet < CNT_SET; } }
            }            

            protected DictNAlgProperty m_dictNAlgProperties;

            #region RATIO
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
            /// <summary>
            /// Словарь со значенями коэффициентов при масштабировании физических величин (микро, милли, кило, Мега)
            /// </summary>
            private Dictionary<int, RATIO> m_dictRatio;
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

            private DateTimeStamp _datetimeRange;
            /// <summary>
            /// Метка времени отображающихся значений, интерперетируется в ~ от типа DataGridView
            /// , ??? зачем public (временно, из-за странной реализации DataGridViewVedomostBl)
            /// </summary>
            public DateTimeStamp DatetimeStamp
            {
                get { return _datetimeRange; }

                set
                {
                    if (_datetimeRange.Equals(value) == false) {
                        _datetimeRange = value;

                        if (_modeData == ModeData.DATETIME) {
                            ClearRows();
                        } else
                            ;
                    } else
                        ;
                }
            }
            /// <summary>
            /// кол-во дней в текущем месяце
            /// </summary>
            /// <returns>кол-во дней</returns>
            public int DaysInMonth
            {
                get {
                    return (DatetimeStamp.Start.Equals(DateTime.MinValue) == false)
                        ? DateTime.DaysInMonth(DatetimeStamp.Start.Year, DatetimeStamp.Start.Month)
                            : -1;
                }
            }

            private int _prevIdNAlgAsRatio;
            private int _prevVsRatioValue;
            private int _prevPrjRatioValue;

            public double GetValueCellAsRatio(int idNAlg, int idPut, double value)
            {
                double dblRes = -1F;

                int vsRatioValue = -1
                    , prjRatioValue = -1;

                if (!(_prevIdNAlgAsRatio == idNAlg)) {
                    // Множитель для значения - для отображения
                    vsRatioValue =
                        m_dictRatio[m_dictNAlgProperties[idNAlg].m_vsRatio].m_value
                        ;
                    // Множитель для значения - исходный в БД
                    prjRatioValue =
                        //m_dictRatio[m_dictPropertiesRows[idAlg].m_ratio].m_value
                        m_dictRatio[m_dictNAlgProperties[idNAlg].m_dictPutParameters[idPut].m_prjRatio].m_value
                        ;

                    _prevVsRatioValue = vsRatioValue;
                    _prevPrjRatioValue = prjRatioValue;
                } else {
                    vsRatioValue = _prevVsRatioValue;
                    prjRatioValue = _prevPrjRatioValue;
                }
                // проверить требуется ли преобразование
                if (!(prjRatioValue == vsRatioValue))
                // домножить значение на коэффициент
                    dblRes = value * Math.Pow(10F, vsRatioValue - prjRatioValue);
                else
                //отображать без изменений
                    dblRes = value;

                return dblRes;
            }
            #endregion

            public virtual void AddNAlgParameter(HandlerDbTaskCalculate.NALG_PARAMETER nAlg)
            {
                if (m_dictNAlgProperties == null)
                    m_dictNAlgProperties = new DictNAlgProperty();
                else
                    ;

                if (m_dictNAlgProperties.ContainsKey(nAlg.m_Id) == false)
                    m_dictNAlgProperties.Add(nAlg.m_Id, new NALG_PROPERTY(nAlg));
                else
                    ;
            }

            public virtual void AddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                if (m_dictNAlgProperties.ContainsKey(putPar.m_idNAlg) == true) {
                    if (m_dictNAlgProperties[putPar.m_idNAlg].m_dictPutParameters == null)
                        m_dictNAlgProperties[putPar.m_idNAlg].m_dictPutParameters = new Dictionary<int, HandlerDbTaskCalculate.PUT_PARAMETER>();
                    else
                        ;

                    if (m_dictNAlgProperties[putPar.m_idNAlg].m_dictPutParameters.ContainsKey(putPar.m_Id) == false)
                        m_dictNAlgProperties[putPar.m_idNAlg].m_dictPutParameters.Add(putPar.m_Id, putPar);
                    else
                        ;
                } else
                    ;
            }
            /// <summary>
            /// Добавить строки в количестве 'cnt', очередная строка имеет
            /// </summary>
            /// <param name="dtStart">Дата для 1-ой строки предсавления</param>
            /// <param name="tsAdding">Смещение между метками времени строк</param>
            /// <param name="cnt">Количество строк в представлении (дней в месяце)</param>
            protected virtual void addRows()
            {
                DateTime dtCurrent;
                TimeSpan tsIncrement = TimeSpan.Zero;
                int cnt = -1;

                dtCurrent = DatetimeStamp.Start;
                tsIncrement = DatetimeStamp.Increment;
                cnt = DaysInMonth;

                if (_modeData == ModeData.DATETIME)
                    if (RowCount == 0)
                        if (dtCurrent.Equals(DateTime.MinValue) == false)
                            for (int i = 0; i < cnt + 1; i++, dtCurrent += tsIncrement)
                                addRow(dtCurrent, !(i < cnt));
                        else
                            throw new Exception(@"HPanelTepCommon.DataGridViewValues::addRows () - не установлена метка даты для DataGridView в целом...");
                    else
                        Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::addRows () - строки уже добавлены..."), Logging.INDEX_MESSAGE.NOT_SET);
                else
                    throw new Exception(string.Format(@"HPanelTepCommon.DataGridViewValues::addRows () - для объекта с типом {0} вызов метода не допускается...", _modeData));
            }

            //public virtual void AddRows()
            //{
            //    addRows();
            //}

            public virtual void AddRows(DateTimeStamp dtStamp)
            {
                DatetimeStamp = dtStamp;

                addRows();
            }

            protected virtual void addRow(DateTime dtRow, bool bEnded)
            {
                int indxRow = -1;

                indxRow = Rows.Add();

                if (bEnded == true) {
                    // окончание периода - месяц
                    Rows[indxRow].Tag = -1;
                    Rows[indxRow].HeaderCell.Value = @"ИТОГО";
                } else {
                    // обычные сутки
                    Rows[indxRow].Tag = dtRow;
                    if (RowHeadersVisible == true)
                        Rows[indxRow].HeaderCell.Value = dtRow.ToShortDateString();
                    else
                        Rows[indxRow].Cells[0].Value = dtRow.ToShortDateString();
                }
            }

            public virtual void Clear()
            {
                ClearValues();

                ClearRows();

                Columns.Clear();
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
