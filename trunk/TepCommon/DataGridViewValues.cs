using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ELW.Library.Math;
using ELW.Library.Math.Expressions;
using HClassLibrary;

namespace TepCommon
{
    partial class HPanelTepCommon
    {
        protected abstract class DataGridViewValues : DataGridView
        {
            /// <summary>
            /// Массив - наименования месяцев в году (Ru-Ru)
            /// </summary>
            public static string[] s_NameMonths =
            {
                "Январь", "Февраль", "Март", "Апрель",
                "Май", "Июнь", "Июль", "Август", "Сентябрь",
                "Октябрь", "Ноябрь", "Декабрь"//, "Январь сл. года"
            };
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

            public event Action<HandlerDbTaskCalculate.KEY_VALUES, HandlerDbTaskCalculate.VALUE> EventCellValueChanged;
            /// <summary>
            /// Конструктор - основной (с параметром)
            /// </summary>
            /// <param name="modeData">Режим отображения значений</param>
            public DataGridViewValues(ModeData modeData/*, Action<HandlerDbTaskCalculate.CHANGE_VALUE> handlerEventCellChangeValue*/, Func<int, int, float, int, float> fGetValueAsRatio)
                : base()
            {
                _modeData = modeData;

                //EventCellValueChanged += new Action<HandlerDbTaskCalculate.CHANGE_VALUE> (handlerEventCellChangeValue);
                getValueAsRatio = new Func<int, int, float, int, float> (fGetValueAsRatio);

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
                , Color.LightGray //DISABLED
                , Color.White //NAN
                , Color.BlueViolet //PARTIAL
                , Color.Sienna //NOT_REC
                , Color.Red //LIMIT
                , Color.White //USER
            };

            protected void activateCellValue_onChanged(bool bActivate)
            {
                CellValueChanged -= onCellValueChanged;

                if (bActivate == true)
                    CellValueChanged += onCellValueChanged;
                else
                    ;
            }

            //private void onCellParsing(object sender, DataGridViewCellParsingEventArgs ev)
            //{
            //    ev.ParsingApplied = true;

            //    EventCellValueChanged?.Invoke(new HandlerDbTaskCalculate.CHANGE_VALUE() {
            //        m_keyValues = new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN, TypeState = HandlerDbValues.STATE_VALUE.EDIT }
            //        //, m_taskCalculateType = HandlerDbTaskCalculate.TaskCalculate.TYPE.UNKNOWN //??? повтор                    
            //        , value = new HandlerDbTaskCalculate.VALUE() { m_iQuality = TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC }
            //        , stamp_action = DateTime.MinValue
            //    });
            //}

