using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    public enum CONN_SETT_TYPE
    {
        MAIN_DB,
        COUNT_CONN_SETT_TYPE,
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
