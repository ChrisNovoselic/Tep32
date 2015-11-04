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
    public abstract class HPanelTepCommon : HPanelCommon, IObjectDictEdit
    {
        //Дополнительные действия при сохранении значений
        protected DelegateIntFunc delegateSaveAdding;

        protected ConnectionSettings m_connSett;
        protected IPlugIn _iFuncPlugin;

        protected Dictionary<int, Control> m_dictControls;

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

            //Создать объект "словарь" дочерних элементов управления
            m_dictControls = new Dictionary<int, Control>();

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

        public void Initialize(object obj)
        {
            try
            {
                if (this.IsHandleCreated == true)
                    //if (this.InvokeRequired == true)
                    this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HPanelEdit::Initialize () - BeginInvoke (initialize) - ...");
            }
        }

        private void initialize(object obj)
        {
            int err = -1;
            string errMsg = string.Empty;

            if (((EventArgsDataHost)obj).par[0] is ConnectionSettings)
            {
                m_connSett = (ConnectionSettings)((EventArgsDataHost)obj).par[0];

                err = 0;
            }
            else
                errMsg = @"не корректен тип объекта с параметрами соедиения";

            if (err == 0)
            {
                initialize(out err, out errMsg);
            }
            else
                ;

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::initialize () - " + errMsg);
            }
            else
            {
            }
        }

        private void initialize(out int err, out string errMsg)
        {
            int iListenerId = -1;

            err = -1;
            errMsg = string.Empty;

            iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB");
            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((!(dbConn == null)) && (err == 0))
            {
                initialize(ref dbConn, out err, out errMsg);
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);
        }

        protected virtual void clear()
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

        protected abstract void initialize(ref DbConnection dbConn, out int err, out string errMsg);

        protected abstract void Activate(bool activate);

        protected void addLabelDesc(int id)
        {
            GroupBox gbDesc = new GroupBox();
            gbDesc.Text = @"Описание";
            gbDesc.Dock = DockStyle.Fill;
            this.Controls.Add(gbDesc, 5, 10);
            this.SetColumnSpan(gbDesc, 8); this.SetRowSpan(gbDesc, 3);

            m_dictControls.Add(id, new Label());
        }

        //Для отображения актуальной "подсказки" для свойства
        protected void HPanelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
        }

        protected void addButton(int id, string text)
        {
            m_dictControls.Add(id, new Button());

            m_dictControls[id].Location = new System.Drawing.Point(1, 1);
            //m_dictControls[indx].Size = new System.Drawing.Size(79, 23);
            m_dictControls[id].Dock = DockStyle.Fill;
            m_dictControls[id].Text = text;
            this.Controls.Add(m_dictControls[id], 0, id);
            this.SetColumnSpan(m_dictControls[id], 1);
        }

        protected void HPanelTepCommon_btnSave_Click(object obj, EventArgs ev)
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1;
            string errMsg = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((!(dbConn == null)) && (err == 0))
            {
                recUpdateInsertDelete(ref dbConn, out err);

                if (!(err == 0))
                {
                    errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
                }
                else
                {                    
                    successRecUpdateInsertDelete();

                    if (!(delegateSaveAdding == null))
                        delegateSaveAdding(iListenerId);
                    else
                        ;
                }
            }
            else
            {
                errMsg = @"нет соединения с БД";
                err = -1;
            }

            DbSources.Sources().UnRegister(iListenerId);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnSave_Click () - " + errMsg);
            }
            else
            {
            }
        }

        protected abstract void recUpdateInsertDelete(ref DbConnection dbConn, out int err);
        protected abstract void successRecUpdateInsertDelete();

        protected void HPanelTepCommon_btnUpdate_Click(object obj, EventArgs ev)
        {
            clear();
        }
    }    
}
