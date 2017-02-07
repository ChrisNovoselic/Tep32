using HClassLibrary;
using InterfacePlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TepCommon;
using Excel = Microsoft.Office.Interop.Excel;

namespace PluginTaskVedomostBl
{
    public class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// ??? переменная с текущем отклоненеим от UTC
        /// </summary>
        static int s_currentOffSet;
        /// <summary>
        /// Для обозначения выбора 1 или 6 блоков
        /// </summary>
        static bool s_flagBl = true;
        /// <summary>
        /// ??? Делегат (возврат пикчи по Ид)
        /// </summary>
        /// <param name="id">ид грида</param>
        /// <returns>picture</returns>
        public delegate PictureBox DelgetPictureOfIdComp(int id);
        /// <summary>
        /// ??? Делегат 
        /// </summary>
        /// <returns>грид</returns>
        public delegate DataGridView DelgetDataGridViewActivate();
        /// <summary>
        /// ??? экземпляр делегата(возврат пикчи по Ид)
        /// </summary>
        static public DelgetPictureOfIdComp s_getPicture;
        /// <summary>
        /// ??? экземпляр делегата(возврат отображения активного)
        /// </summary>
        static public DelgetDataGridViewActivate s_getDGV;
        /// <summary>
        /// ??? экземпляр делегата(возврат Ид)
        /// </summary>
        static public IntDelegateFunc s_getIdComp;
        /// <summary>
        /// флаг очистки отображения
        /// </summary>
        private bool m_bflgClear = false;
        ///// <summary>
        ///// 
        ///// </summary>
        //public static DateTime s_dtDefaultAU = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        /// <summary>
        /// Массив словарей для составления хидеров каждого блока(ТГ)
        /// </summary>
        protected Dictionary<int, List<string[]>> m_dict;
        /// <summary>
        /// Листы с хидерами грида
        /// </summary>
        protected static List<string> s_listGroupSett_1 = new List<string>
        {
         "Острый пар", "Горячий промперегрев", "ХПП"
        };
        protected static List<string> s_listGroupSett_2 = new List<string>
        {
         "Питательная вода","Продувка", "Конденсатор", "Холодный воздух"
         , "Горячий воздух", "Кислород", "VI отбор", "VII отбор"
        };
        protected static List<string> s_listGroupSett_3 = new List<string>
        {
          "Уходящие газы","","" ,"","РОУ", "Сетевая вода", "Выхлоп ЦНД"
        };
        /// <summary>
        /// Лист с группами хидеров отображения
        /// </summary>
        protected static List<List<string>> s_listHeader = new List<List<string>> { s_listGroupSett_1, s_listGroupSett_2, s_listGroupSett_3 };
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1,
            DGV_DATA_B1, DGV_DATA_B2, DGV_DATA_B3,
            DGV_DATA_B4, DGV_DATA_B5, DGV_DATA_B6,
            RADIOBTN_BLK1, RADIOBTN_BLK2, RADIOBTN_BLK3,
            RADIOBTN_BLK4, RADIOBTN_BLK5, RADIOBTN_BLK6,
            LABEL_DESC, TBLP_HGRID, PICTURE_BOXDGV, PANEL_PICTUREDGV,
            COUNT
        }
        /// <summary>
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Индексы массива списков идентификаторов
        /// </summary>
        protected enum INDEX_ID
        {
            UNKNOWN = -1,
            PERIOD, // идентификаторы периодов расчетов, использующихся на форме
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров
            //DENY_COMP_CALCULATED, 
            DENY_COMP_VISIBLED,
            BLOCK_VISIBLED, HGRID_VISIBLE,
            //DENY_PARAMETER_CALCULATED, // запрещенных для расчета
            //DENY_PARAMETER_VISIBLED // запрещенных для отображения
            COUNT
        }
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable[] m_arTableOrigin
            , m_arTableEdit;
        /// <summary>
        /// Массив списков параметров
        /// </summary>
        protected List<int>[] m_arListIds;
        /// <summary>
        /// 
        /// </summary>
        protected HandlerDbTaskCalculate.TaskCalculate.TYPE Type;
        /// <summary>
        /// Объект для обращения к БД
        /// </summary>
        protected HandlerDbTaskVedomostBlCalculate HandlerDb { get { return m_handlerDb as HandlerDbTaskVedomostBlCalculate; } }
        /// <summary>
        /// Создание панели управления
        /// </summary>
        protected PanelManagementVedomostBl PanelManagement
        {
            get
            {
                if (_panelManagement == null)
                    _panelManagement = createPanelManagement();

                return _panelManagement as PanelManagementVedomostBl;
            }
        }
        /// <summary>
        /// Метод для создания панели с активными объектами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            return new PanelManagementVedomostBl();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskVedomostBlCalculate();
        }
        /// <summary>
        /// экземпляр класса вьюхи
        /// </summary>
        protected DGVVedomostBl m_dgvVedomst;
        /// <summary>
        /// 
        /// </summary>
        protected ReportExcel m_rptExcel;
        /// <summary>
        /// экземпляр класса пикчи
        /// </summary>
        protected PictureVedBl m_pictureVedBl;
        /// <summary>
        /// экземпляр класса обрабокти данных
        /// </summary>
        static VedomostBlCalculate s_VedCalculate;
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }

        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementVedomostBl : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// подсказка
            /// </summary>
            ToolTip tlTipGrp = new ToolTip();
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
            public delegate Control GetControl(INDEX_CONTROL indx);
            /// <summary>
            /// экземпляр делегата
            /// </summary>
            public static GetControl _getControls;
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
                : base()
            {
                try
                {
                    InitializeComponents();
                    toolTipText = new string[s_listHeader.Count];
                }
                catch (Exception e)
                {
                    MessageBox.Show("???" + e.ToString());
                }

            }

            /// <summary>
            /// 
            /// </summary>
            private void InitializeComponents()
            {
                _getControls = new GetControl(find);
                //ToolTip tlTipHeader = new ToolTip();
                //tlTipHeader.AutoPopDelay = 5000;
                //tlTipHeader.InitialDelay = 1000;
                //tlTipHeader.ReshowDelay = 500;
                Control ctrl = null;

                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                SuspendLayout();

                posRow = 0;
                //Период расчета - подпись, значение
                SetPositionPeriod(new Point(0, posRow), new Size(this.ColumnCount / 2, 1));

                //Часовой пояс расчета - подпись, значение
                SetPositionTimezone(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount / 2, 1));

                //Дата/время начала периода расчета
                posRow = SetPositionDateTimePicker(new Point(0, posRow = posRow + 1), new Size(this.ColumnCount, 4));

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
                ctrl.Dock = DockStyle.Top;
                //Кнопка - сохранить
                Button ctrlBsave = new Button();
                ctrlBsave.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrlBsave.Text = @"Сохранить";
                ctrlBsave.Dock = DockStyle.Top;
                //
                Button ctrlExp = new Button();
                ctrlExp.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrlExp.Text = @"Экспорт";
                ctrlExp.Dock = DockStyle.Top;

                TableLayoutPanel tlpButton = new TableLayoutPanel();
                tlpButton.Dock = DockStyle.Fill;
                tlpButton.AutoSize = true;
                tlpButton.AutoSizeMode = AutoSizeMode.GrowOnly;
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                tlpButton.Controls.Add(ctrl, 0, 0);
                tlpButton.Controls.Add(ctrlBsave, 1, 0);
                tlpButton.Controls.Add(ctrlExp, 0, 2);
                Controls.Add(tlpButton, 0, posRow = posRow + 2);
                SetColumnSpan(tlpButton, 4); SetRowSpan(tlpButton, 2);
                //Признаки включения/исключения для отображения блока(ТГ)
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Выбрать блок для отображения:";
                TableLayoutPanel tlpChk = new TableLayoutPanel();
                tlpChk.Controls.Add(ctrl, 0, 0);
                //
                ctrl = new TableLayoutPanelkVed();
                ctrl.Name = INDEX_CONTROL.TBLP_BLK.ToString();
                ctrl.Dock = DockStyle.Top;
                tlpChk.Controls.Add(ctrl, 0, 1);
                //Признак для включения/исключения для отображения столбца(ов)
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Включить/исключить столбцы для отображения:";
                tlpChk.Controls.Add(ctrl, 0, 2);
                //
                ctrl = new CheckedListBoxTaskVed();
                ctrl.MouseMove += new MouseEventHandler(showCheckBoxToolTip); ;
                ctrl.Name = INDEX_CONTROL.CLBX_COL_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Top;
                (ctrl as CheckedListBoxTaskVed).CheckOnClick = true;
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
                CheckedListBoxTaskVed chkVed = (this.Controls.Find(INDEX_CONTROL.CLBX_COL_VISIBLED.ToString(), true)[0] as CheckedListBoxTaskVed);

                if (toolTipIndex != chkVed.IndexFromPoint(e.Location))
                {
                    toolTipIndex = chkVed.IndexFromPoint(chkVed.PointToClient(MousePosition));
                    if (toolTipIndex > -1)
                    {
                        //Свич по элементам находящимся в чеклистбоксе
                        switch (chkVed.Items[toolTipIndex].ToString())
                        {
                            case "Группа 1":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 2":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
                                break;
                            case "Группа 3":
                                tlTipGrp.SetToolTip(chkVed, toolTipText[toolTipIndex]);
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
            /// Класс кнопки выбора блока
            /// </summary>
            public class RadioButtonBl : RadioButton
            {
                /// <summary>
                /// номер радиобаттона
                /// </summary>
                private int _idRb;
                /// <summary>
                /// номер радиобаттона
                /// </summary>
                public int idRb
                {
                    get { return _idRb; }
                    set { _idRb = value; }
                }

                /// <summary>
                /// Конструктор(основной)
                /// </summary>
                public RadioButtonBl(string nameRbtn) : base()
                {
                    initialize(nameRbtn);
                }
                /// <summary>
                /// Инициализация объекта
                /// </summary>
                /// <param name="nameRbtn"></param>
                private void initialize(string nameRbtn)
                {
                    Name = nameRbtn;
                }
            }

            /// <summary>
            /// Класс для размещения элементов (блоков) выбора отображения значений
            /// </summary>
            protected class TableLayoutPanelkVed : TableLayoutPanel
            {
                /// <summary>
                /// список активных групп хидеров отображения
                /// </summary>
                protected List<CheckState>[] m_arBoolCheck;
                /// <summary>
                /// 
                /// </summary>
                public RadioButtonBl[] arRb;
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                /// <summary>
                /// 
                /// </summary>
                public TableLayoutPanelkVed()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId
                {
                    get
                    {
                        int cnt = 0;

                        foreach (RadioButton rb in Controls)
                        {
                            if (rb.Checked == true)
                                break;
                            else
                                cnt++;
                        }
                        return m_listId[cnt];
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
                public void AddItems(int[] id, string[] text, bool[] bChecked, RadioButtonBl[] rb, List<CheckState> groupCheck)
                {
                    int indx = -1
                       , col = -1
                       , row = -1;

                    RowCount = 1;
                    ColumnCount = 3;
                    RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                    ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

                    m_arBoolCheck = new List<CheckState>[rb.Count()];

                    if (arRb == null)
                        arRb = rb;

                    for (int i = 0; i < arRb.Length; i++)
                    {
                        arRb[i].CheckedChanged += TableLayoutPanelkVed_CheckedChanged;
                        arRb[i].Text = text[i];
                        m_listId.Add(id[i]);
                        m_arBoolCheck[i] = new List<CheckState>();
                        m_arBoolCheck[i] = groupCheck;
                        arRb[i].Checked = bChecked[i];
                        arRb[i].idRb = i;

                        if (RowCount * ColumnCount < arRb.Length)
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
                        if (!(indx < arRb.Length))
                            //indx += (int)(indx / RowCount);

                            row = indx / RowCount;
                        col = indx % (RowCount - 0);

                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Controls.Add(arRb[i], col, row)));
                            Invoke(new Action(() => AutoScroll = true));
                        }
                        else
                            Controls.Add(arRb[i], col, row);
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
                         indx = (sender as RadioButtonBl).idRb;
                    List<CheckState> _listCheck = new List<CheckState>();
                    PictureBox pictrure;
                    Control cntrl = _getControls(INDEX_CONTROL.CLBX_COL_VISIBLED);

                    if ((sender as RadioButtonBl).Checked == false)
                    {
                        for (int i = 0; i < (cntrl as CheckedListBoxTaskVed).Items.Count; i++)
                            _listCheck.Add((cntrl as CheckedListBoxTaskVed).GetItemCheckState(i));

                        m_arBoolCheck[indx] = _listCheck;
                    }

                    if ((sender as RadioButtonBl).Checked == true)
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
                        foreach (RadioButtonBl rbts in _cntrl.Controls)
                            if (rbts.Checked == true)
                            {
                                _list = m_arBoolCheck[indexRb];
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
                            if ((cntrl as CheckedListBoxTaskVed).Items.Count > 0)
                                for (int i = 0; i < (cntrl as CheckedListBoxTaskVed).Items.Count; i++)
                                {
                                    (cntrl as CheckedListBoxTaskVed).SetItemCheckState(indxState, listCheckState[indxState]);
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
                    m_listId.Clear();
                }
            }

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskVed : CheckedListBox, IControl
            {
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;
                /// <summary>
                /// 
                /// </summary>
                public CheckedListBoxTaskVed()
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

                /// <summary>
                /// Возвращает имя итема
                /// </summary>
                /// <param name="id">ИдИтема</param>
                /// <returns>имя итема</returns>
                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }
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
                        (ctrl as CheckedListBoxTaskVed).AddItem(id_comp, text, arChecked[id_comp]);

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

                foreach (var item in listText)
                {
                    if (strTextToolTip != string.Empty)
                        if (item != "")
                            strTextToolTip += ", ";
                    strTextToolTip += item;
                }

                return strTextToolTip;
            }

            /// <summary>
            /// Добавить элемент компонент станции в списки
            /// , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponentRB(int[] id_comp,
                string[] text,
                INDEX_ID[] arIndexIdToAdd,
                bool[] arChecked,
                RadioButtonBl[] rb
                , List<CheckState> checkedGroup)
            {
                Control ctrl = null;

                for (int i = 0; i < arIndexIdToAdd.Length; i++)
                {
                    ctrl = find(arIndexIdToAdd[i]);

                    if (!(ctrl == null))
                        (ctrl as TableLayoutPanelkVed).AddItems(id_comp, text, arChecked, rb, checkedGroup);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskVed::AddComponentRB () - не найден элемент для INDEX_ID=" + arIndexIdToAdd[i].ToString(), Logging.INDEX_MESSAGE.NOT_SET);
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
            /// Получение ИД контрола
            /// </summary>
            /// <param name="ctrl">контрол</param>
            /// <returns>индекс</returns>
            protected INDEX_ID getIndexIdOfControl(Control ctrl)
            {
                INDEX_CONTROL id = INDEX_CONTROL.UNKNOWN; //Индекс (по сути - идентификатор) элемента управления, инициировавшего событие
                INDEX_ID indxRes = INDEX_ID.UNKNOWN;

                try
                {
                    //Определить идентификатор
                    id = getIndexControl(ctrl);
                    // , соответствующий изменившему состояние элементу 'CheckedListBox'
                    switch (id)
                    {
                        case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_COMP_VISIBLED ? INDEX_ID.DENY_COMP_VISIBLED : INDEX_ID.UNKNOWN;
                            break;
                        case INDEX_CONTROL.CLBX_COL_VISIBLED:
                            indxRes = id == INDEX_CONTROL.CLBX_COL_VISIBLED ? INDEX_ID.HGRID_VISIBLE : INDEX_ID.UNKNOWN;
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

        /// <summary>
        /// класс пикчи
        /// </summary>
        protected class PictureVedBl : PictureBox
        {
            /// <summary>
            /// ид Пикчи
            /// </summary>
            public int m_idCompPicture;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="viewActive">активный грид</param>
            public PictureVedBl(DGVVedomostBl viewActive)
            {
                InitializeComponents(viewActive);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="viewActive">активный грид</param>
            private void InitializeComponents(DGVVedomostBl viewActive)
            {
                int _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

                Size = new Size(viewActive.Width - 10, _drwH);
                m_idCompPicture = viewActive.m_idCompDGV;
                Controls.Add(viewActive);
            }
        }

        /// <summary>
        /// класс вьюхи
        /// </summary>
        protected class DGVVedomostBl : DataGridView
        {
            /// <summary>
            /// ширина и высота
            /// </summary>
            static int s_drwW,
                s_drwH = s_listHeader.Count;
            /// <summary>
            /// 
            /// </summary>
            Rectangle recParentCol;
            /// <summary>
            /// словарь названий заголовков 
            /// верхнего и среднего уровней
            /// </summary>
            public Dictionary<int, List<string>> m_headerTop = new Dictionary<int, List<string>>(),
                m_headerMiddle = new Dictionary<int, List<string>>();
            /// <summary>
            /// словарь соотношения заголовков
            /// </summary>
            public Dictionary<int, int[]> m_arIntTopHeader = new Dictionary<int, int[]> { },
            m_arMiddleCol = new Dictionary<int, int[]> { };
            /// <summary>
            /// перечисление уровней заголовка грида
            /// </summary>
            public enum INDEX_HEADER
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }
            /// <summary>
            /// ИдГрида
            /// </summary>
            private int _idCompDGV;
            private int _CountBL;
            /// <summary>
            /// 
            /// </summary>
            public int m_CountBL
            {
                get { return _CountBL; }
                set { _CountBL = value; }
            }
            /// <summary>
            /// ИдГрида
            /// </summary>
            public int m_idCompDGV
            {
                get { return _idCompDGV; }
                set { _idCompDGV = value; }
            }
            /// <summary>
            /// Перечисление для индексации столбцов со служебной информацией
            /// </summary>
            public enum INDEX_SERVICE_COLUMN : uint { ALG = 0, DATE, COUNT }
            /// <summary>
            /// словарь настроечных данных
            /// </summary>
            private Dictionary<int, ROW_PROPERTY> m_dictPropertiesRows;
            private Dictionary<int, COLUMN_PROPERTY> m_dictPropertyColumns;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="nameDGV">имя грида</param>
            public DGVVedomostBl(string nameDGV)
            {
                InitializeComponents(nameDGV);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="nameDGV">имя окна отображения данных</param>
            private void InitializeComponents(string nameDGV)
            {
                Name = nameDGV;
                Dock = DockStyle.None;
                //Запретить выделение "много" строк
                MultiSelect = false;
                //Установить режим выделения - "полная" строка
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //Установить режим "невидимые" заголовки столбцов
                ColumnHeadersVisible = true;
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
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                AllowUserToResizeColumns = false;
                ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
                ColumnHeadersHeight = ColumnHeadersHeight * s_drwH;//высота от нижнего(headerText)
                ScrollBars = ScrollBars.None;

                AddColumns(-2, "ALG", string.Empty, false);
                AddColumns(-1, "Date", "Дата", true);
            }

            /// <summary>
            /// Класс для описания дополнительных свойств столбца в отображении (таблице)
            /// </summary>
            private class HDataGridViewColumn : DataGridViewTextBoxColumn
            {
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_IdAlg;
                /// <summary>
                /// Идентификатор компонента
                /// </summary>
                public int m_IdComp;
                /// <summary>
                /// Признак запрета участия в расчете
                /// </summary>
                public bool m_bCalcDeny;
                /// <summary>
                /// признак общей группы
                /// </summary>
                public string m_topHeader;
            }

            /// <summary>
            /// Структура для описания добавляемых строк
            /// </summary>
            public class ROW_PROPERTY
            {
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { IS_NAN }
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="idParameter"></param>
                    /// <param name="iQuality"></param>
                    public HDataGridViewCell(int idParameter, TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }

                /// <summary>
                /// Пояснения к параметру в алгоритме расчета
                /// </summary>
                public string m_strMeasure
                    , m_Value;
                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в строке
                /// </summary>
                public int m_vsRound;

                public HDataGridViewCell[] m_arPropertiesCells;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="cntCols"></param>
                public void InitCells(int cntCols)
                {
                    m_arPropertiesCells = new HDataGridViewCell[cntCols];
                    for (int c = 0; c < m_arPropertiesCells.Length; c++)
                        m_arPropertiesCells[c] = new HDataGridViewCell(-1, HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT);
                }
            }

            /// <summary>
            /// Структура для описания добавляемых столбцов
            /// </summary>
            public class COLUMN_PROPERTY
            {
                /// <summary>
                /// Структура с дополнительными свойствами ячейки отображения
                /// </summary>
                public struct HDataGridViewCell //: DataGridViewCell
                {
                    public enum INDEX_CELL_PROPERTY : uint { IS_NAN }
                    /// <summary>
                    /// Признак отсутствия значения
                    /// </summary>
                    public int m_IdParameter;
                    /// <summary>
                    /// Признак качества значения в ячейке
                    /// </summary>
                    public TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE m_iQuality;

                    public HDataGridViewCell(int idParameter, HandlerDbTaskCalculate.ID_QUALITY_VALUE iQuality)
                    {
                        m_IdParameter = idParameter;
                        m_iQuality = iQuality;
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    public bool IsNaN { get { return m_IdParameter < 0; } }
                }

                /// <summary>
                /// Идентификатор параметра в алгоритме расчета
                /// </summary>
                public int m_idAlg;
                /// <summary>
                /// признак агрегации
                /// </summary>
                public int m_Avg;
                /// <summary>
                /// Идентификатор множителя при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRatio;
                /// <summary>
                /// Количество знаков после запятой при отображении (визуальные установки) значений в столбце
                /// </summary>
                public int m_vsRound;
                /// <summary>
                /// Имя колонки
                /// </summary>
                public string nameCol;
                /// <summary>
                /// Текст в колонке
                /// </summary>
                public string hdrText;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public string topHeader;
                /// <summary>
                /// Имя общей группы колонки
                /// </summary>
                public int m_IdComp;
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="nameCol">имя колонки</param>
            /// <param name="headerText">текст заголовка</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, string nameCol, string headerText, bool bVisible)
            {
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    HDataGridViewColumn column = new HDataGridViewColumn() { m_IdAlg = idHeader, m_bCalcDeny = false };
                    alignText = DataGridViewContentAlignment.MiddleRight;
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    column.Frozen = true;
                    column.Visible = bVisible;
                    column.ReadOnly = false;
                    column.Name = nameCol;
                    column.HeaderText = headerText;
                    column.DefaultCellStyle.Alignment = alignText;
                    //column.AutoSizeMode = autoSzColMode;
                    Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DGVVedBl::AddColumn () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Добавление колонки
            /// </summary>
            /// <param name="idHeader">номер колонки</param>
            /// <param name="col_prop">Структура для описания добавляемых столбцов</param>
            /// <param name="bVisible">видимость</param>
            public void AddColumns(int idHeader, COLUMN_PROPERTY col_prop, bool bVisible)
            {
                int indxCol = -1; // индекс столбца при вставке
                DataGridViewContentAlignment alignText = DataGridViewContentAlignment.NotSet;

                try
                {
                    if (m_dictPropertyColumns == null)
                        m_dictPropertyColumns = new Dictionary<int, COLUMN_PROPERTY>();

                    if (!m_dictPropertyColumns.ContainsKey(col_prop.m_idAlg))
                        m_dictPropertyColumns.Add(col_prop.m_idAlg, col_prop);
                    // найти индекс нового столбца
                    // столбец для станции - всегда крайний
                    //foreach (HDataGridViewColumn col in Columns)
                    //    if ((col.m_iIdComp > 0)
                    //        && (col.m_iIdComp < 1000))
                    //    {
                    //        indxCol = Columns.IndexOf(col);
                    //        break;
                    //    }

                    HDataGridViewColumn column = new HDataGridViewColumn() { m_bCalcDeny = false, m_topHeader = col_prop.topHeader, m_IdAlg = idHeader, m_IdComp = col_prop.m_IdComp };
                    alignText = DataGridViewContentAlignment.MiddleRight;

                    if (!(indxCol < 0))// для вставляемых столбцов (компонентов ТЭЦ)
                        ; // оставить значения по умолчанию
                    else
                    {// для добавлямых столбцов
                        //if (idHeader < 0)
                        //{// для служебных столбцов
                        if (bVisible == true)
                        {// только для столбца с [SYMBOL]
                            alignText = DataGridViewContentAlignment.MiddleLeft;
                        }
                        column.Frozen = true;
                        column.ReadOnly = true;
                        //}
                    }

                    column.HeaderText = col_prop.hdrText;
                    column.Name = col_prop.nameCol;
                    column.DefaultCellStyle.Alignment = alignText;
                    column.Visible = bVisible;

                    if (!(indxCol < 0))
                        Columns.Insert(indxCol, column as DataGridViewTextBoxColumn);
                    else
                        Columns.Add(column as DataGridViewTextBoxColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewVedBl::AddColumn (idHeader=" + idHeader + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Удаление набора строк
            /// </summary>
            public void ClearRows()
            {
                if (Rows.Count > 0)
                    Rows.Clear();
            }

            /// <summary>
            /// Очищение отображения от значений
            /// </summary>
            public void ClearValues()
            {
                //CellValueChanged -= onCellValueChanged;

                foreach (DataGridViewRow r in Rows)
                    foreach (DataGridViewCell c in r.Cells)
                        if (r.Cells.IndexOf(c) > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) // нельзя удалять идентификатор параметра
                            c.Value = string.Empty;

                //??? если установить 'true' - редактирование невозможно
                //ReadOnly = false;

                //CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="rowProp">структура строк</param>
            public void AddRow(ROW_PROPERTY rowProp)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

                if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
                    m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);
                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.ALG].Value = rowProp.m_idAlg;
                // инициализировать значения в служебных ячейках
                m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);
            }

            /// <summary>
            /// Добавить строку в таблицу
            /// </summary>
            /// <param name="rowProp">структура строк</param>
            /// <param name="DaysInMonth">кол-во дней в месяце</param>
            public void AddRow(ROW_PROPERTY rowProp, int DaysInMonth)
            {
                int i = -1;
                // создать строку
                DataGridViewRow row = new DataGridViewRow();
                if (m_dictPropertiesRows == null)
                    m_dictPropertiesRows = new Dictionary<int, ROW_PROPERTY>();

                if (!m_dictPropertiesRows.ContainsKey(rowProp.m_idAlg))
                    m_dictPropertiesRows.Add(rowProp.m_idAlg, rowProp);

                // добавить строку
                i = Rows.Add(row);
                // установить значения в ячейках для служебной информации
                Rows[i].Cells[(int)INDEX_SERVICE_COLUMN.DATE].Value = rowProp.m_Value;
                // инициализировать значения в служебных ячейках
                //m_dictPropertiesRows[rowProp.m_idAlg].InitCells(Columns.Count);

                if (i == DaysInMonth)
                    foreach (HDataGridViewColumn col in Columns)
                        Rows[i].Cells[col.Index].ReadOnly = true;//блокировка строк
            }

            /// <summary>
            /// Установка возможности редактирования столбцов
            /// </summary>
            /// <param name="bRead">true/false</param>
            /// <param name="nameCol">имя стобца</param>
            public void AddBRead(bool bRead)
            {
                foreach (HDataGridViewColumn col in Columns)
                    //if (col.Name == nameCol)
                    col.ReadOnly = bRead;
            }

            /// <summary>
            /// 
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
            /// 
            /// </summary>
            protected Dictionary<int, RATIO> m_dictRatio;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tblRatio">таблица параметров</param>
            public void SetRatio(DataTable tblRatio)
            {
                m_dictRatio = new Dictionary<int, RATIO>();

                foreach (DataRow r in tblRatio.Rows)
                    m_dictRatio.Add((int)r[@"ID"], new RATIO()
                    {
                        m_id = (int)r[@"ID"]
                        ,
                        m_value = (int)r[@"VALUE"]
                        ,
                        m_nameRU = (string)r[@"NAME_RU"]
                        ,
                        m_nameEN = (string)r[@"NAME_RU"]
                        ,
                        m_strDesc = (string)r[@"DESCRIPTION"]
                    });
            }

            /// <summary>
            /// Подготовка параметров к рисовке хидера
            /// </summary>
            /// <param name="dgv">активное окно отображения данных</param>
            public void dgvConfigCol(DataGridView dgv)
            {
                int cntCol = 0;
                formingTitleLists((dgv as DGVVedomostBl).m_idCompDGV);

                formRelationsHeading((dgv as DGVVedomostBl).m_idCompDGV);

                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible == true)
                        cntCol++;

                s_drwW = cntCol * dgv.Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width +
                    dgv.Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width / s_listHeader.Count;

                dgv.Paint += new PaintEventHandler(dataGridView1_Paint);
            }

            /// <summary>
            /// Формирование списков заголовков
            /// </summary>
            /// <param name="idTG">номер идТГ</param>
            private void formingTitleLists(int idTG)
            {
                string _oldItem = string.Empty;
                List<string> _listTop = new List<string>(),
                    _listMiddle = new List<string>();

                if (m_headerTop.ContainsKey(idTG))
                    m_headerTop.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_IdAlg >= 0)
                        if (col.Visible == true)
                            if (col.m_topHeader != "")
                                if (col.m_topHeader != _oldItem)
                                {
                                    _oldItem = col.m_topHeader;
                                    _listTop.Add(col.m_topHeader);
                                }
                                else;
                            else
                                _listTop.Add(col.m_topHeader);
                        else;
                    else;

                m_headerTop.Add(idTG, _listTop);

                if (m_headerMiddle.ContainsKey(idTG))
                    m_headerMiddle.Remove(idTG);

                foreach (HDataGridViewColumn col in Columns)
                    if (col.m_IdAlg >= 0)
                        if (col.Visible == true)
                            if (col.Name != _oldItem)
                            {
                                _oldItem = col.Name;
                                _listMiddle.Add(col.Name);
                            }

                m_headerMiddle.Add(idTG, _listMiddle);
            }

            /// <summary>
            /// Формирвоанеи списка отношения 
            /// кол-во верхних заголовков к нижним
            /// </summary>
            /// <param name="idDgv">номер окна отображения</param>
            private void formRelationsHeading(int idDgv)
            {
                string _oldItem = string.Empty;
                int _indx = 0,
                    _untdColM = 0;
                int[] _arrIntTop = new int[m_headerTop[idDgv].Count()],
                    _arrIntMiddle = new int[m_headerMiddle[idDgv].Count()];

                if (m_arIntTopHeader.ContainsKey(idDgv))
                    m_arIntTopHeader.Remove(idDgv);

                foreach (var item in m_headerTop[idDgv])
                {
                    int untdCol = 0;
                    foreach (HDataGridViewColumn col in Columns)
                        if (col.Visible == true)
                            if (col.m_topHeader == item)
                                if (!(item == ""))
                                    untdCol++;
                                else
                                {
                                    untdCol = 1;
                                    break;
                                }
                    _arrIntTop[_indx] = untdCol;
                    _indx++;
                }

                m_arIntTopHeader.Add(idDgv, _arrIntTop);
                _indx = 0;

                if (m_arMiddleCol.ContainsKey(idDgv))
                    m_arMiddleCol.Remove(idDgv);

                foreach (var item in m_headerMiddle[idDgv])
                {
                    foreach (HDataGridViewColumn col in Columns)
                    {
                        if (col.m_IdAlg > -1)
                            if (item == col.Name)
                                _untdColM++;
                            else
                                if (_untdColM > 0)
                                break;
                    }
                    _arrIntMiddle[_indx] = _untdColM;
                    _indx++;
                    _untdColM = 0;
                }
                m_arMiddleCol.Add(idDgv, _arrIntMiddle);
            }

            /// <summary>
            /// Скрыть/показать столбцы из списка групп
            /// </summary>
            /// <param name="dgvActive">активное окно отображения данных</param>
            /// <param name="listHeaderTop">лист с именами заголовков</param>
            /// <param name="isCheck">проверка чека</param>
            public void HideColumns(DataGridView dgv, List<string> listHeaderTop, bool isCheck)
            {
                try
                {
                    foreach (var item in listHeaderTop)
                        foreach (HDataGridViewColumn col in Columns)
                            if (col.m_topHeader == item)
                                if (isCheck)
                                    col.Visible = true;
                                else
                                    col.Visible = false;
                }
                catch (Exception)
                {

                }


                dgvConfigCol(dgv);
            }

            /// <summary>
            /// обработчик события перерисовки грида(построение шапки заголовка)
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            void dataGridView1_Paint(object sender, PaintEventArgs e)
            {
                int _indxCol = 0;
                Rectangle _r1 = new Rectangle();
                Rectangle _r2 = new Rectangle();
                Pen pen = new Pen(Color.Black);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                s_drwH = 3;
                //
                for (int i = 0; i < Columns.Count; i++)
                    if (GetCellDisplayRectangle(i, -1, true).Height > 0 & GetCellDisplayRectangle(i, -1, true).X > 0)
                    {
                        recParentCol = GetCellDisplayRectangle(i, -1, true);
                        _r1 = recParentCol;
                        _r2 = recParentCol;
                        break;
                    }

                s_drwH = _r1.Height / s_drwH;

                foreach (var item in m_headerMiddle[(sender as DGVVedomostBl).m_idCompDGV])
                {
                    //get the column header cell
                    _r1.Width = m_arMiddleCol[(sender as DGVVedomostBl).m_idCompDGV][m_headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item)]
                        * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    _r1.Height = s_drwH + 3;//??? 

                    if (m_headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item) - 1 > -1)
                        _r1.X = _r1.X + m_arMiddleCol[(sender as DGVVedomostBl).m_idCompDGV][m_headerMiddle[(sender as DGVVedomostBl).m_idCompDGV].ToList().IndexOf(item) - 1]
                            * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        _r1.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                        _r1.Y = _r1.Y + _r1.Height;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), _r1);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      _r1,
                      format);
                    e.Graphics.DrawRectangle(pen, _r1);
                }

                foreach (var item in m_headerTop[(sender as DGVVedomostBl).m_idCompDGV])
                {
                    //get the column header cell
                    _r2.Width = m_arIntTopHeader[(sender as DGVVedomostBl).m_idCompDGV][_indxCol] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    _r2.Height = s_drwH + 2;//??? 

                    if (_indxCol - 1 > -1)
                        _r2.X = _r2.X + m_arIntTopHeader[(sender as DGVVedomostBl).m_idCompDGV][_indxCol - 1] * Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                    else
                    {
                        _r2.X += Columns[(int)INDEX_SERVICE_COLUMN.COUNT].Width;
                        _r2.Y += _r2.Y;
                    }

                    e.Graphics.FillRectangle(new SolidBrush(ColumnHeadersDefaultCellStyle.BackColor), _r2);
                    e.Graphics.DrawString(item, ColumnHeadersDefaultCellStyle.Font,
                      new SolidBrush(ColumnHeadersDefaultCellStyle.ForeColor),
                      _r2,
                      format);
                    e.Graphics.DrawRectangle(pen, _r2);
                    _indxCol++;
                }

                //(sender as DGVVedomostBl).Paint -= new PaintEventHandler(dataGridView1_Paint);
            }

            /// <summary>
            /// обработчик события - перерисовки ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            static void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
            {
                if (e.RowIndex == -1 && e.ColumnIndex > -1)
                {
                    e.PaintBackground(e.CellBounds, false);

                    Rectangle r2 = e.CellBounds;
                    r2.Y += e.CellBounds.Height / s_drwH;
                    r2.Height = e.CellBounds.Height / s_drwH;
                    e.PaintContent(r2);
                    e.Handled = true;
                }
            }

            /// <summary>
            /// Отображение данных на вьюхе
            /// </summary>
            /// <param name="tableOrigin">таблица с данными</param>
            /// <param name="typeValues">тип загружаемых данных</param>
            public void ShowValues(DataTable tableOrigin, DataTable tableInParameter, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                DataTable _dtOriginVal = new DataTable(),
                    _dtEditVal = new DataTable();
                int idAlg = -1
                   , idParameter = -1
                   , _hoursOffSet
                   , iCol = 0
                   , _vsRatioValue = -1;
                double dblVal = -1F;

                DataRow[] parameterRows = null,
                    editRow = null;

                _dtOriginVal = tableOrigin.Copy();
                ClearValues();

                if ((int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE == (int)typeValues)
                    if (s_flagBl)
                        _hoursOffSet = -1 * (-(TimeZoneInfo.Local.BaseUtcOffset.Hours + 1) + 24);
                    else
                        _hoursOffSet = (s_currentOffSet / 60);
                else
                    _hoursOffSet = s_currentOffSet / 60;

                if (_dtOriginVal.Rows.Count > 0)
                    foreach (HDataGridViewColumn col in Columns) {
                        if (iCol > ((int)INDEX_SERVICE_COLUMN.COUNT - 1)) {
                            try {
                                parameterRows = tableInParameter.Select(string.Format(tableInParameter.Locale
                                    , "ID_ALG = " + col.m_IdAlg + " AND ID_COMP = " + m_idCompDGV));
                                editRow = _dtOriginVal.Select(string.Format(_dtOriginVal.Locale, "ID_PUT = " + (int)parameterRows[0]["ID"]));
                            } catch (Exception) {
                                MessageBox.Show("???" + "Ошибка выборки данных!");
                            }

                            for (int i = 0; i < editRow.Count(); i++) {
                                _vsRatioValue = m_dictPropertyColumns[col.m_IdAlg].m_vsRatio;

                                if (Convert.ToDateTime(editRow[i][@"WR_DATETIME"]).AddHours(_hoursOffSet).ToShortDateString() ==
                                    Rows[i].Cells["Date"].Value.ToString()) {
                                    Rows[i].Cells[iCol].Value =
                                        (((double)editRow[i][@"VALUE"]).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound,
                                            CultureInfo.InvariantCulture));
                                } else
                                    ;
                            }

                            try {
                                if (m_dictPropertyColumns[col.m_IdAlg].m_Avg == 0)
                                    Rows[RowCount - 1].Cells[iCol].Value =
                                        sumVal(_dtEditVal, col.Index).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound, CultureInfo.InvariantCulture);
                                else
                                    Rows[RowCount - 1].Cells[iCol].Value =
                                        avgVal(_dtEditVal, col.Index).ToString(@"F" + m_dictPropertyColumns[col.m_IdAlg].m_vsRound, CultureInfo.InvariantCulture);
                            } catch (Exception exp) {
                                MessageBox.Show("???" + "Ошибка усредненния данных по столбцу " + col.m_topHeader + "! " + exp.ToString());
                            }
                        } else
                            ;

                        iCol++;
                    }
            }

            /// <summary>
            /// Получение суммы по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>сумма по столбцу</returns>
            private double sumVal(DataTable table, int indxCol)
            {
                double _sumValue = 0F;

                try
                {
                    //foreach (DataRow item in table.Rows)
                    //{
                    //    if (Rows.Count - 1 != table.Rows.IndexOf(item))
                    //        _sumValue += s_VedCalculate.AsParseToF(item[indxCol].ToString());
                    //}
                    foreach (DataGridViewRow row in Rows)
                        if (Rows.Count - 1 != row.Index)
                            if (row.Cells[indxCol].Value != null)
                                if (row.Cells[indxCol].Value.ToString() != "")
                                    _sumValue += s_VedCalculate.AsParseToF(row.Cells[indxCol].Value.ToString());
                }
                catch (Exception e)
                {
                    MessageBox.Show("???" + "Ошибка суммирования столбца!");
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::sumVal () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return _sumValue;
            }

            /// <summary>
            /// Получение среднего по столбцу
            /// </summary>
            /// <param name="indxCol">индекс столбца</param>
            /// <returns>среднее по столбцу</returns>
            private double avgVal(DataTable table, int indxCol)
            {
                int cntNum = 0;
                double _avgValue = 0F
                   , _sumValue = 0F;

                try
                {
                    //foreach (DataRow item in table.Rows)
                    //{
                    //    if (Rows.Count - 1 != table.Rows.IndexOf(item))
                    //    {
                    //        _sumValue += s_VedCalculate.AsParseToF(item[indxCol].ToString());
                    //        cntNum++;
                    //    }
                    //}

                    foreach (DataGridViewRow row in Rows)
                        if (row.Cells[indxCol].Value != null)
                            if (row.Cells[indxCol].Value.ToString() != "")
                            {
                                _sumValue += s_VedCalculate.AsParseToF(row.Cells[indxCol].Value.ToString());
                                cntNum++;
                            }
                }
                catch (Exception exp)
                {
                    MessageBox.Show("???" + "Ошибка усреднения столбца!");
                    Logging.Logg().Exception(exp, @"PanelTaskVedomostBl::avgVal () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return _avgValue = _sumValue / cntNum;
            }

            /// <summary>
            /// Формирование таблицы данных с отображения
            /// </summary>
            /// <param name="dtSourceOrg">таблица с оригинальными данными</param>
            /// <param name="idSession">номер сессии пользователя</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>таблица с новыми данными с вьюхи</returns>
            public DataTable FillTableToSave(DataTable dtSourceOrg, int idSession, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int i = 0,
                    idAlg = -1
                    , _hoursOffSet
                    , vsRatioValue = -1
                    , quality = 0,
                    indexPut = 0;
                double valueToRes = 0;
                DateTime dtVal;

                DataTable dtSourceEdit = new DataTable();
                dtSourceEdit.Columns.AddRange(new DataColumn[] {
                        new DataColumn (@"ID_PUT", typeof (int))
                        , new DataColumn (@"ID_SESSION", typeof (long))
                        , new DataColumn (@"QUALITY", typeof (int))
                        , new DataColumn (@"VALUE", typeof (float))
                        , new DataColumn (@"WR_DATETIME", typeof (DateTime))
                        , new DataColumn (@"EXTENDED_DEFINITION", typeof (float))
                    });

                if (s_flagBl)
                    _hoursOffSet = 1 * (-(TimeZoneInfo.Local.BaseUtcOffset.Hours + 1) + 24);
                else
                    _hoursOffSet = (s_currentOffSet / 60);

                foreach (HDataGridViewColumn col in Columns)
                {
                    if (col.m_IdAlg > 0)
                    {
                        foreach (DataGridViewRow row in Rows)
                        {
                            if (row.Index != row.DataGridView.RowCount - 1)
                                if (row.Cells[col.Index].Value != null)
                                    if (row.Cells[col.Index].Value.ToString() != "")
                                    {
                                        idAlg = col.m_IdAlg;
                                        valueToRes = s_VedCalculate.AsParseToF(row.Cells[col.Index].Value.ToString());
                                        vsRatioValue = m_dictPropertyColumns[idAlg].m_vsRatio;
                                        valueToRes *= Math.Pow(10F, vsRatioValue);
                                        dtVal = Convert.ToDateTime(row.Cells["Date"].Value.ToString());
                                        quality = diffRowsInTables(dtSourceOrg, valueToRes, i, idAlg, typeValues);

                                        dtSourceEdit.Rows.Add(new object[]
                                        {
                                            col.m_IdComp
                                            , idSession
                                            , quality
                                            , valueToRes
                                            , dtVal.AddMinutes(-s_currentOffSet).ToString("F",dtSourceEdit.Locale)
                                            , i
                                        });
                                        i++;
                                    }
                        }
                        indexPut++;
                    }
                }
                dtSourceEdit = sortingTable(dtSourceEdit, "WR_DATETIME");
                return dtSourceEdit;
            }

            /// <summary>
            /// соритровка таблицы по столбцу
            /// </summary>
            /// <param name="table">таблица для сортировки</param>
            /// <param name="sortStr">имя столбца/ов для сортировки</param>
            /// <returns>отсортированная таблица</returns>
            private DataTable sortingTable(DataTable table, string colSort)
            {
                try
                {
                    DataView dView = table.DefaultView;
                    string sortExpression = string.Format(colSort);
                    dView.Sort = sortExpression;
                    table = dView.ToTable();
                }
                catch (Exception e)
                {
                    MessageBox.Show("???" + "Ошибка сортировки таблицы! " + e.ToString());
                }


                return table;
            }

            /// <summary>
            /// Проверка на изменение значений в двух таблицах
            /// </summary>
            /// <param name="origin">оригинальная таблица</param>
            /// <param name="editValue">значение</param>
            /// <param name="i">номер строки</param>
            /// <param name="idAlg">номер алгоритма</param>
            /// <param name="typeValues">тип данных</param>
            /// <returns>показатель изменения</returns>
            private int diffRowsInTables(DataTable origin, double editValue, int i, int idAlg, HandlerDbTaskCalculate.ID_VIEW_VALUES typeValues)
            {
                int quality = 1;
                double originValues;

                origin = sortingTable(origin, "ID_PUT, WR_DATETIME");

                if (origin.Rows.Count - 1 < i)
                    originValues = 0;
                else
                    originValues =
                        s_VedCalculate.AsParseToF(origin.Rows[i]["VALUE"].ToString());

                switch (typeValues)
                {
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE:
                        if (originValues.ToString(@"F" + m_dictPropertyColumns[idAlg].m_vsRound, CultureInfo.InvariantCulture) != editValue.ToString())
                            quality = 2;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE:
                        quality = 1;
                        break;
                    case HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT:
                        break;
                    default:
                        break;
                }

                return quality;
            }
        }

        /// <summary>
        /// Класс формирования отчета Excel 
        /// </summary>
        public class ReportExcel
        {
            /// <summary>
            /// экземпляр интерфейса приложения
            /// </summary>
            private Excel.Application m_excApp;
            /// <summary>
            /// экземпляр интерфейса книги
            /// </summary>
            private Excel.Workbook m_workBook;
            /// <summary>
            /// экземпляр интерфейса листа
            /// </summary>
            private Excel.Worksheet m_wrkSheet;
            //private object _missingObj = System.Reflection.Missing.Value;
            /// <summary>
            /// Массив данных
            /// </summary>
            protected object[,] arrayData;

            /// <summary>
            /// 
            /// </summary>
            protected enum INDEX_DIVISION : int
            {
                UNKNOW = -1,
                SEPARATE_CELL,
                ADJACENT_CELL
            }

            /// <summary>
            /// конструктор(основной)
            /// </summary>
            public ReportExcel()
            {
                m_excApp = new Excel.Application();
                m_excApp.Visible = false;
            }

            /// <summary>
            /// Подключение шаблона листа экселя и его заполнение
            /// </summary>
            /// <param name="dgView">отрбражение данных</param>
            /// <param name="dtRange">дата</param>
            public void CreateExcel(DataGridView dgView, DateTimeRange dtRange)
            {
                if (addWorkBooks())
                {
                    m_workBook.AfterSave += workBook_AfterSave;
                    m_workBook.BeforeClose += workBook_BeforeClose;
                    m_wrkSheet = (Excel.Worksheet)m_workBook.Worksheets.get_Item("VedomostBl");
                    int indxCol = 1;

                    try
                    {
                        paintTable(dgView);
                    }
                    catch (Exception e)
                    {
                        closeExcel();
                        MessageBox.Show("???" + "Ошибка прорисовки таблицы для экспорта! " + e.ToString());
                    }

                    try
                    {
                        fillToArray(dgView);

                        for (int i = 0; i < dgView.Columns.Count; i++)
                            if (i >= ((int)DGVVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                            {
                                Excel.Range colRange = (Excel.Range)m_wrkSheet.Columns[indxCol];

                                if (dgView.Columns[i].HeaderText != "")
                                {
                                    foreach (Excel.Range cell in colRange.Cells)
                                        if (Convert.ToString(cell.Value) != "")
                                        {
                                            if (Convert.ToString(cell.Value) == splitString(dgView.Columns[i].HeaderText))
                                            {
                                                fillSheetExcel(colRange, dgView, i, cell.Row);
                                                break;
                                            }
                                        }
                                }
                                //else
                                //    foreach (Excel.Range cell in colRange.Cells)
                                //        if (Convert.ToString(cell.Value) == dgView.Columns[i].Name)
                                //        {
                                //            fillSheetExcelToNHeader(colRange, dgView, i, cell.Row + 1);
                                //            break;
                                //        }
                                break;
                                //indxCol++;
                            }

                        setSignature(m_wrkSheet, dgView, dtRange);
                        m_excApp.Visible = true;
                        closeExcel();
                    }
                    catch (Exception e)
                    {
                        closeExcel();
                        MessageBox.Show("???" + "Ошибка экспорта данных!" + e.ToString());
                    }
                }
            }

            /// <summary>
            /// Заполнение массива данными
            /// </summary>
            /// <param name="dgvActive">активное отображение данных</param>
            private void fillToArray(DataGridView dgvActive)
            {
                arrayData = new object[dgvActive.RowCount, dgvActive.ColumnCount - 1];
                int indexArray = 0;

                for (int i = 0; i < dgvActive.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvActive.Columns.Count; j++)
                        if (j >= ((int)DGVVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                        {
                            if (j > ((int)DGVVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                                arrayData[i, indexArray] = s_VedCalculate.AsParseToF(dgvActive.Rows[i].Cells[j].Value.ToString());
                            else
                                arrayData[i, indexArray] = dgvActive.Rows[i].Cells[j].Value.ToString();

                            indexArray++;
                        }
                    indexArray = 0;
                }
            }

            /// <summary>
            /// Составление таблицы
            /// </summary>
            /// <param name="dgvActive">активное окно данных</param>
            private void paintTable(DataGridView dgvActive)
            {
                int indxCol = 0,
                    colSheetBegin = 2, colSheetEnd = 1,
                    rowSheet = 2,
                    idDgv = (dgvActive as DGVVedomostBl).m_idCompDGV;
                //m_excApp.Visible = true;
                //получаем диапазон
                Excel.Range colRange = (m_wrkSheet.Cells[2, colSheetBegin - 1] as Excel.Range);
                //записываем данные в ячейки
                colRange.Cells[rowSheet + 1, colSheetBegin - 1] = "Дата";
                //получаем диапазон с условием длины заголовка
                var cellsDate = m_wrkSheet.get_Range(getAdressRangeCol(rowSheet, (rowSheet + 1) + 1, colSheetBegin - 1));
                //объединяем ячейки
                mergeCells(cellsDate.Address);
                cellsDate.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                cellsDate.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                paintBorder(cellsDate, (int)Excel.XlLineStyle.xlContinuous);

                foreach (var list in s_listHeader)
                    foreach (var item in list)
                    {
                        //получаем диапазон
                        colRange = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = item;
                        colSheetEnd += (dgvActive as DGVVedomostBl).m_arIntTopHeader[idDgv][indxCol];
                        //выделяем область(левый верхний угол и правый нижний)
                        var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                        //объединяем ячейки
                        mergeCells(cells.Address);
                        //string w = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range).ColumnWidth.ToString();
                        //(cells as Excel.Range).Width.ToString();
                        //

                        //выравнивание текста в ячейке                  
                        cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                        colSheetBegin = colSheetEnd + 1;

                        indxCol++;
                    }
                colSheetBegin = 2;
                //выделяем область(левый верхний угол и правый нижний)
                var Commoncells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1; rowSheet = 3;

                foreach (var item in (dgvActive as DGVVedomostBl).m_headerMiddle[idDgv])
                {
                    //получаем диапазон
                    colRange = (m_wrkSheet.Cells[rowSheet, colSheetBegin] as Excel.Range);
                    //записываем данные в ячейки
                    colRange.Value2 = item;
                    colSheetEnd += (dgvActive as DGVVedomostBl).m_arMiddleCol[idDgv][(dgvActive as DGVVedomostBl).m_headerMiddle[idDgv].ToList().IndexOf(item)];
                    // выделяем область(левый верхний угол и правый нижний)
                    var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                    //объединяем ячейки
                    mergeCells(cells.Address);

                    //
                    cells.WrapText = true;
                    cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                    colSheetBegin = colSheetEnd + 1;
                }
                colSheetBegin = 2;
                //       
                Commoncells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet, colSheetBegin, colSheetEnd));
                paintBorder(Commoncells, (int)Excel.XlLineStyle.xlContinuous);
                colSheetEnd = 1; rowSheet = 3;

                for (int i = 0; i < dgvActive.Columns.Count; i++)
                {
                    if (i > ((int)DGVVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                    {
                        //получаем диапазон
                        colRange = (m_wrkSheet.Cells[rowSheet + 1, colSheetBegin] as Excel.Range);
                        //записываем данные в ячейки
                        colRange.Value2 = dgvActive.Columns[i].HeaderText;
                        // выделяем область(левый верхний угол и правый нижний)
                        var cells = m_wrkSheet.get_Range(getAdressRangeRow(rowSheet + 1, colSheetBegin, colSheetEnd));

                        cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        paintBorder(cells, (int)Excel.XlLineStyle.xlContinuous);
                        colSheetEnd++;
                        colSheetBegin = colSheetEnd + 1;
                    }
                }
            }

            /// <summary>
            /// Нарисовать границы ячейки
            /// </summary>
            /// <param name="cells">выбранный диапазон ячеек</param>
            /// <param name="typeBorder">тип линий</param>
            private void paintBorder(Excel.Range cells, int typeBorder)
            {
                Excel.XlLineStyle styleBorder = Excel.XlLineStyle.xlContinuous;

                switch ((Excel.XlLineStyle)typeBorder)
                {
                    case Excel.XlLineStyle.xlContinuous:
                        styleBorder = Excel.XlLineStyle.xlContinuous;
                        break;
                    case Excel.XlLineStyle.xlDash:
                        styleBorder = Excel.XlLineStyle.xlDash;
                        break;
                    case Excel.XlLineStyle.xlDashDot:
                        styleBorder = Excel.XlLineStyle.xlDashDot;
                        break;
                    case Excel.XlLineStyle.xlDashDotDot:
                        styleBorder = Excel.XlLineStyle.xlDashDotDot;
                        break;
                    case Excel.XlLineStyle.xlDot:
                        break;
                    case Excel.XlLineStyle.xlDouble:
                        break;
                    case Excel.XlLineStyle.xlSlantDashDot:
                        break;
                    case Excel.XlLineStyle.xlLineStyleNone:
                        break;
                    default:
                        break;
                }
                // внутренние вертикальные
                cells.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = styleBorder;
                // внутренние горизонтальные
                cells.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = styleBorder;
                // верхняя внешняя          
                cells.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = styleBorder;
                // правая внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = styleBorder;
                // левая внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = styleBorder;
                // нижняя внешняя
                cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = styleBorder;
            }

            /// <summary>
            /// Получения адреса диапазона ячеек в столбце
            /// </summary>
            /// <param name="rowSheet">номер строки</param>
            /// <param name="colSheetBegin">номер столбца начала</param>
            /// <param name="colSheetEnd">номер столбца конца</param>
            /// <returns>адрес ячеек в формате "A1:A3"</returns>
            private string getAdressRangeCol(int rowSheetBegin, int rowSheetEnd, int colSheet)
            {
                Excel.Range RowRangeBegin = (Excel.Range)m_wrkSheet.Cells[rowSheetBegin, colSheet],
                  RowRangeEnd = (Excel.Range)m_wrkSheet.Cells[rowSheetEnd, colSheet];
                string adressCell = string.Empty;

                adressCell = RowRangeBegin.Address + ":" + RowRangeEnd.Address;

                return adressCell;
            }

            /// <summary>
            /// Получения адреса диапазона ячеек в строке
            /// </summary>
            /// <param name="rowSheet">номер строки</param>
            /// <param name="colSheetBegin">номер столбца начала</param>
            /// <param name="colSheetEnd">номер столбца конца</param>
            /// <returns>адрес диапазона ячеек в формате "A1:C1"</returns>
            private string getAdressRangeRow(int rowSheet, int colSheetBegin, int colSheetEnd)
            {
                Excel.Range colRangeBegin = (Excel.Range)m_wrkSheet.Cells[rowSheet, colSheetBegin],
                    colRangeEnd = (Excel.Range)m_wrkSheet.Cells[rowSheet, colSheetEnd];
                string adressCell = string.Empty;

                adressCell = colRangeBegin.Address + ":" + colRangeEnd.Address;

                return adressCell;
            }

            /// <summary>
            /// Объединение ячеек
            /// </summary>
            /// <param name="cells">диапазон объединения</param>
            private void mergeCells(string cells)
            {
                m_wrkSheet.get_Range(cells).Merge();
            }

            /// <summary>
            /// Подключение шаблона
            /// </summary>
            /// <returns>признак ошибки</returns>
            private bool addWorkBooks()
            {
                string pathToTemplate = Path.GetFullPath(@"Template\TemplateVedBl.xlsx");
                object pathToTemplateObj = pathToTemplate;
                bool bflag = true;
                try
                {
                    m_workBook = m_excApp.Workbooks.Add(pathToTemplate);
                }
                catch (Exception exp)
                {
                    closeExcel();
                    bflag = false;
                    MessageBox.Show("???" + "Отсутствует шаблон для отчета Excel" + exp.ToString());
                }
                return bflag;
            }

            /// <summary>
            /// Обработка события - закрытие экселя
            /// </summary>
            /// <param name="Cancel"></param>
            void workBook_BeforeClose(ref bool Cancel)
            {
                closeExcel();
            }

            /// <summary>
            /// обработка события сохранения книги
            /// </summary>
            /// <param name="Success"></param>
            void workBook_AfterSave(bool Success)
            {
                closeExcel();
            }

            /// <summary>
            /// Добавление подписи месяца
            /// </summary>
            /// <param name="exclWrksht">лист экселя</param>
            /// <param name="dgv">грид</param>
            /// <param name="dtRange">дата</param>
            private void setSignature(Excel.Worksheet exclWrksht, DataGridView dgv, DateTimeRange dtRange)
            {
                //Excel.Range exclTEC = exclWrksht.get_Range("B2");
                Excel.Range exclRMonth = exclWrksht.get_Range("R1");
                exclRMonth.Value2 = "Ведомость блока №" + (dgv as DGVVedomostBl).m_CountBL + " за " + HDateTime.NameMonths[dtRange.Begin.Month - 1] + " месяц " + dtRange.Begin.Year + " года";
                exclRMonth.Font.Bold = true;
                //HDateTime.NameMonths[dtRange.Begin.Month - 1] + " " + dtRange.Begin.Year;
            }

            /// <summary>
            /// Деление 
            /// </summary>
            /// <param name="headerTxt">строка</param>
            /// <returns>часть строки</returns>
            private string splitString(string headerTxt)
            {
                string[] spltHeader = headerTxt.Split(',');

                if (spltHeader.Length > (int)INDEX_DIVISION.ADJACENT_CELL)
                    return spltHeader[(int)INDEX_DIVISION.ADJACENT_CELL].TrimStart();
                else
                    return spltHeader[(int)INDEX_DIVISION.SEPARATE_CELL];
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcel(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int _indxrow = 0;

                string addressRange = string.Empty,
                 addresBegin, addresEnd;
                int cellBegin, cellEnd = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")
                    {
                        _indxrow = i;
                        break;
                    }
                //формировние начальной и конечной координаты диапазона
                addresBegin = (colRange.Cells[_indxrow] as Excel.Range).Address;
                _indxrow = _indxrow + dgv.Rows.Count;
                cellEnd = cellEnd + (dgv.Columns.Count - 1);
                addresEnd = (m_wrkSheet.Cells[_indxrow - 1, cellEnd] as Excel.Range).Address;
                //получение диапазона
                addressRange = addresBegin + ":" + addresEnd;
                Excel.Range rangeFill = m_wrkSheet.get_Range(addressRange);
                //заполнение
                var arrayVar = arrayData;
                rangeFill.Value2 = arrayVar;
                paintBorder(rangeFill, (int)Excel.XlLineStyle.xlContinuous);
            }

            /// <summary>
            /// Заполнение выбранного стоблца в шаблоне 
            /// (при условии пустого заголовка)
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="dgv">отображение</param>
            /// <param name="indxColDgv">индекс столбца</param>
            /// <param name="indxRowExcel">индекс строки в excel</param>
            private void fillSheetExcelToNHeader(Excel.Range colRange
                , DataGridView dgv
                , int indxColDgv
                , int indxRowExcel)
            {
                int row = 0;

                for (int i = indxRowExcel; i < colRange.Rows.Count; i++)
                    if (((Excel.Range)colRange.Cells[i]).Value == null &&
                        ((Excel.Range)colRange.Cells[i]).MergeCells.ToString() != "True")

                        if (((Excel.Range)colRange.Cells[i - 1]).Value2 == null)
                        {
                            row = i;
                            break;
                        }

                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    //colRange.Cells.NumberFormat = "0";
                    if (indxColDgv >= ((int)DGVVedomostBl.INDEX_SERVICE_COLUMN.COUNT - 1))
                        colRange.Cells[row] = s_VedCalculate.AsParseToF(Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value));
                    else
                        colRange.Cells[row] = Convert.ToString(dgv.Rows[j].Cells[indxColDgv].Value);

                    paintBorder((Excel.Range)colRange.Cells[row], (int)Excel.XlLineStyle.xlContinuous);
                    row++;
                }
            }

            /// <summary>
            /// Удаление пустой строки
            /// (при условии, что ниже пустой строки есть строка с данными)
            /// </summary>
            /// <param name="colRange">столбец в excel</param>
            /// <param name="row">номер строки</param>
            private void deleteNullRow(Excel.Range colRange, int row)
            {
                Excel.Range rangeCol = (Excel.Range)m_wrkSheet.Columns[1];

                while (Convert.ToString(((Excel.Range)rangeCol.Cells[row]).Value) == "")
                {
                    if (Convert.ToString(((Excel.Range)rangeCol.Cells[row + 1]).Value) == "")
                        break;
                    else
                    {
                        Excel.Range rangeRow = (Excel.Range)m_wrkSheet.Rows[row];
                        rangeRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                    }
                }
            }

            /// <summary>
            /// вызов закрытия Excel
            /// </summary>
            private void closeExcel()
            {
                try
                {
                    //Вызвать метод 'Close' для текущей книги 'WorkBook' с параметром 'true'
                    //workBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workBook, new object[] { true });
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excApp);

                    m_excApp = null;
                    m_workBook = null;
                    m_wrkSheet = null;
                    GC.Collect();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        ///класс для обработки данных
        /// </summary>
        public class VedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Экземпляр класса
            /// </summary>
            private parsingData _pData;
            /// <summary>
            /// индекс уровней хидеров
            /// </summary>
            protected enum lvlHeader
            {
                UNKNOW = -1,
                TOP, MIDDLE, LOW,
                COUNT
            }

            /// <summary>
            /// 
            /// </summary>
            public VedomostBlCalculate()
                : base()
            {

            }

            /// <summary>
            /// Создание словаря заголвоков для каждого блока(ТГ)
            /// </summary>
            /// <param name="dtSource">таблица с данными</param>
            /// <param name="param">номер компонента</param>
            /// <returns>массив словарей заголовков</returns>
            public List<string[]> CreateDictHeader(DataTable dtSource, int param)
            {
                _pData = new parsingData(dtSource, param);

                return compilingDict(_pData.ListParam, dtSource.Select("ID_COMP = " + param));
            }

            /// <summary>
            /// Преобразование входных для расчета значений в структуры, пригодные для производства расчетов
            /// </summary>
            /// <param name="arDataTables">Массив таблиц с указанием их предназначения</param>
            protected override int initValues(ListDATATABLE listDataTables)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// сборка и компановка словаря
            /// </summary>
            /// <param name="arlistStr">лист парамтеров</param>
            /// <param name="dtPars">таблица с данными</param>
            /// <returns>массив словарей</returns>
            private List<string[]> compilingDict(List<List<string>> arlistStr, DataRow[] dtPars)
            {
                int cntHeader = 0;
                string[] _arStrHeader;
                List<string[]> listHeader = new List<string[]> { };

                var enumHeader = (from r in dtPars.AsEnumerable()
                                  orderby r.Field<int>("ID")
                                  select new
                                  {
                                      NAME_SHR = r.Field<string>("NAME_SHR"),
                                  }).Distinct();

                listHeader.Clear();

                for (int j = 0; j < arlistStr.Count; j++)
                {
                    if (arlistStr[j].Count < 3)
                        _arStrHeader = new string[arlistStr[j].Count + 1];
                    else
                        _arStrHeader = new string[arlistStr[j].Count];

                    bool bflagStopfor = false;
                    cntHeader = 0;

                    for (int i = arlistStr[j].Count - 1; i > -1; i--)
                    {
                        switch (i)
                        {
                            case (int)lvlHeader.TOP:
                                for (int t = 0; t < s_listHeader.Count; t++)
                                {
                                    for (int n = 0; n < s_listHeader[t].Count; n++)
                                    {
                                        cntHeader++;
                                        if (int.Parse(arlistStr[j].ElementAt((int)lvlHeader.TOP)) == cntHeader)
                                        {
                                            _arStrHeader[i] = s_listHeader[t][n];
                                            listHeader.Add(_arStrHeader);
                                            bflagStopfor = true;
                                            break;
                                        }
                                    }

                                    if (bflagStopfor)
                                        break;
                                }
                                break;
                            case (int)lvlHeader.MIDDLE:

                                if (arlistStr[j].Count < 3)
                                    _arStrHeader[i + 1] = "";

                                _arStrHeader[(int)lvlHeader.MIDDLE] = dtPars[j]["NAME_SHR"].ToString().Trim();
                                break;
                            case (int)lvlHeader.LOW:
                                _arStrHeader[i] = dtPars[j]["DESCRIPTION"].ToString().Trim();
                                break;
                            default:
                                break;
                        }
                    }
                }

                return listHeader;
            }

            /// <summary>
            /// 
            /// </summary>
            private class DataWorkClass
            {

            }

            /// <summary>
            /// преобразование числа в нужный формат отображения
            /// </summary>
            /// <param name="value">число</param>
            /// <returns>преобразованное число</returns>
            public float AsParseToF(string value)
            {
                int _indxChar = 0;
                string _sepReplace = string.Empty;
                bool bFlag = true;
                //char[] _separators = { ' ', ',', '.', ':', '\t'};
                //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();
                float fValue = 0;

                foreach (char item in value.ToCharArray())
                {
                    if (!char.IsDigit(item))
                        if (char.IsLetter(item))
                            value = value.Remove(_indxChar, 1);
                        else
                            _sepReplace = value.Substring(_indxChar, 1);
                    else
                        _indxChar++;

                    switch (_sepReplace)
                    {
                        case ".":
                        case ",":
                        case " ":
                        case ":":
                            float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fValue);
                            bFlag = false;
                            break;
                    }
                }

                if (bFlag)
                    try
                    {
                        fValue = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        if (value.ToString() == "")
                            fValue = 0;
                    }

                return fValue;
            }

            /// <summary>
            /// класс для формирования листа с параметрами 
            /// для формирования заголовков
            /// </summary>
            private class parsingData
            {
                /// <summary>
                /// набор листов с параметрами группировки
                /// </summary>
                private List<List<string>> arList;

                /// <summary>
                /// конструктор с параметрами
                /// </summary>
                /// <param name="dt">таблица с данными</param>
                /// <param name="param">параметр для выборки</param>
                public parsingData(DataTable dt, int param)
                {
                    disaggregationToParts(dt.Select("ID_COMP = " + param));
                }

                /// <summary>
                /// формирование листа параметров вида x.y.z,
                /// где x - TopHeader, y - MiddleHeader, y - LowHeader
                /// </summary>
                /// <param name="dtPars">таблица с данными</param>
                private void disaggregationToParts(DataRow[] dtPars)
                {
                    arList = new List<List<string>>(dtPars.Count());

                    foreach (DataRow row in dtPars)
                    {
                        List<string> list = new List<string>();
                        list = row["N_ALG"].ToString().Split('.', ',').ToList();
                        arList.Add(list);
                    }
                }

                /// <summary>
                /// возвращает лист с парметрами 
                /// для построения словаря заголовков
                /// </summary>
                public List<List<string>> ListParam
                {
                    get
                    {
                        return arList;
                    }
                }
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="iFunc"></param>
        public PanelTaskVedomostBl(IPlugIn iFunc)
            : base(iFunc)
        {
            s_VedCalculate = new VedomostBlCalculate();
            HandlerDb.IdTask = ID_TASK.VEDOM_BL;
            //Session.SetDatetimeRange(s_dtDefaultAU, s_dtDefaultAU.AddDays(1));
            m_dict = new Dictionary<int, List<string[]>> { };

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            InitializeComponent();
            s_getPicture = new DelgetPictureOfIdComp(GetPictureOfIdComp);
            s_getDGV = new DelgetDataGridViewActivate(GetDGVOfIdComp);
            s_getIdComp = new IntDelegateFunc(GetIdComp);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            Control ctrl = new Control();
            // переменные для инициализации кнопок "Добавить", "Удалить"
            string strPartLabelButtonDropDownMenuItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить" 

            SuspendLayout();

            Controls.Add(PanelManagement, 0, posRow);
            SetColumnSpan(PanelManagement, 4); SetRowSpan(PanelManagement, 13);
            //контейнеры для DGV
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = INDEX_CONTROL.PICTURE_BOXDGV.ToString();
            pictureBox.TabStop = false;
            //
            Panel m_paneL = new Panel();
            m_paneL.Name = INDEX_CONTROL.PANEL_PICTUREDGV.ToString();
            m_paneL.Dock = DockStyle.Fill;
            (m_paneL as Panel).AutoScroll = true;
            Controls.Add(m_paneL, 5, posRow);
            SetColumnSpan(m_paneL, 9); SetRowSpan(m_paneL, 10);
            //
            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), 4);

            ResumeLayout(false);
            PerformLayout();

            Button btn = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button);
            btn.Click += // действие по умолчанию
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_EXPORT.ToString(), true)[0] as Button).Click += PanelTaskVedomostBl_expExcel_Click;
            PanelManagement.ItemCheck += new PanelManagementVedomostBl.ItemCheckedParametersEventHandler(panelManagement_ItemCheck);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).CheckedChanged += PanelManagementVedomost_CheckedChanged;
        }

        /// <summary>
        /// Обработчик события - Кнопка экспорта даных в Excel
        /// </summary>
        /// <param name="sender">объект, вызвавщий событие</param>
        /// <param name="e">Аргумент события, описывающий состояние элемента</param>
        private void PanelTaskVedomostBl_expExcel_Click(object sender, EventArgs e)
        {
            m_rptExcel = new ReportExcel();
            m_rptExcel.CreateExcel(getActiveView(), Session.m_rangeDatetime);
        }

        /// <summary>
        /// Обработчик события - изменение отображения кол-во групп заголовка
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        private void panelManagement_ItemCheck(PanelManagementVedomostBl.ItemCheckedParametersEventArgs ev)
        {
            int idItem = -1;

            //Изменить признак состояния компонента ТЭЦ/параметра алгоритма расчета
            if (ev.NewCheckState == CheckState.Unchecked)
                if (m_arListIds[(int)ev.m_indxId].IndexOf(idItem) < 0)
                    m_arListIds[(int)ev.m_indxId].Add(idItem);
                else; //throw new Exception (@"");
            else
                if (ev.NewCheckState == CheckState.Checked)
                if (!(m_arListIds[(int)ev.m_indxId].IndexOf(idItem) < 0))
                    m_arListIds[(int)ev.m_indxId].Remove(idItem);
                else; //throw new Exception (@"");
            else;
            //Отправить сообщение главной форме об изменении/сохранении индивидуальных настроек
            // или в этом же плюгИне измененить/сохраннить индивидуальные настройки
            //Изменить структуру 'HDataGRidVIew's'          
            placementHGridViewOnTheForm(ev);
        }

        /// <summary>
        /// Обработчик события - Признак Корректировка_включена/корректировка_отключена 
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        void PanelManagementVedomost_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Изменить структуру 'HDataGRidVIew's'
        /// </summary>
        /// <param name="item"></param>
        private void placementHGridViewOnTheForm(PanelManagementVedomostBl.ItemCheckedParametersEventArgs item)
        {
            bool bItemChecked = item.NewCheckState == CheckState.Checked ? true :
                  item.NewCheckState == CheckState.Unchecked ? false : false;
            DGVVedomostBl cntrl = (getActiveView() as DGVVedomostBl);
            //Поиск индекса элемента отображения
            switch ((INDEX_ID)item.m_indxId)
            {
                case INDEX_ID.HGRID_VISIBLE:
                    cntrl.HideColumns(cntrl as DataGridView, s_listHeader[item.m_idItem], bItemChecked);
                    ReSizeControls(cntrl as DataGridView);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Нахожджение активного DGV
        /// </summary>
        /// <returns>активная вьюха на панели</returns>
        private DataGridView getActiveView()
        {
            bool _flagb = false;
            Control cntrl = new Control();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                if (item.Visible == true)
                    foreach (DataGridView dgv in item.Controls)
                    {
                        cntrl = dgv;
                        _flagb = true;
                    }
                else if (_flagb)
                    break;

            return (cntrl as DataGridView);
        }

        /// <summary>
        /// Настройка размеров контролов отображения
        /// </summary>
        private void ReSizeControls(DataGridView viewActive)
        {
            int cntCol = 0;

            for (int j = 1; j < viewActive.ColumnCount; j++)
                viewActive.Columns[j].Width = 65;

            foreach (DataGridViewColumn col in viewActive.Columns)
                if (col.Visible == true)
                    cntCol++;

            int _drwW = cntCol * viewActive.Columns[2].Width + 10
                , _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

            GetPictureOfIdComp((viewActive as DGVVedomostBl).m_idCompDGV).Size = new Size(_drwW + 2, _drwH);
            viewActive.Size = new Size(_drwW + 2, _drwH);
        }

        /// <summary>
        /// Обработчик события - добавления строк в грид
        /// (для изменение размера контролов)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void DGVVedomostBl_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ReSizeControls(sender as DataGridView);
        }

        /// <summary>
        /// Возвращает пикчу по номеру
        /// </summary>
        /// <param name="idComp">ид номер грида</param>
        /// <returns>активная пикча на панели</returns>
        public PictureBox GetPictureOfIdComp(int idComp)
        {
            int cnt = 0,
                outCnt = 0;
            PictureBox cntrl = new PictureBox();

            foreach (PictureVedBl item in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
            {
                if (idComp == item.m_idCompPicture)
                {
                    outCnt = cnt;
                    cntrl = (item as PictureBox);
                }
                else
                {
                    (item as PictureBox).Visible = false;
                    (item as PictureBox).Enabled = false;
                }
                cnt++;
            }

            if (outCnt == 0 || outCnt == 5)
                WhichBlIsSelected = true;
            else
                WhichBlIsSelected = false;

            return cntrl;
        }

        /// <summary>
        /// Возвращает по номеру грид
        /// </summary>
        /// <returns>активный грид на панели</returns>
        public DataGridView GetDGVOfIdComp()
        {
            DataGridView cntrl = new DataGridView();

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                foreach (DGVVedomostBl item in picture.Controls)
                    if (item.Visible == true)
                        cntrl = (item as DataGridView);

            return cntrl;

        }

        /// <summary>
        /// Возвращает idComp
        /// </summary>
        /// <returns>индентификатор объекта</returns>
        public int GetIdComp()
        {
            int _idComp = 0;

            foreach (PictureVedBl picture in Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0].Controls)
                if (picture.Visible == true)
                    foreach (DGVVedomostBl item in picture.Controls)
                        if (item.Visible == true)
                            _idComp = item.m_idCompDGV;

            return _idComp;
        }

        /// <summary>
        /// Настройка размеров формы отображения данных
        /// </summary>
        /// <param name="dgv">активное окно отображения данных</param>
        public void SizeDgv(object dgv)
        {
            (dgv as DGVVedomostBl).dgvConfigCol(dgv as DataGridView);
        }

        /// <summary>
        /// Инициализация радиобаттанов
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeRB(Array namePut, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string[] arstrItem;
            PanelManagementVedomostBl.RadioButtonBl[] arRadioBtn;
            int[] arId_comp;
            int rbCnt = (int)INDEX_CONTROL.RADIOBTN_BLK1;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[]
            {
                INDEX_ID.BLOCK_VISIBLED
            };
            //инициализация массивов
            bool[] arChecked = new bool[m_dictTableDictPrj[ID_DBTABLE.COMP].Rows.Count];
            List<CheckState> arGroup = new List<CheckState>();
            arRadioBtn = new PanelManagementVedomostBl.RadioButtonBl[m_dictTableDictPrj[ID_DBTABLE.COMP].Rows.Count];
            arId_comp = new int[m_dictTableDictPrj[ID_DBTABLE.COMP].Rows.Count];
            arstrItem = new string[m_dictTableDictPrj[ID_DBTABLE.COMP].Rows.Count];
            //создание списка гридов по блокам
            foreach (DataRow r in m_dictTableDictPrj[ID_DBTABLE.COMP].Rows)
            {
                if (arGroup.Count > 0)
                    arGroup.Clear();
                //инициализация радиобаттанов
                arRadioBtn[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = new PanelManagementVedomostBl.RadioButtonBl(namePut.GetValue(rbCnt).ToString());

                arId_comp[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = int.Parse(r[@"ID"].ToString());
                m_arListIds[(int)INDEX_ID.ALL_COMPONENT].Add(int.Parse(r[@"ID"].ToString()));
                arstrItem[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = ((string)r[@"DESCRIPTION"]).Trim();
                if (rbCnt == (int)INDEX_CONTROL.RADIOBTN_BLK1)
                    arChecked[0] = true;
                else
                    arChecked[rbCnt - (int)INDEX_CONTROL.RADIOBTN_BLK1] = false;

                rbCnt++;
            }

            for (int i = 0; i < s_listHeader.Count; i++)
                arGroup.Add(CheckState.Checked);

            try
            {
                //if (arId_comp[rbCnt] != 0)
                //добавление радиобатонов на форму
                PanelManagement.AddComponentRB(arId_comp
                          , arstrItem
                          , arIndxIdToAdd
                          , arChecked
                          , arRadioBtn
                          , arGroup);

            } catch (Exception e) {
                Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initializeRB () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

        }

        /// <summary>
        /// Инициализация сетки данных
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeDGV(Array namePut, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            Control ctrl = null;
            DateTime _dtRow = PanelManagement.DatetimeRange.Begin;
            DataTable dtComponentId = HandlerDb.GetHeaderDGV();//получение ид компонентов    

            //создание грида со значениями
            for (int j = (int)INDEX_CONTROL.DGV_DATA_B1; j < (int)INDEX_CONTROL.RADIOBTN_BLK1; j++)
            {
                ctrl = new DGVVedomostBl(namePut.GetValue(j).ToString());
                ctrl.Name = namePut.GetValue(j).ToString();
                (ctrl as DGVVedomostBl).m_idCompDGV = int.Parse(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows[j]["ID"].ToString());
                (ctrl as DGVVedomostBl).m_CountBL = j + 1;

                filingDictHeader(dtComponentId, (ctrl as DGVVedomostBl).m_idCompDGV);

                Dictionary<string, List<int>> _dictVisualSett = visualSettingsCol((ctrl as DGVVedomostBl).m_idCompDGV);

                for (int k = 0; k < m_dict[(ctrl as DGVVedomostBl).m_idCompDGV].Count; k++)
                {
                    int idPar = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (ctrl as DGVVedomostBl).m_idCompDGV)[k]["ID_ALG"].ToString());
                    int _avg = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (ctrl as DGVVedomostBl).m_idCompDGV)[k]["AVG"].ToString());
                    int _idComp = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (ctrl as DGVVedomostBl).m_idCompDGV)[k]["ID"].ToString());

                    (ctrl as DGVVedomostBl).AddColumns(idPar, new DGVVedomostBl.COLUMN_PROPERTY
                    {
                        topHeader = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.TOP].ToString(),
                        nameCol = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.MIDDLE].ToString(),
                        hdrText = m_dict[(ctrl as DGVVedomostBl).m_idCompDGV][k][(int)DGVVedomostBl.INDEX_HEADER.LOW].ToString(),
                        m_idAlg = idPar,
                        m_IdComp = _idComp,
                        m_vsRatio = _dictVisualSett["ratio"][k],
                        m_vsRound = _dictVisualSett["round"][k],
                        m_Avg = _avg
                    }
                       , true);
                }

                for (int i = 0; i < DaysInMonth + 1; i++)
                    if ((ctrl as DGVVedomostBl).Rows.Count != DaysInMonth)
                        (ctrl as DGVVedomostBl).AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            //m_idAlg = id_alg
                            //,
                            m_Value = _dtRow.AddDays(i).ToShortDateString()
                        });
                    else
                    {
                        (ctrl as DGVVedomostBl).RowsAdded += DGVVedomostBl_RowsAdded;
                        (ctrl as DGVVedomostBl).AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            //m_idAlg = id_alg
                            //,
                            m_Value = "ИТОГО"
                        }
                       , DaysInMonth);
                    }

                SizeDgv(ctrl);
                m_pictureVedBl = new PictureVedBl(ctrl as DGVVedomostBl);
                (Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0] as Panel).Controls.Add(m_pictureVedBl);
                //возможность_редактирвоания_значений
                try
                {
                    if (m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()].Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                    {
                        if (int.Parse(m_dictProfile.Objects[((int)ID_PERIOD.MONTH).ToString()].Objects[((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()].Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE)
                            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = true;
                        else
                            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;
                    }
                    else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                    if ((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                        for (int t = 0; t < (ctrl as DGVVedomostBl).RowCount; t++)
                            (ctrl as DGVVedomostBl).AddBRead(false);
                } catch (Exception exp) {
                    MessageBox.Show("???" + "Ошибки проверки возможности редактирования ячеек " + exp.ToString());
                }
            }
        }

        /// <summary>
        /// Инициализация групп отображения заголовков
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeGroup(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            int id_comp;

            INDEX_ID[] arIndxIdToAdd = new INDEX_ID[]
            {
                INDEX_ID.HGRID_VISIBLE
            };

            bool[] arChecked = new bool[s_listHeader.Count];

            //getControl();

            foreach (var list in s_listHeader)
            {
                id_comp = s_listHeader.IndexOf(list);
                strItem = "Группа " + (id_comp + 1);
                // установить признак отображения группы столбцов
                //for (int i = 0; i < arChecked.Count(); i++)
                arChecked[id_comp] = true;
                PanelManagement.AddComponent(id_comp
                    , strItem
                    , list
                    , arIndxIdToAdd
                    , arChecked);
            }
        }

        /// <summary>
        /// Инициализация объектов формы
        /// </summary>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
            string strItem = string.Empty;
            Array namePut = Enum.GetValues(typeof(INDEX_CONTROL));
            int i = -1;
            Control ctrl = null;
            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            for (INDEX_ID id = INDEX_ID.PERIOD; id < INDEX_ID.COUNT; id++)
                switch (id) {
                    case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    case INDEX_ID.ALL_COMPONENT:
                        m_arListIds[(int)id] = new List<int> { };
                        break;
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }
            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMWZONE, COMP, PARAMETER, RATIO
            initialize(new ID_DBTABLE[] { ID_DBTABLE.PERIOD
                    , ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP
                    , Type == HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );

            PanelManagement.Clear();
            //Dgv's
            initializeDGV(namePut, out err, out errMsg);//???
            //groupHeader                                        
            initializeGroup(out err, out errMsg);
            //радиобаттаны
            initializeRB(namePut, out err, out errMsg);
            PanelManagement.ActivateCheckedHandler(true, new INDEX_ID[] { INDEX_ID.HGRID_VISIBLE });
            //активность_кнопки_сохранения
            try
            {
                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                {
                    if (int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE)
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = true;
                    else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
                }
                else
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception exp) {
            // ???
                MessageBox.Show("???" + exp.ToString());
            }

            if (err == 0)
            {
                try
                {
                    if (m_bflgClear == false)
                        m_bflgClear = true;
                    else
                        m_bflgClear = false;

                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE], ID_TIMEZONE.MSK);
                    setCurrentTimeZone(ctrl as ComboBox);
                    //Заполнить элемент управления с периодами расчета
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.PERIOD]
                        , (ID_PERIOD)m_arListIds[(int)INDEX_ID.PERIOD].IndexOf(int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()])));
                    Session.SetCurrentPeriod(PanelManagement.IdPeriod);
                    PanelManagement.SetModeDatetimeRange();

                    (ctrl as ComboBox).Enabled = false;

                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), @"...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
        }

        /// <summary>
        /// Получение визуальных настроек 
        /// для отображения данных на форме
        /// </summary>
        /// <param name="idComp">идКомпонента</param>
        /// <returns>словарь настроечных данных</returns>
        private Dictionary<string, List<int>> visualSettingsCol(int idComp)
        {
            int err = -1
             , id_alg = -1;
            List<int> ratio = new List<int>()
            , round = new List<int>();
            string n_alg = string.Empty;

            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            Dictionary<string, List<int>> _dictSett = new Dictionary<string, List<int>>();

            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
               , new int[] {
                    m_id_panel
                    , idComp }
               , out err);

            IEnumerable<DataRow> listParameter = ListParameter.Select(x => x).Where(x => (int)x["ID_COMP"] == idComp);

            foreach (DataRow r in listParameter)
            {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();
                // не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                    // добавить в список идентификатор параметра алгоритма расчета
                    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg) == true)
                {// установленные в проекте
                    ratio.Add(dictVisualSettings[n_alg.Trim()].m_ratio);
                    round.Add(dictVisualSettings[n_alg.Trim()].m_round);
                }
                else
                {// по умолчанию
                    ratio.Add(HTepUsers.s_iRatioDefault);
                    round.Add(HTepUsers.s_iRoundDefault);
                }
            }
            _dictSett.Add("ratio", ratio);
            _dictSett.Add("round", round);

            return _dictSett;
        }

        /// <summary>
        /// кол-во дней в текущем месяце
        /// </summary>
        /// <returns>кол-во дней</returns>
        public int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Session.m_rangeDatetime.Begin.Year, Session.m_rangeDatetime.Begin.Month);
            }
        }

        /// <summary>
        /// Заполнение словаря[x] заголовков
        /// </summary>
        /// <param name="dt">табилца парамтеров</param>
        /// <param name="paramBl">параметр(идТГ)</param>
        protected void filingDictHeader(DataTable dt, int paramBl)
        {
            m_dict.Add(paramBl, s_VedCalculate.CreateDictHeader(dt, paramBl));//cловарь заголовков
        }

        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        /// <param name="obj">Объект, инициировавший события (список с перечислением часовых поясов)</param>
        /// <param name="ev">Аргумент события</param>
        protected void cbxTimezone_SelectedIndexChanged(object obj, EventArgs ev)
        {
            if (m_bflgClear)
            {
                //Установить новое значение для текущего периода
                setCurrentTimeZone(obj as ComboBox);
                // очистить содержание представления
                clear();
            }
        }

        /// <summary>
        /// Установить новое значение для текущего периода
        /// </summary>
        /// <param name="cbxTimezone">Объект, содержащий значение выбранной пользователем зоны даты/времени</param>
        protected void setCurrentTimeZone(ComboBox cbxTimezone)
        {
            int idTimezone = m_arListIds[(int)INDEX_ID.TIMEZONE][cbxTimezone.SelectedIndex];

            Session.SetCurrentTimeZone((ID_TIMEZONE)idTimezone
                , (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + idTimezone)[0][@"OFFSET_UTC"]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        protected void activateDateTimeRangeValue_OnChanged(bool active)
        {
            if (!(PanelManagement == null))
                if (active == true)
                    PanelManagement.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);
                else
                    if (active == false)
                    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;
                else
                    throw new Exception(@"PanelTaskAutobook::activateDateTimeRangeValue_OnChanged () - не создана панель с элементами управления...");
        }

        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            //Установить новое значение для текущего периода
            Session.SetCurrentPeriod(PanelManagement.IdPeriod);
            //Отменить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(false);
            //Установить новые режимы для "календарей"
            PanelManagement.SetModeDatetimeRange();
            //Возобновить обработку события - изменение начала/окончания даты/времени
            activateDateTimeRangeValue_OnChanged(true);
            if (m_bflgClear)
                // очистить содержание представления
                clear();
        }

        /// <summary>
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            int //err = -1,
              id_alg = -1;
            DGVVedomostBl _dgv = (getActiveView() as DGVVedomostBl);
            string n_alg = string.Empty;
            DateTime dt = new DateTime(dtBegin.Year, dtBegin.Month, 1);

            //settingDateRange();
            Session.SetDatetimeRange(dtBegin, dtEnd);

            if (m_bflgClear)
            {
                clear();

                if (_dgv.Rows.Count != 0)
                    _dgv.ClearRows();

                for (int i = 0; i < DaysInMonth + 1; i++)
                {
                    if (_dgv.Rows.Count != DaysInMonth)
                        _dgv.AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = dt.AddDays(i).ToShortDateString()
                        });
                    else
                        _dgv.AddRow(new DGVVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            ,
                            //m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            //,
                            m_Value = "ИТОГО"
                        }
                        , DaysInMonth);
                }
            }

            _dgv.Rows[dtBegin.Day - 1].Selected = true;
            s_currentOffSet = Session.m_curOffsetUTC;
        }

        ///// <summary>
        ///// Установка длительности периода 
        ///// </summary>
        //private void settingDateRange()
        //{
        //    int cntDays,
        //        today = 0;

        //    PanelManagement.DateTimeRangeValue_Changed -= datetimeRangeValue_onChanged;

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //      (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(-(today - 1));

        //    cntDays = DateTime.DaysInMonth((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Year,
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Month);
        //    today = (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.Day;

        //    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_END.ToString(), true)[0] as HDateTimePicker).Value =
        //        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.HDTP_BEGIN.ToString(), true)[0] as HDateTimePicker).Value.AddDays(cntDays - today);

        //    PanelManagementVedomostBl.DateTimeRangeValue_Changed += new PanelManagementVedomostBl.DateTimeRangeValueChangedEventArgs(datetimeRangeValue_onChanged);

        //}

        /// <summary>
        /// Список строк с параметрами алгоритма расчета для текущего периода расчета
        /// </summary>
        private List<DataRow> ListParameter
        {
            get
            {
                List<DataRow> listRes;

                listRes = m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select().ToList();

                return listRes;
            }
        }

        /// <summary>
        /// загрузка/обновление данных
        /// </summary>
        private void updateDataValues()
        {
            int err = -1
                , cnt = Session.CountBasePeriod
                , iRegDbConn = -1;
            string errMsg = string.Empty;
            DateTimeRange[] dtrGet;

            if (!WhichBlIsSelected)
                dtrGet = HandlerDb.GetDateTimeRangeValuesVar();
            else
                dtrGet = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

            clear();
            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                // установить значения в таблицах для расчета, создать новую сессию
                setValues(dtrGet, out err, out errMsg);

                if (err == 0)
                {
                    if (m_TableOrigin.Rows.Count > 0)
                    {
                        // создать копии для возможности сохранения изменений
                        setValues();
                        // отобразить значения
                        (getActiveView() as DGVVedomostBl).ShowValues(m_arTableOrigin[(int)Session.m_ViewValues], m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER], Session.m_ViewValues);
                        //сохранить готовые значения в таблицу
                        m_arTableEdit[(int)Session.m_ViewValues] = valuesFence();
                    }
                    else
                        deleteSession();
                }
                else
                {
                    // в случае ошибки "обнулить" идентификатор сессии
                    deleteSession();
                    throw new Exception(@"PanelTaskVedomostBl::updatedataValues() - " + errMsg);
                }
            }
            else
                deleteSession();

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
        }

        /// <summary>
        /// Обновить/Вставить/Удалить
        /// </summary>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="unCol">столбец, неучаствующий в InsetUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void updateInsertDel(string nameTable, DataTable origin, DataTable edit, string unCol, out int err)
        {
            err = -1;

            m_handlerDb.RecUpdateInsertDelete(nameTable
                    , @"ID_PUT, DATE_TIME, QUALITY"
                    , unCol
                    , origin
                    , edit
                    , out err);
        }

        /// <summary>
        /// получение значений
        /// создание сессии
        /// </summary>
        /// <param name="arQueryRanges"></param>
        /// <param name="err">номер ошибки</param>
        /// <param name="strErr">текст ошибки</param>
        private void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;
            //Создание сессии
            Session.New();
            if (Session.m_ViewValues == HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE)
                //Запрос для получения архивных данных
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutvalArch(Type, HandlerDb.GetDateTimeRangeValuesVarArchive(), out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar(
                Type
                , Session.ActualIdPeriod
                , Session.CountBasePeriod
                , arQueryRanges
               , out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    //, получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_id_panel
                        , Session.CountBasePeriod
                        , m_dictTableDictPrj[ID_DBTABLE.COMP_LIST]
                        , ref m_arTableOrigin
                        , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                        , out err, out strErr);
                else
                    strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
                        + @" по " + Session.m_rangeDatetime.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
                    + @" по " + Session.m_rangeDatetime.End.ToString();
        }

        /// <summary>
        /// copy
        /// </summary>
        private void setValues()
        {
            m_arTableEdit[(int)Session.m_ViewValues] =
             m_arTableOrigin[(int)Session.m_ViewValues].Clone();
        }

        /// <summary>
        /// формирование таблицы данных
        /// </summary>
        private DataTable valuesFence()
        { //сохранить вх. знач. в DataTable
            return (getActiveView() as DGVVedomostBl).FillTableToSave(m_TableOrigin, (int)Session.m_Id, Session.m_ViewValues);
        }

        /// <summary>
        /// проверка выборки блока(для 1 и 6)
        /// </summary>
        public bool WhichBlIsSelected
        {
            get { return s_flagBl; }

            set { s_flagBl = value; }
        }

        ///// <summary>
        ///// формирование запросов 
        ///// для справочных данных
        ///// </summary>
        ///// <returns>запрос</returns>
        //private string[] getQueryDictPrj()
        //{
        //    string[] arRes = null;

        //    arRes = new string[]
        //    {
        //        //PERIOD
        //        HandlerDb.GetQueryTimePeriods(m_strIdPeriods)
        //        //TIMEZONE
        //        , HandlerDb.GetQueryTimezones(m_strIdTimezones)
        //        // список компонентов
        //        , HandlerDb.GetQueryComp(Type)
        //        // параметры расчета
        //        , HandlerDb.GetQueryParameters(Type)
        //        //// настройки визуального отображения значений
        //        //, @""
        //        // режимы работы
        //        //, HandlerDb.GetQueryModeDev()
        //        //// единицы измерения
        //        //, m_handlerDb.GetQueryMeasures()
        //        // коэффициенты для единиц измерения
        //        , HandlerDb.GetQueryRatio()
        //    };

        //    return arRes;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="err">номер ошибки</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// удачное заврешение UpdateInsertDelete
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] =
               m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE].Copy();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки сохранить
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            DateTimeRange[] dtR = HandlerDb.GetDateTimeRangeValuesVarArchive();

            m_arTableOrigin[(int)Session.m_ViewValues] =
                HandlerDb.GetDataOutval(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, dtR, out err);
            //HandlerDb.GetInVal(Type
            //, dtR
            //, ActualIdPeriod
            //, out err);

            m_arTableEdit[(int)Session.m_ViewValues] =
            HandlerDb.SaveValues(m_arTableOrigin[(int)Session.m_ViewValues]
                , valuesFence()
                , (int)Session.m_currIdTimezone
                , out err);

            saveInvalValue(out err);
        }

        /// <summary>
        /// Сохранение входных знчений
        /// </summary>
        /// <param name="err">номер ошибки</param>
        private void saveInvalValue(out int err)
        {
            DateTimeRange[] dtrPer;

            if (!WhichBlIsSelected)
                dtrPer = HandlerDb.GetDateTimeRangeValuesVar();
            else
                dtrPer = HandlerDb.GetDateTimeRangeValuesVarExtremeBL();

            sortingDataToTable(m_arTableOrigin[(int)Session.m_ViewValues]
                , m_arTableEdit[(int)Session.m_ViewValues]
                , HandlerDb.GetNameTableOut(dtrPer[0].Begin)
                , @"ID"
                , out err
            );
        }

        /// <summary>
        /// разбор данных по разным табилца(взависимости от месяца)
        /// </summary>
        /// <param name="origin">оригинальная таблица</param>
        /// <param name="edit">таблица с данными</param>
        /// <param name="nameTable">имя таблицы</param>
        /// <param name="unCol">столбец, неучаствующий в InsertUpdate</param>
        /// <param name="err">номер ошибки</param>
        private void sortingDataToTable(DataTable origin
            , DataTable edit
            , string nameTable
            , string unCol
            , out int err)
        {
            string nameTableExtrmRow = string.Empty
                          , nameTableNew = string.Empty;
            DataTable editTemporary = new DataTable()
                , originTemporary = new DataTable();

            err = -1;
            editTemporary = edit.Clone();
            originTemporary = origin.Clone();
            nameTableNew = nameTable;

            foreach (DataRow row in edit.Rows)
            {
                nameTableExtrmRow = extremeRow(row["DATE_TIME"].ToString(), nameTableNew);

                if (nameTableExtrmRow != nameTableNew)
                {
                    foreach (DataRow rowOrigin in origin.Rows)
                        if (Convert.ToDateTime(rowOrigin["DATE_TIME"]).Month != Convert.ToDateTime(row["DATE_TIME"]).Month)
                            originTemporary.Rows.Add(rowOrigin.ItemArray);

                    updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);

                    nameTableNew = nameTableExtrmRow;
                    editTemporary.Rows.Clear();
                    originTemporary.Rows.Clear();
                    editTemporary.Rows.Add(row.ItemArray);
                }
                else
                    editTemporary.Rows.Add(row.ItemArray);
            }

            if (editTemporary.Rows.Count > 0)
            {
                foreach (DataRow rowOrigin in origin.Rows)
                    if (extremeRow(Convert.ToDateTime(rowOrigin["DATE_TIME"]).ToString(), nameTableNew) == nameTableNew)
                        originTemporary.Rows.Add(rowOrigin.ItemArray);

                updateInsertDel(nameTableNew, originTemporary, editTemporary, unCol, out err);
            }
        }

        /// <summary>
        /// Нахождение имени таблицы для крайних строк
        /// </summary>
        /// <param name="strDate">дата</param>
        /// <param name="nameTable">изначальное имя таблицы</param>
        /// <returns>имя таблицы</returns>
        private static string extremeRow(string strDate, string nameTable)
        {
            DateTime dtStr = Convert.ToDateTime(strDate);
            string newNameTable = dtStr.Year.ToString() + dtStr.Month.ToString(@"00");
            string[] pref = nameTable.Split('_');

            return pref[0] + "_" + newNameTable;
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(сыр.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE;

            onButtonLoadClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки загрузить(арх.)
        /// </summary>
        /// <param name="obj">Составной объект - календарь</param>
        /// <param name="ev">Аргумент события</param>
        private void HPanelTepCommon_btnHistory_Click(object obj, EventArgs ev)
        {
            Session.m_ViewValues = HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE;

            onButtonLoadClick();
        }

        /// <summary>
        /// оброботчик события кнопки
        /// </summary>
        protected virtual void onButtonLoadClick()
        {
            // ... - загрузить/отобразить значения из БД
            updateDataValues();
        }
    }

    /// <summary>
    /// Класс для взамодействия с основным приложением (вызывающая программа)
    /// </summary>
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 21;
            register(21, typeof(PanelTaskVedomostBl), @"Задача", @"Ведомости эн./блоков");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}



