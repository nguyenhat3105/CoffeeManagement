using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.DAL.Repositories
{
    public class MenuItemsRepository : IMenuItemsRepository
    {
        private readonly IMenuItemsDAO MenuItemsDAO;
        public MenuItemsRepository(IMenuItemsDAO menuItemsDAO)
        {
            MenuItemsDAO = menuItemsDAO ?? throw new ArgumentNullException(nameof(menuItemsDAO));
        }
        public MenuItem Create(MenuItem menuItem)
        {
            return MenuItemsDAO.CreateMenuItems(menuItem);
        }

        public bool Delete(int id)
        {
            return MenuItemsDAO.DeleteMenuItems(id);
        }

        public IEnumerable<MenuItem> GetAll()
        {
            return MenuItemsDAO.GetAllMenuItems();
        }

        public MenuItem? GetById(int id)
        {
            return MenuItemsDAO.GetMenuItemsById(id);
        }

        public void Update(MenuItem menuItem)
        {
            MenuItemsDAO.UpdateMenuItems(menuItem);
        }
        public IEnumerable<Category> GetAllCategories()
        {
            return MenuItemsDAO.GetAllCategories();
        }
    }
}
