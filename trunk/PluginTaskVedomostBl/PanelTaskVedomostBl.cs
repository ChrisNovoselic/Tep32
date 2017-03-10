using HClassLibrary;
using InterfacePlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TepCommon;

namespace PluginTaskVedomostBl
{
    public partial class PanelTaskVedomostBl : HPanelTepCommon
    {
        /// <summary>
        /// ??? переменная с текущем отклоненеим от UTC
        /// </summary>
        static int s_currentOffSet;
        /// <summary>
        /// Для обозначения выбора 1 или 6 блоков
        /// </summary>
        static bool s_flagBl = true;
        ///// <summary>
        ///// ??? Делегат (возврат пикчи по Ид)
        ///// </summary>
        ///// <param name="id">ид грида</param>
        ///// <returns>picture</returns>
        //public delegate PictureBox PictureBoxDelegateIntFunc(int id);
        ///// <summary>
        ///// ??? Делегат 
        ///// </summary>
        ///// <returns>грид</returns>
        //public delegate DataGridView DataGridViewDelegateFunc();
        /// <summary>
        /// ??? экземпляр делегата(возврат пикчи по Ид)
        /// </summary>
        static public Func<int, PictureBox> s_getPicture;
        /// <summary>
        /// ??? экземпляр делегата(возврат отображения активного)
        /// </summary>
        static public Func<DataGridView> s_getDGV;
        /// <summary>
        /// ??? экземпляр делегата(возврат Ид)
        /// </summary>
        static public Func<int> s_getIdComp;
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
        protected Dictionary<int, List<string[]>> m_dictHeaderBlock;
        /// <summary>
        /// Лист с группами хидеров отображения
        /// </summary>
        protected static List<List<string>> s_listGroupHeaders = new List<List<string>> {
            // группа №1
            new List<string> { "Острый пар", "Горячий промперегрев", "ХПП" }
            // группа №2
            , new List<string> { "Питательная вода", "Продувка", "Конденсатор", "Холодный воздух", "Горячий воздух", "Кислород", "VI отбор", "VII отбор" }
            // группа №3
            , new List<string> { "Уходящие газы", "", "", "", "РОУ", "Сетевая вода", "Выхлоп ЦНД" }
        };
        /// <summary>
        /// Набор элементов
        /// </summary>
        protected enum INDEX_CONTROL
        {
            UNKNOWN = -1
            /*, DGV_DATA_B1, DGV_DATA_B2, DGV_DATA_B3, DGV_DATA_B4, DGV_DATA_B5, DGV_DATA_B6
            , RADIOBTN_BLK1, RADIOBTN_BLK2, RADIOBTN_BLK3, RADIOBTN_BLK4, RADIOBTN_BLK5, RADIOBTN_BLK6*/
            , LABEL_DESC, TBLP_HGRID, PICTURE_BOXDGV, PANEL_PICTUREDGV
                , COUNT
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
            /*PERIOD, // идентификаторы периодов расчетов, использующихся на форме
            TIMEZONE, // идентификаторы (целочисленные, из БД системы) часовых поясов
            ALL_COMPONENT, ALL_NALG, // все идентификаторы компонентов ТЭЦ/параметров*/
            //DENY_COMP_CALCULATED, 
            DENY_GROUPHEADER_VISIBLED,
            BLOCK_SELECTED, HGRID_VISIBLED,
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
        /// 
        /// </summary>
        protected ReportExcel m_rptExcel;
        /// <summary>
        /// экземпляр класса пикчи
        /// </summary>
        protected PictureVedBl m_pictureVedBl;
        /// <summary>
        /// ??? почему статик Экземпляр класса обрабокти данных
        /// </summary>
        private static VedomostBlCalculate s_VedCalculate;
        /// <summary>
        /// 
        /// </summary>
        protected DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }
        }
        /// <summary>
        /// ???
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
            public PictureVedBl(DataGridViewVedomostBl viewActive)
            {
                InitializeComponents(viewActive);
            }

