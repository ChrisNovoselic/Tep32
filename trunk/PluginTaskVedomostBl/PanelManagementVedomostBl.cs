﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//using System.Windows.Controls;
using System.Windows.Forms;

using TepCommon;
using ASUTP;

namespace PluginTaskVedomostBl
{
    partial class PanelTaskVedomostBl
    {
        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementVedomostBl : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// Перечисление контролов панели
            /// </summary>
            public enum INDEX_CONTROL { UNKNOWN = -1
                , BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT
                , TXTBX_EMAIL
                , MENUITEM_UPDATE, MENUITEM_HISTORY
                , CLBX_COMP_VISIBLED/*, CLBX_COMP_CALCULATED*/
                , CLBX_GROUPHEADER_VISIBLED
                , CHKBX_MODE_ENABLE = 14 /*, TBLP_BLK, TOOLTIP_GRP,
                PICTURE_BOXDGV, PANEL_PICTUREDGV*/
                    , COUNT
            }
            ///// <summary>
            ///// экземпляр делегата
            ///// </summary>
            //public static Func<INDEX_CONTROL, System.Windows.Forms.Control> fFindControl;            

            private ManagementVisibled m_managementVisible;
            ///// <summary>
            ///// 
            ///// </summary>
            //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            /// <summary>
            /// конструктор класса
            /// </summary>
            public PanelManagementVedomostBl()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels)
            {
                try {
                    InitializeComponents();
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelManagementVedomostBl::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Инициализация (создание/размещение) объектов управления
            /// </summary>
            private void InitializeComponents()
            {
                //fFindControl = new Func<INDEX_CONTROL, Control>(findOfIndexControl);
                //ToolTip tlTipHeader = new ToolTip();
                //tlTipHeader.AutoPopDelay = 5000;
                //tlTipHeader.InitialDelay = 1000;
                //tlTipHeader.ReshowDelay = 500;
                System.Windows.Forms.Control ctrl = null;
                //IControl lcbxGroupHeaderVodibled;

                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"

                SuspendLayout();

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new ASUTP.Control.DropDownButton ();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - экспорт
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                // передать текущий объект для динамического размещения дочерних элементов управления
                m_managementVisible = new ManagementVisibled(this/*, onItemCheck*/);

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

            /// <summary>
            /// Класс для размещения элементов (блоков) выбора отображения значений
            /// </summary>
            private class ManagementVisibled
            {
                /// <summary>
                /// Класс кнопки выбора блока
                /// </summary>
                private class RadioButtonBlock : System.Windows.Forms.RadioButton
                {
                    /// <summary>
                    /// Индекс элемента
                    /// </summary>
                    private int _indx;

                    /// <summary>
                    /// Индекс элемента
                    /// </summary>
                    public int Index
                    {
                        get { return _indx; }

                        set { _indx = value; }
                    }
                    /// <summary>
                    /// Конструктор - основной (с параметром)
                    /// </summary>
                    /// <param name="nameItem">Текст-подпись для элемента</param>
                    public RadioButtonBlock(int tag)
                    {
                        Tag = tag;

                        InitializeComponents();
                    }

                    /// <summary>
                    /// Инициализация элементов управления объекта (создание, размещение)
                    /// </summary>
                    private void InitializeComponents()
                    {
                        Name = string.Format(@"RB_BLOCK_{0}", (int)Tag);
                    }
                }
                /// <summary>
                /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
                /// </summary>
                private class CheckedListBoxGroupHeaders : CheckedListBoxTaskCalculate
                {
                    /// <summary>
                    /// Объект со строкой подсказки
                    /// </summary>
                    System.Windows.Forms.ToolTip m_ToolTip = new System.Windows.Forms.ToolTip();
                    /// <summary>
                    /// текст подсказки
                    /// </summary>
                    private string[] m_arToolTipTextValue;
                    /// <summary>
                    /// Индекс подсказки
                    /// </summary>
                    private int _indexToolTipText;
                    /// <summary>
                    /// Конструктор - основной (без параметров)
                    /// </summary>
                    public CheckedListBoxGroupHeaders()
                        : base()
                    {
                        try {
                            m_arToolTipTextValue = new string[s_listGroupHeaders.Count];

                            Dock = DockStyle.Fill;
                            CheckOnClick = true;

                            MouseMove += new MouseEventHandler(showCheckBoxToolTip);
                        } catch (System.Exception e) {
                            Logging.Logg().Exception(e, @"CheckedListBoxGroupHeaders::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    /// <summary>
                    /// Обработчик события - отображения всплывающей подсказки по группам
                    /// </summary>
                    /// <param name="sender">Объект - инициатор события</param>
                    /// <param name="e">Аргумент события</param>
                    private void showCheckBoxToolTip(object sender, MouseEventArgs e)
                    {
                        if (!(_indexToolTipText == /*clb.*/IndexFromPoint(e.Location))) {
                            _indexToolTipText = /*clb.*/IndexFromPoint(/*clb.*/PointToClient(MousePosition));

                            if (!(_indexToolTipText < 0)) {
                                m_ToolTip.SetToolTip(/*clb*/this, m_arToolTipTextValue[_indexToolTipText]);
                            } else
                                // нет индекса для отображения
                                ;
                        } else
                            // предыдущий текст остался актуальным
                            ;
                    }
                    /// <summary>
                    /// Формирование текста всплывающей подсказки 
                    /// для групп
                    /// </summary>
                    /// <param name="listText">перечень заголовков, входящих в группу</param>
                    /// <returns>текст всплывающей подсказки</returns>
                    private string formatToolTipText(List<string> listText)
                    {
                        string strTextToolTip = string.Empty;

                        foreach (var item in listText) {
                            if (string.IsNullOrEmpty(strTextToolTip) == false)
                                if (string.IsNullOrEmpty(item) == false)
                                    // разделитель добавить только если И слева И справа есть значение
                                    strTextToolTip += ", ";
                                else
                                    ;
                            else
                                ;

                            strTextToolTip += item;
                        }

                        return strTextToolTip;
                    }
                    /// <summary>
                    /// Добавить элемент компонент станции в списки
                    ///, в соответствии с 'arIndexIdToAdd'
                    /// </summary>
                    /// <param name="id">Идентификатор компонента</param>
                    /// <param name="text">Текст подписи к компоненту</param>
                    /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
                    /// <param name="arChecked">Массив признаков состояния для элементов</param>
                    public void AddItem(int id, string text, List<string> textToolTip, bool bChecked)
                    {
                        m_arToolTipTextValue[id] = formatToolTipText(textToolTip);

                        AddItem(id, text, bChecked);
                    }
                }
                /// <summary>
                /// Объект для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
                /// </summary>
                private CheckedListBoxGroupHeaders m_clbGroupHeaderCheckStates;
                /// <summary>
                /// список активных групп хидеров отображения
                /// </summary>
                private List<CheckState>[] m_arGroupHeaderCheckStates;
                /// <summary>
                /// Список всех элементов управления для вызова на отображение значений для компонента ТЭЦ (ТГ)
                /// </summary>
                private List<RadioButtonBlock> m_listRadioButtonBlock;
                /// <summary>
                /// Элемент упаравления - родитель, для размещения элементов управления
                /// </summary>
                private ASUTP.Control.HPanelCommon _panelParent;

                //private ItemCheckEventHandler checkListBox_onItemCheck;
                /// <summary>
                /// Конструктор - основной 
                /// </summary>
                /// <param name="panelParent">Родительский элемент управления (для динамического размещения элементов)</param>>
                public ManagementVisibled(TableLayoutPanel panelParent/*, ItemCheckEventHandler checkListBoxEventHandler*/)
                {
                    _panelParent = panelParent as ASUTP.Control.HPanelCommon;
                    //checkListBox_onItemCheck = checkListBoxEventHandler;

                    InitializeComponents();
                }
                /// <summary>
                /// Инициализировать (создать, разместить) подчиненные(дочерние) элементы управления
                /// </summary>
                private void InitializeComponents()
                {
                    Control ctrl = null;
                    int posRow = -1;

                    posRow = _panelParent.IndexLastRowControl;
                    //
                    //Признаки включения/исключения для отображения блока(ТГ)
                    ctrl = new Label();
                    ctrl.Dock = DockStyle.Bottom;
                    (ctrl as Label).Text = @"Выбрать блок для отображения:";
                    _panelParent.Controls.Add(ctrl, 0, posRow);
                    _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount);
                    // --------------------------------------------------------
                    // 
                    // --------------------------------------------------------                    
                    posRow = _panelParent.IndexLastRowControl;
                    //Подпись + CheckListBox для вызова/снятия с отображения групп столбцов в представлении
                    //  эти 2 элемента требуют перемещения при добавлении RadioButton-блоков
                    //
                    //Подпись для списка признаков включения/исключения для отображения столбца(ов)
                    ctrl = new Label();
                    ctrl.Dock = DockStyle.Bottom;
                    (ctrl as Label).Text = @"Включить/исключить столбцы для отображения:";
                    _panelParent.Controls.Add(ctrl, 0, posRow);
                    _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount);
                    //
                    //Список признаков включения/исключения для отображения столбца(ов)
                    ctrl =
                    m_clbGroupHeaderCheckStates =
                        new CheckedListBoxGroupHeaders();
                    //ctrl = Activator.CreateInstance <CheckedListBoxGroupHeaders>(); 
                    ctrl.Name = INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED.ToString();
                    _panelParent.Controls.Add(ctrl, 0, posRow = posRow + 1);
                    _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount);
                    _panelParent.SetRowSpan(ctrl, 4);
                    //
                    //Признак Корректировка_включена/корректировка_отключена 
                    ctrl = new CheckBox();
                    ctrl.Name = INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString();
                    ctrl.Text = @"Корректировка значений разрешена";
                    ctrl.Dock = DockStyle.Top;
                    ctrl.Enabled = false;
                    (ctrl as CheckBox).Checked = true;
                    _panelParent.Controls.Add(ctrl, 0, posRow = _panelParent.RowCount - 1);
                    _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount / 2);
                    _panelParent.SetRowSpan(ctrl, 1);
                }
                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedBlockId
                {
                    get {
                        return (int)m_listRadioButtonBlock[SelectedBlockIndex].Tag;
                    }
                }
                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedBlockIndex
                {
                    get {
                        int indx = 0;

                        foreach (RadioButton rb in m_listRadioButtonBlock)
                            if (rb.Checked == true)
                                break;
                            else
                                indx++;

                        return indx;
                    }
                }

                private int getIndexRadioButtonBlock(int tag)
                {
                    int iRes = -1; // элемент не найден
                    Control ctrl = null;

                    if (m_listRadioButtonBlock.Count > 0)
                        iRes = m_listRadioButtonBlock.FindIndex(item => { return (int)item.Tag == tag; });
                    else
                    // список пуст
                        ;

                    return iRes;
                }
                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                /// <param name="rb">массив элементов</param>
                /// <param name="groupCheck">массив чеков группы</param>
                public void AddRadioButtonBlock(int id, string text)
                {
                    RadioButtonBlock ctrl = null;

                    if (m_listRadioButtonBlock == null)
                        m_listRadioButtonBlock = new List<RadioButtonBlock>();
                    else
                        ;
                    // не допустить повторного добавления элемента с одинаковым 'Tag'
                    if (getIndexRadioButtonBlock(id) < 0) {
                        m_listRadioButtonBlock.Add(ctrl = new RadioButtonBlock(id));
                        // индекс
                        ctrl.Index = m_listRadioButtonBlock.Count - 1;
                        // подпись
                        ctrl.Text = text;
                        // 1-ый элемент отображать по умолчанию
                        ctrl.Checked = ctrl.Index == 0;
                        // обработчик события
                        ctrl.CheckedChanged += radioButtonBlockChecked_onChanged;
                        // размещение
                        _panelParent.Controls.Add(ctrl, 0, _panelParent.IndexLastRowControl);
                        _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount);
                    } else
                        ;
                }
                /// <summary>
                /// Добавить элемент управления для установки/снятия признака отображения той или иной группы столбцов в представлении
                /// </summary>
                /// <param name="clb">Элемент управления</param>
                /// <param name="arGroupHeaderCheckStates">Значения для компонентов элемента управления</param>
                public void AddItemGroupHeaders(List<CheckState>[] arGroupHeaderCheckStates)
                {                    
                    // добавить элементы в список
                    for (int indxGroupHeader = 0; indxGroupHeader < s_listGroupHeaders.Count; indxGroupHeader++)
                        m_clbGroupHeaderCheckStates.AddItem(
                            indxGroupHeader
                            , string.Format(@"Группа {0}", indxGroupHeader + 1)
                            , s_listGroupHeaders[indxGroupHeader]
                            , true); //??? всегда TRUE 
                    //// зарегистрировать обработчик
                    //m_clbGroupHeaderCheckStates.ItemCheck += checkListBox_onItemCheck;
                    // память для всех состояний элементов И для каждого из компонентов ТЭЦ (ТГ)
                    m_arGroupHeaderCheckStates = new List<CheckState>[arGroupHeaderCheckStates.Length];
                    // значения состояний для всех элементов списка
                    for (int i = 0; i < m_arGroupHeaderCheckStates.Length; i++) {
                        m_arGroupHeaderCheckStates[i] = new List<CheckState>(arGroupHeaderCheckStates[i]);
                    }
                }

                /// <summary>
                /// Переместить элеменнты управления в ~ от количества блоков(строк)
                /// </summary>
                /// <param name="offsetRow">Количество строк для перемещения в направлении "вниз" формы</param>
                public void RelocateControl(int offsetRow)
                {
                    Label label;
                    CheckBox cbxModeEnabled;
                    TableLayoutPanelCellPosition pos;

                    // перемещаем элемент управления для редактирования признака "Редактирование включено"
                    // , сначала этот элемент, т.к. он снизу
                    cbxModeEnabled = _panelParent.Controls.Find(INDEX_CONTROL.CHKBX_MODE_ENABLE.ToString(), true)[0] as CheckBox;
                    pos = _panelParent.GetCellPosition(cbxModeEnabled);

                    _panelParent.SetCellPosition(cbxModeEnabled, new TableLayoutPanelCellPosition(pos.Column, pos.Row + offsetRow > _panelParent.RowCount - 1 ? _panelParent.RowCount - 1 : pos.Row + offsetRow));

                    // перемещаем элементы управления: подпись + список признаков отображения/снятия_с_отображения групп сигналов
                    pos = _panelParent.GetCellPosition(m_clbGroupHeaderCheckStates);
                    label = _panelParent.GetControlFromPosition(pos.Column, pos.Row - 1) as Label;

                    _panelParent.SetCellPosition(m_clbGroupHeaderCheckStates, new TableLayoutPanelCellPosition(pos.Column, pos.Row + offsetRow));
                    _panelParent.SetCellPosition(label, new TableLayoutPanelCellPosition(pos.Column, (pos.Row - 1) + offsetRow));                    
                } 
                /// <summary>
                /// Обработчик события - переключение блока(ТГ)
                /// </summary>
                /// <param name="sender">Объект, инициатор события (??? TableLayoutPanel)</param>
                /// <param name="e">Аргумент события (не используется)</param>
                public void radioButtonBlockChecked_onChanged(object sender, EventArgs e)
                {
                    int cur_id = (int)(sender as RadioButtonBlock).Tag
                        //, sel_id = SelectedBlockId
                        , indx = (sender as RadioButtonBlock).Index; // для получения 
                    List<CheckState> listCheckStateValues = new List<CheckState>();
                    Control ctrl =
                        //findControl(INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED)
                        m_clbGroupHeaderCheckStates
                        ;

                    // сохранить состояние отображаемых/скрытых групп сигналов для СТАРОГО объекта(блока)
                    if ((sender as RadioButtonBlock).Checked == false) {
                        for (int i = 0; i < (ctrl as CheckedListBoxGroupHeaders).Items.Count; i++)
                            listCheckStateValues.Add((ctrl as CheckedListBoxGroupHeaders).GetItemCheckState(i));
                        //??? зачем новый список, можно изменять своевременно старый
                        m_arGroupHeaderCheckStates[indx] = listCheckStateValues;
                    } else
                        ;
                    //Инициировать событие включить/выключить представление(+ многоуровниевый заголовок) для НОВОГО объекта(блока)
                    (_panelParent as PanelManagementTaskCalculate).PerformItemCheck(cur_id
                        , ItemCheckedParametersEventArgs.TYPE.ENABLE
                        , ((sender as RadioButtonBlock).Checked == true) ? CheckState.Checked
                            : ((sender as RadioButtonBlock).Checked == false) ? CheckState.Unchecked
                                : CheckState.Indeterminate);
                    // установить состояние списка отображаемых/скрываемых групп сигналов для НОВОГО объекта(блока)
                    if ((sender as RadioButtonBlock).Checked == true) {
                        setCheckStateValues();
                    } else
                        ;
                }
                /// <summary>
                /// Установка состояний для элементов списка отображаемых/скрываемых групп сигналов 
                /// </summary>
                private void setCheckStateValues()
                {
                    setCheckStateValues(m_arGroupHeaderCheckStates[SelectedBlockIndex]);
                }
                /// <summary>
                /// Установка состояния элемента 
                /// </summary>
                /// <param name="listCheckState">лист с чекедами для групп</param>
                private void setCheckStateValues(List<CheckState> listCheckState)
                {
                    CheckedListBoxGroupHeaders ctrl =
                        //findControl(INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED) as CheckedListBoxTaskVedomostBl
                        m_clbGroupHeaderCheckStates
                        ;

                    if (listCheckState.Count() > 0)
                        try {
                            if (ctrl.Items.Count > 0)
                                if (ctrl.Items.Count == listCheckState.Count())
                                    for (int i = 0, indxState = 0; i < ctrl.Items.Count; i++, indxState++)
                                        ctrl.SetItemCheckState(indxState, listCheckState[indxState]);
                                else
                                // кол-во элементов для установки значений не соответствует кол-ву значений
                                    ;
                            else
                            // кол-во элементов для установки значений == 0
                                ;
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, string.Format(@"ManagementVisible::setCheckStateValues () - кол-во значений для установки = {0}..."
                                , listCheckState.Count())
                                    , Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                    // кол-во состояний для установки == 0
                        ;
                }
                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {//??? что очищается
                    m_listRadioButtonBlock.Clear();

                    m_clbGroupHeaderCheckStates.ClearItems();
                    //m_arGroupHeaderCheckStates.
                }
            }

            /// <summary>
            /// Идентификатор выбранного элемента списка
            /// </summary>
            public int SelectedBlockId
            {
                get {
                    return m_managementVisible.SelectedBlockId;
                }
            }

            /// <summary>
            /// Инициализировать значения для компонента, контролирующего выбор
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            public void AddComponents(IEnumerable<HandlerDbTaskCalculate.TECComponent> listComponent, out int err, out string strMsg)
            {
                err = 0;
                strMsg = string.Empty;

                int indxRadioButton = -1
                    , indxLastRowControl = -1;
                HandlerDbTaskCalculate.TECComponent comp;

                m_managementVisible.RelocateControl(listComponent.Count());

                //создание списка по блокам
                for (indxRadioButton = 0; indxRadioButton < listComponent.Count(); indxRadioButton++) {
                    comp = listComponent.ElementAt(indxRadioButton);
                    // создать элемент для блока
                    m_managementVisible.AddRadioButtonBlock(comp.m_Id, comp.m_nameShr);
                }

                addCheckBoxGroupHeaders(listComponent, out err, out strMsg);
            }

            /// <summary>
            /// Инициализировать значения для компонента, контролирующего отображение групп столбцов(заголовков)
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            private void addCheckBoxGroupHeaders(IEnumerable<HandlerDbTaskCalculate.TECComponent> listComponent, out int err, out string strMsg)
            {
                err = 0;
                strMsg = string.Empty;

                List<CheckState>[] arListCheckStateGroupHeaders;

                arListCheckStateGroupHeaders = new List<CheckState>[listComponent.Count()];

                //создание списка по блокам
                for (int indxBlock = 0; indxBlock < listComponent.Count(); indxBlock++) {
                    arListCheckStateGroupHeaders[indxBlock] = new List<CheckState>(s_listGroupHeaders.Count);

                    for (int indxGroupHeader = 0; indxGroupHeader < s_listGroupHeaders.Count; indxGroupHeader++)
                        if (indxGroupHeader < arListCheckStateGroupHeaders[indxBlock].Count)
                            arListCheckStateGroupHeaders[indxBlock][indxGroupHeader] = CheckState.Checked;
                        else
                            arListCheckStateGroupHeaders[indxBlock].Insert(indxGroupHeader, CheckState.Checked);
                }

                m_managementVisible.AddItemGroupHeaders(arListCheckStateGroupHeaders);                
            }

            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                base.Clear();

                Clear(new INDEX_CONTROL[] { INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED });
            }

            /// <summary>
            /// Очистить значения для элемента(ов) управления
            /// </summary>
            /// <param name="arIndexIdToClear">Массив идентификаторов(прототипов) элементов управления, к которым должна быть применена операция</param>
            public void Clear(INDEX_CONTROL[] arIndexIdToClear)
            {
                foreach (INDEX_CONTROL indx in arIndexIdToClear)
                    clear(indx);

                //while (Controls.Count > 0)
                //    Controls.RemoveAt(0);
            }

            /// <summary>
            /// Очистить значения для элемента(ов) управления
            /// </summary>
            /// <param name="indxIdToClear">Индекс идентификатора в списке</param>
            private void clear(INDEX_CONTROL indxCtrlToClear)
            {
                Control ctrl = findControl(indxCtrlToClear.ToString());

                (ctrl as IControl)?.ClearItems();
            }

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                PerformItemCheck((obj as IControl).SelectedId, ItemCheckedParametersEventArgs.TYPE.VISIBLE, (ev as ItemCheckEventArgs).NewValue);
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                activateControlChecked_onChanged(
                    new INDEX_CONTROL[] { INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED }.ToList().ConvertAll<string>(indx => {
                            return indx.ToString();
                        }).ToArray()
                    , bActivate);
            }
        }

    }
}