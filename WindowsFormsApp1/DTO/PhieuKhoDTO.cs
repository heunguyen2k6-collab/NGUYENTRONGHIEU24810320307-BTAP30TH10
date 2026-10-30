using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.DTO
{
    public class PhieuKhoDTO
    {
        public int MaPhieu { get; set; }
        public string LoaiPhieu { get; set; } // "NHAP" hoặc "XUAT"
        public DateTime NgayLap { get; set; }
        public int? MaNCC { get; set; } // Nullable nếu là phiếu xuất
        public decimal TongTien { get; set; }

        // Danh sách các chi tiết đi kèm phiếu
        public List<ChiTietPhieuKhoDTO> ChiTiets { get; set; } = new List<ChiTietPhieuKhoDTO>();
    }
}