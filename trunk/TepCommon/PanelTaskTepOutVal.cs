using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Drawing;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace TepCommon
{
    public abstract partial class PanelTaskTepOutVal : PanelTaskTepValues
    {
        //protected enum TYPE_OUTVALUES { UNKNOWUN = -1, NORMATIVE, MAKET, COUNT }

        protected struct RANGE_ID_RECORD
        {
            public ID_START_RECORD Start;
            public ID_START_RECORD End;
        }

        RANGE_ID_RECORD m_rangeRecord;
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepOutVal(IPlugIn iFunc, TYPE type)
            : base(iFunc, type)
        {
            InitializeComponents();

            m_rangeRecord = new RANGE_ID_RECORD()
            {
                Start = type == TYPE.OUT_NORM_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                    type == TYPE.OUT_MKT_VALUES ? ID_START_RECORD.ALG :
                        ID_START_RECORD.ALG
                ,
                End = type == TYPE.OUT_NORM_VALUES ? ID_START_RECORD.PUT :
                    type == TYPE.OUT_MKT_VALUES ? ID_START_RECORD.ALG_NORMATIVE :
                        ID_START_RECORD.PUT
            };
        }

        private void InitializeComponents()
        {
        }

        protected override string whereRangeRecord
        {
            get { return @"[ID] BETWEEN " + (int)(m_rangeRecord.Start - 1) + @" AND " + (int)(m_rangeRecord.End - 1); }
        }
         /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected override void onEventCellValueChanged(object dgv, PanelTaskTepValues.DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            throw new NotImplementedException();
        }        
    }
}
