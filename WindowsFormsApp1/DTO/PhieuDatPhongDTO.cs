using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.DTO
{
    public class PhieuDatPhongDTO
    {
        public int MaPhieu { get; set; }
        public string SoPhong { get; set; }
        public int MaKH { get; set; }
        public DateTime NgayNhanPhong { get; set; }
        public DateTime? NgayTraPhong { get; set; }
        public decimal TongTienThanhToan { get; set; }
    }
}
