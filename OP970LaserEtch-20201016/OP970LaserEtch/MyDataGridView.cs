using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OP970LaserEtch
{
    public partial class MyDataGridView : UserControl
    {
        public MyDataGridView()
        {
            InitializeComponent();

            dgv.AutoGenerateColumns = false;
        }

        public void InitializeGraph(string[] strDataName, string[] strHeaders, bool[] bReadOnly, bool bVisible = true)
        {
            dgv.Columns.Clear();

            for (int nIndex = 0; nIndex < strHeaders.Length; nIndex++)
            {
                dgv.Columns.Add(strDataName[nIndex], strHeaders[nIndex]);
                dgv.Columns[nIndex].DataPropertyName = strDataName[nIndex];
                dgv.Columns[nIndex].ReadOnly = bReadOnly[nIndex];
                //dgv.Columns[0].Visible = false;
            }

            if (!bVisible)
            {
                tsb_add.Visible = false;
                tsb_delete.Visible = false;
            }
            else
            {
                tsb_add.Visible = true;
                tsb_delete.Visible = true;
            }
        }

        private void dgv_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    if (dgv.Rows[e.RowIndex].Selected == false)
                    {
                        dgv.ClearSelection();
                        dgv.Rows[e.RowIndex].Selected = true;
                    }
                    if (dgv.SelectedRows.Count == 1)
                    {
                        dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    }
                }
            }
        }

        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
                _nWaitApply++;
                SetMessage("参数已修改，请及时提交");
            }
        }

        private void dgv_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentCell.Value != null && dgv.CurrentCell.Value.ToString() != "" && dgv.IsCurrentCellDirty)
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        #region public interface
        //修改待提交数
        private int _nWaitApply { get; set; }
        public void ClearWaitApply()
        {
            _nWaitApply = 0;
        }
        public void SetMessage(string strMsg)
        {
            tsb_msg.Text = strMsg + string.Format("，当前数据量：{0} 待提交数：", dgv.Rows.Count) + _nWaitApply;
        }

        public void IsOnlyRead()
        {
            toolStrip1.Visible = false;
            dgv.ReadOnly = true;
        }

        public void SetContextMenu(ContextMenuStrip cm)
        {
            dgv.ContextMenuStrip = cm;
        }
        #endregion

        private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            SetMessage("DGV_ERROR");
        }

        private void tsb_add_Click(object sender, EventArgs e)
        {

        }
    }
}
