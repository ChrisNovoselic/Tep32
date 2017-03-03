using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms; //DataGridView
using System.Data.Common; //DbConnection
//using System.Drawing;
using System.Data; //DataTable

using HClassLibrary;
using TepCommon;
using InterfacePlugIn;

namespace PluginProject
{
    public class PanelPrjSources : HPanelEditList
    {
        private static int s_iIdSourceData = 501;
        private static string s_strPswdPropName = @"PASSWORD";
        private DataTable m_tblOriginPswd, m_tblEditPswd;

        public PanelPrjSources(IPlugIn iFunc)
            : base(iFunc, @"SOURCE", @"ID", @"NAME_SHR")
        {
            InitializeComponent();
        }

        private DataGridView m_dgvProp
        {
            get { return ((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_PROP.ToString(), true)[0]); }
        }
        /// <summary>
        /// Инициализация элементов управления объекта (создание, размещение)
        /// </summary>
        private void InitializeComponent()
        {
            m_dgvProp.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(HPanelTepPrjSources_EditingControlShowing);
            //m_dgvProp.CellValueChanged += new DataGridViewCellEventHandler(HPanelTepPrjSources_CellValueChanged);
            m_dgvProp.CellFormatting += new DataGridViewCellFormattingEventHandler(HPanelTepPrjSources_CellFormatting);
        }

        private void initTablePswd ()
        {
            m_tblOriginPswd = m_tblEditPswd.Copy();

            //!!!Дешифрация паролей
            foreach (DataRow r in m_tblEditPswd.Rows)
                r["HASH"] = Crypt.Crypting().Decrypt(r["HASH"].ToString(), Crypt.KEY);
        }

        protected override void initProp(out int err, out string errMsg)
        {
            base.initProp(out err, out errMsg);

            if (err == 0)
            {
                m_tblEditPswd = m_handlerDb.Select (@"SELECT * FROM passwords WHERE ID_ROLE=" + s_iIdSourceData, out err);
                initTablePswd ();

                m_dgvProp.Rows.Add(new object[] { s_strPswdPropName, string.Empty });
                //DataGridViewTextBoxCell cell;
                //cell = new DataGridViewPasswordTextBoxCell();
                //m_dgvProp.Rows[m_dgvProp.RowCount - 1].Cells[1] = new DataGridViewPasswordTextBoxCell();
                //cell = m_dgvProp.Rows[m_dgvProp.RowCount - 1].Cells[1] as DataGridViewTextBoxCell;
            }
            else
            {
            }
        }

        private object[] getRecItemValues(string pswd)
        {
            //??? Идентификатор из крайней строки... ??? по индексу
            int indx = (Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0] as DataGridView).SelectedRows[0].Index;

            return new object[] {
                                    //Int32.Parse (m_tblEdit.Rows[m_tblEdit.Rows.Count - 1][@"ID"].ToString ().Trim ())
                                    Int32.Parse (m_tblEdit.Rows[indx][@"ID"].ToString ().Trim ())
                                    , s_iIdSourceData
                                    , pswd };
        }

        protected override void addRecItem(object [] vals)
        {
            base.addRecItem(vals);

            m_tblEditPswd.Rows.Add(getRecItemValues (string.Empty));
        }

        protected override void delRecItem(int indx)
        {
            DataRow rowPswd = getPassword(indx);

            if (!(rowPswd == null))
            {
                m_tblEditPswd.Rows.Remove(rowPswd);
                m_tblEditPswd.AcceptChanges();
            }
            else
                ;

            base.delRecItem(indx);
        }

        protected override void reinit()
        {
            m_tblEditPswd.Clear();
            m_tblOriginPswd.Clear();

            base.reinit();
        }

        private DataRow getPassword(int indx)
        {
            if ((!(m_tblEdit == null))
                && (!(m_tblEditPswd == null))
                && (indx < m_tblEdit.Rows.Count)
                )
            {
                DataRow[] rowsPswd = m_tblEditPswd.Select(@"ID_EXT=" + m_tblEdit.Rows[indx][@"ID"]
                    //+ @" AND " + @"ID_ROLE=" + s_iIdSourceData
                         );

                if (rowsPswd.Length > 1)
                    //??? Ошибка...
                    throw new Exception(@"HPanelTepPrjSources::getPassword (indx=" + indx + @") - ...");
                else
                    if (rowsPswd.Length == 1)
                        return rowsPswd[0];
                    else
                        ;
            }
            else
                ;

            return null;
        }

        protected override string getTableEditValue(DataGridView dgvProp, int indxItem, int indxProp)
        {
            string strRes = string.Empty;

            //if (indxProp == m_dgvProp.RowCount - 1)
            if (m_dgvProp.Rows[indxProp].Cells[0].Value.ToString().Trim().Equals(s_strPswdPropName) == true) 
            {//Возвратиить пароль...
                DataRow rowPswd = getPassword(indxItem);
                if (!(rowPswd == null))
                    strRes = rowPswd[@"HASH"].ToString().Trim();
                else
                    ; //Оставить 'Empty'
            }
            else
            {//Стандартная обработка...
                strRes = base.getTableEditValue(m_dgvProp, indxItem, indxProp);
            }

            return strRes;
        }

