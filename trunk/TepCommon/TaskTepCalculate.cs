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
                , SESSION
                , IN_ALG, IN_PUT, IN_VALUES
                , OUT_NORM_ALG, OUT_NORM_PUT, OUT_NORM_VALUES
                , COUNT }

            public struct DATATABLE
            {
                INDEX_DATATABLE m_indx;
                DataTable m_table;
            }
            
            public TaskTepCalculate()
            {
            }

            public void CalculateNormative(DATATABLE []arDataTables)
            {
            }

            public void CalculateMaket(DATATABLE[] arDataTables)
            {
            }
        }
    }
}
