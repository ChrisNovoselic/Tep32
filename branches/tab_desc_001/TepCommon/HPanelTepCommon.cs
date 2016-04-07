using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;

namespace TepCommon
{
    public abstract class HPanelTepCommon : HPanelCommon, IObjectDbEdit
    {
        /// <summary>
        /// Дополнительные действия при сохранении значений
        /// </summary>
        protected DelegateFunc delegateSaveAdding;
        /// <summary>
        /// Объект для реализации взаимодействия с главной программой
        /// </summary>
        protected IPlugIn _iFuncPlugin;
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Apelgans


        public enum ID_DT_DESC { TABLE, PROP };
        public DataTable[] Descriptions = new DataTable[] { new DataTable(), new DataTable() };

        public enum ID_TABLE
        {
            MAIN = 1//Главная
            ,PROP = 2//Свойства
            ,DESC = 3
        };

        /// <summary>
        /// Список групп
        /// </summary>
        string[] m_arr_name_group_panel = { "Настройка", "Проект", "Задача" };

        /// <summary>
        /// Строки для описания групп вкладок
        /// </summary>
        string[] m_description_group = new string[] { "Группа для настроек", "Группа для проектов", "Группа для задач" };
        
        public string m_name_panel_desc = string.Empty;

        #endregion

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public HPanelTepCommon(IPlugIn plugIn) : base (13, 13)
        {
            this._iFuncPlugin = plugIn;

            ////Создать объект "словарь" дочерних элементов управления
            //m_dictControls = new Dictionary<int, Control>();

            InitializeComponent();

            m_handlerDb = createHandlerDb ();
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            initializeLayoutStyle();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }        
        /// <summary>
        /// Объект для обмена данными с БД
        /// </summary>
        protected HandlerDbValues m_handlerDb;

        protected virtual HandlerDbValues createHandlerDb () 
        {
            return new HandlerDbValues (); 
        }

        protected void initializeDescPanel()
        {
            int err = -1;
            if (m_name_panel_desc != string.Empty)
            {
                Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
                string desc = "";
                string name = ((PlugInMenuItem)_iFuncPlugin).GetNameOwnerMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id);
                string[] ar_name = name.Split('\\');
                name = ar_name[0];
                for (int i = 0; i < m_arr_name_group_panel.Length; i++)
                {
                    if (m_arr_name_group_panel[i] == name)
                    {
                        ((HPanelDesc)ctrl).SetLblGroup = new string[] { name, m_description_group[i] };
                    }
                }
                

                //Описание вкладки
                string query = "SELECT DESCRIPTION FROM [dbo].[fpanels] WHERE [ID]=" + ((HFuncDbEdit)_iFuncPlugin)._Id;
                DataTable dt = m_handlerDb.Select(query, out err);
                if (dt.Rows.Count != 0)
                {
                    desc = dt.Rows[0][0].ToString();
                    ((HPanelDesc)ctrl).SetLblTab = new string[] { ((PlugInMenuItem)_iFuncPlugin).GetNameMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id), desc };
                }

                //Описания таблиц
                query = "SELECT * FROM [dbo].[table_description] WHERE [ID_PANEL]=" + ((HFuncDbEdit)_iFuncPlugin)._Id;
                Descriptions[(int)ID_DT_DESC.TABLE] = m_handlerDb.Select(query, out err);

                //Описания параметров
                query = "SELECT * FROM [dbo].[param_description] WHERE [ID_PANEL]=" + ((HFuncDbEdit)_iFuncPlugin)._Id;
                Descriptions[(int)ID_DT_DESC.PROP] = m_handlerDb.Select(query, out err);
                
                if (err != 0)
                {
                    Logging.Logg().Error("TepCommon.HpanelTepCommon initializeDescPanel - Select выполнен с ошибкой: " + err, Logging.INDEX_MESSAGE.NOT_SET);
                }

                DataRow[] rows = Descriptions[(int)ID_DT_DESC.TABLE].Select("ID_TABLE=" + (int)ID_TABLE.MAIN);
                if (rows.Length == 1)
                {
                    ((HPanelDesc)ctrl).SetLblDGV1Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                }

                rows = Descriptions[(int)ID_DT_DESC.TABLE].Select("ID_TABLE=" + (int)ID_TABLE.PROP);
                if (rows.Length == 1)
                {
                    ((HPanelDesc)ctrl).SetLblDGV2Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                }

