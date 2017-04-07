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
using System.Collections;
using System.Threading;

namespace PluginAboutTepProgram
{
    public partial class FormAboutTepProgram : Form
    {
        IPlugIn _iFuncPlugin;

        public ManualResetEvent m_mnlResetEventReady;

        public FormAboutTepProgram(IPlugIn iFunc)
        {
            InitializeComponent();
            this._iFuncPlugin = iFunc;

            this.m_lblProductVersion.Text = string.Empty;

            this.FormClosing += new FormClosingEventHandler(FormAboutTepProgram_FormClosing);

            m_mnlResetEventReady = new ManualResetEvent(false);
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
                this.BeginInvoke(new DelegateObjectFunc(updateGUI), obj);
            else
                updateGUI(obj);
        }

        private void updateGUI  (object obj) {
            switch (((EventArgsDataHost)obj).id_detail)
            {
                case (int)HFunc.ID_DATAASKED_HOST.ICON_MAINFORM:
                    this.Icon = (Icon)((EventArgsDataHost)obj).par[0];
                    this.m_pictureBox.Image = this.Icon.ToBitmap ();
                    break;
                case (int)HFunc.ID_DATAASKED_HOST.STR_PRODUCTVERSION:
                    this.m_lblProductVersion.Text = (string)((EventArgsDataHost)obj).par[0];
                    break;
                default:
                    break;
            }

            if ((!(this.m_pictureBox.Image == null))
                && (this.m_lblProductVersion.Text.Equals(string.Empty) == false))
                m_mnlResetEventReady.Set();
            else
                ;
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
            KeyValuePair<int, int> pairIconMainForm
                , pairProductVersion
                , pairShowDialog;
            
            base.OnClickMenuItem(obj, ev);

            id = (int)(obj as ToolStripMenuItem).Tag;
            pairIconMainForm = new KeyValuePair<int, int>(id, (int)ID_DATAASKED_HOST.ICON_MAINFORM);
            pairProductVersion = new KeyValuePair<int, int>(id, (int)ID_DATAASKED_HOST.STR_PRODUCTVERSION);
            //pairShowDialog = new KeyValuePair<int, int>(id, (int)ID_DATAASKED_HOST.FORMABOUT_SHOWDIALOG);

            if ((m_dictDataHostCounter.ContainsKey(pairIconMainForm) == true)
                && (m_dictDataHostCounter.ContainsKey(pairProductVersion) == true)) {
                ShowDialog();
            } else {
            // запрашиваем значения
                // пиктограмма
                if (m_dictDataHostCounter.ContainsKey(pairIconMainForm) == false)
                    DataAskedHost(new object[] { pairIconMainForm.Key, (int)pairIconMainForm.Value });
                else
                    ;
                // версия программы
                if (m_dictDataHostCounter.ContainsKey(pairProductVersion) == false)
                    DataAskedHost(new object[] { pairProductVersion.Key, (int)pairProductVersion.Value });
                else
                    ;
                //// запрашиваем отображение
                //if (m_dictDataHostCounter.ContainsKey(pairShowDialog) == false)
                //    DataAskedHost(new object[] { pairShowDialog.Key, (int)pairShowDialog.Value });
                //else
                //    ;

                if (WaitHandle.WaitAny(new WaitHandle[] { (_objects[_Id] as FormAboutTepProgram).m_mnlResetEventReady }) == 0)
                    ShowDialog();
                else
                    ;
            }
        }

        public void ShowDialog()
        {
            (_objects[_Id] as FormAboutTepProgram).ShowDialog();
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            int id = -1;

            //throw new NotImplementedException();

            id = _Id;

            base.OnEvtDataRecievedHost(obj);

            switch (((EventArgsDataHost)obj).id_detail) {
                case (int)ID_DATAASKED_HOST.ICON_MAINFORM:
                case (int)ID_DATAASKED_HOST.STR_PRODUCTVERSION:
                    (_objects[id] as FormAboutTepProgram).UpdateGUI(obj);
                    break;
                //case (int)ID_DATAASKED_HOST.FORMABOUT_SHOWDIALOG:
                //    ShowDialog();
                //    break;
                default:
                    break;
            }
        }
    }
}
