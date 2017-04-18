using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
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
            /// <summary>
            /// Перечисления для указания режима отображения значений
            /// </summary>
            public enum ModeData {
                /// <summary>
                /// Столбцы - компоненты оборудования
                ///  , строки - параметры алгоритма расчета 1-го уровня (НЕ связаны с компонентами оборудования)
                ///  , метка времени для всех значений одинаковы
                /// </summary>
                NALG
                /// <summary>
                /// Столбцы - параметры алгоритма расчета 2-го уровня (СВЯЗаны с компонентами оборудования)
                ///  , строки - в соответствии с меткой времени
                ///  , метка времени для значений в одной строке одинакова
                /// </summary>
                , DATETIME }
            /// <summary>
            /// Режим отображения значений
            /// </summary>
            public ModeData _modeData;
            /// <summary>
            /// Метка времени начальная (для отображения значений в режиме 'ModeData.DATETIME')
            /// </summary>
            public struct DateTimeStamp
            {
                /// <summary>
                /// Значение начальной метки даты/времени
                /// </summary>
                public DateTime Start;
                /// <summary>
                /// Приращение для начальной метки даты/времени (от строки - к строке)
                ///  , зависит от периода расчета (например, для месяца - сутки, для суток - час)
                /// </summary>
                public int GetIncrementToatalMinutes(int month)
                {
                    return _increment < TimeSpan.MaxValue // признак отображения: менее, чем год по-месячно (кол-во минут известно; DatetimeStamp.Increment указано)
                        ? (int)_increment.TotalMinutes
                            : _increment == TimeSpan.MaxValue // признак отображения: год по-месячно (кол-во минут неизвестно; ; DatetimeStamp.Increment не указано, но установлено в специальное значение)
                                ? DateTime.DaysInMonth(Start.AddMonths(month).Year, month) * 24 * 60
                                    : -1;
                }
                /// <summary>
                /// Интервал времени между метками(Tag) 2-х соседних строк
                ///  , может быть вычисляемой, если например отображаются данные в размере год - месяц (в месяце не одинаковое кол-во суток)
                /// </summary>
                private TimeSpan _increment;
                /// <summary>
                /// Интервал времени между метками(Tag) 2-х соседних строк
                /// </summary>
                public TimeSpan Increment { get { return _increment; } set { if (_increment.Equals(value) == false) { _increment = value; } else; } }
                /// <summary>
                /// Режим отображения меток времени - просто копия аналогичного режима родительской панели
                /// </summary>
                public HandlerDbTaskCalculate.MODE_DATA_DATETIME ModeDataDatetime;
            }

            public event Action<HandlerDbTaskCalculate.CHANGE_VALUE> EventCellValueChanged;
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="modeData">Режим отображения значений</param>
            public DataGridViewValues(ModeData modeData)
                : base()
            {
                _modeData = modeData;

                InitializeComponents();

                //CellParsing += onCellParsing;
                CellValueChanged += onCellValueChanged;
            }
            /// <summary>
            /// Перечисления для индексирования массива со значениями цветов для фона ячеек
            /// </summary>
            protected enum INDEX_COLOR : uint
            {
                EMPTY
                /// <summary>
                /// Индексы: изменена_но_не_сохранена, значение_по_умолчанию, отключена_только_для_чтения, не_может_быть_отображена, не_достоверна, нет_записей_в_БД, превышена_уставка
                /// </summary>               
                , VARIABLE, DEFAULT, DISABLED, NAN, PARTIAL, NOT_REC, LIMIT
                , USER
                    , COUNT
            }
            /// <summary>
            /// Массив со значениями цветов для фона ячеек
            /// </summary>
            protected static Color[] s_arCellColors = new Color[(int)INDEX_COLOR.COUNT] { Color.Gray //EMPTY
                , Color.White //VARIABLE
                , Color.Yellow //DEFAULT
                , Color.LightGray //CALC_DENY
                , Color.White //NAN
                , Color.BlueViolet //PARTIAL
                , Color.Sienna //NOT_REC
                , Color.Red //LIMIT
                , Color.White //USER
            };

            private void onCellParsing(object sender, DataGridViewCellParsingEventArgs ev)
            {
                ev.ParsingApplied = true;

                EventCellValueChanged?.Invoke(new HandlerDbTaskCalculate.CHANGE_VALUE() {
                    m_keyValues = new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN, TypeState = HandlerDbValues.STATE_VALUE.EDIT }
                    //, m_taskCalculateType = HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN //??? повтор                    
                    , value = new HandlerDbTaskCalculate.VALUE() { m_iQuality = TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC }
                    , stamp_action = DateTime.MinValue
                });
            }

            private void onCellValueChanged(object sender, DataGridViewCellEventArgs e)
            {
                HandlerDbTaskCalculate.PUT_PARAMETER putPar;
                int idNAlg = -1
                    , idPut = -1;
                float fltValue = -1F;
                CELL_PROPERTY cellProperty;
                DateTime stamp_value = DateTime.MinValue;

                if ((!(e.ColumnIndex < 0))
                    && (!(e.RowIndex < 0))) {
                    if (!(Rows[e.RowIndex].Cells[e.ColumnIndex].Tag == null))
                        cellProperty = (CELL_PROPERTY)Rows[e.RowIndex].Cells[e.ColumnIndex].Tag;
                    else
                        cellProperty = new CELL_PROPERTY() { m_iQuality = HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC, m_Value = float.MinValue };

                    if ((!(cellProperty.IsEmpty == true))
                        && (float.TryParse(Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out fltValue) == true)) {
                        putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)Columns[e.ColumnIndex].Tag;
                        idNAlg = putPar.m_idNAlg;
                        idPut = putPar.m_Id;
                        cellProperty.SetValue((float)GetValueDbAsRatio(idNAlg, idPut, fltValue));
                        cellProperty.SetQuality(HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER); // значение изменено пользователем
                        if (_modeData == ModeData.DATETIME) {
                            stamp_value = (DateTime)Rows[e.RowIndex].Tag;
                        } else if (_modeData == ModeData.NALG)
                            //??? метка даты/времени - константа 
                            stamp_value = (DateTime)Tag;
                        else
                            ;

                        EventCellValueChanged?.Invoke(new HandlerDbTaskCalculate.CHANGE_VALUE() {
                            m_keyValues = new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = m_dictNAlgProperties[idNAlg].m_type, TypeState = HandlerDbValues.STATE_VALUE.EDIT }
                            , value = new HandlerDbTaskCalculate.VALUE() { m_IdPut = idPut, m_iQuality = cellProperty.m_iQuality, value = cellProperty.m_Value, stamp_value = stamp_value }
                        });
                    } else
                        Logging.Logg().Error(string.Format(@"DataGridViewValues::onCellValueChanged ({0}) - не удалось преобразовать в значение..."
                                , Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString())
                            , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    ;
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

                private float _value;
                /// <summary>
                /// Значение в ячейке
                /// </summary>
                public float m_Value { get { return _value; } set { _value = value; counter(); } }

                public void SetValue(float value) { m_Value = value; }

                private TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE _iQuality;
                /// <summary>
                /// Признак качества значения в ячейке
                /// </summary>
                public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality { get { return _iQuality; } set { _iQuality = value; counter(); } }

                public void SetQuality(TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE quality) { m_iQuality = quality; }

                public CELL_PROPERTY(float value, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                {
                    _cntSet = CNT_SET; //!!! по количеству свойств

                    _value = value;
                    _iQuality = iQuality;
                }

                public bool IsEmpty { get { return _cntSet < CNT_SET; } }
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
            /// Количество дней в текущем(установленном текущим для объекта) месяце
            /// </summary>
            /// <returns>Количество дней</returns>
            public int MaxRowCount
            {
                get {
                    int iRes = -1;

                    if (_modeData == ModeData.DATETIME)
                        iRes = (DatetimeStamp.Start.Equals(DateTime.MinValue) == false)
                            ? DatetimeStamp.Increment.Equals(TimeSpan.MaxValue) == false
                                ? DatetimeStamp.Increment < TimeSpan.FromDays(1)
                                    ? 24 // сутки - час
                                        : DateTime.DaysInMonth(DatetimeStamp.Start.Year, DatetimeStamp.Start.Month) // месяц - сутки
                                            : 12 // год - месяц
                                                : -1;
                    else
                        ;

                    return iRes;
                }
            }
            /// <summary>
            /// Идентификатор параметра алгоритма расчета 1-го уровня
            ///  , использовавшийся ранее для определения множителя и точности при округлении для отображения
            /// </summary>
            private int _prevIdNAlgAsRatio;
            /// <summary>
            /// Идентификатор для определения множителя при отображении (настройка отображения)
            /// </summary>
            private int _prevVsRatioValue;
            /// <summary>
            /// Идентификатор для определения множителя при отображении (проект, исходный множитель из БД источника)
            /// </summary>
            private int _prevPrjRatioValue;

            public float get_value_as_ratio(int idNAlg, int idPut, float value, int iReverse)
            {
                float fltRes = -1F;

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
                    fltRes = value * (float)Math.Pow(10F, -1 * iReverse * vsRatioValue + iReverse * prjRatioValue);
                else
                    //отображать без изменений
                    fltRes = value;

                return fltRes;
            }
            /// <summary>
            /// Возвратить значение в соответствии 
            /// </summary>
            /// <param name="idNAlg">Идентификатор парметра в алгоритме расчета 1-го уровня</param>
            /// <param name="idPut">Идентификатор парметра в алгоритме расчета 2-го уровня</param>
            /// <param name="value">Значение, которое требуется преобразовать (применить множитель)</param>
            /// <returns></returns>
            public float GetValueCellAsRatio(int idNAlg, int idPut, float value)
            {
                return get_value_as_ratio(idNAlg, idPut, value, -1);
            }

            public float GetValueDbAsRatio(int idNAlg, int idPut, float value)
            {
                return get_value_as_ratio(idNAlg, idPut, value, 1);
            }
            #endregion
                /// <summary>
                /// Добавить объект с информацией о параметре в алгоритме расчета 1-го уровня
                /// </summary>
                /// <param name="nAlg">Объект с информацией о параметре в алгоритме расчета 1-го уровня</param>
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
            /// <summary>
            /// Добавить объект с информацией о параметре в алгоритме расчета 2-го уровня
            /// </summary>
            /// <param name="putPar">Объект с информацией о параметре в алгоритме расчета 2-го уровня</param>
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
            /// Добавить строки в количестве 'cnt', очередная строка имеет смещение метки даты/времени, указанное во 2-ом аргументе
            /// </summary>
            /// <param name="dtStart">Дата для 1-ой строки представления</param>
            /// <param name="tsAdding">Смещение между метками времени строк</param>
            /// <param name="cnt">Количество строк в представлении (дней в месяце)</param>
            protected virtual void addRows()
            {
                DateTime dtCurrent;
                //TimeSpan tsIncrement = TimeSpan.Zero; // только для вариант №1
                int cnt = -1;

                dtCurrent = DatetimeStamp.Start;
                //tsIncrement = DatetimeStamp.Increment;
                cnt = MaxRowCount;

                if (_modeData == ModeData.DATETIME)
                    if (RowCount == 0)
                        if (dtCurrent.Equals(DateTime.MinValue) == false)
                            //// вариант №1
                            //for (int i = 0; i < cnt + 1; i++, dtCurrent += tsIncrement)
                            //    addRow(dtCurrent, !(i < cnt));
                            // вариант №2
                            for (int i = 0; i < cnt + 1; i++)
                                addRow((RowCount + 1) == cnt + 1);
                        else
                            throw new Exception(@"HPanelTepCommon.DataGridViewValues::addRows () - не установлена метка даты для DataGridView в целом...");
                    else
                        Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::addRows () - строки уже добавлены..."), Logging.INDEX_MESSAGE.NOT_SET);
                else
                    throw new Exception(string.Format(@"HPanelTepCommon.DataGridViewValues::addRows () - для объекта с типом {0} вызов метода не допускается...", _modeData));
            }
            /// <summary>
            /// Добавить строки, назначить начальную метку даты/времени
            /// </summary>
            /// <param name="dtStamp">Метка даты/времени начальная</param>
            public virtual void AddRows(DateTimeStamp dtStamp)
            {
                DatetimeStamp = dtStamp;

                addRows();
            }
            /// <summary>
            /// Добавить одну строку
            /// </summary>
            /// <param name="dtRow">Метка даты/времени для строки</param>
            /// <param name="bEnded">Признак итоговой строки</param>
            protected virtual void addRow(DateTime dtRow, bool bEnded)
            {
                int indxRow = -1;

                if (_modeData == ModeData.DATETIME) {
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
                } else
                    throw new Exception(string.Format(@"DataGridViewValues::addRow () - нельзя добавить строку: элемент не в режиме 'ModeData.DATETIME'..."));
            }

            protected virtual void addRow(bool bEnded)
            {
                DateTime dtRow = DateTime.MinValue;
                int incrementTotalMinutes = -1;

                if (bEnded == false) {
                    incrementTotalMinutes = DatetimeStamp.GetIncrementToatalMinutes(RowCount + 1);

                    dtRow = ((RowCount == 0)
                        ? (DatetimeStamp.ModeDataDatetime == HandlerDbTaskCalculate.MODE_DATA_DATETIME.Begined)
                            ? DatetimeStamp.Start
                                : DatetimeStamp.Start + TimeSpan.FromMinutes(incrementTotalMinutes) // HandlerDbTaskCalculate.MODE_DATA_DATETIME.Ended
                                    : (DateTime)Rows[RowCount - 1].Tag);

                    if (RowCount > 0) {
                        dtRow += TimeSpan.FromMinutes(incrementTotalMinutes);
                    } else
                        ;
                } else
                    ;

                addRow(dtRow, bEnded);
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
            /// Очистить значения в ячейках представления
            /// </summary>
            public virtual void ClearValues()
            {
                CellValueChanged -= onCellValueChanged;

                bool bCellClear = false;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells) {
                        bCellClear = (Columns[c.ColumnIndex].Tag == null) // установлены ли для столбца свойства
                            ? false // свойства не установлены - очищать ячейку не требуется
                                : Columns[c.ColumnIndex].Tag is HandlerDbTaskCalculate.TECComponent // свойства установлены - это компонент?
                                    ? ((HandlerDbTaskCalculate.TECComponent)Columns[c.ColumnIndex].Tag).m_Id > 0 // свойсто столбца - компонент - очищать, если это реальный, а не псевдо-компонент
                                        : Columns[c.ColumnIndex].Tag is HandlerDbTaskCalculate.PUT_PARAMETER // свойсто столбца - не компонент - значит это параметр алгоритма расчета 2-го порядка
                                            ? ((HandlerDbTaskCalculate.PUT_PARAMETER)Columns[c.ColumnIndex].Tag).m_Id > 0 // свойсто столбца - параметр алгоритма расчета 2-го порядка - очищать, если это реальный параметр
                                                : false;

                        if (bCellClear == true) {
                            // только для реальных компонетов - нельзя удалять идентификатор параметра
                            c.Value = string.Empty;
                            c.Style.BackColor = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                        } else
                            ;
                    }
                //??? если установить 'true' - редактирование невозможно
                ReadOnly = false;

                CellValueChanged += onCellValueChanged;
            }

            /// <summary>
            /// Возвратить цвет ячейки по номеру столбца, строки
            /// </summary>
            /// <param name="id_alg">Идентификатор...</param>
            /// <param name="id_comp">Идентификатор...</param>
            /// <param name="clrRes">Результат - цвет ячейки</param>
            /// <returns>Признак возможности размещения значения в ячейке</returns>
            private bool getColorCellToValue(int id_alg, int id_put, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality, out Color clrRes)
            {
                bool bRes = false;

                bRes = !m_dictNAlgProperties[id_alg].m_dictPutParameters[id_put].IsNaN;
                clrRes = s_arCellColors[(int)INDEX_COLOR.EMPTY];

                if (bRes == true)
                    switch (iQuality) { //??? USER, LIMIT
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT: // только для входной таблицы - значение по умолчанию [inval_def]
                            clrRes = s_arCellColors[(int)INDEX_COLOR.DEFAULT];
                            break;
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.PARTIAL: // см. 'getQueryValuesVar' - неполные данные
                            clrRes = s_arCellColors[(int)INDEX_COLOR.PARTIAL];
                            break;
                        case TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC: // см. 'getQueryValuesVar' - нет ни одной записи
                            clrRes = s_arCellColors[(int)INDEX_COLOR.NOT_REC];
                            break;
                        default:
                            clrRes = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                            break;
                    } else
                    clrRes = s_arCellColors[(int)INDEX_COLOR.NAN];

                return bRes;
            }            

            protected abstract bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value);

            /// <summary>
            /// Отобразить значения (!!! не забывать перед отображением значений отменить регистрацию события - изменение значения в ячейке
            ///  , а после отображения снова зарегистрировать !!!)
            /// </summary>
            /// <param name="inValues">Список с входными значениями</param>
            /// <param name="outValues">Список с выходными значениями</param>
            public void ShowValues(IEnumerable<HandlerDbTaskCalculate.VALUE> inValues
                , IEnumerable<HandlerDbTaskCalculate.VALUE> outValues
                , out int err)
            {
                err = 0;

                int idAlg = -1
                   , idPut = -1
                   , iCol = 0;
                TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality = HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC;
                float fltVal = -1F,
                    fltColumnAgregateValue = 0;
                AGREGATE_ACTION columnAction;
                DataGridViewRow row;
                HandlerDbTaskCalculate.PUT_PARAMETER putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();
                IEnumerable<HandlerDbTaskCalculate.VALUE> columnValues = null;
                Color clrCell = Color.Empty;

                // почему "1"? т.к. предполагается, что в наличии минимальный набор: "строка с данными" + "итоговая строка"
                if (RowCount > 1) {
                    CellValueChanged -= onCellValueChanged;

                    foreach (DataGridViewColumn col in Columns) {
                        try {
                            putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag;
                            columnValues = inValues.Where(value => { return value.m_IdPut == putPar.m_Id; });

                            idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_idNAlg;
                            idPut = ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_Id;
                            iCol = Columns.IndexOf(col);
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, @"DataGridViewValuesReaktivka::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }

                        if ((putPar.IdComponent > 0)
                            && (!(columnValues == null))) {
                            fltColumnAgregateValue = 0F;
                            columnAction = Rows[RowCount - 1].Tag.GetType().IsPrimitive == true // есть ли итоговая строка?
                                ? m_dictNAlgProperties[idAlg].m_sAverage // итоговая строка - есть (операция по агрегации известна)
                                    : AGREGATE_ACTION.UNKNOWN; // итоговой строки - нет (операция по агрегации неизвестна и не выполняется)

                            foreach (HandlerDbTaskCalculate.VALUE value in columnValues) {
                                fltVal = value.value;
                                iQuality = value.m_iQuality;

                                if (!(columnAction == AGREGATE_ACTION.UNKNOWN))
                                    fltColumnAgregateValue += fltVal;
                                else
                                    ;

                                row = Rows.Cast<DataGridViewRow>().FirstOrDefault(r => isRowToShowValues(r, value));

                                if (!(row == null)) {
                                    row.Cells[iCol].Tag = new CELL_PROPERTY() { m_Value = fltVal, m_iQuality = (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality };
                                    row.Cells[iCol].ReadOnly = double.IsNaN(fltVal);

                                    if (getColorCellToValue(idAlg, idPut, (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality, out clrCell) == true) {
                                        //// символ (??? один для строки, но назначается много раз по числу столбцов)
                                        //row.Cells[(int)INDEX_SERVICE_COLUMN.SYMBOL].Value = m_dictNAlgProperties[idAlg].m_strSymbol
                                        //    + @",[" + m_dictRatio[m_dictNAlgProperties[idAlg].m_iRatio].m_nameRU + m_dictNAlgProperties[idAlg].m_strMeausure + @"]";

                                        fltVal = GetValueCellAsRatio(idAlg, idPut, fltVal);

                                        // отобразить с количеством знаков в соответствии с настройками
                                        row.Cells[iCol].Value = fltVal.ToString(m_dictNAlgProperties[idAlg].FormatRound, System.Globalization.CultureInfo.InvariantCulture);
                                    } else
                                        ;

                                    row.Cells[iCol].Style.BackColor = clrCell;
                                } else
                                    // не найдена строка для даты в наборе данных для отображения
                                    Logging.Logg().Warning(string.Format(@"DataGridViewValuesReaktivka::ShowValues () - не найдена строка для даты [DATETIME={0}] в наборе данных для отображения..."
                                            , value.stamp_value.Date)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                            }

                            if (!(columnAction == AGREGATE_ACTION.UNKNOWN)) {
                                if (columnAction == AGREGATE_ACTION.SUMMA)
                                    fltColumnAgregateValue = GetValueCellAsRatio(idAlg, idPut, fltColumnAgregateValue);
                                else if (columnAction == AGREGATE_ACTION.AVERAGE)
                                    fltColumnAgregateValue = GetValueCellAsRatio(idAlg, idPut, fltColumnAgregateValue);
                                else
                                    ;

                                Rows[Rows.Count - 1].Cells[iCol].Value = fltColumnAgregateValue.ToString(m_dictNAlgProperties[idAlg].FormatRound, CultureInfo.InvariantCulture);
                            } else
                                ;
                        } else
                            Logging.Logg().Error(string.Format(@"DataGridViewValuesReaktivka::ShowValues () - не найдено ни одного значения для [ID_PUT={0}] в наборе данных [COUNT={1}] для отображения..."
                                    , ((HandlerDbTaskCalculate.PUT_PARAMETER)col.Tag).m_Id, inValues.Count())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    CellValueChanged += onCellValueChanged;
                } else
                    Logging.Logg().Error(string.Format(@"DataGridViewValuesReaktivka::ShowValues () - нет строк для отображения..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }
}