            private void onCellValueChanged(object sender, DataGridViewCellEventArgs ev)
            {
                int err = -1;

                HandlerDbTaskCalculate.PUT_PARAMETER putPar;
                int idNAlg = -1
                    , idPut = -1;
                float fltValue = -1F;
                CELL_PROPERTY cellProperty;
                bool bRecalculate = false;
                DateTime stamp_value = DateTime.MinValue;

                if ((!(ev.ColumnIndex < 0))
                    && (!(ev.RowIndex < 0))) {
                    // для исключения ошибки при сборке - значения по умолчанию
                    putPar = new HandlerDbTaskCalculate.PUT_PARAMETER();

                    if (!(Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag == null))
                        cellProperty = (CELL_PROPERTY)Rows[ev.RowIndex].Cells[ev.ColumnIndex].Tag;
                    else
                        cellProperty = new CELL_PROPERTY() { m_iQuality = HandlerDbTaskCalculate.ID_QUALITY_VALUE.NOT_REC, m_Value = float.MinValue };

                    if ((!(cellProperty.IsEmpty == true))
                        && (string.IsNullOrEmpty(Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value?.ToString()) == false))
                        if (float.TryParse(Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value?.ToString(), out fltValue) == false) {
                            fltValue = 0F;

                            Logging.Logg().Error(string.Format(@"DataGridViewValues::onCellValueChanged ({0}) - не удалось преобразовать в значение..."
                                    , Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value?.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        } else
                            ;
                    else
                        fltValue = 0F;

                    try {
                        putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)(((COLUMN_TAG)Columns[ev.ColumnIndex].Tag).value);

                        err = 0;
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, string.Format(@"HPanelTepCommon.DataGridViewValues::onCellValueChanged(Column={0}) - возможно, для столбца снят признак 'ReadOnly'...", ev.ColumnIndex), Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (err == 0) {
                        idNAlg = putPar.m_idNAlg;
                        idPut = putPar.m_Id;
                        cellProperty.SetValue((float)GetValueDbAsRatio(idNAlg, idPut, fltValue));
                        cellProperty.SetQuality(HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER); // значение изменено пользователем
                        if (_modeData == ModeData.DATETIME) {
                            stamp_value = (DateTime)Rows[ev.RowIndex].Tag;
                        } else if (_modeData == ModeData.NALG)
                            //??? метка даты/времени - константа 
                            stamp_value = (DateTime)Tag;
                        else
                            ;

                        foreach (DataGridViewColumn col in Columns) {
                            if (!(col.Index == ev.ColumnIndex)) {
                                if (((COLUMN_TAG)col.Tag).Type == TYPE_COLUMN_TAG.FORMULA_HELPER) {
                                    if ((bRecalculate = (((COLUMN_TAG)col.Tag).value as FormulaHelper).IndexColumns.Contains(ev.ColumnIndex)) == true)
                                        break;
                                    else
                                        ;
                                } else {
                                    // идентификатор столбца не является формулой
                                }
                            } else {
                                // столбец является столбцом события
                            }
                        }
                        // принудительно будет вызван метод ShowValues
                        EventCellValueChanged?.Invoke(
                            new HandlerDbTaskCalculate.KEY_VALUES() { TypeCalculate = m_dictNAlgProperties[idNAlg].m_type, TypeState = HandlerDbValues.STATE_VALUE.EDIT }
                            , new HandlerDbTaskCalculate.VALUE() { m_IdPut = idPut, m_iQuality = cellProperty.m_iQuality, value = cellProperty.m_Value, stamp_value = stamp_value }
                            //, stamp_action = DateTime.MinValue
                        );
                    } else
                    // ошибка при определени идентификатора
                        ;
                } else
                // не новый столбец или новая строка (индексы столбца/строки известны)
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
                //Запрет изменения размеров столбцов
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
                //
                ShowCellToolTips = true;

                //RowsAdded += onRowsAdded;
                Columns.CollectionChanged += columns_OnCollectionChanged;
            }

            private void columns_OnCollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
            {
                int idAlg = -1;
                string fmtRoundValue = string.Empty;
                FormulaHelper tagFormaula;
                List<object> tags; // tag-и столбцов, для поиска базового столбца для форматирования значения в тех столбцах, которые не являются входными/выходными параметрами
                HandlerDbTaskCalculate.PUT_PARAMETER tag;
                DataGridViewColumn column;

                if (e.Action == System.ComponentModel.CollectionChangeAction.Add) {
                    //foreach (DataGridViewColumn column in Columns) {
                        column = e.Element as DataGridViewColumn;
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;

                        if (Equals(column.Tag, null) == false) {
                            if (((COLUMN_TAG)column.Tag).Type == TYPE_COLUMN_TAG.PUT_PARAMETER) {
                            // идентификатор столбца - параметр в алгоритме расчета 2-го порядка (с принадлежностью к оборудованию)
                                idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)column.Tag).value).m_idNAlg;

                                fmtRoundValue = m_dictNAlgProperties[idAlg].FormatRound;
                            } else if (((COLUMN_TAG)column.Tag).Type == TYPE_COLUMN_TAG.FORMULA_HELPER) {
                            // идентификатор столбца - формула
                                tagFormaula = ((COLUMN_TAG)column.Tag).value as FormulaHelper;
                                //Подготовить(разобрать) формулу к выполнению расчета
                                tagFormaula.Prepare((from col in Columns.Cast<DataGridViewColumn>() select col.Name));
                        
                                //Поиск 'tag' базового столбца для форматирования значения
                                tag = getBaseColumnTag(tagFormaula.IndexColumns);

                                if (tag.IsNaN == false) {
                                    idAlg = ((HandlerDbTaskCalculate.PUT_PARAMETER)tag).m_idNAlg;

                                    fmtRoundValue = m_dictNAlgProperties[idAlg].FormatRound;
                                } else
                                    fmtRoundValue = NALG_PROPERTY.DefaultFormatValue;                            
                            } else
                                throw new Exception (string.Format(@"HPanelTepCommon.DataGridViewValues::columns_OnCollectionChanged (tag type={0}) - неизвестный тип идентификатора столбца...", column.Tag.GetType().FullName));

                            // установить свойства столбца для отображения
                            column.ValueType = typeof(float);
                            column.DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
                            column.DefaultCellStyle.Format = fmtRoundValue;
                        } else
                            ;
                    //}
                } else
                    ;
            }

