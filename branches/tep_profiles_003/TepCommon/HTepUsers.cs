using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Xml;

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
            
            , QUERY_TIMEZONE = 101 //Идентификатор часового пояса при запросе значений
            , ADRESS_MAIL_AUTOBOOK //Адрес_эп_Активной_ээ 
            , PERIOD_IND //Период_идентификатор  
             

            , VISUAL_SETTING_VALUE_ROUND = 201 //Отображение значений, количество знаков после запятой
            , VISUAL_SETTING_VALUE_RATIO //Отображение значений, множитель относительно базовой единицы измерения
            , INPUT_PARAM //Входные параметры
            , EDIT_COLUMN //Редактирование_столбца 
        };
        /// <summary>
        /// Перечисление - индексы в массиве - аргументе функции 'GetParameterVisualSettings'
        /// </summary>
        public enum INDEX_VISUALSETTINGS_PARAMS
        { /*TASK, PLUGIN, */
            TAB,
            ITEM
                , COUNT
        }
        /// <summary>
        /// Конструктор основной - с  параметром
        /// </summary>
        /// <param name="iListenerId">Идентификатор установленного соединения с БД</param>
        public HTepUsers(int iListenerId)
            : base(iListenerId)
        {
            int err = -1;
            m_tblValues = new DataTable();
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
            List<int> listIdParsedFPanel = new List<int>();

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
                strRes = GetIdIsUseFPanels(ref dbConn, out iRes);
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

        public static string GetAllowed(int id, ConnectionSettings connSett)
        {
            string strRes = string.Empty;

            //HTepProfilesXml.UpdateProfile(connSett);
            
                DictElement dictElement = HTepProfilesXml.GetProfileUser(Id, Role);

            Dictionary<string, string> dictAttr = dictElement.Attributes;

            if (dictAttr.ContainsKey(id.ToString()) == true)
            {
                strRes = dictAttr[id.ToString()];
            }

            return strRes;
        }
        public static void SetAllowed(int id, string val, ConnectionSettings connSett)
        {
            //HTepProfilesXml.UpdateProfile(connSett);
            HTepProfilesXml.EditAttr(val, id, connSett);
        }
        public static bool IsAllowed(int id, ConnectionSettings connSett)
        {
            return bool.Parse(GetAllowed(id, connSett));
        }

        public static DataTable GetTableProfileUnits { get { return HTepProfilesXml.GetTableUnits; } }


        public struct VISUAL_SETTING
        {
            public int m_ratio;
            public int m_round;
        }

        /// <summary>
        /// Структура со значением и типом значения
        /// </summary>
        public struct UNIT_VALUE
        {
            public object m_value;
            public int m_idType;
        }

        static DataTable m_tblValues;
        static bool m_bIsRole;


        public static int s_iRatioDefault = 0
            , s_iRoundDefault = 2;
        /// <summary>
        /// Получить таблицу с установками для отображения значений
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД</param>
        /// <param name="fields">Значения для подстановки в предложение 'where' при выборке записей из профиля группы пользователя (пользователя)</param>
        /// <param name="err">Результат выполнения функции</param>
        /// <returns>Таблица с установками для отображения значений</returns>
        //public static Dictionary<string, VISUAL_SETTING> GetParameterVisualSettings(ConnectionSettings connSett, int[] fields, out int err)
        //{
        //    err = -1; //Обшая ошибка
        //    int idListener = -1;
        //    Dictionary<string, VISUAL_SETTING> dictRes;

        //    idListener = DbSources.Sources().Register(connSett, false, @"MAIN_DB");

        //    dictRes = GetParameterVisualSettings(idListener, fields, out err);

        //    DbSources.Sources().UnRegister(idListener);

        //    return dictRes;
        //}
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
            int id_unit = -1 // идентификатор параметра настроек при отображении значения [profiles_unit]
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
                strQuery = @"SELECT CONTEXT as [ID], [ID_UNIT], [IS_ROLE], [VALUE]"
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
                            //+ @" AND CONTEXT=" + fields[(int)INDEX_VISUALSETTINGS_PARAMS.CONTEXT]
                            + @" ORDER BY [ID_UNIT], [ID]"
                                ;

                tblRes = DbTSQLInterface.Select(ref dbConn, strQuery, null, null, out err);
                //tblRatio = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM [ratio]", null, null, out err);

                try
                {
                    foreach (DataRow r in tblRes.Rows)
                    {
                        n_alg = r[@"ID"].ToString();
                        ratio = s_iRatioDefault;
                        round = s_iRoundDefault;

                        if (dictRes.ContainsKey(n_alg.Trim()) == false)
                        {
                            rowsAlg = tblRes.Select(@"ID='" + n_alg + "'", @"ID_UNIT, IS_ROLE"); // приоритет значений для [IS_ROLE] = 0                            

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

                            dictRes.Add(n_alg.Trim(), new VISUAL_SETTING() { m_ratio = ratio, m_round = round });
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

        public static Dictionary<string, VISUAL_SETTING> GetParameterVisualSettings(ConnectionSettings connSett, int[] fields, out int err)
        {
            err = -1; //Обшая ошибка
            string strQuery = string.Empty;
            int id_unit = -1 // идентификатор параметра настроек при отображении значения [profiles_unit]
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

            //HTepProfilesXml.UpdateProfile(connSett);

            if (fields.Length == (int)INDEX_VISUALSETTINGS_PARAMS.COUNT)
            {
                DictElement dictElement = HTepProfilesXml.GetProfileUserPanel(Id, Role, fields[(int)INDEX_VISUALSETTINGS_PARAMS.TAB]);

                Dictionary<string, DictElement> dictProfile = new Dictionary<string, DictElement>();

                if (dictElement.Objects.ContainsKey(fields[(int)INDEX_VISUALSETTINGS_PARAMS.ITEM].ToString()) == true)
                {
                    dictProfile = dictElement.Objects[fields[(int)INDEX_VISUALSETTINGS_PARAMS.ITEM].ToString()].Objects;


                    foreach (string rAlg in dictProfile.Keys)
                    {
                        foreach (string idUnit in dictProfile[rAlg].Attributes.Keys)
                        {
                            ratio = 0;
                            round = 0;
                            switch ((ID_ALLOWED)Int16.Parse(idUnit))
                            {
                                case ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO:
                                    ratio = int.Parse(dictProfile[rAlg].Attributes[idUnit]);
                                    break;
                                case ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND:
                                    round = int.Parse(dictProfile[rAlg].Attributes[idUnit]);
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

                        dictRes.Add(rAlg.Trim(), new VISUAL_SETTING() { m_ratio = ratio, m_round = round });
                    }

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
                    default:
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
        new public static void GetUsers(ref DbConnection conn, string where, string orderby, out DataTable users, out int err)
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
        new public static void GetRoles(ref DbConnection conn, string where, string orderby, out DataTable roles, out int err)
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

        /// <summary>
        /// Получение словаря с Profile для ролей
        /// </summary>
        public static Dictionary<string, DictElement> GetDicpRolesProfile
        {
            get
            {
                Dictionary<string, DictElement> dictRoles = HTepProfilesXml.DictRoles;
                return dictRoles;
            }
        }

        /// <summary>
        /// Получение словаря с Profile для пользователей
        /// </summary>
        public static Dictionary<string, DictElement> GetDicpUsersProfile
        {
            get
            {
                Dictionary<string, DictElement> dictUsers = HTepProfilesXml.DictUsers;
                return dictUsers;
            }
        }

        /// <summary>
        /// Получение словаря с XML для ролей
        /// </summary>
        public static Dictionary<string, XmlDocument> GetDicpXmlRoles
        {
            get
            {
                Dictionary<string, XmlDocument> xmlRoles = HTepProfilesXml.XmlRoles;
                return xmlRoles;
            }
        }

        /// <summary>
        /// Получение словаря с XML для пользователей
        /// </summary>
        public static Dictionary<string, XmlDocument> GetDicpXmlUsers
        {
            get
            {
                Dictionary<string, XmlDocument> xmlUsers = HTepProfilesXml.XmlUsers;
                return xmlUsers;
            }
        }

        /// <summary>
        /// Метод для получения словаря со значениями прав доступа
        /// </summary>
        /// <param name="iListenerId">Идентификатор для подключения к БД</param>
        /// <param name="id_role">ИД роли</param>
        /// <param name="id_user">ИД пользователя</param>
        /// <param name="bIsRole">Пользователь или роль</param>
        /// <returns>Словарь со значениями</returns>
        public static Dictionary<int, UNIT_VALUE> GetDictProfileItem(DbConnection dbConn, int id_role, int id_user, bool bIsRole, DataTable allProfiles)
        {
            Dictionary<int, UNIT_VALUE> dictPrifileItem = null;

            dictPrifileItem = HTepProfilesXml.GetProfileItem;

            return dictPrifileItem;
        }


        public class HTepProfilesXml
        {
            public static string m_nameTableProfilesUnit = @"profiles_unit";

            protected static DataTable m_tblValues;
            protected static DataTable m_tblTypes;
            static XmlDocument xml_tree;

            public static Dictionary<string, DictElement> DictRoles,
                DictUsers;

            public static Dictionary<string, XmlDocument> XmlRoles,
                XmlUsers;

            static DataTable dtProfiles_Orig, dtProfiles_Edit;

            public enum Type : int { User, Role, Count };
            public enum Component : int { None, Panel, Item, Context, Count };

            #region GetDict

            /// <summary>
            /// Получить таблицу из БД
            /// </summary>
            /// <param name="connSet">Connection Settings</param>
            /// <returns>Таблица с XML</returns>
            private static DataTable getDataTable(ConnectionSettings connSet)
            {
                DataTable dt;
                int err = 0;
                int idListener = DbSources.Sources().Register(connSet, false, "TEP_NTEC_5");
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);
                string query = "SELECT * FROM [TEP_NTEC_5].[dbo].[profiles_new]";
                dt = new DataTable();
                dt = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
                DbSources.Sources().UnRegister(idListener);
                return dt;
            }

            /// <summary>
            /// Получение словаря с профайлом для ролей
            /// </summary>
            /// <param name="connSet">Connection Settings</param>
            private static void getProfileAllRoles()
            {
                string query = "IS_ROLE=1";
                DataRow[] dtUnic = dtProfiles_Orig.Select(query);

                DictRoles = new Dictionary<string, DictElement>();
                XmlRoles = new Dictionary<string, XmlDocument>();

                for (int i = 0; i < dtUnic.Length; i++)
                {
                    Dictionary<string, DictElement> dict = new Dictionary<string, DictElement>();
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(dtUnic[i]["XML"].ToString());
                    dict = getDictFromXml(xml["Role"]);

                    DictRoles.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), dict[dtUnic[i]["ID_EXT"].ToString().Trim()]);

                    XmlRoles.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), xml);
                }
            }

            /// <summary>
            /// Получение словаря с профайлом для пользователей
            /// </summary>
            /// <param name="connSet">Connection Settings</param>
            private static void getProfileAllUsers()
            {
                string query = "IS_ROLE=0";
                DataRow[] dtUnic = dtProfiles_Orig.Select(query);

                DictUsers = new Dictionary<string, DictElement>();
                XmlUsers = new Dictionary<string, XmlDocument>();

                for (int i = 0; i < dtUnic.Length; i++)
                {
                    Dictionary<string, DictElement> dict = new Dictionary<string, DictElement>();
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(dtUnic[i]["XML"].ToString());
                    dict = getDictFromXml(xml["User"]);
                    DictUsers.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), dict[dtUnic[i]["ID_EXT"].ToString().Trim()]);
                    XmlUsers.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), xml);
                }
            }

            /// <summary>
            /// Метод для получения словаря из XML
            /// </summary>
            /// <param name="node">Node для разбора</param>
            /// <returns>Словарь полученный из XML</returns>
            private static Dictionary<string, DictElement> getDictFromXml(XmlNode node)
            {
                Dictionary<string, DictElement> dict = new Dictionary<string, DictElement>();

                Dictionary<string, DictElement> dictChild = new Dictionary<string, DictElement>();

                if (node.ChildNodes.Count != 0)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        DictElement dictElemChild = new DictElement();
                        Dictionary<string, string> dictAttrChild = new Dictionary<string, string>();

                        dictElemChild.Attributes = dictAttrChild;
                        dictElemChild.Objects = new Dictionary<string, DictElement>();

                        foreach (XmlAttribute attr in child.Attributes)
                        {
                            dictAttrChild.Add(attr.Name.Replace('_', ' ').Trim(), attr.Value);
                        }

                        dictElemChild.Attributes = dictAttrChild;

                        if (child.ChildNodes.Count != 0)
                        {
                            dictElemChild.Objects = getDictFromXml(child);
                        }

                        dict.Add(child.LocalName.Replace('_', ' ').Trim(), dictElemChild);
                    }
                }

                return dict;
            }

            /// <summary>
            /// Получить профайл для конкретного пользователя
            /// </summary>
            /// <param name="id_user">ИД пользователя</param>
            /// <param name="id_role">ИД роли</param>
            /// <returns></returns>
            public static DictElement GetProfileUser(int id_user, int id_role)
            {
                DictElement profileUser = getDictElement(DictRoles[id_role.ToString()], DictUsers[id_user.ToString()]);

                return profileUser;
            }

            /// <summary>
            /// Получить профайл для панели конкретного пользователя
            /// </summary>
            /// <param name="id_user">ИД пользователя</param>
            /// <param name="id_role">ИД роли</param>
            /// <param name="id_panel">ИД панели (вкладки)</param>
            /// <returns></returns>
            public static DictElement GetProfileUserPanel(int id_user, int id_role, int id_panel)
            {
                DictElement profileUser = getDictElement(DictRoles[id_role.ToString()], DictUsers[id_user.ToString()]);

                DictElement profilePanel = new DictElement();

                if (profileUser.Objects.ContainsKey(id_panel.ToString()) == true)
                {
                    profilePanel = profileUser.Objects[id_panel.ToString()];
                }

                    return profilePanel;
            }

            /// <summary>
            /// Метод для объединения профайла пользователя и роли
            /// </summary>
            /// <param name="dictObject_Role">Структура с данными роли</param>
            /// <param name="dictObject_User">Структура с данными пользователя</param>
            /// <returns>Структура с объединенными данными</returns>
            private static DictElement getDictElement(DictElement dictObject_Role, DictElement dictObject_User)
            {
                DictElement profileUser = new DictElement();
                profileUser.Attributes = new Dictionary<string, string>();
                profileUser.Objects = new Dictionary<string, DictElement>();

                DictElement dict_role = dictObject_Role;
                DictElement dict_user = dictObject_User;

                foreach (string attr in dict_role.Attributes.Keys)
                {
                    if (dict_user.Attributes.ContainsKey(attr) == false)
                    {
                        profileUser.Attributes.Add(attr, dict_role.Attributes[attr]);
                    }
                    else
                        profileUser.Attributes.Add(attr, dict_user.Attributes[attr]);
                }

                foreach (string attr in dict_role.Objects.Keys)
                {
                    if (dict_user.Objects.ContainsKey(attr) == false)
                    {
                        profileUser.Objects.Add(attr, dict_role.Objects[attr]);
                    }
                    else
                        profileUser.Objects.Add(attr, getDictElement(dict_role.Objects[attr], dict_user.Objects[attr]));
                }

                return profileUser;
            }
            
            #endregion

            public HTepProfilesXml()
            {
            }
            
            /// <summary>
            /// Обновление данных 
            /// </summary>
            /// <param name="connSet"></param>
            public static void UpdateProfile(ConnectionSettings connSet)
            {
                dtProfiles_Orig = new DataTable();
                xml_tree = new XmlDocument();
                XmlRoles = new Dictionary<string, XmlDocument>();
                XmlUsers = new Dictionary<string, XmlDocument>();

                dtProfiles_Orig = getDataTable(connSet);
                GetTableProfileUnits(connSet);
                dtProfiles_Edit = dtProfiles_Orig.Copy();
                getProfileAllRoles();
                getProfileAllUsers();
            }

            #region EditXML

            public static void EditAttr(string value, int id_unit, ConnectionSettings connSet)
            {
                ParamComponent parComp = new ParamComponent();

                parComp.ID_Unit = id_unit;
                parComp.Value = value;
                XmlDocument doc = new XmlDocument();

                try
                {
                    doc = EditAttr(XmlUsers[Id.ToString()], Id, Type.User, Component.None, parComp);
                }
                catch(Exception e)
                {

                }
                saveXml(connSet, doc, Id, Type.User);
            }

            public static void AddActivePanel(int idPanel, ConnectionSettings connSet)
            {
                ParamComponent parComp = new ParamComponent();
                string value = DictUsers[Id.ToString()].Attributes["3"];

                parComp.ID_Unit = 3;
                parComp.Value = value + ',' + idPanel.ToString();

                XmlDocument doc = EditAttr(XmlUsers[Id.ToString()], Id, Type.User, Component.None, parComp);

                saveXml(connSet, doc, Id, Type.User);
            }

            public static void DelActivePanel(int idPanel, ConnectionSettings connSet)
            {
                ParamComponent parComp = new ParamComponent();
                string value = string.Empty;
                string[] panels;
                value = DictUsers[Id.ToString()].Attributes["3"];
                panels = value.Split(',');
                string[] add_pan = new string[panels.Length - 1];

                for (int i = 0; i < panels.Length; i++)
                    if (panels[i] != idPanel.ToString())
                        add_pan[i] = panels[i];

                parComp.ID_Unit = 3;
                parComp.Value = string.Join(",", add_pan);
                XmlDocument doc = EditAttr(XmlUsers[Id.ToString()], Id, Type.User, Component.None, parComp);

                saveXml(connSet, doc, Id, Type.User);
            }

            public static XmlDocument EditAttr(XmlDocument doc, int id, Type type, Component comp, ParamComponent parComp)
            {
                if (doc != null)
                {
                    try
                    {
                        switch (comp)
                        {
                            case Component.Context:
                                doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()]
                                    ["_" + parComp.Context]
                                    .Attributes["_" + parComp.ID_Unit.ToString()].Value = parComp.Value;
                                break;
                            case Component.Item:
                                doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()]
                                    .Attributes["_" + parComp.ID_Unit.ToString()].Value = parComp.Value;

                                break;
                            case Component.Panel:
                                doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    .Attributes["_" + parComp.ID_Unit.ToString()].Value = parComp.Value;

                                break;

                            case Component.None:
                                doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    .Attributes["_" + parComp.ID_Unit.ToString()].Value = parComp.Value;
                                break;
                        }
                    }
                    catch
                    {
                        
                    }
                }
                return doc;
                
            }

            public static XmlDocument AddElement(XmlDocument doc, int id, Type type, Component comp, ParamComponent parComp)
            {
                XmlElement newElement;
                if (doc != null)
                    switch (comp)
                    {
                        case Component.Context:
                            newElement = doc.CreateElement("_"+parComp.Context);

                            doc[type.ToString()]
                                ["_" + id.ToString()]
                                ["_" + parComp.ID_Panel.ToString()]
                                ["_" + parComp.ID_Item.ToString()].AppendChild(newElement);
                            break;
                        case Component.Item:
                            newElement = doc.CreateElement("_" + parComp.ID_Item.ToString());
                            doc[type.ToString()]
                                ["_" + id.ToString()]
                                ["_" + parComp.ID_Panel.ToString()].AppendChild(newElement);

                            break;
                        case Component.Panel:
                            newElement = doc.CreateElement("_" + parComp.ID_Panel.ToString());
                            doc[type.ToString()]
                                ["_" + id.ToString()].AppendChild(newElement);
                            break;

                        case Component.None:
                            newElement = doc.CreateElement("_" + id.ToString());
                            doc[type.ToString()].AppendChild(newElement);
                            break;
                    }

                return doc;
            }

            private static void saveXml(ConnectionSettings connSet, XmlDocument doc, int id, Type type)
            {
                dtProfiles_Edit = new DataTable();
                dtProfiles_Edit = dtProfiles_Orig.Copy();
                foreach (DataRow row in dtProfiles_Edit.Rows)
                {
                    if (row["ID_EXT"].ToString().Trim() == id.ToString() & row["IS_ROLE"].ToString().Trim() == ((int)type).ToString())
                    {
                        row["XML"] = doc.InnerXml;
                    }
                }
                
                int err = 0;
                int idListener = DbSources.Sources().Register(connSet, false, "TEP_NTEC_5");
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn,"profiles_new", "ID_EXT, IS_ROLE", string.Empty,dtProfiles_Orig,dtProfiles_Edit, out err);
                
                DbSources.Sources().UnRegister(idListener);

                UpdateProfile(connSet);
            }

            public static void SaveXml(ConnectionSettings connSet, Dictionary<string, XmlDocument>[] arrDictOrig, Dictionary<string, XmlDocument>[] arrDictEdit)
            {
                int err = 0;
                int idListener = DbSources.Sources().Register(connSet, false, "TEP_NTEC_5");
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                DataTable dtProfiles_Orig, dtProfiles_Edit;

                dtProfiles_Orig = new DataTable();
                dtProfiles_Edit = new DataTable();

                dtProfiles_Orig = getDTfromDict(arrDictOrig);
                dtProfiles_Edit = getDTfromDict(arrDictEdit);

                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, "profiles_new", "ID_EXT, IS_ROLE", string.Empty, dtProfiles_Orig, dtProfiles_Edit, out err);

                DbSources.Sources().UnRegister(idListener);

                UpdateProfile(connSet);
            }

            private static DataTable getDTfromDict(Dictionary<string, XmlDocument>[] arrDict)
            {
                DataTable dt = new DataTable();
                dt = dtProfiles_Orig.Clone();


                for (int i=0;i<(int)Type.Count; i++)
                {
                    foreach(string key in arrDict[i].Keys)
                    {
                        dt.Rows.Add(new object[] {key, i, arrDict[i][key].InnerXml });
                    }
                }


                return dt;
            }

            /// <summary>
            /// Метод для получения словаря с параметрами Profil'а для пользователя
            /// </summary>
            /// <param name="id_ext">ИД пользователя</param>
            /// <param name="bIsRole">Флаг для определения роли</param>
            /// <returns>Словарь с параметрами</returns>
            public static Dictionary<int, UNIT_VALUE> GetProfileItem
            {
                get
                {
                    int id_unit = -1;
                    DataRow[] unitRows = new DataRow[1]; ;

                    Dictionary<int, UNIT_VALUE> dictRes = new Dictionary<int, UNIT_VALUE>();

                    foreach (DataRow r in HTepUsers.GetTableProfileUnits.Rows)
                    {
                        id_unit = (int)r[@"ID"];

                        if (id_unit < 4)
                        {
                            unitRows[0] = GetRowAllowed(id_unit);

                            if (unitRows.Length == 1)
                            {
                                dictRes.Add(id_unit, new UNIT_VALUE() { m_value = unitRows[0][@"VALUE"].ToString().Trim(), m_idType = Convert.ToInt32(unitRows[0][@"ID_UNIT"]) });
                            }
                            else
                                Logging.Logg().Warning(@"", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }

                    return dictRes;
                }
            }
            
            /// <summary>
            /// Метод получения строки со значениями прав доступа
            /// </summary>
            /// <param name="id">ИД типа</param>
            /// <param name="bIsRole"></param>
            /// <returns></returns>
            private static DataRow GetRowAllowed(int id)
            {
                DataRow objRes = null;

                DataRow[] rowsAllowed = m_tblValues.Select("ID_UNIT='" + id + "' and ID_TAB=0");

                switch (rowsAllowed.Length)
                {
                    case 1:
                        objRes = rowsAllowed[0];
                        break;
                    case 2:
                        //В табл. с настройками возможность 'id' определена как для "роли", так и для "пользователя"
                        // требуется выбрать строку с 'IS_ROLE' == 0 (пользователя)
                        // ...
                        foreach (DataRow r in rowsAllowed)
                            if (Int16.Parse(r[@"IS_ROLE"].ToString()) == Convert.ToInt32(m_bIsRole))
                            {
                                objRes = r;
                                break;
                            }
                            else
                                ;
                        break;
                    default: //Ошибка - исключение
                        throw new Exception(@"HUsers.HProfiles::GetAllowed (id=" + id + @") - не найдено ни одной записи...");
                }

                return objRes;
            }

            /// <summary>
            /// Структура с параметрами изменяемого(выбранного) элемента
            /// </summary>
            public struct ParamComponent
            {
                /// <summary>
                /// ИД панели(вкладки)
                /// </summary>
                public int ID_Panel;

                /// <summary>
                /// ИД объекта на вкладке
                /// </summary>
                public int ID_Item;
                
                /// <summary>
                /// ИД параметра
                /// </summary>
                public int ID_Unit;

                /// <summary>
                /// Имя объекта вложенного в объект
                /// </summary>
                public string Context;
                
                /// <summary>
                /// Значение параметра ID_Unit
                /// </summary>
                public string Value;
            }
            #endregion

            /// <summary>
            /// Получение XML для объекта
            /// </summary>
            /// <param name="Type_Xml">Тип объекта</param>
            /// <param name="Id">ИД</param>
            /// <returns>XML документ</returns>
            public XmlDocument GetXml(Type Type_Xml, int Id)
            {
                XmlDocument doc = new XmlDocument();

                switch (Type_Xml)
                {
                    case Type.Role:
                        doc = XmlRoles[Id.ToString()];
                        break;
                    case Type.User:
                        doc = XmlUsers[Id.ToString()];
                        break;
                }

                return doc;
            }

            /// <summary>
            /// Метод для получения таблицы со списком параметров
            /// </summary>
            /// <param name="connSet">ConnectionSettings</param>
            private static void GetTableProfileUnits(ConnectionSettings connSet)
            {
                int err = -1;
                string query = string.Empty
                    , errMsg = string.Empty;

                int idListener = DbSources.Sources().Register(connSet, false, "TEP_NTEC_5");
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                if (!(err == 0))
                    errMsg = @"нет соединения с БД";
                else
                {
                    query = @"SELECT * from " + m_nameTableProfilesUnit;
                    m_tblTypes = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);

                    if (!(err == 0))
                        errMsg = @"Ошибка при чтении ТИПов ДАНных настроек для группы(роли) (irole = " + Role + @"), пользователя (iuser=" + Id + @")";
                    else
                        ;
                }
            }

            /// <summary>
            /// Список параметров
            /// </summary>
            public static DataTable GetTableUnits { get { return m_tblTypes; } }
        }

        /// <summary>
        /// Структура содержащая в себе атрибуты выбранного объекта и словарь с вложенными в него элементами
        /// </summary>
        public struct DictElement
        {
            /// <summary>
            /// Словарь с вложенными объектами
            /// </summary>
            public Dictionary<string, DictElement> Objects;

            /// <summary>
            /// Словарь с атрибутами объекта
            /// </summary>
            public Dictionary<string, string> Attributes;
        }
    }
}
