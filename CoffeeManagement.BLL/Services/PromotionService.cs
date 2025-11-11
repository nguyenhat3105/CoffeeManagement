using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.BLL.Services
{
    public class PromotionService : IPromotionService
    {
        private IPromotionRepository promotionRepository;
        public PromotionService(IPromotionRepository promotionRepository)
        {
            this.promotionRepository = promotionRepository;
        }
        public void AddPromotion(Promotion promotion)
        {
            promotionRepository.AddPromotion(promotion);
        }

        public void DeletePromotion(int id)
        {
            promotionRepository.DeletePromotion(id);
        }

        public List<Promotion> GetAllPromotion()
        {
            return promotionRepository.GetAllPromotion();
        }

        public Promotion? GetPromotionByCode(string code)
        {
            return promotionRepository.GetPromotionByCode(code);
        }

        public void UpdatePromotion(Promotion promotion)
        {
            promotionRepository.UpdatePromotion(promotion);
        }
        public void IncrementUsage(int id)
        {
            promotionRepository.IncrementUsage(id);
        }

        public Promotion? GetValidPromotionByCode(string code, decimal subtotal)
        {
            var promo = promotionRepository.GetPromotionByCode(code);

            if (promo == null) return null; // Không tìm thấy

            // *** SỬA LỖI Ở DÒNG NÀY ***
            var now = DateTime.Now; // Phải dùng giờ địa phương (Now)

            // Kiểm tra các điều kiện
            if (!promo.IsActive) return null; // (1) Không hoạt động
            if (now < promo.StartDate || now > promo.EndDate) return null; // (2) Hết hạn
            if (subtotal < promo.MinPurchaseAmount) return null; // (3) Không đủ tiền

            // Logic này của bạn đã ĐÚNG (xử lý NULL rất tốt)
            if (promo.UsageLimit.HasValue && promo.CurrentUsage >= promo.UsageLimit.Value) return null; // (4) Hết lượt

            return promo; // Hợp lệ
        }
    }
}
