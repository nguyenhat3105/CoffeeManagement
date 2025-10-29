using CoffeeManagement.DAL.Models;
using CoffeeManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : UserControl
    {

        public UserProfile()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            var user = AppSession.CurrentUser;
            TxtFullName.Text = user.FirstName + " " + user.LastName;
            TxtEmail.Text = user.Email;
            TxtUsername.Text = user.Username;
            TxtCreatedAt.Text = user.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            using var ctx = new CoffeeManagementDbContext();
            var role = ctx.Roles.FirstOrDefault(r => r.RoleId == user.RoleId);
            TxtRole.Text = role?.RoleName ?? "Unknown";
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = AppSession.CurrentUser;
            var dialog = new UserUpdateProfile(user);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                // Refresh profile display
                LoadProfile();
            }

        }
    }
}
