using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    /// <summary>
    /// Перечисление - идентификаторы таблиц БД, объектов на основе таблиц БД, могут также быть использованы как индексы
    /// </summary>
    //[Flags]
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
    /// <summary>
    /// Константы для нумерации новых записей в таблице с проектными данными
    /// </summary>
    public enum ID_START_RECORD : uint { ALG = 10001, ALG_NORMATIVE = 15001, PUT = 20001 }
    /// <summary>
    /// Перечисление - идентификаторы задач в составе системы - табл. [task]
    /// </summary>
    public enum ID_TASK : int { UNKNOWN = 0, TEP = 1, BAL_TEPLO, ENG6_GRAF, EL_JUR, VEDOM_BL, AUTOBOOK, REAKTIVKA }
    /// <summary>
    /// Идентификатор библиотеки
    /// </summary>
    public enum ID_PLUGIN : int
    {
        UNKNOWN = 0
        , ABOUT = 1
    }
    /// <summary>
    /// Идентификатор модуля (форма/подзадача в составе библиотеки)
    /// </summary>
    public enum ID_MODULE : int
    {
        UNKNOWN = 0
            , ABOUT = 1
    }
    /// <summary>
    /// Типы объектов со значениями параметров для установления соединения с БД
    /// </summary>
    public enum CONN_SETT_TYPE
    {
        MAIN_DB
            , COUNT_CONN_SETT_TYPE
    };
    /// <summary>
    /// Идентификаторы периодов/промежутков времени (!!! установлены в БД табл. [time] - д.б. точное совпадение)
    /// </summary>
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
    /// <summary>
    /// Идентификаторы(численные) используемых часовых поясов (!!! установлены в БД  табл. [timezone] - д.б. точное совпадение)
    /// </summary>
    public enum ID_TIMEZONE
    {
        UNKNOWN = -1
        , UTC = 0
        , MSK = 1
        , NSK = 2
    }

    public enum ID_COMP
    {
        UNKNOWN = -1
        , TG = 1000
    }
}
