using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;

namespace PluginTaskReaktivka
{
    partial class PanelTaskReaktivka
    {
        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementReaktivka : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            public enum INDEX_CONTROL_BASE
            {
                UNKNOWN = -1,
                BUTTON_SEND, BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL,
                CBX_PERIOD, CBX_TIMEZONE, HDTP_BEGIN, HDTP_END,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CLBX_COMP_VISIBLED, CLBX_COMP_CALCULATED,
                CHKBX_EDIT,
                COUNT
            }
            /// <summary>
            /// Класс аргумента для события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public class ItemCheckedParametersEventArgs : EventArgs
            {
                /// <summary>
                /// Индекс в списке идентификаторов
                ///  для получения ключа в словаре со значениями
                /// </summary>
                public INDEX_ID m_indxIdDeny;
                /// <summary>
                /// Идентификатор в алгоритме расчета
                /// </summary>
                public int m_idItem;
                /// <summary>
                /// Состояние элемента, связанного с компонентом/параметром_расчета
                /// </summary>
                public CheckState m_newCheckState;

                public ItemCheckedParametersEventArgs(int idItem, INDEX_ID indxIdDeny, CheckState newCheckState)
                    : base()
                {
                    m_idItem = idItem;
                    m_indxIdDeny = indxIdDeny;
                    m_newCheckState = newCheckState;
                }
            }
            /// <summary>
            /// Тип обработчика события - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            /// <param name="ev">Аргумент события</param>
            public delegate void ItemCheckedParametersEventHandler(ItemCheckedParametersEventArgs ev);
            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;
            ///// <summary>
            ///// 
            ///// </summary>
            //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            public PanelManagementReaktivka()
                : base(4, 3)
            {
                InitializeComponents();
                (Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker).ValueChanged += new EventHandler(hdtpEnd_onValueChanged);
            }

