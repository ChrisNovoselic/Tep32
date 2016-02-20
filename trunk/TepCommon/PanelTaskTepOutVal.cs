﻿using System;
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

                    tv.ActivateCheckedHandler(bActive);

                    if (bActive == true)
                    {
                        tv.NodeSelect += new DelegateIntIntFunc (onNodeSelect);
                        tv.ItemCheck += new DelegateBoolFunc (onItemCheck);
                    }
                    else
                    {
                        tv.NodeSelect -= onNodeSelect;
                        tv.ItemCheck -= onItemCheck;
                    }
                }
                else
                    base.activateCheckedHandler(bActive, idToActivate);
            }

            private void onNodeSelect(int id_comp, int id_par)
            {
                m_address.m_idComp = id_comp;
                m_address.m_idAlg = id_par;
                m_address.m_indxIdDeny = INDEX_ID.DENY_PARAMETER_CALCULATED;
            }

            protected void onItemCheck(bool bChecked)
            {
                itemCheck(m_address, bChecked == true ? CheckState.Checked : CheckState.Unchecked);
            }

            protected override void addParameter(Control ctrl, int id_alg, int id_comp, int id_put, string text, bool bChecked)
            {
                if (ctrl is TreeViewTaskTepCalcParameters)
                    (ctrl as TreeViewTaskTepCalcParameters).AddItem(id_alg, id_comp, id_put, text, bChecked);
                else
                    base.addParameter(ctrl, id_alg, id_comp, id_put, text, bChecked);
            }
            /// <summary>
            /// Класс для размещения параметров расчета с учетом их иерархической структуры
            /// </summary>
            public class TreeViewTaskTepCalcParameters : TreeView, IControl
            {
                private static string DELIMETER_KEY = @"::";

                public event DelegateIntIntFunc NodeSelect;
                
                public event DelegateBoolFunc ItemCheck;

                public int SelectedId { get { return -1; } }

                public TreeViewTaskTepCalcParameters()
                    : base()
                {
                    this.CheckBoxes = true;

                    //ImageList = new ImageList();
                    //ImageList.Images.Add(System.Drawing.Icon.ExtractAssociatedIcon ());
                }
                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id_alg, int id_comp, int id_put, string text, bool bChecked)
                {
                    TreeNode node = null;
                    TreeNode[] nodesNAlg = null;
                    string strTextNode = string.Empty;

                    nodesNAlg = Nodes.Find(id_alg.ToString(), false);
                    strTextNode = (Parent as PanelManagementTaskTepValues).GetNameComponent(id_comp);

                    switch (nodesNAlg.Length)
                    {
                        case 0:
                            node = Nodes.Add(id_alg.ToString(), text);
                            node = node.Nodes.Add(id_alg.ToString() + DELIMETER_KEY + id_put.ToString(), strTextNode);
                            break;
                        case 1:
                            node = nodesNAlg[0].Nodes.Add(id_alg.ToString() + DELIMETER_KEY + id_put.ToString(), strTextNode);
                            break;
                        default:
                            Logging.Logg().Error(@"TreeViewTaskTepCalcParameters::AddItem (ID_ALG=" + id_alg + @", ID_PUT=" + id_put + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                    }

                    node.Checked = bChecked;

                    if (!(node.Parent == null))
                    {// для элементов, 2-го уровня
                        // по умолчанию для родительского элемента признак установлен
                        bChecked = true;

                        foreach (TreeNode n in node.Parent.Nodes)
                        {
                            if (n.Checked == false)
                            {// если хотя бы один элемент без признака, то родительский элемент тоже без признака
                                bChecked = false;

                                break;
                            }
                            else
                                ;
                        }
                        // установить признак для родительского элемента
                        node.Parent.Checked = bChecked;
                    }
                    else
                        ; // элемент верхнего уровня
                }
                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Nodes.Clear();
                }
                /// <summary>
                /// Назначить/отменить обработчики событий
                /// </summary>
                /// <param name="bActive">Признак назначения/снятия</param>
                public void ActivateCheckedHandler(bool bActive)
                {
                    if (bActive == true)
                        this.AfterCheck += new TreeViewEventHandler(onAfterCheck);
                    else
                        this.AfterCheck -= onAfterCheck;
                }
                /// <summary>
                /// Обработчик события - изменение значения признака элемента
                /// </summary>
                /// <param name="obj">Объект, инициировавший событие</param>
                /// <param name="ev">Аргумент события</param>
                private void onAfterCheck(object obj, TreeViewEventArgs ev)
                {
                    if (ev.Node.Nodes.Count > 0)
                    {
                        ActivateCheckedHandler(false);

                        foreach (TreeNode n in ev.Node.Nodes)
                        {
                            n.Checked = ev.Node.Checked;
                            itemCheck(Int32.Parse(n.Name.Split(new string[] { DELIMETER_KEY }, StringSplitOptions.RemoveEmptyEntries)[1]), ev.Node.Checked);
                        }

                        ActivateCheckedHandler(true);
                    }
                    else
                    {
                        itemCheck(Int32.Parse(ev.Node.Name.Split(new string[] { DELIMETER_KEY }, StringSplitOptions.RemoveEmptyEntries)[1]), ev.Node.Checked);
                    }
                }

                private void itemCheck(int id, bool bChecked)
                {
                    ItemCheck(bChecked);
                }
            }            

            protected override Control createControlParameterCalculated()
            {
                return new TreeViewTaskTepCalcParameters();
            }
        }
    }
}
