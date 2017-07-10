using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Data; //DataTable
using System.Data.Common;

using HClassLibrary;
using InterfacePlugIn;
using System.Globalization;

namespace TepCommon
{
    public abstract partial class HPanelCommon : HClassLibrary.HPanelCommon, IObjectDbEdit
    {
        /// <summary>
        /// Дополнительные действия при сохранении значений
        /// </summary>
        protected DelegateFunc delegateSaveAdding;
        /// <summary>
        /// Объект для реализации взаимодействия с главной программой
        /// </summary>
        protected IPlugIn _iFuncPlugin;
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;        

        protected virtual void initialize(ID_DBTABLE[] arIdTableDictPrj, out int err, out string errMsg)
        {
            err = 0;
            errMsg = string.Empty;

            ID_DBTABLE idDbTable = ID_DBTABLE.UNKNOWN;

            // проверить наличие элементов при, необходимости очистить
            __handlerDb.ValidateDictTableDictPrj();

            foreach (ID_DBTABLE id in /*Enum.GetValues(typeof(ID_DBTABLE))*/arIdTableDictPrj) {
                if (!(id == ID_DBTABLE.UNKNOWN))
                    __handlerDb.AddTableDictPrj(id, out err);
                else
                    err = 2;

                if (err < 0) {
                    // ошибка
                    switch (err) {
                        case -3: // наименовавние таблицы
                            errMsg = @"неизвестное наименовнаие таблицы";
                            break;
                        case -2: // неизвестный тип
                            errMsg = @"неизвестный тип таблицы";
                            break;
                        case -1:
                        default:
                            errMsg = @"неопределенная ошибка";
                            break;
                    }

                    errMsg = string.Format(@"HPanelTepCommon::initialize (тип={0}) - {1}...", id, errMsg);

                    break;
                } else
                    if (err > 0)
                    // предупреждение
                        switch (err) {
                            case 1: // идентификатор указан прежде, чем его можно инициализировать объект для него
                                break;
                            case 2: // идентификатор по умолчанию или один из идентификаторов имеет неопределенное значение
                                break;
                            default:
                                    break;
                        }
                    else
                    // ошибок, предупреждений нет
                        ;
            }

            //??? обязательная таблица
            idDbTable = ID_DBTABLE.COMP_VALUES;
            __handlerDb.AddTableDictPrj(idDbTable, out err);            
        }
        /// <summary>
        /// Очистить объекты, элементы управления от текущих данных
        /// </summary>
        /// <param name="indxCtrl">Индекс элемента управления, инициировавшего очистку
        ///  для возвращения предыдущего значения, при отказе пользователя от очистки</param>
        /// <param name="bClose">Признак полной/частичной очистки</param>
        protected virtual void clear(bool bClose = false)
        {
            //??? повторная проверка
            if (bClose == true) {
                __handlerDb.Clear();

                //_panelManagement.Clear(bClose);

                // ??? - д.б. общий метод полной очистки всех представлений
            }
            else
            //??? очистить содержание всех представлений - д.б. общий метод
                ;
        }

        #region Apelgans
        /// <summary>
        /// Идентификатор текущего объекта панели(класса) в соответствии с решистрацией
        /// </summary>
        protected int m_Id;

        public enum INDEX_DATATABLE_DESCRIPTION { TABLE, PROPERTIES };

        public DataTable[] Descriptions = new DataTable[] { new DataTable(), new DataTable() };

        protected HTepUsers.DictionaryProfileItem m_dictProfile;

        public enum ID_SUBPANEL
        {
            MAIN = 1 //Главная
            , PROP = 2 //Свойства
            , DESC = 3 //Описание
        };
        /// <summary>
        /// Список групп
        /// </summary>
        string[] m_arr_name_group_panel = { "Настройка", "Проект", "Задача" };
        /// <summary>
        /// Строки для описания групп вкладок
        /// </summary>
        string[] m_description_group = new string[] { "Группа для настроек", "Группа для проектов", "Группа для задач" };

        public string m_name_panel_desc = string.Empty;

        #endregion

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public HPanelCommon(IPlugIn plugIn)
            : base(13, 13)
        {
            this._iFuncPlugin = plugIn;

            InitializeComponent();

            m_Id = ID;

            __handlerDb = createHandlerDb();

            m_dictProfile = new HTepUsers.DictionaryProfileItem();
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            initializeLayoutStyle();
        }
        /// <summary>
        /// Инициализация размеров/стилей макета для размещения элементов управления
        /// </summary>
        /// <param name="cols">Количество столбцов в макете</param>
        /// <param name="rows">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }
        /// <summary>
        /// Объект для обмена данными с БД
        /// </summary>
        protected HandlerDbValues __handlerDb;        
        /// <summary>
        /// Найти идентификатор типа текущей панели
        ///  , зарегистрированного в библиотеке
        /// </summary>
        /// <returns>Идентификатор типа панели</returns>
        private int ID
        {
            get {
                int iRes = -1;
                Type thisType = Type.Missing as Type;

                thisType = this.GetType();

                //Вариант №1
                KeyValuePair<int, Type>? pairRes = null;

                pairRes = (_iFuncPlugin as PlugInBase).GetRegisterTypes().First(item => { return item.Value == thisType; });

                if (!(pairRes == null))
                    iRes = pairRes.GetValueOrDefault().Key;
                else
                    ;

                ////Вариант №2
                //Dictionary<int, Type> dictRegId = (_iFuncPlugin as PlugInBase).GetRegisterTypes();

                //foreach (var item in dictRegId)
                //{
                //    if (item.Value == myType)
                //    {
                //        iRes = item.Key;
                //    }
                //}

                return iRes;
            }
        }

