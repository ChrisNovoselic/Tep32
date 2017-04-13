using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskTepForm15506
{
    public class PanelTaskTepForm15506 : HPanelTepCommon
    {
        public PanelTaskTepForm15506(IPlugIn iFunc)
            : base(iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES | HandlerDbTaskCalculate.TaskCalculate.TYPE.OUT_VALUES)
        {
            InitializeComponent();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent() { }

        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
        }
        /// <summary>
        /// Создать объект для взаимодействия с БД
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override HandlerDbValues createHandlerDb()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Создать панель с активными элементами управления
        /// </summary>
        /// <returns>Панель управления</returns>
        protected override PanelManagementTaskCalculate createPanelManagement()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Обработчик события - изменение значения в одном из базовых активных элементов на панели управления
        /// </summary>
        /// <param name="obj">Аргумент события</param>
        protected override void panelManagement_EventIndexControlBase_onValueChanged(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onAddNAlgParameter(HandlerDbTaskCalculate.NALG_PARAMETER obj)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onAddPutParameter(HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onAddComponent(HandlerDbTaskCalculate.TECComponent comp)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Обработчик события - изменение состояния элемента 'CheckedListBox'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события, описывающий состояние элемента</param>
        protected override void panelManagement_onItemCheck(HPanelTepCommon.PanelManagementTaskCalculate.ItemCheckedParametersEventArgs ev)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onSetValuesCompleted(HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateCompleted(HandlerDbTaskCalculate.RESULT res)
        {
            throw new NotImplementedException();
        }

        protected override void handlerDbTaskCalculate_onCalculateProcess(object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 25;
            register(25, typeof(PanelTaskTepForm15506), @"Задача\Расчет ТЭП", @"Форма 15506");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}

