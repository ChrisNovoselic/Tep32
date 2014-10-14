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
        private static FormParameters s_formParameters;
        
        public FormMain()
        {
            InitializeComponent();

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);
        }

        protected override void HideGraphicsSettings() { }
        protected override void UpdateActiveGui() { }

        /// <summary>
        /// Обработчик выбора пункта меню 'Файл - выход'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void бДКонфигурацииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iRes = connectionSettings (CONN_SETT_TYPE.CONFIG_DB);
        }

        private void бДИсточникиДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((s_listFormConnectionSettings == null) ||
                (!((int)CONN_SETT_TYPE.LIST_SOURCE < s_listFormConnectionSettings.Count)) ||
                (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.LIST_SOURCE] == null))
                    Abort(@"Невозможно отобразить окно для редактирования параметров соединения с источниками данных", false);
            else {
                DialogResult result = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.LIST_SOURCE].ShowDialog(this);
            }
        }

        private void параметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_formParameters == null) {
                Abort (@"Не создано окно для редактирования параметров приложения", false);
            } else {
                DialogResult res = s_formParameters.ShowDialog (this);
                if (res == System.Windows.Forms.DialogResult.OK) {
                } else {
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int iRes = -1;

            //ProgramBase.s_iAppID = Resources.Properties.AppID;
            
            if (!(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0))
            {
                //Есть вызов 'Initialize...'
                iRes = connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
            } else {
                string msg = string.Empty;
                iRes = Initialize (out msg);

                if (! (iRes == 0)) {
                    Abort (msg, false);
                } else {
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop ();
        }

        private int Initialize (out string strErr) {
            int iRes = 0;
            strErr = string.Empty;

            s_formParameters = new FormParameters_DB(s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ());

            if (iRes == 0)
                Start ();
            else {
                s_formParameters = null;
            }

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

                iRes = s_listFormConnectionSettings[(int)type].Ready;

                string msg = string.Empty;
                if (iRes == 0)
                {
                    iRes = Initialize(out msg);
                }
                else
                {
                    msg = @"Параметры соединения с БД конфигурации не корректны";
                }

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
        {//m_timer.Interval == ProgramBase.TIMER_START_INTERVAL
        }
    }
}
