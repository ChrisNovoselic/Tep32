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
    public class HandlerDbTaskCalculate : HHandlerDb
    {
        /// <summary>
        /// Наименования таблиц в БД, необходимых для расчета (длина = INDEX_DBTABLE_NAME.COUNT)
        /// </summary>
        public static string[] s_NameDbTables = {
            @"time"
            , @"timezones"
            , @"comp_list"
            , @"mode_dev"
            , @"ratio"
            , @"measure"
            , @"session"
            , @"inalg"
            , @"input"
            , @"inval"
            , @"inval_def"
            , @"outalg"
            , @"output"
            , @"outval"
        };

        public HandlerDbTaskCalculate()
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

        public void CalculateNormative()
        {
            int err = -1
                , iListenerId = -1;
            DbConnection dbConn = null;
            DataTable tableSession = null
                , tableIn = null;

            iListenerId = DbSources.Sources().Register(FormMainBaseWithStatusStrip.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.MAIN_DB].getConnSett(), false, CONN_SETT_TYPE.MAIN_DB.ToString());
            dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            // прочитать идентификатор сессии для текущего пользователя
            tableSession = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.SESSION] + @" WHERE [IS_USER]=" + HTepUsers.Id, null, null, out err);
            
            // прочитать входные значения для сессии
            tableIn = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM " + s_NameDbTables[(int)INDEX_DBTABLE_NAME.INVALUES] + @" WHERE [IS_SESSION]=" + (int)tableSession.Rows[0][@"ID_CALCULATE"], null, null, out err);

            // произвести вычисления

            // сохранить результаты вычисления
        }

        public void CalculateMaket()
        {
            // прочитать идентификатор сессии для текущего пользователя

            // прочитать входные значения для сессии

            // прочитать выходные-нормативы значения для сессии

            // произвести вычисления

            // сохранить результаты вычисления
        }
    }
}
