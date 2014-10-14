using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace Tep32
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {        
        public FormMain()
        {
            InitializeComponent();

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
        }

        protected override void HideGraphicsSettings() { }
        protected override void UpdateActiveGui() { }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void бДКонфигурацииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectionSettings (CONN_SETT_TYPE.CONFIG_DB);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {   
            s_listFormConnectionSettings.Add (new FormConnectionSettings (-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            //s_formConnectionSettings.Add (new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            if (!(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0))
            {
                connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
            } else {
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop ();
        }

        private int Initialize (out string strErr) {
            int iRes = 0;
            strErr = string.Empty;

            if (iRes == 0)
                Start ();
            else
                ;

            return iRes;
        }

        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings[(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                //Остановить все вкладки
                //StopTabPages ();

                //Остановить таймер (если есть)
                Stop ();

                string msg = string.Empty;
                iRes = Initialize(out msg);
                if (!(iRes == 0))
                    //@"Ошибка инициализации пользовательских компонентов формы"
                    Abort(msg, false);
                else
                    ;
            }
            else
                ;

            return iRes;
        }

        protected override bool  UpdateStatusString () {
            bool bRes = true;

            return bRes;
        }

        protected override void timer_Start()
        {
        }
    }
}
