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
                    this.Icon = (Icon)((EventArgsDataHost)obj).par[0];
                    this.m_pictureBox.Image = this.Icon.ToBitmap ();
                    break;
                case (int)HFunc.ID_DATAASKED_HOST.STR_VERSION:
                    this.m_lblVersion.Text = (string)((EventArgsDataHost)obj).par[0];
                    break;
                default:
                    break;
            }
        }
    }

    public class PlugIn : HFunc
    {      
        public PlugIn () : base () {
            _Id = (int)ID_PLUGIN.ABOUT;
            register((int)ID_MODULE.ABOUT, typeof(FormAboutTepProgram), @"Помощь", @"О программе");
        }

        public override void OnClickMenuItem(object obj, /*PlugInMenuItem*/EventArgs ev)
        {
            int id = -1;
            
            base.OnClickMenuItem(obj, ev);

            id = _Id;
            
            if (m_dictDataHostCounter.ContainsKey ((int)ID_DATAASKED_HOST.ICON_MAINFORM) == false)
                DataAskedHost ((int)ID_DATAASKED_HOST.ICON_MAINFORM);
            else
                ;

            if (m_dictDataHostCounter.ContainsKey((int)ID_DATAASKED_HOST.STR_VERSION) == false)                
                DataAskedHost((int)ID_DATAASKED_HOST.STR_VERSION);
            else
                ;

            (_objects[id] as FormAboutTepProgram).ShowDialog(Host as IWin32Window);
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            int id = -1;

            //throw new NotImplementedException();

            id = _Id;

            base.OnEvtDataRecievedHost(obj);

            switch (((EventArgsDataHost)obj).id) {
                case (int)ID_DATAASKED_HOST.ICON_MAINFORM:
                case (int)ID_DATAASKED_HOST.STR_VERSION:
                    (_objects[id] as FormAboutTepProgram).UpdateGUI(obj);
                    break;
                default:
                    break;
            }
        }
    }
}
