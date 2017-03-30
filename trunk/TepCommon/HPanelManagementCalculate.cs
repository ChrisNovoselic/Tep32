using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Reflection;

namespace TepCommon
{
    partial class HPanelTepCommon
    {
        /// <summary>
        /// Интерфейс для всех элементов управления с компонентами станции, параметрами расчета
        /// </summary>
        public interface IControl
        {
            /// <summary>
            /// Идентификатор выбранного элемента списка
            /// </summary>
            int SelectedId { get; }
            ///// <summary>
            ///// Добавить элемент в список
            ///// </summary>
            ///// <param name="text">Текст подписи элемента</param>
            ///// <param name="id">Идентификатор элемента</param>
            ///// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
            //void AddItem(int id, string text, bool bChecked);
            /// <summary>
            /// Удалить все элементы в списке
            /// </summary>
            void ClearItems();
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected abstract class PanelManagementTaskCalculate : HClassLibrary.HPanelCommon
        {
            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskCalculate : CheckedListBox, IControl
            {
                private struct ITEM
                {
                    public int Id;

                    public string Text;                    

                    public override string ToString()
                    {
                        return Text;
                    }
                }
                ///// <summary>
                ///// Список для хранения идентификаторов переменных
                ///// </summary>
                //private List<int> m_listId;

                public CheckedListBoxTaskCalculate()
                    : base()
                {
                    //m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId { get { return /*m_listId*/((ITEM)Items[SelectedIndex]).Id; } }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                    int indxNewItem = -1;

                    indxNewItem = Items.Add(new ITEM() { Id = id, Text = text }, bChecked);
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Items.Clear();
                    //m_listId.Clear();
                }

                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = Items.Cast<ITEM>().First(item => { return item.Id == id; }).Text;

                    return strRes;
                }
            }

            private enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1
                , CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END
                    , COUNT
            }

            public delegate void DateTimeRangeValueChangedEventArgs(DateTime dtBegin, DateTime dtEnd);

            public /*event */DateTimeRangeValueChangedEventArgs DateTimeRangeValue_Changed;

            public static DateTime s_dtDefault = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);

            /// <summary>
            /// Класс аргумента для события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public class ItemCheckedParametersEventArgs : EventArgs
            {
                public enum TYPE { ENABLE, VISIBLE }
                /// <summary>
                /// Индекс в списке идентификаторов
                ///  для получения ключа в словаре со значениями
                /// </summary>
                public TYPE m_type;
                /// <summary>
                /// Идентификатор в алгоритме расчета
                /// </summary>
                public int m_idAlg;

                public int m_idComp;

                public int m_idPut;

                private CheckState _newCheckState;
                /// <summary>
                /// Состояние элемента, связанного с компонентом/параметром_расчета
                /// </summary>
                public CheckState NewCheckState
                {
                    get { return _newCheckState; }
                    set { _newCheckState = value; }
                }

                public ItemCheckedParametersEventArgs(int id, TYPE type, CheckState newCheckState)
                    : base()
                {
                    m_idAlg = -1;
                    m_idComp = -1;
                    m_idPut = -1;

                    if (id < (int)ID_START_RECORD.ALG)
                    // очевидно, что это компонент
                        m_idComp = id;
                    else if (id < (int)ID_START_RECORD.PUT)
                    // очевидно, что это параметр в алгоритме расчета
                        m_idAlg = id;
                    else
                    // очевидно, что это параметр в алгоритме расчета + связанный с компонентом
                        m_idPut = id;

                    m_type = type;

                    _newCheckState = newCheckState;
                }

                public bool IsNAlg { get { return (!(m_idAlg < (int)ID_START_RECORD.ALG)) && (m_idAlg < (int)ID_START_RECORD.PUT); } }

                public bool IsComponent { get { return ((m_idComp > 0) && (m_idComp < (int)HandlerDbTaskCalculate.TECComponent.TYPE.UNREACHABLE)) && (m_idPut < 0); } }

                public bool IsPut { get { return (!(m_idPut < (int)ID_START_RECORD.PUT)); } }
            }

            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;

