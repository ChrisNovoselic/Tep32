using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginTepDictTime
{
    public class PanelTepDictTime : HPanelEdit
    {
        IPlugIn _iFuncPlugin;

        public PanelTepDictTime(IPlugIn iFunc)
            : base(@"time")
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }

        private void InitializeComponent()
        {
        }
    }

    public class PlugIn : HFunc
    {
        public PlugIn()
            : base()
        {
            _Id = 3;

            _nameOwnerMenuItem = @"Настройка";
            _nameMenuItem = @"Интервалы времени";
        }
    }
}
