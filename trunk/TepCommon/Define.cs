using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    public enum ID_START_RECORD : uint { ALG = 10001, PUT = 20001 }

    public enum CONN_SETT_TYPE
    {
        MAIN_DB,
        COUNT_CONN_SETT_TYPE,
    };

    public enum ID_PERIOD
    {
        HOUR = 13
        /*, SHIFTS = 18*/
        , DAY = 19
        , MONTH = 24
        , YEAR = 100
        ,
    }

    public enum ID_TIMEZONE
    {
        UTC = 0
        , MSK = 1
        , NSK = 2
    }
}