            /// <summary>
            /// Инициализация компонента
            /// </summary>
            /// <param name="viewActive">??? активный грид</param>
            private void InitializeComponents(DataGridViewVedomostBl viewActive)
            {
                int _drwH = (viewActive.Rows.Count) * viewActive.Rows[0].Height + 70;

                Size = new Size(viewActive.Width - 10, _drwH);
                m_idCompPicture = (int)viewActive.Tag;
                Controls.Add(viewActive);
            }
        }

        /// <summary>
        ///класс для обработки данных
        /// </summary>
        public class VedomostBlCalculate : HandlerDbTaskCalculate.TaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public VedomostBlCalculate()
                : base()
            {

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
            /// преобразование числа в нужный формат отображения
            /// </summary>
            /// <param name="value">число</param>
            /// <returns>преобразованное число</returns>
            public float AsParseToF(string value)
            {
                float fRes = 0;

                int _indxChar = 0;
                string _sepReplace = string.Empty;
                bool bParsed = true;
                //char[] _separators = { ' ', ',', '.', ':', '\t'};
                //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();                

                foreach (char ch in value.ToCharArray())
                {
                    if (!char.IsDigit(ch))
                        if (char.IsLetter(ch))
                            value = value.Remove(_indxChar, 1);
                        else
                            _sepReplace = value.Substring(_indxChar, 1);
                    else
                        _indxChar++;

                    switch (_sepReplace) {
                        case ".":
                        case ",":
                        case " ":
                        case ":":
                            bParsed = float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fRes);
                            break;
                    }
                }

                if (bParsed == false)
                    try {
                        fRes = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    } catch (Exception) {
                        if (string.IsNullOrEmpty (value.ToString()) == true)
                            fRes = 0;
                        else
                            ;
                    }

                return fRes;
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
            m_dictHeaderBlock = new Dictionary<int, List<string[]>> { };

            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT];

            InitializeComponent();

            s_getPicture = new Func <int, PictureBox> (GetPictureOfIdComp);
            s_getDGV = new Func<DataGridView>(GetDGVOfIdComp);
            s_getIdComp = new Func<int>(GetIdComp);
        }

        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
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
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_UPDATE.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(panelTepCommon_btnUpdate_onClick);
            (btn.ContextMenuStrip.Items.Find(PanelManagementVedomostBl.INDEX_CONTROL.MENUITEM_HISTORY.ToString(), true)[0] as ToolStripMenuItem).Click +=
                new EventHandler(HPanelTepCommon_btnHistory_Click);
            (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(panelTepCommon_btnSave_onClick);
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
            DataGridViewVedomostBl cntrl = (getActiveView() as DataGridViewVedomostBl);
            //Поиск индекса элемента отображения
            switch ((INDEX_ID)item.m_indxId) {
                case INDEX_ID.HGRID_VISIBLED:
                    cntrl.HideColumns(cntrl as DataGridView, s_listGroupHeaders[item.m_idItem], bItemChecked);
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
        private void ReSizeControls(DataGridView dgv)
        {
            int cntVisibleColumns = 0;

            foreach (DataGridViewColumn col in dgv.Columns) {
                if (dgv.Columns.IndexOf(col) > 0)
                    col.Width = 65;
                else
                    ;

                if (col.Visible == true)
                    cntVisibleColumns++;
                else
                    ;
            }

            int _drwW = cntVisibleColumns * dgv.Columns[2].Width + 10
                , _drwH = (dgv.Rows.Count) * dgv.Rows[0].Height + 70;

            GetPictureOfIdComp((int)(dgv as DataGridViewVedomostBl).Tag).Size = new Size(_drwW + 2, _drwH);
            dgv.Size = new Size(_drwW + 2, _drwH);
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
                foreach (DataGridViewVedomostBl item in picture.Controls)
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
                    foreach (DataGridViewVedomostBl item in picture.Controls)
                        if (item.Visible == true)
                            _idComp = (int)item.Tag;

            return _idComp;
        }

        /// <summary>
        /// Настройка размеров формы отображения данных
        /// </summary>
        /// <param name="dgv">активное окно отображения данных</param>
        public void ConfigureDataGridView(DataGridView dgv)
        {
            (dgv as DataGridViewVedomostBl).ConfigureColumns(dgv as DataGridView);
        }

        /// <summary>
        /// Инициализация сетки данных
        /// </summary>
        /// <param name="namePut">массив имен элементов</param>
        /// <param name="err">номер ошибки</param>
        /// <param name="errMsg">текст ошибки</param>
        private void initializeDataGridView(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            int i = -1
                , idPar = -1
                , avg = -1
                , idComp = -1;
            DataGridViewVedomostBl dgv = null;
            DateTime dtRow = PanelManagement.DatetimeRange.Begin;
            //DataTable tableComponentId; // ид компонентов
            Dictionary<string, List<int>> dictVisualSett;

            m_dictHeaderBlock.Clear();

            //создание грида со значениями
            for (i = 0; i < m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows.Count; i++) {
                dgv = new DataGridViewVedomostBl(int.Parse(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST].Rows[i]["ID"].ToString()));
                //??? исключить такое имя (ориентироваться на Tag)
                dgv.Name = string.Format(@"DGV_DATA_B{0}", (i + 1));
                dgv.BlockCount = i + 1;

                m_dictHeaderBlock.Add((int)dgv.Tag, GetListHeaders(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER], (int)dgv.Tag)); // cловарь заголовков
                //??? каждый раз получаем полный список и выбираем необходимый
                dictVisualSett = getVisualSettingsOfIdComponent((int)dgv.Tag);

                for (int k = 0; k < m_dictHeaderBlock[(int)dgv.Tag].Count; k++) {
                    idPar = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["ID_ALG"].ToString());
                    avg = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["AVG"].ToString());
                    idComp = int.Parse(m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER].Select("ID_COMP = " + (int)dgv.Tag)[k]["ID"].ToString());

                    dgv.AddColumns(idPar
                        , new DataGridViewVedomostBl.COLUMN_PROPERTY {
                            topHeader = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.TOP].ToString(),
                            nameCol = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.MIDDLE].ToString(),
                            hdrText = m_dictHeaderBlock[(int)dgv.Tag][k][(int)DataGridViewVedomostBl.INDEX_HEADER.LOW].ToString(),
                            m_idAlg = idPar,
                            m_IdComp = idComp,
                            m_vsRatio = dictVisualSett["ratio"][k],
                            m_vsRound = dictVisualSett["round"][k],
                            m_Avg = avg
                        }
                       , true);
                }

                for (i = 0; i < DaysInMonth + 1; i++)
                    if (dgv.Rows.Count != DaysInMonth)
                        dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY() {
                            //m_idAlg = id_alg
                            //,
                            m_Value = dtRow.AddDays(i).ToShortDateString()
                        });
                    else {
                        dgv.RowsAdded += DGVVedomostBl_RowsAdded;

                        dgv.AddRow(
                            new DataGridViewVedomostBl.ROW_PROPERTY() {
                                //m_idAlg = id_alg
                                //,
                                m_Value = "ИТОГО"
                            }
                            , DaysInMonth
                        );
                    }

                ConfigureDataGridView(dgv);
                m_pictureVedBl = new PictureVedBl(dgv);
                (Controls.Find(INDEX_CONTROL.PANEL_PICTUREDGV.ToString(), true)[0] as Panel).Controls.Add(m_pictureVedBl);
                //возможность_редактирвоания_значений
                try {
                    if (m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()).Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()) == true)
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked =
                            int.Parse(m_dictProfile.GetObjects(((int)ID_PERIOD.MONTH).ToString(), ((int)PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT).ToString()).Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.EDIT_COLUMN).ToString()]) == (int)MODE_CORRECT.ENABLE;
                    else
                        (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked = false;

                    if ((Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.CHKBX_EDIT.ToString(), true)[0] as CheckBox).Checked)
                        for (int t = 0; t < dgv.RowCount; t++)
                            dgv.ReadOnlyColumns = false;
                    else
                        ;
                } catch (Exception e) {
                //???
                    Logging.Logg().Exception (e, string.Format(@"PanelVedomostBl::InitializeDataGridView () - ошибки проверки возможности редактирования ячеек..."), Logging.INDEX_MESSAGE.NOT_SET);
                }
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

            ID_PERIOD idProfilePeriod = ID_PERIOD.UNKNOWN;
            string strItem = string.Empty;
            Control ctrl = null;

            m_arListIds = new List<int>[(int)INDEX_ID.COUNT];

            foreach (INDEX_ID id in Enum.GetValues(typeof(INDEX_ID)))
                switch (id) {
                    /*case INDEX_ID.PERIOD:
                        m_arListIds[(int)id] = new List<int> { (int)ID_PERIOD.HOUR, (int)ID_PERIOD.DAY, (int)ID_PERIOD.MONTH };
                        break;
                    case INDEX_ID.TIMEZONE:
                        m_arListIds[(int)id] = new List<int> { (int)ID_TIMEZONE.UTC, (int)ID_TIMEZONE.MSK, (int)ID_TIMEZONE.NSK };
                        break;
                    case INDEX_ID.ALL_COMPONENT:
                        m_arListIds[(int)id] = new List<int> { };
                        break;*/
                    default:
                        //??? где получить запрещенные для расчета/отображения идентификаторы компонентов ТЭЦ\параметров алгоритма
                        m_arListIds[(int)id] = new List<int>();
                        break;
                }

            //Заполнить таблицы со словарными, проектными величинами
            // PERIOD, TIMWZONE, COMP, PARAMETER, RATIO
            initialize
            //m_markTableDictPrj = new HMark
                (new ID_DBTABLE[] { /*ID_DBTABLE.PERIOD
                    , */ID_DBTABLE.TIME, ID_DBTABLE.TIMEZONE
                    , ID_DBTABLE.COMP_LIST
                    , TaskCalculateType == HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES ? ID_DBTABLE.IN_PARAMETER : ID_DBTABLE.UNKNOWN
                    , ID_DBTABLE.RATIO }
                , out err, out errMsg
            );

            m_dictTableDictPrj.FilterDbTableTimezone = DictionaryTableDictProject.DbTableTimezone.Msk;
            m_dictTableDictPrj.FilterDbTableTime = DictionaryTableDictProject.DbTableTime.Month;
            m_dictTableDictPrj.FilterDbTableCompList = DictionaryTableDictProject.DbTableCompList.Tg;

            //Dgv's
            initializeDataGridView(out err, out errMsg); //???
            //// панель управления - очистка
            //PanelManagement.Clear();
            //радиобаттаны
            PanelManagement.AddComponent(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], out err, out errMsg);
            //groupHeader
            PanelManagement.AddCheckBoxGroupHeaders(m_dictTableDictPrj[ID_DBTABLE.COMP_LIST], out err, out errMsg);
            //активность_кнопки_сохранения
            try {
                if (m_dictProfile.Attributes.ContainsKey(((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()) == true)
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled =
                        int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.IS_SAVE_SOURCE).ToString()]) == (int)MODE_CORRECT.ENABLE;
                else
                    (Controls.Find(PanelManagementVedomostBl.INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Enabled = false;
            } catch (Exception e) {
                // ???
                Logging.Logg().Exception(e, string.Format(@"PanelTaskVedomostBl::initialize () - BUTTON_SAVE.Enabled..."), Logging.INDEX_MESSAGE.NOT_SET);

                err = -2;
            }

            if (err == 0) {
                try {
                    m_bflgClear = !m_bflgClear;

                    //Заполнить элемент управления с часовыми поясами
                    PanelManagement.FillValueTimezone(m_dictTableDictPrj[ID_DBTABLE.TIMEZONE]
                        , ID_TIMEZONE.MSK);
                    //Заполнить элемент управления с периодами расчета
                    idProfilePeriod = (ID_PERIOD)int.Parse(m_dictProfile.Attributes[((int)HTepUsers.HTepProfilesXml.PROFILE_INDEX.PERIOD).ToString()]);
                    PanelManagement.FillValuePeriod(m_dictTableDictPrj[ID_DBTABLE.TIME]
                        , idProfilePeriod);

                    Session.SetCurrentPeriod(PanelManagement.IdPeriod);

                    PanelManagement.SetModeDatetimeRange();
                    PanelManagement.AllowedTimezone = false;
                    PanelManagement.AllowedPeriod = false;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"PanelTaskVedomostBl::initialize () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                Logging.Logg().Error(MethodBase.GetCurrentMethod(), @"...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        #region Обработка измнения значений основных элементов управления на панели управления 'PanelManagement'
        /// <summary>
        /// Обработчик события при изменении значения
        ///  одного из основных элементов управления на панели управления 'PanelManagement'
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_OnEventIndexControlBaseValueChanged(object obj)
        {
            base.panelManagement_OnEventIndexControlBaseValueChanged(obj);

            if (obj is Enum)
                ; // switch ()
            else
                ;
        }

        //protected override void panelManagement_OnEventDetailChanged(object obj)
        //{
        //    base.panelManagement_OnEventDetailChanged(obj);
        //}
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение даты/времени, диапазона даты/времени)
        /// </summary>
        protected override void panelManagement_DatetimeRangeChanged()
        {
            base.panelManagement_DatetimeRangeChanged();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_TimezoneChanged()
        {
            base.panelManagement_TimezoneChanged();
        }
        /// <summary>
        /// Метод при обработке события 'EventIndexControlBaseValueChanged' (изменение часового пояса)
        /// </summary>
        protected override void panelManagement_PeriodChanged()
        {
            base.panelManagement_PeriodChanged();
        }
        /// <summary>
        /// Обработчик события - добавить NAlg-параметр
        /// </summary>
        /// <param name="obj">Объект - NAlg-параметр(основной элемент алгоритма расчета)</param>
        protected override void onAddNAlgParameter(NALG_PARAMETER obj)
        {
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void onAddPutParameter(PUT_PARAMETER obj)
        {
        }
        /// <summary>
        /// Обработчик события - добавить NAlg - параметр
        /// </summary>
        /// <param name="obj">Объект - компонент станции(оборудование)</param>
        protected override void onAddComponent(object obj)
        {
        }
        #endregion

        /// <summary>
        /// Получение визуальных настроек 
        /// для отображения данных на форме
        /// </summary>
        /// <param name="idComp">идКомпонента</param>
        /// <returns>словарь настроечных данных</returns>
        private Dictionary<string, List<int>> getVisualSettingsOfIdComponent(int idComp)
        {
            Dictionary<string, List<int>> dictSettRes = new Dictionary<string, List<int>>();

            int err = -1
             , id_alg = -1;
            Dictionary<string, HTepUsers.VISUAL_SETTING> dictVisualSettings = new Dictionary<string, HTepUsers.VISUAL_SETTING>();
            List<int> ratio = new List<int>()
            , round = new List<int>();
            string n_alg = string.Empty;            

            dictVisualSettings = HTepUsers.GetParameterVisualSettings(m_handlerDb.ConnectionSettings
               , new int[] {
                    m_Id
                    , idComp }
               , out err);

            IEnumerable<DataRow> listParameter = ListParameter.Select(x => x).Where(x => (int)x["ID_COMP"] == idComp);

            foreach (DataRow r in listParameter) {
                id_alg = (int)r[@"ID_ALG"];
                n_alg = r[@"N_ALG"].ToString().Trim();
                //// не допустить добавление строк с одинаковым идентификатором параметра алгоритма расчета
                //if (m_arListIds[(int)INDEX_ID.ALL_NALG].IndexOf(id_alg) < 0)
                //    // добавить в список идентификатор параметра алгоритма расчета
                //    m_arListIds[(int)INDEX_ID.ALL_NALG].Add(id_alg);
                //else
                //    ;

                // получить значения для настройки визуального отображения
                if (dictVisualSettings.ContainsKey(n_alg) == true) {
                // установленные в проекте
                    ratio.Add(dictVisualSettings[n_alg.Trim()].m_ratio);
                    round.Add(dictVisualSettings[n_alg.Trim()].m_round);
                } else {
                // по умолчанию
                    ratio.Add(HTepUsers.s_iRatioDefault);
                    round.Add(HTepUsers.s_iRoundDefault);
                }
            }
            dictSettRes.Add("ratio", ratio);
            dictSettRes.Add("round", round);

            return dictSettRes;
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
        /// Обработчик события - изменение интервала (диапазона между нач. и оконч. датой/временем) расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void datetimeRangeValue_onChanged(DateTime dtBegin, DateTime dtEnd)
        {
            int //err = -1,
              id_alg = -1;
            DataGridViewVedomostBl _dgv = (getActiveView() as DataGridViewVedomostBl);
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
                        _dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY()
                        {
                            m_idAlg = id_alg
                            //, m_strMeasure = ((string)r[@"NAME_SHR_MEASURE"]).Trim()
                            , m_Value = dt.AddDays(i).ToShortDateString()
                        });
                    else
                        _dgv.AddRow(new DataGridViewVedomostBl.ROW_PROPERTY()
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
        protected override List<DataRow> ListParameter
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
                        (getActiveView() as DataGridViewVedomostBl).ShowValues(m_arTableOrigin[(int)Session.m_ViewValues], m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER], Session.m_ViewValues);
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
                m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = HandlerDb.GetDataOutvalArch(TaskCalculateType, HandlerDb.GetDateTimeRangeValuesVarArchive(), out err);
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = HandlerDb.GetValuesVar(
                TaskCalculateType
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
                    HandlerDb.CreateSession(m_Id
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
            return (getActiveView() as DataGridViewVedomostBl).FillTableToSave(m_TableOrigin, (int)Session.m_Id, Session.m_ViewValues);
        }

        /// <summary>
        /// ??? проверка выборки блока(для 1 и 6)
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
        protected override void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
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
        protected override void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
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



