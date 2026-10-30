using System;
using System.Data;
using System.Data.SqlClient;
using WindowsFormsApp1.DTO;

namespace WindowsFormsApp1.DAL
{
    public class HotelDAL
    {
        // Lấy danh sách phòng dạng DataTable phục vụ tìm kiếm ngắt kết nối
        public DataTable GetAllPhongTable()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = "SELECT * FROM Phong";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(dt);
            }
            return dt;
        }

        // Tra cứu khách hàng theo CCCD bằng SqlParameter
        public KhachHangDTO GetKhachHangByCCCD(string cccd)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM KhachHang WHERE CCCD = @CCCD";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CCCD", cccd);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new KhachHangDTO
                        {
                            MaKH = Convert.ToInt32(reader["MaKH"]),
                            HoTen = reader["HoTen"].ToString(),
                            CCCD = reader["CCCD"].ToString(),
                            SoDienThoai = reader["SoDienThoai"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // Thêm khách hàng mới
        public int AddKhachHang(KhachHangDTO kh)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO KhachHang (HoTen, CCCD, SoDienThoai) 
                                VALUES (@HoTen, @CCCD, @SoDienThoai); 
                                SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HoTen", kh.HoTen);
                cmd.Parameters.AddWithValue("@CCCD", kh.CCCD);
                cmd.Parameters.AddWithValue("@SoDienThoai", string.IsNullOrEmpty(kh.SoDienThoai) ? (object)DBNull.Value : kh.SoDienThoai);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Check-in (Transaction: Tạo phiếu + Cập nhật trạng thái phòng)
        public bool CheckIn(string soPhong, int maKH)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string qPhieu = "INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong) VALUES (@SoPhong, @MaKH, GETDATE())";
                    SqlCommand cmdPhieu = new SqlCommand(qPhieu, conn, transaction);
                    cmdPhieu.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmdPhieu.Parameters.AddWithValue("@MaKH", maKH);
                    cmdPhieu.ExecuteNonQuery();

                    string qPhong = "UPDATE Phong SET TrangThai = N'Đang ở' WHERE SoPhong = @SoPhong";
                    SqlCommand cmdPhong = new SqlCommand(qPhong, conn, transaction);
                    cmdPhong.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmdPhong.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        // Lấy mã phiếu chưa thanh toán của phòng
        public int GetMaPhieuHienTai(string soPhong)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT MaPhieu FROM PhieuDatPhong WHERE SoPhong = @SoPhong AND NgayTraPhong IS NULL";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SoPhong", soPhong);
                object res = cmd.ExecuteScalar();
                return res != null ? Convert.ToInt32(res) : 0;
            }
        }

        // Thêm sử dụng dịch vụ
        public bool AddSuDungDichVu(int maPhieu, int maDV, int soLuong)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO SuDungDichVu (MaPhieu, MaDV, SoLuong, ThoiGian) VALUES (@MaPhieu, @MaDV, @SoLuong, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                cmd.Parameters.AddWithValue("@MaDV", maDV);
                cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Lấy danh sách dịch vụ
        public DataTable GetAllDichVu()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = "SELECT * FROM DichVu";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(dt);
            }
            return dt;
        }

        // Check-out (Transaction: Cập nhật phiếu + Đổi trạng thái phòng thành 'Đang dọn dẹp')
        public bool CheckOutTransaction(int maPhieu, string soPhong, decimal tongTien)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string qPhieu = "UPDATE PhieuDatPhong SET NgayTraPhong = GETDATE(), TongTienThanhToan = @TongTien WHERE MaPhieu = @MaPhieu";
                    SqlCommand cmdPhieu = new SqlCommand(qPhieu, conn, transaction);
                    cmdPhieu.Parameters.AddWithValue("@TongTien", tongTien);
                    cmdPhieu.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    cmdPhieu.ExecuteNonQuery();

                    string qPhong = "UPDATE Phong SET TrangThai = N'Đang dọn dẹp' WHERE SoPhong = @SoPhong";
                    SqlCommand cmdPhong = new SqlCommand(qPhong, conn, transaction);
                    cmdPhong.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmdPhong.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}