using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;



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

            try {
                ASUTP.Helper.ProgramBase.Start(ASUTP.Logging.LOG_MODE.FILE_EXE, true); }
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
                    MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ASUTP.Helper.ProgramBase.MessageAppAbort, strHeader);
                    ASUTP.Logging.Logg().Exception(e, strHeader, ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(formMain == null))
                    try { Application.Run(formMain); }
                    catch (Exception e)
                    {
                        strHeader = "Ошибка выполнения приложения";
                        MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ASUTP.Helper.ProgramBase.MessageAppAbort, strHeader);
                        ASUTP.Logging.Logg().Exception(e, strHeader, ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else
                    ;

                ASUTP.Helper.ProgramBase.Exit();
            }
            else
                ;
        }
    }
}
