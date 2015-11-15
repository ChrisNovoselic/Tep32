using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;

namespace Tep64
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int iRes = 0;

            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            try { ProgramBase.Start(); }
            catch (Exception e)
            {
                //MessageBox.Show(null, @"Возможно, повторный запуск приложения" + @".\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                MessageBox.Show(null, e.Message + "\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                iRes = -1;
            }

            FormMain formMain = null;

            if (iRes == 0)
            {
                string strHeader = string.Empty;
                try { formMain = new FormMain(); }
                catch (Exception e)
                {
                    strHeader = "Ошибка запуска приложения";
                    MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                    Logging.Logg().Exception(e, strHeader, Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(formMain == null))
                    try { Application.Run(formMain); }
                    catch (Exception e)
                    {
                        strHeader = "Ошибка выполнения приложения";
                        MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                        Logging.Logg().Exception(e, strHeader, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else
                    ;

                ProgramBase.Exit();
            }
            else
                ;
        }
    }
}
