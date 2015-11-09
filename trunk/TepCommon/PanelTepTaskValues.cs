using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public abstract partial class PanelTepTaskValues : HPanelTepCommon
    {
        private enum INDEX_TABLE_DICTPRJ : uint { PERIOD, COMPONENT, PARAMETER
            , COUNT_TABLE_DICTPRJ }
        /// <summary>
        /// Наименования таблиц с парметрами для расчета
        /// </summary>
        private string m_strNameTableAlg
            , m_strNameTablePut;
        /// <summary>
        /// Массив с идентификаторами периодов расчетов, использующихся на форме
        /// </summary>
        protected int[] m_arIdPeriods;
        /// <summary>
        /// Строка для запроса информации по периодам расчетов
        /// </summary>
        protected string m_strIdPeriods
        {
            get
            {
                string strRes = string.Empty;

                for (int i = 0; i < m_arIdPeriods.Length; i++)
                {
                    strRes += m_arIdPeriods[i] + @",";
                }
                strRes = strRes.Substring(0, strRes.Length - 1);

                return strRes;
            }
        }
        /// <summary>
        /// Таблицы со значениями словарных, проектных данных
        /// </summary>
        private DataTable []m_arTableDictPrjs;

        private List<int> m_listIdParameters;
        /// <summary>
        /// Таблицы со значениями для редактирования
        /// </summary>
        protected DataTable m_tblEdit
            , m_tblOrigin;
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        public PanelTepTaskValues(IPlugIn iFunc, string strNameTableAlg, string strNameTablePut)
            : base(iFunc)
        {
            m_strNameTableAlg = strNameTableAlg;
            m_strNameTablePut = strNameTablePut;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            m_arIdPeriods = new int[] { 13, 18, 19, 24 };
            m_arTableDictPrjs = new DataTable [(int)INDEX_TABLE_DICTPRJ.COUNT_TABLE_DICTPRJ];
            m_listIdParameters = new List<int>();

            m_panelManagement = new PanelManagement ();
            m_dgvValues = new DataGridViewTEPValues ();
            int posColdgvTEPValues = 4
                , hightRowdgvTEPValues = 10;

            SuspendLayout ();

            initializeLayoutStyle ();

            Controls.Add (m_panelManagement, 0, 0);
            SetColumnSpan(m_panelManagement, posColdgvTEPValues); SetRowSpan(m_panelManagement, this.RowCount);

            Controls.Add(m_dgvValues, posColdgvTEPValues, 0);
            SetColumnSpan(m_dgvValues, this.ColumnCount - posColdgvTEPValues); SetRowSpan(m_dgvValues, hightRowdgvTEPValues);

            addLabelDesc(INDEX_CONTROL.LABEL_DESC.ToString(), posColdgvTEPValues, hightRowdgvTEPValues);

            ResumeLayout (false);
            PerformLayout ();

            (Controls.Find(INDEX_CONTROL.BUTTON_LOAD.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnUpdate_Click);
            (Controls.Find(INDEX_CONTROL.BUTTON_SAVE.ToString(), true)[0] as Button).Click += new EventHandler(HPanelTepCommon_btnSave_Click);
        }

        protected override void initialize(ref System.Data.Common.DbConnection dbConn, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            Control ctrl = null;
            CheckedListBox clbxCompCalculated
                , clbxCompVisibled;
            string strItem = string.Empty;
            int i = -1;
            //Заполнить таблицы со словарными, проектными величинами
            string[] arQueryDictPrj = new string[]
            {
                @"SELECT * FROM [time] WHERE [ID] IN (" + m_strIdPeriods + @")"
                , @"SELECT * FROM [comp_list] "
                    + @"WHERE ([ID] = 5 AND [ID_COMP] = 1)"
                        + @" OR ([ID_COMP] = 1000)"
                , @"SELECT put.*, alg.* FROM [dbo].[" + m_strNameTablePut + @"] as put"
                    + @" JOIN [dbo].[" + m_strNameTableAlg + @"] as alg ON alg.ID_TASK = 1 AND alg.ID = put.ID_ALG AND put.ID_TIME in (" + m_strIdPeriods + @")"
            };

            for (i = (int)INDEX_TABLE_DICTPRJ.PERIOD; i < (int)INDEX_TABLE_DICTPRJ.COUNT_TABLE_DICTPRJ; i++)
            {
                m_arTableDictPrjs[i] = DbTSQLInterface.Select(ref dbConn, arQueryDictPrj[i], null, null, out err);

                if (!(err == 0))
                    break;
                else
                    ;
            }

            if (err == 0)
            {
                //Заполнить элемент управления с периодами расчета
                ctrl = Controls.Find(INDEX_CONTROL.CBX_PERIOD.ToString(), true)[0];
                foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PERIOD].Rows)
                    (ctrl as ComboBox).Items.Add (r[@"DESCRIPTION"]);

                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxPeriod_SelectedIndexChanged);
                (ctrl as ComboBox).SelectedIndex = 0;

                //Заполнить элементы управления с компонентами станции
                clbxCompCalculated = Controls.Find(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString(), true)[0] as CheckedListBox;
                clbxCompVisibled = Controls.Find(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString(), true)[0] as CheckedListBox;
                foreach (DataRow r in m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.COMPONENT].Rows)
                {
                    strItem = (string)r[@"DESCRIPTION"];
                    clbxCompCalculated.Items.Add(strItem);
                    clbxCompVisibled.Items.Add(strItem);
                }
            }
            else
                switch ((INDEX_TABLE_DICTPRJ)i)
                {
                    case INDEX_TABLE_DICTPRJ.PERIOD:
                        errMsg = @"Получение интервалов времени для периода расчета";
                        break;
                    case INDEX_TABLE_DICTPRJ.COMPONENT:
                        errMsg = @"Получение списка компонентов станции";
                        break;
                    case INDEX_TABLE_DICTPRJ.PARAMETER:
                        errMsg = @"Получение строковых идентификаторов параметров в алгоритме расчета";
                        break;
                    default:
                        break;
                }
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            return bRes;
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
        }

        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
        }

        private void cbxPeriod_SelectedIndexChanged(object obj, EventArgs ev)
        {
            ComboBox cbx = obj as ComboBox;
            int id_alg = -1;
            string strItem = string.Empty;
            CheckedListBox clbxParsCalculated = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString(), true)[0] as CheckedListBox
                , clbxParsVisibled = Controls.Find(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString(), true)[0] as CheckedListBox;
            clbxParsCalculated.Items.Clear();
            clbxParsVisibled.Items.Clear();
            m_listIdParameters.Clear();
            ////Запросить значения у главной формы
            //((PlugInBase)_iFuncPlugin).DataAskedHost(new object[] { (int)HFunc.ID_DATAASKED_HOST.SELECT, @"SELECT..." });
            DataRow[] rowsParameter =
                m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select(@"ID_TIME=" + m_arIdPeriods[cbx.SelectedIndex], @"N_ALG");
            //Заполнить элементы управления с компонентами станции 
            foreach (DataRow r in rowsParameter)
            {
                id_alg = (int)r[@"ID_ALG"];

                if (m_listIdParameters.IndexOf(id_alg) < 0)
                {
                    m_listIdParameters.Add (id_alg);

                    strItem = ((string)r[@"N_ALG"]).Trim () + @" (" + ((string)r[@"NAME_SHR"]).Trim() + @")";
                    clbxParsCalculated.Items.Add(strItem);
                    clbxParsVisibled.Items.Add(strItem);
                }
                else
                    ;
            }
        }

        protected class DataGridViewTEPValues : DataGridView
        {
            public DataGridViewTEPValues ()
            {
                InitializeComponents ();
            }

            private void InitializeComponents()
            {
                this.Dock = DockStyle.Fill;
            }
        }

        protected class PanelManagement : HPanelCommon
        {
            private class HDateTimePicker : HPanelCommon
            {
                private int _iYear
                    , _iMonth
                    , _iDay
                    , _iHour;

                public HDateTimePicker(int year, int month, int day, int hour = 1)
                    : base(12, 1)
                {
                    _iYear = year;
                    _iMonth = month;
                    _iDay = day;
                    _iHour = hour;

                    InitializeComponents();
                }
                
                private string[] months = { @"январь", @"февраль", @"март"
                    , @"апрель", @"май", @"июнь"
                    , @"июль", @"август", @"сентябрь"
                    , @"октябрь", @"ноябрь", @"декабрь" };
                
                //public HDateTimePicker () : base (12, 1)
                //{
                //    InitializeComponents ();
                //}

                private void InitializeComponents()
                {
                    Control ctrl;
                    int i = -1;

                    SuspendLayout();

                    initializeLayoutStyle ();

                    //Дата - номер дня
                    ctrl = new ComboBox();
                    ctrl.Dock = DockStyle.Fill;
                    (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                    Controls.Add (ctrl, 0, 0);
                    SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);                    
                    for (i = 0; i < 31; i ++)
                        (ctrl as ComboBox).Items.Add (i + 1);
                    (ctrl as ComboBox).SelectedIndex = _iDay - 1;

                    //Дата - наименование месяца
                    ctrl = new ComboBox ();
                    ctrl.Dock = DockStyle.Fill;
                    (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                    Controls.Add(ctrl, 2, 0);
                    SetColumnSpan(ctrl, 5); SetRowSpan(ctrl, 1);
                    for (i = 0; i < 12; i++)
                        (ctrl as ComboBox).Items.Add(months[i]);
                    (ctrl as ComboBox).SelectedIndex = _iMonth - 1;

                    //Дата - год
                    ctrl = new ComboBox();
                    ctrl.Dock = DockStyle.Fill;
                    (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                    Controls.Add(ctrl, 7, 0);
                    SetColumnSpan(ctrl, 3); SetRowSpan(ctrl, 1);
                    for (i = 10; i < 21; i++)
                        (ctrl as ComboBox).Items.Add(@"20" + i.ToString ());
                    (ctrl as ComboBox).SelectedIndex = _iYear - (2000 + 10);

                    //Время - час
                    ctrl = new ComboBox();
                    ctrl.Dock = DockStyle.Fill;
                    (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                    Controls.Add(ctrl, 10, 0);
                    SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                    for (i = 0; i < 24; i++)
                        (ctrl as ComboBox).Items.Add(i + 1);
                    (ctrl as ComboBox).SelectedIndex = _iHour - 1;

                    ResumeLayout(false);
                    PerformLayout();
                }

                protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
                {
                    initializeLayoutStyleEvenly();
                }
            }

            public PanelManagement() : base (8, 21)
            {
                InitializeComponents ();
            }

            private void InitializeComponents ()
            {
                Control ctrl = null;
                int posRow = -1;

                SuspendLayout();

                initializeLayoutStyle();

                posRow = 0;
                //Период расчета
                ////Период расчета - подпись
                //ctrl = new System.Windows.Forms.Label();
                //ctrl.Dock = DockStyle.Bottom;
                //(ctrl as System.Windows.Forms.Label).Text = @"Период:";
                //this.Controls.Add(ctrl, 0, posRow);
                //SetColumnSpan(ctrl, 2); SetRowSpan(ctrl, 1);
                //Период расчета - значение
                ctrl = new ComboBox ();
                ctrl.Name = INDEX_CONTROL.CBX_PERIOD.ToString ();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Расчет - выполнить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN.ToString();
                ctrl.Text = @"Выполнить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Дата/время начала периода расчета
                //Дата/время начала периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время начала периода расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время начала периода расчета - значения
                ctrl = new HDateTimePicker(2015, 1, 1);
                ctrl.Name = INDEX_CONTROL.HDTP_BEGIN.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета
                //Дата/время  окончания периода расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Дата/время  окончания периода расчета:";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Дата/время  окончания периода расчета - значения
                ctrl = new HDateTimePicker(2015, 1, 1, 24);
                ctrl.Name = INDEX_CONTROL.HDTP_END.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);

                //Признаки включения/исключения из расчета
                //Признаки включения/исключения из расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить из расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                //Признак для включения/исключения из расчета компонента
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения из расчета параметра
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Кнопка - импортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_IMPORT.ToString();
                ctrl.Text = @"Импорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);                
                //Кнопка - экспортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения для отображения параметра
                ctrl = new CheckedListBox();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                ResumeLayout(false);
                PerformLayout();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly ();
            }
        }
    }

    public partial class PanelTepTaskValues
    {
        protected enum INDEX_CONTROL { BUTTON_RUN
            , CBX_PERIOD, HDTP_BEGIN, HDTP_END
            , CLBX_COMP_CALCULATED, CLBX_PARAMETER_CALCULATED
            , BUTTON_LOAD, BUTTON_SAVE, BUTTON_IMPORT, BUTTON_EXPORT
            , CLBX_COMP_VISIBLED, CLBX_PARAMETER_VISIBLED
            , LABEL_DESC }

        protected PanelManagement m_panelManagement;
        protected DataGridViewTEPValues m_dgvValues;
    }

    public class PlugInTepTaskValues : HFuncDbEdit
    {
        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
