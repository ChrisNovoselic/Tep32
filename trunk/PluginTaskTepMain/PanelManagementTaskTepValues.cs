using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;

using HClassLibrary;
using InterfacePlugIn;
using TepCommon;

namespace PluginTaskTepMain
{
    public abstract partial class PanelTaskTepValues : PanelTaskTepCalculate
    {
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected abstract class PanelManagementTaskTepValues : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagementTaskTepValues()
                : base(ModeTimeControlPlacement.Queue)
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                Control ctrl = null;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню лдя кнопки "Обновить-Загрузить"                

                SuspendLayout();

                //Расчет - выполнить - макет
                //Расчет - выполнить - норматив
                addButtonRun(0);

                posRow = 5;
                //Признаки включения/исключения из расчета
                //Признаки включения/исключения из расчета - подпись
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить из расчета";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //Признак для включения/исключения из расчета компонента
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения из расчета параметра
                ctrl = createControlNAlgParameterCalculated();
                ctrl.Name = INDEX_CONTROL.MIX_PARAMETER_CALCULATED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 3);

                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Кнопка - импортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_IMPORT.ToString();
                ctrl.Text = @"Импорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                ctrl.Enabled = true;
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                //Кнопка - экспортировать
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, posRow);
                SetColumnSpan(ctrl, 4); SetRowSpan(ctrl, 1);
                ctrl.Enabled = false;

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new System.Windows.Forms.Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as System.Windows.Forms.Label).Text = @"Включить/исключить для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 1);
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);
                //Признак для включения/исключения для отображения параметра
                ctrl = new CheckedListBoxTaskCalculate();
                ctrl.Name = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 3);
                SetColumnSpan(ctrl, 8); SetRowSpan(ctrl, 3);

                ResumeLayout(false);
                PerformLayout();
            }
            /// <summary>
            /// Инициализация размеров/стилей макета для размещения элементов управления
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly();
            }

            ///// <summary>
            ///// Обработчик события - изменение дата/время начала периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //private void hdtpBegin_onValueChanged(object obj, EventArgs ev)
            //{
            //    m_dtRange.Set((obj as HDateTimePicker).Value, m_dtRange.End);

            //    DateTimeRangeValue_Changed(this, EventArgs.Empty);
            //}
            ///// <summary>
            ///// Обработчик события - изменение дата/время окончания периода
            ///// </summary>
            ///// <param name="obj">Составной объект - календарь</param>
            ///// <param name="ev">Аргумент события</param>
            //private void hdtpEnd_onValueChanged(object obj, EventArgs ev)
            //{
            //    HDateTimePicker hdtpEnd = obj as HDateTimePicker;
            //    m_dtRange.Set(hdtpEnd.LeadingValue, hdtpEnd.Value);

            //    if (! (DateTimeRangeValue_Changed == null))
            //        DateTimeRangeValue_Changed(this, EventArgs.Empty);
            //    else
            //        ;
            //}

            /// <summary>
            /// Добавить кнопки, инициирующие процесс расчета
            /// </summary>
            /// <param name="posRow">Позиция по горизонтали для размещения 1-ой (вверху) кнопки</param>
            /// <returns>Позиция по горизонтали для размещения следующего элемента</returns>
            protected abstract int addButtonRun(int posRow);
            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                base.Clear();

                //ActivateCheckedHandler(arIndxIdToClear, false);

                //??? почему только компоненты               
                clearComponents();
                //??? тоже очищаются
                clearParameters();
            }
            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns></returns>
            protected Control find(INDEX_CONTROL indxCtrl)
            {
                Control ctrlRes = null;

                ctrlRes = Controls.Find(indxCtrl.ToString(), true)[0];

                return ctrlRes;
            }

            protected INDEX_CONTROL getIndexControl(Control ctrl)
            {
                INDEX_CONTROL indxRes = INDEX_CONTROL.UNKNOWN;

                string strId = (ctrl as Control).Name;

                if (strId.Equals(INDEX_CONTROL.CLBX_COMP_CALCULATED.ToString()) == true)
                    indxRes = INDEX_CONTROL.CLBX_COMP_CALCULATED;
                else
                    if (strId.Equals(INDEX_CONTROL.MIX_PARAMETER_CALCULATED.ToString()) == true)
                        indxRes = INDEX_CONTROL.MIX_PARAMETER_CALCULATED;
                    else
                        if (strId.Equals(INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString()) == true)
                            indxRes = INDEX_CONTROL.CLBX_COMP_VISIBLED;
                        else
                            if (strId.Equals(INDEX_CONTROL.CLBX_PARAMETER_VISIBLED.ToString()) == true)
                                indxRes = INDEX_CONTROL.CLBX_PARAMETER_VISIBLED;
                            else
                                throw new Exception(@"PanelTaskTepValues::getIndexControl () - не найден объект 'CheckedListBox'...");

                return indxRes;
            }

            private void clearParameters()
            {
                INDEX_CONTROL[] arIndxToClear = new INDEX_CONTROL[] {
                    INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                    , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                };

                for (int i = 0; i < arIndxToClear.Length; i++)
                    clear(arIndxToClear[i]);
            }

            private void clearComponents()
            {
                INDEX_CONTROL[] arIndxToClear = new INDEX_CONTROL[] {
                    INDEX_CONTROL.CLBX_COMP_CALCULATED
                    , INDEX_CONTROL.CLBX_COMP_VISIBLED
                };

                for (int i = 0; i < arIndxToClear.Length; i++)
                    clear(arIndxToClear[i]);
            }

            private void clear(INDEX_CONTROL indxCtrl)
            {
                (find(indxCtrl) as IControl).ClearItems();
            }
            /// <summary>
            /// (Де)Активировать обработчик события
            /// </summary>
            /// <param name="bActive">Признак (де)активации</param>
            protected override void activateControlChecked_onChanged(bool bActive)
            {
                activateControlChecked_onChanged(new INDEX_CONTROL[] {
                        INDEX_CONTROL.CLBX_COMP_CALCULATED
                        , INDEX_CONTROL.CLBX_COMP_VISIBLED
                        , INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                        , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                    }, bActive);
            }

            protected virtual void activateControlChecked_onChanged(INDEX_CONTROL[] arIndxControlToActivate, bool bActive)
            {
                //Из 'OutVal' вернется укороченный, т.к. в 'OutVal' есть 'TreeView' и его обработчик будет (де)активирован в наследуемом методе
                // в базовый метод должны быть переданы только идентификаторы-наименования 'CheckListBox'
                activateControlChecked_onChanged(arIndxControlToActivate.ToList().ConvertAll<string>(indx => { return indx.ToString(); }).ToArray(), bActive);
            }
            /// <summary>
            /// Добавить элемент компонент станции в списки
            ///  , в соответствии с 'arIndexIdToAdd'
            /// </summary>
            /// <param name="id">Идентификатор компонента</param>
            /// <param name="text">Текст подписи к компоненту</param>
            /// <param name="arIndexIdToAdd">Массив индексов в списке </param>
            /// <param name="arChecked">Массив признаков состояния для элементов</param>
            public void AddComponent(TepCommon.HandlerDbTaskCalculate.TECComponent comp)
            {
                Control ctrl = null;
                bool bChecked = false;

                // в этих элементах управления размещаются элементы проекта - компоненты станции(оборудование)
                INDEX_CONTROL[] arIndexControl = new INDEX_CONTROL[] {
                    INDEX_CONTROL.CLBX_COMP_CALCULATED
                    , INDEX_CONTROL.CLBX_COMP_VISIBLED
                };

                foreach (INDEX_CONTROL indxCtrl in arIndexControl)
                {
                    ctrl = find(indxCtrl);

                    if (indxCtrl == INDEX_CONTROL.CLBX_COMP_CALCULATED)
                        bChecked = comp.m_bEnabled;
                    else if (indxCtrl == INDEX_CONTROL.CLBX_COMP_VISIBLED)
                        bChecked = comp.m_bVisibled;
                    else
                        bChecked = false;

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskCalculate).AddItem(comp.m_Id, comp.m_nameShr, bChecked);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddComponent () - не найден элемент для INDEX_ID=" + indxCtrl.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            public void AddNAlgParameter(TepCommon.HandlerDbTaskCalculate.NALG_PARAMETER nAlgPar)
            {
                CheckedListBoxTaskCalculate ctrl;
                bool bChecked = false;

                // в этих элементах управления размещаются элементы проекта - параметры алгоритма расчета
                INDEX_CONTROL[] arIndexControl = new INDEX_CONTROL[] {
                    INDEX_CONTROL.MIX_PARAMETER_CALCULATED
                    , INDEX_CONTROL.CLBX_PARAMETER_VISIBLED
                };

                foreach (INDEX_CONTROL indxCtrl in arIndexControl) {
                    ctrl = find(indxCtrl) as CheckedListBoxTaskCalculate;

                    if (indxCtrl == INDEX_CONTROL.MIX_PARAMETER_CALCULATED)
                        bChecked = nAlgPar.m_bEnabled;
                    else if (indxCtrl == INDEX_CONTROL.CLBX_PARAMETER_VISIBLED)
                        bChecked = nAlgPar.m_bVisibled;
                    else
                        bChecked = false;

                    if (!(ctrl == null))
                        (ctrl as CheckedListBoxTaskCalculate).AddItem(nAlgPar.m_Id, string.Format(@"[{0}]-{1}", nAlgPar.m_nAlg, nAlgPar.m_strNameShr), bChecked);
                    else
                        Logging.Logg().Error(@"PanelManagementTaskTepValues::AddNAlgParameter () - не найден элемент =" + indxCtrl.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            protected virtual Control createControlNAlgParameterCalculated()
            {
                return new CheckedListBoxTaskCalculate();
            }

            //private void onSelectedIndexChanged(object obj, EventArgs ev)
            //{                
            //}

            //protected virtual void addItem(INDEX_ID indxId, Control ctrl, int id, string text, bool bChecked)
            //{
            //    (ctrl as CheckedListBoxTaskTepValues).AddItem(id, text, bChecked);
            //}

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                ItemCheckedParametersEventArgs.TYPE type;
                INDEX_CONTROL indxCtrl = INDEX_CONTROL.UNKNOWN;

                if (Enum.IsDefined(typeof(INDEX_CONTROL), (obj as Control).Name) == true) {
                    indxCtrl = (INDEX_CONTROL)Enum.Parse(typeof(INDEX_CONTROL), (obj as Control).Name);

                    switch (indxCtrl) {                        
                        case INDEX_CONTROL.CLBX_COMP_CALCULATED:
                        case INDEX_CONTROL.MIX_PARAMETER_CALCULATED:
                            type = ItemCheckedParametersEventArgs.TYPE.ENABLE;
                            break;
                        case INDEX_CONTROL.CLBX_COMP_VISIBLED:
                        case INDEX_CONTROL.CLBX_PARAMETER_VISIBLED:
                        default:
                            type = ItemCheckedParametersEventArgs.TYPE.VISIBLE;
                            break;
                    }

                    itemCheck((obj as IControl).SelectedId, type, (ev as ItemCheckEventArgs).NewValue);
                } else
                    Logging.Logg().Error(string.Format(@""), Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }
}
