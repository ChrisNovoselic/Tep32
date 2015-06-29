using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace InterfacePlugIn
{
    public abstract class HFunc : PlugInMenuItem
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
                    ((IObjectDictEdit)_object).Initialize(obj);
                    break;
                default:
                    break;
            }
        }
    }

    public interface IObjectDictEdit {
        void Initialize (object obj);
    }
}
