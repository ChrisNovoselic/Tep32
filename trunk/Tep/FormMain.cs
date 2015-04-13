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
    public partial class FormMain : FormMainBaseWithStatusStrip
    {        
        private static FormParameters s_formParameters;
        private static HTepUsers m_User;
        /// <summary>
        /// Признак процесса авто/загрузки вкладок
        /// для предотвращения сохранения их в режиме "реальное время"
        /// </summary>
        private static int m_iAutoActionTabs = 0;

        private static HPlugIns s_plugIns;
        class HPlugIns : IPlugInHost //, IEnumerable <int>
        {
            private Dictionary<int, IPlugIn> m_dictPlugins;

            public DelegateObjectFunc delegateOnClickMenuPluginItem;

            public HPlugIns(DelegateObjectFunc fClickMenuItem)
            {
                m_dictPlugins = new Dictionary<int, IPlugIn>();
                delegateOnClickMenuPluginItem = fClickMenuItem;
            }

            public bool Register(IPlugIn plug)
            {
                return true;
            }

            public void Add(int id, IPlugIn plugIn)
            {
                m_dictPlugins.Add(id, plugIn);
            }

            public IPlugIn Load(string name, out int iRes)
            {
                IPlugIn plugInRes = null;
                iRes = -1;

                Type objType = null;
                try
                {
                    Assembly ass = null;
                    ass = Assembly.LoadFrom(Environment.CurrentDirectory + @"\" + name + @".dll");
                    if (!(ass == null))
                    {
                        objType = ass.GetType(name + ".PlugIn");
                    }
                    else
                        ;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::loadPlugin () ... LoadFrom () ... plugIn.Name = " + name);
                }

                if (!(objType == null))
                    try
                    {
                        plugInRes = ((IPlugIn)Activator.CreateInstance(objType));
                        plugInRes.Host = (IPlugInHost)this;

                        iRes = 0;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::loadPlugin () ... CreateInstance ... plugIn.Name = " + name);
                    }
                else
                    Logging.Logg().Error(@"FormMain::loadPlugin () ... Assembly.GetType()=null ... plugIn.Name = " + name, Logging.INDEX_MESSAGE.NOT_SET);

                return plugInRes;
            }

            public HPlugIn Find(int id)
            {
                return m_dictPlugins[id] as HPlugIn;
            }

            public void OnEvtDataAskedHost(object obj)
            {
                object rec = null;

                if (((EventArgsDataHost)obj).par[0].GetType().IsPrimitive == true)
                {
                    if ((int)((EventArgsDataHost)obj).par[0] == (int)HFunc.ID_DATAASKED_HOST.CONNSET_MAIN_DB)
                    {
                        rec = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett();
                    }
                    else
                    {
                        switch ((int)((EventArgsDataHost)obj).id)
                        {
                            case 1: //FormAboutTepProgram
                                switch ((int)((EventArgsDataHost)obj).par[0])
                                {
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
                                switch ((int)((EventArgsDataHost)obj).par[0])
                                {
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    //Отправить ответ (исходный идентификатор + требуемый объект)
                    ((HPlugIn)m_dictPlugins[((EventArgsDataHost)obj).id]).OnEvtDataRecievedHost(new EventArgsDataHost((int)((EventArgsDataHost)obj).par[0], new object[] { rec }));
                }
                else
                {
                    if (((EventArgsDataHost)obj).par[0] is ToolStripMenuItem)
                    {
                        try
                        {
                            delegateOnClickMenuPluginItem(obj);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain_EvtDataAskedHost () - BeginInvoke (addTabPage) [id] = " + (int)((EventArgsDataHost)obj).id);
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        public FormMain() : base ()
        {
            InitializeComponent();

            s_plugIns = new HPlugIns(FormMain_EvtDataAskedHost);

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString ());

            m_User = new HTepUsers(idListener);

            ConnectionSettingsSource connSettSource = new ConnectionSettingsSource(idListener);
            s_listFormConnectionSettings.Add(new FormConnectionSettings(idListener, connSettSource.Read, connSettSource.Save));

            DbSources.Sources().UnRegister(idListener);

            m_TabCtrl.OnClose += new HTabCtrlEx.DelegateOnHTabCtrlEx(onCloseTabPage);
        }

        protected override void HideGraphicsSettings() { }
        protected override void UpdateActiveGui(int type) { }

        private void loadProfile()
        {
            HPlugIn plugIn;
            string ids = HTepUsers.GetAllowed((int)HTepUsers.ID_ALLOWED.USERPROFILE_PLUGINS)
                , strNameOwnerMenuItem = string.Empty, strNameMenuItem = string.Empty;
            string[] arIds = ids.Split(',');
            ////Вариант №1
            //ToolStripItem[] menuItems;
            //Вариант №2
            ToolStripItem menuItem;

            foreach (string id in arIds)
            {
                plugIn = s_plugIns.Find(Convert.ToInt32(id));
                strNameOwnerMenuItem = plugIn.NameOwnerMenuItem;
                strNameMenuItem = plugIn.NameMenuItem;

                ////Вариант №1
                //menuItems = this.MainMenuStrip.Items.Find(strNameMenuItem, true);
                //menuItem = menuItems[0];
                //Вариант №2
                menuItem = FindMainMenuItemOfText(strNameMenuItem);

                if ((menuItem as ToolStripMenuItem).Checked == false)
                {
                    m_iAutoActionTabs++;
                    menuItem.PerformClick();
                }
                else
                    ;
            }
        }

        private void профайлЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadProfile();
        }

        private void saveProfile()
        {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString());

            string ids = m_TabCtrl.VisibleIDs;
            HTepUsers.SetAllowed(iListenerId, (int)HTepUsers.ID_ALLOWED.USERPROFILE_PLUGINS, ids);

            DbSources.Sources().UnRegister(iListenerId);
        }

        private void профайлСохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveProfile();
        }

        private void профайлАвтоЗагрузитьСохранитьToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            профайлЗагрузитьToolStripMenuItem.Enabled =
            профайлСохранитьToolStripMenuItem.Enabled =
                ! (sender as ToolStripMenuItem).Checked;

            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString());
            HTepUsers.SetAllowed(iListenerId, (int)HTepUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE_CHECKED, Convert.ToString((sender as ToolStripMenuItem).Checked == true ? 1 : 0));
            DbSources.Sources().UnRegister(iListenerId);
        }

        /// <summary>
        /// Обработчик выбора пункта меню 'Файл - выход'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_iAutoActionTabs = 1;

            Close ();

            m_iAutoActionTabs = 0;
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

            m_report.ActionReport(@"Загрузка главного окна");

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

            this.Focus();

            m_report.ClearStates(false);
        }

        protected override void Abort(string msg, bool bThrow = false, bool bSupport = true)
        {
            //???Удалить все пункты меню...


            base.Abort(msg, bThrow, bSupport);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop ();
        }

        private void FormMain_EvtDataAskedHost(object obj)
        {
            this.BeginInvoke(new DelegateObjectFunc(onClickMenuItem), obj);
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

            if (iRes == 0) {
                //HUsers.GetRoles(ref dbConn, @"ID_EXT=" + tableRes.Rows[0][@"ID_ROLE"], string.Empty, out tableRes, out iRes);
                HUsers.GetRoles(ref dbConn, @"ID_EXT=" + HTepUsers.Role, string.Empty, out tableRes, out iRes);

                if ((iRes == 0)
                    && (! (tableRes == null))
                    && (tableRes.Rows.Count > 0)) {
                    initializeLogging ();

                    int i = -1;
                    //Сформировать список идентификаторов плюгинов
                    string strIdPlugins = string.Empty;

                    //Циклл по строкам - идентификатрам/разрешениям использовать плюгин                    
                    for (i = 0; i < tableRes.Rows.Count; i++)
                    {
                        //Проверить разрешение использовать плюгин
                        if (Int16.Parse(tableRes.Rows[i][@"IsUse"].ToString()) == 1)
                        {
                            strIdPlugins += tableRes.Rows[i][@"ID_PLUGIN"].ToString() + @",";
                        }
                        else
                        {
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
                            plugIn = s_plugIns.Load(tableRes.Rows[i][@"NAME"].ToString().Trim(), out iRes);

                            if (!(iRes < 0))
                            {
                                //Идентификатор плюг'ина
                                idPlugIn = Int16.Parse(tableRes.Rows[i][@"ID"].ToString());
                                //Проверка на соответствие идентификаторов в БД и коде (м.б. и не нужно???)
                                if (((HPlugIn)plugIn)._Id == idPlugIn)
                                {
                                    s_plugIns.Add(idPlugIn, plugIn);

                                    //Поиск пункта "родительского" пункта меню для плюг'ина
                                    miOwner = FindMainMenuItemOfText((plugIn as HPlugIn).NameOwnerMenuItem);

                                    //Проверка найден ли "родительский" пункт меню для плюг'ина
                                    if (miOwner == null)
                                    {
                                        int indx = -1;
                                        string strNameItem = (plugIn as HPlugIn).NameOwnerMenuItem;
                                        if (strNameItem.Equals(@"Помощь") == false)
                                            indx = this.MainMenuStrip.Items.Count - 1;
                                        else
                                            ;
                                        //НЕ найден - создаем
                                        if (indx < 0)
                                            this.MainMenuStrip.Items.Add(new ToolStripMenuItem(strNameItem));
                                        else
                                            this.MainMenuStrip.Items.Insert(indx, new ToolStripMenuItem(strNameItem));
                                        miOwner = FindMainMenuItemOfText((plugIn as HPlugIn).NameOwnerMenuItem);
                                    }
                                    else
                                    {
                                    }

                                    //Добавить пункт меню для плюг'ина
                                    item = miOwner.DropDownItems.Add((plugIn as HPlugIn).NameMenuItem) as ToolStripMenuItem;
                                    //Обработку выбора пункта меню предоставить плюг'ину
                                    item.Click += (plugIn as HPlugIn).OnClickMenuItem;
                                    //Добавить обработчик запросов для плюг'ина от главной формы
                                    (plugIn as HPlugIn).EvtDataAskedHost += new DelegateObjectFunc(s_plugIns.OnEvtDataAskedHost);

                                    initializePlugIn(plugIn);
                                }
                                else
                                {
                                    iRes = -2; //Несоответствие идентификатроов
                                }
                            }
                            else
                            {
                            }
                        }

                        if (iRes == 0)
                        {
                            профайлАвтоЗагрузитьСохранитьToolStripMenuItem.Checked = Convert.ToBoolean(HTepUsers.GetAllowed((int)HTepUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE_CHECKED));
                            профайлАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled = Convert.ToBoolean(HTepUsers.GetAllowed((int)HTepUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE_ACCESS));

                            //Успешный запуск на выполнение приложения
                            Start();
                        }
                        else
                        {
                            switch (iRes)
                            {
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
                        if (iRes == 0) iRes = -1; else ;
                        strErr = @"Не удалось сформировать список разрешенных для использования модулей";
                    }
                } else {
                    if (iRes == 0) iRes = -1; else ;
                    strErr = @"Не удалось сформировать правила для роли пользователя";
                }
            }
            else {
                if (iRes == 0) iRes = -1; else ;
                strErr = @"Не удалось идентифицировать пользователя";
            }

            DbSources.Sources().UnRegister(idListener);

            return iRes;
        }

        private void onClickMenuItem (object obj) {
            HPlugIn plugIn = s_plugIns.Find((int)((EventArgsDataHost)obj).id);
            ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked = ! ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked;

            if (((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked == true) {
                m_TabCtrl.AddTabPage(plugIn.NameMenuItem, plugIn._Id, HTabCtrlEx.TYPE_TAB.FIXED);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add((Control)plugIn.Object);
                //m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add(new HPanelEdit ());
            } else {
                m_TabCtrl.RemoveTabPage(plugIn.NameMenuItem);
            }

            if ((m_iAutoActionTabs == 0)
                && (профайлАвтоЗагрузитьСохранитьToolStripMenuItem as ToolStripMenuItem).Checked == true)
                //профайлСохранитьToolStripMenuItem.PerformClick();
                saveProfile();
            else
                ;

            if (m_iAutoActionTabs > 0)
                m_iAutoActionTabs--;
            else
                ;
        }

        private void onCloseTabPage(object sender, HTabCtrlExEventArgs e)
        {
            ////Вариант №1
            //FindMainMenuItemOfText (e.TabHeaderText).Checked = false;
            //m_TabCtrl.TabPages.RemoveAt (e.TabIndex);

            //Вариант №2
            FindMainMenuItemOfText (e.TabHeaderText).PerformClick ();
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

                if (iRes == 0)
                    if ((профайлАвтоЗагрузитьСохранитьToolStripMenuItem as ToolStripMenuItem).Checked == true)
                    {
                        //профайлЗагрузитьToolStripMenuItem.PerformClick();
                        loadProfile();
                    }
                    else
                        ;
                else
                    ;
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

        protected override int UpdateStatusString () {
            int haveError = 0;
            m_lblDescMessage.Text = m_lblDateMessage.Text = string.Empty;

            if (m_report.actioned_state == true)
            {
                m_lblDateMessage.Text = m_report.last_time_action.ToString();
                m_lblDescMessage.Text = m_report.last_action;
            }
            else
                ;

            if (m_report.errored_state == true)
            {
                haveError = -1;
                m_lblDateMessage.Text = m_report.last_time_error.ToString();
                m_lblDescMessage.Text = m_report.last_error;
            }
            else
                ;

            return haveError;
        }

        protected override void timer_Start()
        {//m_timer.Interval == ProgramBase.TIMER_START_INTERVAL
        }
    }
}
