USE InventoryDB;
GO

-- 1. Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) UNIQUE,
    DiaChi NVARCHAR(255)
);

-- 2. Bảng Vật tư
CREATE TABLE VatTu (
    MaVatTu VARCHAR(20) PRIMARY KEY,
    TenVatTu NVARCHAR(150) NOT NULL,
    DVT NVARCHAR(20),
    SoLuongTon DECIMAL(18,2) DEFAULT 0 CHECK (SoLuongTon >= 0)
);

-- 3. Bảng Phiếu kho
CREATE TABLE PhieuKho (
    MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
    LoaiPhieu NVARCHAR(10) NOT NULL CHECK (LoaiPhieu IN ('NHAP', 'XUAT')),
    NgayLap DATETIME DEFAULT GETDATE(),
    MaNCC INT NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    TongTien DECIMAL(18,2) DEFAULT 0
);

-- 4. Bảng Chi tiết phiếu kho
CREATE TABLE ChiTietPhieuKho (
    MaPhieu INT FOREIGN KEY REFERENCES PhieuKho(MaPhieu),
    MaVatTu VARCHAR(20) FOREIGN KEY REFERENCES VatTu(MaVatTu),
    SoLuong DECIMAL(18,2) CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) CHECK (DonGia >= 0),
    PRIMARY KEY (MaPhieu, MaVatTu)
);

-- 5. Thêm dữ liệu mẫu ban đầu
INSERT INTO NhaCungCap (TenNCC, DienThoai, DiaChi) VALUES 
(N'Công ty Cổ phần Thép Hòa Phát', '02439288888', N'Hà Nội'),
(N'Nhà phân phối Xi Măng Hà Tiên', '02838291111', N'TP. Hồ Chí Minh');

INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon) VALUES 
('VT01', N'Thép Phi 12', N'Cây', 100),
('VT02', N'Xi Măng Hà Tiên', N'Bao', 50),
('VT03', N'Gạch Thắt', N'Viên', 1000);