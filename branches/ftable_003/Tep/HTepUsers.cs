using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection

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
            get { return (int)(m_DataRegistration[(int)INDEX_REGISTRATION.ROLE] == null ? -1 : m_DataRegistration[(int)INDEX_REGISTRATION.ROLE]); }
        }

        public static string GetIdIsUsePlugins(ref DbConnection dbConn, out int iRes)
        {
            string strRes = string.Empty;
            iRes = -1;

            DataTable tableRoles = null;

            HUsers.GetRoles(ref dbConn, @"(ID_EXT=" + Role + @" AND IS_ROLE=1)"
                + @" OR (ID_EXT=" + Id + @" AND IS_ROLE=0)"
                , string.Empty, out tableRoles, out iRes);

            int i = -1
                , indxRow = -1
                , idPlugin = -1;
            //Сформировать список идентификаторов плюгинов
            DataRow[] rowsIsUse;
            List <int> listIdParsedPlugins = new List<int> ();

            //Цикл по строкам - идентификатрам/разрешениям использовать плюгин                    
            for (i = 0; i < tableRoles.Rows.Count; i++)
            {
                idPlugin = (Int16)tableRoles.Rows[i][@"ID_PLUGIN"];
                if (listIdParsedPlugins.IndexOf(idPlugin) < 0)
                {
                    listIdParsedPlugins.Add (idPlugin);
                    //??? возможна повторная обработка
                    indxRow = -1;
                    rowsIsUse = tableRoles.Select(@"ID_PLUGIN=" + idPlugin);
                    //Проверить разрешение использовать плюгин
                    switch (rowsIsUse.Length)
                    {
                        case 0:
                            break;
                        case 1: // в БД указано разрешение только для группы (пользователя)
                            indxRow = 0;
                            break;
                        case 2: // в БД указаны значения как для группы так для пользователя
                            foreach (DataRow r in rowsIsUse)
                            {
                                indxRow++;

                                if ((Byte)r[@"IS_ROLE"] == 0)
                                    // приоритет индивидуальной настройки
                                    break;
                                else
                                    ;
                            }
                            break;
                        default:
                            throw new Exception(@"HTepUsers::GetIdIsUsePlugins (ID_PLUGIN=" + tableRoles.Rows[i][@"ID_PLUGIN"]);
                    }

                    if (!(indxRow < 0))
                        if ((Byte)rowsIsUse[indxRow][@"IsUse"] == 1)
                            strRes += idPlugin + @",";
                        else
                            ;
                    else
                        ;
                }
                else
                    ; // плюгИн уже обработан
            }
            //Удалить крайний символ
            if (strRes.Length > 0)
                strRes = strRes.Substring(0, strRes.Length - 1);
            else
                ;

            return strRes;
        }

        public static string GetIdIsUsePlugins(int idListener, out int iRes)
        {
            string strRes = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out iRes);

            if (iRes == 0)
                strRes = GetIdIsUsePlugins (ref dbConn, out iRes);
            else
                ;

            return strRes;
        }

        public static DataTable GetPlugins(int idListener, out int iRes)
        {
            DataTable tableRes = null;
            iRes = -1;
            string strIdPlugins = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out iRes);

            if (iRes == 0)
            {
                strIdPlugins = GetIdIsUsePlugins(ref dbConn, out iRes);

                if (iRes == 0)
                {
                    //Прочитать наименования плюгинов
                    tableRes = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM plugins WHERE ID IN (" + strIdPlugins + @")", null, null, out iRes);
                }
                else
                    ;
            }
            else
                ;

            return tableRes;
        }
    }
}
