using ASUTP.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using ASUTP.Helper;

namespace Tep64
{
    public abstract partial class FormParameters : ASUTP.Forms.FormParametersBase
    {
        protected interface IParametrSetup<T>
        {
            string Key { get; set; }

            string NameSI { get; set; }

            T Value { get; set; }

            string Default { get; set; }

            bool IsDefault { get; }

            object[] DataRow { get; }

            void Set (T newValue);

            void Reset ();
        }

        protected struct ParametrSetup : IParametrSetup<string>
        {
            public ParametrSetup (string key, string nameSi, string defValue)
            {
                Key = key;
                NameSI = nameSi;
                Default =
                Value =
                    defValue;
            }

            public ParametrSetup (string key, string nameSi, string defValue, string curValue)
                : this (key, nameSi, defValue)
            {
                Value = curValue;
            }

            public object [] DataRow
            {
                get
                {
                    return new object [] { Key, Value, NameSI };
                }
            }

            public bool IsDefault { get { return Equals(Value, Default); } }

            public void Set (string newValue)
            {
                Default =
                Value =
                    newValue;
            }

            public void Reset ()
            {
                Value = Default;
            }

            public string Key { get; set; }

            public string NameSI { get; set; }

            public string Value { get; set; }

            public string Default { get; set; }
        }

        protected class DictionaryParametrSetup<T> : Dictionary<PARAMETR_SETUP, IParametrSetup<T>>
        {
            public string GetWriteRequest (PARAMETR_SETUP key, bool bInsert)
            {
                string strRes = string.Empty;

                if (bInsert == false)
                    strRes = string.Format (@"UPDATE setup SET [VALUE]='{0}', [LAST_UPDATE]=GETDATE() WHERE [KEY]='{1}'", this[key].Value, key);
                else
                    strRes = string.Format (@"INSERT INTO [setup] ([ID],[VALUE],[KEY],[LAST_UPDATE],[ID_UNIT]) VALUES ('{0}','{1}',GETDATE(),{2})", this [key].Value, key, -1);

                return strRes;
            }

            public string GetWriteRequest (bool bInsert)
            {
                string strRes = string.Empty;

                foreach(PARAMETR_SETUP key in this.Keys)
                    strRes += $"{GetWriteRequest(key, bInsert)}{";"}";

                strRes = strRes.Substring (0, strRes.Length - 1);

                return strRes;
            }
        }

        public enum PARAMETR_SETUP { POLL_TIME, ERROR_DELAY, MAX_ATTEMPT, WAITING_TIME, WAITING_COUNT, MAIN_DATASOURCE
            //, ID_APP
                , COUNT_PARAMETR_SETUP
        };

        protected DictionaryParametrSetup<string> m_dictParametrSetup;

        public FormParameters()
            : base()
        {
            InitializeComponent();

            m_dictParametrSetup = new DictionaryParametrSetup<string> () {
                { PARAMETR_SETUP.POLL_TIME, new ParametrSetup("Polling period"          , "сек" , @"30") }
                , { PARAMETR_SETUP.ERROR_DELAY, new ParametrSetup("Error delay"         , "сек" , @"60") }
                , { PARAMETR_SETUP.MAX_ATTEMPT, new ParametrSetup("Max attempts count"  , "ед." , @"3") }
                , { PARAMETR_SETUP.WAITING_TIME, new ParametrSetup("Waiting time"       , "мсек", @"106") }
                , { PARAMETR_SETUP.WAITING_COUNT, new ParametrSetup("Waiting count"     , "мсек", @"39") }
                , { PARAMETR_SETUP.MAIN_DATASOURCE, new ParametrSetup("Main DataSource" , "ном" , @"671") }
                //, { PARAMETR_SETUP.ID_APP, new ParametrSetup("", "", (int)ProgramBase.ID_APP.STATISTIC.ToString ()) }
                ,
            };

            this.btnCancel.Location = new System.Drawing.Point(8, 90);
            this.btnOk.Location = new System.Drawing.Point(89, 90);
            this.btnReset.Location = new System.Drawing.Point(170, 90);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            mayClose = false;
        }

        //protected override void btnOk_Click(object sender, EventArgs e)
        protected void btnOk_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i ++)
                m_dictParametrSetup[i].Value = m_dgvData.Rows [(int)i + 0].Cells [1].Value.ToString ();