        //Для редактирования свойства
        protected override void setTableEditValue(DataGridView dgvProp, int indxRow, int indxCol)
        {
            if (indxRow == m_dgvProp.RowCount - 1)
            {//Обработка окончания редактирования строки пароля...
                DataRow rowPswd = getPassword(((DataGridView)Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0]).SelectedRows[0].Index);
                string pswd = m_dgvProp.Rows[indxRow].Cells[indxCol].Value.ToString ().Trim ();
                if (!(rowPswd == null))
                    rowPswd[@"HASH"] = pswd;
                else
                {
                    m_tblEditPswd.Rows.Add(getRecItemValues(pswd));
                    //m_tblEditPswd.AcceptChanges();
                }

                Console.WriteLine(@"HPanelTepPrjSources::setTableEditValue () - ...");
            }
            else
            {                
                //??? Индекс строки - признак строки с 'ID' ...
                if (indxRow == 0)
                {//Изменен 'ID': в 'm_tblEdit' - "старый", в 'DataGridView' - "новый"

                }
                else
                    ;

                base.setTableEditValue(m_dgvProp, indxRow, indxCol);
            }
        }

        private void HPanelTepPrjSources_EditingControlShowing(object obj, DataGridViewEditingControlShowingEventArgs ev)
        {
            if ((m_dgvProp.SelectedRows [0].Index == m_dgvProp.RowCount - 1)
                //&& (m_dgvProp.SelectedCells [0].ColumnIndex == 1)
                )
            {
                TextBox pswd = ev.Control as TextBox;
                ////Вариант №1
                //if (pswd.UseSystemPasswordChar == false)
                //    pswd.UseSystemPasswordChar = true;
                //else
                //    ;
                //Вариант №2
                if (pswd.PasswordChar == 0)
                    pswd.PasswordChar = '#';
                else
                    ;                
            }
            else
                ;
        }

        //private void HPanelTepPrjSources_CellValueChanged(object obj, DataGridViewCellEventArgs ev)
        //{
        //    if ((ev.RowIndex == m_dgvProp.RowCount - 1)
        //        && (m_dgvProp.Rows[ev.RowIndex].Cells[0].Value.Equals (s_strPswdPropName) == true)
        //        )
        //    {
        //        ((DataGridViewPasswordTextBoxCell)m_dgvProp.Rows[ev.RowIndex].Cells[1]).SetValue ();
        //    }
        //    else
        //    {
        //    }
        //}

        private void HPanelTepPrjSources_CellFormatting(object obj, DataGridViewCellFormattingEventArgs ev)
        {
            if ((ev.RowIndex == m_dgvProp.RowCount - 1)
                && (ev.ColumnIndex == 1))
            {
                DataGridView dgv = Controls.Find(INDEX_CONTROL.DGV_DICT_ITEM.ToString(), true)[0] as DataGridView;

                if (dgv.SelectedRows.Count == 1)
                {
                    DataRow rowPswd = getPassword(dgv.SelectedRows[0].Index);

                    int lPswd = -1;
                    if (! (rowPswd == null))
                    {
                        lPswd = rowPswd[@"HASH"].ToString ().Trim ().Length;
                        if (lPswd > 0)
                        {
                            ev.Value = new string('#', lPswd);                            
                        }
                        else
                            ; //Длина пароля == 0
                    }
                    else
                        ; //Пароь не найден

                    Console.WriteLine(@"HPanelTepPrjSources_CellFormatting () - пароль=" + (rowPswd == null ? @"Нет" : @"Да") + @", длина=" + lPswd);
                }
                else
                    ; //Выделена НЕ одна (0 или более) словарная величина...
            }
            else
            {
            }
        }

        protected override void recUpdateInsertDelete(out int err)
        {
            //!!!Шифрация паролей
            foreach (DataRow r in m_tblEditPswd.Rows)
                r["HASH"] = Crypt.Crypting().Encrypt(r["HASH"].ToString().Trim(), Crypt.KEY);

            m_handlerDb.RecUpdateInsertDelete(@"passwords", @"ID_EXT, ID_ROLE", string.Empty, m_tblOriginPswd, m_tblEditPswd, out err);

            if (err == 0)
                base.recUpdateInsertDelete(out err);
            else
                throw new Exception(@"HPanelTepPrjSources::recUpdateInsertDelete () - err=" + err + @" ...");
        }

        protected override void successRecUpdateInsertDelete()
        {
            initTablePswd();

            base.successRecUpdateInsertDelete();
        }
    }
}
