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
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepOutVal(IPlugIn iFunc, HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc, type)
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected override void onEventCellValueChanged(object dgv, PanelTaskTepValues.DataGridViewTEPValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepOutVal : PanelManagementTaskTepValues
        {
            protected override void activateCheckedHandler(bool bActive, INDEX_ID idToActivate)
            {
                INDEX_CONTROL indxCtrl = INDEX_CONTROL.UNKNOWN;
                TreeViewTaskTepCalcParameters tv = null;

                indxCtrl = getIndexControlOfIndexID(idToActivate);

                if (indxCtrl == INDEX_CONTROL.CLBX_PARAMETER_CALCULATED)
                {
                    tv = (Controls.Find(indxCtrl.ToString(), true)[0] as TreeViewTaskTepCalcParameters);

                    if (bActive == true)
                        tv.ItemCheck += new ItemCheckEventHandler(onItemCheck);
                    else
                        tv.ItemCheck -= onItemCheck;
                }
                else
                    base.activateCheckedHandler(bActive, idToActivate);
            }

            protected override void addItem(INDEX_ID indxId, Control ctrl, int id, string text, bool bChecked)
            {
                if (indxId == INDEX_ID.DENY_PARAMETER_CALCULATED)
                    (ctrl as TreeViewTaskTepCalcParameters).AddItem(id, text, bChecked);
                else
                    base.addItem(indxId, ctrl, id, text, bChecked);
            }
            /// <summary>
            /// Класс для размещения параметров расчета с учетом их иерархической структуры
            /// </summary>
            public class TreeViewTaskTepCalcParameters : TreeView, IControl
            {
                public event ItemCheckEventHandler ItemCheck;

                public int SelectedId { get { return -1; } }

                public TreeViewTaskTepCalcParameters()
                    : base()
                {
                }
                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                }
                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                }
            }            

            protected override Control createControlParameterCalculated()
            {
                return new TreeViewTaskTepCalcParameters();
            }
        }
    }
}
