using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace InterfacePlugIn
{
    public abstract class HFunc : PlugInMenuItem
    {
        public enum ID_DATAASKED_HOST { ICON_MAINFORM = 1001, STR_VERSION = 1002 //Запросить данные у главной формы
                                    , CONNSET_MAIN_DB = 10001
                                    , ACTIVATE_TAB = 10101
                                    };
    }

    public abstract class HFuncDbEdit : HFunc
    {
        /// <summary>
        /// Обработчик события - выбор п. меню
        /// </summary>
        /// <param name="obj">Объект, имнициировавщий событие (п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            int id = -1;
            KeyValuePair<int, int> pair;
            
            base.OnClickMenuItem(obj, ev);

            id = (int)(obj as ToolStripMenuItem).Tag;
            pair = new KeyValuePair<int, int>(id, (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);

            //Проверить признак выполнения запроса к вызвавшему объекту на получение параметров соединения с БД 
            if (m_dictDataHostCounter.ContainsKey(pair) == false)
                // отправить запрос на получение параметров соединения с БД
                DataAskedHost(new object [] {id, (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB}); //Start
            else
                if (m_dictDataHostCounter[pair] % 2 == 0)
                    DataAskedHost(new object [] {id, (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB}); //Start
                else
                {
                    m_dictDataHostCounter[pair]++;

                    (_objects[id] as HPanelCommon).Activate(false);
                    (_objects[id] as HPanelCommon).Stop();
                }

            //Вернуть главной форме параметр
            DataAskedHost(new object [] {id, obj});
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            int id = -1;
            
            //throw new NotImplementedException();

            base.OnEvtDataRecievedHost(obj);

            id = ((EventArgsDataHost)obj).id_main; // идентификатор объекта (см. 'OnEvtDataAskedHost')

            switch (((EventArgsDataHost)obj).id_detail)
            {
                case (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB:
                    ((IObjectDbEdit)_objects[id]).Start((obj as EventArgsDataHost).par[0]);
                    break;
                case (int)ID_DATAASKED_HOST.ACTIVATE_TAB:
                    (_objects[id] as HPanelCommon).Activate((bool)(obj as EventArgsDataHost).par[0]);
                    break;
                default:
                    break;
            }
        }
    }

    public interface IObjectDbEdit {
        /// <summary>
        /// Инициализация значением, полученным от главной формы
        /// </summary>
        /// <param name="obj">Массив объектов для инициализации</param>
        void Start (object obj);

        //void Stop();

        //int Activate(bool bActivate);
    }
}
