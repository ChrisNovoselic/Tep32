using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; //ToolStripMenuItem

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictPlugIns
{
    public class PanelTepDictPlugIns : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictPlugIns(IPlugIn iFunc)
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }

        private void InitializeComponent () {
        }
    }

    public class PlugIn : HFunc
    {      
        public PlugIn () : base () {            
            _Id = 2;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Состав плюгин'ов";
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            bool bNowCreateObject = createObject (typeof(PanelTepDictPlugIns));
            //if (createObject<PanelTepDictPlugIns>() == true)
            if (bNowCreateObject == true)
            {//При необходимости запросить асихронно данные
                //DataAskedHost((int)...);
            }
            else
                ;

            object par = null; //Параметр для передачи в главную форму
            //Изменить состояние пункта меню
            ((ToolStripMenuItem)obj).Checked = ! ((ToolStripMenuItem)obj).Checked;
            //Проверить состояние пункта меню
            if (((ToolStripMenuItem)obj).Checked == true)
            {
                //Передать главной форме объект плюг'ина
                par = Object;
            } else {
                par = ID_DATAASKED_HOST.HIDE_PLIGIN;
            }

            //Передать главной форме параметр
            DataAskedHost(par);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            //throw new NotImplementedException();

            switch (((EventArgsDataHost)obj).id)
            {
                default:
                    break;
            }
        }
    }
}