            //private void onRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
            //{
            //}

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

                public static string DefaultFormatValue = @"F2";
            }

            public enum TYPE_COLUMN_TAG : short { UNKNOWN = short.MinValue, COMPONENT, PUT_PARAMETER, FORMULA_HELPER }

            public struct COLUMN_TAG
            {
                /// <summary>
                /// Идентификатор столбца
                /// </summary>
                public object value;
                /// <summary>
                /// Тип объекта, являющегося идентификатором столбца
                /// </summary>
                public TYPE_COLUMN_TAG Type;
                /// <summary>
                /// Признак отмены агрегационной функции
                ///  (при наличии таковой, по, например, указанной в проекте единице измерения)
                /// </summary>
                public bool ActionAgregateCancel;
                /// <summary>
                /// Индекс(номер, адрес) столбца в книге MS Excel при экспорте значений столбца
                ///  , отсутствие значения - признак отсутствия необходимости экпорта значений столбца
                /// </summary>
                public int TemplateReportAddress;

                public COLUMN_TAG(object tag, int indexMSExcelReportColumn, bool bActionAgregateCancel)
                {
                    value = tag;

                    if (tag is HandlerDbTaskCalculate.PUT_PARAMETER)
                        Type = TYPE_COLUMN_TAG.PUT_PARAMETER;
                    else if (tag is FormulaHelper)
                        Type = TYPE_COLUMN_TAG.FORMULA_HELPER;
                    else
                        Type = TYPE_COLUMN_TAG.UNKNOWN;

                    TemplateReportAddress = indexMSExcelReportColumn;

                    ActionAgregateCancel = bActionAgregateCancel;
                }
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
            /// Делегат для преобразования значений
            /// </summary>
            public Func<int, int, float, int, float> getValueAsRatio;
            /// <summary>
            /// Возвратить значение в соответствии 
            /// </summary>
            /// <param name="idNAlg">Идентификатор парметра в алгоритме расчета 1-го уровня</param>
            /// <param name="idPut">Идентификатор парметра в алгоритме расчета 2-го уровня</param>
            /// <param name="value">Значение, которое требуется преобразовать (применить множитель)</param>
            /// <returns>Значение для отображения в ячейке</returns>
            public float GetValueCellAsRatio(int idNAlg, int idPut, float value)
            {
                return getValueAsRatio(idNAlg, idPut, value, -1);
            }

            public float GetValueDbAsRatio(int idNAlg, int idPut, float value)
            {
                return getValueAsRatio(idNAlg, idPut, value, 1);
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
                string dtLabelRow = string.Empty;

                if (_modeData == ModeData.DATETIME) {
                    indxRow = Rows.Add();

                    if (bEnded == true) {
                        // окончание периода - месяц
                        Rows[indxRow].Tag = -1;
                        Rows[indxRow].HeaderCell.Value = @"ИТОГО";
                    } else {
                        // обычные сутки
                        Rows[indxRow].Tag = dtRow;
                        dtLabelRow = DatetimeStamp.Increment > TimeSpan.FromDays(1)
                            ? s_NameMonths[dtRow.Month - 1]
                                : dtRow.ToShortDateString();

                        if (RowHeadersVisible == true)
                            Rows[indxRow].HeaderCell.Value = dtLabelRow;
                        else
                            Rows[indxRow].Cells[0].Value = dtLabelRow;
                    }

                    Rows[indxRow].DefaultCellStyle.BackColor = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                } else
                    throw new Exception(string.Format(@"DataGridViewValues::addRow () - нельзя добавить строку: элемент не в режиме 'ModeData.DATETIME'..."));
            }
            /// <summary>
            /// Добавить строку к представлению
            /// </summary>
            /// <param name="bEnded">Признак крайней строки</param>
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
            /// <summary>
            /// Полная очистка представления: значения, строки, столбцы
            /// </summary>
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
                activateCellValue_onChanged(false);

