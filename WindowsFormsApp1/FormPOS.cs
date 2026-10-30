using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CafePOS
{
    public partial class FormPOS : Form
    {
        private int currentMaBan = -1;
        private string currentTenBan = "";

        public FormPOS()
        {
            InitializeComponent();

            // Sửa lỗi: Tải dữ liệu trước khi gán event để không bị lỗi DataRowView
            LoadDanhMuc();
            LoadTableLayout();

            // Gán sự kiện tập trung tại đây (đảm bảo trong Form Designer không bị gán trùng)
            cboDanhMuc.SelectedIndexChanged += cboDanhMuc_SelectedIndexChanged;
            btnThemMon.Click += btnThemMon_Click;
            btnThanhToan.Click += btnThanhToan_Click;
        }

        private void LoadTableLayout()
        {
            flpTableLayout.Controls.Clear();
            DataTable dtBan = DatabaseHelper.ExecuteQuery("SELECT * FROM BanAn");

            foreach (DataRow row in dtBan.Rows)
            {
                Button btn = new Button
                {
                    Width = 90,
                    Height = 90,
                    Text = $"{row["TenBan"]}\n({row["TrangThai"]})",
                    Tag = row["MaBan"],
                    Font = new Font("Arial", 9, FontStyle.Bold)
                };

                string trangThai = row["TrangThai"].ToString();
                if (trangThai == "Trống")
                {
                    btn.BackColor = Color.LightGreen;
                }
                else
                {
                    btn.BackColor = Color.Tomato;
                    btn.ForeColor = Color.White;
                }

                btn.Click += BtnTable_Click;
                flpTableLayout.Controls.Add(btn);
            }
        }

        private void BtnTable_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            currentMaBan = Convert.ToInt32(btn.Tag);
            currentTenBan = btn.Text.Split('\n')[0];
            this.Text = $"Quản lý bán hàng - {currentTenBan}";

            LoadHoaDonBan(currentMaBan);
        }

        private void LoadHoaDonBan(int maBan)
        {
            string query = @"
                SELECT m.TenMon, ct.SoLuong, ct.DonGia, (ct.SoLuong * ct.DonGia) AS ThanhTien
                FROM HoaDon hd
                JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                JOIN MonAn m ON ct.MaMon = m.MaMon
                WHERE hd.MaBan = @MaBan AND hd.TrangThai = 0";

            DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@MaBan", maBan) });
            dgvHoaDon.DataSource = dt;

            decimal tongTien = 0;
            foreach (DataRow row in dt.Rows)
            {
                tongTien += Convert.ToDecimal(row["ThanhTien"]);
            }
            lblTongTien.Text = $"Tổng tiền: {tongTien:N0} VNĐ";
        }

        private void LoadDanhMuc()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM DanhMuc");

            // Sửa lỗi: Gán Display/Value Member trước khi gán DataSource
            cboDanhMuc.DisplayMember = "TenDanhMuc";
            cboDanhMuc.ValueMember = "MaDanhMuc";
            cboDanhMuc.DataSource = dt;

            // Load món ăn cho danh mục đầu tiên
            if (cboDanhMuc.SelectedValue != null && cboDanhMuc.SelectedValue is int maDM)
            {
                LoadMonAnByDanhMuc(maDM);
            }
        }

        private void LoadMonAnByDanhMuc(int maDanhMuc)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM MonAn WHERE MaDanhMuc = @MaDM",
                new SqlParameter[] { new SqlParameter("@MaDM", maDanhMuc) }
            );

            cboMonAn.DisplayMember = "TenMon";
            cboMonAn.ValueMember = "MaMon";
            cboMonAn.DataSource = dt;
        }

        private void cboDanhMuc_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Sửa lỗi: Kiểm tra kiều dữ liệu 'is int' để tránh ép kiểu ép vội DataRowView
            if (cboDanhMuc.SelectedValue != null && cboDanhMuc.SelectedValue is int maDM)
            {
                LoadMonAnByDanhMuc(maDM);
            }
        }

        private void btnThemMon_Click(object sender, EventArgs e)
        {
            if (currentMaBan == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboMonAn.SelectedValue == null || !(cboMonAn.SelectedValue is int maMon))
            {
                MessageBox.Show("Vui lòng chọn món ăn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int soLuong = (int)nudSoLuong.Value;

            object objGia = DatabaseHelper.ExecuteScalar("SELECT DonGia FROM MonAn WHERE MaMon = @MaMon",
                new SqlParameter[] { new SqlParameter("@MaMon", maMon) });
            decimal donGia = Convert.ToDecimal(objGia);

            object objMaHD = DatabaseHelper.ExecuteScalar("SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0",
                new SqlParameter[] { new SqlParameter("@MaBan", currentMaBan) });

            int maHoaDon = 0;
            if (objMaHD == null)
            {
                string queryInsertHD = "INSERT INTO HoaDon (MaBan, GioVao, TrangThai) VALUES (@MaBan, GETDATE(), 0); SELECT SCOPE_IDENTITY();";
                maHoaDon = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryInsertHD, new SqlParameter[] { new SqlParameter("@MaBan", currentMaBan) }));

                DatabaseHelper.ExecuteNonQuery("UPDATE BanAn SET TrangThai = N'Có khách' WHERE MaBan = @MaBan",
                    new SqlParameter[] { new SqlParameter("@MaBan", currentMaBan) });
            }
            else
            {
                maHoaDon = Convert.ToInt32(objMaHD);
            }

            string checkCT = "SELECT SoLuong FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD AND MaMon = @MaMon";
            object objSL = DatabaseHelper.ExecuteScalar(checkCT, new SqlParameter[] {
                new SqlParameter("@MaHD", maHoaDon),
                new SqlParameter("@MaMon", maMon)
            });

            if (objSL != null)
            {
                int slCu = Convert.ToInt32(objSL);
                string updateCT = "UPDATE ChiTietHoaDon SET SoLuong = @SLMoi WHERE MaHoaDon = @MaHD AND MaMon = @MaMon";
                DatabaseHelper.ExecuteNonQuery(updateCT, new SqlParameter[] {
                    new SqlParameter("@SLMoi", slCu + soLuong),
                    new SqlParameter("@MaHD", maHoaDon),
                    new SqlParameter("@MaMon", maMon)
                });
            }
            else
            {
                string insertCT = "INSERT INTO ChiTietHoaDon (MaHoaDon, MaMon, SoLuong, DonGia) VALUES (@MaHD, @MaMon, @SL, @DonGia)";
                DatabaseHelper.ExecuteNonQuery(insertCT, new SqlParameter[] {
                    new SqlParameter("@MaHD", maHoaDon),
                    new SqlParameter("@MaMon", maMon),
                    new SqlParameter("@SL", soLuong),
                    new SqlParameter("@DonGia", donGia)
                });
            }

            LoadTableLayout();
            LoadHoaDonBan(currentMaBan);
        }

        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (currentMaBan == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object objMaHD = DatabaseHelper.ExecuteScalar("SELECT MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0",
                new SqlParameter[] { new SqlParameter("@MaBan", currentMaBan) });

            if (objMaHD == null)
            {
                MessageBox.Show("Bàn này chưa có hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int maHoaDon = Convert.ToInt32(objMaHD);

            object objTongTien = DatabaseHelper.ExecuteScalar(@"
                SELECT SUM(SoLuong * DonGia) 
                FROM ChiTietHoaDon 
                WHERE MaHoaDon = @MaHD", new SqlParameter[] { new SqlParameter("@MaHD", maHoaDon) });

            decimal tongTien = objTongTien != DBNull.Value ? Convert.ToDecimal(objTongTien) : 0;

            if (MessageBox.Show($"Xác nhận thanh toán cho {currentTenBan}?\nTổng tiền: {tongTien:N0} VNĐ",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string updateHD = @"UPDATE HoaDon 
                                           SET GioRa = GETDATE(), TongTien = @TongTien, TrangThai = 1 
                                           WHERE MaHoaDon = @MaHD";
                        using (SqlCommand cmd1 = new SqlCommand(updateHD, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@TongTien", tongTien);
                            cmd1.Parameters.AddWithValue("@MaHD", maHoaDon);
                            cmd1.ExecuteNonQuery();
                        }

                        string updateBan = "UPDATE BanAn SET TrangThai = N'Trống' WHERE MaBan = @MaBan";
                        using (SqlCommand cmd2 = new SqlCommand(updateBan, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@MaBan", currentMaBan);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        currentMaBan = -1;
                        this.Text = "Quản lý bán hàng";
                        lblTongTien.Text = "Tổng tiền: 0 VNĐ";

                        LoadTableLayout();
                        dgvHoaDon.DataSource = null;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}