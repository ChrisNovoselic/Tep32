using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    [Flags]
    public enum ID_DBTABLE : short { UNKNOWN = -1
        , TIME, TIMEZONE
        , COMP, COMP_LIST
        , MODE_DEV
        , RATIO
        , MEASURE
        , SESSION
        , INALG, INPUT, INVALUES
        , INVAL_DEF // 10
        , OUTALG /*11*/, OUTPUT, OUTVALUES
        //, PERIOD
        , FTABLE
        , PLUGINS
        , TASK
        , FPANELS
        , IN_PARAMETER // сводная таблица для [INALG], [INPUT]
        , OUT_PARAMETER // сводная таблица для [OUTALG], [OUTPUT]
        , ROLES_UNIT
        , USERS
            //, COUNT
    };    

    public enum ID_START_RECORD : uint { ALG = 10001, ALG_NORMATIVE = 15001, PUT = 20001 }
    /// <summary>
    /// Перечисление - идентификаторы задач в составе системы - табл. [task]
    /// </summary>
    public enum ID_TASK : int { UNKNOWN = 0, TEP = 1, BAL_TEPLO, ENG6_GRAF, EL_JUR, VEDOM_BL, AUTOBOOK, REAKTIVKA }

    public enum ID_PLUGIN : int
    {
        UNKNOWN = 0
        , ABOUT = 1
    }

    public enum ID_MODULE : int
    {
        UNKNOWN = 0
        , ABOUT = 1
    }

    public enum CONN_SETT_TYPE
    {
        MAIN_DB,
        COUNT_CONN_SETT_TYPE,
    };

    public enum ID_PERIOD
    {
        UNKNOWN = -1
        , HOUR = 13
        /*, SHIFTS = 18*/
        , DAY = 19
        , MONTH = 24
        , YEAR = 27
        ,
    }

    public enum ID_TIMEZONE
    {
        UNKNOWN = -1
        , UTC = 0
        , MSK = 1
        , NSK = 2
    }
}