                COLUMN_TAG tag;
                bool bCellClear = false;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells) {
                        // установлены ли для столбца свойства
                        if (Equals(Columns[c.ColumnIndex].Tag, null) == false) {
                            tag = (COLUMN_TAG)Columns[c.ColumnIndex].Tag;

                            bCellClear = tag.Type == TYPE_COLUMN_TAG.COMPONENT // свойства установлены - это компонент?
                                ? ((HandlerDbTaskCalculate.TECComponent)tag.value).m_Id > 0 // свойсто столбца - компонент - очищать, если это реальный, а не псевдо-компонент
                                    : tag.Type == TYPE_COLUMN_TAG.PUT_PARAMETER // свойсто столбца - не компонент - значит это параметр алгоритма расчета 2-го порядка
                                        ? ((HandlerDbTaskCalculate.PUT_PARAMETER)tag.value).m_Id > 0 // свойсто столбца - параметр алгоритма расчета 2-го порядка - очищать, если это реальный параметр
                                            : tag.Type == TYPE_COLUMN_TAG.FORMULA_HELPER //     
                                                ? (tag.value as FormulaHelper).IndexColumns.Count() > 0
                                                    : false;
                        } else
                        // свойства не установлены - очищать ячейку не требуется
                            bCellClear = false;

                        if (bCellClear == true) {
                            // только для реальных компонентов (нельзя удалять идентификатор параметра)
                            //??? в качестве идентификтора столбца может быть не только компонент (но и 'PUT_PARAMETER', 'FORMULA_HELPER')
                            c.Value = string.Empty;
                            c.Style.BackColor = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                        } else
                            ;
                    }
                //??? если установить 'true' - редактирование невозможно
                ReadOnly = false;

                activateCellValue_onChanged(true);
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
            /// <summary>
            /// Признак, указывающий принажлежит ли значение строке
            ///  иными словами: отображать ли значение в этой строке
            /// </summary>
            /// <param name="r">Строка (проверяемая) для отображения значения</param>
            /// <param name="value">Значение для отображения в строке</param>
            /// <returns>Признак - результат проверки условия (Истина - отображать/принадлежит)</returns>
            protected abstract bool isRowToShowValues(DataGridViewRow r, HandlerDbTaskCalculate.VALUE value);
            /// <summary>
            /// Класс для расчета формулы в столбце
            /// </summary>
            protected class FormulaHelper : IDisposable
            {
                /// <summary>
                /// Строка формулы, переданная в качестве аргумента конструктору
                /// </summary>
                private string _formula;
                /// <summary>
                /// Структура для хранений совокупности значений, однозначно идентифицирующих столбец
                ///  , используюющийся при расчете в формуле
                /// </summary>
                private struct KEY
                {
                    /// <summary>
                    /// Индекс столбца
                    /// </summary>
                    public int m_index;
                    /// <summary>
                    /// Наименование(уникальное) столбца
                    /// </summary>
                    public string m_name;
                    /// <summary>
                    /// Наименование 
                    /// </summary>
                    public string m_variable;
                }
                /// <summary>
                /// Список ключей столбцов для поиска/идентификации/сопоставления 
                /// </summary>
                private List<KEY> _listKeyColumns;
                /// <summary>
                /// Перечисление - 
                /// </summary>
                private enum FUNC { SUMM }
                /// <summary>
                /// Перечисление - тип сегмента выражения (переменная, арифметическая операция, скобка)
                /// </summary>
                private enum TYPE_SEGMENT { VAR, OPERATION, BRACE }
                /// <summary>
                /// Перечисление - приоритет выполнения операции (сложение/вычитание - низкий, умножение/деление - средний, возведение в степень/извлечение корня - высокий)
                /// </summary>
                private enum PRORITY_OPERATION { ADDITIVE, MULTI, EXPONENT }
                /// <summary>
                /// Перечисление - тип скобки (открывающая, закрывающая)
                /// </summary>
                private enum TYPE_BRACE { OPENING, CLOSING }
                /// <summary>
                /// Массив - возможные к употреблению в выражении знаки арифмитических операций
                /// </summary>
                private readonly char[] OPERATION = { '+', '-', '*', '/', '^' };
                /// <summary>
                /// Массив - возможные к употреблению в выражении скобки
                /// </summary>
                private readonly char[] BRACE = { '[', ']', '{', '}', '(', ')' };