        protected HandlerDbValues.DictionaryTableDictProject m_dictTableDictPrj { get { return __handlerDb.m_dictTableDictPrj; } }

        protected void initializeDescPanel()
        {
            int err = -1;

            Control ctrl = null;
            string desc = string.Empty
                , name = string.Empty
                , query = string.Empty;
            string[] ar_name = null;
            DataTable table;
            DataRow[] rows = null;

            if (string.IsNullOrEmpty(m_name_panel_desc) == false) {
                try {
                    ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
                    name = ((PlugInMenuItem)_iFuncPlugin).GetNameOwnerMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id);
                    ar_name = name.Split('\\');
                    name = ar_name[0];

                    for (int i = 0; i < m_arr_name_group_panel.Length; i++)
                        if (m_arr_name_group_panel[i] == name)
                            ((HPanelDesc)ctrl).SetLblGroup = new string[] { name, m_description_group[i] };
                        else
                            ;

                    //Описание вкладки
                    query = "SELECT DESCRIPTION FROM [dbo].[fpanels] WHERE [ID]=" + m_Id;
                    table = __handlerDb.Select(query, out err);
                    if (table.Rows.Count != 0) {
                        desc = table.Rows[0][0].ToString();
                        ((HPanelDesc)ctrl).SetLblTab = new string[] { /*((PlugInMenuItem)_iFuncPlugin).GetNameMenuItem(((HFuncDbEdit)_iFuncPlugin)._Id)*/
                            this.Parent.Text, desc
                        };
                    } else
                        ;

                    //Описания таблиц
                    query = "SELECT * FROM [dbo].[table_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE] = __handlerDb.Select(query, out err);

                    //Описания параметров
                    query = "SELECT * FROM [dbo].[param_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES] = __handlerDb.Select(query, out err);

                    //Описания параметров
                    query = "SELECT * FROM [dbo].[param_description] WHERE [ID_PANEL]=" + m_Id;
                    Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES] = __handlerDb.Select(query, out err);

                    if (!(err == 0))
                        Logging.Logg().Error("TepCommon.HpanelTepCommon initializeDescPanel - Select выполнен с ошибкой: " + err, Logging.INDEX_MESSAGE.NOT_SET);
                    else
                        ;

                    if (!(Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Columns.IndexOf("ID_TABLE") < 0)) {
                        rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_SUBPANEL.MAIN);

                        if (rows.Length == 1)
                            Logging.Logg().Error("TepCommon.HpanelTepCommon initializeDescPanel - Select выполнен с ошибкой: " + err, Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;

                        //DataRow[] rows = null;
                        if (!(Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Columns.IndexOf("ID_TABLE=") < 0)) {
                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_SUBPANEL.MAIN);
                            if (rows.Length == 1)
                                ((HPanelDesc)ctrl).SetLblDGV1Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                            else
                                ;

                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_SUBPANEL.PROP);
                            if (rows.Length == 1)
                                ((HPanelDesc)ctrl).SetLblDGV2Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                            else
                                ;

                            rows = Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].Select("ID_TABLE=" + (int)ID_SUBPANEL.DESC);
                            if (rows.Length == 1) {
                                ((HPanelDesc)ctrl).SetLblDGV3Desc = new string[] { rows[0]["NAME"].ToString(), rows[0]["DESCRIPTION"].ToString() };
                                ((HPanelDesc)ctrl).SetLblDGV3Desc_View = false;
                            } else
                                ;
                        } else
                            Logging.Logg().Error(@"HPanelTepCommon::initializeDescPanel () - в таблице [" + Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.TABLE].TableName + @"] не найдено поле [ID_TABLE]"
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, @"HPanelTepCommon::initializeDescPanel () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        public void Start(object obj)
        {
            //try
            //{
            //    if (this.IsHandleCreated == true)
            //        //if (this.InvokeRequired == true)
            //        this.BeginInvoke(new DelegateObjectFunc(initialize), obj);
            //    else
            //        ;
            //}
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, @"HPanelEdit::Initialize () - BeginInvoke (initialize) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //}

            Start();

            //m_handlerDb.InitConnectionSettings(((EventArgsDataHost)obj).par[0] as ConnectionSettings);
            __handlerDb.InitConnectionSettings(obj as ConnectionSettings);
            
            //HTepUsers.HTepProfilesXml.UpdateProfile(m_handlerDb.ConnectionSettings);
            m_dictProfile = HTepUsers.HTepProfilesXml.GetProfileUserPanel(HTepUsers.Id, HTepUsers.Role, m_Id);
        }