            saveParam();
            mayClose = true;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                m_dictParametrSetup[i].Reset();
        }
    }

    public partial class FormParameters_FIleINI : FormParameters
    {
        private static string NAME_SECTION_MAIN = "Main settings (" + ProgramBase.AppName + ")";

        private FileINI m_FileINI;

        public FormParameters_FIleINI(string nameSetupFileINI)
        {
            m_FileINI = new FileINI(nameSetupFileINI, false);
            //ProgramBase.s_iAppID = (int)ProgramBase.ID_APP.STATISTIC;
            //ProgramBase.s_iAppID = Int32.Parse(m_arParametrSetup[(int)PARAMETR_SETUP.ID_APP]);
            //ProgramBase.s_iAppID = Properties.s_

            loadParam(true);
        }

        public override void Update(out int err)
        {
            err = -1;
        }

        protected override void loadParam(bool bInit)
        {
            string value = string.Empty
                , defValue = string.Empty;

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                value = m_FileINI.ReadString(NAME_SECTION_MAIN, m_dictParametrSetup [i].Key, defValue);
                if (value.Equals(defValue) == true)
                {
                    m_dictParametrSetup[i].Reset();
                    m_FileINI.WriteString(NAME_SECTION_MAIN, m_dictParametrSetup [i].Key, m_dictParametrSetup [i].Value);
                }
                else
                    ;
            }

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_dgvData.Rows.Insert((int)i, m_dictParametrSetup[i].DataRow);

                m_dgvData.Rows[(int)i].Height = 19;
                m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
            }
        }

        protected override void saveParam()
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                m_FileINI.WriteString(NAME_SECTION_MAIN, m_dictParametrSetup [i].Key, m_dictParametrSetup [i].Value);
        }
    }

    public partial class FormParameters_DB : FormParameters
    {
        //private string[] KEYDB_PARAMETR_SETUP = { "Polling period", "Error delay", "Max attempts count", @"Waiting time", @"Waiting count", @"Main DataSource",
        //                                            @"Users DomainName", @"Users ID_TEC", @"Users ID_ROLE"
        //                                            //, @"ID_APP"
        //                                            };

        private ASUTP.Database.ConnectionSettings m_connSett;

        public FormParameters_DB(int idListener)
            : base()
        {
        }

        public FormParameters_DB(ConnectionSettings connSett)
            : base ()
        {
            int err = -1;

            m_connSett = connSett;

            loadParam (true);
        }

        public override void Update(out int err)
        {
            err = 0;
         
            loadParam(false);
        }

        protected void setDataGUI(bool bInit)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                if (bInit == true)
                {
                    m_dgvData.Rows.Insert((int)i, m_dictParametrSetup [i].DataRow);

                    m_dgvData.Rows[(int)i].Height = 19;
                    m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                    m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
                }
                else
                    m_dgvData.Rows[(int)i].Cells[1].Value = m_dictParametrSetup [i].Value;
            }
        }

        protected override void loadParam(bool bInit)
        {
            int err = -1;

            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.Yes;

            string query = string.Empty;
            //query = @"SELECT * FROM [dbo].[setup] WHERE [KEY]='" + key + @"'";
            query = string.Format(@"SELECT * FROM setup");
            DataTable table = DbTSQLInterface.Select(m_connSett, query, out err);
            DataRow[] rowRes;
            if (err == (int)DbTSQLInterface.Error.NO_ERROR)
                if (Equals(table, null) == false)
                {
                    query = string.Empty;

                    if (table.Rows.Count > 0)
                        for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++) {
                            //strRead = readString(NAME_PARAMETR_SETUP[(int)i], strDefault, out err);
                            rowRes = table.Select($"KEY='{m_dictParametrSetup[i].Key}");
                            switch (rowRes.Length)
                            {
                                case 1:
                                    m_dictParametrSetup [i].Set (rowRes [0][@"VALUE"].ToString().Trim());
                                    break;
                                case 0:
                                    m_dictParametrSetup[i].Reset();
                                    query += m_dictParametrSetup.GetWriteRequest(i, true) + @";";
                                    break;
                                default:
                                    break;
                            }
                        }
                    else
                        query = m_dictParametrSetup.GetWriteRequest(true);

                    if (query.Equals(string.Empty) == false)
                        DbTSQLInterface.ExecNonQuery(m_connSett, query, out err);
                    else
                        ;
                }
                else
                    err = (int)DbTSQLInterface.Error.TABLE_NULL;
            else
                ;

            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.No;

            setDataGUI(bInit);
        }

        protected override void saveParam()
        {
            int err = -1;

            string query = string.Empty;
            
            query = m_dictParametrSetup.GetWriteRequest(false);

            if (query.Equals(string.Empty) == false)
                DbTSQLInterface.ExecNonQuery(m_connSett, query, out err);
            else
                ;
        }

        private string readString (string key, string defValue) {
            string strRes = defValue;
            int err = -1;
            DataTable table = null;

            table = DbTSQLInterface.Select (m_connSett, $"SELECT [VALUE] FROM [dbo].[setup] WHERE [KEY]='{key}", out err);
            if (table.Rows.Count == 1)
                strRes = table.Rows [0][0].ToString ().Trim ();
            else
                ;

            return strRes;
        }

        private void writeString(string key, string newValue)
        {
            int err = -11;

            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.Yes;

            string defValue = "NotRecord"
                , prevValue = string.Empty
                , query = string.Empty;

            prevValue = readString (key, defValue);
            // проверить признак отсутствия значения
            if (prevValue.CompareTo(defValue) == 0)
                query = $"INSERT INTO [setup] ([ID],[KEY],[VALUE],[LAST_UPDATE],[ID_UNIT],[ID_APP])"
                    + $" VALUES ((SELECT MAX([ID])+1 FROM [setup]),'{key}','{newValue}',GETDATE(),{9},{ProgramBase.s_iAppID})";
            else
                query = string.Format(@"UPDATE [dbo].[setup] SET [VALUE]='{1}' WHERE [KEY]='{0}'", key, newValue);

            DbTSQLInterface.ExecNonQuery (m_connSett, query, out err);

            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.No;
        }
    }
}