                rows = Descriptions[(int)ID_DT_DESC.TABLE].Select("ID_TABLE=" + (int)ID_TABLE.DESC);
                if (rows.Length == 1)
                {
                    ((HPanelDesc)ctrl).SetLblDGV3Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                    ((HPanelDesc)ctrl).SetLblDGV3Desc_View = false;
                }
            }
        }

        public void Start(object obj)
        {
            //try
            //{
            //    if (this.IsHandleCreated == true)
            //        //if (this.InvokeRequired == true)
            //        this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
            //    else
            //        ;
            //}
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, @"HPanelEdit::Initialize () - BeginInvoke (initialize) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //}

            Start();

            m_handlerDb.InitConnectionSettings(((EventArgsDataHost)obj).par[0] as ConnectionSettings);
        }
        ///// <summary>
        ///// Инициализация с заданными параметрами соединения с БД 
        ///// </summary>
        ///// <param name="obj">Аргумент (параметры соединения с БД)</param>
        //private void initialize(object obj)
        //{
        //    int err = -1;
        //    string errMsg = string.Empty;

        //    if (((EventArgsDataHost)obj).par[0] is ConnectionSettings)
        //    {
        //        m_connSett = (ConnectionSettings)((EventArgsDataHost)obj).par[0];

        //        err = 0;
        //    }
        //    else
        //        errMsg = @"не корректен тип объекта с параметрами соедиения";

        //    if (err == 0)
        //    {
        //        initialize(out err, out errMsg);
        //    }
        //    else
        //        ;

        //    if (!(err == 0))
        //    {
        //        throw new Exception(@"HPanelEdit::initialize () - " + errMsg);
        //    }
        //    else
        //    {
        //    }
        //}
        ///// <summary>
        ///// Инициализация с предустановленными параметрами соединения с БД
        ///// </summary>
        ///// <param name="err">Признак результатат выполнения функции</param>
        ///// <param name="errMsg">Пояснение в случае возникновения ошибки</param>
        //private void initialize(out int err, out string errMsg)
        //{
        //    int iListenerId = -1;

        //    err = -1;
        //    errMsg = string.Empty;

        //    initialize(out err, out errMsg);            
        //}

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);
            int err = -1;
            string strErrMsg = string.Empty;

            if ((bRes == true)
                && (active == true))
                initialize(out err, out strErrMsg);
            else
                ;
            initializeDescPanel();

            return bRes;
        }
        /// <summary>
        /// Повторная инициализация
        /// </summary>
        protected virtual void reinit()
        {
            int err = -1;
            string errMsg = string.Empty;

            initialize(out err, out errMsg);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelTepCommon::clear () - " + errMsg);
            }
            else
            {
            }
        }

        protected abstract void initialize(out int err, out string errMsg);

        //protected abstract void Activate(bool activate);
        /// <summary>
        /// Добавить область оперативного описания выбранного объекта на вкладке
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="posCol">Позиция-столбец для размещения области описания</param>
        /// <param name="posRow">Позиция-строка для размещения области описания</param>
        protected void addLabelDesc(string id, int posCol = 5, int posRow = 10)
        {
            GroupBox gbDesc = new GroupBox();
            gbDesc.Text = @"Описание";
            gbDesc.Dock = DockStyle.Fill;
            this.Controls.Add(gbDesc, posCol, posRow);
            this.SetColumnSpan(gbDesc, this.ColumnCount - posCol); 
            this.SetRowSpan(gbDesc, this.RowCount - posRow);

            HPanelDesc ctrl = new HPanelDesc();
            ctrl.Name = id;
            gbDesc.Controls.Add(ctrl);
            m_name_panel_desc = id;
        }
        /// <summary>
        /// Для отображения актуальной "подсказки" для свойства
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void HPanelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
            string desc = string.Empty;
            string name = string.Empty;
            try
            {
                if (((DataGridView)obj).SelectedRows.Count>0)
                {
                    name = ((DataGridView)obj).SelectedRows[0].Cells[0].Value.ToString();
                    foreach (DataRow r in Descriptions[(int)ID_DT_DESC.PROP].Rows)
                    {
                        if (name == r["PARAM_NAME"].ToString())
                        {
                            desc = r["DESCRIPTION"].ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            SetDescSelRow(desc, name);

            
        }

        protected void SetDescSelRow(string desc, string name)
        {
            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblRowDesc = new string[] { name, desc };
        }

        protected void addButton(Button ctrl, string id, int posCol, string text)
        {
            ctrl.Name = id;
            
            ctrl.Location = new System.Drawing.Point(1, 1);
            ctrl.Dock = DockStyle.Fill;
            ctrl.Text = text;
            //??? Идентификатор является позицией-столбцом
            this.Controls.Add(ctrl, 0, posCol);
            this.SetColumnSpan(ctrl, 1);
        }
        /// <summary>
        /// Добавить кнопку
        /// </summary>
        /// <param name="id">Идентификатор кнопки (свойство 'Name')</param>
        /// <param name="posCol">Позиция по вертикали</param>
        /// <param name="text">Подпись на кнопке</param>
        protected void addButton(string id, int posCol, string text)
        {
            Button ctrl = new Button();
            
            addButton(ctrl, id, posCol, text);
        }

        protected virtual void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            recUpdateInsertDelete(out err);

            if (!(err == 0))
            {
                errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
            }
            else
            {                    
                successRecUpdateInsertDelete();

                if (!(delegateSaveAdding == null))
                    delegateSaveAdding();
                else
                    ;
            }            

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnSave_Click () - " + errMsg);
            }
            else
                ;
        }

        protected abstract void recUpdateInsertDelete(out int err);
        protected abstract void successRecUpdateInsertDelete();

        protected virtual void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            reinit();
        }
    }


    public class HPanelDesc : TableLayoutPanel
    {
        enum ID_LBL {lblGroup, lblTab, lblDGV1, lblDGV2, lblDGV3, selRow };

        string[] m_desc_lbl = new string[] {"lblGroupDesc", "lblTabDesc", "lblDGV1Desc", "lblDGV2Desc", "lblDGV3Desc", "selRowDesc" };
        string[] m_name_lbl = new string[] { "lblGroupName", "lblTabName", "lblDGV1Name", "lblDGV2Name", "lblDGV3Name", "selRowName" };
        string[] m_name_lbl_text = new string[] { "Группа вкладок: ", "Вкладка: ", "Таблица: ", "Таблица: ", "Таблица: ", "Выбранная строка: " };

        private void Initialize()
        {
            this.SuspendLayout();
            this.ColumnCount = 7;
            this.RowCount = 14;
            int i = 0;
            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.ColumnCount));
            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.RowCount));
            int rows = 0;
            int col = 0;

            this.Dock = DockStyle.Fill;

            Control ctrl = new Control();

            ctrl = new Label();
            ctrl.Name = "obj";
            ctrl.Text = "Объект";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 2);

            col = 2;

            ctrl = new Label();
            ctrl.Name = "desc";
            ctrl.Text = "Описание";
            ctrl.Dock = DockStyle.Fill;
            ctrl.Visible = true;

            this.Controls.Add(ctrl, col, rows);
            this.SetRowSpan(ctrl, 2);
            this.SetColumnSpan(ctrl, 4);

            col = 0;
            rows = 2;

            for (i = 0; i < m_desc_lbl.Length; i++)
            {
                ctrl = new Label();
                ctrl.Name = m_name_lbl[i];
                ctrl.Text = m_name_lbl_text[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 2);

                ctrl = new Label();
                ctrl.Name = m_desc_lbl[i];
                ctrl.Dock = DockStyle.Fill;
                ctrl.Visible = false;

                col = 2;

                this.Controls.Add(ctrl, col, rows);
                this.SetRowSpan(ctrl, 2);
                this.SetColumnSpan(ctrl, 4);
                rows += 2;
                col = 0;
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public HPanelDesc()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Поле описания группы вкладок
        /// </summary>
        public string[] SetLblGroup
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Text = "Группа вкладок: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания вкладки
        /// </summary>
        public string[] SetLblTab
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Text = "Вкладка: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 1
        /// </summary>
        public string[] SetLblDGV1Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public string[] SetLblDGV2Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public string[] SetLblDGV3Desc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = value[1];
                ctrl.Visible = true;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Text = "Таблица: " + value[0];
                ctrl.Visible = true;
            }
        }

        /// <summary>
        /// Поле описания выбранной строки
        /// </summary>
        public string[] SetLblRowDesc
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[1] != string.Empty)
                {
                    ctrl.Text = value[1];
                    ctrl.Visible = true;
                }
                else
                    ctrl.Visible = false;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                if (value[0] != string.Empty)
                {
                    value[0].Replace('_', ' ');
                    ctrl.Text = "Cтрока: " + value[0];
                    ctrl.Visible = true;
                }

            }
        }

        /// <summary>
        /// Поле отображения описания группы вкладок
        /// </summary>
        public bool SetLblGroup_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblGroup], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания вкладки
        /// </summary>
        public bool SetLblTab_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblTab], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания таблицы 1
        /// </summary>
        public bool SetLblDGV1Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV1], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 2
        /// </summary>
        public bool SetLblDGV2Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV2], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле описания таблицы 3
        /// </summary>
        public bool SetLblDGV3Desc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.lblDGV3], true)[0];
                ctrl.Visible = value;
            }
        }

        /// <summary>
        /// Поле отображения описания выбранной строки
        /// </summary>
        public bool SetLblRowDesc_View
        {
            set
            {
                Control ctrl = new Control();
                ctrl = this.Controls.Find(m_desc_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

                ctrl = this.Controls.Find(m_name_lbl[(int)ID_LBL.selRow], true)[0];
                ctrl.Visible = value;

            }
        }

    }
}
