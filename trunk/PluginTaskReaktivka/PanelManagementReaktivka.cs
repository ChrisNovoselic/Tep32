using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using TepCommon;
using System.Drawing;

namespace PluginTaskReaktivka
{
    partial class PanelTaskReaktivka
    {
        /// <summary>
        /// Панель элементов управления
        /// </summary>
        protected class PanelManagementReaktivka : HPanelTepCommon.PanelManagementTaskCalculate
        {
            /// <summary>
            /// 
            /// </summary>
            public enum INDEX_CONTROL
            {
                UNKNOWN = -1
                , BUTTON_SAVE, BUTTON_LOAD, BUTTON_EXPORT
                , MENUITEM_UPDATE, MENUITEM_HISTORY
                , CLBX_COMP_VISIBLED
                , CHKBX_ENABLED_DATAGRIDVIEW_VALUES
                    , COUNT
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

            public PanelManagementReaktivka()
                : base(ModeTimeControlPlacement.Twin | ModeTimeControlPlacement.Labels)
            {
                InitializeComponents();
            }
            /// <summary>
            /// Инициализация элементов управления объекта (создание, размещение)
            /// </summary>
            private void InitializeComponents()
            {
                //initializeLayoutStyle();
                Control ctrl = new Control(); ;
                // переменные для инициализации кнопок "Добавить", "Удалить"
                string strPartLabelButtonDropDownMenuItem = string.Empty;
                int posRow = -1 // позиция по оси "X" при позиционировании элемента управления
                    , indx = -1; // индекс п. меню для кнопки "Обновить-Загрузить"    
                                 //int posColdgvTEPValues = 6;
                SuspendLayout();

                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                posRow = 6;
                //Кнопки обновления/сохранения, импорта/экспорта
                //Кнопка - обновить
                ctrl = new DropDownButton();
                ctrl.Name = INDEX_CONTROL.BUTTON_LOAD.ToString();
                ctrl.ContextMenuStrip = new ContextMenuStrip();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Входные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_UPDATE.ToString();
                indx = ctrl.ContextMenuStrip.Items.Add(new ToolStripMenuItem(@"Архивные значения"));
                ctrl.ContextMenuStrip.Items[indx].Name = INDEX_CONTROL.MENUITEM_HISTORY.ToString();
                ctrl.Text = @"Загрузить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); SetRowSpan(ctrl, 1);
                //Кнопка - сохранить
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_SAVE.ToString();
                ctrl.Text = @"Сохранить";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, ColumnCount / 2, posRow);
                SetColumnSpan(ctrl, ColumnCount / 2); SetRowSpan(ctrl, 1);
                //
                ctrl = new Button();
                ctrl.Name = INDEX_CONTROL.BUTTON_EXPORT.ToString();
                ctrl.Text = @"Экспорт";
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, 4); //SetRowSpan(ctrl, 1);

                //Признаки включения/исключения для отображения
                //Признак для включения/исключения для отображения компонента
                ctrl = new Label();
                ctrl.Dock = DockStyle.Bottom;
                (ctrl as Label).Text = @"Включить/исключить компонент для отображения";
                this.Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); //SetRowSpan(ctrl, 1);
                //
                ctrl = new CheckedListBoxTaskReaktivka();
                ctrl.Name = INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as CheckedListBoxTaskReaktivka).CheckOnClick = true;
                Controls.Add(ctrl, 0, posRow = posRow + 1);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 4);
                //Признак Корректировка_включена/корректировка_отключена 
                ctrl = new CheckBox();
                ctrl.Name = INDEX_CONTROL.CHKBX_ENABLED_DATAGRIDVIEW_VALUES.ToString();
                ctrl.Text = @"Корректировка значений разрешена";
                ctrl.Dock = DockStyle.Fill;
                ctrl.Enabled = false;
                (ctrl as CheckBox).Checked = true;
                this.Controls.Add(ctrl, 0, posRow = posRow + 4);
                SetColumnSpan(ctrl, ColumnCount); SetRowSpan(ctrl, 1);

                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            /// Класс для размещения элементов (компонентов станции, параметров расчета) с признаком "Использовать/Не_использовать"
            /// </summary>
            protected class CheckedListBoxTaskReaktivka : CheckedListBox, IControl
            {
                int[] arItem;
                /// <summary>
                /// Список для хранения идентификаторов переменных
                /// </summary>
                private List<int> m_listId;

                public CheckedListBoxTaskReaktivka()
                    : base()
                {
                    m_listId = new List<int>();
                }

                /// <summary>
                /// Идентификатор выбранного элемента списка
                /// </summary>
                public int SelectedId { get { return m_listId[SelectedIndex]; } }

                /// <summary>
                /// Добавить элемент в список
                /// </summary>
                /// <param name="text">Текст подписи элемента</param>
                /// <param name="id">Идентификатор элемента</param>
                /// <param name="bChecked">Значение признака "Использовать/Не_использовать"</param>
                public void AddItem(int id, string text, bool bChecked)
                {
                    Items.Add(text, bChecked);
                    m_listId.Add(id);
                }

                /// <summary>
                /// Удалить все элементы в списке
                /// </summary>
                public void ClearItems()
                {
                    Items.Clear();
                    m_listId.Clear();
                }

                /// <summary>
                /// Возвратить наименование элемента по идентификатору
                /// </summary>
                /// <param name="id"><Идентификатор элемента/param>
                /// <returns>Наименованеи элемента</returns>
                public string GetNameItem(int id)
                {
                    string strRes = string.Empty;

                    strRes = (string)Items[m_listId.IndexOf(id)];

                    return strRes;
                }
            }

            /// <summary>
            /// Найти элемент управления на панели идентификатору
            /// </summary>
            /// <param name="indxCtrl">Идентификатор элемента управления</param>
            /// <returns>элемент панели</returns>
            protected Control find(INDEX_CONTROL indxCtrl)
            {
                return find(indxCtrl.ToString());
            }

            /// <summary>
            /// Очистить
            /// </summary>
            public override void Clear()
            {
                base.Clear();

                INDEX_CONTROL[] arIndxControlToClear = new INDEX_CONTROL[] { INDEX_CONTROL.CLBX_COMP_VISIBLED };

                Clear(arIndxControlToClear);
            }

            /// <summary>
            /// Очистить все группы элементов управления, указанных в массиве
            /// </summary>
            /// <param name="arIndxToClear">Массив индексов в списке идентификаторов групп элементов управления</param>
            public void Clear(INDEX_CONTROL[] arIndxToClear)
            {
                for (int i = 0; i < arIndxToClear.Length; i++)
                    clear(arIndxToClear[i]);
            }

            /// <summary>
            /// Очистить элементы управления по индексу в списке идентификаторов групп элементов
            /// </summary>
            /// <param name="idToClear">Индекс в списке группы идентификаторов</param>
            private void clear(INDEX_CONTROL indxToClear)
            {
                (find(indxToClear) as IControl).ClearItems();
            }

            /// <summary>
            /// Обработчик события - изменение значения из списка признаков отображения/снятия_с_отображения
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список)</param>
            /// <param name="ev">Аргумент события</param>
            protected override void onItemCheck(object obj, EventArgs ev)
            {
                itemCheck((obj as IControl).SelectedId, ItemCheckedParametersEventArgs.TYPE.VISIBLE, (ev as ItemCheckEventArgs).NewValue);
            }

            protected override void activateControlChecked_onChanged(bool bActivate)
            {
                activateControlChecked_onChanged(new string[] { INDEX_CONTROL.CLBX_COMP_VISIBLED.ToString() }, bActivate);
            }
        }
    }
}
