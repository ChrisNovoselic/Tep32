using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GemBox.Spreadsheet;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;

using System.Data.OleDb;
using System.IO;
using System.Data.Odbc;
using ASUTP;

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
            , DataTable tableDictPrj
            , out int err)
        {
            err = 0;

            DataTable tableRes = null;

            if ((type & TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) == TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE.IN_VALUES) {
                tableRes = importInValues(idSession, quality, tablePrjPars, tableDictPrj, out err);
            } else {
                tableRes = new DataTable();

                Logging.Logg().Error(@"ImpExpPrevVersionValues::Import () - неизвестный тип импортируемых значений...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return tableRes;
        }

        private static DataTable importInValues(long idSession
            , Int16 quality
            , DataTable tablePrjPars
            , DataTable tableDictPrjRatio
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
            string nAlg = string.Empty
                , mera = string.Empty;
            int idComp = -1, numColumnComp = -1
                , iNumColumnRatio = -1, ratioValue = -1, dbRatioValue = -1
                , cntRowValues = -1;
            object[] vals = null;
            double val = -1F;
            DataRow[] arSelPrjPars = null // массив строк при поиске проектных данных по номеру алгоритма
                , arSelDictPrjRatio = null;

            try {
                iNumColumnRatio = 3;

                tableRes = HandlerDbTaskTepCalculate.CloneVariableDataTable;

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
                                try
                                {
                                    cntRowValues = reader.GetValues(vals);

                                    nAlg = ((string)vals[0]).Trim();
                                    arSelPrjPars = tablePrjPars.Select(string.Format(@"N_ALG='{0}'", nAlg)); //0 - индекс поля 'ORDER' ('N_ALG')

                                    ratioValue = 0;
                                    mera = vals[iNumColumnRatio] is DBNull == false ? ((string)vals[iNumColumnRatio]).Trim().ToLower() : string.Empty;
                                    arSelDictPrjRatio = new DataRow[] { tableDictPrjRatio.AsEnumerable().ToList().Find(r => {
                                        // только если наименование не пустое И индекс 1-го найденного символа = 0
                                        return ((string.IsNullOrEmpty(((string)r[@"NAME_RU"]).Trim()) == false)
                                            && (string.IsNullOrEmpty(mera) == false)) ? mera.IndexOf(((string)r[@"NAME_RU"]).Trim()) == 0 : false;
                                    }) };
                                    if (!(arSelDictPrjRatio[0] == null))
                                        ratioValue = (int)arSelDictPrjRatio[0]["VALUE"];
                                    else
                                        ;

                                    foreach (DataRow r in arSelPrjPars) {                                    
                                        idComp = Int32.Parse(r[@"ID_COMP"].ToString().Trim());
                                        numColumnComp = listLinkIdToNumColumn.Find(item => { return item.id == idComp; }).iNumColumn;

                                        val = ASUTP.Core.HMath.doubleParse((string)vals[numColumnComp]);

                                        dbRatioValue = 0;
                                        arSelDictPrjRatio = tableDictPrjRatio.Select(string.Format(@"ID ={0}", (int)r[@"ID_RATIO"]));
                                        if (arSelDictPrjRatio.Length > 0)
                                            if (arSelDictPrjRatio.Length == 1)
                                                dbRatioValue = (int)arSelDictPrjRatio[0]["VALUE"];
                                            else
                                                ;
                                        else
                                            ;

                                        // проверить требуется ли преобразование
                                        if (!(ratioValue == dbRatioValue))
                                            // домножить значение на коэффициент
                                            val *= Math.Pow(10F, ratioValue - dbRatioValue);
                                        else
                                            ;

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
                    // отказ пользователя от загрузки значений (файл не был выбран)
                        err = 1;
                }
            } catch (Exception e) {
                err = -1; // неизвестная(общая) ошибка - смотреть лог-файл

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