        //public override void Stop()
        //{
        //    while (Controls.Count > 0)
        //        Controls.RemoveAt(0);

        //    base.Stop();
        //}

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);
            int err = -1;
            string strErrMsg = string.Empty;

            try {
                if ((bRes == true)
                    && (active == true)
                    && (IsFirstActivated == true)) {
                    initialize(out err, out strErrMsg);

                    initializeDescPanel();
                } else
                    ;
            } catch (Exception e) {
                Logging.Logg().Exception(e, @"HPanelTepCommon::Activate () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return bRes;
        }
        /// <summary>
        /// Повторная инициализация
        /// </summary>
        protected virtual void reinit()
        {
            int err = -1;
            string errMsg = string.Empty;

            initialize(out err, out errMsg);

            if (!(err == 0))
            {
                throw new Exception(@"HPanelTepCommon::clear () - " + errMsg);
            }
            else
            {
            }
        }

        protected abstract void initialize(out int err, out string errMsg);        
        /// <summary>
        /// Создать объект для обмена данными с БД
        /// </summary>
        /// <returns>Объект для обмена данными с БД</returns>
        protected abstract HandlerDbValues createHandlerDb();
        //protected abstract void Activate(bool activate);
        /// <summary>
        /// Добавить область оперативного описания выбранного объекта на вкладке
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="posCol">Позиция-столбец для размещения области описания</param>
        /// <param name="posRow">Позиция-строка для размещения области описания</param>
        protected virtual void addLabelDesc(string id, int posCol = 5, int posRow = 10)
        {
            GroupBox gbDesc = new GroupBox();
            gbDesc.Text = @"Описание";
            gbDesc.Dock = DockStyle.Fill;
            this.Controls.Add(gbDesc, posCol, posRow);
            this.SetColumnSpan(gbDesc, this.ColumnCount - posCol);
            this.SetRowSpan(gbDesc, this.RowCount - posRow);

            HPanelDesc ctrl = new HPanelDesc();
            ctrl.Name = id;
            gbDesc.Controls.Add(ctrl);
            m_name_panel_desc = id;
        }
        /// <summary>
        /// Для отображения актуальной "подсказки" для свойства
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        protected virtual void panelEdit_dgvPropSelectionChanged(object obj, EventArgs ev)
        {
            string desc = string.Empty;
            string name = string.Empty;
            try {
                if (((DataGridView)obj).SelectedRows.Count > 0) {
                    name = ((DataGridView)obj).SelectedRows[0].Cells[0].Value.ToString();

                    foreach (DataRow r in Descriptions[(int)INDEX_DATATABLE_DESCRIPTION.PROPERTIES].Rows)
                        if (name == r["PARAM_NAME"].ToString())
                            desc = r["DESCRIPTION"].ToString();
                        else
                            ;
                }
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"HPanelCommon::HPanelEdit_dgvPropSelectionChanged () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            SetDescSelRow(desc, name);
        }

        protected void SetDescSelRow(string desc, string name)
        {
            Control ctrl = this.Controls.Find(m_name_panel_desc, true)[0];
            ((HPanelDesc)ctrl).SetLblRowDesc = new string[] { name, desc };
        }

        protected void addButton(Button ctrl, string id, int posCol, string text)
        {
            ctrl.Name = id;

            ctrl.Location = new System.Drawing.Point(1, 1);
            ctrl.Dock = DockStyle.Fill;
            ctrl.Text = text;
            //??? Идентификатор является позицией-столбцом
            this.Controls.Add(ctrl, 0, posCol);
            this.SetColumnSpan(ctrl, 1);
        }
        /// <summary>
        /// Добавить кнопку
        /// </summary>
        /// <param name="id">Идентификатор кнопки (свойство 'Name')</param>
        /// <param name="posCol">Позиция по вертикали</param>
        /// <param name="text">Подпись на кнопке</param>
        protected void addButton(string id, int posCol, string text)
        {
            Button ctrl = new Button();

            addButton(ctrl, id, posCol, text);
        }

        protected virtual void panelTepCommon_btnSave_onClick(object obj, EventArgs ev)
        {
            int err = -1;
            string errMsg = string.Empty;

            recUpdateInsertDelete(out err);

            if (!(err == 0))
            {
                errMsg = @"HPanelEdit::HPanelEdit_btnSave_Click () - DbTSQLInterface.RecUpdateInsertDelete () - ...";
            }
            else
            {
                successRecUpdateInsertDelete();

                if (!(delegateSaveAdding == null))
                    delegateSaveAdding();
                else
                    ;
            }

            if (!(err == 0))
            {
                throw new Exception(@"HPanelEdit::HPanelEdit_btnSave_Click () - " + errMsg);
            }
            else
                ;
        }

        protected abstract void recUpdateInsertDelete(out int err);

        protected abstract void successRecUpdateInsertDelete();

        protected virtual void panelTepCommon_btnUpdate_onClick(object obj, EventArgs ev)
        {
            reinit();
        }
    }
}