            /// <summary>
            /// Инициировать событие - изменение признака элемента
            /// </summary>
            /// <param name="address">Адрес элемента</param>
            /// <param name="checkState">Значение признака элемента</param>
            protected void itemCheck(int idItem, ItemCheckedParametersEventArgs.TYPE type, CheckState checkState)
            {
                ItemCheck(new ItemCheckedParametersEventArgs(idItem, type, checkState));
            }

            /// <summary>
            /// Обработчик события - изменение состояния элемента списка
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected abstract void onItemCheck(object obj, EventArgs ev); //ItemCheckEventArgs

            /// <summary>
            /// Признаки порядка размещения элементов управления (последовательно - один над другим, одновременно - на одной строке, наличие подписей)
            /// </summary>
            [Flags]
            public enum ModeTimeControlPlacement { Unknown, Queue = 0x1, Twin = 0x2, Labels = 0x4 }

            private ModeTimeControlPlacement _timeControlPlacement;

            public ModeTimeControlPlacement TimeControlPlacement { get { return _timeControlPlacement; } set { _timeControlPlacement = value; replacementTimeControl(); } }

            /// <summary>
            /// Тип обработчика события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            /// <param name="ev">Аргумент события</param>
            public delegate void ItemCheckedParametersEventHandler(ItemCheckedParametersEventArgs ev);

