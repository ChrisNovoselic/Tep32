using System;
using System.Windows.Forms;
using System.Data;

using HClassLibrary;
using TepCommon;
using System.Collections.Generic;

namespace PluginTaskTepMain
{
    /// <summary>
    /// Базовый класс для отображения расчетных значений ИРЗ Расчет ТЭП
    ///  при расчете всегда "чужая" сессия (переопределена 'Activate', отправлен запрос для получения "чужого" идентификатора сессии)
    ///  при просмотре архивных значений собственная сессия, но расчет недоступен
    /// </summary>
    public abstract partial class PanelTaskTepOutVal : PanelTaskTepValues
    {
        //protected enum TYPE_OUTVALUES { UNKNOWUN = -1, NORMATIVE, MAKET, COUNT }
        /// <summary>
        /// Конструктор - основной (с параметром)
        /// </summary>
        /// <param name="iFunc">Объект для взаимной связи с главной формой приложения</param>
        protected PanelTaskTepOutVal(IPlugIn iFunc, TepCommon.HandlerDbTaskCalculate.TaskCalculate.TYPE type)
            : base(iFunc, type)
        {
            InitializeComponents();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponents()
        {
        }

        //protected override void initialize()
        //{
        //    base.initialize();

        //    //eventAddPutParameter += new Action<PUT_PARAMETER> ((PanelManagement as PanelManagementTaskTepValues).AddParameter);
        //}

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate(activate);

            int err = 0;

            if (bRes == true)
                if (activate == true)
                    if (IsFirstActivated == false) {
                        // подтвердить наличие сессии расчета
                        HandlerDb.InitSession(out err);

                        if (err < 0)
                            clear();
                        else
                            ;
                    } else
                        ;
                else
                    ;
            else
                ;

            return bRes;
        }
        /// <summary>
        /// Обработчик события - изменение значения в отображении для сохранения
        /// </summary>
        /// <param name="pars"></param>
        protected override void onEventCellValueChanged(object dgv, PanelTaskTepValues.DataGridViewTaskTepValues.DataGridViewTEPValuesCellValueChangedEventArgs ev)
        {
            throw new NotImplementedException();
        }

        //protected override void setValues(DateTimeRange[] arQueryRanges, out int err, out string strErr)
        //{
        //    err = 0;
        //    strErr = string.Empty;
        //    //Запрос для получения ранее учтенных (сохраненных) данных
        //    m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.ARCHIVE] =
        //        //HandlerDb.GetValuesVar(Type
        //        //    , Session.CurrentIdPeriod
        //        //    , CountBasePeriod
        //        //    , getDateTimeRangeValuesVar ()
        //        //    , out err)
        //        new DataTable()
        //            ;
        //    //Запрос для получения автоматически собираемых данных
        //    m_arTableOrigin[(int)TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE] = Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE ?
        //        HandlerDb.GetVariableValues(TaskCalculateType, out err) : Session.m_ViewValues == TepCommon.HandlerDbTaskCalculate.ID_VIEW_VALUES.SOURCE_IMPORT ? ImpExpPrevVersionValues.Import(TaskCalculateType
        //            , Session.m_Id
        //            , (int)TepCommon.HandlerDbTaskCalculate.ID_QUALITY_VALUE.USER, m_dictTableDictPrj[ID_DBTABLE.IN_PARAMETER]
        //            , m_dictTableDictPrj[ID_DBTABLE.RATIO]
        //            , out err) :
        //                new DataTable ();

