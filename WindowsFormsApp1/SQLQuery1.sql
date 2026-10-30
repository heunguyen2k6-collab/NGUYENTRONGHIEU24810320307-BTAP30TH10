-- 1. Tạo Bảng BanAn
CREATE TABLE BanAn (
    MaBan INT IDENTITY(1,1) PRIMARY KEY,
    TenBan NVARCHAR(50) NOT NULL,
    TrangThai NVARCHAR(20) DEFAULT N'Trống'
);

-- 2. Tạo Bảng DanhMuc
CREATE TABLE DanhMuc (
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL
);

-- 3. Tạo Bảng MonAn
CREATE TABLE MonAn (
    MaMon INT IDENTITY(1,1) PRIMARY KEY,
    TenMon NVARCHAR(100) NOT NULL,
    MaDanhMuc INT FOREIGN KEY REFERENCES DanhMuc(MaDanhMuc) ON DELETE CASCADE,
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0)
);

-- 4. Tạo Bảng HoaDon
CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaBan INT FOREIGN KEY REFERENCES BanAn(MaBan),
    GioVao DATETIME NOT NULL DEFAULT GETDATE(),
    GioRa DATETIME NULL,
    TongTien DECIMAL(18,2) DEFAULT 0,
    TrangThai BIT DEFAULT 0 -- 0: Chưa thanh toán, 1: Đã thanh toán
);

-- 5. Tạo Bảng ChiTietHoaDon
CREATE TABLE ChiTietHoaDon (
    MaHoaDon INT FOREIGN KEY REFERENCES HoaDon(MaHoaDon),
    MaMon INT FOREIGN KEY REFERENCES MonAn(MaMon),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHoaDon, MaMon)
);

-- Thêm dữ liệu mẫu
INSERT INTO BanAn (TenBan, TrangThai) VALUES 
(N'Bàn 01', N'Trống'), (N'Bàn 02', N'Trống'), (N'Bàn 03', N'Trống'), 
(N'Bàn 04', N'Trống'), (N'Bàn 05', N'Trống'), (N'Bàn 06', N'Trống');

INSERT INTO DanhMuc (TenDanhMuc) VALUES (N'Cà phê'), (N'Trà sữa'), (N'Đồ ăn nhẹ');

INSERT INTO MonAn (TenMon, MaDanhMuc, DonGia) VALUES 
(N'Cà phê đen', 1, 20000),
(N'Cà phê sữa', 1, 25000),
(N'Trà sữa Trân châu', 2, 35000),
(N'Trà đào cam sả', 2, 30000),
(N'Bánh Mì Que', 3, 15000);