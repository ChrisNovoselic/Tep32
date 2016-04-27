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
        /// <summary>
        /// Форма для редактирования параметров приложения
        /// </summary>
        private static FormParameters s_formParameters;
        /// <summary>
        /// Признак процесса авто/загрузки вкладок
        ///  для предотвращения сохранения их в режиме "реальное время"
        /// </summary>
        private static int m_iAutoActionTabs = 0;
        /// <summary>
        /// Объект со списком загруженных библиотек
        /// </summary>
        private static HPlugIns s_plugIns;
        /// <summary>
        /// Конструктор - основной (без параметров)
        public FormMain() : base ()
        {
            InitializeComponent();

            s_plugIns = new HPlugIns(FormMain_EvtDataAskedHost);

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString ());

            if (! (idListener < 0))
            {
                initProfiles(idListener);

                //ConnectionSettingsSource connSettSource = new ConnectionSettingsSource(idListener);
                //s_listFormConnectionSettings.Add(new FormConnectionSettings(idListener, connSettSource.Read, connSettSource.Save));
            }
            else
                ;

            DbSources.Sources().UnRegister(idListener);

            m_TabCtrl.EventHTabCtrlExClose += new HTabCtrlEx.DelegateHTabCtrlEx(onCloseTabPage);
        }

        private void initProfiles(int iListenerId)
        {
            try
            {
                using (HTepUsers users = new HTepUsers(iListenerId)) { ; }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormMain::initProfiles(iListenerId=" + iListenerId + @") - new HTepUsers () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// ??? Обязательное переопределение от 'FormMainBaseWithStatusStrip'
        /// </summary>
        protected override void HideGraphicsSettings() { }
        /// <summary>
        /// ??? Обязательное переопределение от 'FormMainBaseWithStatusStrip'
        /// </summary>
        /// <param name="type"></param>
        protected override void UpdateActiveGui(int type) { }
        /// <summary>
        /// Загрузить вкладки, сохраненные в профиле пользователя
        /// </summary>
        private void loadProfile()
        {
            PlugInMenuItem plugIn;
            string ids = HTepUsers.GetAllowed((int)HTepUsers.ID_ALLOWED.USERPROFILE_PLUGINS)
                /*/*, strNameOwnerMenuItem = string.Empty, strNameMenuItem = string.Empty*/;
            string[] arIds = ids.Split(new char [] {','}, StringSplitOptions.RemoveEmptyEntries);
            ////Вариант №1
            //ToolStripItem[] menuItems;
            //Вариант №2
            ToolStripItem menuItem;
            int id = -1;

            foreach (string strId in arIds)
            {
                int key = -1;

                id = Convert.ToInt32(strId);
                key = s_plugIns.GetKeyOfIdFPanel(id);

                if (!(key < 0))
                {
                    plugIn = s_plugIns[key] as PlugInMenuItem;
                    if (plugIn == null)
                        continue;
                    else
                        ;
                    ////strNameOwnerMenuItem = plugIn.GetNameOwnerMenuItem (id);
                    //strNameMenuItem = plugIn.GetNameMenuItem (id);

                    ////Вариант №1
                    //menuItems = this.MainMenuStrip.Items.Find(strNameMenuItem, true);
                    //menuItem = menuItems[0];
                    ////Вариант №2
                    //menuItem = FindMainMenuItemOfText(strNameMenuItem);
                    //Вариант №3
                    menuItem = findMainMenuItemOfTag(id);

                    if ((menuItem as ToolStripMenuItem).Checked == false)
                    {
                        m_iAutoActionTabs++;
                        menuItem.PerformClick();
                    }
                    else
                        ;
                }
                else
                    Logging.Logg().Warning(@"FormMain::loadProfile () - не удалось загрузить plugIn.Id=" + id + @" ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private ToolStripMenuItem findMainMenuItemOfTag(ToolStripMenuItem miParent, object tag)
        {
            //Результат 
            ToolStripMenuItem itemRes = null;

            //Цикл по всем элементам пункта меню
            foreach (ToolStripItem mi in miParent.DropDownItems)
                if (mi is ToolStripMenuItem)
                    // т.к. объект 'tag' - идентификатор панели
                    if ((! (mi.Tag == null))
                        && ((int)mi.Tag == (int)tag))
                    {
                        itemRes = mi as ToolStripMenuItem;
                        break;
                    }
                    else
                        //Проверить наличие подменю
                        if (((ToolStripMenuItem)mi).DropDownItems.Count > 0)
                        {
                            //Искать элемент в подменю
                            itemRes = findMainMenuItemOfTag(mi as ToolStripMenuItem, tag);

                            if (!(itemRes == null))
                                break;
                            else
                                ;
                        }
                        else
                            ;
                else
                    ;

            return itemRes;
        }

        private ToolStripMenuItem findMainMenuItemOfTag(object tag)
        {
            ToolStripMenuItem itemRes = null;

            foreach (ToolStripMenuItem mi in MainMenuStrip.Items)
            {
                itemRes = findMainMenuItemOfTag(mi, tag);

                if (!(itemRes == null))
                    break;
                else
                    ;
            }

            return itemRes;
        }
        /// <summary>
        /// Обработчик события - выбор п. меню 'Файл - Профиль - Загрузить'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент </param>
        private void профайлЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadProfile();
        }
        /// <summary>
        /// Сохранить профиль пользователя (открытые вкладки)
        /// </summary>
        private void saveProfile()
        {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString());

            string ids = m_TabCtrl.VisibleIDs;
            HTepUsers.SetAllowed(iListenerId, (int)HTepUsers.ID_ALLOWED.USERPROFILE_PLUGINS, ids);

            DbSources.Sources().UnRegister(iListenerId);
        }
        /// <summary>
        /// Обработчик выбора пункта меню 'Файл - Профиль - Сохранить'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
        private void профайлСохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveProfile();
        }
        /// <summary>
        /// Обработчик выбора пункта меню 'Файл - Профиль - Автоматические загрузка/сохранение'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
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
        /// <param name="sender">Объект, инициировавший событие (пункт меню)</param>
        /// <param name="e">Аргумент события</param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Установить признак автоматических действий с вкладками (закрытие)
            m_iAutoActionTabs = m_TabCtrl.TabCount;

            Close ();
            //Снять признак автоматических действий с вкладками (закрытие)
            m_iAutoActionTabs = 0;
        }        
        /// <summary>
        /// Обработчик события - выбор п. меню 'Настройка - БД_конфигурации'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
        private void бДКонфигурацииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iRes = connectionSettings(CONN_SETT_TYPE.MAIN_DB);
            //??? не оработан рез-т выполнения функции
        }
        /// <summary>
        /// Обработчик события - выбор п. меню 'Настройка - Параметры'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (п. меню)</param>
        /// <param name="e">Аргумент события</param>
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
        /// <summary>
        /// Обработчик события - окончание загрузки объекта формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (форма)</param>
        /// <param name="e">Аргумент события</param>
        private void FormMain_Load(object sender, EventArgs e)
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
            this.Icon =
                //(Icon)TepCommon.Properties.Resources.TepApp
                (Icon)Tep64.Properties.Resources.TepApp
                ;

            this.Focus();

            m_report.ClearStates(false);
        }

        //private void FormMain_Shown(object sender, EventArgs e)
        //{
        //    if (m_TabCtrl.TabCount > 0)
        //        m_TabCtrl.PrevSelectedIndex = 0;
        //    else
        //        ;
        //}
        /// <summary>
        /// Метод аврийного завершения
        /// </summary>
        /// <param name="msg">Текст сообщения перед завершением</param>
        /// <param name="bThrow">Признак генерации исключения</param>
        /// <param name="bSupport">Признак включения в сообщение информации о контактах техн./поддержки</param>
        protected override void Abort(string msg, bool bThrow = false, bool bSupport = true)
        {
            //???Удалить все пункты меню...

            base.Abort(msg, bThrow, bSupport);
        }
        /// <summary>
        /// Обработчик события - закрытие формы
        /// </summary>
        /// <param name="sender">Объект, иницировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs ev)
        {
            Stop ();
        }

        private void stopTabPages()
        {
            foreach (TabPage page in m_TabCtrl.TabPages)
                //FindMainMenuItemOfText(page.Text.Trim()).PerformClick();
                findMainMenuItemOfTag(m_TabCtrl.GetTabPageId(m_TabCtrl.TabPages.IndexOf(page))).PerformClick();
        }

        private void removePluginMenuItem()
        {
            List <string> listPluginMenuItem = null;

            listPluginMenuItem = s_plugIns.GetListNameMenuItems();

            foreach (string item in listPluginMenuItem)
                RemoveMainMenuItemOfText(item);
        }
        /// <summary>
        /// Закрыть/удалить текущие вкладки, п. меню, загруженные библиотеки
        /// </summary>
        private void stop()
        {
            // удалить все вкладки, остановить таймер (если есть)
            stopTabPages();
            //Удалить все пункты меню
            removePluginMenuItem();
            //Выгрузить библиотеки
            s_plugIns.Unload();
        }

        protected override void Stop()
        {
            stop();
            
            base.Stop();
        }
        /// <summary>
        /// Делегат обработки события - выбор п. меню
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (плюгИн вкладки)</param>
        private void FormMain_EvtDataAskedHost(object obj)
        {
            this.BeginInvoke(new DelegateObjectFunc(postOnClickMenuItem), obj);
        }
        /// <summary>
        /// Дополнительные действия по инициализации плюг'ина
        /// </summary>
        /// <param name="plugIn">объект плюг'ина</param>
        private void initializePlugIn (IPlugIn plugIn) {
            if (plugIn is HFunc) {
            } else {
            }
        }
        /// <summary>
        /// Инициализация логгирования
        /// </summary>
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
        /// <summary>
        /// Инициализация п.п. главного меню
        ///  в ~ от разрешенных к загрузке плюгИнов
        /// </summary>
        /// <param name="strErr">Сообщение об ошибке</param>
        /// <returns>Результат инициализации меню</returns>
        private int initializeMenu (out string strErr) {
            int iRes = -1
                , idListener = -1
                , iKeyPlugIn = -1, iKeyFPanel = -1;
            strErr = string.Empty;

            string strUserDomainName = string.Empty;
            string []arIdFPanels = null;

            idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString ());

            initializeLogging ();

            s_plugIns.Load(HTepUsers.GetPlugins(idListener, out iRes), out iRes);

            if (iRes == 0) {
                arIdFPanels = HTepUsers.GetIdIsUseFPanels(idListener, out iRes).Split(new char[] { ',' }, StringSplitOptions.None);
                //Проверить рез-т чтения наименования плюгина
                if (iRes == 0)
                {
                    ToolStripMenuItem miOwner = null
                        , miItem = null;
                    string[] arHierarchyOwnerMenuItems;
                    //Циклл по строкам - идентификатрам/разрешениям использовать плюгин
                    foreach (string key in arIdFPanels)
                    {
                        iKeyFPanel = Int32.Parse(key);
                        iKeyPlugIn = s_plugIns.GetKeyOfIdFPanel(iKeyFPanel);
                        if (!(iKeyPlugIn < 0))
                        {
                            arHierarchyOwnerMenuItems = s_plugIns[iKeyPlugIn].GetNameOwnerMenuItem(iKeyFPanel).Split(new char[] { '\\' }, StringSplitOptions.None);
                            //Поиск пункта "родительского" пункта меню для плюг'ина
                            miOwner = FindMainMenuItemOfText(arHierarchyOwnerMenuItems[0]);
                            //Проверка найден ли "родительский" пункт меню для плюг'ина
                            if (miOwner == null)
                            {//НЕ найден - создаем
                                int indx = -1; // индекс для добавляемого пункта                                
                                if (arHierarchyOwnerMenuItems[0].Equals(@"Помощь") == false)
                                    // индекс для всех пунктов кроме "Помощь"
                                    indx = this.MainMenuStrip.Items.Count - 1;
                                else
                                    ;

                                if (indx < 0)
                                    // для пункта "Помощь" - он всегда крайний
                                    //  , и не имеет сложной иерархии
                                    this.MainMenuStrip.Items.Add(miOwner = new ToolStripMenuItem(arHierarchyOwnerMenuItems[0]));
                                else
                                    // для всех пунктов кроме "Помощь"
                                    this.MainMenuStrip.Items.Insert(indx, miOwner = new ToolStripMenuItem(arHierarchyOwnerMenuItems[0]));
                            }
                            else
                                ;
                            //Реализовать иерархию п.п. (признак наличия иерархии - длина массива)
                            for (int i = 1; i < arHierarchyOwnerMenuItems.Length; i++)
                            {
                                //Найти п. меню очередного уровня
                                miItem = FindMainMenuItemOfText(arHierarchyOwnerMenuItems[i]);
                                if (miItem == null)
                                    // в случае отсутствия добавить к ранее найденному
                                    miOwner.DropDownItems.Add(miItem = new ToolStripMenuItem(arHierarchyOwnerMenuItems[i]));
                                else
                                    ;

                                miOwner = miItem;
                            }
                            //Добавить пункт меню для плюг'ина
                            miItem = miOwner.DropDownItems.Add(s_plugIns[iKeyPlugIn].GetNameMenuItem(iKeyFPanel)) as ToolStripMenuItem;
                            miItem.Tag = iKeyFPanel;
                            //Обработку выбора пункта меню предоставить плюг'ину
                            miItem.Click += s_plugIns[iKeyPlugIn].OnClickMenuItem; //postOnClickMenuItem;
                            //Добавить обработчик запросов для плюг'ина от главной формы
                            // только ОДИН раз
                            if ((s_plugIns[iKeyPlugIn] as PlugInBase).IsEvtDataAskedHostHandled == false)
                                (s_plugIns[iKeyPlugIn] as PlugInBase).EvtDataAskedHost += new DelegateObjectFunc(s_plugIns.OnEvtDataAskedHost);
                            else
                                ;

                            initializePlugIn(s_plugIns[iKeyPlugIn]);
                        }
                        else
                            Logging.Logg().Error(@"FormMain::initializeMenu () - не найден плюгИн для вкладки (ID=" + iKeyFPanel + @")...", Logging.INDEX_MESSAGE.NOT_SET);
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
                strErr = @"Ошибка при загрузке библиотек - см. лог-файл...";
            }            

            DbSources.Sources().UnRegister(idListener);

            return iRes;
        }

        //private void onClickMenuItem(object obj, EventArgs ev)
        //{
        //    EventPlugInMenuItemClick(obj, new PlugInMenuItem.PlugInMenuItemEventArgs(-1));
        //}
        /// <summary>
        /// Обработчик события выбора (отобразить/закрыть вкладку) п. меню
        /// </summary>
        /// <param name="obj">Объект загруженной библиотеки вкладки</param>
        private void postOnClickMenuItem (object obj) {
            int idPlugIn = (int)((EventArgsDataHost)obj).id_main
                , idFPanel = (int)((EventArgsDataHost)obj).id_detail;
            PlugInMenuItem plugIn = s_plugIns[idPlugIn];
            bool bMenuItemChecked =
            ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked =
                ! ((ToolStripMenuItem)((EventArgsDataHost)obj).par[0]).Checked;

            if (bMenuItemChecked == true)
            {
                //Отобразить вкладку
                m_TabCtrl.AddTabPage(plugIn.GetNameMenuItem(idFPanel), idFPanel, HTabCtrlEx.TYPE_TAB.FIXED);
                m_TabCtrl.TabPages[m_TabCtrl.TabCount - 1].Controls.Add((Control)plugIn.GetObject(idFPanel));
            } else {
                //Закрыть вкладку
                m_TabCtrl.RemoveTabPage(plugIn.GetNameMenuItem(idFPanel));
            }

            if (m_iAutoActionTabs > 0)
            {
                m_iAutoActionTabs--;
            }
            else
                ;

            if (m_iAutoActionTabs == 0)
            {// закончился процесс автоматической загрузки (создания/добавления) вкладок
                if ((профайлАвтоЗагрузитьСохранитьToolStripMenuItem as ToolStripMenuItem).Checked == true)
                {
                    saveProfile();
                }
                else
                    ;

                if ((m_TabCtrl.PrevSelectedIndex < 0)
                    && (bMenuItemChecked == true))
                {// только, если перед действием не была добавлена ни одна вкладка
                    m_TabCtrl.PrevSelectedIndex = 0;
                }
                else
                    ;
            }
            else
                ;
        }
        /// <summary>
        /// Обработчик события - закрытие вкладки
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (???)</param>
        /// <param name="e">Аргумент события</param>
        private void onCloseTabPage(object sender, HTabCtrlExEventArgs e)
        {
            ////Вариант №1
            //FindMainMenuItemOfText (e.TabHeaderText).Checked = false;
            //m_TabCtrl.TabPages.RemoveAt (e.TabIndex);

            ////Вариант №2
            //FindMainMenuItemOfText (e.TabHeaderText).PerformClick ();

            //Вариант №3
            findMainMenuItemOfTag(e.Id).PerformClick();
        }
        /// <summary>
        /// Дополнительная инициализация компонентов формы
        /// </summary>
        /// <param name="strErr">Текст сообщения об ошибке при выполнении инициализации</param>
        /// <returns>Результат дополнительной инициализации</returns>
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
        /// <summary>
        /// Настроить/применить праметры соединения с БД_конфигурации
        /// </summary>
        /// <param name="type">Индекс параметров соединения с БД</param>
        /// <returns>Результат выполнения функции</returns>
        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1
                , idListener = -1;
            DialogResult result;
            //Отобразить окно с параметрами соединения
            result = s_listFormConnectionSettings[(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                //Очистить/выгрузить список плюгИнов, пункты меню
                stop();                

                iRes = s_listFormConnectionSettings[(int)type].Ready;

                string msg = string.Empty;
                if (iRes == 0)
                {
                    // персонализация в новой БД - аналог в конструкторе формы
                    idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)type].getConnSett(), false, type.ToString());
                    initProfiles(idListener);
                    // закрыть соединение с новой БД
                    DbSources.Sources().UnRegister(idListener);

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

        private void TabCtrl_EventPrevSelectedIndexChanged(int iPrevSelectedindex)
        {
            //'Activate(false)' и 'Stop' вызываются в 'PlugIn'-е
            //activatePlugIn (m_TabCtrl.GetTabPageId(iPrevSelectedindex), false);
            activateFPanel(m_TabCtrl.GetTabPageId(iPrevSelectedindex), false);
            activateFPanel (m_TabCtrl.GetTabPageId(), true);
        }

        private void activateFPanel(int id_fpanel, bool bActivate)
        {
            int id_plugIn = -1;

            if (!(id_fpanel < 0))
            {
                id_plugIn = s_plugIns.GetKeyOfIdFPanel(id_fpanel);

                if (!(id_plugIn < 0))
                    //Отправить ответ (исходный идентификатор + требуемый объект)
                    ((PlugInBase)s_plugIns[id_plugIn]).OnEvtDataRecievedHost(new EventArgsDataHost(id_fpanel, (int)HFunc.ID_DATAASKED_HOST.ACTIVATE_TAB, new object[] { bActivate }));
                else
                    ;
            }
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменение активной вкладки
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (???)</param>
        /// <param name="ev">Аргумент события</param>
        private void TabCtrl_OnSelectedIndexChanged (object obj, EventArgs ev)
        {
            (obj as HTabCtrlEx).PrevSelectedIndex = (obj as HTabCtrlEx).SelectedIndex;
        }
        /// <summary>
        /// Изменить подписи в строке состояния
        /// </summary>
        /// <returns>Признак результата выполнения функции</returns>
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
        /// <summary>
        /// Выполнить доп./действия при старте таймера
        /// </summary>
        protected override void timer_Start()
        {//m_timer.Interval == ProgramBase.TIMER_START_INTERVAL
        }
    }
}
