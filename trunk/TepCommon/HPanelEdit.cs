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
    public abstract class HPanelTepCommon : TableLayoutPanel, IObjectDictEdit
    {
        protected DataTable m_tblEdit
            , m_tblOrigin;
        protected string m_nameTable;
        protected string m_strKeyFields;

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

        public HPanelTepCommon(IPlugIn plugIn, string nameTable, string keyFields)
        {
            this._iFuncPlugin = plugIn;

            //Создать объект "словарь" дочерних элементов управления
            m_dictControls = new Dictionary<int, Control>();

            m_nameTable = nameTable;
            m_strKeyFields = keyFields;

            InitializeComponent();
        }

        private void InitializeComponent ()
        {
            components = new System.ComponentModel.Container();

            this.Dock = DockStyle.Fill;
            // для отладки - "видимые" границы ячеек
            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            //Установить кол-во строк/столбцов
            this.RowCount = 13; this.ColumnCount = 13;

            //Добавить стили "ширина" столлбцов
            for (int s = 0; s < this.ColumnCount; s++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)100 / this.ColumnCount));

            //Добавить стили "высота" строк
            for (int s = 0; s < this.RowCount; s++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)100 / this.RowCount));
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

            m_tblEdit.Clear();
            m_tblOrigin.Clear();

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

        protected void HPanelEdit_btnSave_Click(object obj, EventArgs ev)
        {
            int iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB")
                , err = -1;
            string errMsg = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((!(dbConn == null)) && (err == 0))
            {
                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, m_nameTable, m_strKeyFields, m_tblOrigin, m_tblEdit, out err);

                if (!(err == 0))
                {
                    errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
                }
                else
                {
                    m_tblOrigin = m_tblEdit.Copy();

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

        protected void HPanelEdit_btnUpdate_Click(object obj, EventArgs ev)
        {
            clear();
        }
    }
}
