using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection

using HClassLibrary;

namespace TepCommon
{
    public class HTepUsers : HUsers
    {
        /// <summary>
        /// Перечисление - идентификаторы ролей (групп) пользователей из БД [roles_unit]
        /// </summary>
        public enum ID_ROLES
        {
            UNKNOWN, ADMIN = 1, DEV, USER, USER_PTO, SOURCE_DATA = 501,
            COUNT_ID_ROLES = 5
        };
        /// <summary>
        /// Перечисление - идентификаторы настраиваемых параметров из БД [profiles_unit]
        /// </summary>
        public enum ID_ALLOWED
        {
            UNKNOWN = -1
            , AUTO_LOADSAVE_USERPROFILE_ACCESS = 1 //Разрешение изменять свойство "Автоматическая загрузка/сохранение ..."
            , AUTO_LOADSAVE_USERPROFILE_CHECKED //Автоматическая загрузка/сохранение списка идентификаторов вкладок, загружаемых автоматически
            , USERPROFILE_PLUGINS //Список вкладок, загружаемых автоматически
            , VISUAL_SETTING_VALUE_ROUND //Отображение значений, количество знаков после запятой
            , VISUAL_SETTING_VALUE_RATIO //Отображение значений, множитель относительно базовой единицы измерения
            , QUERY_TIMEZONE //Идентификатор часового пояса при запросе значений
            ,
        };
        /// <summary>
        /// Перечисление - индексы в массиве - аргументе функции 'GetParameterVisualSettings'
        /// </summary>
        public enum INDEX_VISUALSETTINGS_PARAMS { /*TASK, PLUGIN, */TAB, ITEM
            , COUNT }
        /// <summary>
        /// Конструктор основной - с  параметром
        /// </summary>
        /// <param name="iListenerId">Идентификатор установленного соединения с БД</param>
        public HTepUsers(int iListenerId)
            : base(iListenerId)
        {
        }
        /// <summary>
        /// Роль пользователя (из БД конфигурации)
        /// </summary>
        public static int Role
        {
            get { return (int)(m_DataRegistration[(int)INDEX_REGISTRATION.ROLE] == null ? -1 : m_DataRegistration[(int)INDEX_REGISTRATION.ROLE]); }
        }
        /// <summary>
        /// Получить строку с идентификаторами плюгинов, разрешенных к использованию для пользователя
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="iRes">Результат выполнения функции</param>
        /// <returns>Строка с идентификаторами (разделитель - запятая)</returns>
        public static string GetIdIsUseFPanels(ref DbConnection dbConn, out int iRes)
        {
            string strRes = string.Empty;
            iRes = -1;

            DataTable tableRoles = null;

            HUsers.GetRoles(ref dbConn, @"(ID_EXT=" + Role + @" AND IS_ROLE=1)"
                + @" OR (ID_EXT=" + Id + @" AND IS_ROLE=0)"
                , string.Empty, out tableRoles, out iRes);

            int i = -1
                , indxRow = -1
                , idFPanel = -1;
            //Сформировать список идентификаторов плюгинов
            DataRow[] rowsIsUse;
            List <int> listIdParsedFPanel = new List<int> ();

            if (!(tableRoles.Columns.IndexOf(@"ID_FPANEL") < 0))
            {
                //Цикл по строкам - идентификатрам/разрешениям использовать плюгин                    
                for (i = 0; i < tableRoles.Rows.Count; i++)
                {
                    idFPanel = (Int16)tableRoles.Rows[i][@"ID_FPANEL"];
                    if (listIdParsedFPanel.IndexOf(idFPanel) < 0)
                    {
                        listIdParsedFPanel.Add(idFPanel);
                        //??? возможна повторная обработка
                        indxRow = -1;
                        rowsIsUse = tableRoles.Select(@"ID_FPANEL=" + idFPanel);
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
                                strRes += idFPanel + @",";
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
            }
            else
            {// не найдено необходимое поле в таблице для  формирования списка вккладок для пользователя
                iRes = -1;

                Logging.Logg().Error(@"HTepUsers::GetIdIsUseFPanels () - не найдено необходимое поле [ID_FPANEL] в таблице [" + tableRoles.TableName
                    + @"] для  формирования списка вккладок для пользователя"
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return strRes;
        }
        /// <summary>
        /// Получить строку с идентификаторами плюгинов, разрешенных к использованию для пользователя
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД</param>
        /// <param name="iRes">Результат выполнения функции</param>
        /// <returns>Строка с идентификаторами (разделитель - запятая)</returns>
        public static string GetIdIsUseFPanels(int idListener, out int iRes)
        {
            string strRes = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out iRes);

            if (iRes == 0)
                strRes = GetIdIsUseFPanels (ref dbConn, out iRes);
            else
                ;

            return strRes;
        }
        /// <summary>
        /// Получить таблицу с описанием библиотек, разрешенных к использованию
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД</param>
        /// <param name="iRes">Результат выполнения функции</param>
        /// <returns>Таблица с описанием плюгинов</returns>
        public static DataTable GetPlugins(int idListener, out int iRes)
        {
            DataTable tableRes = null;
            iRes = -1;
            string strIdFPanels = string.Empty;
            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out iRes);

            if (iRes == 0)
            {
                strIdFPanels = GetIdIsUseFPanels(ref dbConn, out iRes);

                if (iRes == 0)
                {
                    //Прочитать наименования плюгинов
                    tableRes = DbTSQLInterface.Select(ref dbConn
                        ,
                            //@"SELECT * FROM plugins WHERE ID IN ("
                            @"SELECT p.[ID] as [ID_PLUGIN], p.[NAME] as [NAME_PLUGIN] FROM plugins as p WHERE [ID] IN (SELECT [ID_PLUGIN] FROM [fpanels] WHERE [ID] IN ("
                                 + strIdFPanels + @")" + @")"
                        , null, null, out iRes);
                }
                else
                    ;
            }
            else
                ;

            return tableRes;
        }

        public struct VISUAL_SETTING
        {
            public int m_ratio;
            public int m_round;
        }

        public static int s_iRatioDefault = 0
            , s_iRoundDefault = 2;
        /// <summary>
        /// Получить таблицу с установками для отображения значений
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД</param>
        /// <param name="fields">Значения для подстановки в предложение 'where' при выборке записей из профиля группы пользователя (пользователя)</param>
        /// <param name="err">Результат выполнения функции</param>
        /// <returns>Таблица с установками для отображения значений</returns>
        public static Dictionary<string, VISUAL_SETTING> GetParameterVisualSettings(ConnectionSettings connSett, int[] fields, out int err)
        {
            err = -1; //Обшая ошибка
            int idListener = -1;
            Dictionary<string, VISUAL_SETTING> dictRes;

            idListener = DbSources.Sources().Register(connSett, false, @"MAIN_DB");

            dictRes = GetParameterVisualSettings(idListener, fields, out err);

            DbSources.Sources().UnRegister(idListener);

            return dictRes;
        }
        /// <summary>
        /// Получить таблицу с установками для отображения значений
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД</param>>
        /// <param name="fields">Значения для подстановки в предложение 'where' при выборке записей из профиля группы пользователя (пользователя)</param>
        /// <param name="err">Результат выполнения функции</param>
        /// <returns>Таблица с установками для отображения значений</returns>
        public static Dictionary<string, VISUAL_SETTING> GetParameterVisualSettings(int idListener, int[] fields, out int err)
        {
            err = -1; //Обшая ошибка
            DbConnection dbConn = null;
            Dictionary<string, VISUAL_SETTING> dictRes = new Dictionary<string, VISUAL_SETTING>();

            dbConn = DbSources.Sources().GetConnection(idListener, out err);

            if (err == 0)
                dictRes = GetParameterVisualSettings(ref dbConn, fields, out err);
            else
                ; // не удалось получить объект соединения с БД по идентификатору

            return dictRes;
        }
        /// <summary>
        /// Получить таблицу с установками для отображения значений
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="fields">Значения для подстановки в предложение 'where' при выборке записей из профиля группы пользователя (пользователя)</param>
        /// <param name="err">Результат выполнения функции</param>
        /// <returns>Таблица с установками для отображения значений</returns>
        public static Dictionary<string, VISUAL_SETTING> GetParameterVisualSettings(ref DbConnection dbConn, int[] fields, out int err)
        {
            err = -1; //Обшая ошибка
            string strQuery = string.Empty;
            int id_alg = -1 // идентификатор параметра в алгоритме расчета
                , id_unit = -1 // идентификатор параметра настроек при отображении значения [profiles_unit]
                , ratio = -1 // коэффициент
                , round = -1 // кол-во знаков при округлении
                , checkSum = (int)ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND
                    + (int)ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO
                , curSum = -1;
            string n_alg = string.Empty;
            Dictionary<string, VISUAL_SETTING> dictRes = new Dictionary<string, VISUAL_SETTING>();
            DataTable tblRes = new DataTable()
                //, tblRatio
                ;
            DataRow[] rowsAlg = null;

            if (fields.Length == (int)INDEX_VISUALSETTINGS_PARAMS.COUNT)
            {
                strQuery = @"SELECT ID_CONTEXT as [ID], [ID_UNIT], [IS_ROLE], [VALUE]"
                            + @" FROM [dbo].[profiles]"
                            + @" WHERE"
                                + @" ID_UNIT IN ("
                                    + (int)ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND + @","
                                    + (int)ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO
                                    + @")"
                                + @" AND ((ID_EXT=" + Id + @" AND " + @"IS_ROLE=0)"
                                    + @" OR (ID_EXT=" + Role + @" AND " + @"IS_ROLE=1))"
                                //+ @" AND ID_TASK=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.TASK]
                                //+ @" AND ID_PLUGIN=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.PLUGIN]
                                + @" AND ID_TAB=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.TAB]
                                + @" AND ID_ITEM=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.ITEM]
                                //+ @" AND ID_CONTEXT=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.CONTEXT]
                            + @" ORDER BY [ID_UNIT], [ID]"
                                ;

                tblRes = DbTSQLInterface.Select(ref dbConn, strQuery, null, null, out err);
                //tblRatio = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM [ratio]", null, null, out err);

                try
                {
                    foreach (DataRow r in tblRes.Rows)
                    {
                        id_alg = (Int16)r[@"ID"];
                        n_alg = r[@"N_ALG"].ToString().Trim();
                        ratio = s_iRatioDefault;
                        round = s_iRoundDefault;

                        if (dictRes.ContainsKey(n_alg) == false)
                        {
                            rowsAlg = tblRes.Select(@"ID=" + id_alg, @"ID_UNIT, IS_ROLE"); // приоритет значений для [IS_ROLE] = 0                            

                            curSum = 0;
                            foreach (DataRow rAlg in rowsAlg)
                            {
                                id_unit = (Int16)rAlg[@"ID_UNIT"];

                                switch ((ID_ALLOWED)id_unit)
                                {
                                    case ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO:
                                        ratio = int.Parse(((string)rAlg[@"VALUE"]).Trim());
                                        //ratio = Math.Pow(10F, (int)tblRatio.Select(@"ID=" + (int)rAlg[@"VALUE"])[0][@"VALUE"]);
                                        break;
                                    case ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND:
                                        round = int.Parse(((string)rAlg[@"VALUE"]).Trim());
                                        break;
                                    default:
                                        break;
                                }

                                curSum += id_unit;

                                if (curSum == checkSum)
                                    break;
                                else
                                    ;
                            }

                            dictRes.Add(n_alg, new VISUAL_SETTING() { m_ratio = ratio, m_round = round });
                        }
                        else
                            ; // continue, этот параметр уже обработан
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"HTepUsers::GetParameterVisualSettings () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                ; // невозможно сформировать запрос - недостаточно параметров

            return dictRes;
        }
        
        protected class HTepProfiles : HProfiles
        {
            public HTepProfiles(int idListener, int id_role, int id_user)
                : base(idListener, id_role, id_user)
            {
            }
            
            /// <summary>
            /// Функция получения доступа
            /// </summary>
            /// <param name="id_tab">ID вкладки</param>
            /// <returns></returns>
            public static object GetAllowed(int id_tab)
            {
                object objRes = false;
                bool bValidate = false;
                int indxRowAllowed = -1;
                Int16 val = -1
                   , type = -1;
                string strVal = string.Empty;

                DataRow[] rowsAllowed = m_tblValues.Select(@"ID_TAB=" + id_tab);
                switch (rowsAllowed.Length)
                {
                    case 1:
                        indxRowAllowed = 0;
                        break;
                    default :
                        //В табл. с настройками возможность 'id' определена как для "роли", так и для "пользователя"
                        // требуется выбрать строку с 'IS_ROLE' == 0 (пользователя)
                        // ...
                        foreach (DataRow r in rowsAllowed)
                        {
                            indxRowAllowed++;
                            if (Int16.Parse(r[@"IS_ROLE"].ToString()) == 0)
                                break;
                            else
                                ;
                        }
                        break;
                    //default: //Ошибка - исключение
                    //    throw new Exception(@"HUsers.HProfiles::GetAllowed (id=" + id_tab + @") - не найдено ни одной записи...");
                }

                // проверка не нужна, т.к. вызывается исключение
                //if ((!(indxRowAllowed < 0))
                //    && (indxRowAllowed < rowsAllowed.Length))
                //{
                strVal = rowsAllowed[indxRowAllowed][@"VALUE"].ToString().Trim();
                objRes = m_tblValues.Clone();

                foreach (DataRow r in rowsAllowed)
                {
                    (objRes as DataTable).Rows.Add(r.ItemArray);
                }

                return objRes;
            }

            /// <summary>
            /// Функция получения доступа
            /// </summary>
            /// <returns></returns>
            public static object GetAllowed()
            {
                object objRes = false;
                bool bValidate = false;
                int indxRowAllowed = -1;
                Int16 val = -1
                   , type = -1;
                string strVal = string.Empty;

                DataRow[] rowsAllowed = m_tblValues.Select();
                switch (rowsAllowed.Length)
                {
                    case 1:
                        indxRowAllowed = 0;
                        break;
                    case 2:
                        //В табл. с настройками возможность 'id' определена как для "роли", так и для "пользователя"
                        // требуется выбрать строку с 'IS_ROLE' == 0 (пользователя)
                        // ...
                        foreach (DataRow r in rowsAllowed)
                        {
                            indxRowAllowed++;
                            if (Int16.Parse(r[@"IS_ROLE"].ToString()) == 0)
                                break;
                            else
                                ;
                        }
                        break;
                    default: //Ошибка - исключение
                        throw new Exception(@"HUsers.HProfiles::GetAllowed () - не найдено ни одной записи...");
                }

                // проверка не нужна, т.к. вызывается исключение
                //if ((!(indxRowAllowed < 0))
                //    && (indxRowAllowed < rowsAllowed.Length))
                //{
                strVal = rowsAllowed[indxRowAllowed][@"VALUE"].ToString().Trim();

                return objRes;
            }
        }

        /// <summary>
        /// Метод для получения Profile для вкладки
        /// </summary>
        /// <param name="id_tab"></param>
        /// <returns></returns>
        public static DataTable GetProfileUser_Tab(int id_tab)
        {
            DataTable m_dt_profileUser = new DataTable();

            m_dt_profileUser = (DataTable)HTepProfiles.GetAllowed(id_tab);
            

            return m_dt_profileUser;
        }

        /// <summary>
        /// Метод для получения Profile пользователя
        /// </summary>
        /// <param name="id_tab"></param>
        /// <returns></returns>
        public static DataTable GetProfileUser_Tab()
        {
            DataTable m_dt_profileUser = new DataTable();

            m_dt_profileUser = (DataTable)HTepProfiles.GetAllowed();

            return m_dt_profileUser;
        }

        /// <summary>
        /// Функция получения строки запроса пользователя
        ///  /// <returns>Строка строку запроса</returns>
        /// </summary>
        private static string getUsersRequest(string where, string orderby)
        {
            string strQuery = string.Empty;
            //strQuer//strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='" + Environment.UserDomainName + "\\" + Environment.UserName + "'";
            //strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='NE\\ChrjapinAN'";
            strQuery = "SELECT * FROM users";
            if ((!(where == null)) && (where.Length > 0))
                strQuery += " WHERE " + where;
            else
                ;

            if ((!(orderby == null)) && (orderby.Length > 0))
                strQuery += " ORDER BY " + orderby;
            else
                ;

            return strQuery;
        }

        /// <summary>
        /// Функция запроса для поиска пользователя
        /// </summary>
        public static void GetUsers(ref DbConnection conn, string where, string orderby, out DataTable users, out int err)
        {
            err = 0;
            users = null;

            if (!(conn == null))
            {
                users = new DataTable();
                Logging.Logg().Debug(@"HUsers::GetUsers () - запрос для поиска пользователей = [" + getUsersRequest(where, orderby) + @"]", Logging.INDEX_MESSAGE.NOT_SET);
                users = DbTSQLInterface.Select(ref conn, getUsersRequest(where, orderby), null, null, out err);
            }
            else
            {
                err = -1;
            }
        }

        /// <summary>
        /// Функция взятия ролей из БД
        /// </summary>
        public static void GetRoles(ref DbConnection conn, string where, string orderby, out DataTable roles, out int err)
        {
            err = 0;
            roles = null;
            string query = string.Empty;

            if (!(conn == null))
            {
                roles = new DataTable();
                query = @"SELECT * FROM ROLES_UNIT";

                if ((where.Equals(null) == true) || (where.Equals(string.Empty) == true))
                    query += @" WHERE ID < 500";
                else
                    query += @" WHERE " + where;

                roles = DbTSQLInterface.Select(ref conn, query, null, null, out err);
            }
            else
            {
                err = -1;
            }
        }

    }
}
