﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Globalization;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskTepInval
{
    public class PanelTaskTepInval : PanelTaskTepValues
    {
        /// <summary>
        /// Перечисление - индексы таблиц для значений
        ///  , собранных в автоматическом режиме
        ///  , "по умолчанию"
        /// </summary>
        private enum INDEX_TABLE_VALUES : int { VARIABLE, DEFAULT, COUNT }
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для связи с вызывающим приложением</param>
        public PanelTaskTepInval(IPlugIn iFunc)
            : base(iFunc, TYPE.IN_VALUES)
        {
            m_arTableOrigin = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE]; }
        }

        protected override System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE]; }
        }

        protected override string whereRangeRecord
        {
            get { return string.Empty; }
        }
        /// <summary>
        /// Запрос для получения значений "по умолчанию"
        /// </summary>
        private string getQueryValuesDef()
        {
            string strRes = string.Empty;

            strRes = @"SELECT"
                + @" *"
                + @" FROM [dbo].[" + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVAL_DEF] + @"] v"
                + @" WHERE [ID_TIME] = " + (int)ActualIdPeriod //(int)_currIdPeriod
                    ;

            return strRes;
        }
        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            err = -1;

            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn
                , HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVAL_DEF]
                , @"ID_INPUT, ID_TIME"
                , m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT]
                , out err);
        }
        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();
        }

        public override void Stop()
        {
            deleteSession();

            base.Stop();
        }
        /// <summary>
        /// Создать новую сессию для расчета
        ///  - вставить входные данные во временную таблицу
        /// </summary>
        /// <param name="dbConn">Ссылка на объектт соединения с БД</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        private void createSession(ref DbConnection dbConn, DateTimeRange dtRange, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            string strQuery = string.Empty
                , strNameColumn = string.Empty;
            string[] arNameColumns = null;
            Type[] arTypeColumns = null;

            if ((m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Columns.Count > 0)
                && (m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows.Count > 0))
            {
                // подготовить содержание запроса при вставке значений, идентифицирующих новую сессию
                strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @" ("
                    + @"[ID_CALCULATE]"
                    + @", [ID_TASK]"
                    + @", [ID_USER]"
                    + @", [ID_TIME]"
                    + @", [ID_TIMEZONE]"
                    + @", [DATETIME_BEGIN]"
                    + @", [DATETIME_END]) VALUES ("
                    ;

                strQuery += _IdSession;
                strQuery += @"," + (int)ID_TASK.TEP;
                strQuery += @"," + HTepUsers.Id;
                strQuery += @"," + (int)_currIdPeriod;
                strQuery += @"," + (int)_currIdTimezone;
                strQuery += @",'" + dtRange.Begin.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"
                strQuery += @",'" + dtRange.End.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"'"; // @"yyyyMMdd HH:mm:ss"

                strQuery += @")";

                //Вставить в таблицу БД новый идентификтор сессии
                DbTSQLInterface.ExecNonQuery(ref dbConn, strQuery, null, null, out err);

                // подготовить содержание запроса при вставке значений во временную таблицу для расчета
                strQuery = @"INSERT INTO " + HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" (";

                arTypeColumns = new Type[m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Columns.Count];
                arNameColumns = new string[m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Columns.Count];
                foreach (DataColumn c in m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Columns)
                {
                    arTypeColumns[c.Ordinal] = c.DataType;
                    if (c.ColumnName.Equals(@"ID") == true)
                        strNameColumn = @"ID_INPUT";
                    else
                        strNameColumn = c.ColumnName;
                    arNameColumns[c.Ordinal] = strNameColumn;
                    strQuery += strNameColumn + @",";
                }
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);

                strQuery += @") VALUES ";

                foreach (DataRow r in m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows)
                {
                    strQuery += @"(";

                    foreach (DataColumn c in m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Columns)
                        strQuery += DbTSQLInterface.ValueToQuery(r[c.Ordinal], arTypeColumns[c.Ordinal]) + @",";

                    // исключить лишнюю запятую
                    strQuery = strQuery.Substring(0, strQuery.Length - 1);

                    strQuery += @"),";
                }
                // исключить лишнюю запятую
                strQuery = strQuery.Substring(0, strQuery.Length - 1);
                //Вставить во временную таблицу в БД входные для расчета значения
                DbTSQLInterface.ExecNonQuery(ref dbConn, strQuery, null, null, out err);
            }
            else
                Logging.Logg().Error(@"PanelTaskTepInVal::createSession () - отсутствуют строки для вставки ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="dbConn">Ссылка на объектт соединения с БД</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        protected override void setValues(ref DbConnection dbConn, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            int cnt = CountBasePeriod //(int)(m_panelManagement.m_dtRange.End - m_panelManagement.m_dtRange.Begin).TotalHours - 0
                , iAVG = -1;
            string query = string.Empty;
            // строки для удаления из таблицы значений "по умолчанию"
            // при наличии дубликатов строк в таблице с загруженными из источников с данными
            DataRow[] rowsSel = null;

            DateTimeRange[] arQueryRanges = getDateTimeRangeValuesVar();
            //Запрос для получения автоматически собираемых данных
            query = getQueryValuesVar(arQueryRanges);
            //Заполнить таблицу автоматически собираемыми данными
            m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE] = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Запрос для получения данных вводимых вручную
                query = getQueryValuesDef();
                //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT] = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                {
                    // удалить строки из таблицы со значениями "по умолчанию"
                    foreach (DataRow rValVar in m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows)
                    {
                        rowsSel = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_INPUT=" + rValVar[@"ID"]);
                        foreach (DataRow rToRemove in rowsSel)
                            m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Rows.Remove(rToRemove);
                    }
                    // вставить строки из таблицы со значениями "по умолчанию"
                    foreach (DataRow rValDef in m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Rows)
                    {
                        rowsSel = m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER].Select(@"ID=" + rValDef[@"ID_INPUT"]);
                        if (rowsSel.Length == 1)
                        {
                            iAVG = (Int16)rowsSel[0][@"AVG"];

                            m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Rows.Add(new object[]
                                    {
                                        rValDef[@"ID_INPUT"]
                                        //, HUsers.Id //ID_USER
                                        , -1 //ID_SOURCE
                                        , _IdSession //ID_SESSION
                                        , (int)ID_QUALITY_VALUE.DEFAULT //QUALITY
                                        , (iAVG == 0) ? cnt * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
                                        , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                    }
                            );
                        }
                        else
                            ; // по идентификатору найден не единственный парпметр расчета
                    }
                    //Начать новую сессию расчета
                    createSession(ref dbConn, new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End), out err, out strErr);                    
                    //Получить входные для расчета значения для возможности редактирования
                    m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE] = DbTSQLInterface.Select (ref dbConn, @"SELECT [ID_INPUT] as [ID],[ID_SOURCE],[ID_SESSION],[QUALITY],[VALUE],[WR_DATETIME] FROM [inval] WHERE [ID_SESSION]=" +_IdSession, null, null, out err);
                    // создать копии для возможности сохранения изменений
                    m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Copy();
                    m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();
                }
                else
                    strErr = @"ошибка получения данных по умолчанию с " + PanelManagement.m_dtRange.Begin.ToString()
                        + @" по " + PanelManagement.m_dtRange.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + PanelManagement.m_dtRange.Begin.ToString()
                    + @" по " + PanelManagement.m_dtRange.End.ToString();
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="dgv">Объект, инициирововший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            DataRow[] rowsParameter = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_INPUT=" + ev.m_IdParameter);

            if (rowsParameter.Length == 1)
            {
                rowsParameter[0][@"VALUE"] = ev.m_Value;
            }
            else
                ;
        }

        protected override PanelTaskTepCalculate.PanelManagementTaskTepCalculate createPanelManagement()
        {
            return new PanelManagementTaskTepInval();
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepInval : PanelManagementTaskTepValues
        {
            protected override int addButtonRun(int posRow)
            {
                Button ctrl = null;
                int iRes = posRow;
                //Расчет - выполнить - норматив
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_PREV.ToString();
                ctrl.Text = @"К нормативу";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, iRes = 0);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Расчет - выполнить - макет
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_RUN_RES.ToString();
                ctrl.Text = @"К макету";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, iRes = iRes + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);

                return iRes;
            }
        }
    }

    public class PlugIn : PlugInTepTaskCalculate
    {
        public PlugIn()
            : base()
        {
            _Id = 17;

            _nameOwnerMenuItem = @"Задача\Расчет ТЭП";
            _nameMenuItem = @"Входные данные";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject(typeof(PanelTaskTepInval));

            base.OnClickMenuItem(obj, ev);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
