using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.DAL.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private IPromotionDAO promotionDAO;
        public PromotionRepository(IPromotionDAO promotionDAO)
        {
            this.promotionDAO = promotionDAO;
        }
        public void AddPromotion(Promotion promotion)
        {
            promotionDAO.AddPromotion(promotion);
        }

        public void DeletePromotion(int id)
        {
            promotionDAO?.DeletePromotion(id);
        }

        public List<Promotion> GetAllPromotion()
        {
            return promotionDAO.GetAllPromotion();
        }

        public Promotion? GetPromotionByCode(string code)
        {
            return promotionDAO?.GetPromotionByCode(code);
        }

        public void UpdatePromotion(Promotion promotion)
        {
            promotionDAO.UpdatePromotion(promotion);
        }
        public void IncrementUsage(int id)
        {
            promotionDAO.IncrementUsage(id);
        }
    }
}