        //    switch (err)
        //    {
        //        case 0:
        //        default:
        //            break;
        //    }
        //}
        /// <summary>
        /// Обработчик события - нажатие кнопки "Результирующее действие - К макету"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected override void btnRunRes_onClick(object obj, EventArgs ev)
        {
            throw new NotImplementedException();
        }
        ///// <summary>
        ///// Инициировать подготовку к расчету
        /////  , выполнить расчет
        /////  , актуализировать таблицы с временными значениями
        ///// </summary>
        ///// <param name="type">Тип требуемого расчета</param>
        //protected override void btnRun_onClick(HandlerDbTaskCalculate.TaskCalculate.TYPE type)
        //{
        //    throw new NotImplementedException();
        //}
        /// <summary>
        /// Удалить сессию (+ очистить реквизиты сессии)
        /// </summary>
        protected override void deleteSession()
        {
            base.deleteSession();

            int err = -1;

            HandlerDb.InitSession(out err);
        }
        /// <summary>
        /// Обработчик события - добавить Put-параметр
        /// </summary>
        /// <param name="obj">Объект - Put-параметр(дополнительный, в составе NAlg, элемент алгоритма расчета)</param>
        protected override void onAddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER obj)
        {
            base.onAddPutParameter(obj);

            (PanelManagement as PanelManagementTaskTepOutVal).AddPutParameter(obj);
        }
        /// <summary>
        /// Класс для размещения управляющих элементов управления
        /// </summary>
        protected class PanelManagementTaskTepOutVal : PanelManagementTaskTepValues
        {
            protected override void activateControlChecked_onChanged(INDEX_CONTROL[] arIndxControlToActivate, bool bActive)
            {
                TreeViewTaskTepCalcParameters tv = null;
                List<INDEX_CONTROL> listIndxControlToActivate = new List<INDEX_CONTROL>(arIndxControlToActivate);

                foreach (INDEX_CONTROL indxControlToActivate in listIndxControlToActivate) {
                    //indxCtrl = getIndexControlOfIndexID(idToActivate);

                    if (indxControlToActivate == INDEX_CONTROL.MIX_PARAMETER_CALCULATED) {
                        tv = (Controls.Find(indxControlToActivate.ToString(), true)[0] as TreeViewTaskTepCalcParameters);

                        tv.ActivateCheckedHandler(bActive);

                        if (bActive == true) {
                            //tv.NodeSelect += new DelegateIntFunc (onNodeSelect);
                            tv.ItemCheck += new DelegateIntIntFunc(onItemCheck);
                        } else {
                            //tv.NodeSelect -= onNodeSelect;
                            tv.ItemCheck -= onItemCheck;
                        }
                    } else
                    // оставить для обработке в бащовых методах
                        ;
                }
                // в списке нет идентификатора объекта обработчик события 'CheckedChanged' уже обработано
                base.activateControlChecked_onChanged(listIndxControlToActivate.ToArray(), bActive);
            }

            protected override int addButtonRun(int posRow)
            {
                int iRes = posRow;

                return iRes;
            }
            /// <summary>
            /// Инициировать отправление события - измнение состояния параметра алгоритма расчета
            ///  , либо базового, либо связанного с компонентом станции
            /// </summary>
            /// <param name="idItem">Идентификатор парметра алгоритма расчета (по значению можно определить базовый он или связанный)</param>
            /// <param name="iChecked">Признак нового состояния элемента управленияЮ и, соответственно, параметра алгоритма расчета</param>
            protected void onItemCheck(int idItem, int iChecked)
            {
                itemCheck(idItem
                    , ItemCheckedParametersEventArgs.TYPE.ENABLE
                    ,  iChecked == 1 ? CheckState.Checked : CheckState.Unchecked);
            }
            /// <summary>
            /// Добавить параметр алгоритма расчета, связанный с компонентом станции
            ///  , добавляется только для древовидной структуры, управляющей включением/исключением из алгоритма расчета
            ///  , для визуализации простой список параметров алгоритма расчета базовых
            /// </summary>
            /// <param name="putPar">Объект с описанием добавляемого параметра</param>
            public void AddPutParameter(TepCommon.HandlerDbTaskCalculate.PUT_PARAMETER putPar)
            {
                Control ctrl = find(INDEX_CONTROL.CLBX_COMP_CALCULATED);

                (ctrl as TreeViewTaskTepCalcParameters).AddItem(putPar.m_idNAlg, putPar.IdComponent, putPar.m_Id, putPar.NameShrComponent, putPar.IsEnabled);
            }
            /// <summary>
            /// ??? Возвратить наименование компонента
            /// </summary>
            /// <param name="id_comp">Идентификатор компонента</param>
            /// <returns>Наименование компонента</returns>
            public string GetNameComponent(int id_comp)
            {
                string strRes = string.Empty;

                CheckedListBoxTaskCalculate ctrl = null;

                ctrl = findControl(INDEX_CONTROL.MIX_PARAMETER_CALCULATED.ToString()) as CheckedListBoxTaskCalculate;
                strRes = ctrl.GetNameItem(id_comp);

                return strRes;
            }
            /// <summary>
            /// Класс для размещения параметров расчета с учетом их иерархической структуры
            /// </summary>
            public class TreeViewTaskTepCalcParameters : TreeView, IControl
            {
                private class KEY_NODE
                {
                    public enum INDEX { ID_ALG, ID_COMP, ID_PUT }

