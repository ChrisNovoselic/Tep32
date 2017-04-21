using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        class PlugIns : HPlugIns
        {
            /// <summary>
            /// Делегат обработки события - выбор п. меню
            /// </summary>
            public Action<EventArgsDataHost> delegateOnEvtDataAskedHost;
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="fClickMenuItem">Делегат обработки события - ваыбор п. меню</param>
            public PlugIns(Action<EventArgsDataHost> fOnEvtDataAskedHost) : base ()
            {
                //_dictPlugins = new Dictionary<int, IPlugIn>();
                delegateOnEvtDataAskedHost = fOnEvtDataAskedHost;
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
                        case (int)HFunc.ID_FUNC_DATA_ASKED_HOST.CONNSET_MAIN_DB:
                            rec = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett();
                            break;
                        case (int)HFunc.ID_FUNC_DATA_ASKED_HOST.ICON_MAINFORM:
                            rec =
                                //TepCommon.Properties.Resources.TepApp
                                Tep64.Properties.Resources.TepApp
                                ;
                            break;
                        case (int)HFunc.ID_FUNC_DATA_ASKED_HOST.STR_PRODUCTVERSION:
                            rec = Application.ProductVersion
                                + @", " + Environment.MachineName
                                + @", " + Environment.UserDomainName + @"\" + Environment.UserName;
                            break;
                        case (int)HFunc.ID_FUNC_DATA_ASKED_HOST.MESSAGE_TO_STATUSSTRIP:
                            // TODO: отобразить сообщение в строке статуса 
                            delegateOnEvtDataAskedHost(obj as EventArgsDataHost);
                            break;
                        default: // обработка индивидуальных для каждой вкладки запросов
                            switch ((ID_FPANEL)((EventArgsDataHost)obj).id_detail)
                            {
                                case ID_FPANEL.ABOUT: //FormAboutTepProgram
                                    switch ((int)((EventArgsDataHost)obj).par[0])
                                    {                                        
                                        default:
                                            break;
                                    }
                                    break;
                                case ID_FPANEL.DICTIONARY_PLUGINS: //PanelTepDictPlugIns
                                    switch ((int)((EventArgsDataHost)obj).par[0])
                                    {
                                        default:
                                            break;
                                    }
                                    break;
                                case ID_FPANEL.TEPMAIN_INVALUES: //PanelTaskTepValues, Расчет ТЭП - входные значения
                                case ID_FPANEL.TEPMAIN_OUTVALUES: //PanelTaskTepValues, Расчет ТЭП - вЫходные значения
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
                            delegateOnEvtDataAskedHost(obj as EventArgsDataHost);
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
                            plugIn = load(tableFPanels.Rows[i][@"NAME_PLUGIN"].ToString().Trim(), out err) as PlugInMenuItem;

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
            /// <summary>
            /// Возвратить идентификатор плюгИна по идентификатору реализованной в нем панели
            /// </summary>
            /// <param name="idFPanel">Идентификатор панели, реализованной в плюгИне</param>
            /// <returns>Идентификатор плюгИна</returns>
            public int GetKeyOfIdFPanel(int idFPanel)
            {
                int iRes = -1;
                KeyValuePair<int, PlugInBase>? pairRes = null;

                // цикл по всем загруженным плюгИнам - проверить регистрацию панели в плюгИне
                pairRes = this.FirstOrDefault(item => { return item.Value.IsRegistred(idFPanel) == true; });

                if (!(pairRes == null))
                    iRes = pairRes.GetValueOrDefault().Key;
                else
                    ;

                //// цикл по всем загруженным плюгИнам
                //foreach (KeyValuePair<int, PlugInBase> pair in this)
                //    // проверить регистрацию панели в плюгИне
                //    if (pair.Value.IsRegistred(idFPanel) == true)
                //    {
                //        iRes = pair.Key;

                //        break;
                //    }
                //    else
                //        ;

                return iRes;
            }
            /// <summary>
            /// Возвратить список п.п. меню для всех загруженных плюгИнов
            /// </summary>
            /// <returns>Список п.п. меню</returns>
            public List<string> GetListNameMenuItems()
            {
                List<string> listRes = new List<string> ();

                foreach (KeyValuePair<int, PlugInBase> pair in this)
                    listRes.AddRange ((pair.Value as PlugInMenuItem).GetNameMenuItems());

                return listRes;
            }
        }
    }
}
