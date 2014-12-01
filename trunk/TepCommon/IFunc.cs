using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TepCommon
{
    public interface IFuncHost
    {
        bool Register(IFunc plug);
    }

    public interface IFunc
    {
    }

    public interface IFuncDictTime : IFunc
    {
    }

    public interface IFuncDictRole : IFunc
    {
    }
}
