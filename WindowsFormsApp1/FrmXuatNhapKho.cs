using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsApp1.BUS;
using WindowsFormsApp1.DTO;

namespace WindowsFormsApp1
{
    public partial class FrmXuatNhapKho : Form
    {
        private KhoBUS khoBUS = new KhoBUS();
        private List<ChiTietPhieuKhoDTO> listChiTietTam = new List<ChiTietPhieuKhoDTO>();

        public FrmXuatNhapKho()
        {
            InitializeComponent();
        }

        private void FrmXuatNhapKho_Load(object sender, EventArgs e)
        {
            // Nạp loại phiếu
            cboLoaiPhieu.Items.Clear();
            cboLoaiPhieu.Items.Add("NHAP");
            cboLoaiPhieu.Items.Add("XUAT");
            cboLoaiPhieu.SelectedIndex = 0;

            // Load danh sách Vật tư
            LoadDataVatTu();
            LoadCboVatTu();
        }

        private void LoadDataVatTu()
        {
            dgvDanhSachVatTu.DataSource = khoBUS.GetAllVatTu();
        }

        private void LoadCboVatTu()
        {
            cboVatTu.DataSource = khoBUS.GetAllVatTu();
            cboVatTu.DisplayMember = "TenVatTu";
            cboVatTu.ValueMember = "MaVatTu";
        }

        // Nút Thêm chi tiết vào GridView tạm
        private void btnThemChiTiet_Click(object sender, EventArgs e)
        {
            if (cboVatTu.SelectedValue == null) return;

            string maVT = cboVatTu.SelectedValue.ToString();
            decimal soLuong = txtSoLuong.Value;
            decimal donGia = 0;
            decimal.TryParse(txtDonGia.Text, out donGia);

            if (soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông báo");
                return;
            }

            listChiTietTam.Add(new ChiTietPhieuKhoDTO
            {
                MaVatTu = maVT,
                SoLuong = soLuong,
                DonGia = donGia
            });

            // Refresh lại DataGridView Chi tiết
            dgvChiTiet.DataSource = null;
            dgvChiTiet.DataSource = listChiTietTam;
        }

        // Nút Lưu Phiếu - Gọi SqlTransaction
        private void btnLuuPhieu_Click(object sender, EventArgs e)
        {
            // Tính tổng tiền
            decimal tongTien = 0;
            foreach (var item in listChiTietTam)
            {
                tongTien += item.ThanhTien;
            }

            PhieuKhoDTO phieu = new PhieuKhoDTO
            {
                LoaiPhieu = cboLoaiPhieu.SelectedItem.ToString(),
                NgayLap = dtpNgayLap.Value,
                MaNCC = 1, // Ví dụ gán mặc định NCC 1 cho phiếu Nhập
                TongTien = tongTien,
                ChiTiets = listChiTietTam
            };

            string errorMsg = "";
            bool result = khoBUS.LuuPhieuKho(phieu, out errorMsg);

            if (result)
            {
                MessageBox.Show("Lưu phiếu kho và cập nhật tồn kho thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reset form & cập nhật lại dữ liệu tồn kho
                listChiTietTam.Clear();
                dgvChiTiet.DataSource = null;
                LoadDataVatTu();
            }
            else
            {
                // Nếu Rollback sẽ nhảy vào thông báo lỗi này
                MessageBox.Show("Lưu phiếu thất bại!\nChi tiết: " + errorMsg, "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Tìm kiếm Vật tư sử dụng SqlParameter
        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string keyword = txtTimKiem.Text.Trim();
            dgvDanhSachVatTu.DataSource = khoBUS.SearchVatTu(keyword);
        }

        // Cảnh báo tồn kho dưới ngưỡng safe (ví dụ: < 20)
        private void btnCanhBao_Click(object sender, EventArgs e)
        {
            dgvDanhSachVatTu.DataSource = khoBUS.GetVatTuCanCanhBao(20);
            MessageBox.Show($"Đã lọc ra danh sách các vật tư có số lượng tồn < 20", "Cảnh báo kho");
        }
    }
}