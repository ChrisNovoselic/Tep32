using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;

namespace InterfacePlugIn
{
    public abstract class HFunc : HPlugIn
    {
        public enum ID_DATAASKED_HOST { ICON_MAINFORM, STR_VERSION //Запросить данные у главной формы
                                    , CONNSET_MAIN_DB
                                    };

        protected string _nameOwnerMenuItem
            , _nameMenuItem;

        public override string NameOwnerMenuItem
        {
            get
            {
                return _nameOwnerMenuItem;
            }
        }

        public override string NameMenuItem
        {
            get
            {
                return _nameMenuItem;
            }
        }
    }

    public abstract class HFuncDictEdit : HFunc
    {
        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            //createObject(typeof(PanelTepDictTime));
            createObject(this.GetType ());

            if (m_markDataHost.IsMarked((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB) == false)
                DataAskedHost((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);
            else
                ;

            //Передать главной форме параметр
            DataAskedHost(obj);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            //throw new NotImplementedException();

            base.OnEvtDataRecievedHost(obj);

            switch (((EventArgsDataHost)obj).id)
            {
                case (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB:
                    ((HObjectDictEdit)_object).Initialize(obj);
                    break;
                default:
                    break;
            }
        }
    }

    public class HObjectDictEdit : TableLayoutPanel
    {
        protected enum INDEX_CONTROL
        {
            BUTTON_ADD, BUTTON_DELETE, BUTTON_SAVE,
            BUTTON_UPDATE
                , DGV_DICT_EDIT,
            DGV_DICT_PROP
                ,
            LABEL_PROP_DESC
                , INDEX_CONTROL_COUNT,
        };
        protected static string[] m_arButtonText = { @"Добавить", @"Удалить", @"Сохранить", @"Обновить" };

        private DataTable m_tblEdit;
        private ConnectionSettings m_connSett;
        protected string m_nameTable;
        private string m_query;

        protected Dictionary<int, Control> m_dictControls;

        public HObjectDictEdit(string nameTable)
        {
            m_nameTable = nameTable;
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
                Logging.Logg().Exception(e, @"PanelTepDictPlugIns::Initialize () - BeginInvoke (initialize) - ...");
            }
        }

        public void initialize(object obj)
        {
            int i = -1
                , err = -1
                , iListenerId = -1;
            string errMsg = string.Empty;

            if (((EventArgsDataHost)obj).par is ConnectionSettings)
            {
                m_connSett = (ConnectionSettings)((EventArgsDataHost)obj).par;

                err = 0;
            }
            else
                errMsg = @"не корректен тип объекта с параметрами соедиения";

            if (err == 0)
            {
                iListenerId = DbSources.Sources().Register(m_connSett, false, @"MAIN_DB");
                DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

                if ((!(dbConn == null)) && (err == 0))
                {
                    m_query = @"SELECT * FROM " + m_nameTable;
                    m_tblEdit = DbTSQLInterface.Select(ref dbConn, m_query, null, null, out err);

                    if (err == 0)
                    {
                        Logging.Logg().Debug(@"HPanelEdit::initialize () - усПех ...");

                        DataGridView dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]);
                        for (i = 0; i < m_tblEdit.Columns.Count; i++)
                        {
                            dgv.Rows.Add(new object[] { m_tblEdit.Columns[i].ColumnName, string.Empty }); ;
                        }

                        ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_EDIT]).SelectionChanged += new EventHandler(HPanelEdit_SelectionChanged);
                        dgv = ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_EDIT]);
                        for (i = 0; i < m_tblEdit.Rows.Count; i++)
                        {
                            dgv.Rows.Add(new object[] { m_tblEdit.Rows[i][@"DESCRIPTION"].ToString().Trim() });
                        }
                    }
                    else
                    {
                        errMsg = @"не удалось получить значения из целевой таблицы [" + m_nameTable + @"]";
                        err = -1;
                    }
                }
                else
                {
                    errMsg = @"нет соединения с БД";
                    err = -1;
                }

                DbSources.Sources().UnRegister(iListenerId);
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

        private void HPanelEdit_SelectionChanged(object obj, EventArgs ev)
        {
            int indx = -1;

            if (((DataGridView)obj).SelectedRows.Count > 0)
            {
                indx = ((DataGridView)obj).SelectedRows[0].Index;

                if ((!(indx < 0)) && (indx < m_tblEdit.Rows.Count))
                    for (int i = 0; i < ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).NewRowIndex; i++)
                        ((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows[i].Cells[1].Value = m_tblEdit.Rows[indx][((DataGridView)m_dictControls[(int)INDEX_CONTROL.DGV_DICT_PROP]).Rows[i].Cells[0].Value.ToString()].ToString().Trim();
                else
                    ;
            }
            else
            {
            }
        }
    }
}
