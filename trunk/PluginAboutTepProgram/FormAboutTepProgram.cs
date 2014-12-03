using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginAboutTepProgram
{
    public partial class FormAboutTepProgram : Form
    {
        IPlugIn _iFuncPlugin;

        public FormAboutTepProgram(IPlugIn iFunc)
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;
        }
    }

    public class PlugIn : HFunc
    {
        public override object Object {
            get {
                if (_object == null)
                    _object = new FormAboutTepProgram (this);
                else
                    ;

                return _object;
            }
        }

        public override string NameOwnerMenuItem
        {
            get {
                return @"О программе";
            }
        }
    }
}
