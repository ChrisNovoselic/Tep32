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

namespace PluginTaskTepMain
{
    public class PanelTaskTepInval : PanelTaskTepValues
    {
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для связи с вызывающим приложением</param>
        public PanelTaskTepInval(IPlugIn iFunc)
            : base(iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES)
        {            
            InitializeComponents();
            // назначить обработчики для кнопок 'К нормативу', 'К макету'
            (Controls.Find (INDEX_CONTROL.BUTTON_RUN_PREV.ToString(), true)[0] as Button).Click += new EventHandler (btnRunPrev_onClick);
            // обработчик 'К макету' - см. в базовом классе
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
        }

        //protected override System.Data.DataTable m_TableOrigin
        //{
        //    get { return m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }

        //    //set { m_arTableOrigin[(int)ID_VIEW_VALUES.SOURCE] = value.Copy(); }
        //}

        //protected override System.Data.DataTable m_TableEdit
        //{
        //    get { return m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE]; }

        //    //set { m_arTableEdit[(int)ID_VIEW_VALUES.SOURCE] = value.Copy(); }
        //}

        protected override HandlerDbValues createHandlerDb()
        {
            return new HandlerDbTaskTepCalculate(m_Id);
        }

        /// <summary>
        /// Сохранить изменения в редактируемых таблицах
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении сохранения в БД</param>
        protected override void recUpdateInsertDelete(out int err)
        {
            err = -1;

            //m_handlerDb.RecUpdateInsertDelete(HandlerDbTaskCalculate.s_dictDbTables[(int)INDEX_DBTABLE_NAME.INVALUES]
            //    , @"ID_PUT, ID_TIME"
            //    , m_arTableOrigin[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
            //    , m_arTableEdit[(int)HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE]
            //    , out err);

            m_handlerDb.RecUpdateInsertDelete(HandlerDbTaskCalculate.s_dictDbTables[ID_DBTABLE.INVAL_DEF].m_name
                , @"ID_PUT, ID_TIME"
                , string.Empty
                , m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT]
                , out err);
        }
        /// <summary>
        /// Обработчик события при успешном сохранении изменений в редактируемых на вкладке таблицах
        /// </summary>
        protected override void successRecUpdateInsertDelete()
        {
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] =
                m_arTableEdit[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT].Copy();
        }
        /// <summary>
        /// Освободить (при закрытии), связанные с функционалом ресурсы
        /// </summary>
        public override void Stop()
        {
            //deleteSession();

            base.Stop();
        }
        /// <summary>
        /// Установить значения таблиц для редактирования
        /// </summary>
        /// <param name="err">Идентификатор ошибки при выполнеинии функции</param>
        /// <param name="strErr">Строка текста сообщения при наличии ошибки</param>
        protected override void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        {
            err = 0;
            strErr = string.Empty;

            Session.New();
            //Запрос для получения архивных данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] = new DataTable();
            //Запрос для получения автоматически собираемых данных
            m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE ?
                HandlerDb.GetValuesVar(TaskCalculateType, Session.ActualIdPeriod, Session.CountBasePeriod, arQueryRanges, out err) :
                    Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_IMPORT ? ImpExpPrevVersionValues.Import(TaskCalculateType
                        , Session.m_Id
                        , (int)TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER, m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
                        , m_dictTableDictPrj[ID_DBTABLE.RATIO]
                        , out err) :
                            new DataTable();
            //Проверить признак выполнения запроса
            if (err == 0)
            {
                //Заполнить таблицу данными вводимых вручную (значения по умолчанию)
                m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT] = HandlerDb.GetValuesDef(Session.ActualIdPeriod, out err);
                //Проверить признак выполнения запроса
                if (err == 0)
                    //Начать новую сессию расчета
                    // , получить входные для расчета значения для возможности редактирования
                    HandlerDb.CreateSession(m_Id
                        , Session.CountBasePeriod
                        , m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
                        , ref m_arTableOrigin
                        , new DateTimeRange(arQueryRanges[0].Begin, arQueryRanges[arQueryRanges.Length - 1].End)
                        , out err, out strErr);
                else
                    strErr = @"ошибка получения данных по умолчанию с " + Session.m_rangeDatetime.Begin.ToString()
                        + @" по " + Session.m_rangeDatetime.End.ToString();
            }
            else
                strErr = @"ошибка получения автоматически собираемых данных с " + Session.m_rangeDatetime.Begin.ToString()
                    + @" по " + Session.m_rangeDatetime.End.ToString();            
        }

        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="dgv">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void onEventCellValueChanged(object dgv, DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            DataRow[] rowsParameter = null;

            for (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES indx = (TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.UNKNOWN + 1);
                indx < TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.COUNT;
                indx++)
                if (!(indx == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT)
                    || ((indx == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.DEFAULT) && (ev.m_iQuality == TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.DEFAULT)))
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

        protected override void buttonLoad_onClick()
        {
            // вызов 'reinit()'
            //base.HPanelTepCommon_btnUpdate_Click(obj, ev);
            // для этой вкладки - требуется просто 'clear'
            // очистить содержание представления
            clear();
            // ... - загрузить/отобразить значения из БД
            base.buttonLoad_onClick();
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки "Предварительное действие - К нормативу"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnRunPrev_onClick(object obj, EventArgs ev)
        {
            btnRun_onClick(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES);
        }

        /// <summary>
        /// Обработчик события - нажатие кнопки "Результирующее действие - К макету"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void btnRunRes_onClick(object obj, EventArgs ev)
        {
            btnRun_onClick(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES);
        }

        /// <summary>
        /// Инициировать подготовку к расчету
        ///  , выполнить расчет
        ///  , актуализировать таблицы с временными значениями
        /// </summary>
        /// <param name="type">Тип требуемого расчета</param>
        private void btnRun_onClick(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        {
            int err = -1;

            try
            {
                if ((!(m_TableOrigin == null))
                    && (!(m_TableEdit == null))) {
                    // обновить входные значения для расчета
                    HandlerDb.UpdateSession(ID_DBTABLE.INVALUES
                        , m_TableOrigin
                        , m_TableEdit
                        , out err);
                    // выполнить расчет
                    HandlerDb.Calculate(type);
                } else
                    Logging.Logg().Warning(@"PanelTaskTepInval::btnRun_onClick (type=" + type.ToString() + @") - попытка расчета без загрузки входных данных..."
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
            catch (Exception e)
            {
                //deleteSession ();

                Logging.Logg().Exception(e, @"PanelTaskTepInval::btnRun_onClick (type=" + type.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
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
        protected override PanelTaskTepCalculate.PanelManagementTaskCalculate createPanelManagement()
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
}
