using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.BLL.Services
{
    public class MenuItemsService : IMenuItemsService
    {
        private readonly IMenuItemsRepository menuItemsRepo;

        public MenuItemsService(IMenuItemsRepository menuItemsRepo)
        {
            this.menuItemsRepo = menuItemsRepo ?? throw new ArgumentNullException(nameof(menuItemsRepo));
        }

        public MenuItem Create(MenuItem menuItem)
        {
            return menuItemsRepo.Create(menuItem);
        }

        public bool Delete(int id)
        {
            return menuItemsRepo.Delete(id);
        }

        public IEnumerable<MenuItem> GetAll()
        {
            return menuItemsRepo.GetAll();
        }

        public MenuItem? GetById(int id)
        {
            return menuItemsRepo.GetById(id);
        }

        public void Update(MenuItem menuItem)
        {
            menuItemsRepo.Update(menuItem);
        }
        public IEnumerable<Category> GetAllCategories()
        {
            return menuItemsRepo.GetAllCategories();
        }
    }
}
