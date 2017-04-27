using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;
using System.Globalization;
using static TepCommon.HandlerDbTaskCalculate;

namespace TepCommon
{
    public abstract partial class HPanelTepCommon : HPanelCommon
    {
        /// <summary>
        /// Перечисление - режимы работы вкладки
        /// </summary>
        protected enum MODE_CORRECT : int { UNKNOWN = -1, DISABLE, ENABLE, COUNT }
        /// <summary>
        /// Тип(ы) расчетов, выполняемых на вкладке (м.б. установлены смешанные)
        /// </summary>
        protected TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE TaskCalculateType;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="plugIn">Объект для связи с вызывающей программой</param>
        /// <param name="type">Тип(ы) расчетов, выполняемых на вкладке</param>
        public HPanelTepCommon(IPlugIn plugIn, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base (plugIn)
        {
            TaskCalculateType = type;            

            _handlerDb.EventAddNAlgParameter += new Action<NALG_PARAMETER>(handlerDbTaskCalculate_onAddNAlgParameter);
            _handlerDb.EventAddPutParameter += new Action<PUT_PARAMETER>(handlerDbTaskCalculate_onAddPutParameter);
            _handlerDb.EventAddComponent += new Action<TECComponent>(handlerDbTaskCalculate_onAddComponent);

            _handlerDb.EventSetValuesCompleted += new Action<RESULT>(handlerDbTaskCalculate_onSetValuesCompleted);
            _handlerDb.EventCalculateCompleted += new Action<RESULT>(handlerDbTaskCalculate_onCalculateCompleted);
            _handlerDb.EventCalculateProccess += new Action<CalculateProccessEventArgs>(handlerDbTaskCalculate_onCalculateProcess);
        }
        /// <summary>
        /// Поле
        /// </summary>
        private PanelManagementTaskCalculate __panelManagement;
        /// <summary>
        /// Свойство для обращения к панели управления
        ///  для автоматического назначения обработчиков событий
        ///  для реализации шаблона Singleton
        /// </summary>
        protected PanelManagementTaskCalculate _panelManagement
        {
            get { return __panelManagement; }

            set {
                if (__panelManagement == null) {
                    __panelManagement = value;
                    // обработчик события при изменении значений в основных элементах управления
                    __panelManagement.EventIndexControlBaseValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlBase_onValueChanged);
                    // обработчик события при изменении значений в дополнительных(добавленных программистом в наследуемых классах) элементах управления
                    __panelManagement.EventIndexControlCustomValueChanged += new DelegateObjectFunc(panelManagement_EventIndexControlCustom_onValueChanged);

                    __panelManagement.ItemCheck += new PanelManagementTaskCalculate.ItemCheckedParametersEventHandler(panelManagement_onItemCheck);
                } else
                    throw new Exception(string.Format(@"HPanelTepCommon._panelManagement::set () - повторное создание панели управления..."));
            }
        }
        /// <summary>
        /// Признак необходимости загрузки из БД входных значений
        /// </summary>
        public bool IsInParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES);
            }
        }
        /// <summary>
        /// Признак необходимости загрузки из БД выходных значений
        /// </summary>
        public bool IsOutParameters
        {
            get {
                return ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_NORM_VALUES)
                    || ((TaskCalculateType & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_TEP_REALTIME);
            }
        }
        /// <summary>
        /// Класс для описания аргумента события на панели управления, связанное с основными элементами управления
        ///  : период расчета, часовой пояс, диапазон для расчета
        /// </summary>
        public class EventIndexControlBaseValueChangedArgs : EventArgs
        {
        }
        /// <summary>
        /// Метод для создания объекта панели управления
        /// </summary>
        /// <returns></returns>
        protected abstract PanelManagementTaskCalculate createPanelManagement();        
        /// <summary>
        /// Значения параметров сессии
        /// </summary>
        protected HandlerDbTaskCalculate.SESSION Session { get { return (__handlerDb as HandlerDbTaskCalculate)._Session; } }
        /// <summary>
        /// Отправить сообщение главной форме для отображения в строке статуса
        /// </summary>
        /// <param name="res">Результат выполнения операции</param>
        /// <param name="message"></param>
        protected void dataAskedHostMessageToStatusStrip(RESULT res, string message)
        {
            (_iFuncPlugin as HFuncDbEdit).DataAskedHost(new object[] { m_Id
                , (int)HFunc.ID_FUNC_DATA_ASKED_HOST.MESSAGE_TO_STATUSSTRIP // par[0] должен быть 'IsPrimitive'
                , res == HandlerDbTaskCalculate.RESULT.Exception ? TYPE_MESSAGE.EXCEPTION
                    : res == HandlerDbTaskCalculate.RESULT.Error ? TYPE_MESSAGE.ERROR
                        : res == HandlerDbTaskCalculate.RESULT.Warning ? TYPE_MESSAGE.WARNING
                            : res == HandlerDbTaskCalculate.RESULT.Ok ? TYPE_MESSAGE.ACTION
                                : res == HandlerDbTaskCalculate.RESULT.Debug ? TYPE_MESSAGE.DEBUG
                                    : TYPE_MESSAGE.UNKNOWN
                , message });
        }
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected override void clear(bool bClose = false)
        {
            if (bClose == true)
                _panelManagement.Clear();
            else
                ;

            _handlerDb.Clear();          

            base.clear();
        }
        /// <summary>
        /// Обработчик события при изменении значений в основных элементах управления на панели упарвления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected virtual void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            if ((obj == null)
                || ((!(obj == null))
                    && ((ID_DBTABLE)obj == ID_DBTABLE.UNKNOWN))) {
            // изменен DateTimeRange
                //??? перед очисткой или после (не требуются ли предыдущий диапазон даты/времени)
                Session.SetDatetimeRange(_panelManagement.DatetimeRange);

                if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok)
                    panelManagement_DatetimeRange_onChanged();
                else
                    ;
            } else {
            // изменены PERIOD или TIMEZONE
                Session.SetDatetimeRange(_panelManagement.DatetimeRange);

                switch ((ID_DBTABLE)obj) {
                    case ID_DBTABLE.TIME:
                        Session.CurrentIdPeriod = _panelManagement.IdPeriod;

                        if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok) {
                            panelManagement_Period_onChanged();
                        } else
                            ;
                        break;
                    case ID_DBTABLE.TIMEZONE:
                        Session.CurrentIdTimezone = _panelManagement.IdTimezone;
                            //, (int)m_dictTableDictPrj[ID_DBTABLE.TIMEZONE].Select(@"ID=" + (int)_panelManagement.IdTimezone)[0][@"OFFSET_UTC"]);                        

                        if (_panelManagement.Ready == PanelManagementTaskCalculate.READY.Ok) {
                            panelManagement_TimezoneChanged();
                        } else
                            ;
                        break;
                    default:
                        throw new Exception(string.Format(@"HPanelTepCommon::panelManagement_EventIndexControlBase_onValueChanged () - {} неизвестный тип события...", obj));
                        //break;
                }
            }

            //// очистить содержание представления
            //clear();
            ////// при наличии признака - загрузить/отобразить значения из БД
            ////if (s_bAutoUpdateValues == true)
            ////    updateDataValues();
            ////else ;
        }
        /// <summary>
        /// Обработчик события при изменении значений в дополнительных элементах управления на панели упарвления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected virtual void panelManagement_EventIndexControlCustom_onValueChanged(object obj) { }        
        /// <summary>
        /// Обработчик события на панели управления - изменение признака выбора снятия/постановки на отображение элемента
        ///  , включения/выключения из расчета элемента
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        protected abstract void panelManagement_onItemCheck(PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev);
        /// <summary>
        /// Добавить на панель все элементы: параметры в алгоритме расчета, компоненты станции
        /// </summary>
        private void add_all()
        {
            _handlerDb.AddComponents(m_dictProfile);

            if (IsInParameters == true)
                _handlerDb.AddAlgParameters(m_Id, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES, m_dictProfile);
            else
                ;

            if (IsOutParameters == true)
                _handlerDb.AddAlgParameters(m_Id, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES, m_dictProfile);
            else
                ;
        }        
        /// <summary>
        /// Обработчик события - изменение диапазона времени расчета
        /// </summary>
        protected virtual void panelManagement_DatetimeRange_onChanged()
        {
        }
        /// <summary>
        /// Обработчик события при изменении периода расчета
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void panelManagement_Period_onChanged()
        {
            add_all();
        }
        /// <summary>
        /// Обработчик события - изменение часового пояса
        /// </summary>
        protected virtual void panelManagement_TimezoneChanged()
        {
            add_all();
        }
        /// <summary>
        /// Метод по завершению загрузки из БД одного параметра в алгоритме расчета
        /// </summary>
        /// <param name="obj">Параметр в алгоритме расчета</param>
        protected virtual void handlerDbTaskCalculate_onAddNAlgParameter(NALG_PARAMETER obj)
        {
        }
        /// <summary>
        /// Метод по завершению загрузки из БД одного параметра, связанного с компонентом станции, в алгоритме расчета
        /// </summary>
        /// <param name="obj">Параметр в алгоритме расчета, связанный с компонентом станции</param>
        protected virtual void handlerDbTaskCalculate_onAddPutParameter(PUT_PARAMETER obj)
        {
        }
        /// <summary>
        /// Метод по завершению загрузки информации по компоненту станции
        /// </summary>
        /// <param name="comp">Объект, описывающий компонент станции</param>
        protected virtual void handlerDbTaskCalculate_onAddComponent(TECComponent comp)
        {
        }
        /// <summary>
        /// Обраюотчик события - завершение загрузки значений из БД
        /// </summary>
        protected abstract void handlerDbTaskCalculate_onSetValuesCompleted(HandlerDbTaskCalculate.RESULT res);
        /// <summary>
        /// Обраюотчик события - завершение выполнения расчета
        /// </summary>
        protected abstract void handlerDbTaskCalculate_onCalculateCompleted(HandlerDbTaskCalculate.RESULT res);

        protected abstract void handlerDbTaskCalculate_onCalculateProcess(CalculateProccessEventArgs ev);
        /// <summary>
        /// Ссылка на объект для обращения к БД
        /// </summary>
        protected HandlerDbTaskCalculate _handlerDb { get { return __handlerDb as HandlerDbTaskCalculate; } }        

        public override void Stop()
        {
            clear(true);

            base.Stop();
        }

        sealed protected override void recUpdateInsertDelete(out int err)
        {
            err = 0;
        }

        sealed protected override void successRecUpdateInsertDelete()
        {
        }

        protected virtual void dgvValues_onEventCellValueChanged(CHANGE_VALUE change_value)
        {
            _handlerDb.SetValue(change_value);
        }
        /// <summary>
        /// ??? дублирование метода 'HMath::Parse' преобразование числа в нужный формат отображения
        /// </summary>
        /// <param name="value">число</param>
        /// <returns>преобразованное число</returns>
        public static float AsParseToF(string value)
        {
            int _indxChar = 0;
            string _sepReplace = string.Empty;
            bool bFlag = true;
            //char[] _separators = { ' ', ',', '.', ':', '\t'};
            //char[] letters = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToArray();
            float fValue = 0;

            foreach (char item in value.ToCharArray()) {
                if (!char.IsDigit(item))
                    if (char.IsLetter(item))
                        value = value.Remove(_indxChar, 1);
                    else
                        _sepReplace = value.Substring(_indxChar, 1);
                else
                    _indxChar++;

                switch (_sepReplace) {
                    case ".":
                    case ",":
                    case " ":
                    case ":":
                        float.TryParse(value.Replace(_sepReplace, "."), NumberStyles.Float, CultureInfo.InvariantCulture, out fValue);
                        bFlag = false;
                        break;
                }
            }

            if (bFlag)
                try {
                    fValue = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                } catch (Exception e) {
                    if (value.ToString() == "")
                        fValue = 0;
                }


            return fValue;
        }
    }
}
