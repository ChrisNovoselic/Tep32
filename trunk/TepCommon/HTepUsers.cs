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
        }
        /// <summary>
        /// Роль пользователя (из БД конфигурации)
        /// </summary>
        public static int Role
        {
            get { return (int)(s_DataRegistration[(int)INDEX_REGISTRATION.ROLE] == null ? -1 : s_DataRegistration[(int)INDEX_REGISTRATION.ROLE]); }
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
        /// <summary>
        /// Метод возвращающий список вкладок открытых пользователем в предыдущей сессии
        /// </summary>
        /// <param name="id">Идентификатор параметра открытых вкладок</param>
        /// <param name="connSett">Параменты подключения к БД</param>
        /// <returns>Список вкладок в виде строки</returns>
        public static string GetAllowed(int context, ConnectionSettings connSett)
        {
            HTepProfilesXml.UpdateProfile(connSett);

            DictionaryProfileItem dictProfileItem = HTepProfilesXml.GetProfileUser(Id, Role);

            return dictProfileItem.GetAttribute(context);
        }

        public static string GetAllowed(int context, int id = -1, int role = -1)
        {
            string strRes = string.Empty;

            DictionaryProfileItem dictProfileItem = null;

            if (id < 0)
                id = Id;
            else
                ;

            if (role < 0)
                role = Role;
            else
                ;

            dictProfileItem = HTepProfilesXml.GetProfileUser(id, role);

            if (!(dictProfileItem == null))
                strRes = dictProfileItem.GetAttribute(context);
            else
                ;

            return strRes;
        }

        public static string GetAllowed(ID_ALLOWED context, int id = -1, int role = -1)
        {
            return GetAllowed((int)context, id, role);
        }
        /// <summary>
        /// Добавление открытой вкладки
        /// </summary>
        /// <param name="id">ИД параметра содержащего открытые вкладки</param>
        /// <param name="val">ИД вкладки</param>
        /// <param name="connSett">Параметры подключения к БД</param>
        public static void SetAllowed(int id, string val, ConnectionSettings connSett)
        {
            //HTepProfilesXml.UpdateProfile(connSett);
            HTepProfilesXml.Edit(id, val, connSett);
        }
        /// <summary>
        /// Метод указывающий содержатся ли вкладки в списке открытых в пред сессии
        /// </summary>
        /// <param name="context">ИД параметра</param>
        /// <param name="connSett">Параметры подключения к БД</param>
        /// <returns>Результат: значение элемента-котекста</returns>
        public static bool IsAllowed(int context, ConnectionSettings connSett)
        {
            return bool.Parse(GetAllowed(context, connSett));
        }

        public static bool IsAllowed(ID_ALLOWED context, ConnectionSettings connSett)
        {
            return bool.Parse(GetAllowed((int)context, connSett));
        }
        /// <summary>
        /// Свойство возвращающее таблицу с описанием атрибутов Profile
        /// </summary>
        new public static DataTable GetTableProfileUnits { get { return HTepProfilesXml.GetTableUnits; } }
        /// <summary>
        /// Структура визуальных настроек элемента
        /// </summary>
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
        /// <summary>
        /// Значение по умолчанию для множителя
        /// </summary>
        public static int s_iRatioDefault = 0;
        /// <summary>
        /// Значение по умолчанию для кол-ва знвков после запятой
        /// </summary>
        public static int s_iRoundDefault = 2;
        /// <summary>
        /// Структура содержащая в себе атрибуты выбранного объекта и словарь с вложенными в него элементами
        /// </summary>
        public class DictionaryProfileItem : Dictionary <string, DictionaryProfileItem>
        {
            /// <summary>
            /// Метод для получения словаря из XML
            /// </summary>
            /// <param name="node">Node для разбора</param>
            /// <returns>Словарь полученный из XML</returns>
            public static DictionaryProfileItem FromXml(XmlNode node)
            {
                DictionaryProfileItem dictProfileItemRes = new DictionaryProfileItem();

                DictionaryProfileItem dictProfileItemChild = null;

                if (node.ChildNodes.Count > 0) {
                    foreach (XmlNode childNode in node.ChildNodes) {
                        dictProfileItemChild = new DictionaryProfileItem();

                        foreach (XmlAttribute attr in childNode.Attributes)
                            dictProfileItemChild.Attributes.Add(attr.Name.Replace('_', ' ').Trim(), attr.Value);

                        if (childNode.ChildNodes.Count > 0)
                            dictProfileItemChild.SetObjects(childNode);
                        else
                        // нет элементов для добавления
                            ;

                        dictProfileItemRes.Add(childNode.LocalName.Replace('_', ' ').Trim(), dictProfileItemChild);
                    }
                } else
                // нет элементов для добавления
                    ;

                return dictProfileItemRes;
            }
            ///// <summary>
            ///// Словарь с вложенными объектами
            ///// </summary>
            //private Dictionary<string, DictionaryProfileItem> _objects;

            public DictionaryProfileItem() : base ()
            {
                Attributes = new Dictionary<string, string>();
            }
            /// <summary>
            /// Возвратить значение объекта по ключу, если ключ сложный возвратиь значения вложенных объектов
            /// </summary>
            /// <param name="keys">Клюс словаря для получения значения</param>
            /// <returns>Значение словаря</returns>
            public DictionaryProfileItem GetObjects(params string[]keys)
            {
                DictionaryProfileItem dictProfileItemRes = new DictionaryProfileItem();

                try {
                    if (keys.Length > 0)
                        foreach (string key in keys)
                            if (dictProfileItemRes.ObjectCount == 0)
                                dictProfileItemRes = ContainsKey(key) == true ? this[key] : new DictionaryProfileItem();
                            else
                                dictProfileItemRes = dictProfileItemRes.ContainsKey(key) == true ? dictProfileItemRes[key] : new DictionaryProfileItem();
                    else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"DictionaryProfileItem::GetObjects () - ...")
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }

                return dictProfileItemRes;
            }

            public string GetAttribute(params object[] keys)
            {
                Func<object, string> fToString = delegate (object obj) {
                    string strRes = string.Empty;

                    Type type = obj.GetType();

                    if (type.Equals(typeof(string)) == true)
                        strRes = (string)obj;
                    else if (type.IsEnum)
                        strRes = ((int)obj).ToString();
                    else if (type.IsPrimitive)
                        strRes = obj.ToString();
                    else
                        ;

                    return strRes;
                };

                return GetAttribute(
                        //Array.ConvertAll<object, string>(keys, (o) => { return (string)o; })
                        keys.ToList().Select(o => fToString(o)).ToArray()
                    );
            }

            public string GetAttribute(params string[] keys)
            {
                string strRes = string.Empty;

                string []objectKeys;
                string attributeKey;

                if (keys.Length > 1) {
                    objectKeys = keys.Take(keys.Length - 1).ToArray();
                    attributeKey = keys[keys.Length - 1];

                    strRes = GetObjects(objectKeys).Attributes[attributeKey];
                } else if (keys.Length == 1) {
                    if (Attributes.ContainsKey(keys[0]) == true)
                        strRes = Attributes[keys[0]];
                    else
                        Logging.Logg().Warning(string.Format(@"DictionaryProfileItem::GetAttribute (keys.Length=1) - ключ {0} не найден...", keys[0]), Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    Logging.Logg().Error(string.Format(@"DictionaryProfileItem::GetAttribute (keys.Length=0) - аргументы не указаны..."), Logging.INDEX_MESSAGE.NOT_SET);

                return strRes;
            }

            public string GetAttribute(ID_PERIOD idPeriod, int context)
            {
                return GetAttribute(((int)idPeriod).ToString(), context.ToString());
            }

            /// <summary>
            /// Установить значение для текущего объекта
            /// </summary>
            /// <param name="node">Источник значений</param>
            public void SetObjects(XmlNode node)
            {
                DictionaryProfileItem objects = FromXml(node);

                foreach (KeyValuePair<string, DictionaryProfileItem> pair in objects)
                    this.Add(pair.Key, pair.Value);
            }
            /// <summary>
            /// Словарь с атрибутами объекта
            /// </summary>
            public Dictionary<string, string> Attributes;

            private string GetAttribute(HTepUsers.HTepProfilesXml.INDEX_PROFILE indxKey)
            {
                return GetAttribute(((int)indxKey).ToString());
            }

            private string GetAttribute(object oKey)
            {
                return GetAttribute(((int)oKey).ToString());
            }


            private string GetAttribute(int iKey)
            {
                return GetAttribute(iKey.ToString());
            }

            private string GetAttribute(string key)
            {
                string strRes = string.Empty;

                if (Attributes.ContainsKey(key) == true)
                    strRes = Attributes[key];
                else
                    ;

                return strRes;
            }
            /// <summary>
            /// Количество значений в текущем объекте
            /// </summary>
            public int ObjectCount { get { return this.Count; } }
            /// <summary>
            /// Количество аттрибутов в текущем объекте
            /// </summary>
            public int AttributesCount { get { return Attributes.Count; } }
        }
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

            DictionaryProfileItem dictProfileItem = null
                , objects = null;
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

            if (fields.Length == (int)INDEX_VISUALSETTINGS_PARAMS.COUNT)
            {
                //Получения элемента со словарями атрибутов и вложенных элементов для панели
                dictProfileItem = HTepProfilesXml.GetProfileUserPanel(Id, Role, fields[(int)INDEX_VISUALSETTINGS_PARAMS.TAB]);

                objects = null;
                //Перебор Item'ов в панели
                if (dictProfileItem.ContainsKey(fields[(int)INDEX_VISUALSETTINGS_PARAMS.ITEM].ToString()) == true)
                {
                    //Словарь с объектами в Item
                    objects = dictProfileItem.GetObjects(fields[(int)INDEX_VISUALSETTINGS_PARAMS.ITEM].ToString());
                    
                    foreach (string rAlg in objects.Keys)
                    {
                        ratio = 0;
                        round = 0;

                        foreach (string idUnit in objects[rAlg].Attributes.Keys)
                        {
                            switch ((ID_ALLOWED)short.Parse(idUnit))
                            {
                                case ID_ALLOWED.VISUAL_SETTING_VALUE_RATIO:
                                    ratio = int.Parse(objects[rAlg].Attributes[idUnit]);
                                    break;
                                case ID_ALLOWED.VISUAL_SETTING_VALUE_ROUND:
                                    round = int.Parse(objects[rAlg].Attributes[idUnit]);
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
        public static Dictionary<string, DictionaryProfileItem> GetDicpRolesProfile
        {
            get
            {
                Dictionary<string, DictionaryProfileItem> dictRoles = HTepProfilesXml.ProfileRoles;
                return dictRoles;
            }
        }
        /// <summary>
        /// Получение словаря с Profile для пользователей
        /// </summary>
        public static Dictionary<string, DictionaryProfileItem> GetDicpUsersProfile
        {
            get
            {
                Dictionary<string, DictionaryProfileItem> dictUsers = HTepProfilesXml.ProfileUsers;
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
        
        protected class HTepProfiles : HProfiles
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="idListener">ИД слушателя</param>
            /// <param name="id_role">ИД роли</param>
            /// <param name="id_user">ИД пользователя</param>
            public HTepProfiles(int idListener, int id_role, int id_user)
                : base(idListener, id_role, id_user)
            {
            }

            /// <summary>
            /// Функция получения доступа
            /// </summary>
            /// <param name="id_tab">ID вкладки</param>
            /// <returns></returns>
            public new static object GetAllowed(int id_tab)
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
        
        public class HTepProfilesXml
        {
            /// <summary>
            /// Имя таблицы содержащей XML с описанием Profile
            /// </summary>
            public static string s_nameTableProfilesData = @"profiles";
            /// <summary>
            /// Имя таблицы содержащей описания атрибутов элементов
            /// </summary>
            public static string s_nameTableProfilesUnit = @"profiles_unit";
            /// <summary>
            /// Перечисление индексов профайла
            /// </summary>
            public enum INDEX_PROFILE
            {
                UNKNOW = -1,
                TIMEZONE = 101, MAIL, PERIOD, ENABLED_CONTROL,
                ROUND = 201, RATIO, INPUT_PARAM, ENABLED_ITEM = 204, VISIBLED_ITEM
            }
            /// <summary>
            /// Таблица содержащая описания атрибутов Profile
            /// </summary>
            protected static DataTable m_tblTypes;
            /// <summary>
            /// Словарь с Профайлом для ролей
            /// </summary>
            public static Dictionary<string, DictionaryProfileItem> ProfileRoles;
            /// <summary>
            /// Словарь с Профайлом для пользователей
            /// </summary>
            public static Dictionary<string, DictionaryProfileItem> ProfileUsers;
            /// <summary>
            /// Словарь с XML для каждой роли
            /// </summary>
            public static Dictionary<string, XmlDocument> XmlRoles;
            /// <summary>
            /// Словарь с XML для каждого пользователя
            /// </summary>
            public static Dictionary<string, XmlDocument> XmlUsers;
            /// <summary> 
            /// Оригинальная таблица с Profile
            /// </summary>
            public static DataTable TableProfiles { get { return s_tableProfiles_Orig; } }
            /// <summary> 
            /// Оригинальная таблица с Profile
            /// </summary>
            private static DataTable s_tableProfiles_Orig;
            /// <summary>
            /// Измененная таблица с Profile
            /// </summary>
            private static DataTable s_tableProfiles_Edit;
            /// <summary>
            /// Тип XML (роль/пользователь)
            /// </summary>
            public enum Type : int { User, Role, Count };
            /// <summary>
            /// Типы элементов
            /// </summary>
            public enum Component : int { None, Panel, Item, Context, Count };

            #region GetDict
            /// <summary>
            /// Получить таблицу из БД
            /// </summary>
            /// <param name="connSet">Connection Settings</param>
            /// <returns>Таблица с XML</returns>
            private static DataTable getDataTable(ConnectionSettings connSet)
            {
                DataTable tableRes;
                int err = 0;

                int idListener = -1;
                string query = string.Empty;
                DbConnection dbConn = null;

                idListener = DbSources.Sources().Register(connSet, false, string.Format(@"Интерфейс: {0}", connSet.dbName));

                dbConn = DbSources.Sources().GetConnection(idListener, out err);
                query = "SELECT * FROM ["+ s_nameTableProfilesData + "]";
                tableRes = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);

                DbSources.Sources().UnRegister(idListener);

                return tableRes;
            }
            /// <summary>
            /// Получение словаря с профайлом для ролей
            /// </summary>
            /// <param name="connSet">Connection Settings</param>
            private static void getProfileAllRoles()
            {
                string query = "IS_ROLE=1";
                DataRow[] dtUnic = s_tableProfiles_Orig.Select(query);

                DictionaryProfileItem dictProfileItem = new DictionaryProfileItem();
                XmlDocument xml = new XmlDocument();

                ProfileRoles = new Dictionary<string, DictionaryProfileItem>();
                XmlRoles = new Dictionary<string, XmlDocument>();

                for (int i = 0; i < dtUnic.Length; i++)
                {
                    dictProfileItem = new DictionaryProfileItem();
                    xml = new XmlDocument();
                    xml.LoadXml(dtUnic[i]["XML"].ToString());
                    dictProfileItem = DictionaryProfileItem.FromXml(xml["Role"]);

                    ProfileRoles.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), dictProfileItem[dtUnic[i]["ID_EXT"].ToString().Trim()]);

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
                DataRow[] dtUnic = s_tableProfiles_Orig.Select(query);

                DictionaryProfileItem dictProfileItem;
                XmlDocument xml;

                ProfileUsers = new Dictionary<string, DictionaryProfileItem>();
                XmlUsers = new Dictionary<string, XmlDocument>();

                for (int i = 0; i < dtUnic.Length; i++) {
                    xml = new XmlDocument();

                    xml.LoadXml(dtUnic[i]["XML"].ToString());
                    dictProfileItem = DictionaryProfileItem.FromXml(xml["User"]);

                    ProfileUsers.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), dictProfileItem[dtUnic[i]["ID_EXT"].ToString().Trim()]);
                    XmlUsers.Add(dtUnic[i]["ID_EXT"].ToString().Trim(), xml);
                }
            }            
            /// <summary>
            /// Получить профайл для конкретного пользователя
            /// </summary>
            /// <param name="id_user">ИД пользователя</param>
            /// <param name="id_role">ИД роли</param>
            /// <returns></returns>
            public static DictionaryProfileItem GetProfileUser(int id_user, int id_role)
            {
                DictionaryProfileItem profileUser = getDictionaryProfileItem(ProfileRoles[id_role.ToString()], ProfileUsers[id_user.ToString()]);

                return profileUser;
            }
            /// <summary>
            /// Получить профайл для панели конкретного пользователя
            /// </summary>
            /// <param name="id_user">ИД пользователя</param>
            /// <param name="id_role">ИД роли</param>
            /// <param name="id_panel">ИД панели (вкладки)</param>
            /// <returns></returns>
            public static DictionaryProfileItem GetProfileUserPanel(int id_user, int id_role, int id_panel)
            {
                DictionaryProfileItem profileUser = getDictionaryProfileItem(ProfileRoles[id_role.ToString()], ProfileUsers[id_user.ToString()]);

                DictionaryProfileItem profilePanel = new DictionaryProfileItem();

                if (profileUser.ContainsKey(id_panel.ToString()) == true)
                {
                    profilePanel = profileUser[id_panel.ToString()];
                }

                return profilePanel;
            }
            /// <summary>
            /// Метод для объединения профайла пользователя и роли
            /// </summary>
            /// <param name="dictObject_Role">Структура с данными роли</param>
            /// <param name="dictObject_User">Структура с данными пользователя</param>
            /// <returns>Структура с объединенными данными</returns>
            private static DictionaryProfileItem getDictionaryProfileItem(DictionaryProfileItem dictObject_Role, DictionaryProfileItem dictObject_User)
            {
                DictionaryProfileItem profileUser = new DictionaryProfileItem();

                DictionaryProfileItem dict_role = dictObject_Role;
                DictionaryProfileItem dict_user = dictObject_User;

                foreach (string attr in dict_role.Attributes.Keys)
                {
                    if (dict_user.Attributes.ContainsKey(attr) == false)
                    {
                        profileUser.Attributes.Add(attr, dict_role.Attributes[attr]);
                    }
                    else
                        profileUser.Attributes.Add(attr, dict_user.Attributes[attr]);
                }

                foreach (string attr in dict_role.Keys)
                {
                    if (dict_user.ContainsKey(attr) == false)
                    {
                        profileUser.Add(attr, dict_role[attr]);
                    }
                    else
                        profileUser.Add(attr, getDictionaryProfileItem(dict_role[attr], dict_user[attr]));
                }

                return profileUser;
            }

            #endregion

            /// <summary>
            /// Конструктор
            /// </summary>
            public HTepProfilesXml()
            {
            }

            /// <summary>
            /// Обновление данных 
            /// </summary>
            /// <param name="connSet">Параметры подключения к БД</param>
            public static void UpdateProfile(ConnectionSettings connSet)
            {
                s_tableProfiles_Orig = new DataTable();
                XmlRoles = new Dictionary<string, XmlDocument>();
                XmlUsers = new Dictionary<string, XmlDocument>();

                s_tableProfiles_Orig = getDataTable(connSet);
                GetTableProfileUnits(connSet);
                s_tableProfiles_Edit = s_tableProfiles_Orig.Copy();
                getProfileAllRoles();
                getProfileAllUsers();
            }

            #region EditXML

            /// <summary>
            /// Метод для редактирования аттрибута текущего пользователя
            /// </summary>
            /// <param name="id_unit">ИД атрибута</param>
            /// <param name="value">Значение</param>
            /// <param name="connSet">ConnectionSettings для сохранения</param>
            public static void Edit(int id_unit, string value, ConnectionSettings connSet)
            {
                ParamComponent parComp = new ParamComponent();

                parComp.ID_Unit = id_unit;
                parComp.Value = value;
                XmlDocument doc = new XmlDocument();

                try
                {
                    doc = Edit(XmlUsers[Id.ToString()], Type.User, Id, Component.None, parComp);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "HTepProfileXml: Ошибка редактирования атрибута с ИД="+ id_unit + ".", Logging.INDEX_MESSAGE.NOT_SET);
                }

                Save(connSet, doc, Id, Type.User);
            }
            /// <summary>
            /// Метод для добавления активной панели
            /// </summary>
            /// <param name="idPanel">ИД панели</param>
            /// <param name="connSet">ConnectionSettings для сохранения</param>
            public static void AddPanel(int idPanel, ConnectionSettings connSet)
            {
                //??? почему == 3
                int idUnit = 3;

                ParamComponent parComp = new ParamComponent();
                string value = ProfileUsers[Id.ToString()].Attributes[idUnit.ToString()];

                parComp.ID_Unit = idUnit;
                parComp.Value = value + ',' + idPanel.ToString();

                XmlDocument doc = Edit(XmlUsers[Id.ToString()], Type.User, Id, Component.None, parComp);

                Save(connSet, doc, Id, Type.User);
            }
            /// <summary>
            /// Метод для удаления активной панели
            /// </summary>
            /// <param name="idPanel">ИД панели</param>
            /// <param name="connSet">ConnectionSettings для сохранения</param>
            public static void RemovePanel(int idPanel, ConnectionSettings connSet)
            {
                //??? почему == 3
                int idUnit = 3;

                ParamComponent parComp = new ParamComponent();
                string value = string.Empty;
                string[] panels;

                value = ProfileUsers[Id.ToString()].Attributes[idUnit.ToString()];
                panels = value.Split(',');
                string[] add_pan = new string[panels.Length - 1];

                for (int i = 0; i < panels.Length; i++)
                    if (panels[i] != idPanel.ToString())
                        add_pan[i] = panels[i];

                parComp.ID_Unit = idUnit;
                parComp.Value = string.Join(",", add_pan);
                XmlDocument doc = Edit(XmlUsers[Id.ToString()], Type.User, Id, Component.None, parComp);

                Save(connSet, doc, Id, Type.User);
            }
            /// <summary>
            /// Метод для редактирования параметра в XML
            /// </summary>
            /// <param name="doc">XML документ</param>
            /// <param name="id">ИД пользователя/роли</param>
            /// <param name="type">Тип XML (пользователь/роль)</param>
            /// <param name="comp">Тип изменяемого компонента</param>
            /// <param name="parComp">Структура с ИД компонентов и изменяемым значением</param>
            /// <returns>XML документ</returns>
            public static XmlDocument Edit(XmlDocument doc, Type type, int id, Component comp, ParamComponent parComp)
            {
                XmlAttribute edit_attr = null;
                XmlNode edit_node = null;
                if (doc != null)
                {
                    try
                    {
                        switch (comp)
                        {
                            case Component.Context:

                                foreach (XmlAttribute attr in doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()]
                                    ["_" + parComp.Context]
                                    .Attributes)
                                {
                                    if (attr.LocalName == "_" + parComp.ID_Unit.ToString())
                                    {
                                        edit_attr = attr;
                                        break;
                                    }
                                }

                                edit_node = doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()]
                                    ["_" + parComp.Context];

                                
                                break;
                            case Component.Item:
                                
                                foreach (XmlAttribute attr in doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()]
                                    .Attributes)
                                {
                                    if (attr.LocalName == "_" + parComp.ID_Unit.ToString())
                                    {
                                        edit_attr = attr;
                                        break;
                                    }
                                }
                                edit_node = doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    ["_" + parComp.ID_Item.ToString()];


                                break;
                            case Component.Panel:

                                foreach (XmlAttribute attr in doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()]
                                    .Attributes)
                                {
                                    if (attr.LocalName == "_" + parComp.ID_Unit.ToString())
                                    {
                                        edit_attr = attr;
                                        break;
                                    }
                                }

                                edit_node = doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    ["_" + parComp.ID_Panel.ToString()];

                                break;

                            case Component.None:

                                foreach (XmlAttribute attr in doc[type.ToString()]
                                    ["_" + id.ToString()]
                                    .Attributes)
                                {
                                    if (attr.LocalName == "_" + parComp.ID_Unit.ToString())
                                    {
                                        edit_attr = attr;
                                        break;
                                    }
                                }

                                edit_node = doc[type.ToString()]
                                    ["_" + id.ToString()];

                                break;
                        }

                        if (edit_attr != null)
                        {
                            if (parComp.Value != string.Empty & parComp.Value!=null)
                                edit_attr.Value = parComp.Value;
                            else
                            {
                                edit_node.Attributes.Remove(edit_attr);
                            }
                        }
                        else
                        {
                            XmlAttribute new_attr = doc.CreateAttribute("_" + parComp.ID_Unit.ToString());
                            new_attr.Value = parComp.Value;
                            edit_node
                            .Attributes.Append(new_attr);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "HTepProfileXml:EditAttr: Ошибка редактирования атрибутов в XML.", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
                return doc;

            }
            /// <summary>
            /// Метод добавления компонента в XML
            /// </summary>
            /// <param name="doc">XML документ</param>
            /// <param name="id">ИД пользователя/роли</param>
            /// <param name="type">Тип XML (пользователь/роль)</param>
            /// <param name="comp">Тип изменяемого компонента</param>
            /// <param name="parComp">Структура с ИД компонентов и изменяемым значением</param>
            /// <returns>XML документ</returns>
            public static XmlDocument Add(XmlDocument doc, Type type, int id, Component comp, ParamComponent parComp)
            {
                XmlElement newElement;
                if (doc != null)
                    switch (comp)
                    {
                        case Component.Context:
                            newElement = doc.CreateElement("_" + parComp.Context);

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
            /// <summary>
            /// Возвратить строку с внутренним текстом элемента
            /// </summary>
            /// <param name="doc">Исходный XML документ</param>
            /// <param name="type">Тип XML (пользователь/роль)</param>
            /// <param name="id">ИД пользователя/роли</param>
            /// <param name="comp">Тип изменяемого компонента (уровень дерева)</param>
            /// <param name="parComp">Структура с ИД компонентов и изменяемым значением</param>
            /// <returns>Строка с внутренним текстом элемента</returns>
            public static string Doc(XmlDocument doc, Type type, int id, Component comp, ParamComponent parComp)
            {
                string strRes = string.Empty;

                if (doc != null)
                    switch (comp) {
                        case Component.Panel:
                            strRes = doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()].OuterXml;
                            break;
                        case Component.Item:
                            strRes = doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()]
                                        ["_" + parComp.ID_Item.ToString()].OuterXml;
                            break;
                        case Component.Context:
                            strRes = doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()]
                                        ["_" + parComp.ID_Item.ToString()]["_" + parComp.Context].OuterXml;
                            break;
                        default:
                            break;
                    }
                else
                    ;

                return strRes;
            }
            /// <summary>
            /// Метод удаления компонента из XML
            /// </summary>
            /// <param name="doc">Исходный XML документ</param>
            /// <param name="id">ИД пользователя/роли</param>
            /// <param name="type">Тип XML (пользователь/роль)</param>
            /// <param name="comp">Тип изменяемого компонента</param>
            /// <param name="parComp">Структура с ИД компонентов и изменяемым значением</param>
            /// <returns>XML документ</returns>
            public static XmlDocument Remove(XmlDocument doc, Type type, int id, Component comp, ParamComponent parComp)
            {
                if (doc != null)
                    switch (comp)
                    {
                        case Component.Context:
                            doc[type.ToString()]
                                ["_" + id.ToString()]
                                ["_" + parComp.ID_Panel.ToString()]
                                ["_" + parComp.ID_Item.ToString()].RemoveChild(
                                    doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()]
                                        ["_" + parComp.ID_Item.ToString()]["_" + parComp.Context]);
                                
                            break;
                        case Component.Item:
                            doc[type.ToString()]
                                ["_" + id.ToString()]
                                ["_" + parComp.ID_Panel.ToString()].RemoveChild(
                                    doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()]
                                        ["_" + parComp.ID_Item.ToString()]);

                            break;
                        case Component.Panel:
                            doc[type.ToString()]
                                ["_" + id.ToString()].RemoveChild(
                                    doc[type.ToString()]
                                        ["_" + id.ToString()]
                                        ["_" + parComp.ID_Panel.ToString()]);
                            break;
                    }

                return doc;
            }
            /// <summary>
            /// Метод для сохранения определённой XML
            /// </summary>
            /// <param name="connSet">ConnectionSettings для сохранения</param>
            /// <param name="doc">Сохраняемый документ</param>
            /// <param name="id">ИД пользователя/роли</param>
            /// <param name="type">Тип XML (пользователь/роль)</param>
            public static void Save(ConnectionSettings connSet, XmlDocument doc, int id, Type type)
            {
                s_tableProfiles_Edit = new DataTable();
                s_tableProfiles_Edit = s_tableProfiles_Orig.Copy();
                foreach (DataRow row in s_tableProfiles_Edit.Rows)
                {
                    if (row["ID_EXT"].ToString().Trim() == id.ToString() & row["IS_ROLE"].ToString().Trim() == ((int)type).ToString())
                    {
                        row["XML"] = doc.InnerXml;
                    }
                }

                int err = 0;
                int idListener = DbSources.Sources().Register(connSet, false, string.Format(@"Интерфейс: {0}", connSet.dbName));
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, s_nameTableProfilesData, "ID_EXT, IS_ROLE", string.Empty, s_tableProfiles_Orig, s_tableProfiles_Edit, out err);

                DbSources.Sources().UnRegister(idListener);

                UpdateProfile(connSet);
            }
            /// <summary>
            /// Метод для сохранения внесенных изменений в таблице
            /// </summary>
            /// <param name="connSet">ConnectionSettings для сохранения</param>
            /// <param name="arrDictOrig">Оригинальная таблица</param>
            /// <param name="arrDictEdit">Измененная таблица</param>
            public static void Save(ConnectionSettings connSet, Dictionary<string, XmlDocument>[] arrDictOrig, Dictionary<string, XmlDocument>[] arrDictEdit)
            {
                int err = 0;
                int idListener = DbSources.Sources().Register(connSet, false, string.Format(@"Интерфейс: {0}", connSet.dbName));
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                DataTable dtProfiles_Orig, dtProfiles_Edit;

                dtProfiles_Orig = new DataTable();
                dtProfiles_Edit = new DataTable();

                dtProfiles_Orig = getDTfromDict(arrDictOrig);
                dtProfiles_Edit = getDTfromDict(arrDictEdit);

                DbTSQLInterface.RecUpdateInsertDelete(ref dbConn, s_nameTableProfilesData, "ID_EXT, IS_ROLE", string.Empty, dtProfiles_Orig, dtProfiles_Edit, out err);

                DbSources.Sources().UnRegister(idListener);

                UpdateProfile(connSet);
            }
            /// <summary>
            /// Метод для получения таблицы из словаря с XML документами
            /// </summary>
            /// <param name="arrDict">Словарь с XML документами</param>
            /// <returns>Таблицу</returns>
            private static DataTable getDTfromDict(Dictionary<string, XmlDocument>[] arrDict)
            {
                DataTable dt = new DataTable();
                dt = s_tableProfiles_Orig.Clone();


                for (int i = 0; i < (int)Type.Count; i++)
                {
                    foreach (string key in arrDict[i].Keys)
                    {
                        dt.Rows.Add(new object[] { key, i, arrDict[i][key].InnerXml });
                    }
                }


                return dt;
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
            public static XmlDocument GetXml(Type Type_Xml, int Id)
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
            /// Метод для получения новой XML(при создании пользователя/роли)
            /// </summary>
            /// <param name="Type_Xml">Тип создаваемой XML</param>
            /// <param name="Id">ИД роли/пользователя</param>
            /// <returns>XmlDocument</returns>
            public static XmlDocument GetNewXml(Type Type_Xml, int Id)
            {
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                doc.AppendChild(doc.CreateElement(Type_Xml.ToString()));
                doc[Type_Xml.ToString()].AppendChild(doc.CreateElement("_"+Id.ToString()));

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

                int idListener = DbSources.Sources().Register(connSet, false, string.Format(@"Интерфейс: {0}", connSet.dbName));
                DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

                if (!(err == 0))
                    errMsg = @"нет соединения с БД";
                else
                {
                    query = @"SELECT * from " + s_nameTableProfilesUnit;
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
    }
}
