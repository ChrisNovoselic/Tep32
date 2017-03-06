using HClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//using System.Windows.Controls;
using System.Windows.Forms;
using TepCommon;

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
                , CHKBX_EDIT = 14 /*, TBLP_BLK, TOOLTIP_GRP,
                PICTURE_BOXDGV, PANEL_PICTUREDGV*/
                    , COUNT
            }
            /// <summary>
            /// экземпляр делегата
            /// </summary>
            public static Func<INDEX_CONTROL, System.Windows.Forms.Control> findControl;            

            private ManagementVisibled m_ManagementVisible;
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
                findControl = new Func<INDEX_CONTROL, Control>(findOfIndexControl);
                //ToolTip tlTipHeader = new ToolTip();
                //tlTipHeader.AutoPopDelay = 5000;
                //tlTipHeader.InitialDelay = 1000;
                //tlTipHeader.ReshowDelay = 500;
                Control ctrl = null;
                //IControl lcbxGroupHeaderVodibled;

                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"

                SuspendLayout();

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
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
                this.Controls.Add(ctrl, ColumnCount / 2, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Кнопка - экспорт
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //
                // передать текущий объект для динамического размещения дочерних элементов управления
                m_ManagementVisible = new ManagementVisibled(this, onItemCheck);                

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
                private class CheckedListBoxGroupHeaders : CheckedListBox, IControl
                {
                    /// <summary>
                    /// подсказка
                    /// </summary>
                    System.Windows.Forms.ToolTip m_ToolTip = new System.Windows.Forms.ToolTip();
                    /// <summary>
                    /// текст подсказки
                    /// </summary>
                    private string[] m_ToolTipText;
                    /// <summary>
                    /// индекс подсказки
                    /// </summary>
                    private int _indexToolTipText;
                    /// <summary>
                    /// Список для хранения идентификаторов переменных
                    /// </summary>
                    private List<int> m_listId;
                    /// <summary>
                    /// Конструктор - основной (без параметров)
                    /// </summary>
                    public CheckedListBoxGroupHeaders()
                        : base()
                    {
                        try {
                            m_listId = new List<int>();

                            m_ToolTipText = new string[s_listGroupHeaders.Count];

                            Dock = DockStyle.Fill;
                            CheckOnClick = true;

                            MouseMove += new MouseEventHandler(showCheckBoxToolTip);
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, @"PanelManagementVedomostBl::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }

                    /// <summary>
                    /// Идентификатор выбранного элемента списка
                    /// </summary>
                    public int SelectedId { get { return m_listId[SelectedIndex]; } }

                    /// <summary>
                    /// Добавить элемент в список
                    /// </summary>
                    /// <param name="id">Идентификатор элемента</param>
                    /// <param name="text">Текст подписи элемента</param>
                    /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                    private void addItem(int id, string text, bool bChecked)
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

                    ///// <summary>
                    ///// Возвращает имя итема
                    ///// </summary>
                    ///// <param name="id">ИдИтема</param>
                    ///// <returns>имя итема</returns>
                    //public string GetNameItem(int id)
                    //{
                    //    string strRes = string.Empty;

                    //    strRes = (string)Items[m_listId.IndexOf(id)];

                    //    return strRes;
                    //}

                    /// <summary>
                    /// обработчик события - отображения всплывающей подсказки по группам
                    /// </summary>
                    /// <param name="sender"></param>
                    /// <param name="e">Аргумент события</param>
                    private void showCheckBoxToolTip(object sender, MouseEventArgs e)
                    {
                        //CheckedListBoxTaskVedomostBl clb = (this.Controls.Find(INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED.ToString(), true)[0] as CheckedListBoxTaskVedomostBl);

                        if (!(_indexToolTipText == /*clb.*/IndexFromPoint(e.Location))) {
                            _indexToolTipText = /*clb.*/IndexFromPoint(/*clb.*/PointToClient(MousePosition));

                            if (!(_indexToolTipText < 0)) {
                                ////Свич по элементам находящимся в чеклистбоксе
                                //switch (clb.Items[_indexToolTipText].ToString()) {
                                //    case "Группа 1":
                                //        m_ToolTip.SetToolTip(clb, m_ToolTipText[_indexToolTipText]);
                                //        break;
                                //    case "Группа 2":
                                //        m_ToolTip.SetToolTip(clb, m_ToolTipText[_indexToolTipText]);
                                //        break;
                                //    case "Группа 3":
                                //        m_ToolTip.SetToolTip(clb, m_ToolTipText[_indexToolTipText]);
                                //        break;
                                //    default:
                                //        break;
                                //}
                                m_ToolTip.SetToolTip(/*clb*/this, m_ToolTipText[_indexToolTipText]);
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
                    public void AddItem(int indx, string text, List<string> textToolTip, bool bChecked)
                    {
                        m_ToolTipText[indx] = formatToolTipText(textToolTip);

                        addItem(indx, text, bChecked);
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
                private HClassLibrary.HPanelCommon _panelParent;

                private ItemCheckEventHandler checkListBox_onItemCheck;
                /// <summary>
                /// Конструктор - основной 
                /// </summary>
                /// <param name="panelParent">Родительский элемент управления (для динамического размещения элементов)</param>>
                public ManagementVisibled(TableLayoutPanel panelParent, ItemCheckEventHandler checkListBoxEventHandler)
                {
                    _panelParent = panelParent as HClassLibrary.HPanelCommon;
                    checkListBox_onItemCheck = checkListBoxEventHandler;

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

                    //Признаки включения/исключения для отображения блока(ТГ)
                    ctrl = new Label();
                    ctrl.Dock = DockStyle.Bottom;
                    (ctrl as Label).Text = @"Выбрать блок для отображения:";
                    _panelParent.Controls.Add(ctrl, 0, posRow);
                    _panelParent.SetColumnSpan(ctrl, _panelParent.ColumnCount);                    
                    //
                    //Признак Корректировка_включена/корректировка_отключена 
                    ctrl = new CheckBox();
                    ctrl.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
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
                        ctrl.CheckedChanged += radioButtonBlock_onCheckedChanged;
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
                public void CreateGroupHeaders(List<CheckState>[] arGroupHeaderCheckStates)
                {
                    Control ctrl = null;
                    int posRow = -1;

                    posRow = _panelParent.IndexLastRowControl;

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
                    // добавить элементы в список
                    for (int indxGroupHeader = 0; indxGroupHeader < s_listGroupHeaders.Count; indxGroupHeader++)
                        m_clbGroupHeaderCheckStates.AddItem(
                            indxGroupHeader
                            , string.Format(@"Группа {0}", indxGroupHeader + 1)
                            , s_listGroupHeaders[indxGroupHeader]
                            , true); //??? всегда TRUE 
                    // зарегистрировать обработчик
                    m_clbGroupHeaderCheckStates.ItemCheck += checkListBox_onItemCheck;
                    // память для всех состояний элементов И для каждого из компонентов ТЭЦ (ТГ)
                    m_arGroupHeaderCheckStates = new List<CheckState>[arGroupHeaderCheckStates.Length];
                    // значения состояний для всех элементов списка
                    for (int i = 0; i < m_arGroupHeaderCheckStates.Length; i++) {
                        m_arGroupHeaderCheckStates[i] = new List<CheckState>(arGroupHeaderCheckStates[i]);
                    }
                }
                /// <summary>
                /// Обработчик события - переключение блока(ТГ)
                /// </summary>
                /// <param name="sender">Объект, инициатор события (??? TableLayoutPanel)</param>
                /// <param name="e">Аргумент события (не используется)</param>
                public void radioButtonBlock_onCheckedChanged(object sender, EventArgs e)
                {
                    int id = SelectedBlockId
                        , indx = (sender as RadioButtonBlock).Index;
                    List<CheckState> listCheckStateValues = new List<CheckState>();
                    PictureBox pictrure;
                    Control ctrl =
                        //findControl(INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED)
                        m_clbGroupHeaderCheckStates
                        ;

                    if ((sender as RadioButtonBlock).Checked == false) {
                        for (int i = 0; i < (ctrl as CheckedListBoxGroupHeaders).Items.Count; i++)
                            listCheckStateValues.Add((ctrl as CheckedListBoxGroupHeaders).GetItemCheckState(i));
                        //??? зачем новый список, можно изменять своевременно старый
                        m_arGroupHeaderCheckStates[indx] = listCheckStateValues;
                    } else
                        ;

                    if ((sender as RadioButtonBlock).Checked == true) {
                        pictrure = s_getPicture(id); //GetPictureOfIdComp
                        pictrure.Visible = true;
                        pictrure.Enabled = true;

                        setCheckStateValues();
                    } else
                        ;
                }
                /// <summary>
                /// Установка состояния элемента 
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
            /// Инициализировать значения для компонента, контролирующего выбор
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            public void AddComponent(DataTable tableCompList, out int err, out string strMsg)
            {
                err = 0;
                strMsg = string.Empty;

                int indxRadioButton = -1;
                Control ctrl = null; // объект для результатов поиска элемента управления

                //создание списка по блокам
                for (indxRadioButton = 0; indxRadioButton < tableCompList.Rows.Count; indxRadioButton++) {
                    // создать элемент для блока
                    m_ManagementVisible.AddRadioButtonBlock(int.Parse(tableCompList.Rows[indxRadioButton][@"ID"].ToString())
                        , ((string)tableCompList.Rows[indxRadioButton][@"DESCRIPTION"]).Trim());
                }
            }

            /// <summary>
            /// Инициализировать значения для компонента, контролирующего отображение групп столбцов(заголовков)
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            public void AddCheckBoxGroupHeaders(DataTable tableCompList, out int err, out string strMsg)
            {
                err = 0;
                strMsg = string.Empty;

                INDEX_ID indxIdToAdd;
                List<CheckState>[] arListCheckStateGroupHeaders;

                arListCheckStateGroupHeaders = new List<CheckState>[tableCompList.Rows.Count];

                //создание списка по блокам
                for (int indxBlock = 0; indxBlock < tableCompList.Rows.Count; indxBlock++) {
                    arListCheckStateGroupHeaders[indxBlock] = new List<CheckState>(s_listGroupHeaders.Count);

                    for (int indxGroupHeader = 0; indxGroupHeader < s_listGroupHeaders.Count; indxGroupHeader++)
                        if (indxGroupHeader < arListCheckStateGroupHeaders[indxBlock].Count)
                            arListCheckStateGroupHeaders[indxBlock][indxGroupHeader] = CheckState.Checked;
                        else
                            arListCheckStateGroupHeaders[indxBlock].Insert(indxGroupHeader, CheckState.Checked);
                }

                m_ManagementVisible.CreateGroupHeaders(arListCheckStateGroupHeaders);                
            }

            #region Поиск элемента
            /// <summary>
            /// Найти элемент управления на панели по индексу идентификатора
            /// </summary>
            /// <param name="id">Индекс идентификатора, используемого для заполнения элемента управления</param>
            /// <returns>Дочерний элемент управления</returns>
            protected Control findOfInfexId(INDEX_ID id)
            {
                Control ctrlRes = null;

                ctrlRes = findOfIndexControl(getIndexControlOfIndexID(id));

                return ctrlRes;
            }

            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns>элемент панели</returns>
            protected Control findOfIndexControl(INDEX_CONTROL indxCtrl)
            {
                Control ctrlRes = null;

                try {
                    ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"PanelManagementTaskTepValues::findOfIndexControl (INDEX_CONTROL={0})", indxCtrl), Logging.INDEX_MESSAGE.NOT_SET);
                }

                return ctrlRes;
            }

            /// <summary>
            /// Возвратить идентификатор элемента управления по идентификатору
            ///  , используемого для его заполнения
            /// </summary>
            /// <param name="indxId">индекс индентификатора контрола</param>
            /// <returns>индекс элемента панели</returns>
            protected INDEX_CONTROL getIndexControlOfIndexID(INDEX_ID indxId)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                switch (indxId)
                {
                    case INDEX_ID.DENY_GROUPHEADER_VISIBLED:
                    case INDEX_ID.HGRID_VISIBLED: //??? какое отношение к панели управления 'PanelManagement'
                        indxRes = INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED;
                        break;
                    case INDEX_ID.BLOCK_SELECTED:
                        //indxRes = INDEX_CONTROL.TBLP_BLK;
                        break;
                    default:
                        break;
                }

                return indxRes;
            }

            /// <summary>
            /// Получение ИД контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>индекс</returns>
            protected INDEX_ID getIndexIdOfControl(Control ctrl)
            {
                INDEX_CONTROL id = INDEX_CONTROL.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
                INDEX_ID indxRes = INDEX_ID.UNKNOWN;

                try {
                    //Определить идентификатор
                    id = getIndexControl(ctrl);
                    // , соответствующий изменившему состояние элементу 'CheckedListBox'
                    switch (id) {
                        case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_GROUPHEADER_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        case INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_GROUPHEADER_VISIBLED ? INDEX_ID.HGRID_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelManagementTaskTepValues::onItemCheck () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return indxRes;
            }

            /// <summary>
            /// Получение индекса контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>имя индекса контрола на панели</returns>
            protected INDEX_CONTROL getIndexControl(Control ctrl)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (Enum.IsDefined(typeof(INDEX_CONTROL), strId) == true)
                    indxRes = (INDEX_CONTROL)Enum.Parse(typeof(INDEX_CONTROL), strId);
                else
                    throw new Exception(string.Format(@"PanelTaskVedomostBl::getIndexControl (Имя={0}, ТИП={1}) - не найден идентификатор...", strId, ctrl.GetType().Name));

                return indxRes;
            }
            #endregion

            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                INDEX_ID[] arIndxIdToClear = null;

                base.Clear();

                arIndxIdToClear = new INDEX_ID[] { INDEX_ID.DENY_GROUPHEADER_VISIBLED };

                Clear(arIndxIdToClear);
            }

            /// <summary>
            /// Очистить значения для элемента(ов) управления
            /// </summary>
            /// <param name="arIndexIdToClear"></param>
            public void Clear(INDEX_ID[] arIndexIdToClear)
            {
                foreach (INDEX_ID indx in arIndexIdToClear)
                    clear(indx);

                //while (Controls.Count > 0)
                //    Controls.RemoveAt(0);
            }

            /// <summary>
            /// Очистить значения для элемента(ов) управления
            /// </summary>
            /// <param name="indxIdToClear">Индекс идентификатора в списке</param>
            private void clear(INDEX_ID indxIdToClear)
            {
                Control ctrl = findOfInfexId(indxIdToClear);

                (ctrl as IControl)?.ClearItems();
            }

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, ItemCheckEventArgs ev)
            {
                itemCheck((int)getIndexIdOfControl(obj as Control), (obj as IControl).SelectedId, ev.NewValue);
            }
        }

    }
}