            private void InitializeComponents()
            {
                //initializeLayoutStyle();
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                                 //int posColdgvTEPValues = 6;
                SuspendLayout();
                posRow = 0;
                //Период расчета - подпись
                Label lblCalcPer = new Label();
                lblCalcPer.Text = "Период расчета";
                //Период расчета - значение
                ComboBox cbxCalcPer = new ComboBox();
                cbxCalcPer.Name = INDEX_CONTROL_BASE.CBX_PERIOD.ToString();
                cbxCalcPer.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxCalcPer.Enabled = false;
                //Часовой пояс расчета - подпись
                Label lblCalcTime = new Label();
                lblCalcTime.Text = "Часовой пояс расчета";
                //Часовой пояс расчета - значение
                ComboBox cbxCalcTime = new ComboBox();
                cbxCalcTime.Name = INDEX_CONTROL_BASE.CBX_TIMEZONE.ToString();
                cbxCalcTime.DropDownStyle = ComboBoxStyle.DropDownList;
                cbxCalcTime.Enabled = false;
                //
                TableLayoutPanel tlp = new TableLayoutPanel();
                tlp.AutoSize = true;
                tlp.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlp.Controls.Add(lblCalcPer, 0, 0);
                tlp.Controls.Add(cbxCalcPer, 0, 1);
                tlp.Controls.Add(lblCalcTime, 1, 0);
                tlp.Controls.Add(cbxCalcTime, 1, 1);
                Controls.Add(tlp, 0, posRow);
                SetColumnSpan(tlp, 4); this.SetRowSpan(tlp, 1);
                //
                TableLayoutPanel tlpValue = new TableLayoutPanel();
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpValue.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
                tlpValue.Dock = DockStyle.Fill;
                //tlpValue.AutoSize = true;
                //tlpValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                ////Дата/время начала периода расчета - подпись
                Label lBeginCalcPer = new Label();
                lBeginCalcPer.Dock = DockStyle.Bottom;
                lBeginCalcPer.Text = @"Дата/время начала периода расчета:";
                ////Дата/время начала периода расчета - значения
                int cntDays = DateTime.DaysInMonth(s_dtDefaultAU.Year, s_dtDefaultAU.Month);
                int today = s_dtDefaultAU.Day;

                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(-(today - 1)), null);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                tlpValue.Controls.Add(lBeginCalcPer, 0, 0);
                tlpValue.Controls.Add(ctrl, 0, 1);
                //Дата/время  окончания периода расчета - подпись
                Label lEndPer = new Label();
                lEndPer.Dock = DockStyle.Top;
                lEndPer.Text = @"Дата/время окончания периода расчета:";
                //Дата/время  окончания периода расчета - значение
                ctrl = new HDateTimePicker(s_dtDefaultAU.AddDays(cntDays - today)
                    , tlpValue.Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker);
                ctrl.Name = INDEX_CONTROL_BASE.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                //              
                tlpValue.Controls.Add(lEndPer, 0, 2);
                tlpValue.Controls.Add(ctrl, 0, 3);
                this.Controls.Add(tlpValue, 0, posRow = posRow + 1);
                SetColumnSpan(tlpValue, 4); SetRowSpan(tlpValue, 1);
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL_BASE.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL_BASE.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Top;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL_BASE.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL_BASE.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;

                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Top;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 1, 0);
                tlpButton.Controls.Add(ctrlExp, 0, 2);
                this.Controls.Add(tlpButton, 0, posRow = posRow + 2);
                SetColumnSpan(tlpButton, 4); SetRowSpan(tlpButton, 2);

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Включить/исключить компонент для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //
                ctrl = new CheckedListBoxTaskReaktivka();
                ctrl.Name = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Top;
                (ctrl as CheckedListBoxTaskReaktivka).CheckOnClick = true;
                Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 2);
                //Признак Корректировка_включена/корректировка_отключена 
                CheckBox cBox = new CheckBox();
                cBox.Name = INDEX_CONTROL_BASE.CHKBX_EDIT.ToString();
                cBox.Text = @"Корректировка значений разрешена";
                cBox.Dock = DockStyle.Top;
                cBox.Enabled = false;
                cBox.Checked = true;
                this.Controls.Add(cBox, 0, posRow = posRow + 1);
                SetColumnSpan(cBox, 4); SetRowSpan(cBox, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Обработчик события - изменение дата/время окончания периода
            /// </summary>
            /// <param name="obj">Составной объект - календарь</param>
            /// <param name="ev">Аргумент события</param>
            protected void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            {
                m_bflgClear = true;
                HDateTimePicker hdtpEndtimePer = obj as HDateTimePicker;

                DateTimeRangeValue_Changed?.Invoke(hdtpEndtimePer.LeadingValue, hdtpEndtimePer.Value);
            }

            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name="idPeriod"></param>
            public DateTimeRange SetPeriod(ID_PERIOD idPeriod)
            {
                DateTimeRange rangeRes = new DateTimeRange();

                HDateTimePicker hdtpBegin = Controls.Find(INDEX_CONTROL_BASE.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker
                    , hdtpEnd = Controls.Find(INDEX_CONTROL_BASE.HDTP_END.ToString(), true)[0] as HDateTimePicker;

                //Выполнить запрос на получение значений для заполнения 'DataGridView'
                switch (idPeriod)
                {
                    case ID_PERIOD.HOUR:
                        break;
                    case ID_PERIOD.DAY:
                        break;
                    case ID_PERIOD.MONTH:
                        hdtpBegin.Mode =
                        hdtpEnd.Mode =
                            HDateTimePicker.MODE.MONTH;
                        hdtpBegin.Value = new DateTime(DateTime.Now.Year
                            , DateTime.Now.Month
                            , 1
                            , 0
                            , 0
                            , 0);
                        break;
                    case ID_PERIOD.YEAR:
                        break;
                    default:
                        break;
                }

                rangeRes.Set(hdtpBegin.Value, hdtpEnd.Value);

                return rangeRes;
            }

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskReaktivka : CheckedListBox, IControl
            {
                int[] arItem;
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                public CheckedListBoxTaskReaktivka()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId { get { return m_listId[SelectedIndex]; } }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                    Items.Add(text, bChecked);
                    m_listId.Add(id);
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Items.Clear();
                    m_listId.Clear();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }
            }

            /// <summary>
            /// Интерфейс для всех элементов управления с компонентами станции, параметрами расчета
            /// </summary>
            protected interface IControl
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
            /// Добавить элемент компонент станции в списки
            /// , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(int id_comp, string text, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            {
                Control ctrl = null;

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskReaktivka).AddItem(id_comp, text, arChecked[i]);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Найти элемент управления на панели по индексу идентификатора
            /// </summary>
            /// <param name="id">Индекс идентификатора, используемого для заполнения элемента управления</param>
            /// <returns>Дочерний элемент управления</returns>
            protected Control find(INDEX_ID id)
            {
                Control ctrlRes = null;

                ctrlRes = find(getIndexControlOfIndexID(id));

                return ctrlRes;
            }

            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns>элемент панели</returns>
            protected Control find(INDEX_CONTROL_BASE indxCtrl)
            {
                Control ctrlRes = null;

                ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];

                return ctrlRes;
            }

            /// <summary>
            /// Возвратить идентификатор элемента управления по идентификатору
            ///  , используемого для его заполнения
            /// </summary>
            /// <param name="indxId"></param>
            /// <returns>индекс элемента панели</returns>
            protected INDEX_CONTROL_BASE getIndexControlOfIndexID(INDEX_ID indxId)
            {
                INDEX_CONTROL_BASE indxRes = INDEX_CONTROL_BASE.UNKNOWN;

                switch (indxId)
                {
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        indxRes = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED;
                        break;
                    default:
                        break;
                }

                return indxRes;
            }

            /// <summary>
            /// Очистить
            /// </summary>
            public void Clear()
            {
                INDEX_ID[] arIndxIdToClear = new INDEX_ID[] { INDEX_ID.DENY_COMP_VISIBLED };

                ActivateCheckedHandler(false, arIndxIdToClear);

                Clear(arIndxIdToClear);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="arIdToClear"></param>
            public void Clear(INDEX_ID[] arIdToClear)
            {
                for (int i = 0; i < arIdToClear.Length; i++)
                    clear(arIdToClear[i]);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="idToClear"></param>
            private void clear(INDEX_ID idToClear)
            {
                (find(idToClear) as IControl).ClearItems();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bActive"></param>
            /// <param name="arIdToActivate"></param>
            public void ActivateCheckedHandler(bool bActive, INDEX_ID[] arIdToActivate)
            {
                for (int i = 0; i < arIdToActivate.Length; i++)
                    activateCheckedHandler(bActive, arIdToActivate[i]);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bActive"></param>
            /// <param name="idToActivate"></param>
            protected virtual void activateCheckedHandler(bool bActive, INDEX_ID idToActivate)
            {
                INDEX_CONTROL_BASE indxCtrl = INDEX_CONTROL_BASE.UNKNOWN;
                CheckedListBox clbx = null;

                indxCtrl = getIndexControlOfIndexID(idToActivate);

                if (!(indxCtrl == INDEX_CONTROL_BASE.UNKNOWN))
                {
                    clbx = (Controls.Find(indxCtrl.ToString(), true)[0] as CheckedListBox);

                    if (bActive == true)
                        clbx.ItemCheck += new ItemCheckEventHandler(onItemCheck);
                    else
                        clbx.ItemCheck -= onItemCheck;
                }
            }

            /// <summary>
            /// Обработчик события - изменение состояния элемента списка
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected void onItemCheck(object obj, ItemCheckEventArgs ev)
            {
                itemCheck((obj as IControl).SelectedId, getIndexIdOfControl(obj as Control), ev.NewValue);
            }

            /// <summary>
            /// Получение ИД контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>индекс</returns>
            protected INDEX_ID getIndexIdOfControl(Control ctrl)
            {
                INDEX_CONTROL_BASE id = INDEX_CONTROL_BASE.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
                INDEX_ID indxRes = INDEX_ID.UNKNOWN;

                try
                {
                    //Определить идентификатор
                    id = getIndexControl(ctrl);
                    // , соответствующий изменившему состояние элементу 'CheckedListBox'
                    switch (id)
                    {
                        case INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED:
                            indxRes = id == INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelManagementTaskTepValues::onItemCheck () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return indxRes;
            }

            /// <summary>
            /// Получение индекса контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>имя индекса контрола на панели</returns>
            protected INDEX_CONTROL_BASE getIndexControl(Control ctrl)
            {
                INDEX_CONTROL_BASE indxRes = INDEX_CONTROL_BASE.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (strId.Equals(INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED.ToString()) == true)
                    indxRes = INDEX_CONTROL_BASE.CLBX_COMP_VISIBLED;
                else
                    throw new Exception(@"PanelTaskReaktivka::getIndexControl () - не найден объект 'CheckedListBox'...");

                return indxRes;
            }

            /// <summary>
            /// Инициировать событие - изменение признака элемента
            /// </summary>
            /// <param name="address">Адрес элемента</param>
            /// <param name="checkState">Значение признака элемента</param>
            protected void itemCheck(int idItem, INDEX_ID indxIdDeny, CheckState checkState)
            {
                ItemCheck(new ItemCheckedParametersEventArgs(idItem, indxIdDeny, checkState));
            }
        }
    }
}
