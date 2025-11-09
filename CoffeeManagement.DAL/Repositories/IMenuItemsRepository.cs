using CoffeeManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.DAL.Repositories
{
    public interface IMenuItemsRepository
    {
        MenuItem Create(MenuItem menuItem);
        bool Delete(int id);
        void Update(MenuItem menuItem);
        MenuItem? GetById(int id);
        IEnumerable<MenuItem> GetAll();
        IEnumerable<Category> GetAllCategories();
    }
}
