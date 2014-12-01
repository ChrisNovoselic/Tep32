using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    public enum CONN_SETT_TYPE
    {
        CONFIG_DB = 0, LIST_SOURCE,
        ADMIN = 0, PBR = 1, DATA_ASKUE = 2 /*Факт. - АСКУЭ*/, DATA_SOTIASSO = 3 /*ТелеМеханика - СОТИАССО*/, MTERM = 4 /*Модес-Терминал*/,
        COUNT_CONN_SETT_TYPE = 5
    };

    public interface ISourceHost
    {
        bool Register (ISource plug);
    }

    public interface ISource
    {
    }

    public interface ISourceTSQL : ISource
    {
    }

    public interface ISourceFiles : ISource
    {
    }
}
