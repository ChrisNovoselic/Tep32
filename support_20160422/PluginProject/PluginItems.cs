using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PlugIn : HFuncDbEdit
    {
        public PlugIn()
            : base()
        {
            _Id = 5;

            register(5, typeof(PanelPrjRolesUnit), @"Проект", @"Роли(группы) пользователей");
            register(7, typeof(PanelPrjRolesFPanels), @"Проект\Права доступа", @"Роли (группы)");
            register(9, typeof(PanelPrjTask), @"Проект", @"Список задач ИРС");
            register(11, typeof(PanelPrjRolesProfiles), @"Проект\Права доступа", @"Элементы интерфейса");
            register(14, typeof(PanelPrjSourceGroup), @"Проект", @"Группы источников данных");
            register(15, typeof(PanelPrjSources), @"Проект", @"Список источников данных");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            base.OnClickMenuItem(obj, ev);
        }
    }
}
