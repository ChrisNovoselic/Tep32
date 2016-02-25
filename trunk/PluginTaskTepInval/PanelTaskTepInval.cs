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
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для связи с вызывающим приложением</param>
        public PanelTaskTepInval(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
        {
            m_arTableOrigin = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            m_arTableEdit = new DataTable[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT];
            
            InitializeComponent();

            (Controls.Find (INDEX_CONTROL.BUTTON_RUN_PREV.ToString(), true)[0] as Button).Click += new EventHandler (btnRunPrev_onClick);
            (Controls.Find(INDEX_CONTROL.BUTTON_RUN_RES.ToString(), true)[0] as Button).Click += new EventHandler(btnRunRes_onClick);
        }

        private void InitializeComponent()
        {
        }

        protected override System.Data.DataTable m_TableOrigin
        {
            get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableOrigin[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }

        protected override System.Data.DataTable m_TableEdit
        {
            get { return m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]; }

            //set { m_arTableEdit[(int)INDEX_TABLE_VALUES.SESSION] = value.Copy(); }
        }
        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            //m_handlerDb.RecUpdateInsertDelete(HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES]
            //    , @"ID_PUT, ID_TIME"
            //    , m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]
            //    , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE]
            //    , out err);

            m_handlerDb.RecUpdateInsertDelete(HandlerDbTaskCalculate.s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVAL_DEF]
                , @"ID_PUT, ID_TIME"
                , m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT]
                , out err);
        }
        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] =
                m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT].Copy();
        }
        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
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
                _IdSession = HMath.GetRandomNumber();
                //Запрос для получения архивных данных
                m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.ARCHIVE] = new DataTable();
                //Запрос для получения автоматически собираемых данных
                m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION] = HandlerDb.GetValuesVar(Type
                    , _IdSession //!!! для записи
                    , ActualIdPeriod
                    , CountBasePeriod
                    , arQueryRanges
                    , out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                {
                    //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                    m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT] = HandlerDb.GetValuesDef(ActualIdPeriod, out err);
                    //Проверить признак выполнения запроса
                    if (err == 0)
                    {
                        //Начать новую сессию расчета
                        // , получить входные для расчета значения для возможности редактирования
                        HandlerDb.CreateSession(_IdSession
                            , _currIdPeriod
                            , CountBasePeriod
                            , _currIdTimezone
                            , m_arTableDictPrjs[(int)INDEX_TABLE_DICTPRJ.PARAMETER]
                            , ref m_arTableOrigin
                            , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                            , out err, out strErr);
                        // создать копии для возможности сохранения изменений
                        for (HandlerDbTaskCalculate.INDEX_TABLE_VALUES indx = (HandlerDbTaskCalculate.INDEX_TABLE_VALUES.UNKNOWN + 1);
                            indx < HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT;
                            indx ++)
                            m_arTableEdit[(int)indx] =
                                m_arTableOrigin[(int)indx].Copy();
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
            DataRow[] rowsParameter = null;

            for (HandlerDbTaskCalculate.INDEX_TABLE_VALUES indx = (HandlerDbTaskCalculate.INDEX_TABLE_VALUES.UNKNOWN + 1);
                indx < HandlerDbTaskCalculate.INDEX_TABLE_VALUES.COUNT;
                indx++)
                if (!(indx == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT)
                    || ((indx == HandlerDbTaskCalculate.INDEX_TABLE_VALUES.DEFAULT) && (ev.m_iQuality == HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT)))
                {
                    if ((!(m_arTableEdit[(int)indx] == null))
                        && (m_arTableEdit[(int)indx].Columns.Contains(@"ID_PUT") == true))
                    {
                        rowsParameter = m_arTableEdit[(int)indx].Select(@"ID_PUT=" + ev.m_IdParameter);

                        if (rowsParameter.Length == 1)
                        {
                            rowsParameter[0][@"VALUE"] = ev.m_Value;
                            //rowsParameter[0][@"QUALITY"] = (int)HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER;
                        }
                        else
                            Logging.Logg().Error(@"PanelTaskInval::onEventCellValueChanged (INDEX_TABLE_VALUES=" + indx.ToString() + @") - не найден параметр при изменении значения (по умолчанию) в 'DataGridView' ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                        ; //??? ошибка - таблица не инициализирована ИЛИ таблица не содержит столбец 'ID_PUT'
                }
                else
                    ;
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки "Предварительное действие - К нормативу"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnRunPrev_onClick(object obj, EventArgs ev)
        {
            btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES);
        }
        /// <summary>
        /// Обработчик события - нажатие кнопки "Результирующее действие - К макету"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnRunRes_onClick(object obj, EventArgs ev)
        {
            btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);            
        }
        /// <summary>
        /// Инициировать подготовку к расчету
        ///  , выполнить расчет
        ///  , актуализировать таблицы с временными значениями
        /// </summary>
        /// <param name="type">Тип требуемого расчета</param>
        private void btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            int err = -1;

            try
            {
                HandlerDb.UpdateSession(INDEX_DBTABLE_NAME.INVALUES
                    , m_arTableOrigin[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                    , m_arTableEdit[(int)HandlerDbTaskCalculate.INDEX_TABLE_VALUES.SESSION]
                    , out err);

                HandlerDb.Calculate (type);
            }
            catch (Exception e)
            {
                //deleteSession ();

                Logging.Logg().Exception(e, @"PanelTaskTepInval::btnRun_onClick (type=" + type.ToString () + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            finally
            {
                //??? сообщение пользователю
            }
        }
        /// <summary>
        /// Создать объект - панель с управляющими элементами управления
        /// </summary>
        /// <returns></returns>
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
            register(17, typeof(PanelTaskTepInval), @"Задача\Расчет ТЭП", @"Входные данные");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
