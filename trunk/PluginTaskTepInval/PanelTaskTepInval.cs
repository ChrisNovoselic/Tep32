using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;

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
            : base(iFunc, @"inalg", @"input", @"inval")
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
        /// <summary>
        /// Запрос для получения значений "по умолчанию"
        /// </summary>
        private string getQueryValuesDef()
        {
            string strRes = string.Empty;

            strRes = @"SELECT"
                + @" *"
                + @" FROM [dbo].[" + m_strNameTableValues + @"_def] v"
                + @" WHERE [ID_TIME] = " + (int)_currIdPeriod
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
                , @"inval_def"
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
        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="dbConn">Ссылка на объектт соединения с БД</param>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при галичии ошибки</param>
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
            //Запрос для получения автоматически собираемых данных
            query = getQueryValuesVar();
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
                                        , HUsers.Id //ID_USER
                                        , -1 //ID_SOURCE
                                        , 0 //ID_SESSION
                                        , -1 //QUALITY
                                        , (iAVG == 0) ? cnt * (double)rValDef[@"VALUE"] : (double)rValDef[@"VALUE"] //VALUE
                                        , HDateTime.ToMoscowTimeZone() //??? GETADTE()
                                    }
                            );
                        }
                        else
                            ; // по иднгтификатору найден не единственный парпметр расчета
                    }
                    // создать копии для возможности сохранения изменений
                    m_arTableEdit[(int)INDEX_TABLE_VALUES.VARIABLE] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE].Copy();
                    m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT] = m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT].Copy();
                }
                else
                    strErr = @"ошибка получения данных по умолчанию с " + m_panelManagement.m_dtRange.Begin.ToString()
                        + @" по " + m_panelManagement.m_dtRange.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + m_panelManagement.m_dtRange.Begin.ToString()
                    + @" по " + m_panelManagement.m_dtRange.End.ToString();
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected override void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            //int id_par = (int)(pars as object [])[0]
            //    , id_comp = (int)(pars as object[])[1]
            //    , idParameter = (int)(pars as object[])[2];
            //double val = (double)(pars as object[])[3];

            DataRow[] rowsParameter = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_INPUT=" + ev.m_IdParameter);

            if (rowsParameter.Length == 1)
            {
                rowsParameter[0][@"VALUE"] = ev.m_Value;
            }
            else
                ;
        }
    }

    public class PlugIn : PlugInTepTaskValues
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