                CompiledExpression _compiledExpression;
                /// <summary>
                /// Конструктор - основной (с аргументом)
                /// </summary>
                /// <param name="formula">Строка с формулой для расчета столбца</param>
                public FormulaHelper(string formula)
                {
                    _IsFunc = false;

                    this._formula = formula;
                }
                /// <summary>
                /// Подготовить для расчета формулу, сопоставив столбцы ее компонентам
                /// </summary>
                /// <param name="columnNames">Наименования столбцов - компонентов формулы</param>
                /// <returns>Признак результата выполнения метода</returns>
                public int Prepare(IEnumerable<string> columnNames)
                {
                    int iRes = 0; // успех

                    //int iSeg = -1; // индекс сегмента
                    PreparedExpression preparedExpression;
                    string formula = string.Empty;
                    int indxVariable = -1;
                    string variable = string.Empty;

                    _listKeyColumns = new List<KEY>();

                    //List<char> separates = OPERATION.Union(BRACE).ToList();
                    //List<TYPE_SEGMENT> segmentTypes;
                    //List<string> segments;

                    //segments = formula.Split(separates.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    //foreach (string seg in segments) {
                    //}

                    formula = _formula;
                    indxVariable = 0;

                    foreach (string colName in columnNames) {
                        if (formula.Contains(colName) == true) {
                            variable = string.Format(@"c{0}", indxVariable);
                            formula = formula.Replace(colName, variable);

                            _listKeyColumns.Add(new KEY() { m_name = colName, m_index = columnNames.ToList().IndexOf(colName), m_variable = variable });

                            indxVariable ++;
                        } else
                            ;
                    }

                    foreach (string func in Enum.GetNames(typeof(FUNC)).ToList())
                        if (formula.Contains(func) == true) {
                            _IsFunc = true;

                            break;
                        } else
                            ;

                    if (_IsFunc == false) {
                        preparedExpression = ToolsHelper.Parser.Parse(formula);
                        _compiledExpression = ToolsHelper.Compiler.Compile(preparedExpression);
                    } else
                        ;

                    return iRes;
                }
                /// <summary>
                /// Рассчитать значение по формуле для столбца, используя указанные в аргументе значения как компоненты формулы
                /// </summary>
                /// <param name="args">Значения компонентов в формуле</param>
                /// <returns>Значение формулы для столбца</returns>
                public float Calculate(IEnumerable <float>args)
                {
                    float fltRes = -1F;

                    List<ELW.Library.Math.Tools.VariableValue> vars;
                    float arg = -1F;
                    int cntNoValues = -1;

                    vars = new List<ELW.Library.Math.Tools.VariableValue>();
                    cntNoValues = 0;
                    for (int indx = 0; indx < args.Count(); indx++) {
                        arg = args.ElementAt(indx);
                        // подсчитать кол-во аргументов без значения
                        if (arg == float.MinValue) {
                            cntNoValues ++;
                            arg = 0F;
                        } else
                            ;

                        vars.Add(new ELW.Library.Math.Tools.VariableValue(arg, _listKeyColumns[indx].m_variable));
                    }

                    if (cntNoValues < vars.Count)
                        fltRes = (float)ToolsHelper.Calculator.Calculate(_compiledExpression, vars);
                    else
                        fltRes = float.MinValue;

                    return fltRes;
                }
                /// <summary>
                /// Список индексов столбцов, от которых зависит(по формуле) текущий столбец
                /// </summary>
                public IEnumerable<int> IndexColumns { get { return Equals(_listKeyColumns, null) == false ? from key in _listKeyColumns select key.m_index : new List<int>(); } }

                public bool IsNaN { get { return (object.Equals(_listKeyColumns, null) == true) && (_listKeyColumns.Count > 0); } }

                private bool _IsFunc = false;
                /// <summary>
                /// Признак: является ли формула для столбца обычной или функцией (функция из перечисления 'FUNC')
                /// </summary>
                public bool IsFunc { get { return _IsFunc; } }

