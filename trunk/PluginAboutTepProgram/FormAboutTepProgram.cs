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

            this.FormClosing += new FormClosingEventHandler(FormAboutTepProgram_FormClosing);
        }

        void FormAboutTepProgram_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            this.Hide ();
        }

        private void m_btnOk_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }

    public class PlugIn : HFunc
    {
        const string _nameOwnerMenuItem = @"Помощь"
            , _nameMenuItem = @"О программе";
        
        public PlugIn () : base () {            
            _Id = 1;
        }
        
        protected override void createObject () {
            if (_object == null)
                _object = new FormAboutTepProgram (this);
            else
                ;
        }

        public override string NameOwnerMenuItem
        {
            get {
                return _nameOwnerMenuItem;
            }
        }

        public override string NameMenuItem
        {
            get
            {
                return _nameMenuItem;
            }
        }

        public override void OnClickMenuItem(object obj, EventArgs ev)
        {
            createObject ();

            DataAskedHost (666);

            ((FormAboutTepProgram)_object).ShowDialog (Host as IWin32Window);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
