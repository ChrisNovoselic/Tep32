using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection; //Assembly
using System.Windows.Forms;

using System.Data.Common; //DbConnection
using System.Data; //DataTable

//using System.Security.Policy.Evidence;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace Tep64
{
    public partial class FormMain
    {        
        class HPlugIns : Dictionary <int, PlugInMenuItem>, IPlugInHost
            //, IEnumerable <IPlugIn>
        {
            public DelegateObjectFunc delegateOnClickMenuPluginItem;
            //http://stackoverflow.com/questions/658498/how-to-load-an-assembly-to-appdomain-with-all-references-recursively
            //http://lsd.luminis.eu/load-and-unload-assembly-in-appdomains/
            //http://www.codeproject.com/Articles/453778/Loading-Assemblies-from-Anywhere-into-a-New-AppDom
            private class ProxyAppDomain : MarshalByRefObject
            {
                public Assembly GetAssembly(string AssemblyPath)
                {
                    try
                    {
                        return Assembly.LoadFrom(AssemblyPath);
                        //If you want to do anything further to that assembly, you need to do it here.
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(ex.Message, ex);
                    }
                }
            }

            private AppDomain m_appDomain;
            private ProxyAppDomain m_proxyAppDomain;
            private static System.Security.Policy.Evidence s_domEvidence = AppDomain.CurrentDomain.Evidence;
            private static AppDomainSetup s_domSetup = new AppDomainSetup();

            public HPlugIns(DelegateObjectFunc fClickMenuItem)
            {
                s_domSetup = new AppDomainSetup();
                s_domSetup.ApplicationBase = System.Environment.CurrentDirectory;
                s_domEvidence = AppDomain.CurrentDomain.Evidence;

                //_dictPlugins = new Dictionary<int, IPlugIn>();
                delegateOnClickMenuPluginItem = fClickMenuItem;
            }
            /// <summary>
            /// Установить взамосвязь
            /// </summary>
            /// <param name="plug">Загружаемый плюгИн</param>
            /// <returns>Признак успешности загрузки</returns>
            public int Register(IPlugIn plug)
            {
                //??? важная функция для взимного обмена сообщенями
                return 0;
            }

            private bool isInitPluginAppDomain { get { return (!(m_appDomain == null)) && (!(m_proxyAppDomain == null)); } }

            private void initPluginDomain()
            {
                m_appDomain = AppDomain.CreateDomain("pluginDomain", s_domEvidence, s_domSetup);

                Type type = typeof(ProxyAppDomain);
                m_proxyAppDomain = (ProxyAppDomain)m_appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            }

            public void Unload()
            {
                if (isInitPluginAppDomain == true)
                {
                    AppDomain.Unload(m_appDomain);

                    m_appDomain = null;
                    m_proxyAppDomain = null;
                }
                else
                    ;

                Clear();
            }
            /// <summary>
            /// Загрузить плюгИн с указанным наименованием
            /// </summary>
            /// <param name="name">Наименование плюгИна</param>
            /// <param name="iRes">Результат загрузки (код ошибки)</param>
            /// <returns>Загруженный плюгИн</returns>
            private PlugInMenuItem load(string name, out int iRes)
            {
                PlugInMenuItem plugInRes = null;
                iRes = -1;

                Type objType = null;
                try
                {
                    if (isInitPluginAppDomain == false)
                        initPluginDomain();
                    else
                        ;

                    Assembly ass = null;
                    ass =
                        m_proxyAppDomain.GetAssembly
                        //Assembly.LoadFrom
                        //m_appDomain.Load
                            (Environment.CurrentDirectory + @"\" + name + @".dll");

                    if (!(ass == null))
                    {
                        objType = ass.GetType(name + ".PlugIn");
                    }
                    else
                        ;
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"FormMain::loadPlugin () ... LoadFrom () ... plugIn.Name = " + name, Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(objType == null))
                    try
                    {
                        plugInRes = ((PlugInMenuItem)Activator.CreateInstance(objType));
                        plugInRes.Host = (IPlugInHost)this; //Вызов 'Register'

                        iRes = 0;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"FormMain::loadPlugin () ... CreateInstance ... plugIn.Name = " + name, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else
                    Logging.Logg().Error(@"FormMain::loadPlugin () ... Assembly.GetType()=null ... plugIn.Name = " + name, Logging.INDEX_MESSAGE.NOT_SET);

                return plugInRes;
            }
            /// <summary>
            /// Обработчик запросов от загруженных плюгИнов
            /// </summary>
            /// <param name="obj">Детализация запроса (объект 'EventArgsDataHost')</param>
            public void OnEvtDataAskedHost(object obj)
            {
                object rec = null;

                if (((EventArgsDataHost)obj).par[0].GetType().IsPrimitive == true)
                {
                    switch ((int)((EventArgsDataHost)obj).par[0])
                    {
                        case (int)HFunc.ID_DATAASKED_HOST.CONNSET_MAIN_DB:
                            rec = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett();
                            break;
                        case (int)HFunc.ID_DATAASKED_HOST.ICON_MAINFORM:
                            rec =
                                //TepCommon.Properties.Resources.TepApp
                                Tep64.Properties.Resources.TepApp
                                ;
                            break;
                        case (int)HFunc.ID_DATAASKED_HOST.STR_VERSION:
                            rec = Application.ProductVersion;
                            break;
                        default: // обработка индивидуальных для каждой вкладки запросов
                            switch ((int)((EventArgsDataHost)obj).id_detail)
                            {
                                case 1: //FormAboutTepProgram
                                    switch ((int)((EventArgsDataHost)obj).par[0])
                                    {                                        
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
                                case 17: //PanelTaskTepValues, Расчет ТЭП - входные значения
                                case 18: //PanelTaskTepValues, Расчет ТЭП - вЫходные значения
                                    switch ((int)((EventArgsDataHost)obj).par[0])
                                    {
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                    //Отправить ответ (исходный идентификатор + требуемый объект)
                    ((PlugInBase)this[((EventArgsDataHost)obj).id_main]).OnEvtDataRecievedHost(
                        new EventArgsDataHost(
                            ((EventArgsDataHost)obj).id_detail
                            , (int)((EventArgsDataHost)obj).par[0]
                            , new object[] { rec }));
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
                            Logging.Logg().Exception(e, @"FormMain_EvtDataAskedHost () - BeginInvoke (addTabPage) [id] = " + (int)((EventArgsDataHost)obj).id_detail, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        if (((EventArgsDataHost)obj).par[0] is object [])
                        {
                            object[] pars = ((EventArgsDataHost)obj).par[0] as object[];
                        }
                        else
                            ;
                }
            }
            /// <summary>
            /// Загрузить все плюгИны
            /// </summary>
            /// <param name="tableNamePlugins">Таблица с наименованиями</param>
            public void Load (DataTable tableFPanels, out int err)
            {
                err = 0;

                int idPlugIn = -1, idFPanel = -1;
                PlugInMenuItem plugIn = null;

                if (!(tableFPanels == null))
                    //Цикл по строкам - идентификатрам/разрешениям использовать функц./панель из плюгина
                    for (int i = 0; (i < tableFPanels.Rows.Count) && (err == 0); i++)
                    {
                        //Идентификатор плюг'ина
                        idPlugIn = Int16.Parse(tableFPanels.Rows[i][@"ID_PLUGIN"].ToString());

                        if (ContainsKey(idPlugIn) == false)
                        {
                            plugIn = load(tableFPanels.Rows[i][@"NAME_PLUGIN"].ToString().Trim(), out err);

                            if (err == 0)
                            {
                                //Проверка на соответствие идентификаторов в БД и коде (м.б. и не нужно???)
                                if (((PlugInBase)plugIn)._Id == idPlugIn)
                                {
                                    Add(idPlugIn, plugIn);
                                }
                                else
                                    err = -2;
                            }
                            else
                                ; // ошибка при загрузке плюгИна
                        }
                        else
                            ; //plugIn уже был загружен
                    }
                else
                {
                    err = -1;

                    Logging.Logg().Error(@"HPlugIns::Load () - входная таблица = NULL...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public int GetKeyOfIdFPanel(int idFPanel)
            {
                int iRes = -1;

                foreach (KeyValuePair<int, PlugInMenuItem> pair in this)
                    if (pair.Value.IsRegistred(idFPanel) == true)
                    {
                        iRes = pair.Key;

                        break;
                    }
                    else
                        ;

                return iRes;
            }

            public List<string> GetListNameMenuItems()
            {
                List<string> listRes = new List<string> ();

                foreach (KeyValuePair<int, PlugInMenuItem> pair in this)
                    listRes.AddRange (pair.Value.GetNameMenuItems());

                return listRes;
            }
        }
    }
}
