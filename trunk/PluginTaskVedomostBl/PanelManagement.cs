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
            /// подсказка
            /// </summary>
            System.Windows.Forms.ToolTip tlTipGrp = new System.Windows.Forms.ToolTip();
            /// <summary>
            /// текст подсказки
            /// </summary>
            string[] toolTipText;
            /// <summary>
            /// индекс подсказки
            /// </summary>
            private int toolTipIndex;
            /// <summary>
            /// Перечисление контролов панели
            /// </summary>
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1,
                BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT,
                TXTBX_EMAIL,
                MENUITEM_UPDATE, MENUITEM_HISTORY,
                CLBX_COMP_VISIBLED, CLBX_COMP_CALCULATED, CLBX_COL_VISIBLED,
                CHKBX_EDIT, TBLP_BLK, TOOLTIP_GRP,
                PICTURE_BOXDGV, PANEL_PICTUREDGV,
                COUNT
            }
            /// <summary>
            /// Делегат
            /// </summary>
            /// <param name="indx">индекс контрола панели</param>
            /// <returns>контрол на панели</returns>
            public delegate System.Windows.Forms.Control ControlDelegateIndexControlFunc(INDEX_CONTROL indx);
            /// <summary>
            /// экземпляр делегата
            /// </summary>
            public static ControlDelegateIndexControlFunc _getControls;
            /// <summary>
            /// Событие - изменение выбора запрет/разрешение
            ///  для компонента/параметра при участии_в_расчете/отображении
            /// </summary>
            public event ItemCheckedParametersEventHandler ItemCheck;
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

                    toolTipText = new string[s_listGroupHeaders.Count];
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelManagementVedomostBl::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

            }

            /// <summary>
            /// 
            /// </summary>
            private void InitializeComponents()
            {
                _getControls = new ControlDelegateIndexControlFunc(find);
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
                //
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                //ctrl.Dock = DockStyle.Top;
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); //SetRowSpan(ctrl, 1);
                //Признаки включения/исключения для отображения блока(ТГ)
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Выбрать блок для отображения:";
                TableLayoutPanel tlpChk = new TableLayoutPanel();
                tlpChk.Controls.Add(ctrl, 0, 0);
                //
                ctrl = new TableLayoutPanelVisibleManagement();
                ctrl.Name = INDEX_CONTROL.TBLP_BLK.ToString();
                ctrl.Dock = DockStyle.Top;
                tlpChk.Controls.Add(ctrl, 0, 1);
                //Признак для включения/исключения для отображения столбца(ов)
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Включить/исключить столбцы для отображения:";
                tlpChk.Controls.Add(ctrl, 0, 2);
                //
                //lcbxGroupHeaderVodibled = Activator.CreateInstance <CheckedListBoxTaskVedomostBl>();
                ctrl = new CheckedListBoxTaskVedomostBl();
                ctrl.MouseMove += new MouseEventHandler(showCheckBoxToolTip); ;
                ctrl.Name = INDEX_CONTROL.CLBX_COL_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Top;
                (ctrl as CheckedListBoxTaskVedomostBl).CheckOnClick = true;
                tlpChk.Controls.Add(ctrl, 0, 3);
                tlpChk.Dock = DockStyle.Fill;
                tlpChk.AutoSize = true;
                tlpChk.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                tlpChk.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
                Controls.Add(tlpChk, 0, posRow = posRow + 4);
                SetColumnSpan(tlpChk, 4); SetRowSpan(tlpChk, 2);
                //Признак Корректировка_включена/корректировка_отключена 
                CheckBox cBox = new CheckBox();
                cBox.Name = INDEX_CONTROL.CHKBX_EDIT.ToString();
                cBox.Text = @"Корректировка значений разрешена";
                cBox.Dock = DockStyle.Top;
                cBox.Enabled = false;
                cBox.Checked = true;
                Controls.Add(cBox, 0, posRow = posRow + 1);
                SetColumnSpan(cBox, 4); SetRowSpan(cBox, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// обработчик события - отображения всплывающей подсказки по группам
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e">Аргумент события</param>
            private void showCheckBoxToolTip(object sender, MouseEventArgs e)
            {
                CheckedListBoxTaskVedomostBl clb = (this.Controls.Find(INDEX_CONTROL.CLBX_COL_VISIBLED.ToString(), true)[0] as CheckedListBoxTaskVedomostBl);

                if (toolTipIndex != clb.IndexFromPoint(e.Location))
                {
                    toolTipIndex = clb.IndexFromPoint(clb.PointToClient(MousePosition));
                    if (toolTipIndex > -1)
                    {
                        //Свич по элементам находящимся в чеклистбоксе
                        switch (clb.Items[toolTipIndex].ToString())
                        {
                            case "Группа 1":
                                tlTipGrp.SetToolTip(clb, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 2":
                                tlTipGrp.SetToolTip(clb, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 3":
                                tlTipGrp.SetToolTip(clb, toolTipText[toolTipIndex]);
                                break;
                        }
                    }
                }
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
                /// Инициализация объекта
                /// </summary>
                /// <param name="nameItem">Текст-подпись для элемента</param>
                private void InitializeComponents()
                {
                    Name = string.Format(@"RB_BLOCK_{0}", (int)Tag);
                }
            }

            /// <summary>
            /// Класс для размещения элементов (блоков) выбора отображения значений
            /// </summary>
            private class TableLayoutPanelVisibleManagement : TableLayoutPanel
            {
                /// <summary>
                /// список активных групп хидеров отображения
                /// </summary>
                protected List<CheckState>[] m_arGroupHeaderCheckStates;
                /// <summary>
                /// 
                /// </summary>
                public RadioButtonBlock[] m_arRadioButtonBlock;

                /// <summary>
                /// 
                /// </summary>
                public TableLayoutPanelVisibleManagement()
                    : base()
                {
                    InitializeComponents();
                }

                private void InitializeComponents()
                {
                    RowCount = 1;
                    ColumnCount = 3;

                    RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId
                {
                    get
                    {
                        int indx = 0;

                        foreach (RadioButton rb in Controls)
                            if (rb.Checked == true)
                                break;
                            else
                                indx++;

                        return (int)m_arRadioButtonBlock[indx].Tag;
                    }
                }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                /// <param name="rb">массив элементов</param>
                /// <param name="groupCheck">массив чеков группы</param>
                public void AddItems(RadioButtonBlock[] arRadioButton)
                {
                    int indx = -1
                       , col = -1
                       , row = -1;                    

                    if (m_arRadioButtonBlock == null)
                        m_arRadioButtonBlock = arRadioButton;

                    for (int i = 0; i < m_arRadioButtonBlock.Length; i++) {
                        m_arRadioButtonBlock[i].CheckedChanged += TableLayoutPanelkVed_CheckedChanged;                        
                        m_arRadioButtonBlock[i].Index = i;

                        if (RowCount * ColumnCount < m_arRadioButtonBlock.Length)
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() => RowCount++));
                                Invoke(new Action(() => RowStyles.Add(new RowStyle(SizeType.Percent, 20F))));
                            }
                            else
                            {
                                if (ColumnCount > RowCount)
                                {
                                    RowCount++;
                                    RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
                                }
                                else
                                {
                                    ColumnCount++;
                                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                                }
                            }
                        }

                        indx = i;
                        if (!(indx < m_arRadioButtonBlock.Length))
                            //indx += (int)(indx / RowCount);

                            row = indx / RowCount;
                        col = indx % (RowCount - 0);

                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Controls.Add(m_arRadioButtonBlock[i], col, row)));
                            Invoke(new Action(() => AutoScroll = true));
                        }
                        else
                            Controls.Add(m_arRadioButtonBlock[i], col, row);
                    }
                }

                public void AddItems(List<CheckState>[] arGroupHeaderCheckStates)
                {
                    m_arGroupHeaderCheckStates = new List<CheckState>[arGroupHeaderCheckStates.Length];

                    for (int i = 0; i < m_arGroupHeaderCheckStates.Length; i++) {
                        m_arGroupHeaderCheckStates[i] = new List<CheckState>(arGroupHeaderCheckStates[i]);
                    }
                }

                /// <summary>
                /// Обработчик события - переключение блока(ТГ)
                /// </summary>
                /// <param name="sender">Объект, инициатор события (??? TableLayoutPanel)</param>
                /// <param name="e">Аргумент события (не используется)</param>
                public void TableLayoutPanelkVed_CheckedChanged(object sender, EventArgs e)
                {
                    int id = SelectedId,
                         indx = (sender as RadioButtonBlock).Index;
                    List<CheckState> _listCheck = new List<CheckState>();
                    PictureBox pictrure;
                    Control cntrl = _getControls(INDEX_CONTROL.CLBX_COL_VISIBLED);

                    if ((sender as RadioButtonBlock).Checked == false)
                    {
                        for (int i = 0; i < (cntrl as CheckedListBoxTaskVedomostBl).Items.Count; i++)
                            _listCheck.Add((cntrl as CheckedListBoxTaskVedomostBl).GetItemCheckState(i));

                        m_arGroupHeaderCheckStates[indx] = _listCheck;
                    }

                    if ((sender as RadioButtonBlock).Checked == true)
                    {
                        pictrure = s_getPicture(id); //GetPictureOfIdComp
                        pictrure.Visible = true;
                        pictrure.Enabled = true;
                        Checked(getListCheckedGroup());
                    }
                }

                /// <summary>
                /// Получение листа с чекедами для каждого из блока
                /// </summary>
                /// <returns>лист с чекедами для групп</returns>
                private List<CheckState> getListCheckedGroup()
                {
                    Control _cntrl = _getControls(INDEX_CONTROL.TBLP_BLK);
                    int indexRb = 0;
                    List<CheckState> _list = new List<CheckState>();

                    try
                    {
                        foreach (RadioButtonBlock rbts in _cntrl.Controls)
                            if (rbts.Checked == true)
                            {
                                _list = m_arGroupHeaderCheckStates[indexRb];
                                break;
                            }
                            else
                                indexRb++;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("???" + "Ошибка формирования списка с чекедами для каждого из блока" + e.ToString());
                    }

                    return _list;
                }

                /// <summary>
                /// Установка состояния элемента 
                /// </summary>
                /// <param name="listCheckState">лист с чекедами для групп</param>
                public void Checked(List<CheckState> listCheckState)
                {
                    Control cntrl = _getControls(INDEX_CONTROL.CLBX_COL_VISIBLED);
                    int indxState = 0;

                    if (listCheckState.Count() > 0)
                        try
                        {
                            if ((cntrl as CheckedListBoxTaskVedomostBl).Items.Count > 0)
                                for (int i = 0; i < (cntrl as CheckedListBoxTaskVedomostBl).Items.Count; i++)
                                {
                                    (cntrl as CheckedListBoxTaskVedomostBl).SetItemCheckState(indxState, listCheckState[indxState]);
                                    indxState++;
                                }
                        }
                        catch (Exception e)
                        {

                        }

                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Controls.Clear();
                }
            }

            ///// <summary>
            ///// Интерфейс для всех элементов управления с компонентами станции, параметрами расчета
            ///// </summary>
            //new private interface IControl
            //{
            //    /// <summary>
            //    /// Идентификатор выбранного элемента списка
            //    /// </summary>
            //    int SelectedId { get; }
            //    ///// <summary>
            //    ///// Добавить элемент в список
            //    ///// </summary>
            //    ///// <param name="text">Текст подписи элемента</param>
            //    ///// <param name="id">Идентификатор элемента</param>
            //    ///// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
            //    //void AddItem(int id, string text, bool bChecked);
            //    /// <summary>
            //    /// Удалить все элементы в списке
            //    /// </summary>
            //    void ClearItems();
            //}

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskVedomostBl : CheckedListBox, IControl
            {
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;
                /// <summary>
                /// Конструктор - основной (без параметров)
                /// </summary>
                public CheckedListBoxTaskVedomostBl()
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
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="text">Текст подписи элемента</param>
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
            }

            /// <summary>
            /// Добавить элемент компонент станции в списки
            ///, в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(int id_comp, string text, List<string> textToolTip, INDEX_ID[] arIndexIdToAdd, bool[] arChecked)
            {
                Control ctrl = null;
                toolTipText[id_comp] = fromationToolTipText(textToolTip);

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskVedomostBl).AddItem(id_comp, text, arChecked[id_comp]);

                    else
                        Logging.Logg().Error(@"PanelManagementTaskVed::AddComponent () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Формирование текста всплывающей подсказки 
            /// для групп
            /// </summary>
            /// <param name="listText">перечень заголовков, входящих в группу</param>
            /// <returns>текст всплывающей подсказки</returns>
            private string fromationToolTipText(List<string> listText)
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
            /// Инициализировать значения для компонента, контролирующего выбор
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            public void AddComponentRadioButton(DataTable tableCompList)
            {
                int indxRadioButton = -1;
                RadioButtonBlock[] arRadioButton;
                Control ctrl = null; // объект для результатов поиска элемента управления
                INDEX_ID indxIdToAdd;

                //инициализация массивов
                arRadioButton = new RadioButtonBlock[tableCompList.Rows.Count];

                //создание списка по блокам
                for (indxRadioButton = 0; indxRadioButton < tableCompList.Rows.Count; indxRadioButton++) {
                    //инициализация радиобаттанов
                    arRadioButton[indxRadioButton] =
                        new PanelManagementVedomostBl.RadioButtonBlock(int.Parse(tableCompList.Rows[indxRadioButton][@"ID"].ToString()));

                    arRadioButton[indxRadioButton].Text = ((string)tableCompList.Rows[indxRadioButton][@"DESCRIPTION"]).Trim();
                    // 1-ый элемент отображать по умолчанию
                    arRadioButton[indxRadioButton].Checked = indxRadioButton == 0;
                }

                indxIdToAdd = INDEX_ID.BLOCK_VISIBLED;
                ctrl = find(indxIdToAdd);

                if (!(ctrl == null))
                    (ctrl as TableLayoutPanelVisibleManagement).AddItems(arRadioButton);
                else
                    Logging.Logg().Error(@"PanelManagementTaskVed::AddComponentRB () - не найден элемент для INDEX_ID=" + indxIdToAdd.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Инициализировать значения для компонента, контролирующего отображение групп столбцов(заголовков)
            ///  для каждого из блоков (??? отобразить все)
            /// </summary>
            /// <param name="tableCompList">Таблица с компонентами ТЭЦ (!!! только блоки)</param>
            public void AddComponentCheckBoxGroupHeaders(DataTable tableCompList)
            {
                Control ctrl = null; // объект для результатов поиска элемента управления
                INDEX_ID indxIdToAdd;
                List<CheckState>[] arListCheckStateGroupHeaders;

                arListCheckStateGroupHeaders = new List<CheckState>[tableCompList.Rows.Count];

                //создание списка по блокам
                for (int indxBlock = 0; indxBlock < tableCompList.Rows.Count; indxBlock++) {
                    arListCheckStateGroupHeaders[indxBlock] = new List<CheckState>(s_listGroupHeaders.Count);

                    for (int indxGroupHeader = 0; indxGroupHeader < s_listGroupHeaders.Count; indxGroupHeader++)
                        arListCheckStateGroupHeaders[indxBlock][indxGroupHeader] = CheckState.Checked;
                }

                indxIdToAdd = INDEX_ID.BLOCK_VISIBLED;
                ctrl = find(indxIdToAdd);

                if (!(ctrl == null))
                    (ctrl as TableLayoutPanelVisibleManagement).AddItems(arListCheckStateGroupHeaders);
                else
                    Logging.Logg().Error(@"PanelManagementTaskVed::AddComponentCheckBoxGroupHeaders () - не найден элемент для INDEX_ID=" + indxIdToAdd.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
            }

            #region Поиск элемента
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
            public Control find(INDEX_CONTROL indxCtrl)
            {
                Control ctrlRes = null;

                ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];

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
                    case INDEX_ID.DENY_COMP_VISIBLED:
                        indxRes = INDEX_CONTROL.CLBX_COL_VISIBLED;
                        break;
                    case INDEX_ID.HGRID_VISIBLE:
                        indxRes = INDEX_CONTROL.CLBX_COL_VISIBLED;
                        break;
                    case INDEX_ID.BLOCK_VISIBLED:
                        indxRes = INDEX_CONTROL.TBLP_BLK;
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
                            indxRes = id == INDEX_CONTROL.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        case INDEX_CONTROL.CLBX_COL_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_COL_VISIBLED ? INDEX_ID.HGRID_VISIBLE : INDEX_ID.UNKNOWN;
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

                if (strId.Equals(INDEX_CONTROL.CLBX_COL_VISIBLED.ToString()) == true)
                    indxRes = INDEX_CONTROL.CLBX_COL_VISIBLED;
                else if (strId.Equals(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString()) == true)
                        indxRes = INDEX_CONTROL.CLBX_COMP_VISIBLED;
                    else
                        throw new Exception(@"PanelTaskVedomostBl::getIndexControl () - не найден объект 'CheckedListBox'...");

                return indxRes;
            }
            #endregion

            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                base.Clear();

                INDEX_ID[] arIndxIdToClear = new INDEX_ID[] { INDEX_ID.DENY_COMP_VISIBLED };

                ActivateCheckedHandler(false, arIndxIdToClear);

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
            }

            /// <summary>
            /// Очистить значения для элемента(ов) управления
            /// </summary>
            /// <param name="indxIdToClear">Индекс идентификатора в списке</param>
            private void clear(INDEX_ID indxIdToClear)
            {
                (find(indxIdToClear) as IControl).ClearItems();
            }

            /// <summary>
            /// (Де)активировать обработчик события
            /// </summary>
            /// <param name="bActive">Признак (де)активации</param>
            /// <param name="arIdToActivate">Массив индексов в списке идентификаторов</param>
            public void ActivateCheckedHandler(bool bActive, INDEX_ID[] arIdToActivate)
            {
                for (int i = 0; i < arIdToActivate.Length; i++)
                    activateCheckedHandler(bActive, arIdToActivate[i]);
            }

            /// <summary>
            /// событие активации
            /// </summary>
            /// <param name="bActive"></param>
            /// <param name="idToActivate"></param>
            protected virtual void activateCheckedHandler(bool bActive, INDEX_ID idToActivate)
            {
                INDEX_CONTROL indxCtrl = INDEX_CONTROL.UNKNOWN;
                CheckedListBox clbx = null;

                indxCtrl = getIndexControlOfIndexID(idToActivate);

                if (!(indxCtrl == INDEX_CONTROL.UNKNOWN))
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
                CheckedListBox clbx = (Controls.Find(INDEX_CONTROL.CLBX_COL_VISIBLED.ToString(), true)[0] as CheckedListBox);

                itemCheck((obj as IControl).SelectedId, getIndexIdOfControl(obj as Control), ev.NewValue);

                //if (clbx.CheckedItems.Count == 0)
                //    clbx.SetItemChecked((obj as IControl).SelectedId + 1 == clbx.Items.Count ? 0 : (obj as IControl).SelectedId + 1, true);
                //else if (clbx.CheckedItems.Count == 1)
                //    if (clbx.CheckedItems.Contains(clbx.CheckedItems[(obj as IControl).SelectedId]))
                //        ;
            }

            /// <summary>
            /// Инициировать событие - изменение признака элемента
            /// </summary>
            /// <param name="address">Адрес элемента</param>
            /// <param name="checkState">Значение признака элемента</param>
            protected void itemCheck(int idItem, INDEX_ID indxId, CheckState checkState)
            {
                ItemCheck(new ItemCheckedParametersEventArgs(idItem, (int)indxId, checkState));
            }
        }

    }
}