            public PanelManagementTaskCalculate(ModeTimeControlPlacement timeControlPlacement)
                : base(8, 21)
            {
                TimeControlPlacement = timeControlPlacement;

                InitializeComponents();

                //HDateTimePicker hdtpEnd = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                //m_dtRange = new DateTimeRange((Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value
                //    , hdtpEnd.Value);
                ////Назначить обработчик события - изменение дата/время начала периода
                //hdtpBegin.ValueChanged += new EventHandler(hdtpBegin_onValueChanged);
                //Назначить обработчик события - изменение дата/время окончания периода
                // при этом отменить обработку события - изменение дата/время начала периода
                // т.к. при изменении дата/время начала периода изменяется и дата/время окончания периода
                (findControl(INDEX_CONTROL_BASE.HDTP_END.ToString()) as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);                
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = null;
                int posRow = -1;

                SuspendLayout();

                initializeLayoutStyle();

                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                //Базовый период отображения данных
                //Базовый период отображения данных - Подпись
                if ((TimeControlPlacement & ModeTimeControlPlacement.Labels) == ModeTimeControlPlacement.Labels) {
                    ctrl = new System.Windows.Forms.Label();
                    //ctrl.Dock = DockStyle.Bottom;
                    ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Bottom);
                    (ctrl as System.Windows.Forms.Label).Text = @"Период расчета";
                    this.Controls.Add(ctrl, 0, posRow = posRow + 1); //posRow = posRow + 1
                    SetColumnSpan(ctrl, this.ColumnCount / 2);
                } else
                    ;
                //Базовый период отображения данных - Значение
                ctrl = new ComboBox();
                ctrl.Name = INDEX_CONTROL_BASE.CBX_PERIOD.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //??? точное размещенеие в коде целевого класса
                this.Controls.Add(ctrl, 0, posRow = posRow + 1); //??? добавлять для возможности последующего поиска
                SetColumnSpan(ctrl, this.ColumnCount / 2);
                //Часовой пояс
                //Часовой пояс - Подпись
                if ((TimeControlPlacement & ModeTimeControlPlacement.Labels) == ModeTimeControlPlacement.Labels) {
                    posRow = -1;

                    ctrl = new System.Windows.Forms.Label();
                    //ctrl.Dock = DockStyle.Bottom;
                    ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Bottom);
                    (ctrl as System.Windows.Forms.Label).Text = @"Часовой пояс";
                    this.Controls.Add(ctrl
                        , (TimeControlPlacement & ModeTimeControlPlacement.Queue) == ModeTimeControlPlacement.Queue ? 0 : ColumnCount / 2
                        , posRow = posRow + 1); //
                    SetColumnSpan(ctrl, this.ColumnCount / 2);
                } else
                    ;                
                //Часовой пояс - Значение
                ctrl = new ComboBox();
                ctrl.Name = INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl
                    , (TimeControlPlacement & ModeTimeControlPlacement.Queue) == ModeTimeControlPlacement.Queue ? 0 : ColumnCount / 2
                    , posRow = posRow + 1); //??? добавлять для возможности последующего поиска (без указания столбца, строки)
                SetColumnSpan(ctrl, this.ColumnCount / 2);

                //Дата/время начала периода расчета
                //Дата/время начала периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время начала периода расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1); //posRow = posRow + 1
                SetColumnSpan(ctrl, this.ColumnCount);
                //Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker(s_dtDefault, null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Dock = DockStyle.Top;
                //ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl, 0, posRow = posRow + 1); //posRow = posRow + 1 //??? добавлять для возможности последующего поиска (без указания столбца, строки)
                SetColumnSpan(ctrl, this.ColumnCount);
                //Дата/время  окончания периода расчета
                //Дата/время  окончания периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время  окончания периода расчета:";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1); //posRow = posRow + 1
                SetColumnSpan(ctrl, this.ColumnCount);
                //Дата/время  окончания периода расчета - значения
                ctrl = new HDateTimePicker(s_dtDefault.AddHours(1), Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Dock = DockStyle.Top;
                //ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //??? точное (столбец, строка) размещенеие в коде целевого класса
                this.Controls.Add(ctrl, 0, posRow = posRow + 1); //posRow = posRow + 1 //??? добавлять для возможности последующего поиска (без указания столбца, строки)                
                SetColumnSpan(ctrl, this.ColumnCount); SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            ///// <summary>
            ///// Обработчик события - изменение дата/время начала периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //private void hdtpBegin_onValueChanged(object obj, EventArgs ev)
            //{
            //    m_dtRange.Set((obj as HDateTimePicker).Value, m_dtRange.End);

            //    DateTimeRangeValue_Changed(this, EventArgs.Empty);
            //}

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            private void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                HDateTimePicker hdtpEnd = obj as HDateTimePicker;
                DateTimeRangeValue_Changed?.Invoke(hdtpEnd.LeadingValue, hdtpEnd.Value);

            }

            /// <summary>
            /// Изменить размещение элементов управления датой/временем
            /// </summary>
            private void replacementTimeControl()
            {
            }

            public DateTimeRange DatetimeRange
            {
                get
                {
                    return new DateTimeRange((Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value
                        , (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value);
                }
            }

            public virtual void Clear()
            {
                INDEX_CONTROL_BASE indxCtrl;
                ComboBox cbx = null;

                indxCtrl = PanelManagementTaskCalculate.INDEX_CONTROL_BASE.CBX_PERIOD;
                //activateComboBoxSelectedIndex_onChanged(indxCtrl, cbxPeriod_SelectedIndexChanged);
                //cbx = Controls.Find(indxCtrl.ToString(), true)[0] as ComboBox;
                //cbx.Enabled = false;
                ////??? даже после отмены регистрации обработчика он вызывается, поэтому не очищаем 
                //cbx.DataSource = null;
                ////cbx.Items.Clear(); // элементы удалены автоматически

                indxCtrl = PanelManagementTaskCalculate.INDEX_CONTROL_BASE.CBX_TIMEZONE;
                //activateComboBoxSelectedIndex_onChanged(indxCtrl, cbxTimezone_SelectedIndexChanged);
                //cbx = Controls.Find(indxCtrl.ToString(), true)[0] as ComboBox;
                //cbx.Enabled = false;
                //cbx.DataSource = null;
                //// элементы удалены автоматически
            }

            private void activateComboBoxSelectedIndex_onChanged(INDEX_CONTROL_BASE indxCtrl, EventHandler handler, bool bActivate = false)
            {
                ComboBox cbx = Controls.Find(indxCtrl.ToString(), true)[0] as ComboBox;

                // вариант №1
                // всегда отписываться
                cbx.SelectedIndexChanged -= handler;

                // при необходимости подписаться
                if (bActivate == true)
                    cbx.SelectedIndexChanged += new EventHandler(handler);
                else
                    ;

                //// вариант №2
                //activateControl_onHandler(Controls.Find(indxCtrl.ToString(), true)[0] as ComboBox, "SelectedIndexChanged", handler, bActivate);

                ////??? вариант №3 универсальный
                ////activateControl_onHandler(cbx, cbx.SelectedIndexChanged, handler, bActivate);
            }

            private void activateControl_onHandler(Control ctrl, string nameEvent, EventHandler handler, bool bActivate = false)
            {
                EventInfo infoEvent;
                FieldInfo infoField;
                PropertyInfo infoProperty;
                System.Reflection.EventInfo[] infoEvents = ctrl.GetType().GetEvents();

                infoEvent = infoEvents.First(ie => { return (ie.Name.Equals(nameEvent) == true); });
                if (!(infoEvent == null)) {
                    infoField = ctrl.GetType().GetField(string.Format("EVENT_{0}", infoEvent.Name.ToUpper())
                        /*, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static*/);
                    infoProperty = typeof(ComboBox).GetProperty("Events"
                        , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);                        

                    if ((!(infoField == null))
                        && (!(infoProperty == null))) {
                        //Get the field EVENT_SELECTEDINDEXCHANGED
                        var eventKey = infoField?.GetValue(ctrl);
                        //Get the event handler list of the Control
                        var eventList = infoProperty?.GetValue(ctrl, null) as EventHandlerList;

                        if (bActivate == false)
                            //Check if there is not any handler for nameEvent
                            if (eventList[eventKey] == null)
                            // списка обработчиков нет
                                ;
                            else
                                if (eventList[eventKey].GetInvocationList().Count() == 0)
                                // ни одного обработчика
                                    ;
                                else
                                    eventList.RemoveHandler(eventKey, handler);
                        else
                            eventList.AddHandler(eventKey, handler);
                    } else
                        Logging.Logg().Error(string.Format(@"::activateControl_onHandler (EVENT={0}) - невозможно получить либо ключ события в списке событий, либо сам список", nameEvent), Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    Logging.Logg().Error(string.Format(@"::activateControl_onHandler (EVENT={0}) - не найдена информация по событию", nameEvent), Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// Событие при изменениии основных настроечных параметров (ПЕРИОДб ЧАСОВОЙ ПОЯС, ДИАПАЗОН ДАТЫ/ВРЕМЕНИ)
            /// </summary>
            public event DelegateObjectFunc EventIndexControlBaseValueChanged;

            public event DelegateObjectFunc EventIndexControlCustomValueChanged;
            /// <summary>
            /// Обработчик события при изменении периода расчета
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
            {
                //Отменить обработку события - изменение начала/окончания даты/времени
                activateDateTimeRangeValue_OnChanged(false);
                //Установить новые режимы для "календарей"
                setModeDatetimeRange();
                //Возобновить обработку события - изменение начала/окончания даты/времени
                activateDateTimeRangeValue_OnChanged(true);
                //Отменить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
                activateControlChecked_onChanged(false);                
                EventIndexControlBaseValueChanged?.Invoke(ID_DBTABLE.TIME);
                //Возобновить обработку событий - изменения состояния параметра в алгоритме расчета ТЭП
                activateControlChecked_onChanged(true);
            }

            protected abstract void activateControlChecked_onChanged(bool bActivate);

            protected virtual void activateControlChecked_onChanged(string[] arNameControlToActivate, bool bActive)
            {
                activateCheckedListBoxChecked_onChanged(arNameControlToActivate, bActive);
            }

            private void activateCheckedListBoxChecked_onChanged(string[] arNameControlToActivate, bool bActive)
            {
                CheckedListBox clbx;

                foreach (string nameControlToActivate in arNameControlToActivate) {
                    //indxCtrl = getIndexControlOfIndexID(idToActivate);

                    if (!(nameControlToActivate == "UNKNOWN")) {
                        clbx = findControl(nameControlToActivate) as  CheckedListBox;

                        if (!(clbx == null))
                            // вариант №1
                            if (bActive == true) {
                                clbx.ItemCheck += new ItemCheckEventHandler(onItemCheck);
                            } else {
                                clbx.ItemCheck -= onItemCheck;
                            }
                            //// вариант №2
                            //activateControl_onHandler(clbx, "ItemCheck", onItemCheck, bActive);
                        else
                        // не CheckedListBox
                            Logging.Logg().Warning(string.Format(@"::activateCheckedListBoxChecked_onChanged () - в качестве аргумента указано .Name={0} не CheckedListBox", nameControlToActivate)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ;
                }
            }
            /// <summary>
            /// Обработчик события - изменение часового пояса
            /// </summary>
            /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
            /// <param name="ev">Аргумент события</param>
            protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
            {
                EventIndexControlBaseValueChanged?.Invoke(ID_DBTABLE.TIMEZONE);
            }

            public ID_PERIOD IdPeriod
            {
                get { return (ID_PERIOD)getComboBoxSelectedValue(INDEX_CONTROL_BASE.CBX_PERIOD); }
            }

            public ID_TIMEZONE IdTimezone
            {
                get { return (ID_TIMEZONE)getComboBoxSelectedValue(INDEX_CONTROL_BASE.CBX_TIMEZONE); }
            }

            private object getComboBoxSelectedValue(INDEX_CONTROL_BASE indx)
            {
                object objRes = -1;

                ComboBox cbx = (Controls.Find(indx.ToString(), true)[0] as ComboBox);

                if ((!(cbx == null))
                    && (!(cbx.SelectedIndex < 0)))
                    objRes =
                        //Convert.ChangeType(((COMBOBOX_ITEM)cbx.SelectedItem).m_Id, typeof(int))
                        Convert.ChangeType(cbx.SelectedValue, typeof(int))
                        ;
                else
                    ;

                return objRes;
            }

            public void FillValuePeriod(DataTable tableValues, ID_PERIOD idSelected)
            {
                fillComboBoxValues(tableValues.Rows.Cast<DataRow>(), INDEX_CONTROL_BASE.CBX_PERIOD, (int)idSelected, @"DESCRIPTION", cbxPeriod_SelectedIndexChanged);
            }

            public void FillValueTimezone(DataTable tableValues, ID_TIMEZONE idSelected)
            {
                fillComboBoxValues(tableValues.Rows.Cast<DataRow>(), INDEX_CONTROL_BASE.CBX_TIMEZONE, (int)idSelected, @"NAME_SHR", cbxTimezone_SelectedIndexChanged);
            }

            //private struct COMBOBOX_ITEM
            //{
            //    public string m_Text;

            //    public object m_Id;
            //}

            private void fillComboBoxValues(IEnumerable<DataRow> rowValues
                , INDEX_CONTROL_BASE indxCtrl
                , int idSelected
                , string nameFieldTextValue
                , EventHandler handler)
            {
                int indx = -1
                    , indxSelected = -1;
                ComboBox ctrl = null;
                //COMBOBOX_ITEM cbxItem;
                DataTable tableValues;

                ctrl = Controls.Find(indxCtrl.ToString(), true)[0] as ComboBox;
                //// вариант №1
                //foreach (DataRow r in tableValues.Rows) {
                //    cbxItem = new COMBOBOX_ITEM() { m_Text = (string)r[nameFieldTextValue], m_Id = Convert.ChangeType(r[@"ID"], typeof(int)) };
                //    indx = ctrl.Items.Add(cbxItem);

                //    if ((int)cbxItem.m_Id == idSelected)
                //        if (indxSelected < 0)
                //            indxSelected = indx;
                //        else
                //            throw new Exception(@"PanelManagementTaskCalculaye::fillComboBoxValues () - неоднозначный выбор элемента по указанному идентификатору ...");
                //    else
                //        ;
                //}

                //ctrl.ValueMember = @"m_Id"; //"ID";
                //ctrl.DisplayMember = @"m_Text"; //nameFieldTextValue;

                // вариант №2
                if (rowValues.Count() > 0) {
                    ctrl.ValueMember = @"ID";
                    ctrl.DisplayMember = nameFieldTextValue;
                    tableValues = rowValues.ElementAt(0).Table.Clone();
                    rowValues.ToList().ForEach(r => { tableValues.Rows.Add(r.ItemArray); });                    
                    ctrl.DataSource = tableValues;

                    ctrl.SelectedIndex = -1;
                    //??? до или после
                    //ctrl.SelectedIndexChanged += new EventHandler(handler);
                    activateComboBoxSelectedIndex_onChanged(indxCtrl, handler, true);

                    //if (!(idSelected < 0))
                        ctrl.SelectedValue = idSelected;
                    //else
                    //    throw new Exception(@"PanelManagementTaskCalculaye::fillComboBoxValues () - не найдена строка для выбора по указанному идентификатру ...");
                } else {
                    Logging.Logg().Error(string.Format(@"PanelManagementTaskCalculate::fillComboBoxValues () - таблица для DataSource пуста..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            /// <summary>
            /// Признак доступности элемента управления для выбора часового пояса
            /// </summary>
            public bool AllowUserTimezoneChanged { get { return Controls.Find(INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0].Enabled; } set { Controls.Find(INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString(), true)[0].Enabled = value; } }
            /// <summary>
            /// Признак доступности элемента управления для выбора периода расчета
            /// </summary>
            public bool AllowUserPeriodChanged { get { return Controls.Find(INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0].Enabled; } set { Controls.Find(INDEX_CONTROL_BASE.CBX_PERIOD.ToString(), true)[0].Enabled = value; } }

            private void setModeDatetimeRange()
            {
                ID_PERIOD idPeriod = IdPeriod;

                HDateTimePicker hdtpBegin = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                    , hdtpEnd = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;
                //Выполнить запрос на получение значений для заполнения 'DataGridView'
                switch (idPeriod)
                {
                    case ID_PERIOD.HOUR:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , DateTime.Now.Hour
                            , 0
                            , 0).AddHours(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddHours(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.HOUR;
                        break;
                    //case ID_PERIOD.SHIFTS:
                    //    hdtpBegin.Mode = HDateTimePicker.MODE.HOUR;
                    //    hdtpEnd.Mode = HDateTimePicker.MODE.HOUR;
                    //    break;
                    case ID_PERIOD.DAY:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , DateTime.Now.Day
                            , 0
                            , 0
                            , 0).AddDays(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddDays(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.DAY;
                        break;
                    case ID_PERIOD.MONTH:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , 1
                            , 0
                            , 0
                            , 0)/*.AddMonths(-1)*/;
                        hdtpEnd.Value = hdtpBegin.Value.AddMonths(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.MONTH;
                        break;
                    case ID_PERIOD.YEAR:
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , 1
                            , 1
                            , 0
                            , 0
                            , 0).AddYears(-1);
                        hdtpEnd.Value = hdtpBegin.Value.AddYears(1);
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.YEAR;
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// Регистрация/отмена регистрации обработчика события - изменение диапазона даты/времени
            /// </summary>
            /// <param name="active">Признак регистрации/отмены регистрации обработчика</param>
            protected void activateDateTimeRangeValue_OnChanged(bool active)
            {
                // всегда отписаться (исключения при пустом событии не происходит)
                DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                // при необходимости (по аргументу) - подписаться
                if (active == true)
                    DateTimeRangeValue_Changed += new DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    ;
            }
            /// <summary>
            /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
            {
                EventIndexControlBaseValueChanged(ID_DBTABLE.UNKNOWN);
            }
        }
    }
}
