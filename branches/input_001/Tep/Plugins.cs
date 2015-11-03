using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection; //Assembly
using System.Windows.Forms;

using System.Data.Common; //DbConnection
using System.Data; //DataTable

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace Tep64
{
    public partial class FormMain
    {
        class HPlugIns : Dictionary <int, IPlugIn>, IPlugInHost
            //, IEnumerable <IPlugIn>
        {
            //public IPlugIn GetEnumerator () {
            //    IPlugIn plugInRes = null;

            //    return plugInRes;
            //}

            //public IPlugIn MoveNext ()
            //{
            //    IPlugIn plugInRes = null;

            //    return plugInRes;
            //}

            //private int _key_current;
            //public IPlugIn Current { get { return m_dictPlugins[_key_current]; } }
            
            //private Dictionary<int, IPlugIn> _dictPlugins;

            public DelegateObjectFunc delegateOnClickMenuPluginItem;

            public HPlugIns(DelegateObjectFunc fClickMenuItem)
            {
                //_dictPlugins = new Dictionary<int, IPlugIn>();
                delegateOnClickMenuPluginItem = fClickMenuItem;
            }

            public bool Register(IPlugIn plug)
            {
                return true;
            }

            //public override void Add(int id, IPlugIn plugIn)
            //{
            //    _dictPlugins.Add(id, plugIn);
            //}

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

            public PlugInMenuItem Find(int id)
            {
                if (this.ContainsKey(id) == true)
                    return this[id] as PlugInMenuItem;
                else
                    return null;
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
                    ((PlugInBase)this[((EventArgsDataHost)obj).id]).OnEvtDataRecievedHost(new EventArgsDataHost((int)((EventArgsDataHost)obj).par[0], new object[] { rec }));
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

            public void Load (DataTable tableNamePlugins)
            {
                int iRes = 0
                    , idPlugIn = -1;
                IPlugIn plugIn = null;

                //Циклл по строкам - идентификатрам/разрешениям использовать плюгин
                for (int i = 0; (i < tableNamePlugins.Rows.Count) && (iRes == 0); i++)
                {
                    plugIn = Load(tableNamePlugins.Rows[i][@"NAME"].ToString().Trim(), out iRes);

                    if (iRes == 0) {
                        //Идентификатор плюг'ина
                        idPlugIn = Int16.Parse(tableNamePlugins.Rows[i][@"ID"].ToString());
                        //Проверка на соответствие идентификаторов в БД и коде (м.б. и не нужно???)
                        if (((PlugInBase)plugIn)._Id == idPlugIn)
                        {
                            Add(idPlugIn, plugIn);
                        }
                        else
                            iRes =  -2;
                    }
                    else
                        ;
                }
            }
        }
    }
}
