using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public abstract partial class PanelTaskTepCalculate : HPanelTepCommon
    {
        protected class HandlerDbTepTaskValues : HHandlerDb
        {
            public HandlerDbTepTaskValues()
                : base()
            {
            }

            public override void StartDbInterfaces()
            {
                throw new NotImplementedException();
            }

            public override void ClearValues()
            {
                throw new NotImplementedException();
            }

            public void Load(ID_PERIOD idTime)
            {
            }

            protected override int StateCheckResponse(int state, out bool error, out object outobj)
            {
                throw new NotImplementedException();
            }

            protected override int StateRequest(int state)
            {
                throw new NotImplementedException();
            }

            protected override int StateResponse(int state, object obj)
            {
                throw new NotImplementedException();
            }

            protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                throw new NotImplementedException();
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }
    }
}