                    public INDEX Index
                    {
                        get {
                            INDEX indxRes = INDEX.ID_ALG;

                            foreach (INDEX indx in Enum.GetValues(typeof(INDEX)))
                                if (_values[(int)indx] < 0)
                                    break;
                                else
                                    indxRes = indx;

                            return indxRes;
                        }
                    }

                    private static string DELIMETER_KEY = @"::";

                    private KEY_NODE() { }

                    public KEY_NODE(int nAlg, int idComp, int idPut)
                    {
                        _values = new int [] { nAlg, idComp, idPut };
                    }

                    public KEY_NODE(string nameNode)
                    {
                        _values = new int[Enum.GetValues(typeof(INDEX)).Length];

                        string[] strIds = null;

                        strIds = nameNode.Split(new string[] { DELIMETER_KEY }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (INDEX indx in Enum.GetValues(typeof(INDEX)))
                            if ((int)indx < strIds.Length)
                                if (int.TryParse(strIds[(int)indx], out _values[(int)indx]) == false) {
                                    _values[(int)indx] = -1;

                                    throw new Exception(string.Format (@"TreeViewTaskTepCalcParameters.KEY_NODE::ctor (nameNode={0}) - ", nameNode));
                                } else
                                    ;
                            else
                                _values[(int)indx] = -1;
                    }

                    private int[] _values;

                    public string ToString(INDEX indx = INDEX.ID_PUT)
                    {
                        string strRes = string.Empty;

                        switch (indx) {
                            case INDEX.ID_ALG:
                                strRes = _values[(int)INDEX.ID_ALG].ToString();
                                break;
                            case INDEX.ID_PUT:
                                strRes = _values[(int)INDEX.ID_ALG].ToString() + DELIMETER_KEY + _values[(int)INDEX.ID_PUT].ToString();
                                break;
                        }

                        return strRes;                        
                    }

                    public int Id { get { return -1; } }
                }

                //public event DelegateIntFunc NodeSelect;
                
                public event DelegateIntIntFunc ItemCheck;

                public int SelectedId
                {
                    get {
                        int iRes = -1;

                        iRes = new KEY_NODE(SelectedNode.Name).Id;

                        return iRes;
                    }
                }

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
                    KEY_NODE keyNode = null;

                    keyNode = new KEY_NODE(id_alg, id_comp, id_put);
                    nodesNAlg = Nodes.Find(id_alg.ToString(), false);                    
                    strTextNode = (Parent as PanelManagementTaskTepOutVal).GetNameComponent(id_comp);

                    switch (nodesNAlg.Length) {
                        case 0:
                            node = Nodes.Add(keyNode.ToString(KEY_NODE.INDEX.ID_ALG), text);
                            node = node.Nodes.Add(keyNode.ToString(), strTextNode);
                            break;
                        case 1:
                            node = nodesNAlg[0].Nodes.Add(keyNode.ToString(), strTextNode);
                            break;
                        default:
                            Logging.Logg().Error(string.Format(@"TreeViewTaskTepCalcParameters::AddItem (KEY={0}) - ..."
                                    , keyNode.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                    }

                    node.Checked = bChecked;

                    if (!(node.Parent == null)) {
                    // для элементов, 2-го уровня
                        // по умолчанию для родительского элемента признак установлен
                        bChecked = true;

                        foreach (TreeNode n in node.Parent.Nodes) {
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
                    if (bActive == true) {
                        //this.AfterSelect +=  new TreeViewEventHandler(onAfterSelect);
                        this.AfterCheck += new TreeViewEventHandler(onAfterCheck);
                    } else {
                        //this.AfterSelect -= onAfterSelect;
                        this.AfterCheck -= onAfterCheck;
                    }
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
                            n.Checked = ev.Node.Checked;

                        ActivateCheckedHandler(true);
                    }
                    else
                        ;

                    itemCheck(SelectedId, ev.Node.Checked == true ? 1 : 0);
                }

                //private void onAfterSelect(object obj, TreeViewEventArgs ev)
                //{
                //    string[] strIds = ev.Node.Name.Split();
                //    NodeSelect(SelectedId);
                //}

                private void itemCheck(int id, int iChecked)
                {
                    ItemCheck(id, iChecked);
                }
            }            

            protected override Control createControlNAlgParameterCalculated()
            {
                return new TreeViewTaskTepCalcParameters();
            }
        }
    }
}
