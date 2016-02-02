using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public partial class HandlerDbTaskCalculate : HHandlerDb
    {
        private class TaskTepCalculate : Object
        {
            public enum INDEX_DATATABLE : short { UNKNOWN = -1
                , FTABLE
                , IN_ALG, IN_PUT, IN_VALUES
                , OUT_NORM_ALG, OUT_NORM_PUT, OUT_NORM_VALUES
                , COUNT }

            const int BL1 = 1029
                    , BL2 = 1030
                    , BL3 = 1031
                    , BL4 = 1032
                    , BL5 = 1033
                    , BL6 = 1034
                    , ST = 5;

            FTable fTable;

            public struct DATATABLE
            {
                public INDEX_DATATABLE m_indx;
                public DataTable m_table;
            }

            private class P_ALG : Dictionary <string, Dictionary<int, P_ALG.P_PUT>>
            {                
                public string m_strId;
                public int m_iId;
                public bool m_bDeny;
                
                public class P_PUT
                {
                    public int m_iId;
                    public int m_iIdComponent;
                    public bool m_bDeny;

                    public float m_fValue;
                }
            }

            P_ALG In;
            P_ALG Norm;
            P_ALG Mkt;

            public TaskTepCalculate()
            {
                In = new P_ALG();
                Norm = new P_ALG();
                Mkt = new P_ALG();

                fTable = new FTable();
            }

            private void initValues(DATATABLE []arDataTables)
            {
                foreach (DATATABLE dataTable in arDataTables)
                    switch (dataTable.m_indx)
                    {
                        case INDEX_DATATABLE.FTABLE:
                            fTable.Set (dataTable.m_table);
                            break;
                        case INDEX_DATATABLE.IN_VALUES:
                            initInValues(dataTable.m_table);
                            break;
                        default:
                            break;
                    }
            }

            private void initInValues(DataTable table)
            {
            }

            private void initNormValues(DataTable table)
            {
            }

            private void initMktValues(DataTable table)
            {
            }

            public DataTable CalculateNormative(DATATABLE []arDataTables)
            {
                DataTable tableRes = new DataTable();

                initValues(arDataTables);

                if (Norm[@"1"][BL1].m_bDeny == false)
                    Norm[@"1"][BL1].m_fValue = In[@"1"][BL4].m_fValue + In[@"2"][BL6].m_fValue * fTable.Calculate(@"2.4", FTable.FRUNK.F2, new float[] { In[@"1"][BL1].m_fValue, Norm[@"1"][BL2].m_fValue });
                else
                    ;

                if (Mkt[@"1"][BL2].m_bDeny == false)
                    Mkt[@"1"][BL2].m_fValue = Norm[@"1"][ST].m_fValue + In[@"2"][BL2].m_fValue;
                else
                    ;

                return tableRes;
            }

            public DataTable CalculateMaket(DATATABLE[] arDataTables)
            {
                DataTable tableRes = new DataTable();

                initValues(arDataTables);

                return tableRes;
            }
        }
    }
}
