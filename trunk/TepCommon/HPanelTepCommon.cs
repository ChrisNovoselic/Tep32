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
        protected HandlerDbTaskCalculate m_handlerDb;

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

            m_handlerDb = new HandlerDbTaskCalculate(ID_TASK.UNKNOWN);
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

            Label ctrl = new Label();
            ctrl.Name = id;
            ctrl.Dock = DockStyle.Top;
            gbDesc.Controls.Add(ctrl);
        }
        /// <summary>
        /// Для отображения актуальной "подсказки" для свойства
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected void HPanelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
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
}
