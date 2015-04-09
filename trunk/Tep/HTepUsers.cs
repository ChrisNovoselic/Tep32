using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace Tep64
{
    class HTepUsers : HUsers
    {
        public enum ID_ROLES
        {
            UNKNOWN, ADMIN = 1, DEV, USER, USER_PTO, SOURCE_DATA = 501,
            COUNT_ID_ROLES = 5
        };

        //Идентификаторы из БД
        public enum ID_ALLOWED
        {
            UNKNOWN = -1
            , AUTO_LOADSAVE_USERPROFILE_ACCESS = 1 //Автоматическая загрузка/сохранение профиля
            , AUTO_LOADSAVE_USERPROFILE_CHECKED //
            , USERPROFILE_PLUGINS
            ,
        };
        public HTepUsers(int iListenerId)
            : base(iListenerId)
        {
        }

        public static int Role
        {
            get { return (int)m_DataRegistration[(int)INDEX_REGISTRATION.ROLE]; }
        }
    }
}
