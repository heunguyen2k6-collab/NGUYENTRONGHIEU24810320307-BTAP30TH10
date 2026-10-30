-- 1. Bảng Phong
CREATE TABLE Phong (
    SoPhong VARCHAR(10) PRIMARY KEY,
    LoaiPhong NVARCHAR(50) NOT NULL,
    GiaTheoGio DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(30) DEFAULT N'Trống'
);

-- 2. Bảng KhachHang
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20) UNIQUE NOT NULL,
    SoDienThoai VARCHAR(15)
);

-- 3. Bảng DichVu
CREATE TABLE DichVu (
    MaDV INT IDENTITY(1,1) PRIMARY KEY,
    TenDV NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL
);

-- 4. Bảng PhieuDatPhong
CREATE TABLE PhieuDatPhong (
    MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong VARCHAR(10) FOREIGN KEY REFERENCES Phong(SoPhong),
    MaKH INT FOREIGN KEY REFERENCES KhachHang(MaKH),
    NgayNhanPhong DATETIME DEFAULT GETDATE(),
    NgayTraPhong DATETIME NULL,
    TongTienThanhToan DECIMAL(18,2) DEFAULT 0
);

-- 5. Bảng SuDungDichVu
CREATE TABLE SuDungDichVu (
    MaPhieu INT FOREIGN KEY REFERENCES PhieuDatPhong(MaPhieu),
    MaDV INT FOREIGN KEY REFERENCES DichVu(MaDV),
    SoLuong INT NOT NULL,
    ThoiGian DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (MaPhieu, MaDV, ThoiGian)
);

-- Thêm dữ liệu mẫu ban đầu
INSERT INTO Phong (SoPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES
('P101', N'Đơn', 100000, N'Trống'),
('P102', N'Đôi', 150000, N'Trống'),
('P201', N'VIP', 300000, N'Trống');

INSERT INTO DichVu (TenDV, DonGia) VALUES
(N'Nước suối', 15000),
(N'Mì tôm', 20000),
(N'Giặt là', 50000);