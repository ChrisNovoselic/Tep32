using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace InterfacePlugIn
{
    public abstract class HFunc : HPlugIn
    {
        public enum ID_DATAASKED_HOST { ICON_MAINFORM, STR_VERSION //Запросить данные у главной формы
                                        , HIDE_PLIGIN //Скрыть закладку (окно) с плюг'ином
                                        , AS_WINDOW //Объект плюг'ина в виде самостоятельного окна
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
}
