using WindowsFormsApp1.BUS; // LƯU Ý: Đổi "QuanLyKhachSan" thành Tên Project của bạn
using WindowsFormsApp1.DTO; // LƯU Ý: Đổi "QuanLyKhachSan" thành Tên Project của bạn
using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1 // LƯU Ý: Đổi "QuanLyKhachSan" thành Tên Project của bạn
{
    public partial class Form1 : Form
    {
        private HotelBUS bus = new HotelBUS();
        private DataTable dtPhong;
        private DataView dvPhong;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDataPhong();
            LoadComboBoxDichVu();
        }

        private void LoadDataPhong()
        {
            dtPhong = bus.GetAllPhong();
            dvPhong = new DataView(dtPhong);
            dgvPhong.DataSource = dvPhong;
        }

        private void LoadComboBoxDichVu()
        {
            cboDichVu.DataSource = bus.GetAllDichVu();
            cboDichVu.DisplayMember = "TenDV";
            cboDichVu.ValueMember = "MaDV";
        }

        // Lọc danh sách phòng ngắt kết nối
        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string k = txtTimKiem.Text.Trim();
            dvPhong.RowFilter = $"LoaiPhong LIKE '%{k}%' OR TrangThai LIKE '%{k}%'";
        }

        // Tra cứu khách hàng cũ theo CCCD khi rời khỏi ô nhập
        private void txtCCCD_Leave(object sender, EventArgs e)
        {
            KhachHangDTO kh = bus.GetKhachHangByCCCD(txtCCCD.Text.Trim());
            if (kh != null)
            {
                txtHoTen.Text = kh.HoTen;
                txtSDT.Text = kh.SoDienThoai;
            }
        }

        // Xử lý Check-in
        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (bus.CheckIn(txtSoPhong.Text.Trim(), txtCCCD.Text.Trim(), txtHoTen.Text.Trim(), txtSDT.Text.Trim()))
            {
                MessageBox.Show("Check-in thành công!");
                LoadDataPhong();
            }
            else
            {
                MessageBox.Show("Check-in thất bại!");
            }
        }

        // Xử lý Gọi dịch vụ
        private void btnGoiDV_Click(object sender, EventArgs e)
        {
            int maDV = Convert.ToInt32(cboDichVu.SelectedValue);
            int soLuong = (int)nudSoLuong.Value;

            if (bus.OrderDichVu(txtSoPhong.Text.Trim(), maDV, soLuong))
            {
                MessageBox.Show("Gọi dịch vụ thành công!");
            }
            else
            {
                MessageBox.Show("Phòng chưa có khách ở!");
            }
        }

        // Xử lý Check-out
        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            string soPhong = txtSoPhong.Text.Trim();
            int maPhieu;
            decimal tongTien = bus.TinhTongTien(soPhong, out maPhieu);

            if (maPhieu == 0)
            {
                MessageBox.Show("Phòng không trong trạng thái thuê!");
                return;
            }

            if (MessageBox.Show($"Tổng tiền: {tongTien:N0} VNĐ. Xác nhận thanh toán?", "Check-out", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (bus.CheckOut(soPhong))
                {
                    MessageBox.Show("Thanh toán thành công! Phòng đã chuyển sang 'Đang dọn dẹp'.");
                    LoadDataPhong();
                }
            }
        }

        // Click dòng trên DataGridView tự động lấy Số phòng lên TextBox
        private void dgvPhong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtSoPhong.Text = dgvPhong.Rows[e.RowIndex].Cells["SoPhong"].Value.ToString();
            }
        }
    }
}