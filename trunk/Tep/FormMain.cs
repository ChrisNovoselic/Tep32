using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data.Common; //DbConnection
using System.Reflection; //Assembly
using System.IO; //Stream

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace Tep64
{
    public partial class FormMain : FormMainBaseWithStatusStrip, IPlugInHost
    {        
        private static FormParameters s_formParameters;
        private Dictionary <int, IPlugIn> m_dictPlugins;

        public FormMain() : base ()
        {
            InitializeComponent();

            m_dictPlugins = new Dictionary<int, IPlugIn> ();

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString ());

            ConnectionSettingsSource connSettSource = new ConnectionSettingsSource(idListener);
            s_listFormConnectionSettings.Add(new FormConnectionSettings(idListener, connSettSource.Read, connSettSource.Save));

            DbSources.Sources().UnRegister(idListener);

            m_TabCtrl.OnClose += new HTabCtrlEx.DelegateOnCloseTab(onCloseTabPage);
        }

        protected override void HideGraphicsSettings() { }
        protected override void UpdateActiveGui(int type) { }

        public bool Register(IPlugIn plug)
        {
            return true;
        }

        /// <summary>
        /// Обработчик выбора пункта меню 'Файл - выход'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void бДКонфигурацииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iRes = connectionSettings(CONN_SETT_TYPE.MAIN_DB);
        }

        private void параметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_formParameters == null) {
                Abort (@"Не создано окно для редактирования параметров приложения", false);
            } else {
                DialogResult res = s_formParameters.ShowDialog (this);
                if (res == System.Windows.Forms.DialogResult.OK) {
                } else {
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int iRes = -1;

            ProgramBase.s_iAppID = Int32.Parse((string)Properties.Resources.AppID);

            if (!(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].Ready == 0))
            {
                //Есть вызов 'Initialize...'
                iRes = connectionSettings(CONN_SETT_TYPE.MAIN_DB);
            } else {
                string msg = string.Empty;
                iRes = Initialize (out msg);

                if (! (iRes == 0)) {
                    Abort (msg, false);
                } else {
                }
            }

            //System.ComponentModel.ComponentResourceManager resources = System.ComponentModel.ComponentResourceManager(typeof (...;
            //Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TepCommon.MainForm.ico");
            this.Icon = (Icon)TepCommon.Properties.Resources.MainForm;

            m_report.ActionReport (@"Загрузка главного окна");

            this.Focus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop ();
        }

        /// <summary>
        /// Допполнительные действия по инициализации плюг'ина
        /// </summary>
        /// <param name="plugIn">объект плюг'ина</param>
        private void initializePlugIn (IPlugIn plugIn) {
            if (plugIn is HFunc) {
            } else {
            }
        }

        private void initializeLogging () {
            //Если ранее тип логирования не был назанчен...
            if (Logging.s_mode == Logging.LOG_MODE.UNKNOWN)
            {
                //назначить тип логирования - БД
                Logging.s_mode = Logging.LOG_MODE.DB;
            }
            else { }
            if (Logging.s_mode == Logging.LOG_MODE.DB)
            {
                //Инициализация БД-логирования
                int err = -1;
                //Вариант №1
                //DataRow rowConnSettLog = null;
                //HClassLibrary.Logging.ConnSett = new ConnectionSettings(rowConnSettLog);
                //Вариант №2
                HClassLibrary.Logging.ConnSett = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett();
            }
            else { }
        }

        private int initializeMenu (out string strErr) {
            int iRes = -1;
            strErr = string.Empty;

            int idListener = -1;
            DbConnection dbConn = null;
            DataTable tableRes = null;
            string strUserDomainName = string.Empty;

            idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, @"MAIN_DB");
            dbConn = DbSources.Sources().GetConnection (idListener, out iRes);
            strUserDomainName = Environment.UserDomainName + @"\" + Environment.UserName;

            HUsers.GetUsers(ref dbConn, @"DOMAIN_NAME='" + strUserDomainName + @"'", string.Empty, out tableRes, out iRes);

            if (iRes == 0) {
                HUsers.GetRoles(ref dbConn, @"ID_EXT=" + tableRes.Rows[0][@"ID_ROLE"], string.Empty, out tableRes, out iRes);

                if (iRes == 0) {
                    initializeLogging ();

                    int i = -1;
                    //Сформировать список идентификаторов плюгинов
                    string strIdPlugins = string.Empty;

                    //Циклл по строкам - идентификатрам/разрешениям использовать плюгин
                    for (i = 0; i < tableRes.Rows.Count; i++)
                    {
                        //Проверить разрешение использовать плюгин
                        if (Int16.Parse(tableRes.Rows[i][@"USE"].ToString()) == 1)
                        {
                            strIdPlugins += tableRes.Rows[i][@"ID_PLUGIN"].ToString() + @",";
                        } else {
                        }
                    }
                    //Удалить крайний символ
                    strIdPlugins = strIdPlugins.Substring(0, strIdPlugins.Length - 1);

                    //Прочитать наименования плюгинов
                    tableRes = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM plugins WHERE ID IN (" + strIdPlugins + @")", null, null, out iRes);

                    //Проверить рез-т чтения наименования плюгина
                    if (iRes == 0)
                    {
                        IPlugIn plugIn = null;
                        ToolStripMenuItem miOwner = null
                            , item = null;
                        int idPlugIn = -1;
                        //Циклл по строкам - идентификатрам/разрешениям использовать плюгин
                        for (i = 0; (i < tableRes.Rows.Count) && (iRes == 0); i++)
                        {
                            //Загрузить плюгин
                            plugIn = loadPlugin(tableRes.Rows[i][@"NAME"].ToString().Trim(), out iRes);

                            if (! (iRes < 0)) {
                                //Идентификатор плюг'ина
                                idPlugIn = Int16.Parse (tableRes.Rows[i][@"ID"].ToString ());
                                //Проверка на соответствие идентификаторов в БД и коде (м.б. и не нужно???)
                                if (((HPlugIn)plugIn)._Id == idPlugIn)
                                {
                                    m_dictPlugins.Add (idPlugIn, plugIn);

                                    //Поиск пункта "родительского" пункта меню для плюг'ина
                                    miOwner = FindMainMenuItemOfText (((HPlugIn)m_dictPlugins[idPlugIn]).NameOwnerMenuItem);

                                    //Проверка найден ли "родительский" пункт меню для плюг'ина
                                    if (miOwner == null) {
                                        //НЕ найден - создаем
                                        this.MainMenuStrip.Items.Add(((HPlugIn)m_dictPlugins[idPlugIn]).NameOwnerMenuItem);
                                        miOwner = FindMainMenuItemOfText(((HPlugIn)m_dictPlugins[idPlugIn]).NameOwnerMenuItem);
                                    } else {
                                    }

                                    //Добавить пункт меню для плюг'ина
                                    item = miOwner.DropDownItems.Add(((HPlugIn)m_dictPlugins[idPlugIn]).NameMenuItem) as ToolStripMenuItem;
                                    //Обработку выбора пункта меню предоставить плюг'ину
                                    item.Click += ((HPlugIn)m_dictPlugins[idPlugIn]).OnClickMenuItem;
                                    //Добавить обработчик запросов для плюг'ина от главной формы
                                    ((HPlugIn)m_dictPlugins[idPlugIn]).EvtDataAskedHost += new DelegateObjectFunc(FormMain_EvtDataAskedHost);

                                    initializePlugIn(plugIn);
                                } else {
                                    iRes = -2; //Несоответствие идентификатроов
                                }
                            } else {
                            }
                        }

                        if (iRes == 0)
                            //Успешный запуск на выполнение приложения
                            Start();
                        else {
                            switch (iRes) {
                                case -2:
                                    strErr = @"Не удалось загрузить все разрешенные для использования модули из списка (несоответствие идентификатроов)";
                                    break;
                                case -1:
                                default:
                                    strErr = @"Не удалось загрузить все разрешенные для использования модули из списка";
                                    break;
                            }
                        }
                    }
                    else
                    {
                        strErr = @"Не удалось сформировать список разрешенных для использования модулей";
                    }
                } else {
                    strErr = @"Не удалось сформировать правила для роли пользователя";
                }
            }
            else {
                strErr = @"Не удалось идентифицировать пользователя";
            }

            DbSources.Sources().UnRegister(idListener);

            return iRes;
        }

        void FormMain_EvtDataAskedHost(object obj)
        {
            object rec = null;

            if (((EventArgsDataHost)obj).par[0].GetType ().IsPrimitive == true) {
                if ((int)((EventArgsDataHost)obj).par[0] == (int)HFunc.ID_DATAASKED_HOST.CONNSET_MAIN_DB) {
                    rec = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett ();
                } else {
                    switch ((int)((EventArgsDataHost)obj).id) {
                        case 1: //FormAboutTepProgram
                            switch ((int)((EventArgsDataHost)obj).par[0]) {
                                case (int)HFunc.ID_DATAASKED_HOST.ICON_MAINFORM:
                                    rec = TepCommon.Properties.Resources.MainForm;
                                    break;
                                case (int)HFunc.ID_DATAASKED_HOST.STR_VERSION:
                                    rec = Application.ProductVersion;                            
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 2: //PanelTepDictPlugIns
                            switch ((int)((EventArgsDataHost)obj).par[0]) {
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Отправить ответ (исходный идентификатор + требуемый объект)
                ((HPlugIn)m_dictPlugins[((EventArgsDataHost)obj).id]).OnEvtDataRecievedHost (new EventArgsDataHost((int)((EventArgsDataHost)obj).par[0], new object [] { rec }));
            }
            else {
                if (((EventArgsDataHost)obj).par[0] is ToolStripMenuItem) {
                    try
                    {
                        this.BeginInvoke(new DelegateObjectFunc(onClickMenuItem), obj);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"FormMain_EvtDataAskedHost () - BeginInvoke (addTabPage) [id] = " + (int)((EventArgsDataHost)obj).id);
                    }
                }
                else {
                }
            }
        }

        private void onClickMenuItem (object obj) {
            int id_plugIn = (int)((EventArgsDataHost)obj).id;
            ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked = ! ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked;

            if (((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked == true) {
                m_TabCtrl.AddTabPage(((HPlugIn)m_dictPlugins[id_plugIn]).NameMenuItem);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add((Control)((HPlugIn)m_dictPlugins[id_plugIn]).Object);
                //m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add(new HPanelEdit ());
            } else {
                m_TabCtrl.TabPages.RemoveAt(m_TabCtrl.IndexOfItemControl((Control)((HPlugIn)m_dictPlugins[id_plugIn]).Object));
            }
        }

        private void onCloseTabPage (object sender, CloseTabEventArgs e) {
            ////Вариант №1
            //FindMainMenuItemOfText (e.TabHeaderText).Checked = false;
            //m_TabCtrl.TabPages.RemoveAt (e.TabIndex);

            //Вариант №2
            FindMainMenuItemOfText (e.TabHeaderText).PerformClick ();
        }

        private IPlugIn loadPlugin(string name, out int iRes)
        {
            IPlugIn plugInRes = null;
            iRes = -1;

            Type objType = null;
            try
            {
                Assembly ass = null;
                ass = Assembly.LoadFrom (Environment.CurrentDirectory + @"\" + name + @".dll");
                if (! (ass == null))
                {
                    objType = ass.GetType(name + ".PlugIn");
                }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormMain::loadPlugin () ... LoadFrom () ... plugIn.Name = " + name);
            }

            if (! (objType == null))
                try
                {
                    plugInRes = ((IPlugIn)Activator.CreateInstance(objType));
                    plugInRes.Host = (IPlugInHost)this;

                    iRes = 0;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"FormMain::loadPlugin () ... CreateInstance ... plugIn.Name = " + name);
                }
            else
                Logging.Logg().Error(@"FormMain::loadPlugin () ... Assembly.GetType()=null ... plugIn.Name = " + name);

            return plugInRes;
        }

        private int Initialize (out string strErr) {
            int iRes = 0;
            strErr = string.Empty;

            s_formParameters = null;
            try
            {
                s_formParameters = new FormParameters_DB(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett());
            }
            catch (Exception e)
            {
                iRes = -1;
                strErr = e.Message;
            }

            if (iRes == 0)
            {
                iRes = initializeMenu(out strErr);
            } else {
                //Сообщение уже сформировано
            }

            return iRes;
        }

        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings[(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                //Остановить все вкладки
                //StopTabPages ();

                //Остановить таймер (если есть)
                Stop ();

                iRes = s_listFormConnectionSettings[(int)type].Ready;

                string msg = string.Empty;
                if (iRes == 0)
                {
                    iRes = Initialize(out msg);
                }
                else
                {
                    msg = @"Параметры соединения с БД конфигурации не корректны";
                }

                if (!(iRes == 0))
                    //@"Ошибка инициализации пользовательских компонентов формы"
                    Abort(msg, false);
                else
                    ;
            }
            else
                ;

            return iRes;
        }

        private void TabCtrl_OnSelectedIndexChanged (object obj, EventArgs ev) {
        }

        protected override bool  UpdateStatusString () {
            bool have_eror = false;
            m_lblDescError.Text = m_lblDateError.Text = string.Empty;

            if (m_report.actioned_state == true)
            {
                m_lblDateError.Text = m_report.last_time_action.ToString();
                m_lblDescError.Text = m_report.last_action;
            }
            else
                ;

            if (m_report.errored_state == true)
            {
                have_eror = true;
                m_lblDateError.Text = m_report.last_time_error.ToString();
                m_lblDescError.Text = m_report.last_error;
            }
            else
                ;

            return have_eror;
        }

        protected override void timer_Start()
        {//m_timer.Interval == ProgramBase.TIMER_START_INTERVAL
        }
    }
}
