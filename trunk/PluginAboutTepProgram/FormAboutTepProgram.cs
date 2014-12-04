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

        public void UpdateGUI (object obj) {
            if (this.InvokeRequired == true)
                this.BeginInvoke (new DelegateObjectFunc (updateGUI), obj);
            else
                ;
        }

        private void updateGUI  (object obj) {
            switch (((EventArgsDataHost)obj).id)
            {
                case (int)HFunc.ID_DATAASKED_HOST.ICON_MAINFORM:
                    this.Icon = (Icon)((EventArgsDataHost)obj).par;
                    this.m_pictureBox.Image = this.Icon.ToBitmap ();
                    break;
                case (int)HFunc.ID_DATAASKED_HOST.STR_VERSION:
                    this.m_lblVersion.Text = (string)((EventArgsDataHost)obj).par;
                    break;
                default:
                    break;
            }
        }
    }

    public class PlugIn : HFunc
    {
        const string _nameOwnerMenuItem = @"Помощь"
            , _nameMenuItem = @"О программе";
        
        public PlugIn () : base () {            
            _Id = 1;
        }
        
        /// <summary>
        /// Создание объекта(объектов) библиотеки
        /// </summary>
        /// <returns>признак создания</returns>
        protected override bool createObject () {
            bool bRes = false;
            
            if (_object == null) {
                _object = new FormAboutTepProgram (this);

                bRes = true; //Объект только создан
            }
            else
                ;

            return bRes;
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
            if (createObject () == true) {
                DataAskedHost ((int)ID_DATAASKED_HOST.ICON_MAINFORM);
                DataAskedHost((int)ID_DATAASKED_HOST.STR_VERSION);
            } else
                ;

            ((FormAboutTepProgram)_object).ShowDialog (Host as IWin32Window);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            //throw new NotImplementedException();

            switch (((EventArgsDataHost)obj).id) {
                case (int)ID_DATAASKED_HOST.ICON_MAINFORM:
                case (int)ID_DATAASKED_HOST.STR_VERSION:
                    ((FormAboutTepProgram)_object).UpdateGUI(obj);
                    break;
                default:
                    break;
            }
        }
    }
}
