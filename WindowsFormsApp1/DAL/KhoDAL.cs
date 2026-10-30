using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WindowsFormsApp1.DTO;

namespace WindowsFormsApp1.DAL
{
    public class KhoDAL
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InventoryDB;Integrated Security=True;Encrypt=False";

        // 1. Lấy danh sách toàn bộ vật tư
        public List<VatTuDTO> GetAllVatTu()
        {
            List<VatTuDTO> list = new List<VatTuDTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MaVatTu, TenVatTu, DVT, SoLuongTon FROM VatTu";
                SqlCommand cmd = new SqlCommand(query, conn);
                 conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new VatTuDTO
                    {
                        MaVatTu = reader["MaVatTu"].ToString(),
                        TenVatTu = reader["TenVatTu"].ToString(),
                        DVT = reader["DVT"].ToString(),
                        SoLuongTon = Convert.ToDecimal(reader["SoLuongTon"])
                    });
                }
            }
            return list;
        }

        // 2. Tìm kiếm vật tư bằng SqlParameter (Chống SQL Injection 100%)
        public List<VatTuDTO> SearchVatTu(string keyword)
        {
            List<VatTuDTO> list = new List<VatTuDTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MaVatTu, TenVatTu, DVT, SoLuongTon FROM VatTu WHERE MaVatTu LIKE @kw OR TenVatTu LIKE @kw";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new VatTuDTO
                    {
                        MaVatTu = reader["MaVatTu"].ToString(),
                        TenVatTu = reader["TenVatTu"].ToString(),
                        DVT = reader["DVT"].ToString(),
                        SoLuongTon = Convert.ToDecimal(reader["SoLuongTon"])
                    });
                }
            }
            return list;
        }

        // 3. Tính tổng giá trị tồn kho bằng ExecuteScalar
        public decimal GetTongGiaTriTonKho()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Giả định tính tổng theo giá trung bình hoặc số lượng tồn nhân đơn giá nhập gần nhất
                string query = "SELECT ISNULL(SUM(SoLuongTon), 0) FROM VatTu";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        // 4. Lấy danh sách vật tư dưới ngưỡng an toàn
        public List<VatTuDTO> GetVatTuDuoiNguong(decimal nguong)
        {
            List<VatTuDTO> list = new List<VatTuDTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MaVatTu, TenVatTu, DVT, SoLuongTon FROM VatTu WHERE SoLuongTon < @Nguong";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nguong", nguong);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new VatTuDTO
                    {
                        MaVatTu = reader["MaVatTu"].ToString(),
                        TenVatTu = reader["TenVatTu"].ToString(),
                        DVT = reader["DVT"].ToString(),
                        SoLuongTon = Convert.ToDecimal(reader["SoLuongTon"])
                    });
                }
            }
            return list;
        }

        // 5. THỰC THI GIAO DỊCH LẬP PHIẾU KHO (SqlTransaction - Chuẩn ACID)
        public bool LuuPhieuKhoTransaction(PhieuKhoDTO phieu, out string errorMessage)
        {
            errorMessage = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Bắt đầu Transaction
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // Bước 1: Chèn vào bảng PhieuKho và lấy MaPhieu vừa tạo (SCOPE_IDENTITY)
                    string sqlPhieu = @"INSERT INTO PhieuKho (LoaiPhieu, NgayLap, MaNCC, TongTien) 
                                        VALUES (@LoaiPhieu, @NgayLap, @MaNCC, @TongTien);
                                        SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdPhieu = new SqlCommand(sqlPhieu, conn, trans);
                    cmdPhieu.Parameters.AddWithValue("@LoaiPhieu", phieu.LoaiPhieu);
                    cmdPhieu.Parameters.AddWithValue("@NgayLap", phieu.NgayLap);
                    cmdPhieu.Parameters.AddWithValue("@MaNCC", (object)phieu.MaNCC ?? DBNull.Value);
                    cmdPhieu.Parameters.AddWithValue("@TongTien", phieu.TongTien);

                    int maPhieuMoi = Convert.ToInt32(cmdPhieu.ExecuteScalar());

                    // Bước 2 & 3: Lặp qua danh sách chi tiết để chèn và Cập nhật Tồn kho
                    foreach (var item in phieu.ChiTiets)
                    {
                        // 2. Chèn bản ghi ChiTietPhieuKho
                        string sqlCT = @"INSERT INTO ChiTietPhieuKho (MaPhieu, MaVatTu, SoLuong, DonGia)
                                         VALUES (@MaPhieu, @MaVatTu, @SoLuong, @DonGia)";
                        SqlCommand cmdCT = new SqlCommand(sqlCT, conn, trans);
                        cmdCT.Parameters.AddWithValue("@MaPhieu", maPhieuMoi);
                        cmdCT.Parameters.AddWithValue("@MaVatTu", item.MaVatTu);
                        cmdCT.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                        cmdCT.Parameters.AddWithValue("@DonGia", item.DonGia);
                        cmdCT.ExecuteNonQuery();

                        // 3. Cập nhật tồn kho theo loại phiếu
                        if (phieu.LoaiPhieu == "NHAP")
                        {
                            string sqlUpdateTon = "UPDATE VatTu SET SoLuongTon = SoLuongTon + @SoLuong WHERE MaVatTu = @MaVatTu";
                            SqlCommand cmdUpdate = new SqlCommand(sqlUpdateTon, conn, trans);
                            cmdUpdate.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmdUpdate.Parameters.AddWithValue("@MaVatTu", item.MaVatTu);
                            cmdUpdate.ExecuteNonQuery();
                        }
                        else if (phieu.LoaiPhieu == "XUAT")
                        {
                            // Kiểm tra số lượng tồn kho trước khi trừ
                            string sqlCheck = "SELECT SoLuongTon FROM VatTu WHERE MaVatTu = @MaVatTu";
                            SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, trans);
                            cmdCheck.Parameters.AddWithValue("@MaVatTu", item.MaVatTu);

                            decimal tonHienTai = Convert.ToDecimal(cmdCheck.ExecuteScalar());

                            if (tonHienTai < item.SoLuong)
                            {
                                // ROLLBACK ngay lập tức nếu thiếu hàng
                                trans.Rollback();
                                errorMessage = $"Không đủ hàng trong kho cho vật tư {item.MaVatTu}! (Tồn: {tonHienTai}, Xuất: {item.SoLuong})";
                                return false;
                            }

                            string sqlUpdateTon = "UPDATE VatTu SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaVatTu = @MaVatTu";
                            SqlCommand cmdUpdate = new SqlCommand(sqlUpdateTon, conn, trans);
                            cmdUpdate.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmdUpdate.Parameters.AddWithValue("@MaVatTu", item.MaVatTu);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }

                    // Bước 4: Commit nếu tất cả công việc thành công
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    // Rollback nếu có bất kỳ lỗi hệ thống nào xảy ra
                    trans.Rollback();
                    errorMessage = "Lỗi hệ thống: " + ex.Message;
                    return false;
                }
            }
        }
    }
}