using System;
using System.Data;
using System.Data.SqlClient;
using WindowsFormsApp1.DAL;
using WindowsFormsApp1.DTO;

namespace WindowsFormsApp1.BUS
{
    public class HotelBUS
    {
        private HotelDAL dal = new HotelDAL();

        public DataTable GetAllPhong() => dal.GetAllPhongTable();
        public DataTable GetAllDichVu() => dal.GetAllDichVu();
        public KhachHangDTO GetKhachHangByCCCD(string cccd) => dal.GetKhachHangByCCCD(cccd);

        public bool CheckIn(string soPhong, string cccd, string hoTen, string sdt)
        {
            KhachHangDTO kh = dal.GetKhachHangByCCCD(cccd);
            int maKH = kh == null ? dal.AddKhachHang(new KhachHangDTO { CCCD = cccd, HoTen = hoTen, SoDienThoai = sdt }) : kh.MaKH;
            return dal.CheckIn(soPhong, maKH);
        }

        public bool OrderDichVu(string soPhong, int maDV, int soLuong)
        {
            int maPhieu = dal.GetMaPhieuHienTai(soPhong);
            if (maPhieu == 0) return false;
            return dal.AddSuDungDichVu(maPhieu, maDV, soLuong);
        }

        public decimal TinhTongTien(string soPhong, out int maPhieu)
        {
            maPhieu = dal.GetMaPhieuHienTai(soPhong);
            if (maPhieu == 0) return 0;

            decimal tongTien = 0;
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                // 1. Tiền phòng = Giờ ở * Giá/giờ
                string qPhong = @"SELECT P.GiaTheoGio, PDP.NgayNhanPhong 
                                 FROM PhieuDatPhong PDP 
                                 JOIN Phong P ON PDP.SoPhong = P.SoPhong 
                                 WHERE PDP.MaPhieu = @MaPhieu";
                SqlCommand cmdP = new SqlCommand(qPhong, conn);
                cmdP.Parameters.AddWithValue("@MaPhieu", maPhieu);
                using (SqlDataReader reader = cmdP.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal giaGio = Convert.ToDecimal(reader["GiaTheoGio"]);
                        DateTime ngayNhan = Convert.ToDateTime(reader["NgayNhanPhong"]);
                        double gioO = Math.Max(1, Math.Ceiling((DateTime.Now - ngayNhan).TotalHours));
                        tongTien += (decimal)gioO * giaGio;
                    }
                }

                // 2. Tiền dịch vụ
                string qDV = @"SELECT ISNULL(SUM(SD.SoLuong * DV.DonGia), 0) 
                              FROM SuDungDichVu SD 
                              JOIN DichVu DV ON SD.MaDV = DV.MaDV 
                              WHERE SD.MaPhieu = @MaPhieu";
                SqlCommand cmdDV = new SqlCommand(qDV, conn);
                cmdDV.Parameters.AddWithValue("@MaPhieu", maPhieu);
                tongTien += Convert.ToDecimal(cmdDV.ExecuteScalar());
            }

            return tongTien;
        }

        public bool CheckOut(string soPhong)
        {
            int maPhieu;
            decimal tongTien = TinhTongTien(soPhong, out maPhieu);
            if (maPhieu == 0) return false;
            return dal.CheckOutTransaction(maPhieu, soPhong, tongTien);
        }
    }
}