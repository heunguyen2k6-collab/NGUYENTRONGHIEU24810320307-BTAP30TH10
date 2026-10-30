using System.Collections.Generic;
using WindowsFormsApp1.DAL;
using WindowsFormsApp1.DTO;

namespace WindowsFormsApp1.BUS
{
    public class KhoBUS
    {
        private KhoDAL khoDAL = new KhoDAL();

        public List<VatTuDTO> GetAllVatTu()
        {
            return khoDAL.GetAllVatTu();
        }

        public List<VatTuDTO> SearchVatTu(string keyword)
        {
            return khoDAL.SearchVatTu(keyword);
        }

        public decimal GetTongTonKho()
        {
            return khoDAL.GetTongGiaTriTonKho();
        }

        public List<VatTuDTO> GetVatTuCanCanhBao(decimal nguong)
        {
            return khoDAL.GetVatTuDuoiNguong(nguong);
        }

        public bool LuuPhieuKho(PhieuKhoDTO phieu, out string msg)
        {
            if (phieu.ChiTiets == null || phieu.ChiTiets.Count == 0)
            {
                msg = "Danh sách chi tiết phiếu kho không được trống!";
                return false;
            }

            return khoDAL.LuuPhieuKhoTransaction(phieu, out msg);
        }
    }
}