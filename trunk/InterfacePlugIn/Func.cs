using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace InterfacePlugIn
{
    public abstract class HFunc : PlugInMenuItem
    {
        public enum ID_DATAASKED_HOST { ICON_MAINFORM = 1001, STR_VERSION = 1002 //Запросить данные у главной формы
                                    , CONNSET_MAIN_DB = 10001
                                    , ACTIVATE_TAB = 10101
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

    public abstract class HFuncDbEdit : HFunc
    {
        /// <summary>
        /// Обработчик события - выбор п. меню
        /// </summary>
        /// <param name="obj">Объект, имнициировавщий событие (п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            //Проверить признак выполнения запроса к вызвавшему объекту на получение параметров соединения с БД 
            if (m_dictDataHostCounter.ContainsKey((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB) == false)
                // отправить запрос на получение параметров соединения с БД
                DataAskedHost((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);
            else
                if (m_dictDataHostCounter[(int)ID_DATAASKED_HOST.CONNSET_MAIN_DB] % 2 == 0)
                    DataAskedHost((int)ID_DATAASKED_HOST.CONNSET_MAIN_DB);
                else
                    m_dictDataHostCounter[(int)ID_DATAASKED_HOST.CONNSET_MAIN_DB]++;

            //Вернуть главной форме параметр
            DataAskedHost(obj);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            //throw new NotImplementedException();

            base.OnEvtDataRecievedHost(obj);

            switch (((EventArgsDataHost)obj).id)
            {
                case (int)ID_DATAASKED_HOST.CONNSET_MAIN_DB:
                    ((IObjectDbEdit)_object).Initialize(obj);
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
        void Initialize (object obj);
    }
}
