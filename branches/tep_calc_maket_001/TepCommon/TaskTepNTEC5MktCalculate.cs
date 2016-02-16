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
    public partial class HandlerDbTaskCalculate : HandlerDbValues
    {
        /// <summary>
        /// Класс для расчета технико-экономических показателей
        /// </summary>
        public partial class TaskTepCalculate : TaskCalculate
        {
            private float calculateMaket(string nAlg)
            {
                float fRes = 0F;

                int i = -1;

                switch (nAlg)
                {
                    case @"1": //
                        break;
                    case @"2": //
                        break;
                    case @"3": //                        
                        break;
                    case @"4": //
                        break;
                    case @"5": //
                        break;
                    case @"6":
                        break;
                    default:
                        Logging.Logg().Error(@"TaskTepCalculate::calculateMaket (N_ALG=" + nAlg + @") - неизвестный параметр...", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }

                return fRes;
            }
        }
    }
}
