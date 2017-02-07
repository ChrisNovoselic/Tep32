using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTaskEJournal
{
    public class PanelTaskEJournal : HPanelTepCommon
    {
        public PanelTaskEJournal(IPlugIn iFunc)
            : base(iFunc)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        protected override void initialize(out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;
        }

        protected override void recUpdateInsertDelete(out int err)
        {
            throw new NotImplementedException();
        }

        protected override void successRecUpdateInsertDelete()
        {
            throw new NotImplementedException();
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
        protected override void panelManagement_OnEventBaseValueChanged(object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 20;
            register(20, typeof(PanelTaskEJournal), @"Задача", @"Электронный журнал");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}