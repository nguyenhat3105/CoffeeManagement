using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.Models; // for Role enum
using CoffeeManagement.Helpers;    // for AppSession
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
  // namespace for the user controls (adjust if you place them elsewhere)

namespace CoffeeManagement
{
    public partial class MainWindow : Window
    {
        private readonly IAuthorizationService _auth;
        public MainWindow()
        {
            InitializeComponent();
            _auth = new AuthorizationService();
            SetupByRole();
            LoadControl("MenuUI"); // default view
        }

        private void SetupByRole()
        {
            var user = AppSession.CurrentUser;
            TxtWelcome.Text = user != null ? $"Welcome, {user.FirstName} {user.LastName} ({user.Username})" : "Welcome";


            if (user != null)
            {
                if (user.RoleId == 1)
                {
 
                }
                else if (user.RoleId == 2)
                {
                    // staff: can see menu & orders
                    BtnMenu.Visibility = Visibility.Collapsed;
                    BtnAdminDashboard.Visibility = Visibility.Collapsed;
                    BtnOrderHistory.Visibility = Visibility.Collapsed;

                }
                else if (user.RoleId == 3)
                {
                    // customer: maybe only menu (or profile) — hide orders & users
                    BtnOrders.Visibility = Visibility.Collapsed;
                    BtnUsers.Visibility = Visibility.Collapsed;
                    BtnMenu.Visibility = Visibility.Collapsed;
                    BtnAdminDashboard.Visibility = Visibility.Collapsed;
                    BtnPromotion.Visibility = Visibility.Collapsed;
                    BtnAdminOrders.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                switch (b.Name)
                {
                    case "BtnUsers": LoadControl("Users"); break;
                    case "BtnMenu": LoadControl("Menu"); break;
                    case "BtnOrders": LoadControl("Orders"); break;
                    case "BtnAdminOrders": LoadControl("AdminOrders"); break;
                    case "BtnAdminDashboard": LoadControl("AdminDashboard"); break;
                    case "BtnProfile": LoadControl("Profile"); break;
                    case "BtnMenuUI": LoadControl("MenuUI"); break;
                    case "BtnOrderHistory": LoadControl("OrderHistory"); break;
                    case "BtnPromotion": LoadControl("Promotion"); break;
                }
            }
        }

        private void LoadControl(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Menu";

            UserControl uc;
            switch (name.Trim())
            {
                case "Users": uc = new UsersListControl(); break;
                case "Menu": uc = new MenuItemsList(); break;
                case "MenuUI": uc = new MenuUI(); break;
                case "Orders": uc = new StaffOrders(); break;
                case "AdminOrders": uc = new AdminAllOrdersView(); break;
                case "AdminDashboard": uc = new AdminDashboardView(); break;
                case "Profile": uc = new UserProfile(); break;
                case "OrderHistory": uc = new OrderHistory(); break;
                case "Promotion": uc = new AdminPromotionsView(); break;
                default: uc = new UsersListControl(); break;
            }

            ContentRegion.Content = uc;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // simple logout: clear session, return to login
            AppSession.Clear();
         
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }



        // --- CÁC HÀM MỚI (BẮT BUỘC PHẢI THÊM) ---

        // 1. Cho phép kéo thả cửa sổ
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // 2. Nút Thu nhỏ
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 3. Nút Phóng to / Khôi phục
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                (sender as Button).Content = "\uE923"; // Icon Khôi phục
            }
            else
            {
                this.WindowState = WindowState.Normal;
                (sender as Button).Content = "\uE922"; // Icon Phóng to
            }
        }

        // 4. Nút Đóng
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
