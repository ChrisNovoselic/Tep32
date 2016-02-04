using System;
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
            : base(iFunc, HandlerDbTaskCalculate.TYPE.IN_VALUES)
        {
            m_arTableOrigin = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)INDEX_TABLE_VALUES.COUNT];
            
            InitializeComponent();

            (Controls.Find (INDEX_CONTROL.BUTTON_RUN_PREV.ToString(), true)[0] as Button).Click += new EventHandler (btnRunPrev_onClick);
            (Controls.Find(INDEX_CONTROL.BUTTON_RUN_RES.ToString(), true)[0] as Button).Click += new EventHandler(btnRunRes_onClick);
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
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="dbConn">Объект соединения с БД</param>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(ref System.Data.Common.DbConnection dbConn, out int err)
        {
            err = -1;

            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn
                , HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVAL_DEF]
                , @"ID_PUT, ID_TIME"
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
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        protected override void setValues(out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            int iRegDbConn = -1;
            DateTimeRange[] arQueryRanges = getDateTimeRangeValuesVar();

            m_handlerDb.RegisterDbConnection(out iRegDbConn);

            if (!(iRegDbConn < 0))
            {
                //Запрос для получения автоматически собираемых данных
                m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE] = m_handlerDb.GetValuesVar(_IdSession
                    , ActualIdPeriod
                    , CountBasePeriod
                    , m_type
                    , arQueryRanges
                    , out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                {
                    //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                    m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT] = m_handlerDb.GetValuesDef(ActualIdPeriod, out err);
                    //Проверить признак выполнения запроса
                    if (err == 0)
                    {
                        //Начать новую сессию расчета
                        // , получить входные для расчета значения для возможности редактирования
                        m_handlerDb.CreateSession(_IdSession
                            , _currIdPeriod
                            , CountBasePeriod
                            , _currIdTimezone
                            , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
                            , ref m_arTableOrigin[(int)INDEX_TABLE_VALUES.VARIABLE]
                            , ref m_arTableOrigin[(int)INDEX_TABLE_VALUES.DEFAULT]
                            , arQueryRanges
                            , out err, out strErr);
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
            else
                ;

            if (!(iRegDbConn > 0))
                m_handlerDb.UnRegisterDbConnection();
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="dgv">Объект, инициирововший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            DataRow[] rowsParameter = m_arTableEdit[(int)INDEX_TABLE_VALUES.DEFAULT].Select(@"ID_PUT=" + ev.m_IdParameter);

            if (rowsParameter.Length == 1)
            {
                rowsParameter[0][@"VALUE"] = ev.m_Value;
            }
            else
                ;
        }

        private void btnRunPrev_onClick(object obj, EventArgs ev)
        {
            m_handlerDb.TepCalculateNormative();
        }

        private void btnRunRes_onClick(object obj, EventArgs ev)
        {
            m_handlerDb.TepCalculateMaket();
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
