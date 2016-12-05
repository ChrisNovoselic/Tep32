using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GemBox.Spreadsheet;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;

using HClassLibrary;
using System.Data.OleDb;
using System.IO;
using System.Data.Odbc;

namespace PluginTaskTepMain
{
    class ImpExpPrevVersionValues : IDisposable // 'IDisposable' для using(...)
    {
        private static OpenFileDialog m_openDlg;

        private static List<LINK_ID_to_NUMCOLUMN> m_listLinkIdToNumColumn;

        //public ImpExpPrevVersionValues(IWin32Window owner = null)
        //{
        //    //_owner = owner;
        //}

        private struct LINK_ID_to_NUMCOLUMN
        {
            public int id;
            public int iNumColumn;
        }

        private static void initialize()
        {
        }

        public static DataTable Import(TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type
            , long idSession
            , Int16 quality
            , DataTable tablePrjPars
            , out int err)
        {
            err = 0;

            DataTable tableRes = null;

            switch (type)
            {
                case TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES:
                    tableRes = importInValues(idSession, quality, tablePrjPars, out err);
                    break;
                default:
                    tableRes = new DataTable();

                    Logging.Logg().Error(@"ImpExpPrevVersionValues::Import () - неизвестный тип импортируемых значений...", Logging.INDEX_MESSAGE.NOT_SET);
                    break;
            }

            return tableRes;
        }

        private static DataTable importInValues(long idSession
            , Int16 quality
            , DataTable tablePrjPars
            , out int err)
        {
            err = 0;

            List <LINK_ID_to_NUMCOLUMN> listLinkIdToNumColumn = new List<LINK_ID_to_NUMCOLUMN>
            {
                new LINK_ID_to_NUMCOLUMN() { id = 1029, iNumColumn = 4 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 1030, iNumColumn = 5 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 1031, iNumColumn = 6 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 1032, iNumColumn = 7 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 1033, iNumColumn = 8 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 1034, iNumColumn = 9 }
                , new LINK_ID_to_NUMCOLUMN()  { id = 5, iNumColumn = 10 }
            };

            OleDbCommand cmd;
            OleDbDataReader reader;
            DataTable tableRes = null;
            string nAlg = string.Empty;
            int idComp = -1, numColumnComp = -1
                , cntRowValues = -1;
            object[] vals = null;
            double val = -1F;
            DataRow[] arSel = null; // массив строк при поиске проектных данных по номеру алгоритма

            try {
                tableRes = new DataTable();
                tableRes.Columns.AddRange(new DataColumn[] {
                    new DataColumn ("ID_PUT", typeof(Int32))
                    , new DataColumn ("ID_SESSION", typeof(long))
                    , new DataColumn ("QUALITY", typeof(Int16))
                    , new DataColumn ("VALUE", typeof(double))                    
                    //, new DataColumn ("AVG", typeof(Int16))
                    , new DataColumn ("WR_DATETIME", typeof(DateTime))
                    , new DataColumn ("EXTENDED_DEFINITION", typeof(Int32))
                });

                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.CheckPathExists =
                    openFileDialog.CheckFileExists =
                    openFileDialog.ShowReadOnly =
                    openFileDialog.ShowHelp =
                    openFileDialog.RestoreDirectory =
                        true;
                    openFileDialog.Multiselect = false;

                    openFileDialog.Title = "Выберите файл для импорта значений";
                    openFileDialog.InitialDirectory = @"D:\TEPW";
                    openFileDialog.DefaultExt = "xls";
                    openFileDialog.Filter = "dBase(Расчет ТЭП)-файлы (*.dbf)|*.dbf" + @"|"
                        + "MS Excel(2003)-файлы (*.xls)|*.xls";
                    openFileDialog.FilterIndex = 0;

                    if (openFileDialog.ShowDialog(Control.FromHandle(Process.GetCurrentProcess().MainWindowHandle)) == DialogResult.OK) {
                        using (OleDbConnection conn = new OleDbConnection(
                            //ConnectionSettings.GetConnectionStringDBF(Path.GetDirectoryName(openFileDialog.FileName))
                            string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=dBASE III;", Path.GetDirectoryName(openFileDialog.FileName))
                            ))
                        {
                            conn.Open();

                            cmd = new OleDbCommand("SELECT * FROM inblok");
                            cmd.Connection = conn;
                            reader = cmd.ExecuteReader();

                            vals = new object[reader.FieldCount];
                            while (reader.Read() == true) {
                                cntRowValues = reader.GetValues(vals);

                                nAlg = ((string)vals[0]).Trim();
                                arSel = tablePrjPars.Select(string.Format(@"N_ALG='{0}'", nAlg)); //0 - индекс поля 'ORDER' ('N_ALG')

                                try {
                                    foreach (DataRow r in arSel) {                                    
                                        idComp = Int32.Parse(r[@"ID_COMP"].ToString().Trim());
                                        numColumnComp = listLinkIdToNumColumn.Find(item => { return item.id == idComp; }).iNumColumn;

                                        val = HMath.doubleParse((string)vals[numColumnComp]);
                                        //if (Double.TryParse(((string)vals[numColumnComp]).Trim(), out val) == true)
                                        if (double.IsNaN(val) == false)
                                            tableRes.Rows.Add(new object[] {
                                                r[@"ID"] //ID_PUT
                                                , idSession //ID_SESSION - значение будет изменено при обработке таблицы
                                                , quality //QUALITY
                                                , val //VALUE
                                                //, r[@"AVG"] //AVG
                                                , DateTime.MinValue //WR_DATETIME - значение будет изменено при обработке таблицы
                                                , -1 //EXTENDED_DEFINITION
                                            });
                                        else
                                            ;
                                    }
                                } catch (Exception e) {
                                    Logging.Logg().Exception(e
                                        , string.Format(@"ImpExpPrevVersionValues::importInValues () - обработка значения для nAlg=", nAlg)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                }
                            }

                            conn.Close();
                        }
                    }
                    else
                        ;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"ImpExpPrevVersionValues::importInValues () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return tableRes == null ? new DataTable() : tableRes;
        }

        public static void Export(out int err)
        {
            err = 0;
        }

        public void Dispose()
        {
        }
    }
}