                #region IDisposable Support
                private bool disposedValue = false; // Для определения избыточных вызовов

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposedValue) {
                        if (disposing) {
                            // TODO: освободить управляемое состояние (управляемые объекты).
                        }

                        // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                        // TODO: задать большим полям значение NULL.

                        disposedValue = true;
                    }
                }

                // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
                // ~FormulaHelper() {
                //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
                //   Dispose(false);
                // }

                // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
                void IDisposable.Dispose()
                {
                    // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
                    Dispose(true);
                    // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
                    // GC.SuppressFinalize(this);
                }
                #endregion
            }
            /// <summary>
            /// Найти идентификаторы-'tag-и'-объекты для столбцов, использующихся в формуле
            ///  (базовые для текущего столбца)
            ///  , - нюанс в том, что базовый столбец может быть сам зависим от других базовых столбцов
            /// </summary>
            /// <param name="indexes">Список индексов базовых столбцов</param>
            /// <returns>Список идентификаторов-'tag-ов'-объектов для столбцов</returns>
            private List<HandlerDbTaskCalculate.PUT_PARAMETER> fFormatColumns (IEnumerable<int> indexes)
            {
                List<HandlerDbTaskCalculate.PUT_PARAMETER> listRes = new List<HandlerDbTaskCalculate.PUT_PARAMETER>();

                COLUMN_TAG tag;

                foreach (int indx in indexes) {
                    tag = (COLUMN_TAG)Columns[indx].Tag;

                    if (tag.Type == TYPE_COLUMN_TAG.PUT_PARAMETER) {
                        listRes.Add((HandlerDbTaskCalculate.PUT_PARAMETER)tag.value);
                    } else if (tag.Type == TYPE_COLUMN_TAG.FORMULA_HELPER)
                        listRes = listRes.Union(fFormatColumns((tag.value as FormulaHelper).IndexColumns)).ToList();
                    else
                    //??? Исключение
                        ;
                }

                return listRes;
            }
            /// <summary>
            /// Возвратить идентификатор('tag') базового столбца
            /// </summary>
            /// <param name="indexes">Список индексов (базовых)столбцов</param>
            /// <returns>Идентификатор базового столбца</returns>
            private HandlerDbTaskCalculate.PUT_PARAMETER getBaseColumnTag(IEnumerable<int> indexes)
            {
                //??? TODO: требуется выбрать из массива такой 'tag' у которого максимальное значение 'm_vsRound'
                return indexes.Count() > 0 ? fFormatColumns(indexes)[0] : new HandlerDbTaskCalculate.PUT_PARAMETER();
            }
            /// <summary>
            /// Возвратить тип агрегационной функции над множеством значений в столбце
            /// </summary>
            /// <param name="id_alg">Идентификатор параметра в аогоритме расчета 1-го порядка</param>
            /// <returns>Тип агрегационной функции над множеством значений в столбце</returns>
            private AGREGATE_ACTION getColumnAction(int id_alg)
            {
                return Rows[RowCount - 1].Tag.GetType().IsPrimitive == true // есть ли итоговая строка?
                    ? m_dictNAlgProperties[id_alg].m_sAverage // итоговая строка - есть (операция по агрегации известна)
                        : AGREGATE_ACTION.UNKNOWN; // итоговой строки - нет (операция по агрегации неизвестна и не выполняется)
            }
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
                FormulaHelper formula;
                List<float> args;

                // почему "1"? т.к. предполагается, что в наличии минимальный набор: "строка с данными" + "итоговая строка"
                if (RowCount > 1) {
                    // отменить обработку события - изменение значения в ячейке представления
                    activateCellValue_onChanged(false);

                    foreach (DataGridViewColumn col in Columns) {
                        iCol = col.Index;
                        fltColumnAgregateValue = 0F;

                        if (!(col.Tag == null))
                            if (((COLUMN_TAG)col.Tag).Type == TYPE_COLUMN_TAG.PUT_PARAMETER) {
                                #region Отображение значений в столбце для обычного параметра
                                try {
                                    putPar = (HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value;
                                    columnValues = inValues.Where(value => { return (value.m_IdPut == putPar.m_Id) && ((value.stamp_value - DateTime.MinValue).TotalDays > 0); });
                                    columnValues = columnValues.Union(outValues.Where(value => { return (value.m_IdPut == putPar.m_Id) && ((value.stamp_value - DateTime.MinValue).TotalDays > 0); }));

                                    idAlg = putPar.m_idNAlg;
                                    idPut = putPar.m_Id;

                                    //fmtRoundValue = m_dictNAlgProperties[idAlg].FormatRound;
                                } catch (Exception e) {
                                    Logging.Logg().Exception(e, @"DataGridViewValues::ShowValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                }

                                if ((putPar.IdComponent > 0)
                                    && (!(columnValues == null))) {
                                    //col.DefaultCellStyle.Format = fmtRoundValue;

                                    columnAction = ((COLUMN_TAG)col.Tag).ActionAgregateCancel == true ? AGREGATE_ACTION.UNKNOWN : getColumnAction(idAlg);

                                    foreach (DataGridViewRow r in Rows) {
                                        if (columnValues.Count() > 0)
                                            // есть значение хотя бы для одной строки
                                            foreach (HandlerDbTaskCalculate.VALUE value in columnValues) {
                                                if (isRowToShowValues(r, value) == true) {
                                                    fltVal = value.value;
                                                    iQuality = value.m_iQuality;

                                                    if (!(columnAction == AGREGATE_ACTION.UNKNOWN))
                                                        fltColumnAgregateValue += fltVal;
                                                    else
                                                        ;

                                                    r.Cells[iCol].Tag = new CELL_PROPERTY() { m_Value = fltVal, m_iQuality = (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality };
                                                    r.Cells[iCol].ReadOnly = Columns[iCol].ReadOnly
                                                        || double.IsNaN(fltVal);

                                                    if (getColorCellToValue(idAlg, idPut, (TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE)iQuality, out clrCell) == false) {
                                                        clrCell = s_arCellColors[(int)INDEX_COLOR.EMPTY];
                                                    } else
                                                        ;

                                                    fltVal = GetValueCellAsRatio(idAlg, idPut, fltVal);

                                                    // отобразить с количеством знаков в соответствии с настройками
                                                    r.Cells[iCol].Value = fltVal;
                                                    r.Cells[iCol].ToolTipText = fltVal.ToString();

                                                    r.Cells[iCol].Style.BackColor = clrCell;
                                                } else
                                                    r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                            } else {
                                            // нет значений ни для одной строки
                                            r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                        }
                                    } // цикл по строкам

                                    if (!(columnAction == AGREGATE_ACTION.UNKNOWN)) {
                                        if (columnAction == AGREGATE_ACTION.SUMMA)
                                            fltColumnAgregateValue = GetValueCellAsRatio(idAlg, idPut, fltColumnAgregateValue);
                                        else if (columnAction == AGREGATE_ACTION.AVERAGE)
                                            fltColumnAgregateValue = GetValueCellAsRatio(idAlg, idPut, fltColumnAgregateValue);
                                        else
                                            ;

                                        Rows[Rows.Count - 1].Cells[iCol].Value =
                                            fltColumnAgregateValue;
                                    } else
                                        ;
                                } else
                                    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - не найдено ни одного значения для [ID_PUT={0}] в наборе данных [COUNT={1}] для отображения..."
                                            , ((HandlerDbTaskCalculate.PUT_PARAMETER)((COLUMN_TAG)col.Tag).value).m_Id, inValues.Count())
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                #endregion
                            } else if (((COLUMN_TAG)col.Tag).Type == TYPE_COLUMN_TAG.FORMULA_HELPER) {
                                formula = (FormulaHelper)((COLUMN_TAG)col.Tag).value;

                                if (formula.IndexColumns.Count() > 0) {
                                    idAlg = getBaseColumnTag(formula.IndexColumns).m_idNAlg;

                                    columnAction = ((COLUMN_TAG)col.Tag).ActionAgregateCancel == true ? AGREGATE_ACTION.UNKNOWN : getColumnAction(idAlg);

                                    foreach (DataGridViewRow r in Rows) {
                                        fltVal = float.MinValue;

                                        try {
                                            if (r.Index < (Rows.Count - 1)) {
                                                // отобразить значения для обычных(не крайних) строк
                                                args = new List<float>();
                                                // получить значения из яччеек в столбцах, являющихся аргументами формулы
                                                foreach (int indxCol in formula.IndexColumns) {
                                                    fltVal = (Equals(r.Cells[indxCol].Value, null) == false)
                                                        ? r.Cells[indxCol].Value.GetType().Equals(typeof(float)) == true
                                                            ? (float)r.Cells[indxCol].Value
                                                                : r.Cells[indxCol].Value.GetType().Equals(typeof(string)) == true
                                                                    ? float.MinValue // значение в ячейке - строка //float.Parse((string)r.Cells[indxCol].Value, System.Globalization.CultureInfo.InvariantCulture)
                                                                        : float.MinValue // неизвестный тип значения (не 'float', не 'string')
                                                                            : float.MinValue; // признак отсутствия значения в ячейке
                                                    // добавить значение - аргумент
                                                    args.Add(fltVal);
                                                }

                                                if (formula.IsFunc == false)
                                                    // не функция, а обычная формула с арифметическими действиями со значениями в столбцах строки
                                                    fltVal = formula.Calculate(args);
                                                else {
                                                    // рассматривается функция - пока известна толко одна: SUMM
                                                    // и только с одним аргументом (значение одного из столбцов)
                                                    if (r.Index == 0)
                                                        // предыдущей строки нет
                                                        fltVal = 0F;
                                                    else
                                                    // предыдущая строка в наличии - учесть ее значение
                                                        // значение предыдущей строки
                                                        fltVal = (Equals(Rows[r.Index - 1].Cells[col.Index].Value, null) == false)
                                                            ? (float)Rows[r.Index - 1].Cells[col.Index].Value //float.Parse((string)Rows[r.Index - 1].Cells[col.Index].Value, System.Globalization.CultureInfo.InvariantCulture)
                                                                : float.MinValue;
                                                    // только одно значение - args[0] (из одного из столбцов)
                                                    if ((fltVal > float.MinValue)
                                                        && (args[0] > float.MinValue))
                                                        fltVal += args[0];
                                                    else
                                                        ;
                                                }
                                                // отображать только значения ('float.MinValue' - значение отсутствует)
                                                if (fltVal > float.MinValue) {
                                                    if (!(columnAction == AGREGATE_ACTION.UNKNOWN)) {
                                                        fltColumnAgregateValue += fltVal;
                                                    } else
                                                        ;
                                                    // отобразить значение
                                                    r.Cells[iCol].Value = fltVal;
                                                    r.Cells[iCol].ToolTipText = fltVal.ToString();
                                                } else
                                                    ;
                                            } else
                                            // отобразить значение для крайней строки
                                                if ((fltColumnAgregateValue > float.MinValue)
                                                    && (!(columnAction == AGREGATE_ACTION.UNKNOWN)))
                                                    r.Cells[iCol].Value = columnAction == AGREGATE_ACTION.SUMMA
                                                        ? fltColumnAgregateValue
                                                            : fltColumnAgregateValue / (Rows.Count - 1);
                                                else
                                                    ;

                                            r.Cells[iCol].Style.BackColor = s_arCellColors[(int)INDEX_COLOR.VARIABLE];
                                        } catch (Exception e) {
                                            Logging.Logg().Exception(e, string.Format(@"HPanelTepCommon.DataGridViewValues::ShowValues (formula.IsFunc={0:c}, DateTime={1}) - ...", formula.IsFunc, r.Tag), Logging.INDEX_MESSAGE.NOT_SET);
                                        }
                                    } // цикл по всем строкам
                                } else
                                // количество индексов столбцов в формуле == 0
                                    ;
                            } else
                            // для столбца указан не известный тип идентификатора ('tag')
                                Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::ShowValues () - {0}-неизвестный тип идентификатора столбца ...", col.Tag.GetType().FullName), Logging.INDEX_MESSAGE.NOT_SET);
                        else
                        // для столбца не указан идентификатор ('tag')
                            Logging.Logg().Error(string.Format(@"HPanelTepCommon.DataGridViewValues::ShowValues () - не укахан идентификатор столбца ..."), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    // восстановить обработку события - изменение значение в ячейке
                    activateCellValue_onChanged(true);
                } else
                    Logging.Logg().Error(string.Format(@"DataGridViewValues::ShowValues () - нет строк для отображения..."), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }
}
