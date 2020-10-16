using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaserCode
{
    public class DgvDetail
    {
        public DgvDetail(DataGridView dgv)
        {
            _dgv = dgv;
            _list_Detail = new List<DetailInfo>();
        }

        private DataGridView _dgv;
        private List<DetailInfo> _list_Detail = null;

        public void DgvDetailAdd(string strName, string strValue)
        {
            _list_Detail.Add(new DetailInfo(strName, strValue));
        }

        public void DgvDetailRefresh()
        {
            _dgv.DataSource = _list_Detail;
            _dgv.Refresh();
        }

        public void DgvDetailClear()
        {
            _list_Detail.Clear();
        }
    }

    public class DetailInfo
    {
        public DetailInfo(string strName, string strValue)
        {
            Name = strName;
            Value = strValue;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
